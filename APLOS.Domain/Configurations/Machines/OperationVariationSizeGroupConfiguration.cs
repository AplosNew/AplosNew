#region Using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Machines
{
    public class OperationVariationSizeGroupConfiguration : EntityTypeConfiguration<OperationVariationSizeGroup>
    {
        public OperationVariationSizeGroupConfiguration()
        {
            ToTable(nameof(OperationVariationSizeGroup), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}