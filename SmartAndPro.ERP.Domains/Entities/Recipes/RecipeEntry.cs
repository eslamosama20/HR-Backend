using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;

namespace SmartAndPro.ERP.Domains.Entities.Recipes;

public class RecipeEntry : Entity , IEntityTypeConfiguration<RecipeEntry>
{
    
    public decimal Quantity { get; set; }
    public Guid MainPurchaseClassId { get; set; }
    public MainPurchaseClass MainPurchaseClass{ get; set; }
    public Guid RecipeId { get; set; }

    public void Configure(EntityTypeBuilder<RecipeEntry> builder)
    {
        builder.ToTable("RecipeEntry")
            .HasKey(x => x.Id);
        
        
        builder.Property(x => x.Quantity)
            .IsRequired();
        
        builder.HasOne(x=>x.MainPurchaseClass)
            .WithMany()
            .HasForeignKey(x => x.MainPurchaseClassId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne<Recipe>()
            .WithMany(t => t.RecipeEntries)
            .HasForeignKey(x => x.RecipeId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Navigation(x => x.MainPurchaseClass)
            .AutoInclude();
        
        builder.HasQueryFilter(p => p.IsActive == true);
    }

}