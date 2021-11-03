using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class OperationTimeCaptureMasterConfiguration : EntityTypeConfiguration<OperationTimeCaptureMaster>
    {
        public OperationTimeCaptureMasterConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(OperationTimeCaptureMaster), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}