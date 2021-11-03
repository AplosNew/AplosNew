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

namespace Aplos.Areas.IE.Controllers
{
    public class QRCodeGenerationOperationController : BaseController
    {
        #region Constructor
        /// <summary>   The unitOfMeasurementService service. </summary>
        private readonly ICriticalService _buyerDepartmentService;
        private readonly ISqlRepository _sqlRepository;

        public QRCodeGenerationOperationController(ICriticalService buyerDepartmentService, ISqlRepository R
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
        public ActionResult OperationQRCode(string Filter)
        {
            return GenerateQRCodeForOperation(Filter);
        }
      

        #endregion

        #region Controllers
        [HttpPost, Authorize]
        public ActionResult GetOperationQRCode()
        {
            string sql = @"  SELECT  ov.Id, ov.OperationId,ov.Code OperationVariationCode,ov.UserName AS OperationVariationName,
                                      o.Code AS OperationCode,o.UserName AS OperationName
                                        FROM mst.OperationVariation AS ov
                                      LEFT OUTER JOIN mst.Operation AS o ON o.Id=ov.OperationId
                                     ";
            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }
      
        #endregion Controllers

        private int MaxPageSize = 500;
        [Authorize]
        public ActionResult GenerateQRCodeForOperation(string filter)
        {

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {

                DataTable dtData = _sqlRepository.GetDataTable(@"  SELECT  ov.Id, ov.OperationId,ov.Code OperationVariationCode,ov.UserName AS OperationVariationName,
                                      o.Code AS OperationCode,o.UserName AS OperationName
                                        FROM mst.OperationVariation AS ov
                                      LEFT OUTER JOIN mst.Operation AS o ON o.Id=ov.OperationId
                                       where ov.Id in (" + filter + ")");


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


                    sheet[ROW, 1].Text = dtData.Rows[i]["OperationVariationCode"].ToString() + "-" + dtData.Rows[i]["OperationVariationName"].ToString();
                    ROW++;

                    CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;
                    System.Drawing.Image barcodeImg = qrCode.Draw(dtData.Rows[i]["Id"].ToString(), 200, 2);

                    pic = sheet.Pictures.AddPicture(ROW, 1, barcodeImg);
                    pic.Width = pic.Height;// (int)(2 * 96);//2 inch 96dpi
                    sheet[ROW, 1].RowHeight = 70;

                    ROW++;
                    sheet[ROW, 1].Text = dtData.Rows[i]["OperationCode"].ToString() + "-" + dtData.Rows[i]["OperationName"].ToString();


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
                workbook.Version = ExcelVersion.Excel2013;
                string strFileName = "QR Operation.xlsx";
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