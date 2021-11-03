using Library.Core;
using System;

namespace Library.Model.Payrolls
{
    public class BonusPolicyMonthlyRetainDistributionStrcPmt : BaseModel
    {
        #region Scalar Properties

        public int ID { get; set; }
        public decimal Value { get; set; }
        #endregion Scalar Properties
        
        #region Navigation Properties
        public string BnsPlyMntRetainID { get; set; }
        public string SalaryHeadID { get; set; }
        #endregion
    }
}