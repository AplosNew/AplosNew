using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class ServicePOTaxConfiguration : EntityTypeConfiguration<ServicePOTax>
    {
        public ServicePOTaxConfiguration()
        {
            HasKey(t => t.Id);
            //Property(t => t.Amount).HasPrecision(18, 10);
            //Property(t => t.TotalTaxAmount).HasPrecision(18, 10);
            ToTable(nameof(ServicePOTax), DbSchema.Transaction);
            Ignore(r => r.ServiceMasterId);
            Ignore(r => r.ServiceRequsitionDetailId);
            Ignore(r => r.ModelState);
            
        }
    }
}