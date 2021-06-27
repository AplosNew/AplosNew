#region Using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups

{
    public class EmployeeLocationConfiguration : EntityTypeConfiguration<EmployeeLocation>
    {
        public EmployeeLocationConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(EmployeeLocation), DbSchema.HKP);
        }
    }
}