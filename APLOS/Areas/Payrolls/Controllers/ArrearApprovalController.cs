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
using Syncfusion.XlsIO;

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
                ArrearProcess _process = new ArrearProcess();
                _process.ApprovelUnapprove(data, ArrearProcessBatchId, isApprove);


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

                ArrearProcess _process = new ArrearProcess();
                _process.DeleteEmployeeArrear(ArrearProcessBatchId, EmployeeSystemId);

                return Json(new { Message = "Data deleted successfully", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost, Authorize]
        public ActionResult ArrearFinancialDataReport(string ArrearProcessBatchId, List<string> EmployeeIds)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "Arrear" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                ArrearProcess _arrear = new ArrearProcess();

                IWorkbook workbook = _arrear.GetArrearFinancialData(ArrearProcessBatchId, EmployeeIds);
                workbook.Version = ExcelVersion.Excel2016;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }

            return null;
        }
    }
}