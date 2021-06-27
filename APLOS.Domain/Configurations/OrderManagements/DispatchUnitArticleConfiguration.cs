using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class DispatchUnitArticleConfiguration : EntityTypeConfiguration<DispatchUnitArticle>
    {
        public DispatchUnitArticleConfiguration()
        {
            Ignore(t => t.ModelState);
            Ignore(t => t.MaterialMasterName);
            Ignore(t => t.ArticleName);
            Property(t => t.Qty).HasPrecision(18, 10);
            ToTable(nameof(DispatchUnitArticle), DbSchema.Transaction);
        }
    }
}