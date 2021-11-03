#region Using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Machines
{
    public class CompanyGroupOperationCategoryConfiguration : EntityTypeConfiguration<CompanyGroupOperationCategory>
    {
        public CompanyGroupOperationCategoryConfiguration()
        {
            ToTable(DbTable.CompanyGroupOperationCategory, DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}