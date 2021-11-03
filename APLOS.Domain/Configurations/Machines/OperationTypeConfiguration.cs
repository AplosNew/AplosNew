#region Using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Machines
{
    public class OperationTypeConfiguration : EntityTypeConfiguration<OperationType>
    {
        public OperationTypeConfiguration()
        {
            ToTable(nameof(OperationType), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}