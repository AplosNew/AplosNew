#region Using

using Library.Model.Biometrics;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Biometrics
{
    public class EmployeeFPInformationConfiguration : EntityTypeConfiguration<EmployeeFPInformation>
    {
        public EmployeeFPInformationConfiguration()
        {
            ToTable(nameof(EmployeeFPInformation), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}