#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Service.Employees;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class RouteEmployeeController : BaseController
    {
        #region Constructor
        private readonly IRouteEmployeeService _routeEmployeeService;
        private readonly ISqlRepository _sqlRepository;
        public RouteEmployeeController(
              IRouteEmployeeService routeEmployeeService,
              ISqlRepository sqlRepository
            )
        {
            _routeEmployeeService = routeEmployeeService;
            _sqlRepository = sqlRepository;

        }
        #endregion
        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        #region -- Operations 
       
        [HttpPost]
        public ActionResult delete(List<RouteEmployeeList> DeleteEmpList)
        {
            try
            {
                foreach (var item in DeleteEmpList)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsExceptionEmployeeList;
                    try
                    {
                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        string sql = @"Update [TRN].[RouteEmployee] set Active=0 WHERE EmployeeId='" + item.SystemID + @"'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

                    }
                    catch (Exception ex)
                    {

                        throw (ex);
                    } 
                }
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public ActionResult Save(RouteEmployee routeEmployee, List<RouteEmployeeList> routeEmployeeList)
        {
            try
            {
                foreach (var item in routeEmployeeList)
                {
                    if (item.RouteUpGridId != null && item.StopageUpGridId == null)
                    {
                        Exception ex = new Exception("Please Select Up Stopage");
                        throw (ex);
                    }
                    if (item.RouteDownGridId != null && item.StopageDownGridId == null)
                    {
                        Exception ex = new Exception("Please Select Down Stopage");
                        throw (ex);
                    }

                    if (item.RouteUpGridId == null && item.RouteDownGridId == null)
                    {
                        Exception ex = new Exception("Please Select Route");
                        throw (ex);
                    }
                }               
                SaveRouteEmployeeSepLis(routeEmployee, routeEmployeeList);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void SaveRouteEmployeeSepLis(RouteEmployee routeEmployee, List<RouteEmployeeList> routeEmployeeList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {

                foreach (var item in routeEmployeeList)
                {

                    string sql = "SELECT * FROM [TRN].[RouteEmployee] WHERE ID='" + item.Id + "' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    DataView DvMaster = new DataView(dsMaster.Tables[0]);

                    if (DvMaster.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[TRN].[RouteEmployee]", out sID);

                        dr["Id"] = "RE" + sID;
                        dr["EmployeeId"] = item.SystemID;

                        dr["UpRouteId"] = item.RouteUpGridId;
                        dr["UpStoppageId"] = item.StopageUpGridId;

                        dr["DownRouteId"] = item.RouteDownGridId;                        
                        dr["DownStoppageId"] = item.StopageDownGridId;

                        dr["PlantId"] = identity.PlantId;
                        dr["Active"] = true;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = DvMaster[0].Row;
                        dr.BeginEdit();

                        dr["EmployeeId"] = item.SystemID;

                        dr["UpRouteId"] = item.RouteUpGridId;
                        dr["UpStoppageId"] = item.StopageUpGridId;

                        dr["DownRouteId"] = item.RouteDownGridId;
                        dr["DownStoppageId"] = item.StopageDownGridId;

                        dr["PlantId"] = identity.PlantId;
                        dr["Active"] = true;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                    DvMaster.RowFilter = null;
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost,Authorize]
        public ActionResult SaveUnAssign(List<UARouteEmployeeList> UArouteEmployeeList)
        {
            try
            {
                foreach (var item in UArouteEmployeeList)
                {
                    if (item.UARouteUpGridId != null && item.UAStopageUpGridId == null)
                    {
                        Exception ex = new Exception("Please Select Up Stopage");
                        throw (ex);
                    }
                    if (item.UARouteDownGridId != null && item.UAStopageDownGridId == null)
                    {
                        Exception ex = new Exception("Please Select Down Stopage");
                        throw (ex);
                    }

                    if (item.UARouteUpGridId == null && item.UARouteDownGridId == null)
                    {
                        Exception ex = new Exception("Please Select Route");
                        throw (ex);
                    }
                }
                SaveUARouteEmployeeSepLis(UArouteEmployeeList);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void SaveUARouteEmployeeSepLis(List<UARouteEmployeeList> UArouteEmployeeList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {

                foreach (var item in UArouteEmployeeList)
                {

                    string sql = "SELECT * FROM [TRN].[RouteEmployee] WHERE ID='" + item.Id + "' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    DataView DvMaster = new DataView(dsMaster.Tables[0]);

                    if (DvMaster.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[TRN].[RouteEmployee]", out sID);

                        dr["Id"] = "RE" + sID;
                        dr["EmployeeId"] = item.SystemID;

                        dr["UpRouteId"] = item.UARouteUpGridId;
                        dr["UpStoppageId"] = item.UAStopageUpGridId;

                        dr["DownRouteId"] = item.UARouteDownGridId;
                        dr["DownStoppageId"] = item.UAStopageDownGridId;

                        dr["PlantId"] = identity.PlantId;
                        dr["Active"] = true;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = DvMaster[0].Row;
                        dr.BeginEdit();

                        dr["EmployeeId"] = item.SystemID;

                        dr["UpRouteId"] = item.UARouteUpGridId;
                        dr["UpStoppageId"] = item.UAStopageUpGridId;

                        dr["DownRouteId"] = item.UARouteDownGridId;
                        dr["DownStoppageId"] = item.UAStopageDownGridId;

                        dr["PlantId"] = identity.PlantId;
                        dr["Active"] = true;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                    DvMaster.RowFilter = null;
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Gried Drop Down

        [HttpGet, Authorize]
        public ActionResult GetGridUpRouteList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select Id,UserName from [MST].[Route] where UpOrDown='Up' and PlantId='" + identity.PlantId + "' and CompanyId='" + identity.CompanyId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetGridUpStopageList(string RouteUpGridId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select s.Id,s.UserName
                            from [HKP].[Stoppage] s
                            left join [MST].[RouteStoppage] rs on rs.StoppageId=s.Id
                            left join [MST].[Route] r on r.Id=rs.RouteId
                            where r.Id='" + RouteUpGridId + @"' and r.UpOrDown='Up'
                            and r.PlantId='" + identity.PlantId + "' and r.CompanyId='" + identity.CompanyId + "' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        #endregion
        
        [HttpGet, Authorize]
        public ActionResult GetUpRouteList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select Id,UserName from [MST].[Route] where UpOrDown='Up' and PlantId='" + identity.PlantId + "' and CompanyId='" + identity.CompanyId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDownRouteList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select Id,UserName from [MST].[Route] where UpOrDown='Down' and PlantId='" + identity.PlantId + "' and CompanyId='" + identity.CompanyId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetUpDropDownStopageList(string RouteUpId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select s.Id,s.UserName
                            from [HKP].[Stoppage] s
                            left join [MST].[RouteStoppage] rs on rs.StoppageId=s.Id
                            left join [MST].[Route] r on r.Id=rs.RouteId
                            where r.Id='" + RouteUpId + @"' and r.UpOrDown='Up'
                            and r.PlantId='" + identity.PlantId + "' and r.CompanyId='" + identity.CompanyId + "' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDownDropDownStopageList(string RouteDownId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select s.Id,s.UserName
                            from [HKP].[Stoppage] s
                            left join [MST].[RouteStoppage] rs on rs.StoppageId=s.Id
                            left join [MST].[Route] r on r.Id=rs.RouteId
                            where r.Id='" + RouteDownId + @"' and r.UpOrDown='Down'
                            and r.PlantId='" + identity.PlantId + "' and r.CompanyId='" + identity.CompanyId + "' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Authorize]
        public ActionResult getEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT Null AS RouteUpList,Null as StopageUpList,Null AS RouteDownList,Null as StopageDownList, Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy') DOC
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
,re.Id,re.UpRouteId as RouteUpGridId,re.DownRouteId as RouteDownGridId,re.UpStoppageId as StopageUpGridId,re.DownStoppageId as StopageDownGridId
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
										left join [TRN].[RouteEmployee] re on re.EmployeeId=emp.SystemId
                              Where EMP.PlantId='" + identity.PlantId + @"' 
                                AND EMP.EmployeeStatus='Active'  And EMP.CompanyId='" + identity.CompanyId + @"'
AND EMP.SystemId  in (select EmployeeId from [TRN].[RouteEmployee] where Active=1)
                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

            string sqlRouteUpList = @"select Id,UserName From [MST].[Route]	where CompanyId='" + identity.CompanyId + "' and PlantId='" + identity.PlantId + "'	and UpOrDown='Up' ";
            string sqlStopageUpList = @" select re.EmployeeId,sp.Id,sp.UserName
								from EmployeeInformation ei 
								inner join [TRN].[RouteEmployee] re on re.EmployeeId=ei.SystemId
								inner join [MST].[RouteStoppage] rs on re.UpRouteId=rs.RouteId 
								inner join [HKP].[Stoppage] sp on sp.Id=rs.StoppageId 
								inner join [MST].[Route] r on r.Id=rs.RouteId
								where 
								r.UpOrDown='Up'  and ei.PlantId='" + identity.PlantId + @"' 
                                AND ei.EmployeeStatus='Active'  And ei.CompanyId='" + identity.CompanyId + @"'
								order by re.EmployeeId";

            var data = _sqlRepository.GetDataCollection(sql);

            List<Dictionary<string, string>> UpStopage;

            var DTRouteUpList = _sqlRepository.GetDataCollection(sqlRouteUpList);
            DataTable dtUpStopageList = _sqlRepository.GetDataTable(sqlStopageUpList);

            for (int i = 0; i < data.Count; i++)
            {
                data[i]["RouteUpList"] = DTRouteUpList;
                UpStopage = new List<Dictionary<string, string>>();
                dtUpStopageList.DefaultView.RowFilter = "EmployeeId='" + data[i]["SystemID"].ToString() + "'";
                for (int DUP = 0; DUP < dtUpStopageList.DefaultView.Count; DUP++)
                {
                    Dictionary<string, string> _data = new Dictionary<string, string>();
                    _data.Add("Id", dtUpStopageList.DefaultView[DUP]["Id"].ToString());
                    _data.Add("UserName", dtUpStopageList.DefaultView[DUP]["UserName"].ToString());
                    UpStopage.Add(_data);
                }
                data[i]["StopageUpList"] = UpStopage;
            }

            string sqlRouteDownList = @"select Id,UserName From [MST].[Route]	where CompanyId='" + identity.CompanyId + "' and PlantId='" + identity.PlantId + "'	and UpOrDown='Down' ";
            string sqlStopageDownList = @" select re.EmployeeId,sp.Id,sp.UserName
								from EmployeeInformation ei 
								inner join [TRN].[RouteEmployee] re on re.EmployeeId=ei.SystemId
								inner join [MST].[RouteStoppage] rs on re.DownRouteId=rs.RouteId 
								inner join [HKP].[Stoppage] sp on sp.Id=rs.StoppageId 
								inner join [MST].[Route] r on r.Id=rs.RouteId
								where 
								r.UpOrDown='Down'  
                                and ei.PlantId='" + identity.PlantId + @"' 
                                AND ei.EmployeeStatus='Active'  And ei.CompanyId='" + identity.CompanyId + @"'
								order by re.EmployeeId";
            List<Dictionary<string, string>> DownStopage;
            var DTRouteDownList = _sqlRepository.GetDataCollection(sqlRouteDownList);

            DataTable dtDownStopageList = _sqlRepository.GetDataTable(sqlStopageDownList);
            for (int i = 0; i < data.Count; i++)
            {
                data[i]["RouteDownList"] = DTRouteDownList;
                DownStopage = new List<Dictionary<string, string>>();
                dtDownStopageList.DefaultView.RowFilter = "EmployeeId='" + data[i]["SystemID"].ToString() + "'";
                for (int DUP = 0; DUP < dtDownStopageList.DefaultView.Count; DUP++)
                {
                    Dictionary<string, string> _data = new Dictionary<string, string>();
                    _data.Add("Id", dtDownStopageList.DefaultView[DUP]["Id"].ToString());
                    _data.Add("UserName", dtDownStopageList.DefaultView[DUP]["UserName"].ToString());
                    DownStopage.Add(_data);
                }
                data[i]["StopageDownList"] = DownStopage;
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getUpStopage(string RouteId, string UpOrDown)
        {
            string sqlStopageUpList = @"  select sp.Id,sp.UserName from
								[MST].[RouteStoppage] rs
								inner join [HKP].[Stoppage] sp on sp.Id=rs.StoppageId 
								inner join [MST].[Route] r on r.Id=rs.RouteId
								where 
								r.UpOrDown='"+UpOrDown+@"' AND r.Id='"+ RouteId + @"'";

            return Json(_sqlRepository.GetDataCollection(sqlStopageUpList), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult getDownStopage(string RouteId, string UpOrDown)
        {
            string sqlStopageUpList = @"  select sp.Id,sp.UserName from
								[MST].[RouteStoppage] rs
								inner join [HKP].[Stoppage] sp on sp.Id=rs.StoppageId 
								inner join [MST].[Route] r on r.Id=rs.RouteId
								where 
								r.UpOrDown='" + UpOrDown + @"' AND r.Id='" + RouteId + @"'";

            return Json(_sqlRepository.GetDataCollection(sqlStopageUpList), JsonRequestBehavior.AllowGet);
        }
       
        [HttpGet,Authorize]
        public ActionResult getUnassignEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT 
Null AS UARouteUpList,Null as UAStopageUpList,Null AS UARouteDownList,Null as UAStopageDownList  ,
          Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy') DOC
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
		,re.Id,re.UpRouteId as UARouteUpGridId,re.DownRouteId as UARouteDownGridId,re.UpStoppageId as UAStopageUpGridId,re.DownStoppageId as UAStopageDownGridId
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
										Left join [TRN].[RouteEmployee] re on re.EmployeeId=emp.SystemId
                              Where EMP.PlantId='" + identity.PlantId + @"' 
                                AND EMP.EmployeeStatus='Active'  And EMP.CompanyId='" + identity.CompanyId + @"' 
                        AND EMP.SystemId not in (select EmployeeId from [TRN].[RouteEmployee] where Active=1)
                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

            string sqlUARouteUpList = @"select Id,UserName From [MST].[Route]	where CompanyId='" + identity.CompanyId + "' and PlantId='" + identity.PlantId + "'	and UpOrDown='Up' ";
            string sqlUAStopageUpList = @" select re.EmployeeId,sp.Id,sp.UserName
								from EmployeeInformation ei 
								inner join [TRN].[RouteEmployee] re on re.EmployeeId=ei.SystemId
								inner join [MST].[RouteStoppage] rs on re.UpRouteId=rs.RouteId 
								inner join [HKP].[Stoppage] sp on sp.Id=rs.StoppageId 
								inner join [MST].[Route] r on r.Id=rs.RouteId
								where 
								r.UpOrDown='Up'  and ei.PlantId='" + identity.PlantId + @"' 
                                AND ei.EmployeeStatus='Active'  And ei.CompanyId='" + identity.CompanyId + @"'
								order by re.EmployeeId";

            var data = _sqlRepository.GetDataCollection(sql);

            List<Dictionary<string, string>> UAUpStopage;

            var DTRouteUpList = _sqlRepository.GetDataCollection(sqlUARouteUpList);
            DataTable dtUpStopageList = _sqlRepository.GetDataTable(sqlUAStopageUpList);

            for (int i = 0; i < data.Count; i++)
            {
                data[i]["UARouteUpList"] = DTRouteUpList;
                UAUpStopage = new List<Dictionary<string, string>>();
                dtUpStopageList.DefaultView.RowFilter = "EmployeeId='" + data[i]["SystemID"].ToString() + "'";
                for (int DUP = 0; DUP < dtUpStopageList.DefaultView.Count; DUP++)
                {
                    Dictionary<string, string> _data = new Dictionary<string, string>();
                    _data.Add("Id", dtUpStopageList.DefaultView[DUP]["Id"].ToString());
                    _data.Add("UserName", dtUpStopageList.DefaultView[DUP]["UserName"].ToString());
                    UAUpStopage.Add(_data);
                }
                data[i]["UAStopageUpList"] = UAUpStopage;
            }

            string sqlUARouteDownList = @"select Id,UserName From [MST].[Route]	where CompanyId='" + identity.CompanyId + "' and PlantId='" + identity.PlantId + "'	and UpOrDown='Down' ";
            string sqlUAStopageDownList = @" select re.EmployeeId,sp.Id,sp.UserName
								from EmployeeInformation ei 
								inner join [TRN].[RouteEmployee] re on re.EmployeeId=ei.SystemId
								inner join [MST].[RouteStoppage] rs on re.DownRouteId=rs.RouteId 
								inner join [HKP].[Stoppage] sp on sp.Id=rs.StoppageId 
								inner join [MST].[Route] r on r.Id=rs.RouteId
								where 
								r.UpOrDown='Down'  
                                and ei.PlantId='" + identity.PlantId + @"' 
                                AND ei.EmployeeStatus='Active'  And ei.CompanyId='" + identity.CompanyId + @"'
								order by re.EmployeeId";
            List<Dictionary<string, string>> UADownStopage;
            var DTRouteDownList = _sqlRepository.GetDataCollection(sqlUARouteDownList);

            DataTable dtDownStopageList = _sqlRepository.GetDataTable(sqlUAStopageDownList);
            for (int i = 0; i < data.Count; i++)
            {
                data[i]["UARouteDownList"] = DTRouteDownList;
                UADownStopage = new List<Dictionary<string, string>>();
                dtDownStopageList.DefaultView.RowFilter = "EmployeeId='" + data[i]["SystemID"].ToString() + "'";
                for (int DUP = 0; DUP < dtDownStopageList.DefaultView.Count; DUP++)
                {
                    Dictionary<string, string> _data = new Dictionary<string, string>();
                    _data.Add("Id", dtDownStopageList.DefaultView[DUP]["Id"].ToString());
                    _data.Add("UserName", dtDownStopageList.DefaultView[DUP]["UserName"].ToString());
                    UADownStopage.Add(_data);
                }
                data[i]["UAStopageDownList"] = UADownStopage;
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #region UnAssignData

        [HttpGet, Authorize]
        public ActionResult GetUAUpRouteList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select Id,UserName from [MST].[Route] where UpOrDown='Up' and PlantId='" + identity.PlantId + "' and CompanyId='" + identity.CompanyId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getUAUpStopage(string RouteId, string UpOrDown)
        {
            string sqlStopageUpList = @"  select sp.Id,sp.UserName from
								[MST].[RouteStoppage] rs
								inner join [HKP].[Stoppage] sp on sp.Id=rs.StoppageId 
								inner join [MST].[Route] r on r.Id=rs.RouteId
								where 
								r.UpOrDown='" + UpOrDown + @"' AND r.Id='" + RouteId + @"'";

            return Json(_sqlRepository.GetDataCollection(sqlStopageUpList), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetUADownRouteList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select Id,UserName from [MST].[Route] where UpOrDown='Down' and PlantId='" + identity.PlantId + "' and CompanyId='" + identity.CompanyId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult getUADownStopage(string RouteId, string UpOrDown)
        {
            string sqlStopageUpList = @"  select sp.Id,sp.UserName from
								[MST].[RouteStoppage] rs
								inner join [HKP].[Stoppage] sp on sp.Id=rs.StoppageId 
								inner join [MST].[Route] r on r.Id=rs.RouteId
								where 
								r.UpOrDown='" + UpOrDown + @"' AND r.Id='" + RouteId + @"'";

            return Json(_sqlRepository.GetDataCollection(sqlStopageUpList), JsonRequestBehavior.AllowGet);
        }
        #endregion

        public class RouteEmployee : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string RouteUpId { get; set; }
            public string RouteDownId { get; set; }
            public string EmployeeId { get; set; }
            public string PlantId { get; set; }
            public string StopageUpId { get; set; }
            public string StopageDownId { get; set; }
            public string UpDown { get; set; }
            public string RouteId { get; set; }
            public string StopageId { get; set; }
            public string UpDownGrid { get; set; }
            public bool Active { get; set; }

            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }

        public class RouteEmployeeList
        {
            public string Id { get; set; }
            public string SystemID { get; set; }
            public string RouteUpGridId { get; set; }
            public string StopageUpGridId { get; set; }
            public string RouteDownGridId { get; set; }
            public string StopageDownGridId { get; set; }

        }
        public class UARouteEmployeeList
        {
            public string Id { get; set; }
            public string SystemID { get; set; }
            public string UARouteUpGridId { get; set; }
            public string UAStopageUpGridId { get; set; }
            public string UARouteDownGridId { get; set; }
            public string UAStopageDownGridId { get; set; }

        }
        #endregion
    }
}