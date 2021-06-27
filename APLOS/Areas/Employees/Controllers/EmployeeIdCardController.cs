#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Service.External;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using System.Drawing;
using System;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using Syncfusion.Pdf;
using Library.Service.Helpers;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeIdCardController : BaseController
    {
        #region Constructor
        private readonly IEmployeeProfileService _employeeInfoService;
        public EmployeeIdCardController(
              IEmployeeProfileService employeeInfoService
            )
        {
            _employeeInfoService = employeeInfoService;
        }
        #endregion

        #region -- Pages
     
        public ActionResult IdCard()  // Id Card for Laila
        {
            return View();
        }

        public ActionResult Aplos() //multiple-idcard
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpPost, Authorize]
        public ActionResult GetAllEmployeeDataWithWorkType()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeInfoService.GetAllEmployeeDataWithWorkType(identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult PrintEmployeeIDCard_backup(string empId, string tempId, string empType, string reportType, string issuDate, string workTypeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "IDCARD-" + empId;
            var workbook = _employeeInfoService.PrintEmployeeIDCard(empId, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, tempId, empType, reportType, issuDate, workTypeId);

            workbook.Save(fileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);

            return null;
        }
        [HttpGet, Authorize]
        public ActionResult PrintEmployeeIDCard(string empId, string tempId, string empType, string reportType, string issuDate, string workTypeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "IDCARD-" + empId;
            var workbook = _employeeInfoService.PrintEmployeeIDCardPpt(empId, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, tempId, empType, reportType, issuDate, workTypeId);

            workbook.Save(fileName + ".pptx", Syncfusion.Presentation.FormatType.Pptx, HttpContext.ApplicationInstance.Response);

            return null;
        }

        //[HttpGet, Authorize]
        //public ActionResult PrintMultipleIDCard(string[] empId, string tempId, string issuDate, string workTypeId, List<Dictionary<string, object>> dataList)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        //    string employeeId = "";
        //    foreach (string item in empId)
        //    {
        //        if (employeeId == "")
        //        {
        //            employeeId = "" + item + ""; ;
        //        }
        //    }

        //    var fileName = "IDCARD-" + empId;
        //    var workbook = _employeeInfoService.EmployeeMultipleIDCard(employeeId, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, tempId, issuDate, workTypeId, dataList);

        //    workbook.Save("IDCARD.pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);

        //    return View();
        //}

        [HttpGet, Authorize]
        public ActionResult PrintMultipleIDCard(string[] empId, string tempId, string issuDate, string workTypeId, List<Dictionary<string, object>> dataList,bool IsCurrentIssueDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           
            string employeeId = "";
            foreach (string item in empId)
            {
                if (employeeId == "")
                {
                    employeeId = "" + item + ""; ;
                }
            }

            var fileName = "IDCARD-" + empId;
            var workbook = _employeeInfoService.EmployeeMultipleIDCardPpt(employeeId, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, tempId, issuDate, workTypeId, dataList, IsCurrentIssueDate);

            workbook.Save("IDCARD.pptx", Syncfusion.Presentation.FormatType.Pptx, HttpContext.ApplicationInstance.Response);

            return null;
        }

    }

    #endregion
}
