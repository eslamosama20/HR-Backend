namespace SmartAndPro.ERP.Application.Common.Dtos.Assets
{
    public class AssetResponseDto
    {
        public Guid Id { get; set; }
        public string URL { get; set; }
        public string? Base64Url { get; set; }
        public string? PublicId { get; set; }
    }
}
