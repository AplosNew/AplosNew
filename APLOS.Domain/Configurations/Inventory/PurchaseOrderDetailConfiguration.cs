using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PurchaseOrderDetailConfiguration : EntityTypeConfiguration<PurchaseOrderDetail>
    {
        public PurchaseOrderDetailConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.TransactionQty).HasPrecision(18, 10);
            Property(t => t.BaseQty).HasPrecision(18, 10);
            Property(t => t.BaseUoMFactor).HasPrecision(18, 10);
            Property(t => t.TransactionRate).HasPrecision(18, 4);
            Property(t => t.TransactionAmount).HasPrecision(18, 2);
            Property(t => t.BaseAmount).HasPrecision(18, 2);
            Property(t => t.IssueQty).HasPrecision(18, 10);
            Property(t => t.TotalTaxAmount).HasPrecision(18, 2);
            Property(t => t.ChargesAmount).HasPrecision(18, 2);
            ToTable(nameof(PurchaseOrderDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}