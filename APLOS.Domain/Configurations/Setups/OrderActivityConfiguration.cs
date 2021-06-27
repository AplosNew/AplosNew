#region Using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups

{
    public class OrderActivityConfiguration : EntityTypeConfiguration<OrderActivity>
    {
        public OrderActivityConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(OrderActivity), DbSchema.SystemConfigurationAndSetup);
        }
    }
}