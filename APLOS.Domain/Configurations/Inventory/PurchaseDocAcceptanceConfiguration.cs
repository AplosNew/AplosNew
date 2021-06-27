using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PurchaseDocAcceptanceConfiguration : EntityTypeConfiguration<PurchaseDocAcceptance>
    {
        public PurchaseDocAcceptanceConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(PurchaseDocAcceptance), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}