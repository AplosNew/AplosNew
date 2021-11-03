#region Using

using Library.Model.Enums;
using Library.Model.External;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.External
{
    public class AplosEmpFieldConfiguration : EntityTypeConfiguration<AplosEmpField>
    {
        public AplosEmpFieldConfiguration()
        {
            ToTable(nameof(AplosEmpField), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}