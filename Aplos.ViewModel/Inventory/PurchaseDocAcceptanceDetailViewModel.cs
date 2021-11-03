using Library.Core;
using System;

namespace Library.ViewModel.Inventory
{
    public class PurchaseDocAcceptanceDetailViewModel : BaseModel 
    {

		public string Id { get; set; }
		public string PurchaseDocAcceptanceId { get; set; }
		public string MaterialMasterId { get; set; }
		public string ArticleId { get; set; }
		public string FirstCharacteristicsId { get; set; }
		public string FirstCharacteristicsValueId { get; set; }
		public string SecondCharacteristicsId { get; set; }
		public string SecondCharacteristicsValueId { get; set; }
		public string ThirdCharacteristicsId { get; set; }
		public string ThirdCharacteristicsValueId { get; set; }
		public decimal TransactionQty { get; set; }
		public string TransactionUoMId { get; set; }
		public decimal TransactionRate { get; set; }
		public decimal TrnAmount { get; set; }
        public decimal TaxAmount { get; set; }

        public string AddedBy { get; set; }		
		public string AddedDate { get; set; }
		public string AddedFromIP { get; set; }
		public string UpdatedBy { get; set; }
		public string UpdatedDate { get; set; }
		public string UpdatedFromIP { get; set; }

		public string POId { get; set; }
		public string PODetailsID { get; set; }
		public decimal AcceptanceRcvQty { get; set; }

		public bool AcceptanceRcvStatusQty { get; set; }
		public string TrnType { get; set; }
		public string MaterialGroupMasterName { get; set; }
		public string AcceptenceDetailId { get; set; }
		public string GLGeneralInfoId { get; set; }
		public string BudgetMasterId { get; set; }
		public string ActivityId { get; set; }
		public string ClearingAccountGLId { get; set; }
		public string ClearingAccountBudgetMasterId { get; set; }
		public string ClearingAccountActivityId { get; set; }
        public decimal TotalMaterialTranAmount { get; set; }
        public decimal ChargesTranAmount { get; set; }
        public decimal ChargesTaxTranAmount { get; set; }
        public string InventoryReceiveId { get; set; }
        public string InventoryReceiveDetailId { get; set; }
		public decimal AcceptanceRate { get; set; }

		public string ServicePOMasterId { get; set; }
		public string ServicePODetailId { get; set; }
	}
}