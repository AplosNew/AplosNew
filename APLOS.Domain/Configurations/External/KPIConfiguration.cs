#region Using

using Library.Model.Enums;
using Library.Model.External;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.External
{
    public class KPIConfiguration : EntityTypeConfiguration<KPI>
    {
        public KPIConfiguration()
        {
            ToTable(nameof(KPI), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}