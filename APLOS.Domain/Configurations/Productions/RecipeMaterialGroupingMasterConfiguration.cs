using Library.Model.Enums;
using Library.Model.Productions;
using Library.Model.Productions.Recipe;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class RecipeMaterialGroupingMasterConfiguration : EntityTypeConfiguration<RecipeMaterialGroupingMaster>
    {
        public RecipeMaterialGroupingMasterConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);

            Ignore(r => r.ModelState);
            // Table & Column Configuration
            ToTable(nameof(RecipeMaterialGroupingMaster), DbSchema.Masters);
        }
    }
}