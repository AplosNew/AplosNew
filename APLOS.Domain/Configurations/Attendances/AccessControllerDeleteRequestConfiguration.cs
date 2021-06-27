using Library.Model.Attendances;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Attendances
{
    public class AccessControllerDeleteRequestConfiguration : EntityTypeConfiguration<AccessControllerDeleteRequest>
    {
        public AccessControllerDeleteRequestConfiguration()
        {
            ToTable(nameof(AccessControllerDeleteRequest), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}