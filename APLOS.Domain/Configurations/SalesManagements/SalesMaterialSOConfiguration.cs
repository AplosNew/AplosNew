using Library.Model.Enums;
using Library.Model.SalesManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.SalesManagements
{
    public class SalesOrderItemConfiguration : EntityTypeConfiguration<SalesOrderItem>
    {
        public SalesOrderItemConfiguration()
        {
            ToTable(nameof(SalesOrderItem), DbSchema.Transaction);
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
        }
    }
}