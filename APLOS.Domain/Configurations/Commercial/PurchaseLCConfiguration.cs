using Library.Model.Commercial;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Costings.IssueTracker
{
    public class PurchaseLCConfiguration : EntityTypeConfiguration<PurchaseLC>
    {
        public PurchaseLCConfiguration()
        {
            ToTable(nameof(PurchaseLC), DbSchema.Dbo);
            Ignore(r => r.ModelState);
            Ignore(r => r.flag);
        }
    }
}