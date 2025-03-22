using ConnectionManager;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Extension;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Extension.OrderControl
{
    public class MailSenderService
    {
        SqlRepository _sqlRepository;
        public MailSenderService()
        {
            _sqlRepository = new SqlRepository();
        }


        public IWorkbook ControlChartReportXls()
        {

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            try
            {

                getOrderControl(out DataTable dtOrderMaster);

                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                DataTable dtDistinctControl = dtOrderMaster.DefaultView.ToTable(true, "ControlTypeId", "ControlType", "ControlTypeDesc");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(dtDistinctControl.Rows.Count + 1);

                for (int k = 0; k < dtDistinctControl.Rows.Count; k++)
                {

                    string DataCaption = "Date";
                    if (dtDistinctControl.Rows[k]["ControlType"].ToString() == OrderControlTypes.ShipmentControl.ToString())
                        DataCaption = "Delivery Date";
                    if (dtDistinctControl.Rows[k]["ControlType"].ToString() == OrderControlTypes.MainRMInhouse.ToString() || dtDistinctControl.Rows[k]["ControlType"].ToString() == OrderControlTypes.MainRMShipment.ToString())
                        DataCaption = "Main RM Inhouse Date";
                    if (dtDistinctControl.Rows[k]["ControlType"].ToString() == OrderControlTypes.OtherRMInhouse.ToString() || dtDistinctControl.Rows[k]["ControlType"].ToString() == OrderControlTypes.OtherRMShipment.ToString())
                        DataCaption = "Other RM Inhouse Date";

                    if (dtDistinctControl.Rows[k]["ControlType"].ToString() == OrderControlTypes.BaseProcessInput.ToString())
                        DataCaption = "Base Process Start Date";



                    workbook.Worksheets[k].Name = dtDistinctControl.Rows[k]["ControlTypeDesc"].ToString();
                    sheet = workbook.Worksheets[k];

                    int ROW = 1; int COL = 1;
                    sheet[ROW, 1].Text = "Control Chart(" + dtDistinctControl.Rows[k]["ControlTypeDesc"].ToString() + ")";
                    sheet[ROW, 1].CellStyle.Font.Size = 16;
                    sheet[ROW, 1].RowHeight = 22;
                    sheet[ROW, 1].CellStyle.Font.Bold = true;
                    // sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;


                    ROW += 2;
                    #region columns

                    sheet[ROW, COL].Text = "Sl";
                    sheet[ROW, COL].ColumnWidth = 4;
                    int colSL = COL;
                    COL++;
                    sheet[ROW, COL].Text = DataCaption;// "Date";
                    sheet[ROW, COL].ColumnWidth = 8;
                    int colDate = COL;
                    int ColCommitmentDate = COL;
                    int ColPlanExFactoryDate = COL;
                    if (dtDistinctControl.Rows[k]["ControlType"].ToString() == OrderControlTypes.ShipmentControl.ToString())
                    {
                        COL++;
                        sheet[ROW, COL].Text = "Commitment Date";
                        sheet[ROW, COL].ColumnWidth = 8;
                        sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        ColCommitmentDate = COL;
                        COL++;
                        sheet[ROW, COL].Text = "Ex-Factory Date";
                        sheet[ROW, COL].ColumnWidth = 8;
                        sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        ColPlanExFactoryDate = COL;
                    }
                    COL++;
                    sheet[ROW, COL].Text = "Customer";
                    sheet[ROW, COL].ColumnWidth = 14;
                    int colCustomer = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Customer Item No";
                    sheet[ROW, COL].ColumnWidth = 14;
                    int colBuyerItem = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Qty";
                    sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet[ROW, COL].ColumnWidth = 8;
                    int colQty = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Remarks";
                    sheet[ROW, COL].ColumnWidth = 16;
                    int colRemarks = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Critical Level";
                    sheet[ROW, COL].ColumnWidth = 8;
                    int colCriticalLevel = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Status";
                    sheet[ROW, COL].ColumnWidth = 8;
                    int colStatus = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Customer PO";
                    sheet[ROW, COL].ColumnWidth = 8;
                    int colPONumber = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Customer Country";
                    sheet[ROW, COL].ColumnWidth = 8;
                    int colBuyerCountry = COL;
                    COL++;
                    sheet[ROW, COL].Text = "SO Description";
                    sheet[ROW, COL].ColumnWidth = 16;
                    int colDescription = COL;
                    int ColMaterial = COL;
                    int ColArticle = COL;
                    if (dtDistinctControl.Rows[k]["ControlType"].ToString() == OrderControlTypes.ShipmentControl.ToString())
                    {
                        COL++;
                        sheet[ROW, COL].Text = "Article";
                        sheet[ROW, COL].ColumnWidth = 8;
                        sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        ColArticle = COL;
                        COL++;
                        sheet[ROW, COL].Text = "Material";
                        sheet[ROW, COL].ColumnWidth = 8;
                        sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        ColMaterial = COL;
                       
                    }

                    COL++;
                    sheet[ROW, COL].Text = "Entity";
                    sheet[ROW, COL].ColumnWidth = 12;
                    int colEntity = COL;
                    
                   
                    int ColDeliveryWeek = COL;
                    if (dtDistinctControl.Rows[k]["ControlType"].ToString() == OrderControlTypes.ShipmentControl.ToString())
                    {
                        COL++;
                        sheet[ROW, COL].Text = "Week No";
                        sheet[ROW, COL].ColumnWidth = 8;
                        sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        ColDeliveryWeek = COL;
                    }
                  
                   
                   
                    
                  
                    COL++;
                    sheet[ROW, COL].Text = "Plant";
                    sheet[ROW, COL].ColumnWidth = 12;
                    int colPlant = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Master Order No";
                    sheet[ROW, COL].ColumnWidth = 10;
                    int colMasterOrderNo = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Order Category";
                    sheet[ROW, COL].ColumnWidth = 10;
                    int colOrderCategory = COL;
                    COL++;
                    sheet[ROW, COL].Text = "PR No";
                    sheet[ROW, COL].ColumnWidth = 10;
                    int colProductionOrderNo = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Sales Order";
                    sheet[ROW, COL].ColumnWidth = 10;
                    int colSONos = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Line Item Reference";
                    sheet[ROW, COL].ColumnWidth = 10;
                    int colLIR = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Update Date";
                    sheet[ROW, COL].ColumnWidth = 8;
                    int colUpdateDate = COL;
                    COL++;
                    //Buyer / Unit / Buyer Style / Buyer PO /  / Buyer Delivery Date/ Week / 
                    //Buyer Country / Buyer Qty / Remark / Critical Level / Status / So Description / Plant 
                    // Master Order No/ PR No / Sales Order / UP Date / Order Responsible Person/ Entity Responsible Person/ Control Responsible Person


                    sheet[ROW, COL].Text = "Customer Order No";
                    sheet[ROW, COL].ColumnWidth = 10;
                    int colBuyerOrderNo = COL;



                    //COL++;
                    //sheet[ROW, COL].Text = "SO Status";
                    //sheet[ROW, COL].ColumnWidth = 8;
                    //int colSOStatus = COL;

                    COL++;
                    sheet[ROW, COL].Text = "Order Responsible Person";
                    sheet[ROW, COL].ColumnWidth = 16;
                    int colOrderResponsiblePerson = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Entity Responsible Person";
                    sheet[ROW, COL].ColumnWidth = 16;
                    int colEntityResponsiblePerson = COL;
                    COL++;
                    sheet[ROW, COL].Text = "Control Responsible Person";
                    sheet[ROW, COL].ColumnWidth = 16;
                    int colControlResponsiblePerson = COL;
                    int ROWSTART = ROW - 1;
                    int ColStartRange = COL;



                    #endregion columns

                    int endCol = COL;

                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                    ROW++;

                    int startRow = ROW;
                    dtOrderMaster.DefaultView.RowFilter = "ControlTypeId = '" + dtDistinctControl.Rows[k]["ControlTypeId"].ToString() + "'";

                    for (int i = 0; i < dtOrderMaster.DefaultView.Count; i++)
                    {

                        //ControlTypeId ControlTypeDesc CriticalityLevel       Customer     OwnOrder    
                        //    BuyerItem OwnItem               

                        sheet[ROW, colSL].Text = (i + 1).ToString();
                        sheet[ROW, colPlant].Text = dtOrderMaster.DefaultView[i]["Plant"].ToString();
                        sheet[ROW, colEntity].Text = dtOrderMaster.DefaultView[i]["Entity"].ToString();
                        sheet[ROW, colCustomer].Text = dtOrderMaster.DefaultView[i]["Customer"].ToString();
                        sheet[ROW, colCriticalLevel].Text = dtOrderMaster.DefaultView[i]["CriticalityLevel"].ToString();
                        sheet[ROW, colDate].Text = clsStaticInfo.GetDate(dtOrderMaster.DefaultView[i]["Date"].ToString());
                        sheet[ROW, colStatus].Text = dtOrderMaster.DefaultView[i]["Status"].ToString();
                        sheet[ROW, colSONos].Text = dtOrderMaster.DefaultView[i]["SoNo"].ToString();
                        sheet[ROW, colLIR].Text = dtOrderMaster.DefaultView[i]["LineItemReference"].ToString();
                        sheet[ROW, colBuyerItem].Text = dtOrderMaster.DefaultView[i]["BuyerItem"].ToString();
                        sheet[ROW, colBuyerCountry].Text = dtOrderMaster.DefaultView[i]["BuyerCountry"].ToString();
                        sheet[ROW, colPONumber].Text = dtOrderMaster.DefaultView[i]["PONumber"].ToString();
                        
                        sheet[ROW, colUpdateDate].Text = clsStaticInfo.GetDate(dtOrderMaster.DefaultView[i]["UpdatedDate"].ToString());

                        sheet[ROW, colMasterOrderNo].Text = dtOrderMaster.DefaultView[i]["MasterOrderNo"].ToString();
                        sheet[ROW, colOrderCategory].Text = dtOrderMaster.DefaultView[i]["OrderCategory"].ToString();
                        sheet[ROW, colProductionOrderNo].Text = dtOrderMaster.DefaultView[i]["PRNo"].ToString();
                        sheet[ROW, colBuyerOrderNo].Text = dtOrderMaster.DefaultView[i]["BuyerOrder"].ToString();
                        sheet[ROW, colEntityResponsiblePerson].Text = dtOrderMaster.DefaultView[i]["EntityEmployee"].ToString();
                        sheet[ROW, colOrderResponsiblePerson].Text = dtOrderMaster.DefaultView[i]["MasterOrderEmployee"].ToString();
                        sheet[ROW, colControlResponsiblePerson].Text = dtOrderMaster.DefaultView[i]["ControlEmployee"].ToString();
                        sheet[ROW, colQty].Number = clsStaticInfo.dbl(dtOrderMaster.DefaultView[i]["Quantity"].ToString());
                        sheet[ROW, colRemarks].Text = dtOrderMaster.DefaultView[i]["Remarks"].ToString();
                        sheet[ROW, colDescription].Text = dtOrderMaster.DefaultView[i]["SODescription"].ToString();


                        if (clsStaticInfo.GetDate(dtOrderMaster.DefaultView[i]["Date"].ToString()) != "")
                        {
                            if (Convert.ToDateTime(clsStaticInfo.GetDate(dtOrderMaster.DefaultView[i]["Date"].ToString())) < Convert.ToDateTime(System.DateTime.Now.ToString("dd-MMM-yyyy")))
                            {
                                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#FFAC9A");
                            }
                            else if (Convert.ToDateTime(clsStaticInfo.GetDate(dtOrderMaster.DefaultView[i]["Date"].ToString())) > Convert.ToDateTime(System.DateTime.Now.ToString("dd-MMM-yyyy")))
                            {
                                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#FDFFB8");

                            }
                            else
                            {
                                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.ColorTranslator.FromHtml("#ADCAFF");

                            }
                        }

                        if (dtDistinctControl.Rows[k]["ControlType"].ToString() == OrderControlTypes.ShipmentControl.ToString())
                        {
                            sheet[ROW, ColCommitmentDate].Text = dtOrderMaster.DefaultView[i]["CommitmentDate"].ToString();
                            sheet[ROW, ColPlanExFactoryDate].Text = dtOrderMaster.DefaultView[i]["PlanExFactoryDate"].ToString();
                            sheet[ROW, ColMaterial].Text = dtOrderMaster.DefaultView[i]["Material"].ToString();
                            sheet[ROW, ColArticle].Text = dtOrderMaster.DefaultView[i]["Article"].ToString();

                            if (clsStaticInfo.GetDate(dtOrderMaster.DefaultView[i]["Date"].ToString()) != "")
                                sheet[ROW, ColDeliveryWeek].Text = "WEEK";// Convert.ToDateTime( clsStaticInfo.GetDate(dtOrderMaster.DefaultView[i]["Date"].ToString())).;
                        }


                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                        ROW++;

                    }

                    IListObject table = sheet.ListObjects.Create(dtDistinctControl.Rows[k]["ControlTypeId"].ToString().Replace("-", ""), sheet[clsStaticInfo.GetxlsCol(1) + (3).ToString() + ":" + clsStaticInfo.GetxlsCol(endCol) + (ROW - 1).ToString()]);
                    table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;

                    sheet.UsedRange.NumberFormat = "#,##0;[Red](#,##0)";
                    sheet.Range[2, 1, ROW, endCol].WrapText = true;
                    sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[4, 1].FreezePanes();
                    //#region ******************Report Header******************

                    sheet.IsDisplayZeros = false;
                    sheet.IsGridLinesVisible = false;

                    //#endregion ******************Report Header******************


                    sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                    sheet.PageSetup.TopMargin = 0.2;
                    sheet.PageSetup.BottomMargin = 0.8;
                    sheet.PageSetup.PrintTitleRows = "$1:$3";
                    sheet.PageSetup.LeftMargin = 0.2;
                    sheet.PageSetup.RightMargin = 0.2;
                    sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet.PageSetup.FitToPagesTall = 0;
                    sheet.PageSetup.FitToPagesWide = 1;
                    sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet.PageSetup.CenterHorizontally = true;

                }

                UnplannedPRList(workbook.Worksheets[workbook.Worksheets.Count - 1]);
                workbook.Version = ExcelVersion.Excel2016;

            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return workbook;

        }
        private void UnplannedPRList(IWorksheet sheet)
        {
            try
            {
                sheet.Name = "Un-planned SO";
                UnplannedPRs(out DataTable dtOrderMaster);

                int ROW = 1; int COL = 1;
                sheet[ROW, 1].Text = "Control Chart(Un-planned Sales Order List)";
                sheet[ROW, 1].CellStyle.Font.Size = 16;
                sheet[ROW, 1].RowHeight = 22;
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                //sheet.Range[ROW, 1].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                //sheet.Range[ROW, 1, ROW, 10].Merge();


                ROW += 2;
                #region columns
                sheet[ROW, COL].Text = "Sl";
                sheet[ROW, COL].ColumnWidth = 4;
                int colSL = COL;
                COL++;
                sheet[ROW, COL].Text = "Plant";
                sheet[ROW, COL].ColumnWidth = 12;
                int colPlant = COL;
                COL++;
                sheet[ROW, COL].Text = "Entity Name";
                sheet[ROW, COL].ColumnWidth = 8;
                int colEntityName = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSONo = COL;
                COL++;
                sheet[ROW, COL].Text = "Line Item Reference";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLIR = COL;
                COL++;
                sheet[ROW, COL].Text = "Order Category";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOrderCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Status";
                sheet[ROW, COL].ColumnWidth = 8;
                int colSOStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Sales Order Planning Status";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSalesOrderPlanningStatus = COL;
                COL++;
                sheet[ROW, COL].Text = "Production Order#";
                sheet[ROW, COL].ColumnWidth = 8;
                int colProductionOrderId = COL;


                //COL++;
                //sheet[ROW, COL].Text = "Buyer";
                //sheet[ROW, COL].ColumnWidth = 14;
                //int colbuyer = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 14;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 14;
                int colMaterial = COL;
                COL++;
                sheet[ROW, COL].Text = "Product";
                sheet[ROW, COL].ColumnWidth = 14;
                int colProduct = COL;


                COL++;
                sheet[ROW, COL].Text = "Delivery Date";
                sheet[ROW, COL].ColumnWidth = 8;
                int colDeliveryDate = COL;

                COL++;
                sheet[ROW, COL].Text = "Master Order Id";
                sheet[ROW, COL].ColumnWidth = 10;
                int colMasterOrderId = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer Order#";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBuyerRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Order#";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOwnRefNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer Item#";
                sheet[ROW, COL].ColumnWidth = 10;
                int colStyleNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Own Item#";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOwnStyleNo = COL;

                COL++;
                sheet[ROW, COL].Text = "SO Desc";
                sheet[ROW, COL].ColumnWidth = 10;
                int colSODesc = COL;
                COL++;
                sheet[ROW, COL].Text = "SO Qty";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 8;
                int colQty = COL;

                int ROWSTART = ROW - 1;
                int ColStartRange = COL;



                #endregion columns

                int endCol = COL;

                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dtOrderMaster.DefaultView.Count; i++)
                {


                    sheet[ROW, colSL].Text = (i + 1).ToString();
                    sheet[ROW, colSOStatus].Text = dtOrderMaster.DefaultView[i]["SalesOrderStatusId"].ToString();
                    sheet[ROW, colProductionOrderId].Text = dtOrderMaster.DefaultView[i]["ProductionOrderId"].ToString();
                    sheet[ROW, colDeliveryDate].Text = clsStaticInfo.GetDate(dtOrderMaster.DefaultView[i]["DeliveryDate"].ToString());
                    sheet[ROW, colMaterial].Text = dtOrderMaster.DefaultView[i]["Material"].ToString();
                    sheet[ROW, colProduct].Text = dtOrderMaster.DefaultView[i]["Product"].ToString();
                    sheet[ROW, colEntityName].Text = dtOrderMaster.DefaultView[i]["EntityName"].ToString();
                    sheet[ROW, colPlant].Text = dtOrderMaster.DefaultView[i]["Plant"].ToString();


                    sheet[ROW, colMasterOrderId].Text = dtOrderMaster.DefaultView[i]["MasterOrderId"].ToString();
                    sheet[ROW, colBuyerRefNo].Text = dtOrderMaster.DefaultView[i]["BuyerRefNo"].ToString();
                    sheet[ROW, colOwnRefNo].Text = dtOrderMaster.DefaultView[i]["OwnRefNo"].ToString();
                    sheet[ROW, colStyleNo].Text = dtOrderMaster.DefaultView[i]["BuyerItemNo"].ToString();
                    sheet[ROW, colOwnStyleNo].Text = dtOrderMaster.DefaultView[i]["OwnItemNo"].ToString();
                    sheet[ROW, colSONo].Text = dtOrderMaster.DefaultView[i]["SalesOrderId"].ToString();
                    sheet[ROW, colLIR].Text = dtOrderMaster.DefaultView[i]["LineItemReference"].ToString();
                    sheet[ROW, colQty].Number = clsStaticInfo.dbl(dtOrderMaster.DefaultView[i]["SOQuantity"].ToString());
                    sheet[ROW, colSODesc].Text = dtOrderMaster.DefaultView[i]["SODesc"].ToString();
                    sheet[ROW, colOrderCategory].Text = dtOrderMaster.DefaultView[i]["OrderCategory"].ToString();
                    sheet[ROW, colCustomer].Text = dtOrderMaster.DefaultView[i]["Customer"].ToString();
                    sheet[ROW, colSalesOrderPlanningStatus].Text = dtOrderMaster.DefaultView[i]["StatusFlag"].ToString();


                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                IListObject table = sheet.ListObjects.Create("TableUnplannedSO", sheet[clsStaticInfo.GetxlsCol(1) + (3).ToString() + ":" + clsStaticInfo.GetxlsCol(endCol) + (ROW - 1).ToString()]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;

                sheet.UsedRange.NumberFormat = "#,##0;[Red](#,##0)";
                sheet.Range[2, 1, ROW, endCol].WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[4, 1].FreezePanes();
                //#region ******************Report Header******************

                sheet.IsDisplayZeros = false;
                sheet.IsGridLinesVisible = false;


                //#endregion ******************Report Header******************


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
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private void UnplannedPRs(out DataTable dt)
        {
            string sql = @"SELECT 
                            CASE WHEN ISNULL(pod.Id,'')='' THEN 'Missing Production Order' 
                            ELSE CASE WHEN ISNULL(TTT.Id,'')='' THEN 'Missing Planning Schedule' ELSE '' END END AS StatusFlag,
                            SO.Id AS SalesOrderId,SO.LineItemReference,so.OrderStatusId AS SalesOrderStatusId,ps.UserName AS ProductionStatus,
                             pod.ProductionOrderId,EN.UserName AS EntityName,PLN.UserName AS Plant,oc.UserName AS OrderCategory,
                            mm.userName AS Material,PM.UserName AS Product,pc.UserName AS ProductCategory,PM.Id ProductMasterId,
                            so.Qty AS SOQuantity, Format(so.DeliveryDate,'dd-MMM-yyyy') DeliveryDate,MO.Id AS MasterOrderId,
                            mo.BuyerReferenceNo AS BuyerRefNo,mo.OwnReferenceNo AS OwnRefNo,moi.BuyerReferenceNo AS BuyerItemNo,moi.OwnReferenceNo AS OwnItemNo,
                            so.[Description] AS SODesc,b.UserName AS Buyer,P.UserName AS Customer
                              FROM trn.SalesOrder SO 
                            LEFT JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                            LEFT JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                            LEFT JOIN ProductionOrderSchedulingParametersType1 AS TTT ON TTT.ProductionOrderID=pod.ProductionOrderId
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            LEFT JOIN hkp.OrderCategory AS oc ON oc.Id=so.OrderCategoryId
                            left join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                            LEFT JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId 
                            left join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                            left join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                            JOIN ORG.Entity EN ON EN.Id=MO.EntityId
                            JOIN ORG.Plant PLN ON PLN.Id=EN.PlantId

                            left outer join [HKP].Buyer B on B.Id=MO.BuyerId
                            left outer join [HKP].[Party] p on P.Id=MO.PartyId

                            WHERE (so.OrderStatusId NOT IN ('Closed','Cancelled','Cancel') AND  MO.OrderStatusId NOT IN ('Closed','Cancelled','Cancel'))
                            AND (ISNULL(pod.Id,'')='' OR ISNULL(TTT.ID,'')='') 

                            ORDER BY so.DeliveryDate";

            dt = _sqlRepository.GetDataTable(sql);

        }
        private void getOrderControl(out DataTable dt)
        {
            try
            {
                dt = _sqlRepository.GetDataTable(@"
select * from (SELECT   CASE WHEN ISNULL(oc.[Status],'')='Closed' THEN 1 ELSE 0 END AS Seq,ocat.UserName AS OrderCategory,
FORMAT(SO.CommitmentDate,'dd-MMM-yyyy') AS CommitmentDate,FORMAT(SO.PlanExFactoryDate,'dd-MMM-yyyy') AS PlanExFactoryDate,
MM.UserName AS Material,MMA.StandardName AS Article,
                                                    PLN.UserName AS Plant,EN.UserName AS Entity,
                                                     oct.Id AS ControlTypeId,oct.ControlType,oct.ControlTypeDesc,oc.CriticalityLevel,so.OrderStatusId AS Status,so.DeliveryDate AS [Date],
                                                    b.UserName AS Buyer,con.UserName AS BuyerCountry ,P.UserName AS Customer,
                                                    mo.id AS MasterOrderNo,moi.BuyerReferenceNo AS BuyerOrder,moi.OwnReferenceNo AS OwnOrder,
                                                    moi.BuyerReferenceNo AS BuyerItem,moi.OwnReferenceNo AS OwnItem,

                                                    eioc.EmployeeName AS ControlEmployee,eimo.EmployeeName AS MasterOrderEmployee,eien.EmployeeName AS EntityEmployee,
                                                    pod.ProductionOrderId AS PRNo,so.Id AS SoNo,SO.LineItemReference,cp.PONumber, so.Qty AS Quantity,oc.UpdatedDate,
                                                     Remarks=STUFF((select ','+format(ocrx.AddedDate,'dd-MMM-yyyy')+' '+ ocrx.Remarks
                                                                    from OrderControlRemarks AS ocrx
		                                                           where oc.Id=ocrx.OrderControlId  ORDER BY ocrx.AddedDate DESC	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
		                                                           so.[Description] AS SODescription
                                                    FROM  OrderControlTypes AS oct


							

                                                    left join trn.SalesOrder AS so ON 1=1
                                                    LEFT JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId
                                                      LEFT JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                                     LEFT OUTER JOIN dbo.OrderControl AS OC ON oc.SalesOrderId=so.Id AND oc.ControlTypeId=oct.id
                                                     --LEFT OUTER JOIN ON oct.Id=oc.ControlTypeId 



                                                     INNER JOIN trn.MasterOrderItem AS moi ON moi.id=so.MasterOrderItemId
                                                    INNER JOIN trn.MasterOrder AS mo ON mo.Id=moi.MasterOrderId
                                                    LEFT JOIN MST.MaterialMaster MM ON MM.Id=MOI.MaterialMasterId
                                                    Left JOIN MST.MaterialMasterArticle MMA ON MMA.Id=MOI.ArticleId
                                                    LEFT JOIN hkp.OrderCategory AS oCat ON ocat.Id=mo.OrderCategoryId
                                                    LEFT JOIN ORG.Entity EN ON EN.Id=MO.EntityId
                                                    LEFT JOIN ORG.Plant PLN ON PLN.Id=EN.PlantId


                                                    LEFT JOIN hkp.Buyer AS b ON b.Id=mo.BuyerId
                                                    left outer join [HKP].[Party] p on P.Id=MO.PartyId
                                                    LEFT JOIN mst.AddressMaster AS am ON am.Id=p.AddressMasterId
													left join scs.Country CON ON con.Id=am.CountryId
                                                    LEFT JOIN EmployeeInformation AS eioc ON eioc.SystemId=oct.ResponsiblePersonId
                                                    LEFT JOIN EmployeeInformation AS eimo ON eimo.SystemId=mo.ResponsiblePersonId
                                                    LEFT JOIN EmployeeInformation AS eien ON eien.SystemId=EN.EmployeeId
                                                    WHERE (so.OrderStatusId NOT IN ('Closed','Cancelled','Cancel') AND  MO.OrderStatusId NOT IN ('Closed','Cancelled','Cancel')) AND oct.ControlType='ShipmentControl' AND DATEADD(DAY,ISNULL(oct.LagDays,0),so.DeliveryDate)   <=  DATEADD(DAY,ISNULL(oct.Days,0),getdate())
                                                    ) AS K
                                                     ORDER BY seq,DATE");



                DataTable dt2 = _sqlRepository.GetDataTable(@"SELECT K.* FROM (SELECT CASE WHEN ISNULL(oct.[Status],'')='Closed' THEN 1 ELSE 0 END AS Seq,
                            PLN.UserName AS Plant,E.UserName AS Entity,
                             oct2.Id AS ControlTypeId,OCT2.ControlType,oct2.ControlTypeDesc,oct.CriticalityLevel,oct.[Status],
                            CASE WHEN oct2.DependentDate='PRMainRMInhouseDate' THEN t.MainRawMaterialInhouseDate ELSE
                            CASE WHEN oct2.DependentDate='PROtherRMInhouseDate' THEN t.OtherRawMaterialInhouseDate ELSE
                            CASE WHEN oct2.DependentDate='BaseProcessStartDate' THEN t.LSD  END END END  AS [Date],


                              Buyer =STUFF((select distinct ','+XB.UserName from 
                                    trn.SalesOrder XSO 
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                        left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                        left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
										                                 
                                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                             BuyerCountry =STUFF((select distinct ','+xCON.UserName from 
													trn.SalesOrder XSO 
													JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
													left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
													left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
													 left outer join [HKP].[Party] xp on xP.Id=xMO.PartyId
                                                    LEFT JOIN mst.AddressMaster AS xam ON xam.Id=xp.AddressMasterId
													left join scs.Country xCON ON xcon.Id=xam.CountryId
										                                 
                                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                            
											 Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                           
            
                            MasterOrderNo=STUFF((select distinct ','+XMO.MasterOrderNo from 
                                           trn.SalesOrder XSO 
                                               JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                               left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                              left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                                   where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            OrderCategory=STUFF((select distinct ','+xocat.UserName from 
                                           trn.SalesOrder XSO 
                                               JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                               left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                              left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                                    LEFT JOIN hkp.OrderCategory AS xoCat ON xocat.Id=xmo.OrderCategoryId
                                                   where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
               
                            

							    BuyerOrder =STUFF((select distinct ','+xmo.BuyerReferenceNo from 
                                    trn.SalesOrder XSO 
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                        left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId										                                 
                                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                            
                                            
                                OwnOrder =STUFF((select distinct ','+xmo.OwnReferenceNo from 
                                    trn.SalesOrder XSO 
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                        left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId										                                 
                                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                            
                                            
                                           BuyerItem =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
                                    trn.SalesOrder XSO 
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId							                                 
                                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                            
                                            
                                           OwnItem =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
                                    trn.SalesOrder XSO 
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId							                                 
                                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                            
                                            eioc.EmployeeName AS ControlEmployee,
                                 MasterOrderEmployee =STUFF((select distinct ','+eimox.EmployeeName from 
                                    trn.SalesOrder XSO 
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                        left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId			
                                        LEFT JOIN EmployeeInformation AS eimox ON eimox.SystemId=XMO.ResponsiblePersonId							                                 
                                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                            
                                            eien.EmployeeName AS EntityEmployee,
                              
                            po.Id AS PRNo,
                              SoNo=STUFF((select distinct ','+xso.Id from 
                                        trn.SalesOrder XSO 
                                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                 LineItemReference=STUFF((select distinct ','+xso.LineItemReference from 
                                        trn.SalesOrder XSO 
                                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),                     
                                               
                                               PONumber=STUFF((select distinct ','+xcp.Id from 
                                        trn.SalesOrder XSO 
                                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                                JOIN trn.CustomerPO AS xcp ON xcp.Id=xso.CustomerPOId
                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            
                            
                            
                            PO.Qty AS Quantity,
                            oct.UpdatedDate,

															 Remarks=STUFF((select ','+format(ocrx.AddedDate,'dd-MMM-yyyy')+' '+ ocrx.Remarks
                from OrderControlRemarks AS ocrx
		       where oct.Id=ocrx.OrderControlId  ORDER BY ocrx.AddedDate DESC	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
															
															  [SODescription]=STUFF((select distinct ','+xso.[Description] from 
                                        trn.SalesOrder XSO 
                                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
															
                        from OrderControlTypes AS oct2
                          LEFT JOIN  trn.ProductionOrder PO ON 1=1
                          JOIN ProductionOrderSchedulingParametersType1 AS T ON t.ProductionOrderID=po.Id
                           left  JOIN OrderControl AS OCT ON oct.ProductionOrderId=PO.Id AND oct2.Id=oct.ControlTypeId
                           -- JOIN  ON oct2.Id=oct.ControlTypeId  --AND isnull(oct.[Status],'')<>'Closed'
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                             left outer join org.Entity E  on e.Id=po.EntityID
                            LEFT OUTER JOIN org.Unit AS u ON u.Id=e.UnitId
                            left outer join org.Plant PLN on pln.Id=PO.PlantId
                            
                            LEFT JOIN EmployeeInformation AS eioc ON eioc.SystemId=oct2.ResponsiblePersonId
                            LEFT JOIN EmployeeInformation AS eien ON eien.SystemId=E.EmployeeId

							left join (
							select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
							)PD ON PD.ProductionOrderId=PO.Id
							
WHERE  ps.UserName IN ('Active','Running') AND oct2.ControlType<>'ShipmentControl' AND po.Id IN (SELECT pod.ProductionOrderId
                                  FROM trn.ProductionOrderDetail AS pod
                          INNER JOIN trn.SalesOrder AS so ON so.Id=pod.SalesOrderId WHERE so.OrderStatusId NOT IN ('Closed','Cancelled','Cancel'))

) AS K  
INNER JOIN OrderControlTypes AS oct2 ON oct2.Id=k.ControlTypeId
 WHERE DATEADD(DAY,ISNULL(oct2.LagDays,0),k.date)   <=  DATEADD(DAY,ISNULL(oct2.Days,0),getdate())
--and OCT2.ControlType NOT IN ('" + OrderControlTypes.MainRMShipment.ToString() + "','" + OrderControlTypes.OtherRMShipment.ToString() + @"','" + OrderControlTypes.MainRMInhouse.ToString() + @"','" + OrderControlTypes.OtherRMInhouse.ToString() + @"')
ORDER BY ControlTypeId,seq,date");

                dt.Merge(dt2);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
