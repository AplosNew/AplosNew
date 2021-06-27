using System.ComponentModel;

namespace Library.Service.Enums
{
    public enum DataSourceCategory
    {
        [Description("Internal-Within Department")]
        InternalWithinDepartment

          , External

          , [Description("Internal-Other Department")]
        InternalOtherDepartment
    }

    public enum DocumentFormate
    {
        Excel,
        pdf,
        Word,
        CrystalReport,
        Register,
        Form,
        Email,
        PPT,
        Txt,
        JPEG
    }

    public enum ActivityCategory
    {
        Planning,
        Follow_Up,
        Review,
        Decision,
        Execute
    }

    public enum Period
    {
        Daily,
        Weekly,
        Fortnight,
        Monthly,
        Quarterly,
        Semiannually,
        Annually
    }

    public enum ActivityImportance
    {
        NotApplicable,
        Low,
        Medium,
        High,
        Critical
    }

    public enum DevelopmentCategory
    {
        SalesManSample,
        FitSample,
        SizeSetSample,
        PPSample
    }

    public enum BuyerActivity
    {
        MarketingAndMerchandising,
        CommercialAndLogistics,
        ProcurementAndPlanning,
        Finance
    }
    public enum InquiryActivity
    {
        PreCosting,
        Sampling
    }
    public enum OrderActivityType
    {
        Buyer,
        Inquiry
    }
}