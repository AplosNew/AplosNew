using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class SkillGroupingConfiguration : EntityTypeConfiguration<SkillGrouping>
    {
        public SkillGroupingConfiguration() 
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(SkillGrouping), "SCS");
            Ignore(r => r.ModelState);
        }
    }
}