using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class RecipeConfigConfiguration : EntityTypeConfiguration<RecipeConfig>
    {
        public RecipeConfigConfiguration()
        {
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            ToTable(nameof(RecipeConfig), DbSchema.SystemConfigurationAndSetup);
        }
    }
}