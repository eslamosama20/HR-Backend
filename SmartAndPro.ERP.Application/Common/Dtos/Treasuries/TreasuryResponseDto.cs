namespace SmartAndPro.ERP.Application.Common.Dtos.Treasuries
{
    public class TreasuryResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid MainAccountId { get; set; }
        public bool IsFrontal { get; set; } = false;
    }
}
