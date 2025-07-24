#region Using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Machines
{
    public class OperationVariationProductMasterConfiguration : EntityTypeConfiguration<OperationVariationProductMaster>
    {
        public OperationVariationProductMasterConfiguration()
        {
            ToTable(nameof(OperationVariationProductMaster), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}