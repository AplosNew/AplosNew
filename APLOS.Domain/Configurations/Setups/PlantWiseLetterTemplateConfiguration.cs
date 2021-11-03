#region Using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups
{
    public class PlantWiseLetterTemplateConfiguration : EntityTypeConfiguration<PlantWiseLetterTemplate>
    {
        public PlantWiseLetterTemplateConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(PlantWiseLetterTemplate), DbSchema.SystemConfigurationAndSetup);
        }
    }
}