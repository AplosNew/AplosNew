using Library.Model.Attendances;
using Library.Model.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Attendances
{
    public class AttdnRawDataConfiguration : EntityTypeConfiguration<AttdnRawData>
    {
        public AttdnRawDataConfiguration()
        {
            ToTable(nameof(AttdnRawData), DbSchema.Dbo);
            Property(t => t.RowId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Ignore(r => r.ModelState);
        }
    }
}