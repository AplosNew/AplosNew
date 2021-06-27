using System.Collections.Generic;

namespace Library.ViewModel.OrderManagements
{
    public class SalesOrderCharacteristicsViewModel
    {
        public string Id { get; set; }
        public int Sequence{ get; set; }
        public string SalesOrderId { get; set; }
        public string FirstCharacteristicsId { get; set; }
        public string SecondCharacteristicsId { get; set; }
        public string CharacteristicsId { get; set; }
        public string CharacteristicsValueId { get; set; }
        public string ValueFreeText { get; set; }
        public decimal Qty { get; set; }
        public string Flag { get; set; }

        public virtual ICollection<SalesOrderCharacteristicsViewModel> ChildList { get; set; }
    }
}