#region Using
using Aplos.Controllers;
using Aplos.HumanResource;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
        EmployeeTransport ET = new EmployeeTransport();
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
        public ActionResult Report()
        {
            return View();
        }
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

       #region -- Operations 

        [HttpPost,Authorize]
        public ActionResult SaveUnAssign(List<UARouteEmployeeList> UArouteEmployeeList)
        {
            try
            {
                foreach (var item in UArouteEmployeeList)
                {
                    if (item.RouteId != null && item.StoppageId == null)
                    {
                        Exception ex = new Exception("Please Select Stopage");
                        throw (ex);
                    }
                    

                    if (item.RouteId == null /*&& item.UARouteDownGridId == null*/)
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
                        dr["EmployeeId"] = item.EmployeeId;

                        dr["RouteId"] = item.RouteId;
                        dr["StoppageId"] = item.StoppageId;
                        dr["ShiftId"] = item.ShiftId;

                        //dr["UpRouteId"] = item.UARouteUpGridId;
                        //dr["UpStoppageId"] = item.UAStopageUpGridId;

                        //dr["DownRouteId"] = item.UARouteDownGridId;
                        //dr["DownStoppageId"] = item.UAStopageDownGridId;

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

                        dr["EmployeeId"] = item.EmployeeId;

                        dr["RouteId"] = item.RouteId;
                        dr["StoppageId"] = item.StoppageId;
                        dr["ShiftId"] = item.ShiftId;

                        //dr["UpRouteId"] = item.UARouteUpGridId;
                        //dr["UpStoppageId"] = item.UAStopageUpGridId;

                        //dr["DownRouteId"] = item.UARouteDownGridId;
                        //dr["DownStoppageId"] = item.UAStopageDownGridId;

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

        #region -- New
       
        //Route Emp start
        [HttpGet, Authorize]
        public ActionResult GetRouteEmployeesData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select RS.Id TripId,RS.TripNo,R.Id RouteId,R.StandardName Route,TD.Id TransportId,TD.TransportUserName Transport 
					                        --,RSD.UpDown
											,REPLACE(REPLACE(
                                                    STUFF((select distinct ','+A.UpDown +':'+ISNULL(format(A.StartTime,'hh:mm tt'),'')StartTime from
                                                        RouteScheduleChild A
                                                            
                                                            where A.RouteScheduleId=RS.Id and A.UpDown='Up'  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''
															)
                                                                    ,'&amp;','&'), 'amp;', '') StartTime
											,REPLACE(REPLACE(
                                                    STUFF((select distinct ','+A.UpDown +':'+ISNULL(format(A.EndTime,'hh:mm tt'),'')StartTime from
                                                        RouteScheduleChild A
                                                            
                                                            where A.RouteScheduleId=RS.Id and A.UpDown='Down'  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''
															)
                                                                    ,'&amp;','&'), 'amp;', '') EndTime
											,R.[From],R.[To],SD.UserName [Shift],TD.Capacity Vacancy,TD.PlanCapacity
					                        ,isnull(O.Alloted,0)Alloted,R.PlantId
					                        ,Balance=TD.PlanCapacity-isnull(O.Alloted,0)
					                        ,R.Remarks
											
					                        from RouteSchedule RS
					                        left join [MST].[Route] R on R.Id=RS.RouteId 
					                        left join TransportDetail TD on TD.Id=RS.TransportId
					                        left join ShiftDefination SD on SD.SystemID=RS.ShiftId
					                        LEFT JOIN(select COUNT(A.EmployeeSystemId) Alloted,A.TripId
															from dbo.EmployeeTransportAllocation A
															where A.AssignStatus=1
									                        Group BY TripId) O ON O.TripId=RS.Id
											Where R.PlantId ='" + identity.PlantId+"'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet, Authorize]
        public JsonResult getemployeeDataListRouteEmp(string plantId)
        {
            return Json(ET.GetemployeeDataListRouteEmp(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getemployeeListRoute(string plantId)
        {
            return Json(ET.GetemployeeListRoute(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTransportSummaryData()
        {
            try
            {
                var sql = @"select R.StandardName Route,TD.Id TransportId,TD.TransportNo,TD.TransportUserName Transport,RS.Id TripId,RS.TripNo,TD.Capacity Vacancy
											,TD.PlanCapacity,isnull(O.Alloted,0)Alloted,Balance=TD.PlanCapacity-isnull(O.Alloted,0)
											
					                        from RouteSchedule RS
					                        left join [MST].[Route] R on R.Id=RS.RouteId 
					                        left join TransportDetail TD on TD.Id=RS.TransportId
					                        LEFT JOIN(select COUNT(A.EmployeeSystemId) Alloted,A.TripId
															from dbo.EmployeeTransportAllocation A
															where A.AssignStatus=1
									                        Group BY TripId) O ON O.TripId=RS.Id";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //Route Emp end

        [HttpGet, Authorize]
        public JsonResult viewUnassign(string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(ET.getviewUnassign(PlantId), JsonRequestBehavior.AllowGet);
        }


        #region Save Operations
        [HttpPost]
        public JsonResult employeeTransportAllocationSave(List<Dictionary<string, object>> EmployeeList)
        {

            try
            {
                ET.SaveEmployeeTransportAllocation(EmployeeList);
                return Json(new { Data = EmployeeList, Message = AplosMessage.Insert });
                //return Json(new { Error = "No", Data = rsl.Save( EmployeeList, ResidenceMasterId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetStopageInformation(string routeId)
        {
            string sql = @"select S.Id,S.UserName
                                        from [MST].[RouteStoppage] RS
                                        left join [HKP].[Stoppage] S on S.Id=RS.StoppageId
                                        where RS.RouteId ='"+routeId+"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public JsonResult SaveUnassignData(List<Dictionary<string, object>> employeeList)
        {

            try
            {

                ET.SaveUnassignData(employeeList);
                return Json(new { Data = employeeList, Message = AplosMessage.Insert });
                //return Json(new { Error = "No", Data = rsl.Save( EmployeeList, ResidenceMasterId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Save Operations

        [HttpGet, Authorize]
        public ActionResult getResidenceReportFilters()

        {
            try
            {
                var sql = @"select ei.SystemId EmployeeId,DE.UserName Designation,ei.EmployeeName,S.UserName Section,SS.UserName SubSection,D.UserName Department
                            ,RG.UserName ResidenceGroup,RM.Id ResidenceId,RM.ResidenceNumber,RM.[Block],RM.ResidentType,RM.ResidenceSubCategory
							,E.UserName Entity

							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
                            left join HKP.Designation DE on DE.Id=ei.DesignationSystemID
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = ei.SectionId
                            left join org.SubSection SS on SS.Id = ei.SubSectionId
                            left join org.Department D on D.Id = ei.DepartmentId
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
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
            public string EmployeeId { get; set; }
            public string RouteId { get; set; }
            public string StoppageId { get; set; }
            public string ShiftId { get; set; }

        }
        #endregion
    }
}