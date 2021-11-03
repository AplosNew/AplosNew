using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class SalaryFixationMailConfiguration : EntityTypeConfiguration<SalaryFixationMail>
    {
        public SalaryFixationMailConfiguration()
        {
            ToTable(nameof(SalaryFixationMail), DbSchema.SystemConfigurationAndSetup);
            Ignore(a => a.ModelState);
        }
    }
}