using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class CommitmentValueAddedProcessConfiguration : EntityTypeConfiguration<CommitmentValueAddedProcess>
    {
        public CommitmentValueAddedProcessConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(CommitmentValueAddedProcess), DbSchema.Transaction);
        }
    }
}