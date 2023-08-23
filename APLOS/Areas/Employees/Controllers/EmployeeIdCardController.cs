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
using System.Data;
using Library.Security.Core;
using System.IO;
using Syncfusion.PresentationToPdfConverter;

using Syncfusion.Presentation;
using Syncfusion.Pdf;

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

        [HttpGet, Authorize]
        public ActionResult PrintMultipleIDCard(string[] empId, string tempId, string issuDate, string workTypeId, List<Dictionary<string, object>> dataList, bool IsCurrentIssueDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;



            string employeeId = "";
            foreach (string id in empId)
            {
                if (employeeId == "")
                {
                    employeeId = "" + id + "";
                }

                string[] empIdList = id.Split(',');
                foreach (string item in empIdList)
                {
                    var empData = _employeeInfoService.Find(item);
                    ConnectionManager.DAL.ConManager objCon;
                    string sql = "SELECT * FROM [dbo].[EmployeeIdCardIssue] WHERE EmpSystemId='" + item + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        dr["Id"] = item;
                        dr["Sequence"] = 1;
                        dr["EmpSystemId"] = item;
                        dr["EmployeeWorkTypeId"] = DBNull.Value;
                        dr["IssueDate"] = empData.DOJ;
                        dr["ExpiryDate"] = DBNull.Value;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;


                        dsMaster.Tables[0].Rows.Add(dr);
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }

            }


            var fileName = "IDCARD-" + empId;
            var workbook = _employeeInfoService.EmployeeMultipleIDCardPpt(employeeId, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, tempId, issuDate, workTypeId, dataList, IsCurrentIssueDate);

            workbook.Save("IDCARD.pptx", Syncfusion.Presentation.FormatType.Pptx, HttpContext.ApplicationInstance.Response);

            return null;
        }

        [HttpPost, Authorize]
        public ActionResult GetPrintMultipleIDCard(string[] empId, string tempId, string issuDate, string workTypeId, List<Dictionary<string, object>> dataList, bool IsCurrentIssueDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string employeeId = "";
            foreach (string id in empId)
            {
                if (employeeId == "")
                {
                    employeeId = "" + id + "";
                }
                else
                {
                    employeeId += "," + id + "";
                }

                //string[] empIdList = id.Split(',');

            }

            ConnectionManager.DAL.ConManager objCon;
            string sql = "SELECT * FROM [dbo].[EmployeeIdCardIssue] WHERE EmpSystemId IN(" + employeeId + ")";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

            foreach (string item in empId)
            {
                var empData = _employeeInfoService.Find(item);

                DataView dv = new DataView(dsMaster.Tables[0]);
                dv.RowFilter = "EmpSystemId='" + item + "'";

                if (dv.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = item + "-1";
                    dr["Sequence"] = 1;
                    dr["EmpSystemId"] = item;
                    dr["EmployeeWorkTypeId"] = DBNull.Value;
                    dr["IssueDate"] = empData.DOJ;
                    dr["ExpiryDate"] = DBNull.Value;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
            }

            clsStaticInfo obj = new clsStaticInfo();
            obj.SaveDataSets(dsMaster);

            var fileName = "IDCARD" + identity.UserId + ".pptx";
            var workbook = _employeeInfoService.EmployeeMultipleIDCardPpt(employeeId, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, tempId, issuDate, workTypeId, dataList, IsCurrentIssueDate);

            string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            workbook.Save(fullPath);
            


            return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
    }

    #endregion
}
