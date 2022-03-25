#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using Library.OrderManagement.Production;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using System.Text.RegularExpressions;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Aplos.Areas.Commercial.Controllers;
using System.Drawing;


#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class GeneralWasteController  : BaseController
    {

        GeneralWasteService ws = new GeneralWasteService();
        string TableName = "dbo.WasteMaster";
        
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public GeneralWasteController(ISqlRepository R)
        {
            _sqlRepository = R;
          
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult getProcess()
        {
            return Json(ws.getProcess(), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public ActionResult getUOM()
        {
            return Json(ws.getUOM(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getEntity()
        {
            return Json(ws.getEntity(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult getView(string Id)
        {
            return Json(ws.getView(Id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM dbo.WasteMaster"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = ws.GetMaster(Id);
                var _child = ws.GetChild(Id);
                return Json(new { master = _master , child = _child }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost , Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            return Json(ws.GetList(strkey), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create( List<Dictionary<string, object>> Data , string Date, string LocationId)
        {
            try
            {
                var data = ws.Create(Data , Date, LocationId);
                return Json(new { Error = false, Data= data, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        //[HttpPost]
        //public ActionResult Delete(string id)
        //{
        //    try
        //    {
        //        ws.Delete(id);

        //        return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

        //    }


        //}

        private string GetDate(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            try
            {
                return Convert.ToDateTime(s).ToString("dd-MMM-yyyy");
            }
            catch (Exception)
            {
                return "";
            }
        }

        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [HttpGet, Authorize]
        public ActionResult GetWasteLocationList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select MS.Id,MS.UserName StorageLocation from HKP.MaterialStorage MS where Active=1 and IsWasteLocation=1 and PlantId='" + identity.PlantId + "' ";

                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetGeneralWasteReport(string Id)
        {
            try
            {
                string fileName = "";
                fileName = GeneralWasteReport(Id, "GeneralWasteReport");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string GeneralWasteReport(string Id, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                var report = new ReportUtility();
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "General Waste Report";
                sheet = workbook.Worksheets[0];

                DataTable data = WasteReportSQL(Id);

                int ROW = 5; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "ChalanId";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColChalanId = COL;
                COL++;

                sheet[ROW, COL].Text = "Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColRemarks = COL;
                COL++;
                sheet[ROW, COL].Text = "Item Name";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColItemName = COL;
                COL++;

                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColProcess = COL;
                COL++;
                sheet[ROW, COL].Text = "Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "SubCategory";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSubCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Quantity";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColQuantity = COL;
                COL++;
                sheet[ROW, COL].Text = "UOM";
                sheet[ROW, COL].ColumnWidth = 22;
                int ColUOM = COL;
                COL++;
                sheet[ROW, COL].Text = "RowId";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColRowId = COL;
                COL++;
                sheet[ROW, COL].Text = "PreparedBy";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColPreparedBy = COL;
                COL++;
                sheet[ROW, COL].Text = "Checked By";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColCheckedBy = COL;
                COL++;
                sheet[ROW, COL].Text = "Authorized By";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColAuthorizedBy = COL;
                COL++;
                sheet[ROW, COL].Text = "Received By";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColReceivedBy = COL;
                
                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColChalanId].Text = data.Rows[i]["ChalanId"].ToString();
                    sheet[ROW, ColDate].Text = GetDate(data.Rows[i]["Date"].ToString());
                    sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, ColCategory].Text = data.Rows[i]["Category"].ToString();
                    sheet[ROW, ColSubCategory].Text = data.Rows[i]["SubCategory"].ToString();
                    sheet[ROW, ColItemName].Text = data.Rows[i]["ItemName"].ToString();
                    sheet[ROW, ColUOM].Text = data.Rows[i]["UOM"].ToString();
                    sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                    sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                    
                    sheet[ROW, ColQuantity].Number = clsStaticInfo.dbl(data.Rows[i]["Quantity"].ToString());
                    sheet[ROW, ColQuantity].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet[ROW, ColUOM].Text = data.Rows[i]["UOM"].ToString();
                    sheet[ROW, ColRowId].Text = data.Rows[i]["RowId"].ToString();
                    sheet[ROW, ColPreparedBy].Text = data.Rows[i]["PreparedBy"].ToString();
                    
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "General Waste Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;




                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable WasteReportSQL(string Id)
        {
            try
            {

                string strSQL = @"select WTD.Id ChalanId,format(WTD.Date,'dd-MMM-yyyy')Date,E.UserName Entity,WTD.Remarks,WM.ItemName,P.UserName Process
									,WM.Category,WM.SubCategory,WTD.Quantity,UOM.UserName UOM,WTD.Id RowId,WTD.AddedBy PreparedBy
									
				                    from WasteTransactionData WTD
									left join ORG.Entity E on E.Id=WTD.EntityId
				                    left join WasteMaster WM on WM.Id=WTD.WasteMasterId
				                    left join SCS.UnitOfMeasurement UOM on UOM.Id=WM.UOMId
									LEFT JOIN WasteIssueDetails WID ON WID.WasteTransactionDataId=WTD.Id
                                    left join HKP.Process P on P.Id=WID.ProcessId

									where WTD.EntityId='" + Id + @"'";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        [HttpGet, Authorize]
        public ActionResult DownloadUsingFullPath(string FullPath, string fileName)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                //string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(FullPath);
                try
                {
                    System.IO.File.Delete(FullPath);
                }
                catch (Exception)
                {
                }

                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;

            }
            catch (Exception ex)
            {


            }
            return null;
        }
    }
}

