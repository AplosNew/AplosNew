using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class BulletinProcessConfiguration : EntityTypeConfiguration<BulletinProcess>
    {
        public BulletinProcessConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(BulletinProcess), DbSchema.Transaction);
        }
    }
}