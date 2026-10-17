using SmartAndPro.ERP.Application.Common.Dtos.HR;
using SmartAndPro.ERP.Domains.Entities.HR;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Utilities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace SmartAndPro.ERP.Application.Services.HR;

public class PaymentCalculatorService
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public PaymentCalculatorService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    public async Task<PaymentResult> CalculatePaymentsAsync(DateOnly month)
    {
        try
        {
            // Define month range
            var start = month.ToDateTime(TimeOnly.MinValue);
            var end = month.AddMonths(1).AddDays(-1).ToDateTime(TimeOnly.MaxValue);
            var startDateOnly = DateOnly.FromDateTime(start);
            var endDateOnly = DateOnly.FromDateTime(end);

            // Ensure all missing attendance / absence records exist up to date
            await AttendanceService.EnsureAttendanceRecordsUpToDateAsync(_repositoriesFactory, startDateOnly, endDateOnly);

            var checkInLogRepository = _repositoriesFactory.GetRepository<CheckInLog>();
            var employeeRepository = _repositoriesFactory.GetRepository<Employee>();
            // Load all employees with their related data
            var employeesQuery = await employeeRepository.GetAll();
            var employees = await employeesQuery.Data
                .Include(e => e.EmploymentType)
                .Include(e => e.Shift)
                .Include(e => e.AttendanceRecords.Where(a =>
                    a.AttendanceDate >= startDateOnly &&
                    a.AttendanceDate <= endDateOnly))
                .ToListAsync();
            var checkInLogsQuery = await checkInLogRepository.Filter(c => c.LogType == LogType.IN
                                                                          && c.Time >= start
                                                                          && c.Time <= end);

            var checkInLogs = await checkInLogsQuery.Data
                .ToListAsync();

            var logLookup = checkInLogs
                .GroupBy(c => (c.EmployeeId, c.Time.Date))
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Time).First());

            var loansQuery = await _repositoriesFactory.GetRepository<MonthlyLoan>().GetAll();
            var loans = await loansQuery.Data
                .Where(l => l.Date >= startDateOnly && l.Date <= endDateOnly)
                .ToListAsync();

            var penaltiesQuery = await _repositoriesFactory.GetRepository<Penalty>().GetAll();
            var penalties = await penaltiesQuery.Data
                .Where(p => p.Date >= startDateOnly && p.Date <= endDateOnly)
                .ToListAsync();

            var bonusesQuery = await _repositoriesFactory.GetRepository<Bonus>().GetAll();
            var bonuses = await bonusesQuery.Data
                .Where(b => b.Date >= startDateOnly && b.Date <= endDateOnly)
                .ToListAsync();

            var factory1Payments = new List<Factory1PaymentResult>();
            var factory2Payments = new List<Factory2PaymentResult>();
            var factory3Payments = new List<Factory3PaymentResult>();

            foreach (var emp in employees)
            {
                if (emp.FactoryType == null) continue;
                var type = emp.EmploymentType;
                decimal monthlyRate = emp.MonthlyRate;
                decimal suppRate = emp.SupplementaryRate;
                decimal monthlyInsurance = emp.MonthlyInsurance ?? 0m;

                // 1) Total base salary + any bonuses
                decimal totalBonuses = bonuses
                    .Where(b => b.EmployeeId == emp.Id)
                    .Sum(b => b.Amount);
                decimal totalSalary = monthlyRate + suppRate + totalBonuses;

                // 2) Hourly rate assuming 30 days/month, 8h per day (or use emp.Shift.HoursPerDay)
                const int workDaysPerMonth = 30;
                var shiftStart = emp.Shift.StartTime; // TimeOnly
                var shiftEnd = emp.Shift.EndTime; // TimeOnly
                double hoursPerDay = Math.Round((shiftEnd - shiftStart).TotalHours, 2);
                decimal hourlyRate = monthlyRate / (workDaysPerMonth * (decimal)hoursPerDay);

                decimal dailyRate = monthlyRate / workDaysPerMonth;

                // 3) Absent days & absent penalties (with leave logic and actual worked days)
                var weeklyDayOffs = type.WeeklyDayOff ?? new List<DayOfWeek> { DayOfWeek.Friday };

                var monthAttendances = emp.AttendanceRecords
                    .Where(a => a.AttendanceDate >= startDateOnly && a.AttendanceDate <= endDateOnly)
                    .ToList();

                int attendedDaysCount = monthAttendances.Count(a =>
                    a.AttendanceFlag == AttendanceFlag.Present ||
                    a.AttendanceFlag == AttendanceFlag.LateEntry ||
                    a.AttendanceFlag == AttendanceFlag.EarlyExit ||
                    a.AttendanceFlag == AttendanceFlag.LateWithPermission);

                int halfDaysCount = monthAttendances.Count(a => a.AttendanceFlag == AttendanceFlag.HalfDay);

                // Effective hire date: if created mid-month but StartDate was left at default (1st of month), use creation date
                var effectiveHireDate = emp.StartDate;
                var createdDate = DateOnly.FromDateTime(emp.CreatedAt);
                if (createdDate.Year == month.Year && createdDate.Month == month.Month && createdDate > effectiveHireDate && effectiveHireDate == startDateOnly)
                {
                    effectiveHireDate = createdDate;
                }

                bool isMidMonthHire = effectiveHireDate > startDateOnly;

                // Pre-hire days in this month (prior to effectiveHireDate): deducted 1:1
                int preHireDaysCount = 0;
                if (isMidMonthHire)
                {
                    var preHireEnd = effectiveHireDate.AddDays(-1) < endDateOnly ? effectiveHireDate.AddDays(-1) : endDateOnly;
                    for (var d = startDateOnly; d <= preHireEnd; d = d.AddDays(1))
                    {
                        preHireDaysCount++;
                    }
                }

                // Days explicitly marked as NotRegistered in attendance records (prior to registration)
                int notRegisteredDays = monthAttendances.Count(a => a.AttendanceFlag == AttendanceFlag.NotRegistered);
                if (notRegisteredDays > preHireDaysCount)
                {
                    preHireDaysCount = notRegisteredDays;
                }

                // Active employment leaves (AttendanceDate >= effectiveHireDate)
                var yearStart = new DateOnly(month.Year, 1, 1);
                var yearEnd = new DateOnly(month.Year, 12, 31);
                var activeYearLeaves = emp.AttendanceRecords
                    .Where(a => a.AttendanceFlag == AttendanceFlag.OnLeaveWithPermission
                        && a.AttendanceDate >= yearStart && a.AttendanceDate <= yearEnd
                        && a.AttendanceDate >= effectiveHireDate
                        && !weeklyDayOffs.Contains(a.AttendanceDate.DayOfWeek))
                    .OrderBy(a => a.AttendanceDate)
                    .ToList();

                var activeMonthLeaves = monthAttendances
                    .Where(a => a.AttendanceFlag == AttendanceFlag.OnLeaveWithPermission
                        && a.AttendanceDate >= effectiveHireDate
                        && !weeklyDayOffs.Contains(a.AttendanceDate.DayOfWeek))
                    .ToList();

                // Excused leaves taken without balance are tagged [بدون رصيد] and deducted 1:1
                int thisMonthExcessLeaves = activeMonthLeaves.Count(a => 
                    a.Reason != null && (a.Reason.Contains("[بدون رصيد]") || a.Reason.Contains("بدون أجر")));

                // Unexcused absences during active employment
                var unexcusedAbsences = monthAttendances
                    .Where(a => (a.AttendanceFlag == AttendanceFlag.Absent || a.AttendanceFlag == AttendanceFlag.OnLeaveWithoutPermission)
                        && a.AttendanceDate >= effectiveHireDate
                        && !weeklyDayOffs.Contains(a.AttendanceDate.DayOfWeek))
                    .ToList();

                decimal absentMultiplier = emp.AbsentRateMultiplier > 0 ? emp.AbsentRateMultiplier : 1.5m;

                bool hasZeroAttendance = (attendedDaysCount == 0 && halfDaysCount == 0);

                // If month is ongoing, future unworked days deducted 1:1
                var today = DateOnly.FromDateTime(DateTime.Today);
                int futureUnworkedDays = 0;
                if (endDateOnly > today && today >= effectiveHireDate)
                {
                    if (isMidMonthHire)
                    {
                        var futureEnd = endDateOnly.Day > 30 ? new DateOnly(endDateOnly.Year, endDateOnly.Month, 30) : endDateOnly;
                        for (var d = today.AddDays(1); d <= futureEnd; d = d.AddDays(1))
                        {
                            futureUnworkedDays++;
                        }
                    }
                    else
                    {
                        for (var d = today.AddDays(1); d <= endDateOnly; d = d.AddDays(1))
                        {
                            if (!weeklyDayOffs.Contains(d.DayOfWeek))
                            {
                                futureUnworkedDays++;
                            }
                        }
                    }
                }

                decimal absentDays;
                decimal totalAbsentPenalty;

                if (hasZeroAttendance)
                {
                    // Employee has not attended any day this month: Net salary must be 0
                    absentDays = workDaysPerMonth;
                    totalAbsentPenalty = totalSalary;
                }
                else
                {
                    decimal preHirePenalty = preHireDaysCount * 1.0m * dailyRate;
                    decimal excessLeavePenalty = thisMonthExcessLeaves * 1.0m * dailyRate;
                    decimal unexcusedPenalty = unexcusedAbsences.Count * absentMultiplier * dailyRate;
                    decimal halfDayPenalty = halfDaysCount * 0.5m * dailyRate;
                    decimal futurePenalty = futureUnworkedDays * 1.0m * dailyRate;

                    decimal unworkedDaysCount = preHireDaysCount + thisMonthExcessLeaves + (unexcusedAbsences.Count * absentMultiplier) + (halfDaysCount * 0.5m) + futureUnworkedDays;
                    absentDays = Math.Min((decimal)workDaysPerMonth, unworkedDaysCount);

                    totalAbsentPenalty = preHirePenalty + excessLeavePenalty + unexcusedPenalty + halfDayPenalty + futurePenalty;

                    // Prorate supplementary rate as well so unworked days don't get full monthly bonus
                    if (suppRate > 0 && workDaysPerMonth > 0)
                    {
                        decimal suppDaily = suppRate / workDaysPerMonth;
                        decimal totalUnworkedCalendarDays = preHireDaysCount + unexcusedAbsences.Count + futureUnworkedDays + thisMonthExcessLeaves;
                        decimal suppPenalty = Math.Min(suppRate, totalUnworkedCalendarDays * suppDaily);
                        totalAbsentPenalty += suppPenalty;
                    }

                    if (totalAbsentPenalty > totalSalary)
                    {
                        totalAbsentPenalty = totalSalary;
                    }
                }

                decimal absentCount = absentDays;

                // Lateness: With permission (1.0x) vs Without permission (LateRateMultiplier)
                double totalLateHoursWithoutPermission = 0;
                double totalLateHoursWithPermission = 0;
                foreach (var a in emp.AttendanceRecords.Where(a => a.AttendanceFlag == AttendanceFlag.LateEntry || a.AttendanceFlag == AttendanceFlag.LateWithPermission))
                {
                    var scheduledStart = a.AttendanceDate.ToDateTime(emp.Shift.StartTime);
                    double lateSpanHours = 0;

                    if (logLookup.TryGetValue((emp.Id, a.AttendanceDate.ToDateTime(TimeOnly.MinValue).Date), out var log))
                    {
                        lateSpanHours = Math.Max(0, (log.Time - scheduledStart).TotalHours);
                    }

                    if (a.AttendanceFlag == AttendanceFlag.LateWithPermission)
                    {
                        totalLateHoursWithPermission += lateSpanHours;
                    }
                    else
                    {
                        totalLateHoursWithoutPermission += lateSpanHours;
                    }
                }

                decimal lateMultiplier = emp.LateRateMultiplier > 0 ? emp.LateRateMultiplier : 1.0m;
                double totalLateHours = totalLateHoursWithPermission + totalLateHoursWithoutPermission;

                // penalty for lateness: With permission = 1x (actual hours only), Without permission = lateMultiplier
                decimal totalLateEntryPenalty = ((decimal)totalLateHoursWithPermission * 1.0m + (decimal)totalLateHoursWithoutPermission * lateMultiplier) * hourlyRate;

                // 4) Loans
                decimal totalLoans = loans
                    .Where(l => l.EmployeeId == emp.Id)
                    .Sum(l => l.Amount);

                // 5) Other penalties (early exit, etc.)
                decimal otherPenalties = penalties
                    .Where(p => p.EmployeeId == emp.Id)
                    .Sum(p => p.Amount);

                // 6) Sum up what to subtract
                decimal totalSubtractions = totalAbsentPenalty
                                            + totalLateEntryPenalty
                                            + totalLoans
                                            + monthlyInsurance
                                            + otherPenalties;

                decimal netPay = totalSalary - totalSubtractions;
                if (netPay < 0)
                {
                    netPay = 0;
                }
                if (emp.FactoryType == FactoryType.Factory1)
                {
                    factory1Payments.Add(new Factory1PaymentResult
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = emp.Name,
                        EmployeeTypeName = type.Code,
                        MonthlyRate = Math.Round(monthlyRate, 2),
                        SupplementaryRate = Math.Round(suppRate, 2),
                        TotalBonuses = Math.Round(totalBonuses, 2),
                        TotalSalary = Math.Round(totalSalary, 2),
                        AbsentDaysCount = Math.Round(absentCount, 2),
                        DailyRate = Math.Round(dailyRate, 2),
                        TotalAbsentPenalty = Math.Round(totalAbsentPenalty, 2),
                        TotalLateHours = Math.Round(totalLateHours, 2),
                        HourlyRate = Math.Round(hourlyRate, 2),
                        TotalLateEntryPenalty = Math.Round(totalLateEntryPenalty, 2),
                        TotalLoans = Math.Round(totalLoans, 2),
                        Insurance = Math.Round(monthlyInsurance, 2),
                        TotalPenalties = Math.Round(otherPenalties, 2),
                        TotalSubtractedAmount = Math.Round(totalSubtractions, 2),
                        NetPay = Math.Round(netPay, 2),
                    });
                }
                else if (emp.FactoryType == FactoryType.Factory2)
                {
                    factory2Payments.Add(new Factory2PaymentResult
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = emp.Name,
                        EmployeeTypeName = type.Code,
                        MonthlyRate = Math.Round(monthlyRate, 2),
                        SupplementaryRate = Math.Round(suppRate, 2),
                        TotalBonuses = Math.Round(totalBonuses, 2),
                        TotalSalary = Math.Round(totalSalary, 2),
                        AbsentDaysCount = Math.Round(absentCount, 2),
                        DailyRate = Math.Round(dailyRate, 2),
                        TotalAbsentPenalty = Math.Round(totalAbsentPenalty, 2),
                        TotalLateHours = Math.Round(totalLateHours, 2),
                        HourlyRate = Math.Round(hourlyRate, 2),
                        TotalLateEntryPenalty = Math.Round(totalLateEntryPenalty, 2),
                        TotalLoans = Math.Round(totalLoans, 2),
                        Insurance = Math.Round(monthlyInsurance, 2),
                        TotalPenalties = Math.Round(otherPenalties, 2),
                        TotalSubtractedAmount = Math.Round(totalSubtractions, 2),
                        NetPay = Math.Round(netPay, 2),
                    });
                }
                else if (emp.FactoryType == FactoryType.Factory3)
                {
                    factory3Payments.Add(new Factory3PaymentResult
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = emp.Name,
                        EmployeeTypeName = type.Code,
                        MonthlyRate = Math.Round(monthlyRate, 2),
                        SupplementaryRate = Math.Round(suppRate, 2),
                        TotalBonuses = Math.Round(totalBonuses, 2),
                        TotalSalary = Math.Round(totalSalary, 2),
                        AbsentDaysCount = Math.Round(absentCount, 2),
                        DailyRate = Math.Round(dailyRate, 2),
                        TotalAbsentPenalty = Math.Round(totalAbsentPenalty, 2),
                        TotalLateHours = Math.Round(totalLateHours, 2),
                        HourlyRate = Math.Round(hourlyRate, 2),
                        TotalLateEntryPenalty = Math.Round(totalLateEntryPenalty, 2),
                        TotalLoans = Math.Round(totalLoans, 2),
                        Insurance = Math.Round(monthlyInsurance, 2),
                        TotalPenalties = Math.Round(otherPenalties, 2),
                        TotalSubtractedAmount = Math.Round(totalSubtractions, 2),
                        NetPay = Math.Round(netPay, 2),
                    });
                }
            }
            var results = new PaymentResult
            {
                Factory1Payments = factory1Payments,
                Factory2Payments = factory2Payments,
                Factory3Payments = factory3Payments,
            };

            return results;
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<OvertimePaymentResult> CalculateOverTimeHoursPaymentsAsync(DateOnly month)
    {
        try
        {
            var start = month.ToDateTime(TimeOnly.MinValue);
            var end = month.AddMonths(1).AddDays(-1).ToDateTime(TimeOnly.MaxValue);
            var periodStart = DateOnly.FromDateTime(start);
            var periodEnd = DateOnly.FromDateTime(end);

            var employeeRepository = _repositoriesFactory.GetRepository<Employee>();
            var overtimeRepository = _repositoriesFactory.GetRepository<OvertimeHours>();
            var checkInLogRepository = _repositoriesFactory.GetRepository<CheckInLog>();

            var employeesQuery = await employeeRepository.GetAll();
            var employees = await employeesQuery.Data
                .Include(e => e.EmploymentType)
                .Include(e => e.Shift)
                .ToListAsync();

            var overtimeQuery = await overtimeRepository.GetAll();
            var overtimeRecords = await overtimeQuery.Data
                .Where(o => o.Date >= periodStart && o.Date <= periodEnd)
                .ToListAsync();

            var checkInLogsQuery = await checkInLogRepository.Filter(c =>
                c.Time >= start && c.Time <= end);
            var checkInLogs = await checkInLogsQuery.Data.ToListAsync();

            var factory1Overtime = new List<FactoryOvertimePaymentResult>();
            var factory2Overtime = new List<FactoryOvertimePaymentResult>();
            var factory3Overtime = new List<FactoryOvertimePaymentResult>();

            foreach (var emp in employees)
            {
                if (emp.FactoryType == null) continue;

                var type = emp.EmploymentType;
                var shift = emp.Shift;
                if (shift == null) continue;

                decimal monthlyRate = emp.MonthlyRate;
                const int workDaysPerMonth = 30;
                double hoursPerDay = Math.Round((shift.EndTime - shift.StartTime).TotalHours, 2);
                decimal hourlyRate = monthlyRate / (workDaysPerMonth * (decimal)hoursPerDay);

                // Manually recorded overtime
                decimal manualOvertime = overtimeRecords
                    .Where(o => o.EmployeeId == emp.Id)
                    .Sum(o => o.HoursCount);

                // Automatically calculated from check-in logs
                var empLogs = checkInLogs
                    .Where(l => l.EmployeeId == emp.Id)
                    .GroupBy(l => l.Time.Date);

                decimal autoOvertime = 0;
                foreach (var dayLogs in empLogs)
                {
                    var logs = dayLogs.OrderBy(l => l.Time).ToList();
                    var lastOut = logs.LastOrDefault(l => l.LogType == LogType.OUT);

                    if (lastOut != null)
                    {
                        var scheduledEnd = DateTime.SpecifyKind(
                            DateOnly.FromDateTime(lastOut.Time).ToDateTime(emp.Shift.EndTime), DateTimeKind.Unspecified);

                        var overtimeSpan = lastOut.Time - scheduledEnd;

                        // If worked more than 30 minutes after scheduled end time
                        if (overtimeSpan.TotalMinutes > 30)
                        {
                            autoOvertime += (decimal)(overtimeSpan.TotalHours);
                        }
                    }
                }

                decimal overtimeMultiplier = emp.OvertimeRateMultiplier > 0 ? emp.OvertimeRateMultiplier : 1.5m;
                decimal totalOvertimeHours = manualOvertime + autoOvertime;
                decimal overtimePayment = Math.Round(totalOvertimeHours * overtimeMultiplier * hourlyRate, 2);

                var resultItem = new FactoryOvertimePaymentResult
                {
                    EmployeeId = emp.Id,
                    EmployeeName = emp.Name,
                    EmployeeTypeName = type.Code,
                    MonthlyRate = Math.Round(monthlyRate, 2),
                    HourlyRate = Math.Round(hourlyRate, 2),
                    OvertimeHours = Math.Round(totalOvertimeHours, 2),
                    OvertimePayment = overtimePayment
                };

                if (emp.FactoryType == FactoryType.Factory1)
                    factory1Overtime.Add(resultItem);
                else if (emp.FactoryType == FactoryType.Factory2)
                    factory2Overtime.Add(resultItem);
                else if (emp.FactoryType == FactoryType.Factory3)
                    factory3Overtime.Add(resultItem);
            }

            return new OvertimePaymentResult
            {
                Factory1OvertimePayments = factory1Overtime,
                Factory2OvertimePayments = factory2Overtime,
                Factory3OvertimePayments = factory3Overtime
            };
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            Console.WriteLine(e);
            throw;
        }
    }
}