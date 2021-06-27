using Library.Model.Enums;
using Library.Model.Productions.Recipe;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions.Recipe
{
    public class RecipeGlobalMasterConfiguration : EntityTypeConfiguration<RecipeGlobalMaster>
    {
        public RecipeGlobalMasterConfiguration()
        {
            // Primary Key
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(RecipeGlobalMaster), DbSchema.Transaction);
        }
    }
}