using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class MachineMasterUIConfiguration : EntityTypeConfiguration<MachineMasterUI>
    {
        public MachineMasterUIConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            //here use TableName
            ToTable("MachineMaster", DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}
