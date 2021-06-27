using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class SamplePackingListMaterialDetailsConfiguration : EntityTypeConfiguration<SamplePackingListMaterialDetails>
    {
        public SamplePackingListMaterialDetailsConfiguration()
        {
            ToTable(nameof(SamplePackingListMaterialDetails), DbSchema.Transaction);
            Ignore(r => r.ModelState);
            Ignore(t => t.OrderUoMId);
            Ignore(t => t.OrderQty);
            Ignore(t => t.PendingQty);
        }
    }
}