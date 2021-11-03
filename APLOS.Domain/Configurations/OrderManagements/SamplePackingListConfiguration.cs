using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class SamplePackingListConfiguration : EntityTypeConfiguration<SamplePackingList>
    {
        public SamplePackingListConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SamplePackingList), DbSchema.Transaction);
        }
    }
}