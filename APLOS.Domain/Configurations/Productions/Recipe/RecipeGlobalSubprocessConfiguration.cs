using Library.Model.Enums;
using Library.Model.Productions.Recipe;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions.Recipe
{
    public class RecipeGlobalSubprocessConfiguration : EntityTypeConfiguration<RecipeGlobalSubprocess>
    {
        public RecipeGlobalSubprocessConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);

            // Table & Column Configuration
            ToTable(nameof(RecipeGlobalSubprocess), DbSchema.Transaction);
        }
    }
}