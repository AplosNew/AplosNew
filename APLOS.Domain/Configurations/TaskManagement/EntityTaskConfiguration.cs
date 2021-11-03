using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class EntityTaskConfiguration : EntityTypeConfiguration<EntityTask>
    {
        public EntityTaskConfiguration()
        {
            ToTable(nameof(EntityTask), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}