using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class TermsAndConditionsPOChildConfiguration : EntityTypeConfiguration<TermsAndConditionsPOChild>
    {
        public TermsAndConditionsPOChildConfiguration()
        {
            HasKey(t => t.Id);
            ToTable(nameof(TermsAndConditionsPOChild), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}