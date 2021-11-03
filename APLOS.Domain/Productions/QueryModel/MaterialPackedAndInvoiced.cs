using Library.Core;

namespace Library.Model.Productions.QueryModel
{
    public class MaterialPackedAndInvoiced : BaseModel
    {
        #region Scalar Properties

        //=========================================================
        public string SalesOrderInvoiceMasterId { get; set; }

        public string SalesOrderInvoicePackingListId { get; set; }
        public string cv2 { get; set; }
        public string cv1 { get; set; }
        public string c3Id { get; set; }
        public string c2Id { get; set; }
        public string c1Id { get; set; }
        public string cv3 { get; set; }
        public decimal Rate { get; set; }

        public string Archive { get; set; }

        public string BalanceQty { get; set; }
        public string Char1Qty { get; set; }
        public string Char2Qty { get; set; }
        public string Characteristics1Id { get; set; }
        public string Characteristics1Name { get; set; }
        public string Characteristics2Id { get; set; }
        public string Characteristics2Name { get; set; }
        public string Characteristics3Id { get; set; }
        public string Characteristics3Name { get; set; }
        public string CharacteristicsValue1Id { get; set; }
        public string CharacteristicsValue1Name { get; set; }
        public string CharacteristicsValue2Id { get; set; }
        public string CharacteristicsValue2Name { get; set; }
        public string CharacteristicsValue3Id { get; set; }
        public string CharacteristicsValue3Name { get; set; }
        public string CurrentQty { get; set; }
        public string Customer { get; set; }
        public string CustomerPOId { get; set; }
        public string DeliveryDate { get; set; }
        public string DeliveryDateId { get; set; }
        public string Detail { get; set; }
        public string FileNo { get; set; }
        public string GridNo { get; set; }
        public string Id { get; set; }
        public string InvoicedQty { get; set; }
        public string IsMMLevel { get; set; }
        public string IsSingleEntry { get; set; }
        public string IsSingleEntry2 { get; set; }
        public string MaterialGroup { get; set; }
        public string MaterialGroupMasterId { get; set; }
        public string MaterialMaster { get; set; }
        public string MaterialMasterId { get; set; }
        public string OrderQty { get; set; }
        public string PackedQty { get; set; }
        public string PackingFormId1 { get; set; }
        public string PackingFormId2 { get; set; }
        public string PommQty { get; set; }
        public string PONumber { get; set; }
        public string SalesOrderCharacteristicsValue1stId { get; set; }
        public string SalesOrderCharacteristicsValue2ndId { get; set; }
        public string SalesOrderMasterId { get; set; }
        public string SalesOrderMaterialMasterId { get; set; }
        public string Submaterial { get; set; }
        public string SubmaterialCode { get; set; }
        public string SubMaterialId { get; set; }
        public string UomId { get; set; }
        public string UomName { get; set; }

        #endregion Scalar Properties
    }
}