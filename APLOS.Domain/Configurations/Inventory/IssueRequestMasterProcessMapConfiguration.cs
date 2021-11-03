using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class IssueRequestMasterProcessMapConfiguration : EntityTypeConfiguration<IssueRequestMasterProcessMap>
    {
        public IssueRequestMasterProcessMapConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(IssueRequestMasterProcessMap), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}