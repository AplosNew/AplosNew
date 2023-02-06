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

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class ProductIntegrityAnalysisMasterController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public ProductIntegrityAnalysisMasterController(ISqlRepository R)
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
        public ActionResult LoadProductIntegrityAnalysisMasterList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * ,(select MP.Code from MST.ManpowerBudget MP where MP.Id=SM.ResponsiblePersoneBgtCodeId) as ResponsiblePersoneBgtCode,
(select D.UserName Department from Org.Department D where D.Id=SM.DepartmentId) as Department,
                            (select P.UserName from HKP.Process P where P.Id=SM.ProcessId) as Process,
							(select P.UserName from HKP.Process P where P.Id=(select ProcessId from HKP.SubProcess SP where SP.Id=SM.SubProcessId)) as SubProcess
                            FROM [TRN].[SkillManagement] SM";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadPIAMEditData(string PIAMID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * ,(select MP.Code from MST.ManpowerBudget MP where MP.Id=SM.ResponsiblePersoneBgtCodeId) as ResponsiblePersoneBgtCode,
(select D.UserName Department from Org.Department D where D.Id=SM.DepartmentId) as Department
                            FROM [TRN].[SkillManagement] SM where SM.Id='" + PIAMID + @"'";
            return Json(new { schedule = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetProcessList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from HKP.Process";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        
        [Authorize, HttpGet]
        public decimal GetItemAutoSequence(string scheduleId)
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(SNO),0) AS SNO FROM [TRN].[SkillManagementItem] where SMID='" + scheduleId + "'");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["SNO"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> ScheduleData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagement] where ScheduleCode='" + ScheduleData["ScheduleCode"] + "'", out DataSet dsSkillManagementCodeValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagement] where StandaredName='" + ScheduleData["StandaredName"] + "'", out DataSet dsSkillManagementSNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagement] where UserName='" + ScheduleData["UserName"] + "'", out DataSet dsSkillManagementUNameValidation, false, "1");
                

                DataSet dsSkillManagement;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [Trn].[SkillManagement] where Id='" + ScheduleData["Id"] + "'", out dsSkillManagement, false, "1");
                string _Id = "";

                #region data update
                if (dsSkillManagement.Tables[0].Rows.Count == 0)
                {
                    if (dsSkillManagementCodeValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Schedule Code Already Exist.");
                    }
                    else if (dsSkillManagementSNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Standared Name Already Exist.");
                    }
                    else if (dsSkillManagementUNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("SkillManagement", out _Id);
                        _Id = "SM" + _Id;
                        ScheduleData["Id"] = _Id;
                        AddNewRow(dsSkillManagement.Tables[0], ScheduleData);
                    }
                }
                else
                {
                    _Id = ScheduleData["Id"].ToString();
                    EditRow(dsSkillManagement.Tables[0].Rows[0], ScheduleData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSkillManagement);

                return Json(new { Error = false, Data = ScheduleData, Message = AplosMessage.Insert });
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

        [HttpPost]
        public ActionResult ScheduleDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                DataSet PositionCount, LevelCount, ItemCount, BudgetCount, TeamCount;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementPositionCode] where SMID='" + id + "'", out PositionCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementItem] where SMID ='" + id + "'", out ItemCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementLevel] where SMID ='" + id + "'", out LevelCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementPersonBudgetCode] where SMID ='" + id + "'", out BudgetCount, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementTeamDefinition] where SMID ='" + id + "'", out TeamCount, false, "1");

                if (PositionCount.Tables[0].Rows.Count == 0 || ItemCount.Tables[0].Rows.Count == 0 || LevelCount.Tables[0].Rows.Count == 0 || BudgetCount.Tables[0].Rows.Count == 0 || TeamCount.Tables[0].Rows.Count == 0)
                {

                    conC.BeginTransaction();
                    conC.executeQuery("delete from TRN.SkillManagement where Id ='" + id + @"'");
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

        [Authorize, HttpPost]
        public ActionResult ItemDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [TRN].[SkillManagementItem] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetBudgetCode()
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
                            where MP.Active = 1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        
        [Authorize, HttpGet]
        public ActionResult LoadItemEditData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT *,
(select Code from [MST].[ManpowerBudget] where Id=ByWhomId) as ByWhom
FROM [TRN].[SkillManagementItem] where Id='" + ItemId + @"'";
            return Json(new { item = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadParameterEditData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from SkillItemParameterDetails where Id='" + ParameterId + @"'";
            return Json(new { Parameter = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        
       
        [Authorize, HttpGet]
        public ActionResult LoadItemDetails(string ScheduleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,(select distinct PerformanceGroup from TRN.SkillManagementLevel where PerformanceGroup=PerformanceGroupId) as PerformanceGroup,
(select Code from [MST].[ManpowerBudget] where Id=ByWhomId) as ByWhom
FROM [TRN].[SkillManagementItem] where SMID ='" + ScheduleId + "' order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getParameterData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM SkillItemParameterDetails where ItemId ='" + ItemId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
      
      
        [HttpPost]
        public JsonResult CreateItem(Dictionary<string, object> ItemData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementItem] where Id<>'" + ItemData["Id"] + "'", out DataSet dsSkillManagementItemValidation, false, "1");

                DataSet dsSkillManagementItem;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[SkillManagementItem] where Id='" + ItemData["Id"] + "'", out dsSkillManagementItem, false, "1");
                string _Id = "";

                #region data update
                if (dsSkillManagementItem.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SkillManagementItem", out _Id);
                    _Id = "SMI" + _Id;
                    ItemData["Id"] = _Id;
                    ItemData["SMID"] = Pid;
                    AddNewRow(dsSkillManagementItem.Tables[0], ItemData);
                }
                else
                {
                    _Id = ItemData["Id"].ToString();
                    ItemData["SMID"] = Pid;
                    EditRow(dsSkillManagementItem.Tables[0].Rows[0], ItemData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSkillManagementItem);

                return Json(new { Error = false, Data = ItemData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [Authorize, HttpPost]
        public JsonResult CreateParameter(Dictionary<string, object> ParameterData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from SkillItemParameterDetails where Id<>'" + ParameterData["Id"] + "'", out DataSet dsItemParameterDetailsValidation, false, "1");

                DataSet dsSkillItemParameterDetails;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from SkillItemParameterDetails where Id='" + ParameterData["Id"] + "'", out dsSkillItemParameterDetails, false, "1");
                string _Id = "";

                #region data update
                if (dsSkillItemParameterDetails.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("SkillItemParameterDetails", out _Id);
                    _Id = "SIP" + _Id;
                    ParameterData["Id"] = _Id;
                    ParameterData["ItemId"] = Pid;
                    AddNewRow(dsSkillItemParameterDetails.Tables[0], ParameterData);
                }
                else
                {
                    _Id = ParameterData["Id"].ToString();
                    ParameterData["ItemId"] = Pid;
                    EditRow(dsSkillItemParameterDetails.Tables[0].Rows[0], ParameterData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsSkillItemParameterDetails);

                return Json(new { Error = false, Data = ParameterData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        #endregion -- Operations
    }
}