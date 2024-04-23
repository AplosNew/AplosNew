using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.HumanResource.Parameter;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class LeaveTransectionController : Controller
    {
        #region Constructor


        ParameterService ps = new ParameterService();
        private readonly ISqlRepository _sqlRepository;

        public LeaveTransectionController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

       
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations


        [Authorize, HttpGet]
        public ActionResult LoadProcessParaMasterList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id Value , UserName Text from ORG.Department";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessParameterEditData(string ProcessParameterID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * FROM [MST].[ProcessParameterMaster]  where Id='" + ProcessParameterID + @"'";
            return Json(new { processparameter = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> ProcessParaMasterData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterMaster] where Code='" + ProcessParaMasterData["Code"] + "'", out DataSet dsProcessParameterMasterCodeValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterMaster] where StandardName='" + ProcessParaMasterData["StandardName"] + "'", out DataSet dsProcessParameterMasterSNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterMaster] where UserName='" + ProcessParaMasterData["UserName"] + "'", out DataSet dsProcessParameterMasterUNameValidation, false, "1");


                DataSet dsProcessParameterMaster;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterMaster] where Id='" + ProcessParaMasterData["Id"] + "'", out dsProcessParameterMaster, false, "1");
                string _Id = "", Id = string.Empty; ;

                #region data update
                if (dsProcessParameterMaster.Tables[0].Rows.Count == 0)
                {

                    if (dsProcessParameterMasterCodeValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Code Already Exist.");
                    }
                    else if (dsProcessParameterMasterSNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Standard Name Already Exist.");
                    }
                    else if (dsProcessParameterMasterUNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("ProcessParameterMaster", out _Id);
                        _Id = "PPM" + _Id;
                        ProcessParaMasterData["Id"] = _Id;
                        AddNewRow(dsProcessParameterMaster.Tables[0], ProcessParaMasterData);
                    }
                }
                else
                {
                    _Id = ProcessParaMasterData["Id"].ToString();
                    EditRow(dsProcessParameterMaster.Tables[0].Rows[0], ProcessParaMasterData);
                }
                #endregion data update


                Id = dsProcessParameterMaster.Tables[0].Rows[0]["Id"].ToString();
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsProcessParameterMaster);

                return Json(new { Id = Id, Error = false, Data = ProcessParaMasterData, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult ProcessParaMasterDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                DataSet EntityCount, AGCount, ProcessCount, PositionCodeCount, ApprovalPersonCount;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterEntity] where PPID='" + id + "'", out EntityCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterActivityGroup] where PPID ='" + id + "'", out AGCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterProcess] where PPID ='" + id + "'", out ProcessCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterPositionCode] where PPID ='" + id + "'", out PositionCodeCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterApprovalPerson] where PPID ='" + id + "'", out ApprovalPersonCount, false, "1");

                if (EntityCount.Tables[0].Rows.Count == 0 && AGCount.Tables[0].Rows.Count == 0 && ProcessCount.Tables[0].Rows.Count == 0 && PositionCodeCount.Tables[0].Rows.Count == 0 && ApprovalPersonCount.Tables[0].Rows.Count == 0)
                {

                    conC.BeginTransaction();
                    conC.executeQuery("delete from [MST].[ProcessParameterMaster] where Id ='" + id + @"'");
                    conC.CommitTransaction();
                }
                else
                {
                    throw new Exception("Transaction are Exists!");
                }
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetParameterMasterSequence()
        {
            try
            {
                return Json(ps.GetParaMasterSequence(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult createParaMaster(Dictionary<string, object> data)
        {
            try
            {
                return Json(new { Error = false, Data = ps.ProcessParameterSave(data), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                string ret = ps.ProcessParameterDelete(id);

                if (ret == "Success")
                {
                    return Json(new { Error = false, Sequence = GetParameterMasterSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Error = true, Message = ret }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        [Authorize, HttpPost]
        public ActionResult GetPositionCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MBD.Id , MBD.Code BudgetCode  , ET.UserName Entity , SD.UserName ShiftDefination , PO.Code PositionCode , PO.UserName PositionName from MST.ManpowerBudget MBD
left join org.Entity ET on ET.Id = MBD.EntityId
left join ShiftDefination SD on SD.SystemID = MBD.ShiftDefinationId
left join org.Position PO on PO.Id = MBD.PositionId
where MBD.Active = 1";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getProcessParameterReasonMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM [HKP].[ProcessParameterReasonMaster] order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getProcessParaReasonMasterData(string ReasonId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * FROM [HKP].[ProcessParameterReasonMaster] PPR where PPR.Id='" + ReasonId + @"' order by SNO";
            return Json(new { ProcessParaReasonMaster = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createParaReasonMaster(Dictionary<string, object> ReasonMasterData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[ProcessParameterReasonMaster] where UserName='" + ReasonMasterData["UserName"] + "'", out DataSet dsProcessParameterReasonMasterReasonValidation, false, "1");

                DataSet dsProcessParameterReasonMaster;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [HKP].[ProcessParameterReasonMaster] where Id='" + ReasonMasterData["Id"] + "'", out dsProcessParameterReasonMaster, false, "1");
                string _Id = "";

                #region data update
                if (dsProcessParameterReasonMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("ProcessParameterReasonMaster", out _Id);
                    _Id = "PPR" + _Id;
                    ReasonMasterData["Id"] = _Id;
                    AddNewRow(dsProcessParameterReasonMaster.Tables[0], ReasonMasterData);
                }
                else
                {
                    _Id = ReasonMasterData["Id"].ToString();
                    EditRow(dsProcessParameterReasonMaster.Tables[0].Rows[0], ReasonMasterData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsProcessParameterReasonMaster);

                return Json(new { Error = false, Data = ReasonMasterData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult ProcessParaReasonMasterDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [HKP].[ProcessParameterReasonMaster] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessParameterEntityDetails(string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN PPE.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,PPE.Id,E.Id EntityId,E.EntityType,E.UserName Entity,E.Code,PPE.Remarks 
                            from ORG.Entity E
							LEFT JOIN [MST].[ProcessParameterEntity] PPE ON PPE.EntityId=E.Id and PPE.PPID='" + MasterId + @"'
                            where E.Active = 1 order by PPE.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createProcessParameterEntity(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[ProcessParameterEntity]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where PPID='" + Pid + "'");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and PPID='" + item["PPID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "PPE" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult createProcessParaActivityGroup(Dictionary<string, object> ActivityGroupData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterActivityGroup] where ActivityGroupName='" + ActivityGroupData["ActivityGroupName"] + "'", out DataSet dsProcessParameterAGValidation, false, "1");

                DataSet dsProcessParameterAG;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterActivityGroup] where Id='" + ActivityGroupData["Id"] + "'", out dsProcessParameterAG, false, "1");
                string _Id = "";

                #region data update
                if (dsProcessParameterAG.Tables[0].Rows.Count == 0)
                {
                    if (dsProcessParameterAGValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Activity Group Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("ProcessParameterAG", out _Id);
                        _Id = "PAG" + _Id;
                        ActivityGroupData["Id"] = _Id;
                        ActivityGroupData["PPID"] = Pid;
                        AddNewRow(dsProcessParameterAG.Tables[0], ActivityGroupData);
                    }
                }
                else
                {
                    _Id = ActivityGroupData["Id"].ToString();
                    ActivityGroupData["PPID"] = Pid;
                    EditRow(dsProcessParameterAG.Tables[0].Rows[0], ActivityGroupData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsProcessParameterAG);

                return Json(new { Error = false, Data = ActivityGroupData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessParaActivityGroupDetails(string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from [MST].[ProcessParameterActivityGroup] where PPID ='" + MasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessParaActivityGroupEditData(string AGId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from [MST].[ProcessParameterActivityGroup] AG where AG.Id='" + AGId + @"'";
            return Json(new { activitygroup = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ProcessParaActivityGroupDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[ProcessParameterActivityGroup] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessParaProcessDetails(string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN PPP.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,PPP.Id,P.Id ProcessId,P.UserName Process,P.Code,PAG.Id as ActivityGroupId,PAG.ActivityGroupName,PPP.Remarks
                            from hkp.Process P
							LEFT JOIN [MST].[ProcessParameterProcess] PPP ON PPP.ProcessId=P.Id and PPP.PPID='" + MasterId + @"'
							LEFT JOIN  MST.ProcessParameterActivityGroup PAG ON PAG.Id=PPP.ActivityGroupId
                            where P.Active = 1 order by PPP.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPPActivityGroupList() 
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,ActivityGroupName as Text from MST.ProcessParameterActivityGroup";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createProcessParameterProcess(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[ProcessParameterProcess]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                //ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                //conC.BeginTransaction();
                //conC.executeQuery("delete from " + TableName + " where PPID='" + Pid + "'");
                //conC.CommitTransaction();

                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and PPID='" + item["PPID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "PPP" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getProcessParameterData(string ProcessParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,
(select Code from [MST].[ManpowerBudget] where Id=PPI.ByWhomId) as ByWhom,
(select P.UserName from hkp.process P where P.Id=PPI.ProcessId) as Process,
(select U.UserName from SCS.UnitOfMeasurement U where U.Id=PPI.UOMId) as UOM,
(select PM.UserName from HKP.ProcessParaMaster PM where PM.Id=PPI.ParameterId) as ParameterName,
PPI.ActivityGroup as AGroup
FROM [MST].[ProcessParameterItem] PPI where ProcessParameterId ='" + ProcessParameterId + "' order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessParameterItemDetails(string ProcessParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,
(select Code from [MST].[ManpowerBudget] where Id=PPI.ByWhomId) as ByWhom,
(select P.UserName from hkp.process P where P.Id=PPI.ProcessId) as Process,
(select U.UserName from SCS.UnitOfMeasurement U where U.Id=PPI.UOMId) as UOM,
(select PM.UserName from HKP.ProcessParaMaster PM where PM.Id=PPI.ParameterId) as ParameterName,
PPI.ActivityGroup as AGroup
FROM [MST].[ProcessParameterItem] PPI where ProcessParameterId ='" + ProcessParameterId + "' order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessParameterItemEditData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT *,
(select Code from [MST].[ManpowerBudget] where Id=PPI.ByWhomId) as ByWhom,
(select P.UserName from hkp.process P where P.Id=PPI.ProcessId) as Process,
(select U.UserName from SCS.UnitOfMeasurement U where U.Id=PPI.UOMId) as UOM,
(select PM.UserName from HKP.ProcessParaMaster PM where PM.Id=PPI.ParameterId) as ParameterName,
PPI.ActivityGroup as AGroup
FROM [MST].[ProcessParameterItem] PPI where Id ='" + ItemId + "'";
            return Json(new { ProcessParameter = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetParameterItemList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select PM.Id, PM.Code,PM.StandardName,PM.UserName  from HKP.ProcessParaMaster PM where PM.IsActive = 1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetByWhomeBudgetCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MP.Id ManPowerBudgetId, MP.Code, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,DEG.UserName AS [LegalDesignation] from MST.ManpowerBudget MP
                            left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            where MP.Active = 1 and EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetUOM()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select UM.Id UOMId, UM.Code,UM.StandardName, UM.UserName UOM from scs.UnitOfMeasurement UM where UM.Active = 1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createProcessParameter(Dictionary<string, object> ItemData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterItem] where Id<>'" + ItemData["Id"] + "'", out DataSet dsProcessParameterItemValidation, false, "1");

                DataSet dsProcessParameterItem;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterItem] where Id='" + ItemData["Id"] + "'", out dsProcessParameterItem, false, "1");
                string _Id = "";

                #region data update
                if (dsProcessParameterItem.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("ProcessParameterItem", out _Id);
                    _Id = "PPI" + _Id;
                    ItemData["Id"] = _Id;
                    ItemData["ProcessParameterId"] = Pid;
                    AddNewRow(dsProcessParameterItem.Tables[0], ItemData);
                }
                else
                {
                    _Id = ItemData["Id"].ToString();
                    ItemData["ProcessParameterId"] = Pid;
                    EditRow(dsProcessParameterItem.Tables[0].Rows[0], ItemData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsProcessParameterItem);

                return Json(new { Error = false, Data = ItemData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult ProcessParameterItemDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[ProcessParameterItem] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult getProcessParameterReasonData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT PPR.*,PRM.UserName ReasonName FROM [MST].[ProcessParameterReason] PPR
left join [HKP].[ProcessParameterReasonMaster] PRM on PRM.Id=PPR.ReasonId
where PPR.ParameterId ='" + ParameterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetReasonNameLists()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from [HKP].[ProcessParameterReasonMaster]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessParameterReasonEditData(string ReasonId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from [MST].[ProcessParameterReason] where Id='" + ReasonId + @"'";
            return Json(new { Reason = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createProcessParameterReason(Dictionary<string, object> ReasonData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterReason] where ReasonId='" + ReasonData["ReasonId"] + "' and ParameterId='" + Pid + "'", out DataSet dsProcessParameterReasonValidation, false, "1");

                DataSet dsProcessParameterReason;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterReason] where Id='" + ReasonData["Id"] + "'", out dsProcessParameterReason, false, "1");
                string _Id = "";

                #region data update
                if (ReasonData["SNO"] == null)
                {
                    throw new Exception("SNO is required");
                }
                else
                {
                    if (ReasonData["ReasonId"] == null)
                    {
                        throw new Exception("Reason is required");
                    }
                    else
                    {
                        if (dsProcessParameterReason.Tables[0].Rows.Count == 0)
                        {
                            if (dsProcessParameterReasonValidation.Tables[0].Rows.Count > 0)
                            {
                                throw new Exception("Reason Name Already Exist.");
                            }
                            else
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID("ProcessParameterReason", out _Id);
                                _Id = "PR" + _Id;
                                ReasonData["Id"] = _Id;
                                ReasonData["ParameterId"] = Pid;
                                AddNewRow(dsProcessParameterReason.Tables[0], ReasonData);
                            }
                        }
                        else
                        {
                            _Id = ReasonData["Id"].ToString();
                            ReasonData["ParameterId"] = Pid;
                            EditRow(dsProcessParameterReason.Tables[0].Rows[0], ReasonData);
                        }
                    }
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsProcessParameterReason);

                return Json(new { Error = false, Data = ReasonData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult ParameterReasonDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[ProcessParameterReason] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult getParameterCheckPointsData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM [MST].[ProcessParameterCheckPoints] where ParameterId ='" + ParameterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadParameterCheckPointsEditData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from [MST].[ProcessParameterCheckPoints] where Id='" + ParameterId + @"'";
            return Json(new { Parameter = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult createCheckPoints(Dictionary<string, object> ParameterData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterCheckPoints] where CheckPoints='" + ParameterData["CheckPoints"] + "' and ParameterId='" + Pid + "'", out DataSet dsProcessParameterCheckPointsValidation, false, "1");

                DataSet dsProcessParameterCheckPoints;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProcessParameterCheckPoints] where Id='" + ParameterData["Id"] + "'", out dsProcessParameterCheckPoints, false, "1");
                string _Id = "";

                #region data update
                if (ParameterData["SNO"] == null)
                {
                    throw new Exception("SNO is required");
                }
                else
                {
                    if (ParameterData["CheckPoints"] == null)
                    {
                        throw new Exception("CheckPoints is required");
                    }
                    else
                    {
                        if (dsProcessParameterCheckPoints.Tables[0].Rows.Count == 0)
                        {
                            if (dsProcessParameterCheckPointsValidation.Tables[0].Rows.Count > 0)
                            {
                                throw new Exception("CheckPoints Already Exist.");
                            }
                            else
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID("ProcessParameterCheckPoint", out _Id);
                                _Id = "PCP" + _Id;
                                ParameterData["Id"] = _Id;
                                ParameterData["ParameterId"] = Pid;
                                AddNewRow(dsProcessParameterCheckPoints.Tables[0], ParameterData);
                            }
                        }
                        else
                        {
                            _Id = ParameterData["Id"].ToString();
                            ParameterData["ParameterId"] = Pid;
                            EditRow(dsProcessParameterCheckPoints.Tables[0].Rows[0], ParameterData);
                        }
                    }
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsProcessParameterCheckPoints);

                return Json(new { Error = false, Data = ParameterData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult CheckPointDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[ProcessParameterCheckPoints] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadParameterWorkCenterDetails(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN PPW.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,PPW.Id,WM.Id as WorkCenterMasterId, WM.Code ,WM.UserName Workcenter, WC.UserName WorkcenterCategory, WCS.UserName WorkcenterSubCategory, P.UserName Process, E.UserName Entity, WM.Capacity, UOM.UserName UOM 
                            from SCS.WorkCenterMaster WM
							LEFT JOIN [MST].[ProcessParameterWorkCenter] PPW ON PPW.WorkCenterMasterId=WM.Id and PPW.ParameterId='" + ParameterId + @"'
							LEFT JOIN HKP.WorkCenterCategory WC on WC.Id = WM.WorkCenterCategoryId
                            LEFT JOIN HKP.WorkCenterSubCategory WCS on WCS.Id = WM.WorkCenterSubcategoryId
                            left join HKP.Process P on P.Id = WM.ProcessId
                            left join org.Entity E on E.Id = WM.EntityId
                            LEFT JOIN SCS.UnitOfMeasurement UOM on UOM.Id = WM.UoMId 
                            where WM.Active = 1 
                            --and WM.EntityId in (select EntityId from MST.ProcessParameterEntity 
							--where PPID=(select PPID from MST.ProcessParameterProcess where Id=(select ProcessParameterId from MST.ProcessParameterItem where Id='" + ParameterId + @"'))) 
                            and WM.ProcessId in (select ProcessId from MST.ProcessParameterItem where Id='" + ParameterId + @"') order by PPW.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createWorkCenter(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[ProcessParameterWorkCenter]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {


                objCon = new ConnectionManager.DAL.ConManager("1");
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where ParameterId='" + Pid + "'");
                conC.CommitTransaction();

                if (DataList != null)
                {


                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and ParameterId='" + item["ParameterId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "PPW" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadParameterPositionCodeDetails(string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN PPC.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,PPC.Id,P.Id PositionCodeId,P.Code PositionCode,P.UserName Position,
D.UserName Division,DEP.UserName Department,S.UserName Section,SS.UserName SUbSection,P.Activity,DEG.UserName Designation,PRO.UserName Process,
P.UserReportGroup,PPC.Remarks 
                            from ORG.Position P
							LEFT JOIN ORG.Division D on D.Id=P.DivisionId
							LEFT JOIN ORG.Department DEP on DEP.Id=P.DepartmentId
							LEFT JOIN ORG.Section S on S.Id=P.SectionId
							LEFT JOIN ORG.SubSection SS on SS.Id=P.SubSectionId
							LEFT JOIN hkp.Designation DEG on DEG.Id=P.DesignationId
							LEFT JOIN hkp.Process PRO on PRO.Id=P.ProcessId
							LEFT JOIN [MST].[ProcessParameterPositionCode] PPC ON PPC.PositionCodeId=P.Id and PPC.PPID='" + MasterId + @"'
                            where P.Active = 1 order by PPC.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createPositionCode(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[ProcessParameterPositionCode]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where PPID='" + Pid + "'");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and PPID='" + item["PPID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "PPC" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadParameterAprovalPersonDetails(string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN PAP.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,PAP.Id,MP.Id BudgetCodeId, MP.Code, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,DEG.UserName AS [LegalDesignation] from MST.ManpowerBudget MP
                            left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
							LEFT JOIN [MST].[ProcessParameterApprovalPerson] PAP ON PAP.BudgetCodeId=MP.Id and PAP.PPID='" + MasterId + @"'
                            where MP.Active = 1";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createApprovalPerson(List<Dictionary<string, object>> DataList, string Pid)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[ProcessParameterApprovalPerson]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + " where PPID='" + Pid + "'");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and PPID='" + item["PPID"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "PAP" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessParameterResponsiblePersonDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN PPR.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,PPR.Id,PPR.SNO,PPR.PositionName,EMP.SystemId ResponsiblePersonId, EMP.EmployeeName
                                from EmployeeInformation EMP 
								LEFT JOIN [MST].[ProcessParameterResponsiblePerson] PPR ON PPR.ResponsiblePersonId=EMP.SystemId
                                where EMP.EmployeeStatus = 'Active' order by PPR.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createProcessParameterResponsible(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[ProcessParameterResponsiblePerson]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + "");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "R" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        [Authorize, HttpGet]
        public ActionResult LoadProcessParameterApprovalResponsiblePersonDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN PPA.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,PPA.Id,PPA.SNO,PPA.PositionName,EMP.SystemId ResponsiblePersonId, EMP.EmployeeName
                                from EmployeeInformation EMP 
								LEFT JOIN [MST].[ProcessParameterApprovalResponsiblePerson] PPA ON PPA.ResponsiblePersonId=EMP.SystemId
                                where EMP.EmployeeStatus = 'Active' order by PPA.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createProcessParameterApproval(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[MST].[ProcessParameterApprovalResponsiblePerson]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from " + TableName + "");
                conC.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");

                if (DataList != null)
                {

                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "A" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

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

        #endregion -- Operations
    }
}