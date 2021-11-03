using Library.Core;

//using Library.Core.Models;

namespace Library.ViewModel.HR
{
    public class SalaryFixationVM : BaseModel
    {
        public bool IsAnnualNonCash { get; set; }
        public bool IsLeave { get; set; }
        public bool IsOpen { get; set; }
        public string SalaryHead { get; set; }

        public string SalaryHeadId { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal FixationAmount { get; set; }

        public bool IsAnnualCash { get; set; }
        public bool IsMonthly { get; set; }
        public bool IsCalculated { get; set; }
        public string Id { get; set; }  //
        public string SalaryRuleId { get; set; }  //_SalaryRuleMasterSystemID

        public bool CurrentStatus { get; set; }
        public bool ExpectedStatus { get; set; }
        public bool FixationStatus { get; set; }

        public bool CurrentStatusL { get; set; }
        public bool ExpectedStatusL { get; set; }
        public bool FixationStatusL { get; set; }

        public bool CurrentStatusN { get; set; }
        public bool ExpectedStatusN { get; set; }
        public bool FixationStatusN { get; set; }
    }
}