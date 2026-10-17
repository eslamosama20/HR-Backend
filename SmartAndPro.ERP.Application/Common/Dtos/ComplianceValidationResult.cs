namespace SmartAndPro.ERP.Application.Common.Dtos;

public class ComplianceValidationResult
{
    public string StandardInvoiceStatus { get; set; }
    public string StandardCreditNoteStatus { get; set; }
    public string StandardDebitNoteStatus { get; set; }
    public string SimplifiedInvoiceStatus { get; set; }
    public string SimplifiedCreditNoteStatus { get; set; }
    public string SimplifiedDebitNoteStatus { get; set; }

    public bool AllStandardCleared =>
        StandardInvoiceStatus == "CLEARED" &&
        StandardCreditNoteStatus == "CLEARED" &&
        StandardDebitNoteStatus == "CLEARED";

    public bool AllSimplifiedReported =>
        SimplifiedInvoiceStatus == "REPORTED" &&
        SimplifiedCreditNoteStatus == "REPORTED" &&
        SimplifiedDebitNoteStatus == "REPORTED";
}
