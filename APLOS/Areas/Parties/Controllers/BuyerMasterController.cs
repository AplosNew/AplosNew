using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Parties;
using Library.Service.Parties;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class BuyerMasterController : BaseController
    {
        private readonly IBuyerMasterService _buyerMasterService;
        private readonly IRepositoryAsync<BuyerMasterActivity> _buyerMasterActivity;
        private readonly ISqlRepository _sqlRepository;
        public BuyerMasterController(IBuyerMasterService buyerMasterService, IRepositoryAsync<BuyerMasterActivity> buyerMasterActivity, ISqlRepository R)
        {
            _buyerMasterService = buyerMasterService;
            _buyerMasterActivity = buyerMasterActivity;
            _sqlRepository = R;
        }

      
        public ActionResult Aplos()
        {
            return View("~/Areas/Parties/Views/BuyerMaster.cshtml");
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string entityId, string buyerId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerMasterService.Query(parameters, identity.CompanyGroupId, entityId, buyerId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllBuyerMasterList(GridParameter parameters, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerMasterService.Query(parameters, identity.CompanyGroupId, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetABuyerMasterList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_buyerMasterService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailList(string masterId)
        {
            return Json(_buyerMasterService.QueryDetail(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetActitvityList(string masterDetailId)
        {
            return Json(_buyerMasterService.QueryActivity(masterDetailId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BuyerMaster buyerMaster, IEnumerable<BuyerMasterDetail> buyerMasterDetails, IEnumerable<BuyerMasterActivity> activityList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            buyerMaster.CompanyGroupId = identity.CompanyGroupId;
            _buyerMasterService.InsertGraph(buyerMaster, buyerMasterDetails, activityList);
            return Json(new { BuyerMaster = buyerMaster, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(BuyerMaster buyerMaster)
        {
            _buyerMasterService.Update(buyerMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult CreateActivity(IEnumerable<BuyerMasterActivity> activityList, string buyerMasterDetailId)
        {
            _buyerMasterService.InsertUpdateActivity(activityList, buyerMasterDetailId);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _buyerMasterService.DeleteMasterDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public ActionResult DeleteEntity(string id)
        {
            _buyerMasterService.DeleteEntity(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public ActionResult DeleteActivity(string id)
        {
            _buyerMasterService.DeleteActivity(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #region Task
        [HttpGet, Authorize]
        public JsonResult GetTaskTemplateMasterCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select Id, UserName from [dbo].[TaskTemplateMaster] WHERE PlantId='" + identity.PlantId + "'";
                return Json(_sqlRepository.GetCombo(sql, "Id", "UserName"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetMasterOrderEntity()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT distinct M.EntityId,E.Code,E.UserName,REPLACE(CONVERT(CHAR(11), E.EffectiveDate, 106),' ','-') AS [EffectiveDate], 
REPLACE(CONVERT(CHAR(11), E.EffectiveDateUpTo, 106),' ','-') AS [EffectiveDate UpTo],P.UserName Plant,D.UserName Division,U.UserName Unit,Flag= CAST(0 as bit)
FROM TRN.MasterOrder M
LEFT JOIN ORG.Entity E ON E.Id=M.EntityId
LEFT JOIN ORG.Plant P ON P.Id=M.PlantId
LEFT JOIN ORG.Division D ON D.Id=E.DivisionId
LEFT JOIN ORG.Unit U ON U.Id=E.UnitId
Where M.PlantId='"+identity.PlantId+"'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetBuyerMasterEntity(string masterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT BM.*,E.Code,E.UserName,REPLACE(CONVERT(CHAR(11), E.EffectiveDate, 106),' ','-') AS [EffectiveDate], 
REPLACE(CONVERT(CHAR(11), E.EffectiveDateUpTo, 106),' ','-') AS [EffectiveDate UpTo],P.UserName Plant
,D.UserName Division,U.UserName Unit
FROM [dbo].[BuyerMasterEntity] BM
JOIN ORG.Entity E ON E.Id=BM.EntityId
JOIN ORG.Plant P ON P.Id=E.PlantId
JOIN ORG.Division D ON D.Id=E.DivisionId
JOIN ORG.Unit U ON U.Id=E.UnitId
Where BM.BuyerMasterId='"+ masterId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveMOE(List<Dictionary<string, object>> data, string masterId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsBC;
            string _Id = string.Empty;
            try
            {
                #region Entity 

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.BuyerMasterEntity where BuyerMasterId='" + masterId + "'", out dsBC, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + Convert.ToInt64(item["Id"]) + "' AND EntityId='"+item["EntityId"] +"'";

                        if (dv.Count == 0)
                        {
                            AddNewRow(dsBC.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }


                }
                #endregion
                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsBC);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
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
        public JsonResult GetTaskData(string taskTemplateMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            //       var sql = @"SELECT distinct TM.Id TaskMasterId,0 Active,TM.Code,TM.StandardName,TM.UserDefineTask,TM.StandardDays,TM.TaskType
            //               FROM [dbo].[TaskTemplate] TT
            //               LEFT JOIN [dbo].[TaskMaster] TM ON TM.Id=TT.TaskMasterId
            //               LEFT JOIN (SELECT * FROM [dbo].[TaskTemplateMaster] Where Id='" + taskTemplateMasterId + @"') TTM ON TTM.Id=TT.TaskTemplateMasterId
            //WHERE TTM.PlantId='" + identity.PlantId + "' AND TM.ResponsiblePersonCategory='Buyer' AND TT.ResponsiblePersonCategory='Buyer'";

            var sql = @"SELECT distinct TM.Id TaskMasterId,0 Active,TM.Code,TM.StandardName,TM.UserDefineTask,TM.StandardDays,TM.TaskType
                    FROM [dbo].[TaskTemplate] TT
                    LEFT JOIN [dbo].[TaskMaster] TM ON TM.Id=TT.TaskMasterId
                    LEFT JOIN (SELECT * FROM [dbo].[TaskTemplateMaster] Where Id='" + taskTemplateMasterId + @"') TTM ON TTM.Id=TT.TaskTemplateMasterId
					WHERE TTM.PlantId='" + identity.PlantId + "' AND  TT.ResponsiblePersonCategory='Buyer'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public JsonResult GetSavedBuyerTaskData(string buyerMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"SELECT BMT.*,EI.EmployeeName TaskEmployeeName FROM [dbo].[BuyerMasterTask] BMT
                      LEFT JOIN EmployeeInformation EI ON EI.SystemId=BMT.EmpSystemId WHERE BuyerMasterId='" + buyerMasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        private string GetPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "BuyerMasterTask", out idFromDB);
            systemID = idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        [HttpPost, Authorize]
        public JsonResult CreateTask(IEnumerable<BuyerMasterTask> entities)
        {

            try
            {
                SaveTaskData(entities);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        

        private void SaveTaskData(IEnumerable<BuyerMasterTask> entities)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster = null;
            try
            {
                if (entities == null)
                    return;
                foreach (var item in entities)
                {
                    string sql = "SELECT * FROM [dbo].[BuyerMasterTask] WHERE Id='" + item.Id + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (!string.IsNullOrEmpty(item.Id) && item.Active)
                    {
                        //edit
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["BuyerMasterId"] = item.BuyerMasterId;
                        dr["TaskMasterId"] = item.TaskMasterId;
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                    if (string.IsNullOrEmpty(item.Id) && item.Active)
                    {
                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = GetPK();
                            dr["BuyerMasterId"] = item.BuyerMasterId;
                            dr["TaskMasterId"] = item.TaskMasterId;
                            dr["EmpSystemId"] = item.EmpSystemId;
                            dr["Active"] = item.Active;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                    }
                    else if (!string.IsNullOrEmpty(item.Id) && !item.Active)
                    {
                        DeleteBuyerTaskData(item.Id);
                    }
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        public void DeleteBuyerTaskData(string SystemID)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.BuyerMasterTask WHERE Id = '" + SystemID + "'";
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
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        #endregion

    }
    
    public class BuyerMasterTask : BaseModel
    {
        public string Id { get; set; }
        public string BuyerMasterId { get; set; }
        public string EmpSystemId { get; set; }
        public string TaskMasterId { get; set; }
        public bool Active { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
}