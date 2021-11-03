using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Machines
{
    public class CompanyGroupOperationTypeConfiguration : EntityTypeConfiguration<CompanyGroupOperationType>
    {
        public CompanyGroupOperationTypeConfiguration()
        {
            ToTable(DbTable.CompanyGroupOperationType, DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}