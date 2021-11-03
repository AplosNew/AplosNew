#region Using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Machines
{
    public class OperationCategoryConfiguration : EntityTypeConfiguration<OperationCategory>
    {
        public OperationCategoryConfiguration()
        {
            ToTable(DbTable.OperationCategory, DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}