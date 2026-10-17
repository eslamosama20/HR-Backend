namespace SmartAndPro.ERP.SharedKernel.Entities.Configurations
{
    public class AppSettings
    {
        public string CorsDomains { get; set; }
        public string DomainName { get; set; }
        public TokenSettings Token { get; set; }
        
        public ZatcaParams ZatcaParams { get; set; }
        public string FrontBaseUrl { get; set; }
        public string WeekendDays { get; set; } = "Friday,Saturday";

    }
    
    public class ZatcaParams
    {
        public SandboxUrls SandboxUrls { get; set; }
        
        public ProdUrls ProdUrls { get; set; }
        public bool IsSandbox { get; set; }
    }

    public class SandboxUrls
    {
        public string BaseUrl { get; set; }
        public string ComplianceCsidEndpoint { get; set; }
        public string ComplianceInvoiceEndpoint { get; set; }
        public string ProdCsidEndpoint { get; set; }
        public string InvoiceClearanceEndPoint { get; set; }
        
        public string InvoiceReportingEndPoint { get; set; }
    }
    public class ProdUrls
    {
        public string BaseUrl { get; set; }
        public string ComplianceCsidEndpoint { get; set; }
        public string ComplianceInvoiceEndpoint { get; set; }
        public string ProdCsidEndpoint { get; set; }
        public string InvoiceClearanceEndPoint { get; set; }
        
        public string InvoiceReportingEndPoint { get; set; }
    }


    public class TokenSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string DurationInDays { get; set; }
    }


}
