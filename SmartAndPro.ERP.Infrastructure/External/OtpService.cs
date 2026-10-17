namespace SmartAndPro.ERP.Infrastructure.External;

public static class OtpService
{
    public static string GenerateOtp()
    {
        var _random = new Random();
        var otpNumber = _random.Next(100000, 999999);
        return otpNumber.ToString();
    }
}