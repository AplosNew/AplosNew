#region using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Machines
{
    public class OperationProcessConfiguration : EntityTypeConfiguration<OperationProcess>
    {
        public OperationProcessConfiguration()
        {
            ToTable(nameof(OperationProcess), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}