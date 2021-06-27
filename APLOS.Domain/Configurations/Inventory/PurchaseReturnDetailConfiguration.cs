using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PurchaseReturnDetailConfiguration : EntityTypeConfiguration<PurchaseReturnDetail>
    {
        public PurchaseReturnDetailConfiguration()
        {
            HasKey(t => t.Id);
            //Property(t => t.ToCurrencyRate).HasPrecision(18, 10);
            Property(t => t.MaterialTranRate).HasPrecision(18, 4);

            Property(t => t.TransactionQty).HasPrecision(18, 10);
            Property(t => t.BaseQty).HasPrecision(18, 10);
            Property(t => t.BaseUoMFactor).HasPrecision(18, 10);
            Property(t => t.MaterialTranRate).HasPrecision(18, 4);
            Property(t => t.MaterialTranAmount).HasPrecision(18, 2);
            Property(t => t.TotalMaterialTranAmount).HasPrecision(18, 2);
            Property(t => t.TotalMaterialBooksCurrencyAmount).HasPrecision(18, 2);          
            Property(t => t.TotalTaxAmount).HasPrecision(18, 2);
            Property(t => t.ChargesTranAmount).HasPrecision(18, 2);
            Property(t => t.ChargesTaxTranAmount).HasPrecision(18, 2);
            Property(t => t.TrnCurrencyBaseRate).HasPrecision(18, 4);
            Property(t => t.BooksCurrencyBaseRate).HasPrecision(18, 4);

            ToTable(nameof(PurchaseReturnDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}