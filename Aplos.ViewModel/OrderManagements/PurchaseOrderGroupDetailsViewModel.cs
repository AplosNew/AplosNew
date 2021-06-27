using Library.Core;
using System;

namespace Library.ViewModel.OrderManagements
{
    public class PurchaseOrderGroupDetailsViewModel : BaseModel
    {
        #region Scalar Properties


        public string Id { get; set; }
        public string CompanyGroupId { get; set; }
        public string CompanyId { get; set; }
        public string PlantId { get; set; }
        public string PurchaseOrderGroupId { get; set; }
        public string MaterialMasterId { get; set; }
        public string MaterialMasterName { get; set; }
        
        public string ArticleId { get; set; }
        public string FirstCharacteristicsId { get; set; }
        public string FirstCharacteristicsValueId { get; set; }
        public string SecondCharacteristicsId { get; set; }
        public string SecondCharacteristicsValueId { get; set; }
        public string ThirdCharacteristicsId { get; set; }
        public string ThirdCharacteristicsValueId { get; set; }
        public string ResponsiblePerson { get; set; }
        public string EmployeeCode { get; set; }


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

        [NeverUpdate]
        public string UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }
        //public string MaterialDetail { get; set; }
        public object TransactionQty { get; set; }
        public object EstimatedRate { get; set; }
        public object TotalAmount { get; set; }

        #endregion Audit Properties



    }
}