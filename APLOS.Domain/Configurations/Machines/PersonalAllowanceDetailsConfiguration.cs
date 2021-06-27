using Library.Model.Enums;
using Library.Model.Machines;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Machines
{
    public class PersonalAllowanceDetailsConfiguration : EntityTypeConfiguration<PersonalAllowanceDetails>
    {
        public PersonalAllowanceDetailsConfiguration()
        {
            ToTable(nameof(PersonalAllowanceDetails), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}