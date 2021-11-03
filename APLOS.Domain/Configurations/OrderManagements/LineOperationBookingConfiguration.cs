using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class LineOperationBookingConfiguration : EntityTypeConfiguration<LineOperationBooking>
    {
        public LineOperationBookingConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(LineOperationBooking), DbSchema.Masters);
        }
    }
}