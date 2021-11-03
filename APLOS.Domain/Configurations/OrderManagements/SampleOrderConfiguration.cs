using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class SampleOrderConfiguration : EntityTypeConfiguration<SampleOrder>
    {
        public SampleOrderConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SampleOrder), DbSchema.Transaction);
        }
    }
}