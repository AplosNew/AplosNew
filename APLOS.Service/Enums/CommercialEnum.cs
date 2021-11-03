namespace Library.Service.Enums
{
    public enum FundUtilization
    {
        Purchase,
        PackingCredit,
        BuildUp,
        ERQ,
        Negotiable
    }

    public enum BuyerDeduction
    {
       LessCommission
    }

    public enum UtilizationSourceType
    {
        FundUtilization,
        BuyerDeduction
    }
    public enum PaymentBasedOn
    {
        Acceptance,
        Negotiation,
        Shipping
    }

}
