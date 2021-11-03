using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class LSDConfiguration : EntityTypeConfiguration<LSD>
    {
        public LSDConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(LSD), DbSchema.Masters);
        }
    }
}