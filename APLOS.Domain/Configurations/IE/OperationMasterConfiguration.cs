using Library.Model.Enums;
using Library.Model.IE;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.IE
{
    public class OperationMasterConfiguration : EntityTypeConfiguration<OperationMaster>
    {
        public OperationMasterConfiguration()
        {
            // Primary Key
            HasKey(t => t.Id);
            // Table & Column Configuration
            ToTable(nameof(OperationMaster), DbSchema.Masters);
            Ignore(r => r.ModelState);
        }
    }
}