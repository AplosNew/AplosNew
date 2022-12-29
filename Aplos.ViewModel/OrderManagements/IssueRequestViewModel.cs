using Library.Core;
using System;

namespace Library.ViewModel.OrderManagements
{
    public class IssueRequestViewModel : BaseModel
    {
       
        public string Id { get; set; }
        public string RequisitionNo { get; set; }
        public string RequisitionDetailId { get; set; }
        public string CostCenterId { get; set; }
        public string ExpenseActivityId { get; set; }
        public string BudgetMasterId { get; set; }
        public string GLGeneralInfoId { get; set; }
        public decimal RequestedQty { get; set; }
        public decimal RejectedQty { get; set; }

        public string Preparedby { get; set; }
        public string IssueRequestMasterId { get; set; }
        public string CheckedBy { get; set; }

        public string CheckedByStatus { get; set; }

        public string AuthorizedBy { get; set; }

        public string AuthorizedByStatus { get; set; }
		public string MaterialMasterId { get; set; }
		public string ArticleId { get; set; }
		public string FirstCharacteristicsId { get; set; }
		public string FirstCharacteristicsValueId { get; set; }
		public string SecondCharacteristicsId { get; set; }
		public string SecondCharacteristicsValueId { get; set; }

		public string ThirdCharacteristicsId { get; set; }
		public string ThirdCharacteristicsValueId { get; set; }
		public string TransactionUoMId { get; set; }

        public string IssueSlipType { get; set; }

        public string InventoryMaterialId { get; set; }

        public string CountryId { get; set; }
        public string SalesOrderId { get; set; }
        public string ProcessId { get; set; }
        public decimal RequisitionForQty { get; set; }

        public string BOQDFirstCharacteristicsValueId { get; set; }
        public string BOQDSecondCharacteristicsValueId { get; set; }
        public string BOQDThirdCharacteristicsValueId { get; set; }
        public string BOQId { get; set; }
        public decimal RequestedQtyNew { get; set; }
      

        public decimal IssueRequestBOQMapQty { get; set; }
        public decimal AllocatedIssueSlipQty { get; set; }

        public decimal OrderQty { get; set; }
        public decimal PlanOrderQty { get; set; }
        public string Destination { get; set; }
        public string PONumber { get; set; }
        public string EmployeeId { get; set; }
        public DateTime PODate { get; set; }
        public string MaterialIssueControlDetailId { get; set; }
        public int SrNo { get; set; }

    }
}