using Library.Model.Attendances;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Attendances
{
    public class AttdnProcessDataConfiguration : EntityTypeConfiguration<AttdnProcessData>
    {
        public AttdnProcessDataConfiguration()
        {
            ToTable(nameof(AttdnProcessData), DbSchema.Dbo);
            //Property(t =>  t.EmpSystemID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasKey(t => new { t.EmpSystemID, t.WorkDate });
            Ignore(r => r.ModelState);
        }
    }
}