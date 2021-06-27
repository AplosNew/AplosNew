using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class OperationVideoUploadConfiguration : EntityTypeConfiguration<OperationVideoUpload>
    {
        public OperationVideoUploadConfiguration()
        {
            ToTable(nameof(OperationVideoUpload), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}