#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups
{
    public class PlantSalaryHeadSequenceConfiguration : EntityTypeConfiguration<PlantSalaryHeadSequence>
    {
        public PlantSalaryHeadSequenceConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(PlantSalaryHeadSequence), DbSchema.Masters);
        }
    }
}