#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class SOPItemConfiguration : EntityTypeConfiguration<SOPItem>
    {
        public SOPItemConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SOPItem), DbSchema.HKP);
        }
    }
}