using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class GaugeFolderConfiguration : EntityTypeConfiguration<GaugeFolder>
    {
        public GaugeFolderConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(GaugeFolder), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}