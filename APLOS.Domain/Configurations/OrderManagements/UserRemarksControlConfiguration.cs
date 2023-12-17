using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class UserRemarksControlConfiguration : EntityTypeConfiguration<UserRemarksControl>
    {
        public UserRemarksControlConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(UserRemarksControl), DbSchema.Transaction);
        }
    }
}