using System.ComponentModel;

namespace Library.Service.Enums
{
    public enum BarcodeGeneratorSettingEnum
    {
        ProductionOrder,
        [Description("Article/ProductCode")]
        ArticleProductCode,
        LineItem,
        [Description("LineItem,SKU1")]
        LineItemSKU1,
        [Description("LineItem,SKU1,SKU2")]
        LineItemSKU1SKU2,
        SalesOrder,
        SKU1,
        SKU2,
        [Description("SO,SKU1")]
        SOSKU1,
        [Description("SO,SKU1,SKU2")]
        SOSKU1SKU2,
        [Description("Article/ProductCode,SKU1")]
        ArticleProductCodeSKU1,
        [Description("Article/ProductCode,SKU1,SKU2")]
        ArticleProductCodeSKU1SKU2,
    }
}