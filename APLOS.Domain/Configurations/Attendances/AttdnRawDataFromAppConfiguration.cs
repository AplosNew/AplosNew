using Library.Model.Attendances;
using Library.Model.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Attendances
{
    public class AttdnRawDataFromAppConfiguration : EntityTypeConfiguration<AttdnRawDataFromApp>
    {
        public AttdnRawDataFromAppConfiguration()
        {
            ToTable(nameof(AttdnRawDataFromApp), DbSchema.Dbo);
           // Property(t => t.RowId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Ignore(r => r.ModelState);
            Ignore(r => r.InTimeUI);
            Ignore(r => r.OutTimeUI);
        }
    }
}