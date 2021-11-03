#region Using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Machines
{
    public class CompanyGroupOperationActivityConfiguration : EntityTypeConfiguration<CompanyGroupOperationActivity>
    {
        public CompanyGroupOperationActivityConfiguration()
        {
            ToTable(DbTable.CompanyGroupOperationActivity, DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}