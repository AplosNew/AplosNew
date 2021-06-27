#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class PFEmployeeVoluntaryValueConfiguration : EntityTypeConfiguration<PFEmployeeVoluntaryValue>
    {
        public PFEmployeeVoluntaryValueConfiguration()
        {
            ToTable(nameof(PFEmployeeVoluntaryValue), DbSchema.Dbo);
            Ignore(r => r.ModelState);
            Ignore(r => r.IsVoluntaryPF);
        }
    }
}