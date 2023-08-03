using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Processes.Controllers
{
    public class ProcessManagementController : Controller
    {
        private readonly ISqlRepository _sqlRepository;
        public ProcessManagementController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        public ActionResult Aplos()
        {
            return View();
        }

        
        public ActionResult LoadEntityDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select E.Id EntityId,E.EntityType,E.UserName Entity,E.Code
                            from ORG.Entity E
                            where E.Active = 1 order by E.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult LoadProcessList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id Value, StandardName Text from HKP.Process where Active = 1 order by Text";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult LoadSubProcessList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id Value, StandardName Text from HKP.SubProcess where Active = 1 order by Text";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult LoadMaterialGrid()
        {
            string sql = @"select distinct MM.Id, MM.Code MaterialCode ,MM.UserName Material, MC.UserName MaterialCategory, MGM.UserName MaterialGroup
, MMA.Code ArticleCode,MMA.StandardName MaterialArticle
from MST.MaterialMaster MM
                            left join MST.MaterialGroupMaster MGM on MGM.Id = MM.MaterialGroupMasterId
                            left join HKP.MaterialCategory MC on MC.ID = MM.MaterialCategoryId
							left join MST.MaterialMasterArticle MMA on MMA.MaterialMasterId = MM.Id
                            where MM.Active = 1 and MGM.Active = 1 and MM.MaterialCategoryId is not null";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadUtilityGrid()
        {
            string sql = @"select UM.Id, UM.UserName UtilityName, UM.StandardName UtilityStdName, UOM.UserName UOM from UtilityMaster UM
                            left join SCS.UnitOfMeasurement UOM on UOM.Id = UM.UoMId
                            where UM.Active = 1";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadResponsiblePopupData()
        {
            try
            {
              
                string CmdText = @"SELECT 
                                  Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode, Emp.EmployeeStatus, Emp.EmployeeCurrentStatus,
                                    Emp.DOS,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                    
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') DOS
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric, PR.PaymentLink Skill, EC.UserName EmployeeCategory
                                         ,EMP.GenderID
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
                                     
										LEFT JOIN MST.DesignationMaster DM on DM.DesignationId = D.Id
										LEFT JOIN HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
										
                                        Where EMP.EmployeeStatus='Active' 
                                       
                                        --ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                

                return Json(_sqlRepository.GetDataCollection(CmdText), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public ActionResult GetUOM()
        {
            string sql = @"Select Id Value, UserName Text from SCS.UnitOfMeasurement";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
    }
}