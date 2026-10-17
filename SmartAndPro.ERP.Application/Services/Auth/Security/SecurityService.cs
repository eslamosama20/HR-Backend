using System.Security.Cryptography;
using System.Text;

namespace SmartAndPro.ERP.Application.Services.Auth.Security;

public static class SecurityService
{
    public static string ComputeHMAC(string val, string key)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(val));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}