using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class BulletinTemplateConfiguration : EntityTypeConfiguration<BulletinTemplate>
    {
        public BulletinTemplateConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(BulletinTemplate), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}