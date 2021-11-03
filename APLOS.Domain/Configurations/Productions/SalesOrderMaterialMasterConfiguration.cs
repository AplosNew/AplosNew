using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class SalesOrderMaterialMasterConfiguration : EntityTypeConfiguration<SalesOrderMaterialMaster>
    {
        public SalesOrderMaterialMasterConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            Ignore(t => t.Characteristics1Id);
            Ignore(t => t.CharacteristicsValue1Id);
            Ignore(t => t.Characteristics2Id);
            Ignore(t => t.CharacteristicsValue2Id);
            ToTable(nameof(SalesOrderMaterialMaster), DbSchema.Transaction);
        }
    }
}