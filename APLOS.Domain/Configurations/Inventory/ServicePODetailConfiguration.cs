using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class ServicePODetailConfiguration : EntityTypeConfiguration<ServicePODetail>
    {
        public ServicePODetailConfiguration()
        {
            HasKey(t => t.Id);
            //Property(t => t.ToCurrencyRate).HasPrecision(18, 10);
            ToTable(nameof(ServicePODetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}