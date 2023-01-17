using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.UI.WebControls;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;


namespace Library.Service.OrderManagements
{
    public class ProductionOrderReports
    {
        private readonly ISqlRepository _sqlRepository;
        public ProductionOrderReports(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        public IWorkbook ProductionReportXls(string entityid, string fromDate, string todate, string ProductionStatus = "All")
        {

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {
                if (string.IsNullOrEmpty(entityid))
                    throw new Exception("Plase select entity");

                if (bplib.clsWebLib.IsDateOK(fromDate) == false)
                    throw new Exception("Plase select from date");

                if (bplib.clsWebLib.IsDateOK(todate) == false)
                    throw new Exception("Plase select to date");

                if (Convert.ToDateTime(fromDate) > Convert.ToDateTime(todate))
                    throw new Exception("To date cannot be earlier than from date");

                Dictionary<string, Dictionary<string, ProductionQtyDistributionSO>> dicProductionQtyDistributionByProcessUD;
                Dictionary<string, Dictionary<string, ProductionQtyDistributionSO>> dicProductionQtyDistributionByProcessRANGE;



                DataTable dt, dtAllProcess, dtOrderMaster;


                dicProductionQtyDistributionByProcessUD = new Dictionary<string, Dictionary<string, ProductionQtyDistributionSO>>();
                dicProductionQtyDistributionByProcessRANGE = new Dictionary<string, Dictionary<string, ProductionQtyDistributionSO>>();
                getAllProcess(entityid, todate, out dtAllProcess);
                for (int i = 0; i < dtAllProcess.Rows.Count; i++)
                {
                    Dictionary<string, ProductionQtyDistributionSO> dicProductionQtyDistribution;
                    getSalesOrderDistributionForProduction(ProductionStatus, todate, entityid, dtAllProcess.Rows[i]["Id"].ToString(), out dicProductionQtyDistribution, out dt);
                    dicProductionQtyDistributionByProcessUD.Add(dtAllProcess.Rows[i]["Id"].ToString(), dicProductionQtyDistribution);


                    Dictionary<string, ProductionQtyDistributionSO> dicProductionQtyDistributionRANGE;
                    getSalesOrderDistributionForProductionRange(ProductionStatus, fromDate, todate, entityid, dtAllProcess.Rows[i]["Id"].ToString(), out dicProductionQtyDistributionRANGE, out dt);
                    dicProductionQtyDistributionByProcessRANGE.Add(dtAllProcess.Rows[i]["Id"].ToString(), dicProductionQtyDistributionRANGE);
                }

                //Dictionary<string, double> dicWIP = new Dictionary<string, double>();
                //getSalesOrderDistributionForProductionWIP(todate, entityid, out dicWIP);

                getOrderMasterForProduction(ProductionStatus, entityid, todate, out dtOrderMaster);

                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[0].Name = "Production";
                sheet = workbook.Worksheets[0];



                int ROW = 1; int COL = 1;
                sheet[ROW, 1].Text = "Daily Production Report";
                sheet[ROW, 1].CellStyle.Font.Size = 16;
                sheet[ROW, 1].RowHeight = 22;
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, 10].Merge();
                ROW++;
                sheet[ROW, 1].Text = "From [" + fromDate + "] to [" + todate + "]";
                sheet[ROW, 1].CellStyle.Font.Size = 14;
                sheet[ROW, 1].RowHeight = 20;
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, 10].Merge();
                ROW++;
                sheet[ROW, 1].Text = "*All values are in PCS";
                sheet[ROW, 1].CellStyle.Font.Size = 12;
                sheet[ROW, 1].RowHeight = 18;
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, 10].Merge();
                ROW++;

                ROW += 2;
                #region columns

                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colBuyer = COL;
               
                COL++;
                sheet[ROW, COL].Text = "Master Order No";
                sheet[ROW, COL].ColumnWidth = 14;
                int colMasterOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Order No";
                sheet[ROW, COL].ColumnWidth = 14;
                int colBuyerOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Order No";
                sheet[ROW, COL].ColumnWidth = 14;
                int colOwnOrderNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Buyer Item No";
                sheet[ROW, COL].ColumnWidth = 14;
                int colBuyerItemNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Item No";
                sheet[ROW, COL].ColumnWidth = 14;
                int colOwnItemNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Product Category";
                sheet[ROW, COL].ColumnWidth = 22;
                int colProductCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 22;
                int colProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 22;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 22;
                int colArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order Id";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Category";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOrderCategory = COL;    //                        

                COL++;
                sheet[ROW, COL].Text = "Order Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colOrderStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Production Order Status";
                sheet[ROW, COL].ColumnWidth = 12;
                int colproductionStatus = COL;
                COL++;


                sheet[ROW, COL].Text = "Delivery Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colDeliveryDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Commitment Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colCommitmentDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Nos";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderNos = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Desc";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSalesOrderDesc = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSOQty = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Planned Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPlannedQty = COL;


                System.Drawing.Color fdColor = System.Drawing.Color.FromArgb(255, 255, 220);
                System.Drawing.Color rangeColor = System.Drawing.Color.FromArgb(224, 255, 220);
                System.Drawing.Color udColor = System.Drawing.Color.FromArgb(255, 255, 220);
                System.Drawing.Color wipColor = System.Drawing.Color.FromArgb(224, 255, 220);

                int ROWSTART = ROW - 1;

                int ColStartRange = COL;
                Dictionary<string, int> dicProcessProductionColFD = new Dictionary<string, int>();
                for (int p = 0; p < dtAllProcess.Rows.Count; p++)
                {
                    COL++;
                    sheet[ROW, COL].Text = dtAllProcess.Rows[p]["UserName"].ToString();
                    sheet[ROW, COL].ColumnWidth = 12;
                    sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    dicProcessProductionColFD.Add(dtAllProcess.Rows[p]["Id"].ToString(), COL);

                }
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].Text = "For The Day Production";
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Interior.Color = fdColor;
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Font.Size = 9f;
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].Merge();
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                ColStartRange = COL;
                Dictionary<string, int> dicProcessProductionColRANGE = new Dictionary<string, int>();
                for (int p = 0; p < dtAllProcess.Rows.Count; p++)
                {
                    COL++;
                    sheet[ROW, COL].Text = dtAllProcess.Rows[p]["UserName"].ToString();
                    sheet[ROW, COL].ColumnWidth = 12;
                    sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    dicProcessProductionColRANGE.Add(dtAllProcess.Rows[p]["Id"].ToString(), COL);

                }
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].Text = "For the Period Production";
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Interior.Color = rangeColor;
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Font.Size = 9f;
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].Merge();
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                ColStartRange = COL;
                Dictionary<string, int> dicProcessProductionColUD = new Dictionary<string, int>();
                for (int p = 0; p < dtAllProcess.Rows.Count; p++)
                {
                    COL++;
                    sheet[ROW, COL].Text = dtAllProcess.Rows[p]["UserName"].ToString();
                    sheet[ROW, COL].ColumnWidth = 12;
                    sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    dicProcessProductionColUD.Add(dtAllProcess.Rows[p]["Id"].ToString(), COL);

                }
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].Text = "Up to date Production";
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Interior.Color = udColor;
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Font.Size = 9f;
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].Merge();
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                ColStartRange = COL;
                Dictionary<string, int> dicProcessProductionColBalance = new Dictionary<string, int>();
                for (int p = 0; p < dtAllProcess.Rows.Count; p++)
                {
                    COL++;
                    sheet[ROW, COL].Text = dtAllProcess.Rows[p]["UserName"].ToString();
                    sheet[ROW, COL].ColumnWidth = 12;
                    sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    dicProcessProductionColBalance.Add(dtAllProcess.Rows[p]["Id"].ToString(), COL);

                }
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].Text = "Up to date Balance Qty";
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Interior.Color = rangeColor;
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Font.Bold = true;
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Font.Size = 9f;
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].Merge();
                sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                //ColStartRange = COL;
                //Dictionary<string, int> dicProcessProductionColWIP = new Dictionary<string, int>();
                //for (int p = 0; p < dtAllProcess.Rows.Count; p++)
                //{
                //    COL++;
                //    sheet[ROW, COL].Text = dtAllProcess.Rows[p]["UserName"].ToString();
                //    sheet[ROW, COL].ColumnWidth = 12;
                //    sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //    dicProcessProductionColWIP.Add(dtAllProcess.Rows[p]["Id"].ToString(), COL);

                //}
                //sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].Text = "WIP";
                //sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Interior.Color = wipColor;
                //sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Font.Bold = true;
                //sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].CellStyle.Font.Size = 9f;
                //sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].BorderAround(ExcelLineStyle.Hair);
                //sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].Merge();
                //sheet.Range[ROW - 1, ColStartRange + 1, ROW - 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                //238,205,225
                #endregion columns

                int endCol = COL;

                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(150, 250, 150);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dtOrderMaster.Rows.Count; i++)
                {
                    sheet[ROW, colPlant].Text = dtOrderMaster.Rows[i]["Plant"].ToString();
                    sheet[ROW, colEntity].Text = dtOrderMaster.Rows[i]["Entity"].ToString();
                    sheet[ROW, colBuyer].Text = dtOrderMaster.Rows[i]["Buyer"].ToString();

                    sheet[ROW, colBuyerOrderNo].Text = dtOrderMaster.Rows[i]["BuyerOrderNo"].ToString();
                    sheet[ROW, colOwnOrderNo].Text = dtOrderMaster.Rows[i]["OwnOrderNo"].ToString();
                    sheet[ROW, colBuyerItemNo].Text = dtOrderMaster.Rows[i]["BuyerItemNo"].ToString();
                    sheet[ROW, colOwnItemNo].Text = dtOrderMaster.Rows[i]["OwnItemNo"].ToString();

                    sheet[ROW, colSalesOrderNos].Text = dtOrderMaster.Rows[i]["SONo"].ToString();
                    sheet[ROW, colSalesOrderDesc].Text = dtOrderMaster.Rows[i]["SODesc"].ToString();



                    sheet[ROW, colProductCategory].Text = dtOrderMaster.Rows[i]["ProductCategory"].ToString();
                    sheet[ROW, colProduct].Text = dtOrderMaster.Rows[i]["Product"].ToString();
                    sheet[ROW, colArticle].Text = dtOrderMaster.Rows[i]["Article"].ToString();

                    sheet[ROW, colCommitmentDate].Text = GetDate(dtOrderMaster.Rows[i]["CommitmentDate"].ToString());
                    sheet[ROW, colDeliveryDate].Text = GetDate(dtOrderMaster.Rows[i]["DeliveryDate"].ToString());
                    sheet[ROW, colMasterOrderNo].Text = dtOrderMaster.Rows[i]["MasterOrderId"].ToString();
                    sheet[ROW, colMaterial].Text = dtOrderMaster.Rows[i]["Material"].ToString();
                    sheet[ROW, colOrderCategory].Text = dtOrderMaster.Rows[i]["OrderCategory"].ToString();
                    sheet[ROW, colOrderStatus].Text = dtOrderMaster.Rows[i]["OrderStatus"].ToString();
                    sheet[ROW, colProductionOrderId].Text = dtOrderMaster.Rows[i]["ProductionOrderId"].ToString();
                    sheet[ROW, colproductionStatus].Text = dtOrderMaster.Rows[i]["productionStatus"].ToString();
                    //sheet[ROW, colResponsiblePerson].Text = dtOrderMaster.Rows[i]["ResponsiblePerson"].ToString();
                    sheet[ROW, colSOQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["SOQty"].ToString());
                    sheet[ROW, colPlannedQty].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["PlannedQty"].ToString());



                    for (int p = 0; p < dtAllProcess.Rows.Count; p++)
                    {
                        if (dicProductionQtyDistributionByProcessUD.ContainsKey(dtAllProcess.Rows[p]["Id"].ToString()))
                        {

                            if (dicProductionQtyDistributionByProcessUD[dtAllProcess.Rows[p]["Id"].ToString()].ContainsKey(dtOrderMaster.Rows[i]["ProductionOrderId"].ToString()))
                            {
                                sheet[ROW, dicProcessProductionColUD[dtAllProcess.Rows[p]["Id"].ToString()]].Number = dicProductionQtyDistributionByProcessUD[dtAllProcess.Rows[p]["Id"].ToString()][dtOrderMaster.Rows[i]["ProductionOrderId"].ToString()].CumulativeQty;
                                sheet[ROW, dicProcessProductionColFD[dtAllProcess.Rows[p]["Id"].ToString()]].Number = dicProductionQtyDistributionByProcessUD[dtAllProcess.Rows[p]["Id"].ToString()][dtOrderMaster.Rows[i]["ProductionOrderId"].ToString()].ProducedQtyToday;


                                sheet[ROW, dicProcessProductionColBalance[dtAllProcess.Rows[p]["Id"].ToString()]].Formula = clsStaticInfo.GetxlsCol(colPlannedQty) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(dicProcessProductionColUD[dtAllProcess.Rows[p]["Id"].ToString()]) + ROW.ToString();
                            }

                        }

                        if (dicProductionQtyDistributionByProcessRANGE.ContainsKey(dtAllProcess.Rows[p]["Id"].ToString()))
                        {

                            if (dicProductionQtyDistributionByProcessRANGE[dtAllProcess.Rows[p]["Id"].ToString()].ContainsKey(dtOrderMaster.Rows[i]["ProductionOrderId"].ToString()))
                            {
                                sheet[ROW, dicProcessProductionColRANGE[dtAllProcess.Rows[p]["Id"].ToString()]].Number = dicProductionQtyDistributionByProcessRANGE[dtAllProcess.Rows[p]["Id"].ToString()][dtOrderMaster.Rows[i]["ProductionOrderId"].ToString()].DistributedQty;
                            }

                        }


                        //string PRProcess = dtOrderMaster.Rows[i]["ProductionOrderId"].ToString() + dtAllProcess.Rows[p]["Id"].ToString();
                        //if (dicWIP.ContainsKey(PRProcess))
                        //{
                        //    sheet[ROW, dicProcessProductionColWIP[dtAllProcess.Rows[p]["Id"].ToString()]].Number = dicWIP[PRProcess];
                        //}
                    }



                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                for (int i = colSOQty; i < endCol + 1; i++)
                {
                    sheet[ROW, i].Formula = "SUM(" + clsStaticInfo.GetxlsCol(i) + startRow.ToString() + ":" + clsStaticInfo.GetxlsCol(i) + (ROW - 1).ToString() + ")";
                }
                sheet.Range[ROW, 1, ROW, colSOQty - 1].Text = "Total";
                sheet.Range[ROW, 1, ROW, colSOQty - 1].Merge();
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet.Range[ROWSTART, dicProcessProductionColFD.Values.Min(), ROW, dicProcessProductionColFD.Values.Max()].CellStyle.Interior.Color = fdColor;
                sheet.Range[ROWSTART, dicProcessProductionColUD.Values.Min(), ROW, dicProcessProductionColUD.Values.Max()].CellStyle.Interior.Color = udColor;
                sheet.Range[ROWSTART, dicProcessProductionColRANGE.Values.Min(), ROW, dicProcessProductionColRANGE.Values.Max()].CellStyle.Interior.Color = rangeColor;
                //sheet.Range[ROWSTART, dicProcessProductionColWIP.Values.Min(), ROW, dicProcessProductionColWIP.Values.Max()].CellStyle.Interior.Color = wipColor;



                sheet.UsedRange.NumberFormat = "#,##0;[Red](#,##0)";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.UsedRange["A3"].FreezePanes();
                sheet.Range[7, 4].FreezePanes();
                //sheet.AutoFilters.FilterRange = sheet[startRow - 1, 1, ROW, endCol];
                //#region ******************Report Header******************
                //ROW = 1;
                //COL = 1;
                sheet.IsDisplayZeros = false;
                //ReportUtility ru = new ReportUtility();
                //Param param = new Param();
                //param.CompanyGroupId = identity.CompanyGroupId;
                //param.CompanyId = identity.CompanyId;
                //ru.Header(ref sheet, param, endCol, "OS-2");

                //#endregion ******************Report Header******************

                //IWorksheet sheet2 = workbook.Worksheets[1];
                //sheet2.Name = "Reference Data";
                //sheet2.ImportDataTable(dt, true, 1, 1);
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;
                workbook.Version = ExcelVersion.Excel2016;

            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return workbook;

        }
        public IWorkbook LineBookingStatusXls(string entityid, string plantid)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {
                if (string.IsNullOrEmpty(entityid) || entityid == "''")
                    throw new Exception("Select entity");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataTable dtOrderMaster;

                getLineBookingStatus(entityid, out dtOrderMaster);


                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[0].Name = "LineBookingStaus";
                sheet = workbook.Worksheets[0];
                sheet.IsGridLinesVisible = false;

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;
                sheet[ROW, COL].Text = "Line No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colLineNo = COL;

                COL++;
                sheet[ROW, COL].Text = "Line Start Date";
                sheet[ROW, COL].ColumnWidth = 12.5;
                int colStartDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Last Booking Date";
                sheet[ROW, COL].ColumnWidth = 12.5;
                int colLastBookingDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Last Buyer/Product";
                sheet[ROW, COL].ColumnWidth = 14.5;
                int colLastBuyerProduct = COL;
                COL++;
                sheet[ROW, COL].Text = "Gap (" + DateTime.Now.ToString("MMM-yyyy") + ")";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colGap1 = COL;
                COL++;
                sheet[ROW, COL].Text = "Gap (" + DateTime.Now.AddMonths(1).ToString("MMM-yyyy") + ")";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colGap2 = COL;
                COL++;
                sheet[ROW, COL].Text = "Gap (" + DateTime.Now.AddMonths(2).ToString("MMM-yyyy") + ")";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colGap3 = COL;
                Dictionary<string, int> dicColIndex = new Dictionary<string, int>();
                DataTable dt = dtOrderMaster.DefaultView.ToTable();
                dt.DefaultView.Sort = "YEAR ASC,Month ASC";
                dt = dt.DefaultView.ToTable();
                DataView dv = new DataView(dt.DefaultView.ToTable(true, "MonthShortName", "WorkingDays"));

                int ColStartRange = COL + 1;
                for (int i = 0; i < dv.Count; i++)
                {


                    if (dv[i]["MonthShortName"].ToString() != "")
                    {
                        COL++;
                        sheet[ROW, COL].Text = dv[i]["MonthShortName"].ToString().Replace("/", "-");
                        sheet[ROW, COL].ColumnWidth = 10;
                        sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                        sheet[ROW + 1, COL].Number = clsStaticInfo.dbl(dv[i]["WorkingDays"].ToString());
                        sheet[ROW + 1, COL].ColumnWidth = 10;
                        sheet[ROW + 1, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                        dicColIndex.Add(dv[i]["MonthShortName"].ToString(), COL);

                    }
                }

                COL++;
                sheet[ROW, COL].Text = "Total Vacant Days";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                int colTotal = COL;
                COL++;
                sheet[ROW, COL].Text = "Account Holder";
                sheet[ROW, COL].ColumnWidth = 16;
                int colAccountHolder = COL;
                COL++;
                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int colremarks = COL;
                #endregion columns

                sheet[ROW + 1, colTotal].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColStartRange) + (ROW + 1).ToString() + ":" + clsStaticInfo.GetxlsCol(colTotal - 1) + (ROW + 1).ToString() + ")";






                int endCol = COL;

                sheet.Range[ROW, 1, ROW + 1, endCol].CellStyle.Interior.Color = System.Drawing.Color.Black;
                sheet.Range[ROW, 1, ROW + 1, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW + 1, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW + 1, endCol].CellStyle.Font.Size = 10f;
                sheet.Range[ROW, 1, ROW + 1, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW + 1, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW + 1;

                string workcenter = "";
                string GroupingData = "";
                List<int> SumRows = new List<int>();
                for (int i = 0; i < dtOrderMaster.Rows.Count; i++)
                {
                    if (GroupingData != dtOrderMaster.Rows[i]["GroupingData"].ToString())
                    {
                        if (i > 0)
                        {
                            ROW++;
                            SumRows.Add(ROW);

                            sheet[ROW, 1].Text = "Total Gap Days";
                            sheet.Range[ROW, 1, ROW, colGap1 - 1].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                            for (int KK = colGap1; KK <= colTotal; KK++)
                            {
                                sheet[ROW, KK].Formula = "SUM(" + clsStaticInfo.GetxlsCol(KK) + (startRow).ToString() + ":" + clsStaticInfo.GetxlsCol(KK) + (ROW - 1).ToString() + ")";
                            }
                            sheet[startRow, colGap1, ROW - 1, colGap3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;

                            sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 10f;
                            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.Black;
                            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                        }

                        ROW++;
                        sheet[ROW, colLineNo].Text = "Work Center Type: " + dtOrderMaster.Rows[i]["GroupingData"].ToString();
                        sheet[ROW, colLineNo, ROW, colLineNo + 2].Merge();
                        sheet[ROW, colLineNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.LightBlue;
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.Black;
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                        GroupingData = dtOrderMaster.Rows[i]["GroupingData"].ToString();
                        startRow = ROW + 1;
                    }
                    if (workcenter != dtOrderMaster.Rows[i]["Id"].ToString())
                    {
                        ROW++;

                        //sheet[ROW, colGap].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["GapDaysAsPerLastPlanningDate"].ToString());

                        sheet[ROW, colGap1].Number = clsStaticInfo.dbl(dtOrderMaster.Compute("SUM(GapDaysAsPerLastPlanningDate)", "Id='" + dtOrderMaster.Rows[i]["Id"].ToString() + "' AND MonthShortName='" + DateTime.Now.AddMonths(0).ToString("MMM/yyyy") + "'").ToString());
                        sheet[ROW, colGap2].Number = clsStaticInfo.dbl(dtOrderMaster.Compute("SUM(GapDaysAsPerLastPlanningDate)", "Id='" + dtOrderMaster.Rows[i]["Id"].ToString() + "' AND MonthShortName='" + DateTime.Now.AddMonths(1).ToString("MMM/yyyy") + "'").ToString());
                        sheet[ROW, colGap3].Number = clsStaticInfo.dbl(dtOrderMaster.Compute("SUM(GapDaysAsPerLastPlanningDate)", "Id='" + dtOrderMaster.Rows[i]["Id"].ToString() + "' AND MonthShortName='" + DateTime.Now.AddMonths(2).ToString("MMM/yyyy") + "'").ToString());


                        sheet[ROW, colPlant].Text = dtOrderMaster.Rows[i]["Plant"].ToString();
                        sheet[ROW, colEntity].Text = dtOrderMaster.Rows[i]["Entity"].ToString();
                        sheet[ROW, colLineNo].Text = dtOrderMaster.Rows[i]["WorkCenter"].ToString();
                        sheet[ROW, colStartDate].Text = dtOrderMaster.Rows[i]["LineStartDate"].ToString();
                        sheet[ROW, colLastBookingDate].Text = GetDate(dtOrderMaster.Rows[i]["LastPlanDate"].ToString());
                        sheet[ROW, colAccountHolder].Text = GetDate(dtOrderMaster.Rows[i]["AccountHolder"].ToString());

                        if (dtOrderMaster.Rows[i]["Product"].ToString() != "")
                            sheet[ROW, colLastBuyerProduct].Text = dtOrderMaster.Rows[i]["Buyer"].ToString() + "/" + dtOrderMaster.Rows[i]["Product"].ToString();

                        sheet[ROW, colTotal].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColStartRange) + ROW.ToString() + ":" + clsStaticInfo.GetxlsCol(colTotal - 1) + ROW.ToString() + ")";

                        workcenter = dtOrderMaster.Rows[i]["Id"].ToString();
                    }



                    if (dicColIndex.ContainsKey(dtOrderMaster.Rows[i]["MonthShortName"].ToString()))
                        sheet[ROW, dicColIndex[dtOrderMaster.Rows[i]["MonthShortName"].ToString()]].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["GapDays"].ToString());




                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                }

                ROW++;
                SumRows.Add(ROW);
                sheet[ROW, 1].Text = "Total Gap Days";
                sheet.Range[ROW, 1, ROW, colGap1 - 1].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                for (int i = colGap1; i <= colTotal; i++)
                {
                    sheet[ROW, i].Formula = "SUM(" + clsStaticInfo.GetxlsCol(i) + (startRow).ToString() + ":" + clsStaticInfo.GetxlsCol(i) + (ROW - 1).ToString() + ")";
                }
                sheet[startRow, colGap1, ROW - 1, colGap3].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 10f;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;

                ROW++;
                sheet[ROW, 1].Text = "Grand Total";
                sheet.Range[ROW, 1, ROW, colGap1 - 1].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                for (int i = colGap1; i <= colTotal; i++)
                {
                    string SumString = "";
                    for (int kk = 0; kk < SumRows.Count; kk++)
                    {
                        SumString += "+" + clsStaticInfo.GetxlsCol(i) + (SumRows[kk]).ToString();
                    }
                    sheet[ROW, i].Formula = SumString;
                }
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 10f;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                //IListObject table = sheet.ListObjects.Create("Table1", sheet[clsStaticInfo.GetxlsCol(1) + (1).ToString() + ":" + clsStaticInfo.GetxlsCol(endCol) + (ROW).ToString()]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
                ReportUtility reportUtility = new ReportUtility();
                //reportUtility.PlantHeader(ref sheet, endCol, "Line Booking Status And Vacant Capacity Report", plantid);
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Line Booking Status And Vacant Capacity Report", identity.CompanyId, identity.CompanyName, "");
                sheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.IsDisplayZeros = false;

                //#endregion ******************Report Header******************

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

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return workbook;


        }


        private void getLineBookingStatus(string entityid, out DataTable dtOrderMaster)
        {
            string FromDate = DateTime.Now.ToString("dd-MMM-yyyy");
            DateTime _todate = DateTime.Now.AddMonths(5);
            int daysInMonth = DateTime.DaysInMonth(_todate.Year, _todate.Month);
            _todate = new DateTime(_todate.Year, _todate.Month, daysInMonth);

            string ToDate = _todate.ToString("dd-MMM-yyyy");

            //            string sql = @"SELECT w.Id,w.Sequence, w.UserName AS WorkCenter,kk.LineStartDate, 
            //                            kk.[YEAR], kk.[MONTH], kk.MonthShortName, SUM(PlannedDays) AS PlannedDays,SUM(WorkingDays) AS WorkingDays
            //  FROM (

            //SELECT FORMAT( e.StartDate,'dd-MMM-yyyy') AS LineStartDate, w.Id, YEAR(ppc.WorkingDate) AS YEAR,MONTH(ppc.WorkingDate) AS MONTH,
            //FORMAT(ppc.WorkingDate,'MMM/yyyy') AS MonthShortName,CASE WHEN ISNULL(pp.ID,'')<>'' AND  isnull( e.StartDate,'')<>'' THEN 1 ELSE 0 END AS PlannedDays,
            //CASE WHEN  isnull( e.StartDate,'')<>'' THEN 1 ELSE 0 END AS WorkingDays
            //  FROM scs.WorkCenterMaster AS w
            //LEFT OUTER JOIN (SELECT WorkCenterMasterId,MAX(StartDate) AS StartDate FROM [SCS].[WorkCenterMasterEffectiveDate] GROUP BY WorkCenterMasterId) E ON e.WorkCenterMasterId=w.Id
            //LEFT OUTER JOIN ProductionPlanningCalendar AS ppc ON w.EntityId=ppc.EntityID AND w.ProcessId=ppc.ProcessID 
            //                        AND ppc.WorkingDate BETWEEN (CASE WHEN e.StartDate<='" + FromDate + @"' THEN '" + FromDate + @"' ELSE e.StartDate END) AND '" + ToDate + @"'
            //                        LEFT OUTER JOIN ProductionPlanningType1 AS pp ON pp.ProductionDate=ppc.WorkingDate 
            //												                        AND pp.EntityID=ppc.EntityID 
            //												                        AND pp.ProcessID=ppc.ProcessID
            //												                        AND pp.WorkCenterMasterId=w.Id


            //                        WHERE W.EntityID='" + entityid + @"' AND w.ProcessID=(SELECT TOP 1 pt.BaseProcessId
            //                                                                    FROM PlanningTypes AS pt WHERE pt.PlanningType='" + EnumPlanningTypes.PlanningType1.ToString() + @"')
            //                        AND ISNULL(ppc.DayType,'') NOT IN ('W','H')


            //--GROUP BY  w.Id,ppc.WorkingDate
            //) AS KK
            //LEFT OUTER JOIN scs.WorkCenterMaster AS w ON w.Id=kk.Id
            //GROUP BY kk.LineStartDate,w.id,w.Sequence,w.UserName,kk.[YEAR], kk.[MONTH], kk.MonthShortName

            //ORDER BY w.Sequence,kk.[YEAR], kk.[MONTH] ASC";


            string sql = @"SELECT  trkp.UserName AS Plant,trke.UserName AS Entity, isnull(w.GroupingData,'Internal') AS GroupingData, w.Id,w.Sequence,  w.UserName AS WorkCenter,kk.LineStartDate, ei.EmployeeName AS AccountHolder,
                            kk.[YEAR], kk.[MONTH], kk.MonthShortName, SUM(PlannedDays) AS PlannedDays,CAL.WorkingDayCount AS WorkingDays,
                            SUM(KK.WorkingDays)-SUM(PlannedDays) AS GapDays,SUM(GapDaysAsPerLastPlanningDate) AS GapDaysAsPerLastPlanningDate,
                           MAX(kk.LastPlanDate) AS LastPlanDate,kk.buyer,kk.Product
                            
  FROM (

  SELECT nn.*,lpp.buyer, lpp.Product,CASE WHEN ISNULL(pp.ID,'')='' AND nn.CalendarDate<=nn.LastPlanDate THEN 1 ELSE 0 END AS GapDaysAsPerLastPlanningDate, pp.ProductionDate AS PlanDate
    FROM (
			SELECT w.EntityId,w.ProcessId,pp.ID AS PlanId,  FORMAT( e.StartDate,'dd-MMM-yyyy') AS LineStartDate,pp.ProductionDate, LP.LastPlanDate, w.Id, YEAR(ppc.WorkingDate) AS YEAR,MONTH(ppc.WorkingDate) AS MONTH,
			ppc.WorkingDate AS CalendarDate,
			FORMAT(ppc.WorkingDate,'MMM/yyyy') AS MonthShortName,
			CASE WHEN ISNULL(pp.ID,'')<>'' AND  isnull( e.StartDate,'')<>'' THEN 1 ELSE 0 END AS PlannedDays,
			--CASE WHEN  isnull( e.StartDate,'')<>'' THEN 1 ELSE 0 END AS WorkingDays
			CASE WHEN  isnull( '01-Jan-2000','')<>'' THEN 1 ELSE 0 END AS WorkingDays

  FROM scs.WorkCenterMaster AS w
LEFT OUTER JOIN (SELECT WorkCenterMasterId,MAX(ProductionDate) AS LastPlanDate
				FROM ProductionPlanningType1 GROUP BY WorkCenterMasterId) AS LP ON lp.WorkCenterMasterId=w.Id
LEFT OUTER JOIN (SELECT WorkCenterMasterId,MAX(StartDate) AS StartDate FROM [SCS].[WorkCenterMasterEffectiveDate] GROUP BY WorkCenterMasterId) E ON e.WorkCenterMasterId=w.Id
LEFT OUTER JOIN ProductionPlanningCalendar AS ppc ON w.EntityId=ppc.EntityID AND w.ProcessId=ppc.ProcessID 
				AND ppc.WorkingDate BETWEEN (CASE WHEN e.StartDate<='" + FromDate + @"' THEN '" + FromDate + @"' ELSE e.StartDate END) AND '" + ToDate + @"'

                        

LEFT OUTER JOIN ProductionPlanningType1 AS pp ON pp.ProductionDate=ppc.WorkingDate 
												AND pp.EntityID=ppc.EntityID 
												AND pp.ProcessID=ppc.ProcessID
												AND pp.WorkCenterMasterId=w.Id
																		
LEFT OUTER JOIN ProductionPlanningCalendar AS ppcx ON w.EntityId=ppcx.EntityID AND w.ProcessId=ppcx.ProcessID 
				AND ppcx.WorkingDate=pp.ProductionDate  

 
                         WHERE W.EntityID IN (" + entityid + @") AND w.ProcessID=(SELECT TOP 1 pt.BaseProcessId
                                                                    FROM PlanningTypes AS pt WHERE pt.PlanningType='" + EnumPlanningTypes.PlanningType1.ToString() + @"')
                        AND ISNULL(ppc.DayType,'') NOT IN ('W','H')
  ) AS NN
 LEFT OUTER JOIN ProductionPlanningType1 AS pp ON pp.ID=NN.PlanId
 LEFT OUTER JOIN (	SELECT distinct  buyer=STUFF((select distinct ','+XB.UserName from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
	                     where T.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			
			Product=STUFF((select distinct ','+xPM.UserName from 
			trn.SalesOrder XSO 
			JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			left outer join mst.MaterialMaster Xmm on Xmm.id=Xmoi.MaterialMasterId
			left outer join trn.ProductDefinition AS Xpd ON Xpd.MaterialMasterId=Xmm.Id
			left outer join [MST].[ProductMaster] xPM on xpm.id=xpd.ProductMasterId
			
			where T.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			 WorkCenterMasterId,T.ProductionDate
				FROM ProductionPlanningType1 T) AS LPP ON LPP.WorkCenterMasterId=nn.Id AND lpp.ProductionDate=nn.LastPlanDate														                        

) AS KK
  LEFT OUTER JOIN (SELECT FORMAT(ppc.WorkingDate,'MMM/yyyy') AS WorkingMonth,COUNT(*) AS WorkingDayCount FROM ProductionPlanningCalendar AS ppc 
                         WHERE ppc.EntityID IN (" + entityid + @") AND ppc.ProcessID=(SELECT TOP 1 pt.BaseProcessId
                                                                    FROM PlanningTypes AS pt WHERE pt.PlanningType='" + EnumPlanningTypes.PlanningType1.ToString() + @"')
                        AND ISNULL(ppc.DayType,'') NOT IN ('W','H') AND ppc.WorkingDate>='" + FromDate + @"' GROUP BY FORMAT(ppc.WorkingDate,'MMM/yyyy')) AS CAL ON cal.WorkingMonth=KK.MonthShortName

LEFT OUTER JOIN scs.WorkCenterMaster AS w ON w.Id=kk.Id

                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = W.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
LEFT OUTER JOIN EmployeeInformation AS ei ON ei.SystemId=w.AccountHolder
GROUP BY  trkp.UserName,trke.UserName, CAL.WorkingDayCount,ei.EmployeeName,kk.LineStartDate,w.id,w.GroupingData,w.Sequence,w.UserName,kk.[YEAR], kk.[MONTH],kk.buyer,kk.Product, kk.MonthShortName

ORDER BY w.GroupingData, w.Sequence, w.UserName ASC,kk.[YEAR], kk.[MONTH] ASC";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);


        }

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
        private void SetDate(IRange Cell, string s)
        {
            if (string.IsNullOrEmpty(s))
                return;

            try
            {
                Cell.DateTime = Convert.ToDateTime(s);
            }
            catch (Exception)
            {
                return;
            }
        }
        private void getAllProcess(string entityid, string todate, out DataTable dtOrderMaster)
        {
            string sql = @" SELECT p.* FROM  hkp.Process AS p 
                            WHERE  p.id IN (  SELECT DISTINCT po.ProcessId
                            FROM trn.ProductionSummary PS
                            LEFT OUTER JOIN trn.ProductionOrderProcessSet AS po ON po.ProductionOrderId=ps.ProductionOrderId
                            WHERE ProductionDate<='" + todate + @"' AND ps.EntityId IN(" + entityid + @"))
                            ORDER BY p.Sequence";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);


        }
        private void getSalesOrderDistributionForProduction(string ProductionStatusId, string date, string entityid, string processid, out Dictionary<string, ProductionQtyDistributionSO> dicDistributedSO, out DataTable dt)
        {
            if (string.IsNullOrEmpty(ProductionStatusId) == true || ProductionStatusId.ToUpper() == "ALL")
                ProductionStatusId = "";
            else
                ProductionStatusId = " AND PO.ProductionStatusId IN (" + ProductionStatusId + ")";

            string sql = @"SELECT po.Id AS ProductionOrderID,PRODPR.ProductionQtyAtPR AS ProductionUptoPreviousDay,
ISNULL(PRODPRTODAY.ProductionQtyAtPR,0) ProducedQtyToday,

ISNULL(prodpr.ProductionQtyAtPR,0)+ISNULL(PRODPRTODAY.ProductionQtyAtPR,0) AS CumulativeQty
from trn.ProductionOrder PO

--production at PR Level
LEFT OUTER JOIN (
					SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
				FROM  trn.ProductionSummary S 
					WHERE CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<CONVERT(DATETIME,'" + date + @"') 
				GROUP BY  s.ProductionOrderId,s.ProcessId
) AS PRODPR ON  PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId='" + processid + @"'
--production at PR Level
LEFT OUTER JOIN (
					SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
				FROM  trn.ProductionSummary S 
				WHERE  CONVERT(DATETIME,format(s.ProductionDate,'dd-MMM-yyyy'))=CONVERT(DATETIME,'" + date + @"') 
				GROUP BY  s.ProductionOrderId,s.ProcessId
) AS PRODPRTODAY ON  PRODPRTODAY.ProductionOrderId=po.id AND PRODPRTODAY.ProcessID='" + processid + @"'
left outer join (
select POD.ProductionOrderId,
SUM((so.Qty*(1+(moi.ExtraOrderPercentage/100)))*(1+(moi.OrderWastagePercentage/100))) AS PlannedQty,
min(so.DeliveryDate) AS FirstDeliveryDate,
max(so.DeliveryDate) AS LastDeliveryDate,
sum(SO.Qty) AS OrderQty from trn.ProductionOrderDetail POD 
left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId

group by POD.ProductionOrderId
) AS ORD on ord.ProductionOrderID=PO.Id
WHERE 1=1 " + ProductionStatusId + @"
ORDER BY po.Id

                            ";


            dt = _sqlRepository.GetDataTable(sql);
            string id = "";
            dicDistributedSO = new Dictionary<string, ProductionQtyDistributionSO>();
            List<ProductionQtyDistributionSO> prBlock = new List<ProductionQtyDistributionSO>();


            for (int i = 0; i < dt.Rows.Count; i++)
            {

                ProductionQtyDistributionSO dis = new ProductionQtyDistributionSO();


                dis.ProductionOrderID = dt.Rows[i]["ProductionOrderID"].ToString();

                dis.ProductionUptoPreviousDay = clsStaticInfo.dbl(dt.Rows[i]["ProductionUptoPreviousDay"].ToString());
                dis.ProducedQtyToday = clsStaticInfo.dbl(dt.Rows[i]["ProducedQtyToday"].ToString());
                dis.CumulativeQty = clsStaticInfo.dbl(dt.Rows[i]["CumulativeQty"].ToString());

                //prBlock.Add(dis);
                dicDistributedSO.Add(dt.Rows[i]["ProductionOrderID"].ToString(), dis);

            }


        }
        private void getSalesOrderDistributionForProductionRange(string ProductionStatusId, string Fromdate, string ToDate, string entityid, string processid, out Dictionary<string, ProductionQtyDistributionSO> dicDistributedSO, out DataTable dt)
        {
            if (string.IsNullOrEmpty(ProductionStatusId) == true || ProductionStatusId.ToUpper() == "ALL")
                ProductionStatusId = "";
            else
                ProductionStatusId = " AND PO.ProductionStatusId IN (" + ProductionStatusId + ")";


            string sql = @"SELECT po.Id AS ProductionOrderID,PRODPR.ProductionQtyAtPR AS ProductionStartDateAtPR
                                from trn.ProductionOrder PO


                                --production at PR Level
                                LEFT OUTER JOIN (
					                                SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
				                                FROM  trn.ProductionSummary S 
					                                WHERE CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy')) between '" + Fromdate + @"' and '" + ToDate + @"'
				                                GROUP BY  s.ProductionOrderId,s.ProcessId
                                ) AS PRODPR ON  PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId='" + processid + @"'

                                left outer join (
                                select POD.ProductionOrderId,
                                SUM((so.Qty*(1+(moi.ExtraOrderPercentage/100)))*(1+(moi.OrderWastagePercentage/100))) AS PlannedQty,
                                min(so.DeliveryDate) AS FirstDeliveryDate,
                                max(so.DeliveryDate) AS LastDeliveryDate,
                                sum(SO.Qty) AS OrderQty from trn.ProductionOrderDetail POD 
                                left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                                left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId

                                group by POD.ProductionOrderId
                                ) AS ORD on ord.ProductionOrderID=PO.Id

                                WHERE 1=1 " + ProductionStatusId + @"
                                ORDER BY po.Id

                            ";


            dt = _sqlRepository.GetDataTable(sql);
            string id = "";
            dicDistributedSO = new Dictionary<string, ProductionQtyDistributionSO>();
            List<ProductionQtyDistributionSO> prBlock = new List<ProductionQtyDistributionSO>();


            for (int i = 0; i < dt.Rows.Count; i++)
            {

                ProductionQtyDistributionSO dis = new ProductionQtyDistributionSO();


                dis.ProductionOrderID = dt.Rows[i]["ProductionOrderID"].ToString();

                dis.DistributedQty = clsStaticInfo.dbl(dt.Rows[i]["ProductionStartDateAtPR"].ToString());


                dicDistributedSO.Add(dt.Rows[i]["ProductionOrderID"].ToString(), dis);

            }


        }
        private void getSalesOrderDistributionForProductionWIP(string ProductionStatusId, string ToDate, string entityid, out Dictionary<string, double> dicWIP)
        {
            if (string.IsNullOrEmpty(ProductionStatusId) == true || ProductionStatusId.ToUpper() == "ALL")
                ProductionStatusId = "";
            else
                ProductionStatusId = " AND PO.ProductionStatusId IN (" + ProductionStatusId + ")";

            string sql = @"SELECT prs.ProductionOrderId,prs.ProcessId,SUM(ps.Quantity) AS Qty, dense_rank() OVER (PARTITION BY prs.ProductionOrderId ORDER BY prs.Sequence ASC) AS Seq,0 AS WIP
                              FROM 
                              trn.ProductionOrder AS po
                            LEFT OUTER JOIN  trn.ProductionOrderProcessSet AS PRS ON prs.ProductionOrderId=po.id
                            LEFT OUTER JOIN trn.ProductionSummary AS ps ON prs.ProductionOrderId=ps.ProductionOrderId AND ps.ProcessId=prs.ProcessId AND ps.ProductionDate<='" + ToDate + @"'
					

                            WHERE  po.EntityID='" + entityid + @"' " + ProductionStatusId + @"
                            GROUP BY prs.ProductionOrderId,prs.ProcessId, prs.Sequence
                            ORDER BY prs.ProductionOrderId, PRS.Sequence
                            ";


            DataTable dt = _sqlRepository.GetDataTable(sql);
            string id = "";
            dicWIP = new Dictionary<string, double>();


            double WIP = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                double PreProcessProduction = 0;
                if (clsStaticInfo.dbl(dt.Rows[i]["Seq"].ToString()) > 1)
                {

                    WIP = clsStaticInfo.dbl(dt.Rows[i - 1]["Qty"].ToString()) - clsStaticInfo.dbl(dt.Rows[i]["Qty"].ToString());

                    dicWIP.Add(dt.Rows[i]["ProductionOrderId"].ToString() + dt.Rows[i]["ProcessId"].ToString(), WIP);
                }

            }


        }

        private void getOrderMasterForProduction(string ProductionStatusId, string entityid, string todate, out DataTable dtOrderMaster)
        {
            if (string.IsNullOrEmpty(ProductionStatusId) == true || ProductionStatusId.ToUpper() == "ALL")
                ProductionStatusId = "";
            else
                ProductionStatusId = " AND PO.ProductionStatusId IN (" + ProductionStatusId + ")";

            string sql = @"select * from ( SELECT   trkp.UserName AS Plant,trke.UserName AS Entity,mm.UserName AS Material,
                            POD.ProductionOrderId,OC.UserName AS OrderCategory,os.UserName AS OrderStatus,ps.UserName  AS productionStatus,
                           sum(so.Qty) AS SOQty,SUM((so.Qty*(1+(moi.ExtraOrderPercentage/100)))*(1+(moi.OrderWastagePercentage/100))) AS PlannedQty,
                            MAX(so.DeliveryDate) AS DeliveryDate,MAX(SO.CommitmentDate) CommitmentDate,
                            ma.StandardName AS Article,pc.UserName AS ProductCategory,PM.UserName AS Product,
                            
                            MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											 
					                                BuyerOrderNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnOrderNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													BuyerItemNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnItemNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                              FROM trn.MasterOrder MO
                            left outer join trn.MasterOrderItem MOI on moi.MasterOrderId=mo.Id
                            INNER join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
                            LEFT OUTER JOIN TRN.ProductionOrderDetail AS pod ON POD.SalesOrderId=SO.Id
                            LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId

                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PO.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId

                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                            LEFT OUTER JOIN mst.MaterialMasterArticle AS MA ON ma.Id=moi.ArticleId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                            left outer join [HKP].[Party] p on P.Id=MO.plantID
                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId

                             WHERE mo.EntityId IN (" + entityid + @") " + ProductionStatusId + @" and PO.Id in (select distinct ProductionOrderId from trn.ProductionSummary where productionDate<='" + todate + @"')
          
                           GROUP BY     trkp.UserName,trke.UserName,ma.StandardName,pc.UserName,PM.UserName,mm.UserName,
                                                POD.ProductionOrderId,OC.UserName,os.UserName,ps.UserName ) K
                            ORDER BY k.Buyer,k.ProductionOrderId";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);


        }

        public IWorkbook Snapshot2DataXls(string fromDate, string todate)
        {

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {               
                if (bplib.clsWebLib.IsDateOK(fromDate) == false)
                    throw new Exception("Plase select from date");

                if (bplib.clsWebLib.IsDateOK(todate) == false)
                    throw new Exception("Plase select to date");

                if (Convert.ToDateTime(fromDate) > Convert.ToDateTime(todate))
                    throw new Exception("To date cannot be earlier than from date");

                DataTable dtOrderMaster;

                getSnapshot2SQL(fromDate, todate, out dtOrderMaster);

                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[0].Name = "Snapshot 2";
                sheet = workbook.Worksheets[0];

                int ROW = 1; int COL = 1;
                sheet[ROW, 1].Text = "Snapshot 2 Report";
                sheet[ROW, 1].CellStyle.Font.Size = 16;
                sheet[ROW, 1].RowHeight = 22;
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, 10].Merge();
                ROW++;
                sheet[ROW, 1].Text = "From [" + fromDate + "] to [" + todate + "]";
                sheet[ROW, 1].CellStyle.Font.Size = 14;
                sheet[ROW, 1].RowHeight = 20;
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, 10].Merge();
                ROW++;
                
                ROW += 1;
                #region columns

                sheet[ROW, COL].Text = "Snapshot Name";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSnapshotName = COL;
                COL++;

                sheet[ROW, COL].Text = "Snapshot Description";
                sheet[ROW, COL].ColumnWidth = 15;
                int colSnapshotDesc = COL;
                COL++;

                sheet[ROW, COL].Text = "Snapshot Taken By";
                sheet[ROW, COL].ColumnWidth = 15;
                int colSnapshotTakenBy = COL;
                COL++;

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 15;
                int colEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "Work Center";
                sheet[ROW, COL].ColumnWidth = 15;
                int colWorkCenter = COL;
                COL++;

                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 15;
                int colProcess = COL;
                COL++;

                sheet[ROW, COL].Text = "Production Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Quantity";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colQuantity = COL;
                COL++;

                sheet[ROW, COL].Text = "Production Hours";
                sheet[ROW, COL].ColumnWidth = 12;
                int colProductionHours = COL;
                COL++;

                sheet[ROW, COL].Text = "Actual Target";
                sheet[ROW, COL].ColumnWidth = 15;
                int colActualTarget = COL;
                COL++;

                sheet[ROW, COL].Text = "Actual Production";
                sheet[ROW, COL].ColumnWidth = 15;
                int colActualProduction = COL;
                                              
                #endregion columns
                int endCol = COL;

                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(150, 250, 150);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dtOrderMaster.Rows.Count; i++)
                {
                    sheet[ROW, colSnapshotName].Text = dtOrderMaster.Rows[i]["SnapshotName"].ToString();
                    sheet[ROW, colSnapshotDesc].Text = dtOrderMaster.Rows[i]["SnapshotDesc"].ToString();
                    sheet[ROW, colSnapshotTakenBy].Text = dtOrderMaster.Rows[i]["SnapshotTakenBy"].ToString();
                    sheet[ROW, colEntity].Text = dtOrderMaster.Rows[i]["Entity"].ToString();

                    sheet[ROW, colWorkCenter].Text = dtOrderMaster.Rows[i]["WorkCenter"].ToString();
                    sheet[ROW, colProcess].Text = dtOrderMaster.Rows[i]["Process"].ToString();
                    sheet[ROW, colProductionDate].Text = dtOrderMaster.Rows[i]["ProductionDate"].ToString();

                    sheet[ROW, colQuantity].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["Quantity"].ToString());
                    sheet[ROW, colProductionHours].Number = clsStaticInfo.dbl(dtOrderMaster.Rows[i]["ProductionHours"].ToString());
                    sheet[ROW, colActualTarget].Text = dtOrderMaster.Rows[i]["ActualTarget"].ToString();
                    sheet[ROW, colActualProduction].Text = dtOrderMaster.Rows[i]["ActualProduction"].ToString();

                    
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.UsedRange.NumberFormat = "#,##0;[Red](#,##0)";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.UsedRange["A3"].FreezePanes();
                sheet.Range[6, 1].FreezePanes();
                sheet.IsDisplayZeros = false;
             
                //#endregion ******************Report Header******************

                //IWorksheet sheet2 = workbook.Worksheets[1];
                //sheet2.Name = "Reference Data";
                //sheet2.ImportDataTable(dt, true, 1, 1);
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;
                workbook.Version = ExcelVersion.Excel2016;

            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return workbook;

        }
        private void getSnapshot2SQL(string fromDate, string todate, out DataTable dtOrderMaster)
        {
            string sql = @"select S2M.SnapshotName,S2M.SnapshotDesc,S2M.SnapshotTakenBy,E.UserName Entity,W.UserName WorkCenter,P.UserName Process,FORMAT                                                       (S2.ProductionDate,'dd-MMM-yyyy') ProductionDate,S2.Quantity
                                                    ,S2.ProductionHours,S2.ActualTarget,S2.ActualProduction
                                                    from [dbo].[ProductionPlanningSnapshot2Type1] S2
                                                    LEFT JOIN [SCS].[WorkCenterMaster] W ON W.Id=S2.WorkCenterMasterId
                                                    LEFT JOIN MST.MaterialMaster M ON M.Id=S2.MaterialMasterId
                                                    LEFT JOIN ORG.Entity E ON E.Id=S2.EntityId
                                                    LEFT JOIN HKP.Process P ON P.Id=S2.ProcessId
                                                    LEFT JOIN ProductionPlanningSnapshot2MasterType1 S2M ON S2M.Id=S2.ProductionPlanningSnapshot2MasterType1Id
                                                    Where FORMAT(S2.ProductionDate,'dd-MMM-yyyy') between '"+ fromDate + "' AND '"+ todate + "'";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);
        }


        private class ProductionQtyDistributionSO
        {

            public string MasterOrderId { get; set; } = "";
            public string ProductionOrderID { get; set; } = "";
            public string SalesOrderID { get; set; } = "";
            public string DeliveryDate { get; set; } = "";

            public double OrderQty { get; set; } = 0;
            public double PlannedQty { get; set; } = 0;
            public double ProductionUptoPreviousDay { get; set; } = 0;
            public double PlanQtyForToday { get; set; } = 0;
            public double ProducedQtyToday { get; set; } = 0;
            public double CumulativeQty { get; set; } = 0;
            public double SOQty { get; set; } = 0;
            public double DistributedQty { get; set; } = 0;
            public double DistributedQtyToday { get; set; } = 0;

        }
    }
}
