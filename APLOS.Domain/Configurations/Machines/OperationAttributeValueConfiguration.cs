#region Using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Machines
{
    public class OperationAttributeValueConfiguration : EntityTypeConfiguration<OperationAttributeValue>
    {
        public OperationAttributeValueConfiguration()
        {
            ToTable(nameof(OperationAttributeValue), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}