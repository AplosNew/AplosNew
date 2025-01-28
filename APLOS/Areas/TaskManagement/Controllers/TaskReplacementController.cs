using Aplos.Controllers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class TaskReplacementController : BaseController
    {
        
        #region Constructor

        private readonly ISqlRepository _sqlRepository;


        public TaskReplacementController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        [Authorize, HttpPost]
        public ActionResult SearchEmployeeFrom(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"
                      select top 100 * from (  SELECT distinct Emp.SystemID AS Id,
                        EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
    
                           WHERE emp.CompanyId='" + identity.CompanyId + @"' ) AS TEMP where " + strkey + " Order By Id";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult SearchEmployee(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"
                      select top 100 * from (  SELECT distinct Emp.SystemID AS Id,
                        EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
    
                           WHERE emp.CompanyId='" + identity.CompanyId + @"' AND EMP.EmployeeStatus='Active' OR (emp.EmpType='GUEST' AND emp.EmployeeStatus='Active' AND emp.GroupID='" + identity.CompanyGroupId + @"') ) AS TEMP where " + strkey + " Order By Id";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost,Authorize]
        public ActionResult getTaskList(Dictionary<string, object> data)
        {
            try
            {
                if (clsStaticInfo.nullrecorder(data["FromEmployeeId"].ToString()) == "")
                    throw new Exception("Please enter from employee");

                if (clsStaticInfo.nullrecorder(data["ToEmployeeId"].ToString()) == "")
                    throw new Exception("Please enter to employee");

                if (clsStaticInfo.nullrecorder(data["FromDate"].ToString()) == "")
                    throw new Exception("Please enter from date");

                if (clsStaticInfo.nullrecorder(data["ToDate"].ToString()) == "")
                    throw new Exception("Please enter to date");

                //if (Convert.ToDateTime(clsStaticInfo.nullrecorder(data["FromDate"].ToString())) < Convert.ToDateTime(System.DateTime.Now.ToString("dd-MMM-yyyy")))
                //    throw new Exception("From date cannot be earlier than system date");

                if (Convert.ToDateTime(clsStaticInfo.nullrecorder(data["ToDate"].ToString())) < Convert.ToDateTime(clsStaticInfo.nullrecorder(data["FromDate"].ToString())))
                    throw new Exception("To date cannot be earlier than from date");





                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;



                string sql = @"SELECT convert(bit,1) AS Checked, tmm.Id, tmm.TaskType,tc.UserName AS TaskCategory,tsc.UserName AS TaskSubCategory, tmm.TaskDescription, tmm.CurrentStatus,
                               tmm.TaskPriority,FORMAT(ta.DueDate,'dd-MMM-yyyy') AS DueDate,
                               FORMAT(ISNULL(ta.RevisedCommitmentDate,ta.CommitmentDate),'dd-MMM-yyyy') AS CommitmentDate
   
                          FROM TaskManagerMaster AS tmm
                        INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id

                        LEFT OUTER JOIN hkp.TaskCategory AS tc ON tc.Id=tmm.TaskCategoryId
                        LEFT OUTER JOIN hkp.TaskSubCategory AS tsc ON tsc.Id=tmm.TaskSubCategoryId


                        WHERE  ta.AuthorizationType='AssignTo' AND isnull(tmm.CurrentStatus,'')<>'CLOSED'
                        AND ta.ResponsiblePersonId='" + clsStaticInfo.nullrecorder(data["FromEmployeeId"].ToString())
                            + @"' AND ta.DueDate BETWEEN convert(date,'" + clsStaticInfo.nullrecorder(data["FromDate"].ToString())
                            + @"') AND convert(date,'" + clsStaticInfo.nullrecorder(data["ToDate"].ToString())
                            + @"')
                        ORDER BY ta.DueDate ASC";



                return Json(new { Error = false, DATA = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Update(Dictionary<string, object> data, string TaskList)
        {
            try
            {
                if (clsStaticInfo.nullrecorder(data["FromEmployeeId"].ToString()) == "")
                    throw new Exception("Please enter from employee");

                if (clsStaticInfo.nullrecorder(data["ToEmployeeId"].ToString()) == "")
                    throw new Exception("Please enter to employee");

                if (clsStaticInfo.nullrecorder(data["FromDate"].ToString()) == "")
                    throw new Exception("Please enter from date");

                if (clsStaticInfo.nullrecorder(data["ToDate"].ToString()) == "")
                    throw new Exception("Please enter to date");

                //if (Convert.ToDateTime(clsStaticInfo.nullrecorder(data["FromDate"].ToString())) < Convert.ToDateTime(System.DateTime.Now.ToString("dd-MMM-yyyy")))
                //    throw new Exception("From date cannot be earlier than system date");

                if (Convert.ToDateTime(clsStaticInfo.nullrecorder(data["ToDate"].ToString())) < Convert.ToDateTime(clsStaticInfo.nullrecorder(data["FromDate"].ToString())))
                    throw new Exception("To date cannot be earlier than from date");

                if(TaskList=="''")
                    throw new Exception("Select at least one task from the list");



                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();

                //string sql = @"DELETE FROM TaskAudit WHERE AuthorizationType NOT IN ('CreatedBy','AssignTo') AND TaskManagerMasterId IN (select Tmm.Id FROM TaskManagerMaster AS tmm
                //                INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                //                WHERE tmm.TaskType='TNA' AND ta.AuthorizationType='AssignTo' 
                //                AND ta.ResponsiblePersonId='" + clsStaticInfo.nullrecorder(data["FromEmployeeId"].ToString())
                //                    + @"' AND ta.DueDate BETWEEN '" + clsStaticInfo.nullrecorder(data["FromDate"].ToString())
                //                    + @"' AND '" + clsStaticInfo.nullrecorder(data["ToDate"].ToString())
                //                    + @"')
                //                        AND ResponsiblePersonId='" + clsStaticInfo.nullrecorder(data["ToEmployeeId"].ToString())
                //                    + @"'";
                string sql = @"DELETE FROM TaskAudit WHERE AuthorizationType NOT IN ('CreatedBy','AssignTo') AND TaskManagerMasterId IN (" + TaskList + @")
                                        AND ResponsiblePersonId='" + clsStaticInfo.nullrecorder(data["ToEmployeeId"].ToString())
                                    + @"'";
                objCon.ExecuteNonQueryWrapper(sql, true, "1");

                //sql = @"UPDATE TaskAudit set ResponsiblePersonId='" + clsStaticInfo.nullrecorder(data["ToEmployeeId"].ToString()) + @"' where TaskManagerMasterId IN (select Tmm.Id FROM TaskManagerMaster AS tmm
                //        INNER JOIN TaskAudit AS ta ON ta.TaskManagerMasterId=tmm.Id
                //        WHERE tmm.TaskType='TNA' AND ta.AuthorizationType='AssignTo' 
                //        AND ta.ResponsiblePersonId='" + clsStaticInfo.nullrecorder(data["FromEmployeeId"].ToString())
                //            + @"' AND ta.DueDate BETWEEN '" + clsStaticInfo.nullrecorder(data["FromDate"].ToString())
                //            + @"' AND '" + clsStaticInfo.nullrecorder(data["ToDate"].ToString())
                //            + @"') AND AuthorizationType='AssignTo' ";

                sql = @"UPDATE TaskAudit set ResponsiblePersonId='" + clsStaticInfo.nullrecorder(data["ToEmployeeId"].ToString())
                    + @"',TakenForNotification=0 where TaskManagerMasterId IN (" + TaskList + @") AND AuthorizationType='AssignTo' ";

                objCon.ExecuteNonQueryWrapper(sql, true, "1");

                objCon.CommitTransaction();

                return Json(new { Error = false, Message = "Task transferred successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
       
    }
}