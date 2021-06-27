using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class CommitmentInquiryConfiguration : EntityTypeConfiguration<CommitmentInquiry>
    {
        public CommitmentInquiryConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(CommitmentInquiry), DbSchema.Transaction);
        }
    }
}