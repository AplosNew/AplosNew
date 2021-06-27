#region using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Setups
{
    public class TestingStandardBuyerConfiguration : EntityTypeConfiguration<TestingStandardBuyer>
    {
        public TestingStandardBuyerConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(TestingStandardBuyer), DbSchema.SystemConfigurationAndSetup);
        }
    }
}