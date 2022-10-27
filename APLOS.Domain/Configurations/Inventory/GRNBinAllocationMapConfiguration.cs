using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class GRNBinAllocationMapConfiguration : EntityTypeConfiguration<GRNBinAllocationMap>
    {
        public GRNBinAllocationMapConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            ToTable(nameof(GRNBinAllocationMap), DbSchema.Transaction); 
            Ignore(r => r.ModelState);
        }
    }
}