using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class IssueRequestMasterSalesOrderMapConfiguration : EntityTypeConfiguration<IssueRequestMasterSalesOrderMap>
    {
        public IssueRequestMasterSalesOrderMapConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(IssueRequestMasterSalesOrderMap), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}