using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class ButtonRecipeConfigConfiguration : EntityTypeConfiguration<ButtonRecipeConfig>
    {
        public ButtonRecipeConfigConfiguration()
        {
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            ToTable(nameof(ButtonRecipeConfig), DbSchema.SystemConfigurationAndSetup);
        }
    }
}