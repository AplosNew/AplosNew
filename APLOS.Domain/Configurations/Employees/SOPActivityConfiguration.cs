#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class SOPActivityConfiguration : EntityTypeConfiguration<SOPActivity>
    {
        public SOPActivityConfiguration()
        {
            ToTable(nameof(SOPActivity), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}