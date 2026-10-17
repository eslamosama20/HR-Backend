using AutoMapper;
using Bee.ZatcaHelper;
using Bee.ZatcaHelper.Contracts;
using Bee.ZatcaHelper.Model;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities.Encoders;
using Serilog;
using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.BankAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyDetails;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPhone;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPolicy;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.ValueObjects;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System.Text;
using System.Text.Json;
using Zatca.EInvoice.SDK;
using Zatca.EInvoice.SDK.Contracts.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement;

public class CompanyService
    : ServiceBase
    <
        CompanyDetails,
        CompanyDetailsPostDto,
        CompanyDetailsPutDto,
        CompanyDetailsResponseDto,
        CompanyDetailsFiltersDto
    >, ICompanyService

{
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly RepositoriesFactory _repositoriesFactory;

    public CompanyService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public override async Task<OperationResponse<CompanyDetailsResponseDto>> DeleteAsync(Guid id)
    {
        var result = new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.GeneralFail, null);

        try
        {
            // Get the company by ID 
            var companyQuery = await _repositoriesFactory.GetRepository<CompanyDetails>().Filter((c) => c.Id == id);

            var company = companyQuery.Data
                .Include(c => c.Phones)
                .Include(c=>c.Policies)
                .Include(c=>c.BankAccounts)
                .FirstOrDefault();

            if (company == null)
                return new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.NotFound, null);


            if (company.Phones != null && company.Phones.Any())
            {
                foreach (var phone in company.Phones)
                {
                    phone.Company = null;
                    await _repositoriesFactory.GetRepository<CompanyPhone>().Delete(phone);

                }

            }

            company.Phones = null;

            if(company.BankAccounts !=null && company.BankAccounts.Any())
            {
                foreach(var bankAccount in company.BankAccounts)
                {
                    bankAccount.Company = null;
                    await _repositoriesFactory.GetRepository<BankAccount>().Delete(bankAccount);
                }
            }

            company.BankAccounts = null;
            if (company.Policies != null && company.Policies.Any())
            {
                foreach (var policy in company.Policies)
                {
                    policy.Company = null;
                    await _repositoriesFactory.GetRepository<CompanyPolicy>().Delete(policy);

                }

            }

            company.Policies = null;

            var deleteOperation = await _repositoriesFactory.GetRepository<CompanyDetails>().Delete(company);

            if (deleteOperation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<CompanyDetailsResponseDto>(deleteOperation.ErrorCode, null);


            return new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.Success, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    public override async Task<OperationResponse<CompanyDetailsResponseDto>> AddAsync(CompanyDetailsPostDto companyDetailsPostDto)
    {
        var result = new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.GeneralFail, null);
        try
        {
            // Map the basic properties using AutoMapper but ignore the collections
            var companyDetails = _mapper.Map<CompanyDetails>(companyDetailsPostDto);
            
            // Clear the collections to avoid issues
            companyDetails.Phones = new List<CompanyPhone>();
            companyDetails.BankAccounts = new List<BankAccount>();
            companyDetails.Policies = new List<CompanyPolicy>();
            
            // Add the company first to get the ID
            var addResult = await _repositoriesFactory.GetRepository<CompanyDetails>().Add(companyDetails);
            if (addResult.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<CompanyDetailsResponseDto>(addResult.ErrorCode, null);
            
            var savedCompany = addResult.Data;
            
            // Add phones
            if (companyDetailsPostDto.Phones != null)
            {
                foreach (var phoneDto in companyDetailsPostDto.Phones)
                {
                    var phone = new CompanyPhone
                    {
                        PhoneNumber = phoneDto.PhoneNumber,
                        IsPrimary = phoneDto.IsPrimary,
                        CompanyId = savedCompany.Id
                    };
                    savedCompany.Phones.Add(phone);
                }
            }
            
            // Add bank accounts
            if (companyDetailsPostDto.BankAccounts != null)
            {
                foreach (var bankAccountDto in companyDetailsPostDto.BankAccounts)
                {
                    var bankAccount = new BankAccount
                    {
                        BankName = bankAccountDto.BankName,
                        BankAccountNumber = bankAccountDto.BankAccountNumber,
                        AccountName = bankAccountDto.AccountName,
                        BankBranchName = bankAccountDto.BankBranchName,
                        SwiftCode = bankAccountDto.SwiftCode,
                        IBAN = bankAccountDto.IBAN,
                        IsPrimary = bankAccountDto.IsPrimary,
                        CompanyId = savedCompany.Id
                    };
                    savedCompany.BankAccounts.Add(bankAccount);
                }
            }
            
            // Add policies
            if (companyDetailsPostDto.Policies != null)
            {
                foreach (var policyDto in companyDetailsPostDto.Policies)
                {
                    var policy = new CompanyPolicy
                    {
                        PolicyParagraph = policyDto.PolicyParagraph,
                        IsPrimary = policyDto.IsPrimary,
                        CompanyId = savedCompany.Id
                    };
                    savedCompany.Policies.Add(policy);
                }
            }
            
            // Update the company with the collections
            var updateResult = await _repositoriesFactory.GetRepository<CompanyDetails>().Update(savedCompany);
            if (updateResult.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<CompanyDetailsResponseDto>(updateResult.ErrorCode, null);
            
            var responseData = _mapper.Map<CompanyDetailsResponseDto>(updateResult.Data);
            return new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    public override async Task<OperationResponse<CompanyDetailsResponseDto>> UpdateAsync(
        CompanyDetailsPutDto companyDetailsPutDto)
    {
        var companyDetailsRepository = _repositoriesFactory.GetRepository<CompanyDetails>();
        var companyDetailsQuery = await companyDetailsRepository.Filter(x => x.Id == companyDetailsPutDto.Id);
        var companyDetails = await companyDetailsQuery.Data
            .Include(c => c.Phones)
            .Include(c => c.Policies)
            .Include(c=>c.BankAccounts)
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync();
        if (companyDetails == null) return new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.NotFound, null);

        companyDetailsPutDto.Csr = companyDetails.Csr;
        companyDetailsPutDto.PrivateKey = companyDetails.PrivateKey;
        companyDetailsPutDto.RequestId = companyDetails.RequestId;
        companyDetailsPutDto.DispositionMessage = companyDetails.DispositionMessage;
        companyDetailsPutDto.BinarySecurityToken = companyDetails.BinarySecurityToken;
        companyDetailsPutDto.Secret = companyDetails.Secret;
        companyDetailsPutDto.IsOnBoarded = companyDetails.IsOnBoarded;
        companyDetailsPutDto.ProdRequestId = companyDetails.ProdRequestId;
        companyDetailsPutDto.ProdDispositionMessage = companyDetails.ProdDispositionMessage;
        companyDetailsPutDto.ProdBinarySecurityToken = companyDetails.ProdBinarySecurityToken;
        companyDetailsPutDto.ProdSecret = companyDetails.ProdSecret;


        // Handle phones - remove deleted ones and update/add others
        var phonesToRemove = companyDetails.Phones.Where(p => 
            !companyDetailsPutDto.Phones.Any(dto => dto.Id.HasValue && dto.Id.Value == p.Id)).ToList();
        
        foreach (var phoneToRemove in phonesToRemove)
        {
            companyDetails.Phones.Remove(phoneToRemove);
            await _repositoriesFactory.GetRepository<CompanyPhone>().Delete(phoneToRemove);
        }

        foreach (var phoneDto in companyDetailsPutDto.Phones ?? new List<CompanyPhonePutDto>())
        {
            if (phoneDto.Id.HasValue)
            {
                var existingPhone = companyDetails.Phones.FirstOrDefault(p => p.Id == phoneDto.Id.Value);
                if (existingPhone != null)
                {
                    existingPhone.PhoneNumber= phoneDto.PhoneNumber;
                    existingPhone.IsPrimary = phoneDto.IsPrimary ?? false;
                }
            }
            else
            {
                companyDetails.Phones.Add(new CompanyPhone
                {
                    PhoneNumber = phoneDto.PhoneNumber,
                    IsPrimary = phoneDto.IsPrimary ?? false,
                    CompanyId = companyDetails.Id
                });
            }
        }

        // Handle bank accounts - remove deleted ones and update/add others
        var bankAccountsToRemove = companyDetails.BankAccounts.Where(b => 
            !companyDetailsPutDto.BankAccounts.Any(dto => dto.Id.HasValue && dto.Id.Value == b.Id)).ToList();
        
        foreach (var bankAccountToRemove in bankAccountsToRemove)
        {
            companyDetails.BankAccounts.Remove(bankAccountToRemove);
            await _repositoriesFactory.GetRepository<BankAccount>().Delete(bankAccountToRemove);
        }

        foreach (var bankAccountDto in companyDetailsPutDto.BankAccounts ?? new List<BankAccountPutDto>())
        {
            if (bankAccountDto.Id.HasValue)
            {
                var existingBankAccount = companyDetails.BankAccounts.FirstOrDefault(b => b.Id == bankAccountDto.Id.Value);

                if (existingBankAccount != null)
                {
                    existingBankAccount.BankName = bankAccountDto.BankName;
                    existingBankAccount.BankAccountNumber = bankAccountDto.BankAccountNumber;
                    existingBankAccount.AccountName = bankAccountDto.AccountName;
                    existingBankAccount.BankBranchName = bankAccountDto.BankBranchName;
                    existingBankAccount.SwiftCode = bankAccountDto.SwiftCode;
                    existingBankAccount.IBAN = bankAccountDto.IBAN;
                    existingBankAccount.IsPrimary = bankAccountDto.IsPrimary ?? false;
                }
            }
            else
            {
                companyDetails.BankAccounts.Add(new BankAccount
                {
                    BankName = bankAccountDto.BankName,
                    BankAccountNumber = bankAccountDto.BankAccountNumber,
                    AccountName = bankAccountDto.AccountName,
                    BankBranchName = bankAccountDto.BankBranchName,
                    SwiftCode = bankAccountDto.SwiftCode,
                    IBAN = bankAccountDto.IBAN,
                    IsPrimary = bankAccountDto.IsPrimary ?? false,
                    CompanyId = companyDetails.Id
                });
            
            }
        }

        // Handle policies - remove deleted ones and update/add others
        var policiesToRemove = companyDetails.Policies.Where(p => 
            !companyDetailsPutDto.Policies.Any(dto => dto.Id.HasValue && dto.Id.Value == p.Id)).ToList();
        
        foreach (var policyToRemove in policiesToRemove)
        {
            companyDetails.Policies.Remove(policyToRemove);
            await _repositoriesFactory.GetRepository<CompanyPolicy>().Delete(policyToRemove);
        }

        foreach (var policyDto in companyDetailsPutDto.Policies ?? new List<CompanyPolicyPutDto>())
        {
            if (policyDto.Id.HasValue)
            {
                var existingPolicy = companyDetails.Policies.FirstOrDefault(p => p.Id == policyDto.Id.Value);
                if (existingPolicy != null)
                {
                    existingPolicy.PolicyParagraph = policyDto.PolicyParagraph;
                    existingPolicy.IsPrimary = policyDto.IsPrimary ?? false;
                }
            }
            else
            {
                companyDetails.Policies.Add(new CompanyPolicy
                {
                    PolicyParagraph = policyDto.PolicyParagraph,
                    IsPrimary = policyDto.IsPrimary ?? false,
                    CompanyId = companyDetails.Id
                });
            }
        }

        companyDetailsPutDto.Phones = companyDetails.Phones
            .Select(p => new CompanyPhonePutDto
            {
                Id = p.Id,
                PhoneNumber = p.PhoneNumber,
                IsPrimary = p.IsPrimary
            }).ToList();

        companyDetailsPutDto.BankAccounts = companyDetails.BankAccounts
            .Select(b => new BankAccountPutDto
            {
                Id = b.Id,
                BankName = b.BankName,
                BankAccountNumber = b.BankAccountNumber,
                AccountName = b.AccountName,
                BankBranchName = b.BankBranchName,
                SwiftCode = b.SwiftCode,
                IBAN = b.IBAN,
                IsPrimary = b.IsPrimary
            }).ToList();

        companyDetailsPutDto.Policies = companyDetails.Policies
            .Select(p => new CompanyPolicyPutDto
            {
                Id = p.Id,
                PolicyParagraph = p.PolicyParagraph,
                IsPrimary = p.IsPrimary
            }).ToList();


        // Map the basic properties using AutoMapper but ignore the collections
        var mappedCompanyDetails = _mapper.Map<CompanyDetails>(companyDetailsPutDto);
        
        // Update the basic properties
        companyDetails.ArabicName = mappedCompanyDetails.ArabicName;
        companyDetails.EnglishName = mappedCompanyDetails.EnglishName;
        companyDetails.Email = mappedCompanyDetails.Email;
        companyDetails.Address = mappedCompanyDetails.Address;
        companyDetails.ArabicAddress = mappedCompanyDetails.ArabicAddress;
        companyDetails.ShortAddress = mappedCompanyDetails.ShortAddress;
        companyDetails.BuildingNumber = mappedCompanyDetails.BuildingNumber;
        companyDetails.SubNumber = mappedCompanyDetails.SubNumber;
        companyDetails.District = mappedCompanyDetails.District;
        companyDetails.EnglishDistrict = mappedCompanyDetails.EnglishDistrict;
        companyDetails.CompanyStatusId = mappedCompanyDetails.CompanyStatusId;
        companyDetails.TaxNumber = mappedCompanyDetails.TaxNumber;
        companyDetails.GaztNumber = mappedCompanyDetails.GaztNumber;
        companyDetails.IsActive = mappedCompanyDetails.IsActive;
        companyDetails.IsZatcaIntegrated = mappedCompanyDetails.IsZatcaIntegrated;
        companyDetails.BusinessCategory = mappedCompanyDetails.BusinessCategory;
        companyDetails.Website = mappedCompanyDetails.Website;
        companyDetails.UnifiedNumber = mappedCompanyDetails.UnifiedNumber;
        companyDetails.RegistrationNumber = mappedCompanyDetails.RegistrationNumber;
        companyDetails.IsOnBoarded = mappedCompanyDetails.IsOnBoarded;

        // Logo handling: update LogoUrl or Logo if provided, otherwise preserve existing
        if (!string.IsNullOrWhiteSpace(mappedCompanyDetails.LogoUrl))
        {
            companyDetails.LogoUrl = mappedCompanyDetails.LogoUrl;
            companyDetails.Logo = null;
        }
        else if (mappedCompanyDetails.Logo != null && mappedCompanyDetails.Logo.Length > 0)
        {
            companyDetails.Logo = mappedCompanyDetails.Logo;
            companyDetails.LogoUrl = null;
        }
        else if (companyDetailsPutDto.LogoUrl != null && string.IsNullOrWhiteSpace(companyDetailsPutDto.LogoUrl)
                 && (companyDetailsPutDto.Logo == null || string.IsNullOrWhiteSpace(companyDetailsPutDto.Logo)))
        {
            // Explicitly cleared
            companyDetails.LogoUrl = null;
            companyDetails.Logo = null;
        }

        // Stamp handling: update StampUrl or Stamp if provided, otherwise preserve existing
        if (!string.IsNullOrWhiteSpace(mappedCompanyDetails.StampUrl))
        {
            companyDetails.StampUrl = mappedCompanyDetails.StampUrl;
            companyDetails.Stamp = null;
        }
        else if (mappedCompanyDetails.Stamp != null && mappedCompanyDetails.Stamp.Length > 0)
        {
            companyDetails.Stamp = mappedCompanyDetails.Stamp;
            companyDetails.StampUrl = null;
        }
        else if (companyDetailsPutDto.StampUrl != null && string.IsNullOrWhiteSpace(companyDetailsPutDto.StampUrl)
                 && (companyDetailsPutDto.Stamp == null || string.IsNullOrWhiteSpace(companyDetailsPutDto.Stamp)))
        {
            // Explicitly cleared
            companyDetails.StampUrl = null;
            companyDetails.Stamp = null;
        }

        var result = await companyDetailsRepository.Update(companyDetails);
        if (result.ErrorCode != ErrorCodes.Success)
            return new OperationResponse<CompanyDetailsResponseDto>(result.ErrorCode, null);
        
        var phoneRepository = _repositoriesFactory.GetRepository<CompanyPhone>();
        var bankAccountRepository = _repositoriesFactory.GetRepository<BankAccount>();
        var policyRepository = _repositoriesFactory.GetRepository<CompanyPolicy>();
        
        foreach (var phone in companyDetails.Phones)
        {
            if (phone.Id == Guid.Empty)
            {
                await phoneRepository.Add(phone);
            }
            else
            {
                await phoneRepository.Update(phone);
            }
        }
        
        foreach (var bankAccount in companyDetails.BankAccounts)
        {
            if (bankAccount.Id == Guid.Empty)
            {
                await bankAccountRepository.Add(bankAccount);
            }
            else
            {
                await bankAccountRepository.Update(bankAccount);
            }
        }
        
        foreach (var policy in companyDetails.Policies)
        {
            if (policy.Id == Guid.Empty)
            {
                await policyRepository.Add(policy);
            }
            else
            {
                await policyRepository.Update(policy);
            }
        }
        
        var updatedCompanyQuery = await companyDetailsRepository.Filter(x => x.Id == companyDetails.Id);
        var updatedCompany = await updatedCompanyQuery.Data
            .Include(c => c.Phones)
            .Include(c => c.BankAccounts)
            .Include(c => c.Policies)
            .FirstOrDefaultAsync();
        
        var responseData = _mapper.Map<CompanyDetailsResponseDto>(updatedCompany);
        return new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.Success, responseData);
    }

    public async Task<OperationResponse<CompanyDetailsResponseDto>> ToggleZatcaIntegrationAsync(Guid id)
    {
        var companyDetailsRepository = _repositoriesFactory.GetRepository<CompanyDetails>();
        var companyDetailsQuery = await companyDetailsRepository.Filter(x => x.Id == id);
        var companyDetails = await companyDetailsQuery.Data
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync();
        if (companyDetails == null) return new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.NotFound, null);
        
        companyDetails.IsZatcaIntegrated = !companyDetails.IsZatcaIntegrated;
        await companyDetailsRepository.Update(companyDetails);
        var companyDetailsResponseDto = _mapper.Map<CompanyDetailsResponseDto>(companyDetails);
        return new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.Success, companyDetailsResponseDto);
    }
    
    public async Task<OperationResponse<CompanyDetailsResponseDto>> IntegrateZatcaAsync(Guid id, string otp)
    {
        try
        {
            var companyDetailsRepository = _repositoriesFactory.GetRepository<CompanyDetails>();
            var companyDetailsQuery = await companyDetailsRepository.Filter(x => x.Id == id);
            var companyDetails = await companyDetailsQuery.Data
                .IgnoreAutoIncludes()
                .FirstOrDefaultAsync();
            if (companyDetails == null)
                return new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.NotFound, null);

            if (companyDetails.IsOnBoarded)
            {
                return new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.BadRequest, null,
                    message: "Company details already onboarded");
            }

            // validate company details properties
            if (string.IsNullOrEmpty(companyDetails.ArabicName)
                || string.IsNullOrEmpty(companyDetails.TaxNumber)
                || string.IsNullOrEmpty(companyDetails.ArabicAddress?.FullAddress)
                || string.IsNullOrEmpty(companyDetails.BusinessCategory))
            {
                return new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.BadRequest, null,
                    message: "Not all required fields are filled");
            }

            /*
            companyDetails.Csr = "-----BEGIN CERTIFICATE REQUEST-----\nMIIDKjCCAtACAQAwgZExCzAJBgNVBAYTAlNBMTIwMAYDVQQLDCnZhdis2YjZh9ix\n2KfYqiDYtNmH2LHYstin2K8g2KfZhNiz2YXYp9mGIDEyMDAGA1UECgwp2YXYrNmI\n2YfYsdin2Kog2LTZh9ix2LLYp9ivINin2YTYs9mF2KfZhiAxGjAYBgNVBAMMEUVH\nUy1TbWFydEFuZFByby00MFYwEAYHKoZIzj0CAQYFK4EEAAoDQgAEAig04kUm9y+V\n5I+H9nW0RWrNqcPNkfDEUsJ1YNqhycdF3l536gMH2HIY2ffCxGdtHIunAM45XALf\nor5idOqA26CCAd0wggHZBgkqhkiG9w0BCQ4xggHKMIIBxjAhBgkrBgEEAYI3FAIE\nFBMSWkFUQ0EtQ29kZS1TaWduaW5nMIIBnwYDVR0RBIIBljCCAZKkggGOMIIBijFV\nMFMGA1UEBAxMMS1TbWFydEFuZFByb3wyLUVHUy1TbWFydEFuZFByby0yLjAuMHwz\nLThmNzdhYjM3LTU1ZTQtNDkxZC05YThiLTZhYTY3MTdlMzkwYjEfMB0GCgmSJomT\n8ixkAQEMDzMwMDQ1MjA1ODYwMDAwMzENMAsGA1UEDAwEMTEwMDGB7jCB6wYDVQQa\nDIHj2KfZhNmF2YbYt9mC2Kkg2KfZhNmF2LHZg9iy2YrYqSDYp9mE2LTZhdin2YTZ\nitipICAtINi02KfYsdi5INij2KjZiiDYsNixINin2YTYutmB2KfYsdmKICAtINin\n2YTZhdix2YPYsiDYp9mE2KrYrNin2LHZiiAg2YHZhtiv2YIg2KPZiNio2LHZiNmK\nLCDYp9mE2YXYr9mK2YbYqSDYp9mE2YXZhtmI2LHYqSwg2KfZhNmF2YXZhNmD2Kkg\n2KfZhNi52LHYqNmK2Kkg2KfZhNiz2LnZiNiv2YrYqSwgNDIzMTExEDAOBgNVBA8M\nB0pld2VscnkwCgYIKoZIzj0EAwIDSAAwRQIgeyUVVhOqAhS4DFuJCh3/JAnyz9T9\nWpDfdehflIPsL3ECIQC8O7IZnJrx/xJUVHr5+I1rW5kkZMdClust4Pqo/v24Ww==\n-----END CERTIFICATE REQUEST-----";
            companyDetails.PrivateKey =
                "-----BEGIN EC PRIVATE KEY-----\nMHQCAQEEILD55XK/t3Ut/BSw1W7y0aL/Vjd84Hsx7v8a5q6lJdkAoAcGBSuBBAAK\noUQDQgAEAig04kUm9y+V5I+H9nW0RWrNqcPNkfDEUsJ1YNqhycdF3l536gMH2HIY\n2ffCxGdtHIunAM45XALfor5idOqA2w==\n-----END EC PRIVATE KEY-----";
            companyDetails.RequestId = 1748296924728;
            companyDetails.DispositionMessage = "ISSUED";
            companyDetails.BinarySecurityToken =
                "TUlJRFR6Q0NBdldnQXdJQkFnSUdBWmNPbmY0NE1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05NalV3TlRJMk1qSXdNVFU1V2hjTk16QXdOVEkyTWpFd01EQXdXakNCa1RFTE1Ba0dBMVVFQmhNQ1UwRXhNakF3QmdOVkJBc01LZG1GMkt6WmlObUgyTEhZcDlpcUlOaTAyWWZZc2RpeTJLZllyeURZcDltRTJMUFpoZGluMllZZ01USXdNQVlEVlFRS0RDblpoZGlzMllqWmg5aXgyS2ZZcWlEWXRObUgyTEhZc3RpbjJLOGcyS2ZaaE5pejJZWFlwOW1HSURFYU1CZ0dBMVVFQXd3UlJVZFRMVk50WVhKMFFXNWtVSEp2TFRRd1ZqQVFCZ2NxaGtqT1BRSUJCZ1VyZ1FRQUNnTkNBQVFDS0RUaVJTYjNMNVhrajRmMmRiUkZhczJwdzgyUjhNUlN3blZnMnFISngwWGVYbmZxQXdmWWNoalo5OExFWjIwY2k2Y0F6amxjQXQraXZtSjA2b0RibzRJQnRUQ0NBYkV3REFZRFZSMFRBUUgvQkFJd0FEQ0NBWjhHQTFVZEVRU0NBWll3Z2dHU3BJSUJqakNDQVlveFZUQlRCZ05WQkFRTVRERXRVMjFoY25SQmJtUlFjbTk4TWkxRlIxTXRVMjFoY25SQmJtUlFjbTh0TWk0d0xqQjhNeTA0WmpjM1lXSXpOeTAxTldVMExUUTVNV1F0T1dFNFlpMDJZV0UyTnpFM1pUTTVNR0l4SHpBZEJnb0praWFKay9Jc1pBRUJEQTh6TURBME5USXdOVGcyTURBd01ETXhEVEFMQmdOVkJBd01CREV4TURBeGdlNHdnZXNHQTFVRUdneUI0OWluMllUWmhkbUcyTGZaZ3RpcElOaW4yWVRaaGRpeDJZUFlzdG1LMktrZzJLZlpoTmkwMllYWXA5bUUyWXJZcVNBZ0xTRFl0TmluMkxIWXVTRFlvOWlvMllvZzJMRFlzU0RZcDltRTJMclpnZGluMkxIWmlpQWdMU0RZcDltRTJZWFlzZG1EMkxJZzJLZlpoTmlxMkt6WXA5aXgyWW9nSU5tQjJZYllyOW1DSU5pajJZallxTml4MllqWmlpd2cyS2ZaaE5tRjJLL1ppdG1HMktrZzJLZlpoTm1GMlliWmlOaXgyS2tzSU5pbjJZVFpoZG1GMllUWmc5aXBJTmluMllUWXVkaXgyS2paaXRpcElOaW4yWVRZczlpNTJZallyOW1LMktrc0lEUXlNekV4TVJBd0RnWURWUVFQREFkS1pYZGxiSEo1TUFvR0NDcUdTTTQ5QkFNQ0EwZ0FNRVVDSVFEdUhCSHNvSy9vTHdCakh3SXV1UmNqY3plMDdvYWN1OXZnUGh1VUtGVHg3d0lnSmUvdUNidjBPL1I5ejVpckxRcThHVHlFMndCQU1zWFh4SURLMGpqRUQwTT0=";
            companyDetails.Secret = "FQa7aoWz4svH8a5sKSLGYwA3FLFabMdNYNhNB0nWnj8=";*/
            
            var csrGenerator = new CsrGenerator();
            var csrResult = csrGenerator.GenerateCsr(new CsrGenerationDto(
                    commonName: $"EGS-SmartAndPro-4",
                    serialNumber: $"1-SmartAndPro|2-EGS-SmartAndPro-2.0.0|3-{Guid.NewGuid().ToString()}",
                    organizationIdentifier: companyDetails.TaxNumber,
                    organizationUnitName: companyDetails.ArabicName,
                    organizationName: companyDetails.ArabicName,
                    countryName: "SA",
                    invoiceType: "1100",
                    locationAddress: companyDetails.ArabicAddress?.FullAddress,
                    industryBusinessCategory: companyDetails.BusinessCategory
                ),
                _appSettings.ZatcaParams.IsSandbox ? EnvironmentType.NonProduction : EnvironmentType.Production,
                pemFormat:true
            );
            if (csrResult.IsValid)
            {
                
                companyDetails.Csr = csrResult.Csr;
                companyDetails.PrivateKey = csrResult.PrivateKey;
                var csrBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(csrResult.Csr));
                var complianceCsrApiClient = new ComplianceCsrApiClient(new GlobalVariables
                    {
                        BaseUrl = _appSettings.ZatcaParams.IsSandbox
                            ? _appSettings.ZatcaParams.SandboxUrls.BaseUrl
                            : _appSettings.ZatcaParams.ProdUrls.BaseUrl,
                        ComplianceCsidEndpoint = _appSettings.ZatcaParams.IsSandbox
                            ? _appSettings.ZatcaParams.SandboxUrls.ComplianceCsidEndpoint
                            : _appSettings.ZatcaParams.ProdUrls.ComplianceCsidEndpoint
                    }
                );
                var complianceCsrRequest = new ComplianceCsrRequest(otp: otp, csr: csrBase64);
                var complianceCsrResponse = await complianceCsrApiClient.GetToken(complianceCsrRequest);
                if (complianceCsrResponse == null)
                    throw new Exception("Failed to get the token");
                companyDetails.BinarySecurityToken = complianceCsrResponse.BinarySecurityToken;
                companyDetails.Secret = complianceCsrResponse.Secret;
                companyDetails.RequestId = complianceCsrResponse.RequestId;
                companyDetails.DispositionMessage = complianceCsrResponse.DispositionMessage;

                var complianceValidationResult = ValidateComplianceInvoices(companyDetails);
                if (!complianceValidationResult.AllStandardCleared) throw new Exception("Compliance Standard Invoices not cleared");
                if (!complianceValidationResult.AllSimplifiedReported) throw new Exception("Compliance Simplified Invoices not reported");
                LogService.LogInformation($"Compliance Validation Result:\nAll Standard Invoices Cleared: {complianceValidationResult.AllStandardCleared.ToString()},\nAll Simplified Invoices Reported: {complianceValidationResult.AllSimplifiedReported.ToString()}");
                var prodCsidApiClient = new ProdCsidApiClient(new GlobalVariables
                    {
                        BaseUrl = _appSettings.ZatcaParams.IsSandbox
                            ? _appSettings.ZatcaParams.SandboxUrls.BaseUrl
                            : _appSettings.ZatcaParams.ProdUrls.BaseUrl,
                        ProdCsidEndpoint = _appSettings.ZatcaParams.IsSandbox
                            ? _appSettings.ZatcaParams.SandboxUrls.ProdCsidEndpoint
                            : _appSettings.ZatcaParams.ProdUrls.ProdCsidEndpoint
                    }
                );
                var prodCsidResponse = await prodCsidApiClient.GetToken(new ProdCsidOnboardingRequest(
                    (long)companyDetails.RequestId, companyDetails.BinarySecurityToken, companyDetails.Secret));

                if (prodCsidResponse == null)
                    throw new Exception("Failed to get the token");

                companyDetails.ProdRequestId = prodCsidResponse.RequestId;
                companyDetails.ProdBinarySecurityToken = prodCsidResponse.BinarySecurityToken;
                companyDetails.ProdSecret = prodCsidResponse.Secret;
                companyDetails.ProdDispositionMessage = prodCsidResponse.DispositionMessage;

                companyDetails.IsOnBoarded = true;

                await companyDetailsRepository.Update(companyDetails);
                return new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.Success, null);

            }

            return new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.GeneralFail, null);
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            return new OperationResponse<CompanyDetailsResponseDto>(ErrorCodes.GeneralFail, null, message: $"Exception: {e.Message}. Inner: {e.InnerException?.Message}. Stack: {e.StackTrace}");
        }
    }

    private ComplianceValidationResult ValidateComplianceInvoices(CompanyDetails companyDetails)
    {
        var result = new ComplianceValidationResult();
        var complianceInvoiceApiClient = new ComplianceInvoiceApiClient(new GlobalVariables
            {
                BaseUrl = _appSettings.ZatcaParams.IsSandbox ?  _appSettings.ZatcaParams.SandboxUrls.BaseUrl : _appSettings.ZatcaParams.ProdUrls.BaseUrl,
                ComplianceInvoiceEndpoint = _appSettings.ZatcaParams.IsSandbox ?  _appSettings.ZatcaParams.SandboxUrls.ComplianceInvoiceEndpoint : _appSettings.ZatcaParams.ProdUrls.ComplianceInvoiceEndpoint
            }
        );

        // 2. Standard Invoice
        var standardInvoice = new StandardInvoice
        {
            Id = "INV-1001",
            UUID = Guid.NewGuid().ToString(),
            IssuedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            IssuedTime = DateTime.UtcNow.ToString("HH:mm:ssZ"),
            InvoiceTypeCode = "388",
            DocumentCurrencyCode = "SAR",
            TaxCurrencyCode = "SAR",
            ActualDeliveryDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            LatestDeliveryDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),

            SupplierInfo = new SupplierInfo
            {
                RegistrationName = companyDetails.ArabicName,
                PartyPostalAddress = new PartyPostalAddress
                {
                    StreetName = companyDetails.ArabicAddress?.Street,
                    CityName = companyDetails.ArabicAddress?.City,
                    Country = "SA",
                    PostalZone = companyDetails.ArabicAddress?.ZipCode
                },
                CRN = companyDetails.TaxNumber,
                VAT = _appSettings.ZatcaParams.IsSandbox
                    ? "399999999900003"
                    : companyDetails.TaxNumber
            },

            CustomerInfo = new CustomerInfo
            {
                RegistrationName = "العميل التجريبي",
                VAT = "399999999900003",
            },

            TaxTotal = new TaxTotal
            {
                TaxableAmount = new Money("SAR", 10.00),
                TaxAmount = new Money("SAR", 1.50),
                Percent = "15.00",
                TaxCategory = "S",
            },

            LegalMonetaryTotal = new LegalMonetaryTotal
            {
                LineExtensionAmount = new Money("SAR", 10.00),       // Before tax
                TaxExclusiveAmount = new Money("SAR", 10.00),        // Before tax
                TaxInclusiveAmount = new Money("SAR", 11.50),        // After tax
                PayableAmount = new Money("SAR", 11.50),             // What the customer pays
                AllowanceTotalAmount = new Money("SAR", 0.00),       // No discount
                PrepaidAmount = new Money("SAR", 0.00),
            },

            InvoiceLineItems = new List<InvoiceLineItem>
            {
                new InvoiceLineItem
                {
                    ID = "1",
                    InvoicedQuantity = "1",
                    LineExtensionAmount = new Money("SAR", 10.00), // Price × Quantity (before tax)
                    TaxAmount = new Money("SAR", 1.50),
                    RoundingAmount = new Money("SAR", 0.00),
                    TaxSchemeId = "S",
                    TaxScheme = "VAT",
                    TaxPercent = "15.00",
                    ItemName = "تجريبي",
                    PriceAmount = new Money("SAR", 10.00), // Unit price
                }
            },

            DocumentLevelDiscountAmount = 0m,
            DocumentLevelDiscountReason = "",
        };
        var invoiceXml = StandardInvoiceXmlGenerator.Generate(standardInvoice, companyDetails.PrivateKey, companyDetails.BinarySecurityToken);
        var complianceInvoiceResponse =  complianceInvoiceApiClient.CheckCompliance(new InvoiceClearanceRequest(invoiceXml, companyDetails.BinarySecurityToken, companyDetails.Secret));
        if (complianceInvoiceResponse == null)  
            throw new Exception("Failed to get the token");
        LogService.LogInformation($"Clearance Status | Standard Invoice: {complianceInvoiceResponse.ClearanceStatus}");

        result.StandardInvoiceStatus = complianceInvoiceResponse.ClearanceStatus ?? "NOT_CLEARED";
        
        // 2. Standard Credit Note
        var standardCreditNote = new StandardInvoiceAdjustment
            {
                AdjustmentReason = "Returned goods",
                AdjsutmentInvoiceNumber = "INV-1001",
                Id = "CN-1001",
                UUID = Guid.NewGuid().ToString(),
                IssuedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                IssuedTime = DateTime.UtcNow.ToString("HH:mm:ssZ"),
                DocumentCurrencyCode = "SAR",
                TaxCurrencyCode = "SAR",
                ActualDeliveryDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                LatestDeliveryDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),

                SupplierInfo = new SupplierInfo
                {
                    RegistrationName = companyDetails.ArabicName,
                    PartyPostalAddress = new PartyPostalAddress
                    {
                        StreetName = companyDetails.ArabicAddress?.Street,
                        CityName = companyDetails.ArabicAddress?.City,
                        Country = "SA",
                        PostalZone = companyDetails.ArabicAddress?.ZipCode
                    },
                    CRN = companyDetails.TaxNumber,
                    VAT = _appSettings.ZatcaParams.IsSandbox
                        ? "399999999900003"
                        : companyDetails.TaxNumber
                },

                CustomerInfo = new CustomerInfo
                {
                    RegistrationName = "العميل التجريبي",
                    VAT = "399999999900003",
                },

                TaxTotal = new TaxTotal
                {
                    TaxableAmount = new Money("SAR", 10.00),
                    TaxAmount = new Money("SAR", 1.50),
                    Percent = "15.00",
                    TaxCategory = "S",
                },

                LegalMonetaryTotal = new LegalMonetaryTotal
                {
                    LineExtensionAmount = new Money("SAR", 10.00),       // Before tax
                    TaxExclusiveAmount = new Money("SAR", 10.00),        // Before tax
                    TaxInclusiveAmount = new Money("SAR", 11.50),        // After tax
                    PayableAmount = new Money("SAR", 11.50),             // What the customer pays
                    AllowanceTotalAmount = new Money("SAR", 0.00),       // No discount
                    PrepaidAmount = new Money("SAR", 0.00),
                },

                InvoiceLineItems = new List<InvoiceLineItem>
                {
                    new InvoiceLineItem
                    {
                        ID = "1",
                        InvoicedQuantity = "1",
                        LineExtensionAmount = new Money("SAR", 10.00), // Price × Quantity (before tax)
                        TaxAmount = new Money("SAR", 1.50),
                        RoundingAmount = new Money("SAR", 0.00),
                        TaxSchemeId = "S",
                        TaxScheme = "VAT",
                        TaxPercent = "15.00",
                        ItemName = "تجريبي",
                        PriceAmount = new Money("SAR", 10.00), // Unit price
                    }
                },

                DocumentLevelDiscountAmount = 0m,
                DocumentLevelDiscountReason = "",
            };

        var creditNoteXml = StandardInvoiceXmlGenerator.GenerateForCreditNote(standardCreditNote, companyDetails.PrivateKey, companyDetails.BinarySecurityToken);

        var complianceCreditNoteResponse =  complianceInvoiceApiClient.CheckCompliance(new InvoiceClearanceRequest(creditNoteXml, companyDetails.BinarySecurityToken, companyDetails.Secret));
        if (complianceCreditNoteResponse == null)
            throw new Exception("Failed to get the token");
        LogService.LogInformation($"Clearance Status | Standard Credit Note: {complianceCreditNoteResponse.ClearanceStatus}");
        result.StandardCreditNoteStatus = complianceCreditNoteResponse.ClearanceStatus ?? "NOT_CLEARED";
        
        // 3. Standard Debit Note
        var standardDebitNote = new StandardInvoiceAdjustment
            {
                AdjustmentReason = "Added goods",
                AdjsutmentInvoiceNumber = "INV-1001",
                Id = "DN-1001",
                UUID = Guid.NewGuid().ToString(),
                IssuedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                IssuedTime = DateTime.UtcNow.ToString("HH:mm:ssZ"),
                DocumentCurrencyCode = "SAR",
                TaxCurrencyCode = "SAR",
                ActualDeliveryDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                LatestDeliveryDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),

                SupplierInfo = new SupplierInfo
                {
                    RegistrationName = companyDetails.ArabicName,
                    PartyPostalAddress = new PartyPostalAddress
                    {
                        StreetName = companyDetails.ArabicAddress?.Street,
                        CityName = companyDetails.ArabicAddress?.City,
                        Country = "SA",
                        PostalZone = companyDetails.ArabicAddress?.ZipCode
                    },
                    CRN = companyDetails.TaxNumber,
                    VAT = _appSettings.ZatcaParams.IsSandbox    
                        ? "399999999900003"
                        : companyDetails.TaxNumber
                },

                CustomerInfo = new CustomerInfo
                {
                    RegistrationName = "العميل التجريبي",
                    VAT = "399999999900003",
                },

                TaxTotal = new TaxTotal
                {
                    TaxableAmount = new Money("SAR", 10.00),
                    TaxAmount = new Money("SAR", 1.50),
                    Percent = "15.00",
                    TaxCategory = "S",
                },

                LegalMonetaryTotal = new LegalMonetaryTotal
                {
                    LineExtensionAmount = new Money("SAR", 10.00),       // Before tax
                    TaxExclusiveAmount = new Money("SAR", 10.00),        // Before tax
                    TaxInclusiveAmount = new Money("SAR", 11.50),        // After tax
                    PayableAmount = new Money("SAR", 11.50),             // What the customer pays
                    AllowanceTotalAmount = new Money("SAR", 0.00),       // No discount
                    PrepaidAmount = new Money("SAR", 0.00),
                },

                InvoiceLineItems = new List<InvoiceLineItem>
                {
                    new InvoiceLineItem
                    {
                        ID = "1",
                        InvoicedQuantity = "1",
                        LineExtensionAmount = new Money("SAR", 10.00),
                        TaxAmount = new Money("SAR", 1.50),
                        RoundingAmount = new Money("SAR", 0),
                        TaxSchemeId = "S",
                        TaxScheme = "VAT",
                        TaxPercent = "15.00",
                        ItemName = "تجريبي",
                        PriceAmount = new Money("SAR", 10.00),
                    }
                },

                DocumentLevelDiscountAmount = 0m,
                DocumentLevelDiscountReason = "",
            };

        var debitNoteXml = StandardInvoiceXmlGenerator.GenerateForDebitNote(standardDebitNote, companyDetails.PrivateKey, companyDetails.BinarySecurityToken);

        var complianceDebitNoteResponse =  complianceInvoiceApiClient.CheckCompliance(new InvoiceClearanceRequest(debitNoteXml, companyDetails.BinarySecurityToken, companyDetails.Secret));
        if (complianceDebitNoteResponse == null)  
            throw new Exception("Failed to get the token");
        LogService.LogInformation($"Clearance Status | Standard Debit Note: {complianceDebitNoteResponse.ClearanceStatus}");
        result.StandardDebitNoteStatus = complianceDebitNoteResponse.ClearanceStatus ?? "NOT_CLEARED";

        // 4. Simplified Invoice (Same as Standard Invoice but without Customer VAT)
        standardInvoice.CustomerInfo.VAT = null;
        var simplifiedInvoiceXml = StandardInvoiceXmlGenerator.GenerateSimplifiedInvoice(standardInvoice, companyDetails.PrivateKey, companyDetails.BinarySecurityToken);
        
        var complianceSimplifiedInvoiceResponse =  complianceInvoiceApiClient.CheckCompliance(new InvoiceClearanceRequest(simplifiedInvoiceXml, companyDetails.BinarySecurityToken, companyDetails.Secret));
        if (complianceSimplifiedInvoiceResponse == null)  
            throw new Exception("Failed to get the token");
        LogService.LogInformation($"Clearance Status | Simplified Invoice: {complianceSimplifiedInvoiceResponse.ClearanceStatus ?? complianceSimplifiedInvoiceResponse.ReportingStatus}");
        result.SimplifiedInvoiceStatus = complianceSimplifiedInvoiceResponse.ReportingStatus ?? "NOT_REPORTED";
        
        // 5. Simplified Credit Note (Same as Standard Credit Note but without Customer VAT)
        standardCreditNote.CustomerInfo.VAT = null;
        var simplifiedCreditNoteXml = StandardInvoiceXmlGenerator.GenerateSimplifiedCreditNote(standardCreditNote, companyDetails.PrivateKey, companyDetails.BinarySecurityToken);
        var complianceSimplifiedCreditNoteResponse =  complianceInvoiceApiClient.CheckCompliance(new InvoiceClearanceRequest(simplifiedCreditNoteXml, companyDetails.BinarySecurityToken, companyDetails.Secret));
        if (complianceSimplifiedCreditNoteResponse == null)  
            throw new Exception("Failed to get the token");
        LogService.LogInformation($"Clearance Status | Simplified Credit Note: {complianceSimplifiedCreditNoteResponse.ClearanceStatus ?? complianceSimplifiedCreditNoteResponse.ReportingStatus}");
        result.SimplifiedCreditNoteStatus = complianceSimplifiedCreditNoteResponse.ReportingStatus ?? "NOT_REPORTED";
        
        // 6. Simplified Debit Note (Same as Standard Debit Note but without Customer VAT)
        standardDebitNote.CustomerInfo.VAT = null;
        var simplifiedDebitNoteXml = StandardInvoiceXmlGenerator.GenerateSimplifiedDebitNote(standardDebitNote, companyDetails.PrivateKey, companyDetails.BinarySecurityToken);
        var complianceSimplifiedDebitNoteResponse =  complianceInvoiceApiClient.CheckCompliance(new InvoiceClearanceRequest(simplifiedDebitNoteXml, companyDetails.BinarySecurityToken, companyDetails.Secret));
        if (complianceSimplifiedDebitNoteResponse == null)  
            throw new Exception("Failed to get the token");
        LogService.LogInformation($"Clearance Status | Simplified Debit Note: {complianceSimplifiedDebitNoteResponse.ClearanceStatus ?? complianceSimplifiedDebitNoteResponse.ReportingStatus}");
        result.SimplifiedDebitNoteStatus = complianceSimplifiedDebitNoteResponse.ReportingStatus ?? "NOT_REPORTED";
        return result;
    }
    
    
}
