using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class CriticalConfiguration : EntityTypeConfiguration<Critical>
    {
        public CriticalConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(Critical), DbSchema.HKP);
        }
    }
}
