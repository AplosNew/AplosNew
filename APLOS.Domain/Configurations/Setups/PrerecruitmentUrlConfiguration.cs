#region Using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups
{
    public class PrerecruitmentUrlConfiguration : EntityTypeConfiguration<PrerecruitmentUrl>
    {
        public PrerecruitmentUrlConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(PrerecruitmentUrl), DbSchema.ModuleAndMenuSetup);
        }
    }
}