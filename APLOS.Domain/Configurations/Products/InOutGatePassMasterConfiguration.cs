using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class InOutGatePassMasterConfiguration : EntityTypeConfiguration<InOutGatePassMaster>
    {
        public InOutGatePassMasterConfiguration()
        {
            Ignore(r => r.ChallanItemTypeId);
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(InOutGatePassMaster), DbSchema.Transaction);
        }
    }
}

