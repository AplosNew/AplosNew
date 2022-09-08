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