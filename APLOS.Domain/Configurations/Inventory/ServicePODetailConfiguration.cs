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
            Property(t => t.Rate).HasPrecision(18, 4);
            ToTable(nameof(ServicePODetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}