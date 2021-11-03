#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class InterviewRankingConfiguration : EntityTypeConfiguration<InterviewRanking>
    {
        public InterviewRankingConfiguration()
        {
            ToTable(nameof(InterviewRanking), DbSchema.HKP);
            Ignore(r => r.ModelState);
        }
    }
}