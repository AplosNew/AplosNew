using Library.Model.Accounts;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Accounts
{
    public class ServiceMasterPartyConfiguration : EntityTypeConfiguration<ServiceMasterParty>
    {
        public ServiceMasterPartyConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(ServiceMasterParty), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}