using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class PlanningTypesConfiguration : EntityTypeConfiguration<PlanningTypes>
    {
        public PlanningTypesConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(PlanningTypes), DbSchema.Dbo);
        }
    }
}