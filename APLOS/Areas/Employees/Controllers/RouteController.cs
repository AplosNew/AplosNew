#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Threading;
using Library.Data.Sql;
using Library.Crosscutting.Security;
using System;
using System.Data;
using OTSBD;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class RouteController : BaseController
    {
        #region Constructor
        private readonly IRouteService _routeService;
        private readonly IRouteStoppageService _routeStoppageService;
        private readonly ISqlRepository _sqlRepository;

        public RouteController(
              IRouteService routeService
            , IRouteStoppageService routeStoppageService
             , ISqlRepository sqlRepository

            )
        {
            _routeService = routeService;
            _routeStoppageService = routeStoppageService;
            _sqlRepository = sqlRepository;

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
        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_routeService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllDriver(GridParameter parameters)
        {
            return Json(_routeService.GetAllDriver(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select r.UserName,r.Code,r.ShortName,r.StandardName ,e.EmployeeName as DriverName,FM.UserName as FixedAsset
                            ,r.Active,r.DriverId,r.AssetId,r.UpOrDown,r.[Description],r.Remarks,e.EmployeeCode,R.Id
                            from [MST].[Route] r
                            left join EmployeeInformation e on e.SystemId=r.DriverId
                            left join TRN.FixedAssetRegister FAR on FAR.id=r.AssetId
                            LEFT JOIN MST.FixedAssetMaster FM ON FM.Id=FAR.FixedAssetMasterId
                            where r.PlantId='" + identity.PlantId + "' and r.CompanyId='" + identity.CompanyId + "' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet,Authorize]
        public ActionResult getRouteStopage(string RouteId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"  select s.Id as StopagePrimaryId,s.CityId,rs.Sequence,S.Code,S.ShortName,S.StandardName,S.UserName,S.[Description]
	 , COALESCE(rs.UpInTime,rs.DownInTime,'12:00 AM')as [Time]
                              from [HKP].[Stoppage] s 
                              left join  [MST].[RouteStoppage] rs on rs.StoppageId=s.Id
                              left join [MST].[Route] r on r.Id=rs.RouteId
                                where r.Id='" + RouteId + @"'
	                            and s.CompanyId='"+identity.CompanyId+@"' and s.CompanyGroupId='"+identity.CompanyGroupId+ @"' order by rs.Sequence ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet,Authorize]
        public ActionResult getStopageData(string routeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select RS.*,S.UserName,C.UserName as City from MST.RouteStoppage RS
                            left outer join HKP.Stoppage as S on RS.StoppageId=S.Id
                            left outer join SCS.City as C on S.CityId=C.Id
                            where RS.RouteId='" + routeId + "'order by Sequence  ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Authorize]
        public ActionResult GetStopageInformation()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select s.Id as StopagePrimaryId,s.CityId,s.Sequence,S.Code,S.ShortName,S.StandardName,S.UserName,S.[Description]
                            from [HKP].[Stoppage] s 
                                where s.CompanyId='" + identity.CompanyId + "' and s.CompanyGroupId='" + identity.CompanyGroupId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Authorize]
        public ActionResult getDriver()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy') DOC
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
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
                              Where EMP.PlantId='" + identity.PlantId + @"' 
                                AND EMP.EmployeeStatus='Active'  And EMP.CompanyId='" + identity.CompanyId + @"'
                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Authorize]
        public ActionResult getFixedAsset()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select FAR.Id,FAR.SerialNo,FORMAT(FAR.CapitalizationDate,'dd-MMM-yyyy')CapitalizationDate,FAR.YearOfInstallation,FAR.[Description]
                            ,FM.UserName FixedAssetName
                            from TRN.FixedAssetRegister FAR
                            LEFT JOIN MST.FixedAssetMaster FM ON FM.Id=FAR.FixedAssetMasterId
                            where FAR.PlantId='" + identity.PlantId + @"' and FAR.CompanyId='" + identity.CompanyId + @"' and FAR.CompanyGroupId='" + identity.CompanyGroupId + @"'
                            ORDER BY SerialNo";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(RouteModel Route, List<StopageListModel> StopageList)
        {
            try
            {
                string RouteId = string.Empty;
                RouteId = SaveRoute(Route);
                SaveStopateList(Route, StopageList, RouteId, out DataSet dsDelete);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public string SaveRoute(RouteModel Route)
        {
            string Id = string.Empty;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM [MST].[Route] WHERE ID='" + Route.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MST.Route", out sID);
                    Id = "R" + sID;
                    dr["Id"] = Id;
                    dr["PlantId"] = identity.PlantId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["DriverId"] = Route.DriverId;
                    dr["AssetId"] = Route.AssetId;
                    dr["UpOrDown"] = Route.UpOrDown;

                    dr["Code"] = Route.Code;
                    dr["UserName"] = Route.UserName;
                    dr["StandardName"] = Route.StandardName;
                    dr["ShortName"] = Route.ShortName;
                    dr["Description"] = Route.Description;
                    dr["Remarks"] = Route.Remarks;
                    dr["Active"] = Route.Active;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();

                    Id = dr["Id"].ToString();

                    dr["PlantId"] = identity.PlantId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["DriverId"] = Route.DriverId;
                    dr["AssetId"] = Route.AssetId;
                    dr["UpOrDown"] = Route.UpOrDown;

                    dr["Code"] = Route.Code;
                    dr["UserName"] = Route.UserName;
                    dr["StandardName"] = Route.StandardName;
                    dr["ShortName"] = Route.ShortName;
                    dr["Description"] = Route.Description;
                    dr["Remarks"] = Route.Remarks;
                    dr["Active"] = Route.Active;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void SaveStopateList(RouteModel Route, List<StopageListModel> StopageList, string RouteId, out DataSet dsDelete)
        {
            int _Count = 0;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            dsDelete = null;
            try
            {
                string EmpIdLoop = "";

                foreach (var item in StopageList)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "'" + item.StopagePrimaryId + "'";
                    }
                    else
                    {
                        EmpIdLoop += ",'" + item.StopagePrimaryId + "'";

                    }
                }
                DeleteStopage(EmpIdLoop, RouteId, out dsDelete);

                foreach (var item in StopageList)
                {
                     _Count ++;
                    if (item.Time == null)
                    {
                        item.Time = "12:00 AM";
                    }
                    string sql = "SELECT * FROM [MST].[RouteStoppage] WHERE ID='" + item.Id + "' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    DataView DvMaster = new DataView(dsMaster.Tables[0]);

                    if (DvMaster.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MST.RouteStoppage", out sID);

                        dr["Id"] = "RS" + sID;

                        dr["RouteId"] = RouteId;
                        dr["StoppageId"] = item.StopagePrimaryId;

                        if (Route.UpOrDown == "Up")
                            dr["UpInTime"] = item.Time;
                        else
                            dr["DownInTime"] = item.Time;

                        dr["Sequence"] = _Count;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = DvMaster[0].Row;
                        dr.BeginEdit();

                        dr["RouteId"] = item.RouteId;
                        dr["StoppageId"] = item.StoppageId;
                        if (Route.UpOrDown == "Up")
                            dr["UpInTime"] = item.Time;
                        else
                            dr["DownInTime"] = item.Time;
                        dr["Sequence"] = item.Sequence;

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

        void DeleteStopage(string StopagePrimaryId,string RouteId, out System.Data.DataSet dsRef)
        {
           // string strSQL;
            string strSQLR;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"Delete FROM [MST].[RouteStoppage]  where StoppageId in (" + StopagePrimaryId + @")";
                strSQLR = @"Delete FROM [MST].[RouteStoppage]  where RouteId='"+ RouteId+"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
                objCon.OpenDataSetThroughAdapter(strSQLR, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        [HttpGet]
        public ActionResult Delete(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sqlStopage = @"delete from [MST].[RouteStoppage]  WHERE RouteId='" + Id + @"'";
                string sql = @"delete from [MST].[Route] WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlStopage, out dsExceptionEmployeeList, false, "1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        public class RouteModel : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string PlantId { get; set; }
            public string CompanyId { get; set; }
            public string AssetId { get; set; }
            public string DriverId { get; set; }
            public string Code { get; set; }
            public string UserName { get; set; }
            public string StandardName { get; set; }
            public string ShortName { get; set; }
            public string Description { get; set; }
            public string Remarks { get; set; }
            public bool Active { get; set; }
            public string UpOrDown { get; set; }


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
        public class StopageListModel : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string StopagePrimaryId { get; set; }
            public string RouteId { get; set; }
            public string StoppageId { get; set; }
            public DateTime? UpInTime { get; set; }
            public DateTime? DownInTime { get; set; }
            public string Time { get; set; }
            public string Sequence { get; set; }

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
        #endregion
    }
}