#region using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Machines
{
    public class OperationConfiguration : EntityTypeConfiguration<Operation>
    {
        public OperationConfiguration()
        {
            ToTable(nameof(Operation), DbSchema.Masters);
            Ignore(r => r.ModelState);
            Property(r => r.BasicProcessTime).HasPrecision(18, 4);
            Property(r => r.AssociateProcessTime).HasPrecision(18, 4);
            Property(r => r.PersonalAllowance).HasPrecision(18, 4);
            Property(r => r.MachineAllowance).HasPrecision(18, 4);
            Property(r => r.AdditionalAllowance).HasPrecision(18, 4);
        }
    }
}