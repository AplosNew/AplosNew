using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class ServicePOAckTaxConfiguration : EntityTypeConfiguration<ServicePOAckTax>
    {
        public ServicePOAckTaxConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(ServicePOAckTax), DbSchema.Transaction);
            Ignore(r => r.ServicePoDetailId);
            Ignore(r => r.ModelState);
        }
    }
}