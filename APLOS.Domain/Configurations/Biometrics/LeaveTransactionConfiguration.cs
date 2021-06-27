#region Using

using Library.Model.Biometrics;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Biometrics
{
    public class LeaveTransactionConfiguration : EntityTypeConfiguration<LeaveTransaction>
    {
        public LeaveTransactionConfiguration()
        {
            ToTable(nameof(LeaveTransaction), DbSchema.Dbo);
            HasKey(t => t.SystemID);
            Ignore(r => r.ModelState);
        }
    }
}