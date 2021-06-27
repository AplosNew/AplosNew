using Library.Model.Enums;
using Library.Model.Productions;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Productions
{
    public class AuthorizationConfigConfiguration : EntityTypeConfiguration<AuthorizationConfig>
    {
        public AuthorizationConfigConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);

            // Table & Column Configuration
            ToTable(nameof(AuthorizationConfig), DbSchema.Dbo);
        }
    }
}