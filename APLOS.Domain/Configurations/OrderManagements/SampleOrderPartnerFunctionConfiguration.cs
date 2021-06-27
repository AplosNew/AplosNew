using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class SampleOrderPartnerFunctionConfiguration : EntityTypeConfiguration<SampleOrderPartnerFunction>
    {
        public SampleOrderPartnerFunctionConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SampleOrderPartnerFunction), DbSchema.Transaction);
        }
    }
}