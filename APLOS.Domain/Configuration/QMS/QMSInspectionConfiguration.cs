using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Library.Model.QMS;
using Library.Model.Enums;
using System.Data.Entity.ModelConfiguration;

namespace Library.Model.Configurations.QMS
{
    
    public class QMSInspectionConfiguration : EntityTypeConfiguration<QMSInspection>
    {
        public QMSInspectionConfiguration()
        {
            ToTable(nameof(QMSInspection), DbSchema.Dbo);
            HasKey(t => t.Id);
            Ignore(r => r.ModelState);
        }
    }
}
