using System.Collections.Generic;

namespace Library.ViewModel.Accounts
{
    public class MultiplePaymentDetailViewModel
    {
        public string Id { get; set; }

        public decimal Amount { get; set; }
        public bool IsPark { get; set; }

        public string MultiplePaymentId { get; set; }
        public string CurrencyId { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceDetailId { get; set; }
        public string PartyId { get; set; }
        public string PartyPlantId { get; set; }

        public string ExchangeType { get; set; }

        public decimal ExchangeAmount { get; set; }
        public string CompanyCurrencyId { get; set; }
        public string CompanyCurrencyName { get; set; }
        public string CompanyFromCurrencyId { get; set; }
        public decimal CompanyCurrencyRate { get; set; }
        public decimal CompanyCurrencyConversion { get; set; }
    }
}