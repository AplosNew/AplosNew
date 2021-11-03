using Library.Model.Enums;
using Library.Model.Products;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Products
{
    public class CompanyGroupWiseProductConfiguration : EntityTypeConfiguration<CompanyGroupWiseProduct>
    {
        public CompanyGroupWiseProductConfiguration()
        {
            Ignore(r => r.ModelState);
            HasKey(t => t.Id);
            ToTable(nameof(CompanyGroupWiseProduct), DbSchema.HKP);
        }
    }
}