namespace SmartAndPro.ERP.Application.Common.Dtos.Assets
{
    public class AssetDto
    {
        public Guid Id { get; set; }
        public string Base64 { get; set; }
        public string? Base64Url { get; set; }
        public string? Url { get; set; }
        public string? PublicId { get; set; }
    }
}
