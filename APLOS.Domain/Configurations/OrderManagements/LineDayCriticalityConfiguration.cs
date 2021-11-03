using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class LineDayCriticalityConfiguration : EntityTypeConfiguration<LineDayCriticality>
    {
        public LineDayCriticalityConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(LineDayCriticality), DbSchema.SystemConfigurationAndSetup);
        }
    }
}
