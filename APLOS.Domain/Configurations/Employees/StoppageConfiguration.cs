#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class StoppageConfiguration : EntityTypeConfiguration<Stoppage>
    {
        public StoppageConfiguration()
        {
            ToTable(nameof(Stoppage), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}