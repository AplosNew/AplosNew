using Library.Core;

namespace Library.ViewModel.OrderManagements
{
    public class RequisitionIssueDetailViewModel : BaseModel
    {

        public string Id { get; set; }

        public decimal IssueQty { get; set; }
        public decimal IssueValidQty { get; set; }
        public decimal IssueRejectedQty { get; set; }

        public string IssueMasterId { get; set; }

        public string IssueRequestId { get; set; }

        public string IssueRequestMasterId { get; set; }
        public string IssueDetailId { get; set; }

        public string ThirdCharacteristicsValueId { get; set; }
        public string ThirdCharacteristicsId { get; set; }
        public string SecondCharacteristicsValueId { get; set; }
        public string SecondCharacteristicsId { get; set; }
        public string FirstCharacteristicsValueId { get; set; }
        public string FirstCharacteristicsId { get; set; }
        public string ArticleId { get; set; }
        public string MaterialMasterId { get; set; }
        public string OpeningBalanceId { get; set; }
        public string MaterialStorageId { get; set; }
        public string CountryId { get; set; }
        public string EntityId { get; set; }
        public string PlantId { get; set; }
    }
}