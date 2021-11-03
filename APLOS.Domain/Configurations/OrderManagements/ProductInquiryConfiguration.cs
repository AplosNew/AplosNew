using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class ProductInquiryConfiguration : EntityTypeConfiguration<ProductInquiry>
    {
        public ProductInquiryConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ProductInquiry), DbSchema.Transaction);
        }
    }
}