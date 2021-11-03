using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class PlantWiseSalaryRegisterSortingParametersConfiguration : EntityTypeConfiguration<PlantWiseSalaryRegisterSortingParameters>
    {
        public PlantWiseSalaryRegisterSortingParametersConfiguration()
        {
            ToTable(nameof(PlantWiseSalaryRegisterSortingParameters), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}