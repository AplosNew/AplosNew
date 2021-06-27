using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class CustomerPOConfiguration : EntityTypeConfiguration<CustomerPO>
    {
        public CustomerPOConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);

            // Table & Column Configuration
            ToTable(nameof(CustomerPO), DbSchema.Transaction);
        }
    }
}