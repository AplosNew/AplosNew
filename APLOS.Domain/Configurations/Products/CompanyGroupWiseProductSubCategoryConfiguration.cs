using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class CompanyGroupWiseProductSubCategoryConfiguration : EntityTypeConfiguration<CompanyGroupWiseProductSubCategory>
    {
        public CompanyGroupWiseProductSubCategoryConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(CompanyGroupWiseProductSubCategory), DbSchema.HKP);
        }
    }
}