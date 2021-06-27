#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class RouteStoppageConfiguration : EntityTypeConfiguration<RouteStoppage>
    {
        public RouteStoppageConfiguration()
        {
            ToTable(nameof(RouteStoppage), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}