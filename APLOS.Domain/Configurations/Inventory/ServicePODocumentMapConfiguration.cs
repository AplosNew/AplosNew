using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class ServicePODocumentMapConfiguration : EntityTypeConfiguration<ServicePODocumentMap>
    {
        public ServicePODocumentMapConfiguration()
        {
            HasKey(t => t.Id);
           // Property(t => t.ToCurrencyRate).HasPrecision(18, 10);
            ToTable(nameof(ServicePODocumentMap), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}