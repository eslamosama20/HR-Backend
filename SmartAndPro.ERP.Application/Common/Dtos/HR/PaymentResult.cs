using SmartAndPro.ERP.Application.Services.HR;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR;

public class PaymentResult
{
    public List<Factory1PaymentResult> Factory1Payments { get; set; } = new();
    public List<Factory2PaymentResult> Factory2Payments { get; set; } = new();
    public List<Factory3PaymentResult> Factory3Payments { get; set; } = new();
}