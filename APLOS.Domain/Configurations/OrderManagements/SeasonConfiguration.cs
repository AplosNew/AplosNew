using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class SeasonConfiguration : EntityTypeConfiguration<Seasons>
    {
        public SeasonConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable("Season", DbSchema.HKP);
        }
    }
}