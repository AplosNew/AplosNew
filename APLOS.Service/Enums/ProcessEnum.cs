namespace Library.Service.Enums
{
    public enum EnumProcessConfigBomOrRecipeList
    {
        Recipe
    }

    public enum EnumProcessConfigLevelList
    {
        Grid,
        MaterialMaster,
        Product,
        Style
    }

    public enum EnumProcessConfigMaterialTaggingTypeList
    {
        Operation,
        Process
    }
    public enum EnumProcessSetDetailJobWorkTypeList
    {
        EntityWithinCompany,
        EntityWithinGroup,
        Party
    }
    public enum EnumJobWorkTypeList
    {
        EntityWithinCompany,
        EntityWithinGroup,
        Vendor
    }
    public enum EnumProductionProcessGroupJobWorkTypeList
    {
        Internal,
        External
    }
    public enum EnumJobDescriptionLevelList
    {
        Critical,
        Important,
        Normal
    }
    public enum EnumJobDescriptionPrimaryOrSecondaryList
    {
        Primary,
        Secondary
    }
    public enum EnumJobDescriptionFrequencyList
    {
        Annually,
        Semi,
        Quarterly,
        Monthly
        //Fortnightly,
        //Weekly,
        //Daily
    }
    public enum EnumJobDescriptionNatureOrActivityList
    {
        Planning,
        Production
    }
    public enum EnumJobDescriptionSystemOrManualList
    {
        System,
        Manual
    }
    public enum EnumRequiredTimeUnit
    {
        Days,
        Hour,
        Minutes,
        second
    }
    public enum EnumProcessNature
    {
        Packing,
        Dispatch
    }

    public enum ProductionBookingLevel
    {
        MasterOrderItem,
        ProductionOrder,
        ProductCode,
        SalesOrder,
        SubProductionOrder
        //UptoSKU2,
        //UptoSKU3
    }
}

