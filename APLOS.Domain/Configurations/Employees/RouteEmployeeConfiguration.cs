#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class RouteEmployeeConfiguration : EntityTypeConfiguration<RouteEmployee>
    {
        public RouteEmployeeConfiguration()
        {
            ToTable(nameof(RouteEmployee), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}