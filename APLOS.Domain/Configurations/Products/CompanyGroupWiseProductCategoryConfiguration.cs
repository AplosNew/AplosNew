using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class CompanyGroupWiseProductCategoryConfiguration : EntityTypeConfiguration<CompanyGroupWiseProductCategory>
    {
        public CompanyGroupWiseProductCategoryConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(CompanyGroupWiseProductCategory), DbSchema.HKP);
        }
    }
}