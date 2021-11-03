#region Using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups
{
    public class TestingConfiguration : EntityTypeConfiguration<Testing>
    {
        public TestingConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(Testing), DbSchema.SystemConfigurationAndSetup);
        }
    }
}