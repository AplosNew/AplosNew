#region Using

using Library.Model.Enums;
using Library.Model.External;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.External
{
    public class AplosEmpFieldTagConfiguration : EntityTypeConfiguration<AplosEmpFieldTag>
    {
        public AplosEmpFieldTagConfiguration()
        {
            ToTable(nameof(AplosEmpFieldTag), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}