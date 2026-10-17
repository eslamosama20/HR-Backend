using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization
{
    public class ForgotPasswordDTO
    {
        [JsonRequired]
        [Required(ErrorMessage = "Please enter a valid Email")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
