using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Assets
{
    public class Asset : Entity
    {
        public string URL { get; set; }
        public string? Base64Url { get; set; }
        public string? PublicId { get; set; }
    }
}
