namespace SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public string? ArabicName { get; set; }
    public string Description { get; set; }
    
    public Guid? DomainDetailsId { get; set; }
    
    public DomainDetailsDto? DomainDetails { get; set; }
}