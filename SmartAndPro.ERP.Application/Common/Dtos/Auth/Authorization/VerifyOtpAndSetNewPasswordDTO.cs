namespace SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization
{
    public class VerifyOtpAndSetNewPasswordDTO
    {
        public string OtpReference { get; set; }
        public string OtpValue { get; set; }
        public string UserId { get; set; }
        public string NewPassword { get; set; }
    }
}
