using Library.Model.Enums;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class POGVendorConfiguration : EntityTypeConfiguration<POGVendor>
    {
        public POGVendorConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);

            // Table & Column Configuration
            ToTable(nameof(POGVendor), DbSchema.Transaction);
        }
    }
}