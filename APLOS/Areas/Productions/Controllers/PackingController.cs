#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using Library.OrderManagement.Production;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Library.Data;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using System.Text.RegularExpressions;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Aplos.Areas.Commercial.Controllers;
using System.Drawing;
using Library.Model.Enums;
using Syncfusion.ExcelToPdfConverter;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class PackingController : BaseController
    {
        PackingData det = new PackingData();

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public PackingController(ISqlRepository R)
        {
            _sqlRepository = R;
            det = new PackingData();
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetList(string ToDate, string FromDate, string type, string group, string column, string value, string Loc)
        {
            try
            {
                var jj = det.GetData(ToDate, FromDate, type, group, column, value, Loc);
                var jsondata = Json(new { Error = false, DATA = jj }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult getClickData(string productCode, string poid)
        {
            return Json(det.getClickData(poid, productCode), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getPurposeCategory()
        {
            try
            {
                return Json(det.getPurposeCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpPost]
        public ActionResult GetEntity()
        {
            try
            {
                return Json(det.GetEntity(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult getLocations()
        {
            return Json(det.getLocations(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getpackingGridOne(string productCode)
        {
            try
            {
                return Json(det.getpackingGridOne(productCode), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getMOwithCustomers(string Customers)
        {
            try
            {
                return Json(det.getMOwithCustomers(Customers), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [Authorize, HttpGet]
        public ActionResult getSOfromProduct(string column, string value)
        {
            try
            {
                return Json(det.getSOfromProduct(column, value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpGet]
        public ActionResult getCustomers()
        {
            try
            {
                return Json(det.getCustomers(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getEmployees()
        {
            try
            {
                return Json(det.getEmployees(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getByWhomEmployees()
        {
            try
            {
                return Json(det.getByWhomEmployees(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getStorageLoc()
        {
            try
            {
                return Json(det.getStorageLoc(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult getEntity()
        {
            try
            {
                return Json(det.getEntity(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult getSoFromCustomer(string customer)
        {
            try
            {
                return Json(det.getSoFromCustomer(customer), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult getPoLotReference(string productCode, string toDispatch, string PO, string FromDate, string ToDate)
        {
            try
            {
                return Json(det.getPoLotReference(productCode, toDispatch, PO, FromDate, ToDate), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult getCartonsDetails(string LotNo, string ProductCode, string PO)
        {
            try
            {
                var kk = det.getCartonsDetails(LotNo, ProductCode, PO, out List<Dictionary<string, object>> dts);
                return Json(new { Data = kk, Inactive = dts }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult getPackingList()
        {
            try
            {
                var kk = det.getPackingList();
                return Json(new { Data = kk }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [Authorize, HttpGet]
        public ActionResult PackingList(string PackingId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            GetPackingListReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, PackingId);

            return View();
        }


        [Authorize, HttpGet]
        public ActionResult PackingListPDFReport(ReportFormat reportFormat, string packingId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = PackingListPDFReport(packingId);
                var reportFileName = packingId + " PackingList";

                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        PdfDocument document = new PdfDocument();
                        ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings();
                        settings.TemplateDocument = document;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document = converter1.Convert(settings);
                        }
                        document.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
                        return null;
                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public ActionResult PackingListXLReport(ReportFormat reportFormat, string packingId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = PackingListPDFReport(packingId);
                var reportFileName = packingId + " PackingList";

                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        PdfDocument document = new PdfDocument();
                        ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings();
                        settings.TemplateDocument = document;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document = converter1.Convert(settings);
                        }
                        document.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
                        return null;
                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        private IWorkbook PackingListPDFReport(string packingId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            ReportUtility ru = null;
            var data = det.GetScanDataReport(packingId);
            var sheet = workbook.Worksheets[0];

            ru = new ReportUtility();

            #region sheet1
            sheet.Name = "Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;
            string Article = "";

            #region Grid Headers
            sheet.Range[ROW, COL + 3].Text = "Invoice No:" + " " +  data.Rows[0]["InvoiceNo"].ToString();
            sheet.Range[ROW, COL, ROW, COL + 3].Merge();
            sheet.Range[ROW, COL, ROW, COL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL, ROW, COL + 3].BorderAround(ExcelLineStyle.Thin);

            /*sheet.Range[ROW, COL + 2].Text = data.Rows[0]["InvoiceNo"].ToString();
            sheet.Range[ROW, COL + 2, ROW, COL + 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL + 2, ROW, COL + 6].Merge();
            sheet.Range[ROW, COL + 2, ROW, COL + 6].BorderAround(ExcelLineStyle.Thin);*/
            ROW++;

            sheet.Range[ROW, COL + 3].Text = "Invoice Date:" + " " + data.Rows[0]["InvoiceDate"].ToString();
            sheet.Range[ROW, COL, ROW, COL + 3].Merge();
            sheet.Range[ROW, COL, ROW, COL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL, ROW, COL + 3].BorderAround(ExcelLineStyle.Thin);
            /*sheet.Range[ROW, COL + 2].Text = data.Rows[0]["InvoiceDate"].ToString();
            sheet.Range[ROW, COL + 2, ROW, COL + 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL + 2, ROW, COL + 6].Merge();
            sheet.Range[ROW, COL + 2, ROW, COL + 6].BorderAround(ExcelLineStyle.Thin)*/;
            ROW++;

            sheet.Range[ROW, COL + 3].Text = "Name of Consignee:" + " " + data.Rows[0]["ConsigneeBilltoName"].ToString();
            sheet.Range[ROW, COL, ROW, COL + 3].Merge();
            sheet.Range[ROW, COL, ROW, COL + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL, ROW, COL + 3].BorderAround(ExcelLineStyle.Thin);
            /*sheet.Range[ROW, COL + 2].Text = data.Rows[0]["ConsigneeBilltoName"].ToString();
            sheet.Range[ROW, COL + 2, ROW, COL + 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL + 2, ROW, COL + 6].Merge();
            sheet.Range[ROW, COL + 2, ROW, COL + 6].WrapText = true;
            sheet.Range[ROW, COL + 2, ROW, COL + 6].BorderAround(ExcelLineStyle.Thin);*/
            ROW++;

            int ArtRow = ROW;
            
            ROW = 10;
            report.SetHeaderText(ref sheet, ROW, COL, "S.No", 5, ExcelHAlign.HAlignCenter);
            int SNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 40, ExcelHAlign.HAlignCenter);
            int ColArticles = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shade", 10, ExcelHAlign.HAlignCenter);
            int ColSahde = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "LOT NO", 15, ExcelHAlign.HAlignCenter);
            int ColLot = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "REF NO", 9, ExcelHAlign.HAlignCenter);
            int ColREF = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "No of Cones", 5, ExcelHAlign.HAlignCenter);
            int ColNocones = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "N.WEIGHT", 8, ExcelHAlign.HAlignCenter);
            int ColNtWt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "G.WEIGHT", 8, ExcelHAlign.HAlignCenter);
            int ColGWt = COL;
            COL++;

            ROW++;
            endCol = COL;
            #endregion Headers
            ROW = 11;
            int catFRow = ROW;
            int SRC = 0;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                SRC++;
                if (Article != data.Rows[i]["Article"].ToString())
                {
                    SRC = 1;
                    Article = data.Rows[i]["Article"].ToString();

                    if (catFRow < ROW)
                    {
                        sheet[ROW, 2].Text = "TOTAL:";

                        sheet.Range[ROW, ColNtWt].Formula = "=SUM(" + ru.GetColumnNameForXls(ColNtWt) + catFRow + ":" + ru.GetColumnNameForXls(ColNtWt) + (ROW - 1) + ")";
                        sheet.Range[ROW, ColNtWt].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet[ROW, ColNtWt].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[ROW, ColGWt].Formula = "=SUM(" + ru.GetColumnNameForXls(ColGWt) + catFRow + ":" + ru.GetColumnNameForXls(ColGWt) + (ROW - 1) + ")";
                        sheet.Range[ROW, ColGWt].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet[ROW, ColGWt].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[ROW, 1, ROW, ColGWt].BorderAround(ExcelLineStyle.Thin);
                        sheet.Range[ROW, 1, ROW, ColGWt].CellStyle.Font.Bold = true;
                        ROW++;
                        ArtRow = ROW;
                        ROW++;
                    }
                    /*sheet[ArtRow, 1, ArtRow, 5].Text = data.Rows[i]["Article"].ToString();
                    sheet.Range[ArtRow, 1, ArtRow, 5].Merge();
                    sheet.Range[ArtRow, 1, ArtRow, 5].WrapText = true;
                    sheet.Range[ArtRow, 1, ArtRow, 5].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[ArtRow, 1, ArtRow, 5].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[ArtRow, 1, ArtRow, 5].BorderAround(ExcelLineStyle.Thin);*/
                    if (catFRow < ROW)
                    {
                        catFRow = ROW;
                    }
                }
                sheet[ROW, SNo].Text = SRC.ToString();
                sheet.Range[ROW, SNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, SNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet[ROW, ColArticles].Text = data.Rows[i]["ATS"].ToString();
                sheet.Range[ROW, ColArticles].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColArticles].RowHeight = 25;
                sheet[ROW, ColSahde].Text = data.Rows[i]["Shade"].ToString();
                sheet.Range[ROW, ColSahde].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColLot].Text = data.Rows[i]["LotNo"].ToString();
                sheet.Range[ROW, ColLot].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColLot].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet[ROW, ColREF].Text = data.Rows[i]["RefNo"].ToString();
                sheet[ROW, ColREF].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet[ROW, ColNocones].Text = data.Rows[i]["Cones"].ToString();
                sheet.Range[ROW, ColNocones].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[ROW, ColREF].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColNtWt].Number = clsStaticInfo.dbl(data.Rows[i]["netWeight"].ToString());
                sheet.Range[ROW, ColNtWt].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColNtWt].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet[ROW, ColGWt].Number = clsStaticInfo.dbl(data.Rows[i]["GWeight"].ToString());
                sheet.Range[ROW, ColGWt].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColGWt].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[ROW, SNo, ROW, ColGWt].BorderAround(ExcelLineStyle.Thin);
                ROW++;
                ArtRow = ROW;
            }

            sheet[ROW, 2].Text = "TOTAL:";

            sheet.Range[ROW, ColNtWt].Formula = "=SUM(" + ru.GetColumnNameForXls(ColNtWt) + catFRow + ":" + ru.GetColumnNameForXls(ColNtWt) + (ROW - 1) + ")";
            sheet.Range[ROW, ColNtWt].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColNtWt].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW, ColGWt].Formula = "=SUM(" + ru.GetColumnNameForXls(ColGWt) + catFRow + ":" + ru.GetColumnNameForXls(ColGWt) + (ROW - 1) + ")";
            sheet.Range[ROW, ColGWt].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet[ROW, ColGWt].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[ROW, 1, ROW, ColGWt].BorderAround(ExcelLineStyle.Thin);
            sheet.Range[ROW, 1, ROW, ColGWt].CellStyle.Font.Bold = true;

            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, 1, "Packing List", identity.CompanyId);
            //reportUtility.PlantHeader(ref sheet, 1, "Packing List", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Portrait);

            return workbook;
        }

        public void GetPackingListReport(string companyGroupId, string companyId, string plantId, string UserId, string PackingId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ReportUtility ru = new ReportUtility();
            fileName = "PackingList" + plantId + ".docx";

            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            WordDocument document = new WordDocument(File, FormatType.Docx);

            try
            {
                WSection section = document.Sections[0];

                DataTable dsOrderMaster;
                DataTable dsTermsAndCondition;

                dsOrderMaster = PackingListSQL(PackingId);
                dsTermsAndCondition = TermsAndConditionSQL(dsOrderMaster.Rows[0]["ContractId"].ToString());

                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makePackingListService(companyGroupId, companyId, plantId, PackingId, document, dsOrderMaster);   // {materialItems}
                var TermsAndCondition = makeTermsAndCondition(companyGroupId, companyId, plantId, PackingId, document, dsTermsAndCondition);   // {materialItems}

                //document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                ////document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                //document.Replace("{TotalInWords}", ru.InWord((MaterialTotal), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);


                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        //ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Tables[0].Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                        document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);

                        
                    }
                    if (text == "{PRINTEDBY}")
                    {
                        document.Replace(text, identity.Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                //string fn = "Printed By: " + identity.Name + "                                        Date&Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt");

                //IWParagraph paragraph = section.AddParagraph();
                //WFootnote footnote = (WFootnote)paragraph.AppendFootnote(Syncfusion.DocIO.FootnoteType.Endnote);
                //footnote.MarkerCharacterFormat.SubSuperScript = SubSuperScript.SuperScript;
                //document.EndnoteNumberFormat = FootEndNoteNumberFormat.LowerCaseRoman;               
                //paragraph = footnote.TextBody.AddParagraph();
                //paragraph.AppendText(fn);
               

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                
               
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "PackingListReport-" + PackingId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                //throw ex;

            }

            document.Close();
        }
        public DataTable PackingListSQL(string PackingId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT  P.PackingId, moi.Id MasterOrderItemID,c.Id as ContractId,mm.UserName MaterialDescription,mma.StandardName as Article,h.Code as HSNCode
                                ,PLC.LCRef,Format(PLC.LCDate,'dd-MMM-yyyy') LCDate,B.UserName as IssueingBank,AM.Address1 IssueingBankAddress,ISNULL(sc.GWeight,0)GWeight,ISNULL(sc.NetWeight,0)NetWeight, ISNULL(sc.NoOfPackages,0)NoOfPackages

                                ,CartonSerialNo = (Select Stuff((Select distinct ','+isc.RefNo
                                from dbo.ItemScanChild isc 
								left join trn.POLotReference pol on pol.Id = isc.PackingId
							    left join trn.PackingLineItem pli on pli.PackingLineItemId = pol.PackingLineItemId
                                where isc.NetWeight=sc.NetWeight and isc.GWeight=sc.GWeight and pli.PackingId = '" + PackingId + @"'
                                for xml path('')
                                ),1,1,''))

                                ,ISNULL(sc.TotalQtyNetWeight,0)TotalQtyNetWeight,ISNULL(sc.GrossWeight,0)GrossWeight,sc.ProductCode, sc.LotNo,FORMAT(p.AddedDate,'dd-MMM-yyyy') PackingDate,
                                u.UserName as UoM,pbt.UserName as ConsigneeBilltoName,pst.UserName as ConsigneeShiptoName,pst.UserName as AcceptedBy,c.InvoicingByAddress as ConsigneeBillToAddress,c.DeliveryByAddress as ConsigneeShipToAddress,cu.Code as CurrencyName,cu.Id CurrencyId,
                                c.ContractNo,FORMAT(c.AddedDate,'dd-MMM-yyyy') AddedDate,PT.UserName PaymentTerm
                              ,SP.SalesId InvoiceNo,FORMAT(S.InvoiceDate,'dd-MMM-yyyy') InvoiceDate,P.AddedBy CreatedBy
                                from trn.Packing as p 
                                LEFT JOIN TRN.PackingLineItem pli on pli.PackingId=p.PackingId
                                LEFT JOIN TRN.POLotReference plr on plr.PackingLineItemId= pli.PackingLineItemId
                                LEFT JOIN TRN.SalesOrder as so on so.Id=pli.SOId
                                LEFT JOIN TRN.MasterOrderItem as moi on moi.id=so.MasterOrderItemId
                                LEFT JOIN dbo.[contract] as c on c.id = so.contractId
								
                                LEFT JOIN dbo.PurchaseLC PLC on PLC.ContractId=C.Id						
                                LEFT JOIN  MST.BankMaster IB on IB.Id=PLC.OpeningBankMasterId
                                LEFT JOIN  HKP.Bank B on B.Id=IB.BankId
                                LEFT JOIN MST.AddressMaster AM on AM.Id = B.AddressMasterId
                                
                                LEFT JOIN HKP.Party as pc on pc.Id=c.CustomerId
                                LEFT JOIN HKP.PartyPlant as pbt on pbt.Id=c.InvoicingPartyPlantId
                                LEFT JOIN HKP.PartyPlant as pst on pst.Id=c.DeliveryPartyPlantId
                                LEFT JOIN MST.Destination DS ON DS.Id=SO.DestinationId
                                LEFT JOIN MST.MaterialMaster as mm on mm.Id=moi.MaterialMasterId
                                LEFT JOIN HKP.HSNCode as h on h.Id=mm.HSNCodeId
                                LEFT JOIN MST.MaterialMasterArticle as mma on mma.MaterialMasterId=mm.Id AND MOI.ArticleId=MMA.Id
                                LEFT JOIN TRN.MasterOrder as mo on mo.id=moi.MasterOrderId
                                LEFT JOIN SCS.UnitOfMeasurement as u on u.Id=mo.TotalQtyUOMId
                                LEFT JOIN SCS.Currency as cu on cu.Id=mo.CurrencyId
                                LEFT JOIN MST.PaymentTerm PT ON PT.Id=MO.PaymentTermId
								
                                LEFT JOIN 
                                (
                                SELECT sc.netWeight,sc.GWeight,
                                Count(sc.RefNo) as NoOfPackages, sc.ProductCode ,sc.POId , sc.LotNo
                                ,(sc.NetWeight * Count(sc.RefNo)) as TotalQtyNetWeight,(sc.GWeight * Count(sc.RefNo)) as GrossWeight
                                FROM dbo.ItemScanChild sc 
								LEFT JOIN TRN.POLotReference pol on pol.Id = sc.PackingId
							    LEFT JOIN TRN.PackingLineItem pli on pli.PackingLineItemId = pol.PackingLineItemId
							    WHERE pli.PackingId = '" + PackingId + @"'  and sc.Booked = 1
                                GROUP BY  sc.ProductCode ,sc.POId , sc.LotNo,sc.netWeight,sc.GWeight
                                ) as sc on sc.LotNo = plr.LotNo and sc.ProductCode = plr.ProductCode and sc.POId = plr.PONo

                                LEFT JOIN dbo.SalesPacking SP on SP.PackingId=p.PackingId
								LEFT JOIN TRN.Sales S on S.Id=SP.SalesId

                                where P.PackingId ='" + PackingId + @"'";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public DataTable TermsAndConditionSQL(string ContractId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT ROW_NUMBER() OVER(ORDER BY TC.Sequence) RoWNo,
                        tc.Description as TermsAndConditions from dbo.ContractTermsAndConditions as ctc
                        left outer join hkp.TermsAndConditions as tc on tc.Id=ctc.TermsAndConditionsId
                        where ctc.ContractId='" + ContractId + "' Order By TC.Sequence ";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }


        public double makePackingListService(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{MaterialDescription}";

            DataTable sales;

            int LasColumnIndex = 10;

            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Sl#");
            range.ApplyCharacterFormat(FontBold);
            int colSrNo = COL; COL++;
            wTable.Rows[ROW].Cells[colSrNo].Width = 35;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description Of Goods");
            range.ApplyCharacterFormat(FontBold);
            int colDescriptionOfGoods = COL; COL++;
            wTable.Rows[ROW].Cells[colDescriptionOfGoods].Width = 110;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 35;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Product Code");
            range.ApplyCharacterFormat(FontBold);
            int colProductCode = COL; COL++;
            wTable.Rows[ROW].Cells[colProductCode].Width = 40;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("PR/LOT No");
            range.ApplyCharacterFormat(FontBold);
            int colPRLotNo = COL; COL++;
            wTable.Rows[ROW].Cells[colPRLotNo].Width = 59;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Avg Net Weight/Package");
            range.ApplyCharacterFormat(FontBold);
            int colAvgNetWeight = COL; COL++;
            wTable.Rows[ROW].Cells[colAvgNetWeight].Width = 47;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Avg Gross Weight/Package");
            range.ApplyCharacterFormat(FontBold);
            int colcolAvgGrossWeight = COL; COL++;
            wTable.Rows[ROW].Cells[colcolAvgGrossWeight].Width = 47;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("No OF Package");
            range.ApplyCharacterFormat(FontBold);
            int colNoOFPackage = COL; COL++;
            wTable.Rows[ROW].Cells[colNoOFPackage].Width = 48;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Carton Serial No");
            range.ApplyCharacterFormat(FontBold);
            int colCartonSerialNo = COL; COL++;
            wTable.Rows[ROW].Cells[colCartonSerialNo].Width = 55;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Quantity Net Weight In" + "(" + "" + dsOrderMaster.Rows[0]["UoM"].ToString() + "" + ")" + " ");
            range.ApplyCharacterFormat(FontBold);
            int colTotalQty = COL; COL++;
            wTable.Rows[ROW].Cells[colTotalQty].Width = 58;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Gross Weight In " + "(" + "" + dsOrderMaster.Rows[0]["UoM"].ToString() + "" + ")" + " ");
            range.ApplyCharacterFormat(FontBold);
            int colGrossWeight = COL;
            wTable.Rows[ROW].Cells[colGrossWeight].Width = 45;

            #endregion column headers
            //double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            int PreviousNo = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                //
                ROW++;
                sl++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }

                TROW.Cells[colSrNo].AddParagraph().AppendText(sl.ToString());
                TROW.Cells[colDescriptionOfGoods].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialDescription"].ToString());
                TROW.Cells[colHSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colProductCode].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ProductCode"].ToString());
                TROW.Cells[colPRLotNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["LotNo"].ToString());
                TROW.Cells[colAvgNetWeight].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["NetWeight"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colcolAvgGrossWeight].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["GWeight"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colNoOFPackage].AddParagraph().AppendText(dsOrderMaster.Rows[i]["NoOfPackages"].ToString());
                //TROW.Cells[colCartonSerialNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["CartonSerialNo"].ToString());
                if (i == 0)
                {
                    if (Convert.ToInt32(dsOrderMaster.Rows[i]["NoOfPackages"].ToString()) == 1)
                        TROW.Cells[colCartonSerialNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["NoOfPackages"].ToString());
                    else
                        TROW.Cells[colCartonSerialNo].AddParagraph().AppendText(1 + "-" + dsOrderMaster.Rows[i]["NoOfPackages"].ToString());
                    PreviousNo += Convert.ToInt32(dsOrderMaster.Rows[i]["NoOfPackages"].ToString());

                }
                else
                {
                    int LastPkg = Convert.ToInt32(dsOrderMaster.Rows[i]["NoOfPackages"].ToString()) + PreviousNo;
                    TROW.Cells[colCartonSerialNo].AddParagraph().AppendText(PreviousNo + 1 + "-" + LastPkg);
                    PreviousNo += Convert.ToInt32(dsOrderMaster.Rows[i]["NoOfPackages"].ToString());

                }


                TROW.Cells[colTotalQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TotalQtyNetWeight"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colGrossWeight].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["GrossWeight"].ToString()).ToString("#,##0.00"));

                //totalValue += clsStdLib.dbl(sales.Rows[i]["TrnAmount"].ToString());
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            range.ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                //|| dicTaxes.ContainsValue(C)
                if (C == colDescriptionOfGoods || C == colHSN || C == colProductCode || C == colPRLotNo || C == colAvgNetWeight || C == colcolAvgGrossWeight || C == colCartonSerialNo)
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
            }
            #endregion Totals
            ROW++;
            #region Sub Total
            //double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(Amount)", "").ToString());
            #endregion Total
            ROW++;


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            //for (int R = 0; R < wTable.Rows.Count; R++)
            //{
            //    WTableRow TROW = wTable.Rows[R];
            //    TROW.Cells[0].Width = 30;
            //    if (dv.Count < 3)
            //        TROW.Cells[0].Width = 30 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

            //    for (int CE = 0; CE < TROW.Cells.Count; CE++)
            //    {
            //        foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
            //        {
            //            item.ApplyStyle("MyStyle");
            //        }
            //    }
            //}


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            //for (int i = 0; i < dv.Count; i++)
            //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            //for (int i = 0; i <= colTotalTaxableAmount; i++)
            //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return 0;
        }

        public double makeTermsAndCondition(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsTermsAndCondition)
        {
            string replaceString = "{TermsAndCondition}";


            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("TERMS OF DELIVERY AND PAYMENT");
            range.ApplyCharacterFormat(FontBold);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 250;

            #endregion column headers
            //double totalValue = 0;
            int sl = 0;
            //int startRow = 0;
            for (int i = 0; i < dsTermsAndCondition.Rows.Count; i++)
            {
                ROW++;
                sl++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsTermsAndCondition.Rows[i]["RoWNo"].ToString() + "." + dsTermsAndCondition.Rows[i]["TermsAndConditions"].ToString());

            }
            ROW++;

            #region Total
            //int TotalRow = ROW;
            //wTable.AddRow();
            //WTableRow _TROW = wTable.LastRow;

            //range.ApplyCharacterFormat(FontBold);
            #endregion Total
            ROW++;
            #region paragrpath formats

            IWParagraphStyle myStyle = document.AddParagraphStyle("ServiceStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            #endregion paragrpath formats

            #region merging section

            //tax codes merging (horizontal)
            ROW = 0;
            ROW++;
            #endregion merging section
            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return 0;
        }




        [HttpPost]
        public JsonResult CreateAll(Dictionary<string, object> Packingdata, Dictionary<string, object> PackingLineItemdata, Dictionary<string, object> POLotRefData, List<Dictionary<string, object>> Cartons, List<Dictionary<string, object>> POLotCollection, int lastIndex)
        {
            try
            {
                det.CreateAll(Packingdata, PackingLineItemdata, POLotRefData, Cartons, POLotCollection, lastIndex, out int ll);
                return Json(new { Error = false, Data = Packingdata, lastIndex = ll, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult getgridPacking()
        {
            try
            {

                return Json(det.getgridPacking(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult UntagPacking(string packingId)
        {
            try
            {
                det.UntagPacking(packingId);
                return Json(new { Message = AplosMessage.Deleted });
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        

        [HttpGet, Authorize]
        public ActionResult openPackingLineItemModal(string PackingId)
        {
            try
            {

                return Json(det.getPackingLineItemModal(PackingId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult openPOLotRefGridModal(string PackingLineItemId)
        {
            try
            {
                return Json(det.getPOLotRefGridModal(PackingLineItemId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPrintReport(string PackingId)
        {

            try
            {
                var workbook = GetFilterData(PackingId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "LoadingPlanReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IWorkbook GetFilterData(string PackingId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            sheet.Name = "Loading Plan Report";




            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable mainHeader = det.getMainHeadersReport(PackingId);
            DataTable data = det.GetReportData(PackingId);
            DataTable packageDetail = det.getPackageDetailReport(PackingId);
            #region Main Headers

            SetHeaderTextTop(ref sheet, ROW, COL, "Customer", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["Customer"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Packing Id", 13, ExcelHAlign.HAlignLeft);

            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["PackingId"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Packing Creation Date", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["Date"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Inactive Date", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["InactiveDate"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "By Whom", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["ByWhom"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            COL = 1;
            ROW++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Despatch Responsible Person Id", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["DRespPerson"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Storage Location", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["StorageLoc"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Entity", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["Entity"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            SetHeaderTextTop(ref sheet, ROW, COL, "Remarks (If Any)", 13, ExcelHAlign.HAlignLeft);
            COL += 2;
            SetTextTop(ref sheet, ROW, COL, mainHeader.Rows[0]["Remarks"].ToString(), 13, ExcelHAlign.HAlignLeft);
            COL++;
            COL = 1;
            ROW += 2;
            #endregion


            sheet.Range[ROW, COL].Text = "ITEM DETAIL ";
            sheet.Range[ROW, COL].ColumnWidth = 13;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW += 2;
            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Packing Line Id", 13, ExcelHAlign.HAlignCenter);
            int ColPLID = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Master Order No", 13, ExcelHAlign.HAlignCenter);
            int ColMONo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Item Id", 13, ExcelHAlign.HAlignCenter);
            int ColItemId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO Id", 13, ExcelHAlign.HAlignCenter);
            int ColSoId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material", 13, ExcelHAlign.HAlignCenter);
            int ColMaterial = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 13, ExcelHAlign.HAlignCenter);
            int ColArticle = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Product Code", 13, ExcelHAlign.HAlignCenter);
            int ColProductCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PO No", 15, ExcelHAlign.HAlignCenter);
            int ColPONo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Lot No", 13, ExcelHAlign.HAlignCenter);
            int ColLotNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shade No", 13, ExcelHAlign.HAlignCenter);
            int Colshade = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plan Qty", 13, ExcelHAlign.HAlignCenter);
            int ColPlanQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Bages", 13, ExcelHAlign.HAlignCenter);
            int ColBages = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO PO No", 13, ExcelHAlign.HAlignCenter);
            int ColSoPoNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Stock Qty", 13, ExcelHAlign.HAlignCenter);
            int ColStockQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "No Of Packages", 13, ExcelHAlign.HAlignCenter);
            int ColPackages = COL;
            COL++;

            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColPLID].Text = data.Rows[i]["PackingLineItemId"].ToString();
                sheet[ROW, ColMONo].Text = data.Rows[i]["MasterOrderNo"].ToString();
                sheet[ROW, ColItemId].Text = data.Rows[i]["ItemId"].ToString();
                sheet[ROW, ColSoId].Text = data.Rows[i]["SoId"].ToString();
                sheet[ROW, ColMaterial].Text = data.Rows[i]["Material"].ToString();
                sheet[ROW, ColArticle].Text = data.Rows[i]["Article"].ToString();
                sheet[ROW, ColProductCode].Text = data.Rows[i]["ProductCode"].ToString();
                sheet[ROW, ColPONo].Text = data.Rows[i]["PONo"].ToString();
                sheet[ROW, ColLotNo].Text = data.Rows[i]["LotNo"].ToString();
                sheet[ROW, Colshade].Text = data.Rows[i]["ShadeNo"].ToString();
                sheet[ROW, ColPlanQty].Text = data.Rows[i]["PlanQty"].ToString();
                sheet[ROW, ColBages].Text = data.Rows[i]["Bages"].ToString();
                sheet[ROW, ColSoPoNo].Text = data.Rows[i]["SoPoNo"].ToString();
                sheet[ROW, ColStockQty].Text = data.Rows[i]["StockQty"].ToString();
                sheet[ROW, ColPackages].Text = data.Rows[i]["NoOfPackages"].ToString();



                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            ROW++;
            #region Package Detail
            COL = 1;
            sheet.Range[ROW, COL].Text = "PACKAGE DETAIL ";
            sheet.Range[ROW, COL].ColumnWidth = 13;
            sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ROW += 2;
            COL = 1;
            report.SetHeaderText(ref sheet, ROW, COL, "Packing Line Id", 13, ExcelHAlign.HAlignCenter);
            int ColPkLineId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Cartons", 13, ExcelHAlign.HAlignCenter);
            sheet.Range[ROW, 2, ROW, 12].Merge();
            int ColCartons = COL;
            ROW++;
            COL = 12;
            sheet.Range[ROW, 2, ROW, COL].Merge();

            for (int i = 0; i < packageDetail.Rows.Count; i++)
            {
                sheet[ROW, ColPkLineId].Text = packageDetail.Rows[i]["PackingLineItemId"].ToString();

                sheet[ROW, ColPkLineId].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet[ROW, ColCartons].Text = packageDetail.Rows[i]["Cartons"].ToString();
                sheet.Range[ROW, 1, ROW, COL].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, COL].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }
            ROW++;
            #endregion
            endRow = ROW - 1;
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Loading Plan Report", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;

        }
        private void SetTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {

            sheet.Range[row, col - 1, row, col].Text = txt;
            sheet.Range[row, col - 1, row, col].Merge();
            sheet.Range[row, col - 1, row, col].ColumnWidth = width;
            sheet.Range[row, col - 1, row, col].HorizontalAlignment = al;
            sheet.Range[row, col - 1, row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;

        }
        private void SetText(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {

            sheet.Range[row, col, row, col].Text = txt;
            sheet.Range[row, col, row, col].ColumnWidth = width;
            sheet.Range[row, col, row, col].HorizontalAlignment = al;
            sheet.Range[row, col, row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;

        }


        [HttpPost, Authorize]
        public ActionResult GetStockReport(string ToDate, string FromDate, string type, string group, string column, string value, string Loc)
        {

            try
            {
                var workbook = GetStockReportForm(ToDate, FromDate, type, group, column, value, Loc);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "StockReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        private IWorkbook GetStockReportForm(string ToDate, string FromDate, string type, string group, string column, string value, string Loc)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = det.GetStockData(ToDate, FromDate, type, group, column, value, Loc);


            var sheet = workbook.Worksheets[0];
            sheet.Name = "Stock Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            //sheet.Range[ROW, COL].Text = "From - "+FromDate+" , To - "+ToDate;
            //sheet.Range[ROW, COL].ColumnWidth = 13;
            //sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            //sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //ROW += 2;

            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Assigned", 13, ExcelHAlign.HAlignCenter);
            int ColAssigned = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Customer", 13, ExcelHAlign.HAlignCenter);
            int ColCustomer = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Ex Factory Date", 13, ExcelHAlign.HAlignCenter);
            int ColEFDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Commitment Date", 13, ExcelHAlign.HAlignCenter);
            int ColCDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO ID", 13, ExcelHAlign.HAlignCenter);
            int ColSoId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO Qty", 13, ExcelHAlign.HAlignCenter);
            int ColSoQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Dispatch Qty", 13, ExcelHAlign.HAlignCenter);
            int ColDisQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Balance To Dispatch", 15, ExcelHAlign.HAlignCenter);
            int ColBal = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Product Code", 13, ExcelHAlign.HAlignCenter);
            int ColProdCode = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "PR NO", 13, ExcelHAlign.HAlignCenter);
            int ColPrNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Lot No", 13, ExcelHAlign.HAlignCenter);
            int ColLotNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Assigned Qty", 13, ExcelHAlign.HAlignCenter);
            int ColAssig = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Un-Assigned Qty", 13, ExcelHAlign.HAlignCenter);
            int ColUnAssig = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "No Of Bag/Ctn", 13, ExcelHAlign.HAlignCenter);
            int ColBg = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Bag/Ctn Net Weight", 13, ExcelHAlign.HAlignCenter);
            int ColNtWt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Contract No", 13, ExcelHAlign.HAlignCenter);
            int ColContract = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 13, ExcelHAlign.HAlignCenter);
            int ColRem = COL;
            COL++;

            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColAssigned].Text = data.Rows[i]["Assigned"].ToString();
                sheet[ROW, ColCustomer].Text = data.Rows[i]["Customer"].ToString();
                sheet[ROW, ColEFDate].Text = data.Rows[i]["ExFactoryDate"].ToString();
                sheet[ROW, ColCDate].Text = data.Rows[i]["CommitmentDate"].ToString();
                sheet[ROW, ColSoId].Text = data.Rows[i]["SoId"].ToString();
                sheet[ROW, ColSoQty].Text = data.Rows[i]["SoQty"].ToString();
                sheet[ROW, ColDisQty].Text = data.Rows[i]["Dispatch"].ToString();
                sheet[ROW, ColBal].Text = data.Rows[i]["ToBeDispatch"].ToString();
                sheet[ROW, ColProdCode].Text = data.Rows[i]["ProductCode"].ToString();
                sheet[ROW, ColPrNo].Text = data.Rows[i]["PO"].ToString();
                sheet[ROW, ColLotNo].Text = data.Rows[i]["LotNo"].ToString();
                sheet[ROW, ColAssig].Text = data.Rows[i]["AssignedQty"].ToString();
                sheet[ROW, ColUnAssig].Text = data.Rows[i]["Available"].ToString();
                sheet[ROW, ColBg].Text = data.Rows[i]["Cartons"].ToString();
                sheet[ROW, ColNtWt].Text = data.Rows[i]["StockQty"].ToString();
                sheet[ROW, ColContract].Text = data.Rows[i]["ContractNo"].ToString();
                sheet[ROW, ColRem].Text = data.Rows[i]["Remarks"].ToString();



                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Stock Report", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }

        [HttpPost, Authorize]
        public ActionResult GetFinishedStocksReport(string Loc, string ToDate, string FromDate)
        {

            try
            {
                var workbook = GetFinishedStocksReportForm(Loc, ToDate, FromDate);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "FinishedStockReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        private IWorkbook GetFinishedStocksReportForm(string Loc, string ToDate, string FromDate)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = det.getGroupFinishedStocksReport(Loc, FromDate, ToDate);

            var data1 = det.getAllFinishedStocksReport(Loc, ToDate, FromDate);

            var sheet = workbook.Worksheets[0];
            var sheet1 = workbook.Worksheets[1];


            #region sheet1
            sheet.Name = "Finished Stock Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            //sheet.Range[ROW, COL].Text = "From - "+FromDate+" , To - "+ToDate;
            //sheet.Range[ROW, COL].ColumnWidth = 13;
            //sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            //sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //ROW += 2;

            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 40, ExcelHAlign.HAlignCenter);
            int ColArt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Lot No", 13, ExcelHAlign.HAlignCenter);
            int ColLot = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Bag Size", 13, ExcelHAlign.HAlignCenter);
            int ColBagSize = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Bags", 13, ExcelHAlign.HAlignCenter);
            int ColBags = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Net Weight", 13, ExcelHAlign.HAlignCenter);
            int ColNtWt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Gross Weight", 13, ExcelHAlign.HAlignCenter);
            int ColGWt = COL;
            COL++;

            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;

            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {
                if (Article != data.Rows[i]["StandardName"].ToString())
                {

                    Article = data.Rows[i]["StandardName"].ToString();
                    sheet[ROW, ColArt].Text = data.Rows[i]["StandardName"].ToString();

                    if (i != 0 && ArtRow != (ROW - 1))
                    {
                        sheet.Range[ArtRow, ColArt, ROW - 1, ColArt].Merge();
                        sheet.Range[ArtRow, ColArt, ROW - 1, ColArt].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    }
                    ArtRow = ROW;
                }

                if (LotNum != data.Rows[i]["LotNo"].ToString())
                {

                    LotNum = data.Rows[i]["LotNo"].ToString();
                    sheet[ROW, ColLot].Text = data.Rows[i]["LotNo"].ToString();
                    if (i != 0 && LotRow != (ROW - 1))
                    {
                        sheet.Range[LotRow, ColLot, ROW - 1, ColLot].Merge();
                        sheet.Range[LotRow, ColLot, ROW - 1, ColLot].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    }
                    LotRow = ROW;
                }


                sheet[ROW, ColBagSize].Number = clsStaticInfo.dbl(data.Rows[i]["BagSize"].ToString());
                sheet[ROW, ColBags].Number = clsStaticInfo.dbl(data.Rows[i]["Bags"].ToString());
                sheet[ROW, ColNtWt].Number = clsStaticInfo.dbl(data.Rows[i]["NtWt"].ToString());
                sheet[ROW, ColGWt].Number = clsStaticInfo.dbl(data.Rows[i]["GtWt"].ToString());

                arr[0] += clsStaticInfo.dbl(data.Rows[i]["Bags"].ToString());
                arr[1] += clsStaticInfo.dbl(data.Rows[i]["NtWt"].ToString());
                arr[2] += clsStaticInfo.dbl(data.Rows[i]["GtWt"].ToString());

                sheet.Range[ROW, ColBagSize, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, ColBagSize, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            ROW++;

            sheet[ROW, ColArt].Text = "TOTAL";
            sheet[ROW, ColBags].Number = arr[0];
            sheet[ROW, ColNtWt].Number = arr[1];
            sheet[ROW, ColGWt].Number = arr[2];

            sheet.Range[ROW, ColArt, ROW, ColBagSize].Merge();
            sheet.Range[ROW, ColArt, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, ColArt, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[ROW, ColArt, ROW, endCol].CellStyle.Font.Bold = true;
            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1


            #region sheet2

            sheet1.Name = "All Stocks";

            int ROW1 = 6;
            int endCol1 = 1;
            int COL1 = 1;

            //sheet.Range[ROW, COL].Text = "From - "+FromDate+" , To - "+ToDate;
            //sheet.Range[ROW, COL].ColumnWidth = 13;
            //sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
            //sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
            //sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //ROW += 2;

            #region Grid Headers

            report.SetHeaderText(ref sheet1, ROW1, COL1, "Article", 40, ExcelHAlign.HAlignCenter);
            int ColArt1 = COL1;
            COL1++;

            report.SetHeaderText(ref sheet1, ROW1, COL1, "Lot No", 13, ExcelHAlign.HAlignCenter);
            int ColLot1 = COL1;
            COL1++;

            report.SetHeaderText(ref sheet1, ROW1, COL1, "Cartons", 13, ExcelHAlign.HAlignCenter);
            int ColCarton = COL1;
            COL1++;

            report.SetHeaderText(ref sheet1, ROW1, COL1, "Net Weight", 13, ExcelHAlign.HAlignCenter);
            int ColNtWt1 = COL1;
            COL1++;

            report.SetHeaderText(ref sheet1, ROW1, COL1, "Gross Weight", 13, ExcelHAlign.HAlignCenter);
            int ColGWt1 = COL1;
            COL1++;

            ROW1++;
            endCol1 = COL1;
            #endregion Headers


            var startRow1 = 0;
            var endRow1 = 0;
            int RowIndex1 = ROW1;
            startRow1 = ROW1;

            //string Article1 = "";
            //string LotNum1 = "";
            //int ArtRow1 = 0;
            //int LotRow1 = 0;

            //double[] arr1 = new double[3];

            for (int i = 0; i < data1.Rows.Count; i++)
            {
                sheet1[ROW1, ColArt1].Text = data1.Rows[i]["StandardName"].ToString();
                sheet1[ROW1, ColLot1].Text = data1.Rows[i]["LotNo"].ToString();
                sheet1[ROW1, ColCarton].Text = data1.Rows[i]["Cartons"].ToString();
                sheet1[ROW1, ColNtWt1].Number = clsStaticInfo.dbl(data1.Rows[i]["NtWt"].ToString());
                sheet1[ROW1, ColGWt1].Number = clsStaticInfo.dbl(data1.Rows[i]["GtWt"].ToString());

                //arr1[0] += clsStaticInfo.dbl(data1.Rows[i]["Bags"].ToString());
                //arr1[1] += clsStaticInfo.dbl(data1.Rows[i]["NtWt"].ToString());
                //arr1[2] += clsStaticInfo.dbl(data1.Rows[i]["GtWt"].ToString());

                sheet1.Range[ROW1, ColArt1, ROW1, endCol1].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[ROW1, ColArt1, ROW1, endCol1].BorderAround(ExcelLineStyle.Hair);

                ROW1++;

            }

            ROW1++;

            //sheet[ROW, ColArt].Text = "TOTAL";
            //sheet[ROW, ColBags].Number = arr[0];
            //sheet[ROW, ColNtWt].Number = arr[1];
            //sheet[ROW, ColGWt].Number = arr[2];

            //sheet.Range[ROW, ColArt, ROW, ColBagSize].Merge();
            //sheet.Range[ROW, ColArt, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            //sheet.Range[ROW, ColArt, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            //sheet.Range[ROW, ColArt, ROW, endCol].CellStyle.Font.Bold = true;
            //ROW++;

            endRow1 = ROW1 - 1;
            endRow1 = ROW1 - 1;
            #endregion sheet2


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            sheet1.UsedRange.WrapText = true;
            sheet1.UsedRange.CellStyle.Font.Size = 8;

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "Finished Stock Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            reportUtility.CompanyHeader(ref sheet1, endCol1, "All Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet1, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
    }
}