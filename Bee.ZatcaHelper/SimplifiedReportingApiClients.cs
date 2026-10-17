using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Xml;
using Bee.ZatcaHelper.Contracts;
using Bee.ZatcaHelper.Util;
using WebClient = Bee.ZatcaHelper.Util.WebClient;

namespace Bee.ZatcaHelper;

public class SimplifiedInvoiceReportingApiClient
{
    private readonly string _baseUrl;
    private readonly string _reportingEndpoint;

    public SimplifiedInvoiceReportingApiClient(GlobalVariables globalVariables)
    {
        _baseUrl = globalVariables.BaseUrl;
        _reportingEndpoint = globalVariables.InvoiceReportingEndPoint;
    }

    public InvoiceReportingResponse? ReportSimplifiedInvoice(InvoiceClearanceRequest request)
    {
        var customHeaders = new Dictionary<string, string> { { "accept-language", "en" } };
        var result = new WebClient(_baseUrl, customHeaders, request.BinaryToken, request.Secret)
            .PostAsJsonAsync(_reportingEndpoint, request.Body);
        var response = result.Result.Content.ReadFromJsonAsync<InvoiceReportingResponse>().Result;
        if (!result.Result.IsSuccessStatusCode)
        {
            var errorContent = result.Result.Content.ReadAsStringAsync().Result;
            throw new Exception($"ZATCA Reporting Error: {result.Result.StatusCode} - {errorContent}");
        }
        return response;
    }
}

public class SimplifiedCreditNoteReportingApiClient
{
    private readonly string _baseUrl;
    private readonly string _reportingEndpoint;

    public SimplifiedCreditNoteReportingApiClient(GlobalVariables globalVariables)
    {
        _baseUrl = globalVariables.BaseUrl;
        _reportingEndpoint = globalVariables.InvoiceReportingEndPoint;
    }

    public InvoiceReportingResponse? ReportSimplifiedCreditNote(InvoiceClearanceRequest request)
    {
        var customHeaders = new Dictionary<string, string> { { "accept-language", "en" } };
        var result = new WebClient(_baseUrl, customHeaders, request.BinaryToken, request.Secret)
            .PostAsJsonAsync(_reportingEndpoint, request.Body);
        var response = result.Result.Content.ReadFromJsonAsync<InvoiceReportingResponse>().Result;
        if (!result.Result.IsSuccessStatusCode)
        {
            var errorContent = result.Result.Content.ReadAsStringAsync().Result;
            throw new Exception($"ZATCA Reporting Error: {result.Result.StatusCode} - {errorContent}");
        }
        return response;
    }
}

public class SimplifiedDebitNoteReportingApiClient
{
    private readonly string _baseUrl;
    private readonly string _reportingEndpoint;

    public SimplifiedDebitNoteReportingApiClient(GlobalVariables globalVariables)
    {
        _baseUrl = globalVariables.BaseUrl;
        _reportingEndpoint = globalVariables.InvoiceReportingEndPoint;
    }

    public InvoiceReportingResponse? ReportSimplifiedDebitNote(InvoiceClearanceRequest request)
    {
        var customHeaders = new Dictionary<string, string> { { "accept-language", "en" } };
        var result = new WebClient(_baseUrl, customHeaders, request.BinaryToken, request.Secret)
            .PostAsJsonAsync(_reportingEndpoint, request.Body);
        var response = result.Result.Content.ReadFromJsonAsync<InvoiceReportingResponse>().Result;
        if (!result.Result.IsSuccessStatusCode)
        {
            var errorContent = result.Result.Content.ReadAsStringAsync().Result;
            throw new Exception($"ZATCA Reporting Error: {result.Result.StatusCode} - {errorContent}");
        }
        return response;
    }
}
