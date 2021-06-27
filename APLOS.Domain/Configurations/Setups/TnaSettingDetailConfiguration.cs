#region using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Setups
{
    public class TnaSettingDetailConfiguration : EntityTypeConfiguration<TnaSettingDetail>
    {
        public TnaSettingDetailConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(TnaSettingDetail), DbSchema.Dbo);
        }
    }
}