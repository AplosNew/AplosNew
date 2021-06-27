#region Using

using Library.Model.Enums;
using Library.Model.Setups;
using Library.ViewModel.Setup;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups

{
    public class NotificationSettingConfiguration : EntityTypeConfiguration<NotificationSetting>
    {
        public NotificationSettingConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(NotificationSetting), DbSchema.Dbo);
        }
    }
}