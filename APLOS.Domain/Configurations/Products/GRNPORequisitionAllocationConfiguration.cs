using Library.Model.Enums;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class GRNPORequisitionAllocationConfiguration : EntityTypeConfiguration<GRNPORequisitionAllocation>
    {
        public GRNPORequisitionAllocationConfiguration()
        {
            // Primary Key
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);          

            // Table & Column Configuration
            ToTable(nameof(GRNPORequisitionAllocation), DbSchema.Transaction);
        }
    }
}