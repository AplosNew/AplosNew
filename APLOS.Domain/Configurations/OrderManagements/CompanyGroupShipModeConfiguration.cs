using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class CompanyGroupShipModeConfiguration : EntityTypeConfiguration<CompanyGroupShipMode>
    {
        public CompanyGroupShipModeConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(CompanyGroupShipMode), DbSchema.Masters);
        }
    }
}