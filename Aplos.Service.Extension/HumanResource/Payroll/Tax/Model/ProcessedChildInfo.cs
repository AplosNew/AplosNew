using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Extension.Payroll.Tax.Model
{
     public class ProcessedChildInfo
    {
        public string SlrProcMstSystemID { get; set; }
        public string SalaryID { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }

        public string PayAbleShSystemID { get; set; }
        public string EntryCurrencyID { get; set; }
        public decimal EntryAmount { get; set; }

        public string DefineCurrencyID { get; set; }
        public decimal DefineAmount { get; set; }

        public string DisbusmentCurrencyID { get; set; }
        public decimal DisbusmentAmount { get; set; }

        public string AcltExcDisbSlrHDID { get; set; }
        public decimal AcltExcDisbSlrHDAmt { get; set; }
    }
}
