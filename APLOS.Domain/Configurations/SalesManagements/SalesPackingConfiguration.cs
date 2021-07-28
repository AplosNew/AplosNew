using Library.Model.Enums;
using Library.Model.SalesManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.SalesManagements
{
    public class SalesPackingConfiguration : EntityTypeConfiguration<SalesPacking>
    {
        public SalesPackingConfiguration()
        {
            ToTable(nameof(SalesPacking), DbSchema.Dbo);
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
        }
    }
}