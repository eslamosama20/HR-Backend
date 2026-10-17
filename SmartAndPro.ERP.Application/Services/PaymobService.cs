using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace SmartAndPro.ERP.Application.Services
{
    public class PaymobService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly int _integrationId;
        private readonly string _iframeId;
        private readonly string _currency;

        public PaymobService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            
            _baseUrl = configuration["Paymob:BaseUrl"] ?? "https://accept.paymob.com/api";
            _apiKey = configuration["Paymob:ApiKey"];
            _integrationId = int.Parse(configuration["Paymob:IntegrationId"] ?? "0");
            _iframeId = configuration["Paymob:IframeId"];
            _currency = configuration["Paymob:Currency"] ?? "EGP";
        }

        public async Task<PaymobPaymentResponse> InitiatePaymentAsync(
            decimal amountInEGP,
            string merchantOrderId,
            PaymobBillingData billingData)
        {
            try
            {
                // Step 1: Authenticate
                var authToken = await AuthenticateAsync();
                
                // Step 2: Create Order
                var amountCents = (int)(amountInEGP * 100);
                var orderId = await CreateOrderAsync(authToken, amountCents, merchantOrderId);
                
                // Step 3: Get Payment Key
                var paymentToken = await GetPaymentKeyAsync(authToken, orderId, amountCents, billingData);
                
                // Step 4: Redirect to Paymob Payment Page (Standard Redirect - NO IFRAME!)
                // This is the official Paymob redirect URL that works without iframe
                var paymentUrl = $"https://accept.paymob.com/api/acceptance/post_pay";
                
                Console.WriteLine($"✅ Payment Token Generated: {paymentToken}");
                Console.WriteLine($"✅ Payment URL (Standard Redirect): {paymentUrl}");
                
                return new PaymobPaymentResponse
                {
                    PaymentToken = paymentToken,
                    OrderId = orderId,
                    PaymentUrl = paymentUrl,
                    IntegrationId = _integrationId,
                    IframeId = null // No iframe needed!
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Paymob payment initialization failed: {ex.Message}", ex);
            }
        }

        private async Task<string> AuthenticateAsync()
        {
            var url = $"{_baseUrl}/auth/tokens";
            var body = new { api_key = _apiKey };
            
            var response = await _httpClient.PostAsJsonAsync(url, body);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<PaymobAuthResponse>();
            return result?.Token ?? throw new Exception("Authentication failed - no token returned");
        }

        private async Task<int> CreateOrderAsync(string authToken, int amountCents, string merchantOrderId)
        {
            var url = $"{_baseUrl}/ecommerce/orders";
            var body = new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = amountCents,
                currency = _currency,
                items = new[]
                {
                    new
                    {
                        name = "Hotel Booking",
                        amount_cents = amountCents,
                        description = $"Booking {merchantOrderId}",
                        quantity = 1
                    }
                }
            };
            
            var response = await _httpClient.PostAsJsonAsync(url, body);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Paymob order creation failed: {response.StatusCode} - {responseContent}");
            }
            
            var result = JsonConvert.DeserializeObject<PaymobOrderResponse>(responseContent);
            return result?.Id ?? throw new Exception("Order creation failed - no order ID returned");
        }

        private async Task<string> GetPaymentKeyAsync(
            string authToken,
            int orderId,
            int amountCents,
            PaymobBillingData billingData)
        {
            var url = $"{_baseUrl}/acceptance/payment_keys";
            
            // Sanitize billing data for Paymob (only Latin characters accepted)
            var sanitizedBillingData = new
            {
                apartment = string.IsNullOrWhiteSpace(billingData.Apartment) ? "NA" : RemoveArabicCharacters(billingData.Apartment),
                email = billingData.Email ?? "guest@booking.com",
                floor = string.IsNullOrWhiteSpace(billingData.Floor) ? "NA" : RemoveArabicCharacters(billingData.Floor),
                first_name = RemoveArabicCharacters(billingData.FirstName) ?? "Guest",
                street = string.IsNullOrWhiteSpace(billingData.Street) ? "NA" : RemoveArabicCharacters(billingData.Street),
                building = string.IsNullOrWhiteSpace(billingData.Building) ? "NA" : RemoveArabicCharacters(billingData.Building),
                phone_number = CleanPhoneNumber(billingData.PhoneNumber),
                shipping_method = "NA",
                postal_code = billingData.PostalCode ?? "00000",
                city = RemoveArabicCharacters(billingData.City) ?? "Cairo",
                country = billingData.Country ?? "EG",
                last_name = RemoveArabicCharacters(billingData.LastName) ?? "User",
                state = string.IsNullOrWhiteSpace(billingData.State) ? "NA" : RemoveArabicCharacters(billingData.State)
            };
            
            var body = new
            {
                auth_token = authToken,
                amount_cents = amountCents.ToString(),
                expiration = 3600,
                order_id = orderId.ToString(),
                billing_data = sanitizedBillingData,
                currency = _currency,
                integration_id = _integrationId
            };
            
            var requestJson = JsonConvert.SerializeObject(body);
            Console.WriteLine($"🔑 Payment Key Request: {requestJson}");
            
            var response = await _httpClient.PostAsJsonAsync(url, body);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"📥 Payment Key Response ({response.StatusCode}): {responseContent}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Paymob payment key failed: {response.StatusCode} - {responseContent}");
            }
            
            var result = JsonConvert.DeserializeObject<PaymobPaymentKeyResponse>(responseContent);
            return result?.Token ?? throw new Exception("Payment key generation failed - no token returned");
        }
        
        private string RemoveArabicCharacters(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // Remove Arabic characters, keep only Latin letters, numbers, spaces, and basic punctuation
            var cleaned = new string(input.Where(c => 
                (c >= 'A' && c <= 'Z') || 
                (c >= 'a' && c <= 'z') || 
                (c >= '0' && c <= '9') || 
                c == ' ' || c == '.' || c == '-' || c == '_'
            ).ToArray()).Trim();

            return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
        }

        private string CleanPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return "01000000000";

            // Remove all non-numeric characters
            var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Ensure it starts with country code (20 for Egypt)
            if (!cleaned.StartsWith("20"))
            {
                // If starts with 0, replace with 20
                if (cleaned.StartsWith("0"))
                    cleaned = "20" + cleaned.Substring(1);
                else
                    cleaned = "20" + cleaned;
            }

            return cleaned;
        }

        private async Task<string> CreateMobileWalletPaymentAsync(string authToken, int orderId, int amountCents, string paymentToken, PaymobBillingData billingData)
        {
            try
            {
                // Using Paymob's Mobile Wallet API (no iframe needed)
                var url = $"{_baseUrl}/acceptance/payments/pay";
                
                var requestBody = new
                {
                    source = new
                    {
                        identifier = CleanPhoneNumber(billingData.PhoneNumber),
                        subtype = "WALLET"
                    },
                    payment_token = paymentToken
                };

                var requestJson = JsonConvert.SerializeObject(requestBody);
                Console.WriteLine($"� Mobile Wallet Payment Request: {requestJson}");

                var response = await _httpClient.PostAsJsonAsync(url, requestBody);
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"📥 Mobile Wallet Response ({response.StatusCode}): {content}");

                response.EnsureSuccessStatusCode();

                var result = JsonConvert.DeserializeObject<MobileWalletResponse>(content);
                
                // Return redirect URL or transaction ID
                if (!string.IsNullOrEmpty(result?.RedirectUrl))
                    return result.RedirectUrl;
                    
                return result?.Id?.ToString() ?? throw new Exception("Mobile wallet payment creation failed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating mobile wallet payment: {ex.Message}");
                throw;
            }
        }
    }

    // DTOs
    public class PaymobPaymentResponse
    {
        public string PaymentToken { get; set; }
        public int OrderId { get; set; }
        public string PaymentUrl { get; set; }
        public int IntegrationId { get; set; }
        public string IframeId { get; set; }
    }

    public class PaymobAuthResponse
    {
        public string Token { get; set; }
    }

    public class PaymobOrderResponse
    {
        public int Id { get; set; }
    }

    public class PaymobPaymentKeyResponse
    {
        public string Token { get; set; }
    }

    public class PaymentIntentionResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("pending")]
        public bool Pending { get; set; }
        
        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public class MobileWalletResponse
    {
        [JsonProperty("id")]
        public int? Id { get; set; }
        
        [JsonProperty("pending")]
        public bool Pending { get; set; }
        
        [JsonProperty("success")]
        public bool Success { get; set; }
        
        [JsonProperty("redirect_url")]
        public string RedirectUrl { get; set; }
        
        [JsonProperty("iframe_redirection_url")]
        public string IframeRedirectionUrl { get; set; }
    }

    public class PaymobBillingData
    {
        [JsonProperty("apartment")]
        public string Apartment { get; set; }
        
        [JsonProperty("email")]
        public string Email { get; set; }
        
        [JsonProperty("floor")]
        public string Floor { get; set; }
        
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        
        [JsonProperty("street")]
        public string Street { get; set; }
        
        [JsonProperty("building")]
        public string Building { get; set; }
        
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }
        
        [JsonProperty("shipping_method")]
        public string ShippingMethod { get; set; }
        
        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }
        
        [JsonProperty("city")]
        public string City { get; set; }
        
        [JsonProperty("country")]
        public string Country { get; set; }
        
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        
        [JsonProperty("state")]
        public string State { get; set; }
    }
}
