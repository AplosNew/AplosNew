#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class SOPAttachmentDetailConfiguration : EntityTypeConfiguration<SOPAttachmentDetail>
    {
        public SOPAttachmentDetailConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SOPAttachmentDetail), DbSchema.HKP);
        }
    }
}