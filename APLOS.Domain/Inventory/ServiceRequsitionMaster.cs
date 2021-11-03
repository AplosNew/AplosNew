using Library.Core;
using Library.Model.OpeningBalances;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class ServiceRequsitionMaster : BaseModel
    {

        #region Scalar Properties
        public string Id { get; set; }
        public string CompanyGroupId { get; set; }
		public string CompanyId { get; set; }

		public string PlantId { get; set; } 


		
		public string EntityId { get; set; }

        public string RequisitionType { get; set; }

        public string RequirmentType { get; set; }

        public string Remarks { get; set; }

        public string ReasonWhyItIsNotPlanEarlier { get; set; }
       // public string ReasonWhy { get; set; } 
        public DateTime? RequisitionDate { get; set; }
        public string QualityApprovalResponsiblePersonId { get; set; }
       // public string QualityApproval { get; set; }  

        public string NeedSpecialAppId { get; set; }
        public string CheckedBy { get; set; }
        public string CheckedByStatus { get; set; }
        public string AuthorizedBy { get; set; }
        public string AuthorizedByStatus { get; set; }
        public string IsApproved { get; set; }
        public string RequisitionStatus { get; set; }
		public string ReqEmpId { get; set; }

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