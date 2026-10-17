using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Serilog;

namespace Bee.ZatcaHelper.Util;

public class WebClient
{
    private readonly HttpClient _client = new();

    public WebClient(string baseUrl, Dictionary<string, string> customHeaders)
    {
        _client.BaseAddress = new Uri(baseUrl);
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("Accept-Version", "V2");

        foreach (var (header, value) in customHeaders.Select(x => (x.Key, x.Value)))
        {
            _client.DefaultRequestHeaders.Add(header, value);
        }
    }

    public WebClient(string baseUrl, Dictionary<string, string> customHeaders, string userName, string password)
        : this(baseUrl, customHeaders)
    {
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes(userName + ":" + password));
        _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {credentials}");
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync(string controller, object data)
    {
        HttpResponseMessage response = new();

        try
        {
            var requestUri = new Uri(_client.BaseAddress!, controller);
            var jsonPayload = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            Log.Information("Sending POST request to {Url} with payload:\n{Payload}", requestUri, jsonPayload);

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            response = await _client.PostAsync(controller, content).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Log.Information("Received response (Status: {StatusCode}):\n{ResponseBody}", response.StatusCode, responseContent);

            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception occurred while sending request: {Message}", ex.Message);
        }

        return response;
    }
}
