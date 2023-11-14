namespace Library.Service.Enums
{
   
    public enum LoanTransactionType
    {
        Loan,
        LoanPayment,
        LoanInterestPayable,
        InterestPayable,
        AccrulInterestPayment,
        CashInterestPayment,
        InterestPayableReverse,
        ChargesPayableReverse,
        AdditionalLoanPayable,
        AdditionalLoanPayment,
        OtherExpensesPayable,
        OtherExpensesPayment,
        LoanTax
    }

    public enum InvestmentTransactionType
    {
        Investment,
        InvestmentSetOff,
        InvestmentInterestReceivable,
        InterestReceivable,
        AdditionalInvestmentReceivable,
        InvestmentTax
    }
}