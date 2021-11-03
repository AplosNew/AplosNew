using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Machines
{
    public class PersonalAllowanceConfiguration : EntityTypeConfiguration<PersonalAllowance>
    {
        public PersonalAllowanceConfiguration()
        {
            ToTable(nameof(PersonalAllowance), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}