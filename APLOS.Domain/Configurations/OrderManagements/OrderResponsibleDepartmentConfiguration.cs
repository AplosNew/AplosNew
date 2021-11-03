using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class OrderResponsibleDepartmentConfiguration : EntityTypeConfiguration<OrderResponsibleDepartment>
    {
        public OrderResponsibleDepartmentConfiguration()
        {
            Property(t=>t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Ignore(r => r.ModelState);
            ToTable(nameof(OrderResponsibleDepartment), DbSchema.Masters);
        }
    }
}