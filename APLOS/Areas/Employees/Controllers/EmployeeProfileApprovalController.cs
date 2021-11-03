using Aplos.Controllers;
using Aplos.HumanResource;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Service.Employees;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;


namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeProfileApprovalController : BaseController
    {
        #region Constructor

        private readonly IEmployeeProfileService _employeeProfileService;

        public EmployeeProfileApprovalController(IEmployeeProfileService employeeProfileService )
        {
            _employeeProfileService = employeeProfileService;
        }

        #endregion Constructor
        EmployeeProfile employeeProfile = new EmployeeProfile();

        #region Pages

        [Authorize]
        public ActionResult EmployeeProfileApproval()
        {
            return View();
        }


        #endregion Pages
      

        #region Mediasoft Fair Shop Data Export
       


        public static void WriteDataTableToCSV(DataTable dt, string fileName)
        {
            WriteOutCSVResponseHeaders(fileName);
            WriteOutDataTable(dt);
            System.Web.HttpContext.Current.Response.End();
        }


        /// <summary>
        /// Writes out the response headers needed for outputting a CSV file.
        /// </summary>
        /// <param name="fileName">File name for the outputted file</param>
        public static void WriteOutCSVResponseHeaders(string fileName)
        {
            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.ClearHeaders();
            System.Web.HttpContext.Current.Response.ClearContent();
            System.Web.HttpContext.Current.Response.AddHeader("content-disposition", string.Format("attachment; filename={0}-{1}.csv", fileName, DateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss")));
            System.Web.HttpContext.Current.Response.AddHeader("Pragma", "public");
            System.Web.HttpContext.Current.Response.ContentType = "text/csv";
            System.Web.HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
        }


        /// <summary>
        /// Writes out the header row and data rows from a data table.
        /// </summary>
        /// <param name="dt">DataTable which holds the data</param>
        public static void WriteOutDataTable(DataTable dt)
        {
            WriteOutHeaderRow(dt, dt.Columns.Count);
            WriteOutDataRows(dt, dt.Columns.Count, dt.Rows.Count);
        }

        /// <summary>
        /// Writes the header row from a datatable as Http Response
        /// </summary>
        /// <param name="dt">DataTable which holds the data</param>
        /// <param name="colCount">Number of columns</param>
        private static void WriteOutHeaderRow(DataTable dt, int colCount)
        {
            string CSVHeaderRow = string.Empty;
            for (int col = 0; col <= colCount - 1; col++)
            {
                CSVHeaderRow = string.Format("{0}\"{1}\",", CSVHeaderRow, dt.Columns[col].ColumnName);
            }
            WriteRow(CSVHeaderRow);
        }

        /// <summary>
        /// Writes the data rows of a datatable as Http Responses
        /// </summary>
        /// <param name="dt">DataTable which holds the data</param>
        /// <param name="colCount">Number of columns</param>
        /// <param name="rowCount">Number of columns</param>
        private static void WriteOutDataRows(DataTable dt, int colCount, int rowCount)
        {
            string CSVDataRow = string.Empty;
            for (int row = 0; row <= rowCount - 1; row++)
            {
                var dataRow = dt.Rows[row];
                CSVDataRow = string.Empty;
                for (int col = 0; col <= colCount - 1; col++)
                {
                    CSVDataRow = string.Format("{0}\"{1}\",", CSVDataRow, dataRow[col]);
                }
                WriteRow(CSVDataRow);
            }
        }

        /// <summary>
        /// Write out a row as an Http Response.
        /// </summary>
        /// <param name="row">The data row to write out</param>
        private static void WriteRow(string row)
        {
            //System.Web.HttpContext.Current.Response.Write(row.TrimEnd(","));
            System.Web.HttpContext.Current.Response.Write(row.TrimEnd(','));
            System.Web.HttpContext.Current.Response.Write(Environment.NewLine);
        }

        #endregion
      

        #region Employee Approval

        [HttpGet]
        public JsonResult GetUnApprovedEmployeeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //JsonResult json = Json(_employeeProfileService.GetUnApprovedEmployeeList(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            JsonResult json = Json(employeeProfile.GetUnApprovedEmployeeList(identity.CompanyGroupId, identity.PlantId, identity.IsSysAdmin, identity.UserId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpPost]
        public ActionResult SaveApprovedEmployee(IEnumerable<ParaEmployeeInformation> employeeInformation)
        {
            DataSet dsEmployeeOTInformation = Library.Service.Helpers.DataTableExtensions.ToDataSet<ParaEmployeeInformation>(employeeInformation);
            //DataSet dsEmployeeOTInformation = null;
            _employeeProfileService.SaveApprovedEmployeeData(dsEmployeeOTInformation);
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
    public class ParaEmployeeInformation
    {
        public bool CheckBoxSelect { get; set; }
        public string SystemId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DOBs { get; set; }
        public string DOJs { get; set; }
        public string Department { get; set; }
        public string PositionName { get; set; }
        public string EntityName { get; set; }
        public string Designation { get; set; }
        public string Section { get; set; }
        public string LegalDesignation { get; set; }
        public string SubSection { get; set; }
        public string Code { get; set; }

    }
}