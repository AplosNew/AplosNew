using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class InquiryConfiguration : EntityTypeConfiguration<Inquiry>
    {
        public InquiryConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(Inquiry), DbSchema.Transaction);
        }
    }
}