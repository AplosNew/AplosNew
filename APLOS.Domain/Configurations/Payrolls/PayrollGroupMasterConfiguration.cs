#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Payrolls
{
    public class PayrollGroupMasterConfiguration : EntityTypeConfiguration<PayrollGroupMaster>
    {
        public PayrollGroupMasterConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(PayrollGroupMaster), DbSchema.Masters);
        }
    }
}