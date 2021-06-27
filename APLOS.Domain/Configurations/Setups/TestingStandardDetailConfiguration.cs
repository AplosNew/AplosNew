#region using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Setups
{
    public class TestingStandardConfiguration : EntityTypeConfiguration<TestingStandard>
    {
        public TestingStandardConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(TestingStandard), DbSchema.SystemConfigurationAndSetup);
        }
    }
}