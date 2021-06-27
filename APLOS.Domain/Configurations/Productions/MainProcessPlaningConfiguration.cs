using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class MainProcessPlaningConfiguration : EntityTypeConfiguration<MainProcessPlanning>
    {
        public MainProcessPlaningConfiguration()
        {
            Ignore(r => r.ModelState);
            Ignore(t => t.IsDb);
            ToTable(nameof(MainProcessPlanning), DbSchema.Transaction);
        }
    }
}