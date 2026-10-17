using SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.CustomerProfile;

public class CustomerProfileResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    
    public string ContactNumber { get; set; }
    
    public string Email { get; set; }
    
    public string BankName { get; set; }
    
    public string BankAccountNumber { get; set; }
    
    public string IBAN { get; set; }
    
    public string TaxNumber { get; set; }
    
    public string Street { get; set; }
    public string? EnglishStreet { get; set; }

    public string District { get; set; }
    public string? EnglishDistrict { get; set; }

    public string ShortAddress { get; set; }

    public string BuildingNumber { get; set; }

    public string? SubNumber { get; set; }

    public string PostalCode { get; set; }
    
    public string City { get; set; }
    public string? EnglishCity { get; set; }
    
    public string Country { get; set; }
    public string? EnglishCountry { get; set; }

    public string AdditionalContactNumber { get; set; }

    public string Fax { get; set; }

    public string Website { get; set; }

    public string Instructions { get; set; }

    public string GaztNumber { get; set; }
    
    public string? OtherId { get; set; }
    
    public Guid? MainAccountId { get; set; }
    public MainAccountResponseDto MainAccount { get; set; }


    public DateTime CreatedAt { get; set; }
    public bool IsFrontal { get; set; } = false;
    public bool IsReservationCompany { get; set; } = false;
    public bool IsForProjects { get; set; } = false;
    public List<CustomerDepartmentDto> Departments { get; set; } = new();
    
    public decimal CreditBalance { get; set; } = 0;
}
