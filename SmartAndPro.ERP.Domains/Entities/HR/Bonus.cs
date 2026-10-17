using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities.HR;

public class Bonus : Entity , IEntityTypeConfiguration<Bonus>
{
    public sealed override Guid Id { get; set; }
    
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; }
    
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    
    
    public void Configure(EntityTypeBuilder<Bonus> builder)
    {
        builder
            .ToTable("Bonuses")
            .HasKey(x => x.Id);
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        builder.Navigation(p => p.Employee).AutoInclude();

        builder.HasQueryFilter(p => p.IsActive);
    }
}