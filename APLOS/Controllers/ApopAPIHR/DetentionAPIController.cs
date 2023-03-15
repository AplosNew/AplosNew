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
        public List<Default2> GetReason(string ProcessId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetReason(out List<Default2> activelists, ProcessId);
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

        public List<Weight> GetCartonBookedQty(string SalesId)
        {
            clsDataContext clsData = new clsDataContext();
            clsData.GetCartonBookedQty(out List<Weight> activelists, SalesId);
            return activelists;
        }
        #endregion Aman
        #endregion written by Aman

    }
}
