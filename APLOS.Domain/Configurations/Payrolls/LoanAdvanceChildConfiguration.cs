#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Payrolls
{
    public class LoanAdvanceChildConfiguration : EntityTypeConfiguration<LoanAdvanceChild>
    {
        public LoanAdvanceChildConfiguration()
        {
            ToTable(nameof(LoanAdvanceChild), DbSchema.Dbo);
            Ignore(r => r.ModelState);
            HasKey(t => t.SystemID);
        }
    }
}