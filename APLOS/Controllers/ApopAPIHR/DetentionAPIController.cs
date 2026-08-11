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
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace Aplos.Controllers.ApopAPIHR
{
    public class DetentionAPIController : ApiController
    {
        
        clsDataContext clsData = new clsDataContext();


        private readonly IModuleAppService _moduleAppService;
        private readonly IDesignationService _designationService;
        private readonly IEntityService _entityService;
        private readonly IStructureRelationshipService _structureRelationshipService;
        private readonly IManpowerBudgetJobDescriptionService _manpowerBudgetJobDescriptionService;
        private readonly IManpowerBudgetService _manpowerBudgetService;
        private readonly IPlantService _plantService;
        private readonly ISqlRepository _sqlRepository;
        public DetentionAPIController(IModuleAppService moduleAppService
            , IEntityService entityService
            , IManpowerBudgetJobDescriptionService manpowerBudgetJobDescriptionService
            , IStructureRelationshipService structureRelationshipService
            , IManpowerBudgetService manpowerBudgetService
            , IPlantService plantService
            , IDesignationService designationService
            , ISqlRepository sqlRepository)
        {

            _moduleAppService = moduleAppService;
            _designationService = designationService;
            _entityService = entityService;
            _manpowerBudgetService = manpowerBudgetService;
            _structureRelationshipService = structureRelationshipService;
            _manpowerBudgetJobDescriptionService = manpowerBudgetJobDescriptionService;
            _plantService = plantService;
            _sqlRepository = sqlRepository;
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

        public List<DefaultMyAppIconList> getmyappiconVisibal(string userid, string Iconid)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.getmyappiconVisibal(out List<DefaultMyAppIconList> myappiconlis, userid, Iconid);
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

        public List<Default2> GetShift(string GroupId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetShift(out List<Default2> activelists, GroupId);
            return activelists;
        }
        public List<AttendanceReport> GetAttdnreport(string date, string shiftid, string groupid, string inmis, string locations, string entityid, string tbs, string longabsent, string Budgetcodeid)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetAttdnreport(out List<AttendanceReport> activelists, date, shiftid, groupid, inmis, locations, entityid , tbs, longabsent, Budgetcodeid);
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

        public string PostUpdateParmenentBudgetCodeChange([FromBody] IEnumerable<TempBudgetCode> DataToSave, string EmpsysId)
        {
            try
            {
                string Id = clsData.PostUpdateParmenentBudgetCodeChange(DataToSave, EmpsysId);
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

        #region Pending dispatch
        public List<Default2> GetMoCustomer()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetMoCustomer(out List<Default2> activelists);
            return activelists;
        }

        public List<SocreationGet> GetSODetail(string CustomerId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetSODetail(out List<SocreationGet> activelists, CustomerId);
            return activelists;
        }

        public List<SocreationGet> GetPODetail(string SOId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetPODetail(out List<SocreationGet> activelists, SOId);
            return activelists;
        }

        public string PostPendingDispatchSave([FromBody] IEnumerable<PendingDispatchGet> DataToSave)
        {
            try
            {
                string Id = clsData.PostPendingDispatchSave(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        #endregion Pending dispatch

        #region Daily Inverification
        public List<Default2> GetActiveEmployee(string EmpSysId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetActiveEmployee(out List<Default2> activelists , EmpSysId);
            return activelists;
        }
        public List<Default2> GetTransportEmployee(string EmpSysId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetTransportEmployee(out List<Default2> activelists, EmpSysId);
            return activelists;
        }
        #endregion Daily Inverification

        #region Payslip
        public List<Default2> GetEmployeeBankDetail(string EmpSysId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetEmployeeBankDetail(out List<Default2> activelists, EmpSysId);
            return activelists;
        }
        #endregion Payslip

        #region Addinfo
        public List<Default2> GetSoParty()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetSoParty(out List<Default2> activelists);
            return activelists;
        }
        public List<Default2> GetSO(string Category, string PartyId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetSO(out List<Default2> activelists, Category, PartyId);
            return activelists;
        }
        public List<AddInfoList> GetAddInfoFiled(string Ids, string Category)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetAddInfoFiled(out List<AddInfoList> activelists, Ids, Category);
            return activelists;
        }
        public string PostSalesAddInfo([FromBody] IEnumerable<SalesAddinfo> DataToSave)
        {
            try
            {
                string Id = clsData.PostSalesAddInfo(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        #endregion Addinfo

        #region Auburn
        [HttpPost]
        public string PostScanRawData([FromBody] IEnumerable<PacketScanData> DataToSave)
        {
            try
            {
                string Id = clsData.PostScanRawData(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        public List<Default2> GetLineNo()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetLineNo(out List<Default2> activelists);
            return activelists;
        }

        public List<NewBudgetCodeChange> GetNewBudget(string SystemId, string ShiftId , string LineId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetNewBudget(out List<NewBudgetCodeChange> activelists, SystemId, ShiftId, LineId);
            return activelists;
        }
        #endregion Auburn

        #region Pratibha
        //Entity Wise Work Center
        public List<Default2> GetEntityWiseWC(string Userid)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetEntityWiseWC(out List<Default2> activelists, Userid);
            return activelists;
        }

        #region Ultimo Data

        public string PostUltimoData([FromBody] IEnumerable<UltimoDataGetSetUnitNew> DataToSave)
        {
            try
            {
                string Id = clsData.PostUltimoData(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }


        public string PostUltimoDataUnit2([FromBody] IEnumerable<UltimoDataGetSet> DataToSave)
        {
            try
            {
                string Id = clsData.PostUltimoDataUnit2(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        #endregion Ultimo Data

        public List<Default2> GetHRReportList()
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetHRReportList(out List<Default2> activelists);
            return activelists;
        }


        #region TNA API

        public IHttpActionResult GetTNAReport()
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {

                // return Json(_plantService.GetCboByCompany(companyId));
                return Json(_sqlRepository.GetDataTable(@"SELECT K.*
                                  FROM (SELECT 
                                TAM.ProcessId,CASE WHEN tm.CurrentStatus='Closed' THEN format(tm.ClosingDate,'dd-MMM-yyyy') ELSE NULL END AS ClosingDate,
                                CASE WHEN tm.CurrentStatus='Closed' THEN isnull(USRCL.FullName,isnull(EACL.EmployeeName,TM.ClosedBy)) ELSE NULL END AS ClosedBy,
                                pr.DepartmentId,ATO.ResponsiblePersonId AS AssignToId,AB.ResponsiblePersonId AS AssignById,TM.CurrentStatus,
                               mott.Sequence, isnull(TAM.TaskCategoryId,'')TaskCategoryId,isnull(TAM.TaskSubCategoryId,'')AS TaskSubCategoryId,
                                tc.UserName AS Category,tsc.UserName as SubCategory,FORMAT(TSK.CreatedTime,'dd-MMM-yyyy hh:mm:ss tt') AS LastChatDate,TSK.CommentText AS LastChatComment,
                                format(TT.OriginalSequentialEndDate,'dd-MMM-yyyy') AS DueDate,
                                format(OriginalSequentialStartDate,'dd-MMM-yyyy') AS OriginalSequentialStartDate, format(OriginalSequentialEndDate,'dd-MMM-yyyy') AS OriginalSequentialEndDate,
                                format(TempStartDate,'dd-MMM-yyyy') AS TempStartDate, format(TempEndDate,'dd-MMM-yyyy') AS TempEndDate,
                                concat(TM.TaskType,'/',MO.Dependency) AS TaskType,
                                datediff(day,TT.OriginalSequentialEndDate,TM.closingDate) AS EarlyOrLateBy,
mott.TaskDescription TaskName,
                           tm.TaskDescription AS Task,format(ISNULL(ATO.RevisedCommitmentDate,ISNULL(ATO.CommitmentDate,NULL)),'dd-MMM-yyyy') AS CommitmentDate,
EAB.EmployeeName AS AssignBy,EATO.EmployeeName AS AssignTo,TTD.DependentDatesEnum,TTD.TaskDependentOn,FORMAT(TT.DependentDate,'dd-MMM-yyyy')DependentDate,
                                MO.*
                                 FROM TaskManagerMaster AS tm
                                    inner join (SELECT  'Order' AS Dependency, tt.TaskTemplateId,TMMM.Id AS TaskMasterId, 
                                        MO.MasterOrderNo AS MasterOrderId,MO.BuyerId,
                             B.UserName AS Buyer
                            
                            ,StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
                            trn.MasterOrderItem XMOI 
                            where MO.Id=XMOI.MasterOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            SONo=STUFF((select distinct ','+so.Id from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            LineItemReference=STUFF((select distinct ','+so.LineItemReference from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            SOQty=(select sum(SO.Qty) from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId),

                            PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
                            where MO.Id=XMOI.MasterOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                          
                            ,Department=bd.UserName,Division=bd2.UserName
                            FROM TaskManagerMaster AS TMMM

                              INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMMM.TNATasksId 
                            LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                            inner JOIN [TRN].[MasterOrder] AS MO ON MO.Id = TM.MasterOrderId
                             LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId     
                            UNION

                            SELECT  'Item' AS Dependency, tt.TaskTemplateId, TMM.Id AS TaskMasterId,
                             MO.MasterOrderNo,B.Id, B.UserName AS Buyer
                            ,StyleNo= MOI.BuyerReferenceNo,
                            SONo=STUFF((select distinct ','+so.Id from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            LineItemReference=STUFF((select distinct ','+so.LineItemReference from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                            SOQty=(select sum(so.Qty) from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            where MO.Id=XMOI.MasterOrderId),

                            PRNo=STUFF((select distinct ','+pod.ProductionOrderId from 
                            trn.MasterOrderItem XMOI 
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=xmoi.Id 
                            INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id 
                            where MO.Id=XMOI.MasterOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,Department=bd.UserName,Division=bd2.UserName
                            FROM TaskManagerMaster AS TMM

                            LEFT OUTER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
                            inner JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                            inner JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = TM.MasterOrderItemId
                            LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
                            LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId 
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    


                            UNION 

                            SELECT 'Sales Order' AS Dependency, tt.TaskTemplateId, TMM.Id AS TaskMasterId,
                               MO.MasterOrderNo,B.Id, B.UserName AS Buyer
                            ,StyleNo= MOI.BuyerReferenceNo
                            ,SONo=so.Id
                            ,so.LineItemReference
                            ,SOQty=SO.Qty
                            ,PRNo=STUFF((select distinct ','+xpod.ProductionOrderId from  trn.ProductionOrderDetail AS xpod
                            where xpod.SalesOrderId = so.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                          
                            ,Department=bd.UserName,Division=bd2.UserName
                            FROM TaskManagerMaster AS TMM

                              INNER JOIN [dbo].[TNATasks] AS TT ON TT.Id = TMM.TNATasksId 
                            LEFT OUTER JOIN TNAMaster AS TM ON TM.Id = TT.TNAMasterId
                            inner JOIN [TRN].[SalesOrder] AS SO ON SO.Id =  TM.SalesOrderId
                            LEFT OUTER JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.Id = SO.MasterOrderItemId
                            LEFT OUTER JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
                            LEFT OUTER JOIN [HKP].[Buyer] AS B ON B.Id = MO.BuyerId
                            LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=mo.BuyerDepartmentId    LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=mo.BuyerDivisionId    
                            UNION 

                          

                            SELECT 'Prod. Order' AS Dependency,tt.TaskTemplateId, TMM.Id AS TaskMasterId, 
                               pr.MasterOrderId,PR.BuyerId,pr.Buyer,pr.StyleNo, pr.SONo,pr.LineItemReference,PR.SOQty, pr.ProductionOrderId
                            ,Department=bd.UserName,Division=bd2.UserName

                                 FROM TaskManagerMaster AS tmm
                                INNER JOIN TNATasks AS TT ON TT.Id=tmm.TNATasksId
                                INNER JOIN TNAMaster AS T ON t.Id=tt.TNAMasterId  AND isnull(t.ProductionOrderId,'')<>''
                                    INNER JOIN trn.ProductionOrder AS po ON PO.Id=t.ProductionOrderId
                                INNER JOIN
                                (
                                SELECT distinct po.Id AS ProductionOrderId,mo.BuyerDepartmentId,mo.BuyerDivisionId,
                                b.Id AS BuyerId,b.UserName AS Buyer,
                                
                                MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
trn.MasterOrderItem XMOI 
INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
where podx.ProductionOrderId=po.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

,StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
trn.MasterOrderItem XMOI 
INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
where podx.ProductionOrderId=po.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                
                                 ,SONo=STUFF((select distinct ','+sox.Id from 
trn.MasterOrderItem XMOI 
INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
where podx.ProductionOrderId=po.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                                ,LineItemReference=STUFF((select distinct ','+sox.LineItemReference from 
trn.MasterOrderItem XMOI 
INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
where podx.ProductionOrderId=po.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                                                ,SOQty=(select sum(sox.Qty) from 
trn.MasterOrderItem XMOI 
INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
where podx.ProductionOrderId=po.Id)
                               
FROM trn.ProductionOrder PO
INNER JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=po.Id AND pod.Id=(SELECT TOP 1 Id FROM trn.ProductionOrderDetail AS px WHERE px.ProductionOrderId=po.Id)
                                INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId
                                inner join trn.MasterOrderItem MOI on MOI.Id=so.MasterOrderItemId
INNER JOIN trn.MasterOrder AS mo ON mo.Id=MOI.MasterOrderId
LEFT OUTER JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                ) AS PR ON pr.ProductionOrderId=po.Id
                                
                                LEFT OUTER JOIN [HKP].[TaskSubCategory] AS TSC ON TSC.Id = TMM.TaskSubCategoryId
LEFT OUTER JOIN HKP.TaskCategory AS TC ON TC.Id = TMM.TaskCategoryId
LEFT OUTER JOIN hkp.BuyerDepartment AS bd ON bd.Id=PR.BuyerDepartmentId   
LEFT OUTER JOIN hkp.BuyerDivision AS bd2 ON bd2.Id=PR.BuyerDivisionId  ) AS MO on MO.TaskMasterId=tm.Id
                                INNER JOIN TNATasks AS TT ON TT.Id=tm.TNATasksId
                                LEFT OUTER JOIN TaskAudit AS AB ON ab.TaskManagerMasterId=tm.Id AND ab.AuthorizationType='CreatedBy'
                                LEFT OUTER JOIN TaskAudit AS ATO ON ATO.TaskManagerMasterId=tm.Id AND ATO.AuthorizationType='AssignTo'

                                LEFT OUTER JOIN EmployeeInformation AS EAB ON eab.SystemId=ab.ResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS EATO ON EATO.SystemId=ATO.ResponsiblePersonId
                                LEFT OUTER JOIN EmployeeInformation AS EACL ON EACL.SystemId=TM.ClosedBy
                                LEFT OUTER JOIN SEC.[USER] AS USRCL ON USRCL.UserId=TM.ClosedBy

                               LEFT JOIN MST.ManpowerBudget AS mb ON mb.Id=EATO.BudgetCode
       LEFT JOIN ORG.Position pr ON pr.Id=MB.PositionId
LEFT OUTER JOIN org.Department AS DTO ON dto.Id=pr.DepartmentId
                                left outer join TaskComments TSK on TSK.TaskManagerMasterId=TM.Id AND TSK.ID=(SELECT TOP 1 ID FROM TaskComments T WHERE T.TaskManagerMasterId=TM.ID ORDER BY T.CreatedTime DESC)

                              
                                LEFT OUTER JOIN MasterOrderTaskTemplate AS mott ON mott.Id=MO.TaskTemplateId
                                LEFT OUTER JOIN TaskMaster AS TAM ON TAM.Id=mott.TaskMasterId
                                INNER JOIN hkp.TaskCategory AS tc ON TAM.TaskCategoryId=tc.Id AND TC.Active=1
                                INNER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id=TAM.TaskSubCategoryId AND TSC.Active=1

                                LEFT OUTER JOIN hkp.Process AS p ON p.Id=TAM.ProcessId
                                INNER JOIN hkp.TaskAppliedOn AS tao ON tao.Id=tam.TaskAppliedOnId
                                left join  HKP.TaskDependentDates AS TTD on TTD.id=mott.TaskDependentDatesId
                                    
                                ) AS K  WHERE 1=1   ORDER BY Buyer,StyleNo,SONo,PRNo
"));
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        #endregion TNA API

        #endregion Pratibha

        #region Stich


        public IHttpActionResult GetInspectionType(string Userid)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select IT.Id Value , IT.UserName Name from [dbo].[InspectionType] IT 
left join [dbo].[InspectionEmployeeApplicable]  ITE on ITE.InspectionTypeID = IT.Id
left join sec.[user] SU on SU.EmployeeId = ITE.EmployeeId
where SU.UserId = '" + Userid + "'"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetWorkcenter(string EntityId , string ProcessId)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select Id Value , UserName Name from scs.WorkCenterMaster where ProcessId = '" + ProcessId + "' and EntityId = '" + EntityId + "' order by Code asc"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetPO(string EntityId)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select Id Value , Id Name from trn.ProductionOrder where ProductionStatusId = '20252' and EntityId = '" + EntityId + "'"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetSO(string POID)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select So.Id Value , SO.LineItemReference Name from [TRN].[ProductionOrderDetail] PO 
left join Trn.SalesOrder so on So.id = po.SalesOrderId where PO.Id = '" + POID + "'"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetPOSO(string Plantid)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select Distinct PO.Id PO , SO.Id SO, SO.LineItemReference LineItem from trn.ProductionOrder po
left join [TRN].[ProductionOrderDetail] pod on pod.ProductionOrderId = po.id
left join trn.SalesOrder so on so.id = pod.salesorderid 
where PO.ProductionStatusId = '20252'  and PO.PlantId = '" + Plantid + "'"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetStitchShift(string Plantid)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select SystemID ShiftId ,CONCAT(UserName ,' ' , FORMAT(Intime,'HH:mm') , ' ' , FORMAT(OutTime,'HH:mm') ) ShiftName , FORMAT(Intime,'HH:mm') InTime 
 ,FORMAT(OutTime,'HH:mm') OutTime from ShiftDefination where IsActive = 1 and  Plantid = '" + Plantid + "'"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetSizeColor(string SO)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select Distinct FC.Id SKU1Id , SC.Id SKU2Id , FC.SalesOrderId SO , Chv.UserName Color , Chvs.UserName Size , FC.Qty TotalQty ,CEILING(( SC.Qty + ISNULL(SC.Qty*(Isnull(mo.ExtraOrderPercentage,0)/100),0) )) CSWiseQty , SC.ValueFreeText Dia   
,FeedingQty = isnull( (Select Sum(ITGC.Qty) FeedingQty from [TRN].[InspectionTranChild]  ITC
				left join [dbo].[InspectionTranGrandChild] ITGC on ITGC.InspectionTranChildId = ITC.Id
				where ITC.SalesOrderId = FC.SalesOrderId and ITC.SKU1Id = FC.Id and ITC.SKU2Id = SC.Id and ITC.InspectionTypeEnteryLevelId = '8'
				group by ITC.SalesOrderId,ITC.SKU1Id,ITC.SKU2Id),0)
--,ITE.UserName Button ,Concat(Convert(numeric(18) , Isnull(ITG.Qty,0)) , '/',Convert(numeric(18), SC.Qty) ) QTY 
from TRN.FirstCharacteristics FC 
left join TRN.SecondCharacteristics SC on SC.FirstCharacteristicsId = FC.Id
left join [HKP].[Characteristics] Ch on Ch.Id = FC.CharacteristicsId 
left join [HKP].[CharacteristicsValue]  Chv on Chv.Id = FC.CharacteristicsValueId
left join [HKP].[Characteristics] Chs on Chs.Id = SC.CharacteristicsId 
left join [HKP].[CharacteristicsValue]  Chvs on Chvs.Id = Sc.CharacteristicsValueId
left join [TRN].[InspectionTranChild]  TRC on TRC.SalesOrderId = FC.SalesOrderId
left join InspectionTypeEnteryLevel ITE on ITE.Id = TRC.InspectionTypeEnteryLevelId 
left join [dbo].[InspectionTranGrandChild] ITG on ITG.InspectionTranChildId	 = TRC.Id
left join trn.SalesOrder so on so.id = fc.SalesOrderId 
left join trn.MasterOrderItem moi on moi.id = so.MasterOrderItemId
left join trn.MasterOrder mo on mo.id = moi.MasterOrderId
where FC.SalesOrderId = '" + SO + "'"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetEmpbyuserid(string Userid)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"select EMP.SystemId   ,EMA.PIN MYAppPin , EMP.Employeecode ,EMP.EmployeeName, Employeestatus,EmployeeCurrentStatus,
UN.Id EntityId,Un.Username as Entity, DP.StandardName as Department, SC.StandardName as Section, SBC.Id SubSectionId,SBC.StandardName as SubSection, 
x.UserName as Category,LDSG.Id LegalDegId, LDSG.StandardName as LegalDesignation, GDSG.StandardName as GivenDesignation, 
 MB.Code BudgetCode,POS.Id PositionId,POS.Code PositionCode  , PT.Username PLant , PT.Id PlantId
,US.UserId AplosId  , SD.SystemId ShiftId , Dv.Username Division , MB.Active MBActive , emp.EmploymentType
,MB.Id BudgetId , AG.StandardName AccountGroup , ln.Username Linename , ln.id Lineid, POS.Skilltype
from EmployeeInformation emp
LEFT JOIN MST.ManpowerBudget MB ON MB.Id = emp.BudgetCode 
left join org.Position pos on pos.Id =  mb.PositionId
left join org.division Dv on DV.Id = POS.Divisionid
left join ORG.Entity UN on UN.Id =  MB.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
left join mst.DesignationMasterLegalDesignation dmld on dmld.LegalDesignationId = GDSG.Id
left join mst.DesignationMaster dm on dm.Id = dmld.DesignationMasterId
left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
left join sec.[User] US on US.EmployeeId = emp.SystemId
left join hkp.EmployeeMobileAppsAuthorization EMA on EMA.EmployeeId = emp.SystemId
left join ShiftDefination SD on SD.SystemId = MB.ShiftDefinationid
left join org.plant PT on PT.Id = emp.PlantId
left join [dbo].[AccountsGroup] AG on AG.Id = MB.AccountsGroupId
left join org.line ln on ln.id = mb.Lineid 
where US.UserId = '" + Userid + "'"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetInspectionTypeDetail()
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select  IT.Id InspectionTypeId ,IT.UserName InspectionType , ITE.Id InspectionTypeEnteryLevelId, ITE.InspectionTypeId ITEITID , ITE.Grade Grade , ITE.UserName ITEUsername
,ITE.LineItem , ITE.ProductCode , ITE.ProductionOrder , ITE.SalesOrder , ITE.SKU1 , ITE.SKU2 , ITE.SKU3 , ITE.MaxQty , ITE.Picture , ITE.Operation , ITE.Defect 
,ITP.ProcessId , IE.EntityId , IEA.EmployeeId , IUA.BudgetId,pc.UserName ProcessName
from InspectionType IT
left join InspectionTypeEnteryLevel ITE on ITE.InspectionTypeId = IT.Id
left join InspectionTypeProcess ITP  on ITP.InspectionTypeId = IT.Id
left join InspectionEntity IE on IE.InspectionTypeId = IT.Id
left join InspectionEmployeeApplicable IEA on IEA.InspectionTypeId = IT.Id
left join InspectionUserApplicable IUA on IUA.InspectionTypeId = IT.Id
left join hkp.process pc on pc.id = ITP.ProcessId"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetALLQTYBYSO(string SO , string SKU1 ,string SKU2 , string InspectionTypeId)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Declare  @SO varchar(100) = '" + SO + "' , @SKU1 varchar(100) = '" + SKU1 + "',@SKU2  varchar(100) = '" + SKU2 +" ' , @InspectionTypeId varchar(100) = '" + InspectionTypeId + @"';

SELECT
    TRN.SalesOrderId,
    TRN.SKU1Id,
    TRN.SKU2Id,
    ITY.Id AS InspectionTypeId,

    CONVERT(NUMERIC(18), SUM(ISNULL(ITG.Qty+ITG.PassQty + ITG.RecheckQty + ITG.RejectQty,0))) AS TotalQty,

    CONVERT(NUMERIC(18),
        SUM(CASE
                WHEN ITEL.UserName='PASS' THEN ISNULL(ITG.Qty,0)
                WHEN ITEL.UserName='ALTER' THEN ISNULL(ITG.PassQty,0)
                ELSE 0
            END)) AS PassedQty,

    CONVERT(NUMERIC(18),
        SUM(CASE
                WHEN ITEL.UserName='ALTER' THEN ISNULL(ITG.Qty,0)
                ELSE 0
            END)) AS AlteredQty,

    (
        SELECT SUM(
                CASE
                    WHEN ITEL2.UserName='RECHECK' THEN ISNULL(ITG2.Qty,0)
                    WHEN ITEL2.UserName='ALTER' THEN ISNULL(ITG2.RecheckQty,0)
                    ELSE 0
                END)
        FROM TRN.InspectionTranChild TRN2
        JOIN dbo.InspectionTypeEnteryLevel ITEL2
            ON ITEL2.Id = TRN2.InspectionTypeEnteryLevelId
        JOIN dbo.InspectionTranGrandChild ITG2
            ON ITG2.InspectionTranChildId = TRN2.Id
        WHERE CONVERT(date, ITG2.AddedDate) = CONVERT(date, GETDATE())
          AND TRN2.SalesOrderId = TRN.SalesOrderId
          AND TRN2.SKU1Id = TRN.SKU1Id
          AND TRN2.SKU2Id = TRN.SKU2Id  and ITEL2.InspectionTypeId =  ITY.Id
    ) AS RecheckQty,

    (
        SELECT SUM(
                CASE
                    WHEN ITEL2.UserName='REJECT' THEN ISNULL(ITG2.Qty,0)
                    WHEN ITEL2.UserName='ALTER' THEN ISNULL(ITG2.RejectQty,0)
                    ELSE 0
                END)
        FROM TRN.InspectionTranChild TRN2
        JOIN dbo.InspectionTypeEnteryLevel ITEL2
            ON ITEL2.Id = TRN2.InspectionTypeEnteryLevelId
        JOIN dbo.InspectionTranGrandChild ITG2
            ON ITG2.InspectionTranChildId = TRN2.Id
        WHERE CONVERT(date, ITG2.AddedDate) = CONVERT(date, GETDATE())
          AND TRN2.SalesOrderId = TRN.SalesOrderId
          AND TRN2.SKU1Id = TRN.SKU1Id
          AND TRN2.SKU2Id = TRN.SKU2Id  and ITEL2.InspectionTypeId =  ITY.Id
    ) AS RejectedQty

FROM dbo.InspectionTypeEnteryLevel ITEL
JOIN TRN.InspectionTranChild TRN
    ON TRN.InspectionTypeEnteryLevelId = ITEL.Id
LEFT JOIN dbo.InspectionTranGrandChild ITG
    ON ITG.InspectionTranChildId = TRN.Id
LEFT JOIN TRN.Inspection IT
    ON IT.Id = TRN.InspectionId
LEFT JOIN dbo.InspectionType ITY
    ON ITY.Id = IT.InspectionTypeId

WHERE CONVERT(date, ITG.AddedDate) = CONVERT(date, GETDATE())
  AND TRN.SalesOrderId = @SO
  AND TRN.SKU1Id = @SKU1
  AND TRN.SKU2Id = @SKU2
  AND ITY.Id = @InspectionTypeId

GROUP BY
    TRN.SalesOrderId,
    TRN.SKU1Id,
    TRN.SKU2Id,
    ITY.Id;"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetALLQTYBYSOUSER(string SO, string SKU1, string SKU2, string InspectionTypeId , string userid)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"SELECT TRN.SalesOrderId, TRN.SKU1Id, TRN.SKU2Id,ITY.Id InspectionTypeId,
       Convert(numeric(18),SUM(ISNULL(ITG.Qty,0))) AS TotalQty,
   Convert(Numeric(18),SUM(CASE WHEN ITEL.UserName='PASS' THEN ISNULL(ITG.Qty,0)  when ITEL.UserName='ALTER' THEN ISNULL(ITG.PassQty,0) else 0 END )) AS PassedQty,
   convert(numeric(18),sum(Case when ITEL.UserName='ALTER' THEN ISNULL(ITG.RecheckQty,0) else 0 END )) RecheckQty,
   Convert(Numeric(18),SUM(CASE WHEN ITEL.UserName <> 'PASS' THEN ISNULL((ITG.Qty + ITG.RejectQty + ITG.RecheckQty),0) else 0 END)) AS InspectedQty
FROM dbo.InspectionTypeEnteryLevel ITEL
JOIN TRN.InspectionTranChild TRN ON TRN.InspectionTypeEnteryLevelId = ITEL.Id
LEFT JOIN dbo.InspectionTranGrandChild ITG ON ITG.InspectionTranChildId = TRN.Id
left join [TRN].[Inspection] IT on IT.Id = trn.InspectionId
left join [dbo].[InspectionType] ITY on ITY.Id = IT.InspectionTypeId
left join TRN.FirstCharacteristics FC on FC.Id = TRN.SKU1Id and FC.SalesOrderId = TRN.SalesOrderId
left join TRN.SecondCharacteristics SC on SC.Id = TRN.SKU2Id AND sc.SalesOrderId = TRN.SalesOrderId
where Convert(Date,ITG.AddedDate) = CONVERT(Date,Getdate())  and TRN.SalesOrderId = '" + SO + "' and TRN.SKU1Id = '" + SKU1 + "' and TRN.SKU2Id = '" + SKU2 + "' and ITY.Id = '" + InspectionTypeId + "' and ITG.AddedBy = '" + userid + "' GROUP BY TRN.SalesOrderId, TRN.SKU1Id, TRN.SKU2Id , ITY.Id;"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetPOWiseImage(string productionorderid, string ProcessId)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select PA.Id Id , PA.Code , PA.AreaName , PA.ImageName , PA.Id ImageID , PA.Zone , PA.XAxis ,PA.YAxis
,ImageUrl  = CONCAT('/POPResources/DefectPic/' , PA.ImageName )
from [dbo].[ProductArea] PA
left join [MST].[ImageMaster] IM on IM.Id = PA.ImageMasterId
left join [MST].[ImageProduct] IMP on IMP.ImageMasterId = IM.Id
left join TRN.ProductionBulletinTemplate PB on PB.ProductMasterId = IMP.ProductMasterId
left join hkp.ImageMasterProcess impe on impe.Imagemasterid = IM.Id
where PB.ProductionOrderId = '" + productionorderid + "'  and  impe.ProcessId = '" + ProcessId + "'"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetOperation(string POID, string AreaCode)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select PBT.OperationVariationId OperationId ,OV.UserName Operation , PBT.AreaCode  from [TRN].[ProductionBulletinTemplateDetail] PBT
left join [TRN].[ProductionBulletinTemplateMaster] PBM on PBM.Id = PBT.ProductionBulletinTemplateMasterId
Left join [TRN].[ProductionBulletinTemplate] PB on PB.Id = PBM.ProductionBulletinTemplateId
left join [MST].[OperationVariation] OV on OV.Id = PBT.OperationVariationId
where PB.ProductionOrderid = '" + POID + "' and PBT.AreaCode = '" + AreaCode + "'"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetDefactMaster(string ProcessId)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select Distinct DM.Id, SrNo,DefectCategory,DefectCode,Remarks,DefectNames,DefectsLocalName , DM.ProcessId , QualityProcessId,TypesofDefects , Zone 
from [HKP].[DefectMaster] DM 
left join defectmasterprocess dmp on dmp.DefectMasterId = dm.Id
where dmp.ProcessId = '" + ProcessId + "'"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        [HttpPost]
        public string SaveInspection([FromBody] IEnumerable<InspectionModel> DataToSave)
        {
            try
            {
                string Id = clsData.CreateInspection(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]
        public string SaveInspectionTran([FromBody] IEnumerable<InspectionTranModel> DataToSave)
        {
            try
            {
                string Id = clsData.CreateInspectionTran(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]
        public string SaveInspectionTranGrand([FromBody] IEnumerable<InspectionTranGrandModel> DataToSave)
        {
            try
            {
                string Id = clsData.CreateInspectionTranGrand(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]
        public string PostUpdateInspectionTranGrand([FromBody] IEnumerable<InspectionTranGrandModel> DataToSave)
        {
            try
            {
                string Id = clsData.PostUpdateInspectionTranGrand(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public IHttpActionResult GetQRCODEDetails(string QRCodeDet)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select Distinct ITGC.Id , ITGC.QRCODE , ITGC.ISResolve , ITGC.Qty , so.LineItemReference , ITY.UserName InspectionType    from [dbo].[InspectionTranGrandChild] ITGC
left join [TRN].[InspectionTranChild] ITC on ITC.Id = ITGC.InspectionTranChildId
left join [TRN].[Inspection] IT on IT.Id = ITC.InspectionId
left join trn.SalesOrder so on so.Id = ITC.SalesOrderId
left join [dbo].[InspectionType] ITY on ITY.Id = IT.InspectionTypeId
where ITGC.QRCODE = '" + QRCodeDet + "'"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetRecheckDetail(string SO, string SKU1, string SKU2)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"
 SELECT
    A.*,
    D.DefectNames,
    O.OperationNames
FROM
(
    SELECT DISTINCT
        ITGC.Id,
        ITGC.QRCODE,
        ITGC.ISResolve,
        ITGC.RecheckQty,
        SO.LineItemReference,
        ITY.UserName AS InspectionType,
        ITGC.DefectId, ITGC.AreaCode,
        ITGC.OperationId
    FROM dbo.InspectionTranGrandChild ITGC
    LEFT JOIN TRN.InspectionTranChild ITC
        ON ITC.Id = ITGC.InspectionTranChildId
    LEFT JOIN TRN.Inspection IT
        ON IT.Id = ITC.InspectionId
    LEFT JOIN TRN.SalesOrder SO
        ON SO.Id = ITC.SalesOrderId
    LEFT JOIN dbo.InspectionType ITY
        ON ITY.Id = IT.InspectionTypeId
		where ITC.Salesorderid = '" + SO + "' and ITC.SKU1Id = '" + SKU1 + "' and ITC.SKU2Id = '" + SKU2 + @"' and ITGC.RecheckQty > 0 ) A
OUTER APPLY
(
    SELECT STRING_AGG(DM.DefectNames, ', ') AS DefectNames
    FROM STRING_SPLIT(A.DefectId, ',') S
    INNER JOIN HKP.DefectMaster DM
        ON DM.Id = LTRIM(RTRIM(S.value))
) D
OUTER APPLY
(
    SELECT STRING_AGG(OV.UserName, ', ') AS OperationNames
    FROM STRING_SPLIT(A.OperationId, ',') O
    INNER JOIN MST.OperationVariation OV
        ON OV.Id = LTRIM(RTRIM(O.value))
) O; "));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }


        public IHttpActionResult GetValidateData(string SO, string SKU1, string SKU2 ,string  WorkCenterMasterId , string InspectionType)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"DECLARE @SalesOrderId VARCHAR(50)='" + SO + @"';
DECLARE @SKU1Id VARCHAR(50)='" + SKU1 + @"';
DECLARE @SKU2Id VARCHAR(50)='" + SKU2 + @"';
DECLARE @WorkCenterMasterId VARCHAR(50)='" + WorkCenterMasterId  + @"';
DECLARE @InspectionType VARCHAR(50)='" + InspectionType + @"';   -- EOL / Finished / Final Output

DECLARE @Today DATE = CAST(GETDATE() AS DATE);

WITH Qty AS
(
    SELECT
        ITY.Id AS InspectionType,
        CAST(ITGC.AddedDate AS DATE) AS TranDate,
        SUM(ISNULL((Case when IEL.UserName = 'PASS' then  ITGC.Qty when IEL.UserName = 'ALTER' then ITGC.PassQty else 0 end),0)) AS Qty
    FROM TRN.InspectionTranChild ITC
    INNER JOIN dbo.InspectionTranGrandChild ITGC
        ON ITGC.InspectionTranChildId = ITC.Id
    INNER JOIN TRN.Inspection IT
        ON IT.Id = ITC.InspectionId
    INNER JOIN dbo.InspectionType ITY
        ON ITY.Id = IT.InspectionTypeId
		inner join InspectionTypeEnteryLevel IEL on IEL.Id = ITC.InspectionTypeEnteryLevelId
    WHERE ITC.SalesOrderId = @SalesOrderId
      AND ITC.SKU1Id = @SKU1Id
      AND ITC.SKU2Id = @SKU2Id and IT.WorkCenterMasterId = @WorkCenterMasterId
    GROUP BY
        ITY.Id,
        CAST(ITGC.AddedDate AS DATE)
),
Summary AS
(
    SELECT
        SUM(CASE WHEN InspectionType='24' AND TranDate<@Today THEN Qty ELSE 0 END) AS PrevLF,
        SUM(CASE WHEN InspectionType='1'          AND TranDate<@Today THEN Qty ELSE 0 END) AS PrevEOL,
        SUM(CASE WHEN InspectionType='21'     AND TranDate<@Today THEN Qty ELSE 0 END) AS PrevFinished,
       -- SUM(CASE WHEN InspectionType='22' AND TranDate<@Today THEN Qty ELSE 0 END) AS PrevFinal,

        SUM(CASE WHEN InspectionType='24' AND TranDate=@Today THEN Qty ELSE 0 END) AS TodayLF,
        SUM(CASE WHEN InspectionType='1'          AND TranDate=@Today THEN Qty ELSE 0 END) AS TodayEOL,
        SUM(CASE WHEN InspectionType='21'     AND TranDate=@Today THEN Qty ELSE 0 END) AS TodayFinished
       -- ,SUM(CASE WHEN InspectionType='22' AND TranDate=@Today THEN Qty ELSE 0 END) AS TodayFinal
    FROM Qty
)


SELECT
    AllowedQty =
    CASE
        WHEN @InspectionType='1'
            THEN ISNULL(TodayLF,0) + (ISNULL(PrevLF,0) - ISNULL(PrevEOL,0))

        WHEN @InspectionType='21'
            THEN ISNULL(TodayEOL,0) + (ISNULL(PrevEOL,0) - ISNULL(PrevFinished,0))

       -- WHEN @InspectionType='22'
         --   THEN ISNULL(TodayFinished,0) + (ISNULL(PrevFinished,0) - ISNULL(PrevFinal,0))
    END,

    AlreadyEnteredToday =
    CASE
        WHEN @InspectionType='1'
            THEN ISNULL(TodayEOL,0)

        WHEN @InspectionType='21'
            THEN ISNULL(TodayFinished,0)

       -- WHEN @InspectionType='22'
       --     THEN ISNULL(TodayFinal,0)
    END,

    PreviousWIP =
    CASE
        WHEN @InspectionType='1'
            THEN ISNULL(PrevLF,0) - ISNULL(PrevEOL,0)

        WHEN @InspectionType='21'
            THEN ISNULL(PrevEOL,0) - ISNULL(PrevFinished,0)

       -- WHEN @InspectionType='22'
        --    THEN ISNULL(PrevFinished,0) - ISNULL(PrevFinal,0)
    END 
	from Summary"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        // photo upload

        [HttpPost]

        public async  Task<IHttpActionResult> PostUploadPhoto()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                    return BadRequest();

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // Get Id
                int id = Convert.ToInt32(await provider.Contents
                    .First(x => x.Headers.ContentDisposition.Name.Trim('"') == "Id")
                    .ReadAsStringAsync());

                // Get Photo
                var file = provider.Contents
                    .First(x => x.Headers.ContentDisposition.FileName != null);

                byte[] bytes = await file.ReadAsByteArrayAsync();

                // Save as 123.jpg
                string fileName = id + ".jpg";

                string folderPath = @"F:\aPOP\Pratibha\IIS\POPResources\RejectPic\";

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fullPath = Path.Combine(folderPath, fileName);

                File.WriteAllBytes(fullPath, bytes);

                return Ok(new
                {
                    Success = true,
                    FileName = fileName
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        // AQL  Audit

        public IHttpActionResult GetAQLLevel()
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select 'First 50PCS Review' Value , 'First 50 PCS Review' Name
Union All
Select 'Pre-Final' Value , 'Pre-Final' Name
Union All
Select 'InLine' Value , 'InLine' Name
Union all
Select 'AQL-LotAudit' Value , 'AQL-LotAudit' Name"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

       

        public IHttpActionResult GetAQLMaster()
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select Id , FromLotSize , ToLotSize,SampleSize , AQLLevel,Accept,Reject from [HKP].[AQLMaster]"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetAQLLevelValue()
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select Distinct AQLLevel  Value , AQLLevel Name from [HKP].[AQLMaster]"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetAQLAuditData(string SO, string SKU1, string SKU2, string ATCID)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select Top 1 ATC.Id , ATC.SalesOrderId , ATC.SKU1Id , ATC.SKU2Id, ATC.AuditSampleQty , ATS.Id AQLId , ATS.AQLLevel 
,ATS.LotSize , ATS.SampleSize , ATS.AQLLevelValue , ATS.AcceptPoint
,AuditDoneQty = Isnull((Select Sum(QTY + RejectQty) from [dbo].[AQLTranGrandChild]  where AQLTranChildId = ATC.Id group by AQLTranChildId),0)
, TotalAQLAudtitDone = Isnull((Select SUM(ATGs.Qty + ATGs.RejectQty) TotalAQLAudtitDone from dbo.[AQLTranGrandChild] ATGs
								left join [TRN].[AQLTranChild] ATCs on ATGs.AQLTranChildId = ATCs.Id
								left join [TRN].[AQLTransection] ATSs on ATSs.Id = ATCs.AQLTransectionId
								where ATSs.Id = ATS.Id
								group by ATSs.Id),0)
, TotalAuditSampleQty = Isnull((Select SUM( ATCn.AuditSampleQty) TotalAuditSampleQty from  [TRN].[AQLTranChild] ATCn 
								left join [TRN].[AQLTransection] ATSn on ATSn.Id = ATCn.AQLTransectionId
								where ATSn.Id = ATS.Id
								group by ATSn.Id),0)
,AuditRejectQty = Isnull((Select SUM(ATGs.RejectQty) TotalAQLAudtitDone from dbo.[AQLTranGrandChild] ATGs
								left join [TRN].[AQLTranChild] ATCs on ATGs.AQLTranChildId = ATCs.Id
								left join [TRN].[AQLTransection] ATSs on ATSs.Id = ATCs.AQLTransectionId
								where ATSs.Id = ATS.Id
								group by ATSs.Id),0)
--,TotalAuitQty = ISNULL( Select  ,0)
from [TRN].[AQLTranChild] ATC
left join [TRN].[AQLTransection] ATS on ATS.Id = ATC.AQLTransectionId
where ATC.SalesOrderId  = '" + SO + "' and ATC.SKU1Id = '" + SKU1 + "' and  ATC.SKU2Id = '" + SKU2 + "'  and ATS.Id = '" + ATCID + @"'
order by ATC.AddedDate desc"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetAQLReportDataFirst(string AQLId)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select Distinct
ATS.Id ReportNumber, Format(ATS.DateTime,'yyyy-mm-dd') ReportDate , ATS.LotSize , ATS.SampleSize , ATS.AcceptPoint, RejectPoint = (ATS.AcceptPoint + 1)
,ATS.AQLLevelValue , ATS.AQLLevel , SD.ShiftType , EI.EmployeeName  , wcm.UserName [Line] 
,SO.Id Salesorder , SO.LineItemReference LineItem , PO.Id PO 
 ,Chv.UserName Color, case when ATS.LotNumber is not null then  Concat(SizeData.Size , '  , LOTNO: ',ATS.LotNumber) else SizeData.Size  end  AS Size
 ,PB.BulletinName [Description] , pt.UserName Customer  
 ,SUM(ISNULL(DefectCount.OperationDefect,0)) AS OperationDefect
,SUM(ISNULL(DefectCount.OtherDefect,0)) AS OtherDefect
,SUM(ISNULL(DefectCategoryCount.MinorDefect,0)) AS MinorDefect
,SUM(ISNULL(DefectCategoryCount.MajorDefect,0)) AS MajorDefect
, TotalMajorDefect = ((case when SUM(ISNULL(DefectCategoryCount.MinorDefect,0)) = 0 then 0 else FLOOR(SUM(ISNULL(DefectCategoryCount.MinorDefect,0))/3) end ) + SUM(ISNULL(DefectCategoryCount.MajorDefect,0)))
,sum(ATGC.Rejectqty) Rejectqty
,FC.Qty OrderQty
,OperationMajorDefect = ((case when SUM(ISNULL(OPOTDefectCount.OperationMinorDefect,0)) = 0 then 0 else FLOOR(SUM(ISNULL(OPOTDefectCount.OperationMinorDefect,0))/3) end ) + SUM(ISNULL(OPOTDefectCount.OperationMajorDefect,0)))
,OtherMajorDefect = ((case when SUM(ISNULL(OPOTDefectCount.OtherMinorDefect,0)) = 0 then 0 else FLOOR(SUM(ISNULL(OPOTDefectCount.OtherMinorDefect,0))/3) end ) + SUM(ISNULL(OPOTDefectCount.OtherMajorDefect,0)))
from [TRN].[AQLTransection]  ATS
left join ShiftDefination SD on SD.SystemID = ATS.ShiftId
left join sec.[User] US on US.UserId = ATS.AddedBy 
left join EmployeeInformation EI on EI.SystemId = US.EmployeeId 
left join scs.WorkCenterMaster wcm on wcm.id = ATS.WorkCenterMasterId
left join [TRN].[AQLTranChild] ATC on ATC.AQLTransectionId = ATS.Id
left join trn.salesorder so on so.id = ATC.Salesorderid 
left join [TRN].[ProductionOrderDetail] pod on  so.id = pod.salesorderid 
left join trn.ProductionOrder po on po.id = pod.ProductionOrderId
left join TRN.FirstCharacteristics FC  on FC.Id = ATC.SKU1Id 
left join TRN.SecondCharacteristics SC on SC.Id = ATC.SKU2Id
left join [HKP].[Characteristics] Ch on Ch.Id = FC.CharacteristicsId 
left join [HKP].[CharacteristicsValue]  Chv on Chv.Id = FC.CharacteristicsValueId
left join [HKP].[Characteristics] Chs on Chs.Id = SC.CharacteristicsId 
left join [HKP].[CharacteristicsValue]  Chvs on Chvs.Id = Sc.CharacteristicsValueId
left join TRN.ProductionBulletinTemplate PB on PB.Productionorderid = PO.Id
left join trn.masterorderitem moi on moi.Id = so.MasterOrderItemId
left join trn.MasterOrder mo on mo.id = moi.MasterOrderId 
left join hkp.Party pt on pt.id = mo.PartyId 
left join AQLTranGrandChild ATGC on ATGC.AQLTranChildId = ATC.Id
OUTER APPLY
(
    SELECT
        SUM(CASE WHEN DM.TypesOfDefects = 'Operation' THEN 1 ELSE 0 END) AS OperationDefect,
        SUM(CASE WHEN DM.TypesOfDefects = 'Other' THEN 1 ELSE 0 END) AS OtherDefect
    FROM STRING_SPLIT(ATGC.DefectId, ',') S
    INNER JOIN HKP.DefectMaster DM
        ON DM.Id = TRY_CAST(S.value as varchar(max))
) DefectCount
OUTER APPLY
(
    SELECT
        SUM(CASE WHEN DM.TypesOfDefects = 'Operation' and (DM.DefectCategory = 'Major' or DM.DefectCategory = 'Critical')  THEN 1 ELSE 0 END) AS OperationMajorDefect,
        SUM(CASE WHEN DM.TypesOfDefects = 'Operation' and (DM.DefectCategory = 'Minor')  THEN 1 ELSE 0 END) AS OperationMinorDefect,
        SUM(CASE WHEN DM.TypesOfDefects = 'Other'  and (DM.DefectCategory = 'Major' or DM.DefectCategory = 'Critical') THEN 1 ELSE 0 END) AS OtherMajorDefect,
        SUM(CASE WHEN DM.TypesOfDefects = 'Other'  and (DM.DefectCategory = 'Minor') THEN 1 ELSE 0 END) AS OtherMinorDefect
    FROM STRING_SPLIT(ATGC.DefectId, ',') S
    INNER JOIN HKP.DefectMaster DM
        ON DM.Id = TRY_CAST(S.value as varchar(max))
) OPOTDefectCount
OUTER APPLY
(
    SELECT
        SUM(CASE WHEN DM.DefectCategory = 'Major' THEN 1 when DM.DefectCategory = 'Critical' then 1 ELSE 0 END) AS MajorDefect,
        SUM(CASE WHEN DM.DefectCategory = 'Minor' THEN 1 ELSE 0 END) AS MinorDefect
       -- SUM(CASE WHEN DM.DefectCategory = 'Critical' THEN 1 ELSE 0 END) AS CriticalDefect
    FROM STRING_SPLIT(ATGC.DefectId, ',') S
    INNER JOIN HKP.DefectMaster DM
        ON DM.Id = TRY_CAST(S.value as varchar(max))
) DefectCategoryCount
OUTER APPLY
(
    SELECT STRING_AGG(CONCAT(T.Size, ': ', T.AuditQty), ', ') AS Size
    FROM
    (
        SELECT DISTINCT
            Chvs2.UserName AS Size,
            ATC2.AuditQty
        FROM TRN.AQLTranChild ATC2
        LEFT JOIN TRN.SecondCharacteristics SC2
            ON SC2.Id = ATC2.SKU2Id
        LEFT JOIN HKP.CharacteristicsValue Chvs2
            ON Chvs2.Id = SC2.CharacteristicsValueId
        WHERE ATC2.AQLTransectionId = ATS.Id
          AND ATC2.SalesOrderId = SO.Id
          AND ATC2.SKU1Id = ATC.SKU1Id
    ) T
) SizeData
where ATS.Id = '" + AQLId + @"'
group by ATS.Id , Format(ATS.DateTime,'yyyy-mm-dd')  , ATS.LotSize , ATS.SampleSize , ATS.AcceptPoint 
,ATS.AQLLevelValue , ATS.AQLLevel , SD.ShiftType , EI.EmployeeName  , wcm.UserName
,SO.Id  , SO.LineItemReference  , PO.Id ,Chv.UserName,PB.BulletinName, pt.UserName ,
SizeData.Size,FC.Qty,ATS.LotNumber"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetAQLReportDataSecond(string AQLId)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"SELECT
    DM.DefectNames,
    DM.DefectCategory,
    COUNT(*) AS DefectCount
FROM TRN.AQLTransection ATS
LEFT JOIN TRN.AQLTranChild ATC
    ON ATC.AQLTransectionId = ATS.Id
LEFT JOIN AQLTranGrandChild ATGC
    ON ATGC.AQLTranChildId = ATC.Id
CROSS APPLY STRING_SPLIT(ATGC.DefectId, ',') S
INNER JOIN HKP.DefectMaster DM
    ON DM.Id = TRY_CAST(S.value AS varchar(max))
WHERE ATS.Id = '" + AQLId + @"'
GROUP BY
    DM.DefectNames,
    DM.DefectCategory
ORDER BY
    DM.DefectCategory,
    DM.DefectNames;"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetAQLImagedata(string AQLId)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select ATGC.Id Value ,CONCAT('/POPResources/AQLAuditPic/' , ATGC.Id , '.jpg' ) Name from [TRN].[AQLTransection]  ATS
left join [TRN].[AQLTranChild] ATC on ATC.AQLTransectionId = ATS.Id
left join AQLTranGrandChild ATGC on ATGC.AQLTranChildId = ATC.Id
where ATS.Id = '" + AQLId + @"' and DefectId <> ''"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        [HttpPost]
        public string SaveAQL([FromBody] IEnumerable<AQLModel> DataToSave)
        {
            try
            {
                string Id = clsData.CreateAQL(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]
        public string SaveAQLTran([FromBody] IEnumerable<AQLTranModel> DataToSave)
        {
            try
            {
                string Id = clsData.CreateAQLTran(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]
        public string SaveAQLTranGrand([FromBody] IEnumerable<AQLTranGrandModel> DataToSave)
        {
            try
            {
                string Id = clsData.CreateAQLTranGrand(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]

        public async Task<IHttpActionResult> PostAQLUploadPhoto()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                    return BadRequest();

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // Get Id
                int id = Convert.ToInt32(await provider.Contents
                    .First(x => x.Headers.ContentDisposition.Name.Trim('"') == "Id")
                    .ReadAsStringAsync());

                // Get Photo
                var file = provider.Contents
                    .First(x => x.Headers.ContentDisposition.FileName != null);

                byte[] bytes = await file.ReadAsByteArrayAsync();

                // Save as 123.jpg
                string fileName = id + ".jpg";

                string folderPath = @"F:\aPOP\Pratibha\IIS\POPResources\AQLAuditPic\";

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fullPath = Path.Combine(folderPath, fileName);

                File.WriteAllBytes(fullPath, bytes);

                return Ok(new
                {
                    Success = true,
                    FileName = fileName
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        public IHttpActionResult GetAplosPlayStoreAppVersion()
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"select '1.0.4' as Version from org.CompanyGroup"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        #endregion Stich

        #region Attendance tracker

        public IHttpActionResult GetEmployeeInformation(string Empcode, string PlantId)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {

                // return Json(_plantService.GetCboByCompany(companyId));
                return Json(_sqlRepository.GetDataTable(@"select EMP.SystemId as SysId,EMA.PIN MYAppPin , EMP.Employeecode as Code,EMP.EmployeeName,DOB, DOJ, DOS, Employeestatus,EmployeeCurrentStatus, Nationalid, Fathername, Mothername, 
		GenderID, Presentaddress1, ParmanentAddress1, cellphnno, Emailid, PresentArea, 
UN.Id EntityId,Un.Username as Entity, DP.StandardName as Department, SC.StandardName as Section, SBC.Id SubSectionId,SBC.StandardName as SubSection, 
x.UserName as Category,LDSG.Id LegalDegId, LDSG.StandardName as LegalDesignation, GDSG.StandardName as GivenDesignation, 
MB.Code BudgetCode,POS.Code PositionCode  , PT.Username PLant
,US.UserId AplosId  , SD.SystemId ShiftId , Dv.Username Division , MB.Active MBActive , emp.EmploymentType
,MB.Id BudgetId , AG.StandardName AccountGroup
from EmployeeInformation emp
LEFT JOIN MST.ManpowerBudget MB ON MB.Id = emp.BudgetCode 
left join org.Position pos on pos.Id =  mb.PositionId
left join org.division Dv on DV.Id = POS.Divisionid
left join ORG.Entity UN on UN.Id =  MB.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
left join mst.DesignationMasterLegalDesignation dmld on dmld.LegalDesignationId = GDSG.Id
left join mst.DesignationMaster dm on dm.Id = dmld.DesignationMasterId
left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
left join sec.[User] US on US.EmployeeId = emp.SystemId
left join hkp.EmployeeMobileAppsAuthorization EMA on EMA.EmployeeId = emp.SystemId
left join ShiftDefination SD on SD.SystemId = MB.ShiftDefinationid
left join org.plant PT on PT.Id = emp.PlantId
left join [dbo].[AccountsGroup] AG on AG.Id = MB.AccountsGroupId
where   emp.Employeecode = '" + Empcode + "' and PT.Id = '" + PlantId + "'"));
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

     

        [HttpPost]
        public string SaveEmployeeFeedback([FromBody] IEnumerable<EmployeeFeedBackModel> DataToSave)
        {
            try
            {
                string Id = clsData.CreateFeedback(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public IHttpActionResult GetServiceEntity(string Empcode, string plantid)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"Select Id Value , UserName Name  from org.Entity where Active = 1 and Plantid = '" + plantid + "' order by Id desc"));
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        public IHttpActionResult GetServiceLine(string Empcode, string plantid)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {

                return Json(_sqlRepository.GetDataTable(@"Select Id Value , UserName Name  from org.Line where Active = 1  order by Id desc"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }


        public IHttpActionResult GetServiceAttdn(string date, string lineid, string entityid, string category)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {
                return Json(_sqlRepository.GetDataTable(@"select Ei.Systemid , EI.Employeecode , EI.EmployeeName ,EI.CellPhnNo PhoneNo, apd.Daystatus , 
(Select Top 1 rm.UserName from Employeefeedback EF
Left join  [HKP].[AbsentismReasoningMaster] rm on rm.id = EF.ReasoningId 
where EF.EmpSystemId = apd.EmpSystemID and EF.Date = apd.WorkDate order by EF.AddedDate desc)  Reason , 
(Select Top 1 EF.Remarks from Employeefeedback EF where EF.EmpSystemId = apd.EmpSystemID and EF.Date = apd.WorkDate order by EF.AddedDate desc) Remarks 
from AttdnProcessData apd
left join Employeeinformation ei on ei.systemid = apd.Empsystemid
left join mst.manpowerbudget mb on mb.id = ei.budgetcode 
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EI.LegalDesignationId
left join mst.DesignationMasterLegalDesignation dmld on dmld.LegalDesignationId = GDSG.Id
left join mst.DesignationMaster dm on dm.Id = dmld.DesignationMasterId
left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
where apd.Daystatus = 'A' and x.UserName = '" + category + "' and  mb.lineid = '" + lineid + "' and mb.entityid = '" + entityid + "' and apd.workdate = '" + date + "' order by EI.Employeecode asc"));

            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }



        public IHttpActionResult GetAttendance(string Empcode, string month, string PlantId)
        {
            /* clsDataContext clsData = new clsDataContext();
             clsData.GetTNAReport(out List<TNAGetSet> activelists);
             return activelists;*/

            try
            {

                // return Json(_plantService.GetCboByCompany(companyId));
                return Json(_sqlRepository.GetDataTable(@"select Ei.Systemid ,Ei.EmployeeCode,APd.WorkDate,Apd.DayStatus, APd.InTime,APd.OutTime,APd.OTHr from AttdnProcessData as APd
Left join EmployeeInformation as EI on EI.SystemID=APD.EmpSystemId
left join org.plant PT on PT.Id = EI.PlantId
where  EI.EmployeeCode='" + Empcode + "' and format(APd.WorkDate,'MMMM') = '" + month + "' and PT.Id = '" + PlantId + "'  and APd.PlantID = '20252' AND APd.WorkDate < CAST(GETDATE() AS DATE) order by APd.WorkDate desc"));
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }

        }

        [HttpPost]
        public string PostAttendanceCorrect([FromBody] IEnumerable<AttendanceCorrectModel> DataToSave)
        {
            try
            {
                string Id = clsData.PostAttendanceCorrect(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]
        public string PostAttendanceOT([FromBody] IEnumerable<AttendanceOTModel> DataToSave)
        {
            try
            {
                string Id = clsData.PostAttendanceOT(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }



        #endregion Attendance tracker
    }
}
