#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class JobDescriptionItemConfiguration : EntityTypeConfiguration<JobDescriptionItem>
    {
        public JobDescriptionItemConfiguration()
        {
            ToTable(nameof(JobDescriptionItem), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}