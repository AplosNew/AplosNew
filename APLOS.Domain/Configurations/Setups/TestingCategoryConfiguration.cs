#region Using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups

{
    public class TestingCategoryConfiguration : EntityTypeConfiguration<TestingCategory>
    {
        public TestingCategoryConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(TestingCategory), DbSchema.HKP);
        }
    }
}