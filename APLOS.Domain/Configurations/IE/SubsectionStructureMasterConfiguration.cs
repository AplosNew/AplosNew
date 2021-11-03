using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class SubsectionStructureMasterConfiguration : EntityTypeConfiguration<SubsectionStructureMaster>
    {
        public SubsectionStructureMasterConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(SubsectionStructureMaster), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}