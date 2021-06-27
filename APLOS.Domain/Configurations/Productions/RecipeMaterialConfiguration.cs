using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class RecipeMaterialConfiguration : EntityTypeConfiguration<RecipeMaterial>
    {
        public RecipeMaterialConfiguration()
        {
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            ToTable(nameof(RecipeMaterial), DbSchema.Transaction);
        }
    }
}