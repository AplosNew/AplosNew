using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class ProductionBulletinTemplateDetailConfiguration : EntityTypeConfiguration<ProductionBulletinTemplateDetail>
    {
        public ProductionBulletinTemplateDetailConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Property(t => t.AdditionalSPT).HasPrecision(18, 4);
            Property(t => t.TotalSPT).HasPrecision(18, 4);
            Property(t => t.AvgAllotedTime).HasPrecision(18, 4);
            // Table & Column Configuration
            ToTable(nameof(ProductionBulletinTemplateDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
            Ignore(r => r.OperationCode);
        }
    }
}