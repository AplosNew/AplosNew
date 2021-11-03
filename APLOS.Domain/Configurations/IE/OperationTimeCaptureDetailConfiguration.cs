using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class OperationTimeCaptureDetailConfiguration : EntityTypeConfiguration<OperationTimeCaptureDetail>
    {
        public OperationTimeCaptureDetailConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(OperationTimeCaptureDetail), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}