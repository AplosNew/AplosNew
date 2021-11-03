using Library.Core;
using System;
using System.Collections.Generic;

namespace Library.Model.Productions.SalesOrderInvoice
{
    public class SalesOrderInvoiceMaster : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string CompanyGroupId { get; set; }
        public string CompanyId { get; set; }
        public string PlantId { get; set; }
        public string EntityId { get; set; }
        public string SalesOrganizationId { get; set; }
        public string CustomerId { get; set; }
        public string SalesGroupId { get; set; }
        public string InvoiceNo { get; set; }
        public string CurrencyId { get; set; }
        public decimal InvoiceValue { get; set; }
        public string SalesTypeId { get; set; }
        public string PaymentTermId { get; set; }
        public DateTime InvoiceDate { get; set; }

        public DateTime? BaseOnDueDate { get; set; }
        public DateTime ActualDueDate { get; set; }
        public DateTime? RevisedDueDate { get; set; }
        public int BaseNoOfDays { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        ///
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        ///
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        ///
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

        public ICollection<SalesOrderInvoicePackingList> SalesOrderInvoicePackingList { get; set; }
    }
}