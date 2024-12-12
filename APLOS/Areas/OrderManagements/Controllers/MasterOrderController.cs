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
        public JsonResult CreateSalesOrder(string masterItemId, SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.InsertOrUpdateSOGraph(masterItemId, salesOrderMaster);
            Library.Planning.OrderManagement.MasterOrder mo = new Library.Planning.OrderManagement.MasterOrder();
            mo.GenerateLogForTnA(masterItemId, Library.Service.Enums.TaskAppliedOnEnum.SalesOrder);

            return Json(new { Data = salesOrderMaster, Message = AplosMessage.Updated });
        }

        //[HttpPost, Authorize]
        //public JsonResult CreateSplitSalesOrder(string masterItemId, SalesOrderMaster salesOrderMaster)
        //{
        //    _masterOrderService.InsertOrUpdateSplitSOGraph(masterItemId, salesOrderMaster);
        //    return Json(new { Message = AplosMessage.Updated });
        //}

        [HttpPost, Authorize]
        public JsonResult CreateSplitSalesOrder(string masterItemId, SalesOrderMaster salesOrderMaster)
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

            MasterOrder.SplitSalesOrderData(masterItemId, salesOrderMaster, para);
            return Json(new { Message = AplosMessage.Updated + " Please reduce SKU Qty." });
        }

        [HttpPost]
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

        #region Contract
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
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[SalesAdditionalInfo] where  LineItemId='" + lineId + "'", out dsChild, false, "1");
                int count = 0;
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
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[SalesAdditionalInfo] where  SalesOrderId='" + SOId + "'", out dsChild, false, "1");
                int count = 0;
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
#endregion