using SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.SalesRepresentative;

public class SalesRepresentativeResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }

    public Guid? MainAccountId { get; set; }

    public MainAccountResponseDto MainAccount { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsFrontal { get; set; } = false;
}