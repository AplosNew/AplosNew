using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class BulletinCalculationConfiguration : EntityTypeConfiguration<BulletinCalculation>
    {
        public BulletinCalculationConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(BulletinCalculation), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}