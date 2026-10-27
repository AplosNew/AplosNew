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
using Library.Model.Materials;
using Library.Service.Materials;
using Library.OrderManagement.Production;
using Library.Service.Systems;
using Library.Service.TaskScheduler;
using Library.Data;
using System.Reflection;
#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class MasterOrderController : BaseController
    {
        #region -- Constructor
        Library.Planning.OrderManagement.MasterOrder MasterOrder = new Library.Planning.OrderManagement.MasterOrder();
        private readonly string ExchangeRateTableName = "MasterOrderExchangeRates";
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly IMasterOrderService _masterOrderService;
        private readonly IPartyService _partyService;
        private readonly ICustomerPOService _customerPOService;
        private readonly ISqlRepository _sqlRepository;
        private readonly ICharacteristicsValueService _characteristicsValueService;
        private readonly IPKGeneratorService _pkGeneratorService;
        public MasterOrderController(IMasterOrderService masterOrderService, IPartyService partyService, ICustomerPOService customerPOService, ISqlRepository R, ICharacteristicsValueService characteristicsValueService, IPKGeneratorService pkGeneratorService)
        {
            _masterOrderService = masterOrderService;
            _partyService = partyService;
            _customerPOService = customerPOService;
            _sqlRepository = R;
            _characteristicsValueService = characteristicsValueService;
            _pkGeneratorService = pkGeneratorService;
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
        public ActionResult CheckBy()
        {
            return View();
        }

        public ActionResult ApproveBy()
        {
            return View();
        }

        public ActionResult IndependentOrder()
        {
            return View();
        }

        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult SKUUpload()
        {
            return View();
        }

        public ActionResult SKURP()
        {
            return View();
        }

        public ActionResult ReasonMaster()
        {
            return View();
        }


        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetLineItemAdditionalInfoData(string lineItemId)
        {
            return Json(MasterOrder.GetLineItemAdditionalInfoData(lineItemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSOAdditionalInfoData(string SalesOrderId)
        {
            return Json(MasterOrder.GetSOAdditionalInfoData(SalesOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetUoMCboByProductMaster()
        {
            try
            {
                return Json(MasterOrder.GetUoMCboByProductMaster(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetProductionRef(string pg)
        {
            try
            {
                return Json(MasterOrder.GetProductionRef(pg), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetOrderCostingMasterTemplateDataByArticle(string articleId)
        {
            try
            {
                return Json(MasterOrder.GetOrderCostingMasterTemplateDataByArticle(articleId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetProductLibrary(string ArticleId)
        {
            return Json(MasterOrder.GetProductLibrary(ArticleId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetFirstAccountHolder(string buyerId)
        {
            return Json(MasterOrder.GetFirstAccountHolder(buyerId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingSOFormulaData(string masterOrderItemId)
        {
            return Json(MasterOrder.GetCostingSOFormulaData(masterOrderItemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingSORateData(string SalesOrderId, string lineId)
        {
            return Json(MasterOrder.GetCostingSORateData(SalesOrderId, lineId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetItemRateData(string masterOrderItemId)
        {
            return Json(MasterOrder.GetItemRateData(masterOrderItemId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult CalculateRate(IEnumerable<OpenHeadModelNew> OpenHeadNew)
        {
            DataTable dtValue = new DataTable();
            dtValue.TableName = "TempTable";
            dtValue.Columns.Add("OrderLineCostingItemID");
            dtValue.Columns.Add("Amount");
            string sFormulaResult = null;

            DataSet dsOpenHead = Library.Service.Helpers.DataTableExtensions.ToDataSet<OpenHeadModelNew>(OpenHeadNew);
            for (int i = 0; i < dsOpenHead.Tables[0].Rows.Count; i++)
            {
                if (i == 0)
                {
                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["OrderLineCostingItemID"] = dsOpenHead.Tables[0].Rows[i]["OrderLineCostingItemId"].ToString().Trim();
                    dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["Value"].ToString().Trim();

                    dtValue.Rows.Add(dtValueRow);
                }
                else if (i > 0 && string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                {
                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["OrderLineCostingItemID"] = dsOpenHead.Tables[0].Rows[i]["OrderLineCostingItemId"].ToString().Trim();
                    dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["Value"].ToString().Trim();

                    dtValue.Rows.Add(dtValueRow);
                }

                if (!string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                {
                    MasterOrder.ReLoadFormulaWithValue(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString(), ref dtValue, out string _formulaValue);
                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString("##,##0.00");

                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["OrderLineCostingItemID"] = dsOpenHead.Tables[0].Rows[i]["OrderLineCostingItemId"].ToString().Trim();
                    dtValueRow["Amount"] = sFormulaResult;

                    dtValue.Rows.Add(dtValueRow);

                    DataView dv = new DataView(dsOpenHead.Tables[0]);
                    dv.RowFilter = "OrderLineCostingItemId='" + dsOpenHead.Tables[0].Rows[i]["OrderLineCostingItemId"].ToString() + "'";
                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();
                        drmo["Value"] = sFormulaResult;
                        drmo.EndEdit();

                    }


                }


            }


            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dsOpenHead.Tables[0]);
            return Json(new { NewData, Message = AplosMessage.Success });
        }

        [Authorize]
        public JsonResult CalculateSOCost(IEnumerable<SOCostModelNew> OpenHeadNew)
        {
            DataTable dtValue = new DataTable();
            dtValue.TableName = "TempTable";
            dtValue.Columns.Add("OrderLineCostingItemID");
            dtValue.Columns.Add("Amount");
            string sFormulaResult = null;

            DataSet dsOpenHead = Library.Service.Helpers.DataTableExtensions.ToDataSet<SOCostModelNew>(OpenHeadNew);
            for (int i = 0; i < dsOpenHead.Tables[0].Rows.Count; i++)
            {
                if (i == 0)
                {
                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["OrderLineCostingItemID"] = dsOpenHead.Tables[0].Rows[i]["OrderLineCostingItemId"].ToString().Trim();
                    dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["SOValue"].ToString().Trim();

                    dtValue.Rows.Add(dtValueRow);
                }
                else if (i > 0 && string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                {
                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["OrderLineCostingItemID"] = dsOpenHead.Tables[0].Rows[i]["OrderLineCostingItemId"].ToString().Trim();
                    dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["SOValue"].ToString().Trim();

                    dtValue.Rows.Add(dtValueRow);
                }
                if (!string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                {
                    MasterOrder.ReLoadFormulaWithValue(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString(), ref dtValue, out string _formulaValue);
                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString("##,##0.00");

                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["OrderLineCostingItemID"] = dsOpenHead.Tables[0].Rows[i]["OrderLineCostingItemId"].ToString().Trim();
                    dtValueRow["Amount"] = sFormulaResult;

                    dtValue.Rows.Add(dtValueRow);

                    DataView dv = new DataView(dsOpenHead.Tables[0]);
                    dv.RowFilter = "OrderLineCostingItemId='" + dsOpenHead.Tables[0].Rows[i]["OrderLineCostingItemId"].ToString() + "'";
                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();
                        drmo["SOValue"] = sFormulaResult;
                        drmo.EndEdit();

                    }


                }
            }


            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dsOpenHead.Tables[0]);
            return Json(new { NewData, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult CreateMasterOrderItemCostingRate(List<Dictionary<string, object>> data, string lineId)
        {
            try
            {

                SaveMasterOrderItemCostingRateData(data, lineId);


                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private void SaveMasterOrderItemCostingRateData(List<Dictionary<string, object>> data, string lineId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (string.IsNullOrEmpty(lineId))
                {
                    throw new Exception("Select Line Item.");
                }
                #region FUND 
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster = null;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MasterOrderItemCostingRate where  MasterOrderItemId='" + lineId + "'", out dsMaster, false, "1");
                int idc = 0;
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        idc++;
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            string id = _pkGeneratorService.MakePK(lineId, idc, 3);
                            item["Id"] = id;
                            item["MasterOrderItemId"] = lineId;

                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            item["MasterOrderItemId"] = lineId;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);


            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateSOCostingConfirm(List<Dictionary<string, object>> data, string lineId)
        {
            try
            {
                SaveSOCostingConfirmData(data, lineId);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private void SaveSOCostingConfirmData(List<Dictionary<string, object>> data, string lineId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                #region SOCostingConfirm 

                if (string.IsNullOrEmpty(lineId))
                {
                    throw new Exception("Select SO Item.");
                }
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster, dsSO = null;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.SOCostingConfirmation where  SalesOrderId='" + lineId + "'", out dsMaster, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM TRN.SalesOrder where  Id='" + lineId + "'", out dsSO, false, "1");
                int idc = 0;
                decimal upcharge = 0;
                List<SOCostModelNew> soList = new List<SOCostModelNew>();

                //while (dsMaster.Tables[0].DefaultView.Count > 0)
                //    dsMaster.Tables[0].DefaultView[0].Delete();
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        idc++;
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            string id = _pkGeneratorService.MakePK(lineId, idc, 3);
                            item["Id"] = id;
                            item["SalesOrderId"] = lineId;

                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            item["SalesOrderId"] = lineId;
                            EditRow(drmo, item);
                        }
                        if (soList.Count == 0)
                        {
                            var so = new SOCostModelNew
                            {
                                SOItemName = item["SOItemName"].ToString(),
                                SOValue = Convert.ToDecimal(item["SOValue"])

                            };
                            soList.Add(so);
                        }
                        else
                        {
                            var socheck = soList.Where(s => s.SOItemName == item["SOItemName"].ToString()).FirstOrDefault();
                            if (socheck != null)
                            {
                                foreach (var it in soList.Where(s => s.SOItemName == item["SOItemName"].ToString()))
                                {
                                    if (it.SOItemName == item["SOItemName"].ToString())
                                    {
                                        it.SOValue += Convert.ToDecimal(item["SOValue"]);
                                    }
                                }
                            }
                            else
                            {
                                var so = new SOCostModelNew
                                {
                                    SOItemName = item["SOItemName"].ToString(),
                                    SOValue = Convert.ToDecimal(item["SOValue"])
                                };

                                soList.Add(so);
                            }
                        }
                        upcharge += Convert.ToDecimal(item["ValueDiff"]);
                    }
                }

                #endregion

                DataView sodv = new DataView(dsSO.Tables[0]);
                sodv.RowFilter = "Id='" + lineId + "'";
                if (sodv.Count > 0)
                {
                    DataRow drso = sodv[0].Row;

                    drso.BeginEdit();
                    foreach (var so in soList)
                    {
                        if (so.SOItemName == "Rate")
                        {
                            drso["Rate"] = so.SOValue;
                        }
                        if (so.SOItemName == "SalesExpense")
                        {
                            drso["SalesExpense"] = so.SOValue;
                        }
                        if (so.SOItemName == "Discount")
                        {
                            drso["Discount"] = so.SOValue;
                        }
                        if (so.SOItemName == "CM")
                        {
                            drso["CM"] = so.SOValue;
                        }
                        if (so.SOItemName == "DirectMaterialCost")
                        {
                            drso["DirectMaterialCost"] = so.SOValue;
                        }
                        if (so.SOItemName == "DirectProcessCost")
                        {
                            drso["DirectProcessCost"] = so.SOValue;
                        }
                        if (so.SOItemName == "Commission")
                        {
                            drso["Commission"] = so.SOValue;
                        }
                        if (so.SOItemName == "ValueLoss")
                        {
                            drso["ValueLoss"] = so.SOValue;
                        }
                        if (so.SOItemName == "Other")
                        {
                            drso["Other"] = so.SOValue;
                        }
                    }
                    // drso["ContractId"] = cId;

                    drso["UpCharge"] = upcharge;
                    drso["UpdatedBy"] = identity.Name;
                    drso["UpdatedDate"] = DateTime.Now.ToString();
                    drso["UpdatedFromIP"] = identity.IPAddress;

                    drso.EndEdit();

                }



                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsSO);


            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetContractByMasterOrder(string masterId)
        {

            return Json(MasterOrder.GetContractByMasterOrder(masterId), JsonRequestBehavior.AllowGet);
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

        [Authorize, HttpGet]
        public JsonResult GetPreparedEmployeeList(GridParameter parameters, string employeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_masterOrderService.GetPreparedEmployeeList(parameters, identity.PlantId, employeeId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmpListResponsible(GridParameter parameters, string CompanyId, string plantId, string partyAccountGroupId, string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId))
            {
                plantId = identity.PlantId;
            }
            return Json(_masterOrderService.GetEmployeeListResponsible(parameters, CompanyId, plantId, partyAccountGroupId, partyId), JsonRequestBehavior.AllowGet);
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
        public JsonResult GetSOListForCheck(string masterItemId)
        {
            return Json(MasterOrder.GetSOListForCheck(masterItemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSOListForApprove(string masterItemId)
        {
            return Json(MasterOrder.GetSOListForApprove(masterItemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetpackingTypeDataList(string SOId, string PackingType)
        {
            return Json(_masterOrderService.GetpackingTypeList(SOId, PackingType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string companyId)
        {
            return Json(_masterOrderService.Query(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetList(string companyId, string column, string value)
        {
            var jsondata = Json(_masterOrderService.GetList(companyId, column, value), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetCheckByList(string companyId, string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(MasterOrder.GetCheckByList(companyId, column, value, identity.EmployeeId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetApproveList(string companyId, string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(MasterOrder.GetApproveList(companyId, column, value, identity.EmployeeId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
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

        [HttpGet, Authorize]
        public JsonResult GetMasterItemForApproveList(string masterOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_masterOrderService.GetMasterItemForApproveList(masterOrderId, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMasterItemForCheckList(string masterOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(MasterOrder.GetMasterItemForCheckList(masterOrderId, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetItemsData(string masterOrderId)
        {
            return Json(_masterOrderService.GetItemsData(masterOrderId), JsonRequestBehavior.AllowGet);
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
        public JsonResult Create(MasterOrder entity, List<MasterOrderTNA> taskList, List<Dictionary<string, object>> CurrencyData, UserRemarksControl userRemarksControl)
        {
            _masterOrderService.Insert(entity, taskList, userRemarksControl);
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
        public JsonResult Edit(MasterOrder entity, string masterId, IEnumerable<MasterOrderResPerson> personList, IEnumerable<MasterOrderItem> itemList, List<Dictionary<string, object>> CurrencyData, UserRemarksControl userRemarksControl)
        {
            _masterOrderService.Update(entity, masterId, personList, itemList, userRemarksControl);
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

        [HttpPost]
        public JsonResult CreateSalesOrder(string masterItemId, SalesOrderMaster salesOrderMaster, MasterOrder masterorder)
        {
            _masterOrderService.InsertOrUpdateSOGraph(masterItemId, salesOrderMaster, masterorder);
            Library.Planning.OrderManagement.MasterOrder mo = new Library.Planning.OrderManagement.MasterOrder();
            mo.GenerateLogForTnA(masterItemId, Library.Service.Enums.TaskAppliedOnEnum.SalesOrder);

            return Json(new { Data = salesOrderMaster, Message = AplosMessage.Updated });
        }


        [HttpPost, Authorize]
        public JsonResult CreateSplitSalesOrder(string masterItemId, SalesOrderMaster salesOrderMaster, MasterOrder masterOrder)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IdentityParameter para = new IdentityParameter
            {
                AddedBy = identity.Name,
                AddedDate = DateTime.Now,
                AddedFromIP = identity.IPAddress,
                UpdatedBy = identity.Name,
                UpdatedDate = DateTime.Now,
                UpdatedFromIP = identity.IPAddress
            };

            MasterOrder.SplitSalesOrderData(masterItemId, salesOrderMaster, para, masterOrder.Id, masterOrder.TaskTemplateMasterId);
            return Json(new { Message = AplosMessage.Updated + " Please reduce SKU Qty." });
        }

        [HttpPost]
        public JsonResult UpdateSalesOrder(string masterItemId, SalesOrderMaster salesOrderMaster, IEnumerable<SalesOrderTax> taxCategoryList, MasterOrder masterorder)
        {
            _masterOrderService.UpdateSOGraph(masterItemId, salesOrderMaster, taxCategoryList, masterorder);
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

        [HttpPost, Authorize]
        public JsonResult CreateItemDescription(MasterOrderItem data)
        {
            MasterOrder.SaveItemDescription(data);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public JsonResult GetContractPercentage(string masterOrderItemId)
        {
            return Json(MasterOrder.GetContractPercentage(masterOrderItemId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetPaymentTermChangeable(string CompanyId, string PartyId)
        {
            return Json(MasterOrder.GetPaymentTermChangeable(CompanyId, PartyId), JsonRequestBehavior.AllowGet);
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


        [HttpPost, Authorize]
        public JsonResult UpdatePO(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {

                    DataSet dsMaster;
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[CustomerPO] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                    #region data update
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + data["Id"] + "'";

                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();

                        dr["PONumber"] = data["PONumber"];
                        dr["PODate"] = data["PODate"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();

                    }
                    #endregion data update

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);


                }
                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                throw ex;
            }

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
                DataTable dtOrderMaster = MasterOrder.GetOrderMaster(MasterOrderId);
                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                DataTable dtMasterOrderItem = MasterOrder.GetMasterOrderItem(MasterOrderId);
                DataTable dtSalesOrderItem = MasterOrder.GetSalesOrderItem(MasterOrderId);

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

        #endregion

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
                return Json(MasterOrder.GetAutoSequence(itemId), JsonRequestBehavior.AllowGet);
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
                return Json(MasterOrder.GetCostingItemCbo(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateQBOQ(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {
                    var IsDuplicateEntryAllowed = MasterOrder.CheckCombination(data);

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
                    var IsDuplicateEntryAllowed = MasterOrder.CheckCombination(data);

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
                return Json(MasterOrder.GetQBOQByMasterOrderItem(itemId), JsonRequestBehavior.AllowGet);
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


        [HttpGet, Authorize]
        public ActionResult GetMasterOrderAmountAndQty(string masterId)
        {
            return Json(MasterOrder.GetMasterOrderAmountAndQty(masterId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult CreatePackingDetail(Dictionary<string, object> data, string MasterOrderId)
        {
            try
            {
                MasterOrder.SavePackingDetailData(data, MasterOrderId);

                return Json(new { Contract = data, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetPackingDetail(string masterOderId)
        {
            return Json(MasterOrder.GetPackingDetail(masterOderId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult DeletePackingDetail(string PackingDetailId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsPackingDetail;
            try
            {
                string sqlStopage = @"delete from PackingDetail  WHERE Id='" + PackingDetailId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsPackingDetail, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPackingDetailData()
        {
            try
            {
                return Json(MasterOrder.GetPackingDetailData(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetSavedSOData(string PackingDetailId)
        {
            JsonResult json = Json(MasterOrder.GetSavedSOData(PackingDetailId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetSODataList(string masterid)
        {
            try
            {
                return Json(MasterOrder.GetSODataList(masterid), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSOData(string lineItem)
        {
            try
            {
                return Json(MasterOrder.GetSOData(lineItem), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateSOData(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {

                    string _Id;
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("select * from dbo.PackingSODetail where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("PackingSODetail", out _Id);

                        data["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }
                    #endregion data update

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public ActionResult DeleteChildSO(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[PackingSODetail] Where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreatePackingType(Dictionary<string, object> data, List<Dictionary<string, object>> SKUList)
        {
            try
            {
                if (data != null)
                {
                    DataSet dsMaster, dscMaster;

                    string _Id;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("select * from dbo.PackingTypeChild where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("PackingTypeChild", out _Id);

                        data["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }


                    #region FUND 
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.SKUDetail where  PackingTypeChildId='" + data["Id"] + "'", out dscMaster, false, "1");
                    if (SKUList != null)
                    {
                        foreach (var item in SKUList)
                        {
                            DataView dv = new DataView(dscMaster.Tables[0]);
                            dv.RowFilter = "Id='" + item["Id"] + "'";

                            item["PackingTypeChildId"] = _Id;
                            if (dv.Count == 0)
                            {
                                item["Id"] = GetSKUListPK();
                                item["PackingTypeChildId"] = _Id;

                                AddNewRow(dscMaster.Tables[0], item);
                            }
                            else
                            {
                                DataRow drmo = dv[0].Row;
                                EditRow(drmo, item);
                            }
                        }
                    }

                    #endregion



                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dscMaster);
                }
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion

        private string GetSKUListPK()
        {
            string SKUID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SKUDetail", out SKUID);
            return SKUID;
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedPackingType(string PackingDetailId)
        {
            JsonResult json = Json(MasterOrder.GetSavedPackingType(PackingDetailId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost, Authorize]
        public ActionResult DeletePackingType(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery("delete from dbo.SKUDetail where PackingTypeChildId='" + id + "'");
                //con.CommitTransaction();

                con.executeQuery("delete from dbo.PackingTypeChild where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSKU1List(string SOId)
        {
            JsonResult json = Json(MasterOrder.GetSKU1List(SOId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetSKU2List(string SOId)
        {
            JsonResult json = Json(MasterOrder.GetSKU2List(SOId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost, Authorize]
        public JsonResult CreateSKUDetail(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {

                    string _Id;
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("select * from dbo.SKUDetail where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("SKUDetail", out _Id);

                        data["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }
                    #endregion data update

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedSKUDetail(string PackingTypeId)
        {
            JsonResult json = Json(MasterOrder.GetSavedSKUDetail(PackingTypeId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedPackingTypeChild(string PTId)
        {
            JsonResult json = Json(MasterOrder.GetSavedPackingTypeChild(PTId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpPost, Authorize]
        public ActionResult DeleteSKUDetail(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.SKUDetail where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Copy SO

        [HttpGet, Authorize]
        public ActionResult GetItemMaterialSKUData(string materialMasterId, string sequence)
        {
            return Json(MasterOrder.GetItemMaterialSKUData(materialMasterId, sequence), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetFromItemMaterialSKU1Data(string ItemId)
        {
            return Json(MasterOrder.GetFromItemMaterialSKU1Data(ItemId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetFromItemMaterialSKU2Data(string ItemId)
        {
            return Json(MasterOrder.GetFromItemMaterialSKU2Data(ItemId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CopySOByMOI(string MasterId, string masterItemId, List<Dictionary<string, object>> SKU1List, List<Dictionary<string, object>> SKU2List)
        {
            try
            {

                MasterOrder.CopySalesOrderByMOIData(MasterId, masterItemId, SKU1List, SKU2List);
                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpPost, Authorize]
        public JsonResult CopySalesOrder(string MasterId, string masterItemId, decimal TotalMOIQty)
        {
            try
            {
                MasterOrder.CopySalesOrderData(MasterId, masterItemId, TotalMOIQty);
                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpPost, Authorize]
        public ActionResult SODataReport(string masterOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();

                string fileName = "";

                fileName = MasterOrder.CreateSODataReportSheet(masterOrderId);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost, Authorize]
        public ActionResult SODataDetailReport(string masterOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();

                string fileName = "";

                fileName = MasterOrder.CreateSODataDetailReportSheet(masterOrderId);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }

        }
        #endregion

        [HttpPost, Authorize]
        public JsonResult CreateCharacteristicsValue(CharacteristicsValue entity, string MaterialMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.CompanyGroupId = identity.CompanyGroupId;
                _characteristicsValueService.InsertBOMSKU(entity);

                return Json(new { CharacteristicsValue = entity, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpGet]
        public JsonResult GetApproveByCboList()
        {
            return Json(MasterOrder.GetApproveByCboList(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCompanyCboList(string companyGroupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(companyGroupId))
            {
                companyGroupId = identity.CompanyGroupId;
            }
            return Json(MasterOrder.GetCompanyCboList(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CheckSalesOrder(SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.CheckSOGraph(salesOrderMaster);

            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult ApproveSalesOrder(MasterOrder entity, SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.ApproveSOGraph(entity, salesOrderMaster);

            return Json(new { Message = AplosMessage.Updated });
        }


        [HttpPost, Authorize]
        public JsonResult CreateMOAdditionalInfo(List<Dictionary<string, object>> data, string lineId)
        {
            try
            {
                int count = 0;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild, dsId;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[SalesAdditionalInfo] where  LineItemId='" + lineId + "'", out dsChild, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT Count(Id)IdC FROM [dbo].[SalesAdditionalInfo] where  LineItemId='" + lineId + "'", out dsId, false, "1");
                if (dsId.Tables[0].Rows.Count > 0)
                {
                    count = Convert.ToInt32(dsId.Tables[0].Rows[0]["IdC"].ToString());
                }
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            count++;
                            item["Id"] = lineId + "-" + count;
                            item["LineItemId"] = lineId;
                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsChild);
                }


                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateSOAdditionalInfo(List<Dictionary<string, object>> data, string SOId)
        {
            try
            {
                int count = 0;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild, dsId;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[SalesAdditionalInfo] where  SalesOrderId='" + SOId + "'", out dsChild, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT Count(Id)IdC FROM [dbo].[SalesAdditionalInfo] where  SalesOrderId='" + SOId + "'", out dsId, false, "1");
                if (dsId.Tables[0].Rows.Count > 0)
                {
                    count = Convert.ToInt32(dsId.Tables[0].Rows[0]["IdC"].ToString());
                }
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            count++;
                            item["Id"] = SOId + "-" + count;
                            item["SalesOrderId"] = SOId;
                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsChild);
                }


                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetMOSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = MasterOrder.GetMOSampleFile(identity.Name);
            var reportFileName = "Master Order Upload Template";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        }

        [HttpPost, Authorize]
        public JsonResult ImportMOData(FormCollection form)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<MasterOrder> data = new List<MasterOrder>();

                var file = Request.Files["file"];

                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {

                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                else
                {
                    throw new CustomException(Resources.ExcelUploadError);
                }
                string path = "";
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
                FileInfo docFile;
                string exception = "\r\n";
                try
                {
                    try
                    {
                        string connString = string.Empty;
                        ExcelEngine excelEngine = null;
                        IApplication application = null;
                        IWorkbook workbook = null;

                        excelEngine = new ExcelEngine();
                        application = excelEngine.Excel;
                        workbook = excelEngine.Excel.Workbooks.Open(path);

                        DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                        DataSet dsExcel = new DataSet();
                        dsExcel.Tables.Add(dt);


                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            exception += "\r\nTrying to delete";
                            docFile.Delete();
                        }

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                Library.Model.OrderManagements.MasterOrder vm = new Library.Model.OrderManagements.MasterOrder();

                                vm.CompanyId = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.PlantId = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                vm.EntityId = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                                vm.OrderType = dsExcel.Tables[0].Rows[i][3].ToString().Trim();
                                vm.PartyId = dsExcel.Tables[0].Rows[i][4].ToString().Trim();
                                vm.BuyerId = dsExcel.Tables[0].Rows[i][5].ToString().Trim();
                                vm.BuyerBrandId = dsExcel.Tables[0].Rows[i][6].ToString().Trim();
                                vm.BuyerDivisionId = dsExcel.Tables[0].Rows[i][7].ToString().Trim();
                                vm.OrderStatusId = dsExcel.Tables[0].Rows[i][8].ToString();
                                vm.OrderCategoryId = dsExcel.Tables[0].Rows[i][9].ToString();
                                vm.SeasonId = dsExcel.Tables[0].Rows[i][10].ToString();
                                vm.OrderYear = dsExcel.Tables[0].Rows[i][11].ToString();
                                vm.CurrencyId = dsExcel.Tables[0].Rows[i][12].ToString();
                                vm.TotalQty = Convert.ToDecimal(dsExcel.Tables[0].Rows[i][13].ToString());
                                vm.NoOfLineItem = Convert.ToInt32(dsExcel.Tables[0].Rows[i][14].ToString());
                                vm.ResponsiblePersonId = dsExcel.Tables[0].Rows[i][15].ToString();
                                vm.Remarks = dsExcel.Tables[0].Rows[i][16].ToString();
                                vm.OrderWastagePercentage = Convert.ToDecimal(dsExcel.Tables[0].Rows[i][17].ToString());
                                vm.ExtraOrderPercentage = Convert.ToDecimal(dsExcel.Tables[0].Rows[i][18].ToString());
                                vm.TotalQtyUOMId = dsExcel.Tables[0].Rows[i][19].ToString();
                                vm.Type = dsExcel.Tables[0].Rows[i][20].ToString();
                                vm.BuyerDepartmentId = dsExcel.Tables[0].Rows[i][21].ToString();
                                vm.BuyerReferenceNo = dsExcel.Tables[0].Rows[i][22].ToString();
                                vm.OwnReferenceNo = dsExcel.Tables[0].Rows[i][23].ToString();
                                data.Add(vm);

                            }
                        }
                        else
                        {
                            throw new Exception("Please Select File");
                        }
                    }
                    catch (Exception ex)
                    {

                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            docFile.Delete();
                        }
                        throw (ex);
                    }

                }
                catch (Exception ex)
                {
                    //throw ex;
                }
                finally
                {
                }
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public Dictionary<string, object> GetTaskTemplateMasterId(string buyerId, string buyerDivisionId, string buyerDepartmentId)
        {
            try
            {
                var sql = "SELECT BM.TaskTemplateMasterId FROM mst.BuyerMaster AS  BM WHERE bm.BuyerId='" + buyerId + "' AND isnull(bm.BuyerDepartmentId,'" + buyerDepartmentId + "')='" + buyerDepartmentId + "' AND isnull(bm.BuyerDivisionId,'" + buyerDivisionId + "')='" + buyerDivisionId + "'";
                return _sqlRepository.GetData(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetPartyPlantPaymentTerm(string partyId)
        {
            try
            {
                string sql = @"SELECT PP.Id AS [Value], PP.UserName AS [Text], AM.Address1, CP.PaymentTermId,CP.NoOfDay
FROM [HKP].[PartyPlant] AS PP
LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PP.AddressMasterId
LEFT JOIN (Select top 1 CP.PaymentTermId,CP.PartyId,PD.NoOfDay from [HKP].[CompanyParty] AS CP 
left join [MST].[PaymentTermDetail] PD ON PD.PaymentTermId=CP.PaymentTermId
Where CP.PartyId='"+ partyId + @"' and PD.NoOfDay<>0)CP ON CP.PartyId=PP.PartyId
WHERE PP.PartyId='" + partyId + @"' ORDER BY 2";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult SaveMOData(IEnumerable<MasterOrder> dataList)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string _Id = "";
                string sql = "";


                sql = "SELECT * FROM TRN.MasterOrder WHERE 1=2";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                foreach (var entity in dataList)
                {

                    if (!string.IsNullOrEmpty(entity.BuyerId))
                    {
                        var taskTemplateMasterId = GetTaskTemplateMasterId(entity.BuyerId, entity.BuyerDivisionId, entity.BuyerDepartmentId);
                        if (taskTemplateMasterId.Count > 0)
                        {
                            entity.TaskTemplateMasterId = taskTemplateMasterId["TaskTemplateMasterId"].ToString();
                        }

                    }

                    var PartyPlantPaymentTerm = GetPartyPlantPaymentTerm(entity.PartyId);
                    if (PartyPlantPaymentTerm.Rows.Count > 0)
                    {
                        entity.InvoicingPartyPlantId = PartyPlantPaymentTerm.Rows[0]["Value"].ToString();
                        entity.DeliveryPartyPlantId = PartyPlantPaymentTerm.Rows[0]["Value"].ToString();
                        entity.DeliveryByAddress = PartyPlantPaymentTerm.Rows[0]["Address1"].ToString();
                        entity.InvoicingByAddress = PartyPlantPaymentTerm.Rows[0]["Address1"].ToString();
                        entity.PaymentTermId = PartyPlantPaymentTerm.Rows[0]["PaymentTermId"].ToString();
                        entity.PaymentTermDays =Convert.ToInt32(PartyPlantPaymentTerm.Rows[0]["NoOfDay"].ToString());
                        entity.BaseOnDueDate = DateTime.Now;
                        entity.MatureDate =Convert.ToDateTime(entity.BaseOnDueDate).AddDays(entity.PaymentTermDays);
                    }

                    entity.MasterOrderNo = entity.Id;
                    if (entity.BuyerDepartmentId == "ALL")
                    {
                        entity.BuyerDepartmentId = null;
                    }
                    if (entity.BuyerDivisionId == "ALL")
                    {
                        entity.BuyerDivisionId = null;
                    }
                    if (entity.BuyerBrandId == "NULL")
                    {
                        entity.BuyerBrandId = null;
                    }


                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + entity.Id + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(MasterOrder), out _Id);

                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["Id"] = _Id;
                        dr["CompanyId"] = entity.CompanyId;
                        dr["PlantId"] = entity.PlantId;
                        dr["EntityId"] = entity.EntityId;
                        dr["OrderType"] = entity.OrderType;
                        dr["PartyId"] = entity.PartyId;
                        dr["BuyerId"] = entity.BuyerId;
                        dr["BuyerBrandId"] =  entity.BuyerBrandId;
                        dr["BuyerDivisionId"] = entity.BuyerDivisionId;
                        dr["MasterOrderNo"] = _Id;
                        dr["OrderStatusId"] = entity.OrderStatusId;
                        dr["OrderCategoryId"] = entity.OrderCategoryId;
                        dr["SeasonId"] = entity.SeasonId;
                        dr["OrderYear"] = entity.OrderYear;
                        dr["CurrencyId"] = entity.CurrencyId;
                        dr["TotalQty"] = entity.TotalQty;
                        dr["NoOfLineItem"] = entity.NoOfLineItem;
                        dr["ResponsiblePersonId"] = entity.ResponsiblePersonId;
                        dr["Remarks"] = entity.Remarks;
                        dr["OrderWastagePercentage"] = entity.OrderWastagePercentage;
                        dr["ExtraOrderPercentage"] = entity.ExtraOrderPercentage;
                        dr["TotalQtyUOMId"] = entity.TotalQtyUOMId;
                        dr["InvoicingPartyPlantId"] = entity.InvoicingPartyPlantId;
                        dr["DeliveryPartyPlantId"] = entity.DeliveryPartyPlantId;
                        dr["InvoicingByAddress"] = entity.InvoicingByAddress;
                        dr["DeliveryByAddress"] = entity.DeliveryByAddress;
                        dr["TestingStandardId"] = entity.TestingStandardId;
                        dr["IsReplacement"] = entity.IsReplacement;
                        dr["Type"] = entity.Type;
                        dr["SpecialTaxId"] = entity.SpecialTaxId;
                        dr["IsExtraOrderPercentage"] = entity.IsExtraOrderPercentage;
                        dr["BuyerDepartmentId"] = entity.BuyerDepartmentId;
                        dr["TaskTemplateMasterId"] = entity.TaskTemplateMasterId;
                        dr["BuyerReferenceNo"] = entity.BuyerReferenceNo;
                        dr["OwnReferenceNo"] = entity.OwnReferenceNo;
                        dr["PaymentTermId"] = entity.PaymentTermId;
                        dr["PaymentTermDays"] = entity.PaymentTermDays;
                        dr["ExceptionalProcessId"] = entity.ExceptionalProcessId;
                        dr["ExceptionalSubProcessId"] = entity.ExceptionalSubProcessId;
                        dr["BaseOnDueDate"] = entity.BaseOnDueDate;
                        dr["MatureDate"] = entity.MatureDate;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                }
                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetMOISampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = MasterOrder.GetMOISampleFile(identity.Name);
            var reportFileName = "Master Order Item Upload Template";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        }

        [HttpPost, Authorize]
        public JsonResult ImportMOIData(FormCollection form)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<MasterOrderItem> data = new List<MasterOrderItem>();

                var file = Request.Files["file"];

                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {

                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                else
                {
                    throw new CustomException(Resources.ExcelUploadError);
                }
                string path = "";
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
                FileInfo docFile;
                string exception = "\r\n";
                try
                {
                    try
                    {
                        string connString = string.Empty;
                        ExcelEngine excelEngine = null;
                        IApplication application = null;
                        IWorkbook workbook = null;

                        excelEngine = new ExcelEngine();
                        application = excelEngine.Excel;
                        workbook = excelEngine.Excel.Workbooks.Open(path);

                        DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                        DataSet dsExcel = new DataSet();
                        dsExcel.Tables.Add(dt);


                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            exception += "\r\nTrying to delete";
                            docFile.Delete();
                        }

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                Library.Model.OrderManagements.MasterOrderItem vm = new Library.Model.OrderManagements.MasterOrderItem();

                                vm.MaterialMasterId = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.ArticleId = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                vm.BuyerReferenceNo = dsExcel.Tables[0].Rows[i][2].ToString();
                                vm.OwnReferenceNo = dsExcel.Tables[0].Rows[i][3].ToString();                               
                                vm.TotalQty = Convert.ToDecimal(dsExcel.Tables[0].Rows[i][4].ToString());
                                vm.OrderWastagePercentage = Convert.ToDecimal(dsExcel.Tables[0].Rows[i][5].ToString());
                                vm.ExtraOrderPercentage = Convert.ToDecimal(dsExcel.Tables[0].Rows[i][6].ToString());
                                vm.Type = dsExcel.Tables[0].Rows[i][7].ToString();
                                vm.ProductionGrouping = dsExcel.Tables[0].Rows[i][8].ToString();
                                vm.UOMId = dsExcel.Tables[0].Rows[i][9].ToString();
                                data.Add(vm);

                            }
                        }
                        else
                        {
                            throw new Exception("Please Select File");
                        }
                    }
                    catch (Exception ex)
                    {

                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            docFile.Delete();
                        }
                        throw (ex);
                    }

                }
                catch (Exception ex)
                {
                    //throw ex;
                }
                finally
                {
                }
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpPost]
        public JsonResult SaveMOIData(IEnumerable<MasterOrderItem> dataList, string masterId)
        {
            _masterOrderService.SaveMOIData(dataList, masterId);
            return Json(new { Message = AplosMessage.Insert });
        }


        [HttpGet, Authorize]
        public ActionResult GetSOSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = MasterOrder.GetSOSampleFile(identity.Name);
            var reportFileName = "Sales Order Upload Template";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        }

        [HttpPost, Authorize]
        public JsonResult ImportSOData(FormCollection form)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<SalesOrderMaster> data = new List<SalesOrderMaster>();

                var file = Request.Files["file"];

                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {

                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                else
                {
                    throw new CustomException(Resources.ExcelUploadError);
                }
                string path = "";
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
                FileInfo docFile;
                string exception = "\r\n";
                try
                {
                    try
                    {
                        string connString = string.Empty;
                        ExcelEngine excelEngine = null;
                        IApplication application = null;
                        IWorkbook workbook = null;

                        excelEngine = new ExcelEngine();
                        application = excelEngine.Excel;
                        workbook = excelEngine.Excel.Workbooks.Open(path);

                        DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                        DataSet dsExcel = new DataSet();
                        dsExcel.Tables.Add(dt);


                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            exception += "\r\nTrying to delete";
                            docFile.Delete();
                        }

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                SalesOrderMaster vm = new SalesOrderMaster();

                                vm.OrderCategoryId = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.DeliveryDate =Convert.ToDateTime(dsExcel.Tables[0].Rows[i][1].ToString().Trim());
                                vm.ShipmentModeId = dsExcel.Tables[0].Rows[i][2].ToString();
                                vm.DestinationId = dsExcel.Tables[0].Rows[i][3].ToString();
                                vm.Qty = Convert.ToDecimal(dsExcel.Tables[0].Rows[i][4].ToString());
                                vm.PackingTypeId = dsExcel.Tables[0].Rows[i][5].ToString();
                                vm.ResponsiblePersonId = dsExcel.Tables[0].Rows[i][6].ToString();
                                vm.LSD = Convert.ToDateTime(dsExcel.Tables[0].Rows[i][7].ToString());
                                vm.MainRawMaterialInhouseDate = Convert.ToDateTime(dsExcel.Tables[0].Rows[i][8].ToString());
                                vm.OtherRawMaterialInhouseDate = Convert.ToDateTime(dsExcel.Tables[0].Rows[i][8].ToString());
                                data.Add(vm);

                            }
                        }
                        else
                        {
                            throw new Exception("Please Select File");
                        }
                    }
                    catch (Exception ex)
                    {

                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            docFile.Delete();
                        }
                        throw (ex);
                    }

                }
                catch (Exception ex)
                {
                    //throw ex;
                }
                finally
                {
                }
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpPost]
        public JsonResult SaveSOData(IEnumerable<SalesOrderMaster> dataList, string masterId)
        {
            _masterOrderService.SaveSOData(dataList, masterId);
            return Json(new { Message = AplosMessage.Insert });
        }

        private List<CharValue> GetCharacteristicValues(int characteristicsId)
        {
            var list = new List<CharValue>();
         
             string sql = @"SELECT Id, ShortName FROM HKP.CharacteristicsValue Where Active = 1 AND Archive = 0";

            list = ConvertDataTable<CharValue>(_sqlRepository.GetDataTable(sql));
            return list;
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            try
            {
                var data = new List<T>();
                foreach (DataRow row in dt.Rows)
                {
                    var item = GetItem<T>(row);
                    data.Add(item);
                }
                return data;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static T _GetItem<T>(DataRow dr)
        {
            var temp = typeof(T);
            var obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        if (dr[column.ColumnName] == DBNull.Value)
                            dr[column.ColumnName] = "";
                        pro.SetValue(obj, dr[column.ColumnName], null);
                        break;
                    }
                }
            }
            return obj;
        }

        private static T GetItem<T>(DataRow dr)
        {
            var temp = typeof(T);
            var obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                var pro = temp.GetProperty(column.ColumnName);
                if (pro == null || !pro.CanWrite) continue;

                object raw = dr[column.ColumnName];
                Type targetType = Nullable.GetUnderlyingType(pro.PropertyType) ?? pro.PropertyType;

                if (raw == DBNull.Value || raw == null || (raw is string s && s == "" && targetType != typeof(string)))
                {
                    // leave default value (0 / null / "")
                    if (targetType == typeof(string)) pro.SetValue(obj, "", null);
                    continue;
                }

                pro.SetValue(obj, Convert.ChangeType(raw, targetType), null);
            }
            return obj;
        }

        [HttpGet]
        public ActionResult DownloadTemplate(string salesOrderId)
        {
            var sizeColumns = GetCharacteristicValues(characteristicsId: 2); // sizes

            using (var engine = new ExcelEngine())
            {
                IApplication app = engine.Excel;
                app.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = app.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];
                sheet.Name = "SKU Upload";

                sheet.Range["A1"].Text = "FG Colour";
                sheet.Range["A1"].CellStyle.Font.Bold = true;

                int col = 2;
                foreach (var size in sizeColumns)
                {
                    var cell = sheet.Range[1, col];
                    cell.Text = size.Code;           // e.g. "S", "M", "L"...
                    cell.CellStyle.Font.Bold = true;
                    col++;
                }

                // One example/empty row so the user sees the expected shape.
                sheet.Range["A2"].Text = string.Empty; // e.g. "FRUIT DOMAIN"
                sheet.UsedRange.AutofitColumns();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"SKU_Template_{salesOrderId}.xlsx");
                }
            }
        }

        #region SKUResPerson


        [HttpGet, Authorize]
        public ActionResult GetSKUResPersonList()
        {
            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            var jsondata = Json(order.GetSKUEmployee(), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost]
        public JsonResult UpdateSKUResPerson(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.SKUResponsiblePerson where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                   
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult CreateSKUResPerson(List<Dictionary<string, object>> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsEntity;
            try
            {

                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.SKUResponsiblePerson", out dsEntity, false, "1");
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsEntity.Tables[0]);
                        dv.RowFilter = "Id='" + Convert.ToInt64(item["Id"]) + "' AND EmpSystemId='"+item["EmpSystemId"] + "'";

                        if (dv.Count == 0)
                        {
                            AddNewRow(dsEntity.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsEntity);

                return Json(new { Error = false, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteSKUResPerson(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.SKUResponsiblePerson where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        //[HttpPost, Authorize]
        //public JsonResult SavePackerEmployee(List<Dictionary<string, object>> data, string masterId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    ConnectionManager.DAL.ConManager objCon;
        //    DataSet dsEntity;
        //    try
        //    {

        //        #region Entity 
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.PackerCategoryEmployee where  PackerCategoryId='" + masterId + "'", out dsEntity, false, "1");
        //        if (data != null)
        //        {
        //            foreach (var item in data)
        //            {
        //                DataView dv = new DataView(dsEntity.Tables[0]);
        //                dv.RowFilter = "Id='" + Convert.ToInt64(item["Id"]) + "'";

        //                if (dv.Count == 0)
        //                {
        //                    AddNewRow(dsEntity.Tables[0], item);
        //                }
        //                else
        //                {
        //                    DataRow drmo = dv[0].Row;
        //                    EditRow(drmo, item);
        //                }
        //            }
        //        }

        //        #endregion

        //        clsStaticInfo obj = new clsStaticInfo();
        //        obj.SaveDataSets(dsEntity);

        //        return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });

        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Error = true, Message = ex.Message });
        //    }
        //}

        //[HttpGet, Authorize]
        //public ActionResult GetPackerEmployee(string masterId)
        //{
        //    Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
        //    var jsondata = Json(order.GetPackerEmployee(masterId), JsonRequestBehavior.AllowGet);
        //    jsondata.MaxJsonLength = int.MaxValue;
        //    return jsondata;
        //}

        //[HttpPost]
        //public ActionResult DeleteEmployee(string id)
        //{

        //    try
        //    {

        //        if (string.IsNullOrEmpty(id))
        //            throw new Exception("Select entry first");

        //        ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
        //        con.BeginTransaction();
        //        con.executeQuery("delete from [dbo].[PackerCategoryEmployee] where Id='" + id + "'");
        //        con.CommitTransaction();

        //        return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

        //    }
        //}


        [HttpGet, Authorize]
        public ActionResult getPOFilters()
        {
            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            JsonResult json = Json(order.Productionfilters(), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [Authorize, HttpPost]
        public ActionResult GetSalesOrder(Dictionary<string, string> parameters)
        {

            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            JsonResult json = Json(order.GetSalesOrder(parameters), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [Authorize, HttpPost]
        public ActionResult GetSKUData(string salesOrderId)
        {

            Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
            JsonResult json = Json(order.GetSKUData(salesOrderId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetRP1Data()
        {
            try
            {
                Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
                return Json(order.GetResponsiblePerson1Cbo(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetRP2Data()
        {
            try
            {
                Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
                return Json(order.GetResponsiblePerson2Cbo(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetRP3Data()
        {
            try
            {
                Library.OrderManagement.Production.ProductionOrder order = new Library.OrderManagement.Production.ProductionOrder();
                return Json(order.GetResponsiblePerson3Cbo(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        public JsonResult CreateSKUControl(List<Dictionary<string, object>> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsEntity;
            try
            {

                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.SKUControl", out dsEntity, false, "1");
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsEntity.Tables[0]);
                        dv.RowFilter = "Id='" + Convert.ToInt64(item["Id"]) + "'";

                        if (dv.Count == 0)
                        {
                            AddNewRow(dsEntity.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsEntity);

                return Json(new { Error = false, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        #endregion

        #region Reason Master


        [HttpPost, Authorize]
        public ActionResult GetReasonMasterList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT S.*,E.EmployeeName ResponsiblePersonName,P.UserName Process
FROM dbo.ReasonMaster S
LEFT JOIN HKP.Process P ON P.Id=S.ProcessId
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=S.ResponsiblePersonId) AS TEMP WHERE " + strkey + "";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CreateReasonMaster(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.ReasonMaster where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("ReasonMaster", out _Id);

                    data["Id"] = _Id;
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

                return Json(new { Error = false, Data = data, Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult DeleteReasonMaster(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.ReasonMaster where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }



        #endregion

        #region SKU Upload
        [Authorize, HttpGet]
        public ActionResult GetSalesOrderListSearch(string column, string value, string moentity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            string Entity = "'" + moentity.Replace(",", "','") + "'";//replaced with ""

            string activeStatus = "";
            string plantSql = @"select * from scs.PlantConfig where plantid='" + identity.PlantId + "'";
            DataTable dtPlantConfig = _sqlRepository.GetDataTable(plantSql);
            if (dtPlantConfig.Rows.Count > 0)
                if (bplib.clsWebLib.GetBoolData(dtPlantConfig.Rows[0]["IsProductionOrderCreatedAfterConfirmationOfSO"].ToString()))
                    activeStatus = " AND isnull(SO.IsConfirm,0)=1 ";


            string sql = @"SELECT 
                             MO.Type,isnull(moi.Consignment,0) AS Consignment,
                             CASE WHEN ISNULL(eout.Id,'')<>'' OR ISNULL(TOUT.Id,'')<>'' THEN CONCAT(POWN.UserName,'(',EOWN.UserName,')') ELSE '' END AS OrderOwner
                            ,TEMP.* FROM (SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,0 AS Checked,null AS Id,null AS ProductionOrderId
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,moi.BuyerReferenceNo,moi.OwnReferenceNo,mo.BuyerReferenceNo BuyerOrderNo,mo.OwnReferenceNo AS OwnOrderNo
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping 
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName,PM.UserName AS ProductName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , isnull(PO.PONumber,'') AS PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description,CASE WHEN isnull(so.WeekNo,0)=0 THEN  DATEPART(week,so.DeliveryDate) ELSE so.WeekNo END AS DeliveryWeek
	                            , Flag = CAST(0 AS BIT),SO.DestinationDescription
                       FROM [TRN].[SalesOrder] AS SO 
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                       LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)

                       WHERE   
                       (
                            --if there is no jobwork, i can create my own production order
                       	    (ISNULL(moi.JobWorkType,'')='' AND MO.PlantId='" + identity.PlantId + @"' )
                                OR 
                       	    (ISNULL(moi.JobWorkType,'')<>'' AND EOUT.PlantId='" + identity.PlantId + @"' )
                       )
                       AND (OS.Id='" + Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"' " + activeStatus + @" and  SO.Id not IN (SELECT DISTINCT SalesOrderId FROM [TRN].[ProductionOrderDetail])) AND MOI.ArticleId<>''
                        
						UNION
						
						SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,0 AS Checked,POD.Id,POD.ProductionOrderId
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,moi.BuyerReferenceNo,moi.OwnReferenceNo,mo.BuyerReferenceNo BuyerOrderNo,mo.OwnReferenceNo AS OwnOrderNo
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName,PM.UserName AS ProductName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , isnull(PO.PONumber,'') AS PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description,CASE WHEN isnull(so.WeekNo,0)=0 THEN  DATEPART(week,so.DeliveryDate) ELSE so.WeekNo END AS DeliveryWeek
	                            , Flag = CAST(0 AS BIT),SO.DestinationDescription
                       FROM [TRN].[SalesOrder] AS SO 
                        left outer join [TRN].[ProductionOrderDetail] POD on POD.SalesOrderId=SO.Id 
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                       --WHERE  SO.Id IN (SELECT DISTINCT SalesOrderId FROM [TRN].[ProductionOrderDetail] WHERE ProductionOrderID='')
                        
						
						
						) AS TEMP 

                            LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON TEMP.MasterOrderItemId=MOI.Id
                            LEFT JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
							LEFT JOIN org.Entity AS EOUT ON EOUT.Id=ISNULL(moi.EntityIdWithinCompany,moi.EntityIdWithinGroup)
							LEFT JOIN org.Plant AS POUT ON POUT.Id=EOUT.PlantId
							LEFT JOIN hkp.Party AS TOUT ON tout.Id=moi.PartyId
							LEFT JOIN org.Plant AS POWN ON POWN.Id=MO.PlantId
							LEFT JOIN org.Entity AS EOWN ON EOWN.Id=MO.EntityId

WHERE " + strkey + "  and MO.PlantId='" + identity.PlantId + @"' AND MO.EntityId IN (" + Entity + @")  ORDER BY  TEMP.ProductionGrouping,TEMP.MaterialMasterId,TEMP.ArticleId";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSKUSampleFile(ReportFormat reportFormat, string salesOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = GetWorkBookSKUSampleFile(identity.Name, salesOrderId);
            var reportFileName = "SKU Upload Sample File";

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }
        private void GetSKU1(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT CV.UserName+'_#'+CV.Id Text FROM [HKP].[Characteristics] C 
                            LEFT JOIN [HKP].[CharacteristicsValue] CV on CV.CharacteristicsId=C.Id
                            WHERE CV.CharacteristicsId=1";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        private void GetSKU2(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT CV.UserName+'_#'+CV.Id Text FROM [HKP].[Characteristics] C 
                            LEFT JOIN [HKP].[CharacteristicsValue] CV on CV.CharacteristicsId=C.Id
                            WHERE CV.CharacteristicsId=2";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        public void CreateSource(DataSet ds, int Col, string Header, ref IWorksheet sheetSource)
        {
            try
            {
                ReportUtility ru = new ReportUtility();
                ru.SetText(ref sheetSource, 1, Col, Header);
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    var un = ds.Tables[0].Rows[i]["Text"].ToString();
                    int k = i + 2;
                    ru.SetText(ref sheetSource, k, Col, un);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public IWorkbook GetWorkBookSKUSampleFile(string Name, string salesOrderId)
        {
            #region declare
            DataSet dsSKU1;
            DataSet dsSKU2;

            #endregion
            try
            {
                ReportUtility ru = new ReportUtility();

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = ru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                GetSKU1(out dsSKU1);
                GetSKU2(out dsSKU2);
                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;

                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                IWorksheet sheetSource = null;
                sheetSource = workbook.Worksheets[1];

                xlsRow = 1;
                CreateSource(dsSKU1, 1, "SKU1", ref sheetSource); int SKU1Col = 2;
                CreateSource(dsSKU2, 4, "SKU2", ref sheetSource); int SKU2Col = 4;

                int maxRow = 5001;
                #region ------------------Column Header------------------

                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SalesOrderId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10; int colSalesOrderId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SKU1", ExcelKnownColors.Red); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15; int colSKU1Id = xlsCol;// xlsCol += 1;
                ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, SKU1Col, dsSKU1.Tables[0].Rows.Count); xlsCol += 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SKU1ValueId", ExcelKnownColors.Red); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10; int colSKU1ValueId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "FirstCharacteristicsId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15; int colFirstCharacteristicsId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SKU2", ExcelKnownColors.Red); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12; int colSKU2Id = xlsCol; //xlsCol += 1;
                ru.SetList(ref sheet1, xlsRow, maxRow, xlsCol, sheetSource, SKU2Col, dsSKU2.Tables[0].Rows.Count); xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Qty"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12; int colQty = xlsCol; xlsCol += 1;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                xlsRow++;

                #endregion ------------------Column Header------------------

                DataTable dtData = GetSKUSampleData(salesOrderId);
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    sheet1[xlsRow, colSalesOrderId].Text = dtData.Rows[i]["SalesOrderId"].ToString();
                    sheet1[xlsRow, SKU1Col].Text = dtData.Rows[i]["SKU1"].ToString();
                    sheet1[xlsRow, colFirstCharacteristicsId].Text = dtData.Rows[i]["FirstCharacteristicsId"].ToString();
                    sheet1[xlsRow, SKU2Col].Text = dtData.Rows[i]["SKU2"].ToString();
                    sheet1[xlsRow, colQty].Text = dtData.Rows[i]["Qty"].ToString();
                    xlsRow++;
                }

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 10;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$1";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Sheet1";
                #endregion Page Setup

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetSKUSampleData(string salesOrderId)
        {
            var cmdText = @"SELECT FC.Id,CV.UserName+'_#'+CV.Id SKU1,FC.CharacteristicsValueId SKU1Value
                ,FC.SalesOrderId,CV2.UserName+'_#'+CV2.Id SKU2,SC.CharacteristicsValueId SKU2Value,SC.FirstCharacteristicsId
                ,FC.Qty SKU1Qty,SC.Qty 
                from [TRN].[FirstCharacteristics] FC 
                LEFT JOIN [TRN].[SecondCharacteristics] SC ON SC.FirstCharacteristicsId=FC.Id
				LEFT JOIN [HKP].[Characteristics] C ON C.Id=FC.CharacteristicsId
				LEFT JOIN [HKP].[CharacteristicsValue] CV on CV.CharacteristicsId=C.Id and CV.Id=FC.CharacteristicsValueId
				LEFT JOIN [HKP].[Characteristics] C2 ON C2.Id=SC.CharacteristicsId
				LEFT JOIN [HKP].[CharacteristicsValue] CV2 on CV2.CharacteristicsId=C2.Id  and CV2.Id=SC.CharacteristicsValueId
                where FC.SalesOrderId='" + UploadSqlEsc(salesOrderId) + @"'
                ORDER BY FC.Id";

            return _sqlRepository.GetDataTable(cmdText);
        }

        private static string UploadSqlEsc(string s)
        {
            return (s ?? "").Replace("'", "''");
        }

        // numeric literal for DataView filters on int/decimal columns (never trusts the raw text)
        private static string UploadNum(object v)
        {
            decimal d;
            return decimal.TryParse(Convert.ToString(v), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out d)
                ? d.ToString(System.Globalization.CultureInfo.InvariantCulture) : "0";
        }

        private static object UploadVal(object v)
        {
            return v == null ? (object)DBNull.Value : v;
        }

        // view model returned to the grid after import; property names match the excel headers
        public class UploadedSku1ViewModel
        {
            public string Id { get; set; }
            public string SalesOrderId { get; set; }
            public string SKU1Id { get; set; }
            public string SKU1ValueId { get; set; }
            public string SKU2Id { get; set; }
            public string SKU2ValueId { get; set; }
            public string FirstCharacteristicsId { get; set; }
            public decimal Qty { get; set; }
        }
        [HttpPost, Authorize]
        public JsonResult ImportSKUData(FormCollection form)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<UploadedSku1ViewModel> data = new List<UploadedSku1ViewModel>();

                var file = Request.Files["file"];

                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() != ".xlsx" && extension.ToLower() != ".xls")
                        throw new CustomException(Resources.ExcelUploadError);
                }
                else
                {
                    throw new CustomException(Resources.ExcelUploadError);
                }
                string path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                file.SaveAs(path);

                FileInfo docFile;
                try
                {
                    ExcelEngine excelEngine = new ExcelEngine();
                    IApplication application = excelEngine.Excel;
                    IWorkbook workbook = excelEngine.Excel.Workbooks.Open(path);

                    DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);

                    // the file is only needed to read it once
                    docFile = new FileInfo(path);
                    if (docFile.Exists)
                    {
                        docFile.Delete();
                    }

                    if (dt.Rows.Count == 0)
                        throw new Exception("Please Select File");

                    // fail early when the uploaded file is not based on the sample file
                    foreach (var h in new string[] { "SalesOrderId", "SKU1", "FirstCharacteristicsId", "SKU2", "Qty" })
                    {
                        if (!dt.Columns.Contains(h))
                            throw new Exception("Column '" + h + "' not found in the uploaded file. Please download the sample file again.");
                    }

                    objCon = new ConnectionManager.DAL.ConManager("1");

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow row = dt.Rows[i];
                        // rows without a value in the main column are blank lines of the sheet
                        if (string.IsNullOrWhiteSpace(row["SKU1"].ToString()))
                            continue;

                        UploadedSku1ViewModel vm = new UploadedSku1ViewModel();
                        string cSKU1Id = row["SKU1"].ToString().Trim();
                        string cSKU2Id = row["SKU2"].ToString().Trim();

                        vm.SalesOrderId = row["SalesOrderId"].ToString();
                        vm.FirstCharacteristicsId = row["FirstCharacteristicsId"].ToString();
                        if (cSKU1Id == "") throw new Exception("Row " + (i + 2) + ": SKU1Id is required. Please check !!");
                        vm.SKU1Id = row["SKU1"].ToString();

                        if (cSKU2Id == "") throw new Exception("Row " + (i + 2) + ": SKU2Id is required. Please check !!");
                        vm.SKU2Id = row["SKU2"].ToString();

                        vm.Qty = ParseDecimalOrFormula(row["Qty"]);

                        data.Add(vm);
                    }
                }
                catch (Exception ex)
                {
                    docFile = new FileInfo(path);
                    if (docFile.Exists)
                    {
                        docFile.Delete();
                    }
                    throw (ex);
                }
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        private static string MakePK(string masterId, int currentId, int padLeft)
        {
            if (string.IsNullOrWhiteSpace(masterId))
                throw new ArgumentException("masterId is required.", nameof(masterId));

            if (currentId < 0)
                throw new ArgumentOutOfRangeException(nameof(currentId), "currentId cannot be negative.");

            if (padLeft < 0)
                throw new ArgumentOutOfRangeException(nameof(padLeft), "padLeft cannot be negative.");

            // Pad only the numeric part with leading zeros, then append it to the master ID
            return masterId.Trim() + currentId.ToString().PadLeft(padLeft, '0');
        }
        [HttpPost, Authorize]
        public JsonResult SaveUploadedData(List<Dictionary<string, object>> sku1List, List<Dictionary<string, object>> dataList, string salesOrderId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsSKU1;
            DataSet dsSKU2;
            try
            {
                #region Entity
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[FirstCharacteristics] where SalesOrderId='" + UploadSqlEsc(salesOrderId) + "'", out dsSKU1, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[SecondCharacteristics] where SalesOrderId='" + UploadSqlEsc(salesOrderId) + "'", out dsSKU2, false, "1");

                if (dataList != null)
                {
                    var maxnumbersku1 = _masterOrderService.GetSKU1MaxNumber(salesOrderId);
                    var maxnumbersku2 = _masterOrderService.GetSKU2MaxNumber(salesOrderId);

                    foreach (var sku1 in sku1List)
                    {
                        sku1["SalesOrderId"] = salesOrderId;
                        string characteristisValId = GetCboPK(sku1["SKU1Id"].ToString());
                        dsSKU1.Tables[0].DefaultView.RowFilter = "[CharacteristicsValueId]=" + UploadNum(characteristisValId) + " AND [SalesOrderId]=" + UploadNum(sku1["SalesOrderId"]) + "";

                        if (dsSKU1.Tables[0].DefaultView.Count == 0)
                        {
                            maxnumbersku1++;
                            sku1["Id"] = MakePK(sku1["SalesOrderId"].ToString(), maxnumbersku1, 2);
                            sku1["CharacteristicsId"] = 1;
                            sku1["CharacteristicsValueId"] = characteristisValId;
                            AddNewRow(dsSKU1.Tables[0], sku1);
                        }
                        else
                        {
                            DataRow dr = dsSKU1.Tables[0].DefaultView[0].Row;      // fix 1: dsSKU1
                            sku1["Id"] = dr["Id"].ToString();                      // fix 1: keep existing Id for children
                            dr.BeginEdit();
                            dr["SalesOrderId"] = UploadVal(sku1["SalesOrderId"]);
                            dr["CharacteristicsId"] = 1;
                            dr["CharacteristicsValueId"] = characteristisValId;
                            dr["Qty"] = UploadVal(sku1["Qty"]);
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();
                        }

                        foreach (var item in dataList.Where(r => Equals(r["SKU1Id"], sku1["SKU1Id"])))
                        {
                            item["SalesOrderId"] = salesOrderId;
                            string characteristisValId2 = GetCboPK(item["SKU2Id"].ToString());

                            // fix 3: also filter by parent SKU1 row
                            dsSKU2.Tables[0].DefaultView.RowFilter =
                                "[CharacteristicsValueId]=" + UploadNum(characteristisValId2) +
                                " AND [SalesOrderId]=" + UploadNum(item["SalesOrderId"]) +
                                " AND [FirstCharacteristicsId]='" + UploadSqlEsc(sku1["Id"].ToString()) + "'";

                            if (dsSKU2.Tables[0].DefaultView.Count == 0)
                            {
                                maxnumbersku2++;                                                    // fix 2
                                item["Id"] = MakePK(sku1["Id"].ToString(), maxnumbersku2, 2);
                                item["CharacteristicsId"] = 2;
                                item["CharacteristicsValueId"] = characteristisValId2;              // main fix
                                item["FirstCharacteristicsId"] = sku1["Id"].ToString();
                                item["ValueFreeText"] = UploadVal(item["Qty"]);
                                item["Sequence"] = 1;
                                AddNewRow(dsSKU2.Tables[0], item);
                            }
                            else
                            {
                                DataRow dr2 = dsSKU2.Tables[0].DefaultView[0].Row;
                                dr2.BeginEdit();
                                dr2["SalesOrderId"] = UploadVal(item["SalesOrderId"]);
                                dr2["CharacteristicsId"] = 2;
                                dr2["CharacteristicsValueId"] = characteristisValId2;
                                dr2["Qty"] = UploadVal(item["Qty"]);
                                dr2["ValueFreeText"] = UploadVal(item["Qty"]);
                                dr2["UpdatedBy"] = identity.Name;
                                dr2["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr2["UpdatedFromIP"] = identity.IPAddress;
                                dr2.EndEdit();
                            }
                        }
                    }

                }
                #endregion
                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsSKU1, dsSKU2);
                return Json(new { Error = false, Data = dataList, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }



        string GetCboPK(string colvalue)
        {
            string r = string.Empty;
            string token = "_#";
            if (colvalue != null)
            {
                var _index = colvalue.IndexOf(token);
                if (_index != -1)
                {
                    r = colvalue.Substring(_index + token.Length).Trim().Replace("\n", "").Replace("\r", "");
                }
            }
            return r;
        }

        private decimal ParseDecimalOrFormula(object cellValue)
        {
            if (cellValue == null || cellValue == DBNull.Value)
                return 0;

            string text = cellValue.ToString().Trim();
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            // Direct number (covers the normal case, e.g. "25", "25.5")
            if (decimal.TryParse(text, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal directValue))
                return directValue;

            // Strip a leading "=" if the cell was entered as an Excel formula string, e.g. "=200/4"
            if (text.StartsWith("="))
                text = text.Substring(1).Trim();

            // Simple two-operand expression: 200/4, 200*4, 200+4, 200-4
            var match = System.Text.RegularExpressions.Regex.Match(
                text, @"^(-?\d+(\.\d+)?)\s*([\+\-\*/])\s*(-?\d+(\.\d+)?)$");

            if (match.Success)
            {
                decimal left = decimal.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                decimal right = decimal.Parse(match.Groups[4].Value, System.Globalization.CultureInfo.InvariantCulture);
                char op = match.Groups[3].Value[0];

                switch (op)
                {
                    case '/':
                        return right == 0 ? 0 : left / right;
                    case '*':
                        return left * right;
                    case '+':
                        return left + right;
                    case '-':
                        return left - right;
                }
            }

            // Couldn't parse as a number or a simple formula - default to 0
            return 0;
        }

        private int ParseIntOrDefault(object cellValue)
        {
            if (cellValue == null || cellValue == DBNull.Value)
                return 0;

            string text = cellValue.ToString().Trim();
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            // handles "25", "25.0" style cells (Excel numeric cells often round-trip as double text)
            if (int.TryParse(text, out int intVal))
                return intVal;

            if (decimal.TryParse(text, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal decVal))
                return (int)decVal;

            return 0;
        }
        #endregion
    }

    public class CharValue
    {
        public int Id { get; set; }
        public string Code { get; set; } // ShortName (change to StandardName/UserName if that's your display field)
    }

    public class OpenHeadModelNew
    {

        public string Id { get; set; }
        public string MasterOrderItemId { get; set; }
        public string OrderLineCostingItemId { get; set; }
        public string SOItemName { get; set; }
        public string UserName { get; set; }
        public string Formula { get; set; }
        public string FormulaId { get; set; }
        public string CostingType { get; set; }
        public string CostingComponent { get; set; }
        public decimal Value { get; set; }
        public string EntryState { get; set; }
        public string ValueIN { get; set; }

    }

    public class SOCostModelNew
    {

        public string Id { get; set; }
        public string OrderLineCostingItemId { get; set; }
        public string UserName { get; set; }
        public string SOItemName { get; set; }
        public string Formula { get; set; }
        public string FormulaId { get; set; }
        public string CostingType { get; set; }
        public decimal ItemValue { get; set; }
        public decimal SOValue { get; set; }
        public decimal ValueDiff { get; set; }
        public string SalesOrderId { get; set; }
        public string Remark { get; set; }

    }
}

