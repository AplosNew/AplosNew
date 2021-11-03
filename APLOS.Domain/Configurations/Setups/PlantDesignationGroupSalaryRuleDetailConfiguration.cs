#region Using

using Library.Model.Enums;
using Library.Model.Payrolls;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups
{
    public class PlantDesignationGroupSalaryRuleDetailConfiguration : EntityTypeConfiguration<PlantDesignationGroupSalaryRuleDetail>
    {
        public PlantDesignationGroupSalaryRuleDetailConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(PlantDesignationGroupSalaryRuleDetail), DbSchema.Organizations);
        }
    }
}