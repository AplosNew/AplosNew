using Library.Model.Enums;
using Library.Model.TaskManagement;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.TaskManagement
{
    public class DelayReasonConfiguration : EntityTypeConfiguration<DelayReason>
    {
        public DelayReasonConfiguration()
        {
            ToTable(nameof(DelayReason), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}