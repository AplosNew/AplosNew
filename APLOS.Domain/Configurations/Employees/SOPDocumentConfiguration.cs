#region Using

using Library.Model.Employees;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.Employees
{
    public class SOPDocumentConfiguration : EntityTypeConfiguration<SOPDocument>
    {
        public SOPDocumentConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(SOPDocument), DbSchema.HKP);
        }
    }
}