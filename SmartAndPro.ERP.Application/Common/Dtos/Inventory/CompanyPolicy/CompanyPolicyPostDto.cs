using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPolicy;

public class CompanyPolicyPostDto
    {
    public string? PolicyParagraph { get; set; }

    public bool IsPrimary { get; set; }
    
    }