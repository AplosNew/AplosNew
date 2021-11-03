using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class SizeGroupConfiguration : EntityTypeConfiguration<SizeGroup>
    {
        public SizeGroupConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(SizeGroup), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}