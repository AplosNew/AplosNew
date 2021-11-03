using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class CompanyGroupOrderCategoryConfiguration : EntityTypeConfiguration<CompanyGroupOrderCategory>
    {
        public CompanyGroupOrderCategoryConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(CompanyGroupOrderCategory), DbSchema.HKP);
        }
    }
}