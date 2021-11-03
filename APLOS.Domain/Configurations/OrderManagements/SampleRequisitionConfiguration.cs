using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class SampleRequisitionConfiguration : EntityTypeConfiguration<SampleRequisition>
    {
        public SampleRequisitionConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SampleRequisition), DbSchema.Transaction);
        }
    }
}