using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class SubsectionStructureDetailConfiguration : EntityTypeConfiguration<SubsectionStructureDetail>
    {
        public SubsectionStructureDetailConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(SubsectionStructureDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}