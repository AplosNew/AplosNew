using Library.Model.Attendances;
using Library.Model.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Attendances
{
    public class AttdnDataDownLoadLogConfiguration : EntityTypeConfiguration<AttdnDataDownLoadLog>
    {
        public AttdnDataDownLoadLogConfiguration()
        {
            ToTable(nameof(AttdnDataDownLoadLog), DbSchema.Dbo);
            Property(t => t.RowId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Ignore(r => r.ModelState);
        }
    }
}