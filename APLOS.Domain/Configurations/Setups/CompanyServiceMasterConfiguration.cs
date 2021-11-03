#region Using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups
{
    public class ServiceMasterCompanyExtensionConfiguration : EntityTypeConfiguration<CompanyServiceMaster>
    {
        public ServiceMasterCompanyExtensionConfiguration()
        {
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            ToTable(nameof(CompanyServiceMaster), DbSchema.HKP);
        }
    }
}