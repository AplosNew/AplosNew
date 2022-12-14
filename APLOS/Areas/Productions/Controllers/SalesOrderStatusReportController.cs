using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using Library.HumanResource.Attendance;

namespace Aplos.Areas.Productions.Controllers
{
    public class SalesOrderStatusReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        public SalesOrderStatusReportController(
            ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor



        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpPost, Authorize]
        public ActionResult XlsSalesOrderStatusReport(List<string> EntityList)
        {
            try
            {
                var workbook = SalesOrderStatusReport(EntityList);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "SOStatusReport.xlsx";
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
        private IWorkbook SalesOrderStatusReport(List<string> EntityList)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine,1);
            workbook.Version = ExcelVersion.Excel2016;

            var data = SalesOrderStatusReportQuery(EntityList);

            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Sales Order Status Report";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Customer Group", 14, ExcelHAlign.HAlignLeft);
            int ColCusG = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Created Date", 12, ExcelHAlign.HAlignLeft);
            int ColCd = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Customer", 25, ExcelHAlign.HAlignLeft);
            int ColCus = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Master Order No", 12, ExcelHAlign.HAlignLeft);
            int ColMO = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Master Order Date", 12, ExcelHAlign.HAlignLeft);
            int ColMOD = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColItem = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 40, ExcelHAlign.HAlignLeft);
            int ColArt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ContractID", 20, ExcelHAlign.HAlignLeft);
            int ColCont = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Own Ref No", 12, ExcelHAlign.HAlignLeft);
            int ColOwn = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Buyer Order No", 12, ExcelHAlign.HAlignLeft);
            int ColBuy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Product Code", 20, ExcelHAlign.HAlignLeft);
            int ColProdC = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Product Detail", 20, ExcelHAlign.HAlignLeft);
            int ColProd = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PR No", 20, ExcelHAlign.HAlignLeft);
            int ColPR = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO NO", 12, ExcelHAlign.HAlignLeft);
            int COlSo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO Category", 16, ExcelHAlign.HAlignLeft);
            int ColSOCat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SO Qty", 12, ExcelHAlign.HAlignRight);
            int ColQty = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Ex Factory Date", 12, ExcelHAlign.HAlignLeft);
            int ColEFD = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Commitment Date", 12, ExcelHAlign.HAlignLeft);
            int ColComm = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Delivery Date", 12, ExcelHAlign.HAlignLeft);
            int ColDel = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate", 12, ExcelHAlign.HAlignRight);
            int ColRate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ExchangeRate", 12, ExcelHAlign.HAlignRight);
            int ColExRate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Dispatch Qty", 12, ExcelHAlign.HAlignRight);
            int ColDis = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Balance To Dispatch", 12, ExcelHAlign.HAlignRight);
            int ColBal = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "FG Current Stock", 12, ExcelHAlign.HAlignRight);
            int ColAll = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 20, ExcelHAlign.HAlignLeft);
            int ColResponsiblePerson = COL;
        

            endCol = COL;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

            ROW++;
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
                sheet[ROW, ColCus].Text = data.Rows[i]["Customer"].ToString();
                sheet[ROW, ColCusG].Text = data.Rows[i]["CustomerGroup"].ToString();
                sheet[ROW, ColMO].Number = clsStaticInfo.dbl(data.Rows[i]["MasterOrderNo"].ToString());
                sheet[ROW, ColMOD].DateTime = Convert.ToDateTime(data.Rows[i]["MasterOrderDate"].ToString());
                sheet[ROW, ColCd].DateTime = Convert.ToDateTime(data.Rows[i]["CreatedDate"].ToString());
                sheet[ROW, ColDel].DateTime = Convert.ToDateTime(data.Rows[i]["DeliveryDate"].ToString());
                sheet[ROW, ColOwn].Text = data.Rows[i]["OwnReferenceNo"].ToString();
                sheet[ROW, ColCont].Text = data.Rows[i]["ContractID"].ToString();
                sheet[ROW, ColBuy].Text = data.Rows[i]["BuyerOrderNo"].ToString();
                sheet[ROW, ColArt].Text = data.Rows[i]["Article"].ToString();
                sheet[ROW, ColItem].Text = data.Rows[i]["ItemId"].ToString();
                sheet[ROW, COlSo].Text = data.Rows[i]["SONo"].ToString();
                sheet[ROW, ColProd].Text = data.Rows[i]["ProdDetails"].ToString();
                sheet[ROW, ColProdC].Text = data.Rows[i]["ProductCode"].ToString();
                sheet[ROW, ColPR].Text = data.Rows[i]["ProductionOrderId"].ToString();
                sheet[ROW, ColQty].Number = clsStaticInfo.dbl(data.Rows[i]["SOQty"].ToString());
                sheet[ROW, ColEFD].Text = data.Rows[i]["ExFactoryDate"].ToString();
                sheet[ROW, ColComm].Text = data.Rows[i]["CommitmentDate"].ToString();
                sheet[ROW, ColSOCat].Text = data.Rows[i]["SOCategory"].ToString();
                sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data.Rows[i]["Rates"].ToString());
                sheet[ROW, ColExRate].Number = clsStaticInfo.dbl(data.Rows[i]["ExchangeRate"].ToString());
                sheet[ROW, ColDis].Number = clsStaticInfo.dbl(data.Rows[i]["DispatchQty"].ToString());
                sheet[ROW, ColBal].Number = clsStaticInfo.dbl(data.Rows[i]["BalanceToDispatch"].ToString());
                sheet[ROW, ColAll].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedStock"].ToString());
                sheet[ROW, ColResponsiblePerson].Text = data.Rows[i]["ResponsiblePerson"].ToString();

                ROW++;

            }
            sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyHeader(ref sheet, endCol, "Sales Order Status Report", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion -- Operations  

        #region Queries

        private DataTable SalesOrderStatusReportQuery(List<string> EntityList)
        {
            try
            {
                string ent = "";
                if (EntityList.Count > 0 && EntityList[0] != "")
                {
                    ent = "AND mo.EntityId IN (''";
                    foreach (string e in EntityList)
                    {
                        ent += ",'" + e + "'";
                    }
                    ent += ")";
                }

                var str = @"Select  p.UserName as Customer, mo.MasterOrderNo , format(mo.AddedDate,'dd-MMM-yyyy') as MasterOrderDate ,moi.ContractId, moi.OwnReferenceNo , moi.BuyerReferenceNo as BuyerOrderNo , mma.StandardName as Article, moi.Id as ItemId , so.Id as SONo , so.Qty as SOQty , format(so.PlanExFactoryDate,'dd-MMM-yyyy') as ExFactoryDate , 
                            format(so.CommitmentDate , 'dd-MMM-yyyy') as CommitmentDate , format(so.DeliveryDate , 'dd-MMM-yyyy') as DeliveryDate , oc.UserName as SOCategory , so.Rate , so.CM , isnull(sm.DispatchQty,0) as DispatchQty , 
                            (so.Qty -  isnull(sm.DispatchQty,0)) as BalanceToDispatch , moi.ProductLibraryId, PAG.UserName as CustomerGroup,pl.Code as ProductCode, pod.ProductionOrderId,format(mo.AddedDate,'dd-MMM-yyyy') as CreatedDate,

                             (Select Stuff((
                                                        Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
                                                        from dbo.ProductLibraryAttribute pla
                                                        where pla.ProductLibraryId = moi.ProductLibraryId
                                                        for XML PATH('')
                                                        ) , 1, 2, '')) as ProdDetails,

                            (Select sum(NetWeight) 
                            from dbo.ItemScanChild sc
                            left join dbo.ItemScan s on s.Id = sc.MasterId 
                            left join dbo.ProductLibrary pl on pl.Code = sc.ProductCode
                            left join MST.MaterialMovementMaster MMM ON MMM.Id = SC.LocMasterId
                            where pl.Id = moi.ProductLibraryId 
                            and s.WorkDate <= GetDate()
                            and sc.Booked = 0 and  (MMM.PurposeId <> 'MP7' AND MMM.PurposeId <> 'MP8' AND MMM.PurposeId <> 'MP9' AND MMM.PurposeId <> 'MP12')) as AllotedStock
                            , so.Rate as Rates, mor.ExchangeRate,E.EmployeeName ResponsiblePerson
                            from trn.SalesOrder so
                            left join trn.MasterOrderItem moi on moi.Id = so.MasterOrderItemId
                            left join trn.MasterOrder mo on mo.Id = moi.MasterOrderId
                            left join mst.MaterialMasterArticle mma on mma.Id = moi.ArticleId
                            left join hkp.OrderCategory oc on oc.Id = so.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS on OS.id=so.OrderStatusId
                            left join dbo.ProductLibrary pl on pl.ID = moi.ProductLibraryId
                            left join MasterOrderExchangeRates mor on mor.TransactionId = mo.Id
                            left join
                            (
                            Select SalesOrderId , SUM(isnull(sm.TransactionQty , 0)) as DispatchQty
                            from trn.SalesMaterial sm
                            group by SalesOrderId
                            ) as sm on sm.SalesOrderId = so.Id
                            left join hkp.Party p on p.Id = mo.PartyId
                            LEFT JOIN [HKP].[CompanyParty] AS COMP ON COMP.PartyId=P.Id AND COMP.PartyType='Customer'
                             LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=COMP.PartyAccountGroupId
                             left join trn.ProductionOrderDetail pod on pod.SalesOrderId = so.Id
                             left join dbo.EmployeeInformation E ON e.SystemId=so.ResponsiblePersonId
                            where os.Id not in ('Closed' , 'Cancelled') 
                            order by pag.UserName asc, convert(datetime, mo.AddedDate, 103) desc
                            ";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
