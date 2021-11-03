using Library.Model.Enums;
using Library.Model.Productions.Recipe;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions.Recipe
{
    public class RecipeGlobalOperationConfiguration : EntityTypeConfiguration<RecipeGlobalOperation>
    {
        public RecipeGlobalOperationConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);

            // Table & Column Configuration
            ToTable(nameof(RecipeGlobalOperation), DbSchema.Transaction);
        }
    }
}