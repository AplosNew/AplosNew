#region Using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Machines
{
    public class OperationVariationAttributeValueConfiguration : EntityTypeConfiguration<OperationVariationAttributeValue>
    {
        public OperationVariationAttributeValueConfiguration()
        {
            ToTable(nameof(OperationVariationAttributeValue), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}