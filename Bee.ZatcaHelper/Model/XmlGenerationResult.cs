using System.Xml;

namespace Bee.ZatcaHelper.Model;

public class XmlGenerationResult
{
    public string invoiceHash { get; set; }
    public string uuid { get; set; }
    public string invoice { get; set; }
    public XmlDocument invoiceXml { get; set; }
    
    public string? GeneratedQR { get; set; }
}