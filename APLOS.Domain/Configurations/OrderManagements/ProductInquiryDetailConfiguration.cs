using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class ProductInquiryDetailConfiguration : EntityTypeConfiguration<ProductInquiryDetail>
    {
        public ProductInquiryDetailConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(ProductInquiryDetail), DbSchema.Transaction);
        }
    }
}