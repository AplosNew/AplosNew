using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class BulletinTemplateBuyerInfoConfiguration : EntityTypeConfiguration<BulletinTemplateBuyerInfo>
    {
        public BulletinTemplateBuyerInfoConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(BulletinTemplateBuyerInfo), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}