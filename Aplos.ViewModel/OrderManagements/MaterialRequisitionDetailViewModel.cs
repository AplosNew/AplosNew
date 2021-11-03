using Library.Core;
using System;

namespace Library.ViewModel.OrderManagements
{
    public class MaterialRequisitionDetailViewModel : BaseModel
    {
       
        public string Id { get; set; }
        public string CompanyGroupId { get; set; }
        public string MaterialReqqusitionMasterId { get; set; }
        public string ActivityId { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string FirstCharacteristicsId { get; set; }
        public string FirstCharacteristicsValueId { get; set; }
        public string SecondCharacteristicsId { get; set; }
        public string SecondCharacteristicsValueId { get; set; }
        public string ThirdCharacteristicsId { get; set; }
        public string ThirdCharacteristicsValueId { get; set; }
        public string MaterialDetail { get; set; }
        public string TransactionUoMId { get; set; }
        public string CurrencyId { get; set; }
        public Decimal? TransactionQty { get; set; }
        public Decimal? EstimatedRate { get; set; }
        public Decimal? TotalAmount { get; set; }
        public string BudgetType { get; set; }
        public string Reason { get; set; }
        public string Remarks { get; set; }
        public string QualityApprovalResponsiblePersonId { get; set; }
        public string NeedSpecialAppId { get; set; }
        public string FutureReqApp { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string BudgetMasterId { get; set; }
        public string GLGeneralInfoId { get; set; }
         
        public string LocalImported { get; set; }
        
        public DateTime? CommitmentDate { get; set; }
        public Decimal? PORcvQty { get; set; }
        public string POQtyStatus { get; set; }
		public decimal OrginalQty { get; set; }
		public decimal ApprovedQty { get; set; }
      
         
    }
}