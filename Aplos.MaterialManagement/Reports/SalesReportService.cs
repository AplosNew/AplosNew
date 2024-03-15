using Library.Crosscutting;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Addresses;
using Library.Model.Logs;
using Library.Service.Currencies;
using Library.Service.Extension;
using Library.Service.Helpers;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using Library.Core;
using Library.Data.UnitOfWorks;

namespace Library.MaterialManagement.Reports
{
    public class SalesReportService : ISalesReportService
    {
        private readonly IRepositoryAsync<SMTPConfiguration> _smtpConfigurationRepository;
        private readonly ISqlRepository _sqlRepository;//
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IRepositoryAsync<MailLog> _mailLogRepository;
        private readonly IUnitOfWork _unitOfWork;
        public SalesReportService(
            ISqlRepository sqlRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
                , IRepositoryAsync<SMTPConfiguration> smtpConfigurationRepository
             , IRepositoryAsync<MailLog> mailLogRepository
             , IUnitOfWork unitOfWork
            )
        {
            _sqlRepository = sqlRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _smtpConfigurationRepository = smtpConfigurationRepository;
            _mailLogRepository = mailLogRepository;
            _unitOfWork = unitOfWork;
        }

        public IWorkbook GetSalesReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string salesId)
        {
            var excelEngine = new ExcelEngine();
            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var dsLocal = GetSalesMasterData(companyGroupId, companyId, plantId, salesId);
            var dsLocalwriteoff = GetSalesMaterialData(companyGroupId, companyId, plantId, salesId);
            var salesServicedata = GetSalesServiceData(companyGroupId, companyId, plantId, salesId);

            if (dsLocal.Rows.Count == 0)
                throw new Exception("No Data Found!");

            reportFileName = Convert.ToDateTime(dsLocal.Rows[0]["EntryDate"]).ToString("yyMMdd") + " " + dsLocal.Rows[0]["InvoiceNo"];

            reportUtility.SetMasterHeaderText(ref sheet, 8, 1, "Customer");
            reportUtility.SetText(ref sheet, 8, 2, dsLocal.Rows[0]["Customer"].ToString(), ExcelHAlign.HAlignLeft);

            reportUtility.SetMasterHeaderText(ref sheet, 9, 1, "Invoicing By");
            reportUtility.SetText(ref sheet, 9, 2, dsLocal.Rows[0]["InvoicingBy"].ToString(), ExcelHAlign.HAlignLeft);

            reportUtility.SetMasterHeaderText(ref sheet, 8, 3, "Doc Date");
            reportUtility.SetText(ref sheet, 8, 4, dsLocal.Rows[0]["EntryDate"].ToString(), ExcelHAlign.HAlignLeft);

            reportUtility.SetMasterHeaderText(ref sheet, 8, 5, "Currency");
            reportUtility.SetText(ref sheet, 8, 6, dsLocalwriteoff.Rows[0]["Currency"].ToString(), ExcelHAlign.HAlignLeft);

            reportUtility.SetMasterHeaderText(ref sheet, 9, 3, "Entry Date");
            reportUtility.SetText(ref sheet, 9, 4, dsLocal.Rows[0]["EntryDate"].ToString(), ExcelHAlign.HAlignLeft);

            reportUtility.SetMasterHeaderText(ref sheet, 10, 5, "Total Tax(BC) ");
            reportUtility.SetText(ref sheet, 10, 6, Convert.ToDouble(dsLocal.Rows[0]["TotalTax_BC"].ToString()), ExcelHAlign.HAlignLeft);

            reportUtility.SetMasterHeaderText(ref sheet, 9, 5, "Total Amount(BC)");
            reportUtility.SetText(ref sheet, 9, 6, Convert.ToDouble(dsLocal.Rows[0]["TotalAmount_BC"].ToString()), ExcelHAlign.HAlignLeft);

            reportUtility.SetMasterHeaderText(ref sheet, 10, 1, "Vendor Doc.RefNo");
            reportUtility.SetText(ref sheet, 10, 2, dsLocal.Rows[0]["InvoiceNo"].ToString(), ExcelHAlign.HAlignLeft);

            reportUtility.SetMasterHeaderText(ref sheet, 10, 3, "Invoice No");
            reportUtility.SetText(ref sheet, 10, 4, dsLocal.Rows[0]["InvoiceNo"].ToString(), ExcelHAlign.HAlignLeft);

            reportUtility.SetMasterHeaderText(ref sheet, 11, 5, "Gross Total ");
            reportUtility.SetText(ref sheet, 11, 6, Convert.ToDouble(dsLocal.Rows[0]["GrossTotal"].ToString()), ExcelHAlign.HAlignLeft);
            var col = 1;
            reportUtility.SetHeaderText(ref sheet, 14, col, "MaterialGroupMaster", 22); col++;
            reportUtility.SetHeaderText(ref sheet, 14, col, "MaterialMaster", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 14, col, "Article", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 14, col, "Qty", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 14, col, "Rate", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 14, col, "Amount(TRN)", 11); col++;
            reportUtility.SetHeaderText(ref sheet, 14, col, "Tax", 11); col++;

            var summerCol = col - 1;

            var colLast = col;
            var row = 15;
            var startRow = row;
            double _Total_Amount = 0;
            double vAmount = 0;
            for (int n = 0; n < dsLocalwriteoff.Rows.Count; n++)
            {
                col = 1;
                reportUtility.SetText(ref sheet, row, col, dsLocalwriteoff.Rows[n]["MaterialGroupMasterName"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocalwriteoff.Rows[n]["MaterialMasterName"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocalwriteoff.Rows[n]["StandardName"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocalwriteoff.Rows[n]["TransactionQty"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocalwriteoff.Rows[n]["TransactionRate"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocalwriteoff.Rows[n]["TransactionAmount"].ToString())); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocalwriteoff.Rows[n]["TaxAmount"].ToString())); col++;

                //sheet.Range[row +1,1, col, 5].Merge();

                sheet.Range[row, 1, row, col - 1].BorderInside(ExcelLineStyle.Thin);
                sheet.Range[row, 1, row, col - 1].BorderAround(ExcelLineStyle.Thin);

                row++;

            }

            var lastRow = row;
            reportUtility.SetHeaderText(ref sheet, lastRow, 5, "Total :", ExcelHAlign.HAlignRight);
            sheet.Range[row, 1].BorderAround(ExcelLineStyle.Thin);
            sheet.Range[row, 1, row, 4].Merge();
            //reportUtility.SetText(ref sheet, lastRow, 5, "Total :", true);
            for (int i = 0; i < 2; i++)
            {
                summerCol++;
                sheet.Range[lastRow, summerCol - 2].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(summerCol - 2) + startRow + ":" + reportUtility.GetColumnNameForXls(summerCol - 2) + (lastRow - 1) + ")";
                sheet.Range[lastRow, summerCol - 2].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[lastRow, summerCol - 2].CellStyle.Font.Bold = true;
                sheet.Range[lastRow, summerCol - 2].BorderAround(ExcelLineStyle.Hair);
            }

            #region Signature
            if (salesServicedata.Rows.Count > 0)
            {
                row = row + 1;
                col = 1;

                reportUtility.SetHeaderTexte(ref sheet, row, col, "Service Group"); col++;
                reportUtility.SetHeaderTexte(ref sheet, row, col, "Service"); col++;
                reportUtility.SetHeaderTexte(ref sheet, row, col, "Amount(TRN)"); col++;
                reportUtility.SetHeaderTexte(ref sheet, row, col, "Tax"); col++;
                reportUtility.SetHeaderTexte(ref sheet, row, col, ""); col++;
                reportUtility.SetHeaderTexte(ref sheet, row, col, ""); col++;
                reportUtility.SetHeaderTexte(ref sheet, row, col, "");
                col = 0;
                for (int i = 0; i < salesServicedata.Rows.Count; i++)
                {
                    col = 1;
                    row += 1;
                    reportUtility.SetText(ref sheet, row, col, salesServicedata.Rows[i]["ServiceGroup"].ToString()); col++;
                    reportUtility.SetText(ref sheet, row, col, salesServicedata.Rows[i]["Servicevalue"].ToString()); col++;
                    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(salesServicedata.Rows[i]["Amount"].ToString())); col++;
                    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(salesServicedata.Rows[i]["TaxAmount"].ToString())); col++;
                    reportUtility.SetText(ref sheet, row, col, ""); col++;
                    reportUtility.SetText(ref sheet, row, col, ""); col++;
                    reportUtility.SetText(ref sheet, row, col, "");
                    //sheet.Range[row +1,1, col, 5].Merge();

                    sheet.Range[row, 1, row, col].BorderInside(ExcelLineStyle.Thin);
                    sheet.Range[row, 1, row, col].BorderAround(ExcelLineStyle.Thin);



                }
            }
            else
            {
                row = row + 1;
                col = 1;

                reportUtility.SetHeaderTexte(ref sheet, row, col, ""); col++;
                reportUtility.SetHeaderTexte(ref sheet, row, col, ""); col++;
                reportUtility.SetHeaderTexte(ref sheet, row, col, ""); col++;
                reportUtility.SetHeaderTexte(ref sheet, row, col, ""); col++;
                reportUtility.SetHeaderTexte(ref sheet, row, col, ""); col++;
                reportUtility.SetHeaderTexte(ref sheet, row, col, ""); col++;
                reportUtility.SetHeaderTexte(ref sheet, row, col, "");
            }
            //sheet.Range[row, 1, row, col].BorderInside(ExcelLineStyle.Thin);
            //sheet.Range[row, 1, row, col].BorderAround(ExcelLineStyle.Thin);
            // sheet.Range[row-1, 1, row-1, col].BorderAround(ExcelLineStyle.Thin);
            row = row + 4;
            sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

            sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

            sheet.Range[row, 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 7, "Authorized By", true);
            ///SERVICE PART

            #endregion Signature

            sheet.UsedRange.AutofitColumns();
            sheet.UsedRange.CellStyle.Font.Size = 8;
            reportUtility.CompanyPlantHeader(ref sheet, 7, "Sales Report", companyId, plantId, plantName, null);
            reportUtility.PageSetup(ref sheet, 7, ExcelPageOrientation.Portrait);



            return workbook;
        }
        private DataTable GetSalesMasterData(string companyGroupId, string companyId, string plantId, string salesId)
        {
            var cmdText = @"SELECT SUM(ISNULL(SM.TransactionAmount,0) * ISNULL(SA.ToCurrencyRate,0)) TotalAmount_BC,SUM(ISNULL(SM.TaxAmount,0)* ISNULL(SA.ToCurrencyRate,0)) TotalTax_BC
								,NULL TotalCharge_BC,SUM(ISNULL(SM.TransactionAmount,0) * ISNULL(SA.ToCurrencyRate,0))+SUM(ISNULL(SM.TaxAmount,0)* ISNULL(SA.ToCurrencyRate,0)) GrossTotal
								,PT.UserName AS Customer, PP.UserName InvoicingBy, SA.InvoiceNo
								, REPLACE(CONVERT(CHAR(11), SA.InvoiceDate, 106),' ','-') AS InvoiceDate, REPLACE(CONVERT(CHAR(11), SA.EntryDate, 106),' ','-') AS EntryDate,SA.ComercialInvoiceNo,SA.BLNumber,FORMAT(SA.BLDate,'dd-MMM-yyyy')BLDate,SA.EXPFromNo,FORMAT(SA.EXPDate,'dd-MMM-yyyy')EXPDate,SA.ItemDescription
								FROM TRN.SalesMaterial AS SM 
								LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId
								LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
								LEFT JOIN HKP.Party AS PT ON PT.Id=SA.PartyId
								LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=SA.InvoicingPartyPlantId
								WHERE SA.CompanyGroupId='" + companyGroupId + "' AND SA.CompanyId='" + companyId + "' AND SA.PlantId='" + plantId + "' AND SA.Id='" + salesId + @"'
								Group BY PT.UserName, PP.UserName, SA.InvoiceNo, SA.InvoiceDate, SA.EntryDate
                                ,SA.ComercialInvoiceNo,SA.BLNumber,SA.BLDate,SA.EXPFromNo,SA.EXPDate,SA.ItemDescription";
            return _sqlRepository.GetDataTable(cmdText);
        }
        private DataTable GetSalesMaterialData(string companyGroupId, string companyId, string plantId, string salesId)
        {
            var cmdText = @"SELECT SM.Id, SM.SalesId, MGM.UserName AS MaterialGroupMasterName, SM.MaterialMasterId, MM.UserName MaterialMasterName, SM.ArticleId, ART.StandardName
								, SM.TransactionQty, TUoM.UserName AS TransactionUoM, SM.TransactionRate, CU.Code AS Currency, SM.TransactionAmount, SM.TaxAmount, SM.NetAmount 
								FROM TRN.SalesMaterial AS SM 
								LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId
								LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
								LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
								LEFT JOIN MST.MaterialMasterArticle AS ART ON SM.ArticleId=ART.Id
								LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
								JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
								WHERE SA.CompanyGroupId='" + companyGroupId + "' AND SA.CompanyId='" + companyId + "' AND SA.PlantId='" + plantId + "' AND SA.Id='" + salesId + @"'";
            return _sqlRepository.GetDataTable(cmdText);
        }
        private DataTable GetSalesServiceData(string companyGroupId, string companyId, string plantId, string salesId)
        {
            var cmdText = @"SELECT SM.Id, SM.SalesId, MGM.UserName AS ServiceGroup, SM.ServiceMasterId, SEM.UserName Servicevalue
								, CU.Code AS Currency, SM.Amount , SM.TaxAmount, SM.NetAmount 
								FROM TRN.SalesService AS SM 
								LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId
								LEFT JOIN HKP.ServiceMaster AS SEM ON SEM.Id=SM.ServiceMasterId
								LEFT JOIN HKP.ServiceGroup AS MGM ON SEM.ServiceGroupId=MGM.Id
								LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
								WHERE SA.CompanyGroupId='" + companyGroupId + "' AND SA.CompanyId='" + companyId + "' AND SA.PlantId='" + plantId + "' AND SA.Id='" + salesId + @"'";
            return _sqlRepository.GetDataTable(cmdText);
        }

        //#region SalesWordReport

        public void GetSalesWordReportService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "TaxInvoice" + plantId + ".docx";

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

                dsOrderMaster = loadGRNMaterialMaster(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeOrderDetailsTable(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}

                var SalesTotal = makeOrderServiceTable(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {{ServiceItems}}

                document.Replace("{GrandTotal}", (MaterialTotal + SalesTotal).ToString("F2") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal + SalesTotal), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);


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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);


                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }
                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = fileName;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();

            }
            catch (Exception ex)
            {
                throw ex;

            }

            document.Close();
        }




        public double makeLocalTaxInvoiceTaxTable(WordDocument document, DataTable dsOrderMaster, string salesId)
        {
            string replaceString = "{TaxCollectedAtSource}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign1");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = loadLocalTaxInvoiceAdditionalTax(salesId);


            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    //LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxname");
            range.ApplyCharacterFormat(FontBold);
            int colTaxname = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Percentage");
            range.ApplyCharacterFormat(FontBold);
            int colPercentage = COL;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Tax Amount");
                range.ApplyCharacterFormat(FontBold);

            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;


            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                //ROW++;
                //wTable.AddRow();
                WTableRow TROW = wTable.LastRow;


                IParagraphItem p = TROW.Cells[colTaxname].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Taxname"].ToString());
                TROW.Cells[colPercentage].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["Percentage"].ToString()).ToString("#,##0.0000"));

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["BooksCurrencyTaxAmount"].ToString()).ToString("#,##0.00"));
            }

            #region Sub Total


            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(BooksCurrencyTaxAmount)", "").ToString());

            #endregion Total


            //ROW++;

            #region Total Payable
            #endregion Total Payable

            //ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle3 = document.AddParagraphStyle("MyStyle3");
            //Sets the formatting of the style
            myStyle3.CharacterFormat.FontSize = 8f;
            myStyle3.CharacterFormat.TextColor = Color.Black;
            myStyle3.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 35;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = +((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle3");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            int k = document.Replace(replaceString, textBodyPart, false, false);
            return total;
        }

        public double makeLocalTaxInvoiceTaxWithoutSUITable(WordDocument document, DataTable dsOrderMaster, string salesId)
        {
            string replaceString = "{TaxCollectedAtSource}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign1");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = loadLocalTaxInvoiceAdditionalTax(salesId);


            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    //LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxname");
            range.ApplyCharacterFormat(FontBold);
            int colTaxname = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Percentage");
            range.ApplyCharacterFormat(FontBold);
            int colPercentage = COL;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Tax Amount");
                range.ApplyCharacterFormat(FontBold);

            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;


            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                //ROW++;
                //wTable.AddRow();
                WTableRow TROW = wTable.LastRow;


                IParagraphItem p = TROW.Cells[colTaxname].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Taxname"].ToString());
                TROW.Cells[colPercentage].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["Percentage"].ToString()).ToString("#,##0.0000"));

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["BooksCurrencyTaxAmount"].ToString()).ToString("#,##0.00"));
            }

            #region Sub Total


            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(BooksCurrencyTaxAmount)", "").ToString());

            #endregion Total


            //ROW++;

            #region Total Payable
            #endregion Total Payable

            //ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle3 = document.AddParagraphStyle("MyStyle3");
            //Sets the formatting of the style
            myStyle3.CharacterFormat.FontSize = 8f;
            myStyle3.CharacterFormat.TextColor = Color.Black;
            myStyle3.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 35;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = +((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle3");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            int k = document.Replace(replaceString, textBodyPart, false, false);
            return total;
        }

        public DataTable loadLocalTaxInvoiceAdditionalTax(string salesId)
        {
            string strSQL;

            try
            {
                strSQL = @"select TxC.UserName Taxname,SA.Id,SA.TaxCodeId as TaxCode,SA.BooksCurrencyTaxAmount,SA.Percentage
						from TRN.SalesAdditionalTax SA
						left join TRN.Sales as S on S.Id=SA.SalesId
						left join MST.TaxCode as TxC on TxC.id = SA.TaxCodeId
                        where S.Id='" + salesId + "'";

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

        public DataTable loadLocalTaxInvoiceWithoutSUIAdditionalTax(string salesId)
        {
            string strSQL;

            try
            {
                strSQL = @"select TxC.UserName Taxname,SA.Id,SA.TaxCodeId as TaxCode,SA.BooksCurrencyTaxAmount,SA.Percentage
						from TRN.SalesAdditionalTax SA
						left join TRN.Sales as S on S.Id=SA.SalesId
						left join MST.TaxCode as TxC on TxC.id = SA.TaxCodeId
                        where S.Id='" + salesId + "'";

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

        public void GetLotWiseTaxInvoiceServiceReporttoMail(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            //fileName = "LocalTaxInvoice" + plantId + ".docx";
            fileName = "TAXINVOICE" + ".docx";

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

                dsOrderMaster = GetLotWiseSalesReportData(salesId);
                if (dsOrderMaster.Rows.Count > 0)
                {
                    if (string.IsNullOrEmpty(dsOrderMaster.Rows[0]["Email"].ToString()))
                    {
                        throw new Exception("Customer mailId not found.");
                    }
                }
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = GetLotWiseSalesTaxInvoiceService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                var SalesTotal = makeOrderServiceTable(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {{ServiceItems}}
                var dsInventoryReceiveAdditionalTax = loadLocalTaxInvoiceAdditionalTax(salesId);


                var InventoryReceiveAdditionalTax = 0.00;
                if (dsInventoryReceiveAdditionalTax.Rows.Count > 0)

                {
                    InventoryReceiveAdditionalTax = makeLocalTaxInvoiceTaxTable(document, dsInventoryReceiveAdditionalTax, salesId);//Service Details 
                    //document.Replace("{ServiceDetails}", "Service Details", true, true);

                    //{TotalInWords}
                }
                document.Replace("{GrandTotal}", (MaterialTotal + SalesTotal + InventoryReceiveAdditionalTax).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["BaseCurrencyName"].ToString(), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal + SalesTotal + InventoryReceiveAdditionalTax), dsOrderMaster.Rows[0]["BaseCurrencyId"].ToString()), true, true);


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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                document.Replace("{FileCopyName}", "Original Copy", false, false);
                //removing any unused place holder  
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }


                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string FileName = "TaxInvoice-" + salesId + ".pdf";

                MemoryStream ms = new MemoryStream();
                // Save and close the document.
                pdfDocument.Save(ms);
                pdfDocument.Close(true);

                //Reset the memory stream position.  
                ms.Position = 0;

                //Attach the file
                Attachment file = new Attachment(ms, FileName, "application/pdf");

                EmailSender email = null;
                var dom = _smtpConfigurationRepository.Query(a => a.CompanyGroupId == companyGroupId && a.CompanyId == companyId).Select().FirstOrDefault();
                if (dom == null)
                    throw new CustomException("This 'company group' has no web address!");

                email = new EmailSender(dom.Host, dom.Port, dom.MailingUserName, dom.Password, dom.IsSSL);

                var message = email.PrepareMessage(dom.SenderSystemName + "<" + dom.MailingUserName + ">", dsOrderMaster.Rows[0]["Email"].ToString(), null, null, "Tax Invoice Report", "Please Find Attached.");
                message.Attachments.Add(file);
                email.Send(message);


            }
            catch (Exception ex)
            {
                throw ex;
            }

            MailLog log = new MailLog();

            log.AddedBy = UserId;
            log.AddedDate = DateTime.Now;
            log.AddedFromIP = "";
            log.AppVersion = "";
            log.CompanyGroupId = companyGroupId;
            log.ModelState = ModelState.Added;
            log.RecordTime = DateTime.Now;
            log.ServiceName = "TaxInvoiceMailToCustomer";
            log.UserId = UserId;
            log.AttachmentName = fileName;
            log.IsSuccess = false;
            log.SenderName = null;
            log.MailGenerator = "";
            log.Remarks = "" + salesId + " Tax Invoice mail to Customer";
            _mailLogRepository.Insert(log);
            _unitOfWork.SaveChanges();
            document.Close();
        }

        public void GetLotWiseTaxInvoiceService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            //fileName = "LocalTaxInvoice" + plantId + ".docx";
            fileName = "TAXINVOICE" + ".docx";

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

                dsOrderMaster = GetLotWiseSalesReportData(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = GetLotWiseSalesTaxInvoiceService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                var SalesTotal = makePackingSalesServiceTable(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {{ServiceItems}}
                var dsInventoryReceiveAdditionalTax = loadLocalTaxInvoiceAdditionalTax(salesId);


                var InventoryReceiveAdditionalTax = 0.00;
                if (dsInventoryReceiveAdditionalTax.Rows.Count > 0)

                {
                    InventoryReceiveAdditionalTax = makeLocalTaxInvoiceTaxTable(document, dsInventoryReceiveAdditionalTax, salesId);//Service Details 
                    //document.Replace("{ServiceDetails}", "Service Details", true, true);

                    //{TotalInWords}
                }
                document.Replace("{GrandTotal}", (MaterialTotal + SalesTotal + InventoryReceiveAdditionalTax).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["BaseCurrencyName"].ToString(), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal + SalesTotal + InventoryReceiveAdditionalTax), dsOrderMaster.Rows[0]["BaseCurrencyId"].ToString()), true, true);


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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                var sourceDoc = document.Clone();
                document.Replace("{FileCopyName}", "Original Copy", false, false);
                document.ImportContent(sourceDoc, ImportOptions.KeepSourceFormatting);
                document.Replace("{FileCopyName}", "Duplicate Copy", false, false);
                document.ImportContent(sourceDoc, ImportOptions.KeepSourceFormatting);
                document.Replace("{FileCopyName}", "Triplicate for recipient", false, false);


                //removing any unused place holder  
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
                string Prefix = "TaxInvoice-" + salesId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();



                //document.Protect(ProtectionType.AllowOnlyReading, "password");
                //string filename = "TaxInvoice-" + salesId + ".docx";
                //document.Save(filename, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                //document.Close();

            }
            catch (Exception ex)
            {
                //throw ex;
            }

            document.Close();
        }

        public void LocalTaxInvoiceService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "LocalTaxInvoice" + plantId + ".docx";

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

                dsOrderMaster = GetloadLocalTaxMaterialMaster(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeLocalTaxInvoiceService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                var SalesTotal = makeOrderServiceTable(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {{ServiceItems}}
                var dsInventoryReceiveAdditionalTax = loadLocalTaxInvoiceAdditionalTax(salesId);


                var InventoryReceiveAdditionalTax = 0.00;
                if (dsInventoryReceiveAdditionalTax.Rows.Count > 0)

                {
                    InventoryReceiveAdditionalTax = makeLocalTaxInvoiceTaxTable(document, dsInventoryReceiveAdditionalTax, salesId);//Service Details 
                    //document.Replace("{ServiceDetails}", "Service Details", true, true);

                    //{TotalInWords}
                }
                document.Replace("{GrandTotal}", (MaterialTotal + SalesTotal + InventoryReceiveAdditionalTax).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["BaseCurrencyName"].ToString(), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal + SalesTotal + InventoryReceiveAdditionalTax), dsOrderMaster.Rows[0]["BaseCurrencyId"].ToString()), true, true);


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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                var sourceDoc = document.Clone();
                document.Replace("{FileCopyName}", "Original Copy", false, false);
                document.ImportContent(sourceDoc, ImportOptions.KeepSourceFormatting);
                document.Replace("{FileCopyName}", "Duplicate Copy", false, false);
                document.ImportContent(sourceDoc, ImportOptions.KeepSourceFormatting);
                document.Replace("{FileCopyName}", "Triplicate for recipient", false, false);


                //removing any unused place holder  
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
                string Prefix = "LocalTaxInvoice" + plantId;

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

        public void LocalTaxInvoiceWithoutSKUService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "LocalTaxInvoiceWithoutSKU" + plantId + ".docx";

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

                dsOrderMaster = loadLocalTaxWithoutSUIMaterialMaster(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeLocalTaxInvoiceWithoutSKUService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                var SalesTotal = makeOrderWithoutSUIServiceTable(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {{ServiceItems}}
                var dsInventoryReceiveAdditionalTax = loadLocalTaxInvoiceWithoutSUIAdditionalTax(salesId);


                var InventoryReceiveAdditionalTax = 0.00;
                if (dsInventoryReceiveAdditionalTax.Rows.Count > 0)

                {
                    InventoryReceiveAdditionalTax = makeLocalTaxInvoiceTaxWithoutSUITable(document, dsInventoryReceiveAdditionalTax, salesId);//Service Details 
                    //document.Replace("{ServiceDetails}", "Service Details", true, true);

                    //{TotalInWords}
                }
                document.Replace("{GrandTotal}", (MaterialTotal + SalesTotal + InventoryReceiveAdditionalTax).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["BaseCurrencyName"].ToString(), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal + SalesTotal + InventoryReceiveAdditionalTax), dsOrderMaster.Rows[0]["BaseCurrencyId"].ToString()), true, true);


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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                var sourceDoc = document.Clone();
                document.Replace("{FileCopyName}", "Original Copy", false, false);
                document.ImportContent(sourceDoc, ImportOptions.KeepSourceFormatting);
                document.Replace("{FileCopyName}", "Duplicate Copy", false, false);
                document.ImportContent(sourceDoc, ImportOptions.KeepSourceFormatting);
                document.Replace("{FileCopyName}", "Triplicate for recipient", false, false);


                //removing any unused place holder  
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
                string Prefix = "LocalTaxInvoiceWithoutSKU" + plantId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();


            }
            catch (Exception ex)
            {
                throw ex;
            }

            document.Close();
        }

        public void LocalTaxInvoiceWithProductDetailService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "LocalTaxInvoice" + plantId + ".docx";

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

                dsOrderMaster = loadLocalTaxMaterialMasterWithProductDetail(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeLocalTaxInvoiceWithProductDetailService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                var SalesTotal = makeOrderWithoutSUIServiceTable(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {{ServiceItems}}
                var dsInventoryReceiveAdditionalTax = loadLocalTaxInvoiceWithoutSUIAdditionalTax(salesId);


                var InventoryReceiveAdditionalTax = 0.00;
                if (dsInventoryReceiveAdditionalTax.Rows.Count > 0)

                {
                    InventoryReceiveAdditionalTax = makeLocalTaxInvoiceTaxWithoutSUITable(document, dsInventoryReceiveAdditionalTax, salesId);//Service Details 
                    //document.Replace("{ServiceDetails}", "Service Details", true, true);

                    //{TotalInWords}
                }
                document.Replace("{GrandTotal}", (MaterialTotal + SalesTotal + InventoryReceiveAdditionalTax).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["BaseCurrencyName"].ToString(), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal + SalesTotal + InventoryReceiveAdditionalTax), dsOrderMaster.Rows[0]["BaseCurrencyId"].ToString()), true, true);


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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                var sourceDoc = document.Clone();
                document.Replace("{FileCopyName}", "Original Copy", false, false);
                document.ImportContent(sourceDoc, ImportOptions.KeepSourceFormatting);
                document.Replace("{FileCopyName}", "Duplicate Copy", false, false);
                document.ImportContent(sourceDoc, ImportOptions.KeepSourceFormatting);
                document.Replace("{FileCopyName}", "Triplicate for recipient", false, false);


                //removing any unused place holder  
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
                string Prefix = "LocalTaxInvoice" + plantId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();


            }
            catch (Exception)
            {

            }

            document.Close();
        }

        public DataTable TermsAndConditionSQL(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT ROW_NUMBER() OVER(ORDER BY A.UserName) RoWNo,A.* FROM (
Select DISTINCT MA.UserName
from [dbo].MasterLCTermsAndConditions MA
LEFT JOIN dbo.[Contract] C ON C.MasterLcId=MA.MasterLcId
LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
LEFT JOIN HKP.TermsAndConditions TC ON TC.Id=MA.TermsAndConditionsId
                        where SM.SalesId='" + SalesId + @"')A  ";

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

        public DataTable GetAddinfo(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"Select A.* from (
Select DISTINCT MA.Description,MA.Sequence
from [dbo].[MasterLCAddInfo] MA
LEFT JOIN dbo.[Contract] C ON C.MasterLcId=MA.MasterLcId
LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
Where  SM.SalesId='" + SalesId + @"')A ORDER BY A.Sequence";

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

        public double makeTermsAndCondition(string salesId, WordDocument document, DataTable dsTermsAndCondition)
        {
            string replaceString = "{conditions}";


            IWParagraphStyle ConrightAlign = document.AddParagraphStyle("ConrightAlign");
            //Sets the formatting of the style
            ConrightAlign.CharacterFormat.FontSize = 8f;
            ConrightAlign.CharacterFormat.TextColor = Color.Black;
            ConrightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 580;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsTermsAndCondition.Rows[i]["UserName"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            }
            ROW++;

            #region Total

            #endregion Total
            ROW++;
            #region paragrpath formats

            IWParagraphStyle myStyle = document.AddParagraphStyle("TermsAndConditionsStyle");
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

        public double makeCartoons(WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{Cartonss}";


            IWParagraphStyle ConrightAlign = document.AddParagraphStyle("CartoonrightAlign");
            //Sets the formatting of the style
            ConrightAlign.CharacterFormat.FontSize = 8f;
            ConrightAlign.CharacterFormat.TextColor = Color.Black;
            ConrightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 100;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Cartonss"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            }
            ROW++;

            #region Total

            #endregion Total
            ROW++;
            #region paragrpath formats

            IWParagraphStyle myStyle = document.AddParagraphStyle("CartoonStyle");
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

        public double makeserviceInfo(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 550;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;


            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle = document.AddParagraphStyle("AddinfoStyle");
            //Sets the formatting of the style
            myaddStyle.CharacterFormat.FontSize = 8f;
            myaddStyle.CharacterFormat.TextColor = Color.Black;
            myaddStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public double makeaddInfo(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 550;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;


            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle = document.AddParagraphStyle("AddinfoStyle");
            //Sets the formatting of the style
            myaddStyle.CharacterFormat.FontSize = 8f;
            myaddStyle.CharacterFormat.TextColor = Color.Black;
            myaddStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public void CommercialInvoiceService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "CommercialInvoice" + plantId + ".docx";

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

                DataTable dsOrderMaster, dsConditions, dsaddInfo;

                dsOrderMaster = GetloadCommercialLocalTaxMaterialMaster(salesId);
                dsConditions = TermsAndConditionSQL(salesId);
                dsaddInfo = GetAddinfo(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeCommercialInvoiceService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                
                var addInfo = makeaddInfo(salesId, document, dsaddInfo);   // {makeaddInfo}
                var TermsAndCondition = makeTermsAndCondition(salesId, document, dsConditions);   // {conditions}
                var totalQty = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(POTransactionQty)", "CustomerNo='" + dsOrderMaster.Rows[0]["CustomerNo"].ToString() + "'"));
                var FREIGHTVALUE = totalQty * clsStaticInfo.dbl(dsOrderMaster.Rows[0]["AdditionalFrieghtValue"].ToString());
                var FCAVALUE = MaterialTotal - FREIGHTVALUE;
                document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{GrandTotals}", (MaterialTotal).ToString("#,##0.00"), true, true);
                document.Replace("{FREIGHTVALUE}", (FREIGHTVALUE).ToString("#,##0.00"), true, true);
                document.Replace("{FCAVALUE}", (FCAVALUE).ToString("#,##0.00"), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                /*DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "CommercialInvoice" + plantId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);*/
                fileName = "CommercialInvoice-" + salesId + ".docx";
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {


            }

            document.Close();
        }

        public double makeaddInfoWithoutHeading(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 280;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle = document.AddParagraphStyle("AddinfoStyle");
            //Sets the formatting of the style
            myaddStyle.CharacterFormat.FontSize = 8f;
            myaddStyle.CharacterFormat.TextColor = Color.Black;
            myaddStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public void LRDraftService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "LRDraft" + plantId + ".docx";

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

                DataTable dsOrderMaster, dsConditions, dsaddInfo;

                dsOrderMaster = GetloadCommercialLocalTaxMaterialMaster(salesId);
                dsConditions = TermsAndConditionSQL(salesId);
                dsaddInfo = GetAddinfo(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeCommercialInvoiceService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                var addInfo = makeaddInfoWithoutHeading(salesId, document, dsaddInfo);   // {makeaddInfo}
                var TermsAndCondition = makeTermsAndCondition(salesId, document, dsConditions);   // {conditions}
                var Cartoons = makeCartoons(document, dsOrderMaster);   // {conditions}
                var totalQty = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(POTransactionQty)", "CustomerNo='" + dsOrderMaster.Rows[0]["CustomerNo"].ToString() + "'"));
                var FREIGHTVALUE = totalQty * clsStaticInfo.dbl(dsOrderMaster.Rows[0]["AdditionalFrieghtValue"].ToString());
                var FCAVALUE = MaterialTotal - FREIGHTVALUE;
                document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{GrandTotals}", (MaterialTotal).ToString("#,##0.00"), true, true);
                document.Replace("{FREIGHTVALUE}", (FREIGHTVALUE).ToString("#,##0.00"), true, true);
                document.Replace("{FCAVALUE}", (FCAVALUE).ToString("#,##0.00"), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWordNew(clsStaticInfo.dbl(dsOrderMaster.Rows[0]["NoCartons"].ToString()), null), true, true);

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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                /* DocToPDFConverter converter = new DocToPDFConverter();

                 //Converts Word document into PDF document
                 PdfDocument pdfDocument = converter.ConvertToPDF(document);
                 pdfDocument.PageSettings.Width = 1200;
                 pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                 //Releases all resources used by DocToPDFConverter
                 converter.Dispose();

                 //Closes the instance of document objects

                 //Saves the PDF file 
                 string Prefix = "LRDraft" + plantId;

                 pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                 //Closes the instance of document objects
                 pdfDocument.Close(true);*/
                fileName = "BL-LR Draft-" + salesId + ".docx";
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {


            }

            document.Close();
        }

        public void BeneficiaryCertificate(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "BeneficiaryCertificate.docx";

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

                DataTable dsOrderMaster, dsConditions, dsaddInfo, dsaddInfo1, dsaddInfo2, dsaddInfo3, dsaddInfo4, dsaddInfo5, dsaddInfo6, dsaddInfo7, dsaddInfo8, dsaddInfo9 , dsaddInfo10 , dsaddInfo11;

                dsOrderMaster = GetloadCommercialLocalTaxMaterialMasterBC(salesId);
                dsConditions = TermsAndConditionSQL(salesId);
                dsaddInfo = GetAddinfo(salesId);
                dsaddInfo1 = GetAddinfo(salesId);
                dsaddInfo2 = GetAddinfo(salesId);
                dsaddInfo3 = GetAddinfo(salesId);
                dsaddInfo4 = GetAddinfo(salesId);
                dsaddInfo5 = GetAddinfo(salesId);
                dsaddInfo6 = GetAddinfo(salesId);
                dsaddInfo7 = GetAddinfo(salesId);
                dsaddInfo8 = GetAddinfo(salesId);
                dsaddInfo9 = GetAddinfo(salesId);
                dsaddInfo10 = GetAddinfo(salesId);
                dsaddInfo11 = GetAddinfo(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                 var MaterialTotal = makeCommercialInvoiceService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                var addInfo = makeaddInfoBE(salesId, document, dsaddInfo);   // {makeaddInfo}
                var addInfo1 = makeaddInfoBE1(salesId, document, dsaddInfo1);   // {makeaddInfo}
                var addInfo2 = makeaddInfoBE2(salesId, document, dsaddInfo2);   // {makeaddInfo}
                var addInfo3 = makeaddInfoBE3(salesId, document, dsaddInfo3);   // {makeaddInfo}
                var addInfo4 = makeaddInfoBE4(salesId, document, dsaddInfo4);   // {makeaddInfo}
                var addInfo5 = makeaddInfoBE5(salesId, document, dsaddInfo5);   // {makeaddInfo}
                var addInfo6 = makeaddInfoBE6(salesId, document, dsaddInfo6);   // {makeaddInfo}
                var addInfo7 = makeaddInfoBE7(salesId, document, dsaddInfo7);   // {makeaddInfo}
                var addInfo8 = makeaddInfoBE8(salesId, document, dsaddInfo8);   // {makeaddInfo}
                var addInfo9 = makeaddInfoBE9(salesId, document, dsaddInfo9);   // {makeaddInfo}
                var addInfo10 = makeaddInfoBE10(salesId, document, dsaddInfo10);   // {makeaddInfo}
                var addInfo11 = makeaddInfoBE11(salesId, document, dsaddInfo11);   // {makeaddInfo}
                var TermsAndCondition = makeTermsAndCondition(salesId, document, dsConditions);   // {conditions}
                var totalQty = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(POTransactionQty)", "CustomerNo='" + dsOrderMaster.Rows[0]["CustomerNo"].ToString() + "'"));
                var FREIGHTVALUE = totalQty * clsStaticInfo.dbl(dsOrderMaster.Rows[0]["AdditionalFrieghtValue"].ToString());
                var FCAVALUE = MaterialTotal - FREIGHTVALUE;
                document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{GrandTotals}", (MaterialTotal).ToString("#,##0.00"), true, true);
                document.Replace("{FREIGHTVALUE}", (FREIGHTVALUE).ToString("#,##0.00"), true, true);
                document.Replace("{FCAVALUE}", (FCAVALUE).ToString("#,##0.00"), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWordNew(clsStaticInfo.dbl(dsOrderMaster.Rows[0]["NoCartons"].ToString()), null), true, true);

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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                /* DocToPDFConverter converter = new DocToPDFConverter();

                 //Converts Word document into PDF document
                 PdfDocument pdfDocument = converter.ConvertToPDF(document);
                 pdfDocument.PageSettings.Width = 1200;
                 pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                 //Releases all resources used by DocToPDFConverter
                 converter.Dispose();

                 //Closes the instance of document objects

                 //Saves the PDF file 
                 string Prefix = "LRDraft" + plantId;

                 pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                 //Closes the instance of document objects
                 pdfDocument.Close(true);*/
                fileName = "BeneficiaryCertificate-" + salesId + ".docx";
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {


            }

            document.Close();
        }

        public double makeaddInfoBE(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle = document.AddParagraphStyle("AddinfoStyle");
            //Sets the formatting of the style
            myaddStyle.CharacterFormat.FontSize = 8f;
            myaddStyle.CharacterFormat.TextColor = Color.Black;
            myaddStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        #region Add Info 
        public double makeaddInfoBE1(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo1}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign1");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle1 = document.AddParagraphStyle("AddinfoStyle1");
            //Sets the formatting of the style
            myaddStyle1.CharacterFormat.FontSize = 8f;
            myaddStyle1.CharacterFormat.TextColor = Color.Black;
            myaddStyle1.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public double makeaddInfoBE2(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo2}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign2");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle2 = document.AddParagraphStyle("AddinfoStyle2");
            //Sets the formatting of the style
            myaddStyle2.CharacterFormat.FontSize = 8f;
            myaddStyle2.CharacterFormat.TextColor = Color.Black;
            myaddStyle2.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public double makeaddInfoBE3(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo3}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign3");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle3 = document.AddParagraphStyle("AddinfoStyle3");
            //Sets the formatting of the style
            myaddStyle3.CharacterFormat.FontSize = 8f;
            myaddStyle3.CharacterFormat.TextColor = Color.Black;
            myaddStyle3.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public double makeaddInfoBE4(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo4}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign4");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle4 = document.AddParagraphStyle("AddinfoStyle4");
            //Sets the formatting of the style
            myaddStyle4.CharacterFormat.FontSize = 8f;
            myaddStyle4.CharacterFormat.TextColor = Color.Black;
            myaddStyle4.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public double makeaddInfoBE5(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo5}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign5");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle5 = document.AddParagraphStyle("AddinfoStyle5");
            //Sets the formatting of the style
            myaddStyle5.CharacterFormat.FontSize = 8f;
            myaddStyle5.CharacterFormat.TextColor = Color.Black;
            myaddStyle5.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public double makeaddInfoBE6(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo6}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign6");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle6 = document.AddParagraphStyle("AddinfoStyle6");
            //Sets the formatting of the style
            myaddStyle6.CharacterFormat.FontSize = 8f;
            myaddStyle6.CharacterFormat.TextColor = Color.Black;
            myaddStyle6.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public double makeaddInfoBE7(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo7}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign7");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle7 = document.AddParagraphStyle("AddinfoStyle7");
            //Sets the formatting of the style
            myaddStyle7.CharacterFormat.FontSize = 8f;
            myaddStyle7.CharacterFormat.TextColor = Color.Black;
            myaddStyle7.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public double makeaddInfoBE8(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo8}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign8");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle8 = document.AddParagraphStyle("AddinfoStyle8");
            //Sets the formatting of the style
            myaddStyle8.CharacterFormat.FontSize = 8f;
            myaddStyle8.CharacterFormat.TextColor = Color.Black;
            myaddStyle8.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public double makeaddInfoBE9(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo9}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign9");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle9 = document.AddParagraphStyle("AddinfoStyle9");
            //Sets the formatting of the style
            myaddStyle9.CharacterFormat.FontSize = 8f;
            myaddStyle9.CharacterFormat.TextColor = Color.Black;
            myaddStyle9.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public double makeaddInfoBE10(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo10}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign10");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle9 = document.AddParagraphStyle("AddinfoStyle10");
            //Sets the formatting of the style
            myaddStyle9.CharacterFormat.FontSize = 8f;
            myaddStyle9.CharacterFormat.TextColor = Color.Black;
            myaddStyle9.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public double makeaddInfoBE11(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo11}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign11");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 500;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle9 = document.AddParagraphStyle("AddinfoStyle11");
            //Sets the formatting of the style
            myaddStyle9.CharacterFormat.FontSize = 8f;
            myaddStyle9.CharacterFormat.TextColor = Color.Black;
            myaddStyle9.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        #endregion Add Info 

        public void BillofExchange(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "BillofExchange.docx";

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

                DataTable dsOrderMaster, dsConditions, dsaddInfo;

                dsOrderMaster = GetloadCommercialLocalTaxMaterialMaster(salesId);
                dsConditions = TermsAndConditionSQL(salesId);
                dsaddInfo = GetAddinfo(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeCommercialInvoiceService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                var addInfo = makeaddInfoBE(salesId, document, dsaddInfo);    // {makeaddInfo}
                var TermsAndCondition = makeTermsAndCondition(salesId, document, dsConditions);   // {conditions}
                var totalQty = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(POTransactionQty)", "CustomerNo='" + dsOrderMaster.Rows[0]["CustomerNo"].ToString() + "'"));
                var FREIGHTVALUE = totalQty * clsStaticInfo.dbl(dsOrderMaster.Rows[0]["AdditionalFrieghtValue"].ToString());
                var FCAVALUE = MaterialTotal - FREIGHTVALUE;
                document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{GrandTotals}", (MaterialTotal).ToString("#,##0.00"), true, true);
                document.Replace("{FREIGHTVALUE}", (FREIGHTVALUE).ToString("#,##0.00"), true, true);
                document.Replace("{FCAVALUE}", (FCAVALUE).ToString("#,##0.00"), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                /*DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "CommercialInvoice" + plantId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);*/

                fileName = "BillofExchange-" + salesId + ".docx";
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {


            }

            document.Close();
        }

        public double makeaddInfoCTO(string salesId, WordDocument document, DataTable dsaddInfo)
        {
            string replaceString = "{addInfo}";


            IWParagraphStyle arightAlign = document.AddParagraphStyle("addrightAlign");
            //Sets the formatting of the style
            arightAlign.CharacterFormat.FontSize = 8f;
            arightAlign.CharacterFormat.TextColor = Color.Black;
            arightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            DFontSize.FontSize = 6f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
            wTable.Rows[ROW].Cells[COL].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 230;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsaddInfo.Rows.Count; i++)
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
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsaddInfo.Rows[i]["Description"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Left.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Right.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Top.BorderType = BorderStyle.Cleared;
                TROW.Cells[colTermsAndCondition].CellFormat.Borders.Bottom.BorderType = BorderStyle.Cleared;

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

            IWParagraphStyle myaddStyle = document.AddParagraphStyle("AddinfoStyle");
            //Sets the formatting of the style
            myaddStyle.CharacterFormat.FontSize = 8f;
            myaddStyle.CharacterFormat.TextColor = Color.Black;
            myaddStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

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

        public void CertificateofOrigin(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "CertificateofOrigin.docx";

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

                DataTable dsOrderMaster, dsConditions, dsaddInfo;

                dsOrderMaster = GetloadCommercialLocalTaxMaterialMaster(salesId);
                dsConditions = TermsAndConditionSQL(salesId);
                dsaddInfo = GetAddinfo(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeCommercialInvoiceService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                var addInfo = makeaddInfoCTO(salesId, document, dsaddInfo);   // {makeaddInfo}
                var TermsAndCondition = makeTermsAndCondition(salesId, document, dsConditions);   // {conditions}
                var totalQty = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(POTransactionQty)", "CustomerNo='" + dsOrderMaster.Rows[0]["CustomerNo"].ToString() + "'"));
                var FREIGHTVALUE = totalQty * clsStaticInfo.dbl(dsOrderMaster.Rows[0]["AdditionalFrieghtValue"].ToString());
                var FCAVALUE = MaterialTotal - FREIGHTVALUE;
                document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{GrandTotals}", (MaterialTotal).ToString("#,##0.00"), true, true);
                document.Replace("{FREIGHTVALUE}", (FREIGHTVALUE).ToString("#,##0.00"), true, true);
                document.Replace("{FCAVALUE}", (FCAVALUE).ToString("#,##0.00"), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWordNew(clsStaticInfo.dbl(dsOrderMaster.Rows[0]["NoCartons"].ToString()), null), true, true);

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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                /*DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "CertificateofOrigin" + plantId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);*/
                fileName = "CertificateofOrigin-" + salesId + ".docx";
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {


            }

            document.Close();
        }

        public void BankLatter(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId, string BankName)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            if (BankName != "" || BankName != null || BankName != "null")
            {
                if (BankName == "HDFC 59265400002289")
                {
                    fileName = "REQUEST-LETTER-HDFC.docx";
                }
                if (BankName == "ICICI Bank Limited")
                {
                    fileName = "REQUEST-LETTER-ICICI.docx";
                }
                if (BankName == "Standard Chartered Bank (Ludhiana)")
                {
                    fileName = "SCB.docx";
                }
                strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                File = strPath;
                if (!System.IO.File.Exists(strPath))
                {
                    throw new CustomException("File <" + fileName + "> Not Found.");
                }
            }
            else
            {
                throw new CustomException("Bank Is not Selected");
            }



            WordDocument document = new WordDocument(File, FormatType.Docx);

            try
            {
                WSection section = document.Sections[0];

                DataTable dsOrderMaster, dsConditions, dsaddInfo;

                dsOrderMaster = GetloadCommercialLocalTaxMaterialMaster(salesId);
                dsConditions = TermsAndConditionSQL(salesId);
                dsaddInfo = GetAddinfo(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeCommercialInvoiceService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                var addInfo = makeaddInfoCTO(salesId, document, dsaddInfo);   // {makeaddInfo}
                var TermsAndCondition = makeTermsAndCondition(salesId, document, dsConditions);   // {conditions}
                var totalQty = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(POTransactionQty)", "CustomerNo='" + dsOrderMaster.Rows[0]["CustomerNo"].ToString() + "'"));
                var FREIGHTVALUE = totalQty * clsStaticInfo.dbl(dsOrderMaster.Rows[0]["AdditionalFrieghtValue"].ToString());
                var FCAVALUE = MaterialTotal - FREIGHTVALUE;
                document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{GrandTotals}", (MaterialTotal).ToString("#,##0.00"), true, true);
                document.Replace("{FREIGHTVALUE}", (FREIGHTVALUE).ToString("#,##0.00"), true, true);
                document.Replace("{FCAVALUE}", (FCAVALUE).ToString("#,##0.00"), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWordNew(clsStaticInfo.dbl(dsOrderMaster.Rows[0]["NoCartons"].ToString()), null), true, true);

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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                /*DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "CertificateofOrigin" + plantId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);*/
                fileName = "REQUEST-LETTER-" + salesId + ".docx";
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {


            }

            document.Close();
        }

        public void InsuranceCoverLetter(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "INSURANCE COVER LETTER.docx";

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

                DataTable dsOrderMaster, dsConditions, dsaddInfo;

                dsOrderMaster = GetloadCommercialLocalTaxMaterialMaster(salesId);
                dsConditions = TermsAndConditionSQL(salesId);
                dsaddInfo = GetAddinfo(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeCommercialInvoiceService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                var addInfo = makeaddInfoBE(salesId, document, dsaddInfo);   // {makeaddInfo}
                var TermsAndCondition = makeTermsAndCondition(salesId, document, dsConditions);   // {conditions}
                var totalQty = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(POTransactionQty)", "CustomerNo='" + dsOrderMaster.Rows[0]["CustomerNo"].ToString() + "'"));
                var FREIGHTVALUE = totalQty * clsStaticInfo.dbl(dsOrderMaster.Rows[0]["AdditionalFrieghtValue"].ToString());
                var FCAVALUE = MaterialTotal - FREIGHTVALUE;
                document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{GrandTotals}", (MaterialTotal).ToString("#,##0.00"), true, true);
                document.Replace("{FREIGHTVALUE}", (FREIGHTVALUE).ToString("#,##0.00"), true, true);
                document.Replace("{FCAVALUE}", (FCAVALUE).ToString("#,##0.00"), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWordNew(clsStaticInfo.dbl(dsOrderMaster.Rows[0]["NoCartons"].ToString()), null), true, true);

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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                /*DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "CertificateofOrigin" + plantId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);*/
                fileName = "Insurance-" + salesId + ".docx";
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {


            }

            document.Close();
        }

        public void ANNEXUREReport(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "ANNEXURE.docx";

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

                DataTable dsOrderMaster, dsConditions, dsaddInfo;

                dsOrderMaster = GetloadCommercialLocalTaxMaterialMaster(salesId);
                dsConditions = TermsAndConditionSQL(salesId);
                dsaddInfo = GetAddinfo(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeCommercialInvoiceService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                var addInfo = makeaddInfoBE(salesId, document, dsaddInfo);   // {makeaddInfo}
                var TermsAndCondition = makeTermsAndCondition(salesId, document, dsConditions);   // {conditions}
                var totalQty = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(POTransactionQty)", "CustomerNo='" + dsOrderMaster.Rows[0]["CustomerNo"].ToString() + "'"));
                var FREIGHTVALUE = totalQty * clsStaticInfo.dbl(dsOrderMaster.Rows[0]["AdditionalFrieghtValue"].ToString());
                var FCAVALUE = MaterialTotal - FREIGHTVALUE;
                document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{GrandTotals}", (MaterialTotal).ToString("#,##0.00"), true, true);
                document.Replace("{FREIGHTVALUE}", (FREIGHTVALUE).ToString("#,##0.00"), true, true);
                document.Replace("{FCAVALUE}", (FCAVALUE).ToString("#,##0.00"), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWordNew(clsStaticInfo.dbl(dsOrderMaster.Rows[0]["NoCartons"].ToString()), null), true, true);

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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                /*DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "CertificateofOrigin" + plantId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);*/
                fileName = "ANNEXURE-" + salesId + ".docx";
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {


            }

            document.Close();
        }

        public void CommercialInvoicePackingListService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "CommercialInvoicePackingList" + plantId + ".docx";

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

                DataTable dsOrderMaster, dsConditions, dsaddInfo;

                dsOrderMaster = GetloadCommercialLocalTaxMaterialMaster(salesId);
                dsConditions = TermsAndConditionSQL(salesId);
                dsaddInfo = GetAddinfo(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeCommercialInvoicePackingListService(companyGroupId, companyId, plantId, salesId, document, dsOrderMaster);   // {materialItems}
                var addInfo = makeaddInfo(salesId, document, dsaddInfo);   // {makeaddInfo}
                var TermsAndCondition = makeTermsAndCondition(salesId, document, dsConditions);   // {conditions}

                document.Replace("{GrandTotal}", (MaterialTotal).ToString(), true, true);

                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                /*DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "PackingList-" + salesId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);*/
                fileName = "PackingList-" + salesId + ".docx";
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {


            }

            document.Close();
        }

        public DataTable GetloadCommercialLocalTaxMaterialMaster(string SalesId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT IR.Id CustomerNo,CRNC.Code,cmp.BaseCurrencyId,IR.CurrencyId,p.UserName Customer,P.UserName Buyer,P.TINNO CustomerGSTNo,p.VATResistrationNo AS CustomerPANNo
    ,Addres.Address1 VendorAddress ,Addres.Address1 VendorAddressICL,ISNULL(HSNC.Code,MHSN.Code) HSNCode,Plant.GSTIN,Plant.VATResistrationNo AS PlantPANNo,DPARTYPL.GSTIN ShipGSTIN
    ,INVPARTYPL.GSTIN BillGSTIN,IR.DocRefNo,IR.InvoiceNo , IR.InvoiceNo InvoiceNoNew
    ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS DocDate
    ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
	,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDateNew
    ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDateANX
    ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
    ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate   
    ,INVPARTYPL.UserName InvoiceParty,INVPARTYPL.UserName InvoiceParty2,IR.InvoicingByAddress AS ConsigneeAddress,IR.DeliveryByAddress,DPARTYPL.UserName DeliveryParty 
    ,PSI.PreCarriageBy,PSI.PlaceOfReceiptByPreCarriage,PSI.CNFContainerNo,PSI.CNFVesselName,PSI.CNFVesselTrackingNo,CRNC.Code AS CurrencyName,IR.ToCurrencyRate
    ,BASECRNC.Code AS BaseCurrencyName,PayTerm.UserName PaymentTerm,MM.UserName MaterialMaster,MGM.UserName MaterialGroupMaster
    ,Article=CASE WHEN ISNULL(moi.LCArticle,'')<>'' THEN moi.LCArticle WHEN ISNULL(AA.ArticlePartyName,'')<>'' THEN AA.ArticlePartyName ELSE MMA.StandardName END
    ,ArticleWithBuyer=CASE WHEN ISNULL(moi.LCArticle,'')<>'' THEN moi.LCArticle WHEN ISNULL(AA.ArticlePartyName,'')<>'' THEN AA.ArticlePartyName ELSE MMA.StandardName END + '  ' + moi.BuyerReferenceNo
     ,POTransactionQty=CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCN.NetWeight,0)=0 THEN IRD.TransactionQty ELSE SCN.NetWeight END) 
    ,POTransactionQtyGWT=CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCN.GWeight,0)=0 THEN IRD.TransactionQty ELSE SCN.GWeight END)
    ,CONVERT(NUMERIC(10,4),IRD.TransactionRate, 4) TransactionRate
	,TrnAmount=CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCN.NetWeight,0)=0 THEN ROUND((IRD.TransactionQty * IRD.TransactionRate), 2) ELSE ROUND((SCN.NetWeight * IRD.TransactionRate), 2) END)
	--,SumTrnAmount =  sum(ROUND((SCN.NetWeight * IRD.TransactionRate), 2))
	,BaseAmount=CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCN.NetWeight,0)=0 THEN IRD.BaseAmount ELSE ROUND((SCN.NetWeight * IRD.TransactionRate), 2) END)
   ,BooksCurrencyTransactionAmount=CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCN.NetWeight,0)=0 THEN IRD.BooksCurrencyTransactionAmount ELSE ROUND((SCN.NetWeight * CONVERT(NUMERIC(10,4),IRD.BooksCurrencyBaseRate)), 4) END)	
    ,CONVERT(NUMERIC(10,2),IRD.BooksCurrencyTaxAmount)BooksCurrencyTaxAmount
    ,CONVERT(NUMERIC(10,4),IRD.BooksCurrencyBaseRate)BooksCurrencyBaseRate
    ,TUoM.UserName AS TransactionUoM
    ,PONumber = REPLACE(REPLACE(STUFF((
                    SELECT DISTINCT ', ' + CPO.PONumber
                    FROM TRN.SalesMaterial SM
                    JOIN TRN.SalesOrder SO ON SO.Id = SM.SalesOrderId
                    JOIN TRN.CustomerPO CPO ON CPO.id = SO.CustomerPOId
                    WHERE IR.Id = SM.SalesId
                    FOR XML path('')
                        ,TYPE
                    ).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
    
    ,IR.ComercialInvoiceNo,IR.BLNumber,FORMAT(IR.BLDate, 'dd-MMM-yyyy') BLDate,IR.EXPFromNo,FORMAT(IR.EXPDate, 'dd-MMM-yyyy') EXPDate,IR.ItemDescription
    ,PSI.TransportVehicleNo,PSI.TransportDriverName,PSI.TransportDriverNo,PPSI.UserName TransporterName,BM.AccountTitle,BM.AccountNumber,BMA.Address1
    ,PSI.TransportDocRefNo,FORMAT(PSI.TransportDocDate, 'dd-MMM-yyyy') CNFBLAWBDate,B.UserName AS Bank,BB.UserName AS BankBranch
	,(SELECT Stuff((
                    SELECT distinct',' + pla.AttributeValue
                    FROM dbo.ProductLibraryAttribute pla
                    WHERE pla.ProductLibraryId = MOI.ProductLibraryId
                    FOR XML PATH('')
                    ), 1, 1, '')
        ) AS ProdDetails,SCN.Bags Cartons, SCN.LotNo, CONVERT(NUMERIC(10,2),SCN.GWeight)GWeight, MO.Type,MO.MasterOrderNo,SO.Id SalesOrderNo,MO.BuyerReferenceNo,BB.IFSCCode
,LcNo=Stuff((
                    SELECT distinct',' + LC.LCRef
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LcAmount= (
                    SELECT CONVERT(NUMERIC(10,2) , LC.Amount)
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id)
,LcAmountBL = (
                    SELECT CONVERT(NUMERIC(10,2) , LC.Amount)
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id)
,LcNoNew=Stuff((
                    SELECT distinct',' + LC.LCRef
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LcAdvisingBankDS = Stuff((
                    SELECT distinct',' + LC.LeinDescription
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,BenificiaryACCNO = Stuff((
                    SELECT distinct',' + OB.AccountNumber
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.BankMaster OB on OB.Id = LC.BenificiaryBankId
					LEFT JOIN HKP.Bank B on B.Id = OB.BankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,BenificiaryBank=Stuff((
                    SELECT distinct',' + B.UserName
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.BankMaster OB on OB.Id = LC.BenificiaryBankId
					LEFT JOIN HKP.Bank B on B.Id = OB.BankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,OpeningBank=Stuff((
                    SELECT distinct',' + NB.UserName
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,OPC=Stuff((
                    SELECT distinct',' + DelCN.UserName
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
					LEFT JOIN SCS.Country DelCN ON DelCN.Id=NB.CountryId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LcOpeningBank=Stuff((
                    SELECT distinct',' + NB.UserName
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LcOpeningBankBE=Stuff((
                    SELECT distinct',' + NB.UserName
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LcOpeningBankfirst=Stuff((
                    SELECT distinct',' + NB.BankAddress
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LcOpeningBanksecond=Stuff((
                    SELECT distinct',' + NB.BankAddress
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LCDate=Stuff((
                    SELECT distinct',' + FORMAT(LC.LCDate, 'dd-MMM-yyyy')
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LCShipingdate=Stuff((
                    SELECT distinct',' + FORMAT(LC.LCShipmentDate, 'dd-MMM-yyyy')
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LCExpirydate=Stuff((
                    SELECT distinct',' + FORMAT(LC.ExpiryDate, 'dd-MMM-yyyy')
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,BenificiaryBankDescription=Stuff((
                    SELECT distinct',' + OA.Address1
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.BankMaster OB on OB.Id = LC.BenificiaryBankId
					LEFT JOIN HKP.Bank B on B.Id = OB.BankId
					LEFT JOIN MST.AddressMaster OA on OA.Id = B.AddressMasterId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,OpeningBankAddress=Stuff((
                    SELECT distinct',' + LC.OpeningDescription
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,ContractNo=Stuff((
                    SELECT distinct',' + C.Id
                    FROM  dbo.[Contract] C 
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')

,ContractDate=Stuff((
                    SELECT distinct',' + FORMAT(C.AddedDate,'dd-MMM-yyyy')
                    FROM  dbo.[Contract] C 
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,ShipmentMode=Stuff((
                    SELECT distinct',' + SHM.UserName
                    FROM  dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.ShipMode SHM ON SHM.Id=LC.ShipmentModeId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,PortOfLoading=Stuff((
                    SELECT distinct',' + PL.UserName
                    FROM  dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.[Port] AS PL ON PL.Id = LC.PortOfLoadingId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,PLCountry=Stuff((
                    SELECT distinct',' + CC.UserName
                    FROM  dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.[Port] AS PL ON PL.Id = LC.PortOfLoadingId
					left join  scs.country CC on CC.Id = PL.CountryId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,PortOfDischarge=Stuff((
                    SELECT distinct',' + PL.UserName
                    FROM  dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.[Port] AS PL ON PL.Id = LC.PortOfLandingId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,PDcountry=Stuff((
                    SELECT distinct',' + CC.UserName
                    FROM  dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.[Port] AS PL ON PL.Id = LC.PortOfLandingId
					left join  scs.country CC on CC.Id = PL.CountryId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')

,PortOfDelivary=Stuff((
                    SELECT distinct',' + PL.UserName
                    FROM  dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.[Port] AS PL ON PL.Id = LC.PortOfLandingId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,PDLCountry=Stuff((
                    SELECT distinct',' + CC.UserName
                    FROM  dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.[Port] AS PL ON PL.Id = LC.PortOfLandingId
					left join  scs.country CC on CC.Id = PL.CountryId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,Countryoffinal=Stuff((
                    SELECT distinct',' + PL.UserName
                    FROM  dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.[Port] AS PL ON PL.Id = LC.FinalDestinationId
					LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,NetWeights =convert(NUMERIC(10,2), (select sum(NetWeight) from Itemscanchild 
				where SalesId = IR.Id
				))
,NoCartons =convert(NUMERIC(10,0), (select COUNT(RefNo) from Itemscanchild 
				where SalesId = IR.Id
				))
,NoCartonss =convert(NUMERIC(10,0), (select COUNT(RefNo) from Itemscanchild 
				where SalesId = IR.Id
				))
,GrossWeights=convert(NUMERIC(10,2), (select sum(GWeight) from Itemscanchild 
				where SalesId = IR.Id
				))
,PSI.PreCarriageDocRef LRCopy 
,FORMAT(PSI.PreCarriageDocDate,'dd-MMM-yyyy') LRDate

,DelCN.UserName INVOICEDILEVERYPLANTCOUNTRY,IR.AdditionalFrieghtValue
,DelCN.UserName INVOICEDILEVERYPLANTCOUNTRYNew
,DelCN.UserName INVOICEDILEVERYPLANTCOUNTRYBE

,SumTrnAmount = (select sum(CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCNS.NetWeight,0)=0 THEN ROUND((IRDS.TransactionQty * IRDS.TransactionRate), 2)
					ELSE ROUND((SCNS.NetWeight * IRDS.TransactionRate), 2) END))
					from trn.SalesMaterial IRDS
					LEFT JOIN (SELECT distinct LotNo, SalesId,SalesMaterialId,  COUNT(RefNo) Bags, 
								SUM(NetWeight)NetWeight,SUM(GWeight)GWeight FROM ItemScanChild 
								group by SalesId ,SalesMaterialId, LotNo) SCNS on  SCNS.SalesMaterialId=IRDS.Id
								WHERE IRDS.SalesId = IR.Id)
,SumTrnAmountBL = (select sum(CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCNS.NetWeight,0)=0 THEN ROUND((IRDS.TransactionQty * IRDS.TransactionRate), 2)
					ELSE ROUND((SCNS.NetWeight * IRDS.TransactionRate), 2) END))
					from trn.SalesMaterial IRDS
					LEFT JOIN (SELECT distinct LotNo, SalesId,SalesMaterialId,  COUNT(RefNo) Bags, 
								SUM(NetWeight)NetWeight,SUM(GWeight)GWeight FROM ItemScanChild 
								group by SalesId ,SalesMaterialId, LotNo) SCNS on  SCNS.SalesMaterialId=IRDS.Id
								WHERE IRDS.SalesId = IR.Id)
,SumTrnAmountNew = (select sum(CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCNS.NetWeight,0)=0 THEN ROUND((IRDS.TransactionQty * IRDS.TransactionRate), 2)
					ELSE ROUND((SCNS.NetWeight * IRDS.TransactionRate), 2) END))
					from trn.SalesMaterial IRDS
					LEFT JOIN (SELECT distinct LotNo, SalesId,SalesMaterialId,  COUNT(RefNo) Bags, 
								SUM(NetWeight)NetWeight,SUM(GWeight)GWeight FROM ItemScanChild 
								group by SalesId ,SalesMaterialId, LotNo) SCNS on  SCNS.SalesMaterialId=IRDS.Id
								WHERE IRDS.SalesId = IR.Id)
,CurrentDate = format(GETDATE(),'dd-MM-yyyy')
,LCDateNew=Stuff((
                    SELECT distinct',' + FORMAT(LC.LCDate, 'yyMMdd')
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LCDateBE=Stuff((
                    SELECT distinct',' + FORMAT(LC.LCDate, 'yyMMdd')
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,case when isnull(PTD.NoOfDay,0) > 0 then  (convert(varchar(10), PTD.NoOfDay) + ' ' + 'DAYS AFTER THE DATE OF LORRY RECEIPT ') 
 else ' SIGHT' end PaymentTermDays
,SwiftCode =Stuff((
                    SELECT distinct',' + NB.SWIFTCode
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,Clause1 =Stuff((
                    SELECT distinct',' + LLc.Clause1
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
					left join LCClauses LLc on LLc.MasterLCId = LC.id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
, LotNOs =Replace(STUFF((SELECT distinct ',' + LotNo   FROM ItemScanChild 
			where SalesId = IR.InvoiceNo  FOR XML PATH('') ) , 1,1 , ''),',' , Char(13) + Char(10))
--,Cartonss =Replace(STUFF((SELECT distinct ',' + convert(varchar(50), COUNT(RefNo))
--             FROM ItemScanChild 
--			 where SalesId = Ir.Id
--			group by SalesId ,SalesMaterialId, LotNo
--			  FOR XML PATH('') ) , 1,1 , ''),',' , Char(13) + Char(10))
,SCN.Bags Cartonss
,Articless=Replace(STUFF((select distinct ',' + CASE WHEN ISNULL(moi.LCArticle,'')<>'' THEN moi.LCArticle 
WHEN ISNULL(AA.ArticlePartyName,'')<>'' THEN AA.ArticlePartyName ELSE MMA.StandardName END 
from TRN.Sales IRs
LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IRs.Id
LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
left join [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IRD.ArticleId
LEFT JOIN dbo.ArticleAlias AA ON AA.ArticleId=MMA.Id AND AA.MasterOrderItemID=MOI.Id
WHERE IRs.Id = IR.Id
			  FOR XML PATH('') ) , 1,1 , ''),',' , Char(13) + Char(10))

,PSI.RFIDSealNo 
,PSI.LineSealNo
,NEGADD.Address1 NegotiationBankAdd
,NEGBB.SWIFTCode NegotiatingBankSwiftCode
,PSI.ShippingBillNo
,PSI.PortCode
,FORMAT(PSI.ShippingBillDate,'dd-MMM-yyyy')ShippingBillDate
,FORMAT(PSI.ShipmentDate,'dd-MMM-yyyy')ShipmentDate
,FORMAT(PSI.ShipmentDate,'dd-MMM-yyyy')ShipmentDate1
,CONVERT(numeric(10,2) , SAI.Value) AdvanceRevceive
,PSI.ExportRefNo EPN
,NEGBNKMT.AccountNumber NegotiatingAccNo
,NEGBNKMT.AccountTitle NegotiatingBankName
,FORMAT(PSI.ShipmentDate + 2,'dd-MM-yyyy')ShipmentDateNew
,DelCN.UserName DeliveryCountry
, case when IR.InvoicingPartyPlantId = IR.DeliveryPartyPlantId then '' else  DPARTYPL.UserName end DelivertyPT
,convert(numeric(10,2), PSI.CargoGrossWt ) ContainerGWeight
,(convert(numeric(10,2), PSI.CargoGrossWt ) - convert(numeric(10,2), PSI.CargoNetWt)) ContainerTWeight

,LCInsuranceCompany=Stuff((
                    SELECT distinct',' + LC.InsuranceCompany
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LCInsuranceCompany1=Stuff((
                    SELECT distinct',' + LC.InsuranceCompany
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LCInsuranceCompanyDescription=Stuff((
                    SELECT distinct',' + LC.InsuranceCompanyDescription
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LCInsuranceCoverNote=Stuff((
                    SELECT distinct',' + LC.InsuranceCoverNote
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')

FROM TRN.Sales IR
LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
LEFT JOIN dbo.PostSalesInvoice PSI ON PSI.SalesId = IR.Id
LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = cmp.BaseCurrencyId
LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
left join mst.PaymentTermDetail PTD on PTD.PaymentTermId = PayTerm.Id and PTD.Sequence = 3
LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
LEFT JOIN HKP.Party P ON P.Id = IR.PartyId
LEFT JOIN [MST].[AddressMaster] Addres ON Addres.Id = P.AddressMasterId
LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
LEFT JOIN [MST].[AddressMaster] DelAddres ON DelAddres.Id = DPARTYPL.AddressMasterId
LEFT JOIN SCS.Country DelCN ON DelCN.Id=DelAddres.CountryId
left join [TRN].[MasterOrder] MO on MO.Id = MOI.MasterOrderId
left join ProductLibrary PLA on PLA.Id = MOI.ProductLibraryId
LEFT JOIN (SELECT distinct LotNo, SalesId,SalesMaterialId,  COUNT(RefNo) Bags, 
            SUM(NetWeight)NetWeight,SUM(GWeight)GWeight FROM ItemScanChild 
			group by SalesId ,SalesMaterialId, LotNo) SCN on SCN.SalesId = IR.Id AND SCN.SalesMaterialId=IRD.Id

LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IRD.MaterialMasterId
LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IRD.ArticleId
LEFT JOIN dbo.ArticleAlias AA ON AA.ArticleId=MMA.Id AND AA.MasterOrderItemID=MOI.Id
LEFT JOIN [HKP].[HSNCode] AS MHSN ON MHSN.ID = MM.HSNCodeId
LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID = MMA.HSNCodeId

LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
LEFT JOIN HKP.Party PPSI ON PPSI.Id = PSI.TransportAgentId
LEFT JOIN MST.BankMaster BM ON BM.Id = IR.PaymentToReceiveBankId
LEFT JOIN HKP.Bank B ON B.Id = BM.BankId
LEFT JOIN HKP.BankBranch BB ON BB.BankId = BM.BankId AND BB.Id = BM.BankBranchId
LEFT JOIN [MST].[AddressMaster] BMA ON BMA.Id = BB.AddressMasterId
left join mst.BankMaster NEGBNKMT on NEGBNKMT.Id = PSI.BankMasterId
left join hkp.Bank NEGBNK on NEGBNK.Id = NEGBNKMT.BankId
left join hkp.BankBranch NEGBB on NEGBB.Id = NEGBNKMT.BankBranchId
left join MST.AddressMaster NEGADD on NEGADD.Id = NEGBB.AddressMasterId
left join SalesAdditionalInfo SAI on SAI.SalesId = IR.Id 
left join HKP.AdditionalInfo AI on AI.Id  = SAI.AdditionalInfoId and AI.UserName = 'Advance'

                       WHERE IR.Id ='" + SalesId + "' AND SCN.Bags<>''";

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

        public DataTable GetloadCommercialLocalTaxMaterialMasterBC(string SalesId)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT IR.Id CustomerNo,CRNC.Code,cmp.BaseCurrencyId,IR.CurrencyId,p.UserName Customer,P.UserName Buyer,P.TINNO CustomerGSTNo,p.VATResistrationNo AS CustomerPANNo
    ,Addres.Address1 VendorAddress,ISNULL(HSNC.Code,MHSN.Code) HSNCode,Plant.GSTIN,Plant.VATResistrationNo AS PlantPANNo,DPARTYPL.GSTIN ShipGSTIN
    ,INVPARTYPL.GSTIN BillGSTIN,IR.DocRefNo,IR.InvoiceNo , IR.InvoiceNo InvoiceNoNew
    ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS DocDate
    ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
	,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDateNew
    ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
    ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate   
    ,INVPARTYPL.UserName InvoiceParty,INVPARTYPL.UserName InvoiceParty2,IR.InvoicingByAddress AS ConsigneeAddress,IR.DeliveryByAddress,DPARTYPL.UserName DeliveryParty 
    ,PSI.PreCarriageBy,PSI.PlaceOfReceiptByPreCarriage,PSI.CNFContainerNo,PSI.CNFVesselName,PSI.CNFVesselTrackingNo,CRNC.Code AS CurrencyName,IR.ToCurrencyRate
    ,BASECRNC.Code AS BaseCurrencyName,PayTerm.UserName PaymentTerm,MM.UserName MaterialMaster,MGM.UserName MaterialGroupMaster
    ,Article=CASE WHEN ISNULL(moi.LCArticle,'')<>'' THEN moi.LCArticle WHEN ISNULL(AA.ArticlePartyName,'')<>'' THEN AA.ArticlePartyName ELSE MMA.StandardName END
    ,ArticleWithBuyer=CASE WHEN ISNULL(moi.LCArticle,'')<>'' THEN moi.LCArticle WHEN ISNULL(AA.ArticlePartyName,'')<>'' THEN AA.ArticlePartyName ELSE MMA.StandardName END + '  ' + moi.BuyerReferenceNo
     ,POTransactionQty=CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCN.NetWeight,0)=0 THEN IRD.TransactionQty ELSE SCN.NetWeight END) 
    ,CONVERT(NUMERIC(10,4),IRD.TransactionRate, 4) TransactionRate
	,TrnAmount=CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCN.NetWeight,0)=0 THEN ROUND((IRD.TransactionQty * IRD.TransactionRate), 2) ELSE ROUND((SCN.NetWeight * IRD.TransactionRate), 2) END)
	--,SumTrnAmount =  sum(ROUND((SCN.NetWeight * IRD.TransactionRate), 2))
	,BaseAmount=CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCN.NetWeight,0)=0 THEN IRD.BaseAmount ELSE ROUND((SCN.NetWeight * IRD.TransactionRate), 2) END)
   ,BooksCurrencyTransactionAmount=CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCN.NetWeight,0)=0 THEN IRD.BooksCurrencyTransactionAmount ELSE ROUND((SCN.NetWeight * CONVERT(NUMERIC(10,4),IRD.BooksCurrencyBaseRate)), 4) END)	
    ,CONVERT(NUMERIC(10,2),IRD.BooksCurrencyTaxAmount)BooksCurrencyTaxAmount
    ,CONVERT(NUMERIC(10,4),IRD.BooksCurrencyBaseRate)BooksCurrencyBaseRate
    ,TUoM.UserName AS TransactionUoM
      ,PSI.ExportRefNo EPN
    ,PONumber = REPLACE(REPLACE(STUFF((
                    SELECT DISTINCT ', ' + CPO.PONumber
                    FROM TRN.SalesMaterial SM
                    JOIN TRN.SalesOrder SO ON SO.Id = SM.SalesOrderId
                    JOIN TRN.CustomerPO CPO ON CPO.id = SO.CustomerPOId
                    WHERE IR.Id = SM.SalesId
                    FOR XML path('')
                        ,TYPE
                    ).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
    
    ,IR.ComercialInvoiceNo,IR.BLNumber,FORMAT(IR.BLDate, 'dd-MMM-yyyy') BLDate,IR.EXPFromNo,FORMAT(IR.EXPDate, 'dd-MMM-yyyy') EXPDate,IR.ItemDescription
    ,PSI.TransportVehicleNo,PSI.TransportDriverName,PSI.TransportDriverNo,PPSI.UserName TransporterName,BM.AccountTitle,BM.AccountNumber,BMA.Address1
    ,PSI.TransportDocRefNo,FORMAT(PSI.TransportDocDate, 'dd-MMM-yyyy') CNFBLAWBDate,B.UserName AS Bank,BB.UserName AS BankBranch
	,(SELECT Stuff((
                    SELECT distinct',' + pla.AttributeValue
                    FROM dbo.ProductLibraryAttribute pla
                    WHERE pla.ProductLibraryId = MOI.ProductLibraryId
                    FOR XML PATH('')
                    ), 1, 1, '')
        ) AS ProdDetails,SCN.Bags Cartons, SCN.LotNo, CONVERT(NUMERIC(10,2),SCN.GWeight)GWeight, MO.Type,MO.MasterOrderNo,SO.Id SalesOrderNo,MO.BuyerReferenceNo,BB.IFSCCode
,LcNo=Stuff((
                    SELECT distinct',' + LC.LCRef
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LcNoNN=Stuff((
                    SELECT distinct',' + LC.LCRef
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LcAmount= (
                    SELECT sum(CONVERT(NUMERIC(10,2) , LC.Amount))
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id)
,LcAmountBL = (
                    SELECT sum(CONVERT(NUMERIC(10,2) , LC.Amount))
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id)
,LcNoNew=Stuff((
                    SELECT distinct',' + LC.LCRef
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,BenificiaryBank=Stuff((
                    SELECT distinct',' + B.UserName
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.BankMaster OB on OB.Id = LC.BenificiaryBankId
					LEFT JOIN HKP.Bank B on B.Id = OB.BankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,OpeningBank=Stuff((
                    SELECT distinct',' + NB.UserName
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,OPC=Stuff((
                    SELECT distinct',' + DelCN.UserName
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
					LEFT JOIN SCS.Country DelCN ON DelCN.Id=NB.CountryId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LcOpeningBank=Stuff((
                    SELECT distinct',' + NB.UserName
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LcOpeningBankBE=Stuff((
                    SELECT distinct',' + NB.UserName
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LcOpeningBankfirst=Stuff((
                    SELECT distinct',' + NB.BankAddress
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LcOpeningBanksecond=Stuff((
                    SELECT distinct',' + NB.BankAddress
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LCDate=Stuff((
                    SELECT distinct',' + FORMAT(LC.LCDate, 'dd-MMM-yyyy')
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LCShipingdate=Stuff((
                    SELECT distinct',' + FORMAT(LC.LCShipmentDate, 'dd-MMM-yyyy')
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LCExpirydate=Stuff((
                    SELECT distinct',' + FORMAT(LC.ExpiryDate, 'dd-MMM-yyyy')
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,BenificiaryBankDescription=Stuff((
                    SELECT distinct',' + OA.Address1
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.BankMaster OB on OB.Id = LC.BenificiaryBankId
					LEFT JOIN HKP.Bank B on B.Id = OB.BankId
					LEFT JOIN MST.AddressMaster OA on OA.Id = B.AddressMasterId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,OpeningBankAddress=Stuff((
                    SELECT distinct',' + LC.OpeningDescription
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,ContractNo=Stuff((
                    SELECT distinct',' + C.Id
                    FROM  dbo.[Contract] C 
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')

,ContractDate=Stuff((
                    SELECT distinct',' + FORMAT(C.AddedDate,'dd-MMM-yyyy')
                    FROM  dbo.[Contract] C 
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,ShipmentMode=Stuff((
                    SELECT distinct',' + SHM.UserName
                    FROM  dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.ShipMode SHM ON SHM.Id=LC.ShipmentModeId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,PortOfLoading=Stuff((
                    SELECT distinct',' + PL.UserName
                    FROM  dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.[Port] AS PL ON PL.Id = LC.PortOfLoadingId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,PortOfDischarge=Stuff((
                    SELECT distinct',' + PL.UserName
                    FROM  dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.[Port] AS PL ON PL.Id = LC.PortOfLandingId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,PortOfDelivary=Stuff((
                    SELECT distinct',' + PL.UserName
                    FROM  dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.[Port] AS PL ON PL.Id = LC.PortOfLandingId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,Countryoffinal=Stuff((
                    SELECT distinct',' + PL.UserName
                    FROM  dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN MST.[Port] AS PL ON PL.Id = LC.FinalDestinationId
					LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,NetWeights =convert(NUMERIC(10,2), (select sum(NetWeight) from Itemscanchild 
				where SalesId = IR.Id
				))
,NetWeightsNN =convert(NUMERIC(10,2), (select sum(NetWeight) from Itemscanchild 
				where SalesId = IR.Id
				))
,NoCartons =convert(NUMERIC(10,0), (select COUNT(RefNo) from Itemscanchild 
				where SalesId = IR.Id
				))
,NoCartonss =convert(NUMERIC(10,0), (select COUNT(RefNo) from Itemscanchild 
				where SalesId = IR.Id
				))
,GrossWeights=convert(NUMERIC(10,2), (select sum(GWeight) from Itemscanchild 
				where SalesId = IR.Id
				))
,PSI.PreCarriageDocRef LRCopy 
,FORMAT(PSI.PreCarriageDocDate,'dd-MMM-yyyy') LRDate

,DelCN.UserName INVOICEDILEVERYPLANTCOUNTRY,IR.AdditionalFrieghtValue
,DelCN.UserName INVOICEDILEVERYPLANTCOUNTRYNew
,DelCN.UserName INVOICEDILEVERYPLANTCOUNTRYBE

,SumTrnAmount = (select sum(CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCNS.NetWeight,0)=0 THEN ROUND((IRDS.TransactionQty * IRDS.TransactionRate), 2)
					ELSE ROUND((SCNS.NetWeight * IRDS.TransactionRate), 2) END))
					from trn.SalesMaterial IRDS
					LEFT JOIN (SELECT distinct LotNo, SalesId,SalesMaterialId,  COUNT(RefNo) Bags, 
								SUM(NetWeight)NetWeight,SUM(GWeight)GWeight FROM ItemScanChild 
								group by SalesId ,SalesMaterialId, LotNo) SCNS on  SCNS.SalesMaterialId=IRDS.Id
								WHERE IRDS.SalesId = IR.Id)
,SumTrnAmountNew = (select sum(CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCNS.NetWeight,0)=0 THEN ROUND((IRDS.TransactionQty * IRDS.TransactionRate), 2)
					ELSE ROUND((SCNS.NetWeight * IRDS.TransactionRate), 2) END))
					from trn.SalesMaterial IRDS
					LEFT JOIN (SELECT distinct LotNo, SalesId,SalesMaterialId,  COUNT(RefNo) Bags, 
								SUM(NetWeight)NetWeight,SUM(GWeight)GWeight FROM ItemScanChild 
								group by SalesId ,SalesMaterialId, LotNo) SCNS on  SCNS.SalesMaterialId=IRDS.Id
								WHERE IRDS.SalesId = IR.Id)
,CurrentDate = format(PSI.ShipmentDate,'dd-MM-yyyy')
,LCDateNew=Stuff((
                    SELECT distinct',' + FORMAT(LC.LCDate, 'yyMMdd')
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LCDateNewNN=Stuff((
                    SELECT distinct',' + FORMAT(LC.LCDate, 'yyMMdd')
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,LCDateBE=Stuff((
                    SELECT distinct',' + FORMAT(LC.LCDate, 'yyMMdd')
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,PTD.NoOfDay PaymentTermDays
,SwiftCode =Stuff((
                    SELECT distinct',' + NB.SWIFTCode
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,Clause1 =Stuff((
                    SELECT distinct',' + LLc.Clause1
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
					left join LCClauses LLc on LLc.MasterLCId = LC.id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,PSI.RFIDSealNo 
,PSI.LineSealNo
,NEGADD.Address1 NegotiationBankAdd
,NEGBB.SWIFTCode NegotiatingBankSwiftCode
,PSI.ShippingBillNo
,PSI.PortCode
,FORMAT(PSI.ShippingBillDate,'dd-MMM-yyyy')ShippingBillDate
,FORMAT(PSI.ShipmentDate,'dd-MMM-yyyy')ShipmentDate
,CONVERT(numeric(10,2) , SAI.Value) AdvanceRevceive
,Clause2 =Stuff((
                    SELECT distinct',' + LLc.Clause2
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
					left join LCClauses LLc on LLc.MasterLCId = LC.id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,Clause3 =Stuff((
                    SELECT distinct',' + LLc.Clause3
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
					left join LCClauses LLc on LLc.MasterLCId = LC.id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,Clause4 =Stuff((
                    SELECT distinct',' + LLc.Clause4
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
					left join LCClauses LLc on LLc.MasterLCId = LC.id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,Clause5 =Stuff((
                    SELECT distinct',' + LLc.Clause5
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
					left join LCClauses LLc on LLc.MasterLCId = LC.id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,Clause6 =Stuff((
                    SELECT distinct',' + LLc.Clause6
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
					left join LCClauses LLc on LLc.MasterLCId = LC.id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,Clause7 =Stuff((
                    SELECT distinct',' + LLc.Clause7
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
					left join LCClauses LLc on LLc.MasterLCId = LC.id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,Clause8 =Stuff((
                    SELECT distinct',' + LLc.Clause8
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
					left join LCClauses LLc on LLc.MasterLCId = LC.id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,Clause9 =Stuff((
                    SELECT distinct',' + LLc.Clause9
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
					left join LCClauses LLc on LLc.MasterLCId = LC.id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,Clause10 =Stuff((
                    SELECT distinct',' + LLc.Clause10
                    FROM dbo.MasterLC LC 
					LEFT JOIN dbo.[Contract] C ON C.MasterLCId=LC.Id
					LEFT JOIN [dbo].NegotiatingBank NB ON NB.Id=LC.OpeningBankId
                    LEFT JOIN TRN.SalesOrder SO ON SO.ContractId=C.Id
					LEFT JOIN TRN.SalesMaterial SM ON SM.SalesOrderId=SO.Id
					left join LCClauses LLc on LLc.MasterLCId = LC.id
                    WHERE SM.SalesId=IR.Id
                    FOR XML PATH('')
                    ), 1, 1, '')
,CurrentDate1 = format(PSI.ShipmentDate,'dd-MM-yyyy')
,CurrentDate2 = format(PSI.ShipmentDate,'dd-MM-yyyy')
,CurrentDate3 = format(PSI.ShipmentDate,'dd-MM-yyyy')
,CurrentDate4 = format(PSI.ShipmentDate,'dd-MM-yyyy')
,CurrentDate5 = format(PSI.ShipmentDate,'dd-MM-yyyy')
,CurrentDate6 = format(PSI.ShipmentDate,'dd-MM-yyyy')
,CurrentDate7 = format(PSI.ShipmentDate,'dd-MM-yyyy')
,CurrentDate8 = format(PSI.ShipmentDate,'dd-MM-yyyy')
,CurrentDate9 = format(PSI.ShipmentDate,'dd-MM-yyyy')
,CurrentDate10 = format(PSI.ShipmentDate,'dd-MM-yyyy')
,CurrentDate11 = format(PSI.ShipmentDate,'dd-MM-yyyy')
,p.UserName Customer1
,Addres.Address1 VendorAddress1
,p.UserName Customer2
,Addres.Address1 VendorAddress2
,p.UserName Customer3
,Addres.Address1 VendorAddress3
,p.UserName Customer4
,Addres.Address1 VendorAddress4
,p.UserName Customer5
,Addres.Address1 VendorAddress5
,p.UserName Customer6
,Addres.Address1 VendorAddress6
,p.UserName Customer7
,Addres.Address1 VendorAddress7
,p.UserName Customer8
,Addres.Address1 VendorAddress8
,p.UserName Customer9
,Addres.Address1 VendorAddress9
,p.UserName Customer10
,Addres.Address1 VendorAddress10

,IR.InvoiceNo  InvoiceNo1
,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate1
,FORMAT(PSI.ShipmentDate,'dd-MMM-yyyy')ShipmentDate1
,IR.InvoiceNo InvoiceNo2
,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate2
,FORMAT(PSI.ShipmentDate,'dd-MMM-yyyy')ShipmentDate2
,IR.InvoiceNo InvoiceNo3
,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate3
,FORMAT(PSI.ShipmentDate,'dd-MMM-yyyy')ShipmentDate3
,IR.InvoiceNo InvoiceNo4
,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate4
,FORMAT(PSI.ShipmentDate,'dd-MMM-yyyy')ShipmentDate4
,IR.InvoiceNo InvoiceNo5
,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate5
,FORMAT(PSI.ShipmentDate,'dd-MMM-yyyy')ShipmentDate5
,IR.InvoiceNo InvoiceNo6
,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate6
,FORMAT(PSI.ShipmentDate,'dd-MMM-yyyy')ShipmentDate6
,IR.InvoiceNo InvoiceNo7
,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate7
,FORMAT(PSI.ShipmentDate,'dd-MMM-yyyy')ShipmentDate7
,IR.InvoiceNo InvoiceNo8
,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate8
,FORMAT(PSI.ShipmentDate,'dd-MMM-yyyy')ShipmentDate8
,IR.InvoiceNo InvoiceNo9
,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate9
,FORMAT(PSI.ShipmentDate,'dd-MMM-yyyy')ShipmentDate9
,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate10
,IR.InvoiceNo InvoiceNo10
,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate11
,IR.InvoiceNo InvoiceNo11
,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate12
,IR.InvoiceNo InvoiceNo12

,PSI.PreCarriageDocRef LRCopy1 
,FORMAT(PSI.PreCarriageDocDate,'dd-MMM-yyyy') LRDate1
,PSI.PreCarriageDocRef LRCopy2 
,FORMAT(PSI.PreCarriageDocDate,'dd-MMM-yyyy') LRDate2
,PSI.PreCarriageDocRef LRCopy3 
,FORMAT(PSI.PreCarriageDocDate,'dd-MMM-yyyy') LRDate3
,PSI.PreCarriageDocRef LRCopy4 
,FORMAT(PSI.PreCarriageDocDate,'dd-MMM-yyyy') LRDate4
,PSI.PreCarriageDocRef LRCopy5 
,FORMAT(PSI.PreCarriageDocDate,'dd-MMM-yyyy') LRDate5
,PSI.PreCarriageDocRef LRCopy6 
,FORMAT(PSI.PreCarriageDocDate,'dd-MMM-yyyy') LRDate6
,PSI.PreCarriageDocRef LRCopy7 
,FORMAT(PSI.PreCarriageDocDate,'dd-MMM-yyyy') LRDate7
,PSI.PreCarriageDocRef LRCopy8 
,FORMAT(PSI.PreCarriageDocDate,'dd-MMM-yyyy') LRDate8
,PSI.PreCarriageDocRef LRCopy9 
,FORMAT(PSI.PreCarriageDocDate,'dd-MMM-yyyy') LRDate9
,PSI.PreCarriageDocRef LRCopy10 
,FORMAT(PSI.PreCarriageDocDate,'dd-MMM-yyyy') LRDate10
,PSI.PreCarriageDocRef LRCopy11 
,FORMAT(PSI.PreCarriageDocDate,'dd-MMM-yyyy') LRDate11

FROM TRN.Sales IR
LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
LEFT JOIN dbo.PostSalesInvoice PSI ON PSI.SalesId = IR.Id
LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = cmp.BaseCurrencyId
LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
left join mst.PaymentTermDetail PTD on PTD.PaymentTermId = PayTerm.Id and PTD.Sequence = 3
LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
LEFT JOIN HKP.Party P ON P.Id = IR.PartyId
LEFT JOIN [MST].[AddressMaster] Addres ON Addres.Id = P.AddressMasterId
LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
LEFT JOIN [MST].[AddressMaster] DelAddres ON DelAddres.Id = DPARTYPL.AddressMasterId
LEFT JOIN SCS.Country DelCN ON DelCN.Id=DelAddres.CountryId
left join [TRN].[MasterOrder] MO on MO.Id = MOI.MasterOrderId
left join ProductLibrary PLA on PLA.Id = MOI.ProductLibraryId
LEFT JOIN (SELECT distinct LotNo, SalesId,SalesMaterialId,  COUNT(RefNo) Bags, 
            SUM(NetWeight)NetWeight,SUM(GWeight)GWeight FROM ItemScanChild 
			group by SalesId ,SalesMaterialId, LotNo) SCN on SCN.SalesId = IR.Id AND SCN.SalesMaterialId=IRD.Id

LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IRD.MaterialMasterId
LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IRD.ArticleId
LEFT JOIN dbo.ArticleAlias AA ON AA.ArticleId=MMA.Id AND AA.MasterOrderItemID=MOI.Id
LEFT JOIN [HKP].[HSNCode] AS MHSN ON MHSN.ID = MM.HSNCodeId
LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID = MMA.HSNCodeId

LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
LEFT JOIN HKP.Party PPSI ON PPSI.Id = PSI.TransportAgentId
LEFT JOIN MST.BankMaster BM ON BM.Id = IR.PaymentToReceiveBankId
LEFT JOIN HKP.Bank B ON B.Id = BM.BankId
LEFT JOIN HKP.BankBranch BB ON BB.BankId = BM.BankId AND BB.Id = BM.BankBranchId
LEFT JOIN [MST].[AddressMaster] BMA ON BMA.Id = BB.AddressMasterId
left join mst.BankMaster NEGBNKMT on NEGBNKMT.Id = PSI.BankMasterId
left join hkp.Bank NEGBNK on NEGBNK.Id = NEGBNKMT.BankId
left join hkp.BankBranch NEGBB on NEGBB.Id = NEGBNKMT.BankBranchId
left join MST.AddressMaster NEGADD on NEGADD.Id = NEGBB.AddressMasterId
left join SalesAdditionalInfo SAI on SAI.SalesId = IR.Id 
left join HKP.AdditionalInfo AI on AI.Id  = SAI.AdditionalInfoId and AI.UserName = 'Advance'
                       WHERE IR.Id ='" + SalesId + "' AND SCN.Bags<>''";

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

        #region Sales Invoice Report By Aakash007
        public void SalesInvoiceService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "SalesInvoice" + plantId + ".docx";

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
                DataTable dsSalesInvoiceHeader;
                dsSalesInvoiceHeader = loadSalesInvoiceHeader(salesId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsSalesInvoiceHeader.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeSalesInvoiceService(companyGroupId, companyId, plantId, salesId, document, dsSalesInvoiceHeader);   // {materialItems}

                document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsSalesInvoiceHeader.Rows[0]["CurrencyName"].ToString(), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal), dsSalesInvoiceHeader.Rows[0]["CurrencyId"].ToString()), true, true);


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
                        document.Replace(text, dsSalesInvoiceHeader.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                    if (text == "{PRINTEDBY}")
                    {
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

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
                string Prefix = "SalesInvoice" + plantId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {

            }

            document.Close();
        }
        #endregion

        #region SalesWordReport
        public string SaveFileName { get; private set; }


        public double makeOrderDetailsTable(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{materialItems}";

            DataTable sales, materialTax;
            //Sales== Master Query
            sales = loadGRNMaterialMaster(salesId);
            materialTax = loadOrderMasterTax(salesId);

            int LasColumnIndex = 9;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(materialTax.DefaultView.ToTable(true, "TaxCode"));

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 100;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar2].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar3].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 100;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UoM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL++;
            wTable.Rows[ROW].Cells[colUoM].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL;
            wTable.Rows[ROW].Cells[colRate].Width = 50;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 100;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
            }


            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }

            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);

                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);
                }
                ROW++;
            }
            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
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
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colHSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("F2"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString());
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("F2"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("F2"));


                //totalValue += clsStdLib.dbl(sales.Rows[i]["TrnAmount"].ToString());

                if (dv.Count > 0)
                {
                    DataView dvtax = new DataView(materialTax.DefaultView.ToTable());

                    for (int T = 0; T < dv.Count; T++)
                    {
                        //dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "'";
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND SalesMaterialId='" + materialTax.Rows[i]["SalesMaterialId"].ToString() + "'";

                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));
                        }
                    }
                }
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
                if (C == colArticle || C == colHSN || C == colUoM || C == colRate || C == colQty || C == colChar1 || C == colChar2 || C == colChar3 || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2")).ApplyCharacterFormat(FontBold);
            }
            #endregion Total

            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString())
                    //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                    + clsStdLib.dbl(materialTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 30;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 30 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


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

            return total;
        }

        public double GetLotWiseSalesTaxInvoiceService(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{materialItems}";
            string taxreplaceString = "{salesTax}";

            DataTable sales, materialTax;
            //Sales== Master Query
            //sales = GetLotWiseSalesReportData(salesId);
            materialTax = GetSalesTax(salesId);

            int LasColumnIndex = 8;
            int TaxLasColumnIndex = 3;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();


            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Product Description");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 165;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Shade");
            range.ApplyCharacterFormat(FontBold);
            int colShade = COL; COL++;
            wTable.Rows[ROW].Cells[colShade].Width = 55;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Lot");
            range.ApplyCharacterFormat(FontBold);
            int colLot = COL; COL++;
            wTable.Rows[ROW].Cells[colLot].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Bag");
            range.ApplyCharacterFormat(FontBold);
            int colBag = COL; COL++;
            wTable.Rows[ROW].Cells[colBag].Width = 40;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Gross");
            range.ApplyCharacterFormat(FontBold);
            int colGross = COL; COL++;
            wTable.Rows[ROW].Cells[colGross].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Net");
            range.ApplyCharacterFormat(FontBold);
            int colNet = COL; COL++;
            wTable.Rows[ROW].Cells[colNet].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate/Kg");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL; COL++;
            wTable.Rows[ROW].Cells[colRate].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Amount(INR)");
            range.ApplyCharacterFormat(FontBold);
            int colTotalTaxableAmount = COL;
            wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 95;


            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
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
                IWTextRange textRangeArt = TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                textRangeArt.CharacterFormat.FontSize = 8;

                IWTextRange textRangeSh = TROW.Cells[colShade].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Shade"].ToString());
                textRangeSh.CharacterFormat.FontSize = 8;

                IWTextRange textRangeLotNo = TROW.Cells[colLot].AddParagraph().AppendText(dsOrderMaster.Rows[i]["LotNo"].ToString());
                textRangeLotNo.CharacterFormat.FontSize = 8;

                IWTextRange textRangeHSN = TROW.Cells[colHSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                textRangeHSN.CharacterFormat.FontSize = 8;

                IWTextRange textRangeBags = TROW.Cells[colBag].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Bags"].ToString());
                textRangeBags.CharacterFormat.FontSize = 8;

                IWTextRange textRangeGWeight = TROW.Cells[colGross].AddParagraph().AppendText(dsOrderMaster.Rows[i]["GWeight"].ToString());
                textRangeGWeight.CharacterFormat.FontSize = 8;

                IWTextRange textRangeNet = TROW.Cells[colNet].AddParagraph().AppendText(dsOrderMaster.Rows[i]["POTransactionQty"].ToString());
                textRangeNet.CharacterFormat.FontSize = 8;

                IWTextRange textRangeRate = TROW.Cells[colRate].AddParagraph().AppendText(dsOrderMaster.Rows[i]["BooksCurrencyBaseRate"].ToString());
                textRangeRate.CharacterFormat.FontSize = 8;

                IWTextRange textRangeTrnAmount = TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(dsOrderMaster.Rows[i]["BooksCurrencyTransactionAmount"].ToString());
                textRangeTrnAmount.CharacterFormat.FontSize = 8;

            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            //wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            // _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            range.ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colArticle || C == colShade || C == colLot || C == colHSN || C == colRate || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                //_TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
                if (C == 4)
                {
                    document.Replace("{TotalBag}", (value).ToString("#,##0.00"), true, true);
                }
                if (C == 5)
                {
                    document.Replace("{TotalGross}", (value).ToString("#,##0.00"), true, true);
                }
                if (C == 6)
                {
                    document.Replace("{TotalNet}", (value).ToString("#,##0.00"), true, true);
                }
                if (C == 8)
                {
                    document.Replace("{TotalAmount}", (value).ToString("#,##0.00"), true, true);
                }
            }
            #endregion Total

            ROW++;
            #region Sub Total

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString())
                    //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                    + clsStdLib.dbl(materialTax.Compute("SUM(TaxAmount)", "").ToString());


            #endregion Total

            ROW++;
            #region Total Payable

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;


            #region SalesTax

            WTable wTaxTable = new WTable(document);
            int TXROW = 0; int TXCOL = 0;
            wTaxTable.ResetCells(1, TaxLasColumnIndex + 1);

            IWTextRange trange = wTaxTable.Rows[TXROW].Cells[TXCOL].AddParagraph().AppendText("Charges/Tax & Rate");
            trange.ApplyCharacterFormat(FontBold);
            int colTaxCode = TXCOL; TXCOL++;
            wTaxTable.Rows[TXROW].Cells[colArticle].Width = 100;


            trange = wTaxTable.Rows[ROW].Cells[TXCOL].AddParagraph().AppendText("Per.(%)");
            trange.ApplyCharacterFormat(FontBold);
            int colPercentage = TXCOL; TXCOL++;
            wTaxTable.Rows[TXROW].Cells[colPercentage].Width = 50;

            trange = wTaxTable.Rows[ROW].Cells[TXCOL].AddParagraph().AppendText("TaxON");
            trange.ApplyCharacterFormat(FontBold);
            int colTaxON = TXCOL; TXCOL++;
            wTaxTable.Rows[TXROW].Cells[colTaxON].Width = 100;

            trange = wTaxTable.Rows[ROW].Cells[TXCOL].AddParagraph().AppendText("TaxAmount");
            trange.ApplyCharacterFormat(FontBold);
            int colTaxAmount = TXCOL;
            wTaxTable.Rows[TXROW].Cells[colTaxAmount].Width = 100;

            for (int i = 0; i < materialTax.Rows.Count; i++)
            {
                TXROW++;
                wTaxTable.AddRow();
                WTableRow TAXROW = wTaxTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TAXROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TAXROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TAXROW.Cells[CE].Width = wTaxTable.Rows[0].Cells[CE].Width;

                }
                IWTextRange textRange = TAXROW.Cells[colTaxCode].AddParagraph().AppendText(materialTax.Rows[i]["TaxCode"].ToString());
                IWTextRange textRangeP = TAXROW.Cells[colPercentage].AddParagraph().AppendText(materialTax.Rows[i]["Percentage"].ToString());
                IWTextRange textRangeT = TAXROW.Cells[colTaxON].AddParagraph().AppendText(materialTax.Rows[i]["TaxON"].ToString());
                IWTextRange textRangeTA = TAXROW.Cells[colTaxAmount].AddParagraph().AppendText(materialTax.Rows[i]["TaxAmount"].ToString());
                textRange.CharacterFormat.FontSize = 8;
                textRangeP.CharacterFormat.FontSize = 8;
                textRangeT.CharacterFormat.FontSize = 8;
                textRangeTA.CharacterFormat.FontSize = 8;

            }
            //   trange.CharacterFormat.FontSize = 8;

            #endregion


            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.CharacterFormat.FontSize = 8f;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            TextBodyPart textBodyPart1 = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            textBodyPart1.BodyItems.Add(wTaxTable);
            document.Replace(taxreplaceString, textBodyPart1, true, true);

            return total;
        }

        public double makeLocalTaxInvoiceService(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{materialItems}";

            DataTable sales, materialTax;
            //Sales== Master Query
            sales = GetloadLocalTaxMaterialMaster(salesId);
            materialTax = loadOrderMasterTax(salesId);

            int LasColumnIndex = 9;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(materialTax.DefaultView.ToTable(true, "TaxCode"));


            for (int i = 0; i < dv.Count; i++)
            {
                LasColumnIndex++;
                dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                LasColumnIndex++;
            }


            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 110;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 110;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("BuyerRef#");
            range.ApplyCharacterFormat(FontBold);
            int colBuyerRef = COL; COL++;
            wTable.Rows[ROW].Cells[colBuyerRef].Width = 80;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("PONumber");
            range.ApplyCharacterFormat(FontBold);
            int colPONumber = COL; COL++;
            wTable.Rows[ROW].Cells[colPONumber].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 45;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 50;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UoM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL++;
            wTable.Rows[ROW].Cells[colUoM].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL;
            wTable.Rows[ROW].Cells[colRate].Width = 50;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount " + "(" + " " + sales.Rows[0]["BaseCurrencyName"].ToString() + " " + ")" + " ");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 100;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);

                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
            }


            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }



            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
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
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colBuyerRef].AddParagraph().AppendText(dsOrderMaster.Rows[i]["YourOrderRefNo"].ToString());
                TROW.Cells[colPONumber].AddParagraph().AppendText(dsOrderMaster.Rows[i]["PONumber"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString() + "-" + dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString() + "-" + dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());

                //TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                //TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                //TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colHSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString());
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["BooksCurrencyBaseRate"].ToString()).ToString("#,##0.0000"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["BooksCurrencyTransactionAmount"].ToString()).ToString("#,##0.00"));


                //totalValue += clsStdLib.dbl(sales.Rows[i]["TrnAmount"].ToString());

                if (dv.Count > 0)
                {
                    DataView dvtax = new DataView(materialTax.DefaultView.ToTable());

                    for (int T = 0; T < dv.Count; T++)
                    {
                        //dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "'";
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' And SalesMaterialId = '" + dsOrderMaster.Rows[i]["SalesMaterialId"].ToString() + "' ";

                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("#,##0.00"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["BooksCurrencyTransactionAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
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
                if (C == colArticle || C == colBuyerRef || C == colPONumber || C == colHSN || C == colUoM || C == colRate || C == colChar1 || dicTaxes.ContainsValue(C))
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
            #endregion Total

            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString())
                    //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                    + clsStdLib.dbl(materialTax.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 30;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 30 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


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

            return total;
        }

        public double makeLocalTaxInvoiceWithoutSKUService(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{materialItems}";

            DataTable sales, materialTax;
            //Sales== Master Query
            sales = loadLocalTaxWithoutSUIMaterialMaster(salesId);
            materialTax = loadOrderMasterTax(salesId);

            int LasColumnIndex = 7;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(materialTax.DefaultView.ToTable(true, "TaxCode"));


            for (int i = 0; i < dv.Count; i++)
            {
                LasColumnIndex++;
                dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                LasColumnIndex++;
            }


            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            //IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            //range.ApplyCharacterFormat(FontBold);
            //int colMaterialGroup = COL; COL++;
            //wTable.Rows[ROW].Cells[colMaterialGroup].Width = 110;


            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 25;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("BuyerRef#");
            range.ApplyCharacterFormat(FontBold);
            int colBuyerRef = COL; COL++;
            wTable.Rows[ROW].Cells[colBuyerRef].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("PONumber");
            range.ApplyCharacterFormat(FontBold);
            int colPONumber = COL; COL++;
            wTable.Rows[ROW].Cells[colPONumber].Width = 48;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU");
            //range.ApplyCharacterFormat(FontBold);
            //int colChar1 = COL; COL++;
            //wTable.Rows[ROW].Cells[colChar1].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 45;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 40;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UoM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL++;
            wTable.Rows[ROW].Cells[colUoM].Width = 28;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL;
            wTable.Rows[ROW].Cells[colRate].Width = 42;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount " + "(" + " " + sales.Rows[0]["BaseCurrencyName"].ToString() + " " + ")" + " ");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 46;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);

                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
            }


            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }



            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
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
                //TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colBuyerRef].AddParagraph().AppendText(dsOrderMaster.Rows[i]["YourOrderRefNo"].ToString());
                TROW.Cells[colPONumber].AddParagraph().AppendText(dsOrderMaster.Rows[i]["PONumber"].ToString());
                //TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString() + "-" + dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString() + "-" + dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());

                //TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                //TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                //TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colHSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString());
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["BooksCurrencyBaseRate"].ToString()).ToString("#,##0.0000"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["BooksCurrencyTransactionAmount"].ToString()).ToString("#,##0.00"));


                //totalValue += clsStdLib.dbl(sales.Rows[i]["TrnAmount"].ToString());

                if (dv.Count > 0)
                {
                    DataView dvtax = new DataView(materialTax.DefaultView.ToTable());

                    for (int T = 0; T < dv.Count; T++)
                    {
                        //dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "'";
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' And SalesMaterialId = '" + dsOrderMaster.Rows[i]["SalesMaterialId"].ToString() + "' ";

                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("#,##0.00"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["BooksCurrencyTransactionAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
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
                if (C == colArticle || C == colBuyerRef || C == colPONumber || C == colHSN || C == colUoM || C == colRate || /*C == colChar1 ||*/ dicTaxes.ContainsValue(C))
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
            #endregion Total

            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString())
                    //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                    + clsStdLib.dbl(materialTax.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 30;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 30 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


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

            return total;
        }

        public double makeLocalTaxInvoiceWithProductDetailService(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{materialItems}";

            DataTable sales, materialTax;
            //Sales== Master Query
            sales = loadLocalTaxMaterialMasterWithProductDetail(salesId);
            materialTax = loadOrderMasterTax(salesId);

            int LasColumnIndex = 9;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(materialTax.DefaultView.ToTable(true, "TaxCode"));


            for (int i = 0; i < dv.Count; i++)
            {
                LasColumnIndex++;
                dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                LasColumnIndex++;
            }


            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 110;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 110;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("BuyerRef#");
            range.ApplyCharacterFormat(FontBold);
            int colBuyerRef = COL; COL++;
            wTable.Rows[ROW].Cells[colBuyerRef].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("PONumber");
            range.ApplyCharacterFormat(FontBold);
            int colPONumber = COL; COL++;
            wTable.Rows[ROW].Cells[colPONumber].Width = 55;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Product Details");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 55;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            //range.ApplyCharacterFormat(FontBold);
            //int colSKU1 = COL; COL++;
            //wTable.Rows[ROW].Cells[colSKU1].Width = 65;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            //range.ApplyCharacterFormat(FontBold);
            //int colSKU2 = COL; COL++;
            //wTable.Rows[ROW].Cells[colSKU2].Width = 40;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 50;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UoM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL++;
            wTable.Rows[ROW].Cells[colUoM].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL;
            wTable.Rows[ROW].Cells[colRate].Width = 45;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount " + "(" + " " + sales.Rows[0]["BaseCurrencyName"].ToString() + " " + ")" + " ");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 65;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);

                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
            }


            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }



            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
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
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colBuyerRef].AddParagraph().AppendText(dsOrderMaster.Rows[i]["YourOrderRefNo"].ToString());
                TROW.Cells[colPONumber].AddParagraph().AppendText(dsOrderMaster.Rows[i]["PONumber"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ProdDetails"].ToString());

                //TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                //TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                //TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colHSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                //TROW.Cells[colSKU1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                //TROW.Cells[colSKU2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString());
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["BooksCurrencyBaseRate"].ToString()).ToString("#,##0.0000"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["BooksCurrencyTransactionAmount"].ToString()).ToString("#,##0.00"));


                //totalValue += clsStdLib.dbl(sales.Rows[i]["TrnAmount"].ToString());

                if (dv.Count > 0)
                {
                    DataView dvtax = new DataView(materialTax.DefaultView.ToTable());

                    for (int T = 0; T < dv.Count; T++)
                    {
                        //dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "'";
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' And SalesMaterialId = '" + dsOrderMaster.Rows[i]["SalesMaterialId"].ToString() + "' ";

                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("#,##0.00"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["BooksCurrencyTransactionAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
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
                if (C == colArticle || C == colBuyerRef || C == colPONumber || C == colHSN || C == colUoM || C == colRate || C == colChar1 || dicTaxes.ContainsValue(C))
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
            #endregion Total

            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString())
                    //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                    + clsStdLib.dbl(materialTax.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 30;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 30 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


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

            return total;
        }

        public double makeSalesInvoiceService(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsSalesInvoiceHeader)
        {
            string replaceString = "{materialItems}";

            DataTable dsSalesInvoiceMaterialData, dsTax;

            dsSalesInvoiceMaterialData = loadSalesInvoiceMaterial(salesId);
            dsTax = loadSalesTax(salesId);
            DataTable sales, materialTax;

            int LasColumnIndex = 5;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }

            }
            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material Group");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            //wTable.Rows[ROW].Cells[colMaterialGroup].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialMaster = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            //wTable.Rows[ROW].Cells[colArticle].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].CellFormat.VerticalAlignment = VerticalAlignment.Middle;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL; COL++;
            wTable.Rows[ROW].Cells[colRate].CellFormat.VerticalAlignment = VerticalAlignment.Middle;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
            range.ApplyCharacterFormat(FontBold);
            int colAmount = COL; /*COL++;*/
            wTable.Rows[ROW].Cells[colAmount].CellFormat.VerticalAlignment = VerticalAlignment.Middle;

            if (dv.Count > 0)
            {
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    //two columns required for tax
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    range.ApplyCharacterFormat(FontBold);
                    //COL++;
                    //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                    //range.ApplyCharacterFormat(FontBold);

                }
            }
            else
            {
                //COL++;

                //colTotalTaxableAmount = COL;
                //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Value (" + dsSalesInvoiceMaterialData.Rows[0]["CurrencyName"].ToString() + ")");
                //range.ApplyCharacterFormat(FontBold);
            }


            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                    range.ApplyCharacterFormat(FontBold);
                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }


            }
            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsSalesInvoiceMaterialData.Rows.Count; i++)
            {
                ROW++;
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

                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsSalesInvoiceMaterialData.Rows[i]["MaterialGroupMasterName"].ToString());
                TROW.Cells[colMaterialMaster].AddParagraph().AppendText(dsSalesInvoiceMaterialData.Rows[i]["MaterialMasterName"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsSalesInvoiceMaterialData.Rows[i]["StandardName"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(dsSalesInvoiceMaterialData.Rows[i]["TransactionQty"].ToString());
                //TROW.Cells[colRate].AddParagraph().AppendText(dsSalesInvoiceMaterialData.Rows[i]["TransactionRate"].ToString());

                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsSalesInvoiceMaterialData.Rows[i]["TransactionRate"].ToString()).ToString("F4"));
                TROW.Cells[colAmount].AddParagraph().AppendText(clsStdLib.dbl(dsSalesInvoiceMaterialData.Rows[i]["TransactionAmount"].ToString()).ToString("F2"));

                totalValue += clsStdLib.dbl(dsSalesInvoiceMaterialData.Rows[i]["TransactionAmount"].ToString());

                if (dv.Count > 0)
                {
                    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                    //double totalTax = 0;

                    for (int T = 0; T < dv.Count; T++)
                    {
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND SalesMaterialId ='" + dsSalesInvoiceMaterialData.Rows[i]["Id"].ToString() + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colArticle || C == colMaterialGroup || C == colMaterialMaster || C == colQty || C == colRate || C == colQty || dicTaxes.ContainsValue(C))
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
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsSalesInvoiceMaterialData.Compute("SUM(TransactionAmount)", "").ToString())
                + clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable
            //ROW++;
            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                //TROW.Cells[0].Width = 120;
                //if (dv.Count < 3)
                //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }
            #endregion paragrpath formats
            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

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
            return total;
        }

        public double makeCommercialInvoiceService(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{materialItems}";

            DataTable sales, materialTax;

            int LasColumnIndex = 7;

            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            FontBold.FontSize = 8.5f;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 150;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Product Details");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 55;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Lot No");
            range.ApplyCharacterFormat(FontBold);
            int colLot = COL; COL++;
            wTable.Rows[ROW].Cells[colLot].Width = 65;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 55;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Cartons");
            range.ApplyCharacterFormat(FontBold);
            int colCartons = COL; COL++;
            wTable.Rows[ROW].Cells[colCartons].Width = 45;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty(KGS)");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 60;

           /* range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UoM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL; COL++;
            wTable.Rows[ROW].Cells[colUoM].Width = 35;*/

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate(USD)");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL; COL++;
            wTable.Rows[ROW].Cells[colRate].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Amount" + "(" + "" + dsOrderMaster.Rows[0]["CurrencyName"].ToString() + "" + ")" + "");
            range.ApplyCharacterFormat(FontBold);
            int colAmount = COL;
            wTable.Rows[ROW].Cells[colAmount].Width = 75;


            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
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
                //TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ArticleWithBuyer"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ProdDetails"].ToString()).ApplyCharacterFormat(DFontSize);
                //TROW.Cells[colStyle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["BuyertemRef"].ToString());
                TROW.Cells[colLot].AddParagraph().AppendText(dsOrderMaster.Rows[i]["LotNo"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colCartons].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Cartons"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colHSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("#,##0.00")).ApplyCharacterFormat(DFontSize);
               /* TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString()).ApplyCharacterFormat(DFontSize); ; ;*/
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("#,##0.0000")).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00")).ApplyCharacterFormat(DFontSize);

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
                if (C == colArticle || C == colHSN ||  C == colRate ||  C == colChar1 || C == colLot)
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }

                if (C == colCartons)
                {
                    _TROW.Cells[C].AddParagraph().AppendText(value.ToString()).ApplyCharacterFormat(FontBold);
                }
                else
                {
                    _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
                }
            }
            #endregion Total

            ROW++;
            #region Sub Total

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString());

            #endregion Total

            ROW++;
            #region Total Payable

            #endregion Total Payable

            ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;



            #endregion paragrpath formats

            #region merging section

            ROW = 0;

            ROW++;

            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;

            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            string replaceaddInfoString = "{addInfo}";
            int addLasColumnIndex = 0;
            WTable wTableadd = new WTable(document);
            int ROWAdd = 0; int COLAdd = 0;
            wTableadd.ResetCells(1, addLasColumnIndex + 1);

            return total;
        }

        public double makeCommercialInvoicePackingListService(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{materialItems}";

            DataTable sales, materialTax;

            int LasColumnIndex = 6;

            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            FontBold.FontSize = 8.5f;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("ARTICLE");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 200;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("PRODUCT DETAILS");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("LOT NO");
            range.ApplyCharacterFormat(FontBold);
            int colLot = COL; COL++;
            wTable.Rows[ROW].Cells[colLot].Width = 80;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("NET WEIGHT (KGS.)");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("No of Cartons");
            range.ApplyCharacterFormat(FontBold);
            int colCartons = COL; COL++;
            wTable.Rows[ROW].Cells[colCartons].Width = 50;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("GROSS WEIGHT (KGS.)");
            range.ApplyCharacterFormat(FontBold);
            range.ApplyCharacterFormat(DFontSize);
            int colGW = COL;
            wTable.Rows[ROW].Cells[colQty].Width = 60;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
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
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ArticleWithBuyer"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ProdDetails"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colLot].AddParagraph().AppendText(dsOrderMaster.Rows[i]["LotNo"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colCartons].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Cartons"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colHSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("#,##0.00")).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colGW].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQtyGWT"].ToString()).ToString("#,##0.00")).ApplyCharacterFormat(DFontSize);

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
                if (C == colArticle || C == colHSN || C == colChar1 || C == colLot)
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                if (C == colCartons)
                {
                    _TROW.Cells[C].AddParagraph().AppendText(value.ToString()).ApplyCharacterFormat(FontBold);
                }
                else
                {
                    _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
                }

            }
            #endregion Total

            ROW++;
            #region Sub Total

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(Cartons)", "").ToString());

            #endregion Total

            ROW++;
            #region Total Payable

            #endregion Total Payable

            ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;



            #endregion paragrpath formats

            #region merging section

            ROW = 0;

            ROW++;

            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;

            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            string replaceaddInfoString = "{addInfo}";
            int addLasColumnIndex = 0;
            WTable wTableadd = new WTable(document);
            int ROWAdd = 0; int COLAdd = 0;
            wTableadd.ResetCells(1, addLasColumnIndex + 1);

            return total;
        }

        public double makeOrderServiceTable(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{ServiceItems}";

            ReportUtility ru = new ReportUtility();

            DataTable salesService, serviceTax;

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;



            salesService = loadSalesMaster(salesId);
            if (salesService.Rows.Count == 0)
            {
                document.Replace("{ServiceCaption}", "", false, false);
                document.Replace(replaceString, "", false, false);
                return 0;

            }
            document.Replace("{ServiceCaption}", "Service Details", false, false);
            serviceTax = loadGRNServiceMasterTex(salesId);

            int LasColumnIndex = 2;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(serviceTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Service Name");
            range.ApplyCharacterFormat(FontBold);
            int colServiceGroup = COL; COL++;

            wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Amount");
                range.ApplyCharacterFormat(FontBold);
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
                    range.ApplyCharacterFormat(FontBold);


                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }
            }
            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < salesService.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                TROW.Cells[colServiceGroup].AddParagraph().AppendText(salesService.Rows[i]["Service"].ToString());
                //TROW.Cells[colServiceGroup].AddParagraph().AppendText(salesService.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(salesService.Rows[i]["BooksCurrencyTransactionAmount"].ToString()).ToString("#,##0.00"));

                totalValue += clsStdLib.dbl(salesService.Rows[i]["Amount"].ToString());

                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(serviceTax.DefaultView.ToTable());
                    //double totalTax = 0;

                    for (int T = 0; T < dv.Count; T++)
                    {
                        //dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "'";
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND SalesMaterialId='" + serviceTax.Rows[i]["SalesMaterialId"].ToString() + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("#,##0.00"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["BooksCurrencyTaxAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colHSN || dicTaxes.ContainsValue(C))
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
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(salesService.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString())
                   //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                   + clsStdLib.dbl(serviceTax.Compute("SUM(BooksCurrencyTaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("ServiceStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 120;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("ServiceStyle");
                    }
                }
            }

            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);

            IWParagraphStyle style = document.AddParagraphStyle("ServiceSubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("ServiceSubTotalStyle");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return total;
        }

        public double makePackingSalesServiceTable(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{ServiceItems}";

            ReportUtility ru = new ReportUtility();

            DataTable materialTax;

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;



            materialTax = loadPackingSalesServiceTaxData(salesId);
            
            #region SalesTax

            int TaxLasColumnIndex = 5;

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            FontBold.FontSize = 6f;

            WTable wTaxTable = new WTable(document);
            int TXROW = 0; int TXCOL = 0;
            wTaxTable.ResetCells(1, TaxLasColumnIndex + 1);

            IWTextRange trange = wTaxTable.Rows[TXROW].Cells[TXCOL].AddParagraph().AppendText("Service Name");
            trange.ApplyCharacterFormat(FontBold);
            int colService = TXCOL; TXCOL++;
            wTaxTable.Rows[TXROW].Cells[colService].Width = 100;


            trange = wTaxTable.Rows[TXROW].Cells[TXCOL].AddParagraph().AppendText("Tax & Rate");
            trange.ApplyCharacterFormat(FontBold);
            int colTaxCode = TXCOL; TXCOL++;
            wTaxTable.Rows[TXROW].Cells[colTaxCode].Width = 30;

            trange = wTaxTable.Rows[TXROW].Cells[TXCOL].AddParagraph().AppendText("Per.(%)");
            trange.ApplyCharacterFormat(FontBold);
            int colPercentage = TXCOL; TXCOL++;
            wTaxTable.Rows[TXROW].Cells[colPercentage].Width = 30;

            trange = wTaxTable.Rows[TXROW].Cells[TXCOL].AddParagraph().AppendText("TaxON");
            trange.ApplyCharacterFormat(FontBold);
            int colTaxON = TXCOL; TXCOL++;
            wTaxTable.Rows[TXROW].Cells[colTaxON].Width = 50;

            trange = wTaxTable.Rows[TXROW].Cells[TXCOL].AddParagraph().AppendText("TaxAmount");
            trange.ApplyCharacterFormat(FontBold);
            int colTaxAmount = TXCOL; TXCOL++;
            wTaxTable.Rows[TXROW].Cells[colTaxAmount].Width = 50;

            trange = wTaxTable.Rows[TXROW].Cells[TXCOL].AddParagraph().AppendText("TotalAmount");
            trange.ApplyCharacterFormat(FontBold);
            int colTotalAmount = TXCOL;
            wTaxTable.Rows[TXROW].Cells[colTotalAmount].Width = 50;

            for (int i = 0; i < materialTax.Rows.Count; i++)
            {
                TXROW++;
                wTaxTable.AddRow();
                WTableRow TAXROW = wTaxTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TAXROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TAXROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TAXROW.Cells[CE].Width = wTaxTable.Rows[0].Cells[CE].Width;

                }
                IWTextRange textRangeS = TAXROW.Cells[colService].AddParagraph().AppendText(materialTax.Rows[i]["Service"].ToString());
                IWTextRange textRange = TAXROW.Cells[colTaxCode].AddParagraph().AppendText(materialTax.Rows[i]["TaxCode"].ToString());
                IWTextRange textRangeP = TAXROW.Cells[colPercentage].AddParagraph().AppendText(materialTax.Rows[i]["Percentage"].ToString());
                IWTextRange textRangeT = TAXROW.Cells[colTaxON].AddParagraph().AppendText(materialTax.Rows[i]["TaxON"].ToString());
                IWTextRange textRangeTA = TAXROW.Cells[colTaxAmount].AddParagraph().AppendText(materialTax.Rows[i]["TaxAmount"].ToString());
                IWTextRange textRangeTOA = TAXROW.Cells[colTotalAmount].AddParagraph().AppendText(materialTax.Rows[i]["TotalAmount"].ToString());
                textRangeS.CharacterFormat.FontSize = 6;
                textRange.CharacterFormat.FontSize = 6;
                textRangeP.CharacterFormat.FontSize = 6;
                textRangeT.CharacterFormat.FontSize = 6;
                textRangeTA.CharacterFormat.FontSize = 6;
                textRangeTOA.CharacterFormat.FontSize = 6;

            }
            //   trange.CharacterFormat.FontSize = 8;

            #endregion

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTaxTable);
            document.Replace(replaceString, textBodyPart, true, true);

            double total = clsStdLib.dbl(materialTax.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString())
                 + clsStdLib.dbl(materialTax.Compute("SUM(TaxON)", "").ToString());

            return total;
        }

        public double makeOrderWithoutSUIServiceTable(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{ServiceItems}";

            ReportUtility ru = new ReportUtility();

            DataTable salesService, serviceTax;

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;



            salesService = loadSalesMaster(salesId);
            if (salesService.Rows.Count == 0)
            {
                document.Replace("{ServiceCaption}", "", false, false);
                document.Replace(replaceString, "", false, false);
                return 0;

            }
            document.Replace("{ServiceCaption}", "Serive Details", false, false);
            serviceTax = loadGRNServiceMasterTex(salesId);

            int LasColumnIndex = 2;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(serviceTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Service Name");
            range.ApplyCharacterFormat(FontBold);
            int colServiceGroup = COL; COL++;

            wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Amount");
                range.ApplyCharacterFormat(FontBold);
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
                    range.ApplyCharacterFormat(FontBold);


                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }
            }
            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < salesService.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                TROW.Cells[colServiceGroup].AddParagraph().AppendText(salesService.Rows[i]["Service"].ToString());
                //TROW.Cells[colServiceGroup].AddParagraph().AppendText(salesService.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(salesService.Rows[i]["BooksCurrencyTransactionAmount"].ToString()).ToString("#,##0.00"));

                totalValue += clsStdLib.dbl(salesService.Rows[i]["Amount"].ToString());

                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(serviceTax.DefaultView.ToTable());
                    //double totalTax = 0;

                    for (int T = 0; T < dv.Count; T++)
                    {
                        //dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "'";
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND SalesMaterialId='" + serviceTax.Rows[i]["SalesMaterialId"].ToString() + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("#,##0.00"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["BooksCurrencyTaxAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colHSN || dicTaxes.ContainsValue(C))
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
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(salesService.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString())
                   //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                   + clsStdLib.dbl(serviceTax.Compute("SUM(BooksCurrencyTaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("ServiceStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 120;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("ServiceStyle");
                    }
                }
            }

            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);

            IWParagraphStyle style = document.AddParagraphStyle("ServiceSubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("ServiceSubTotalStyle");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return total;
        }

        public DataTable loadGRNMaterialMaster(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IR.Id CustomerNo
                               ,IR.CompanyGroupId
                                ,IR.CompanyId
								,p.UserName Customer
								,Addres.Address1 VendorAddress
								,ISNULL(HSNC.Code,MHSN.Code) HSNCode
                                ,Plant.GSTIN 
                                ,DPARTYPL.GSTIN ShipGSTIN
                                ,INVPARTYPL.GSTIN BillGSTIN
								,IR.DocRefNo   
	                            ,IR.InvoiceNo
                                ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
                                ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
		                        ,IR.InvoicingPartyPlantId
		                        ,INVPARTYPL.UserName InvoiceParty
                                ,INVPARTYPL.UserName InvoiceParty2
		                        ,IR.InvoicingByAddress
		                        ,IR.DeliveryByAddress
		                        ,DPARTYPL.UserName DeliveryParty
		                        ,IR.DeliveryPartyPlantId		
		                        ,IRD.MaterialMasterId
		                        
		                        ,IR.AddedBy
		                        ,IR.AddedDate
		                        ,IR.UpdatedBy
		                        ,IR.UpdatedDate
		                        ,0 IsApproved
		                        ,IR.PartyType
		                        ,0 IsNonCreditable
		                        ,IR.CurrencyId
	                            ,CRNC.Code AS CurrencyName
	                            ,IR.ToCurrencyRate
		                        ,BASECRNC.Code AS BaseCurrencyName
		                        ,PayTerm.UserName PaymentTerm
	                          ,MM.UserName MaterialMaster
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
	                          ,IRD.ArticleId
	                          ,MMA.StandardName Article
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IRD.FirstCharacteristicsValueId
	                          ,FCV.UserName AS FirstCharacteristicsValue
                              ,IRD.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                          ,IRD.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                          ,ROUND(IRD.TransactionQty, 2) POTransactionQty
	                          ,ROUND(IRD.TransactionRate, 4) TransactionRate
	                          ,ROUND((IRD.TransactionQty * IRD.TransactionRate), 2) AS TrnAmount
	                          ,IRD.BaseAmount
	                          ,IRD.TaxAmount AS BaseTaxAmount
	                          ,TaxAmount = (
		                            SELECT SUM(TaxAmount)
		                            FROM [TRN].[PurchaseOrderTax]
		                            WHERE InventoryReceiveDetailId = IRD.Id
		                            )
	                          ,ServiceTaxAmount = (
		                            SELECT SUM(TaxAmount)
		                            FROM [TRN].[SalesService]
		                            WHERE SalesId = IRD.Id
		                            )
	                         
	                          ,IRD.TransactionUoMId
	                          ,TUoM.UserName AS TransactionUoM
                              ,IRD.Id InventoryReceiveDetailId
							
							  ,OurOrderRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+MO.OwnReferenceNo FROM 
                                        TRN.SalesOrderItem SOI
										JOIN TRN.MasterOrder MO ON MO.Id=SOI.MasterOrderId
                                        WHERE IR.Id=SOI.SalesId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
							,YourOrderRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+MO.BuyerReferenceNo FROM 
                                        TRN.SalesOrderItem SOI
										JOIN TRN.MasterOrder MO ON MO.Id=SOI.MasterOrderId
                                        WHERE IR.Id=SOI.SalesId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
                                ,IR.ComercialInvoiceNo,IR.BLNumber,FORMAT(IR.BLDate,'dd-MMM-yyyy')BLDate,IR.EXPFromNo,FORMAT(IR.EXPDate,'dd-MMM-yyyy')EXPDate,IR.ItemDescription,IR.AddedBy CreatedBy
                           FROM TRN.Sales IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId

                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = IR.CurrencyId
                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                         LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                         LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId 
						 LEFT JOIN HKP.Party P ON P.Id=IR.PartyId
						 LEFT JOIN [MST].[AddressMaster] Addres ON Addres.Id= P.AddressMasterId
						  

                        -- LEFT JOIN trn.inventoryReceiveDetail IRD ON IR.Id = IRD.InventoryReceiveId

                         LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
                         LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IRD.MaterialMasterId
                         LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IRD.ArticleId
						 	LEFT JOIN [HKP].[HSNCode] AS MHSN ON MHSN.ID=MM.HSNCodeId
						 	LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MMA.HSNCodeId
                         LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
                         JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                          WHERE IR.Id ='" + SalesId + "'";

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
        public DataTable loadSalesInvoiceHeader(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IR.Id CustomerNo,IRD.Id SalesMaterialId
								,IR.CompanyGroupId
                                ,IR.CompanyId,CRNC.Code
								,p.UserName Customer
                                ,P.UserName Buyer
								,ir.CurrencyId
								,cmp.BaseCurrencyId
								,P.TINNO CustomerGSTNo
								,p.VATResistrationNo as CustomerPANNo
								,Addres.Address1 VendorAddress
								,ISNULL(HSNC.Code,MHSN.Code) HSNCode
                                ,Plant.GSTIN 
								,Plant.VATResistrationNo as PlantPANNo
                                ,DPARTYPL.GSTIN ShipGSTIN
                                ,INVPARTYPL.GSTIN BillGSTIN
								,IR.DocRefNo   
	                            ,IR.InvoiceNo
                                ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS DocDate
                                ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
                                ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
		                        ,IR.InvoicingPartyPlantId
		                        ,INVPARTYPL.UserName InvoiceParty
                                ,INVPARTYPL.UserName InvoiceParty2
		                        ,IR.InvoicingByAddress as ConsigneeAddress
		                        ,IR.DeliveryByAddress
		                        ,DPARTYPL.UserName DeliveryParty
		                        ,IR.DeliveryPartyPlantId	
		                        ,IRD.MaterialMasterId
								,PSI.PreCarriageBy
								,PSI.PlaceOfReceiptByPreCarriage
								,PSI.CNFContainerNo
								,PSI.CNFVesselName
								,PSI.CNFVesselTrackingNo 	
                                ,LC.LcNo,LC.BenificiaryBank,LC.OpeningBank
								,FORMAT(LC.LCDate,'dd-MMM-yyyy')LCDate
                                ,LC.BenificiaryBankDescription
                                ,LC.OpeningBankAddress
								,D.UserName as FinalDestination
								,PL.UserName as PortOfLanding
								,PD.UserName as PortOfDischarge
								,PoD.UserName as PortOfDelivery	                       
	                            ,CRNC.Code AS CurrencyName
	                            ,IR.ToCurrencyRate
		                        ,BASECRNC.Code AS BaseCurrencyName
		                        ,PayTerm.UserName PaymentTerm
	                          ,MM.UserName MaterialMaster
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
	                          ,MMA.StandardName Article
	                          ,FC.UserName FirstChar
	                          ,FCV.UserName AS FirstCharacteristicsValue
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.UserName SecondChar
	                          ,TC.UserName ThirdChar
	                          ,ROUND(IRD.TransactionQty, 2) POTransactionQty
	                          ,ROUND(IRD.TransactionRate, 4) TransactionRate
	                          ,ROUND((IRD.TransactionQty * IRD.TransactionRate), 2) AS TrnAmount 
	                          ,IRD.BaseAmount
	                          ,IRD.TaxAmount AS BaseTaxAmount
	                          ,TaxAmount = (
		                            SELECT SUM(TaxAmount)
		                            FROM [TRN].[PurchaseOrderTax]
		                            WHERE InventoryReceiveDetailId = IRD.Id
		                            )
	                          ,ServiceTaxAmount = (
		                            SELECT SUM(TaxAmount)
		                            FROM [TRN].[SalesService]
		                            WHERE SalesId = IRD.Id
		                            )
	                          ,TUoM.UserName AS TransactionUoM
							  ,PONumber=REPLACE(REPLACE(
										STUFF((select distinct ', '+CPO.PONumber FROM 
                                        TRN.SalesMaterial SM
										JOIN TRN.SalesOrder SO ON SO.Id=SM.SalesOrderId
										JOIN TRN.CustomerPO CPO ON CPO.id=SO.CustomerPOId
                                        WHERE IR.Id=SM.SalesId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
							  ,OurOrderRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+MO.OwnReferenceNo FROM 
                                        TRN.SalesOrderItem SOI
										JOIN TRN.MasterOrder MO ON MO.Id=SOI.MasterOrderId
                                        WHERE IR.Id=SOI.SalesId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
							,YourOrderRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+MO.BuyerReferenceNo FROM 
                                        TRN.SalesOrderItem SOI
										JOIN TRN.MasterOrder MO ON MO.Id=SOI.MasterOrderId
                                        WHERE IR.Id=SOI.SalesId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
										,AddedDate=REPLACE(REPLACE(
										STUFF((select distinct ', '+FORMAT(MO.AddedDate,'dd-MMM-yyyy') FROM 
                                        TRN.SalesOrderItem SOI
										JOIN TRN.MasterOrder MO ON MO.Id=SOI.MasterOrderId
                                        WHERE IR.Id=SOI.SalesId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
                                ,IR.ComercialInvoiceNo,IR.BLNumber,FORMAT(IR.BLDate,'dd-MMM-yyyy')BLDate,
								IR.EXPFromNo,FORMAT(IR.EXPDate,'dd-MMM-yyyy')EXPDate,IR.ItemDescription
								,PSI.TransportVehicleNo,PSI.TransportDriverName,PPSI.UserName TransporterName,BM.AccountTitle,BM.AccountNumber
								,BMA.Address1,PSI.TransportDocRefNo,FORMAT(PSI.TransportDocDate,'dd-MMM-yyyy') CNFBLAWBDate
								,B.UserName as Bank,BB.UserName as BankBranch
								,IRD.BooksCurrencyTransactionAmount
								,IRD.BooksCurrencyTaxAmount
								,IRD.BooksCurrencyBaseRate
								,IR.AddedBy CreatedBy
                        FROM TRN.Sales IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId

						 LEFT JOIN dbo.PostSalesInvoice PSI ON PSI.SalesId=IR.Id
					     LEFT JOIN MST.[Port] as PL on PL.Id= PSI.PortOfLoadingId
						 LEFT JOIN MST.[Port] as PD on PD.Id= PSI.PortOfDischargeId
						 LEFT JOIN MST.[Port] as PoD on PoD.Id= PSI.PortOfDelivaryId
						 LEFT JOIN MST.Destination as D on D.Id= PSI.FinalDestinationId


                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = cmp.BaseCurrencyId
                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                         LEFT JOIN HKP.PartyPlant  INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                         LEFT JOIN HKP.PartyPlant  DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId 
						 LEFT JOIN HKP.Party P ON P.Id=IR.PartyId
						 LEFT JOIN [MST].[AddressMaster] Addres ON Addres.Id= P.AddressMasterId
                         LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
                         LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IRD.MaterialMasterId
                         LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IRD.ArticleId
						 LEFT JOIN [HKP].[HSNCode] AS MHSN ON MHSN.ID=MM.HSNCodeId
						 	LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MMA.HSNCodeId
                         LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
                         LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
						 LEFT JOIN HKP.Party PPSI ON PPSI.Id=PSI.TransportAgentId
						 LEFT JOIN MST.BankMaster BM ON BM.Id=PSI.BankMasterId
						 LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
						 LEFT JOIN HKP.BankBranch BB ON BB.BankId=BM.BankId And BB.Id=BM.BankBranchId
						 LEFT JOIN [MST].[AddressMaster] BMA ON BMA.Id= BB.AddressMasterId
                         LEFT JOIN (
						 select distinct
						 PLC.LCRef as LcNo,PLC.LCDate,PLC.BenificiaryBank,PLC.BenificiaryBankDescription 
						 ,B.UserName OpeningBank,SOI.SalesId
						 ,OA.Address1 OpeningBankAddress
						 from trn.SalesOrderItem as SOI
						 LEFT JOIN TRN.MasterOrderItem  MOI on MOI.Id=SOI.MasterOrderItemId
						 LEFT JOIN TRN.SalesOrder SO on MOI.Id = SO.MasterOrderItemId
                         LEFT JOIN dbo.[Contract]  C on c.Id = SO.ContractId
						 LEFT JOIN dbo.PurchaseLC PLC on PLC.ContractId=C.Id						
						 LEFT JOIN  MST.BankMaster OB on OB.Id=PLC.OpeningBankMasterId
						 LEFT JOIN  HKP.Bank B on B.Id=OB.BankId
						 LEFT JOIN MST.AddressMaster OA on OA.Id = B.AddressMasterId						
						 ) LC on LC.SalesId=IR.Id
                          WHERE IR.Id ='" + SalesId + "'";

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
        public DataTable loadSalesInvoiceMaterial(string SalesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string strSQL;
            try
            {
                strSQL = @"SELECT SM.Id, SM.SalesId, MGM.UserName AS MaterialGroupMasterName, SM.MaterialMasterId, MM.UserName MaterialMasterName, SM.ArticleId, ART.StandardName
								, SM.TransactionQty, TUoM.UserName AS TransactionUoM, SM.TransactionRate, CU.Code AS Currency, SM.TransactionAmount, SM.TaxAmount, SM.NetAmount 
								FROM TRN.SalesMaterial AS SM 
								LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId
								LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
								LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
								LEFT JOIN MST.MaterialMasterArticle AS ART ON SM.ArticleId=ART.Id
								LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
								JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
								WHERE SA.CompanyGroupId='" + identity.CompanyGroupId + @"' AND SA.CompanyId='" + identity.CompanyId + @"' AND SA.PlantId='" + identity.PlantId + @"' AND SA.Id='" + SalesId + @"'";

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
        public DataTable loadLocalTaxMaterialMaster(string SalesId)
        {
            string strSQL;
            try
            {


                strSQL = @"SELECT IR.Id CustomerNo,SR.Id SalesReturnNo, IRD.Id SalesMaterialId
                                 , IR.CompanyGroupId
                                ,IR.CompanyId,CRNC.Code
								,p.UserName Customer
                                , P.UserName Buyer
                                 , ir.CurrencyId
								,cmp.BaseCurrencyId
								,P.TINNO CustomerGSTNo
                                , p.VATResistrationNo as CustomerPANNo
								,Addres.Address1 VendorAddress
                                , ISNULL(HSNC.Code,MHSN.Code) HSNCode
                                 , Plant.GSTIN
								,Plant.VATResistrationNo as PlantPANNo
                                ,DPARTYPL.GSTIN ShipGSTIN
                                , INVPARTYPL.GSTIN BillGSTIN
                                 , IR.DocRefNo
	                            ,IR.InvoiceNo
                                ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS DocDate
                                , REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
                                 , REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                  , REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
                                   , IR.InvoicingPartyPlantId
		                        ,INVPARTYPL.UserName InvoiceParty
                                , INVPARTYPL.UserName InvoiceParty2
                                 , IR.InvoicingByAddress as ConsigneeAddress
		                        ,IR.DeliveryByAddress
		                        ,DPARTYPL.UserName DeliveryParty
                                , IR.DeliveryPartyPlantId
		                        ,IRD.MaterialMasterId
								,PSI.PreCarriageBy
								,PSI.PlaceOfReceiptByPreCarriage
								,PSI.CNFContainerNo
								,PSI.CNFVesselName
								,PSI.CNFVesselTrackingNo
                                ,LC.LcNo,LC.BenificiaryBank,LC.OpeningBank
								,FORMAT(LC.LCDate, 'dd-MMM-yyyy')LCDate
                                ,LC.BenificiaryBankDescription
                                ,LC.OpeningBankAddress
								,D.UserName as FinalDestination
								,PL.UserName as PortOfLanding
								,PD.UserName as PortOfDischarge
								,PoD.UserName as PortOfDelivery
	                            ,CRNC.Code AS CurrencyName
	                            ,IR.ToCurrencyRate
		                        ,BASECRNC.Code AS BaseCurrencyName
		                        ,PayTerm.UserName PaymentTerm
                              , MM.UserName MaterialMaster
                               , MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
                              , MMA.StandardName Article
                               , FC.UserName FirstChar
                                , FCV.UserName AS FirstCharacteristicsValue
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.UserName SecondChar
                              , TC.UserName ThirdChar
                               , ROUND(IRD.TransactionQty, 2) POTransactionQty
	                          ,ROUND(IRD.TransactionRate, 4) TransactionRate
	                          ,ROUND((IRD.TransactionQty * IRD.TransactionRate), 2) AS TrnAmount
                              , IRD.BaseAmount
	                          ,IRD.TaxAmount AS BaseTaxAmount
	                          ,TaxAmount = (
                                    SELECT SUM(TaxAmount)

                                    FROM[TRN].[PurchaseOrderTax]

                                    WHERE InventoryReceiveDetailId = IRD.Id
		                            )
	                          ,ServiceTaxAmount = (
                                    SELECT SUM(TaxAmount)

                                    FROM[TRN].[SalesService]

                                    WHERE SalesId = IRD.Id
		                            )
	                          ,TUoM.UserName AS TransactionUoM
							  ,PONumber = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + CPO.PONumber FROM
                                        TRN.SalesMaterial SM

                                        JOIN TRN.SalesOrder SO ON SO.Id = SM.SalesOrderId

                                        JOIN TRN.CustomerPO CPO ON CPO.id = SO.CustomerPOId
                                        WHERE IR.Id = SM.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
							  ,OurOrderRefNo = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + MO.OwnReferenceNo FROM
                                        TRN.SalesOrderItem SOI

                                        JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                                        WHERE IR.Id = SOI.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
							,YourOrderRefNo = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + MO.BuyerReferenceNo FROM
                                        TRN.SalesOrderItem SOI

                                        JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                                        WHERE IR.Id = SOI.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
										,AddedDate = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + FORMAT(MO.AddedDate, 'dd-MMM-yyyy') FROM
                                        TRN.SalesOrderItem SOI

                                        JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                                        WHERE IR.Id = SOI.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
                                ,IR.ComercialInvoiceNo,IR.BLNumber,FORMAT(IR.BLDate, 'dd-MMM-yyyy')BLDate,
								IR.EXPFromNo,FORMAT(IR.EXPDate, 'dd-MMM-yyyy')EXPDate,IR.ItemDescription
								,PSI.TransportVehicleNo,PSI.TransportDriverName,PPSI.UserName TransporterName, BM.AccountTitle,BM.AccountNumber
								,BMA.Address1,PSI.TransportDocRefNo,FORMAT(PSI.TransportDocDate, 'dd-MMM-yyyy') CNFBLAWBDate
								,B.UserName as Bank,BB.UserName as BankBranch
								,IRD.BooksCurrencyTransactionAmount
								,IRD.BooksCurrencyTaxAmount
								,IRD.BooksCurrencyBaseRate
                        ,(Select Stuff((
						Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
						from dbo.ProductLibraryAttribute pla
						LEFT JOIN dbo.SalesPacking SP ON pla.ProductLibraryId = SP.ProductLibraryId
						WHERE SP.SalesId=IR.Id
						for XML PATH('')
						) , 1, 2, '')) as ProdDetails,IR.AddedBy CreatedBy
                        FROM TRN.SalesReturn SR
                        Left JOIN TRN.Sales IR ON IR.Id=SR.SalesId
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN dbo.PostSalesInvoice PSI ON PSI.SalesId = IR.Id
                         LEFT JOIN MST.[Port] as PL on PL.Id = PSI.PortOfLoadingId
                         LEFT JOIN MST.[Port] as PD on PD.Id = PSI.PortOfDischargeId
                         LEFT JOIN MST.[Port] as PoD on PoD.Id = PSI.PortOfDelivaryId
                         LEFT JOIN MST.Destination as D on D.Id = PSI.FinalDestinationId
                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = cmp.BaseCurrencyId
                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                         LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                         LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
                         LEFT JOIN HKP.Party P ON P.Id = IR.PartyId
                         LEFT JOIN[MST].[AddressMaster] Addres ON Addres.Id = P.AddressMasterId
                         LEFT JOIN trn.SalesReturnDetail AS IRD ON IRD.SalesReturnId = SR.Id
                         LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IRD.MaterialMasterId
                         LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IRD.ArticleId
                         LEFT JOIN[HKP].[HSNCode] AS MHSN ON MHSN.ID = MM.HSNCodeId
                         LEFT JOIN[HKP].[HSNCode] AS HSNC ON HSNC.ID = MMA.HSNCodeId
                         LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
                         LEFT JOIN[SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                         LEFT JOIN HKP.Party PPSI ON PPSI.Id = PSI.TransportAgentId
                         LEFT JOIN MST.BankMaster BM ON BM.Id = IR.PaymentToReceiveBankId
                         LEFT JOIN HKP.Bank B ON B.Id = BM.BankId
                         LEFT JOIN HKP.BankBranch BB ON BB.BankId = BM.BankId And BB.Id = BM.BankBranchId
                         LEFT JOIN[MST].[AddressMaster] BMA ON BMA.Id = BB.AddressMasterId
                         LEFT JOIN(
                         select distinct
                         PLC.LCRef as LcNo,PLC.LCDate,PLC.BenificiaryBank,PLC.BenificiaryBankDescription
						 ,B.UserName OpeningBank, SOI.SalesId
						 ,OA.Address1 OpeningBankAddress
                         from trn.SalesOrderItem as SOI
                         LEFT JOIN TRN.MasterOrderItem MOI on MOI.Id = SOI.MasterOrderItemId
                         LEFT JOIN TRN.SalesOrder SO on MOI.Id = SO.MasterOrderItemId
                         LEFT JOIN dbo.[Contract]  C on c.Id = SO.ContractId
                         LEFT JOIN dbo.PurchaseLC PLC on PLC.ContractId = C.Id
                         LEFT JOIN  MST.BankMaster OB on OB.Id = PLC.OpeningBankMasterId
                         LEFT JOIN  HKP.Bank B on B.Id = OB.BankId
                         LEFT JOIN MST.AddressMaster OA on OA.Id = B.AddressMasterId						
						 ) LC on LC.SalesId = IR.Id
                         WHERE SR.Id ='" + SalesId + "'";

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

        public DataTable GetLotWiseSalesReportData(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IR.Id CustomerNo   
    ,CRNC.Code
    ,cmp.BaseCurrencyId
    ,p.UserName Customer
    ,P.UserName Buyer 
    ,P.TINNO CustomerGSTNo
    ,p.VATResistrationNo AS CustomerPANNo
    ,Addres.Address1 VendorAddress,ISNULL(DAddres.Email,Addres.Email)Email
    ,ISNULL(HSNC.Code,MHSN.Code) HSNCode
    ,Plant.GSTIN
    ,Plant.VATResistrationNo AS PlantPANNo
    ,DPARTYPL.GSTIN ShipGSTIN
    ,INVPARTYPL.GSTIN BillGSTIN
    ,IR.DocRefNo
    ,IR.InvoiceNo
    ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS DocDate
    ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
    ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
    ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
   
    ,INVPARTYPL.UserName InvoiceParty
    ,INVPARTYPL.UserName InvoiceParty2
    ,IR.InvoicingByAddress AS ConsigneeAddress
    ,IR.DeliveryByAddress
    ,DPARTYPL.UserName DeliveryParty
 
    ,PSI.PreCarriageBy
    ,PSI.PlaceOfReceiptByPreCarriage
    ,PSI.CNFContainerNo
    ,PSI.CNFVesselName
    ,PSI.CNFVesselTrackingNo
    ,D.UserName AS FinalDestination
    ,PL.UserName AS PortOfLanding
    ,PD.UserName AS PortOfDischarge
    ,PoD.UserName AS PortOfDelivery
    ,CRNC.Code AS CurrencyName
    ,IR.ToCurrencyRate
    ,BASECRNC.Code AS BaseCurrencyName
    ,PayTerm.UserName PaymentTerm
    ,MM.UserName MaterialMaster
    ,MGM.UserName MaterialGroupMaster
    ,Article=CASE WHEN ISNULL(moi.LCArticle,'')<>'' THEN moi.LCArticle WHEN ISNULL(AA.ArticlePartyName,'')<>'' THEN AA.ArticlePartyName ELSE MMA.StandardName END
   
     ,POTransactionQty=CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCN.NetWeight,0)=0 THEN IRD.TransactionQty ELSE SCN.NetWeight END) 
    ,CONVERT(NUMERIC(10,4),IRD.TransactionRate, 4) TransactionRate
	,TrnAmount=CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCN.NetWeight,0)=0 THEN ROUND((IRD.TransactionQty * IRD.TransactionRate), 2) ELSE ROUND((SCN.NetWeight * IRD.TransactionRate), 2) END)
	 ,BaseAmount=CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCN.NetWeight,0)=0 THEN IRD.BaseAmount ELSE ROUND((SCN.NetWeight * IRD.TransactionRate), 2) END)
   ,BooksCurrencyTransactionAmount=CONVERT(NUMERIC(10,2),CASE WHEN ISNULL(SCN.NetWeight,0)=0 THEN IRD.BooksCurrencyTransactionAmount ELSE ROUND((SCN.NetWeight * CONVERT(NUMERIC(10,4),IRD.BooksCurrencyBaseRate)), 4) END)	
    ,CONVERT(NUMERIC(10,2),IRD.BooksCurrencyTaxAmount)BooksCurrencyTaxAmount
    ,CONVERT(NUMERIC(10,4),IRD.BooksCurrencyBaseRate)BooksCurrencyBaseRate
    ,TUoM.UserName AS TransactionUoM
    ,PONumber = REPLACE(REPLACE(STUFF((
                    SELECT DISTINCT ', ' + CPO.PONumber
                    FROM TRN.SalesMaterial SM
                    JOIN TRN.SalesOrder SO ON SO.Id = SM.SalesOrderId
                    JOIN TRN.CustomerPO CPO ON CPO.id = SO.CustomerPOId
                    WHERE IR.Id = SM.SalesId
                    FOR XML path('')
                        ,TYPE
                    ).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
    
    ,IR.ComercialInvoiceNo
    ,IR.BLNumber
    ,FORMAT(IR.BLDate, 'dd-MMM-yyyy') BLDate
    ,IR.EXPFromNo
    ,FORMAT(IR.EXPDate, 'dd-MMM-yyyy') EXPDate
    ,IR.ItemDescription
    ,PSI.TransportVehicleNo
    ,PSI.TransportDriverName
    ,PSI.TransportDriverNo
    ,PPSI.UserName TransporterName
    ,BM.AccountTitle
    ,BM.AccountNumber
    ,BMA.Address1
    ,PSI.TransportDocRefNo
    ,FORMAT(PSI.TransportDocDate, 'dd-MMM-yyyy') CNFBLAWBDate
    ,B.UserName AS Bank
    ,BB.UserName AS BankBranch
	
    ,(
        SELECT Stuff((
                    SELECT distinct ',' + pla.AttributeValue
                    FROM dbo.ProductLibraryAttribute pla
                    WHERE pla.ProductLibraryId = MOI.ProductLibraryId
                    FOR XML PATH('')
                    ), 1, 1, '')
        ) AS Shade
		,IR.AddedBy CreatedBy
        , SCN.Bags, SCN.LotNo, CONVERT(NUMERIC(10,2),SCN.GWeight)GWeight, MO.Type,MO.MasterOrderNo
,SalesOrderNo = REPLACE(REPLACE(STUFF((
                    SELECT DISTINCT ', ' + SO.Id
                    FROM TRN.SalesMaterial SM
                    JOIN TRN.SalesOrder SO ON SO.Id = SM.SalesOrderId
                    WHERE IR.Id = SM.SalesId
                    FOR XML path('')
                        ,TYPE
                    ).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
,MO.BuyerReferenceNo,BB.IFSCCode		
FROM TRN.Sales IR
LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
LEFT JOIN dbo.PostSalesInvoice PSI ON PSI.SalesId = IR.Id
LEFT JOIN MST.[Port] AS PL ON PL.Id = PSI.PortOfLoadingId
LEFT JOIN MST.[Port] AS PD ON PD.Id = PSI.PortOfDischargeId
LEFT JOIN MST.[Port] AS PoD ON PoD.Id = PSI.PortOfDelivaryId
LEFT JOIN MST.Destination AS D ON D.Id = PSI.FinalDestinationId
LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = cmp.BaseCurrencyId
LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
LEFT JOIN [MST].[AddressMaster] DAddres ON DAddres.Id = DPARTYPL.AddressMasterId
LEFT JOIN HKP.Party P ON P.Id = IR.PartyId
LEFT JOIN [MST].[AddressMaster] Addres ON Addres.Id = P.AddressMasterId
LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id

left join [TRN].[MasterOrder] MO on MO.Id = MOI.MasterOrderId
left join ProductLibrary PLA on PLA.Id = MOI.ProductLibraryId
LEFT JOIN (SELECT SalesId,SalesMaterialId, LotNo, COUNT(RefNo) Bags, 
            SUM(NetWeight)NetWeight,SUM(GWeight)GWeight FROM ItemScanChild group by SalesId ,SalesMaterialId, LotNo) SCN on SCN.SalesId = IR.Id AND SCN.SalesMaterialId=IRD.Id

LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IRD.MaterialMasterId
LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IRD.ArticleId
LEFT JOIN dbo.ArticleAlias AA ON AA.ArticleId=MMA.Id AND AA.MasterOrderItemID=MOI.Id
LEFT JOIN [HKP].[HSNCode] AS MHSN ON MHSN.ID = MM.HSNCodeId
LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID = MMA.HSNCodeId

LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
LEFT JOIN HKP.Party PPSI ON PPSI.Id = PSI.TransportAgentId
LEFT JOIN MST.BankMaster BM ON BM.Id = IR.PaymentToReceiveBankId
LEFT JOIN HKP.Bank B ON B.Id = BM.BankId
LEFT JOIN HKP.BankBranch BB ON BB.BankId = BM.BankId AND BB.Id = BM.BankBranchId
LEFT JOIN [MST].[AddressMaster] BMA ON BMA.Id = BB.AddressMasterId
                         WHERE IR.Id ='" + SalesId + "' AND SCN.Bags<>''";

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

        public DataTable GetloadLocalTaxMaterialMaster(string SalesId)
        {
            string strSQL;
            try
            {


                strSQL = @"SELECT IR.Id CustomerNo, IRD.Id SalesMaterialId
                                 , IR.CompanyGroupId
                                ,IR.CompanyId,CRNC.Code
								,p.UserName Customer
                                , P.UserName Buyer
                                 , ir.CurrencyId
								,cmp.BaseCurrencyId
								,P.TINNO CustomerGSTNo
                                , p.VATResistrationNo as CustomerPANNo
								,Addres.Address1 VendorAddress
                                , ISNULL(MHSN.Code,MHC.Code) HSNCode
                                 , Plant.GSTIN
								,Plant.VATResistrationNo as PlantPANNo
                                ,DPARTYPL.GSTIN ShipGSTIN
                                , INVPARTYPL.GSTIN BillGSTIN
                                 , IR.DocRefNo
	                            ,IR.InvoiceNo
                                ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS DocDate
                                , REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
                                 , REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                  , REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
                                   , IR.InvoicingPartyPlantId
		                        ,INVPARTYPL.UserName InvoiceParty
                                , INVPARTYPL.UserName InvoiceParty2
                                 , IR.InvoicingByAddress as ConsigneeAddress
		                        ,IR.DeliveryByAddress
		                        ,DPARTYPL.UserName DeliveryParty
                                , IR.DeliveryPartyPlantId
		                        ,IRD.MaterialMasterId
								,PSI.PreCarriageBy
								,PSI.PlaceOfReceiptByPreCarriage
								,PSI.CNFContainerNo
								,PSI.CNFVesselName
								,PSI.CNFVesselTrackingNo
                                ,LC.LcNo,LC.BenificiaryBank,LC.OpeningBank
								,FORMAT(LC.LCDate, 'dd-MMM-yyyy')LCDate
                                ,LC.BenificiaryBankDescription
                                ,LC.OpeningBankAddress
								,D.UserName as FinalDestination
								,PL.UserName as PortOfLanding
								,PD.UserName as PortOfDischarge
								,PoD.UserName as PortOfDelivery
	                            ,CRNC.Code AS CurrencyName
	                            ,IR.ToCurrencyRate
		                        ,BASECRNC.Code AS BaseCurrencyName
		                        ,PayTerm.UserName PaymentTerm
                              , MM.UserName MaterialMaster
                               , MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
                              ,Article=CASE WHEN ISNULL(moi.LCArticle,'')<>'' THEN moi.LCArticle WHEN ISNULL(AA.ArticlePartyName,'')<>'' THEN AA.ArticlePartyName ELSE MMA.StandardName END
                               , FC.UserName FirstChar
                                , FCV.UserName AS FirstCharacteristicsValue
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.UserName SecondChar
                              , TC.UserName ThirdChar
                               , ROUND(IRD.TransactionQty, 2) POTransactionQty
	                          ,ROUND(IRD.TransactionRate, 4) TransactionRate
	                          ,ROUND((IRD.TransactionQty * IRD.TransactionRate), 2) AS TrnAmount
                              , IRD.BaseAmount
	                          ,IRD.TaxAmount AS BaseTaxAmount
	                          ,TaxAmount = (
                                    SELECT SUM(TaxAmount)

                                    FROM[TRN].[PurchaseOrderTax]

                                    WHERE InventoryReceiveDetailId = IRD.Id
		                            )
	                          ,ServiceTaxAmount = (
                                    SELECT SUM(TaxAmount)

                                    FROM[TRN].[SalesService]

                                    WHERE SalesId = IRD.Id
		                            )
	                          ,TUoM.UserName AS TransactionUoM
							  ,PONumber = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + CPO.PONumber FROM
                                        TRN.SalesMaterial SM

                                        JOIN TRN.SalesOrder SO ON SO.Id = SM.SalesOrderId

                                        JOIN TRN.CustomerPO CPO ON CPO.id = SO.CustomerPOId
                                        WHERE IR.Id = SM.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
							  ,OurOrderRefNo = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + MO.OwnReferenceNo FROM
                                        TRN.SalesOrderItem SOI

                                        JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                                        WHERE IR.Id = SOI.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
							,YourOrderRefNo = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + MO.BuyerReferenceNo FROM
                                        TRN.SalesOrderItem SOI

                                        JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                                        WHERE IR.Id = SOI.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
										,AddedDate = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + FORMAT(MO.AddedDate, 'dd-MMM-yyyy') FROM
                                        TRN.SalesOrderItem SOI

                                        JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                                        WHERE IR.Id = SOI.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
                                ,IR.ComercialInvoiceNo,IR.BLNumber,FORMAT(IR.BLDate, 'dd-MMM-yyyy')BLDate,
								IR.EXPFromNo,FORMAT(IR.EXPDate, 'dd-MMM-yyyy')EXPDate,IR.ItemDescription
								,PSI.TransportVehicleNo,PSI.TransportDriverName,PPSI.UserName TransporterName, BM.AccountTitle,BM.AccountNumber
								,BMA.Address1,PSI.TransportDocRefNo,FORMAT(PSI.TransportDocDate, 'dd-MMM-yyyy') CNFBLAWBDate
								,B.UserName as Bank,BB.UserName as BankBranch
								,IRD.BooksCurrencyTransactionAmount
								,IRD.BooksCurrencyTaxAmount
								,IRD.BooksCurrencyBaseRate
                        ,(Select Stuff((
						Select distinct ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
						from dbo.ProductLibraryAttribute pla
						LEFT JOIN dbo.SalesPacking SP ON pla.ProductLibraryId = SP.ProductLibraryId
						WHERE SP.SalesId=IR.Id
						for XML PATH('')
						) , 1, 2, '')) as ProdDetails,IR.AddedBy CreatedBy
                        FROM TRN.Sales IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN dbo.PostSalesInvoice PSI ON PSI.SalesId = IR.Id
                         LEFT JOIN MST.[Port] as PL on PL.Id = PSI.PortOfLoadingId
                         LEFT JOIN MST.[Port] as PD on PD.Id = PSI.PortOfDischargeId
                         LEFT JOIN MST.[Port] as PoD on PoD.Id = PSI.PortOfDelivaryId
                         LEFT JOIN MST.Destination as D on D.Id = PSI.FinalDestinationId
                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = cmp.BaseCurrencyId
                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                         LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                         LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
                         LEFT JOIN HKP.Party P ON P.Id = IR.PartyId
                         LEFT JOIN[MST].[AddressMaster] Addres ON Addres.Id = P.AddressMasterId
                         LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
 LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
                         LEFT JOIN (Select distinct HsnCodeId,SalesMaterialId FROM TRN.SalesTax ) STH ON STH.SalesMaterialId=IRD.Id
                         LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IRD.MaterialMasterId
                         LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IRD.ArticleId
LEFT JOIN dbo.ArticleAlias AA ON AA.ArticleId=MMA.Id AND AA.MasterOrderItemID=MOI.Id
                         LEFT JOIN[HKP].[HSNCode] AS MHSN ON MHSN.ID = STH.HSNCodeId
                         LEFT JOIN[HKP].[HSNCode] AS MHC ON MHC.ID = MMA.HSNCodeId
                         LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
                         LEFT JOIN[SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                         LEFT JOIN HKP.Party PPSI ON PPSI.Id = PSI.TransportAgentId
                         LEFT JOIN MST.BankMaster BM ON BM.Id = IR.PaymentToReceiveBankId
                         LEFT JOIN HKP.Bank B ON B.Id = BM.BankId
                         LEFT JOIN HKP.BankBranch BB ON BB.BankId = BM.BankId And BB.Id = BM.BankBranchId
                         LEFT JOIN[MST].[AddressMaster] BMA ON BMA.Id = BB.AddressMasterId
                         LEFT JOIN(
                         select distinct
                         PLC.LCRef as LcNo,PLC.LCDate,PLC.BenificiaryBank,PLC.BenificiaryBankDescription
						 ,B.UserName OpeningBank, SOI.SalesId
						 ,OA.Address1 OpeningBankAddress
                         from trn.SalesOrderItem as SOI
                         LEFT JOIN TRN.MasterOrderItem MOI on MOI.Id = SOI.MasterOrderItemId
                         LEFT JOIN TRN.SalesOrder SO on MOI.Id = SO.MasterOrderItemId
                         LEFT JOIN dbo.[Contract]  C on c.Id = SO.ContractId
                         LEFT JOIN dbo.PurchaseLC PLC on PLC.ContractId = C.Id
                         LEFT JOIN  MST.BankMaster OB on OB.Id = PLC.OpeningBankMasterId
                         LEFT JOIN  HKP.Bank B on B.Id = OB.BankId
                         LEFT JOIN MST.AddressMaster OA on OA.Id = B.AddressMasterId						
						 ) LC on LC.SalesId = IR.Id
                         WHERE IR.Id ='" + SalesId + "'";

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

        public DataTable loadLocalTaxMaterialMasterWithProductDetail(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IR.Id CustomerNo
	,IRD.Id SalesMaterialId
	,IR.CompanyGroupId
	,IR.CompanyId
	,CRNC.Code
	,p.UserName Customer
	,P.UserName Buyer
	,ir.CurrencyId
	,cmp.BaseCurrencyId
	,P.TINNO CustomerGSTNo
	,p.VATResistrationNo AS CustomerPANNo
	,Addres.Address1 VendorAddress
	,ISNULL(HSNC.Code,MHSN.Code) HSNCode
	,Plant.GSTIN
	,Plant.VATResistrationNo AS PlantPANNo
	,DPARTYPL.GSTIN ShipGSTIN
	,INVPARTYPL.GSTIN BillGSTIN
	,IR.DocRefNo
	,IR.InvoiceNo
	,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS DocDate
	,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
	,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
	,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
	,IR.InvoicingPartyPlantId
	,INVPARTYPL.UserName InvoiceParty
	,INVPARTYPL.UserName InvoiceParty2
	,IR.InvoicingByAddress AS ConsigneeAddress
	,IR.DeliveryByAddress
	,DPARTYPL.UserName DeliveryParty
	,IR.DeliveryPartyPlantId
	,IRD.MaterialMasterId
	,PSI.PreCarriageBy
	,PSI.PlaceOfReceiptByPreCarriage
	,PSI.CNFContainerNo
	,PSI.CNFVesselName
	,PSI.CNFVesselTrackingNo
	,D.UserName AS FinalDestination
	,PL.UserName AS PortOfLanding
	,PD.UserName AS PortOfDischarge
	,PoD.UserName AS PortOfDelivery
	,CRNC.Code AS CurrencyName
	,IR.ToCurrencyRate
	,BASECRNC.Code AS BaseCurrencyName
	,PayTerm.UserName PaymentTerm
	,MM.UserName MaterialMaster
	,MM.MaterialGroupMasterId
	,MGM.UserName MaterialGroupMaster
	,Article=CASE WHEN ISNULL(moi.LCArticle,'')<>'' THEN moi.LCArticle WHEN ISNULL(AA.ArticlePartyName,'')<>'' THEN AA.ArticlePartyName ELSE MMA.StandardName END
	,FC.UserName FirstChar
	,FCV.UserName AS FirstCharacteristicsValue
	,SCV.UserName AS SecondCharacteristicsValue
	,TCV.UserName AS ThirdCharacteristicsValue
	,SC.UserName SecondChar
	,TC.UserName ThirdChar
	,ROUND(IRD.TransactionQty, 2) POTransactionQty
	,ROUND(IRD.TransactionRate, 4) TransactionRate
	,ROUND((IRD.TransactionQty * IRD.TransactionRate), 2) AS TrnAmount
	,IRD.BaseAmount
	,IRD.TaxAmount AS BaseTaxAmount
	,TaxAmount = (
		SELECT SUM(TaxAmount)
		FROM [TRN].[PurchaseOrderTax]
		WHERE InventoryReceiveDetailId = IRD.Id
		)
	,ServiceTaxAmount = (
		SELECT SUM(TaxAmount)
		FROM [TRN].[SalesService]
		WHERE SalesId = IRD.Id
		)
	,TUoM.UserName AS TransactionUoM
	,PONumber = REPLACE(REPLACE(STUFF((
					SELECT DISTINCT ', ' + CPO.PONumber
					FROM TRN.SalesMaterial SM
					JOIN TRN.SalesOrder SO ON SO.Id = SM.SalesOrderId
					JOIN TRN.CustomerPO CPO ON CPO.id = SO.CustomerPOId
					WHERE IR.Id = SM.SalesId
					FOR XML path('')
						,TYPE
					).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
	,OurOrderRefNo = REPLACE(REPLACE(STUFF((
					SELECT DISTINCT ', ' + MO.OwnReferenceNo
					FROM TRN.SalesOrderItem SOI
					JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
					WHERE IR.Id = SOI.SalesId
					FOR XML path('')
						,TYPE
					).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
	,YourOrderRefNo = REPLACE(REPLACE(STUFF((
					SELECT DISTINCT ', ' + MOI.BuyerReferenceNo
					FROM TRN.SalesOrderItem SOI
					--JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
					JOIN TRN.MasterOrderItem MOI ON MOI.Id = SOI.MasterOrderItemId
					WHERE IR.Id = SOI.SalesId
					FOR XML path('')
						,TYPE
					).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
	,AddedDate = REPLACE(REPLACE(STUFF((
					SELECT DISTINCT ', ' + FORMAT(MO.AddedDate, 'dd-MMM-yyyy')
					FROM TRN.SalesOrderItem SOI
					JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
					WHERE IR.Id = SOI.SalesId
					FOR XML path('')
						,TYPE
					).value('.', 'VARCHAR(MAX)'), 1, 1, ''), '&amp;', '&'), 'amp;', '')
	,IR.ComercialInvoiceNo
	,IR.BLNumber
	,FORMAT(IR.BLDate, 'dd-MMM-yyyy') BLDate
	,IR.EXPFromNo
	,FORMAT(IR.EXPDate, 'dd-MMM-yyyy') EXPDate
	,IR.ItemDescription
	,PSI.TransportVehicleNo
	,PSI.TransportDriverName
	,PPSI.UserName TransporterName
	,BM.AccountTitle
	,BM.AccountNumber
	,BMA.Address1
	,PSI.TransportDocRefNo
	,FORMAT(PSI.TransportDocDate, 'dd-MMM-yyyy') CNFBLAWBDate
	,B.UserName AS Bank
	,BB.UserName AS BankBranch
	,IRD.BooksCurrencyTransactionAmount
	,IRD.BooksCurrencyTaxAmount
	,IRD.BooksCurrencyBaseRate
	,(
		SELECT Stuff((
					SELECT distinct ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
					FROM dbo.ProductLibraryAttribute pla
					WHERE pla.ProductLibraryId = MOI.ProductLibraryId
					FOR XML PATH('')
					), 1, 2, '')
		) AS ProdDetails,IR.AddedBy CreatedBy 
, LEFT(MO.[Type], 1) [Type]
FROM TRN.Sales IR
LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
LEFT JOIN dbo.PostSalesInvoice PSI ON PSI.SalesId = IR.Id
LEFT JOIN MST.[Port] AS PL ON PL.Id = PSI.PortOfLoadingId
LEFT JOIN MST.[Port] AS PD ON PD.Id = PSI.PortOfDischargeId
LEFT JOIN MST.[Port] AS PoD ON PoD.Id = PSI.PortOfDelivaryId
LEFT JOIN MST.Destination AS D ON D.Id = PSI.FinalDestinationId
LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = cmp.BaseCurrencyId
LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
LEFT JOIN HKP.Party P ON P.Id = IR.PartyId
LEFT JOIN [MST].[AddressMaster] Addres ON Addres.Id = P.AddressMasterId
LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id

left join [TRN].[MasterOrder] MO on MO.Id = MOI.MasterOrderId


LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IRD.MaterialMasterId
LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IRD.ArticleId
LEFT JOIN dbo.ArticleAlias AA ON AA.ArticleId=MMA.Id AND AA.MasterOrderItemID=MOI.Id
LEFT JOIN [HKP].[HSNCode] AS MHSN ON MHSN.ID = MM.HSNCodeId
LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID = MMA.HSNCodeId
LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
LEFT JOIN HKP.Party PPSI ON PPSI.Id = PSI.TransportAgentId
LEFT JOIN MST.BankMaster BM ON BM.Id = IR.PaymentToReceiveBankId
LEFT JOIN HKP.Bank B ON B.Id = BM.BankId
LEFT JOIN HKP.BankBranch BB ON BB.BankId = BM.BankId
	AND BB.Id = BM.BankBranchId
LEFT JOIN [MST].[AddressMaster] BMA ON BMA.Id = BB.AddressMasterId
                                    WHERE IR.Id ='" + SalesId + "'";



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

        public DataTable loadLocalTaxWithoutSUIMaterialMaster(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @" SELECT IR.Id CustomerNo, IRD.Id SalesMaterialId
                                 , IR.CompanyGroupId
                                ,IR.CompanyId,CRNC.Code
								,p.UserName Customer
                                , P.UserName Buyer
                                 , ir.CurrencyId
								,cmp.BaseCurrencyId
								,P.TINNO CustomerGSTNo
                                , p.VATResistrationNo as CustomerPANNo
								,Addres.Address1 VendorAddress
                                , ISNULL(HSNC.Code,MHSN.Code) HSNCode
                                 , Plant.GSTIN
								,Plant.VATResistrationNo as PlantPANNo
                                ,DPARTYPL.GSTIN ShipGSTIN
                                , INVPARTYPL.GSTIN BillGSTIN
                                 , IR.DocRefNo
	                            ,IR.InvoiceNo
                                ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS DocDate
                                , REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
                                 , REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                  , REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
                                   , IR.InvoicingPartyPlantId
		                        ,INVPARTYPL.UserName InvoiceParty
                                , INVPARTYPL.UserName InvoiceParty2
                                 , IR.InvoicingByAddress as ConsigneeAddress
		                        ,IR.DeliveryByAddress
		                        ,DPARTYPL.UserName DeliveryParty
                                , IR.DeliveryPartyPlantId
		                        ,IRD.MaterialMasterId
								,PSI.PreCarriageBy
								,PSI.PlaceOfReceiptByPreCarriage
								,PSI.CNFContainerNo
								,PSI.CNFVesselName
								,PSI.CNFVesselTrackingNo
                                ,LC.LcNo,LC.BenificiaryBank,LC.OpeningBank
								,FORMAT(LC.LCDate, 'dd-MMM-yyyy')LCDate
                                ,LC.BenificiaryBankDescription
                                ,LC.OpeningBankAddress
								,D.UserName as FinalDestination
								,PL.UserName as PortOfLanding
								,PD.UserName as PortOfDischarge
								,PoD.UserName as PortOfDelivery
	                            ,CRNC.Code AS CurrencyName
	                            ,IR.ToCurrencyRate
		                        ,BASECRNC.Code AS BaseCurrencyName
		                        ,PayTerm.UserName PaymentTerm
                              , MM.UserName MaterialMaster
                               , MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
                              ,Article=CASE WHEN ISNULL(moi.LCArticle,'')<>'' THEN moi.LCArticle WHEN ISNULL(AA.ArticlePartyName,'')<>'' THEN AA.ArticlePartyName ELSE MMA.StandardName END
                               , FC.UserName FirstChar
                                , FCV.UserName AS FirstCharacteristicsValue
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.UserName SecondChar
                              , TC.UserName ThirdChar
                               , ROUND(IRD.TransactionQty, 2) POTransactionQty
	                          ,ROUND(IRD.TransactionRate, 4) TransactionRate
	                          ,ROUND((IRD.TransactionQty * IRD.TransactionRate), 2) AS TrnAmount
                              , IRD.BaseAmount
	                          ,IRD.TaxAmount AS BaseTaxAmount
	                          ,TaxAmount = (
                                    SELECT SUM(TaxAmount)

                                    FROM[TRN].[PurchaseOrderTax]

                                    WHERE InventoryReceiveDetailId = IRD.Id
		                            )
	                          ,ServiceTaxAmount = (
                                    SELECT SUM(TaxAmount)

                                    FROM[TRN].[SalesService]

                                    WHERE SalesId = IRD.Id
		                            )
	                          ,TUoM.UserName AS TransactionUoM
							  ,PONumber = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + CPO.PONumber FROM
                                        TRN.SalesMaterial SM

                                        JOIN TRN.SalesOrder SO ON SO.Id = SM.SalesOrderId

                                        JOIN TRN.CustomerPO CPO ON CPO.id = SO.CustomerPOId
                                        WHERE IR.Id = SM.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
							  ,OurOrderRefNo = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + MO.OwnReferenceNo FROM
                                        TRN.SalesOrderItem SOI

                                        JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                                        WHERE IR.Id = SOI.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
							,YourOrderRefNo = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + MO.BuyerReferenceNo FROM
                                        TRN.SalesOrderItem SOI

                                        JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                                        WHERE IR.Id = SOI.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
										,AddedDate = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + FORMAT(MO.AddedDate, 'dd-MMM-yyyy') FROM
                                        TRN.SalesOrderItem SOI

                                        JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                                        WHERE IR.Id = SOI.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
                                ,IR.ComercialInvoiceNo,IR.BLNumber,FORMAT(IR.BLDate, 'dd-MMM-yyyy')BLDate,
								IR.EXPFromNo,FORMAT(IR.EXPDate, 'dd-MMM-yyyy')EXPDate,IR.ItemDescription
								,PSI.TransportVehicleNo,PSI.TransportDriverName,PPSI.UserName TransporterName, BM.AccountTitle,BM.AccountNumber
								,BMA.Address1,PSI.TransportDocRefNo,FORMAT(PSI.TransportDocDate, 'dd-MMM-yyyy') CNFBLAWBDate
								,B.UserName as Bank,BB.UserName as BankBranch
								,IRD.BooksCurrencyTransactionAmount
								,IRD.BooksCurrencyTaxAmount
								,IRD.BooksCurrencyBaseRate
                        ,(Select Stuff((
						Select distinct ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
						from dbo.ProductLibraryAttribute pla
						LEFT JOIN dbo.SalesPacking SP ON pla.ProductLibraryId = SP.ProductLibraryId
						WHERE SP.SalesId=IR.Id
						for XML PATH('')
						) , 1, 2, '')) as ProdDetails,IR.AddedBy CreatedBy
                        FROM TRN.Sales IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN dbo.PostSalesInvoice PSI ON PSI.SalesId = IR.Id
                         LEFT JOIN MST.[Port] as PL on PL.Id = PSI.PortOfLoadingId
                         LEFT JOIN MST.[Port] as PD on PD.Id = PSI.PortOfDischargeId
                         LEFT JOIN MST.[Port] as PoD on PoD.Id = PSI.PortOfDelivaryId
                         LEFT JOIN MST.Destination as D on D.Id = PSI.FinalDestinationId
                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = cmp.BaseCurrencyId
                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                         LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                         LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
                         LEFT JOIN HKP.Party P ON P.Id = IR.PartyId
                         LEFT JOIN[MST].[AddressMaster] Addres ON Addres.Id = P.AddressMasterId
                         LEFT JOIN trn.SalesMaterial AS IRD ON IRD.SalesId = IR.Id
 LEFT JOIN [TRN].[SalesOrder] AS SO ON IRD.SalesOrderId = SO.Id
LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
                         LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IRD.MaterialMasterId
                         LEFT JOIN[HKP].[HSNCode] AS MHSN ON MHSN.ID = MM.HSNCodeId
                         LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IRD.ArticleId
LEFT JOIN dbo.ArticleAlias AA ON AA.ArticleId=MMA.Id AND AA.MasterOrderItemID=MOI.Id
                         LEFT JOIN[HKP].[HSNCode] AS HSNC ON HSNC.ID = MMA.HSNCodeId
                         LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
                         LEFT JOIN[SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                         LEFT JOIN HKP.Party PPSI ON PPSI.Id = PSI.TransportAgentId
                         LEFT JOIN MST.BankMaster BM ON BM.Id = IR.PaymentToReceiveBankId
                         LEFT JOIN HKP.Bank B ON B.Id = BM.BankId
                         LEFT JOIN HKP.BankBranch BB ON BB.BankId = BM.BankId And BB.Id = BM.BankBranchId
                         LEFT JOIN[MST].[AddressMaster] BMA ON BMA.Id = BB.AddressMasterId
                         LEFT JOIN(
                         select distinct
                         PLC.LCRef as LcNo,PLC.LCDate,PLC.BenificiaryBank,PLC.BenificiaryBankDescription
						 ,B.UserName OpeningBank, SOI.SalesId
						 ,OA.Address1 OpeningBankAddress
                         from trn.SalesOrderItem as SOI
                         LEFT JOIN TRN.MasterOrderItem MOI on MOI.Id = SOI.MasterOrderItemId
                         LEFT JOIN TRN.SalesOrder SO on MOI.Id = SO.MasterOrderItemId
                         LEFT JOIN dbo.[Contract]  C on c.Id = SO.ContractId
                         LEFT JOIN dbo.PurchaseLC PLC on PLC.ContractId = C.Id
                         LEFT JOIN  MST.BankMaster OB on OB.Id = PLC.OpeningBankMasterId
                         LEFT JOIN  HKP.Bank B on B.Id = OB.BankId
                         LEFT JOIN MST.AddressMaster OA on OA.Id = B.AddressMasterId						
						 ) LC on LC.SalesId = IR.Id
                         WHERE IR.Id ='" + SalesId + "'";

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

        public DataTable loadSalesItems(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IOS.Id ServiceId, SM.UserName  Service ,IOS.Amount,IOS.TaxAmount,IOS.AddedBy,IOS.AddedDate,IOS.UpdatedBy,IOS.UpdatedDate 
                               FROM TRN.Sales   IR
                            INNER join trn.SalesService IOS ON IOS.SalesId = IR.Id
                            INNER JOIN HKP.ServiceMaster SM ON IOS.ServiceMasterId = SM.Id 
                             where IR.Id = '" + SalesId + "'";

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

        public DataTable loadSalesTax(string SalesId)

        {
            string strSQL;
            try
            {
                strSQL = @"select SalesServiceId,PO.Id PurchaseOrderId, IRT.SalesMaterialId,tg.Code AS TaxCode,IRT.Percentage, IRT.Amount TaxAmount
                              from TRN.Sales PO
                               INNER JOIN trn.SalesMaterial IRD ON IRD.SalesId = PO.Id
                               Inner join trn.SalesTax IRT ON IRT.SalesId = PO.Id and IRT.SalesMaterialId = IRD.Id
                               LEFT OUTER JOIN[MST].[TaxCategory] TG ON tg.Id = IRT.TaxCategoryId
                                 WHERE PO.Id = '" + SalesId + @"'
         and IRT.SalesMaterialId is not null and IRT.SalesServiceId is null
         ORDER BY tg.[Sequence] ";

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
        public DataTable loadOrderMasterTaxes(string SalesId)

        {
            string strSQL;
            try
            {
                strSQL = @"select InventoryServiceId,PO.Id PurchaseOrderId,InventoryReceiveDetailId,tg.Code AS TaxCode,IRT.Percentage, IRT.TaxAmount from TRN.InventoryReceive PO
                               INNER JOIN trn.inventoryReceiveDetail IRD ON IRD.InventoryReceiveId = PO.Id
                               Inner join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveId = PO.Id and IRT.InventoryReceiveDetailId = IRD.Id
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                 WHERE PO.Id='" + SalesId + @"' 
         and InventoryReceiveDetailId is not null and  InventoryServiceId is null 
         ORDER BY tg.[Sequence] ";

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

        public DataTable GetSalesTax(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"Select CONVERT(NUMERIC(10,2),SUM(ISNULL(ST.BooksCurrencyTransactionAmount,0))) BooksCurrencyTransactionAmount
,ST.SalesId,ST.TaxCategoryId,TC.Code TaxCode,CONVERT(NUMERIC(10,2),ST.Percentage)Percentage
,CONVERT(NUMERIC(10,2),SUM(SM.BooksCurrencyTransactionAmount)) TaxON
,TaxAmount=CONVERT(NUMERIC(10,2),(SUM(SM.BooksCurrencyTransactionAmount)*ST.Percentage)/100)
from TRN.SalesTax ST
left join TRN.SalesMaterial SM  ON ST.SalesMaterialId=SM.Id
left join TRN.Sales S ON S.Id=SM.SalesId
LEFT JOIN MST.TaxCategory TC ON TC.Id=ST.TaxCategoryId
Where S.SourceType='Packing' AND ST.SalesId='" + SalesId + @"'
Group By ST.SalesId,ST.TaxCategoryId,TC.Code,ST.Percentage";

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

        public DataTable loadOrderMasterTax(string SalesId)

        {
            string strSQL;
            try
            {
                strSQL = @"select 
                                    PO.SalesId,PO.Id SalesMaterialId,
                                    IRT.Id AS SalesTax,tg.Code AS TaxCode,
                                    s.tocurrencyRate,
                                    IRT.Percentage,
                                    (IRT.Amount * s.tocurrencyRate) as TaxAmount
                                   	,ISNULL(IRT.BooksCurrencyTransactionAmount,0) BooksCurrencyTransactionAmount
									,ISNULL(po.BooksCurrencyTaxAmount,0) BooksCurrencyTaxAmount
									,ISNULL(po.BooksCurrencyBaseRate,0) BooksCurrencyBaseRate

							    from TRN.[SalesMaterial] PO
                               Inner join trn.SalesTax IRT ON IRT.SalesMaterialId = PO.Id 
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
							   left outer join trn.sales as s on s.id=po.salesId
                                 WHERE PO.SalesId='" + SalesId + @"' 
								 and IRT.SalesMaterialId  IS NOT NULL AND  IRT.SalesServiceId IS NULL 
								 ORDER BY tg.[Sequence]";

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

        public DataTable loadSalesMaster(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IOS.Id ServiceId, SM.UserName  Service ,IOS.Amount,IOS.TaxAmount,IOS.AddedBy,IOS.AddedDate,IOS.UpdatedBy,IOS.UpdatedDate 
                            ,IOS.BooksCurrencyTaxAmount,IOS.BooksCurrencyTransactionAmount
                               FROM TRN.Sales   IR
                            INNER join trn.SalesService IOS ON IOS.SalesId = IR.Id
                            INNER JOIN HKP.ServiceMaster SM ON IOS.ServiceMasterId = SM.Id 
                            where IR.Id = '" + SalesId + @"'";

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

        public DataTable loadPackingSalesServiceTaxData(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"Select SM.UserName  Service,CONVERT(NUMERIC(10,2),SUM(ISNULL(ST.BooksCurrencyTransactionAmount,0))) BooksCurrencyTransactionAmount
,ST.SalesId,ST.TaxCategoryId,TC.Code TaxCode,CONVERT(NUMERIC(10,2),ST.Percentage)Percentage
,CONVERT(NUMERIC(10,2),SUM(SS.BooksCurrencyTransactionAmount)) TaxON
,TaxAmount=CONVERT(NUMERIC(10,2),(SUM(SS.BooksCurrencyTransactionAmount)*ST.Percentage)/100)
,TotalAmount=CONVERT(NUMERIC(10,2),SUM(SS.BooksCurrencyTransactionAmount))+CONVERT(NUMERIC(10,2),(SUM(SS.BooksCurrencyTransactionAmount)*ST.Percentage)/100)
from TRN.SalesTax ST
left join TRN.SalesService SS  ON ST.SalesServiceId=SS.Id
INNER JOIN HKP.ServiceMaster SM ON SS.ServiceMasterId = SM.Id 
left join TRN.Sales S ON S.Id=SS.SalesId
LEFT JOIN MST.TaxCategory TC ON TC.Id=ST.TaxCategoryId
Where ST.SalesId='" + SalesId + @"'
Group By ST.SalesId,ST.TaxCategoryId,TC.Code,ST.Percentage,SM.UserName";

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
        public DataTable loadGRNServiceMasterTex(string SalesId)
        {
            string strSQL;
            try
            {
                strSQL = @"select PO.SalesId,PO.Id SalesMaterialId,IRT.Id AS SalesTax,tg.Code AS TaxCode,IRT.Percentage, IRT.Amount TaxAmount
,IRT.BooksCurrencyTransactionAmount BooksCurrencyTaxAmount,po.BooksCurrencyTransactionAmount
								from TRN.[SalesService] PO
                               Inner join trn.SalesTax IRT ON IRT.SalesServiceId = PO.Id 
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                 WHERE PO.SalesId='" + SalesId + @"'
								 and IRT.SalesServiceId  IS NOT NULL AND  IRT.SalesMaterialId IS NULL 
								 ORDER BY tg.[Sequence] ";

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

        class clsStdLib
        {
            public static string passWord = "prodDisplay";
            public clsStdLib()
            {

            }
            public enum mType
            {
                Error,
                Success,
                Information
            }
            public static bool passwordGet = true;
            public static string[] sMonth = new string[] { "<Unselect>", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            public static string DataRankNames(int dayNo)
            {

                if (dayNo <= 0)
                    return "";

                //if (dayNo.ToString().Length > 1)
                //{
                //    string Right = dayNo.ToString().Substring(dayNo.ToString().Length - 2, 2);
                //    if (clsStdLib.dbl(Right) >= 10 && clsStdLib.dbl(Right) <= 20)
                //        return dayNo + "th";
                //}

                string RightString = dayNo.ToString().Substring(dayNo.ToString().Length - 1, 1);
                switch (RightString)
                {
                    case "1":
                        return dayNo + "st";
                    case "2":
                        return dayNo + "nd";
                    case "3":
                        return dayNo + "rd";
                    default:
                        return dayNo + "th";

                }

            }

            #region date related
            public static readonly string dateFormat = "dd-MMM-yyyy";
            public static readonly string sqliteDateFormat = "yyyy-MM-dd";
            public static readonly string AppToDBdateFormat = "yyyy-MM-dd hh:mm:ss";
            public static bool IsDateOK(string strdate)
            {
                try
                {
                    if (strdate.Length != 11)
                    {
                        return false;
                    }
                    if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
                    {
                        return false;
                    }
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            private static bool DateOkCheck(string strdate)
            {
                try
                {
                    System.DateTime myDt = System.Convert.ToDateTime(strdate);
                    return true;
                }
                catch (System.Exception ex)
                {
                    return false;
                }
                finally
                {
                    //
                }
            }// end function
            public static object chk_NullDateData(object dateValue)
            {
                if (DateOkCheck("" + dateValue.ToString()) == false)
                {
                    dateValue = "";
                }

                if (("" + dateValue.ToString()) == "")
                {
                    System.DateTime dt = new System.DateTime(1901, 1, 1);
                    dateValue = (object)dt;
                }
                return (object)dateValue;
            }
            public static System.DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
            {
                string strDate = null;
                dateValue = chk_NullDateData(dateValue);
                strDate = dateValue.ToString();
                if (strDate != "")
                {
                    if (input_date_format.Trim() != "")
                    {
                        if (output_date_format.Trim() != "")
                        {
                            System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo();
                            InputFormat.ShortDatePattern = input_date_format;
                            System.DateTime myDt = System.Convert.ToDateTime(strDate, InputFormat);
                            strDate = myDt.ToString(output_date_format);
                        }
                    }
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static Object DateData_AppToDB(object dateValue, string DB_Level_date_format)
            {
                if (string.IsNullOrEmpty((string)dateValue))
                    return DBNull.Value;

                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString(), DB_Level_date_format).ToString();
                }

                string m = System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);
                return System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);


            }// End of function
            public static System.DateTime DateData_DBToApp(object dateValue)
            {
                string strDate = null;
                strDate = dateValue.ToString();

                System.Globalization.DateTimeFormatInfo myDBDateFormat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
                strDate = DateData_DBToApp(dateValue, myDBDateFormat.ShortDatePattern.ToString()).ToString();
                return System.Convert.ToDateTime(strDate);
            }// End function
            public static System.DateTime DateData_DBToApp(object dateValue, string DB_Level_date_format)
            {
                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format 
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, DB_Level_date_format, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString()).ToString();
                }
                return System.Convert.ToDateTime(strDate);
            }// End of function
            public static String makeBaseBlank(object dateValue)
            {
                System.DateTime dt;
                dt = System.Convert.ToDateTime(dateValue.ToString());
                if (dt.Year == 1901)
                {
                    return "";
                }
                else
                {
                    return dateValue.ToString();
                }
            }// End of function
            ///<summary>
            ///return day difference in integer. 
            ///    Example 1: firstDate[Less Than]lastDate returns positive value
            ///    Example 2: firstDate>lastDate returns negative value
            ///    Example 3: firstDate=lastDate returns 0 [zero]**/
            /// </summary>
            public static int dateDiff(string firstDate, string lastDate)
            {

                int difference = 0;
                try
                {
                    firstDate = Convert.ToDateTime(firstDate).ToString("dd-MMM-yyyy");
                    lastDate = Convert.ToDateTime(lastDate).ToString("dd-MMM-yyyy");

                    if (IsDateOK(firstDate) == false)
                    {
                        Exception ex = new Exception("Invalid [First Date]");
                        throw (ex);
                    }
                    if (IsDateOK(lastDate) == false)
                    {
                        Exception ex = new Exception("Invalid [Last Date]");
                        throw (ex);
                    }
                    DateTime dateFirstDate = Convert.ToDateTime(firstDate);
                    DateTime dateLastDate = Convert.ToDateTime(lastDate);
                    TimeSpan TimeSpan = dateLastDate.Subtract(dateFirstDate);


                    difference = TimeSpan.Days;
                }
                catch (Exception ex)
                {
                    throw (ex);
                }

                return difference;
            }



            public static string getSqliteDate(string standardDate)
            {
                return (Convert.ToDateTime(standardDate).ToString(sqliteDateFormat));
            }
            public static string getStandardDateFromSqliteDate(string SqliteDate)
            {
                if (SqliteDate.Length != 10)
                    return "";
                if (SqliteDate.Split('-').Length != 3)
                    return "";
                //many things to validate 
                //but i have less time :)
                string month = ValidLength(sMonth[Convert.ToInt32(SqliteDate.Split('-')[1])], 3).ToString();


                return SqliteDate.Split('-')[2] + "-" + month + "-" + SqliteDate.Split('-')[0];
            }
            #endregion date related

            #region numeric
            public static bool IsNumeric(string strNumber)
            {
                Double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Length == 0)
                {
                    return false;
                }
                return Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d);
            } // End Function
            public static string GetNumericData(string strNumber)
            {
                double d;
                strNumber = strNumber.Replace(",", "");
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0"; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return strNumber;
                }
                else
                {
                    return "0";
                }
            }// end function
            public static string GetNumericDataInDecimalFormat(string strNumber, int precision)
            {
                if (precision < 1)
                    return strNumber;

                string s_precision = new String('0', precision);

                double d;
                System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
                if (strNumber.Trim() == "")
                { return "0." + s_precision; }
                else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
                {
                    return string.Format("{0:0." + s_precision + "}", d);
                }
                else
                {
                    return "0." + s_precision;
                }
            }// end function
            public static double dbl(string d)
            {
                return Convert.ToDouble(GetNumericData(d));

            }
            public static int Percentage(int total, double percentage)
            {
                return (int)(total * (percentage / 100));

            }
            //validation
            public static void numericValidation(string value, bool isMandatory, bool isInteger, bool negativeAllowed, string fieldName)
            {

                try
                {



                    if (isMandatory == true)
                    {
                        if (value.Trim() == "")
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }
                        if (Convert.ToDouble(GetNumericData(value.Trim())) == 0)
                        {
                            Exception ex = new Exception("please insert [" + fieldName + "]");
                            throw (ex);
                        }

                        if (value.Trim() != "")
                        {
                            if (IsNumeric(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }

                    if (value.Trim() != "")
                    {
                        if (IsNumeric(value.Trim()) == false)
                        {
                            Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                            throw (ex);
                        }
                        if (isInteger == true)
                        {

                            if (isInt(value.Trim()) == false)
                            {
                                Exception ex = new Exception("Number must be integer for the field [" + fieldName + "]");
                                throw (ex);
                            }

                        }
                        if (negativeAllowed == false)
                        {
                            if (Convert.ToDouble(GetNumericData(value.Trim())) < 0)
                            {
                                Exception ex = new Exception("Negative values are not allowed for the field [" + fieldName + "]");
                                throw (ex);
                            }
                        }
                    }



                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }


            }

            ///<summary>
            ///check whether a value is integer or not returns true if integer, 
            ///false if floating or string containing alpahnumeric
            ///</summary>
            public static bool isInt(string num)
            {

                bool isInt;
                int number;
                try
                {
                    isInt = System.Int32.TryParse(num, out number);
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
                return isInt;
            }


            #endregion numeric

            #region string

            public static readonly string excelNegativePOsitiveSign = @"+#,##0.00;-#,##0.00;* ??;@";
            public static readonly string NegativePOsitiveSign = @"+#,##0.00;-#,##0.00;0";
            public static readonly string NumberFormatString = "#,##0.000;(#,##0.000);* ??;@";
            public static readonly string NumberFormatStringFourDecimal = "#,##0.0000;(#,##0.0000);* ??;@";
            public static readonly string NumberFormatStringFiveDecimal = "#,##0.00000;(#,##0.00000);* ??;@";
            public static readonly string NumberFormatStringTwoDecimal = "#,##0.00;(#,##0.00);* ??;@";
            public static readonly string NumberFormatStringTwoDecimalWithZero = "#,##0.00;(#,##0.00)";
            public static readonly string NumberFormatStringInteger = "#,##0;(#,##0);* ??;@";
            public static readonly string NumberFormatStringIntegerWithZero = "#,##0;(#,##0)";
            public static readonly string NumberFormatStringText = "@"; //format cell data as text


            public static object ValidLength(string str)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");

                return (object)removechar.Trim();

            }
            public static object ValidLength(string str, int length)
            {

                string removechar = "";
                if (str.Trim() == "")
                {
                    return (object)Convert.DBNull;
                }
                removechar = str.Trim();
                removechar = removechar.Replace("'", " ");


                int strLen = removechar.Length;
                if (strLen > length)
                    removechar = removechar.Substring(0, length);

                return (object)removechar.Trim();

            }
            public static string FileNameLegalChar(string fileName)
            {
                string illegalChar = @"~`!@#$%^&*=/\|>,<";
                foreach (char c in illegalChar)
                {
                    fileName = fileName.Replace(c.ToString(), " ");
                }

                return fileName;
            }
            private StringCollection getTableColumns(ref DataSet dsLocal)
            {
                StringCollection strcol = new StringCollection();
                for (int COL = 0; COL < dsLocal.Tables[0].Columns.Count; COL++)
                {
                    strcol.Add(dsLocal.Tables[0].Columns[COL].ColumnName.ToUpper());
                }

                return strcol;

            }
            public static string emptyString(string str)
            {
                //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
                if (str == "&nbsp;")
                    str = "";
                if (string.IsNullOrEmpty(str) == true)
                    str = "";


                return str;
            }//this function returns an empty string(not a null) from null or empty '&nbsp;' from the page
            #endregion string


            #region others
            public void copyDataset(DataSet source, ref DataSet destination)
            {
                StringCollection strColDestinationColumns = getTableColumns(ref destination);//upper case
                DataRow drLocal = null;
                for (int ROW = 0; ROW < source.Tables[0].Rows.Count; ROW++)
                {
                    drLocal = destination.Tables[0].NewRow();
                    for (int COL = 0; COL < source.Tables[0].Columns.Count; COL++)
                    {
                        if (strColDestinationColumns.Contains(source.Tables[0].Columns[COL].ToString().ToUpper()))
                        {
                            drLocal[source.Tables[0].Columns[COL].ToString()] = ValidLength(source.Tables[0].Rows[ROW][source.Tables[0].Columns[COL].ToString()].ToString());
                        }
                    }
                    destination.Tables[0].Rows.Add(drLocal);
                }


            }
            public static string GetxlsCol(int intCol)
            {
                //returns excel columns based on column number. tested 1 to 256 column numbers
                try
                {
                    if (intCol < 1 || intCol > 256)
                    {
                        System.Exception ex = new Exception("Invalid Column Value");
                        throw (ex);
                    }
                    intCol = intCol - 1;
                    int intFirstLetter = ((intCol) / 512) + 64;
                    int intSecondLetter = ((intCol % 512) / 26) + 64;
                    int intThirdLetter = (intCol % 26) + 65;
                    char FirstLetter;
                    char SecondLetter;
                    if (intFirstLetter > 64)
                        FirstLetter = (char)intFirstLetter;
                    else
                        FirstLetter = ' ';

                    if (intSecondLetter > 64)
                        SecondLetter = (char)intSecondLetter;
                    else
                        SecondLetter = ' ';

                    char ThirdLetter = (char)intThirdLetter;
                    return string.Concat(FirstLetter, SecondLetter, ThirdLetter).Trim();
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {

                }
            }//returns excel columns based on column number. tested 1 to 256 column numbers
            #endregion others

            public static object RetValidLen(string Data)
            {
                if (string.IsNullOrEmpty(Data))
                    return DBNull.Value;

                return Data;
            }
            public static double sum(string columnName, DataTable dtLocal, string criteria)
            {
                double total = 0;
                DataRow[] dr = dtLocal.Select(criteria);
                foreach (DataRow d in dr)
                {
                    total += dbl(d[columnName].ToString());
                }


                return total;
            }
        }

        #endregion

        #region Sales Return
        public void SalesReturnService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesReturnId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "SalesReturn" + plantId + ".docx";

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

                dsOrderMaster = GetloadLocalSalesReturnMaster(salesReturnId);
                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeLocalSalesReturnService(companyGroupId, companyId, plantId, salesReturnId, document, dsOrderMaster);   // {materialItems}
                                                                                                                                               //var SalesTotal = makeSalerReturnOrderServiceTable(companyGroupId, companyId, plantId, salesReturnId, document, dsOrderMaster);   // {{ServiceItems}}
                                                                                                                                               //var dsInventoryReceiveAdditionalTax = loadSalesReturnAdditionalTax(salesReturnId);


                //var InventoryReceiveAdditionalTax = 0.00;
                //if (dsInventoryReceiveAdditionalTax.Rows.Count > 0)

                //{
                //    InventoryReceiveAdditionalTax = makeSalesReturnTaxTable(document, dsInventoryReceiveAdditionalTax, salesReturnId);//Service Details 
                //    //document.Replace("{ServiceDetails}", "Service Details", true, true);

                //{TotalInWords}
                //}
                document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["BaseCurrencyName"].ToString(), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal), dsOrderMaster.Rows[0]["BaseCurrencyId"].ToString()), true, true);


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
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                var sourceDoc = document.Clone();
                document.Replace("{FileCopyName}", "Original Copy", false, false);
                document.ImportContent(sourceDoc, ImportOptions.KeepSourceFormatting);
                document.Replace("{FileCopyName}", "Duplicate Copy", false, false);
                document.ImportContent(sourceDoc, ImportOptions.KeepSourceFormatting);
                document.Replace("{FileCopyName}", "Triplicate for recipient", false, false);


                //removing any unused place holder  
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
                string Prefix = "SalesReturn" + plantId;

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

        public DataTable GetloadLocalSalesReturnMaster(string salesReturnId)
        {
            string strSQL;
            try
            {


                strSQL = @"SELECT IR.Id CustomerNo, IRD.Id SalesMaterialId
                                 , IR.CompanyGroupId
                                ,IR.CompanyId,CRNC.Code
								,p.UserName Customer
                                , P.UserName Buyer
                                 , ir.CurrencyId
								,cmp.BaseCurrencyId
								,P.TINNO CustomerGSTNo
                                , p.VATResistrationNo as CustomerPANNo
								,Addres.Address1 VendorAddress
                                , ISNULL(HSNC.Code,MHSN.Code) HSNCode
                                 , Plant.GSTIN
								,Plant.VATResistrationNo as PlantPANNo
                                ,DPARTYPL.GSTIN ShipGSTIN
                                , INVPARTYPL.GSTIN BillGSTIN
                                 , IR.DocRefNo
	                            ,IR.InvoiceNo
                                ,REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS DocDate
                                , REPLACE(Convert(VARCHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
                                 , REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                                  , REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
                                   , IR.InvoicingPartyPlantId
		                        ,INVPARTYPL.UserName InvoiceParty
                                , INVPARTYPL.UserName InvoiceParty2
                                 , IR.InvoicingByAddress as ConsigneeAddress
		                        ,IR.DeliveryByAddress
		                        ,DPARTYPL.UserName DeliveryParty
                                , IR.DeliveryPartyPlantId
		                        ,IRD.MaterialMasterId
								,PSI.PreCarriageBy
								,PSI.PlaceOfReceiptByPreCarriage
								,PSI.CNFContainerNo
								,PSI.CNFVesselName
								,PSI.CNFVesselTrackingNo
                                ,LC.LcNo,LC.BenificiaryBank,LC.OpeningBank
								,FORMAT(LC.LCDate, 'dd-MMM-yyyy')LCDate
                                ,LC.BenificiaryBankDescription
                                ,LC.OpeningBankAddress
								,D.UserName as FinalDestination
								,PL.UserName as PortOfLanding
								,PD.UserName as PortOfDischarge
								,PoD.UserName as PortOfDelivery
	                            ,CRNC.Code AS CurrencyName
	                            ,IR.ToCurrencyRate
		                        ,BASECRNC.Code AS BaseCurrencyName
		                        ,PayTerm.UserName PaymentTerm
                              , MM.UserName MaterialMaster
                               , MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
                              , MMA.StandardName Article
                               , FC.UserName FirstChar
                                , FCV.UserName AS FirstCharacteristicsValue
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.UserName SecondChar
                              , TC.UserName ThirdChar
                               , ROUND(IRD.TransactionQty, 2) POTransactionQty
	                          ,ROUND(IRD.TransactionRate, 4) TransactionRate
	                          ,ROUND((IRD.TransactionQty * IRD.TransactionRate), 2) AS TrnAmount
                              , IRD.BaseAmount
	                          ,IRD.TaxAmount AS BaseTaxAmount
	                          ,TaxAmount = (
                                    SELECT SUM(TaxAmount)

                                    FROM[TRN].[PurchaseOrderTax]

                                    WHERE InventoryReceiveDetailId = IRD.Id
		                            )
	                          ,ServiceTaxAmount = (
                                    SELECT SUM(TaxAmount)

                                    FROM[TRN].[SalesService]

                                    WHERE SalesId = IRD.Id
		                            )
	                          ,TUoM.UserName AS TransactionUoM
							  ,PONumber = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + CPO.PONumber FROM
                                        TRN.SalesMaterial SM

                                        JOIN TRN.SalesOrder SO ON SO.Id = SM.SalesOrderId

                                        JOIN TRN.CustomerPO CPO ON CPO.id = SO.CustomerPOId
                                        WHERE IR.Id = SM.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
							  ,OurOrderRefNo = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + MO.OwnReferenceNo FROM
                                        TRN.SalesOrderItem SOI

                                        JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                                        WHERE IR.Id = SOI.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
							,YourOrderRefNo = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + MO.BuyerReferenceNo FROM
                                        TRN.SalesOrderItem SOI

                                        JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                                        WHERE IR.Id = SOI.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
										,AddedDate = REPLACE(REPLACE(
                                        STUFF((select distinct ', ' + FORMAT(MO.AddedDate, 'dd-MMM-yyyy') FROM
                                        TRN.SalesOrderItem SOI

                                        JOIN TRN.MasterOrder MO ON MO.Id = SOI.MasterOrderId
                                        WHERE IR.Id = SOI.SalesId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
                                ,IR.ComercialInvoiceNo,IR.BLNumber,FORMAT(IR.BLDate, 'dd-MMM-yyyy')BLDate,
								IR.EXPFromNo,FORMAT(IR.EXPDate, 'dd-MMM-yyyy')EXPDate,IR.ItemDescription
								,PSI.TransportVehicleNo,PSI.TransportDriverName,PPSI.UserName TransporterName, BM.AccountTitle,BM.AccountNumber
								,BMA.Address1,PSI.TransportDocRefNo,FORMAT(PSI.TransportDocDate, 'dd-MMM-yyyy') CNFBLAWBDate
								,B.UserName as Bank,BB.UserName as BankBranch
								,IRD.BooksCurrencyTransactionAmount
								,IRD.BooksCurrencyTaxAmount
								,IRD.BooksCurrencyBaseRate
                        ,(Select Stuff((
						Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
						from dbo.ProductLibraryAttribute pla
						LEFT JOIN dbo.SalesPacking SP ON pla.ProductLibraryId = SP.ProductLibraryId
						WHERE SP.SalesId=IR.Id
						for XML PATH('')
						) , 1, 2, '')) as ProdDetails,IR.AddedBy CreatedBy,FORMAT(SR.SalesReturnDate,'dd-MMM-yyyy')SalesReturnDate,SR.Id SalesReturnNo
                        FROM TRN.SalesReturn SR
						 left join TRN.Sales IR on IR.Id=SR.SalesId
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN dbo.PostSalesInvoice PSI ON PSI.SalesId = IR.Id
                         LEFT JOIN MST.[Port] as PL on PL.Id = PSI.PortOfLoadingId
                         LEFT JOIN MST.[Port] as PD on PD.Id = PSI.PortOfDischargeId
                         LEFT JOIN MST.[Port] as PoD on PoD.Id = PSI.PortOfDelivaryId
                         LEFT JOIN MST.Destination as D on D.Id = PSI.FinalDestinationId
                         LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                         LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = cmp.BaseCurrencyId
                         LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                         LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                         LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
                         LEFT JOIN HKP.Party P ON P.Id = IR.PartyId
                         LEFT JOIN[MST].[AddressMaster] Addres ON Addres.Id = P.AddressMasterId
                         LEFT JOIN trn.SalesReturnDetail AS IRD ON IRD.SalesReturnId = SR.Id
                         LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IRD.MaterialMasterId
                         LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IRD.ArticleId
                         LEFT JOIN[HKP].[HSNCode] AS MHSN ON MHSN.ID = MM.HSNCodeId
                         LEFT JOIN[HKP].[HSNCode] AS HSNC ON HSNC.ID = MMA.HSNCodeId
                         LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId = TCV.Id
                         LEFT JOIN[SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                         LEFT JOIN HKP.Party PPSI ON PPSI.Id = PSI.TransportAgentId
                         LEFT JOIN MST.BankMaster BM ON BM.Id = IR.PaymentToReceiveBankId
                         LEFT JOIN HKP.Bank B ON B.Id = BM.BankId
                         LEFT JOIN HKP.BankBranch BB ON BB.BankId = BM.BankId And BB.Id = BM.BankBranchId
                         LEFT JOIN[MST].[AddressMaster] BMA ON BMA.Id = BB.AddressMasterId
                         LEFT JOIN(
                         select distinct
                         PLC.LCRef as LcNo,PLC.LCDate,PLC.BenificiaryBank,PLC.BenificiaryBankDescription
						 ,B.UserName OpeningBank, SOI.SalesId
						 ,OA.Address1 OpeningBankAddress
                         from trn.SalesOrderItem as SOI
                         LEFT JOIN TRN.MasterOrderItem MOI on MOI.Id = SOI.MasterOrderItemId
                         LEFT JOIN TRN.SalesOrder SO on MOI.Id = SO.MasterOrderItemId
                         LEFT JOIN dbo.[Contract]  C on c.Id = SO.ContractId
                         LEFT JOIN dbo.PurchaseLC PLC on PLC.ContractId = C.Id
                         LEFT JOIN  MST.BankMaster OB on OB.Id = PLC.OpeningBankMasterId
                         LEFT JOIN  HKP.Bank B on B.Id = OB.BankId
                         LEFT JOIN MST.AddressMaster OA on OA.Id = B.AddressMasterId						
						 ) LC on LC.SalesId = IR.Id
                         WHERE SR.Id ='" + salesReturnId + "'";

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

        public double makeLocalSalesReturnService(string companyGroupId, string companyId, string plantId, string salesReturnId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{materialItems}";

            DataTable sales, materialTax;
            //Sales== Master Query
            sales = GetloadLocalSalesReturnMaster(salesReturnId);
            materialTax = loadSalesReturnMasterTax(salesReturnId);

            int LasColumnIndex = 9;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(materialTax.DefaultView.ToTable(true, "TaxCode"));


            for (int i = 0; i < dv.Count; i++)
            {
                LasColumnIndex++;
                dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                LasColumnIndex++;
            }


            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 110;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 110;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("BuyerRef#");
            range.ApplyCharacterFormat(FontBold);
            int colBuyerRef = COL; COL++;
            wTable.Rows[ROW].Cells[colBuyerRef].Width = 80;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("PONumber");
            range.ApplyCharacterFormat(FontBold);
            int colPONumber = COL; COL++;
            wTable.Rows[ROW].Cells[colPONumber].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 45;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 50;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UoM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL++;
            wTable.Rows[ROW].Cells[colUoM].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL;
            wTable.Rows[ROW].Cells[colRate].Width = 50;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount " + "(" + " " + sales.Rows[0]["BaseCurrencyName"].ToString() + " " + ")" + " ");
                wTable.Rows[ROW].Cells[colTotalTaxableAmount].Width = 100;
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        //two columns required for tax
                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                        range.ApplyCharacterFormat(FontBold);

                        COL++;
                        range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
                range.ApplyCharacterFormat(FontBold);
            }


            if (dv.Count > 0)
            {
                wTable.Rows.Add(TemplateRow);
                ROW++;
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                for (int i = 0; i < dv.Count; i++)
                {
                    try
                    {
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate(%)");
                        range.ApplyCharacterFormat(FontBold);
                        range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                        range.ApplyCharacterFormat(FontBold);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }



            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
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
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colBuyerRef].AddParagraph().AppendText(dsOrderMaster.Rows[i]["YourOrderRefNo"].ToString());
                TROW.Cells[colPONumber].AddParagraph().AppendText(dsOrderMaster.Rows[i]["PONumber"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString() + "-" + dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString() + "-" + dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());

                //TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                //TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                //TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colHSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString());
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["BooksCurrencyBaseRate"].ToString()).ToString("#,##0.0000"));
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["BooksCurrencyTransactionAmount"].ToString()).ToString("#,##0.00"));


                //totalValue += clsStdLib.dbl(sales.Rows[i]["TrnAmount"].ToString());

                if (dv.Count > 0)
                {
                    DataView dvtax = new DataView(materialTax.DefaultView.ToTable());

                    for (int T = 0; T < dv.Count; T++)
                    {
                        //dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "'";
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' And SalesMaterialId = '" + dsOrderMaster.Rows[i]["SalesMaterialId"].ToString() + "' ";

                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("#,##0.00"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["BooksCurrencyTransactionAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
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
                if (C == colArticle || C == colBuyerRef || C == colPONumber || C == colHSN || C == colUoM || C == colRate || C == colChar1 || dicTaxes.ContainsValue(C))
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
            #endregion Total

            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString())
                    //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                    + clsStdLib.dbl(materialTax.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 30;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 30 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


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

            return total;
        }
        public DataTable loadSalesReturnMasterTax(string salesReturnId)
        {
            string strSQL;
            try
            {
                strSQL = @"select  PO.SalesId,PO.Id SalesMaterialId, IRT.Id AS SalesTax,tg.Code AS TaxCode,
                                    S.ToCurrencyRate, IRT.Percentage, (IRT.Amount ) as TaxAmount
                                   	,ROUND(ISNULL(IRT.Amount* s.tocurrencyRate,0),2) BooksCurrencyTransactionAmount
									,ISNULL(po.BooksCurrencyTaxAmount,0) BooksCurrencyTaxAmount
									,ISNULL(po.BooksCurrencyBaseRate,0) BooksCurrencyBaseRate

							    from TRN.[SalesReturnDetail] PO
                               Inner join trn.SalesReturnTax IRT ON IRT.SalesReturnDetailId = PO.Id 
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
							   left outer join trn.sales as S on S.id=po.salesId
                                 WHERE PO.SalesReturnId='" + salesReturnId + @"'
								 and IRT.SalesReturnDetailId  IS NOT NULL
								 ORDER BY tg.[Sequence]";

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
        public double makeSalerReturnOrderServiceTable(string companyGroupId, string companyId, string plantId, string salesReturnId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{ServiceItems}";

            ReportUtility ru = new ReportUtility();

            DataTable salesService, serviceTax;

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;



            salesService = loadSalesReturnMaster(salesReturnId);
            if (salesService.Rows.Count == 0)
            {
                document.Replace("{ServiceCaption}", "", false, false);
                document.Replace(replaceString, "", false, false);
                return 0;

            }
            document.Replace("{ServiceCaption}", "Serive Details", false, false);
            serviceTax = loadSalesReturnGRNServiceMasterTex(salesReturnId);

            int LasColumnIndex = 2;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(serviceTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Service Name");
            range.ApplyCharacterFormat(FontBold);
            int colServiceGroup = COL; COL++;

            wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxable Amount");
                range.ApplyCharacterFormat(FontBold);
                //COL++;
                for (int i = 0; i < dv.Count; i++)
                {
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText(dv[i]["TaxCode"].ToString());
                    COL++;
                    range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("");
                }
            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Amount");
                range.ApplyCharacterFormat(FontBold);
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;

            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {

                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()]].AddParagraph().AppendText("Rate");
                    range.ApplyCharacterFormat(FontBold);


                    range = wTable.Rows[ROW].Cells[dicTaxes[dv[i]["TaxCode"].ToString()] + 1].AddParagraph().AppendText("Amount");
                    range.ApplyCharacterFormat(FontBold);

                }
            }
            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < salesService.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }
                TROW.Cells[colServiceGroup].AddParagraph().AppendText(salesService.Rows[i]["Service"].ToString());
                //TROW.Cells[colServiceGroup].AddParagraph().AppendText(salesService.Rows[i]["HSNCode"].ToString());
                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(salesService.Rows[i]["BooksCurrencyTransactionAmount"].ToString()).ToString("#,##0.00"));

                totalValue += clsStdLib.dbl(salesService.Rows[i]["Amount"].ToString());

                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(totalValue.ToString("F2"));

                if (dv.Count > 0)
                {
                    //dsTax.Tables[0].DefaultView.RowFilter = "MasterOrderItemId='" + dsOrderItems.Tables[0].Rows[i]["MasterOrderItemId"].ToString() + "'";
                    DataView dvtax = new DataView(serviceTax.DefaultView.ToTable());
                    //double totalTax = 0;

                    for (int T = 0; T < dv.Count; T++)
                    {
                        //dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "'";
                        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND SalesMaterialId='" + serviceTax.Rows[i]["SalesMaterialId"].ToString() + "'";
                        if (dvtax.Count > 0)
                        {
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("#,##0.00"));
                            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["BooksCurrencyTaxAmount"].ToString()).ToString("#,##0.00"));
                        }
                    }
                }
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colHSN || dicTaxes.ContainsValue(C))
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
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            double total = clsStdLib.dbl(salesService.Compute("SUM(BooksCurrencyTransactionAmount)", "").ToString())
                   //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
                   + clsStdLib.dbl(serviceTax.Compute("SUM(BooksCurrencyTaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable


            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("ServiceStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 120;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("ServiceStyle");
                    }
                }
            }

            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            for (int i = 0; i < dv.Count; i++)
                wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            for (int i = 0; i <= colTotalTaxableAmount; i++)
                wTable.ApplyVerticalMerge(i, ROW - 1, ROW);

            IWParagraphStyle style = document.AddParagraphStyle("ServiceSubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("ServiceSubTotalStyle");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return total;
        }
        public DataTable loadSalesReturnMaster(string salesReturnId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT IOS.Id ServiceId, SM.UserName  Service ,IOS.Amount,IOS.TaxAmount,IOS.AddedBy,IOS.AddedDate,IOS.UpdatedBy,IOS.UpdatedDate 
                            ,IOS.BooksCurrencyTaxAmount,IOS.BooksCurrencyTransactionAmount
                            FROM TRN.SalesReturn SR
						    left join TRN.Sales IR on IR.Id=SR.SalesId
                            INNER join trn.SalesService IOS ON IOS.SalesId = IR.Id
                            INNER JOIN HKP.ServiceMaster SM ON IOS.ServiceMasterId = SM.Id 
                            where SR.Id = '" + salesReturnId + @"'";

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
        public DataTable loadSalesReturnGRNServiceMasterTex(string salesReturnId)
        {
            string strSQL;
            try
            {
                strSQL = @"select PO.SalesId,PO.Id SalesMaterialId,IRT.Id AS SalesTax,tg.Code AS TaxCode,IRT.Percentage, IRT.Amount TaxAmount
,IRT.BooksCurrencyTransactionAmount BooksCurrencyTaxAmount,po.BooksCurrencyTransactionAmount
								from TRN.[SalesService] PO
                               Inner join trn.SalesTax IRT ON IRT.SalesServiceId = PO.Id 
                               LEFT OUTER JOIN [MST].[TaxCategory] TG ON tg.Id=IRT.TaxCategoryId
                                 WHERE PO.SalesId='" + salesReturnId + @"'
								 and IRT.SalesServiceId  IS NOT NULL AND  IRT.SalesMaterialId IS NULL 
								 ORDER BY tg.[Sequence] ";

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

        public DataTable loadSalesReturnAdditionalTax(string salesReturnId)
        {
            string strSQL;

            try
            {
                strSQL = @"select TxC.UserName Taxname,SA.Id,SA.TaxCodeId as TaxCode,SA.BooksCurrencyTaxAmount,SA.Percentage
						from TRN.SalesAdditionalTax SA
						left join TRN.Sales as S on S.Id=SA.SalesId
						left join MST.TaxCode as TxC on TxC.id = SA.TaxCodeId
                        where S.Id='" + salesReturnId + "'";

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
        public double makeSalesReturnTaxTable(WordDocument document, DataTable dsOrderMaster, string salesReturnId)
        {
            string replaceString = "{TaxCollectedAtSource}";

            ReportUtility ru = new ReportUtility();

            DataTable dsTax;
            //clsDataContext data = new clsDataContext();

            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign1");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;


            dsTax = loadSalesReturnAdditionalTax(salesReturnId);


            int LasColumnIndex = 1;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            if (dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    LasColumnIndex++;
                    dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
                    //LasColumnIndex++;
                }
            }

            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();


            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;
            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Taxname");
            range.ApplyCharacterFormat(FontBold);
            int colTaxname = COL; COL++;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Percentage");
            range.ApplyCharacterFormat(FontBold);
            int colPercentage = COL;

            int colTotalTaxableAmount = COL;
            if (dv.Count > 0)
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Tax Amount");
                range.ApplyCharacterFormat(FontBold);

            }
            else
            {
                COL++;
                colTotalTaxableAmount = COL;
                range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            }

            wTable.Rows.Add(TemplateRow);
            ROW++;


            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                //ROW++;
                //wTable.AddRow();
                WTableRow TROW = wTable.LastRow;


                IParagraphItem p = TROW.Cells[colTaxname].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Taxname"].ToString());
                TROW.Cells[colPercentage].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["Percentage"].ToString()).ToString("#,##0.0000"));

                TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["BooksCurrencyTaxAmount"].ToString()).ToString("#,##0.00"));
            }

            #region Sub Total


            double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(BooksCurrencyTaxAmount)", "").ToString());

            #endregion Total


            //ROW++;

            #region Total Payable
            #endregion Total Payable

            //ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle3 = document.AddParagraphStyle("MyStyle3");
            //Sets the formatting of the style
            myStyle3.CharacterFormat.FontSize = 8f;
            myStyle3.CharacterFormat.TextColor = Color.Black;
            myStyle3.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 35;
                if (dv.Count < 3)
                    TROW.Cells[0].Width = +((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle3");
                    }
                }
            }


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            int k = document.Replace(replaceString, textBodyPart, false, false);
            return total;
        }

        #endregion Sales Return
    }
}

