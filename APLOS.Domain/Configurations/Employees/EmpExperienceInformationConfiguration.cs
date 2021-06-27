#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class EmpExperienceInformationConfiguration : EntityTypeConfiguration<EmpExperienceInformation>
    {
        public EmpExperienceInformationConfiguration()
        {
            ToTable(nameof(EmpExperienceInformation), DbSchema.Dbo);
            HasKey(r => r.SystemID);
            Ignore(r => r.ModelState);
        }
    }
}