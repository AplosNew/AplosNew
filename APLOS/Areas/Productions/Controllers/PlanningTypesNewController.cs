#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;
using System.Collections.Generic;
using System.Data;
using Library.Security.Core;
using Library.Crosscutting.Security;
using System;
using System.Threading;
using System.Web.Script.Serialization;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class PlanningTypesNewController : BaseController
    {
        #region Constructor
        /// <summary>   The PlanningTypesService service. </summary>
        private readonly IPlanningTypesService _planningTypesService;
        private readonly ISqlRepository _sqlRepository;
        public PlanningTypesNewController(IPlanningTypesService planningTypesService, ISqlRepository R)
        {
            _planningTypesService = planningTypesService;
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT PlanningType AS [Value], UserName AS [Text] FROM [dbo].[PlanningTypes]"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_planningTypesService.Query(parameters), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(PlanningTypes planningTypes)
        {
            _planningTypesService.Insert(planningTypes);
            return Json(new { PlanningTypes = planningTypes, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PlanningTypes planningTypes)
        {
            _planningTypesService.Update(planningTypes);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _planningTypesService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost,Authorize]
        public JsonResult CreateResponsiblePersion(List<Dictionary<string, object>> data)

        {
            SaveResponsiblePersionData(data);
            return Json(new { Message = AplosMessage.Insert });
        }


        private void SaveResponsiblePersionData(List<Dictionary<string, object>> data)
        {
            try
            {
                if (data != null)
                {
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.PlanningTypesResponsiblePerson", out dsMaster, false, "1");

                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "EmpSystemId='" + item["EmpSystemId"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = item["EmpSystemId"];
                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
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

        [HttpPost,Authorize]
        public ActionResult DeleteResponsibleEmployee(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.PlanningTypesResponsiblePerson where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        [HttpGet, Authorize]
        public ActionResult GetResponsibleEmployeeData(string PlanningTypesId)
        {
            JsonResult json = Json(GetSavedResponsibleEmployeeData(PlanningTypesId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        public IEnumerable<object> GetSavedResponsibleEmployeeData(string PlanningTypesId)
        {
            try
            {
                string CmdText = @"SELECT PE.Id,PE.EmpSystemId
							    	,E.EmployeeName
							    	,PMB.Code BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
							    	,E.EmpType
							    	,E.GivenDesignationId
									--,EC.Id EmployeeCategoryId
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
                                    ,E.EmployeeCode
									,E.EmpPicPath
                                    ,E.DOJ
                                    ,P.UserName Plant
									,SS.UserName SubSection
                                    ,E.EmployeeCodeNumeric
                                    ,C.UserName Company
							    FROM [dbo].[PlanningTypesResponsiblePerson] PE
							    LEFT JOIN  EmployeeInformation E ON E.SystemId=PE.EmpSystemId
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
                                WHERE PE.PlanningTypesId='"+ PlanningTypesId + "' Order by EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetAllActiveEmployeeData(string PlanningTypesId)
        {
            JsonResult json = Json(GetAllEmployeeData(PlanningTypesId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        public IEnumerable<object> GetAllEmployeeData(string PlanningTypesId)
        {
            try
            {
                string CmdText = @"SELECT CAST (0 AS bit) Flag,E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,PMB.Code BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,E.EmployeeCategorySystemID EmployeeCategoryId
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
                                    ,E.EmployeeCode
									,E.EmpPicPath
                                    ,E.DOJ
                                    ,P.UserName Plant
									,SS.UserName SubSection
                                    ,E.EmployeeCodeNumeric
                                    ,C.UserName Company
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
                                WHERE E.EmployeeStatus='Active' AND E.EmpType<>'Guest' AND E.SystemId NOT IN(SELECT EmpSystemId FROM [dbo].[PlanningTypesResponsiblePerson] WHERE PlanningTypesId='"+ PlanningTypesId + "') Order by EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult GetWorkCenterList(string processId, string subprocessId, string PlantId,string PlanningTypesId)
        {
            string sql = @"SELECT WCM.Id AS WorkCenterMasterId,e.UserName AS Entity,p.UserName AS Plant
, WCM.EntityId, WCM.Code, WCM.UserName,WCM.Capacity,WCM.UoMId,uom.Code UOM
FROM SCS.WorkCenterMaster AS WCM
INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
LEFT JOIN SCS.UnitOfMeasurement AS uom ON uom.Id = WCM.UoMId
WHERE WCM.Active=1 AND WCM.PlantId='"+ PlantId + @"' AND WCM.ProcessId='"+ processId + @"'  
AND ISNULL(WCM.Id,'') IN
(SELECT WorkCenterMasterId FROM [SCS].[WorkCenterMasterSubProcess] WHERE SubProcessId='"+ subprocessId + @"') 
AND WCM.Id NOT IN(Select WorkCenterMasterId from [dbo].[PlanningTypesWorkCenter] Where PlanningTypesId='"+ PlanningTypesId + @"')
order by p.userName, e.UserName,WCM.sequence";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreateWS(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.PlanningTypesWorkCenter where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";


                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PlanningTypesWorkCenter", out _Id);

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


                return Json(new { Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedWCData(string PlanningTypesId)
        {
            JsonResult json = Json(GetSavedWCDataByPlanningType(PlanningTypesId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        public IEnumerable<object> GetSavedWCDataByPlanningType(string PlanningTypesId)
        {
            try
            {
                string CmdText = @"SELECT PWC.*,WCM.UserName WorkCenterMaster,WCM.Capacity,WCM.UoMId,uom.Code UOM FROM [dbo].[PlanningTypesWorkCenter] PWC 
LEFT JOIN SCS.WorkCenterMaster AS wcm ON wcm.Id=PWC.WorkCenterMasterId
LEFT JOIN SCS.UnitOfMeasurement AS uom ON uom.Id = WCM.UoMId
WHERE PWC.PlanningTypesId='" + PlanningTypesId + "'";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetShiftList(GridParameter parameters, string ShiftDefinationIDs, string plantId, string wcids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_planningTypesService.GetShiftList(parameters, identity.CompanyGroupId, plantId, new JavaScriptSerializer().Deserialize<string[]>(ShiftDefinationIDs), wcids), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetMinute(Dictionary<string, object> data)
        {
            //var ts = Convert.ToDateTime(data["ProductionShiftEndTime"]).Subtract(Convert.ToDateTime(data["ProductionShiftStartTime"]));
            int minutes = 0;
            if (!string.IsNullOrEmpty(data["ProductionShiftStartTime"].ToString())&& !string.IsNullOrEmpty(data["ProductionShiftEndTime"].ToString()))
            {
                DateTime date1 = Convert.ToDateTime(data["ProductionShiftStartTime"]);
                DateTime date2 = Convert.ToDateTime(data["ProductionShiftEndTime"]);
                TimeSpan ts = date2 - date1;
                minutes = (int)ts.TotalMinutes; 
            }

            return Json(minutes, JsonRequestBehavior.AllowGet);
        }

        [HttpPost,Authorize]
        public JsonResult CreateShift(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.PlanningTypesShift where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                DateTime date1 = Convert.ToDateTime(data["ProductionShiftStartTime"]);
                DateTime date2 = Convert.ToDateTime(data["ProductionShiftEndTime"]);
                DateTime NextDayDate = date2.AddDays(1);
                TimeSpan ts = date2 - date1;
                TimeSpan Nd = NextDayDate - date1;
                int minutes = (int)ts.TotalMinutes;

               

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PlanningTypesShift", out _Id);
                    if (minutes >= 720 || minutes < 0)
                    {
                        data["ProductionShiftEndTime"] = NextDayDate;
                        data["ProductionTime"] = Nd.TotalMinutes;
                    }
                    else
                    {
                        data["ProductionShiftEndTime"] = date2;
                        data["ProductionTime"] = ts.TotalMinutes;
                    }
                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();

                    if (minutes >= 720 || minutes < 0)
                    {
                        data["ProductionShiftEndTime"] = NextDayDate;
                        data["ProductionTime"] = Nd.TotalMinutes;
                    }
                    else
                    {
                        data["ProductionShiftEndTime"] = date2;
                        data["ProductionTime"] = ts.TotalMinutes;
                    }
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new { Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedShiftData(string PlanningTypesId)
        {
            JsonResult json = Json(GetSavedShiftDataByPlanningType(PlanningTypesId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        public IEnumerable<object> GetSavedShiftDataByPlanningType(string PlanningTypesId)
        {
            try
            {
                string CmdText = @"SELECT PWC.[Id]
      ,PWC.[ShiftId]
      ,PWC.[PlanningTypesId]
      ,ISNULL(format(PWC.ProductionShiftStartTime,'hh:mm tt'),'') ProductionShiftStartTime
	  ,ISNULL(format(PWC.ProductionShiftEndTime,'hh:mm tt'),'') ProductionShiftEndTime
      ,PWC.[ProductionTime]
      ,PWC.[Remark]
      ,PWC.[IsExceptionApplicable]
      ,PWC.[AddedBy]
      ,PWC.[AddedDate]
      ,PWC.[AddedFromIP]
      ,PWC.[UpdatedBy]
      ,PWC.[UpdatedDate]
      ,PWC.[UpdatedFromIP]
	  ,WCM.UserName Shift
  FROM [dbo].[PlanningTypesShift] PWC 
LEFT JOIN dbo.ShiftDefination AS wcm ON wcm.SystemId=PWC.ShiftId
WHERE PWC.PlanningTypesId='" + PlanningTypesId + "'";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateWeek(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.PlanningTypesWeekDays where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PlanningTypesWeekDays", out _Id);
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


                return Json(new { Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedWeekData(string PlanningTypesId)
        {
            JsonResult json = Json(GetSavedWeekDataByPlanningType(PlanningTypesId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        public IEnumerable<object> GetSavedWeekDataByPlanningType(string PlanningTypesId)
        {
            try
            {
                string CmdText = @"SELECT PWC.* FROM [dbo].[PlanningTypesWeekDays] PWC WHERE PWC.PlanningTypesId='" + PlanningTypesId + "'";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateHoliday(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.PlanningTypesHoliday where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PlanningTypesHoliday", out _Id);
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


                return Json(new { Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedHolidayData(string PlanningTypesId)
        {
            JsonResult json = Json(GetSavedHolidayDataByPlanningType(PlanningTypesId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        public IEnumerable<object> GetSavedHolidayDataByPlanningType(string PlanningTypesId)
        {
            try
            {
                string CmdText = @"SELECT PWC.*,FORMAT(PWC.HolidayDate,'dd-MMM-yyyy')HD FROM [dbo].[PlanningTypesHoliday] PWC WHERE PWC.PlanningTypesId='" + PlanningTypesId + "'";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


      
        [HttpPost, Authorize]
        public JsonResult CreateDate(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.PlanningTypesDate Where Id='"+data["Id"]+"'", out dsMaster, false, "1");
                DateTime sdate = Convert.ToDateTime(data["FromDate"]);
                DateTime edate = Convert.ToDateTime(data["ToDate"]);
                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    while (sdate <= edate)
                    {
                        long n = long.Parse(sdate.ToString("yyyyMMddHHmmss"));
                        _Id = n.ToString();
                        data["Id"] = n.ToString(); 
                        data["PlanningDate"] = sdate; 
                        AddNewRow(dsMaster.Tables[0], data);
                        sdate = sdate.AddDays(1);
                    }
                   
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new { Message = AplosMessage.Success });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedDateData(string PlanningTypesId)
        {
            JsonResult json = Json(GetSavedDateDataByPlanningType(PlanningTypesId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        public ActionResult GetLatestPlanDate(string PlanningTypesId)
        {
            try
            {
                string sql = @"SELECT FORMAT(DATEADD(day, 1, MAX(PlanningDate)),'dd-MMM-yyyy') FromDate FROM dbo.PlanningTypesDate AS pt  WHERE PlanningTypesId='" + PlanningTypesId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSavedDateDataByPlanningType(string PlanningTypesId)
        {
            try
            {
                string CmdText = @"SELECT [Id],[PlanningTypesId],FORMAT([PlanningDate],'dd-MMM-yyyy')PlanningDate,[AddedBy],[AddedDate],[AddedFromIP],[UpdatedBy],[UpdatedDate],[UpdatedFromIP] FROM [dbo].[PlanningTypesDate] WHERE PlanningTypesId='" + PlanningTypesId + "'";
                return _sqlRepository.GetDataCollection(CmdText);
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

        public ActionResult GetPlanCapacityDataByPlanningType(string PlanningTypesId)
        {
            try
            {
                string sql = @"SELECT  NULL Id,PW.WorkCenterMasterId,WCM.UserName WorkCenter,FORMAT(WCD.StartDate,'dd-MMM-yyyy')EffectiveDate,FORMAT(PD.PlanningDate,'dd-MMM-yyyy')PlanningDate,ps.ShiftId,sd.UserName [Shift]
,ApplicableShift=CASE WHEN PD.PlanningDate>=WCD.StartDate THEN 0 ELSE 1 END
,WeekOff= CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END,PWD.WeekDays
,Holiday=ISNULL(PHD.HDCount,0)
,NetWorkingShift=CASE WHEN ((CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END)+ISNULL(PHD.HDCount,0)+(CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END))>0 THEN 0 ELSE 1 END
,[FromTime]=CASE WHEN(CASE WHEN (CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END)>0 THEN 0 ELSE 1 END)>0 THEN ISNULL(format(ps.ProductionShiftStartTime,'hh:mm tt'),'')  ELSE NULL END
,[ToTime]=CASE WHEN(CASE WHEN (CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END)>0 THEN 0 ELSE 1 END)>0 THEN ISNULL(format(ps.ProductionShiftEndTime,'hh:mm tt'),'')  ELSE NULL END
,NetWorkingMinute=DATEDIFF(MINUTE,CASE WHEN(CASE WHEN (CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END)>0 THEN 0 ELSE 1 END)>0 THEN ISNULL(format(ps.ProductionShiftStartTime,'hh:mm tt'),'')  ELSE NULL END
,CASE WHEN(CASE WHEN (CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END)>0 THEN 0 ELSE 1 END)>0 THEN ISNULL(format(ps.ProductionShiftEndTime,'hh:mm tt'),'')  ELSE NULL END)
,PlanShift=(CASE WHEN (CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END)>0 THEN 0 ELSE 1 END)
,PlanMinute=(DATEDIFF(MINUTE,CASE WHEN(CASE WHEN (CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END)>0 THEN 0 ELSE 1 END)>0 THEN ISNULL(format(ps.ProductionShiftStartTime,'hh:mm tt'),'')  ELSE NULL END
,CASE WHEN(CASE WHEN (CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END)>0 THEN 0 ELSE 1 END)>0 THEN ISNULL(format(ps.ProductionShiftEndTime,'hh:mm tt'),'')  ELSE NULL END))
*(CASE WHEN (CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END)>0 THEN 0 ELSE 1 END)
,NULL Remark
,Capacity=PW.PlanCapacity * ((DATEDIFF(MINUTE,CASE WHEN(CASE WHEN (CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END)>0 THEN 0 ELSE 1 END)>0 THEN ISNULL(format(ps.ProductionShiftStartTime,'hh:mm tt'),'')  ELSE NULL END
,CASE WHEN(CASE WHEN (CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END)>0 THEN 0 ELSE 1 END)>0 THEN ISNULL(format(ps.ProductionShiftEndTime,'hh:mm tt'),'')  ELSE NULL END)))
,CapacityInVolume=(PW.PlanCapacity * ((DATEDIFF(MINUTE,CASE WHEN(CASE WHEN (CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END)>0 THEN 0 ELSE 1 END)>0 THEN ISNULL(format(ps.ProductionShiftStartTime,'hh:mm tt'),'')  ELSE NULL END
,CASE WHEN(CASE WHEN (CASE WHEN PWD.IsWorkingDays=1 THEN 1 ELSE 0 END)>0 THEN 0 ELSE 1 END)>0 THEN ISNULL(format(ps.ProductionShiftEndTime,'hh:mm tt'),'')  ELSE NULL END))))*PW.AverageLoadFactor
  FROM dbo.PlanningTypesWorkCenter AS PW
LEFT JOIN [SCS].[WorkCenterMaster] WCM ON WCM.Id = PW.WorkCenterMasterId
LEFT JOIN [SCS].[WorkCenterMasterEffectiveDate] WCD ON PW.WorkCenterMasterId=WCD.WorkCenterMasterId
LEFT JOIN [dbo].[PlanningTypesDate] PD ON PD.PlanningTypesId=PW.PlanningTypesId 
LEFT JOIN [dbo].[PlanningTypesShift] PS ON PS.PlanningTypesId=PW.PlanningTypesId 
LEFT JOIN [dbo].ShiftDefination AS sd ON PS.ShiftId=sd.SystemID 
LEFT JOIN [dbo].[PlanningTypesWeekDays] PWD ON PWD.PlanningTypesId=PW.PlanningTypesId AND DATENAME(weekday,PD.PlanningDate)=PWD.WeekDays
LEFT JOIN(Select COUNT(Id) HDCount,PlanningTypesId,HolidayDate FROM [dbo].[PlanningTypesHoliday] GROUP BY PlanningTypesId,HolidayDate) PHD ON PHD.PlanningTypesId=PW.PlanningTypesId AND PD.PlanningDate=PHD.HolidayDate
WHERE PW.PlanningTypesId='" + PlanningTypesId + @"' AND PD.PlanningDate NOT IN(Select PlanningDate from [dbo].[CapacityPlanning] Where PlanningTypesId='"+ PlanningTypesId + @"')
UNION ALL
SELECT CP.Id,CP.WorkCenterMasterId,WCM.UserName WorkCenter,FORMAT(CP.EffectiveDate,'dd-MM-yyyy')EffectiveDate,FORMAT(CP.PlanningDate,'dd-MM-yyyy')PlanningDate,CP.ShiftId,sd.UserName [Shift],CP.ApplicableShift
,CP.WeekOff, NULL WeekDays,0 Hoiliday,CP.NetWorkingShift,CP.FromTime,CP.ToTime,CP.NetWorkingMinute,CP.PlanShift,CP.PlanMinute,CP.Remark,CP.Capacity,CP.CapacityInVolume
FROM [dbo].[CapacityPlanning] CP
LEFT JOIN [SCS].[WorkCenterMaster] WCM ON WCM.Id = CP.WorkCenterMasterId
LEFT JOIN [dbo].ShiftDefination AS sd ON CP.ShiftId=sd.SystemID 
WHERE CP.PlanningTypesId='"+ PlanningTypesId + "'";

                JsonResult json = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateCapacityPlanning(List<Dictionary<string, object>> data)

        {
            SaveCapacityPlanningData(data);
            return Json(new { Message = AplosMessage.Insert });
        }


        private void SaveCapacityPlanningData(List<Dictionary<string, object>> data)
        {
            try
            {
                string _Id = null;
                if (data != null)
                {
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.CapacityPlanning", out dsMaster, false, "1");

                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "CapacityPlanning", out _Id);
                    int c = 0;
                    foreach (var item in data)
                    {
                        c++;
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = _Id+"-"+c;
                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
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

        #endregion
    }
}