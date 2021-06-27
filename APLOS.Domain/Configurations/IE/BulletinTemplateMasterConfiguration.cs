using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class BulletinTemplateMasterConfiguration : EntityTypeConfiguration<BulletinTemplateMaster>
    {
        public BulletinTemplateMasterConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(BulletinTemplateMaster), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}