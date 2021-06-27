#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class ApprovalConfigurationConfiguration : EntityTypeConfiguration<ApprovalConfiguration>
    {
        public ApprovalConfigurationConfiguration()
        {
            ToTable(nameof(ApprovalConfiguration), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}