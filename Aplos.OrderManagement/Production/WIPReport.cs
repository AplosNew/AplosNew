using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Library.OrderManagement.Production
{
    public class WIPReport
    {
        SqlRepository _sqlRepository = null;

        public WIPReport()
        {
            _sqlRepository = new SqlRepository();

        }
        public List<Dictionary<string, object>> GetAllCompanies()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return _sqlRepository.GetDataCollection(@"SELECT Id AS CompanyId,c.UserName AS Company
                                                         FROM org.Company AS c WHERE c.[Active]=1 ORDER BY c.Sequence");

        }

        public List<Dictionary<string, object>> GetAllPlants()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return _sqlRepository.GetDataCollection(@"SELECT CompanyId,p.Id AS PlantId,p.UserName AS Plant
                          FROM org.Plant AS p 
                        JOIN org.Company AS c ON c.Id=p.CompanyId
                        WHERE p.[Active]=1 ORDER BY p.Sequence
                          ");

        }
        public string GetAllProcessAndInventory()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return @"SELECT isnull(p.Id,s.Id) AS Id, pais.ProcessId,pais.SFGInventoryId,ISNULL(p.UserName,s.UserName) AS UserName
                        FROM ProcessAndInventorySequence AS pais
                        LEFT JOIN hkp.Process AS p ON p.Id=pais.ProcessId
                        LEFT JOIN hkp.SFGInventory AS s ON s.Id=pais.SFGInventoryId
                        WHERE pais.PlantId='" + identity.PlantId + @"' AND pais.[Active]=1
                        ORDER BY pais.Sequence";

        }
        public IWorkbook GetWIPReportLineWiseNew(string companyId, string PlantId, string date)
        {

            #region declare
            clsReport objRpt = null;
            clsReport objRptSR = null;
            ReportUtility oru = new ReportUtility();
            DataTable dtProcessList = null;
            dtProcessList = _sqlRepository.GetDataTable(GetAllProcessAndInventory());
            if (dtProcessList.Rows.Count == 0)
                throw new Exception("No process/inventory list was defined for this report. Please assign the list in Administration Panel");

            DataTable BaseProcess = _sqlRepository.GetDataTable("SELECT * FROM [dbo].[PlanningTypes] WHERE PlanningType='PlanningType1'");
            string BaseProcessId = "";
            if (BaseProcess.Rows.Count > 0)
                BaseProcessId = BaseProcess.Rows[0]["BaseProcessId"].ToString();
            else
            {
                dtProcessList.DefaultView.RowFilter = "isnull(ProcessId,'')<>''";
                if (dtProcessList.DefaultView.Count > 0)
                    BaseProcessId = dtProcessList.Rows[0]["Id"].ToString();

            }
            dtProcessList.DefaultView.RowFilter = "isnull(Id,'')='" + BaseProcessId + "'";
            if (dtProcessList.DefaultView.Count > 0)
                dtProcessList.DefaultView[0].Delete();
            else
                BaseProcessId = "";

            dtProcessList.DefaultView.RowFilter = null;
            dtProcessList = dtProcessList.DefaultView.ToTable();


            Dictionary<string, Dictionary<string, DataRow>> dicProcessUD = new Dictionary<string, Dictionary<string, DataRow>>();
            Dictionary<string, Dictionary<string, DataRow>> dicProcessFD = new Dictionary<string, Dictionary<string, DataRow>>();


            DataTable dtMainData = new DataTable();
            StringCollection strColPrId = new StringCollection();

            Dictionary<string, DataRow> dicBaseProductionUD;
            Dictionary<string, DataRow> dicProductionUD;


            Dictionary<string, DataRow> dicBaseProductionFD;
            Dictionary<string, DataRow> dicProductionFD;

            #region Base process and Inventory

            #region UD
            GetProductionListWithWCRowProcessWise(BaseProcessId, date, "<=", out dicBaseProductionUD, out strColPrId);
            dicProcessUD.Add(BaseProcessId, dicBaseProductionUD);
            foreach (var item in dicBaseProductionUD)
            {
                if (dtMainData.Columns.Count == 0)
                    dtMainData = item.Value.Table.Clone();
                dtMainData.ImportRow(item.Value);
            }

            #endregion UD

            #region FD
            GetProductionListWithWCRowProcessWise(BaseProcessId, date, "=", out dicBaseProductionFD, out StringCollection strColPrIdFD);
            dicProcessFD.Add(BaseProcessId, dicBaseProductionFD);
            foreach (var item in dicBaseProductionFD)
            {
                if (dicBaseProductionUD.ContainsKey(item.Key))
                    continue;
                if (dtMainData.Columns.Count == 0)
                    dtMainData = item.Value.Table.Clone();
                dtMainData.ImportRow(item.Value);
            }

            #endregion FD
            #endregion Base process and Inventory



            for (int i = 0; i < dtProcessList.Rows.Count; i++)
            {
                #region UD
                if (dtProcessList.Rows[i]["ProcessId"].ToString() != "")
                    GetProductionListWithoutWCRowProcessWise(dtProcessList.Rows[i]["Id"].ToString(), date, "<=", out dicProductionUD);
                else
                    GetProductionListWithoutWCRowInventoryWise(dtProcessList.Rows[i]["Id"].ToString(), date, "<=", out dicProductionUD);


                dicProcessUD.Add(dtProcessList.Rows[i]["Id"].ToString(), dicProductionUD);

                foreach (var item in dicProductionUD)
                {
                    if (strColPrId.Contains(item.Key))
                        continue;
                    strColPrId.Add(item.Key);

                    if (dtMainData.Columns.Count == 0)
                        dtMainData = item.Value.Table.Clone();

                    dtMainData.ImportRow(item.Value);

                }

                #endregion UD

                #region FD
                if (dtProcessList.Rows[i]["ProcessId"].ToString() != "")
                    GetProductionListWithoutWCRowProcessWise(dtProcessList.Rows[i]["Id"].ToString(), date, "=", out dicProductionFD);
                else
                    GetProductionListWithoutWCRowInventoryWise(dtProcessList.Rows[i]["Id"].ToString(), date, "=", out dicProductionFD);


                dicProcessFD.Add(dtProcessList.Rows[i]["Id"].ToString(), dicProductionFD);

                foreach (var item in dicProductionFD)
                {
                    if (strColPrId.Contains(item.Key))
                        continue;
                    strColPrId.Add(item.Key);

                    if (dtMainData.Columns.Count == 0)
                        dtMainData = item.Value.Table.Clone();

                    dtMainData.ImportRow(item.Value);

                }



                #endregion FD
            }


            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();

            string _currencyId = string.Empty;
            #endregion
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;

                IWorkbook workbook = application.Workbooks.Create(1);

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompany = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE Id = '" + companyId + @"'");
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompany.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                var dtCmp = objRptSR.SelectedPlantWiseCompanyDT(PlantId);

                var dtFactory = objRptSR.SelectedPlantDT(PlantId);
                #region Variable
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";

                int SLNo = 1;
                #endregion


                dtProcessList = _sqlRepository.GetDataTable(GetAllProcessAndInventory());
                //workbook = application.Workbooks.Create(4);
                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];

                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;
                xlsRow = 8;
                #region ------------------Column Header------------------
                int iBuyer = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Buyer";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;
                xlsCol++;
                int iCustomer = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Customer";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;
                xlsCol++;
                int iProduct = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Product";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;
                xlsCol++;
                int iPrNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "PR No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8;
                xlsCol++;
                int iPRQty = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "PR Qty";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol++;
                int iBuyerOrder = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Buyer Order#";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int iMaterialMaster = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Material";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;
                xlsCol++;
                int iMaterialMasterArticle = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Article";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;
                xlsCol++;
                int iSoDescription = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "SO.Description";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iWorkcenter = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Workcenter";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int iSoDeliveryDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "SO.Delivery Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 11;




                string FirstProcessId = "";
                Dictionary<string, int> dicProcessColIndex = new Dictionary<string, int>();
                Dictionary<string, int> dicProcessKillColIndex = new Dictionary<string, int>();
                Dictionary<string, int> dicSFGColIndex = new Dictionary<string, int>();
                int FDIN = 0, UDIN = 1, FDOUT = 2, UDOUT = 3,/* FDKILL = 4, UDKILL = 5,*/ PWIP = 4;
                int ColStart = xlsCol + 1;
                for (int i = 0; i < dtProcessList.Rows.Count; i++)
                {
                    xlsCol++;
                    dicProcessColIndex.Add(dtProcessList.Rows[i]["Id"].ToString(), xlsCol);
                    int sCol = xlsCol; int PCol = xlsCol;
                    if (FirstProcessId == "")
                    {
                        FirstProcessId = dtProcessList.Rows[i]["Id"].ToString();

                        sheet1[xlsRow, xlsCol].Text = "FD";
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                        xlsCol++;
                        sheet1[xlsRow, xlsCol].Text = "UD";
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                        sheet1[xlsRow - 1, xlsCol - 1].Text = "OUT"; sheet1.Range[xlsRow - 1, xlsCol - 1, xlsRow - 1, xlsCol].Merge();

                    }
                    else
                    {

                        sheet1[xlsRow, xlsCol].Text = "FD";
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                        xlsCol++;
                        sheet1[xlsRow, xlsCol].Text = "UD";
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                        sheet1[xlsRow - 1, xlsCol - 1].Text = "IN"; sheet1.Range[xlsRow - 1, xlsCol - 1, xlsRow - 1, xlsCol].Merge();
                        xlsCol++;
                        sheet1[xlsRow, xlsCol].Text = "FD";
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                        xlsCol++;
                        sheet1[xlsRow, xlsCol].Text = "UD";
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                        sheet1[xlsRow - 1, xlsCol - 1].Text = "OUT"; sheet1.Range[xlsRow - 1, xlsCol - 1, xlsRow - 1, xlsCol].Merge();
                        xlsCol++;
                        sheet1[xlsRow, xlsCol].Text = "WIP";
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                        sheet1.Range[xlsRow - 1, xlsCol, xlsRow, xlsCol].Merge();
                    }
                    sheet1[xlsRow - 2, sCol].Text = dtProcessList.Rows[i]["UserName"].ToString(); sheet1.Range[xlsRow - 2, sCol, xlsRow - 2, xlsCol].Merge();

                }

                //for kill
                for (int i = 0; i < dtProcessList.Rows.Count; i++)
                {
                    if (dtProcessList.Rows[i]["ProcessId"].ToString() == "")
                        continue;

                    xlsCol++;
                    dicProcessKillColIndex.Add(dtProcessList.Rows[i]["Id"].ToString(), xlsCol);
                    sheet1[xlsRow, xlsCol].Text = "FD";
                    sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                    xlsCol++;
                    sheet1[xlsRow, xlsCol].Text = "UD";
                    sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                    sheet1[xlsRow - 1, xlsCol - 1].Text = dtProcessList.Rows[i]["UserName"].ToString() + " Kill"; sheet1.Range[xlsRow - 2, xlsCol - 1, xlsRow - 1, xlsCol].Merge();
                }

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow - 2, ColStart, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 2, ColStart, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 2, ColStart, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow - 2, ColStart, xlsRow, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet1.Range[xlsRow - 2, ColStart, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow - 2, ColStart, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 2, ColStart, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow - 2, ColStart, xlsRow, endXlsCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;

                int colEndofValues = xlsCol;
                xlsCol++;
                int iProductCat = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Production Category";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int iOrderNos = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Order Nos";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int iOwnOrder = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Own Order#";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;

                xlsCol++;
                int iBuyerItem = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Buyer Item#";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;

                xlsCol++;
                int iOwnItem = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Own Item#";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int iProductionStatus = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Production Status";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int iRemarks = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Remarks";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;


                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;



                xlsRow++;
                #endregion ------------------Column Header------------------
                #region ----------------------Data-----------------------  
                int startXlsRow = xlsRow;

                dtMainData.DefaultView.Sort = "buyer,Customer,Product,ProductionOrderId,WorkCenterSequence,SODesc";
                dtMainData = dtMainData.DefaultView.ToTable();
                for (int i = 0; i < dtMainData.Rows.Count; i++)
                {
                    sheet1[xlsRow, iBuyer].Text = dtMainData.Rows[i]["buyer"].ToString();
                    sheet1[xlsRow, iCustomer].Text = dtMainData.Rows[i]["Customer"].ToString();
                    sheet1[xlsRow, iMaterialMaster].Text = dtMainData.Rows[i]["Material"].ToString();
                    sheet1[xlsRow, iMaterialMasterArticle].Text = dtMainData.Rows[i]["Article"].ToString();
                    sheet1[xlsRow, iProduct].Text = dtMainData.Rows[i]["Product"].ToString();
                    sheet1[xlsRow, iPrNo].Text = dtMainData.Rows[i]["ProductionOrderId"].ToString();
                    sheet1[xlsRow, iPRQty].Number = clsStaticInfo.dbl(dtMainData.Rows[i]["Qty"].ToString());
                    //sheet1[xlsRow, iPRQty].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1[xlsRow, iSoDescription].Text = dtMainData.Rows[i]["SODesc"].ToString();
                    sheet1[xlsRow, iProductCat].Text = dtMainData.Rows[i]["ProductCategory"].ToString();
                    sheet1[xlsRow, iBuyerOrder].Text = dtMainData.Rows[i]["BuyerRefNo"].ToString();
                    sheet1[xlsRow, iSoDeliveryDate].Text = dtMainData.Rows[i]["DeliveryDate"].ToString();
                    sheet1[xlsRow, iOwnItem].Text = dtMainData.Rows[i]["OwnRefNo"].ToString();
                    sheet1[xlsRow, iBuyerItem].Text = dtMainData.Rows[i]["StyleNo"].ToString();
                    sheet1[xlsRow, iOwnOrder].Text = dtMainData.Rows[i]["OwnStyleNo"].ToString();
                    sheet1[xlsRow, iProductionStatus].Text = dtMainData.Rows[i]["ProductionStatus"].ToString();
                    sheet1[xlsRow, iRemarks].Text = dtMainData.Rows[i]["ProductionRemarks"].ToString();
                    sheet1[xlsRow, iWorkcenter].Text = dtMainData.Rows[i]["Workcenter"].ToString();

                    sheet1[xlsRow, iOrderNos].Text = dtMainData.Rows[i]["MasterOrderId"].ToString();

                    int prcidnex = -1;

                    #region UD
                    foreach (var Process in dicProcessUD)
                    {
                        prcidnex++;

                        //sheet1[xlsRow, dicProcessColIndex[Process.Key] + 6].Formula = "SUM(" + clsStaticInfo.GetxlsCol(dicProcessColIndex[Process.Key] + 2) + xlsRow.ToString() + " + " + clsStaticInfo.GetxlsCol(dicProcessColIndex[Process.Key] + 5) + xlsRow.ToString() + ")";
                        DataRow drData;
                        if (Process.Key == BaseProcessId)
                        {
                            if (Process.Value.ContainsKey(dtMainData.Rows[i]["ProductionOrderId"].ToString() + "-" + dtMainData.Rows[i]["WorkCenterMasterId"].ToString()) == false)
                                continue;

                            drData = Process.Value[dtMainData.Rows[i]["ProductionOrderId"].ToString() + "-" + dtMainData.Rows[i]["WorkCenterMasterId"].ToString()];
                            Process.Value.Remove(dtMainData.Rows[i]["ProductionOrderId"].ToString() + "-" + dtMainData.Rows[i]["WorkCenterMasterId"].ToString());
                        }
                        else
                        {
                            if (Process.Value.ContainsKey(dtMainData.Rows[i]["ProductionOrderId"].ToString()) == false)
                                continue;

                            drData = Process.Value[dtMainData.Rows[i]["ProductionOrderId"].ToString()];
                            Process.Value.Remove(dtMainData.Rows[i]["ProductionOrderId"].ToString());

                        }


                        if (FirstProcessId == Process.Key)
                        {
                            sheet1[xlsRow, dicProcessColIndex[Process.Key] + 1].Number = clsStaticInfo.dbl(drData["OutQuantity"].ToString());

                            if (dicProcessKillColIndex.ContainsKey(Process.Key))
                                sheet1[xlsRow, dicProcessKillColIndex[Process.Key] + 1].Number = clsStaticInfo.dbl(drData["KillQuantity"].ToString());
                        }
                        else
                        {

                            sheet1[xlsRow, dicProcessColIndex[Process.Key] + UDIN].Number = clsStaticInfo.dbl(drData["InQuantity"].ToString());
                            sheet1[xlsRow, dicProcessColIndex[Process.Key] + UDOUT].Number = clsStaticInfo.dbl(drData["OutQuantity"].ToString());

                            if (dicProcessKillColIndex.ContainsKey(Process.Key))
                            {
                                sheet1[xlsRow, dicProcessKillColIndex[Process.Key] + 1].Number = clsStaticInfo.dbl(drData["KillQuantity"].ToString());


                                sheet1[xlsRow, dicProcessColIndex[Process.Key] + PWIP].Formula =
                               clsStaticInfo.GetxlsCol(dicProcessColIndex[Process.Key] + UDIN) + xlsRow + "-" +
                               clsStaticInfo.GetxlsCol(dicProcessKillColIndex[Process.Key] + 1) + xlsRow + "-" +
                               clsStaticInfo.GetxlsCol(dicProcessColIndex[Process.Key] + UDOUT) + xlsRow;
                            }
                            else
                            {
                                sheet1[xlsRow, dicProcessColIndex[Process.Key] + PWIP].Formula =
                                clsStaticInfo.GetxlsCol(dicProcessColIndex[Process.Key] + UDIN) + xlsRow + "-" +
                                clsStaticInfo.GetxlsCol(dicProcessColIndex[Process.Key] + UDOUT) + xlsRow;

                            }
                        }
                    }




                    #endregion UD

                    #region FD
                    foreach (var Process in dicProcessFD)
                    {


                        DataRow drData;
                        if (Process.Key == BaseProcessId)
                        {
                            if (Process.Value.ContainsKey(dtMainData.Rows[i]["ProductionOrderId"].ToString() + "-" + dtMainData.Rows[i]["WorkCenterMasterId"].ToString()) == false)
                                continue;

                            drData = Process.Value[dtMainData.Rows[i]["ProductionOrderId"].ToString() + "-" + dtMainData.Rows[i]["WorkCenterMasterId"].ToString()];
                            Process.Value.Remove(dtMainData.Rows[i]["ProductionOrderId"].ToString() + "-" + dtMainData.Rows[i]["WorkCenterMasterId"].ToString());
                        }
                        else
                        {
                            if (Process.Value.ContainsKey(dtMainData.Rows[i]["ProductionOrderId"].ToString()) == false)
                                continue;

                            drData = Process.Value[dtMainData.Rows[i]["ProductionOrderId"].ToString()];
                            Process.Value.Remove(dtMainData.Rows[i]["ProductionOrderId"].ToString());

                        }
                        if (FirstProcessId == Process.Key)
                        {
                            sheet1[xlsRow, dicProcessColIndex[Process.Key] + 0].Number = clsStaticInfo.dbl(drData["OutQuantity"].ToString());
                            if (dicProcessKillColIndex.ContainsKey(Process.Key))
                                sheet1[xlsRow, dicProcessKillColIndex[Process.Key] + 0].Number = clsStaticInfo.dbl(drData["KillQuantity"].ToString());
                        }
                        else
                        {
                            sheet1[xlsRow, dicProcessColIndex[Process.Key] + FDIN].Number = clsStaticInfo.dbl(drData["InQuantity"].ToString());
                            sheet1[xlsRow, dicProcessColIndex[Process.Key] + FDOUT].Number = clsStaticInfo.dbl(drData["OutQuantity"].ToString());
                            if (dicProcessKillColIndex.ContainsKey(Process.Key))
                                sheet1[xlsRow, dicProcessKillColIndex[Process.Key] + 0].Number = clsStaticInfo.dbl(drData["KillQuantity"].ToString());
                        }

                    }




                    #endregion FD




                    xlsRow++;

                }

                sheet1[xlsRow, 1].Text = "Total";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                for (int CL = ColStart; CL <= colEndofValues; CL++)
                {
                    sheet1[xlsRow, CL].Formula = "SUM(" + clsStaticInfo.GetxlsCol(CL) + startXlsRow.ToString() + ":" + clsStaticInfo.GetxlsCol(CL) + (xlsRow - 1).ToString() + ")";
                }
                sheet1.AutoFilters.FilterRange = sheet1.Range[startXlsRow - 1, 1, xlsRow, endXlsCol];
                sheet1.Range[startXlsRow, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, xlsRow - 1, endXlsCol].WrapText = true;
                int sheetEndXlsRow = xlsRow;
                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(3);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "WIP Report As on: " + date;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1["A9"].FreezePanes();


                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$8";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + "" + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "WIP  Report";
                #endregion Page Setup    
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook GetWIPReportLineWiseNewPivot(string companyId, string PlantId, string date)
        {

            #region declare
            clsReport objRpt = null;
            clsReport objRptSR = null;
            ReportUtility oru = new ReportUtility();
            DataTable dtProcessList = null;
            dtProcessList = _sqlRepository.GetDataTable(GetAllProcessAndInventory());
            if (dtProcessList.Rows.Count == 0)
                throw new Exception("No process/inventory list was defined for this report. Please assign the list in Administration Panel");

            DataTable BaseProcess = _sqlRepository.GetDataTable("SELECT * FROM [dbo].[PlanningTypes] WHERE PlanningType='PlanningType1'");
            string BaseProcessId = "";
            if (BaseProcess.Rows.Count > 0)
                BaseProcessId = BaseProcess.Rows[0]["BaseProcessId"].ToString();
            else
            {
                dtProcessList.DefaultView.RowFilter = "isnull(ProcessId,'')<>''";
                if (dtProcessList.DefaultView.Count > 0)
                    BaseProcessId = dtProcessList.Rows[0]["Id"].ToString();

            }
            dtProcessList.DefaultView.RowFilter = "isnull(Id,'')='" + BaseProcessId + "'";
            if (dtProcessList.DefaultView.Count > 0)
                dtProcessList.DefaultView[0].Delete();
            else
                BaseProcessId = "";

            dtProcessList.DefaultView.RowFilter = null;
            dtProcessList = dtProcessList.DefaultView.ToTable();


            Dictionary<string, Dictionary<string, DataRow>> dicProcessUD = new Dictionary<string, Dictionary<string, DataRow>>();
            Dictionary<string, Dictionary<string, DataRow>> dicProcessFD = new Dictionary<string, Dictionary<string, DataRow>>();


            DataTable dtMainData = new DataTable();
            StringCollection strColPrId = new StringCollection();

            Dictionary<string, DataRow> dicBaseProductionUD;
            Dictionary<string, DataRow> dicProductionUD;


            Dictionary<string, DataRow> dicBaseProductionFD;
            Dictionary<string, DataRow> dicProductionFD;

            #region Base process and Inventory

            #region UD
            GetProductionListWithWCRowProcessWise(BaseProcessId, date, "<=", out dicBaseProductionUD, out strColPrId);
            dicProcessUD.Add(BaseProcessId, dicBaseProductionUD);
            foreach (var item in dicBaseProductionUD)
            {
                if (dtMainData.Columns.Count == 0)
                    dtMainData = item.Value.Table.Clone();
                dtMainData.ImportRow(item.Value);
            }

            #endregion UD

            #region FD
            GetProductionListWithWCRowProcessWise(BaseProcessId, date, "=", out dicBaseProductionFD, out StringCollection strColPrIdFD);
            dicProcessFD.Add(BaseProcessId, dicBaseProductionFD);
            foreach (var item in dicBaseProductionFD)
            {
                if (dicBaseProductionUD.ContainsKey(item.Key))
                    continue;
                if (dtMainData.Columns.Count == 0)
                    dtMainData = item.Value.Table.Clone();
                dtMainData.ImportRow(item.Value);
            }

            #endregion FD
            #endregion Base process and Inventory



            for (int i = 0; i < dtProcessList.Rows.Count; i++)
            {
                #region UD
                if (dtProcessList.Rows[i]["ProcessId"].ToString() != "")
                    GetProductionListWithoutWCRowProcessWise(dtProcessList.Rows[i]["Id"].ToString(), date, "<=", out dicProductionUD);
                else
                    GetProductionListWithoutWCRowInventoryWise(dtProcessList.Rows[i]["Id"].ToString(), date, "<=", out dicProductionUD);


                dicProcessUD.Add(dtProcessList.Rows[i]["Id"].ToString(), dicProductionUD);

                foreach (var item in dicProductionUD)
                {
                    if (strColPrId.Contains(item.Key))
                        continue;
                    strColPrId.Add(item.Key);

                    if (dtMainData.Columns.Count == 0)
                        dtMainData = item.Value.Table.Clone();

                    dtMainData.ImportRow(item.Value);

                }

                #endregion UD

                #region FD
                if (dtProcessList.Rows[i]["ProcessId"].ToString() != "")
                    GetProductionListWithoutWCRowProcessWise(dtProcessList.Rows[i]["Id"].ToString(), date, "=", out dicProductionFD);
                else
                    GetProductionListWithoutWCRowInventoryWise(dtProcessList.Rows[i]["Id"].ToString(), date, "=", out dicProductionFD);


                dicProcessFD.Add(dtProcessList.Rows[i]["Id"].ToString(), dicProductionFD);

                foreach (var item in dicProductionFD)
                {
                    if (strColPrId.Contains(item.Key))
                        continue;
                    strColPrId.Add(item.Key);

                    if (dtMainData.Columns.Count == 0)
                        dtMainData = item.Value.Table.Clone();

                    dtMainData.ImportRow(item.Value);

                }



                #endregion FD
            }


            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();

            string _currencyId = string.Empty;
            #endregion
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;

                IWorkbook workbook = application.Workbooks.Create(2);

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    DataTable dtCompany = _sqlRepository.GetDataTable("SELECT * FROM ORG.COMPANY WHERE Id = '" + companyId + @"'");
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), dtCompany.Rows[0]["Image"].ToString());  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                var dtCmp = objRptSR.SelectedPlantWiseCompanyDT(PlantId);

                var dtFactory = objRptSR.SelectedPlantDT(PlantId);
                #region Variable
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";

                int SLNo = 1;
                #endregion


                dtProcessList = _sqlRepository.GetDataTable(GetAllProcessAndInventory());
                //workbook = application.Workbooks.Create(4);
                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[1];

                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;
                xlsRow = 8;
                #region ------------------Column Header------------------
                int iBuyer = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Buyer";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;
                xlsCol++;
                int iCustomer = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Customer";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;
                xlsCol++;
                int iProduct = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Product";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;
                xlsCol++;
                int iPrNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "PR No";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8;
                xlsCol++;
                int iPrQty = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "PR Qty";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol++;
                int iBuyerOrder = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Buyer Order#";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int iMaterialMaster = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Material";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;
                xlsCol++;
                int iMaterialMasterArticle = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Article";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 14;
                xlsCol++;
                int iSoDescription = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "SO.Description";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol++;
                int iWorkcenter = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Workcenter";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int iSoDeliveryDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "SO.Delivery Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 11;



                Dictionary<string, int> PivotColumnIndex = new Dictionary<string, int>();

                string FirstProcessId = "";
                Dictionary<string, int> dicProcessColIndex = new Dictionary<string, int>();
                Dictionary<string, int> dicProcessKillColIndex = new Dictionary<string, int>();
                Dictionary<string, int> dicSFGColIndex = new Dictionary<string, int>();
                int FDIN = 0, UDIN = 1, FDOUT = 2, UDOUT = 3,/* FDKILL = 4, UDKILL = 5,*/ PWIP = 4;
                int ColStart = xlsCol + 1;
                for (int i = 0; i < dtProcessList.Rows.Count; i++)
                {
                    xlsCol++;
                    dicProcessColIndex.Add(dtProcessList.Rows[i]["Id"].ToString(), xlsCol);
                    int sCol = xlsCol; int PCol = xlsCol;
                    if (FirstProcessId == "")
                    {
                        FirstProcessId = dtProcessList.Rows[i]["Id"].ToString();

                        sheet1[xlsRow, xlsCol].Text = dtProcessList.Rows[i]["UserName"].ToString() + " FD Prod"; PivotColumnIndex.Add(sheet1[xlsRow, xlsCol].Text, xlsCol);
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                        xlsCol++;
                        sheet1[xlsRow, xlsCol].Text = dtProcessList.Rows[i]["UserName"].ToString() + " UD Prod"; PivotColumnIndex.Add(sheet1[xlsRow, xlsCol].Text, xlsCol);
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;


                    }
                    else
                    {

                        sheet1[xlsRow, xlsCol].Text = dtProcessList.Rows[i]["UserName"].ToString() + " FD IN"; PivotColumnIndex.Add(sheet1[xlsRow, xlsCol].Text, xlsCol);
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                        xlsCol++;
                        sheet1[xlsRow, xlsCol].Text = dtProcessList.Rows[i]["UserName"].ToString() + " UD IN"; PivotColumnIndex.Add(sheet1[xlsRow, xlsCol].Text, xlsCol);
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                        xlsCol++;
                        sheet1[xlsRow, xlsCol].Text = dtProcessList.Rows[i]["UserName"].ToString() + " FD OUT"; PivotColumnIndex.Add(sheet1[xlsRow, xlsCol].Text, xlsCol);
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                        xlsCol++;
                        sheet1[xlsRow, xlsCol].Text = dtProcessList.Rows[i]["UserName"].ToString() + " UD OUT"; PivotColumnIndex.Add(sheet1[xlsRow, xlsCol].Text, xlsCol);
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                        xlsCol++;
                        sheet1[xlsRow, xlsCol].Text = dtProcessList.Rows[i]["UserName"].ToString() + " WIP"; PivotColumnIndex.Add(sheet1[xlsRow, xlsCol].Text, xlsCol);
                        sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                    }

                }
                int endXlsColWIP = xlsCol;
                //for kill
                for (int i = 0; i < dtProcessList.Rows.Count; i++)
                {
                    if (dtProcessList.Rows[i]["ProcessId"].ToString() == "")
                        continue;

                    xlsCol++;
                    dicProcessKillColIndex.Add(dtProcessList.Rows[i]["Id"].ToString(), xlsCol);
                    sheet1[xlsRow, xlsCol].Text = dtProcessList.Rows[i]["UserName"].ToString() + " Kill FD"; PivotColumnIndex.Add(sheet1[xlsRow, xlsCol].Text, xlsCol);
                    sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                    xlsCol++;
                    sheet1[xlsRow, xlsCol].Text = dtProcessList.Rows[i]["UserName"].ToString() + " Kill UD"; PivotColumnIndex.Add(sheet1[xlsRow, xlsCol].Text, xlsCol);
                    sheet1[xlsRow, xlsCol].ColumnWidth = 7;
                }

                int endXlsColKill = xlsCol;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, ColStart, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, ColStart, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, ColStart, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, ColStart, xlsRow, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet1.Range[xlsRow, ColStart, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, ColStart, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, ColStart, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, ColStart, xlsRow, endXlsCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;

                int colEndofValues = xlsCol;
                xlsCol++;
                int iProductCat = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Production Category";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int iOrderNos = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Order Nos";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int iOwnOrder = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Own Order#";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;

                xlsCol++;
                int iBuyerItem = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Buyer Item#";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;

                xlsCol++;
                int iOwnItem = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Own Item#";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int iProductionStatus = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Production Status";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int iRemarks = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Remarks";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;


                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;



                xlsRow++;
                #endregion ------------------Column Header------------------
                #region ----------------------Data-----------------------  
                int startXlsRow = xlsRow;

                dtMainData.DefaultView.Sort = "buyer,Customer,Product,ProductionOrderId,WorkCenterSequence,SODesc";
                dtMainData = dtMainData.DefaultView.ToTable();
                for (int i = 0; i < dtMainData.Rows.Count; i++)
                {
                    sheet1[xlsRow, iBuyer].Text = dtMainData.Rows[i]["buyer"].ToString();
                    sheet1[xlsRow, iCustomer].Text = dtMainData.Rows[i]["Customer"].ToString();
                    sheet1[xlsRow, iMaterialMaster].Text = dtMainData.Rows[i]["Material"].ToString();
                    sheet1[xlsRow, iMaterialMasterArticle].Text = dtMainData.Rows[i]["Article"].ToString();
                    sheet1[xlsRow, iProduct].Text = dtMainData.Rows[i]["Product"].ToString();
                    sheet1[xlsRow, iPrNo].Text = dtMainData.Rows[i]["ProductionOrderId"].ToString();
                    sheet1[xlsRow, iPrQty].Number = clsStaticInfo.dbl(dtMainData.Rows[i]["Qty"].ToString());
                    sheet1[xlsRow, iPrQty].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet1[xlsRow, iSoDescription].Text = dtMainData.Rows[i]["SODesc"].ToString();
                    sheet1[xlsRow, iProductCat].Text = dtMainData.Rows[i]["ProductCategory"].ToString();
                    sheet1[xlsRow, iBuyerOrder].Text = dtMainData.Rows[i]["BuyerRefNo"].ToString();
                    sheet1[xlsRow, iSoDeliveryDate].Text = dtMainData.Rows[i]["DeliveryDate"].ToString();
                    sheet1[xlsRow, iOwnItem].Text = dtMainData.Rows[i]["OwnRefNo"].ToString();
                    sheet1[xlsRow, iBuyerItem].Text = dtMainData.Rows[i]["StyleNo"].ToString();
                    sheet1[xlsRow, iOwnOrder].Text = dtMainData.Rows[i]["OwnStyleNo"].ToString();
                    sheet1[xlsRow, iProductionStatus].Text = dtMainData.Rows[i]["ProductionStatus"].ToString();
                    sheet1[xlsRow, iRemarks].Text = dtMainData.Rows[i]["ProductionRemarks"].ToString();
                    sheet1[xlsRow, iWorkcenter].Text = dtMainData.Rows[i]["Workcenter"].ToString();

                    sheet1[xlsRow, iOrderNos].Text = dtMainData.Rows[i]["MasterOrderId"].ToString();

                    int prcidnex = -1;

                    #region UD
                    foreach (var Process in dicProcessUD)
                    {
                        prcidnex++;

                        //sheet1[xlsRow, dicProcessColIndex[Process.Key] + 6].Formula = "SUM(" + clsStaticInfo.GetxlsCol(dicProcessColIndex[Process.Key] + 2) + xlsRow.ToString() + " + " + clsStaticInfo.GetxlsCol(dicProcessColIndex[Process.Key] + 5) + xlsRow.ToString() + ")";
                        DataRow drData;
                        if (Process.Key == BaseProcessId)
                        {
                            if (Process.Value.ContainsKey(dtMainData.Rows[i]["ProductionOrderId"].ToString() + "-" + dtMainData.Rows[i]["WorkCenterMasterId"].ToString()) == false)
                                continue;

                            drData = Process.Value[dtMainData.Rows[i]["ProductionOrderId"].ToString() + "-" + dtMainData.Rows[i]["WorkCenterMasterId"].ToString()];
                            Process.Value.Remove(dtMainData.Rows[i]["ProductionOrderId"].ToString() + "-" + dtMainData.Rows[i]["WorkCenterMasterId"].ToString());
                        }
                        else
                        {
                            if (Process.Value.ContainsKey(dtMainData.Rows[i]["ProductionOrderId"].ToString()) == false)
                                continue;

                            drData = Process.Value[dtMainData.Rows[i]["ProductionOrderId"].ToString()];
                            Process.Value.Remove(dtMainData.Rows[i]["ProductionOrderId"].ToString());

                        }


                        if (FirstProcessId == Process.Key)
                        {
                            sheet1[xlsRow, dicProcessColIndex[Process.Key] + 1].Number = clsStaticInfo.dbl(drData["OutQuantity"].ToString());

                            if (dicProcessKillColIndex.ContainsKey(Process.Key))
                                sheet1[xlsRow, dicProcessKillColIndex[Process.Key] + 1].Number = clsStaticInfo.dbl(drData["KillQuantity"].ToString());
                        }
                        else
                        {

                            sheet1[xlsRow, dicProcessColIndex[Process.Key] + UDIN].Number = clsStaticInfo.dbl(drData["InQuantity"].ToString());
                            sheet1[xlsRow, dicProcessColIndex[Process.Key] + UDOUT].Number = clsStaticInfo.dbl(drData["OutQuantity"].ToString());

                            if (dicProcessKillColIndex.ContainsKey(Process.Key))
                            {
                                sheet1[xlsRow, dicProcessKillColIndex[Process.Key] + 1].Number = clsStaticInfo.dbl(drData["KillQuantity"].ToString());


                                sheet1[xlsRow, dicProcessColIndex[Process.Key] + PWIP].Formula =
                               clsStaticInfo.GetxlsCol(dicProcessColIndex[Process.Key] + UDIN) + xlsRow + "-" +
                               clsStaticInfo.GetxlsCol(dicProcessKillColIndex[Process.Key] + 1) + xlsRow + "-" +
                               clsStaticInfo.GetxlsCol(dicProcessColIndex[Process.Key] + UDOUT) + xlsRow;
                            }
                            else
                            {

                                sheet1[xlsRow, dicProcessColIndex[Process.Key] + PWIP].Formula =
                                 clsStaticInfo.GetxlsCol(dicProcessColIndex[Process.Key] + UDIN) + xlsRow + "-" +
                                 clsStaticInfo.GetxlsCol(dicProcessColIndex[Process.Key] + UDOUT) + xlsRow;
                            }
                        }
                    }




                    #endregion UD

                    #region FD
                    foreach (var Process in dicProcessFD)
                    {


                        DataRow drData;
                        if (Process.Key == BaseProcessId)
                        {
                            if (Process.Value.ContainsKey(dtMainData.Rows[i]["ProductionOrderId"].ToString() + "-" + dtMainData.Rows[i]["WorkCenterMasterId"].ToString()) == false)
                                continue;

                            drData = Process.Value[dtMainData.Rows[i]["ProductionOrderId"].ToString() + "-" + dtMainData.Rows[i]["WorkCenterMasterId"].ToString()];
                            Process.Value.Remove(dtMainData.Rows[i]["ProductionOrderId"].ToString() + "-" + dtMainData.Rows[i]["WorkCenterMasterId"].ToString());
                        }
                        else
                        {
                            if (Process.Value.ContainsKey(dtMainData.Rows[i]["ProductionOrderId"].ToString()) == false)
                                continue;

                            drData = Process.Value[dtMainData.Rows[i]["ProductionOrderId"].ToString()];
                            Process.Value.Remove(dtMainData.Rows[i]["ProductionOrderId"].ToString());

                        }
                        if (FirstProcessId == Process.Key)
                        {
                            sheet1[xlsRow, dicProcessColIndex[Process.Key] + 0].Number = clsStaticInfo.dbl(drData["OutQuantity"].ToString());
                            if (dicProcessKillColIndex.ContainsKey(Process.Key))
                                sheet1[xlsRow, dicProcessKillColIndex[Process.Key] + 0].Number = clsStaticInfo.dbl(drData["KillQuantity"].ToString());
                        }
                        else
                        {
                            sheet1[xlsRow, dicProcessColIndex[Process.Key] + FDIN].Number = clsStaticInfo.dbl(drData["InQuantity"].ToString());
                            sheet1[xlsRow, dicProcessColIndex[Process.Key] + FDOUT].Number = clsStaticInfo.dbl(drData["OutQuantity"].ToString());
                            if (dicProcessKillColIndex.ContainsKey(Process.Key))
                                sheet1[xlsRow, dicProcessKillColIndex[Process.Key] + 0].Number = clsStaticInfo.dbl(drData["KillQuantity"].ToString());
                        }

                    }




                    #endregion FD




                    xlsRow++;

                }

                sheet1[xlsRow, 1].Text = "Total";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                for (int CL = ColStart; CL <= colEndofValues; CL++)
                {
                    sheet1[xlsRow, CL].Formula = "SUM(" + clsStaticInfo.GetxlsCol(CL) + startXlsRow.ToString() + ":" + clsStaticInfo.GetxlsCol(CL) + (xlsRow - 1).ToString() + ")";
                }
                sheet1.AutoFilters.FilterRange = sheet1.Range[startXlsRow - 1, 1, xlsRow, endXlsCol];
                sheet1.Range[startXlsRow, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, xlsRow - 1, endXlsCol].WrapText = true;
                int sheetEndXlsRow = xlsRow;
                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet1.UsedRange["A9"].FreezePanes();


                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeaderNew(ref sheet1, 1, "WIP Report (As on " + date + ")", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);
                sheet1[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[1, 1, 6, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                sheet1.IsGridLinesVisible = false;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$8";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + "" + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "WIP  Report";
                #endregion Page Setup    


                // IChartShape chart = sheet1.Charts.Add();
                // chart.XPos = endXlsCol;
                // //Set chart type
                // chart.ChartType = ExcelChartType.Line;

                // //Set Chart Title
                // chart.ChartTitle = "Cutting Cage";

                // //Set first serie
                // IChartSerie productA = chart.Series.Add("ProductA");
                // productA.Values = sheet1.Range["L9:L" + (xlsRow - 1)];
                //// productA.CategoryLabels = sheet1.Range["A2:A6"];

                // //Set second serie
                // IChartSerie productB = chart.Series.Add("ProductB");
                // productB.Values = sheet1.Range["N9:N" + (xlsRow - 1)];
                // //productB.CategoryLabels = sheet1.Range["A2:A6"];




                IWorksheet sheet = workbook.Worksheets[0];
                int ROW = 6, COL = 1;

                string fPath = fPath = HostingEnvironment.MapPath("~/") + "TempReport" + identity.UserId + ".xlsx";


                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }
                #region Buyer Summary
                workbook.Worksheets[0].Name = "Pivot";
                IWorksheet pivotSheet = workbook.Worksheets[0];

                IPivotCache cache = workbook.PivotCaches.Add(sheet1[startXlsRow - 1, 1, xlsRow - 1, endXlsCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);

                pivotTable.Fields[iBuyer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[iCustomer - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[iProduct - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[iMaterialMaster - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[iMaterialMasterArticle - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[iPrNo - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[iPrQty - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[iBuyerOrder - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[iSoDescription - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[iWorkcenter - 1].Axis = PivotAxisTypes.Row;

                pivotTable.Fields[iPrQty - 1].Subtotals = PivotSubtotalTypes.None;


                foreach (var colindex in PivotColumnIndex)
                {
                    IPivotField field = pivotTable.Fields[colindex.Value - 1];
                    field.NumberFormat = clsStaticInfo.NumberFormat();
                    pivotTable.DataFields.Add(field, colindex.Key, PivotSubtotalTypes.Sum);
                }


                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Pivot WIP", identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;


                #endregion Buyer Summary


                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public IWorkbook GetWIPReportProcessWise(string ProcessId, string date)
        {

            #region declare
            clsReport objRpt = null;
            clsReport objRptSR = null;
            ReportUtility oru = new ReportUtility();
            DataTable dtProcessList = null;


            DataTable dtMainData = new DataTable();
            DataTable dtSFGinventory = new DataTable();

            StringCollection strColPrId = new StringCollection();

            GetProductionListWithWCRowProcessWise(ProcessId, date, "<=", out Dictionary<string, DataRow> dicProduction, out StringCollection strcol);

            foreach (var item in dicProduction)
            {
                if (strColPrId.Contains(item.Key))
                    continue;
                strColPrId.Add(item.Key);

                if (dtMainData.Columns.Count == 0)
                    dtMainData = item.Value.Table.Clone();

                dtMainData.ImportRow(item.Value);

            }

            dtProcessList = _sqlRepository.GetDataTable(@"SELECT * FROM HKP.Process WHERE Id='" + ProcessId + "'");

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();

            string _currencyId = string.Empty;
            #endregion
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;

                IWorkbook workbook = application.Workbooks.Create(1);


                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");


                #region Variable
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";

                int SLNo = 1;
                #endregion



                //workbook = application.Workbooks.Create(4);
                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];

                xlsRow = 6;
                sheet1.Range[xlsRow, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].RowHeight = 20;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, 10].Merge();
                xlsRow++;
                #region ------------------Column Header------------------
                int iPrNo = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Production Order#";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int colWorkcenter = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Workcenter";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 12;
                xlsCol++;
                int iProductCat = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Production Category";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19;
                xlsCol++;
                int iBuyer = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Buyer";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 32;
                xlsCol++;
                int iOrderNos = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Order Nos";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8;

                xlsCol++;
                int iBuyerOrder = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Buyer Order#";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19;
                xlsCol++;
                int iSoDescription = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "So.Description";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;

                xlsCol++;
                int iSoDeliveryDate = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "So.Delivery Date";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 18;


                xlsCol++;
                int iProduct = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Product";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20;

                xlsCol++;
                int iCustomer = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Customer";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;

                xlsCol++;
                int iOwnOrder = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Own Order#";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;

                xlsCol++;
                int iBuyerItem = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Buyer Item#";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;

                xlsCol++;
                int iOwnItem = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Own Item#";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;
                int iProductionStatus = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Production Status";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;
                int iRemarks = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Remarks";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30;
                xlsCol++;
                int colIn = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "In";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int colOut = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Out";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int colKill = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Kill";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                xlsCol++;
                int colWIP = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "WIP";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                xlsRow++;
                #endregion ------------------Column Header------------------
                #region ----------------------Data-----------------------  
                int startXlsRow = xlsRow;
                for (int i = 0; i < dtMainData.Rows.Count; i++)
                {
                    sheet1[xlsRow, iPrNo].Text = dtMainData.Rows[i]["ProductionOrderId"].ToString();
                    sheet1[xlsRow, colWorkcenter].Text = dtMainData.Rows[i]["WorkCenter"].ToString();
                    sheet1[xlsRow, iProductCat].Text = dtMainData.Rows[i]["ProductCategory"].ToString();
                    sheet1[xlsRow, iBuyer].Text = dtMainData.Rows[i]["buyer"].ToString();
                    sheet1[xlsRow, iBuyerOrder].Text = dtMainData.Rows[i]["BuyerRefNo"].ToString();
                    sheet1[xlsRow, iSoDescription].Text = dtMainData.Rows[i]["SODesc"].ToString();
                    sheet1[xlsRow, iSoDeliveryDate].Text = dtMainData.Rows[i]["DeliveryDate"].ToString();
                    sheet1[xlsRow, iProduct].Text = dtMainData.Rows[i]["Product"].ToString();
                    sheet1[xlsRow, iCustomer].Text = dtMainData.Rows[i]["Customer"].ToString();
                    sheet1[xlsRow, iOwnItem].Text = dtMainData.Rows[i]["OwnRefNo"].ToString();
                    sheet1[xlsRow, iBuyerItem].Text = dtMainData.Rows[i]["StyleNo"].ToString();
                    sheet1[xlsRow, iOwnOrder].Text = dtMainData.Rows[i]["OwnStyleNo"].ToString();
                    sheet1[xlsRow, iProductionStatus].Text = dtMainData.Rows[i]["ProductionStatus"].ToString();
                    sheet1[xlsRow, iRemarks].Text = dtMainData.Rows[i]["ProductionRemarks"].ToString();

                    sheet1[xlsRow, iOrderNos].Text = dtMainData.Rows[i]["MasterOrderId"].ToString();

                    string KEY = dtMainData.Rows[i]["ProductionOrderId"].ToString() + "-" + dtMainData.Rows[i]["WorkCenterMasterId"].ToString();
                    if (dicProduction.ContainsKey(KEY))
                    {
                        sheet1[xlsRow, colIn].Number = clsStaticInfo.dbl(dicProduction[KEY]["InQuantity"].ToString());
                        sheet1[xlsRow, colOut].Number = clsStaticInfo.dbl(dicProduction[KEY]["OutQuantity"].ToString());
                        sheet1[xlsRow, colKill].Number = clsStaticInfo.dbl(dicProduction[KEY]["KillQuantity"].ToString());
                    }

                    sheet1[xlsRow, colWIP].Formula = clsStaticInfo.GetxlsCol(colIn) + xlsRow.ToString() + "-" + clsStaticInfo.GetxlsCol(colOut) + xlsRow.ToString() + "-" + clsStaticInfo.GetxlsCol(colKill) + xlsRow.ToString();

                    xlsRow++;

                }


                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;
                int sheetEndXlsRow = xlsRow;
                #endregion ----------------------Data-----------------------



                xlsRow = 1;
                xlsCol = 3;


                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;



                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1[8, 3].FreezePanes();


                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.NumberFormat = clsStaticInfo.NumberFormat(0);

                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet1, endXlsCol, "Workcenter Wise WIP for " + dtProcessList.Rows[0]["UserName"].ToString() + "(as on  " + date + ")", identity.PlantId);
                reportUtility.PageSetup(ref sheet1, 6, ExcelPageOrientation.Landscape);
                sheet1.Range[1, 1, 6, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;



                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$8";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + "" + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "WIP  Report";
                #endregion Page Setup    
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void GetProductionListRow(string processId, string date, out Dictionary<string, DataRow> prOrderDic)
        {
            prOrderDic = new Dictionary<string, DataRow>();

            try
            {
                DataTable dt = null;

                string strSql = "";

                strSql = @"SELECT * FROM 
                            (SELECT
                            PO.Id,isnull(po.Remarks,'') AS ProductionRemarks,isnull(s.UserName,'') AS ProductionStatus, isnull(EN.UserName,'') AS EntityName,
                            
                            isnull(PS.UserName,'') AS ProductionStatusName,SO.*
                            FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            LEFT OUTER JOIN (select
                            pod.ProductionOrderId POID,
                            mm.userName AS Material,PM.UserName AS Product,pc.UserName AS ProductCategory,PM.Id ProductMasterId,
                            -- Min(LSD) AS LSD,max(CommitmentDate) AS CommitmentDate ,
                            sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,
                            MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SONo=STUFF((select distinct ','+sox.Id from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SODesc=STUFF((select distinct ','+sox.[Description] from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            buyer=STUFF((select distinct ','+XB.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            
                            Customer=STUFF((select distinct ','+XP.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            
                            from
                            
                            
                            trn.SalesOrder SO
                            JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                            
                            group by pod.ProductionOrderId,mm.userName,PM.UserName,pc.UserName,PM.Id) AS SO ON so.POID=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId) prDetail 
                            inner JOIN 
                            (SELECT ps.ProductionOrderId,
                            SUM(CASE WHEN ps.ToProcessId='" + processId + @"' THEN ps.Quantity ELSE 0 END) AS InQuantity,
                            SUM(CASE WHEN ps.ProcessId='" + processId + @"' THEN ps.Quantity ELSE 0 END) AS OutQuantity ,
                            SUM(CASE WHEN ps.ToProcessId='" + processId + @"' THEN ps.Quantity ELSE 0 END) -
                            SUM(CASE WHEN ps.ProcessId='" + processId + @"' THEN ps.Quantity ELSE 0 END) AS WIP 
                            FROM TRN.ProductionSummary AS ps
                            WHERE (ps.ProcessId='" + processId + @"' OR ps.ToProcessId='" + processId + @"') AND ps.ProductionDate<='" + date + @"'
                            GROUP BY ps.ProductionOrderId) prSum ON prDetail.Id = prSum.ProductionOrderId";

                dt = _sqlRepository.GetDataTable(strSql);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    prOrderDic.Add(dt.Rows[i]["ProductionOrderId"].ToString(), dt.Rows[i]);
                }
            }
            catch (Exception ex)
            {

            }
        }




        private void GetProductionListWithWCRowProcessWise(string processId, string date, string Criteria, out Dictionary<string, DataRow> prOrderDic, out StringCollection strColProductionOrder)
        {
            prOrderDic = new Dictionary<string, DataRow>();
            strColProductionOrder = new StringCollection();
            try
            {
                DataTable dt = null;

                string strSql = "";

                strSql = @"SELECT prDetail.*,prsum.*,wcm.UserName AS WorkCenter,wcm.Sequence AS WorkCenterSequence,e.UserName AS Entity,p.UserName AS Plant FROM 
                            (SELECT
                            PO.Id,case when ISNULL(  POS.Qty, 0)=0 then PO.PlannedQty else POS.Qty end as Qty,isnull(po.Remarks,'') AS ProductionRemarks,isnull(s.UserName,'') AS ProductionStatus, isnull(EN.UserName,'') AS EntityName,
                           
                            isnull(PS.UserName,'') AS ProductionStatusName,SO.*
                            FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
							LEFT  OUTER JOIN ProductionOrderSchedulingParametersType1 POS on POS.ProductionOrderID=PO.Id 
                            LEFT OUTER JOIN (select
                            pod.ProductionOrderId POID,
                            mm.userName AS Material,MMA.StandardName AS Article,PM.UserName AS Product,pc.UserName AS ProductCategory,PM.Id ProductMasterId,
                            -- Min(LSD) AS LSD,max(CommitmentDate) AS CommitmentDate ,
                            sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,
                            MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SONo=STUFF((select distinct ','+sox.Id from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SODesc=STUFF((select distinct ','+sox.[Description] from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            buyer=STUFF((select distinct ','+XB.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            
                            Customer=STUFF((select distinct ','+XP.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            
                            from
                            
                            
                            trn.SalesOrder SO
                            JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                            left outer join mst.MaterialMasterArticle mma on mma.id=MOI.ArticleId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                            
                            group by pod.ProductionOrderId,mm.userName,mma.StandardName,PM.UserName,pc.UserName,PM.Id) AS SO ON so.POID=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE (isnull(s.StandardName,'')<>'Closed' OR convert(date,po.ClosingDate)>CONVERT(DATE,'" + date + @"'))) prDetail 
                                                 INNER JOIN 
                            (	
                                select ProductionOrderId,WorkCenterMasterId,SUM(InQuantity) AS InQuantity,SUM(OutQuantity) AS OutQuantity,SUM(KillQuantity) AS KillQuantity   from 
			                        (SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS InQuantity,0 AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ToProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)" + Criteria + @"convert(date,'" + date + @"') 

			                         union all 
			 
			                         SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)" + Criteria + @"convert(date,'" + date + @"') 

			                          union all 
			 
			                         SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END  AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)" + Criteria + @"convert(date,'" + date + @"') 
                                    
                                    union all 
			 
			                         SELECT q.ProductionOrderId,q.WorkCenterMasterID,0 AS InQuantity,0 AS OutQuantity,isnull(q.DefectiveQty,0) AS  KillQuantity
                                      FROM trn.Quality AS q
                                      JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=q.WorkCenterMasterID
			                         WHERE wcm.ProcessId='" + processId + @"' AND convert(date,Q.ProductionDate)" + Criteria + @"convert(date,'" + date + @"') 
			                ) AS K group by ProductionOrderId,WorkCenterMasterId) prSum ON prDetail.Id = prSum.ProductionOrderId
                            left join scs.WorkCenterMaster AS wcm ON wcm.Id = prSum.WorkCenterMasterId
                            JOIN org.Entity AS e ON e.Id=wcm.EntityId
                            JOIN org.Plant AS p ON p.Id=e.PlantId

				ORDER BY wcm.Sequence";

                dt = _sqlRepository.GetDataTable(strSql);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    prOrderDic.Add(dt.Rows[i]["ProductionOrderId"].ToString() + "-" + dt.Rows[i]["WorkCenterMasterId"].ToString(), dt.Rows[i]);
                    if (strColProductionOrder.Contains(dt.Rows[i]["ProductionOrderId"].ToString()) == false)
                        strColProductionOrder.Add(dt.Rows[i]["ProductionOrderId"].ToString());
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void GetProductionListWithoutWCRowProcessWise(string processId, string date, string Criteria, out Dictionary<string, DataRow> prOrderDic)
        {
            prOrderDic = new Dictionary<string, DataRow>();

            try
            {
                DataTable dt = null;

                string strSql = "";

                strSql = @"SELECT prDetail.*,prsum.*,wcm.UserName AS WorkCenter,wcm.Sequence AS WorkCenterSequence,e.UserName AS Entity,p.UserName AS Plant FROM 
                            (SELECT EN.Id AS EntityId,EN.PlantId,PO.ClosingDate,
                            PO.Id,isnull(po.Remarks,'') AS ProductionRemarks,isnull(s.UserName,'') AS ProductionStatus, isnull(EN.UserName,'') AS EntityName,
                            
                            isnull(PS.UserName,'') AS ProductionStatusName,SO.*
                            FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            LEFT OUTER JOIN (select
                            pod.ProductionOrderId POID,
                            mm.userName AS Material,MMA.StandardName AS Article,PM.UserName AS Product,pc.UserName AS ProductCategory,PM.Id ProductMasterId,
                            -- Min(LSD) AS LSD,max(CommitmentDate) AS CommitmentDate ,
                            sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,
                            MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SONo=STUFF((select distinct ','+sox.Id from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SODesc=STUFF((select distinct ','+sox.[Description] from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            buyer=STUFF((select distinct ','+XB.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            
                            Customer=STUFF((select distinct ','+XP.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            
                            from
                            
                            
                            trn.SalesOrder SO
                            JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                            left outer join mst.MaterialMasterArticle mma on mma.id=MOI.ArticleId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                            
                            group by pod.ProductionOrderId,mm.userName,mma.StandardName,PM.UserName,pc.UserName,PM.Id) AS SO ON so.POID=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE (isnull(s.StandardName,'')<>'Closed' OR convert(date,po.ClosingDate)>CONVERT(DATE,'" + date + @"'))) prDetail 
                                                 INNER JOIN 
                            (	
                                select ProductionOrderId,'' AS WorkCenterMasterId,SUM(InQuantity) AS InQuantity,SUM(OutQuantity) AS OutQuantity,SUM(KillQuantity) AS KillQuantity   from 
			                        (SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS InQuantity,0 AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ToProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)" + Criteria + @"convert(date,'" + date + @"') 

			                         union all 
			 
			                         SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)" + Criteria + @"convert(date,'" + date + @"') 

			                          union all 
			 
			                         SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END  AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)" + Criteria + @"convert(date,'" + date + @"') 
                                    union all 
			 
			                         SELECT q.ProductionOrderId,q.WorkCenterMasterID,0 AS InQuantity,0 AS OutQuantity,isnull(q.DefectiveQty,0) AS  KillQuantity
                                      FROM trn.Quality AS q
                                      JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=q.WorkCenterMasterID
			                         WHERE wcm.ProcessId='" + processId + @"' AND convert(date,Q.ProductionDate)" + Criteria + @"convert(date,'" + date + @"') 
			                ) AS K group by ProductionOrderId) prSum ON prDetail.Id = prSum.ProductionOrderId
                            left JOIN scs.WorkCenterMaster AS wcm ON wcm.Id = prSum.WorkCenterMasterId
                            JOIN org.Entity AS e ON e.Id=prDetail.EntityId
                            JOIN org.Plant AS p ON p.Id=prDetail.PlantId";

                dt = _sqlRepository.GetDataTable(strSql);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    prOrderDic.Add(dt.Rows[i]["ProductionOrderId"].ToString(), dt.Rows[i]);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void GetProductionListWithoutWCRowInventoryWise(string InventoryId, string date, string Criteria, out Dictionary<string, DataRow> prOrderDic)
        {
            prOrderDic = new Dictionary<string, DataRow>();

            try
            {
                DataTable dt = null;

                string strSql = "";

                strSql = @"SELECT prDetail.*,prsum.*,wcm.UserName AS WorkCenter,wcm.Sequence AS WorkCenterSequence,e.UserName AS Entity,p.UserName AS Plant FROM 
                            (SELECT EN.Id AS EntityId,EN.PlantId,PO.ClosingDate,
                            PO.Id,isnull(po.Remarks,'') AS ProductionRemarks,isnull(s.UserName,'') AS ProductionStatus, isnull(EN.UserName,'') AS EntityName,
                            
                            isnull(PS.UserName,'') AS ProductionStatusName,SO.*
                            FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            LEFT OUTER JOIN (select
                            pod.ProductionOrderId POID,
                            mm.userName AS Material,MMA.StandardName AS Article,PM.UserName AS Product,pc.UserName AS ProductCategory,PM.Id ProductMasterId,
                            -- Min(LSD) AS LSD,max(CommitmentDate) AS CommitmentDate ,
                            sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,
                            MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SONo=STUFF((select distinct ','+sox.Id from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SODesc=STUFF((select distinct ','+sox.[Description] from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            buyer=STUFF((select distinct ','+XB.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            
                            Customer=STUFF((select distinct ','+XP.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            
                            from
                            
                            
                            trn.SalesOrder SO
                            JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                            left outer join mst.MaterialMasterArticle mma on mma.id=MOI.ArticleId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                            
                            group by pod.ProductionOrderId,mm.userName,mma.StandardName,PM.UserName,pc.UserName,PM.Id) AS SO ON so.POID=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE (isnull(s.StandardName,'')<>'Closed' OR convert(date,po.ClosingDate)>CONVERT(DATE,'" + date + @"'))) prDetail  
                                                 INNER JOIN 
                            (	
                                select ProductionOrderId,'' AS WorkCenterMasterId,SUM(InQuantity) AS InQuantity,SUM(OutQuantity) AS OutQuantity,SUM(KillQuantity) AS KillQuantity   from 
			                        (SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS InQuantity,0 AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ToSFGInventoryId='" + InventoryId + @"' AND convert(date,ps.ProductionDate)" + Criteria + @"convert(date,'" + date + @"') 

			                         union all 
			 
			                         SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.FromSFGInventoryId='" + InventoryId + @"' AND convert(date,ps.ProductionDate)" + Criteria + @"convert(date,'" + date + @"') 

			                          union all 
			 
			                         SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END  AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.FromSFGInventoryId='" + InventoryId + @"' AND convert(date,ps.ProductionDate)" + Criteria + @"convert(date,'" + date + @"') 
			                ) AS K group by ProductionOrderId) prSum ON prDetail.Id = prSum.ProductionOrderId
                            left JOIN scs.WorkCenterMaster AS wcm ON wcm.Id = prSum.WorkCenterMasterId
                            JOIN org.Entity AS e ON e.Id=prDetail.EntityId
                            JOIN org.Plant AS p ON p.Id=prDetail.PlantId";

                dt = _sqlRepository.GetDataTable(strSql);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    prOrderDic.Add(dt.Rows[i]["ProductionOrderId"].ToString(), dt.Rows[i]);
                }
            }
            catch (Exception ex)
            {

            }
        }


        public void GetProductionListWithoutWCRowProcessWiseDashboard(string PlantId, string processId, string date, out DataTable dt)
        {

            dt = new DataTable();
            try
            {
                string strSql = "";
                strSql = @"SELECT case when isnull(PRO.IsFirst,0)=0 THEN prsum.InQuantity- prsum.OutQuantity- prsum.KillQuantity ELSE NULL END AS WIP, wcm.EntityId,
prDetail.*,prsum.*,prtoday.InQuantity AS InQuantityToday, prtoday.OutQuantity OutQuantityToday,prtoday.KillQuantity KillQuantityToday,
wcm.UserName AS WorkCenter,wcm.Sequence AS WorkCenterSequence,isnull(WCM.Capacity,0) AS Capacity,e.UserName AS Entity,p.UserName AS Plant ,
case WHEN  case when isnull(PRO.IsFirst,0)=0 THEN prsum.InQuantity- prsum.OutQuantity- prsum.KillQuantity ELSE NULL END>WCM.Capacity THEN 
case when isnull(PRO.IsFirst,0)=0 THEN prsum.InQuantity- prsum.OutQuantity- prsum.KillQuantity ELSE NULL END ELSE WCM.Capacity END AS MaxValue,
case when prtoday.OutQuantity>0 THEN CASE WHEN  ISNULL(TRC.Id,'')<>'' THEN  CASE WHEN ISNULL(TR.Id,'')='' THEN 0 ELSE 1 END ELSE 1 END ELSE 1 END AS AlignedWithPlan,
case when prtoday.OutQuantity>0 THEN CASE WHEN  ISNULL(TRCPR.Id,'')<>'' THEN  CASE WHEN ISNULL(TRPR.Id,'')='' THEN 0 ELSE 1 END ELSE 1 END ELSE 1 END AS AlignedWithPlanPR

FROM 
                        (SELECT
                            PO.Id,isnull(po.Remarks,'') AS ProductionRemarks,PBT.Id as BulletinId,PBT.BulletinName,isnull(s.UserName,'') AS ProductionStatus, isnull(EN.UserName,'') AS EntityName,
                            PO.ClosingDate,
                            isnull(PS.UserName,'') AS ProductionStatusName,SO.*
                            FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
							LEFT JOIN TRN.ProductionBulletinTemplate as PBT on PBT.ProductionOrderId=po.Id
                            LEFT OUTER JOIN (select
                            pod.ProductionOrderId POID,
                            mm.userName AS Material,PM.UserName AS Product,pc.UserName AS ProductCategory,PM.Id ProductMasterId,
                            -- Min(LSD) AS LSD,max(CommitmentDate) AS CommitmentDate ,
                            sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,
                            MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SONo=STUFF((select distinct ','+sox.Id from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SODesc=STUFF((select distinct ','+sox.[Description] from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            buyer=STUFF((select distinct ','+XB.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            
                            Customer=STUFF((select distinct ','+XP.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            
                            from
                            
                            
                            trn.SalesOrder SO
                            JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                            
                            group by pod.ProductionOrderId,mm.userName,PM.UserName,pc.UserName,PM.Id) AS SO ON so.POID=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE (isnull(s.StandardName,'')<>'Closed'  OR convert(date,po.ClosingDate)>CONVERT(DATE,'" + date + @"'))) prDetail 
                                                 LEFT JOIN 
                            (	
                                select ProductionOrderId,WorkCenterMasterId,SUM(InQuantity) AS InQuantity,SUM(OutQuantity) AS OutQuantity,SUM(KillQuantity) AS KillQuantity   from 
			                        (SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS InQuantity,0 AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ToProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)<=convert(date,'" + date + @"') 

			                         union all 
			 
			                         SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)<=convert(date,'" + date + @"') 

			                          union all 
			 
			                         SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END  AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)<=convert(date,'" + date + @"') 
                                    
                                    union all 
			 
			                         SELECT q.ProductionOrderId,q.WorkCenterMasterID,0 AS InQuantity,0 AS OutQuantity,isnull(q.DefectiveQty,0) AS  KillQuantity
                                      FROM trn.Quality AS q
                                      JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=q.WorkCenterMasterID
			                         WHERE wcm.ProcessId='" + processId + @"' AND convert(date,Q.ProductionDate)<=convert(date,'" + date + @"') 
			                ) AS K group by ProductionOrderId,WorkCenterMasterId) prSum ON prDetail.Id = prSum.ProductionOrderId

                           LEFT JOIN 
                            (	
                                select ProductionOrderId,WorkCenterMasterId,SUM(InQuantity) AS InQuantity,SUM(OutQuantity) AS OutQuantity,SUM(KillQuantity) AS KillQuantity   from 
			                        (SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS InQuantity,0 AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ToProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)=convert(date,'" + date + @"') 

			                         union all 
			 
			                         SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)=convert(date,'" + date + @"') 

			                          union all 
			 
			                         SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END  AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)=convert(date,'" + date + @"') 
                                    
                                    union all 
			 
			                         SELECT q.ProductionOrderId,q.WorkCenterMasterID,0 AS InQuantity,0 AS OutQuantity,isnull(q.DefectiveQty,0) AS  KillQuantity
                                      FROM trn.Quality AS q
                                      JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=q.WorkCenterMasterID
			                         WHERE wcm.ProcessId='" + processId + @"' AND convert(date,Q.ProductionDate)=convert(date,'" + date + @"') 
			                ) AS K group by ProductionOrderId,WorkCenterMasterId)  prToday ON prDetail.Id = prToday.ProductionOrderId AND prSum.WorkCenterMasterId=prToday.WorkCenterMasterId
                            left join scs.WorkCenterMaster AS wcm ON wcm.Id = prSum.WorkCenterMasterId
                                       
                                LEFT JOIN trn.DailyProductionTarget AS TRC ON wcm.Id= TRC.WorkCenterMasterID AND convert(date,TRC.TargetDate)=convert(date,'" + date + @"') 
                                        AND TRC.ID=(SELECT TOP 1 Id FROM  trn.DailyProductionTarget AS TRX 
                                                   WHERE trX.WorkCenterMasterID=wcm.Id
                                        AND convert(date,trx.TargetDate)=convert(date,'" + date + @"'))


                                        LEFT JOIN trn.DailyProductionTarget AS TR ON wcm.Id= tr.WorkCenterMasterID AND convert(date,tr.TargetDate)=convert(date,'" + date + @"') 
                                        AND tr.ID=(SELECT TOP 1 Id FROM  trn.DailyProductionTarget AS TRX 
                                                   WHERE trX.WorkCenterMasterID=wcm.Id
                                        AND convert(date,trx.TargetDate)=convert(date,'" + date + @"') AND trx.ProductionOrderId=prToday.ProductionOrderId)


                                    LEFT JOIN trn.DailyProductionTarget AS TRCPR ON TRCPR.PlantID=WCM.PlantID AND convert(date,TRCPR.TargetDate)=convert(date,'" + date + @"') 
                                        AND TRCPR.ID=(SELECT TOP 1 Id FROM  trn.DailyProductionTarget AS TRX 
                                                   WHERE TRX.PlantID=WCM.PlantID AND convert(date,trx.TargetDate)=convert(date,'" + date + @"'))

                                LEFT JOIN trn.DailyProductionTarget AS TRPR ON TRPR.PlantID=WCM.PlantID AND convert(date,TRPR.TargetDate)=convert(date,'" + date + @"') 
                                        AND TRPR.ID=(SELECT TOP 1 Id FROM  trn.DailyProductionTarget AS TRX 
                                                   WHERE  TRX.PlantID=WCM.PlantID AND convert(date,trx.TargetDate)=convert(date,'" + date + @"') AND trx.ProductionOrderId=prToday.ProductionOrderId)

                            left join hkp.Process PRO on PRO.Id=WCM.ProcessId
                            JOIN org.Entity AS e ON e.Id=wcm.EntityId
                            JOIN org.Plant AS p ON p.Id=e.PlantId
                where e.PlantId='" + PlantId + @"'
				ORDER BY wcm.Sequence";



                dt = _sqlRepository.GetDataTable(strSql);

            }
            catch (Exception ex)
            {

            }
        }
        public void GetProductionLisPRWiseDashboard(string PlantId, string processId, string date, out DataTable dt)
        {

            dt = new DataTable();
            try
            {


                string strSql = "";
                strSql = @"SELECT prDetail.POQty AS ProductionOrderQty,prDetail.POQty-prsum.OutQuantity AS ProductionOrderBalanceQty,case when isnull(PRO.IsFirst,0)=0 THEN prsum.InQuantity- prsum.OutQuantity- prsum.KillQuantity ELSE NULL END AS WIP,prDetail.*,prsum.*,prtoday.InQuantity AS InQuantityToday, prtoday.OutQuantity OutQuantityToday,prtoday.KillQuantity KillQuantityToday,e.UserName AS Entity,p.UserName AS Plant FROM 
                            (SELECT
                            PO.Id,PBT.Id BulletinId, PBT.BulletinName, PO.Qty AS POQty,isnull(po.Remarks,'') AS ProductionRemarks,isnull(s.UserName,'') AS ProductionStatus, isnull(EN.UserName,'') AS EntityName,
                            PO.EntityId,PO.ClosingDate,
                            isnull(PS.UserName,'') AS ProductionStatusName,SO.*
                            FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
							LEFT JOIN TRN.ProductionBulletinTemplate as PBT on PBT.ProductionOrderId=PO.Id
                            LEFT OUTER JOIN (select
                            pod.ProductionOrderId POID,
                            mm.userName AS Material,PM.UserName AS Product,pc.UserName AS ProductCategory,PM.Id ProductMasterId,
                            -- Min(LSD) AS LSD,max(CommitmentDate) AS CommitmentDate ,
                            sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,
                            MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SONo=STUFF((select distinct ','+sox.Id from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SODesc=STUFF((select distinct ','+sox.[Description] from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            buyer=STUFF((select distinct ','+XB.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            
                            Customer=STUFF((select distinct ','+XP.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            
                            from
                            
                            
                            trn.SalesOrder SO
                            JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                            
                            group by pod.ProductionOrderId,mm.userName,PM.UserName,pc.UserName,PM.Id) AS SO ON so.POID=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE (isnull(s.StandardName,'')<>'Closed'  OR convert(date,po.ClosingDate)>CONVERT(DATE,'" + date + @"'))) prDetail 
                                                 INNER JOIN 
                            (	
                                select ProductionOrderId,SUM(InQuantity) AS InQuantity,SUM(OutQuantity) AS OutQuantity,SUM(KillQuantity) AS KillQuantity   from 
			                        (SELECT ps.ProductionOrderId,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS InQuantity,0 AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ToProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)<=convert(date,'" + date + @"') 

			                         union all 
			 
			                         SELECT ps.ProductionOrderId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)<=convert(date,'" + date + @"') 

			                          union all 
			 
			                         SELECT ps.ProductionOrderId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END  AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)<=convert(date,'" + date + @"') 
                                    
                                    union all 
			 
			                         SELECT q.ProductionOrderId,0 AS InQuantity,0 AS OutQuantity,isnull(q.DefectiveQty,0) AS  KillQuantity
                                      FROM trn.Quality AS q
                                      JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=q.WorkCenterMasterID
			                         WHERE wcm.ProcessId='" + processId + @"' AND convert(date,Q.ProductionDate)<=convert(date,'" + date + @"') 
			                ) AS K group by ProductionOrderId) prSum ON prDetail.Id = prSum.ProductionOrderId

                           LEFT JOIN 
                            (	
                                select ProductionOrderId,SUM(InQuantity) AS InQuantity,SUM(OutQuantity) AS OutQuantity,SUM(KillQuantity) AS KillQuantity   from 
			                        (SELECT ps.ProductionOrderId,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS InQuantity,0 AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ToProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)=convert(date,'" + date + @"') 

			                         union all 
			 
			                         SELECT ps.ProductionOrderId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)=convert(date,'" + date + @"') 

			                          union all 
			 
			                         SELECT ps.ProductionOrderId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END  AS KillQuantity 
				                        FROM trn.ProductionSummary AS ps
			                         WHERE ps.ProcessId='" + processId + @"' AND convert(date,ps.ProductionDate)=convert(date,'" + date + @"') 
                                    
                                    union all 
			 
			                         SELECT q.ProductionOrderId,0 AS InQuantity,0 AS OutQuantity,isnull(q.DefectiveQty,0) AS  KillQuantity
                                      FROM trn.Quality AS q
                                      JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=q.WorkCenterMasterID
			                         WHERE wcm.ProcessId='" + processId + @"' AND convert(date,Q.ProductionDate)=convert(date,'" + date + @"') 
			                ) AS K group by ProductionOrderId)  prToday ON prDetail.Id = prToday.ProductionOrderId 
                            left join hkp.Process PRO on PRO.Id='" + processId + @"'
                            JOIN org.Entity AS e ON e.Id=prDetail.EntityId
                            JOIN org.Plant AS p ON p.Id=e.PlantId where E.PlantId='" + PlantId + @"'";



                dt = _sqlRepository.GetDataTable(strSql);

            }
            catch (Exception ex)
            {

            }
        }

        public List<Dictionary<string, object>> GetDailyPlanVsProduction(string PlantId, string EntityId, string date)
        {


            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string Filter = "";
                if (string.IsNullOrEmpty(EntityId) == false && EntityId.ToUpper() != "NULL")
                    Filter = " AND wcm.EntityId='" + EntityId + @"'";



                string strSql = @"SELECT WCM.Id AS WorkCenterMasterID, WCM.Sequence,WCM.UserName AS Workcenter, 
                                ISNULL(CASE WHEN sum(K.Production)>=sum(K.PlanQuantity) THEN sum(K.Production) ELSE sum(K.PlanQuantity) END,0) AS MaxQuantity,
                                    ISNULL(sum(K.Production),0) AS Production, ISNULL(sum(K.PlanQuantity),0) AS PlanQuantity 

                                    from scs.WorkCenterMaster AS wcm
                                    Left join 
                                    (
                                    select WCM.Id AS WorkCenterMasterID,wcm.EntityId,wcm.Sequence, wcm.UserName AS Workcenter,0 AS Production,isnull(tg.Quantity,0) AS PlanQuantity
                                      from scs.WorkCenterMaster AS wcm
                                    JOIN  trn.DailyProductionTarget TG ON wcm.Id=tg.WorkCenterMasterID AND TG.TargetDate='" + date + @"' AND TG.PlantId='" + PlantId + @"'
                                    join trn.ProductionOrder PO ON PO.Id=TG.ProductionOrderId
                                    JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                                    
                                    WHERE wcm.ProcessId=(SELECT TOP 1 xpt.BaseProcessId FROM PlanningTypes xpt WHERE xpt.PlanningType='PlanningType1')  " + Filter + @"
                                    UNION ALL

                                    select tg.WorkCenterMasterID,wcm.EntityId,wcm.Sequence, wcm.UserName AS Workcenter,tg.Quantity AS Production,0 AS PlanQuantity
                                      from trn.ProductionSummary TG
                                    join trn.ProductionOrder PO ON PO.Id=TG.ProductionOrderId
                                    LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                                    JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=tg.WorkCenterMasterID
                                     WHERE TG.ProductionDate='" + date + @"'  AND TG.PlantId='" + PlantId + @"' " + Filter + @" AND wcm.ProcessId=(SELECT TOP 1 xpt.BaseProcessId FROM PlanningTypes xpt WHERE xpt.PlanningType='PlanningType1')
                                    ) AS K ON K.WorkCenterMasterID=WCM.Id

                                    LEFT JOIN org.Entity AS e ON e.Id=k.EntityId
                                    where wcm.ProcessId=(SELECT TOP 1 xpt.BaseProcessId FROM PlanningTypes xpt WHERE xpt.PlanningType='PlanningType1')  " + Filter + @"
                                    GROUP BY e.Code, WCM.Id, WCM.Sequence, WCM.UserName
                                    ORDER BY WCM.Sequence";



                return _sqlRepository.GetDataCollection(strSql);

            }
            catch (Exception ex)
            {

            }

            return null;
        }
        public List<Dictionary<string, object>> GetDailyLast30DaysPlanVsProduction(string PlantId, string EntityId, string date)
        {



            try
            {
                string Filter = "";
                if (string.IsNullOrEmpty(EntityId) == false && EntityId.ToUpper() != "NULL")
                    Filter = " AND wcm.EntityId='" + EntityId + @"'";

                //will use later
                string _calendar = @"SELECT DISTINCT TOP 10 K.WorkingDate from (SELECT
                                        DENSE_RANK() OVER (PARTITION BY WCM.EntityID ORDER BY  WCM.EntityID,WCM.WorkingDate desc) AS RNK,
                                         * FROM ProductionPlanningCalendar AS WCM

                                        WHERE isnull(WCM.DayType,'')='' 
                                        AND WCM.ProcessID=(SELECT TOP 1 xpt.BaseProcessId FROM PlanningTypes xpt WHERE xpt.PlanningType='PlanningType1')
                                        AND WCM.WorkingDate<='" + date + @"'

                                        ) AS K WHERE K.RNK<=10
                                        ORDER BY K.WorkingDate DESC";



                string FromDate = Convert.ToDateTime(date).AddDays(-10).ToString("dd-MMM-yyyy");
                string ToDate = date;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                string strDays = "";
                DateTime tempFromDate = Convert.ToDateTime(date).AddDays(-10);
                for (int i = 0; i < 10; i++)
                {
                    if (strDays == "")
                        strDays = "SELECT CONVERT(DATE,'" + tempFromDate.AddDays(i) + "') AS ProductionDate";
                    else
                        strDays += "\r\n UNION ALL SELECT CONVERT(DATE,'" + tempFromDate.AddDays(i) + "')";

                }

                string strSql = @"SELECT DT.ProductionDate,FORMAT(DT.ProductionDate,'dd/MMM') AS [Date],
                                    isnull(sum(K.Production),0) AS Production, isnull(sum(K.PlanQuantity),0) AS PlanQuantity,
                                    isnull(CASE WHEN sum(K.Production)>=sum(K.PlanQuantity) THEN sum(K.Production) ELSE sum(K.PlanQuantity) END,0) AS MaxQuantity

                                     FROM 
 
                                     (" + strDays + @") AS DT
 
                                     LEFT JOIN (
                                    SELECT tg.TargetDate AS ProductionDate,0 AS Production,SUM(tg.Quantity) AS PlanQuantity
                                      from trn.DailyProductionTarget TG
                                    join trn.ProductionOrder PO ON PO.Id=TG.ProductionOrderId
                                    LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                                    JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=tg.WorkCenterMasterID
                                     WHERE TG.PlantId='" + PlantId + @"' AND TargetDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'  " + Filter + @"  AND wcm.ProcessId=(SELECT TOP 1 xpt.BaseProcessId FROM PlanningTypes xpt WHERE xpt.PlanningType='PlanningType1')
                                    GROUP BY tg.TargetDate

                                    UNION ALL

                                    select tg.ProductionDate,SUM(tg.Quantity) AS Production,0 AS PlanQuantity
                                      from trn.ProductionSummary TG
                                    join trn.ProductionOrder PO ON PO.Id=TG.ProductionOrderId
                                    LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                                    JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=tg.WorkCenterMasterID
                                     WHERE TG.PlantId='" + PlantId + @"' AND tg.ProductionDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' " + Filter + @"  AND wcm.ProcessId=(SELECT TOP 1 xpt.BaseProcessId FROM PlanningTypes xpt WHERE xpt.PlanningType='PlanningType1')
                                    GROUP BY tg.ProductionDate
                                    ) AS K ON k.ProductionDate=dt.ProductionDate
                                    GROUP BY DT.ProductionDate
                                    ORDER BY DT.ProductionDate";



                return _sqlRepository.GetDataCollection(strSql);

            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public List<Dictionary<string, object>> GetLastDaysPlanVsProductionStatistics(string PlantId, string EntityId, string date)
        {



            try
            {
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                //last 10 days
                string FromDate = Convert.ToDateTime(date).AddDays(-10).ToString("dd-MMM-yyyy");
                string ToDate = Convert.ToDateTime(date).AddDays(-1).ToString("dd-MMM-yyyy");
                data.Add(GetLastNDaysPlanVsProduction(PlantId, EntityId, "Last 10 Days (" + FromDate + @" to " + ToDate + @")", FromDate, ToDate));

                DateTime _temp = Convert.ToDateTime(date).AddMonths(-1);
                FromDate = new DateTime(_temp.Year, _temp.Month, 1).ToString("dd-MMM-yyyy");
                ToDate = new DateTime(_temp.Year, _temp.Month, DateTime.DaysInMonth(_temp.Year, _temp.Month)).ToString("dd-MMM-yyyy");
                data.Add(GetLastNDaysPlanVsProduction(PlantId, EntityId, "Last Month (" + _temp.ToString("MMMM/yyyy") + @")", FromDate, ToDate));

                _temp = Convert.ToDateTime(date);
                FromDate = new DateTime(_temp.Year, _temp.Month, 1).ToString("dd-MMM-yyyy");
                ToDate = new DateTime(_temp.Year, _temp.Month, DateTime.DaysInMonth(_temp.Year, _temp.Month)).ToString("dd-MMM-yyyy");
                data.Add(GetLastNDaysPlanVsProduction(PlantId, EntityId, "This Month (" + _temp.ToString("MMMM/yyyy") + @")", FromDate, ToDate));

                return data;

            }
            catch (Exception ex)
            {

            }

            return null;
        }


        public Dictionary<string, object> GetLastNDaysPlanVsProduction(string PlantId, string EntityId, string Caption, string FromDate, string ToDate)
        {
            try
            {
                string Filter = "";
                if (string.IsNullOrEmpty(EntityId) == false && EntityId.ToUpper() != "NULL")
                    Filter = " AND wcm.EntityId='" + EntityId + @"'";



                //will use later
                string _calendar = @"SELECT DISTINCT K.WorkingDate from (SELECT
                                        DENSE_RANK() OVER (PARTITION BY WCM.EntityID ORDER BY  WCM.EntityID,WCM.WorkingDate desc) AS RNK,
                                         * FROM ProductionPlanningCalendar AS WCM

                                        WHERE isnull(WCM.DayType,'')='' 
                                        AND WCM.ProcessID=(SELECT TOP 1 xpt.BaseProcessId FROM PlanningTypes xpt WHERE xpt.PlanningType='PlanningType1')
                                        AND convert(date,WCM.WorkingDate) BETWEEN CONVERT(DATE," + FromDate + @") AND CONVERT(DATE," + ToDate + @")

                                        ) AS K
                                        ORDER BY K.WorkingDate DESC";





                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DateTime tempFromDate = Convert.ToDateTime(FromDate).AddDays(0);
                string strDays = "";
                while (Convert.ToDateTime(tempFromDate) <= Convert.ToDateTime(ToDate))
                {
                    if (strDays == "")
                        strDays = "SELECT CONVERT(DATE,'" + tempFromDate + "') AS ProductionDate";
                    else
                        strDays += "\r\n UNION ALL SELECT CONVERT(DATE,'" + tempFromDate + "')";

                    tempFromDate = tempFromDate.AddDays(1);
                }


                string strSql = @"select '" + Caption + @"' AS Caption,
                                        SUM(pp.PlanQuantity) AS TotalTargetQty,AVG(PP.PlanQuantity) AS AverageTargetQty,
                                        SUM(pp.PlanAmountInCM) AS TotalTargetAmount,AVG(PP.PlanAmountInCM) AS AverageTargetAmount,

                                        SUM(pp.Production) AS TotalProductionQty,AVG(PP.Production) AS AverageProductionQty  ,
                                        SUM(pp.ProductionAmountInCM) AS TotalProductionAmount,AVG(PP.ProductionAmountInCM) AS AverageProductionAmount


                                        from (SELECT DT.ProductionDate,FORMAT(DT.ProductionDate,'dd/MMM') AS [Date],
                                    isnull(sum(K.Production),0) AS Production, isnull(sum(K.PlanQuantity),0) AS PlanQuantity,
                                    isnull(sum(K.Production*rate.CM),0) AS ProductionAmountInCM, isnull(sum(K.PlanQuantity*rate.CM),0) AS PlanAmountInCM

                                     FROM 
 
                                     (" + strDays + @") AS DT
 
                                     LEFT JOIN (
                                  SELECT tg.ProductionOrderId, tg.TargetDate AS ProductionDate,0 AS Production,SUM(tg.Quantity) AS PlanQuantity
                                        from trn.DailyProductionTarget TG
                                    join trn.ProductionOrder PO ON PO.Id=TG.ProductionOrderId
                                    LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                                    JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=tg.WorkCenterMasterID
                                     WHERE TG.PlantId='" + PlantId + @"' AND (isnull(s.StandardName,'')<>'Closed' OR convert(date,po.ClosingDate)>CONVERT(DATE,TG.TargetDate)) AND TargetDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'  " + Filter + @"  AND wcm.ProcessId=(SELECT TOP 1 xpt.BaseProcessId FROM PlanningTypes xpt WHERE xpt.PlanningType='PlanningType1')
                                GROUP BY tg.ProductionOrderId,tg.TargetDate

                                    UNION ALL

                               select tg.ProductionOrderId, tg.ProductionDate,SUM(tg.Quantity) AS Production,0 AS PlanQuantity
                                    from trn.ProductionSummary TG
                                join trn.ProductionOrder PO ON PO.Id=TG.ProductionOrderId
                                LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                                JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=tg.WorkCenterMasterID
                                     WHERE TG.PlantId='" + PlantId + @"' AND (isnull(s.StandardName,'')<>'Closed'  OR convert(date,po.ClosingDate)>CONVERT(DATE,tg.ProductionDate)) AND tg.ProductionDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' " + Filter + @"  AND wcm.ProcessId=(SELECT TOP 1 xpt.BaseProcessId FROM PlanningTypes xpt WHERE xpt.PlanningType='PlanningType1')
                               GROUP BY tg.ProductionOrderId,tg.ProductionDate
                                    ) AS K ON k.ProductionDate=dt.ProductionDate

                                  LEFT JOIN ( 
	                                    SELECT POD.ProductionOrderId, SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate*SO.Qty ELSE  so.Rate * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1)* so.Qty END)/SUM(so.Qty) AS FOB,
                                    SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM*SO.Qty ELSE  so.CM * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1)* so.Qty END)/SUM(so.Qty) AS CM
							                                                                  from trn.ProductionOrderDetail POD 
                                                                                            left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                                                                                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                                                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                                                            left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                                                                                            left JOIN org.Company AS c ON c.Id=mo.CompanyId
                                                                                            left outer join MasterOrderExchangeRates RT on RT.TransactionId=MO.Id
								                     	                                    LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=C.BaseCurrencyId AND rer.PlantId='" + PlantId + @"'
                                                                                            LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId='" + PlantId + @"'
	                                    GROUP BY POD.ProductionOrderId  ) AS RATE ON RATE.ProductionOrderId=k.ProductionOrderId
                                    GROUP BY DT.ProductionDate) AS PP";



                return _sqlRepository.GetData(strSql);


            }
            catch (Exception ex)
            {

            }

            return null;
        }
        public string GetType1ProcessName()
        {



            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;




                string strSql = @"SELECT TOP 1 p.UserName FROM PlanningTypes xpt 
                                    join HKP.Process P on P.Id=xpt.BaseProcessId
                                    WHERE xpt.PlanningType='PlanningType1'
                                   ";



                DataTable dt = _sqlRepository.GetDataTable(strSql);
                if (dt.Rows.Count > 0)
                    return dt.Rows[0]["UserName"].ToString();

            }
            catch (Exception ex)
            {

            }

            return "";
        }
        public string GetType1ProcessId()
        {



            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;




                string strSql = @"SELECT TOP 1 p.Id FROM PlanningTypes xpt 
                                    join HKP.Process P on P.Id=xpt.BaseProcessId
                                    WHERE xpt.PlanningType='PlanningType1'
                                   ";



                DataTable dt = _sqlRepository.GetDataTable(strSql);
                if (dt.Rows.Count > 0)
                    return dt.Rows[0]["Id"].ToString();

            }
            catch (Exception ex)
            {

            }

            return "";
        }


        private void GetProductionListSFGRow(string processId, string date, out Dictionary<string, DataRow> prOrderSFGDic)
        {
            prOrderSFGDic = new Dictionary<string, DataRow>();

            try
            {
                DataTable dt = null;

                string strSql = "";

                strSql = @"SELECT * FROM 
                            (SELECT
                            PO.Id,isnull(po.Remarks,'') AS ProductionRemarks,isnull(s.UserName,'') AS ProductionStatus, isnull(EN.UserName,'') AS EntityName,
                            
                            isnull(PS.UserName,'') AS ProductionStatusName,SO.*
                            FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            LEFT OUTER JOIN (select
                            pod.ProductionOrderId POID,
                            mm.userName AS Material,PM.UserName AS Product,pc.UserName AS ProductCategory,PM.Id ProductMasterId,
                            -- Min(LSD) AS LSD,max(CommitmentDate) AS CommitmentDate ,
                            sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,
                            MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrder XMOI
                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SONo=STUFF((select distinct ','+sox.Id from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            SODesc=STUFF((select distinct ','+sox.[Description] from
                            trn.MasterOrderItem XMOI
                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id
                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id
                            where podx.ProductionOrderId=pod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            buyer=STUFF((select distinct ','+XB.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            
                            Customer=STUFF((select distinct ','+XP.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
                            where pod.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            
                            from
                            
                            
                            trn.SalesOrder SO
                            JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                            
                            group by pod.ProductionOrderId,mm.userName,PM.UserName,pc.UserName,PM.Id) AS SO ON so.POID=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId) prDetail 
                            inner JOIN 
                            (
                               SELECT ps.ProductionOrderId,
                                SUM(CASE WHEN SFGTo.ProcessId='" + processId + @"' THEN ps.Quantity ELSE 0 END) AS InQuantity,
                                SUM(CASE WHEN SFGFrom.ProcessId='" + processId + @"' THEN ps.Quantity ELSE 0 END) AS OutQuantity ,
                                SUM(CASE WHEN SFGTo.ProcessId='" + processId + @"' THEN ps.Quantity ELSE 0 END) -
                                SUM(CASE WHEN SFGFrom.ProcessId='" + processId + @"' THEN ps.Quantity ELSE 0 END) AS WIP 
                                FROM TRN.ProductionSummary AS ps
                                Left Outer join hkp.SFGInventory SFGFrom  on SFGFrom.Id = ps.FromSFGInventoryId
                                Left Outer join hkp.SFGInventory SFGTo  on SFGTo.Id = ps.ToSFGInventoryId
                                
                                WHERE (SFGFrom.ProcessId='" + processId + @"' OR SFGTo.ProcessId='" + processId + @"') AND ps.ProductionDate <='" + date + @"'
                                GROUP BY ps.ProductionOrderId
                            ) prSum ON prDetail.Id = prSum.ProductionOrderId";

                dt = _sqlRepository.GetDataTable(strSql);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    prOrderSFGDic.Add(dt.Rows[i]["ProductionOrderId"].ToString(), dt.Rows[i]);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
