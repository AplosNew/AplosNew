using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class PreCostingDetailConfiguration : EntityTypeConfiguration<PreCostingDetail>
    {
        public PreCostingDetailConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(PreCostingDetail), DbSchema.Transaction);
        }
    }
}