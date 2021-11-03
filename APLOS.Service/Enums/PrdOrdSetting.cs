using System.ComponentModel;

namespace Library.Service.Enums
{
    public enum PrdOrdSettingEnum
    {
        //[Description(nameof(Entity))]
        //Entity = 1,

        //[Description("Master Order")]
        //MasterOrder = 2,

        //[Description(nameof(Buyer))]
        //Buyer = 3,

        //[Description("Buyer PO")]
        //BuyerPO = 4,

        //[Description("Sales Order")]
        //SalesOrder = 5,

        //[Description("Finish Goods")]
        //FinishGoods = 6,

        //[Description(nameof(Article))]
        //Article = 7

        
        [Description("Master Order")]
        MasterOrder = 1,

        [Description("Customer")]
        Customer = 2,

        [Description("Sales Order")]
        SalesOrder = 3,

        [Description("Material Master")]
        MaterialMaster = 4,

        [Description(nameof(Article))]
        Article = 5

    }
}