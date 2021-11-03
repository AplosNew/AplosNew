using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class SalesOrderMasterConfiguration : EntityTypeConfiguration<SalesOrderMaster>
    {
        public SalesOrderMasterConfiguration()
        {
            Ignore(r => r.ModelState);
            Property(r => r.Rate).HasPrecision(18, 4);
            ToTable("SalesOrder", DbSchema.Transaction);
        }
    }
}