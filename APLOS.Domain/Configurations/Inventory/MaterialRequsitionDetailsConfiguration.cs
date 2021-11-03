using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class MaterialRequsitionDetailsConfiguration : EntityTypeConfiguration<MaterialRequsitionDetails>
    {
        public MaterialRequsitionDetailsConfiguration()
        {
            HasKey(t => t.Id);
            //Property(t => t.Amount).HasPrecision(18, 10);
            //Property(t => t.TotalTaxAmount).HasPrecision(18, 10);
            ToTable(nameof(MaterialRequsitionDetails), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}