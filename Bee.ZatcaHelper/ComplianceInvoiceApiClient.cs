using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bee.ZatcaHelper.Contracts;
using Bee.ZatcaHelper.Util;
using WebClient = Bee.ZatcaHelper.Util.WebClient;

namespace Bee.ZatcaHelper;

public class ComplianceInvoiceApiClient
{
    private readonly string _baseUrl;
    private readonly string? _complianceInvoiceEndpoint;

    public ComplianceInvoiceApiClient(GlobalVariables globalVariables)
    {
        _baseUrl = globalVariables.BaseUrl;
        _complianceInvoiceEndpoint = globalVariables.ComplianceInvoiceEndpoint;
        if (string.IsNullOrEmpty(_baseUrl) || string.IsNullOrEmpty(_complianceInvoiceEndpoint))
            throw new Exception("Base Url or Compliance Invoice Endpoint can't be empty");
    }
    


    public InvoiceComplianceResponse CheckCompliance(InvoiceClearanceRequest complianceInvoiceRequest)
    {
        var customHeaders = new Dictionary<string, string> { { "accept-language", "en" } };
        var result = new WebClient(_baseUrl, customHeaders,
            complianceInvoiceRequest.BinaryToken, complianceInvoiceRequest.Secret).PostAsJsonAsync(
            _complianceInvoiceEndpoint, complianceInvoiceRequest.Body);
        if (!result.Result.IsSuccessStatusCode)
        {
            var errorContent = result.Result.Content.ReadAsStringAsync().Result;
            throw new Exception($"ZATCA Invoice Error: {result.Result.StatusCode} - {errorContent}");
        }

        var response =  result.Result.Content.ReadFromJsonAsync<InvoiceComplianceResponse>().Result;

        if (response?.ClearanceStatus != "CLEARED" | response?.ReportingStatus != "REPORTED") return response;
        return response;
    }


}
