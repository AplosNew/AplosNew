using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class ProductionBulletinTemplateConfiguration : EntityTypeConfiguration<ProductionBulletinTemplate>
    {
        public ProductionBulletinTemplateConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(ProductionBulletinTemplate), DbSchema.Transaction);
            Ignore(r => r.ModelState);
            Ignore(r => r.ProductionOrderId);
        }
    }
}