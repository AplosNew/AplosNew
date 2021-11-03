using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PurchaseDocAcceptanceChargesConfiguration : EntityTypeConfiguration<PurchaseDocAcceptanceCharges>
    {
        public PurchaseDocAcceptanceChargesConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(PurchaseDocAcceptanceCharges), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}