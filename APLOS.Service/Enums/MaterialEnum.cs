using System.ComponentModel;

namespace Library.Service.Enums
{
    public enum EnumMaterialTypeNatureList
    {
        Asset,
        Consumable,
        FinishedGoods,
        SemiFinishedGoods,
        Spare,
        RawMaterial
    }

    public enum UsageTypeList
    {
        General
        , Repair
        , Consumable
    }

    public enum RequirementTypeList
    {
        Regular
        , Emergency
    }

    public enum HazardsList
    {
        NotApplicable
        , Low
        , Medium
        , High
    }

    public enum FlammabilityList
    {
        NotApplicable
        , Low
        , Medium
        , High
    }

    public enum PurchaseFrequencyList
    {
        Daily
        , Weekly
        , Fortnight
        , Monthly
        , Quarterly
        , Semiannually
        , Yearly
    }

    public enum AttributePropertiesEnum
    {
        Integer,
        Decimal,
        Alphanumeric
    }

    public enum ValueAssignmentEnum
    {
        General,
        Specific,
        FreeText
    }

    public enum ProductEfficency
    {
        [Description(nameof(Costing))]
        Costing = 1,

        [Description(nameof(Planning))]
        Planning = 2,

        [Description("Product Target")]
        ProductTarget = 3
    }

    public enum InventoryIssuePolicy
    {
        FIFO,
        LIFO,
        WeightedAverage,
    }
}