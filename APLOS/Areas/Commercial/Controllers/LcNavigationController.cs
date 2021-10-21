#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Commercial.Controllers
{
    public class LcNavigationController : BaseController
    {  

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public LcNavigationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
        public ActionResult Aplos()
        {
            return View();
        }     
        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCSearchByDate(string fromDate,string toDate)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                 var data = navigation.GetPurchaseLCSearchByDate(fromDate,toDate);
                return Json(new { DATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCSearch()
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();
                var data = navigation.GetPurchaseLCSearch();
                return Json(new { DATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetNonTagLCSearchByDate(string fromDate, string toDate)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.GetNonTagLCSearchByDate(fromDate, toDate);
                return Json(new { DATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }




        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCReport(Dictionary<string, object> Filter, string FilterFields)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();
               PurchaseLCReport(Filter,FilterFields);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public void PurchaseLCReport(Dictionary<string, object> Filter, string FilterFields)
        {
            try
            {
                string sql = PurchaseLCReportSql(Filter, FilterFields);
                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                workbook.Version = ExcelVersion.Excel2013;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "PurchaseLc.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);

                sheet.Name = "Purchase LC";

                DataTable dtPurchaseLc = _sqlRepository.GetDataTable(sql);

                int ROW = 6;
                int COL = 1;


                sheet[ROW, COL].Text = "Sl No.";

                sheet[ROW, COL].ColumnWidth = 6;
                int colSlNo = COL;
                COL++;

                sheet[ROW, COL].Text = "LC No.";

                sheet[ROW, COL].ColumnWidth = 10;
                int colLCNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Opening Bank";

                sheet[ROW, COL].ColumnWidth = 10;
                int colOpeningBank = COL;
                COL++;
                sheet[ROW, COL].Text = "Opening Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colOpeningDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Vendor";
                sheet[ROW, COL].ColumnWidth = 20;
                int colVendor = COL;
                COL++;
                sheet[ROW, COL].Text = "Value";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colValue = COL;
                COL++;
                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 5;
                int colCurrency = COL;
                COL++;
                sheet[ROW, COL].Text = "LCA No";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLCANo = COL;
                COL++;
                sheet[ROW, COL].Text = "LC Type";
                sheet[ROW, COL].ColumnWidth = 10;
                int colLCType = COL;
                COL++;
                sheet[ROW, COL].Text = "Tenure";
                sheet[ROW, COL].ColumnWidth = 10;
                int colTenure = COL;
                COL++;
                sheet[ROW, COL].Text = "Benificiary Bank";
                sheet[ROW, COL].ColumnWidth = 15;
                int colBenificiaryBank = COL;
                COL++;
                sheet[ROW, COL].Text = "PO Value";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colPOValue = COL;
                COL++;
                sheet[ROW, COL].Text = "Acceptance Value";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colAcceptanceValue = COL;

                COL++;
                sheet[ROW, COL].Text = "GRN Value";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colGRNValue = COL;
                COL++;
                sheet[ROW, COL].Text = "Payment Made";
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet[ROW, COL].ColumnWidth = 10;
                int colPaymentMade = COL;
                COL++;
                sheet[ROW, COL].Text = "Contract No";

                sheet[ROW, COL].ColumnWidth = 10;
                int colContractNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 10;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "LC Id";

                sheet[ROW, COL].ColumnWidth = 10;
                int colLCId = COL;



                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                int StartRow = ROW; //row 20
                for (int i = 0; i < dtPurchaseLc.Rows.Count; i++)
                {


                    sheet[ROW, colSlNo].Number = (i + 1);

                    sheet[ROW, colLCNo].Text = dtPurchaseLc.Rows[i]["LCNo"].ToString();
                    sheet[ROW, colOpeningBank].Text = dtPurchaseLc.Rows[i]["OpeningBank"].ToString();
                    sheet[ROW, colOpeningDate].Text = dtPurchaseLc.Rows[i]["OpeningDate"].ToString();
                    sheet[ROW, colVendor].Text = dtPurchaseLc.Rows[i]["Vendor"].ToString();
                    sheet[ROW, colValue].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["Value"].ToString());
                    sheet[ROW, colCurrency].Text = dtPurchaseLc.Rows[i]["Currency"].ToString();
                    sheet[ROW, colLCANo].Text = dtPurchaseLc.Rows[i]["LCANo"].ToString();
                    sheet[ROW, colLCType].Text = dtPurchaseLc.Rows[i]["LCType"].ToString();
                    sheet[ROW, colTenure].Text = dtPurchaseLc.Rows[i]["Tenure"].ToString();
                    sheet[ROW, colBenificiaryBank].Text = dtPurchaseLc.Rows[i]["BenificiaryBank"].ToString();
                    sheet[ROW, colPOValue].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["POValue"].ToString());
                    sheet[ROW, colAcceptanceValue].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["AcceptanceValue"].ToString());
                    sheet[ROW, colGRNValue].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["GRNValue"].ToString());
                    sheet[ROW, colPaymentMade].Text = dtPurchaseLc.Rows[i]["PaymentMade"].ToString();
                    sheet[ROW, colContractNo].Text = dtPurchaseLc.Rows[i]["ContractNo"].ToString();
                    sheet[ROW, colCustomer].Text = dtPurchaseLc.Rows[i]["Customer"].ToString();
                    sheet[ROW, colLCId].Text = dtPurchaseLc.Rows[i]["LCId"].ToString();



                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }

                sheet.Range[StartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colPOValue, ROW, colPOValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colAcceptanceValue, ROW, colAcceptanceValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[StartRow, colGRNValue, ROW, colGRNValue].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();

                sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colSlNo, ROW, colSlNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Purchase LC", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string FileName = "Purchase LC.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private string PurchaseLCReportSql(Dictionary<string, object> Filter, string FilterFields)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select
                        PL.LCRef as LCNo,
                        B.UserName as OpeningBank,
                        FORMAT( PL.LCDate,'dd-MMM-yyyy' )as OpeningDate,
                        P.UserName as  Vendor,
                        PL.Amount as Value,
                        Cur.Code as Currency,
                        PL.LCANo,PL.Type as LCType,
                        PL.Tenure,
                        PL.BenificiaryBank,                       
                        po.POAmount as POValue
                        ,ac.AcceptanceValue,
                        grn.GRNCount
                        ,grn.GRNTotalAmount as GRNValue,
                        case when PM.PaymentMade = 0 then null else PM.PaymentMade end as PaymentMade,
                        con.ContractNo,
                        cus.Customer,
                        PL.Id as LCId
						,PL.PINo,ML.LCRef MasterLCNo,PL.Id MasterLCId,Con.UDNo
						,Loan.Amount Loan
                        from PurchaseLC as PL
                        left outer join MST.BankMaster as OBank on PL.OpeningBankMasterId=OBank.Id
                        left outer join HKP.Bank as B on OBank.BankId=b.Id
                        left outer join [Contract] as Con on PL.ContractId= Con.Id
                        left outer join scs.Currency as Cur on PL.CurrencyId = Cur.Id
                        left outer join MST.Destination as D on PL.CurrencyId=D.Id
                        left outer join HKP.Party as P on PL.VendorId = p.Id
						left outer join MasterLC ML on ML.Id=con.MasterLCId
                        left join (
						          select po.PurchaseLCId,sum(pod.TransactionAmount) AS POAmount,count(distinct po.Id) AS POCount from TRN.PurchaseOrder PO 
                                  inner JOin trn.PurchaseOrderDetail POD ON POD.InventoryReceiveId=po.Id
                                      group by  po.PurchaseLCId) AS PO on PO.PurchaseLCId=pl.Id
                        left join(
									select  po.PurchaseLCId as LCId,sum(g.TotalMaterialTranAmount) as GRNTotalAmount,count(distinct g.InventoryReceiveId) as GRNCount from TRN.purchaseorder as po 
									inner join TRN.InventoryReceiveDetail as g on g.POId=po.Id
									group by po.PurchaseLCId
                        ) as grn on grn.LCId = PL.Id 
                        left join (
									select sum(AD.TotalMaterialTranAmount) as AcceptanceValue,A.PurchaseLCId,A.Id  from TRN.PurchaseDocAcceptanceDetail as AD
									 inner join trn.PurchaseDocAcceptance as A on A.Id=AD.PurchaseDocAcceptanceId
									group by A.PurchaseLCId,A.Id
                        ) as ac on ac.PurchaseLCId = PL.Id

						left join(select PDA.PurchaseLCId,sum(LAA.Amount) Amount from TRN.LoanAgainstAcceptance LAA 
											left outer join TRN.PurchaseDocAcceptance PDA on PDA.Id=LAA.PurchaseDocAcceptanceId
											group by PDA.PurchaseLCId												
						) Loan on Loan.PurchaseLCId=PL.Id

                        left outer join (
										 select con.Id as Id, customer.UserName as Customer from Contract as con 
										inner join HKP.Party as customer on con.CustomerId=customer.Id)
										as cus on cus.Id=PL.ContractId
                         left join (
										 select Ac.PurchaseLCId,sum(i.WrittenOffAmount) AS PaymentMade from TRN.PurchaseDocAcceptance AC
										inner join  trn.invoice I on i.PurchaseDocAcceptanceId=ac.Id
										 group by Ac.PurchaseLCId
						 ) as PM on PM.PurchaseLCId=PL.Id
                         where pl.plantId='" + identity.PlantId + @"' and PL.Id in ('" + FilterFields + @"')";

        }


        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCPOList(string PurchaseLCId)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.GetPurchaseLCPOList(PurchaseLCId);

                return Json(new { MaterialPODATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCServicePOList(string PurchaseLCId)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.GetPurchaseLCServicePOList(PurchaseLCId);

                return Json(new { ServicePODATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost, Authorize]
        public ActionResult ServicePOBreakDownDataList(string POID)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.ServicePOBreakDownList(POID);

                return Json(new { ServicePOBrDATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost, Authorize]
        public ActionResult JWPOBreakDownDataList(string POID)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.JWPOBreakDownList(POID);

                return Json(new { JWPOBrDATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCJWPOList(string PurchaseLCId)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.GetPurchaseLCJWPOList(PurchaseLCId);

                return Json(new { JWPODATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult POBreakDownDataList(string POID)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.POBreakDownList(POID);

                return Json(new { POBrDATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult NonLcGRNBreakDownDataList(string POID)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.NonLcGRNBreakDownList(POID);

                return Json(new { NonLCGRNData = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GRNBreakDownDataList(string GRNID)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.GRNBreakDownList(GRNID);

                return Json(new { GRNBrDATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult ACBreakDownDataList(string ACID)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.ACBreakDownList(ACID);

                return Json(new { ACBrDATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCGRNList(string PurchaseLCId)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();
                var data = navigation.GetPurchaseLCGRNList(PurchaseLCId);
                return Json(new { GRNDATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCACList(string PurchaseLCId)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();
                var data = navigation.GetPurchaseLCACList(PurchaseLCId);
                return Json(new { ACDATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCLoanList(string PurchaseLCId)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.GetPurchaseLCLoanList(PurchaseLCId);

                return Json(new { LoanDATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCSetOff(string PurchaseLCId)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.GetPurchaseLCSetOff(PurchaseLCId);

                return Json(new { SetOffDATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCLoanSetOff(string PurchaseLCId)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.GetPurchaseLCLoanSetOff(PurchaseLCId);

                return Json(new { LoanSetOffDATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {

            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select
                        PL.LCRef as LCNo,
                        B.UserName as OpeningBank,
                        FORMAT( PL.LCDate,'dd-MMM-yyyy' )as OpeningDate ,
                        P.UserName as  Vendor,
                        ISNULL(PL.Amount,0) [Value],
                        Cur.Code as Currency,
                        PL.LCANo,PL.Type as LCType,
                        PL.Tenure,
                        PL.BenificiaryBank                 
                        ,ISNULL(PO.MaterialPOAmount,0) MaterialPOAmount		
						,ISNULL(PO.ServicePOAmount,0) ServicePOAmount
						,ISNULL(PO.JWPOAmount,0) JWPOAmount
						,ISNULL(grn.GRNTotalAmount,0) GRNValue
						,variance=ISNULL(CASE 
						 WHEN po.MaterialPOAmount=0 AND PO.ServicePOAmount=0 THEN (PO.JWPOAmount -grn.GRNTotalAmount) 
						 WHEN po.ServicePOAmount=0 AND PO.JWPOAmount=0 THEN (PO.MaterialPOAmount -grn.GRNTotalAmount)
						 WHEN po.MaterialPOAmount=0 AND PO.JWPOAmount=0 THEN (PO.ServicePOAmount -grn.GRNTotalAmount)
						 END,0)
						,ISNULL(PO.POCount,0) POCount
                        ,PL.AddedDate
                        ,Isnull(ac.AcceptanceValue,0) AcceptanceValue
						,Isnull(invpy.InvPayment,0) SetOff 
						,Isnull(Loan.Amount,0) Loan
						,Isnull(LoanSetOff.LoanSetOff,0) LoanSetOff
						,ISNULL(ac.AcceptanceCount,0) AcceptanceCount						
                        ,ISNULL(grn.GRNCount,0) GRNCount
	                    ,IsClosed=case when PL.Status='Active' then 'No' else 'Yes' END
						,[Sequence]=case when Pl.IsAccepptanceFirst=1 then 'AccepptanceFirst' else'GRNFirst' END ,
                        con.ContractNo,
                        cus.Customer,
                        PL.Id as LCId
						,PL.PINo,ML.LCRef MasterLCNo,PL.Id MasterLCId,Con.UDNo
						,FORMAT(PL.ExpiryDate,'dd-MMM-yyyy') ExpiryDate						
						,[Status]=case when PL.Status='Active' then 'Active' else 'Closed' END				
					
                      --,IsAccepptanceFirst1= case when pl.IsAccepptanceFirst ='1' then convert (bit,'True') else convert (bit,'False') end
						,IsAccepptanceFirst= case when pl.IsAccepptanceFirst =1 then'True' else 'False' end
                        from PurchaseLC as PL
                        left outer join MST.BankMaster as OBank on PL.OpeningBankMasterId=OBank.Id
                        left outer join HKP.Bank as B on OBank.BankId=b.Id
                        left outer join [Contract] as Con on PL.ContractId= Con.Id
                        left outer join scs.Currency as Cur on PL.CurrencyId = Cur.Id
                        left outer join MST.Destination as D on PL.CurrencyId=D.Id
                        left outer join HKP.Party as P on PL.VendorId = p.Id
						left outer join MasterLC ML on ML.Id=con.MasterLCId
						left outer  join (Select COUNT(Id) AcceptanceCount,sum(AcceptanceAmount) AcceptanceValue,PurchaseLCId from TRN.PurchaseDocAcceptance GROUP BY PurchaseLCId) AC on AC.PurchaseLCId=PL.Id

                        left join (
						        select k.PurchaseLCId,sum(MaterialPOAmount) AS MaterialPOAmount,sum(JWPOAmount) AS JWPOAmount,sum(ServicePOAmount) AS ServicePOAmount
								,count(distinct k.Id) AS POCount from (  
								select po.PurchaseLCId,pod.TransactionAmount AS MaterialPOAmount,0 AS JWPOAmount,0 AS ServicePOAmount, po.Id
								   from TRN.PurchaseOrder PO 
                                  inner JOin trn.PurchaseOrderDetail POD ON POD.InventoryReceiveId=po.Id
                                     
									  union ALL

							    select po.PurchaseLCId,0 AS MaterialPOAmount,pod.TransactionAmount,0 AS ServicePOAmount, po.Id
								 from [dbo].[OSTransformationPO]  PO 
                                  inner JOin [dbo].[OSTransformationPODetail] POD ON POD.OSTransformationPOId=po.Id

								   union ALL

								   select po.PurchaseLCId,0 AS MaterialPOAmount,0 AS JWPOAmount,POD.Amount AS ServicePOAmount, po.Id
								 from trn.ServicePOMaster PO 
                                  inner JOin trn.ServicePODetail POD ON POD.ServicePOMasterId=po.Id

                                     
									  ) AS K group by k.PurchaseLCId
									  ) AS PO on PO.PurchaseLCId=pl.Id

                        left join(
									select  po.PurchaseLCId as LCId,sum(g.TotalMaterialTranAmount) as GRNTotalAmount,count(distinct g.InventoryReceiveId) as GRNCount from TRN.purchaseorder as po 
									inner join TRN.InventoryReceiveDetail as g on g.POId=po.Id
									group by po.PurchaseLCId
									union 
									
									 select  po.PurchaseLCId as LCId,sum(g.TransactionQty*JWTCC.RatePerUnit) as GRNTotalAmount,count(distinct g.InventoryReceiveId) as GRNCount from  [dbo].[OSTransformationPO] as po 
									inner join TRN.InventoryReceiveDetail as g on g.OSTransformationPOId=po.Id
									LEFT JOIN [MST].[JobWorkTransformationMaster] JWTM ON JWTM.Id=g.OSTransformationPOId
						            LEFT JOIN dbo.OSTransformationPODetail JWTCC ON JWTCC.Id=g.OSTransformationPODetailId
									group by po.PurchaseLCId
								    union 
								    select  po.PurchaseLCId as LCId,sum(g.Amount) as GRNTotalAmount,count(distinct g.ServicePOMasterId) as GRNCount from  trn.ServicePOMaster PO 
									inner join TRN.ServiceAcknowledgementDetail as g on g.ServicePOMasterId=po.Id
									group by po.PurchaseLCId

                        ) as grn on grn.LCId = PL.Id 
                       
	left join(select PDA.PurchaseLCId,sum(LAA.Amount) Amount
										from TRN.LoanAgainstAcceptance LAA 
											left outer join TRN.PurchaseDocAcceptance PDA on PDA.Id=LAA.PurchaseDocAcceptanceId											
											group by PDA.PurchaseLCId												
						) Loan on Loan.PurchaseLCId=PL.Id

					   left join(select PDA.PurchaseLCId,SUM(FDW.Amount) LoanSetOff 
										from TRN.LoanAgainstAcceptance LAA 
											left outer join TRN.PurchaseDocAcceptance PDA on PDA.Id=LAA.PurchaseDocAcceptanceId
											LEFT JOIN TRN.Financing F ON F.LoanAgainstAcceptanceId=LAA.Id 
											LEFT JOIN TRN.FinancingDetailWriteOff FDW ON FDW.FinancingId=F.Id
											group by PDA.PurchaseLCId												
						) LoanSetOff on LoanSetOff.PurchaseLCId=PL.Id
						left join(select PDA1.PurchaseLCId,sum(isnull(FDW.Amount,0)) InvPayment
										from TRN.PurchaseDocAcceptance PDA1 
											LEFT JOIN TRN.Invoice F ON F.PurchaseDocAcceptanceId=PDA1.Id 
											LEFT JOIN TRN.InvoiceWriteOffDetail FDW ON FDW.InvoiceId=F.Id
											where FDW.Amount>0 and PDA1.PurchaseLCId<>''
											group by PDA1.PurchaseLCId												
						) invpy on invpy.PurchaseLCId=PL.Id

                        left outer join (
										 select con.Id as Id, customer.UserName as Customer from Contract as con 
										inner join HKP.Party as customer on con.CustomerId=customer.Id
										)
										as cus on cus.Id=PL.ContractId
                         where 
						 pl.plantId='" + identity.PlantId+@"') AS TEMP WHERE " + strkey + "order by TEMP.OpeningDate DESC ";
            
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetNonTagLcSearchList(string column, string value)        
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select * from (select PO.Id PONo,FORMAT(PO.PODate,'dd-MMM-yyy') PODate
,PT.PaymentMode,PO.DocRefNo VendorRef
, POD.POAmount,c.Code Currency,P.UserName Vendor
,GRN.GRNTotalAmount
,PO.AddedDate,PT.UserName PaymentTerm
from trn.PurchaseOrder PO
left outer join (select sum(TransactionAmount) POAmount,InventoryReceiveId from  TRN.PurchaseOrderDetail group by InventoryReceiveId)POD on POD.InventoryReceiveId=PO.Id
left outer join mst.PaymentTerm PT on PT.Id=PO.PaymentTermId
left outer join SCS.Currency C on c.Id=PO.CurrencyId
left outer join hkp.Party P on P.Id=PO.PartyId
                            left join
                             (select  IRD.POId,sum(IRD.TotalMaterialTranAmount) as GRNTotalAmount,count(distinct IRD.InventoryReceiveId) as GRNCount
							from TRN.InventoryReceiveDetail IRD
							group by IRD.POId)
                            as grn on grn.POId=PO.Id
							where PO.PurchaseLCId is null and  PO.PlantId='"+identity.PlantId+ @"' and PT.PaymentMode='LC'
							

union all
select PO.Id PONo,FORMAT(PO.PODate,'dd-MMM-yyy') PODate
,PT.PaymentMode,PO.DocRefNo VendorRef
, POD.POAmount,c.Code Currency,P.UserName Vendor
,GRN.GRNTotalAmount
,PO.AddedDate,PT.UserName PaymentTerm
from [dbo].[OSTransformationPO] PO
left outer join (select sum(TransactionAmount) POAmount,InventoryReceiveId from  TRN.PurchaseOrderDetail
group by InventoryReceiveId)POD on POD.InventoryReceiveId=PO.Id
left outer join mst.PaymentTerm PT on PT.Id=PO.PaymentTermId
left outer join SCS.Currency C on c.Id=PO.CurrencyId
left outer join hkp.Party P on P.Id=PO.PartyId
                            left join
                             (select  IRD.POId,sum(IRD.TotalMaterialTranAmount) as GRNTotalAmount
							 ,count(distinct IRD.InventoryReceiveId) as GRNCount
							from TRN.InventoryReceiveDetail IRD
							group by IRD.POId)
                            as grn on grn.POId=PO.Id
							where PO.PurchaseLCId is null and  PO.PlantId='" + identity.PlantId + @"' and PT.PaymentMode='LC'
							
							union all

select PO.Id PONo,FORMAT(PO.PODate,'dd-MMM-yyy') PODate
,PT.PaymentMode,PO.DocRefNo VendorRef
, POD.POAmount,c.Code Currency,P.UserName Vendor
,GRN.GRNTotalAmount
,PO.AddedDate,PT.UserName PaymentTerm
from TRN.ServicePOMaster PO
left outer join (select sum(Amount) POAmount,ServicePOMasterId from TRN.ServicePODetail
group by ServicePOMasterId)POD on POD.ServicePOMasterId=PO.Id
left outer join mst.PaymentTerm PT on PT.Id=PO.PaymentTermId
left outer join SCS.Currency C on c.Id=PO.CurrencyId
left outer join hkp.Party P on P.Id=PO.PartyId
                            left join
                             (select  IRD.ServicePOMasterId,sum(IRD.Amount) as GRNTotalAmount
							 ,count(distinct IRD.ServiceAcknowledgementMasterId) as GRNCount
							from TRN.ServiceAcknowledgementDetail IRD
							group by IRD.ServicePOMasterId)
                            as grn on grn.ServicePOMasterId=PO.Id
							where PO.PurchaseLCId is null and  PO.PlantId='" + identity.PlantId + @"' and PT.PaymentMode='LC'							
							)
a) AS TEMP WHERE " + strkey;

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }




        [HttpPost, Authorize]
        public ActionResult GetLCClosePopUpData(string lcId, bool type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
            //return Json(new { DATA = _accountVoucherReportService.GetPartyPaymentStatusSummaryData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), Error = false }, JsonRequestBehavior.AllowGet);
            return Json(new { DATA = GetPartyPaymentDetailPopUpListData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, lcId, type), Error = false }, JsonRequestBehavior.AllowGet);

        }

        public List<Dictionary<string, object>> GetPartyPaymentDetailPopUpListData(string companyGroupId, string companyId, string plantId, string id, bool type)
        {
            string temp = null;
            if (type == true)
            {
                temp = " and PLC.Id='" + id + @"'";
                //temp = " where PLC.Id='" + id + @"'";

                var sql = @"select 'PDA' [Type],PLC.Id,PLC.LCANo,PLC.IsAccepptanceFirst,v.VoucherNo,V.PostingDate,GL.UserName GL ,b.UserName Budget,A.UserName Activity
                ,SUM(pdad.TotalMaterialTranAmount) DrAmount,0 CrAmount ,pda.AcceptanceNo
                FROM dbo.PurchaseLC PLC 
                LEFT JOIN TRN.PurchaseDocAcceptance pda ON pda.PurchaseLCId=PLC.Id 
                LEFT JOIN TRN.PurchaseDocAcceptanceDetail pdad ON pdad.PurchaseDocAcceptanceId=pda.Id
                LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=pdad.GLGeneralInfoId
                LEFT JOIN MST.BudgetMaster BM ON BM.Id=pdad.BudgetMasterId
                LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                LEFT JOIN HKP.Activity A ON A.Id=pdad.ActivityId
                LEFT JOIN TRN.Voucher V ON V.Id=pda.VoucherId
                WHERE  pda.VoucherId<>'' and PLC.IsAccepptanceFirst=1 
                 "+temp+ @"
                group by PLC.Id,PLC.LCANo,PLC.IsAccepptanceFirst,v.VoucherNo,V.PostingDate,GL.UserName  ,b.UserName ,A.UserName ,pda.AcceptanceNo



                union all
                select 'GRN' [Type],PLC.Id,PLC.LCANo,PLC.IsAccepptanceFirst,v.VoucherNo,V.PostingDate,GL.UserName GL ,b.UserName Budget,A.UserName Activity
                ,0 DrAmount,SUM(IRD.TotalMaterialTranAmount) CrAmount ,null AcceptanceNo
                FROM dbo.PurchaseLC PLC 
                Left Join TRN.PurchaseOrder PO ON PO.PurchaseLCId=PLC.Id
                LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.POId=PO.Id 
                LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
                LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=IRD.PostCRGLGeneralInfoId
                LEFT JOIN MST.BudgetMaster BM ON BM.Id=IRD.PostCRBudgetMasterId
                LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                LEFT JOIN HKP.Activity A ON A.Id=IRD.PostCRActivityId
                LEFT JOIN TRN.Voucher V ON V.Id=IR.VoucherId
                WHERE IR.[Status]='Posting' and ir.VoucherId<>'' 
                and PLC.IsAccepptanceFirst=1 
                 " + temp + @"
                group by PLC.Id,PLC.LCANo,PLC.IsAccepptanceFirst,v.VoucherNo,V.PostingDate,GL.UserName  ,b.UserName ,A.UserName";
                // sql += temp";
                return _sqlRepository.GetDataCollection(sql);
            }

            else
            {
                temp = " and  PLC.Id='" + id + @"'";

                var sql = @"select 'GRN' [Type],PLC.Id,PLC.LCANo,PLC.IsAccepptanceFirst,v.VoucherNo,V.PostingDate,GL.UserName GL ,b.UserName Budget,A.UserName Activity
                ,0 DrAmount,SUM(IRD.TotalMaterialTranAmount) CrAmount ,IR.Id GRNNo ,null AcceptanceNo
                FROM dbo.PurchaseLC PLC 
                Left Join TRN.PurchaseOrder PO ON PO.PurchaseLCId=PLC.Id
                LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.POId=PO.Id 
                LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
                LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=IRD.PostCRGLGeneralInfoId
                LEFT JOIN MST.BudgetMaster BM ON BM.Id=IRD.PostCRBudgetMasterId
                LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                LEFT JOIN HKP.Activity A ON A.Id=IRD.PostCRActivityId
                LEFT JOIN TRN.Voucher V ON V.Id=IR.VoucherId
                WHERE IR.[Status]='Posting' and ir.VoucherId<>'' and PLC.IsAccepptanceFirst=0 
                " + temp+ @"
                group by PLC.Id,PLC.LCANo,PLC.IsAccepptanceFirst,v.VoucherNo,V.PostingDate,GL.UserName  ,b.UserName ,A.UserName ,IR.Id



                union all
                select 'PDA' [Type],PLC.Id,PLC.LCANo,PLC.IsAccepptanceFirst,v.VoucherNo,V.PostingDate,GL.UserName GL ,b.UserName Budget,A.UserName Activity
                ,SUM(pdad.TotalMaterialTranAmount) DrAmount,0 CrAmount ,null GRNNo , pda.AcceptanceNo
                FROM dbo.PurchaseLC PLC 
                LEFT JOIN TRN.PurchaseDocAcceptance pda ON pda.PurchaseLCId=PLC.Id 
                LEFT JOIN TRN.PurchaseDocAcceptanceDetail pdad ON pdad.PurchaseDocAcceptanceId=pda.Id
                LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=pdad.GLGeneralInfoId
                LEFT JOIN MST.BudgetMaster BM ON BM.Id=pdad.BudgetMasterId
                LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                LEFT JOIN HKP.Activity A ON A.Id=pdad.ActivityId
                LEFT JOIN TRN.Voucher V ON V.Id=pda.VoucherId
                WHERE  pda.VoucherId<>'' 
                and PLC.IsAccepptanceFirst=0 
                  " + temp + @"
                group by PLC.Id,PLC.LCANo,PLC.IsAccepptanceFirst,v.VoucherNo,V.PostingDate,GL.UserName  ,b.UserName ,A.UserName, pda.AcceptanceNo ";
                // sql += temp";
                return _sqlRepository.GetDataCollection(sql);
            }



        }




    }
}