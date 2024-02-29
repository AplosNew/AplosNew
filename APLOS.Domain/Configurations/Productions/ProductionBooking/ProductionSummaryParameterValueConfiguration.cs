using Library.Model.Enums;
using Library.Model.Productions;
using Library.Model.Productions.ProductionBooking;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions.ProductionBooking
{
    public class ProductionSummaryParameterValueConfiguration : EntityTypeConfiguration<ProductionSummaryParameterValue>
    {
        public ProductionSummaryParameterValueConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            Property(r => r.Value).HasPrecision(18, 4);
            ToTable(nameof(ProductionSummaryParameterValue), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}