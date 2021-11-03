using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PurchaseDocAcceptancePOMapConfiguration : EntityTypeConfiguration<PurchaseDocAcceptancePOMap>
    {
        public PurchaseDocAcceptancePOMapConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(PurchaseDocAcceptancePOMap), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}