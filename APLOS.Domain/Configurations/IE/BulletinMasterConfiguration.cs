using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class BulletinMasterConfiguration : EntityTypeConfiguration<BulletinMaster>
    {
        public BulletinMasterConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(BulletinMaster), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}