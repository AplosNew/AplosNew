#region using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Setups
{
    public class TnaSettingMasterConfiguration : EntityTypeConfiguration<TnaSettingMaster>
    {
        public TnaSettingMasterConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(TnaSettingMaster), DbSchema.Dbo);
        }
    }
}