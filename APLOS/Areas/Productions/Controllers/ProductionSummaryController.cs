#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;
using Library.Model.Productions.ProductionBooking;
using Library.Data.Sql;
using Library.OrderManagement.Production;
using System;
using System.Data;
using Library.Security.Core;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using System.Linq;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionSummaryController : BaseController
    {
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        private readonly ISqlRepository _sqlRepository;
        #region Constructor
        /// <summary>   The ProductionSummaryService service. </summary>
        private readonly IProductionSummaryService _ProductionSummaryService;

        public ProductionSummaryController(IProductionSummaryService ProductionSummaryService, ISqlRepository sqlRepository)
        {
            _ProductionSummaryService = ProductionSummaryService;
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult AplosWC()
        {
            return View();
        }
        public ActionResult AplosSFG()
        {
            return View();
        }

        public ActionResult AplosInOut()
        {
            return View();
        }


        public ActionResult Reject()
        {
            return View();
        }

        public ActionResult Report()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetIsProductionHourOpen()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_productionSummaryData.GetIsProductionHourOpen(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionBookingPeriodCbo()
        {
            return Json(_productionSummaryData.GetProductionBookingPeriodCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetShiftList(string processId)
        {
            return Json(_productionSummaryData.GetShiftList(processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCharacteristicsValueCbo(string soid)
        {
            return Json(_ProductionSummaryService.GetCharacteristicsValueCbo(soid), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCharacteristicsValueByPrOCbo(string soid)
        {
            return Json(_ProductionSummaryService.GetCharacteristicsValueByPrOCbo(soid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetLotNumberCbo(string SalesOrderId, string ProductionOrderId, string ProcessId, string productionLevel)
        {
            return Json(_productionSummaryData.GetLotNumberCbo(SalesOrderId,ProductionOrderId,ProcessId,productionLevel), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetWCProcessCbo(string processid, string entityId, string shiftId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetCbo(identity.PlantId, processid, entityId, identity.CompanyId, shiftId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetToWCProcessCbo(string processid, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetToWCCbo(identity.PlantId, processid, entityId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetWCProcessCboNew(string processid, string entityId,string productionDate,string shiftId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetCboWC(identity.PlantId, processid, entityId, productionDate, shiftId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetBookingLevel(string FromId, string ToId)
        {
            return Json(_productionSummaryData.GetBookingLevel(FromId, ToId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSFGMovementFromCbo(string entity)
        {
            return Json(_productionSummaryData.GetSFGMovementFromCbo(entity), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSFGMovementToCbo(string FromId, string flag, string EntityId)
        {
            return Json(_productionSummaryData.GetSFGMovementToCbo(FromId,flag, EntityId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetShiftGroupCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ProductionSummaryService.GetShiftGroupCbo(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCharInfoByPrO(string masterid, string workdate, string mmid, string soid, string artid, string CharCount, string CharacteristicsValueId)
        {
            return Json(_ProductionSummaryService.GetCharInfoByPrO(masterid, workdate, mmid, soid, artid, CharCount, CharacteristicsValueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCharInfo(string masterid, string workdate, string mmid, string soid, string artid, string CharCount, string CharacteristicsValueId)
        {
            return Json(_ProductionSummaryService.GetCharInfo(masterid, workdate, mmid, soid, artid, CharCount, CharacteristicsValueId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMentorAndRespPersonByWCM(string wcmId)
        {
            return Json(_ProductionSummaryService.GetMentorAndRespPersonByWCM(wcmId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetChar1Info(string masterid, string soid)
        {
            return Json(_ProductionSummaryService.GetChar1Info(masterid, soid), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetChar1InfobyPrO(string masterid, string soid)
        {
            return Json(_ProductionSummaryService.GetChar1InfobyPrO(masterid, soid), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLineItemGrid(string entityid, string processid, string workdate, string shiftid, string wcid, string ProductionLevel)
        {
            return Json(_productionSummaryData.GetLineItemGrid(entityid, processid, workdate, shiftid, wcid, ProductionLevel), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLineItemGridInOut(string entityid, string processid, string workdate, string shiftid, string wcid, string ProductionLevel)
        {
            return Json(_productionSummaryData.GetLineItemGrid(entityid, processid, workdate, shiftid, wcid, ProductionLevel), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLineItemGridSFG(string entityid, string processid, string workdate, string shiftid, string wcid, string ProductionLevel, string status)
        {

            return Json(_productionSummaryData.GetLineItemGridSFG(entityid, processid, workdate, shiftid, wcid, ProductionLevel, status), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSFGWIPQty(string EntityId, string processId, string workCenterMasterId, string salesOrderId, string productionOrderId, string status, bool IsCrossAllowed)
        {
            return Json(_productionSummaryData.GetSFGWIPQty(EntityId, processId, workCenterMasterId, salesOrderId, productionOrderId, status, IsCrossAllowed), JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public ActionResult GetEntityProcessOrderTotalQty(string EntityId, string processId, string salesOrderId, string productionOrderId, string status)
        {
            return Json(_productionSummaryData.GetEntityProcessOrderTotalQty(EntityId, processId, salesOrderId, productionOrderId, status), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetProcessParaData(string processId, string masterId, string ProductionOrderId)
        {
            return Json(_productionSummaryData.GetProcessParaData(processId, masterId, ProductionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProcessDetentionData(string processId, string entityId, string productionDate,string shiftId, string workcenter)
        {
            try
            {
                string sql = "";
                string DetentionTypeListsql = "";
                string DetentionListsql = "";
                sql = @"
SELECT CAST (CASE WHEN MMT.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,MMT.Sequence,MMT.Id, MMT.EntityId, MMT.DetentionId, MMT.DetentionTypeId, MMT.ProcessId, MMT.DepartmentId, MMT.ShiftId, MMT.ResponsiblePersonId as ResponsiblePersonId, 
MMT.Remark, MMT.AddedBy, MMT.AddedDate, MMT.AddedFromIP, MMT.UpdatedBy, MMT.UpdatedDate, MMT.UpdatedFromIP
,E.UserName Entity,D.UserName DepartmentName,DM.DetentionUserName Detention,FORMAT(MMT.Date,'dd-MMM-yyyy')[Date],P.UserName Process
										,format(MMT.FromTime,'hh:mm tt') as FromTime,format(MMT.ToTime,'hh:mm tt') as ToTime,MMT.Minute as [Minute],SD.UserName Shift,
										EI.EmployeeName ResponsiblePerson,EI.EmployeeCode ResponsiblePersonCode,MMT.Remark,MMT.WorkCenterId,WC.UserName as WorkCenter
			                            from MachineMasterTransaction MMT
			                            left join ORG.Entity E on E.Id=MMT.EntityId
										left join ORG.Department D on D.Id=MMT.DepartmentId
										left join DetentionMaster DM on DM.Id=MMT.DetentionId
										left join HKP.Process P on P.Id=MMT.ProcessId
										left join ShiftDefination SD on SD.SystemID=MMT.ShiftId
										left Join SCS.WorkCenterMaster WC on WC.id=MMT.WorkCenterId
										left join EmployeeInformation EI on EI.SystemId=MMT.ResponsiblePersonId
                where MMT.EntityId = '" + entityId + "' and MMT.ProcessId = '" + processId + "'  and MMT.Date = '" + productionDate + "' and MMT.ShiftId = '" + shiftId + "' and MMT.WorkCenterId = '" + workcenter + "'";


                //return _sqlRepository.GetDataCollection(sql, null);

                DetentionTypeListsql = @"select DT.UserName As Text, DT.Id As Value from MachineMasterTransaction MMT 
                                         left outer join hkp.DetentionType DT ON DT.id=MMT.DetentionTypeId";

                DetentionListsql = @"Select DM.DetentionUserName As Text, DM.Id As Value,DM.IsAssetApplicable,DM.IsWorkCenterApplicable from MachineMasterTransaction MMT 
                                     left outer join  DetentionMaster DM ON DM.id=MMT.DetentionId";

                List<Dictionary<string, object>> MainList = _sqlRepository.GetDataCollection(sql);
                List<Dictionary<string, object>> detentiontypelist = _sqlRepository.GetDataCollection(DetentionTypeListsql);
                List<Dictionary<string, object>> detentionList = _sqlRepository.GetDataCollection(DetentionListsql);
                for (int i = 0; i < MainList.Count; i++)
                {
                    try
                    {
                        //List<Dictionary<string, object>> k = detentiontypelist.ToList();
                        List<Dictionary<string, object>> k = detentiontypelist.Where(ee => clsStaticInfo.nullrecorder(ee["Value"]) == clsStaticInfo.nullrecorder(MainList[i]["DetentionTypeId"])).ToList();
                        MainList[i]["DetentionTypeList"] = k;

                    }
                    catch (Exception)
                    {
                       
                    }

                    try
                    {
                        List<Dictionary<string, object>> m = detentionList.Where(ee => clsStaticInfo.nullrecorder(ee["Value"]) == clsStaticInfo.nullrecorder(MainList[i]["DetentionId"])).ToList();


                        MainList[i]["DetentionList"] = m;
                    }
                    catch (Exception)
                    {

                    }


                }
                return Json(MainList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return Json(_productionSummaryData.GetProcessDetentionData(processId, entityId, productionDate, shiftId, workcenter), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTotalProductionQty(string wcid, string workdate)
        {
            return Json(_ProductionSummaryService.GetTotalProductionQty(wcid, workdate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTotalSOQty(string salesOrderId, string processId)
        {
            return Json(_ProductionSummaryService.GetTotalQty(salesOrderId, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTotalPOQty(string productionOrderId, string processId)
        {
            return Json(_productionSummaryData.GetTotalPOQty(productionOrderId, processId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetItemsData(string entityid, string workCenterMasterId, string productionLevel, string processId, string ProductionOrderId)
        {
            return Json(_productionSummaryData.GetItemsData(entityid, workCenterMasterId, productionLevel, processId, ProductionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProductionOrderData(string entityid, string workCenterMasterId, string productionLevel, string processId, string status)
        {
            return Json(_productionSummaryData.GetProductionOrderData(entityid, workCenterMasterId, productionLevel, processId, status), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProductionOrderDataList(string entityid, string workCenterMasterId, string productionLevel, string processId)
        {
            return Json(_productionSummaryData.GetProductionOrderDataList(entityid, workCenterMasterId, productionLevel, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSFGSOItem(string entityid, string workCenterMasterId, string productionLevel, string processId, string status, bool IsFirst, string ProductionOrderId)
        {
            return Json(_productionSummaryData.GetSFGSOItem(entityid, workCenterMasterId, productionLevel, processId, status, IsFirst, ProductionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd, List<Dictionary<string, object>> ProcessParaList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ps.PlantId = identity.PlantId;
            _ProductionSummaryService.SaveMaster(ps, psd, identity.CompanyGroupId);
            if (ProcessParaList != null)
            {
                SaveMasterOrderItemCostingRateData(ProcessParaList, ps.Id);
            }
            return Json(new { ProductionSummary = ps, Message = AplosMessage.Success });
        }
        [HttpPost]
        public JsonResult CreateWC(List<Dictionary<string, object>> DataList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _ProductionSummaryService.SaveMasterWC(DataList);
            return Json(new { Message = AplosMessage.Success });
        }
        [HttpPost]
        public JsonResult createDetentionWC(List<Dictionary<string, object>> DataList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _ProductionSummaryService.SaveDetentionWC(DataList);
            return Json(new { Message = AplosMessage.Success });
        }
        private void SaveMasterOrderItemCostingRateData(List<Dictionary<string, object>> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (string.IsNullOrEmpty(masterId))
                {
                    throw new Exception("Select Line Item.");
                }
                #region FUND 
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster = null;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.ProductionSummaryParameterValue where  ProductionSummaryId='" + masterId + "'", out dsMaster, false, "1");
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
                            item["Id"] = masterId + idc;
                            item["ProductionSummaryId"] = masterId;

                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            item["ProductionSummaryId"] = masterId;
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

        [HttpPost]
        public JsonResult CreateInOut(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ps.PlantId = identity.PlantId;
            _ProductionSummaryService.SaveInOutMaster(ps, psd, identity.CompanyGroupId);
            return Json(new { ProductionSummary = ps, Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetWIPQtyForValidation(string Id, string EntityId, string processId, string workCenterMasterId, string salesOrderId, string productionOrderId, string status, bool IsCrossAllowed)
        {
            return Json(_productionSummaryData.GetWIPQtyForValidation(Id, EntityId, processId, workCenterMasterId, salesOrderId, productionOrderId, status, IsCrossAllowed), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSFGTotalQty(string salesOrderId, string processId, string status)
        {
            return Json(_productionSummaryData.GetSFGTotalQty(salesOrderId, processId, status), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSFGTotalPOQty(string productionOrderId, string processId, string status)
        {
            return Json(_productionSummaryData.GetSFGTotalPOQty(productionOrderId, processId, status), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CreateSFG(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd, string level, string productionOrderId, string salesOrderId, string processId, string status, bool IsFirst, bool IsCrossAllowed)
        {

            try
            {
                if (IsFirst == true && status == "INVENTORY")
                {
                    if (level == "ProductionOrder")
                    {
                        var allData = _productionSummaryData.GetTotalSFGPOQty(ps.Id, productionOrderId, processId, status);
                        if (allData != null)
                        {

                            int TotalSalesOrderQty = Convert.ToInt32(allData["PlannedQty"].ToString());
                            int RemainingQty = Convert.ToInt32(allData["RemainingQty"].ToString());
                            int TotalProductionQty = Convert.ToInt32(allData["TotalProductionQty"].ToString());

                            if (RemainingQty < 0)
                            {
                                throw new Exception("Order Quantity dosen't available.");
                            }

                            if (TotalSalesOrderQty < (TotalProductionQty + ps.Quantity))
                            {
                                throw new Exception("Produced Quantity should less than Order Quantity.");
                            }

                        }

                    }
                    else
                    {
                        var allData = _productionSummaryData.GetTotalSOSFGQty(ps.Id, salesOrderId, processId, status);
                        int TotalSalesOrderQty = Convert.ToInt32(allData["PlannedQty"].ToString());
                        int RemainingQty = Convert.ToInt32(allData["RemainingQty"].ToString());
                        int TotalProductionQty = Convert.ToInt32(allData["TotalProductionQty"].ToString());

                        if (RemainingQty < 0)
                        {
                            throw new Exception("Order Quantity dosen't available.");
                        }

                        if (TotalSalesOrderQty < (TotalProductionQty + ps.Quantity))
                        {
                            throw new Exception("Produced Quantity should less than Sales Order Quantity.");
                        }
                    }
                }

                if (IsFirst == false)
                {
                    if (status == "INVENTORY")
                    {
                        processId = ps.FromSFGInventoryId;
                    }
                    else
                    {
                        processId = ps.ProcessId;
                    }
                    var wipData = _productionSummaryData.GetWIPQtyValidation(ps.Id, ps.EntityId, processId, ps.WorkCenterMasterId, salesOrderId, productionOrderId, status, IsCrossAllowed);

                    if (wipData != null)
                    {
                        decimal InQ = Convert.ToDecimal(wipData["InQuantity"].ToString());
                        decimal OutQ = Convert.ToDecimal(wipData["OutQuantity"].ToString());

                        if (InQ - (OutQ + ps.Quantity) < 0)
                        {
                            throw new Exception("Total out quantity is greater than total in quantity.");
                        }
                    }

                }

                //if (IsFirst == true && status == "INVENTORY")
                //{
                //    var wipData = _productionSummaryData.GetWIPQtyValidation(ps.Id, ps.EntityId, processId, ps.WorkCenterMasterId, salesOrderId, productionOrderId, status, IsCrossAllowed);

                //    if (wipData != null)
                //    {
                //        decimal InQ = Convert.ToDecimal(wipData["InQuantity"].ToString());
                //        decimal OutQ = Convert.ToDecimal(wipData["OutQuantity"].ToString());

                //        if (InQ - (OutQ + ps.Quantity) < 0)
                //        {
                //            throw new Exception("Total out quantity is greater than total in quantity.");
                //        }
                //    }

                //}




                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ps.PlantId = identity.PlantId;

                _ProductionSummaryService.SaveInOutMaster(ps, psd, identity.CompanyGroupId);
                return Json(new { ProductionSummary = ps, Message = AplosMessage.Success });
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost, Authorize]
        public JsonResult createDetail(string psid, IEnumerable<ProductionSummaryDetail> psd)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _ProductionSummaryService.SaveDetail(psid, psd);
            return Json(new { ProductionSummaryDetail = psd, Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult createSecondDetail(IEnumerable<ProductionSummaryDetail> psd, ProductionSummary productionSummary)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _ProductionSummaryService.SaveSecondDetail(psd, productionSummary, identity.CompanyGroupId, identity.PlantId);
            return Json(new { ProductionSummaryDetail = psd, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _ProductionSummaryService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost]
        public ActionResult DeleteInOut(string id)
        {
            _ProductionSummaryService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult DeleteSFG(string id)
        {
            _ProductionSummaryService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetInWC(string FDUD, string PlantId, string EntityId, string WorkCenterMasterId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetWIPInWC(FDUD, EntityId, WorkCenterMasterId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOutWC(string FDUD, string PlantId, string EntityId, string WorkCenterMasterId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetOutWC(FDUD, EntityId, WorkCenterMasterId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult Calculate(IEnumerable<OpenHeadModelNew> OpenHeadNew)
        {
            DataTable dtValue = new DataTable();
            dtValue.TableName = "TempTable";
            dtValue.Columns.Add("ProductionBookingParameterId");
            dtValue.Columns.Add("Amount");
            string sFormulaResult = null;

            DataSet dsOpenHead = Library.Service.Helpers.DataTableExtensions.ToDataSet<OpenHeadModelNew>(OpenHeadNew);
            for (int i = 0; i < dsOpenHead.Tables[0].Rows.Count; i++)
            {
                if (i == 0)
                {
                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["ProductionBookingParameterId"] = dsOpenHead.Tables[0].Rows[i]["ProductionBookingParameterId"].ToString().Trim();
                    dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["Value"].ToString().Trim();

                    dtValue.Rows.Add(dtValueRow);
                }
                else if (i > 0 && string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                {
                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["ProductionBookingParameterId"] = dsOpenHead.Tables[0].Rows[i]["ProductionBookingParameterId"].ToString().Trim();
                    dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["Value"].ToString().Trim();

                    dtValue.Rows.Add(dtValueRow);
                }

                if (!string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                {
                    _productionSummaryData.ReLoadFormulaWithValue(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString(), ref dtValue, out string _formulaValue);
                     sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString("#####");

                    DataRow dtValueRow = dtValue.NewRow();

                    dtValueRow["ProductionBookingParameterId"] = dsOpenHead.Tables[0].Rows[i]["ProductionBookingParameterId"].ToString().Trim();
                    
                    if (sFormulaResult == "" || sFormulaResult == "∞")
                    {
                        dtValueRow["Amount"] = 0;
                    }
                    else
                    {
                        dtValueRow["Amount"] = sFormulaResult;
                    }

                    dtValue.Rows.Add(dtValueRow);

                    DataView dv = new DataView(dsOpenHead.Tables[0]);
                    dv.RowFilter = "ProductionBookingParameterId='" + dsOpenHead.Tables[0].Rows[i]["ProductionBookingParameterId"].ToString() + "'";
                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();
                        if (sFormulaResult == "" || sFormulaResult == "∞" || sFormulaResult == "NaN")
                        {
                            drmo["Value"] = 0;
                        }
                        else
                        {
                            drmo["Value"] = sFormulaResult;
                        }
                        drmo.EndEdit();

                    }


                }


            }


            List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dsOpenHead.Tables[0]);
            return Json(new { NewData, Message = AplosMessage.Success });
        }

        #endregion

        #region Production Report with parameter
        [HttpPost, Authorize]
        public ActionResult StockRegisterData(string ToDate, string FromDate, string EntityId, string ShiftId, string ProcessId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(GetStockRegisterData(FromDate, ToDate, EntityId, ShiftId, ProcessId));
                var jsondata = Json(new { NewData, Message = AplosMessage.Success });
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetStockRegisterData(string FromDate, string ToDate, string EntityId, string ShiftId, string ProcessId)
        {
            try
            {
                var str = @"SELECT e2.UserName Entity,P.UserName Process,PSQ.Sequence ProcessSequence,FORMAT(PS.ProductionDate,'dd-MMM-yyyy')ProductionDate
										,CSG.UserName [Shift],WCM.UserName WorkCenterMaster,PS.ProductionOrderId,PS.LotNumber,E.EmployeeName ResponsiblePerson,E.EmployeeName Mentor
										,PS.Remarks,'' MaterialMaster,''Article,''BuyerRefrence,''Productcode,PS.AddedBy,FORMAT(PS.AddedDate,'dd-MMM-yyyy')AddedDate,PS.UpdatedBy,FORMAT(PS.UpdatedDate,'dd-MMM-yyyy')UpdateDate
										FROM [TRN].[ProductionSummary] PS
										LEFT JOIN ORG.Entity AS e2 ON e2.Id = PS.EntityId
										LEFT JOIN HKP.Process P ON P.Id=PS.ProcessId
										LEFT JOIN [dbo].[ProcessAndInventorySequence] PSQ ON PSQ.ProcessId = P.Id
										LEFT JOIN EmployeeInformation E ON E.SystemId=PS.ResponsiblePersonId
										LEFT JOIN EmployeeInformation M ON M.SystemId=PS.MentorId
										LEFT JOIN SCS.WorkCenterMaster WCM ON WCM.Id=PS.WorkCenterMasterId
                                        LEFT JOIN dbo.ShiftDefination csg ON csg.SystemId=pp.ProductionShiftId
										Where
										PS.EntityId='" + EntityId + @"' and
										
										PS.ProductionDate between '" + FromDate + "' AND '" + ToDate + "'";
                return _sqlRepository.GetDataTable(str);

            }
            catch (Exception e)
            {
                throw e;
            }
        }


        [HttpPost, Authorize]
        public ActionResult StockRegisterReport(string ToDate, string FromDate, string SlNo)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string fileName = "";
                fileName = CreateStockRegisterReportSheet(identity.CompanyId, identity.PlantId, FromDate, ToDate, SlNo, "Stock Register Report " + FromDate + " To " + ToDate + "");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string CreateStockRegisterReportSheet(string CompanyId, string PlantId, string FromDate, string ToDate, string SlNo, string SheetName)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var data = GetStockRegisterReportData(CompanyId, PlantId, FromDate, ToDate, SlNo, true);

            var sheet = workbook.Worksheets[0];

            #region sheet1
            sheet.Name = "PurchaseRegisterGRNWise";

            int ROW = 7;
            int endCol = 1;
            int COL = 1;

            //sheet.Range[ROW, COL].Text = "From - "+FromDate+" , To - "+ToDate;
            //sheet.Range[ROW, COL].ColumnWidth = 13;
            //sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            //sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //ROW += 2;

            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 25, ExcelHAlign.HAlignLeft);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process", 18, ExcelHAlign.HAlignLeft);
            int ColProcess = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process sequence", 18, ExcelHAlign.HAlignLeft);
            int ColProcessSequence = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Date", 13, ExcelHAlign.HAlignLeft);
            int ColDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shift", 15, ExcelHAlign.HAlignLeft);
            int ColShift = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Work Center", 18, ExcelHAlign.HAlignLeft);
            int ColWorkCenter = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Po No.", 10, ExcelHAlign.HAlignLeft);
            int ColPoNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Lot No.", 10, ExcelHAlign.HAlignLeft);
            int ColLotNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Production Work Station", 12, ExcelHAlign.HAlignLeft);
            int ColProductionWorkStation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 12, ExcelHAlign.HAlignLeft);
            int ColResponsiblePerson = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Mentor", 10, ExcelHAlign.HAlignLeft);
            int ColMentor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Production", 11, ExcelHAlign.HAlignLeft);
            int ColProduction = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Parameter 1", 20, ExcelHAlign.HAlignLeft);
            int ColPeramiter1 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Parameter 2", 12, ExcelHAlign.HAlignLeft);
            int ColPeramiter2 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Parameter 3", 13, ExcelHAlign.HAlignLeft);
            int ColPeramiter3 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Parameter 4", 12, ExcelHAlign.HAlignLeft);
            int ColPeramiter4 = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 13, ExcelHAlign.HAlignRight);
            //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material Master", 15, ExcelHAlign.HAlignRight);
            int ColMaterialMaster = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 16, ExcelHAlign.HAlignRight);
            int ColArticle = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Buyer Refrence", 13, ExcelHAlign.HAlignRight);
            int ColBuyerRefrence = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Product Code", 13, ExcelHAlign.HAlignRight);
            int ColProductCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Add By", 10, ExcelHAlign.HAlignRight);
            int ColAddBy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Add Date", 13, ExcelHAlign.HAlignRight);
            int ColAddDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Updated By", 16, ExcelHAlign.HAlignRight);
            int ColUpdatedBy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Updated Time", 10, ExcelHAlign.HAlignRight);
            int ColUpdatedTime = COL;

            endCol = COL;
            #endregion Headers


            sheet.Range[ROW, 1, ROW, COL].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            ROW++;
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColEntity].Text = data.Rows[i]["PartyName"].ToString();
                sheet[ROW, ColProcess].Text = data.Rows[i]["InvoicingPartyPlant"].ToString();
                sheet[ROW, ColProcessSequence].Text = data.Rows[i]["DeliveryPartyPlant"].ToString();
                sheet[ROW, ColDate].Text = data.Rows[i]["PartyCode"].ToString();
                sheet[ROW, ColShift].Text = data.Rows[i]["GSTINNo"].ToString();
                sheet[ROW, ColWorkCenter].Text = data.Rows[i]["Employee"].ToString();
                sheet[ROW, ColPoNo].Text = data.Rows[i]["GRNNo"].ToString();
                sheet[ROW, ColLotNo].Text = data.Rows[i]["GRNEntryDate"].ToString();
                sheet[ROW, ColProductionWorkStation].Text = data.Rows[i]["VoucherNo"].ToString();
                sheet[ROW, ColResponsiblePerson].Text = data.Rows[i]["PostingDate"].ToString();
                sheet[ROW, ColMentor].Text = data.Rows[i]["DocRefNo"].ToString();
                sheet[ROW, ColProduction].Text = data.Rows[i]["DocRefDate"].ToString();
                sheet[ROW, ColPeramiter1].Text = data.Rows[i]["GrnDocDateDifference"].ToString();
                sheet[ROW, ColPeramiter2].Text = data.Rows[i]["GateEntryNo"].ToString();
                sheet[ROW, ColPeramiter3].Text = data.Rows[i]["GateName"].ToString();
                sheet[ROW, ColPeramiter4].Text = data.Rows[i]["CurrencyName"].ToString();
                sheet[ROW, ColRemarks].Text = data.Rows[i]["PartyGroup"].ToString();
                sheet[ROW, ColMaterialMaster].Text = data.Rows[i]["PartyCategory"].ToString();
                sheet[ROW, ColArticle].Text = data.Rows[i]["PartySubCategory"].ToString();
                sheet[ROW, ColBuyerRefrence].Text = data.Rows[i]["PartyType"].ToString();
                sheet[ROW, ColProductCode].Text = data.Rows[i]["PartyAccountGroup"].ToString();
                sheet[ROW, ColAddBy].Text = data.Rows[i]["PartyAccountGroup"].ToString();
                sheet[ROW, ColAddDate].Text = data.Rows[i]["PartyAccountGroup"].ToString();
                sheet[ROW, ColUpdatedBy].Text = data.Rows[i]["PartyAccountGroup"].ToString();
                sheet[ROW, ColUpdatedTime].Text = data.Rows[i]["PartyAccountGroup"].ToString();


                sheet.Range[ROW, ColEntity, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, ColEntity, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }

            //ROW++;

            //if (FromDate != "" && ToDate != "")
            //{


            //	report.SetText(ref sheet, ROW, Convert.ToInt32(ColMaterialTranAmount) - 1, "Total");
            //	sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount) - 1].CellStyle.Font.Bold = true;
            //	//sheet.Range[1, ROW, Convert.ToInt32(ColMaterialTranAmount) - 1, ROW].Merge();
            //	object sumObject;

            //	//sumObject = data.Compute("Sum(MaterialTranAmount)", "");
            //	//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].CellStyle.Font.Bold = true;
            //	//report.SetText(ref sheet, ROW, Convert.ToInt32(ColMaterialTranAmount), Convert.ToDouble(sumObject).ToString("0.##"));
            //	//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //	//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

            //	sumObject = data.Compute("Sum(TotalMaterialBaseAmount)", "");
            //	sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].CellStyle.Font.Bold = true;
            //	report.SetText(ref sheet, ROW, Convert.ToInt32(ColTotalMaterialBaseAmount), Convert.ToDouble(sumObject).ToString("0.##"));
            //	sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //	sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

            //	sumObject = data.Compute("Sum(Payment)", "");
            //	sheet.Range[ROW, Convert.ToInt32(ColPayment)].CellStyle.Font.Bold = true;
            //	report.SetText(ref sheet, ROW, Convert.ToInt32(ColPayment), Convert.ToDouble(sumObject).ToString("0.##"));
            //	sheet.Range[ROW, Convert.ToInt32(ColPayment)].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //	sheet.Range[ROW, Convert.ToInt32(ColPayment)].VerticalAlignment = ExcelVAlign.VAlignTop;

            //	sumObject = data.Compute("Sum(Balance)", "");
            //	sheet.Range[ROW, Convert.ToInt32(ColBalance)].CellStyle.Font.Bold = true;
            //	report.SetText(ref sheet, ROW, Convert.ToInt32(ColBalance), Convert.ToDouble(sumObject).ToString("0.##"));
            //	sheet.Range[ROW, Convert.ToInt32(ColBalance)].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //	sheet.Range[ROW, Convert.ToInt32(ColBalance)].VerticalAlignment = ExcelVAlign.VAlignTop;

            //}

            endRow = ROW - 1;
            endRow = ROW - 1;

            #endregion sheet



            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.UsedRange.CellStyle.Font.Size = 8;



            ReportUtility reportUtility = new ReportUtility();
            //reportUtility.CompanyHeader(ref sheet, endCol, "Purchase Report Register GRN Wise", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);


            sheet.Name = SheetName;
            sheet.UsedRange.WrapText = true;
            sheet.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet, COL, SheetName, PlantId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
            workbook.Version = ExcelVersion.Excel2016;

            workbook.SaveAs(filePath);
            workbook.Close();
            excelEngine.Dispose();
            return filePath;

        }

        public DataTable GetStockRegisterReportData(string CompanyId, string PlantId, string FromDate, string ToDate, string GRNNo, bool isreport)
        {
            try
            {
                var str = @"SELECT   IR.Id GRNNo,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate,
							IR.GateEntryNo,p.UserName AS PartyName,P.Code PartyCode,isnull(PP.GSTIN,'') GSTINNo
						   ,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0),2) MaterialTranAmount
						   ,ROUND(Isnull(IRD.TotalTaxAmount,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2) TotalTaxAmount
						   ,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0)+Isnull(IRD.TotalTaxAmount,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2) TotalMaterialBaseAmount
						   ,SUM(ROUND(ISNULL(I.WrittenOffAmount*I.CompanyCurrencyRate,0),4)) as Payment
						   ,( ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0)+Isnull(IRD.TotalTaxAmount,0)+Isnull(IRD.ChargesTaxTranAmount,0),2))-(SUM(ROUND(ISNULL(I.WrittenOffAmount*I.CompanyCurrencyRate,0),4))) as Balance
						   ,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						   ,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						   ,IR.DocRefNo,CU.Code CurrencyName,IR.PartyType
						   ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup

							--new add
							,'' DocRefDate,'' GrnDocDateDifference,'' GateName,'' InvoicingPartyPlant,'' DeliveryPartyPlant,'' Employee
					from [TRN].[InventoryReceive] AS IR
					left jOIN (select InventoryReceiveId,Sum(TransactionQty)TransactionQty,Sum(MaterialTranAmount)MaterialTranAmount
						,Sum(TotalMaterialTranAmount)TotalMaterialTranAmount,Sum(TotalMaterialBooksCurrencyAmount)TotalMaterialBooksCurrencyAmount
						,SUM(TotalTaxAmount) TotalTaxAmount,sum(ChargesTaxTranAmount) ChargesTaxTranAmount
						FROM [TRN].[InventoryReceiveDetail]
					group by InventoryReceiveId ) AS IRD ON IR.Id=IRD.InventoryReceiveId 
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
					LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
					LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
					LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
					LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND cp.PlantId=IR.PlantId
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor'
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
					left join trn.Voucher V on V.Id=I.VoucherId
                    left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					left join trn.Voucher V1 on V1.Id=ep.VoucherId
						
					where  IR.PlantId='" + PlantId + @"' AND convert(Date,IR.GRNDate) BETWEEN  '" + FromDate + @"' AND '" + ToDate + @"' 
                    AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN')

					group by IR.GRNDate,IR.Id,IR.GateEntryNo,p.UserName,P.Code,PP.GSTIN,IRD.TotalMaterialTranAmount,IRD.TotalMaterialBooksCurrencyAmount,IRD.TotalTaxAmount,IRD.ChargesTaxTranAmount
					,MaterialTranAmount,IR.EmployeeId,IR.EmployeeId,V.VoucherNo,V1.VoucherNo,ep.PostingDate,I.PostingDate,IR.DocRefNo,CU.Code,IR.PartyType,PAG.UserName
					,PC.UserName,PSC.UserName,PG.UserName,IR.ToCurrencyRate";

                if (isreport)
                {

                    var newsql = "select * from(" + str + ") y where y.GRNNo in (" + GRNNo + @")";
                    return _sqlRepository.GetDataTable(newsql);

                }
                else
                {
                    str += "";
                    return _sqlRepository.GetDataTable(str);
                }


            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            return Json(filters(), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object> filters()
        {
            try
            {
                var sql = @"SELECT * FROM ( SELECT  
                                        isnull(e.Id,'') AS EntityId,isnull(e.UserName,'') Entity,
										pln.Id PlantId,Pln.UserName Plant,
                                        isnull(ps.Id,'') AS ProductionStatusId, isnull(ps.UserName,'') AS ProductionStatus
										,PO.Id ProductionOrderId
                                      , ResponsiblePersonId=STUFF((select distinct ','+XMO.ResponsiblePersonId from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join dbo.EmployeeInformation XEmp on XEmp.SystemId=XMO.ResponsiblePersonId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
	                                         , ResponsiblePerson=STUFF((select distinct ','+XEmp.EmployeeName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join dbo.EmployeeInformation XEmp on XEmp.SystemId=XMO.ResponsiblePersonId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                   , Buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

														 SOStatusId=STUFF((select distinct ','+XB.Id from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].OrderStatus XB on XB.Id=XSO.OrderStatusId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


													
																 MOStatusId=STUFF((select distinct ','+XB.Id from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].OrderStatus XB on XB.Id=XMO.OrderStatusId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

		
													 BuyerId=STUFF((select distinct ','+XB.Id from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

																
                                                    CustomerId=STUFF((select distinct ','+XP.Id from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),   
                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                                                 


                                        from trn.ProductionOrder PO
				                                inner join ProductionOrderSchedulingParametersType1 T1 on t1.ProductionOrderID=po.Id
												
				                              
				                                left outer join org.Entity E on e.Id=PO.EntityID
				                             
				                                left outer join org.Plant PLN on pln.Id=E.PlantId
				                                LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId

                              WHERE  PO.ProductionStatusId<>'Closed'
                                ) AS KK	";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetOrderReport(Dictionary<string, string> parameters, string fromDate, string toDate, string dateType)
        {
            try
            {
                string fileName = "";
                fileName = OrderReport(parameters, fromDate, toDate, dateType, "OrderReport");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string OrderReport(Dictionary<string, string> parameters, string fromDate, string toDate, string dateType, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(3);
                workbook.Worksheets[2].Name = "Data";
                sheet = workbook.Worksheets[2];
                DataTable dtOrder;
                OrderReportSQL(parameters, fromDate, toDate, dateType, out dtOrder);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 16;
                int colResponsiblePerson = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "Commitment Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCommitmentDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Item Reference No.";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyerRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 22;
                int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Shipment Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Ex Factory Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlanExFactoryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Id";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order Id";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Start Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionStartDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order Category";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionOrderCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "LSD";
                sheet[ROW, COL].ColumnWidth = 12;
                int colLSD = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Raw Material Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colMainrawMaterialDate = COL;
                COL++;
                sheet[ROW, COL].Text = "other Raw Material Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOtherRawMaterialDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colRate = COL;
                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colCM = COL;
                COL++;
                sheet[ROW, COL].Text = "SPT";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSPT = COL;
                COL++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int colRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSOQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Shipped Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colShippedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Bal Shipment";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBalShipment = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlan = COL;
                COL++;
                sheet[ROW, COL].Text = "To Plan";
                sheet[ROW, COL].ColumnWidth = 16;
                int colToPlan = COL;
                COL++;
                sheet[ROW, COL].Text = "Process Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProcessStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Product Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductCode = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 16;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Ref";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOwnRef = COL;
                COL++;
                sheet[ROW, COL].Text = "Description";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDescription = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colorderRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colorderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Material Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colMainMaterialRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Material Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colMainMaterialStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Other Raw Material Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOtherRawMaterialRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Other Raw Material Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOtherRawMaterialStatus = COL;
                COL++;


                sheet[ROW, COL].Text = "Input Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int colInputRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Input Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colInputStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Line Target";
                sheet[ROW, COL].ColumnWidth = 12;
                int colLineTarget = COL;
                COL++;
                sheet[ROW, COL].Text = "No of Line Plan";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colNoOfLinePlan = COL;
                COL++;

                sheet[ROW, COL].Text = "Priority";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPriority = COL;
                COL++;
                sheet[ROW, COL].Text = "Line No.";
                sheet[ROW, COL].ColumnWidth = 12;
                int colLineNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Value";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOrderValue = COL;
                COL++;
                sheet[ROW, COL].Text = "CM Value";
                sheet[ROW, COL].ColumnWidth = 12;
                int colCMValue = COL;

                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {
                    sheet[ROW, colResponsiblePerson].Text = dtOrder.Rows[i]["ResponsiblePerson"].ToString();

                    sheet[ROW, colBuyer].Text = dtOrder.Rows[i]["Buyer"].ToString();
                    sheet[ROW, colCustomer].Text = dtOrder.Rows[i]["Customer"].ToString();
                    sheet[ROW, colCommitmentDate].Text = dtOrder.Rows[i]["CommitmentDate"].ToString();
                    sheet[ROW, colBuyerRefNo].Text = dtOrder.Rows[i]["BuyerReferenceNo"].ToString();
                    sheet[ROW, colArticle].Text = dtOrder.Rows[i]["Article"].ToString();

                    sheet[ROW, colDeliveryDate].Text = dtOrder.Rows[i]["DeliveryDate"].ToString();
                    sheet[ROW, colPlanExFactoryDate].Text = dtOrder.Rows[i]["PlanExFactoryDate"].ToString();
                    sheet[ROW, colSalesOrderId].Text = dtOrder.Rows[i]["SalesOrderId"].ToString();
                    sheet[ROW, colSalesOrderStatus].Text = dtOrder.Rows[i]["SalseOrderStatus"].ToString();
                    sheet[ROW, colProductionOrderId].Text = dtOrder.Rows[i]["ProductionOrderID"].ToString();
                    sheet[ROW, colProductionStatus].Text = dtOrder.Rows[i]["ProductionStatus"].ToString();
                    sheet[ROW, colLSD].Text = dtOrder.Rows[i]["SOLSD"].ToString();
                    sheet[ROW, colMainrawMaterialDate].Text = dtOrder.Rows[i]["SOMainRawMaterialInhouseDate"].ToString();
                    sheet[ROW, colOtherRawMaterialDate].Text = dtOrder.Rows[i]["SOOtherRawMaterialInhouseDate"].ToString();
                    sheet[ROW, colRate].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["Rate"].ToString());


                    sheet[ROW, colCM].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["CM"].ToString());
                    sheet[ROW, colSPT].Number = OTSBD.clsStaticInfo.dbl(dtOrder.Rows[i]["SPT"].ToString());
                    sheet[ROW, colRemarks].Text = dtOrder.Rows[i]["Remarks"].ToString();
                    sheet[ROW, colSOQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["SOQty"].ToString());
                    sheet[ROW, colShippedQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ShippedQty"].ToString());
                    sheet[ROW, colBalShipment].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["BalShipment"].ToString());


                    sheet[ROW, colPlan].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["TotalPlanQty"].ToString());
                    sheet[ROW, colToPlan].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["RemainingPlanQuantity"].ToString());
                    //sheet[ROW, colProcessStatus].Text = dtOrder.Rows[i]["BuyerReferenceNo"].ToString();

                    sheet[ROW, colProductCode].Text = dtOrder.Rows[i]["ProductCode"].ToString();
                    sheet[ROW, colProduct].Text = dtOrder.Rows[i]["Product"].ToString();
                    sheet[ROW, colDescription].Text = dtOrder.Rows[i]["Description"].ToString();


                    sheet[ROW, colMaterial].Text = dtOrder.Rows[i]["Material"].ToString();
                    sheet[ROW, colOwnRef].Text = dtOrder.Rows[i]["OwnOrderNo"].ToString();
                    sheet[ROW, colorderRemarks].Text = dtOrder.Rows[i]["OrderRemarks"].ToString();
                    sheet[ROW, colorderStatus].Text = dtOrder.Rows[i]["OrderControlStatus"].ToString();
                    sheet[ROW, colMainMaterialRemarks].Text = dtOrder.Rows[i]["MainRMInhouseRemarks"].ToString();
                    sheet[ROW, colMainMaterialStatus].Text = dtOrder.Rows[i]["MainRMInhouseStatus"].ToString();
                    sheet[ROW, colOtherRawMaterialRemarks].Text = dtOrder.Rows[i]["OtherRMInhouseRemarks"].ToString();
                    sheet[ROW, colOtherRawMaterialStatus].Text = dtOrder.Rows[i]["OtherRMInhouseStatus"].ToString();
                    sheet[ROW, colInputRemarks].Text = dtOrder.Rows[i]["InputRemarks"].ToString();
                    sheet[ROW, colInputStatus].Text = dtOrder.Rows[i]["InputStatus"].ToString();
                    sheet[ROW, colLineTarget].Text = dtOrder.Rows[i]["PlannedLinePreference"].ToString();
                    sheet[ROW, colNoOfLinePlan].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["AllocatedLines"].ToString());
                    sheet[ROW, colPriority].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ProductionPriority"].ToString());
                    sheet[ROW, colLineNo].Text = dtOrder.Rows[i]["RunningOrderLinePreference"].ToString();
                    sheet[ROW, colOrderValue].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["OrderValue"].ToString());
                    sheet[ROW, colCMValue].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["CMValue"].ToString());
                    sheet[ROW, colProductionStartDate].Text = dtOrder.Rows[i]["ProductionStartDate"].ToString();
                    sheet[ROW, colProductionOrderCategory].Text = dtOrder.Rows[i]["ProductionOrderCategory"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Order Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;



                #region Sheet Report
                workbook.Worksheets[1].Name = "Report";
                sheet = workbook.Worksheets[1];


                ROW = 6; COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 16;
                colResponsiblePerson = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                colBuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "Commitment Date";
                sheet[ROW, COL].ColumnWidth = 16;
                colCommitmentDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Item Reference No.";
                sheet[ROW, COL].ColumnWidth = 16;
                colBuyerRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 22;
                colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Shipment Date";
                sheet[ROW, COL].ColumnWidth = 12;
                colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan Ex Factory Date";
                sheet[ROW, COL].ColumnWidth = 12;
                colPlanExFactoryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Id";
                sheet[ROW, COL].ColumnWidth = 16;
                colSalesOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Status";
                sheet[ROW, COL].ColumnWidth = 16;
                colSalesOrderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order Id";
                sheet[ROW, COL].ColumnWidth = 12;
                colProductionOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Status";
                sheet[ROW, COL].ColumnWidth = 12;
                colProductionStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Start Date";
                sheet[ROW, COL].ColumnWidth = 12;
                colProductionStartDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order Category";
                sheet[ROW, COL].ColumnWidth = 12;
                colProductionOrderCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "LSD";
                sheet[ROW, COL].ColumnWidth = 12;
                colLSD = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Raw Material Date";
                sheet[ROW, COL].ColumnWidth = 12;
                colMainrawMaterialDate = COL;
                COL++;
                sheet[ROW, COL].Text = "other Raw Material Date";
                sheet[ROW, COL].ColumnWidth = 12;
                colOtherRawMaterialDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                colRate = COL;
                COL++;
                sheet[ROW, COL].Text = "CM";
                sheet[ROW, COL].ColumnWidth = 6;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                colCM = COL;
                COL++;
                sheet[ROW, COL].Text = "SPT";
                sheet[ROW, COL].ColumnWidth = 10;
                colSPT = COL;
                COL++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                colRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                colSOQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Shipped Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                colShippedQty = COL;
                COL++;
                sheet[ROW, COL].Text = "Bal Shipment";
                sheet[ROW, COL].ColumnWidth = 16;
                colBalShipment = COL;
                COL++;
                sheet[ROW, COL].Text = "Plan";
                sheet[ROW, COL].ColumnWidth = 16;
                colPlan = COL;
                COL++;
                sheet[ROW, COL].Text = "To Plan";
                sheet[ROW, COL].ColumnWidth = 16;
                colToPlan = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                colorderRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Status";
                sheet[ROW, COL].ColumnWidth = 12;
                colorderStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Material Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                colMainMaterialRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Main Material Status";
                sheet[ROW, COL].ColumnWidth = 12;
                colMainMaterialStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Other Raw Material Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                colOtherRawMaterialRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Other Raw Material Status";
                sheet[ROW, COL].ColumnWidth = 12;
                colOtherRawMaterialStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Input Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                colInputRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Input Status";
                sheet[ROW, COL].ColumnWidth = 12;
                colInputStatus = COL;
                #endregion columns

                endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                startRow = ROW;

                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {

                    sheet[ROW, colPlant].Text = dtOrder.Rows[i]["Plant"].ToString();
                    sheet[ROW, colEntity].Text = dtOrder.Rows[i]["MasterOrderEntity"].ToString();
                    sheet[ROW, colResponsiblePerson].Text = dtOrder.Rows[i]["ResponsiblePerson"].ToString();

                    sheet[ROW, colBuyer].Text = dtOrder.Rows[i]["Buyer"].ToString();
                    sheet[ROW, colCustomer].Text = dtOrder.Rows[i]["Customer"].ToString();
                    sheet[ROW, colCommitmentDate].Text = dtOrder.Rows[i]["CommitmentDate"].ToString();
                    sheet[ROW, colBuyerRefNo].Text = dtOrder.Rows[i]["BuyerReferenceNo"].ToString();
                    sheet[ROW, colArticle].Text = dtOrder.Rows[i]["Article"].ToString();

                    sheet[ROW, colDeliveryDate].Text = dtOrder.Rows[i]["DeliveryDate"].ToString();
                    sheet[ROW, colPlanExFactoryDate].Text = dtOrder.Rows[i]["PlanExFactoryDate"].ToString();
                    sheet[ROW, colSalesOrderId].Text = dtOrder.Rows[i]["SalesOrderId"].ToString();
                    sheet[ROW, colSalesOrderStatus].Text = dtOrder.Rows[i]["SalseOrderStatus"].ToString();
                    sheet[ROW, colProductionOrderId].Text = dtOrder.Rows[i]["ProductionOrderID"].ToString();
                    sheet[ROW, colProductionStatus].Text = dtOrder.Rows[i]["ProductionStatus"].ToString();
                    sheet[ROW, colLSD].Text = dtOrder.Rows[i]["SOLSD"].ToString();
                    sheet[ROW, colMainrawMaterialDate].Text = dtOrder.Rows[i]["SOMainRawMaterialInhouseDate"].ToString();
                    sheet[ROW, colOtherRawMaterialDate].Text = dtOrder.Rows[i]["SOOtherRawMaterialInhouseDate"].ToString();
                    sheet[ROW, colRate].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["Rate"].ToString());


                    sheet[ROW, colCM].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["CM"].ToString());
                    sheet[ROW, colSPT].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["SPT"].ToString());
                    sheet[ROW, colRemarks].Text = dtOrder.Rows[i]["Remarks"].ToString();
                    sheet[ROW, colSOQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["SOQty"].ToString());
                    sheet[ROW, colShippedQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["ShippedQty"].ToString());
                    sheet[ROW, colBalShipment].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["BalShipment"].ToString());


                    sheet[ROW, colPlan].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["TotalPlanQty"].ToString());
                    sheet[ROW, colToPlan].Number = Library.Service.Extension.clsStaticInfo.dbl(dtOrder.Rows[i]["RemainingPlanQuantity"].ToString());
                    sheet[ROW, colorderRemarks].Text = dtOrder.Rows[i]["OrderRemarks"].ToString();
                    sheet[ROW, colorderStatus].Text = dtOrder.Rows[i]["OrderControlStatus"].ToString();
                    sheet[ROW, colMainMaterialRemarks].Text = dtOrder.Rows[i]["MainRMInhouseRemarks"].ToString();
                    sheet[ROW, colMainMaterialStatus].Text = dtOrder.Rows[i]["MainRMInhouseStatus"].ToString();
                    sheet[ROW, colOtherRawMaterialRemarks].Text = dtOrder.Rows[i]["OtherRMInhouseRemarks"].ToString();
                    sheet[ROW, colOtherRawMaterialStatus].Text = dtOrder.Rows[i]["OtherRMInhouseStatus"].ToString();
                    sheet[ROW, colInputRemarks].Text = dtOrder.Rows[i]["InputRemarks"].ToString();
                    sheet[ROW, colInputStatus].Text = dtOrder.Rows[i]["InputStatus"].ToString();
                    sheet[ROW, colProductionStartDate].Text = dtOrder.Rows[i]["ProductionStartDate"].ToString();
                    sheet[ROW, colProductionOrderCategory].Text = dtOrder.Rows[i]["ProductionOrderCategory"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                //IListObject table = sheet.ListObjects.Create("Table1", sheet[(1) + (6).ToString() + ":" + (endCol) + (ROW).ToString()]);
                table = sheet.ListObjects.Create("Table2", sheet.Range[6, 1, ROW, endCol]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                reportUtility.PlantHeader(ref sheet, endCol, "Order Report", identity.PlantId);
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange["A7"].FreezePanes();

                identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility = new ReportUtility();
                // reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "OrderReport", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet.IsGridLinesVisible = false;
                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;


                #endregion
                #region Pivot
                string fPath = fPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "TempReport" + identity.UserId + ".xlsx";

                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }

                workbook.Worksheets[0].Name = "Order";

                IWorksheet pivotSheet = workbook.Worksheets[0];
                IPivotCache cache = workbook.PivotCaches.Add(workbook.Worksheets[1][startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);

                pivotTable.Fields[colPlant - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colEntity - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colResponsiblePerson - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCustomer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colCommitmentDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colBuyerRefNo - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[colArticle - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDeliveryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPlanExFactoryDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSalesOrderStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionStatus - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionOrderId - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionStartDate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colProductionOrderCategory - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colLSD - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colMainrawMaterialDate - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colOtherRawMaterialDate - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colSPT - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colRemarks - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colorderRemarks - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colorderStatus - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colMainMaterialRemarks - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colMainMaterialStatus - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colOtherRawMaterialRemarks - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colOtherRawMaterialStatus - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colInputRemarks - 1].Axis = PivotAxisTypes.Row;
                //pivotTable.Fields[colInputStatus - 1].Axis = PivotAxisTypes.Row;


                IPivotField field = pivotTable.Fields[colRate - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "Rate", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colCM - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "CM", PivotSubtotalTypes.Sum);


                field = pivotTable.Fields[colSOQty - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "SO Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colShippedQty - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "Shipped Qty", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colBalShipment - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);
                pivotTable.DataFields.Add(field, "Bal Shipment", PivotSubtotalTypes.Sum);


                field = pivotTable.Fields[colPlan - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(0);
                pivotTable.DataFields.Add(field, "Plan", PivotSubtotalTypes.Sum);

                field = pivotTable.Fields[colToPlan - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(0);
                pivotTable.DataFields.Add(field, "To Plan", PivotSubtotalTypes.Sum);


                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colPlant - 1 || i == colEntity - 1 || i == colResponsiblePerson - 1 || i == colCustomer - 1 || i == colBuyer - 1)
                        continue;
                    pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }

                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Order Report", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
                workbook.Worksheets[0].UsedRange["A7"].FreezePanes();


                #endregion Buyer Summary
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void OrderReportSQL(Dictionary<string, string> parameters, string fromDate, string toDate, string dateType, out DataTable dtOrder)
        {
            string date = "";

            if (dateType == "ExFactoryD" && !string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                date = " AND so.PlanExFactoryDate between '" + fromDate + @"' and '" + toDate + @"' ";
            }
            if (dateType == "ShipmentD" && !string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                date = " AND so.DeliveryDate between '" + fromDate + @"' and '" + toDate + @"' ";
            }
            if (dateType == "CommitmentD" && !string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                date = " AND so.CommitmentDate between '" + fromDate + @"' and '" + toDate + @"' ";
            }


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT p2.Id PlantId,p2.UserName AS Plant,
e.Id AS MasterOrderEntityId,e.UserName AS MasterOrderEntity,
e2.Id AS ProductionOrderEntityId,e2.UserName AS ProductionOrderEntity,
p.UserName AS Customer,MO.Remarks,
b.UserName AS Buyer,ss.UserName AS Season,
ISNULL(CASE WHEN ISNULL(T.Qty,0)>0 THEN T.Qty ELSE PO.PlannedQty END,0) AS TotalPlanQty,
ISNULL(PRODPR.ProductionQtyAtPR,0) AS ProducedQty,
ISNULL(CASE WHEN ISNULL(T.Qty,0)>0 THEN T.Qty ELSE PO.PlannedQty END,0)-(ISNULL(PRODPR.ProductionQtyAtPR,0)-ISNULL(PRDQ.ProductionBookedQty,0)) AS RemainingPlanQuantity,


BDEP.UserName AS BuyerDepartment,bd.UserName AS BuyerDivision, ei.EmployeeName AS ResponsiblePerson,mo.MasterOrderNo,MO.TotalQty MasterOrderQty,
FORMAT(MO.AddedDate,'dd-MMM-yyyy') MasterOrderCreationDate,OC.UserName AS OrderCategory,os.UserName AS OrderStatus, mo.BuyerReferenceNo AS BuyerOrderNo
,MO.OwnReferenceNo AS OwnOrderNo,
MOI.Id AS LineItemId,MOI.BuyerReferenceNo,moi.ProductionGrouping,FORMAT(MOI.AddedDate,'dd-MMM-yyyy') MasterOrderItemCreationDate,
mm.UserName AS Material,mma.StandardName AS Article, pc.UserName AS ProductCategory, pm.UserName AS Product,MOI.TotalQty AS ItemQty,uom.UserName AS UOM,
PL.Id ProductLibrayId,PL.Code ProductCode,OrderRemarks=(FORMAT(SC.AddedDate,'dd-MMM-yyyy')+'-'+SC.Remarks),SC.[Status] OrderControlStatus,SC.CriticalityLevel
,MainRMInhouseRemarks=(FORMAT(M.AddedDate,'dd-MMM-yyyy')+'-'+M.Remarks),M.[Status] MainRMInhouseStatus
,OtherRMInhouseRemarks=(FORMAT(O.AddedDate,'dd-MMM-yyyy')+'-'+O.Remarks),O.[Status] OtherRMInhouseStatus
,InputRemarks=(FORMAT(I.AddedDate,'dd-MMM-yyyy')+'-'+I.Remarks),I.[Status] InputStatus



,so.Id AS SalesOrderId, so.DestinationId,dest.UserName AS Destination,
so.ShipmentModeId,smo.UserName AS ShipMode, OCS.Id SalesOrderCategoryId,OCS.UserName AS SalesOrderCategory,
OSS.Id SalseOrderStatusId,osS.UserName AS SalseOrderStatus, ISNULL(so.Qty,0) SOQty,SO.CM,SO.Rate,
FORMAT(so.DeliveryDate,'dd-MMM-yyyy') DeliveryDate, FORMAT(so.CommitmentDate,'dd-MMM-yyyy') CommitmentDate, FORMAT(so.PlanExFactoryDate,'dd-MMM-yyyy') PlanExFactoryDate
, FORMAT(so.MainRawMaterialInhouseDate,'dd-MMM-yyyy') SOMainRawMaterialInhouseDate,
FORMAT(so.OtherRawMaterialInhouseDate,'dd-MMM-yyyy') SOOtherRawMaterialInhouseDate,FORMAT(so.LSD,'dd-MMM-yyyy') SOLSD
,CP.PONumber,SO.Description,FORMAT(so.AddedDate,'dd-MMM-yyyy') SalesOrderCreationDate,
t.ProductionOrderID,ps.UserName AS ProductionStatus, t.NoOfWorkStation, t.Efficiency,
t.SPT, t.PlanWorkingHoursPerDay, t.FirstDayOutPut,
t.PlanTargetPerHour, t.IncrementValue, t.IncrementType,
t.DayToReachTheTarget,
--t.CommitmentDate ,
t.ProductionPriority, t.TargetPerHour, t.TargetPerDay,
t.MinimumLineDays, t.RequiredLineDays,
t.RequiredNoOfLines, t.AllocatedLines, t.Qty AS ExplicitProductionQty,
t.LSD AS PRLSD, t.MainRawMaterialInhouseDate AS PRMainRawMaterialInhouseDate, t.OtherRawMaterialInhouseDate AS PROtherRawMaterialInhouseDate,
t.RunningOrderBlockSize,l.LastProcessDate AS SewingCompletionDate,
ActiveOrderLinePreference=STUFF((select distinct ','+xw.UserName from
trn.ProductionOrderWorkCenter AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, ''),
RunningOrderLinePreference=STUFF((select distinct ','+xw.UserName from
trn.RunningOrderWorkCenter AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, ''),



PlannedLinePreference=STUFF((select distinct ','+xw.UserName from
ProductionPlanningType1 AS xp
INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
where PO.Id=xp.ProductionOrderId for xml path('') ), 1, 1, ''),


Format( case when  isnull(PRDD.ProductionDate,'')='' and  isnull(PLND.ProductionDate,'')='' THEN null
else case when 
isnull(PRDD.ProductionDate,PLND.ProductionDate) <= isnull(PLND.ProductionDate,PRDD.ProductionDate) THEN PRDD.ProductionDate
else PLND.ProductionDate END END,'dd-MMM-yyyy') AS ProductionStartDate,

case when isnull(PRDD.ProductionDate,'')='' then 'ToStart' else 'Started' END AS ProductionOrderCategory
,isnull(SM.TransactionQty,0) ShippedQty,isnull(SO.Qty,0)-ISNUll(SM.TransactionQty,0) BalShipment,
Isnull(so.CM,0)*isnull(so.Rate,0) CMValue
, Isnull(so.Qty,0)*isnull(so.Rate,0) OrderValue
FROM trn.MasterOrder MO
LEFT JOIN org.Plant AS p2 ON p2.id=mo.PlantId
LEFT JOIN org.Entity AS e ON e.Id=mo.EntityId
left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
LEFT join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
LEFT OUTER JOIN hkp.Season SS ON ss.Id=mo.SeasonId

LEFT OUTER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId

LEFT JOIN org.Entity AS e2 ON e2.Id=po.EntityId
LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T ON t.ProductionOrderID=po.Id
LEFT OUTER JOIN (
SELECT K.ProductionOrderID,max(K.LastProcessDate) AS LastProcessDate FROM (
SELECT ppt.ProductionOrderID,ppt.ProductionDate AS LastProcessDate
FROM ProductionPlanningType1 AS ppt
UNION ALL
SELECT ppt.ProductionOrderID,ppt.ProductionDate AS LastProcessDate
FROM trn.ProductionSummary AS ppt
) AS K GROUP BY K.ProductionOrderID
) AS L ON l.ProductionOrderID=po.Id
--production at PR Level
LEFT OUTER JOIN (
SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
FROM trn.ProductionSummary S
WHERE CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<=CONVERT(DATETIME, format(getdate(),'dd-MMM-yyyy'))
GROUP BY s.ProductionOrderId,s.ProcessId
) AS PRODPR ON PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
left outer join (SELECT pod.ProductionOrderId,
sum(isnull(so.ProductionBookedQty,0)) ProductionBookedQty
FROM trn.SalesOrder AS so
INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id



GROUP BY pod.ProductionOrderId
) AS PRDQ ON PRDQ.ProductionOrderId=po.Id
left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
left outer join mst.MaterialMasterArticle AS mma on mma.id=moi.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId



left outer join [HKP].[Party] p on P.Id=MO.PartyId
left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerDivisionId
left outer join [HKP].[BuyerDEPARTMENT] BDEP on BDEP.id=mo.BuyerDepartmentId
left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
left outer join mst.Destination DEST on dest.Id=so.DestinationId
left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId



left outer join [HKP].[OrderCategory] OCS on ocS.id=So.OrderCategoryId
left outer join [HKP].[OrderStatus] OSS on OSS.id=So.OrderStatusId



left outer join hkp.Season S on s.id=mo.SeasonId
left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId
LEFT OUTER JOIN scs.UnitOfMeasurement AS uom ON uom.Id=MO.TotalQtyUOMId
LEFT JOIN dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId



LEFT JOIN(
SELECT AMTR.Remarks,B.ProductionOrderId,AMTR.AddedDate,B.[Status]
FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'MainRMInhouse'
) M ON M.ProductionOrderId=PO.Id



LEFT JOIN(
SELECT AMTR.Remarks,B.ProductionOrderId,AMTR.AddedDate ,B.[Status]
FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'OtherRMInhouse'
) O ON O.ProductionOrderId=PO.Id



LEFT JOIN(
SELECT AMTR.Remarks,B.ProductionOrderId,AMTR.AddedDate ,B.[Status]
FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'BaseProcessInput'
) I ON I.ProductionOrderId=PO.Id



LEFT JOIN(
SELECT AMTR.Remarks,B.SalesOrderId,AMTR.AddedDate ,B.[Status],B.CriticalityLevel
FROM OrderControlTypes A
JOIN dbo.OrderControl B ON B.ControlTypeId=A.Id
LEFT JOIN dbo.OrderControlRemarks AMTR ON AMTR.OrderControlId=B.Id
AND AMTR.Id=(Select top(1) Id from dbo.OrderControlRemarks Where OrderControlId=B.Id Order by AddedDate desc)
Where A.ControlType= 'ShipmentControl'
) SC ON SC.SalesOrderId=SO.Id



LEFT OUTER JOIN (select PS.ProductionOrderId,min( PS.ProductionDate) ProductionDate from TRN.ProductionSummary PS group by PS.ProductionOrderId) PRDD on PRDD.ProductionOrderId=po.Id 
LEFT OUTER JOIN (select PPT.ProductionOrderID,min(PPT.ProductionDate) ProductionDate from dbo.ProductionPlanningType1 PPT  group by PPT.ProductionOrderID) PLND on PLND.ProductionOrderID=po.Id
LEFT OUTER JOIN TRN.SalesMaterial SM on SM.SalesOrderId=SO.Id



   WHERE os.UserName='Active'
AND MO.PlantId in(" + parameters["PlantId"] + @")
AND MO.EntityId in(" + parameters["EntityId"] + @")
AND MO.PartyId in(" + parameters["CustomerId"] + @")
AND MO.BuyerId in(" + parameters["BuyerId"] + @")
AND MO.ResponsiblePersonId in(" + parameters["ResponsiblePersonId"] + @")
AND MO.OrderStatusId in(" + parameters["MOStatusId"] + @")
AND OSS.Id in(" + parameters["SOStatusId"] + @")
AND ps.Id in(" + parameters["ProductionStatusId"] + @")

" + date + @"

ORDER BY p2.UserName,e.UserName, mo.MasterOrderNo";
            dtOrder = _sqlRepository.GetDataTable(sql);


        }


        #endregion
    }
    public class OpenHeadModelNew
    {
        public string Id { get; set; }
        public string ProductionSummaryId { get; set; }
        public string ProductionBookingParameterId { get; set; }
        public string UserName { get; set; }
        public string Formula { get; set; }
        public string FormulaId { get; set; }
        public decimal Value { get; set; }
        public string EntryState { get; set; }
        public string ValueIN { get; set; }
        public bool IsProduction { get; set; }
    }
}