using Library.Model.Enums;
using Library.Model.OrderManagements;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.OrderManagements
{
    public class MasterOrderAtributeValueConfiguration : EntityTypeConfiguration<MasterOrderAttributeValue>
    {
        public MasterOrderAtributeValueConfiguration()
        {
            Ignore(r => r.ModelState);
            ToTable(nameof(MasterOrderAttributeValue), DbSchema.Transaction);
        }
    }
}