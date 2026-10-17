using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;

namespace SmartAndPro.ERP.Domains.Entities.Recipes;

public class Recipe : Entity , IEntityTypeConfiguration<Recipe>
{
    public string ReferenceNumber { get; set; }
    public string? Description { get; set; }
    
    public Guid MainSalesClassId { get; set; }
    public MainSalesClass? MainSalesClass { get; set; }
    
    public List<RecipeEntry> RecipeEntries { get; set; }
    
    public int NumberOfPersons { get; set; } = 1;

    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("Recipe")
            .HasKey(x => x.Id);
        
        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.NumberOfPersons)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.Description)
            .HasMaxLength(255);
        
        builder.HasMany(x => x.RecipeEntries)
            .WithOne()
            .HasForeignKey(te => te.RecipeId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x=>x.MainSalesClass)
            .WithMany()
            .HasForeignKey(x => x.MainSalesClassId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.RecipeEntries)
            .AutoInclude();
        
        builder.Navigation(x => x.MainSalesClass)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);
    }

}