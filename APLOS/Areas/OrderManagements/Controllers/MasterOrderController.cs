#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using Library.Service.OrderManagements;
using Library.Service.Parties;
using Library.Service.Productions;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Data.Sql;
using OTSBD;
using Library.Service.Helpers;
using System.Collections.Specialized;
using Library.Service.Enums;
using Aplos.Helpers;
using System.Web;
using System.Linq;
#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class MasterOrderController : BaseController
    {
        #region -- Constructor

        private readonly string ExchangeRateTableName = "MasterOrderExchangeRates";

        private readonly IMasterOrderService _masterOrderService;
        private readonly IPartyService _partyService;
        private readonly ICustomerPOService _customerPOService;
        private readonly ISqlRepository _sqlRepository;
        public MasterOrderController(IMasterOrderService masterOrderService, IPartyService partyService, ICustomerPOService customerPOService, ISqlRepository R)
        {
            _masterOrderService = masterOrderService;
            _partyService = partyService;
            _customerPOService = customerPOService;
            _sqlRepository = R;
        }

        #endregion
        private string CellAddr(int Col, int Row)
        {
            return clsStaticInfo.GetxlsCol(Col) + Row.ToString();
        }

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }


        public ActionResult IndependentOrder()
        {
            return View();
        }

        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetProductLibrary()
        {
            return Json(_sqlRepository.GetDataCollection(@"SELECT PL.Id as Value,Text=CASE WHEN PL.RecipeOrProductionGroup = 'Recipe' THEN RGM.UserName+' ('+PL.RecipeOrProductionGroup+')' ELSE PL.ProductionGroup+' ('+PL.RecipeOrProductionGroup+')' END
                        FROM dbo.ProductLibrary PL
                        LEFT JOIN[TRN].[RecipeGlobalMaster] RGM ON RGM.Id = PL.RecipeId WHERE PL.Active =1"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetContractByMasterOrder(string masterId)
        {
            string sql = @"SELECT DISTINCT C.*, P.UserName AS CustomerName,PM.UserName MarketingCommisssion FROM dbo.[Contract] C
                            JOIN TRN.MasterOrderItem I ON I.ContractId=C.Id
                            JOIN TRN.MasterOrder M ON M.Id=I.MasterOrderId
                            JOIN [HKP].[Party] AS P ON C.CustomerId=P.Id 
                            LEFT JOIN [HKP].[Party] AS PM ON C.MarketingCommisssionId=PM.Id 
                            WHERE M.Id='" + masterId + "'";
            //string sql = @"SELECT C.*, P.UserName AS CustomerName,PM.UserName MarketingCommisssion FROM dbo.[Contract] C
            //                JOIN [HKP].[Party] AS P ON C.CustomerId=P.Id 
            //                LEFT JOIN [HKP].[Party] AS PM ON C.MarketingCommisssionId=PM.Id 
            //                WHERE C.MasterOrderId='" + masterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDelivaryDate(string year, int weekNo, string buyerId)
        {
            return Json(_masterOrderService.GetDelivaryDate(year, weekNo, buyerId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetOrderDateSetting(string shipmentModeId, string buyerId)
        {
            return Json(_masterOrderService.GetOrderDateSetting(shipmentModeId, buyerId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSalesOrderTaxCategoryList(string salesOrderId)
        {

            return Json(_masterOrderService.GetSalesOrderTaxCategoryList(salesOrderId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryList(string masterOrderId, string plantId, string hsnCodeId, string specialTaxId, string PODate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId) || plantId == "null") plantId = identity.PlantId;
            return Json(_masterOrderService.GetTaxCategoryList(identity.CompanyGroupId, masterOrderId, plantId, hsnCodeId, specialTaxId, PODate), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeListResponsible(GridParameter parameters, string plantId, string partyAccountGroupId, string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId))
            {
                plantId = identity.PlantId;
            }
            return Json(_masterOrderService.GetEmployeeListResponsible(parameters, identity.CompanyId, plantId, partyAccountGroupId, partyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFirstSkuList(string salesOrderId)
        {
            return Json(_masterOrderService.GetFirstSkuSalesOrderId(salesOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllSkuSalesOrderId(string salesOrderId)
        {
            var firstData = _masterOrderService.GetFirstSkuSalesOrderId(salesOrderId);
            var secondtData = _masterOrderService.GetSecondSkuSalesOrderId(salesOrderId);
            var thirdData = _masterOrderService.GetThirdSkuSalesOrderId(salesOrderId);
            return Json(new { firstData, secondtData, thirdData }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCharacteristicsByMaterialMasterId(string materialMasterId)
        {
            return Json(_masterOrderService.GetCharacteristicsByMaterialMasterId(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetChValueCbo(string materialId)
        {
            //return Json(_masterOrderService.GetChValueCbo(materialId), JsonRequestBehavior.AllowGet);
            return Json(_masterOrderService.GetChValueCboByMaterialId(materialId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSOandItemList(string masterItemId)
        {
            return Json(_masterOrderService.GetSOList(masterItemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string companyId)
        {
            return Json(_masterOrderService.Query(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetIdependentList(GridParameter parameters, string companyId)
        {
            return Json(_masterOrderService.QueryIdependent(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAttributeListByMaterialMasterId(string materialMasterId)
        {
            return Json(_masterOrderService.GetAttributeListByMaterialMasterId(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOrderAttributeListByMasterId(string masterItemId, string materialMasterId)
        {
            return Json(_masterOrderService.GetOrderAttributeListByMasterId(masterItemId, materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetArticleCodeList(string materialMasterId, string articleCode)
        {
            return Json(_masterOrderService.GetArticleCodeList(materialMasterId, articleCode), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSpecialTaxList(string plantId)
        {
            return Json(_masterOrderService.GetSpecialTaxList(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaskList(string buyerId, string buyerDepartmentId, string buyerDivisionId, string moId)
        {
            return Json(_masterOrderService.GetTaskList(buyerId, buyerDepartmentId, buyerDivisionId, moId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCompanyPartyDataList(GridParameter parameters, string companyId, string plantId, string partyType)
        {
            if (plantId == "null") plantId = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_masterOrderService.GetCompanyPartyList(parameters, identity.CompanyGroupId, companyId, plantId, partyType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMasterItemList(string masterOrderId)
        {
            return Json(_masterOrderService.GetMasterItemList(masterOrderId), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public ActionResult GetDepartmentPersonList(string plantId, string partyAccountGroupId, string partyId, bool flag)
        //{
        //    return Json(_masterOrderService.GetDepartmentPersonList(plantId, partyAccountGroupId, partyId, flag), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public ActionResult GetResponsiblePersonList(string masterId)
        {
            return Json(_masterOrderService.GetResponsiblePersonList(masterId), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public ActionResult GetDepartmentPersonCbo(string plantId, string partyAccountGroupId, string partyId)
        //{
        //    return Json(_masterOrderService.GetDepartmentPersonCbo(plantId, partyAccountGroupId, partyId), JsonRequestBehavior.AllowGet);
        //}
        [HttpPost]
        public JsonResult Create(MasterOrder entity, List<MasterOrderTNA> taskList, List<Dictionary<string, object>> CurrencyData)
        {
            _masterOrderService.Insert(entity, taskList);
            Library.Planning.OrderManagement.MasterOrder mo = new Library.Planning.OrderManagement.MasterOrder();
            mo.GenerateLogForTnA(entity.Id, Library.Service.Enums.TaskAppliedOnEnum.MasterOrder);

            Library.General.Conversions.CurrencyConversions con = new Library.General.Conversions.CurrencyConversions(ExchangeRateTableName);
            con.SaveConversion(entity.Id, CurrencyData);
            return Json(new { MasterOrder = entity, Message = AplosMessage.Success });
        }
        [HttpPost]
        public JsonResult CreateIndependent(MasterOrder entity)
        {
            _masterOrderService.InsertOrUpdate(entity);
            return Json(new { MasterOrder = entity, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(MasterOrder entity, string masterId, IEnumerable<MasterOrderResPerson> personList, IEnumerable<MasterOrderItem> itemList, List<Dictionary<string, object>> CurrencyData)
        {
            _masterOrderService.Update(entity, masterId, personList, itemList);
            Library.Planning.OrderManagement.MasterOrder mo = new Library.Planning.OrderManagement.MasterOrder();
            mo.GenerateLogForTnA(masterId, Library.Service.Enums.TaskAppliedOnEnum.MasterOrder);

            Library.General.Conversions.CurrencyConversions con = new Library.General.Conversions.CurrencyConversions(ExchangeRateTableName);
            con.SaveConversion(entity.Id, CurrencyData);

            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult CreateAttributeValue(string masterItemId, IEnumerable<MasterOrderAttributeValue> attributeValueList)
        {
            _masterOrderService.InsertOrUpdateGraph(masterItemId, attributeValueList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult CreateSalesOrder(string masterItemId, SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.InsertOrUpdateSOGraph(masterItemId, salesOrderMaster);
            Library.Planning.OrderManagement.MasterOrder mo = new Library.Planning.OrderManagement.MasterOrder();
            mo.GenerateLogForTnA(masterItemId, Library.Service.Enums.TaskAppliedOnEnum.SalesOrder);

            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult CreateSplitSalesOrder(string masterItemId, SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.InsertOrUpdateSplitSOGraph(masterItemId, salesOrderMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult UpdateSalesOrder(string masterItemId, SalesOrderMaster salesOrderMaster, IEnumerable<SalesOrderTax> taxCategoryList)
        {
            _masterOrderService.UpdateSOGraph(masterItemId, salesOrderMaster, taxCategoryList);
            Library.Planning.OrderManagement.MasterOrder mo = new Library.Planning.OrderManagement.MasterOrder();
            mo.GenerateLogForTnA(masterItemId, Library.Service.Enums.TaskAppliedOnEnum.Style);
            return Json(new { Message = AplosMessage.Updated });
        }



        [HttpPost, Authorize]
        public JsonResult CreateSalesOrderTax(string salesOrderId, IEnumerable<SalesOrderTax> taxCategoryList)
        {
            _masterOrderService.InsertOrUpdateSalesOrderTax(salesOrderId, taxCategoryList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult DeleteSalesOrder(string masterItemId, SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.DeleteSOGraph(masterItemId, salesOrderMaster);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public JsonResult CreateCharacteristics(IEnumerable<SalesOrderCharacteristicsViewModel> entities, int listLength, string soId)
        {
            _masterOrderService.InsertOrUpdateCharacteristics(entities, listLength, soId);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _masterOrderService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        [HttpGet, Authorize]
        public ActionResult UpdateLoggedTnA()
        {
            try
            {
                Library.Planning.OrderManagement.MasterOrder MasterOrderTnA = new Library.Planning.OrderManagement.MasterOrder();
                MasterOrderTnA.RunTNASchedule();

                return Json(new { Message = "TnA updated successfully", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult DeleteItem(string id)
        {
            try
            {
                _masterOrderService.DeleteItem(id);

                var directory = ResourcesPathReader.GetMOIDocumentPath();
                var path = Path.Combine(directory);
                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                string sql = "SELECT * FROM TRN.MasterOrderItem WHERE Id='" + id + "'";
                DataSet dsLocal = null;
                connection.BeginTransaction();
                connection.getDataSet(sql, out dsLocal);
                connection.CommitTransaction();
                var FN = dsLocal.Tables[0].Rows[0]["FileName"].ToString();

                if (System.IO.File.Exists(path + id + Path.GetExtension(FN)))
                    System.IO.File.Delete(path + id + Path.GetExtension(FN));

                return Json(new { Message = AplosMessage.Deleted });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost, Authorize]
        public ActionResult DeleteSO(string id)
        {
            _masterOrderService.DeleteSO(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost, Authorize]
        public ActionResult DeleteFirstSku(string id)
        {
            _masterOrderService.DeleteFirstSku(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public JsonResult GetSOBookedQtyAndLevel(string salesOrderId)
        {
            return Json(_masterOrderService.GetSOBookedQtyAndLevel(salesOrderId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPOBookedQtyAndLevel(string salesOrderId)
        {
            return Json(_masterOrderService.GetPOBookedQtyAndLevel(salesOrderId), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region -- Customer Po

        [HttpGet, Authorize]
        public JsonResult GetListByMasterOrder(string companyId, string masterOrderId)
        {
            return Json(_customerPOService.GetListByMasterOrder(companyId, masterOrderId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreatePO(CustomerPO entity)
        {
            return Json(new { tuple = _customerPOService.InsertGraphPo(entity), Message = AplosMessage.Insert });
        }
        #endregion -- Customer Po

        #region Report

        [HttpGet, Authorize]
        public ActionResult GetMasterOrderReport(string masterOrderId)
        {
            try
            {
                // ReportFormat reportFormat = "pdf";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                _masterOrderService.GetProformaInvoiceReportService(identity.CompanyId, identity.PlantId, masterOrderId);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }


        //[Authorize, HttpGet]
        //public ActionResult ProformaInvoiceReportService(string grnId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        //    _masterOrderService.GetProformaInvoiceReportService(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, grnId);

        //    return View();
        //}
        #endregion


        //Master Order Details Report
        [HttpGet, Authorize]
        public ActionResult MasterOrderReport(string MasterOrderId, bool isMatrix)
        {

            try
            {
                // if (string.IsNullOrEmpty(MasterLCList))
                //   throw new Exception("Please select at least one master Order");



                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = GetMasterOrderReport(MasterOrderId, isMatrix);

                string strFileName = "Master Order.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }

        //Get Master order report
        private IWorkbook GetMasterOrderReport(string MasterOrderId, bool isMatrix)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            try
            {
                DataTable dtOrderMaster = _sqlRepository.GetDataTable(@"select mo.Id, mo.type, b.UserName as Buyer, p.UserName as Customer
                ,  mo.OrderYear as Year, mo.TotalQty as TotalQuantity
                    , uom.UserName as UnitOfMeasurement, mo.NoOfLineItem, mo.OrderWastagePercentage
                    , mo.ExtraOrderPercentage, mo.BuyerReferenceNo, mo.OwnReferenceNo, BDept.UserName as BuyerDepartment
                    , BDev.UserName as BuyerDevision, MoCur.Code MasterOrderCurrency from trn.MasterOrder MO 
                    left join scs.Currency MoCur on MoCur.id = mo.CurrencyId 
                    left join scs.UnitOfMeasurement UOM on uom.id = mo.TotalQtyUOMId 
                    left join hkp.buyer B on b.id = mo.buyerid 
                    left join hkp.party p on p.id = mo.partyid 
                    left join hkp.BuyerDepartment BDept on BDept.id = mo.buyerDepartmentid 
                    left join HKP.buyerdivision BDev on BDev.id = mo.BuyerDivisionId where mo.Id='" + MasterOrderId + "'");
                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                DataTable dtMasterOrderItem = _sqlRepository.GetDataTable(@"select moi.id as MasterOrderItemNo,moi.BuyerReferenceNo
                 ,moi.OwnReferenceNo,moi.TotalQty as TotalMOIQuantity, moi.MasterOrderId
                 ,moi.OrderWastagePercentage, moi.ExtraOrderPercentage ,mm.UserName as Material ,mma.StandardName as Article, moi.Type
                 from trn.MasterOrderItem MOI
                 left join TRN.MasterOrder mo  on mo.id=moi.MasterOrderId
                 left join MST.MaterialMaster MM on mm.id = moi.MaterialMasterId
                 left join MST.MaterialMasterArticle mma on mma.id= moi.ArticleId
                 left join scs.TestingStandard ts on ts.id=moi.TestingStandardId

                 where moi.MasterOrderId='" + MasterOrderId + "'");


                DataTable dtSalesOrderItem = _sqlRepository.GetDataTable(@"SELECT K.MasterOrderItemId, K.SalesOrderNo, K.PONumber, K.PODate, K.OrderStatus,
                                           K.Destination, K.UpCharge, K.MainRawMaterialInhouseDate,
                                           K.[Description], K.SOType, K.OrderCategory, K.DeliveryDate, K.ShipmentMode,
                                           K.Rate, K.Discount, K.CM, K.LSD, K.OtherRawMaterialInhouseDate, K.Reason,
                                           K.CommitmentDate, K.FirstCharacteristics, K.FirstCharacteristicsValue,
                                           K.SecondCharacteristics, K.SecondCharacteristicsValue,
                                           K.ThirdCharacteristics, K.ThirdCharacteristicsValue, sum(K.Qty) AS Qty, K.Quantity
                                      from (select so.MasterOrderItemId, so.id as SalesOrderNo
                ,cpo.PONumber , Replace(CONVERT(VARCHAR(11),  CPO.PODate, 106), ' ', '-') AS PODate
                    ,os.UserName as OrderStatus --,d.UserName as Destination
                   ,Destination= CASE WHEN so.DestinationDescription IS NULL THEN d.UserName  ELSE d.UserName+' ('+ISNULL(so.DestinationDescription,'')+')'  END
                ,so.Qty as Quantity, so.UpCharge, so.MainRawMaterialInhouseDate, so.Description
                ,so.SOType, oc.username as OrderCategory
                ,so.DeliveryDate, sm.UserName as ShipmentMode
                ,so.Rate, so.Discount,so.CM,so.LSD, so.OtherRawMaterialInhouseDate , so.Reason , so.CommitmentDate

                ,c1.username as FirstCharacteristics, isnull(fcs.ValueFreeText,CV1.UserName) as FirstCharacteristicsValue
                ,c2.username as SecondCharacteristics ,isnull(SCS.ValueFreeText, CV2.UserName) as SecondCharacteristicsValue
                ,c3.username as ThirdCharacteristics , isnull(ThirdCS.ValueFreeText,CV3.UserName) as ThirdCharacteristicsValue
                ,case when isnull(thirdCs.Id,'')<>'' THEN ThirdCs.Qty
                ELSE case when isnull(scs.id,'')<>'' THEN scs.Qty
                ELSE case when isnull(fcs.Id,'')<>'' THEN fcs.Qty
                ELSE 0 END END END AS Qty
                from trn.SalesOrder SO
                left join trn.masterorderitem moi on moi.id= so.masterorderitemid
                left join HKP.OrderCategory OC on oc.id = so.OrderCategoryId
                left join hkp.OrderStatus OS on os.id = so.OrderStatusId
                left join mst.shipMode SM on sm.id = so.shipmentModeId
                left join mst.Destination d on d.id =so.DestinationId
                left join trn.CustomerPO CPO on cpo.id =so.CustomerPOId

                left join TRN.FirstCharacteristics FCS on fcs.SalesOrderId = so.id
                left join hkp.Characteristics C1 on c1.id = fcs.CharacteristicsId
                left join HKP.CharacteristicsValue CV1 on cv1.id= fcs.CharacteristicsValueId

                left join TRN.SecondCharacteristics SCS on scs.SalesOrderId=so.id and scs.FirstCharacteristicsId=fcs.Id
                left join hkp.Characteristics C2 on c2.id = scs.CharacteristicsId
                left join HKP.CharacteristicsValue CV2 on cv2.id= scs.CharacteristicsValueId

                left join TRN.ThirdCharacteristics ThirdCS on ThirdCS.SalesOrderId=so.id and scs.id=ThirdCS.SecondCharacteristicsId
                left join hkp.Characteristics C3 on c3.id = ThirdCS.CharacteristicsId
                left join HKP.CharacteristicsValue CV3 on CV3.id= ThirdCS.CharacteristicsValueId

                 where moi.MasterOrderId='" + MasterOrderId + @"'

					 ) AS K
					 
                GROUP BY K.MasterOrderItemId, K.SalesOrderNo, K.PONumber, K.PODate, K.OrderStatus,
                       K.Destination, K.UpCharge, K.MainRawMaterialInhouseDate,
                       K.[Description], K.SOType, K.OrderCategory, K.DeliveryDate, K.ShipmentMode,
                       K.Rate, K.Discount, K.CM, K.LSD, K.OtherRawMaterialInhouseDate, K.Reason,
                       K.CommitmentDate, K.FirstCharacteristics, K.FirstCharacteristicsValue,
                       K.SecondCharacteristics, K.SecondCharacteristicsValue,
                       K.ThirdCharacteristics, K.ThirdCharacteristicsValue, K.Quantity");

                worksheet.Name = "MasterOrderDetailsReport";

                int ROW = 6; int COL = 1;

                //foreach (ExcelKnownColors val in Enum.GetValues(typeof(ExcelKnownColors)))
                //{
                //    worksheet[ROW, 1].CellStyle.Interior.ColorIndex = val;
                //    worksheet[ROW, 1].Text = val.ToString();
                //    ROW++;
                //}

                int MasterOrderDetailsStartRow = ROW;
                worksheet[ROW, COL].Text = "Master Order Details:";
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                ROW++;

                int leftColumnCaption = COL;
                int leftColumnValue = leftColumnCaption + 1;

                int MiddleColumnCaption = leftColumnValue + 2;
                int MiddleColumnValue = MiddleColumnCaption + 1;

                int RightColumnCaption = MiddleColumnValue + 2;
                int RightColumnValue = RightColumnCaption + 1;

                //Master Order.............................................................
                //worksheet[ROW, leftColumnCaption].Text = "Master Order No";
                //worksheet[ROW, leftColumnValue].Text = "MasterOrderNo";

                worksheet[ROW, leftColumnCaption].Text = "Order#";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Id"].ToString();
                // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                worksheet.Range[ROW, leftColumnCaption, ROW, leftColumnValue].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Buyer.Ref";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["BuyerReferenceNo"].ToString();
                worksheet.Range[ROW, MiddleColumnCaption, ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, RightColumnCaption].Text = "Own.Ref";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["OwnReferenceNo"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                ROW++;

                worksheet[ROW, leftColumnCaption].Text = "Buyer";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Buyer"].ToString();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Buyer Dep.";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["BuyerDepartment"].ToString();
                worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, RightColumnCaption].Text = "Buyer Div";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["BuyerDevision"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                ROW++;

                worksheet[ROW, leftColumnCaption].Text = "Customer";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Customer"].ToString();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                worksheet[ROW, MiddleColumnCaption].Text = "Currency";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["MasterOrderCurrency"].ToString();
                worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;
                ROW++;


                worksheet[ROW, leftColumnCaption].Text = "Total Order Quantity";
                worksheet[ROW, leftColumnValue].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[0]["TotalQuantity"].ToString());
                worksheet[ROW, leftColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                // worksheet[ROW, leftColumnValue, ROW , leftColumnValue].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, leftColumnValue, ROW, leftColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                //worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Bold = true;
                worksheet[ROW, leftColumnValue].HorizontalAlignment = ExcelHAlign.HAlignRight;

                worksheet[ROW, leftColumnValue + 1].Text = dtOrderMaster.Rows[0]["UnitOfMeasurement"].ToString();
                //worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                worksheet.Range[MasterOrderDetailsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom44;



                ROW += 2;


                //Master Order Item....................................................................................................
                StringCollection strColSO = new StringCollection();

                for (int i = 0; i < dtMasterOrderItem.Rows.Count; i++)
                {
                    int MasterItemsStartRow = ROW; // row 12
                    worksheet[ROW, COL].Text = "Item Details:"; //col 1
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    ROW++;


                    // int MasterItemsStartRow = ROW;
                    strColSO = new StringCollection();
                    // worksheet[ROW, leftColumnCaption].Text = "Items Details";



                    worksheet[ROW, leftColumnCaption].Text = "Material";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["Material"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Article";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["Article"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Buyer Ref";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["BuyerReferenceNo"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, MiddleColumnCaption].Text = "Own Ref";
                    worksheet[ROW, MiddleColumnValue].Text = dtMasterOrderItem.Rows[i]["OwnReferenceNo"].ToString();
                    worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, RightColumnCaption].Text = "Qty";
                    worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                    worksheet[ROW, RightColumnValue].Number = clsStaticInfo.dbl(dtMasterOrderItem.Rows[i]["TotalMOIQuantity"].ToString());
                    //worksheet.Range[ROW, RightColumnValue, ROW, RightColumnValue].CellStyle.Font.Bold = true;
                    // worksheet[ROW, RightColumnValue].CellStyle.Font.Bold = true;
                    worksheet[ROW, RightColumnValue, ROW, RightColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                    worksheet.Range[MasterItemsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom18;
                    ROW++;


                    dtSalesOrderItem.DefaultView.RowFilter = "MasterOrderItemId='" + dtMasterOrderItem.Rows[i]["MasterOrderItemNo"].ToString() + "'";
                    DataTable dtSalesOrderFilteredByItem = dtSalesOrderItem.DefaultView.ToTable();
                    for (int KK = 0; KK < dtSalesOrderItem.DefaultView.Count; KK++)
                    {


                        if (strColSO.Contains(dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString()))
                            continue;
                        int SOStartRow = ROW;  //row 16
                        int SoStart = COL;
                        worksheet[ROW, COL].Text = "Sales Order Details & Breakdown:";
                        worksheet[ROW, COL].CellStyle.Font.Bold = true;

                        //int RightColumnCaptionPo = RightColumnValue + 1;
                        //int RightColumnValuePo = RightColumnCaptionPo + 1;
                        COL++; COL++; COL++;
                        int colPo = COL;
                        COL++;
                        int colPoValue = COL;
                        worksheet[ROW, colPo].Text = "PO No.";
                        worksheet[ROW, colPoValue].Text = dtSalesOrderItem.DefaultView[KK]["PONumber"].ToString();
                        worksheet[ROW, colPo].CellStyle.Font.Bold = true;
                        // worksheet[ROW, colPoValue, ROW, colPoValue].NumberFormat = clsStaticInfo.NumberFormat();

                        COL++; COL++;
                        int colPoDate = COL;
                        COL++;
                        int colPoDateValue = COL;
                        worksheet[ROW, colPoDate].Text = "PO Date";
                        worksheet[ROW, colPoDateValue].Text = dtSalesOrderItem.DefaultView[KK]["PODate"].ToString();
                        worksheet[ROW, colPoDate].CellStyle.Font.Bold = true;

                        ROW++;
                        COL = SoStart;
                        // int SOStartRow = ROW;

                        strColSO.Add(dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString());

                        worksheet[ROW, leftColumnCaption].Text = "SO No";
                        worksheet[ROW, leftColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString();
                        worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                        worksheet[ROW, MiddleColumnCaption].Text = "Del. Date";
                        worksheet[ROW, MiddleColumnValue].Text = Convert.ToDateTime(dtSalesOrderItem.DefaultView[KK]["DeliveryDate"].ToString()).ToString("dd-MMM-yyyy");
                        worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                        worksheet[ROW, RightColumnCaption].Text = "Qty";
                        worksheet[ROW, RightColumnValue].Number = clsStaticInfo.dbl(dtSalesOrderItem.DefaultView[KK]["Quantity"].ToString());
                        worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                        worksheet[ROW, RightColumnValue, ROW, RightColumnValue].NumberFormat = clsStaticInfo.NumberFormat();
                        // worksheet[ROW, RightColumnValue].CellStyle.Font.Bold = true;

                        //int RightColumnCaptionPo = RightColumnValue+1;
                        //int RightColumnValuePo = RightColumnCaptionPo + 1;
                        //worksheet[ROW, RightColumnCaptionPo].Text = "PO No.";
                        //worksheet[ROW, RightColumnValuePo].Number = clsStaticInfo.dbl(dtSalesOrderItem.DefaultView[KK]["PONumber"].ToString());
                        //worksheet[ROW, RightColumnCaptionPo].CellStyle.Font.Bold = true;
                        //worksheet[ROW, RightColumnValuePo, ROW, RightColumnValuePo].NumberFormat = clsStaticInfo.NumberFormat();
                        ROW++;

                        worksheet[ROW, leftColumnCaption].Text = "Dest.";
                        worksheet[ROW, leftColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["Destination"].ToString();
                        worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;


                        worksheet[ROW, MiddleColumnCaption].Text = "Ship Mode";
                        worksheet[ROW, MiddleColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["ShipmentMode"].ToString();
                        worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                        worksheet[ROW, RightColumnCaption].Text = "Ord. Status";
                        worksheet[ROW, RightColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["OrderStatus"].ToString();
                        worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                        worksheet.Range[SOStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom19;


                        //int RightColumnCaptionPoDate = RightColumnValue + 1;
                        //int RightColumnValuePoDate = RightColumnCaptionPoDate + 1;
                        //worksheet[ROW, RightColumnCaptionPoDate].Text = "Po Date";
                        //worksheet[ROW, RightColumnValuePoDate].Text = dtSalesOrderItem.DefaultView[KK]["PODate"].ToString();
                        //worksheet[ROW, RightColumnCaptionPoDate].CellStyle.Font.Bold = true;
                        //worksheet.Range[SOStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom19;
                        ROW++;

                        dtSalesOrderFilteredByItem.DefaultView.RowFilter = "SalesOrderNo='" + dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString() + "'"; //????
                        DataTable dtBreakdownData = dtSalesOrderFilteredByItem.DefaultView.ToTable();
                        DrawSOBreakdownData(dtBreakdownData, worksheet, ref ROW, isMatrix);

                        ROW++;
                    }

                    ROW += 2; // Gap for Material
                }

                int endCol = RightColumnValue;


                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.CellStyle.Font.Size = 8f;


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref worksheet, endCol, "Master Order#" + MasterOrderId, identity.PlantId);
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;
                return workbook;

            }
            catch (Exception ex)
            {
                throw (ex);

            }




        }

        private void DrawSOBreakdownData(DataTable dtData, IWorksheet sheet, ref int ROW, bool Matrix = true)
        {

            string FirstCharacteristicsName = "";
            string SecondCharacteristicsName = "";
            string ThirdCharacteristicsName = "";

            DataView dvDistinctCharName = new DataView(dtData.DefaultView.ToTable(true, "FirstCharacteristics")); //all yellow ??
            if (dvDistinctCharName.Count > 0)
                FirstCharacteristicsName = dvDistinctCharName[0]["FirstCharacteristics"].ToString();

            dvDistinctCharName = new DataView(dtData.DefaultView.ToTable(true, "SecondCharacteristics"));
            if (dvDistinctCharName.Count > 0)
                SecondCharacteristicsName = dvDistinctCharName[0]["SecondCharacteristics"].ToString();


            dvDistinctCharName = new DataView(dtData.DefaultView.ToTable(true, "ThirdCharacteristics"));
            if (dvDistinctCharName.Count > 0)
                ThirdCharacteristicsName = dvDistinctCharName[0]["ThirdCharacteristics"].ToString();


            if (FirstCharacteristicsName == "" && SecondCharacteristicsName == "" && ThirdCharacteristicsName == "")
                return;

            if (FirstCharacteristicsName != "" && SecondCharacteristicsName == "" && ThirdCharacteristicsName == "")
            {
                PrintSingleDimensionData(dtData, sheet, FirstCharacteristicsName, ref ROW);
            }

            if (FirstCharacteristicsName != "" && SecondCharacteristicsName != "" && ThirdCharacteristicsName == "")
            {
                if (Matrix == true)
                    PrintMatrixData(dtData, sheet, ref ROW);
                else
                    PrintLinearData(dtData, sheet, ref ROW);
            }


        }
        void PrintSingleDimensionData(DataTable dtData, IWorksheet sheet, string FirstCharacteristicsName, ref int ROW)
        {
            int COL = 1;
            sheet[ROW, COL].Text = FirstCharacteristicsName;  // Heading FirstCharacteristicsName ??? 
            int ColCharValue = COL;
            COL++;
            sheet[ROW, COL].Text = "Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            int colQuantity = COL;

            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            ROW++;

            int StartRow = ROW;
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                sheet[ROW, ColCharValue].Text = dtData.Rows[i]["FirstCharacteristicsValue"].ToString();
                sheet[ROW, colQuantity].Number = clsStaticInfo.dbl(dtData.Rows[i]["Qty"].ToString());
                //sheet[ROW, colQuantity].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, colQuantity].NumberFormat = clsStaticInfo.NumberFormat();
                ROW++;
            }
            sheet[ROW, colQuantity].Formula = "SUM(" + CellAddr(colQuantity, StartRow) + ":" + CellAddr(colQuantity, ROW - 1) + ")";
            // sheet[StartRow, colQuantity, ROW, colQuantity].NumberFormat = "#,##0.00;(#,##0.00)";
            sheet[StartRow, colQuantity, ROW, colQuantity].NumberFormat = clsStaticInfo.NumberFormat();
            //sheet[ROW, colQuantity].NumberFormat =clsStaticInfo.NumberFormat(); //do
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true; //?
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent; //?
        }
        void PrintMatrixData(DataTable dtData, IWorksheet sheet, ref int ROW)
        {

            if (dtData.Rows.Count == 0)
                return;

            int COL = 0;

            COL++;  // 0+1=1 FG Color/FG Size Row 19
            sheet[ROW, COL].Text = dtData.Rows[0]["FirstCharacteristics"].ToString() + "/" + dtData.Rows[0]["SecondCharacteristics"].ToString();
            int colFirstChar = COL;// colFirstChar=FG Color/FG Size
            int colFirstSecCharValue = colFirstChar + 1;

            DataView dvDistinctSecondCharateristicsValues = new DataView(dtData.DefaultView.ToTable(true, "SecondCharacteristicsValue"));
            Dictionary<string, int> dicColumnIndex = new Dictionary<string, int>();
            for (int i = 0; i < dvDistinctSecondCharateristicsValues.Count; i++)
            {
                COL++;
                sheet[ROW, COL].Text = dvDistinctSecondCharateristicsValues[i]["SecondCharacteristicsValue"].ToString();
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                dicColumnIndex.Add(dvDistinctSecondCharateristicsValues[i]["SecondCharacteristicsValue"].ToString(), COL);
                //sheet[ROW, COL].NumberFormat = "#,##0.00;(#,##0.00)";
                // sheet[ROW, COL].NumberFormat = "#,##0.00;(#,##0.00)";
                //sheet[ROW, COL].NumberFormat =clsStaticInfo.NumberFormat([Precision=);
                // sheet[ROW, COL].CellStyle.Font.Bold = true;

            }

            COL++;
            sheet[ROW, COL].Text = "Total Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            int colTotal = COL;

            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true; //row 19 of heading 
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            StringCollection strCol = new StringCollection();
            int StartRow = ROW; //row 20
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                if (strCol.Contains(dtData.Rows[i]["FirstCharacteristicsValue"].ToString()) == false)
                {
                    strCol.Add(dtData.Rows[i]["FirstCharacteristicsValue"].ToString());

                    sheet[ROW, colFirstChar].Text = dtData.Rows[i]["FirstCharacteristicsValue"].ToString();


                    dtData.DefaultView.RowFilter = "FirstCharacteristicsValue='" + dtData.Rows[i]["FirstCharacteristicsValue"].ToString() + "'";
                    for (int SL = 0; SL < dtData.DefaultView.Count; SL++)
                    {
                        sheet[ROW, dicColumnIndex[dtData.DefaultView[SL]["SecondCharacteristicsValue"].ToString()]].Number = clsStaticInfo.dbl(dtData.DefaultView[SL]["Qty"].ToString());
                        //sheet[ROW, dicColumnIndex[dtData.DefaultView[SL]["SecondCharacteristicsValue"].ToString()]].NumberFormat = "#,##0.00;(#,##0.00)";
                        sheet[ROW, dicColumnIndex[dtData.DefaultView[SL]["SecondCharacteristicsValue"].ToString()]].NumberFormat = clsStaticInfo.NumberFormat();
                    }
                    //int colFirstSecCharValue  = colFirstChar + 1;
                    sheet[ROW, colTotal].Formula = "SUM(" + CellAddr(colFirstSecCharValue, ROW) + ":" + CellAddr(colTotal - 1, ROW) + ")";
                    sheet[ROW, colTotal].NumberFormat = clsStaticInfo.NumberFormat();
                    sheet[ROW, colTotal].CellStyle.Font.Bold = true;


                    ROW++;
                }
            }

            sheet[ROW, colFirstChar].Text = "Total Qty"; //row 21
            sheet[ROW, colFirstChar].CellStyle.Font.Bold = true;
            for (int colSum = colFirstSecCharValue; colSum <= colTotal; colSum++)
            {
                sheet[ROW, colSum].Formula = "SUM(" + CellAddr(colSum, StartRow) + ":" + CellAddr(colSum, ROW - 1) + ")";
                //sheet[ROW, colSum].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, colSum].NumberFormat = clsStaticInfo.NumberFormat();
            }
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            //sheet[ROW, endCol].NumberFormat = "#,##0.00;(#,##0.00)";
            sheet[ROW, endCol].NumberFormat = clsStaticInfo.NumberFormat();

            sheet[StartRow, colFirstChar + 1, ROW, colTotal - 1].NumberFormat = clsStaticInfo.NumberFormat();

            sheet.Range[StartRow - 1, colTotal, ROW, colTotal].CellStyle.Font.Bold = true; //???
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
        }
        void PrintLinearData(DataTable dtData, IWorksheet sheet, ref int ROW)
        {

            if (dtData.Rows.Count == 0)
                return;

            int COL = 0;

            COL++;
            sheet[ROW, COL].Text = dtData.Rows[0]["FirstCharacteristics"].ToString();
            int colFirstChar = COL;
            COL++;
            sheet[ROW, COL].Text = dtData.Rows[0]["SecondCharacteristics"].ToString();
            int colSecondChar = COL;
            COL++;
            sheet[ROW, COL].Text = "Qty";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            // sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true;
            sheet[ROW, COL].CellStyle.Font.Bold = true;
            int colQuantity = COL;


            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            StringCollection strCol = new StringCollection();
            int StartRow = ROW;
            for (int i = 0; i < dtData.Rows.Count; i++)
            {


                sheet[ROW, colFirstChar].Text = dtData.Rows[i]["FirstCharacteristicsValue"].ToString();
                sheet[ROW, colSecondChar].Text = dtData.Rows[i]["SecondCharacteristicsValue"].ToString();
                sheet[ROW, colQuantity].Number = clsStaticInfo.dbl(dtData.Rows[i]["Qty"].ToString());


                ROW++;

            }

            sheet[ROW, colFirstChar].Text = "Total Qty";
            sheet[ROW, colFirstChar].CellStyle.Font.Bold = true;

            sheet[ROW, colQuantity].Formula = "SUM(" + CellAddr(colQuantity, StartRow) + ":" + CellAddr(colQuantity, ROW - 1) + ")";
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            sheet[ROW, colQuantity].NumberFormat = clsStaticInfo.NumberFormat();

            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[StartRow - 1, colQuantity, ROW, colQuantity].CellStyle.Font.Bold = true;
        }

        [HttpPost, Authorize]
        public JsonResult CreateItemDescription(MasterOrderItem data)
        {
            SaveItemDescription(data);
            return Json(new { Message = AplosMessage.Success });
        }

        private void SaveItemDescription(MasterOrderItem data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {

                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;

                    string sql = "SELECT * FROM TRN.MasterOrderItem WHERE Id='" + data.Id + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {

                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["BuyerItemDescription"] = data.BuyerItemDescription;
                        dr["MainRawMaterialDescription"] = data.MainRawMaterialDescription;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.Name;

                        dr.EndEdit();
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetContractPercentage(string masterOrderItemId)
        {
            return Json(_sqlRepository.GetDataCollection(@"Select ISNULL(CF.[Percentage],0) [Percentage] from dbo.[Contract] C
                    LEFT JOIN dbo.ContractFund CF ON CF.ContractId = C.Id AND FundUtilization = 'LessCommission'
                    Where C.Id = (Select ContractId from TRN.MasterOrderItem where Id = '" + masterOrderItemId + "')"), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetPaymentTermChangeable(string CompanyId, string PartyId)
        {
            return Json(_sqlRepository.GetDataCollection(@"Select ISNULL(IsPaymentTermChangeable,0)IsPaymentTermChangeable from [HKP].[CompanyParty] Where PartyId='" + PartyId + "' AND CompanyId='" + CompanyId + "' AND PartyType='Customer'"), JsonRequestBehavior.AllowGet);
        }

        #region Attachment

        [HttpPost, Authorize]
        public ActionResult SaveDefault(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the order first");




                foreach (var file in UploadDefault)
                {

                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.GetMOIDocumentPath(), fileName);

                    var directory = ResourcesPathReader.GetMOIDocumentPath();
                    var path = Path.Combine(directory);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetMOIDocumentPath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetMOIDocumentPath());
                        }
                        catch (Exception)
                        {

                        }
                    }



                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "SELECT * FROM TRN.MasterOrderItem WHERE Id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();
                    var FN = dsLocal.Tables[0].Rows[0]["FileName"].ToString();
                    if (fileN != FN)
                        if (System.IO.File.Exists(path + UploadDefault_data + Path.GetExtension(FN)))
                            System.IO.File.Delete(path + UploadDefault_data + Path.GetExtension(FN));

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["FileName"] = fileN;

                        dsLocal.Tables[0].Rows[0].EndEdit();

                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }



        #endregion

        #region QBOQ
        [HttpGet, Authorize]
        public ActionResult GetAutoSequence(string itemId)
        {
            try
            {
                string sql = @"SELECT (ISNULL((MAX(ISNULL(Sequence,0))),0)+1) Sequence FROM [dbo].[QuickBOQ] Where MasterOrderItemId='" + itemId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetCostingItemCbo()
        {
            try
            {
                var sql = @"Select CI.Id,CI.UserName from [HKP].[CostingItem] CI
                            LEFT JOIN [HKP].[CostingComponent] CC ON CC.Id=CI.CostingComponentId
                            Where CostingSegment='" + CostingSegment.DirectMaterial + "' Order By CI.UserName";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool CheckCombination(Dictionary<string, object> data)
        {
            try
            {

                var _sql = @"SELECT * FROM [dbo].[QuickBOQ] where id<>'" + data["Id"] + "' and ArticleId='" + data["ArticleId"] + "' AND MasterOrderItemId='" + data["MasterOrderItemId"] + "' AND MaterialMasterId='" + data["MaterialMasterId"] + "' AND CostingItemId='" + data["CostingItemId"] + "'";
                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost, Authorize]
        public JsonResult CreateQBOQ(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {
                    var IsDuplicateEntryAllowed = CheckCombination(data);

                    if (IsDuplicateEntryAllowed)
                    {

                        DataSet dsMaster;
                        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                        con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[QuickBOQ] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                        string _Id = "";

                        #region data update
                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("QuickBOQ", out _Id);

                            data["Id"] = "QB" + _Id;
                            AddNewRow(dsMaster.Tables[0], data);
                        }
                        else
                        {
                            _Id = data["Id"].ToString();
                            EditRow(dsMaster.Tables[0].Rows[0], data);
                        }
                        #endregion data update

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);
                    }
                    else
                    {
                        throw new Exception("Selected combination already exists...");
                    }
                }
                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [HttpPost, Authorize]
        public JsonResult EditQBOQ(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {
                    var IsDuplicateEntryAllowed = CheckCombination(data);

                    if (IsDuplicateEntryAllowed)
                    {


                        DataSet dsMaster;
                        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                        con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[QuickBOQ] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                        string _Id = "";

                        #region data update
                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("QuickBOQ", out _Id);

                            data["Id"] = "QB" + _Id;
                            AddNewRow(dsMaster.Tables[0], data);
                        }
                        else
                        {
                            _Id = data["Id"].ToString();
                            EditRow(dsMaster.Tables[0].Rows[0], data);
                        }
                        #endregion data update

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);
                    }
                    else
                    {
                        throw new Exception("Selected combination already exists...");
                    }
                }
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        [HttpGet, Authorize]
        public ActionResult GetQBOQByMasterOrderItem(string itemId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT B.*,MM.UserName MaterialMaster,MMA.ShortName Article,U.Code,C.UserName CostingItem 
                            , EntityOrVendorName= CASE WHEN B.EntityIdWithinCompany<>'' THEN EWC.UserName 
					                        WHEN B.EntityIdWithinGroup<>'' THEN EWG.UserName
					                        WHEN B.VendorId<>'' THEN PRT.UserName
					                        ELSE PRT.UserName END
                                            ,PR.UserName Process
                            FROM [dbo].[QuickBOQ] B
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=B.MaterialMasterId
                            LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=B.ArticleId
                            LEFT JOIN SCS.UnitOfMeasurement U ON U.Id=B.UoMId
                            LEFT JOIN HKP.CostingItem C ON C.Id=B.CostingItemId
                            LEFT JOIN ORG.Entity AS EWC ON B.EntityIdWithinCompany=EWC.Id
                            LEFT JOIN ORG.Entity AS EWG ON B.EntityIdWithinGroup=EWG.Id
                            LEFT JOIN HKP.Party AS PRT ON B.VendorId=PRT.Id
                            LEFT JOIN HKP.Process AS PR ON B.ProcessId=PR.Id
                            WHERE B.MasterOrderItemId='" + itemId + "'";
                return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult DeleteQuickBOQ(string id)
        {
            DeleteQuickBOQData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteQuickBOQData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[QuickBOQ] WHERE Id = '" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function
        #endregion QBOQ

        #region Contract
        [HttpGet, Authorize]
        public ActionResult GetMasterOrderAmountAndQty(string masterId)
        {
            string sql = @"SELECT  SUM(SI.TotalQty) TotalQty, SUM(SO.Amount)Amount,SUM(SO.Qty) Qty
                    FROM [TRN].[MasterOrderItem] AS I
                    inner join [TRN].[MasterOrder] AS A ON A.Id=I.MasterOrderId
                    LEFT JOIN (
                    Select SUM(TotalQty) TotalQty,MasterOrderId,Id FROM [TRN].[MasterOrderItem] Group By MasterOrderId,Id
                    ) SI ON SI.Id=I.Id
                    LEFT JOIN (
                    SELECT SUM(S.Qty) Qty, SUM(S.Qty*S.Rate) Amount, MOI.Id
                    FROM TRN.SalesOrder S
                    LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=S.MasterOrderItemId
                    GROUP BY MOI.Id
                    ) SO ON SO.Id=I.Id
                    WHERE I.MasterOrderId='" + masterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult CreateContract(Dictionary<string, object> model, List<Dictionary<string, object>> funds, List<MasterOrderItem> masterOrderItem)
        {
            try
            {

                SaveData(model, out string contractId, funds, masterOrderItem);

                return Json(new { Contract = model, Id = contractId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }
        private void SaveData(Dictionary<string, object> data, out string contractId, List<Dictionary<string, object>> funds, List<MasterOrderItem> masterOrderItem)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[Contract] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "Contract", out _Id);

                    data["Id"] = "C" + _Id;
                    data["CompanyId"] = identity.CompanyId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["CompanyId"] = identity.CompanyId;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                contractId = dsMaster.Tables[0].Rows[0]["Id"].ToString();


                con.OpenDataSetThroughAdapter("SELECT * FROM TRN.MasterOrderItem WHERE MasterOrderId IN (" + data["MasterOrderId"] + ")", out DataSet dsMasterOrder, false, "1");

                foreach (var item in masterOrderItem)
                {
                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + item.Id + "'";

                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();

                        drmo["ContractId"] = contractId;
                        drmo["UpdatedBy"] = identity.Name;
                        drmo["UpdatedDate"] = DateTime.Now.ToString();
                        drmo["UpdatedFromIP"] = identity.IPAddress;

                        drmo.EndEdit();

                    }

                }



                #region FUND 

                DataSet dsChild;

                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ContractFund where  ContractId='" + contractId + "'", out dsChild, false, "1");
                #region data update

                if (funds != null)
                {
                    foreach (var item in funds)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = GetContractFundPK();
                            item["ContractId"] = contractId;

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsChild, dsMasterOrder);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private string GetContractFundPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ContractFund", out sID);
            return sID;
        }


        #endregion

        #region Copy SO

        [HttpGet, Authorize]
        public ActionResult GetItemMaterialSKUData(string materialMasterId, string sequence)
        {
            string sql = @" SELECT CV.Id AS [Value], CV.UserName AS [Text], CV.CharacteristicsId FROM [HKP].[Characteristics] C
                             LEFT JOIN hkp.CharacteristicsValue CV ON CV.CharacteristicsId=C.Id
                             Where CV.MaterialMasterId='"+ materialMasterId + @"' AND CV.CharacteristicsId 
							 IN (SELECT MMC.CharacteristicsId  FROM [MST].[MaterialMasterCharacteristics] MMC  Where MaterialMasterId='"+ materialMasterId + @"' AND MMC.Sequence="+ sequence + @"
							 ) AND C.ValueAssignmentLevel='Specific' Order by CV.UserName";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetFromItemMaterialSKU1Data(string ItemId)
        {
            string sql = @"SELECT distinct FCH.CharacteristicsValueId, CHV.UserName AS CharacteristicsValueName	                        
                        FROM [TRN].[FirstCharacteristics] AS FCH
                        JOIN [HKP].[Characteristics] AS CH ON FCH.CharacteristicsId=CH.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV ON FCH.CharacteristicsValueId=CHV.Id
                        JOIN [TRN].[SalesOrder] AS SO ON FCH.SalesOrderId=SO.Id
                        JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                        WHERE MOI.Id='" + ItemId + @"' ORDER BY CHV.UserName";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetFromItemMaterialSKU2Data(string ItemId)
        {
            string sql = @"SELECT distinct FCH.CharacteristicsValueId, CHV.UserName AS CharacteristicsValueName
                        FROM [TRN].[SecondCharacteristics] AS FCH
                        JOIN [HKP].[Characteristics] AS CH ON FCH.CharacteristicsId=CH.Id
						LEFT JOIN [HKP].[CharacteristicsValue] AS CHV ON FCH.CharacteristicsValueId=CHV.Id
                        JOIN [TRN].[SalesOrder] AS SO ON FCH.SalesOrderId=SO.Id
                        JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       WHERE MOI.Id='"+ ItemId + "' ORDER BY CHV.UserName";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CopySOByMOI(string MasterId, string masterItemId, List<Dictionary<string, object>> SKU1List, List<Dictionary<string, object>> SKU2List)
        {
            try
            {
                
                CopySalesOrderByMOIData(MasterId,masterItemId, SKU1List, SKU2List);
                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public void CopySalesOrderByMOIData(string MasterId,string masterItemId, List<Dictionary<string, object>> SKU1List, List<Dictionary<string, object>> SKU2List)
        {
            DataSet dsToSalesOrder;
            DataSet dsToFirstCharacteristics;
            DataSet dsToSecondCharacteristics;
            DataSet dsToThirdCharacteristics;
            try
            {
                

                DataSet dsSOId;
                GetSOId(MasterId, out dsSOId);
                string NewId = dsSOId.Tables[0].Rows[0]["Id"].ToString();
                string NewSoId = string.Empty;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SalesOrder] WHERE 1=2", out dsToSalesOrder, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[FirstCharacteristics] WHERE 1=2", out dsToFirstCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SecondCharacteristics] WHERE 1=2", out dsToSecondCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[ThirdCharacteristics] WHERE 1=2", out dsToThirdCharacteristics, false, "1");

                DataTable dtFromMaster = _sqlRepository.GetDataTable("SELECT * FROM [TRN].[SalesOrder] WHERE MasterOrderItemId='" + masterItemId + "'");
                DataTable dtFromFirstCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.FirstCharacteristics Where SalesOrderId IN(Select Id from TRN.SalesOrder Where MasterOrderItemId='"+ masterItemId + "')");
                DataTable dtFromSecondCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.SecondCharacteristics Where SalesOrderId IN(Select Id from TRN.SalesOrder Where MasterOrderItemId='"+ masterItemId + "')");
                DataTable dtFromThirdCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.ThirdCharacteristics Where SalesOrderId IN(Select Id from TRN.SalesOrder Where MasterOrderItemId='"+ masterItemId + "')");

                int SCount = 0;
                for (int m  = 0; m < dtFromMaster.Rows.Count; m++)
                {
                    SCount++;
                    DataRow drSalesOrder = dsToSalesOrder.Tables[0].NewRow();
                    CopyRow(dtFromMaster.Rows[m], ref drSalesOrder);
                    drSalesOrder["Id"] = MasterId+ Convert.ToInt32(NewId) + SCount;
                    NewSoId= drSalesOrder["Id"].ToString();
                    drSalesOrder["MasterOrderItemId"] = MasterId;
                    dsToSalesOrder.Tables[0].Rows.Add(drSalesOrder);

                    dtFromFirstCharacteristics.DefaultView.RowFilter = "SalesOrderId='" + dtFromMaster.Rows[m]["Id"].ToString() + "'";
                    for (int i = 0; i < dtFromFirstCharacteristics.DefaultView.Count; i++)
                    {
                        DataRow drFirstCharacteristics = dsToFirstCharacteristics.Tables[0].NewRow();
                        CopyRow(dtFromFirstCharacteristics.DefaultView[i].Row, ref drFirstCharacteristics);
                        drFirstCharacteristics["Id"] = NewSoId + (i + 1);
                        drFirstCharacteristics["SalesOrderId"] = NewSoId;
                      

                        foreach (var item in SKU1List)
                        {
                            if (drFirstCharacteristics["CharacteristicsValueId"].ToString()==item["CharacteristicsValueId"].ToString())
                            {
                                drFirstCharacteristics["CharacteristicsValueId"] = item["ToSKU1Id"].ToString();
                            }
                            // break;
                        }

                        dsToFirstCharacteristics.Tables[0].Rows.Add(drFirstCharacteristics);

                        dtFromSecondCharacteristics.DefaultView.RowFilter = "SalesOrderId='" + dtFromMaster.Rows[m]["Id"].ToString() + "' AND FirstCharacteristicsId='" + dtFromFirstCharacteristics.DefaultView[i]["Id"] + "'";
                        for (int K = 0; K < dtFromSecondCharacteristics.DefaultView.Count; K++)
                        {
                            DataRow drSecondCharacteristics = dsToSecondCharacteristics.Tables[0].NewRow();
                            CopyRow(dtFromSecondCharacteristics.DefaultView[K].Row, ref drSecondCharacteristics);
                            drSecondCharacteristics["Id"] = NewSoId + (i + 1) + (K + 1);
                            drSecondCharacteristics["SalesOrderId"] = NewSoId;
                            drSecondCharacteristics["FirstCharacteristicsId"] = NewSoId + (i + 1);

                            foreach (var item in SKU2List)
                            {
                                if (drSecondCharacteristics["CharacteristicsValueId"].ToString() == item["CharacteristicsValueId"].ToString())
                                {
                                    drSecondCharacteristics["CharacteristicsValueId"] = item["ToSKU2Id"].ToString();
                                }
                               // break;
                            }
                            dsToSecondCharacteristics.Tables[0].Rows.Add(drSecondCharacteristics);

                            dtFromThirdCharacteristics.DefaultView.RowFilter = "SalesOrderId='" + dtFromMaster.Rows[m]["Id"].ToString() + "' AND SecondCharacteristicsId='" + dtFromSecondCharacteristics.DefaultView[K]["Id"] + "'";
                            for (int j = 0; j < dtFromThirdCharacteristics.DefaultView.Count; j++)
                            {
                                DataRow drThirdCharacteristics = dsToThirdCharacteristics.Tables[0].NewRow();
                                CopyRow(dtFromThirdCharacteristics.DefaultView[j].Row, ref drThirdCharacteristics);
                                drThirdCharacteristics["Id"] = NewSoId + (i + 1) + (j + 1);
                                drThirdCharacteristics["SalesOrderId"] = NewSoId;
                                drThirdCharacteristics["SecondCharacteristicsId"] = NewSoId + (i + 1) + (K + 1);
                                dsToThirdCharacteristics.Tables[0].Rows.Add(drThirdCharacteristics);
                            }
                        }
                    }
                }
               

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsToSalesOrder, dsToFirstCharacteristics, dsToSecondCharacteristics, dsToThirdCharacteristics);


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost,Authorize]
        public JsonResult CopySalesOrder(string MasterId, string masterItemId)
        {
            try
            {
                CopySalesOrderData(MasterId, masterItemId);
                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public void GetSalesOrderId(string masterItemId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT ISNULL((MAX(Id)+1),0) Id FROM [TRN].[SalesOrder] WHERE MasterOrderItemId='" + masterItemId + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetSOId(string masterItemId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT ISNULL((MAX(Id)),0) Id FROM [TRN].[SalesOrder] WHERE MasterOrderItemId='" + masterItemId + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void CopySalesOrderData(string MasterId, string masterItemId)
        {
            DataSet dsToSalesOrder;
            DataSet dsToFirstCharacteristics;
            DataSet dsToSecondCharacteristics;
            DataSet dsToThirdCharacteristics;
            try
            {

                DataSet dsSOId;
                GetSalesOrderId(masterItemId, out dsSOId);
                string NewId = dsSOId.Tables[0].Rows[0]["Id"].ToString();

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SalesOrder] WHERE 1=2", out dsToSalesOrder, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[FirstCharacteristics] WHERE 1=2", out dsToFirstCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SecondCharacteristics] WHERE 1=2", out dsToSecondCharacteristics, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[ThirdCharacteristics] WHERE 1=2", out dsToThirdCharacteristics, false, "1");

                DataTable dtFromMaster = _sqlRepository.GetDataTable("SELECT * FROM [TRN].[SalesOrder] WHERE Id='" + MasterId + "'");
                DataTable dtFromFirstCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.FirstCharacteristics WHERE SalesOrderId='" + MasterId + "'");
                DataTable dtFromSecondCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.SecondCharacteristics WHERE SalesOrderId='" + MasterId + "'");
                DataTable dtFromThirdCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TRN.ThirdCharacteristics WHERE SalesOrderId='" + MasterId + "'");

                DataRow drSalesOrder = dsToSalesOrder.Tables[0].NewRow();
                CopyRow(dtFromMaster.Rows[0], ref drSalesOrder);
                drSalesOrder["Id"] = NewId;
                drSalesOrder["ParentId"] = MasterId;
                dsToSalesOrder.Tables[0].Rows.Add(drSalesOrder);

                for (int i = 0; i < dtFromFirstCharacteristics.Rows.Count; i++)
                {
                    DataRow drFirstCharacteristics = dsToFirstCharacteristics.Tables[0].NewRow();
                    CopyRow(dtFromFirstCharacteristics.Rows[i], ref drFirstCharacteristics);
                    drFirstCharacteristics["Id"] = NewId + (i + 1);
                    drFirstCharacteristics["SalesOrderId"] = NewId;
                    dsToFirstCharacteristics.Tables[0].Rows.Add(drFirstCharacteristics);

                    dtFromSecondCharacteristics.DefaultView.RowFilter = "SalesOrderId='" + MasterId + "' AND FirstCharacteristicsId='" + dtFromFirstCharacteristics.Rows[i]["Id"] + "'";
                    for (int K = 0; K < dtFromSecondCharacteristics.DefaultView.Count; K++)
                    {
                        DataRow drSecondCharacteristics = dsToSecondCharacteristics.Tables[0].NewRow();
                        CopyRow(dtFromSecondCharacteristics.DefaultView[K].Row, ref drSecondCharacteristics);
                        drSecondCharacteristics["Id"] = NewId + (i + 1) + (K + 1);
                        drSecondCharacteristics["SalesOrderId"] = NewId;
                        drSecondCharacteristics["FirstCharacteristicsId"] = NewId + (i + 1);
                        dsToSecondCharacteristics.Tables[0].Rows.Add(drSecondCharacteristics);

                        dtFromThirdCharacteristics.DefaultView.RowFilter = "SalesOrderId='" + MasterId + "' AND SecondCharacteristicsId='" + dtFromSecondCharacteristics.Rows[K]["Id"] + "'";
                        for (int j = 0; j < dtFromThirdCharacteristics.DefaultView.Count; j++)
                        {
                            DataRow drThirdCharacteristics = dsToThirdCharacteristics.Tables[0].NewRow();
                            CopyRow(dtFromThirdCharacteristics.DefaultView[j].Row, ref drThirdCharacteristics);
                            drThirdCharacteristics["Id"] = NewId + (i + 1) + (j + 1);
                            drThirdCharacteristics["SalesOrderId"] = NewId;
                            drThirdCharacteristics["SecondCharacteristicsId"] = NewId + (i + 1) + (K + 1);
                            dsToThirdCharacteristics.Tables[0].Rows.Add(drThirdCharacteristics);
                        }
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsToSalesOrder, dsToFirstCharacteristics, dsToSecondCharacteristics, dsToThirdCharacteristics);


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = drSource[drSource.Table.Columns[COL].ColumnName];

                }
                catch (Exception ex)
                {
                }
                try
                {
                    drDestination["AddedBy"] = identity.Name;
                    drDestination["AddedDate"] = DateTime.Now;
                    drDestination["AddedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedBy"] = identity.Name;
                    drDestination["UpdatedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedDate"] = DateTime.Now;

                }
                catch (Exception ex)
                {
                }
            }

        }
        #endregion
    }
}