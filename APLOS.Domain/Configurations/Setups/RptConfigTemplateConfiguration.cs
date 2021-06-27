#region using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion using

namespace Library.Model.Configurations.Setups
{
    public class RptConfigTemplateConfiguration : EntityTypeConfiguration<RptConfigTemplate>
    {
        public RptConfigTemplateConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(RptConfigTemplate), DbSchema.SystemConfigurationAndSetup);
        }
    }
}