#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Biometrics;
using System.Collections.Generic;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Model.Attendances;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Web.Script.Serialization;
using System;
using clsAttendance;
using Library.Data.Sql;
using System.IO;
using Library.Data;
using Library.Service.Helpers;
using Newtonsoft.Json;
using System.Data.OleDb;
using Syncfusion.XlsIO;
using System.Text.RegularExpressions;
using System.Globalization;
using Library.Model.Enums;
using Library.Service.HumanResources;
using Library.HumanResource.Attendance.Manual;
using System.Linq;
using Library.HumanResource.Attendance;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class ExceptionOTProcessController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IAttendanceManagementService _AttendanceManagementService;

        public ExceptionOTProcessController(
               ISqlRepository sqlRepository,
               IAttendanceManagementService AttendanceManagementService

            )
        {

            _sqlRepository = sqlRepository;
            _AttendanceManagementService = AttendanceManagementService;

        }
        #endregion


        public ActionResult Aplos()
        {
            return View();
        }

        #region --Get--

        [HttpPost, Authorize]
        public ActionResult GetEmployee(string FDate, string TDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(GetEmpData(identity.PlantId, identity.CompanyId, FDate, TDate), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetEmployeeList(string FDate, string TDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(GetEmpDataL(identity.PlantId, identity.CompanyId, FDate, TDate), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        public IEnumerable<object> GetEmpData(string plantId, string companyId, string date,string TDate)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select isToBeSelect = case when Ex.EmpSystemId is null then Convert(bit, 'False') ELSE Convert(bit, 'True') END,ex.Id,format(ex.WorkDate,'dd-MMM-yyyy')WorkDate
                                ,e.SystemId EmpSystemId,e.EmployeeCode,e.EmployeeName,format(e.DOJ,'dd-MMM-yyyy')DOJ,EC.StandardName EmployeeCategory
                                ,DeG.UserName Designation,dp.UserName Department,SE.UserName Section,SuS.UserName SubSection,U.UserName Unit
                                ,isnull(L.UserName,'') Line,d.UserName Division
						from EmployeeInformation e
                        LEFT JOIN ExceptionOTProcess ex ON ex.EmpSystemId=e.SystemId and (ex.WorkDate between '" + date + @"' and '"+ TDate + @"' )
						LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                        LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id
                        LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = E.LegalDesignationId
                        LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                        left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                        left join HKP.Designation DeG on DeG.Id=dm.DesignationId
                        left join HKP.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
                        left join ORG.Section SE on SE.Id=PR.SectionId
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                        LEFT JOIN ORG.Unit AS U ON U.Id= En.UnitId
                        LEFT JOIN ORG.Division AS d ON d.Id= En.DivisionId
						where 
                         e.PlantId='" + plantId + @"' and e.DOJ <= ( '" + date + @"') and (e.DOS is null or e.DOS >= '" + date + @"') order by e.EmployeeCode";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetEmpDataL(string plantId, string companyId, string date, string TDate)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select e.SystemId EmpSystemId,e.EmployeeCode,e.EmployeeName,format(e.DOJ,'dd-MMM-yyyy')DOJ,EC.StandardName EmployeeCategory
                                ,DeG.UserName Designation,dp.UserName Department,SE.UserName Section,SuS.UserName SubSection,U.UserName Unit
                                ,isnull(L.UserName,'') Line,d.UserName Division
						from EmployeeInformation e
						LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                        LEFT JOIN ORG.Entity En ON PMB.EntityId = En.Id
                        LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = E.LegalDesignationId
                        LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                        left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                        left join HKP.Designation DeG on DeG.Id=dm.DesignationId
                        left join HKP.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
                        left join ORG.Section SE on SE.Id=PR.SectionId
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                        LEFT JOIN ORG.Unit AS U ON U.Id= En.UnitId
                        LEFT JOIN ORG.Division AS d ON d.Id= En.DivisionId
						where 
                         e.PlantId='"+plantId+"' and e.DOJ <= ( '"+ TDate + "') and (e.DOS is null or e.DOS >= '" + TDate + "') order by e.EmployeeCode";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        #endregion

        #region --Save--

        [HttpPost]
        public ActionResult Save(List<ExceptionOT> data, string WorkDate,string ToDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ExceptionOTProcess mau = new ExceptionOTProcess();
                mau.Save(data, WorkDate, ToDate);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[ExceptionOTProcess] WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function
        #endregion
    }
}