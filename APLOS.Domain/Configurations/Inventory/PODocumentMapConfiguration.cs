using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class PODocumentMapConfiguration : EntityTypeConfiguration<PODocumentMap>
    {
        public PODocumentMapConfiguration()
        {
            HasKey(t => t.Id);
           // Property(t => t.ToCurrencyRate).HasPrecision(18, 10);
            ToTable(nameof(PODocumentMap), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}