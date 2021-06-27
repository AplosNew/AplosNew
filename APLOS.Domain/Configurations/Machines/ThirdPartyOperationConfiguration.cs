#region using

using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Machines
{
    public class ThirdPartyOperationConfiguration : EntityTypeConfiguration<ThirdPartyOperation>
    {
        public ThirdPartyOperationConfiguration()
        {
            ToTable(nameof(ThirdPartyOperation), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}