#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class PreRecruitmentDocumentConfiguration : EntityTypeConfiguration<PreRecruitmentDocument>
    {
        public PreRecruitmentDocumentConfiguration()
        {
            ToTable(nameof(PreRecruitmentDocument), DbSchema.Dbo);
            Ignore(r => r.GivenDesignationId);
            Ignore(r => r.ModelState);
        }
    }
}