using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class ServicePOMasterConfiguration : EntityTypeConfiguration<ServicePOMaster>
    {
        public ServicePOMasterConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.ToCurrencyRate).HasPrecision(18, 10);
            ToTable(nameof(ServicePOMaster), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}