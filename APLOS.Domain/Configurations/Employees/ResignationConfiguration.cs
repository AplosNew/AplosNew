using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Employees
{
    public class ResignationConfiguration : EntityTypeConfiguration<Resignation>
    {
        public ResignationConfiguration()
        {
            ToTable(nameof(Resignation), DbSchema.Transaction);
            Ignore(r => r.ModelState);
        }
    }
}