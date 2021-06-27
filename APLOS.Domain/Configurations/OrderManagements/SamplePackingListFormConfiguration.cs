using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class SamplePackingListFormConfiguration : EntityTypeConfiguration<SamplePackingListForm>
    {
        public SamplePackingListFormConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SamplePackingListForm), DbSchema.Transaction);
        }
    }
}