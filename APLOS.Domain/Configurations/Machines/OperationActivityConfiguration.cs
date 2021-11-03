#region Using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Machines
{
    public class OperationActivityConfiguration : EntityTypeConfiguration<OperationActivity>
    {
        public OperationActivityConfiguration()
        {
            ToTable(DbTable.OperationActivity, DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}