#region

using Library.Model.Enums;
using Library.Model.IE;
using Library.Model.Machines;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

#endregion

namespace Library.Model.Configurations.Machines
{
    public class MachineMasterConfiguration : EntityTypeConfiguration<MachineMaster>
    {
        public MachineMasterConfiguration()
        {
            HasKey(t => t.Id);
            Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            ToTable("MachineMasters", DbSchema.Masters);
            Property(t => t.Id)
                .HasColumnName("Id");
            Ignore(r => r.ModelState); 
        }
    }
}