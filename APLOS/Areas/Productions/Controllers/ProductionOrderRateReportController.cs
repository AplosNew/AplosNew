#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.OrderManagement.Production;
using Library.Crosscutting.Security;
using System.Data;
using Library.Security.Core;
using System.Threading;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using System.Drawing;
using OTSBD;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionOrderRateReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public ProductionOrderRateReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        #region --- Daily Day Status Report---
        [HttpPost, Authorize]
        public JsonResult RReport(string FromDate, string ToDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var workbook = RateReport(FromDate, ToDate);
                return Json(new { FileName = workbook, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string RateReport(string FromDate, string ToDate)
        {
            #region Variable
            ReportUtility oru = new ReportUtility();
            string yot = string.Empty;//OTConsiderOn
            string tot = string.Empty;//OTConsiderOn
            DataView dvAttn = null;
            DataSet dsFactory = null;
            DataSet dslocal = null;
            DataSet dsCmp = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            var report = new ReportUtility();
            clsReport objRpt = null;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                #region DataSet
                GetReport(FromDate, ToDate, out dslocal);
                objRpt = new clsReport();
                dvAttn = new DataView();
                dvAttn.Table = dslocal.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId.Trim(), out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                if (dvAttn.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 7;
                    int intRow = 0;

                    int strCount = 0;
                    #region ------------------Column Header------------------
                    xlsCol = 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Sl No.";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Production Order No.";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Entity";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Material Master";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Article";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Production Status";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;

                    sheet1.Range[xlsRow, xlsCol].Text = "Product";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Product Category";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Buyer";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Customer";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Buyer Order";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int p = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Own Order";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int q = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Buyer Item";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 9;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int y = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Own Item";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    int yo = xlsCol;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "PO Number";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "SO No";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Rate";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Qty";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Amount";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Currency";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "SKU1";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "SKU2";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 13;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    endXlsCol = xlsCol;
                    xlsCol = 1;
                    xlsRow += 1;
                    #endregion ------------------Column Header------------------
                    //strCount = 0;
                    for (int i = 0; i < dvAttn.Count; i++)
                    {

                        xlsCol = 1;
                        #region ----------------------Data-----------------------

                        strCount += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = strCount;
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["POId"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Entity"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["MaterialMaster"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Article"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ProductionStatus"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;

                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Product"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ProductCategory"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Buyer"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Customer"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["BuyerOrder"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["OwnOrder"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["BuyerItem"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["OwnItem"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["PONumber"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;




                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["SONo"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Rate"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = OTSBD.clsStaticInfo.dbl(dvAttn[i]["PoQty"].ToString());
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = OTSBD.clsStaticInfo.dbl(dvAttn[i]["Amount"].ToString());
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["Currency"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["FirstCharacteristics"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["SeceondCharacteristics"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsRow += 1;

                        #region Line Setup
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].WrapText = true;
                        #endregion


                        #endregion ----------------------Data-----------------------

                    }

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), identity.CompanyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);


                        }


                    }
                    catch (Exception)
                    {

                    }

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    string FactoryAddress = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = CmpName;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 30;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                        //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = FactoryName;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 26;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "Production Order Rate Report";
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "From Date:- " + FromDate + " To Date:- " + ToDate;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 9;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 7;
                    #endregion

                    #region Page Setup

                    sheet1.Name = "RateReport";
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    sheet1.PageSetup.PrintTitleRows = "$1:$5";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    #endregion             

                    workbook.Version = ExcelVersion.Excel97to2003;
                    report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);

                    var filePath = "";
                    var SheetName = "";
                    //return workbook;
                    workbook.Version = ExcelVersion.Excel97to2003;
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
                    workbook.SaveAs(filePath);
                    workbook.Close();
                    excelEngine.Dispose();
                    return filePath;
                }
                else
                {
                    throw new Exception("No Data found...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        public void GetReport(string FromDate, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {

                strSql = @"SELECT Ma.Id, SKUId=case when Ma.SelectedDropDownValue is null then '' else Ma.SelectedDropDownValue end,
                                    PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, sum(PD.Qty)Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity,PD.Article,PD.MaterialMaster
									,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,popd.Rate,isnull(ss.Quantity,0) PoQty,(popd.Rate*isnull(ss.Quantity,0)) Amount,Currency= STUFF((select distinct ','+c.Code from 
																SCS.Currency c 
																JOIN TRN.MasterOrder m ON m.CurrencyId=c.Id
																JOIN TRN.MasterOrderItem mi ON mi.MasterOrderId=m.Id
																JOIN TRN.SalesOrder s ON s.MasterOrderItemId=mi.Id
																JOIN TRN.ProductionOrderDetail p ON p.SalesOrderId=s.Id
																where p.ProductionOrderId=po.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,fcv.UserName FirstCharacteristics,scv.UserName SeceondCharacteristics

								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory,SO.Qty
                                        ,mm.UserName MaterialMaster,mma.StandardName Article								   
								        ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
							

								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                   LEFT JOIN MST.MaterialMasterArticle mma on mma.Id=MOI.ArticleId 
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id									
                                    left join ProductionOrderProcessWithRateMaster Ma on Ma.ProductionOrderId =PO.Id
									left join ProductionOrderProcessWithRateDetails popd on popd.ProductionOrderProcessWithRateMasterId = ma.Id
									left join HKP.Characteristics fc on fc.Id = popd.FirstCharacteristicsId 
									left join HKP.Characteristics sc on sc.Id = popd.SecondCharacteristicsId
									left join HKP.CharacteristicsValue fcv on fcv.Id = popd.FirstCharacteristicsValueId 
									left join HKP.CharacteristicsValue scv on scv.Id = popd.SecondCharacteristicsValueId 
									left join TRN.ProductionSummary ss on ss.ProductionOrderId = ma.ProductionOrderId and ma.ProcessId=ss.ProcessId								  
									WHERE	
								   --PO.Id IN(select ProductionOrderId  from [TRN].[ProductionOrderProcessSet] Pr where Pr.EntityIdWithinCompany=Ma.ProductionEntityId and ProcessId=ma.ProcessId)
								   --and
								   PS.StandardName in ('Active', 'Running') and ISNULL(ma.Id,'')<>'' and ss.ProductionDate between '" + FromDate + "' and '" + ToDate + @"'
                                   group by Ma.Id, PO.Id ,PS.UserName,PO.RequiredTimeUnit,LSD,CommitmentDate,PD.Product, PD.ProductCategory
								   ,PD.Buyer,PD.Customer, PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId
								   ,E.UserName,Ma.SelectedDropDownValue,PD.MaterialMaster,PD.Article ,popd.Rate,ss.Quantity,fcv.UserName,scv.UserName
                                   order by PO.Id";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSql, out dsRef);
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        #endregion

    }
}