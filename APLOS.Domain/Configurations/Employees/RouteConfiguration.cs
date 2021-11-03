#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class RouteConfiguration : EntityTypeConfiguration<Route>
    {
        public RouteConfiguration()
        {
            ToTable(nameof(Route), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}