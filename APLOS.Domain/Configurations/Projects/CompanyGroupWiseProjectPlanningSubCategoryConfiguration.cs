using Library.Model.Enums;
using Library.Model.Projects;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Projects
{
    public class CompanyGroupWiseProjectPlanningSubCategoryConfiguration : EntityTypeConfiguration<CompanyGroupWiseProjectPlanningSubCategory>
    {
        public CompanyGroupWiseProjectPlanningSubCategoryConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(CompanyGroupWiseProjectPlanningSubCategory), DbSchema.HKP);
        }
    }
}