using Library.Model.Enums;
using Library.Model.HumanResources;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.HumanResources
{
    public class MaternityLeavePolicyConfiguration : EntityTypeConfiguration<MaternityLeavePolicy>
    {
        public MaternityLeavePolicyConfiguration()
        {
            ToTable(nameof(MaternityLeavePolicy), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}