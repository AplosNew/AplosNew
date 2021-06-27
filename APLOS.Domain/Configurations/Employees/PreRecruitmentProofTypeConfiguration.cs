#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class PreRecruitmentProofTypeConfiguration : EntityTypeConfiguration<PreRecruitmentProofType>
    {
        public PreRecruitmentProofTypeConfiguration()
        {
            ToTable(nameof(PreRecruitmentProofType), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}