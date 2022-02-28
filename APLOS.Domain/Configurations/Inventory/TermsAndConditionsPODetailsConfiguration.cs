using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class TermsAndConditionsPODetailsConfiguration : EntityTypeConfiguration<TermsAndConditionsPODetails>
    {
        public TermsAndConditionsPODetailsConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(TermsAndConditionsPODetails), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}