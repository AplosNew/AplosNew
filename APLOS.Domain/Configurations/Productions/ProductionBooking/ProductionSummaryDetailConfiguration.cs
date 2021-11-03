using Library.Model.Enums;
using Library.Model.Productions;
using Library.Model.Productions.ProductionBooking;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions.ProductionBooking
{
    public class ProductionSummaryDetailConfiguration : EntityTypeConfiguration<ProductionSummaryDetail>
    {
        public ProductionSummaryDetailConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(ProductionSummaryDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}