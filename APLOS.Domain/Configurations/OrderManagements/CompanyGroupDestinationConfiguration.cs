using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class CompanyGroupDestinationConfiguration : EntityTypeConfiguration<CompanyGroupDestination>
    {
        public CompanyGroupDestinationConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(CompanyGroupDestination), DbSchema.Masters);
        }
    }
}