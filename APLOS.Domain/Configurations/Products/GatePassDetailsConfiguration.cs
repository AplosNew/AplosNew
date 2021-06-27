using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class GatePassDetailsConfiguration : EntityTypeConfiguration<GatePassDetails>
    {
        public GatePassDetailsConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(GatePassDetails), DbSchema.Transaction);
        }
    }
}

