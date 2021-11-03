using Library.Core;
using System;

namespace Library.ViewModel.Inventory
{
    public class GatePassDetailsViewModel : BaseModel 
    {

		public string Id { get; set; }
		public string GatePassMasterId { get; set; }
		public string MaterialMasterId { get; set; }
		public string ArticleId { get; set; }
		public string FirstCharacteristicsId { get; set; }
		public string FirstCharacteristicsValueId { get; set; }
		public string SecondCharacteristicsId { get; set; }
		public string SecondCharacteristicsValueId { get; set; }
		public string ThirdCharacteristicsId { get; set; }
		public string ThirdCharacteristicsValueId { get; set; }
		public string MaterialDetail { get; set; }
		public decimal TransactionQty { get; set; }
		public string TransactionUoMId { get; set; }
		public string Remarks { get; set; }
		public bool IsReturnable { get; set; }
		public DateTime? ReturnableDate { get; set; }
		public bool IsMutilated { get; set; }

		public decimal Rate { get; set; }
		public string ChallanNo { get; set; }
		public string ChallanNoDetailId { get; set; }  
		

	}
}