using Library.Model.Enums;
using Library.Model.Productions.Recipe;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions.Recipe
{
    public class RecipeGlobalRawMaterialConfiguration : EntityTypeConfiguration<RecipeGlobalRawMaterial>
    {
        public RecipeGlobalRawMaterialConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);

            // Table & Column Configuration
            ToTable(nameof(RecipeGlobalRawMaterial), DbSchema.Transaction);
        }
    }
}