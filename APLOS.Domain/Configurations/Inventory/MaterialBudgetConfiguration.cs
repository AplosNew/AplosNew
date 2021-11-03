using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class MaterialBudgetConfiguration : EntityTypeConfiguration<MaterialBudget>
    {
        public MaterialBudgetConfiguration()
        {
            HasKey(t => t.Id);
            //Property(t => t.Amount).HasPrecision(18, 10);
            //Property(t => t.TotalTaxAmount).HasPrecision(18, 10);
            ToTable(nameof(MaterialBudget), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}