using Library.Core;
using System;
using System.Xml.Serialization;

namespace Library.ViewModel.Inventory
{
    public class ServiceAcknowledgementDetailViewModel : BaseModel   
    {

        public string Id { get; set; }
        public string ServiceAcknowledgementDetailId { get; set; }

        public decimal Amount { get; set; }
        public decimal TotalTaxAmount { get; set; }

        public decimal TotalAmount { get; set; }


        #region Navigation Properties
        public string ServiceAcknowledgementMasterId { get; set; }
        public string ServiceMasterId { get; set; }
        public string ServicePOMasterId { get; set; }
        public string ServicePODetailId { get; set; }
        public string PostDrGLGeneralInfoId { get; set; }

        public string PostDrBudgetMasterId { get; set; }

        public string PostDrActivityId { get; set; }

        public string PostCrGLGeneralInfoId { get; set; }

        public string PostCrBudgetMasterId { get; set; }

        public string PostCrActivityId { get; set; }
        #endregion

    }
}