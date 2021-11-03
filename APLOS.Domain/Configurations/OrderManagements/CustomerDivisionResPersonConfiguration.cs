using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class CustomerDivisionResPersonConfiguration : EntityTypeConfiguration<CustomerDivisionResPerson>
    {
        public CustomerDivisionResPersonConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(CustomerDivisionResPerson), DbSchema.Masters);
        }
    }
}