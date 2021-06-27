#region using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Setups
{
    public class TestingStandardDetailConfiguration : EntityTypeConfiguration<TestingStandardDetail>
    {
        public TestingStandardDetailConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(TestingStandardDetail), DbSchema.SystemConfigurationAndSetup);
        }
    }
}