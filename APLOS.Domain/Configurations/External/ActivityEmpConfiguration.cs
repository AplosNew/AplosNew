#region Using

using Library.Model.Enums;
using Library.Model.External;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.External
{
    public class ActivityEmpConfiguration : EntityTypeConfiguration<ActivityEmp>
    {
        public ActivityEmpConfiguration()
        {
            ToTable(nameof(ActivityEmp), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}