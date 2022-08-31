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
                                    ,E.DepartmentId
                                    ,E.DivisionId
									,E.SectionId
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
							    FROM [dbo].[PlanningTypesResponsiblePerson] PE
							    LEFT JOIN  EmployeeInformation E ON E.SystemId=PE.EmpSystemId
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON E.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON E.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
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
                                    ,E.DepartmentId
                                    ,E.DivisionId
									,E.SectionId
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
							    LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON E.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON E.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
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
        public ActionResult GetWorkCenterList(string processId, string subprocessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT WCM.Id AS WorkCenterMasterId,e.UserName AS Entity,p.UserName AS Plant
	                             , WCM.EntityId, WCM.Code, WCM.UserName,WCM.Capacity,WCM.UoMId,uom.Code UOM
                            FROM SCS.WorkCenterMaster AS WCM
                            INNER JOIN org.Entity AS e ON e.Id=wcm.EntityId
                            INNER JOIN org.Plant AS p ON p.Id=wcm.PlantId
                            LEFT JOIN SCS.UnitOfMeasurement AS uom ON uom.Id = WCM.UoMId
                            WHERE WCM.PlantId='" + identity.PlantId + "' AND WCM.ProcessId='" + processId + "'  AND ISNULL(WCM.Id,'') IN(SELECT WorkCenterMasterId FROM [SCS].[WorkCenterMasterSubProcess] WHERE SubProcessId='"+ subprocessId + "') order by p.userName, e.UserName,WCM.sequence";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
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


        #endregion
    }
}