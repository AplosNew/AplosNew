using Library.Core;
using System;

namespace Library.Model.Products
{
    public class GatePassMaster : BaseModel
    {
        #region Scalar Properties
        public string Id	{ get; set; }
        public string CompanyGroupId { get; set; }
        //public string CompanyId  { get; set; }
        public string PlantId  { get; set; }
        public string GatePassType  { get; set; }
        public string GatePassStatus  { get; set; }
        public DateTime? ReturnableDate  { get; set; }
        public DateTime? GatePassEntryDate  { get; set; }
        public string FromEmployeeId  { get; set; }
        public string Through { get; set; }
        public string CourierName { get; set; }
        public string RunnerEmployeeId  { get; set; }
        public string ToType  { get; set; }
        public string ToPartyCode  { get; set; }
        public string ToBuyerId  { get; set; }
        public string ToPlantId  { get; set; }
        public string ToUnitId  { get; set; }
        public string ToDivisionId  { get; set; }
        public string ToDepartment  { get; set; }
        public string DepartmentEmployeeId  { get; set; }
        public string OtherCompanyName  { get; set; }
        public string PersonName  { get; set; }
        public string MobileNo  { get; set; }
        public string Address  { get; set; }
        public string Remarks  { get; set; }
        public string CheckedBy  { get; set; }
        public string CheckedByStatus  { get; set; }
	    public string CheckedHoldRejectReason  { get; set; }
	    public string ApprovedBy  { get; set; }
	    public string ApprovedByStatus  { get; set; }
	    public string ApprovedHoldRejectReason  { get; set; }
	    public string SenderSecurityEmployeeId  { get; set; } 
	    public string SenderSecurityApprovedStatus  { get; set; }
	    public string ReceiverSecurityEmployeeId  { get; set; }
 
	     public string ReceiverSecurityApprovedStatus  { get; set; }
	    public string VendorBuyerOtherCompanyReceivedStatus  { get; set; }
        public string ChallanNo { get; set; }

        public string TransportAgentMobileNo { get; set; }
        public string TransportAgentName { get; set; }
        public string VehicleNo { get; set; }
      
        public string GatePassStatus1 { get; set; }
        public string GateRegisterType { get; set; }
        public string ReceivedChallanNO { get; set; }

        public string InvoiceNo { get; set; }
        public decimal InvoiceValue { get; set; }

        public string PurposeofGatePass { get; set; }
        public string ConsignmentNo { get; set; } 
        public string DriverName { get; set; }
        public string NoofPackages { get; set; }  
        public string GateEntryNo { get; set; }
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