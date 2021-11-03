using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class PortConfiguration : EntityTypeConfiguration<Port>
    {
        public PortConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(Port), DbSchema.Masters);
        }
    }
}