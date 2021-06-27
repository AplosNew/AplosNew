#region Using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Machines
{
    public class OperationVariationConfiguration : EntityTypeConfiguration<OperationVariation>
    {
        public OperationVariationConfiguration()
        {
            ToTable(nameof(OperationVariation), DbSchema.Masters);
            Ignore(r => r.ModelState);
            Property(r => r.AdditionalSAM).HasPrecision(18,4);
            Property(r => r.SubOperationSAM).HasPrecision(18,4);
            Property(r => r.TotalSAM).HasPrecision(18,4);
            Property(r => r.MachineAllowance).HasPrecision(18,4);
            Property(r => r.AdditionalAllowance).HasPrecision(18,4);
        }
    }
}