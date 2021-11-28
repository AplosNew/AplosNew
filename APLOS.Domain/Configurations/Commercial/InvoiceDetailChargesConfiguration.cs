using Library.Model.Commercial;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Commercial
{
    public class InvoiceDetailChargesConfiguration : EntityTypeConfiguration<InvoiceDetailCharges>
    {
        public InvoiceDetailChargesConfiguration()
        {
            ToTable(nameof(InvoiceDetailCharges), DbSchema.Transaction);
            Ignore(r => r.ModelState);
            Ignore(r => r.GLGeneralInfoId);
            Ignore(r => r.BudgetMasterId);
            Ignore(r => r.ActivityId);
        }
    }
}