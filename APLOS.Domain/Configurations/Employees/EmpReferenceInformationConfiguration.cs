#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class EmpReferenceInformationConfiguration : EntityTypeConfiguration<EmpReferenceInformation>
    {
        public EmpReferenceInformationConfiguration()
        {
            ToTable(nameof(EmpReferenceInformation), DbSchema.Dbo);
            HasKey(r => r.SystemID);
            Ignore(r => r.ModelState);
        }
    }
}