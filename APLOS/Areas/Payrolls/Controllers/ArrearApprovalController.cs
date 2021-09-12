using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Payrolls.Setting;
using static Library.Service.Payrolls.Setting.clsCurrencyRule;
using Library.HumanResource.Payroll.Setting;
using Library.HumanResource.Payroll.Arrear;
using System.Threading.Tasks;
using Library.Service.TaskScheduler;
using Library.Service.Payrolls.SalaryProcessActive;
using Library.Service.Payrolls.SalaryProcess;
using Library.HumanResource.Payroll.SalaryProcess;
using Library.HumanResource.Payroll.Allowance;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class ArrearApprovalController : BaseController
    {


        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public string PlantId { get; private set; }

        public ArrearApprovalController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
            //return await Task.Factory.StartNew(() =>
            //{
            //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //    clsMobileNotification.SendData(identity.CompanyGroupId);

            //});
        }

        [HttpPost, Authorize]
        public ActionResult GetEmpList(string batchId)
        {

            try
            {
                string sql = string.Empty;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ArrearProcess obj = new ArrearProcess();

                JsonResult json = Json(obj.GetEmployeeForApproval(batchId));
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }

        }
        [HttpPost, Authorize]
        public ActionResult ApprovelUnapprove(List<string> data, string ArrearProcessBatchId, bool isApprove)
        {

            try
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (isApprove == true)
                        _sqlRepository.ExecuteSqlCommand("Update ArrearSummaryBatchWise set IsApproved=1 where ArrearProcessBatchId='" + ArrearProcessBatchId + @"' AND EmployeeSystemId='" + data[i] + @"'");
                    else
                        _sqlRepository.ExecuteSqlCommand("Update ArrearSummaryBatchWise set IsApproved=0 where ArrearProcessBatchId='" + ArrearProcessBatchId + @"' AND EmployeeSystemId='" + data[i] + @"'");

                }


                return Json(new { Message = "Data updated successfully", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost, Authorize]
        public ActionResult DeleteEmployeeArrear(string ArrearProcessBatchId, string EmployeeSystemId)
        {

            try
            {

                _sqlRepository.ExecuteSqlCommand(@"DELETE FROM ArrearProcChild WHERE SystemID IN (
                                                SELECT C.SystemID FROM ArrearProcChild AS C
                                                JOIN ArrearProcMaster AS M ON m.SystemID=c.SlrProcMstSystemID
                                                WHERE M.ArrearProcessBatchId='" + ArrearProcessBatchId + @"' AND c.EmpInfoSystemID='" + EmployeeSystemId + @"'
                                                )");
                _sqlRepository.ExecuteSqlCommand("Delete from ArrearSummaryMonthWise where ArrearProcessBatchId='" + ArrearProcessBatchId + @"' AND EmployeeSystemId='" + EmployeeSystemId + @"'");
                _sqlRepository.ExecuteSqlCommand("Delete from ArrearSummaryBatchWise where isnull(IsApproved,0)=0 AND ArrearProcessBatchId='" + ArrearProcessBatchId + @"' AND EmployeeSystemId='" + EmployeeSystemId + @"'");

                _sqlRepository.ExecuteSqlCommand(@"DELETE FROM ArrearProcMaster WHERE SystemID IN (
                                    SELECT APM.SystemID FROM ArrearProcMaster AS apm
                                    LEFT JOIN ArrearProcChild AS apc ON apm.SystemID=apc.SlrProcMstSystemID AND apc.SystemID=(SELECT TOP 1 SystemId FROM ArrearProcChild AS apc2 WHERE apc2.SlrProcMstSystemID=apm.SystemID)
                                    WHERE ISNULL(apc.SystemID,'')=''
                                    )");
                return Json(new { Message = "Data deleted successfully", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }

        }

    }
}