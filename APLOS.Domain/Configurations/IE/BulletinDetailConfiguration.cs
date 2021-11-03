using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class BulletinDetailConfiguration : EntityTypeConfiguration<BulletinDetail>
    {
        public BulletinDetailConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(BulletinDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}