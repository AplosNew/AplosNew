using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class GRNAcceptanceMapConfiguration : EntityTypeConfiguration<GRNAcceptanceMap>
    {
        public GRNAcceptanceMapConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
           // Property(t => t.TotalQty).HasPrecision(18, 10);
            //Property(t => t.AvgRate).HasPrecision(18, 10);
            // Table & Column Configuration
            ToTable(nameof(GRNAcceptanceMap), DbSchema.Transaction); 
            Ignore(r => r.ModelState);
        }
    }
}