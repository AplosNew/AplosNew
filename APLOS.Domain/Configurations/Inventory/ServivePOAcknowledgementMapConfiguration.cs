using Library.Model.Enums;
using Library.Model.Inventory;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.Inventory
{
    public class ServivePOAcknowledgementMapConfiguration : EntityTypeConfiguration<ServivePOAcknowledgementMap>
    {
        public ServivePOAcknowledgementMapConfiguration()
        {
            HasKey(t => t.Id);
            //Property(t => t.Amount).HasPrecision(18, 10);
            //Property(t => t.TotalTaxAmount).HasPrecision(18, 10);
            ToTable(nameof(ServivePOAcknowledgementMap), DbSchema.Transaction); 
            Ignore(r => r.ModelState);
        }
    }
}