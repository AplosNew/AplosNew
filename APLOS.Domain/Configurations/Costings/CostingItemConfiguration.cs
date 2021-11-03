using Library.Model.Costings;
using Library.Model.Enums;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Costings.IssueTracker
{
    public class CostingItemConfiguration : EntityTypeConfiguration<CostingItem>
    {
        public CostingItemConfiguration()
        {
            ToTable(nameof(CostingItem), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}