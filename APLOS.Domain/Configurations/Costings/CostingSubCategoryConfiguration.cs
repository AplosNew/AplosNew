using Library.Model.Costings;
using Library.Model.Enums;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Costings.IssueTracker
{
    public class CostingSubCategoryConfiguration : EntityTypeConfiguration<CostingComponent>
    {
        public CostingSubCategoryConfiguration()
        {
            ToTable(nameof(CostingComponent), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}