using Library.Model.Commercial;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Costings.IssueTracker
{
    public class PurchaseLCChargesConfiguration : EntityTypeConfiguration<PurchaseLCCharges>
    {
        public PurchaseLCChargesConfiguration()
        {
            ToTable(nameof(PurchaseLCCharges), DbSchema.Dbo);
            Ignore(r => r.ModelState);
            Ignore(t => t.LCDate);
        }
    }
}