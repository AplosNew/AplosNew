#region using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Machines
{
    public class OperationFgComponentConfiguration : EntityTypeConfiguration<OperationFgComponent>
    {
        public OperationFgComponentConfiguration()
        {
            Ignore(t => t.Archive);
            Ignore(r => r.ModelState);
            ToTable(nameof(OperationFgComponent), DbSchema.Masters);
        }
    }
}