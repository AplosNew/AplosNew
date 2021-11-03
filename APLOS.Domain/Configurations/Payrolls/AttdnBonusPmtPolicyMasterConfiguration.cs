#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Payrolls
{
    public class AttdnBonusPmtPolicyMasterConfiguration : EntityTypeConfiguration<AttdnBonusPmtPolicyMaster>
    {
        public AttdnBonusPmtPolicyMasterConfiguration()
        {
            HasKey(t => t.ID);
            Ignore(r => r.ModelState);
            ToTable(nameof(AttdnBonusPmtPolicyMaster), DbSchema.Dbo);
        }
    }
}