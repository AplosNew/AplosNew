using Library.Core;
using Library.Model.OpeningBalances;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class MaterialBudget : BaseModel
    {




        #region


        public string Id { get; set; }
        public string CompanyGroupId { get; set; }        
        public string EntityId { get; set; }
        public string EmployeeId{ get; set; }
        public string ActivityId { get; set; }
        public string BudgetMasterId { get; set; }
        public string GLGeneralInfoId{ get; set; }
        
        public string MaterialMasterId { get; set; }
        public string  ArticleId { get; set; }
        public string  FirstCharacteristicsId { get; set; }
        public string  FirstCharacteristicsValueId { get; set; }
        public string SecondCharacteristicsId{ get; set; }
        public string SecondCharacteristicsValueId { get; set; }
        public string  ThirdCharacteristicsId { get; set; }
        public string ThirdCharacteristicsValueId { get; set; }
        public string BudgetPeriod{ get; set; }
        public decimal BudgetQty{ get; set; }
        public string ProcurementFrequencyCategory { get; set; }
        public decimal ProcurementsPlanDay{ get; set; }
        public string Remarks{ get; set; }

       

      

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

       

       
       

      
        

       
        


          

        
  
    }
}