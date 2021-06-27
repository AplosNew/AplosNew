using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class GateEntryConfiguration : EntityTypeConfiguration<GateEntry>
    {
        public GateEntryConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(GateEntry), DbSchema.Transaction);
        }
    }
}

