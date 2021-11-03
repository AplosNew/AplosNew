using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class GatePassMasterConfiguration : EntityTypeConfiguration<GatePassMaster>
    {
        public GatePassMasterConfiguration()
        {
            Ignore(r => r.ModelState);
            Ignore(r => r.GatePassStatus1);
            HasKey(t => t.Id);           
            ToTable(nameof(GatePassMaster), DbSchema.Transaction);
           
        }
    }
}

