using Library.Model.Enums;
using Library.Model.Productions;
using Library.Model.Productions.ProductionBooking;
using Library.Model.Productions.Recipe;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions.ProductionBooking
{
    public class ProductionSummaryConfiguration : EntityTypeConfiguration<ProductionSummary>
    {
        public ProductionSummaryConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(ProductionSummary), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}