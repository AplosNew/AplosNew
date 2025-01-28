#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using Zen.Barcode;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class QRCodeGenerationEmployeeController : BaseController
    {
        #region Constructor
        /// <summary>   The unitOfMeasurementService service. </summary>
        private readonly ICriticalService _buyerDepartmentService;
        private readonly ISqlRepository _sqlRepository;

        public QRCodeGenerationEmployeeController(ICriticalService buyerDepartmentService, ISqlRepository R
            )
        {
            this._buyerDepartmentService = buyerDepartmentService;
            _sqlRepository = R;
        }
        #endregion


        #region Aplos

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Reports


        [HttpPost, Authorize]
        public ActionResult EmployeeQRCode(string Filter)
        {
            return GenerateQRCodeForEmployee(Filter);
        }


        #endregion

        #region Controllers
        [HttpPost, Authorize]
        public ActionResult GetEmployeeQRCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"  SELECT distinct Emp.SystemID AS Id,
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
                             WHERE EMP.EmployeeStatus='Active' AND EMP.PlantId='" + identity.PlantId + @"'";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #endregion Controllers

        private int MaxPageSize = 500;
        [Authorize]
        public ActionResult GenerateQRCodeForEmployee(string filter)
        {

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataTable dtData = _sqlRepository.GetDataTable(@" SELECT ei.SystemId,ei.EmployeeCode,ei.EmployeeName
                                                                  FROM EmployeeInformation AS ei
                                                                WHERE ei.SystemId in (" + filter + @") AND ei.EmployeeStatus='Active' AND ei.PlantId='" + identity.PlantId + @"'");


                if (dtData.Rows.Count == 0)
                    throw new Exception("No data found");

                double TotalWS = Math.Ceiling((double)((double)dtData.Rows.Count / MaxPageSize));

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create((int)TotalWS);
                workbook.Worksheets[0].Name = "QR";
                sheet = workbook.Worksheets[0];

                IPictureShape pic = null;
                int ROW = 1;
                int CurrentSheet = 0;
                for (int i = 0; i < dtData.Rows.Count; i++)
                {

                    if ((double)i % MaxPageSize == 0)
                    {
                        sheet = workbook.Worksheets[CurrentSheet];


                        sheet.Name = (i + 1).ToString() + " To " + (i + MaxPageSize);
                        ROW = 1;
                        sheet[ROW, 1].ColumnWidth = 30;
                        CurrentSheet++;
                    }
                    if (ROW > 1)
                        sheet.HPageBreaks.Add(sheet.Range[ROW, 1]);


                    sheet[ROW, 1].Text = dtData.Rows[i]["EmployeeCode"].ToString();
                    ROW++;

                    CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;
                    System.Drawing.Image barcodeImg = qrCode.Draw(dtData.Rows[i]["SystemId"].ToString(), 200, 2);

                    pic = sheet.Pictures.AddPicture(ROW, 1, barcodeImg);
                    pic.Width = pic.Height;// (int)(2 * 96);//2 inch 96dpi
                    sheet[ROW, 1].RowHeight = 70;

                    ROW++;
                    sheet[ROW, 1].Text = dtData.Rows[i]["EmployeeName"].ToString();


                    ROW += 2;
                }




                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;


                string strFileName = "QR Employee.xlsx";
                //workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                //workbook.Close();
                //excelEngine.Dispose();

                workbook.Version = ExcelVersion.Excel2013;
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
        }
    }
}