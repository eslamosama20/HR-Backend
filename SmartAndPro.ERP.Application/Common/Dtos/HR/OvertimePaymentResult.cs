using SmartAndPro.ERP.Application.Services.HR;

namespace SmartAndPro.ERP.Application.Common.Dtos.HR;

public class OvertimePaymentResult
{
    public List<FactoryOvertimePaymentResult> Factory1OvertimePayments { get; set; } = new();
    public List<FactoryOvertimePaymentResult> Factory2OvertimePayments { get; set; } = new();
    public List<FactoryOvertimePaymentResult> Factory3OvertimePayments { get; set; } = new();
}