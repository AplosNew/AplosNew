namespace Library.Service.Enums
{
    public enum EnumManpowerTypeList
    {
        //[Description("Indirect Name")]
        Indirect,

        Direct
    }

    public enum EnumOrderStatus
    {
        Excepted,
        Pending,
        Confirmed
    }

    public enum EnumOrderGrade
    {
        Normal,
        Critical,
        Semicritical
    }

    public enum EnumSOResponsiblePersonBy
    {
        Position,
        Budget,
        Employee
    }

    public enum EnumAssetAttribute
    {
        MachineType
    }

    public enum EnumStatus
    {
        Running,
        Closed
    }

    public enum EnumStatusForProjectPlanning
    {
        UpComming,
        Running,
        Closed
    }

    public enum EnumPackFormType
    {
        First,
        Second
    }

    public enum EnumStyleCategory
    {
        Normal,
        Critical,
        SemiCrtitical
    }
}