using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class DisciplinaryActionMasterConfiguration : EntityTypeConfiguration<DisciplinaryActionMaster>
    {
        public DisciplinaryActionMasterConfiguration()
        {
            ToTable(nameof(DisciplinaryActionMaster), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}