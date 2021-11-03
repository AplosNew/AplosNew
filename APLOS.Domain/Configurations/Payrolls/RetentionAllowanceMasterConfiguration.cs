using Library.Model.Enums;
using Library.Model.Payrolls;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Payrolls
{
    public class RetentionAllowanceMasterConfiguration : EntityTypeConfiguration<RetentionAllowanceMaster>
    {
        public RetentionAllowanceMasterConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(RetentionAllowanceMaster), DbSchema.Masters);
        }
    }
}