#region Using

using Library.Model.Biometrics;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Biometrics
{
    public class ShortLeaveAllocationConfiguration : EntityTypeConfiguration<ShortLeaveAllocation>
    {
        public ShortLeaveAllocationConfiguration()
        {
            ToTable(nameof(ShortLeaveAllocation), DbSchema.Dbo);
            HasKey(t => t.SystemID);
            Ignore(r => r.ModelState);
        }
    }
}