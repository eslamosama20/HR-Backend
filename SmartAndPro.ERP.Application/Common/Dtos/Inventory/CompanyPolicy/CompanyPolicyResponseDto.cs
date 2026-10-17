using System;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPolicy;

public class CompanyPolicyResponseDto
    {
        public Guid Id { get; set; }
        public string? PolicyParagraph { get; set; }

        public bool IsPrimary { get; set; }
    }