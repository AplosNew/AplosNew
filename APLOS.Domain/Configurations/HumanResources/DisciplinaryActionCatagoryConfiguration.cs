using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class DisciplinaryActionCategoryConfiguration : EntityTypeConfiguration<DisciplinaryActionCategory>
    {
        public DisciplinaryActionCategoryConfiguration()
        {
            ToTable(nameof(DisciplinaryActionCategory), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}