#region using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Machines
{
    public class OperationAttributeConfiguration : EntityTypeConfiguration<OperationAttribute>
    {
        public OperationAttributeConfiguration()
        {
            ToTable(nameof(OperationAttribute), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}