using Library.Model.Enums;
using Library.Model.SalesManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.SalesManagements
{
    public class SalesConfiguration : EntityTypeConfiguration<Sales>
    {
        public SalesConfiguration()
        {
            ToTable(nameof(Sales), DbSchema.Transaction);
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
        }
    }
}