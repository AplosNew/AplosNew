using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class ProductionBulletinTemplateMasterConfiguration : EntityTypeConfiguration<ProductionBulletinTemplateMaster>
    {
        public ProductionBulletinTemplateMasterConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(ProductionBulletinTemplateMaster), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}