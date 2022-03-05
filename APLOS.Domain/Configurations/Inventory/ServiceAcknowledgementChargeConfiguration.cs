using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class ServiceAcknowledgementChargeConfiguration : EntityTypeConfiguration<ServiceAcknowledgementCharge>
    {
        public ServiceAcknowledgementChargeConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(ServiceAcknowledgementCharge), DbSchema.Transaction);            
            Ignore(r => r.ModelState);
            
        }
    }
}