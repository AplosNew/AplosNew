using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class WorkGroupConfiguration : EntityTypeConfiguration<WorkGroup>
    {
        public WorkGroupConfiguration()
        {
            ToTable(nameof(WorkGroup), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}