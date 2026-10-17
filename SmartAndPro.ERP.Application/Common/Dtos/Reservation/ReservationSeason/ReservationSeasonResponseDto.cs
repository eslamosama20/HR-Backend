using SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationSeason;

public class ReservationSeasonResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }

    public Guid? RoomRevenueMainAccountId { get; set; }

    public MainAccountResponseDto RoomRevenueMainAccount { get; set; }

    public Guid? RoomExpenseMainAccountId { get; set; }

    public MainAccountResponseDto RoomExpenseMainAccount { get; set; }

    public Guid? MealRevenueMainAccountId { get; set; }

    public MainAccountResponseDto MealRevenueMainAccount { get; set; }

    public Guid? MealExpenseMainAccountId { get; set; }

    public MainAccountResponseDto MealExpenseMainAccount { get; set; }

    public Guid? OtherRevenueMainAccountId { get; set; }

    public MainAccountResponseDto OtherRevenueMainAccount { get; set; }

    public Guid? OtherExpenseMainAccountId { get; set; }

    public MainAccountResponseDto OtherExpenseMainAccount { get; set; }


    public DateTime CreatedAt { get; set; }

    public bool IsFrontal { get; set; } = false;
}