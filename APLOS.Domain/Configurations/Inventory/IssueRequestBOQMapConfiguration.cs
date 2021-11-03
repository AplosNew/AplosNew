using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class IssueRequestBOQMapConfiguration : EntityTypeConfiguration<IssueRequestBOQMap>
    {
        public IssueRequestBOQMapConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(IssueRequestBOQMap), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}