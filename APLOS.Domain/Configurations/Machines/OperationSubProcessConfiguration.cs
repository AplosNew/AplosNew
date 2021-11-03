#region using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Machines
{
    public class OperationSubProcessConfiguration : EntityTypeConfiguration<OperationSubProcess>
    {
        public OperationSubProcessConfiguration()
        {
            Ignore(t => t.Archive);
            Ignore(r => r.ModelState);
            ToTable(nameof(OperationSubProcess), DbSchema.Masters);
        }
    }
}