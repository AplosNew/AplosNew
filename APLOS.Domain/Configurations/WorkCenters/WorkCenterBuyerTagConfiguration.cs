using Library.Model.Enums;
using Library.Model.WorkCenters;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.WorkCenters
{
    public class WorkCenterBuyerTagConfiguration : EntityTypeConfiguration<WorkCenterBuyerTag>
    {
        public WorkCenterBuyerTagConfiguration()
        {
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
            ToTable("WorkCenterBuyerTag", DbSchema.HKP);
        }
    }
}