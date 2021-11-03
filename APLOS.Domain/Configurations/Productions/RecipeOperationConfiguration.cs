using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class RecipeOperationConfiguration : EntityTypeConfiguration<RecipeOperation>
    {
        public RecipeOperationConfiguration()
        {
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            ToTable(nameof(RecipeOperation), DbSchema.HKP);
        }
    }
}