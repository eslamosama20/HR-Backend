using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.TrailBalance;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;

namespace SmartAndPro.ERP.Application.Services.Accounting;

public class TrailBalanceService
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public TrailBalanceService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    public async Task<OperationResponse<List<TrailBalanceResponseRootDto>>> GetTrailBalanceData(DateOnly startDate, DateOnly endDate)
    {
        try
        {
            var rootAccountsQuery = await _repositoriesFactory.GetRepository<RootAccount>().GetAll();
            
            var rootAccounts = await rootAccountsQuery.Data.ToListAsync();

            var transactionEntriesQuery = await _repositoriesFactory.GetRepository<TransactionEntry>().GetAll();
            
            var totalTransactionEntries = await transactionEntriesQuery.Data.ToListAsync();
            
            // Helper function to calculate totals for entries (debits and credits)
            decimal CalculateTotalDebit(List<TransactionEntry> entries) => entries.Sum(e => e.DebitAmount);
            decimal CalculateTotalCredit(List<TransactionEntry> entries) => entries.Sum(e => e.CreditAmount);

            var trailBalanceData = new List<TrailBalanceResponseRootDto>();

            //Parallel.ForEach(rootAccounts, rootAccount =>
            foreach (var rootAccount in rootAccounts)
            {
                var headerAccountsUnderRootAccount = rootAccount.HeaderAccounts;
                var rootDto = new TrailBalanceResponseRootDto
                {
                    AccountNumber = rootAccount.ReferenceCode,
                    AccountEnglishName = rootAccount.EnglishName,
                    AccountArabicName = rootAccount.ArabicName,
                    PeriodDebit = 0,
                    PeriodCredit = 0,
                    PreviousDebit = 0,
                    PreviousCredit = 0,
                    BalanceDebit = 0,
                    BalanceCredit = 0,
                    Balance = 0,
                    Headers = new List<TrailBalanceResponseHeaderDto>()
                };

                foreach (var headerAccount in headerAccountsUnderRootAccount)
                {
                    var detailAccountsUnderHeaderAccount = headerAccount.DetailAccounts;

                    var headerDto = new TrailBalanceResponseHeaderDto
                    {
                        AccountNumber = headerAccount.ReferenceCode,
                        AccountEnglishName = headerAccount.EnglishName,
                        AccountArabicName = headerAccount.ArabicName,
                        PeriodDebit = 0,
                        PeriodCredit = 0,
                        BalanceDebit = 0,
                        BalanceCredit = 0,
                        PreviousDebit = 0,
                        PreviousCredit = 0,
                        Balance = 0,
                        Details = new List<TrailBalanceResponseDetailDto>()
                    };

                    foreach (var detailAccount in detailAccountsUnderHeaderAccount)
                    {
                        var mainAccountsUnderDetailAccount = detailAccount.MainAccounts;

                        var detailDto = new TrailBalanceResponseDetailDto
                        {
                            AccountNumber = detailAccount.ReferenceCode,
                            AccountEnglishName = detailAccount.EnglishName,
                            AccountArabicName = detailAccount.ArabicName,
                            PeriodDebit = 0,
                            PeriodCredit = 0,
                            BalanceDebit = 0,
                            BalanceCredit = 0,
                            PreviousDebit = 0,
                            PreviousCredit = 0,
                            Balance = 0,
                            Mains = new List<TrailBalanceResponseMainDto>()
                        };

                        foreach (var mainAccount in mainAccountsUnderDetailAccount)
                        {
                            var totalAccountTransactionEntries = totalTransactionEntries
                                .Where(x => x.MainAccountId == mainAccount.Id).ToList();
                            var periodAccountTransactionEntries = totalAccountTransactionEntries.Where(x =>
                                    x.Transaction.TransactionDate >= startDate && x.Transaction.TransactionDate <= endDate && !x.Transaction.IsOpeningTransaction)
                                .ToList();
                            var closestOpeningTransactionEntry = totalAccountTransactionEntries
                                .Where(x =>  x.Transaction.IsOpeningTransaction)
                                .OrderByDescending(x => x.Transaction.TransactionDate)
                                .FirstOrDefault();
                            var previousAccountTransactionEntries = totalAccountTransactionEntries
                                .Where(x => x.Transaction.TransactionDate < startDate 
                                            && !x.Transaction.IsOpeningTransaction
                                            )
                                .ToList();

                            if (closestOpeningTransactionEntry != null)
                            {
                                previousAccountTransactionEntries.Add(closestOpeningTransactionEntry);
                            }
                            

                            var periodMainAccountDebit = CalculateTotalDebit(periodAccountTransactionEntries);
                            var periodMainAccountCredit = CalculateTotalCredit(periodAccountTransactionEntries);
                            
                            var previousMainAccountDebit = CalculateTotalDebit(previousAccountTransactionEntries);
                            var previousMainAccountCredit = CalculateTotalCredit(previousAccountTransactionEntries);

                            var mainDto = new TrailBalanceResponseMainDto
                            {
                                AccountNumber = mainAccount.ReferenceCode,
                                AccountEnglishName = mainAccount.EnglishName,
                                AccountArabicName = mainAccount.ArabicName,
                                PeriodDebit = periodMainAccountDebit,
                                PeriodCredit = periodMainAccountCredit,
                                PreviousDebit = previousMainAccountDebit,
                                PreviousCredit = previousMainAccountCredit,
                                BalanceDebit = previousMainAccountDebit + periodMainAccountDebit,
                                BalanceCredit = previousMainAccountCredit + periodMainAccountCredit,
                                Balance = (periodMainAccountDebit - periodMainAccountCredit) + (previousMainAccountDebit - previousMainAccountCredit),
                            };
                            detailDto.PeriodDebit += periodMainAccountDebit;
                            detailDto.PeriodCredit += periodMainAccountCredit;
                            detailDto.PreviousDebit += previousMainAccountDebit;
                            detailDto.PreviousCredit += previousMainAccountCredit;
                            detailDto.BalanceDebit += previousMainAccountDebit + periodMainAccountDebit;
                            detailDto.BalanceCredit += previousMainAccountCredit + periodMainAccountCredit;
                            detailDto.Balance += (periodMainAccountDebit - periodMainAccountCredit) + (previousMainAccountDebit - previousMainAccountCredit);
                            detailDto.Mains.Add(mainDto);
                        }
                        headerDto.PeriodDebit += detailDto.PeriodDebit;
                        headerDto.PeriodCredit += detailDto.PeriodCredit;
                        headerDto.PreviousDebit += detailDto.PreviousDebit;
                        headerDto.PreviousCredit += detailDto.PreviousCredit;
                        headerDto.BalanceDebit += detailDto.PreviousDebit + detailDto.PeriodDebit;
                        headerDto.BalanceCredit += detailDto.PreviousCredit + detailDto.PeriodCredit;
                        headerDto.Balance += (detailDto.PreviousDebit - detailDto.PreviousCredit) + (detailDto.PeriodDebit - detailDto.PeriodCredit);
                        headerDto.Details.Add(detailDto);
                    }
                    rootDto.PeriodDebit += headerDto.PeriodDebit;
                    rootDto.PeriodCredit += headerDto.PeriodCredit;
                    rootDto.PreviousDebit += headerDto.PreviousDebit;
                    rootDto.PreviousCredit += headerDto.PreviousCredit;
                    rootDto.BalanceDebit += headerDto.PreviousDebit + headerDto.PeriodDebit;
                    rootDto.BalanceCredit += headerDto.PreviousCredit + headerDto.PeriodCredit;
                    rootDto.Balance += (headerDto.PreviousDebit - headerDto.PreviousCredit) + (headerDto.PeriodDebit - headerDto.PeriodCredit);
                    rootDto.Headers.Add(headerDto);

                }
                trailBalanceData.Add(rootDto);
            };

            return new OperationResponse<List<TrailBalanceResponseRootDto>>(ErrorCodes.Success, trailBalanceData);
        }
        catch (Exception e)
        {
            LogService.LogError(e);
        }
        return new OperationResponse<List<TrailBalanceResponseRootDto>>(ErrorCodes.GeneralFail, null);
    }

    public async Task<byte[]> GetDocument(DateOnly startDate, DateOnly endDate, string level)
    {
        try
        {
            // Get trail balance data
            var trailBalanceResponse = await GetTrailBalanceData(startDate, endDate);
            if (trailBalanceResponse?.Data == null || !trailBalanceResponse.Data.Any())
            {
                LogService.LogError($"Trail balance data is null or empty for startDate: {startDate}, endDate: {endDate}, level: {level}");
                throw new InvalidOperationException("Trail balance data not found");
            }

            // Get company details - use default/dummy data if not in database
            CompanyDetails companyDetails = new CompanyDetails
            {
                Id = Guid.NewGuid(),
                ArabicName = "Smart & Pro ERP",
                EnglishName = "Smart & Pro ERP",
                TaxNumber = "000000000000000",
                Email = "info@company.com",
                Website = "www.company.com"
            };
            
            try
            {
                var companyRepo = _repositoriesFactory.GetRepository<CompanyDetails>();
                var dbContext = companyRepo.GetDbContext();
                
                // Use ADO.NET directly to bypass EF Core completely
                var connection = dbContext.Database.GetDbConnection();
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                // Try to get company details from database
                command.CommandText = @"SELECT TOP 1 Id, ArabicName, EnglishName, TaxNumber, Email, Website, Logo 
                                       FROM CompanyDetails";
                
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    companyDetails = new CompanyDetails
                    {
                        Id = reader.GetGuid(0),
                        ArabicName = reader.IsDBNull(1) ? "Smart & Pro ERP" : reader.GetString(1),
                        EnglishName = reader.IsDBNull(2) ? "Smart & Pro ERP" : reader.GetString(2),
                        TaxNumber = reader.IsDBNull(3) ? "000000000000000" : reader.GetString(3),
                        Email = reader.IsDBNull(4) ? "info@company.com" : reader.GetString(4),
                        Website = reader.IsDBNull(5) ? "www.company.com" : reader.GetString(5),
                        Logo = reader.IsDBNull(6) ? null : (byte[])reader.GetValue(6)
                    };
                    
                    LogService.LogInformation($"Successfully loaded company from database: {companyDetails.ArabicName}");
                }
                else
                {
                    LogService.LogInformation("No CompanyDetails found in database - using default values");
                }
                
                await connection.CloseAsync();
            }
            catch (Exception ex)
            {
                LogService.LogError($"Error loading company details: {ex.Message}");
                LogService.LogInformation("Using default company details due to error");
            }

            // Generate PDF using TrailBalanceDocument
            var document = new DocumentUtils.TrailBalanceDocument(
                trailBalanceResponse.Data,
                companyDetails,
                startDate,
                endDate,
                level
            );
            
            var pdfBytes = document.GeneratePdf();
            
            LogService.LogInformation($"Generated PDF document for trail balance (startDate: {startDate}, endDate: {endDate}, level: {level}, size: {pdfBytes.Length} bytes)");
            
            return pdfBytes;
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            LogService.LogError($"GetDocument error details: {e.Message}\nStackTrace: {e.StackTrace}");
            if (e.InnerException != null)
            {
                LogService.LogError($"Inner exception: {e.InnerException.Message}");
            }
            throw;
        }
    }
}