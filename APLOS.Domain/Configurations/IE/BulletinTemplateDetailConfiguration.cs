using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class BulletinTemplateDetailConfiguration : EntityTypeConfiguration<BulletinTemplateDetail>
    {
        public BulletinTemplateDetailConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Property(t => t.AdditionalSPT).HasPrecision(18, 4);
            Property(t => t.TotalSPT).HasPrecision(18, 4);
            Property(t => t.AvgAllotedTime).HasPrecision(18, 4);
            Property(t => t.AllotedWorkstation).HasPrecision(18, 4);
            Property(t => t.AllotedManpower).HasPrecision(18, 4);
            Property(t => t.OperationTargetPerHr).HasPrecision(18, 4);
            Property(t => t.RequiredManPower).HasPrecision(18, 4);
            Property(t => t.OperationLength).HasPrecision(18, 2);
            Property(t => t.FabricWidth).HasPrecision(18, 4);
            Property(t => t.NeedleConsumption).HasPrecision(18, 4);
            Property(t => t.BobbinConsumption).HasPrecision(18, 4);
            Property(t => t.LooperConsumption).HasPrecision(18, 4);
            Property(t => t.SPIConsumption).HasPrecision(18, 4);
            Property(t => t.Consumption).HasPrecision(18, 4);
            // Table & Column Configuration
            ToTable(nameof(BulletinTemplateDetail), DbSchema.Masters);
            Ignore(r => r.ModelState);
            Ignore(r => r.OperationCode);
        }
    }
}