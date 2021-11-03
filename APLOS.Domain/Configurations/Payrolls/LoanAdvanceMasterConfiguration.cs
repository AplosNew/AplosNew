#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Payrolls
{
    public class LoanAdvanceMasterConfiguration : EntityTypeConfiguration<LoanAdvanceMaster>
    {
        public LoanAdvanceMasterConfiguration()
        {
            ToTable(nameof(LoanAdvanceMaster), DbSchema.Dbo);
            HasKey(t => t.SystemID);
            Ignore(t => t.Active);
            Ignore(t => t.ModelState);
        }
    }
}