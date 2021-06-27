using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.Tax
{
  public class ProfessionalTaxEmployeeWiseMonthly
    {
        public string EmpSystemId { get; set; }
        public decimal EarnedAmount { get; set; }
        public decimal StructureAmount { get; set; }
    }
}
