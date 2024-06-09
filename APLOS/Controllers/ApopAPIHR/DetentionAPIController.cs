using Aplos.Helpers;
using HRService;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Modules;
using Library.Service.Organizations;
using Library.Service.Securites;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using HRService;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using Library.MaterialManagement.Material;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using static HRService.clsDataContext;

namespace Aplos.Controllers.ApopAPIHR
{
    public class DetentionAPIController : ApiController
    {
        
        clsDataContext clsData = new clsDataContext();
        public DetentionAPIController()
        {

            
        }

        public List<ActiveTask> GetCloseTask(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetCloseTask(out List<ActiveTask> activelist, UserId);
            return activelist;
        }

        public List<closeTask> GetOnTimeTaskAssigned(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetOnTimeTaskAssigned(out List<closeTask> activelists, UserId);
            return activelists;
        }



        public List<closeTask> GetLateTaskAssigned(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetLateTaskAssigned(out List<closeTask> activelists, UserId);
            return activelists;
        }




        public List<closeTask> GetOnTimeTaskCreation(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetOnTimeTaskCreation(out List<closeTask> activelists, UserId);
            return activelists;
        }



        public List<closeTask> GetLateTaskCreation(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetLateTaskCreation(out List<closeTask> activelists, UserId);
            return activelists;
        }

        public List<WorkCenterList> GetWorkCenter(string processid)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.getWorkcenter(out List<WorkCenterList> workcenterlst, processid);
            return workcenterlst;
        }
        public List<DepartmentList> GetDepartment(string detentionid)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.getDepartment(out List<DepartmentList> DepartmentList, detentionid);
            return DepartmentList;
        }
        public List<AllDepartmentList> GetAllDepartment()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.getAllDepartment(out List<AllDepartmentList> DepartmentList);
            return DepartmentList;
        }
        // myappdefault
        public List<MyAppDefaultlist> GetMyAppDefault(string IconName  )
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetMyAppDefault(out List<MyAppDefaultlist> myappdefaultlist, IconName);
            return myappdefaultlist;
        }

        public List<QualificationList> GetQualification()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualification(out List<QualificationList> Qualificationlist);
            return Qualificationlist;
        }


        public List<DetentionTypeList> GetDetentionTypes()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.getDetentionType(out List<DetentionTypeList> detentionTypeIdLst);
            return detentionTypeIdLst;
        }

        public List<DetentionResponsiblePersonList> GetDetentionResponsible(string detentiontypeid)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetDetentionResponsible(out List<DetentionResponsiblePersonList> detResPList, detentiontypeid);
            return detResPList;
        }

        public List<DetentionIssueByNo> GetIssueByNo(string EmployeeId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetIssueByNo(out List<DetentionIssueByNo> detentionIssueByNo, EmployeeId);
            return detentionIssueByNo;
        }

        public List<DetentionLogGridList> GetDetentionLogGrid()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetDetentionLogGrid(out List<DetentionLogGridList> detentionLoggridlist);
            return detentionLoggridlist;
        }

        #region MyAppDefault
        public List<DefaultMyAppIconList> getmyappicon(string userid ,string Iconid )
        {
            clsDataContext clsData = new clsDataContext();
            clsData.getmyappicon(out List<DefaultMyAppIconList> myappiconlis, userid, Iconid);
            return myappiconlis;
        }
        public List<DefaultMyAppIconList> getModuleaccess(string userid, string Moduleid)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.getModuleaccess(out List<DefaultMyAppIconList> myappiconlis, userid, Moduleid);
            return myappiconlis;
        }
        #endregion MyAppDefault
        public List<GetDetentionclose> GetDetentionLogDetail(string from, string to, string departmentId, string detentionTypeId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetDetentionLogDetail(out List<GetDetentionclose> detentionLoggridlist, from, to, departmentId, detentionTypeId);
            return detentionLoggridlist;
        }
        public List<GetDetentionclose> GetDetentionLogDetailfromto(string from, string to)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetDetentionLogDetailfromto(out List<GetDetentionclose> detentionLoggridlist, from, to);
            return detentionLoggridlist;
        }
        public List<GetDetentionclose> GetDetentionLogDetailfromtodepartment(string from, string to, string departmentId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetDetentionLogDetailfromtodepartment(out List<GetDetentionclose> detentionLoggridlist, from, to, departmentId);
            return detentionLoggridlist;
        }
        public List<GetDetentionclose> GetDetentionLogDetailfromtodetention(string from, string to, string detentionTypeId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetDetentionLogDetailfromtodetention(out List<GetDetentionclose> detentionLoggridlist, from, to, detentionTypeId);
            return detentionLoggridlist;
        }

        #region todaydated
        public List<ActiveTask> GetActiveTask(string UserId, string Date)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetActiveTask(out List<ActiveTask> activelist, UserId, Date);
            return activelist;
        }

        public List<ActiveTask> GetCloseTask(string UserId, string Date)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetCloseTask(out List<ActiveTask> activelist, UserId, Date);
            return activelist;
        }
        #endregion todaydated

        #region Task
        public List<Tasks> GetTodayAssignedTask(string UserId, string Date)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetTodayAssignedTask(out List<Tasks> activelists, UserId, Date);
            return activelists;
        }

        public List<ChatTask> GetTaskChats(string Id)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetTaskChats(out List<ChatTask> activelists, Id);
            return activelists;
        }

        public List<AssignTaskDatals> GetTaskAssignedDetail(string Id)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetTaskAssignedDetail(out List<AssignTaskDatals> activelists, Id);
            return activelists;
        }

        public List<Tasks> GetOverDueAssignedTask(string UserId, string Date)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetOverDueAssignedTask(out List<Tasks> activelists, UserId, Date);
            return activelists;
        }

        public List<Tasks> GetNextWeakAssignedTask(string UserId, string Date)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetNextWeakAssignedTask(out List<Tasks> activelists, UserId, Date);
            return activelists;
        }

        public List<Tasks> GetFutureAssignedTask(string UserId, string Date)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetFutureAssignedTask(out List<Tasks> activelists, UserId, Date);
            return activelists;
        }

        public List<Tasks> GetTodayCreateTask(string UserId, string Date)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetTodayCreateTask(out List<Tasks> activelists, UserId, Date);
            return activelists;
        }

        public List<Tasks> GetOverDueCreateTask(string UserId, string Date)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetOverDueCreateTask(out List<Tasks> activelists, UserId, Date);
            return activelists;
        }

        public List<Tasks> GetNextWeakCreateTask(string UserId, string Date)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetNextWeakCreateTask(out List<Tasks> activelists, UserId, Date);
            return activelists;
        }

        public List<Tasks> GetFutureCreateTask(string UserId, string Date)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetFutureCreateTask(out List<Tasks> activelists, UserId, Date);
            return activelists;
        }
        #endregion Task

        [HttpPost]
        public string PostGetDetentionLogGrid([FromBody] IEnumerable<CreateDetentionList> DataToSave)
        {
            try
            {
                string Id = clsData.PostCreateDetention(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpPost]
        public string PostProductionService([FromBody] IEnumerable<ProcessService> DataToSave)
        {
            try
            {
                string Id = clsData.PostProductionService(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpPost]
        public string PostProductionServiceChild([FromBody] IEnumerable<ProcessServiceChild> DataToSave)
        {
            try
            {
                string Id = clsData.PostProductionServiceChild(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpPost]
        public string PostProductionSummaryParameterValue([FromBody] IEnumerable<ParameterGetset> DataToSave)
        {
            try
            {
                string Id = clsData.PostProductionSummaryParameterValue(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]
        public string PostProductionServiceParameter([FromBody] IEnumerable<ProcessServiceParameter> DataToSave)
        {
            try
            {
                string Id = clsData.PostProductionServiceParameter(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        #region Production service Test
        [HttpPost]
        public string PostProductionServiceTest([FromBody] IEnumerable<ProcessServiceTest> DataToSave)
        {
            try
            {
                string Id = clsData.PostProductionServiceTest(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        #endregion Production service Test

        public List<Process> GetProcess()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetProcess(out List<Process> Processlist);
            return Processlist;
        }


        // responsible person
        public List<Process> GetResponsible()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetResponsible(out List<Process> Processlist);
            return Processlist;
        }


        #region written by Aman
        #region AllTaskList
        public List<Tasks> GetMyCreationActive(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetMyCreationActive(out List<Tasks> activelists, UserId);
            return activelists;
        }

        public List<Tasks> GetMyTaskActive(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetMyTaskActive(out List<Tasks> activelists, UserId);
            return activelists;
        }

        public List<Tasks> GetTocheckActive(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetTocheckActive(out List<Tasks> activelists, UserId);
            return activelists;
        }

        public List<Tasks> GetTocrosscheckActive(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetTocrosscheckActive(out List<Tasks> activelists, UserId);
            return activelists;
        }
        public List<Tasks> GetToapprovedActive(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetToapprovedActive(out List<Tasks> activelists, UserId);
            return activelists;
        }


        public List<Tasks> GetMyCreationClose(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetMyCreationClose(out List<Tasks> activelists, UserId);
            return activelists;
        }

        public List<Tasks> GetMyTaskClose(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetMyTaskClose(out List<Tasks> activelists, UserId);
            return activelists;
        }

        public List<Tasks> GetTocheckClose(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetTocheckClose(out List<Tasks> activelists, UserId);
            return activelists;
        }

        public List<Tasks> GetTocrosscheckClose(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetTocrosscheckClose(out List<Tasks> activelists, UserId);
            return activelists;
        }
        public List<Tasks> GetToapprovedClose(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetToapprovedClose(out List<Tasks> activelists, UserId);
            return activelists;
        }

        #endregion AllTaskList

        #region Deshboard
        public List<Default2> GetDeshboard(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetDeshboard(out List<Default2> activelists, UserId);
            return activelists;
        }
        #endregion Deshboard
        public List<Default2> GetEmployee()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetEmployee(out List<Default2> activelists);
            return activelists;
        }

        public List<Default2> GetEmployeeInColumn()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetEmployeeInColumn(out List<Default2> activelists);
            return activelists;
        }

        public List<Default3> GetEmployeeSystem()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetEmployeeSystem(out List<Default3> activelists);
            return activelists;
        }
        public List<Default2> GetReason(string ProcessId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetReason(out List<Default2> activelists, ProcessId);
            return activelists;
        }
        public List<EmployeeInfo> getEmployeedetails(string EmployeeSysId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.getEmployeedetails(out List<EmployeeInfo> activelists, EmployeeSysId);
            return activelists;
        }
        public List<Default2> GetWorkCenterId(string WorkCenter)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetWorkCenterId(out List<Default2> activelists, WorkCenter);
            return activelists;
        }
        public List<PODetail> GetPODetail(string POId, string ProcessId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetPODetail(out List<PODetail> activelists, POId, ProcessId);
            return activelists;
        }

        #region Aman
        public List<Default2> GetProductionStatus()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetProductionStatus(out List<Default2> activelists);
            return activelists;
        }

        public List<Default2> GetCustomerName()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetCustomerName(out List<Default2> activelists);
            return activelists;
        }

        public List<Default2> GetSalesReturnId()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetSalesReturnId(out List<Default2> activelists);
            return activelists;
        }

        public List<Default2> GetTransactionQty(string SalesReturnId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetTransactionQty(out List<Default2> activelists, SalesReturnId);
            return activelists;
        }

        public List<Default2> GetPoStatusWise()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetPoStatusWise(out List<Default2> activelists);
            return activelists;
        }

        public List<Weight> GetCartonBookedQty(string SalesId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetCartonBookedQty(out List<Weight> activelists, SalesId);
            return activelists;
        }

        public List<Default2> GetPoStatusWiseNew(string StatusId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetPoStatusWiseNew(out List<Default2> activelists, StatusId);
            return activelists;
        }

        public List<Default2> GetWrongCarten(string Refno, string SalesId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetWrongCarten(out List<Default2> activelists, Refno, SalesId);
            return activelists;
        }

        public List<Default2> GetProcessTagKg(string ProcessId, string EntityId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetProcessTagKg(out List<Default2> activelists, ProcessId, EntityId);
            return activelists;
        }

        public List<Default2> GetProductionParameterId(string ProcessId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetProductionParameterId(out List<Default2> activelists, ProcessId);
            return activelists;
        }

        public List<Default2> GetShifByProcess(string ProcessId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetShifByProcess(out List<Default2> activelists, ProcessId);
            return activelists;
        }

        public List<ProductionEntryDetail> GetProductionOrderDetail(string ProcessId, string entityId, string productionDate, string shiftId , string Workcenter )
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetProductionOrderDetail(out List<ProductionEntryDetail> activelists, ProcessId, entityId, productionDate, shiftId, Workcenter );
            return activelists;
        }
        public List<PODetailsArtilce> GetPOBaseArticle(string ProcessId, string entityId, string POId, string Workcenter , string BookingLevel)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetPOBaseArticle(out List<PODetailsArtilce> activelists, ProcessId, entityId, POId, Workcenter , BookingLevel);
            return activelists;
        }

        public List<Default2> GetProductionParameter(string ParameterId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetProductionParameter(out List<Default2> activelists, ParameterId);
            return activelists;
        }

        public List<Default2> GetProductionCalculateValue(string Formula)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetProductionCalculateValue(out List<Default2> activelists, Formula);
            return activelists;
        }

        public List<Default2> GetProductionCalculate(string ParameterId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetProductionCalculate(out List<Default2> activelists, ParameterId);
            return activelists;
        }

        public List<POWiseReport> GetPoWisereport(string POId, string POStatusId, string CustomerId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetPoWisereport(out List<POWiseReport> activelists, POId, POStatusId, CustomerId);
            return activelists;
        }

        #endregion Aman
        #endregion written by Aman

        #region Sales Return
        public List<Default2> GetSalesNumber()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetSalesNumber(out List<Default2> activelists);
            return activelists;
        }
        #endregion Sales Return

        #region Attedance
        public List<Default2> GetUserGroup(string EmpsysId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetUserGroup(out List<Default2> activelists , EmpsysId);
            return activelists;
        }

        public List<SevenDaysAttdn> GetSevenDaysAttendance(string Empcode)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetSevenDaysAttendance(out List<SevenDaysAttdn> activelists, Empcode);
            return activelists;
        }

        public List<SevenDaysAttdn> GetSevenDaysAttendanceDefault(string Empcode)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetSevenDaysAttendance(out List<SevenDaysAttdn> activelists, Empcode);
            return activelists;
        }

        public List<EmpInformation> GetEmpInformation(string Empcode)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetEmpInformation(out List<EmpInformation> activelists, Empcode);
            return activelists;
        }

        public List<Locations> GetLocation()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetLocation(out List<Locations> activelists);
            return activelists;
        }

        public List<Default2> GetShift()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetShift(out List<Default2> activelists);
            return activelists;
        }
        public List<AttendanceReport> GetAttdnreport(string date, string shiftid, string groupid, string inmis, string locations, string entityid, string tbs, string longabsent)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetAttdnreport(out List<AttendanceReport> activelists, date, shiftid, groupid, inmis, locations, entityid , tbs, longabsent);
            return activelists;
        }
        #endregion Attedance


      


        #region Seven Days Attendance 
        public string PostPlantinoutcontrl([FromBody] IEnumerable<Plantcontrol> DataToSave)
        {
            try
            {
                string Id = clsData.PostPlantinoutcontrl(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public List<Plantcontrol> GetLastOut(string EmpSysId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetLastOut(out List<Plantcontrol> activelists, EmpSysId);
            return activelists;
        }
        #endregion Seven Days Attendance 


        #region Budget Code Change 

        public string PostChangeBudgetCode([FromBody] IEnumerable<TempBudgetCode> DataToSave)
        {
            try
            {
                string Id = clsData.PostChangeBudgetCode(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public List<TempBudgetCode> GetNewBudgetCode(string EmpsysId, string WorkDate)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetNewBudgetCode(out List<TempBudgetCode> activelists, EmpsysId, WorkDate);
            return activelists;
        }

     

        public string PostUpdateBudgetCodeChange([FromBody] IEnumerable<TempBudgetCode> DataToSave, string EmpsysId, string WorkDate)
        {
            try
            {
                string Id = clsData.PostUpdateBudgetCodeChange(DataToSave, EmpsysId, WorkDate);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        #endregion Budget Code Change 

        // Location  
        public List<Default2> GetCartoonLocation()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetCartoonLocation(out List<Default2> activelists);
            return activelists;
        }

        // Barcode Scan
        public string PostBarcodeScanData([FromBody] IEnumerable<BarcodeScan> DataToSave)
        {
            try
            {
                string Id = clsData.PostBarcodeScanData(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        #region VCehicle
        public string PostVehicleRequisition([FromBody] IEnumerable<Vehicle> DataToSave)
        {
            try
            {
                string Id = clsData.PostVehicleRequisition(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public string PostDailyAccountClosing([FromBody] IEnumerable<AccountBalence> DataToSave )
        {
            try
            {
                string Id = clsData.PostDailyAccountClosing(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string PostUpdateVehicleRequisition([FromBody] IEnumerable<Vehicle> DataToSave, string VehicleId)
        {
            try
            {
                string Id = clsData.PostUpdateVehicleRequisition(DataToSave, VehicleId);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public string PostCancelVehicleRequisition([FromBody] IEnumerable<Vehicle> DataToSave, string VehicleId)
        {
            try
            {
                string Id = clsData.PostCancelVehicleRequisition(DataToSave, VehicleId);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public List<Default2> GetVehicleLocation(string ID)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetVehicleLocation(out List<Default2> activelists , ID);
            return activelists;
        }

        public List<Default2> GetPurpose()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetPurpose(out List<Default2> activelists);
            return activelists;
        }

        public List<Default3> GetPurposeResponsible(string PurposeId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetPurposeResponsible(out List<Default3> activelists, PurposeId);
            return activelists;
        }


        public List<VehicleCreation> GetVehicleCreations(string EmpsysId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetVehicleCreations(out List<VehicleCreation> activelists, EmpsysId);
            return activelists;
        }

        public List<VehicleStatus> GetVehiclestatus(string EmpsysId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetVehiclestatus(out List<VehicleStatus> activelists, EmpsysId);
            return activelists;
        }


        public List<VehicleOutin> GetVehicleOutlist()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetVehicleOutlist(out List<VehicleOutin> activelists);
            return activelists;
        }

        public List<Vehiclecreationdetails> GetVehicleCreationDetail(string MasterId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetVehicleCreationDetail(out List<Vehiclecreationdetails> activelists , MasterId);
            return activelists;
        }

        public List<VehicleOutin> GetVehiclInlist()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetVehiclInlist(out List<VehicleOutin> activelists);
            return activelists;
        }


        public string PostVehicleInOutEntry([FromBody] IEnumerable<VehicleInout> DataToSave, string VInOutId)
        {
            try
            {
                string Id = clsData.PostVehicleInOutEntry(DataToSave, VInOutId);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public List<VehicleOutin> GetVehicleApprove(string EmpSystemId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetVehicleApprove(out List<VehicleOutin> activelists, EmpSystemId);
            return activelists;
        }

        public string PostVehicleTrip([FromBody] IEnumerable<VehicleApproveList> DataToSave)
        {
            try
            {
                string Id = clsData.PostVehicleTrip(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public string PostUpdateVehicleApprove([FromBody] IEnumerable<Vehicle> DataToSave, string VehicleId)
        {
            try
            {
                string Id = clsData.PostUpdateVehicleApprove(DataToSave, VehicleId);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public string PostUpdateVehicleReject([FromBody] IEnumerable<Vehicle> DataToSave, string VehicleId)
        {
            try
            {
                string Id = clsData.PostUpdateVehicleReject(DataToSave, VehicleId);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public string PostCombineVehicleApprove([FromBody] IEnumerable<Vehicle> DataToSave)
        {
            try
            {
                string Id = clsData.PostCombineVehicleApprove(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        #endregion VCehicle

        #region Incedent

        public List<Default2> GetIncedentCategoryDetail(string Id)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetIncedentCategoryDetail(out List<Default2> activelists, Id);
            return activelists;
        }

        public List<Default2> GetIncidentCategory()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetIncidentCategory(out List<Default2> activelists);
            return activelists;
        }

        public List<ROCode> GetEmployeeBudget(string Id)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetEmployeeBudget(out List<ROCode> activelists, Id);
            return activelists;
        }

        public List<Default2> GetRoName(string Id)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetRoName(out List<Default2> activelists, Id);
            return activelists;
        }

        public List<Default2> GetIncidentTitle()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetIncidentTitle(out List<Default2> activelists);
            return activelists;
        }

        public string PostIncedentCreation([FromBody] IEnumerable<Incedent> DataToSave)
        {
            try
            {
                string Id = clsData.PostIncedentCreation(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        #endregion Incedent

        #region Attdn Lock
        public List<Default2> GetCheckAttdnLock(string Date)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetCheckAttdnLock(out List<Default2> activelists, Date);
            return activelists;
        }
        #endregion Attdn Lock

        #region Quality control 
        public List<QualityGenaralIssue> GetQualityGeneraWiseIssue(string ResposibleId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityGeneraWiseIssue(out List<QualityGenaralIssue> activelists, ResposibleId);
            return activelists;
        }

        public List<QualityPOIssue> GetQualityPOWiseIssue(string POIssueDate, string ResponsibleId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityPOWiseIssue(out List<QualityPOIssue> activelists, POIssueDate, ResponsibleId);
            return activelists;
        }

        public List<Default> GetQualityShift(string processId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityShift(out List<Default> activelists, processId);
            return activelists;
        }

        public List<Default> GetQualityPO(string EntityId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityPO(out List<Default> activelists, EntityId);
            return activelists;
        }

        public List<Default> GetQualityPeriod(string IssueId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityPeriod(out List<Default> activelists, IssueId);
            return activelists;
        }

        public string PostQualityHeader([FromBody] IEnumerable<QualityHeader> DataToSave)
        {
            try
            {
                string Id = clsData.PostQualityHeader(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public string PostQualityHeaderChild([FromBody] IEnumerable<QualityHeaderChild> DataToSave)
        {
            try
            {
                string Id = clsData.PostQualityHeaderChild(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public List<Default> GetQualityGrade()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityGrade(out List<Default> activelists);
            return activelists;
        }

        public List<Default> GetQualityActionToBeTaken()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityActionToBeTaken(out List<Default> activelists);
            return activelists;
        }

        public List<Default> GetQualityWorkCenter(string IssueId, string EntityId, string ProcessId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityWorkCenter(out List<Default> activelists, IssueId, EntityId, ProcessId);
            return activelists;
        }

        public List<QualityChild> GetQualityChildList(string IssueId, string PId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityChildList(out List<QualityChild> activelists, IssueId, PId);
            return activelists;
        }

        public List<Default2> GetGIEmployee()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetGIEmployee(out List<Default2> activelists);
            return activelists;
        }

        public List<Default2> GetPIEmployee()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetPIEmployee(out List<Default2> activelists);
            return activelists;
        }

        public List<Default2> GetResponsibleEmployee()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetResponsibleEmployee(out List<Default2> activelists);
            return activelists;
        }
        public List<Default2> GetProductionBookingLevel(string ProcessId, string EntityId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetProductionBookingLevel(out List<Default2> activelists, ProcessId , EntityId);
            return activelists;
        }
        public List<ArticleItem> GetArticleItems(string ProductionOrderId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetArticleItems(out List<ArticleItem> activelists, ProductionOrderId);
            return activelists;
        }
        public string PostQualityProcess([FromBody] IEnumerable<QualityPlanProcess> DataToSave)
        {
            try
            {
                string Id = clsData.PostQualityProcess(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        #endregion Quality control 

        #region Leave
        public List<Leavesystem> GetLeaveBalance(string EmpId, string CalId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetLeaveBalanceType(out List<Leavesystem> activelists, EmpId, CalId);
            return activelists;
        }
        /* public IHttpActionResult GetLeaveBalance(string EmpId, string CalId)
         {
             try
             {
                 clsDataContext app = new clsDataContext();
                 var result = app.GetLeaveBalanceType(EmpId, CalId);
                 //var result = _leaveapp.GetLeaveBalanceType(GroupId, PlantId, EmpId, CalId);
                 return Json(result);
             }
             catch (Exception ex)
             {
                 var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                 {
                     ReasonPhrase = ex.Message
                 };
                 throw new HttpResponseException(resp);
             }
         }*/
        #endregion Leave
        #region EmployeeUserId
        public List<Default2> GetEmployeeUserId(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetEmployeeUserId(out List<Default2> activelists, UserId);
            return activelists;
        }



        #endregion EmployeeUserId

        #region Bank
        public List<Default2> GetBankCategory()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetBankCategory(out List<Default2> activelists);
            return activelists;
        }

        public List<Default2> GetBankSubCategory()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetBankSubCategory(out List<Default2> activelists);
            return activelists;
        }

        public List<Default2> GetBankName(string categoryId, string subcategoryId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetBankName(out List<Default2> activelists, categoryId, subcategoryId);
            return activelists;
        }

        public List<Default2> GetBankAccount(string bankId , string categoryId, string subcategoryId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetBankAccount(out List<Default2> activelists, bankId, categoryId , subcategoryId);
            return activelists;
        }

        public string PostVehicleRequisitionChild([FromBody] IEnumerable<VehicleChild> DataToSave)
        {
            try
            {
                string Id = clsData.PostVehicleRequisitionChild(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion Bank

        #region Gate pass 
        public List<GatePassCheckApprove> GetGatepasschecking(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetGatepasschecking(out List<GatePassCheckApprove> activelists, UserId);
            return activelists;
        }

        public List<GatePassCheckApprove> GetGatepassapproving(string UserId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetGatepassapproving(out List<GatePassCheckApprove> activelists, UserId);
            return activelists;
        }

        public List<Default2> GetGatepassAprovelperson()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetGatepassAprovelperson(out List<Default2> activelists);
            return activelists;
        }

        public string PostGatePassChecking([FromBody] IEnumerable<GatePassCheckApprove> DataToSave, string GatePassId)
        {
            try
            {
                string Id = clsData.PostGatePassChecking(DataToSave, GatePassId);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public string PostGatePassApprove([FromBody] IEnumerable<GatePassCheckApprove> DataToSave, string GatePassId)
        {
            try
            {
                string Id = clsData.PostGatePassApprove(DataToSave, GatePassId);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        #endregion Gate pass 
        #region Leave Approve
        public List<Default2> GetLeaveApprovestatus(string Fmdate , string Todate)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetLeaveApprovestatus(out List<Default2> activelists , Fmdate , Todate);
            return activelists;
        }
        #endregion Leave Approve

        #region Invoice remarks
        public List<Default2> GetInvoiceResponsibleperson()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetInvoiceResponsibleperson(out List<Default2> activelists);
            return activelists;
        }
        public List<Default2> GetInvoiceCustomen( string Respr, string Type)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetInvoiceCustomen(out List<Default2> activelists, Respr, Type);
            return activelists;
        }

        public List<Default2> GetInvoiceNumber(string Respr, string Type, string Customer)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetInvoiceNumber(out List<Default2> activelists, Respr, Type, Customer);
            return activelists;
        }
        public List<InvoiceDataGetset> GetInvoiceData(string ResPer, string Type , string Customer, string InvoiceNo)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetInvoiceData(out List<InvoiceDataGetset> activelists , ResPer, Type, Customer, InvoiceNo);
            return activelists;
        }

        public List<InvoiceDataEntry> GetInvoiceRemarksData(string ActionById)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetInvoiceRemarksData(out List<InvoiceDataEntry> activelists, ActionById);
            return activelists;
        }
        public List<InvoiceRemarksDataInvoice> GetInvoiceRemarksDataInvoice(string InvoiceNo)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetInvoiceRemarksDataInvoice(out List<InvoiceRemarksDataInvoice> activelists, InvoiceNo);
            return activelists;
        }

        public string PostInvoiceRemarks([FromBody] IEnumerable<InvoiceDataEntry> DataToSave)
        {
            try
            {
                string Id = clsData.PostInvoiceRemarks(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        public string PostInvoiceRemarksClos([FromBody] IEnumerable<InvoiceDataEntry> DataToSave, string IRId)
        {
            try
            {
                string Id = clsData.PostInvoiceRemarksClos(DataToSave, IRId);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        public List<Default2> GetEmployeeInColumnWithoutAssociate()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetEmployeeInColumnWithoutAssociate(out List<Default2> activelists);
            return activelists;
        }

        #endregion Invoice remarks
        #region PaymentStatus
        public List<PaymentStatus> GetPaymentStatus(string PartyId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetPaymentStatus(out List<PaymentStatus> activelists, PartyId);
            return activelists;
        }
        public List<INvoiceWiseAccount> GetPaymentstatusInvoiceWise(string PartyId, string RespId, string CustomerType)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetPaymentstatusInvoiceWise(out List<INvoiceWiseAccount> activelists, PartyId , RespId , CustomerType);
            return activelists;
        }
        #endregion PaymentStatus
        #region Quality Acion 
        public List<QualityControll> GetQualityControll(string FromDate, string ToDate, string ResponsiblePersonId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityControll(out List<QualityControll> activelists, FromDate, ToDate, ResponsiblePersonId);
            return activelists;
        }
        public List<QualityControllUpdate> GetQualityActionUpdateParameter(string HeaderId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityActionUpdateParameter(out List<QualityControllUpdate> activelists, HeaderId);
            return activelists;
        }

        public List<Default2> GetEmployeeQualityUpdate()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetEmployeeQualityUpdate(out List<Default2> activelists);
            return activelists;
        }
        public string PostQualityActionUpdate([FromBody] IEnumerable<QualityActionUpdate> DataToSave, string PId, string Status)
        {
            try
            {
                string Id = clsData.PostQualityActionUpdate(DataToSave , PId , Status);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public List<QualityActionUpdate> GetQualityActionUpdate(string ParameterId , string SNO)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityActionUpdate(out List<QualityActionUpdate> activelists, ParameterId, SNO);
            return activelists;
        }

        public List<QualityControll> GetQualityConfirmControll(string ResponsiblePersonId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityConfirmControll(out List<QualityControll> activelists, ResponsiblePersonId);
            return activelists;
        }
        public List<QualityControllUpdate> GetQualityConfirmActionUpdateParameter(string HeaderId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityConfirmActionUpdateParameter(out List<QualityControllUpdate> activelists, HeaderId);
            return activelists;
        }
        public List<QualityActionUpdate> GetQualityConfirmActionUpdate(string ParameterId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetQualityConfirmActionUpdate(out List<QualityActionUpdate> activelists, ParameterId);
            return activelists;
        }
        public string PostQualityConfirmationUpdate([FromBody] IEnumerable<QualityActionUpdate> DataToSave, string PId, string Status , string ConfirmationRemarks , string ConfirmBy)
        {
            try
            {
                string Id = clsData.PostQualityConfirmationUpdate(DataToSave, PId, Status , ConfirmationRemarks , ConfirmBy);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public string PostQualityConfirmationMasterUpdate([FromBody] IEnumerable<QualityConfirmssControllMaster> DataToSave)
        {
            try
            {
                string Id = clsData.PostQualityConfirmationMasterUpdate(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion Quality Acion 
        #region Utility Master
        public List<UtilityMasterGet> GetUtilityTransectionDetail(string UtilityMasterId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetUtilityTransectionDetail(out List<UtilityMasterGet> activelists, UtilityMasterId);
            return activelists;
        }

        public string PostUtilityTransection([FromBody] IEnumerable<UtilityMasterGet> DataToSave)
        {
            try
            {
                string Id = clsData.PostUtilityTransection(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public List<Default2> GetUtilityMasterList()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetUtilityMasterList(out List<Default2> activelists);
            return activelists;
        }
        #endregion Utility Master
        #region Production Entry
        public List<Default2> GetDateFilter(string Time)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetDateFilter(out List<Default2> activelists , Time);
            return activelists;
        }

        public List<Default2> GetTaskEmployee()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetTaskEmployee(out List<Default2> activelists);
            return activelists;
        }

        /* public string Calculate([FromBody] IEnumerable<OpenHeadModelNew> OpenHeadNew)
         {
             try
             {
                 string Id = clsData.Calculate(OpenHeadNew);
                 return Id;
             }
             catch (Exception ex)
             {
                 return ex.ToString();
             }
         }*/
        #endregion Production Entry

        #region Advance
        public List<Default2> GetEmpAttdn(string EmpSysId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetEmpAttdn(out List<Default2> activelists, EmpSysId);
            return activelists;
        }

        public List<AdavnceDetailGetSet> GetEmpAdvanceDetail(string EmpSysId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetEmpAdvanceDetail(out List<AdavnceDetailGetSet> activelists, EmpSysId);
            return activelists;
        }
        #endregion Advance

        #region OrderControlReport
        public List<Default2> GetMoResPer()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetMoResPer(out List<Default2> activelists);
            return activelists;
        }
        public List<Default2> GetOrderStatus()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetOrderStatus(out List<Default2> activelists);
            return activelists;
        }

        public List<OederControllGetSet> GetOrderControlReportDetail(string ResPer, string Type, string Status, string Date, string Days, string ToSP)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetOrderControlReportDetail(out List<OederControllGetSet> activelists, ResPer, Type, Status, Date, Days, ToSP);
            return activelists;
        }
        public string PostQualityControlRemarks([FromBody] IEnumerable<OrderControlRemarksGet> DataToSave)
        {
            try
            {
                string Id = clsData.PostQualityControlRemarks(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string PostShippingdate([FromBody] IEnumerable<ShippingRemarksGet> DataToSave)
        {
            try
            {
                string Id = clsData.PostShippingdate(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion OrderControlReport
    }
}
