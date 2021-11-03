using Library.Model.Enums;
using Library.Model.Productions.Recipe;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions.Recipe
{
    public class RecipeGlobalUtilityConfiguration : EntityTypeConfiguration<RecipeGlobalUtility>
    {
        public RecipeGlobalUtilityConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(RecipeGlobalUtility), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}