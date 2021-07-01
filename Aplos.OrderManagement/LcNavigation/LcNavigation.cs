using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Library.OrderManagement.LcNavigation
{
    public class LcNavigation
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public LcNavigation()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();


        }


        public List<Dictionary<string, object>> GetPurchaseLCSearchByDate(string fromDate, string toDate)
        {
            try
            {
                string sql = PurchaseLCSearchByDateSql(fromDate, toDate);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string PurchaseLCSearchByDateSql(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select
                        PL.LCRef as LCNo,
                        B.UserName as OpeningBank,
                        FORMAT( PL.LCDate,'dd-MMM-yyyy' )as OpeningDate,
                        P.UserName as  Vendor,
                        PL.Amount as Value,
                        Cur.Name as Currency,
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
                         where pl.plantId='" + identity.PlantId + @"' and PL.LCDate between '" + fromDate + @"' and '" + toDate + @"'";

        }

        public List<Dictionary<string, object>> GetPurchaseLCSearch()
        {
            try
            {
                string sql = PurchaseLCSearchSql();
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string PurchaseLCSearchSql()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"
                     select

                        PL.LCRef as LCNo,
                        B.UserName as OpeningBank,
                        FORMAT( PL.LCDate,'dd-MMM-yyyy' )as OpeningDate,
                        P.UserName as  Vendor,
                        PL.Amount as Value,
                        Cur.Name as Currency,
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
                        from PurchaseLC as PL
                        left outer join MST.BankMaster as OBank on PL.OpeningBankMasterId=OBank.Id
                        left outer join HKP.Bank as B on OBank.BankId=b.Id
                        left outer join Contract as Con on PL.ContractId= Con.Id
                        left outer join scs.Currency as Cur on PL.CurrencyId = Cur.Id
                        left outer join MST.Destination as D on PL.CurrencyId=D.Id
                        left outer join HKP.Party as P on PL.VendorId = p.Id
                        left join (select po.PurchaseLCId,sum(pod.TransactionAmount) AS POAmount,count(distinct po.Id) AS POCount from TRN.PurchaseOrder PO 
                        inner JOin trn.PurchaseOrderDetail POD ON POD.InventoryReceiveId=po.Id
                        group by  po.PurchaseLCId) AS PO on PO.PurchaseLCId=pl.Id
                        left join(
                        select  po.PurchaseLCId as LCId,sum(g.TotalMaterialTranAmount) as GRNTotalAmount,count(distinct g.InventoryReceiveId) as GRNCount from TRN.purchaseorder as po 
                        inner join TRN.InventoryReceiveDetail as g on g.POId=po.Id
                        group by po.PurchaseLCId
                        ) as grn on grn.LCId = PL.Id 
                        left join (
                        select sum(AD.TotalMaterialTranAmount) as AcceptanceValue,A.PurchaseLCId  from TRN.PurchaseDocAcceptanceDetail as AD
                         inner join trn.PurchaseDocAcceptance as A on A.Id=AD.PurchaseDocAcceptanceId
                        group by A.PurchaseLCId
                        ) as ac on ac.PurchaseLCId = PL.Id
						left join(select PDA.PurchaseLCId,sum(LAA.Amount) Amount from TRN.LoanAgainstAcceptance LAA 
											left outer join TRN.PurchaseDocAcceptance PDA on PDA.Id=LAA.PurchaseDocAcceptanceId
											group by PDA.PurchaseLCId												
						) Loan on Loan.PurchaseLCId=PL.Id
                        left outer join (
                         select con.Id as Id, customer.UserName as Customer from Contract as con 
                        inner join HKP.Party as customer on con.CustomerId=customer.Id)
                        as cus on cus.Id=PL.ContractId
                         left join (select Ac.PurchaseLCId,sum(i.WrittenOffAmount) AS PaymentMade from TRN.PurchaseDocAcceptance AC
                        inner join  trn.invoice I on i.PurchaseDocAcceptanceId=ac.Id
                         group by Ac.PurchaseLCId) as PM on PM.PurchaseLCId=PL.Id
                         where pl.plantId='" + identity.PlantId + @"'";

        }

        public void PurchaseLCReport(string fromDate, string toDate)
        {
            try
            {


                string sql = PurchaseLCSearchByDateSql(fromDate, toDate);
                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

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

                string strFileName = "Purchase LC.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<Dictionary<string, object>> GetPurchaseLCPOList(string PurchaseLCId)
        {

            try
            {
                string sql = PurchaseLCPOSql(PurchaseLCId);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private string PurchaseLCPOSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select

                            po.Id as PONo,
                            PL.Id as PurchaseLCID,sum(POD.TransactionAmount) as TotalValue,
                            c.Name as Currency,Ac.AcceptanceValue,
                            FORMAT( po.PODate,'dd-MMM-yyyy' ) as PODate  
                            ,po.DocRefNo as VendorRefNo, grn.GRNValue as GRNValue


                            from PurchaseLC as PL

                            join trn.PurchaseOrder as PO 
                            on po.PurchaseLCId = pl.Id
                            inner join SCS.Currency as C on 
                            PL.CurrencyId=c.Id

                            inner join trn.PurchaseOrderDetail as POD 
                            on POD.InventoryReceiveId=po.Id

                            left join
                             (
                            select ir.POId,sum(IRD.TotalMaterialTranAmount) as GRNValue  from trn.InventoryReceive as IR 
                            inner join TRN.InventoryReceiveDetail as IRD on IRD.InventoryReceiveId = IR.Id
                            group by IR.POId
                            )
                            as grn on grn.POId=po.Id

                             left join (
                            select pa.POId,sum(pd.TotalMaterialTranAmount) as AcceptanceValue from TRN.PurchaseDocAcceptance as PA 
                            inner join TRN.PurchaseDocAcceptanceDetail as PD on PD.PurchaseDocAcceptanceId=PA.Id
                            group by pa.POId
                            ) as AC on ac.POId=PO.Id
                             where po.purchaseLcId='" + PurchaseLCId + @"'
                            group by po.Id,pl.Id,c.Name,po.PODate,po.DocRefNo,grn.GRNValue,AC.AcceptanceValue";
        }

        public List<Dictionary<string, object>> GetPurchaseLCGRNList(string PurchaseLCId)
        {

            try
            {
                string sql = PurchaseLCGRNSql(PurchaseLCId);
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        private string PurchaseLCGRNSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select 
						
						PL.Id as PurchaseLCId,
						sum(IRD.TotalMaterialTranAmount) as GRNValue,IRD.InventoryReceiveId as GRNNo,
					FORMAT( IR.GRNDate,'dd-MMM-yyyy') as GRNDate,po.DocRefNo as VendorRefNo,
                    c.Name as Currency
					,gate.UserName as GateName, ir.GateEntryNo
					from PurchaseLC as PL 
					 join TRN.PurchaseOrder as PO 
						on po.PurchaseLCId=pl.Id
						  left join TRN.InventoryReceiveDetail as IRD on IRD.POId=po.Id
                     join TRN.InventoryReceive as IR on IR.Id=IRD.InventoryReceiveId
					   left outer join trn.GateEntry as G on IR.GateEntryNo = g.id
                    left outer join dbo.PlantWiseGate as  gate on gate.Id=g.PlantWiseGateId
					 left join SCS.Currency as c on c.id=PL.CurrencyId
                    where PurchaseLCId='" + PurchaseLCId + @"'
					  group by PL.Id,IRD.InventoryReceiveId ,IR.GRNDate,po.DocRefNo,c.Name,gate.UserName,ir.GateEntryNo";

        }
        public List<Dictionary<string, object>> GetPurchaseLCACList(string PurchaseLCId)
        {
            try
            {
                string sql = PurchaseLCACSql(PurchaseLCId);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string PurchaseLCACSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select
PL.Id as PurchaseLCId,PA.AcceptanceNo, FORMAT( PA.AcceptanceDate,'dd-MMM-yyyy' ) as AcceptanceDate
,Format(A.PODate,'dd-MMM-yyyy') PODate,A.POAmount,A.PurchaseOrderId PONo,A.GRNAmount,A.InventoryReceiveId GRNNo,FORMAT(A.GRNDate,'dd-MMM-yyyy')GRNDate
                        ,c.Name as Currency,A.AcceptanceValue
						from PurchaseLC as PL
						join SCS.Currency as c on PL.CurrencyId=c.Id
						left outer join trn.PurchaseDocAcceptance as PA on PA.PurchaseLCId=PL.Id						
						left join (
						select 
						PA.PurchaseDocAcceptanceId,PA.POId PurchaseOrderId,PO.PODate,PO.DocRefNo
						,sum(pd.TransactionAmount) AS POAmount,sum(GRN.GRNAmount)AS GRNAmount,GRN.InventoryReceiveId,GRN.GRNDate,sum(PA.MaterialTranAmount) AcceptanceValue
						from trn.PurchaseDocAcceptanceDetail PA
						join trn.PurchaseOrderDetail PD on PD.Id=PA.PODetailId
						join trn.PurchaseOrder PO ON PO.Id=PA.POId
						left join (
						select RD.PODetailsId,sum(rd.GRNTotalAmount) AS GRNAmount,RD.InventoryReceiveId,IR.GRNDate from trn.InventoryReceive IR 
						join trn.InventoryReceiveDetail RD on rd.InventoryReceiveId=Ir.Id
						group by RD.PODetailsId,RD.InventoryReceiveId,IR.GRNDate
						) AS GRN ON GRN.PODetailsId=PD.Id
						group by PA.PurchaseDocAcceptanceId,PA.POId ,PO.PODate,PO.DocRefNo,GRN.InventoryReceiveId,GRN.GRNDate
						)						
						A on A.PurchaseDocAcceptanceId=PA.Id						
                        where PL.Id ='"+PurchaseLCId+@"'
						group by PL.Id,PA.AcceptanceNo,PA.AcceptanceDate,c.Name,A.PODate,A.POAmount,A.PurchaseOrderId,A.GRNAmount,A.InventoryReceiveId,A.GRNDate,A.AcceptanceValue";

        }

        public List<Dictionary<string, object>> GetPurchaseLCLoanList(string PurchaseLCId)
        {
            try
            {
                string sql = PurchaseLCLoanSql(PurchaseLCId);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string PurchaseLCLoanSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select laa.Id as LoanId,laa.PurchaseDocAcceptanceId,pda.AcceptanceNo,Format(pda.AcceptanceDate,'dd-MMM-yyyy') AcceptanceDate
						,Format(LoanDate,'dd-MMM-yyyy') LoanDate,laa.LoanNo,laa.Amount,v.VoucherNo
						from PurchaseLC Pl 
						left outer join trn.PurchaseDocAcceptance pda on pda.PurchaseLCId=pl.Id
						left outer join trn.LoanAgainstAcceptance laa on laa.PurchaseDocAcceptanceId=pda.Id
					    LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAA.BankMasterId
						LEFT JOIN trn.Voucher v on v.Id=laa.VoucherId
						where pl.Id='" + PurchaseLCId+@"'";
        }

    }
}
