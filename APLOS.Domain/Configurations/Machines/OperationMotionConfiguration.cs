#region using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Machines
{
    public class OperationMotionConfiguration : EntityTypeConfiguration<OperationMotion>
    {
        public OperationMotionConfiguration()
        {
            ToTable(nameof(OperationMotion), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}