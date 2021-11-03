using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class RecipeProductionOrderConfiguration : EntityTypeConfiguration<RecipeProductionOrder>
    {
        public RecipeProductionOrderConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(RecipeProductionOrder), DbSchema.Transaction);
        }
    }
}