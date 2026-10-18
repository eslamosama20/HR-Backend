using AutoMapper;
using SmartAndPro.ERP.Domains.Entities.HR;
using SmartAndPro.ERP.Domains.Entities;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Attendance;
using SmartAndPro.ERP.Application.Common.Dtos.HR.BankDocument;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Bonus;
using SmartAndPro.ERP.Application.Common.Dtos.HR.CheckInLog;
using SmartAndPro.ERP.Application.Common.Dtos.HR.CompanyDocument;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Employee;
using SmartAndPro.ERP.Application.Common.Dtos.HR.EmploymentApplication;
using SmartAndPro.ERP.Application.Common.Dtos.HR.EmploymentType;
using SmartAndPro.ERP.Application.Common.Dtos.HR.HRTrustReceipt;
using SmartAndPro.ERP.Application.Common.Dtos.HR.InsuranceRecord;
using SmartAndPro.ERP.Application.Common.Dtos.HR.LeaveApplication;
using SmartAndPro.ERP.Application.Common.Dtos.HR.MonthlyLoan;
using SmartAndPro.ERP.Application.Common.Dtos.HR.OvertimeHours;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Penalty;
using SmartAndPro.ERP.Application.Common.Dtos.HR.ResignationRequest;
using SmartAndPro.ERP.Application.Common.Dtos.HR.SalarySlip;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Shift;
using SmartAndPro.ERP.Application.Common.Dtos.FingerprintDeviceSettings;

namespace SmartAndPro.ERP.Application.Common.AutoMapper;

public class HRMappingProfiles : Profile
{
    public HRMappingProfiles()
    {
        // Shift
        CreateMap<ShiftPostDto, Shift>();
        CreateMap<ShiftPutDto, Shift>();
        CreateMap<Shift, ShiftResponseDto>();

        // EmploymentType
        CreateMap<EmploymentTypePostDto, EmploymentType>()
            .ForMember(dest => dest.WeeklyDayOff,
                opt => opt.MapFrom(src =>
                    !string.IsNullOrWhiteSpace(src.WeeklyDayOff)
                        ? src.WeeklyDayOff
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => Enum.Parse<DayOfWeek>(s))
                            .ToList()
                        : null
                ));
        CreateMap<EmploymentTypePutDto, EmploymentType>()
            .ForMember(dest => dest.WeeklyDayOff,
                opt => opt.MapFrom(src =>
                    !string.IsNullOrWhiteSpace(src.WeeklyDayOff)
                        ? src.WeeklyDayOff
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => Enum.Parse<DayOfWeek>(s))
                            .ToList()
                        : null
                ));
        CreateMap<EmploymentType, EmploymentTypeResponseDto>()
            .ForMember(dest => dest.WeeklyDayOff,
                opt => opt.MapFrom(src =>
                    src.WeeklyDayOff.Count > 0
                        ? string.Join(",", src.WeeklyDayOff)
                        : null
                ));

        // Employee
        CreateMap<EmployeePostDto, Employee>()
            .ForMember(dest => dest.FactoryType,
                opt => opt.MapFrom(src =>
                    src.FactoryType != null ? Enum.Parse<FactoryType>(src.FactoryType) : (FactoryType?)null));
        CreateMap<EmployeePutDto, Employee>()
            .ForMember(dest => dest.FactoryType,
                opt => opt.MapFrom(src =>
                    src.FactoryType != null ? Enum.Parse<FactoryType>(src.FactoryType) : (FactoryType?)null));
        CreateMap<Employee, EmployeeResponseDto>()
            .ForMember(x => x.EmploymentType, opt => opt.MapFrom(src => src.EmploymentType))
            .ForMember(x => x.Shift, opt => opt.MapFrom(src => src.Shift))
            .ForMember(x => x.AttendanceRecords, opt => opt.MapFrom(src => src.AttendanceRecords))
            .ForMember(x => x.FactoryType, opt => opt.MapFrom(src => src.FactoryType.ToString()));

        // CheckInLog
        CreateMap<CheckInLogPostDto, CheckInLog>()
            .ForMember(dest => dest.LogType, opt => opt.MapFrom(src => Enum.Parse<LogType>(src.LogType)));
        CreateMap<CheckInLogPutDto, CheckInLog>()
            .ForMember(dest => dest.LogType, opt => opt.MapFrom(src => Enum.Parse<LogType>(src.LogType)));
        CreateMap<CheckInLog, CheckInLogResponseDto>()
            .ForMember(x => x.Employee, opt => opt.MapFrom(src => src.Employee))
            .ForMember(x => x.LogType, opt => opt.MapFrom(src => src.LogType.ToString()));

        // Attendance
        CreateMap<AttendancePostDto, Attendance>()
            .ForMember(dest => dest.AttendanceFlag, opt => opt.MapFrom(src => Enum.Parse<AttendanceFlag>(src.AttendanceFlag)));
        CreateMap<AttendancePutDto, Attendance>()
            .ForMember(dest => dest.AttendanceFlag, opt => opt.MapFrom(src => Enum.Parse<AttendanceFlag>(src.AttendanceFlag)));
        CreateMap<Attendance, AttendanceResponseDto>()
            .ForMember(x => x.Employee, opt => opt.MapFrom(src => src.Employee))
            .ForMember(x => x.AttendanceFlag, opt => opt.MapFrom(src => src.AttendanceFlag.ToString()));

        // MonthlyLoan
        CreateMap<MonthlyLoanPostDto, MonthlyLoan>();
        CreateMap<MonthlyLoanPutDto, MonthlyLoan>();
        CreateMap<MonthlyLoan, MonthlyLoanResponseDto>()
            .ForMember(x => x.Employee, opt => opt.MapFrom(src => src.Employee));

        // Bonus
        CreateMap<BonusPostDto, Bonus>();
        CreateMap<BonusPutDto, Bonus>();
        CreateMap<Bonus, BonusResponseDto>()
            .ForMember(x => x.Employee, opt => opt.MapFrom(src => src.Employee));

        // OvertimeHours
        CreateMap<OvertimeHoursPostDto, OvertimeHours>();
        CreateMap<OvertimeHoursPutDto, OvertimeHours>();
        CreateMap<OvertimeHours, OvertimeHoursResponseDto>()
            .ForMember(x => x.Employee, opt => opt.MapFrom(src => src.Employee));

        // Penalty
        CreateMap<PenaltyPostDto, Penalty>();
        CreateMap<PenaltyPutDto, Penalty>();
        CreateMap<Penalty, PenaltyResponseDto>()
            .ForMember(x => x.Employee, opt => opt.MapFrom(src => src.Employee));

        // FingerprintDeviceSettings
        CreateMap<FingerprintDeviceSettingsPostDto, FingerprintDeviceSettings>();
        CreateMap<FingerprintDeviceSettingsPutDto, FingerprintDeviceSettings>()
            .ForMember(x => x.Factory1LastSyncTime, opt => opt.Ignore())
            .ForMember(x => x.Factory1LastSyncStatus, opt => opt.Ignore())
            .ForMember(x => x.Factory1LastSyncMessage, opt => opt.Ignore())
            .ForMember(x => x.Factory1LastSyncFetchedCount, opt => opt.Ignore())
            .ForMember(x => x.Factory1LastSyncInsertedCount, opt => opt.Ignore())
            .ForMember(x => x.Factory1LastSyncSkippedCount, opt => opt.Ignore())
            .ForMember(x => x.Factory2LastSyncTime, opt => opt.Ignore())
            .ForMember(x => x.Factory2LastSyncStatus, opt => opt.Ignore())
            .ForMember(x => x.Factory2LastSyncMessage, opt => opt.Ignore())
            .ForMember(x => x.Factory2LastSyncFetchedCount, opt => opt.Ignore())
            .ForMember(x => x.Factory2LastSyncInsertedCount, opt => opt.Ignore())
            .ForMember(x => x.Factory2LastSyncSkippedCount, opt => opt.Ignore())
            .ForMember(x => x.Factory3LastSyncTime, opt => opt.Ignore())
            .ForMember(x => x.Factory3LastSyncStatus, opt => opt.Ignore())
            .ForMember(x => x.Factory3LastSyncMessage, opt => opt.Ignore())
            .ForMember(x => x.Factory3LastSyncFetchedCount, opt => opt.Ignore())
            .ForMember(x => x.Factory3LastSyncInsertedCount, opt => opt.Ignore())
            .ForMember(x => x.Factory3LastSyncSkippedCount, opt => opt.Ignore());
        CreateMap<FingerprintDeviceSettings, FingerprintDeviceSettingsResponseDto>();

        // LeaveApplication
        CreateMap<LeaveApplicationPostDto, LeaveApplication>();
        CreateMap<LeaveApplicationPutDto, LeaveApplication>();
        CreateMap<LeaveApplication, LeaveApplicationResponseDto>()
            .ForMember(x => x.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Name : null))
            .ForMember(x => x.EmployeeCode, opt => opt.MapFrom(src => src.Employee != null ? (src.Employee.ReferenceNumber ?? src.Employee.DeviceId) : null));

        // CompanyDocument
        CreateMap<CompanyDocumentPostDto, CompanyDocument>();
        CreateMap<CompanyDocumentPutDto, CompanyDocument>();
        CreateMap<CompanyDocument, CompanyDocumentResponseDto>();

        // ResignationRequest
        CreateMap<ResignationRequestPostDto, ResignationRequest>();
        CreateMap<ResignationRequestPutDto, ResignationRequest>();
        CreateMap<ResignationRequest, ResignationRequestResponseDto>()
            .ForMember(x => x.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Name : null));

        // HRTrustReceipt
        CreateMap<HRTrustReceiptPostDto, HRTrustReceipt>();
        CreateMap<HRTrustReceiptPutDto, HRTrustReceipt>();
        CreateMap<HRTrustReceipt, HRTrustReceiptResponseDto>()
            .ForMember(x => x.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Name : null));

        // EmploymentApplication
        CreateMap<EmploymentApplicationPostDto, EmploymentApplication>();
        CreateMap<EmploymentApplicationPutDto, EmploymentApplication>();
        CreateMap<EmploymentApplication, EmploymentApplicationResponseDto>();

        // BankDocument
        CreateMap<BankDocumentPostDto, BankDocument>();
        CreateMap<BankDocumentPutDto, BankDocument>();
        CreateMap<BankDocument, BankDocumentResponseDto>()
            .ForMember(x => x.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Name : null))
            .ForMember(x => x.DaysUntilExpiration, opt => opt.MapFrom(src => (src.ExpirationDate.ToDateTime(TimeOnly.MinValue) - DateTime.Now).Days));

        // InsuranceRecord
        CreateMap<InsuranceRecordPostDto, InsuranceRecord>();
        CreateMap<InsuranceRecordPutDto, InsuranceRecord>();
        CreateMap<InsuranceRecord, InsuranceRecordResponseDto>()
            .ForMember(x => x.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Name : null));

        // SalarySlip
        CreateMap<SalarySlipPostDto, SalarySlip>();
        CreateMap<SalarySlipPutDto, SalarySlip>();
        CreateMap<SalarySlip, SalarySlipResponseDto>()
            .ForMember(x => x.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Name : null))
            .ForMember(x => x.DeviceId, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.DeviceId : null))
            .ForMember(x => x.JobTitle, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.JobTitle : null));
    }
}
