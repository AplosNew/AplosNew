#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Payrolls
{
    public class BonusPolicyMasterConfiguration : EntityTypeConfiguration<BonusPolicyMaster>
    {
        public BonusPolicyMasterConfiguration()
        {
            HasKey(t => t.SystemID);
            Ignore(r => r.ModelState);
            ToTable(nameof(BonusPolicyMaster), DbSchema.Dbo);
        }
    }
}