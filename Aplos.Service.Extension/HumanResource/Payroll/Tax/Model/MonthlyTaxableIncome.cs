using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Extension.Payroll.Tax.Model
{
   public class MonthlyTaxableIncome
    {
        public decimal TaxableAmount { get; set; }
        public decimal TaxableAmountStr { get; set; }
        public string EmpInfoSystemID { get; set; }
        public string TaxPolicyMstID { get; set; }
    }

    public class OBPTax
    {
        public decimal OpeningTaxPaid { get; set; }
        public decimal OpeningTaxableIncomeEarned { get; set; }
        public string EmpSystemId { get; set; }
    }
}

