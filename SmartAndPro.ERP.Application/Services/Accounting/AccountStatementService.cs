using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.AccountStatement;
using SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.ValueObjects;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Domains.Entities.Projects;

namespace SmartAndPro.ERP.Application.Services.Accounting;

public class AccountStatementService
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;

    public AccountStatementService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    public async Task<OperationResponse<AccountStatementResponseDto>> GetAccountStatement(DateOnly startDate, DateOnly endDate, Guid accountId, Guid? projectId = null, bool isFrontal = false)
    {
        try
        {
            var accountStatementData = new List<AccountStatementRecordDto>();

            decimal totalPreviousDebit = 0;
            decimal totalPreviousCredit = 0;
            
            var mainAccountQuery = await _repositoriesFactory.GetRepository<MainAccount>().Filter(x => x.Id == accountId);
            var mainAccount = await mainAccountQuery.Data.FirstOrDefaultAsync();
            if (mainAccount != null)
            {
                var previousBalance = await CalculatePreviousBalance(mainAccount.Id, startDate, projectId, isFrontal);
                totalPreviousDebit += previousBalance.PreviousDebit;
                totalPreviousCredit += previousBalance.PreviousCredit;
                await GetStatementForMainAccount(mainAccount, startDate, endDate, accountStatementData, projectId, isFrontal);
            }
            
            decimal runningBalance = 0;

            foreach (var statement in accountStatementData)
            {
                // Update the running balance for each statement entry
                runningBalance += statement.DebitAmount - statement.CreditAmount;

                // Set the balance in the statement entry
                statement.Balance = runningBalance;
            }

            var accountStatementResponse = new AccountStatementResponseDto
            {
                PreviousDebit = totalPreviousDebit,
                PreviousCredit = totalPreviousCredit,
                AccountStatementRecords = accountStatementData
            };
            return new OperationResponse<AccountStatementResponseDto>(ErrorCodes.Success, accountStatementResponse);
        }
        catch (Exception e)
        {
            LogService.LogError(e);
        }
        return new OperationResponse<AccountStatementResponseDto>(ErrorCodes.GeneralFail, null);
    }

    
    private async Task<PreviousBalance> CalculatePreviousBalance(Guid accountId, DateOnly startDate, Guid? projectId = null, bool isFrontal = false)
    {
        var totalAccountTransactionEntriesQuery = await _repositoriesFactory.GetRepository<TransactionEntry>()
            .Filter(x => x.MainAccountId == accountId);
        totalAccountTransactionEntriesQuery.Data = totalAccountTransactionEntriesQuery.Data
            .AsTracking()
            .Include(x => x.Transaction);
        if (isFrontal && !projectId.HasValue)
        {
            totalAccountTransactionEntriesQuery.Data = totalAccountTransactionEntriesQuery.Data.Where(x => x.Transaction.IsFrontal);
        }
    
        if (projectId.HasValue)
        {
            var connectionsQuery = await _repositoriesFactory.GetRepository<ReceiptConnection>()
                .Filter(rc => rc.ProjectId == projectId.Value);
            var connections = await connectionsQuery.Data.ToListAsync();
            var paycheckReceiptIds = connections.Where(rc => rc.PaycheckReceiptId.HasValue).Select(rc => rc.PaycheckReceiptId!.Value).ToList();
            var paymentReceiptIds = connections.Where(rc => rc.PaymentReceiptId.HasValue).Select(rc => rc.PaymentReceiptId!.Value).ToList();

            // Fetch transaction IDs from PaycheckReceipts
            var paycheckReceiptsRepo = _repositoriesFactory.GetRepository<PaycheckReceipt>();
            var paycheckReceiptsQuery = await paycheckReceiptsRepo.Filter(pr => paycheckReceiptIds.Contains(pr.Id));
            var matchingPaycheckTransactionIds = await paycheckReceiptsQuery.Data
                .Where(pr => pr.TransactionId != null)
                .Select(pr => pr.TransactionId!.Value)
                .ToListAsync();

            // Fetch transaction IDs from PaymentReceipts
            var paymentReceiptsRepo = _repositoriesFactory.GetRepository<PaymentReceipt>();
            var paymentReceiptsQuery = await paymentReceiptsRepo.Filter(pr => paymentReceiptIds.Contains(pr.Id));
            var matchingPaymentTransactionIds = await paymentReceiptsQuery.Data
                .Where(pr => pr.TransactionId != null)
                .Select(pr => pr.TransactionId!.Value)
                .ToListAsync();

            totalAccountTransactionEntriesQuery.Data = totalAccountTransactionEntriesQuery.Data.Where(x =>
                (x.Transaction.PaycheckReceiptId.HasValue && paycheckReceiptIds.Contains(x.Transaction.PaycheckReceiptId.Value)) ||
                (x.Transaction.PaymentReceiptId.HasValue && paymentReceiptIds.Contains(x.Transaction.PaymentReceiptId.Value)) ||
                matchingPaycheckTransactionIds.Contains(x.TransactionId) ||
                matchingPaymentTransactionIds.Contains(x.TransactionId)
            );
        }

        var totalAccountTransactionEntries = await totalAccountTransactionEntriesQuery.Data.ToListAsync();

        var closestOpeningTransactionEntry = totalAccountTransactionEntries
            .Where(x => x.Transaction.IsOpeningTransaction)
            .OrderByDescending(x => x.Transaction.TransactionDate)
            .FirstOrDefault();

        var previousAccountTransactionEntries = totalAccountTransactionEntries
            .Where(x => x.Transaction.TransactionDate < startDate 
                        && !x.Transaction.IsOpeningTransaction
                        )
            .ToList();

        if (closestOpeningTransactionEntry !=null)
        {
            previousAccountTransactionEntries.Add(closestOpeningTransactionEntry);
        }

        var previousDebit = previousAccountTransactionEntries.Sum(x => x.DebitAmount);
        var previousCredit = previousAccountTransactionEntries.Sum(x => x.CreditAmount);

        return new PreviousBalance
        {
            PreviousDebit = previousDebit,
            PreviousCredit = previousCredit
        };
    }

    private async Task GetStatementForMainAccount(MainAccount mainAccount, DateOnly startDate, DateOnly endDate,
        List<AccountStatementRecordDto> accountStatementData, Guid? projectId = null, bool isFrontal = false)
    {
        var transactionEntriesQuery =
            await _repositoriesFactory.GetRepository<TransactionEntry>()
                .Filter(
                    x => x.MainAccountId == mainAccount.Id
                    && x.Transaction.TransactionDate >= startDate
                    && x.Transaction.TransactionDate <= endDate
                    && !x.Transaction.IsOpeningTransaction
                    );
        transactionEntriesQuery.Data = transactionEntriesQuery.Data
            .AsTracking()
            .Include(x => x.Transaction)
                .ThenInclude(t => t.TransactionType);
        if (isFrontal && !projectId.HasValue)
        {
            transactionEntriesQuery.Data = transactionEntriesQuery.Data.Where(x => x.Transaction.IsFrontal);
        }

        if (projectId.HasValue)
        {
            var connectionsQuery = await _repositoriesFactory.GetRepository<ReceiptConnection>()
                .Filter(rc => rc.ProjectId == projectId.Value);
            var connections = await connectionsQuery.Data.ToListAsync();
            var paycheckReceiptIds = connections.Where(rc => rc.PaycheckReceiptId.HasValue).Select(rc => rc.PaycheckReceiptId!.Value).ToList();
            var paymentReceiptIds = connections.Where(rc => rc.PaymentReceiptId.HasValue).Select(rc => rc.PaymentReceiptId!.Value).ToList();

            // Fetch transaction IDs from PaycheckReceipts
            var paycheckReceiptsRepo = _repositoriesFactory.GetRepository<PaycheckReceipt>();
            var paycheckReceiptsQuery = await paycheckReceiptsRepo.Filter(pr => paycheckReceiptIds.Contains(pr.Id));
            var matchingPaycheckTransactionIds = await paycheckReceiptsQuery.Data
                .Where(pr => pr.TransactionId != null)
                .Select(pr => pr.TransactionId!.Value)
                .ToListAsync();

            // Fetch transaction IDs from PaymentReceipts
            var paymentReceiptsRepo = _repositoriesFactory.GetRepository<PaymentReceipt>();
            var paymentReceiptsQuery = await paymentReceiptsRepo.Filter(pr => paymentReceiptIds.Contains(pr.Id));
            var matchingPaymentTransactionIds = await paymentReceiptsQuery.Data
                .Where(pr => pr.TransactionId != null)
                .Select(pr => pr.TransactionId!.Value)
                .ToListAsync();

            transactionEntriesQuery.Data = transactionEntriesQuery.Data.Where(x =>
                (x.Transaction.PaycheckReceiptId.HasValue && paycheckReceiptIds.Contains(x.Transaction.PaycheckReceiptId.Value)) ||
                (x.Transaction.PaymentReceiptId.HasValue && paymentReceiptIds.Contains(x.Transaction.PaymentReceiptId.Value)) ||
                matchingPaycheckTransactionIds.Contains(x.TransactionId) ||
                matchingPaymentTransactionIds.Contains(x.TransactionId)
            );
        }

        var transactionEntries = await transactionEntriesQuery.Data
                                .OrderBy(x => x.Transaction.TransactionDate)
                                .ToListAsync();

        var paycheckIds = transactionEntries.Where(x => x.Transaction.PaycheckReceiptId.HasValue).Select(x => x.Transaction.PaycheckReceiptId!.Value).Distinct().ToList();
        var paymentIds = transactionEntries.Where(x => x.Transaction.PaymentReceiptId.HasValue).Select(x => x.Transaction.PaymentReceiptId!.Value).Distinct().ToList();

        var prjConnectionsQuery = await _repositoriesFactory.GetRepository<ReceiptConnection>()
            .Filter(rc => (rc.PaycheckReceiptId.HasValue && paycheckIds.Contains(rc.PaycheckReceiptId.Value))
                       || (rc.PaymentReceiptId.HasValue && paymentIds.Contains(rc.PaymentReceiptId.Value)));
        
        var prjConnections = await prjConnectionsQuery.Data.Include(rc => rc.Project).ToListAsync();
        
        var projectNamesByPaycheck = prjConnections
            .Where(rc => rc.PaycheckReceiptId.HasValue && rc.Project != null)
            .GroupBy(rc => rc.PaycheckReceiptId!.Value)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(rc => rc.Project!.ProjectName)));

        var projectNamesByPayment = prjConnections
            .Where(rc => rc.PaymentReceiptId.HasValue && rc.Project != null)
            .GroupBy(rc => rc.PaymentReceiptId!.Value)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(rc => rc.Project!.ProjectName)));

        

        foreach (var entry in transactionEntries)
        {
            string projectSuffix = "";
            if (entry.Transaction.PaycheckReceiptId.HasValue && projectNamesByPaycheck.TryGetValue(entry.Transaction.PaycheckReceiptId.Value, out var prjName))
            {
                projectSuffix = $" [{prjName}]";
            }
            else if (entry.Transaction.PaymentReceiptId.HasValue && projectNamesByPayment.TryGetValue(entry.Transaction.PaymentReceiptId.Value, out var prjName2))
            {
                projectSuffix = $" [{prjName2}]";
            }

            var description = entry?.Description ?? entry?.Transaction?.Description;
            if (!string.IsNullOrEmpty(projectSuffix) && !string.IsNullOrEmpty(description))
            {
                description = $"{projectSuffix} - {description}";
            }

            accountStatementData.Add(new AccountStatementRecordDto
            {
                IsTransactionPost1 = entry.Transaction.IsTransactionPost1,
                TransactionDate = entry.Transaction.TransactionDate,
                TransactionNumber = entry.Transaction.ReferenceNumber,
                TransactionId = entry.Transaction.Id,
                DebitAmount = entry.DebitAmount,
                CreditAmount = entry.CreditAmount,
                Balance = 0,
                Description = description,
                TransactionTypeArabicName = entry.Transaction.TransactionType.ArabicName,
                TransactionTypeEnglishName = entry.Transaction.TransactionType.EnglishName,
                AccountId = mainAccount.Id
            });
        }
    }

    public async Task<byte[]> GetDocument(DateOnly startDate, DateOnly endDate, Guid accountId, Guid? projectId = null, bool isFrontal = false)
    {
        try
        {
            var accountStatementResponse = await GetAccountStatement(startDate, endDate, accountId, projectId, isFrontal);
            if (accountStatementResponse?.Data == null)
            {
                LogService.LogError($"Account statement data is null for accountId: {accountId}, startDate: {startDate}, endDate: {endDate}");
                throw new InvalidOperationException("Account statement data not found");
            }

            var mainAccountQuery = await _repositoriesFactory.GetRepository<MainAccount>().Filter(x => x.Id == accountId);
            var mainAccount = await mainAccountQuery.Data.FirstOrDefaultAsync();
            if (mainAccount == null)
            {
                LogService.LogError($"MainAccount not found for accountId: {accountId}");
                throw new InvalidOperationException("Main account not found");
            }

            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
            if (companyDetails == null)
            {
                LogService.LogError("CompanyDetails not found");
                throw new InvalidOperationException("Company details not found");
            }

            var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
            var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

            var document = new AccountStatementDocument(
                accountStatementResponse.Data,
                mainAccount,
                companyDetails,
                pdfTemplateConfig,
                startDate,
                endDate
            );

            var pdfBytes = document.GeneratePdf();
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                LogService.LogError("Generated PDF is null or empty");
                throw new InvalidOperationException("Failed to generate PDF document");
            }

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
            throw; // Re-throw to let controller handle it
        }
    }

    public async Task<byte[]> GetSummaryDocument(DateOnly startDate, DateOnly endDate, Guid accountId, Guid? projectId = null, bool isFrontal = false)
    {
        try
        {
            var accountStatementResponse = await GetAccountStatement(startDate, endDate, accountId, projectId, isFrontal);
            if (accountStatementResponse?.Data == null)
            {
                LogService.LogError($"Account statement data is null for accountId: {accountId}, startDate: {startDate}, endDate: {endDate}");
                throw new InvalidOperationException("Account statement data not found");
            }

            var mainAccountQuery = await _repositoriesFactory.GetRepository<MainAccount>().Filter(x => x.Id == accountId);
            var mainAccount = await mainAccountQuery.Data.FirstOrDefaultAsync();
            if (mainAccount == null)
            {
                LogService.LogError($"MainAccount not found for accountId: {accountId}");
                throw new InvalidOperationException("Main account not found");
            }

            var companyDetailsQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().GetAll();
            var companyDetails = companyDetailsQuery.Data?.FirstOrDefault();
            if (companyDetails == null)
            {
                LogService.LogError("CompanyDetails not found");
                throw new InvalidOperationException("Company details not found");
            }

            var pdfTemplateConfigQuery = await _repositoriesFactory.GetRepository<Domains.Entities.PdfTemplateConfiguration>().GetAll();
            var pdfTemplateConfig = pdfTemplateConfigQuery.Data?.FirstOrDefault();

            var document = new SummaryAccountStatementDocument(
                accountStatementResponse.Data,
                mainAccount,
                companyDetails,
                pdfTemplateConfig,
                startDate,
                endDate
            );

            var pdfBytes = document.GeneratePdf();
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                LogService.LogError("Generated summary PDF is null or empty");
                throw new InvalidOperationException("Failed to generate summary PDF document");
            }

            return pdfBytes;
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            LogService.LogError($"GetSummaryDocument error details: {e.Message}\nStackTrace: {e.StackTrace}");
            if (e.InnerException != null)
            {
                LogService.LogError($"Inner exception: {e.InnerException.Message}");
            }
            throw;
        }
    }
}

public class PreviousBalance
{
    public decimal PreviousDebit { get; set; }
    public decimal PreviousCredit { get; set; }
}

public class AccountStatementDocument : IDocument
{
    private readonly AccountStatementResponseDto _accountStatement;
    private readonly MainAccount _mainAccount;
    private readonly CompanyDetails _companyDetails;
    private readonly Domains.Entities.PdfTemplateConfiguration _pdfTemplateConfig;
    private readonly DateOnly _startDate;
    private readonly DateOnly _endDate;

    public AccountStatementDocument(
        AccountStatementResponseDto accountStatement,
        MainAccount mainAccount,
        CompanyDetails companyDetails,
        Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null,
        DateOnly startDate = default,
        DateOnly endDate = default)
    {
        _accountStatement = accountStatement;
        _mainAccount = mainAccount;
        _companyDetails = companyDetails;
        _pdfTemplateConfig = pdfTemplateConfig ?? new Domains.Entities.PdfTemplateConfiguration();
        _startDate = startDate;
        _endDate = endDate;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        try
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(10));

                PdfTemplateUtils.ApplyHeaderFooter(
                page,
                _pdfTemplateConfig,
                (pg, header) => pg.Header().Element(c => PdfTemplateUtils.RenderHeaderFooter(c, header, _companyDetails)),
                (pg, footer) => pg.Footer().Element(c => PdfTemplateUtils.RenderHeaderFooter(c, footer, _companyDetails)),
                new PdfHeaderFooterConfig
                {
                    Columns = new List<PdfColumnGroup>
                    {
                        new PdfColumnGroup
                        {
                            Rows = new List<PdfRowGroup>
                            {
                                new PdfRowGroup
                                {
                                    Items = new List<PdfItem>
                                    {
                                        new PdfItem{ Id = "logo", Type = "field", FontSize = "inherit", Align = "center" },
                                        new PdfItem{ Id = "englishName", Type = "field", FontSize = "14px", Align = "center" }
                                    }
                                }
                            }
                        }
                    }
                },
                new PdfHeaderFooterConfig
                {
                    Columns = new List<PdfColumnGroup>
                    {
                        new PdfColumnGroup
                        {
                            Rows = new List<PdfRowGroup>
                            {
                                new PdfRowGroup
                                {
                                    Items = new List<PdfItem>
                                    {
                                        new PdfItem{ Id = "primaryPhone", Type = "field", FontSize = "inherit", Align = "center" },
                                        new PdfItem{ Id = "email", Type = "field", FontSize = "inherit", Align = "center" },
                                        new PdfItem{ Id = "website", Type = "field", FontSize = "inherit", Align = "right" }
                                    }
                                }
                            }
                        }
                    }
                }
            );

            page.Content().Column(col =>
            {
                // Header
                col.Item().PaddingBottom(10).Border(1).Padding(8).Row(r =>
                {
                    // التاريخ في أقصى الشمال (Left)
                    r.RelativeItem().AlignLeft().Column(c =>
                    {
                        if (_startDate != default && _endDate != default)
                        {
                            c.Item().Text(t =>
                            {
                                t.Span($"من: {_startDate:dd/MM/yyyy}").SemiBold();
                            });
                            c.Item().PaddingTop(2).Text(t =>
                            {
                                t.Span($"إلى: {_endDate:dd/MM/yyyy}").SemiBold();
                            });
                        }
                    });
                    
                    // العنوان في المنتصف (Center)
                    r.RelativeItem().AlignCenter().Text(t =>
                    {
                        t.Span("كشف حساب").SemiBold().FontSize(16);
                    });
                    
                    // اسم الحساب في أقصى اليمين (Right)
                    r.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().Text(t =>
                        {
                            t.Span("الحساب: ").SemiBold();
                            t.Span($"{_mainAccount.ReferenceCode} - {_mainAccount.ArabicName}");
                        });
                    });
                });

                // Table - True RTL Order (Right to Left: تاريخ القيد - رقم القيد - نوع القيد - الوصف - مدين - دائن - الرصيد)
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(85); // Date (تاريخ القيد) - Rightmost
                        columns.ConstantColumn(75); // Transaction Number (رقم القيد)
                        columns.ConstantColumn(80); // Transaction Type (نوع القيد)
                        columns.RelativeColumn(1f); // Description (الوصف)
                        columns.ConstantColumn(70); // Debit (مدين)
                        columns.ConstantColumn(70); // Credit (دائن)
                        columns.ConstantColumn(80); // Balance (الرصيد) - Leftmost
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderStyle).AlignCenter().Text("تاريخ القيد");
                        header.Cell().Element(HeaderStyle).AlignCenter().Text("رقم القيد");
                        header.Cell().Element(HeaderStyle).AlignCenter().Text("نوع القيد");
                        header.Cell().Element(HeaderStyle).AlignCenter().Text("الوصف");
                        header.Cell().Element(HeaderStyle).AlignCenter().Text("مدين");
                        header.Cell().Element(HeaderStyle).AlignCenter().Text("دائن");
                        header.Cell().Element(HeaderStyle).AlignCenter().Text("الرصيد");
                    });

                    // Previous Balance Row
                    if (_accountStatement.PreviousDebit != 0 || _accountStatement.PreviousCredit != 0)
                    {
                        var previousBalance = _accountStatement.PreviousDebit - _accountStatement.PreviousCredit;
                        table.Cell().Element(DataStyle).AlignCenter().Text("الرصيد السابق");
                        table.Cell().Element(DataStyle).AlignCenter().Text("-");
                        table.Cell().Element(DataStyle).AlignCenter().Text("-");
                        table.Cell().Element(DataStyle).AlignCenter().Text("الرصيد السابق");
                        table.Cell().Element(DataStyle).AlignCenter().Text(_accountStatement.PreviousDebit.ToString("0.##"));
                        table.Cell().Element(DataStyle).AlignCenter().Text(_accountStatement.PreviousCredit.ToString("0.##"));
                        table.Cell().Element(DataStyle).AlignCenter().Text(previousBalance.ToString("0.##"));
                    }

                    // Data Rows
                    decimal runningBalance = _accountStatement.PreviousDebit - _accountStatement.PreviousCredit;
                    decimal totalDebit = _accountStatement.PreviousDebit;
                    decimal totalCredit = _accountStatement.PreviousCredit;
                    
                    foreach (var record in _accountStatement.AccountStatementRecords)
                    {
                        runningBalance += record.DebitAmount - record.CreditAmount;
                        totalDebit += record.DebitAmount;
                        totalCredit += record.CreditAmount;
                        
                        table.Cell().Element(DataStyle).AlignCenter().Text(record.TransactionDate.ToString("dd/MM/yyyy"));
                        table.Cell().Element(DataStyle).AlignCenter().Text(string.IsNullOrWhiteSpace(record.TransactionNumber) ? "-" : record.TransactionNumber);
                        table.Cell().Element(DataStyle).AlignCenter().Text(string.IsNullOrWhiteSpace(record.TransactionTypeArabicName) ? "-" : record.TransactionTypeArabicName);
                        table.Cell().Element(DataStyle).AlignRight().Text(string.IsNullOrWhiteSpace(record.Description) ? "-" : record.Description);
                        table.Cell().Element(DataStyle).AlignCenter().Text(record.DebitAmount == 0 ? "-" : record.DebitAmount.ToString("0.##"));
                        table.Cell().Element(DataStyle).AlignCenter().Text(record.CreditAmount == 0 ? "-" : record.CreditAmount.ToString("0.##"));
                        table.Cell().Element(DataStyle).AlignCenter().Text(runningBalance.ToString("0.##"));
                    }

                    // Totals Row - RTL Order
                    table.Cell().ColumnSpan(4).Element(TotalStyle).AlignCenter().Text("الإجمالي");
                    table.Cell().Element(TotalStyle).AlignCenter().Text(totalDebit.ToString("0.##"));
                    table.Cell().Element(TotalStyle).AlignCenter().Text(totalCredit.ToString("0.##"));
                    table.Cell().Element(TotalStyle).AlignCenter().Text(runningBalance.ToString("0.##"));
                });

                // Signatures Section - Safe margins, Company on Right, Customer on Left
                col.Item().PaddingTop(30).Row(r =>
                {
                    // Company signature on the RIGHT (RTL)
                    r.RelativeItem(1).PaddingRight(10).Column(c =>
                    {
                        var companyName = _companyDetails.ArabicName ?? _companyDetails.EnglishName ?? "";
                        c.Item().AlignRight().Text(companyName).SemiBold().FontSize(11);
                        c.Item().PaddingTop(6).AlignRight().Text("التوقيع: ........................................").FontSize(9);
                        
                        if (_companyDetails.Stamp != null && PdfTemplateUtils.IsValidImageBytes(_companyDetails.Stamp))
                        {
                            c.Item().PaddingTop(8).AlignRight().Height(70).Width(70).Image(_companyDetails.Stamp).FitArea();
                        }
                    });

                    // Customer signature on the LEFT (RTL)
                    r.RelativeItem(1).PaddingLeft(10).Column(c =>
                    {
                        var customerName = _mainAccount.ClientProfile != null 
                            ? (_mainAccount.ClientProfile.ArabicName ?? _mainAccount.ClientProfile.EnglishName ?? _mainAccount.ArabicName ?? _mainAccount.EnglishName)
                            : (_mainAccount.ArabicName ?? _mainAccount.EnglishName);

                        c.Item().AlignLeft().Text(customerName).SemiBold().FontSize(11);
                        c.Item().PaddingTop(6).AlignLeft().Text("التوقيع: ........................................").FontSize(9);
                    });
                });
            });
        });
    }
        catch (Exception ex)
        {
            LogService.LogError($"Error in AccountStatementDocument.Compose: {ex.Message}");
            LogService.LogError($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                LogService.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    static IContainer HeaderStyle(IContainer c) => c.Background("#F1F5F9").Border(1).Padding(4).DefaultTextStyle(t => t.SemiBold());
    static IContainer DataStyle(IContainer c) => c.BorderBottom(1).BorderColor("#E2E8F0").Padding(2);
    static IContainer TotalStyle(IContainer c) => c.Background("#FFE4B5").Border(1).BorderColor("#000000").Padding(4).DefaultTextStyle(t => t.SemiBold());
}

public class SummaryAccountStatementDocument : IDocument
{
    private readonly AccountStatementResponseDto _accountStatement;
    private readonly MainAccount _mainAccount;
    private readonly CompanyDetails _companyDetails;
    private readonly Domains.Entities.PdfTemplateConfiguration _pdfTemplateConfig;
    private readonly DateOnly _startDate;
    private readonly DateOnly _endDate;

    public SummaryAccountStatementDocument(
        AccountStatementResponseDto accountStatement,
        MainAccount mainAccount,
        CompanyDetails companyDetails,
        Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null,
        DateOnly startDate = default,
        DateOnly endDate = default)
    {
        _accountStatement = accountStatement;
        _mainAccount = mainAccount;
        _companyDetails = companyDetails;
        _pdfTemplateConfig = pdfTemplateConfig ?? new Domains.Entities.PdfTemplateConfiguration();
        _startDate = startDate;
        _endDate = endDate;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        try
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(10));

                PdfTemplateUtils.ApplyHeaderFooter(
                    page,
                    _pdfTemplateConfig,
                    (pg, header) => pg.Header().Element(c => PdfTemplateUtils.RenderHeaderFooter(c, header, _companyDetails)),
                    (pg, footer) => pg.Footer().Element(c => PdfTemplateUtils.RenderHeaderFooter(c, footer, _companyDetails)),
                    new PdfHeaderFooterConfig
                    {
                        Columns = new List<PdfColumnGroup>
                        {
                            new PdfColumnGroup
                            {
                                Rows = new List<PdfRowGroup>
                                {
                                    new PdfRowGroup
                                    {
                                        Items = new List<PdfItem>
                                        {
                                            new PdfItem{ Id = "logo", Type = "field", FontSize = "inherit", Align = "center" },
                                            new PdfItem{ Id = "englishName", Type = "field", FontSize = "14px", Align = "center" }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new PdfHeaderFooterConfig
                    {
                        Columns = new List<PdfColumnGroup>
                        {
                            new PdfColumnGroup
                            {
                                Rows = new List<PdfRowGroup>
                                {
                                    new PdfRowGroup
                                    {
                                        Items = new List<PdfItem>
                                        {
                                            new PdfItem{ Id = "primaryPhone", Type = "field", FontSize = "inherit", Align = "center" },
                                            new PdfItem{ Id = "email", Type = "field", FontSize = "inherit", Align = "center" },
                                            new PdfItem{ Id = "website", Type = "field", FontSize = "inherit", Align = "right" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                );

                page.Content().Column(col =>
                {
                    // Header
                    col.Item().PaddingBottom(15).Border(1).BorderColor("#2563eb").Background("#EFF6FF").Padding(15).Row(r =>
                    {
                        // التاريخ في أقصى الشمال (Left)
                        r.RelativeItem().AlignLeft().Column(c =>
                        {
                            if (_startDate != default && _endDate != default)
                            {
                                c.Item().Text(t =>
                                {
                                    t.Span($"من: {_startDate:yyyy-MM-dd}").SemiBold();
                                });
                                c.Item().PaddingTop(2).Text(t =>
                                {
                                    t.Span($"إلى: {_endDate:yyyy-MM-dd}").SemiBold();
                                });
                            }
                        });
                        
                        // العنوان في المنتصف (Center)
                        r.RelativeItem().AlignCenter().Text(t =>
                        {
                            t.Span("كشف حساب مختصر").SemiBold().FontSize(16);
                        });
                        
                        // اسم الحساب في أقصى اليمين (Right)
                        r.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text(t =>
                            {
                                t.Span("الحساب: ").SemiBold();
                                t.Span($"{_mainAccount.ReferenceCode} - {_mainAccount.ArabicName}");
                            });
                        });
                    });

                    // Calculate totals
                    decimal totalDebit = _accountStatement.PreviousDebit;
                    decimal totalCredit = _accountStatement.PreviousCredit;
                    decimal runningBalance = _accountStatement.PreviousDebit - _accountStatement.PreviousCredit;
                    
                    foreach (var record in _accountStatement.AccountStatementRecords)
                    {
                        runningBalance += record.DebitAmount - record.CreditAmount;
                        totalDebit += record.DebitAmount;
                        totalCredit += record.CreditAmount;
                    }

                    // Summary Table (only totals)
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1); // Icon/Total label
                            columns.RelativeColumn(1); // Debit
                            columns.RelativeColumn(1); // Credit
                            columns.RelativeColumn(1); // Balance
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).AlignCenter().Text("الرصيد");
                            header.Cell().Element(HeaderStyle).AlignCenter().Text("دائن");
                            header.Cell().Element(HeaderStyle).AlignCenter().Text("مدين");
                            header.Cell().Element(HeaderStyle).AlignCenter().Text("");
                        });

                        // Totals Row - RTL order (matching header order: دائن, مدين, الرصيد, "")
                        table.Cell().Element(TotalStyle).AlignCenter().Text(runningBalance.ToString("0.##")); // تحت "الرصيد"
                        table.Cell().Element(TotalStyle).AlignCenter().Text(totalCredit.ToString("0.##")); // تحت "دائن"
                        table.Cell().Element(TotalStyle).AlignCenter().Text(totalDebit.ToString("0.##")); // تحت "مدين"
                        table.Cell().Element(TotalStyle).AlignCenter().Text("الإجمالي"); // تحت "" (الفارغ)
                    });

                    // Signatures Section - Customer on the left, Company on the right (RTL)
                    col.Item().PaddingTop(30).Row(r =>
                    {
                        // Customer signature on the left (RTL)
                        r.RelativeItem(2).Column(c =>
                        {
                            // Customer name - get from ClientProfile or MainAccount
                            var customerName = _mainAccount.ClientProfile != null 
                                ? (_mainAccount.ClientProfile.ArabicName ?? _mainAccount.ClientProfile.EnglishName ?? _mainAccount.ArabicName ?? _mainAccount.EnglishName)
                                : (_mainAccount.ArabicName ?? _mainAccount.EnglishName);
                            var isCustomerArabic = _mainAccount.ClientProfile != null 
                                ? !string.IsNullOrWhiteSpace(_mainAccount.ClientProfile.ArabicName)
                                : !string.IsNullOrWhiteSpace(_mainAccount.ArabicName);
                            
                            if (isCustomerArabic)
                            {
                                c.Item().AlignRight().Text(t =>
                                {
                                    t.Span(customerName).SemiBold().FontSize(12);
                                });
                            }
                            else
                            {
                                c.Item().AlignLeft().Text(t =>
                                {
                                    t.Span(customerName).SemiBold().FontSize(12);
                                });
                            }
                            
                            // "التوقيع" - directly under customer name
                            c.Item().PaddingTop(5).AlignRight().Text(t =>
                            {
                                t.Span("التوقيع").SemiBold().FontSize(10);
                            });
                            
                            // Dotted line - under signature label
                            c.Item().PaddingTop(5).AlignRight().Text("................................................................................................................").FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                        
                        // Company signature on the right (RTL)
                        r.RelativeItem(2).Column(c =>
                        {
                            // Company name - aligned based on language (Arabic = right, English = left)
                            var companyName = _companyDetails.ArabicName ?? _companyDetails.EnglishName ?? "";
                            var isArabic = !string.IsNullOrWhiteSpace(_companyDetails.ArabicName);
                            
                            if (isArabic)
                            {
                                c.Item().AlignRight().Text(t =>
                                {
                                    t.Span(companyName).SemiBold().FontSize(12);
                                });
                            }
                            else
                            {
                                c.Item().AlignLeft().Text(t =>
                                {
                                    t.Span(companyName).SemiBold().FontSize(12);
                                });
                            }
                            
                            // "التوقيع" - directly under company name
                            c.Item().PaddingTop(5).AlignRight().Text(t =>
                            {
                                t.Span("التوقيع").SemiBold().FontSize(10);
                            });
                            
                            // Dotted line - under signature label
                            c.Item().PaddingTop(5).AlignRight().Text(".............................................................................................................").FontSize(8).FontColor(Colors.Grey.Medium);
                            
                            // Company stamp - under dotted line
                            if (_companyDetails.Stamp != null && _companyDetails.Stamp.Length > 0)
                            {
                                c.Item().PaddingTop(5).AlignRight().Height(80).Width(80).Image(_companyDetails.Stamp).FitArea();
                            }
                        });
                    });
                });
            });
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error in SummaryAccountStatementDocument.Compose: {ex.Message}");
            LogService.LogError($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                LogService.LogError($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    static IContainer HeaderStyle(IContainer c) => c.Background("#42acff").Border(1).BorderColor("#E2E8F0").Padding(4).DefaultTextStyle(t => t.SemiBold().FontColor("#ffffff"));
    static IContainer DataStyle(IContainer c) => c.Border(1).BorderColor("#E2E8F0").Padding(2);
    static IContainer TotalStyle(IContainer c) => c.Background("#E2E8F0").Border(1).BorderColor("#000000").Padding(4).DefaultTextStyle(t => t.SemiBold());
}