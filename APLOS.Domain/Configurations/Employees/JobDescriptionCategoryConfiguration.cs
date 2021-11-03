#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class JobDescriptionCategoryConfiguration : EntityTypeConfiguration<JobDescriptionCategory>
    {
        public JobDescriptionCategoryConfiguration()
        {
            ToTable(nameof(JobDescriptionCategory), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}