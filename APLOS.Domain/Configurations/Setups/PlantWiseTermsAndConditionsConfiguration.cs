#region Using

using Library.Model.Enums;
using Library.Model.Setups;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Setups
{
	public class PlantWiseTermsAndConditionsConfiguration : EntityTypeConfiguration<PlantWiseTermsAndConditions>
	{
		public PlantWiseTermsAndConditionsConfiguration()
		{
			Ignore(r => r.ModelState);
			ToTable(nameof(PlantWiseTermsAndConditions), DbSchema.SystemConfigurationAndSetup);
		}
	}
}