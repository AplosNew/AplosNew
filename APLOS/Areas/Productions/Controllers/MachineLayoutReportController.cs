#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;
using System;
using OTSBD;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using Syncfusion.XlsIO;
using System.IO;
using Library.Service.Helpers;
using System.Linq;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class MachineLayoutReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public MachineLayoutReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpPost, Authorize]
        public ActionResult Report(string EntityId, string ProcessId, string ProductionDate, string WorkCenterMasterId, Dictionary<string, object> Data)
        {
            #region Variable
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsReport objRpt = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            string FactoryName = "";
            string CmpName = "";
            string companyId = identity.CompanyId;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            DateTime dtFrmDt = DateTime.Now;
            DateTime dtEndDate = DateTime.Now;
            string MonthName = string.Empty;
            #endregion Variable

            try
            {
                #region DataSet
                GetData(EntityId, ProcessId, ProductionDate, WorkCenterMasterId, out DataSet dsMasterData);
                SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                SelectedPlant(identity.PlantId, out dsFactory);
                #endregion DataSet
                if (dsMasterData.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("No data found");
                }
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                xlsRow = 6;
                #region Variables
                int strCount = 0;
                int iTGT = 0;
                int iOperation = 0;
                int iSMV = 0;
                int iTOMC = 0;
                int iUnit = 0;
                int iTOMC2 = 0;
                int iSMV2 = 0;
                int iOperation2 = 0;
                int iTG2 = 0;
                #endregion Variables

                #region ------------------Column Header------------------

                xlsCol = 1;
                xlsRow = 6;
                int HeaderStartRow = xlsRow;
                sheet1.Range[xlsRow, xlsCol].Text = "Buyer";
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol + 2].Text = Data["Buyer"].ToString().Trim();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 6].Merge();

                xlsCol = 1;
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Text = "Style";
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol + 2].Text = Data["BuyerItemNo"].ToString().Trim();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 6].Merge();

                xlsCol = 1;
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Item";
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol + 2].Text = Data["Article"].ToString().Trim();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 6].Merge();

                xlsCol = 1;
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Colour";
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol + 2].Text = "" /*+ Data["Colour"].ToString().Trim()*/;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 6].Merge();

                xlsCol = 1;
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                xlsRow += 1;
                xlsCol = 9;
                xlsRow = 6;

                sheet1.Range[xlsRow, xlsCol].Text = "Date";
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol + 2].Text = ProductionDate.ToString().Trim();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 4].Merge();
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Total SMV";
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol + 2].Text = Data["SMV"].ToString().Trim();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 2, xlsRow, xlsCol + 4].Merge();

                xlsRow += 1;
                int StartCalRow = xlsRow;
                int endCalCol = xlsCol + 2;
                sheet1.Range[xlsRow, xlsCol].Text = "Operators";
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol + 2].Text = Data["ManPowerWithMachine"].ToString().Trim();
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Helpers";
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol + 2].Text = Data["ManPowerWithHand"].ToString().Trim();
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsRow += 1;
                xlsCol = 1;
                xlsRow += 1;

                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsRow += 1;
                xlsCol = 14;
                xlsRow = 6;

                sheet1.Range[xlsRow, xlsCol].Text = "M/C-SPT";
                sheet1.Range[xlsRow, xlsCol + 1].Text = "Manual-SMV";
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsRow += 1;
                int CalRow = xlsRow;int CalCol = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "3.78";
                sheet1.Range[xlsRow, xlsCol + 1].Text = " 1.27 " /*+ Data["SPT"].ToString().Trim()*/;
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol - 2].Text = "Line";
                sheet1.Range[xlsRow, xlsCol - 1].Text = Data["Line"].ToString().Trim();
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsRow += 1;
                int rows = xlsRow;int cols = xlsCol - 1;
                sheet1.Range[xlsRow, xlsCol - 2].Text = "TGT 100% :PCs";
                sheet1.Range[xlsRow, xlsCol - 1].Formula = "(" + 60 +"/"+  clsStaticInfo.GetxlsCol(CalCol) + CalRow + ") * " + clsStaticInfo.GetxlsCol(endCalCol) + (StartCalRow) + "";
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet1.Range[xlsRow, xlsCol].Text = "TGT 85% :PCs";
                sheet1.Range[xlsRow, xlsCol + 1].Text = "333" /*+ Data["Operators"].ToString().Trim()*/;
                sheet1.Range[xlsRow, xlsCol + 1].Formula = "(" +clsStaticInfo.GetxlsCol(cols) + rows + ") * 0.85";
                sheet1.Range[xlsRow, xlsCol + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet1.Range[HeaderStartRow, 1, xlsRow, xlsCol + 1].BorderAround(ExcelLineStyle.Thin);
                sheet1.Range[HeaderStartRow, 1, xlsRow, xlsCol + 1].BorderInside(ExcelLineStyle.Thin);
                sheet1.Range[HeaderStartRow, 1, xlsRow, xlsCol + 1].CellStyle.Font.Bold = true;
                sheet1.Range[HeaderStartRow, 1, xlsRow, xlsCol + 1].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(255, 218, 185);

                #endregion ------------------Column Header------------------

                #region ------------------Details Header-----------------

                xlsRow = 11;
                xlsCol = 1;
                iTGT = xlsCol;
                sheet1.Range[xlsRow, iTGT].Text = "TGT";
                //sheet1.Range[xlsRow, iTGT].ColumnWidth = 16;
                sheet1.Range[xlsRow, iTGT].RowHeight = 40;
                sheet1.Range[xlsRow, iTGT].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, iTGT].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iTGT].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, iTGT, xlsRow, iTGT].Merge();

                xlsCol = 2;
                iOperation = xlsCol;
                sheet1.Range[xlsRow, iOperation].Text = "Operation Name";
                // sheet1.Range[xlsRow, iOperation].ColumnWidth = 35;
                //sheet1.Range[xlsRow, iTGT].RowHeight = 30;
                sheet1.Range[xlsRow, iOperation].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iOperation].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, iOperation, xlsRow, iOperation + 3].Merge();
                xlsCol = 6;
                iSMV = xlsCol;
                sheet1.Range[xlsRow, iSMV].Text = "SMV";
                //sheet1.Range[xlsRow, iSMV].ColumnWidth = 16;
                //sheet1.Range[xlsRow, iTGT].RowHeight = 30;
                sheet1.Range[xlsRow, iSMV].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iSMV].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, iSMV, xlsRow, iSMV].Merge();

                xlsCol = 7;
                iTOMC = xlsCol;
                sheet1.Range[xlsRow, iTOMC].Text = "Type Of MC";
                //sheet1.Range[xlsRow, iTOMC].ColumnWidth = 16;
                //sheet1.Range[xlsRow, iTGT].RowHeight = 30;
                sheet1.Range[xlsRow, iTOMC].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iTOMC].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, iTOMC, xlsRow, iTOMC].Merge();

                xlsCol = 8;
                iUnit = xlsCol;
                //sheet1.Range[xlsRow, iUnit].Text = "Center Table";
                //sheet1.Range[xlsRow, iUnit].ColumnWidth = 19;
                //sheet1.Range[xlsRow, iTGT].RowHeight = 30;
                sheet1.Range[xlsRow, iUnit].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //sheet1.Range[xlsRow, iOperation, xlsRow, iOperation + 2].Merge();
                xlsCol = 9;
                iTOMC2 = xlsCol;
                sheet1.Range[xlsRow, iTOMC2].Text = "Type Of MC";
                //sheet1.Range[xlsRow, iTOMC2].ColumnWidth = 16;
                //sheet1.Range[xlsRow, iTGT].RowHeight = 30;
                sheet1.Range[xlsRow, iTOMC2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iTOMC2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, iTOMC2, xlsRow, iTOMC2].Merge();

                xlsCol = 10;
                iSMV2 = xlsCol;
                sheet1.Range[xlsRow, iSMV2].Text = "SMV";
                //sheet1.Range[xlsRow, iSMV2].ColumnWidth = 16;
                //sheet1.Range[xlsRow, iTGT].RowHeight = 30;
                sheet1.Range[xlsRow, iSMV2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iSMV2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, iSMV2, xlsRow, iSMV2].Merge();

                xlsCol = 11;
                iOperation2 = xlsCol;
                sheet1.Range[xlsRow, iOperation2].Text = "Operation Name";
                //sheet1.Range[xlsRow, iOperation2].ColumnWidth = 35;
                //sheet1.Range[xlsRow, iTGT].RowHeight = 30;
                sheet1.Range[xlsRow, iOperation2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iOperation2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, iOperation2, xlsRow, iOperation2 + 3].Merge();

                xlsCol = 15;
                iTG2 = xlsCol;
                sheet1.Range[xlsRow, iTG2].Text = "TGT";
                //sheet1.Range[xlsRow, iTG2].ColumnWidth = 16;
                sheet1.Range[xlsRow, iTGT].RowHeight = 40;
                sheet1.Range[xlsRow, iTG2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iTG2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, iTG2, xlsRow, iTG2].Merge();


                #endregion ------------------Details Header-----------------

                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Thin);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;


                endXlsCol = xlsCol;
                xlsCol = 1;
                xlsRow += 1;


                int startrow = xlsRow - 1;
                for (int i = 0; i <= dsMasterData.Tables[0].Rows.Count - 1; i++)
                {
                    //xlsCol = 1;

                    #region ----------------------Data-----------------------                    

                    sheet1.Range[xlsRow, iTGT].Text = dsMasterData.Tables[0].Rows[i]["WorkstationTargetPerHour"].ToString().Trim();
                    sheet1.Range[xlsRow, iTGT].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iTGT].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iTGT, xlsRow, iTGT].Merge();
                    sheet1.Range[xlsRow, iTGT].RowHeight = 40;

                    sheet1.Range[xlsRow, iOperation].Text = dsMasterData.Tables[0].Rows[i]["OperationVariationName"].ToString().Trim();
                    sheet1.Range[xlsRow, iOperation].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iOperation].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iOperation, xlsRow, iOperation + 3].Merge();

                    sheet1.Range[xlsRow, iSMV].Text = dsMasterData.Tables[0].Rows[i]["TotalSPT"].ToString().Trim();
                    sheet1.Range[xlsRow, iSMV].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iSMV].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iSMV, xlsRow, iSMV].Merge();

                    sheet1.Range[xlsRow, iTOMC].Text = dsMasterData.Tables[0].Rows[i]["MACHINE"].ToString().Trim();
                    sheet1.Range[xlsRow, iTOMC].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iTOMC].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iTOMC, xlsRow, iTOMC].Merge();

                    i++;
                    if (i > dsMasterData.Tables[0].Rows.Count - 1)
                    {
                        sheet1.Range[xlsRow - 1, 1, xlsRow , endXlsCol].BorderInside(ExcelLineStyle.Thin);
                        sheet1.Range[xlsRow - 1, 1, xlsRow , endXlsCol].BorderAround(ExcelLineStyle.Thin);
                        sheet1.Range[xlsRow - 1, 1, xlsRow , endXlsCol].WrapText = true;
                        continue;
                    }
                    sheet1.Range[xlsRow, iTG2].Text = dsMasterData.Tables[0].Rows[i]["WorkstationTargetPerHour"].ToString().Trim();
                    sheet1.Range[xlsRow, iTG2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iTG2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iTG2, xlsRow, iTG2].Merge();

                    sheet1.Range[xlsRow, iOperation2].Text = dsMasterData.Tables[0].Rows[i]["OperationVariationName"].ToString().Trim();
                    sheet1.Range[xlsRow, iOperation2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iOperation2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iOperation2, xlsRow, iOperation2 + 3].Merge();

                    sheet1.Range[xlsRow, iSMV2].Text = dsMasterData.Tables[0].Rows[i]["TotalSPT"].ToString().Trim();
                    sheet1.Range[xlsRow, iSMV2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iSMV2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iSMV2, xlsRow, iSMV2].Merge();

                    sheet1.Range[xlsRow, iTOMC2].Text = dsMasterData.Tables[0].Rows[i]["MACHINE"].ToString().Trim();
                    sheet1.Range[xlsRow, iTOMC2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iTOMC2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iTOMC2, xlsRow, iTOMC2].Merge();

                    xlsRow++;
                    #endregion ----------------------Data-----------------------

                    #region Line Setup

                    sheet1.Range[xlsRow - 1, 1, xlsRow -1, endXlsCol].BorderInside(ExcelLineStyle.Thin);
                    sheet1.Range[xlsRow - 1, 1, xlsRow -1, endXlsCol].BorderAround(ExcelLineStyle.Thin);
                    sheet1.Range[xlsRow - 1, 1, xlsRow -1, endXlsCol].WrapText = true;

                    #endregion Line Setup
                }
                int endrow = xlsRow ;
                sheet1.Range[startrow, iUnit, endrow, iUnit].Merge();
                sheet1.Range[startrow, iUnit, endrow, iUnit].Text = "Center Table";
                sheet1[startrow, iUnit, endrow, iUnit].CellStyle.Rotation = 90;
                sheet1[startrow, iUnit, endrow, iUnit].CellStyle.Font.Size = 15;
                sheet1[startrow, iUnit, endrow, iUnit].CellStyle.Font.Bold = true;
                sheet1.Range[startrow, iUnit, endrow, iUnit].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[startrow, iUnit, endrow, iUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[startrow, iUnit, endrow, iUnit].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_50_percent;

                xlsRow++;
                sheet1.Range[xlsRow, 3].Text = "Left Side";
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3, xlsRow, 4].Merge();
                sheet1.Range[xlsRow, 3, xlsRow, 4].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow,4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 11].Text = "Right Side";
                sheet1.Range[xlsRow, 11].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 11, xlsRow, 12].Merge();
                sheet1.Range[xlsRow, 11, xlsRow, 12].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 11, xlsRow, 12].VerticalAlignment = ExcelVAlign.VAlignCenter;

                int MachineStartRow = endrow + 3;
                int MachineStartCol = iTG2 - 2;

                int iLateBy = 9;
                #region ******************Report Header******************
                try
                {
                    string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
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
                sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 17;
                sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 25;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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
                sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 15;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Machine Layout Report: " + ProductionDate;
                sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                //sheet1.UsedRange["A12"].FreezePanes();

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                //sheet1.UsedRange.CellStyle.Font.Size = 8;
                //sheet1.Range["A1"].CellStyle.Font.Size = 14;
                //sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                xlsRow = MachineStartRow;
                xlsCol = MachineStartCol;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Text = "Machine type";
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;

                xlsRow++;

                int TotalCalculation = xlsRow;

              //DataTable dtMachineSummary=  dsMasterData.Tables[0].AsEnumerable().GroupBy(x => new
              //  {
              //      SalesOrderId = x["Machine"]
              //  })
              //                       .Select(x =>
              //                       {
              //                           DataRow row = dsMasterData.Tables[0].NewRow();
              //                           row["Machine"] = x.Key.SalesOrderId;
              //                           row["Rcount"] = x.Sum(r => (decimal)r["Rcount"]);
              //                           return row;
              //                       }
              //                       ).CopyToDataTable();


                DataTable dtDistinctMachine = dsMasterData.Tables[0].DefaultView.ToTable(true, "Machine");
                int startFormulaRow = xlsRow;
                int startFormulaCol = xlsCol+2;
                for (int i = 0; i < dtDistinctMachine.Rows.Count; i++)
                {
                    dsMasterData.Tables[0].DefaultView.RowFilter = "Machine='"+ dtDistinctMachine.Rows[i]["MACHINE"].ToString() + "'";

                    sheet1.Range[xlsRow, xlsCol].Text = dtDistinctMachine.Rows[i]["MACHINE"].ToString();
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();

                    sheet1.Range[xlsRow, xlsCol + 2].Number = dsMasterData.Tables[0].DefaultView.Count;
                    sheet1.Range[xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsRow++;
                }

                sheet1.Range[MachineStartRow , MachineStartCol, xlsRow, MachineStartCol+2].BorderInside(ExcelLineStyle.Thin);
                sheet1.Range[MachineStartRow , MachineStartCol, xlsRow, MachineStartCol+2].BorderAround(ExcelLineStyle.Thin);
                sheet1.Range[MachineStartRow , MachineStartCol, xlsRow, MachineStartCol+2].WrapText = true;

                sheet1.Range[xlsRow, xlsCol].Text = "Total ManPower";
                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol].RowHeight = 13.2;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;

                sheet1.Range[xlsRow, xlsCol + 2].Formula= "SUM(" + clsStaticInfo.GetxlsCol(startFormulaCol) + startFormulaRow + ":" + clsStaticInfo.GetxlsCol(startFormulaCol) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, xlsCol + 2].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$11";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;

                sheet1.Name = "MachineLayout";

                #endregion Page Setup

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = "Machine Layout Report " + ProductionDate + " .xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objRpt = null;
                excelEngine = null;
                application = null;
                workbook = null;
            }
        }
        public void SelectedPlantWiseCompany(string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT p.UserName PlantName,c.UserName CompanyName ,ISNULL(a.Address1,'')+','+ ISNULL(a.Address2,'') Address1, a.Phone,a.Email
                                ,cm.Address1 cAddress1 ,cm.Address2 cAddress2
                                FROM org.Plant p
							LEFT OUTER JOIN org.Company c on c.Id=p.CompanyId
							LEFT OUTER JOIN mst.AddressMaster a on a.Id=p.AddressMasterId
							LEFT OUTER JOIN mst.AddressMaster cm on cm.Id=c.AddressMasterId
							WHERE p.Id='" + sPlantID + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function
        public void SelectedPlant(string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT P.UserName,AM.Address1+','+ ISNULL(AM.Address2,'') Address1 FROM ORG.Plant P
                            LEFT OUTER JOIN MST.AddressMaster AM ON P.AddressMasterId=AM.Id
                             WHERE P.Id = '" + sPlantID + @"'";
                //strSql = @"SELECT * FROM ORG.Plant WHERE Id = '" + sPlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function
        public void GetData(string EntityId, string ProcessId, string ProductionDate, string WorkCenterMasterId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT ov.UserName OperationVariationName  ,pbtd.OperationTargetPerHr,o.IsMachineRequired,pbtd.TotalSPT,
								CASE WHEN ISNULL(o.IsMachineRequired,'')='M' THEN 'Manual' ELSE ISNULL(mma.StandardName,'Missing Machcine') END AS MACHINE
		                        ,CONVERT(INT,pbtd.OperationTargetPerHr/CASE WHEN pbtd.RequiredManPower>0 THEN pbtd.RequiredManPower ELSE 1 END) WorkstationTargetPerHour
                                FROM LineLayoutDailyTargetData AS llbpbd
                                LEFT JOIN LineLayoutDailyTarget AS lldt ON lldt.Id = llbpbd.LineLayoutDailyTargetId
                                LEFT JOIN MST.OperationVariation AS ov ON ov.Id=llbpbd.OperationVariationId
                                JOIN TRN.ProductionBulletinTemplateDetail AS pbtd ON pbtd.OperationVariationId =llbpbd.OperationVariationId AND pbtd.ProductionBulletinTemplateMasterId=lldt.ProductionBulletinTemplateMasterId
                                AND pbtd.OperationVariationId=(SELECT TOP 1 OperationVariationId FROM LineLayoutDailyTargetData Y WHERE y.LineLayoutDailyTargetId=lldt.Id AND y.OperationVariationId=llbpbd.OperationVariationId) 
                                LEFT JOIN mst.Operation AS o ON o.Id=ov.OperationId   
                                LEFT JOIN  trn.FixedAssetRegister AS far ON far.Id=llbpbd.FixedAssetRegisterId  
                                LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=far.MaterialMasterArticleId       
                                WHERE lldt.TargetDate ='" + ProductionDate + "' AND lldt.ProcessId='" + ProcessId + @"' AND lldt.EntityId='" + EntityId + @"' AND lldt.WorkCenterMasterId='" + WorkCenterMasterId + @"'
                                ORDER BY llbpbd.Sequence";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function
        #endregion
    }
}