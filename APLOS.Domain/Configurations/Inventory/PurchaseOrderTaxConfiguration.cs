using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PurchaseOrderTaxConfiguration : EntityTypeConfiguration<PurchaseOrderTax>
    {
        public PurchaseOrderTaxConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.Percentage).HasPrecision(18, 10);
            Property(t => t.TaxAmount).HasPrecision(18, 10);
            ToTable(nameof(PurchaseOrderTax), DbSchema.Transaction);
            Ignore(r => r.ModelState);
           // Ignore(r => r.TotalAmount);
            
        }
    }
}