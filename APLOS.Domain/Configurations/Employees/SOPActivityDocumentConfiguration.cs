#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class SOPActivityDocumentConfiguration : EntityTypeConfiguration<SOPActivityDocument>
    {
        public SOPActivityDocumentConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SOPActivityDocument), DbSchema.HKP);
        }
    }
}