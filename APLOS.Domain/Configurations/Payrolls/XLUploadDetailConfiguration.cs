using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Payrolls
{
    public class XLUploadDetailConfiguration : EntityTypeConfiguration<XLUploadDetail>
    {
        public XLUploadDetailConfiguration()
        {
            ToTable(nameof(XLUploadDetail), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}