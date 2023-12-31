using System;

namespace Library.ViewModel.Invoices
{
    public class MultiplePaymentViewModel
    {
        public string Id { get; set; }

        public bool IsFifo { get; set; }
        public bool IsPark { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime DueUpToDate { get; set; }
        public DateTime TentativeDate { get; set; }

        /// <summary>
        /// Data source Ex.: Opening Balance, Customer Invoice, Integration, Sales Invoice.
        /// </summary>
        public string SourceType { get; set; }

        public string ApprovalStatus { get; set; }


       


        
        public string CompanyGroupId { get; set; }

        public string CompanyId { get; set; }
        public string PlantId { get; set; }
        public string BankMasterId { get; set; }
        public string PartyId { get; set; }
        public string PartyPlantId { get; set; }


    }
}