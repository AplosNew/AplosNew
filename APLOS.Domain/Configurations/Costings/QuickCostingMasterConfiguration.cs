using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Costings
{
    public class QuickCostingMasterConfiguration : EntityTypeConfiguration<CostingMasterTemplate>
    {
        public QuickCostingMasterConfiguration()
        {
            ToTable(nameof(CostingMasterTemplate), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}