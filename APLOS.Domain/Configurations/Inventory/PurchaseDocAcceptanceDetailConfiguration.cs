using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PurchaseDocAcceptanceDetailConfiguration : EntityTypeConfiguration<PurchaseDocAcceptanceDetail>
    {
        public PurchaseDocAcceptanceDetailConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(PurchaseDocAcceptanceDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}