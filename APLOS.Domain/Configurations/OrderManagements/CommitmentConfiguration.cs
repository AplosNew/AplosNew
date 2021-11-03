using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class CommitmentConfiguration : EntityTypeConfiguration<Commitment>
    {
        public CommitmentConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(Commitment), DbSchema.Transaction);
        }
    }
}