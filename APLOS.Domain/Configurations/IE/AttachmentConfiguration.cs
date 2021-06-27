using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class AttachmentConfiguration : EntityTypeConfiguration<Attachment>
    {
        public AttachmentConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(Attachment), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}