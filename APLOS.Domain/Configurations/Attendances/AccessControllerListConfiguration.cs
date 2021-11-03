using Library.Model.Attendances;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Attendances
{
    public class AccessControllerListConfiguration : EntityTypeConfiguration<AccessControllerList>
    {
        public AccessControllerListConfiguration()
        {
            ToTable(nameof(AccessControllerList), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}