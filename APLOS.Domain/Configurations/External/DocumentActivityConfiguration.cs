#region Using

using Library.Model.Enums;
using Library.Model.External;
using System.Data.Entity.ModelConfiguration;

#endregion Using

namespace Library.Model.Configurations.External
{
    public class DocumentActivityConfiguration : EntityTypeConfiguration<DocumentActivity>
    {
        public DocumentActivityConfiguration()
        {
            ToTable(nameof(DocumentActivity), DbSchema.Dbo);
            Ignore(r => r.ModelState);
        }
    }
}