using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class CommitmentMonthConfiguration : EntityTypeConfiguration<CommitmentMonth>
    {
        public CommitmentMonthConfiguration()
        {
            Ignore(r => r.ModelState);
            Ignore(r => r.MonthYear);
            ToTable(nameof(CommitmentMonth), DbSchema.Transaction);
        }
    }
}