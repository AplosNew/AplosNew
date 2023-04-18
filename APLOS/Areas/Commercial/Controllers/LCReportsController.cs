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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Commercial.Controllers
{
    public class LCReportsController : BaseController
    {
        string TableName = "hkp.LCChargesType";
        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public LCReportsController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
        private string CellAddr(int Col, int Row)
        {
            return clsStaticInfo.GetxlsCol(Col) + Row.ToString();
        }


        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult MasterLCReport(string MasterLCList)
        {
            try
            {
                if (string.IsNullOrEmpty(MasterLCList))
                    throw new Exception("Please select at least one master LC");

                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = GetMasterLCReport(excelEngine, MasterLCList);

                string strFileName = "Master LC.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }
        [HttpPost, Authorize]
        public ActionResult GetMasterLCList(string FromDate, string ToDate, string lcType)
        {

            try
            {
                //if (bplib.clsWebLib.IsDateOK(FromDate) == false)
                //    throw new Exception("Plase select from date");


                //if (bplib.clsWebLib.IsDateOK(ToDate) == false)
                //    throw new Exception("Plase select to date");

                FromDate = Convert.ToDateTime(FromDate).ToString("dd-MMM-yyyy");
                ToDate = Convert.ToDateTime(ToDate).ToString("dd-MMM-yyyy");

                if (Convert.ToDateTime(FromDate) > Convert.ToDateTime(ToDate))
                    throw new Exception("To date cannot be earlier than from date");

                //string sql = @"select convert(bit,1) AS isSelected, MLC.Id , mlc.LCRef as MasterLCRefNo,Format( mlc.LCDate,'dd-MMM-yyyy') as MasterLCDate
                //            ,Format( mlc.ExpiryDate, 'dd-MMM-yyyy')as ExpiryDate, mlc.Amount as MasterLCAmount,
                //            mlc.Type, mlc.Tenure, mlc.OpeningDescription,mlc.LeinDescription
                //            , MasCur.Code as MasterCurrency, MLC.OpeningBank
                //            , MLC.LeinBank , bb.UserName as BenificiaryBank, fd.UserName as FinalDestination, Cus.UserName as Customer
                //            from MasterLC MLC
                //            left outer join scs.Currency MasCur on MasCur.Id= mlc.CurrencyId
                //            left outer join mst.BankMaster bm on bm.id=mlc.BenificiaryBankId
                //            left outer join hkp.Bank bb on bb.id= bm.BankId
                //            left outer join mst.Destination fd on fd.id= mlc.FinalDestinationId
                //            left outer join hkp.Party as Cus on Cus.Id=mlc.CustomerId
                //      where MLC.LCDate between '" + FromDate + @"' and '" + ToDate + "'";

                string sql = @"select convert(bit,0) AS isSelected,
				 PLC.Id PurchaseLCId 

				,ISNULL(mlc.LCRef,'') as MasterLCRefNo
                    ,PurchaseLCRef= STUFF((select distinct ','+XVD.LCRef 
                    from dbo.PurchaseLC XVD 
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

					-- ,Format( PLC.LCDate,'dd-MMM-yyyy') as PurchaseLCOpeningDate
				   ,LCOpeningDate= STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), XVD.LCDate, 106),' ','-') 
				   from dbo.PurchaseLC XVD 
				   where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

				 ,Format( PLC.ExpiryDate, 'dd-MMM-yyyy')as ExpiryDate
				 
                  --  , PurchaseCur.Code as PurchaseCurrency
		            ,PurCurrencyCode= STUFF((select distinct ','+XC.Code
                    from dbo.PurchaseLC XVD 
                    LEFT JOIN SCS.Currency XC ON XC.Id=XVD.CurrencyId
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

 							,ISNULL( PLC.Amount,0) as PurchaseLCAmount
							,ISNULL( plc.Rate,0)Rate
                           , PLC.Type, PLC.Tenure

							--,isnull( bm.AccountTitle ,'')OpeningBank
							,OpeningBank=isnull( STUFF((select distinct ','+xbm.AccountTitle 
						    from dbo.PurchaseLC XVD 
							left join MST.BankMaster xbm on xbm.Id=XVD.OpeningBankMasterId
							where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

							--, PLC.BenificiaryBank
							,BenificiaryBank=isnull( STUFF((select distinct ','+XVD.BenificiaryBank 
							from dbo.PurchaseLC XVD 
							where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

							,PLC.BenificiaryBankDescription
                           ,ISNULL(PLC.LeinBank,0)LeinBank
							, ISNULL(plc.LeinBankDescription,0)LeinBankDescription
						
					
							,plc.VendorId
							,isnull( P.UserName,'') as Vendor
							,plc.PortOfLoading
							,plc.FinalDestination
							,plc.Status
							,PLC.LCANo
							,PLC.PaymentBasedOn
							,format( plc.ShipmentDate, 'dd-MMMM-yyyyy')ShipmentDate

							--,plc.PINo
						   ,PINo= STUFF((select distinct ','+XVD.PINo 
						   from dbo.PurchaseLC XVD 
						   where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


							,plc.ContractId
						--	c.ContractNo,c.ContractDate,p.UserName Customer

                    ,ContractNo= isnull( STUFF((select distinct ','+XC.ContractNo 
					from dbo.[Contract] XC 
					left join PurchaseLC xPlc ON XC.Id=xPlc.ContractId
                    where xPlc.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                    ,Customer= isnull( STUFF((select distinct ','+XCU.UserName 
					from dbo.PurchaseLC XVD 
                    LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
                    join HKP.Party XCU ON XCU.Id=XC.CustomerId
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                    ,MasterLCNo= isnull( STUFF((select distinct ','+XC.MasterLCId
					from dbo.PurchaseLC XVD 
                    LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                    ,UDNo= isnull( STUFF((select distinct ','+XC.UDNo 
                    from dbo.PurchaseLC XVD 
                    LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

				    ,isnull( plc.OrderSpecific,'')OrderSpecific

					,PONo= isnull( STUFF((select distinct ','+xpomap.POId 
                    from  dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
                    LEFT JOIN trn.PurchaseDocAcceptancePOMap xpomap on xpomap.PurchaseDocAcceptanceId=xp.Id
                    where XVD.Id=PLC.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                    ,GRNNo= isnull( STUFF((select distinct ','+xgrnmap.GRNId from
                    dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
                    LEFT JOIN trn.GRNAcceptanceMap xgrnmap on xgrnmap.PurchaseDocumentAcceptanceId=xp.Id
                    where XVD.Id=XP.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                            from PurchaseLC PLC
                            left outer join scs.Currency PurchaseCur on PurchaseCur.Id= PLC.CurrencyId
                            left outer join mst.BankMaster bm on bm.id=PLC.OpeningBankMasterId
                            left outer join hkp.Bank b on b.id= bm.BankId
                           -- left outer join mst.Destination fd on fd.id= PLC.FinalDestinationId
                            left outer join hkp.Party as P on P.Id=PLC.VendorId
                            left join dbo.Contract c on c.Id=PLC.ContractId
							left join dbo.MasterLC MLC on MLC.Id=c.MasterLCId

		                    where PLC.LCDate between '" + FromDate + "' and '" + ToDate + @"' 
							and PLC.ContractId  <>''";



                var data = _sqlRepository.GetDataCollection(sql);

                return Json(new { DATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }


            //return View();
        }

        private IWorkbook GetMasterLCReport(ExcelEngine excelEngine, string MasterLCList) // GetMasterOrderReport
        {
            excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            try
            {
                worksheet.Name = "LCReport";

                int COL = 1; int ROW = 6;

                int startCol = COL;
                worksheet[ROW, COL].Text = "SL. No";
                int colSLNO = COL;
                worksheet[ROW, COL].ColumnWidth = 7;
                COL++;

                worksheet[ROW, COL].Text = "Customer";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colPartyId = COL;
                worksheet[ROW, COL].ColumnWidth = 25;
                COL++;

                //worksheet[ROW, COL].Text = "Sales LC No";
                //int colMasterLCId = COL;
                //worksheet[ROW, COL].ColumnWidth = 15;
                //COL++;

                worksheet[ROW, COL].Text = "Master LC Id";
                int colMasterLCId = COL;
                worksheet[ROW, COL].ColumnWidth = 12;
                COL++;

                worksheet[ROW, COL].Text = "Master LC No.";
                int colMasterLCRefNo = COL;
                worksheet[ROW, COL].ColumnWidth = 13;
                COL++;


                worksheet[ROW, COL].Text = "LC Value";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colMasterLCAmount = COL;
                worksheet[ROW, COL].ColumnWidth = 14;
                COL++;

                worksheet[ROW, COL].Text = "Currency";
                int colCurrencyCode = COL;
                worksheet[ROW, COL].ColumnWidth = 9;
                COL++;

                worksheet[ROW, COL].Text = "Contract Id";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colContractId = COL;
                worksheet[ROW, COL].ColumnWidth = 11;
                COL++;

                worksheet[ROW, COL].Text = "Contract No";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colContractNo = COL;
                worksheet[ROW, COL].ColumnWidth = 11;
                COL++;

                worksheet[ROW, COL].Text = "Buyer";
                int colMasterLCCustomerId = COL;
                worksheet[ROW, COL].ColumnWidth = 32;
                COL++;


                worksheet[ROW, COL].Text = "Contract SO Qty";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSalesOrderQty = COL;
                worksheet[ROW, COL].ColumnWidth = 16;
                COL++;

                worksheet[ROW, COL].Text = "Contract SO Value";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSalesOrderValue = COL;
                worksheet[ROW, COL].ColumnWidth = 17;
                COL++;

                worksheet[ROW, COL].Text = "Currency";
                int colMasterOrderCurrencyId = COL;
                worksheet[ROW, COL].ColumnWidth = 9;
                COL++;


                worksheet[ROW, COL].Text = "Commission";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colContractFundCommission = COL;
                worksheet[ROW, COL].ColumnWidth = 17;
                COL++;

                worksheet[ROW, COL].Text = "Fund Utilization";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colContractFundUtilization = COL;
                worksheet[ROW, COL].ColumnWidth = 17;
                COL++;


                worksheet[ROW, COL].Text = "Purchase Margin";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colContractFundPercentage = COL;
                worksheet[ROW, COL].ColumnWidth = 17;
                COL++;


                worksheet[ROW, COL].Text = "Purchase LC No";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colPurchaseLCNo = COL;
                worksheet[ROW, COL].ColumnWidth = 16;
                COL++;

                worksheet[ROW, COL].Text = "Vendor";
                int colPartyUserName = COL;
                worksheet[ROW, COL].ColumnWidth = 30;
                COL++;

                worksheet[ROW, COL].Text = "Opening Date";
                int colPurchaseLCLCDate = COL;
                worksheet[ROW, COL].ColumnWidth = 14;
                COL++;

                worksheet[ROW, COL].Text = "Opening Value";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPurchaseLCAmount = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Currency";
                int colPurchaseLCCurrencyId = COL;
                worksheet[ROW, COL].ColumnWidth = 10;
                COL++;

                worksheet[ROW, COL].Text = "Percentage";
                int colPercentage = COL;
                worksheet[ROW, COL].ColumnWidth = 21;
                COL++;

                worksheet[ROW, COL].Text = "Present LC Value";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPresentLCValue = COL;
                worksheet[ROW, COL].ColumnWidth = 17;
                COL++;


                worksheet[ROW, COL].Text = "LastAmendment Date";
                int colLastAmendmentDate = COL;
                worksheet[ROW, COL].ColumnWidth = 20;
                COL++;

                worksheet[ROW, COL].Text = "LC Utilization";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPurchaseOrderDetailTrnQtyRate = COL;
                worksheet[ROW, COL].ColumnWidth = 16;
                COL++;

                worksheet[ROW, COL].Text = "LC Accepted Value";
                int colLCAcceptedValue = COL;
                worksheet[ROW, COL].ColumnWidth = 17;
                
                int endCol = COL;

                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Size = 12;
                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Bold = true;

                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Yellow;
                worksheet.Range[ROW, startCol, ROW, COL].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, startCol, ROW, COL].BorderInside(ExcelLineStyle.Hair);
                // worksheet.Range[ROW,  ROW].BorderInside(ExcelLineStyle.Hair);


                var dsData = GetLcData(MasterLCList);




                if (dsData.Rows.Count == 0)
                {
                    throw new Exception("No Data Found");
                }


                //con.getDataSet(@"Select * from EmployeeInformation", out DataSet dsData);
                //left join EmpDateWiseShiftAssign on ei.EmployeeCode=EmpDateWiseShiftAssign.GroupID
                ROW++;
                int StartDataRow = ROW;//7
                // worksheet = workbook.Worksheets[8];
                string group1 = ""; string group2 = ""; string group3 = "";
                int startRowGroup1 = ROW; int startRowGroup2 = ROW; int StartRowGroup3 = ROW;
                int SerialNumber = 0;
                var catFRow = ROW;
                ArrayList al = new ArrayList();
                var lastEmpCat = string.Empty;
                ReportUtility ru = new ReportUtility();
                for (int i = 0; i < dsData.Rows.Count; i++)
                {
                    var catLRow = ROW;
                    if (group1 != dsData.Rows[i]["MasterLCId"].ToString())
                    {
                        if (i > 0)
                        {
                            #region Subtotal
                            if (catFRow < ROW)
                            {
                                lastEmpCat = group1;
                                al.Add(ROW);
                                SetHeadText(worksheet, ROW, 1, " Subtotal:");
                                worksheet.Range[ROW, 1, ROW, (colMasterLCAmount - 1)].Merge();

                                worksheet.Range[ROW, colMasterLCAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colMasterLCAmount) + catFRow + ":" + ru.GetColumnNameForXls(colMasterLCAmount) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colSalesOrderQty].Formula = "=SUM(" + ru.GetColumnNameForXls(colSalesOrderQty) + catFRow + ":" + ru.GetColumnNameForXls(colSalesOrderQty) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colSalesOrderValue].Formula = "=SUM(" + ru.GetColumnNameForXls(colSalesOrderValue) + catFRow + ":" + ru.GetColumnNameForXls(colSalesOrderValue) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colContractFundCommission].Formula = "=SUM(" + ru.GetColumnNameForXls(colContractFundCommission) + catFRow + ":" + ru.GetColumnNameForXls(colContractFundCommission) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colContractFundUtilization].Formula = "=SUM(" + ru.GetColumnNameForXls(colContractFundUtilization) + catFRow + ":" + ru.GetColumnNameForXls(colContractFundUtilization) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colPurchaseLCAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colPurchaseLCAmount) + catFRow + ":" + ru.GetColumnNameForXls(colPurchaseLCAmount) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colPercentage].Formula = "=SUM(" + ru.GetColumnNameForXls(colPercentage) + catFRow + ":" + ru.GetColumnNameForXls(colPercentage) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colPresentLCValue].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresentLCValue) + catFRow + ":" + ru.GetColumnNameForXls(colPresentLCValue) + (ROW - 1) + ")";

                                worksheet.Range[ROW, colMasterLCAmount, ROW, colPresentLCValue].CellStyle.Font.Bold = true;

                                ROW++;
                            }
                            #endregion


                            if (ROW > startRowGroup1 + 1)
                            {
                                //worksheet[startRowGroup1, colSLNO, ROW - 1, colSLNO].Merge();
                                //worksheet[startRowGroup1, colMasterLCId, ROW - 1, colMasterLCId].Merge();
                                //worksheet[startRowGroup1, colMasterLCRefNo, ROW - 1, colMasterLCRefNo].Merge();
                                //worksheet[startRowGroup1, colMasterLCAmount, ROW - 1, colMasterLCAmount].Merge();
                                //// worksheet[startRowGroup1, colMasterLCCustomerId, ROW - 1, colMasterLCCustomerId].Merge();
                                //worksheet[startRowGroup1, colCurrencyCode, ROW - 1, colCurrencyCode].Merge();
                                //worksheet[startRowGroup1, colPartyId, ROW - 1, colPartyId].Merge();

                            }
                            
                        }


                        SerialNumber++;
                        startRowGroup1 = ROW;
                        group1 = dsData.Rows[i]["MasterLCId"].ToString();

                        

                        worksheet[ROW, colSLNO].Text = (SerialNumber).ToString();
                        worksheet[ROW, colMasterLCId].Text = dsData.Rows[i]["MasterLCId"].ToString();

                        worksheet[ROW, colMasterLCRefNo].Text = dsData.Rows[i]["MasterLCRefNo"].ToString();
                        worksheet[ROW, colMasterLCAmount].Number = clsStaticInfo.dbl(dsData.Rows[i]["MasterLCValue"].ToString());

                        //  worksheet[ROW, colMasterLCCustomerId].Text = dsData.Tables[0].Rows[i]["Buyer"].ToString();
                        worksheet[ROW, colCurrencyCode].Text = dsData.Rows[i]["MasterLCcurrency"].ToString();
                        worksheet[ROW, colPartyId].Text = dsData.Rows[i]["Customer"].ToString();
                        if (catFRow < ROW)
                        {
                            catFRow = ROW;
                        }
                    }

                    if (group2 != group1 + dsData.Rows[i]["ContractId"].ToString()) //ContractNo, ContractId 
                    {
                        if (i > 0)
                        {

                            //if (ROW > startRowGroup2 + 1)
                            //{
                            //    worksheet[startRowGroup2, colContractFundPercentage, ROW - 1, colContractFundPercentage].Merge();
                            //    worksheet[startRowGroup2, colContractId, ROW - 1, colContractId].Merge();
                            //    worksheet[startRowGroup2, colContractNo, ROW - 1, colContractNo].Merge();

                            //    worksheet[startRowGroup2, colMasterLCCustomerId, ROW - 1, colMasterLCCustomerId].Merge(); // new
                            //    worksheet[startRowGroup2, colMasterOrderCurrencyId, ROW - 1, colMasterOrderCurrencyId].Merge();
                            //    worksheet[startRowGroup2, colSalesOrderQty, ROW - 1, colSalesOrderQty].Merge();
                            //    worksheet[startRowGroup2, colSalesOrderValue, ROW - 1, colSalesOrderValue].Merge();
                            //    worksheet[startRowGroup2, colContractFundCommission, ROW - 1, colContractFundCommission].Merge();
                            //    worksheet[startRowGroup2, colContractFundUtilization, ROW - 1, colContractFundUtilization].Merge();

                            //}

                        }
                        startRowGroup2 = ROW;
                        group2 = group1 + dsData.Rows[i]["ContractId"].ToString(); //ContractNo, ContractId

                        worksheet[ROW, colContractFundCommission].Formula = clsStaticInfo.GetxlsCol(colSalesOrderValue) + ROW.ToString() + "*" + (clsStaticInfo.dbl(dsData.Rows[i]["CommissionPercentage"].ToString())).ToString() + "%";
                        worksheet[ROW, colContractFundUtilization].Formula = clsStaticInfo.GetxlsCol(colSalesOrderValue) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colContractFundCommission) + ROW.ToString();

                        worksheet[ROW, colContractFundPercentage].Formula = clsStaticInfo.GetxlsCol(colContractFundUtilization) + ROW.ToString() + "*" + clsStaticInfo.dbl(dsData.Rows[i]["PurchaseMargin"].ToString()) + "%";

                        worksheet[ROW, colContractId].Text = dsData.Rows[i]["ContractId"].ToString(); //ContractNo, ContractId
                        worksheet[ROW, colContractNo].Text = dsData.Rows[i]["ContractNo"].ToString(); //ContractNo, ContractId
                        worksheet[ROW, colMasterLCCustomerId].Text = dsData.Rows[i]["Buyer"].ToString(); // New
                        worksheet[ROW, colMasterOrderCurrencyId].Text = dsData.Rows[i]["MasterOrderCurrency"].ToString();
                        worksheet[ROW, colSalesOrderQty].Number = clsStaticInfo.dbl(dsData.Rows[i]["ContractOrderQty"].ToString());
                        worksheet[ROW, colSalesOrderQty].NumberFormat = clsStaticInfo.NumberFormat();
                        worksheet[ROW, colSalesOrderValue].Number = clsStaticInfo.dbl(dsData.Rows[i]["ContractOrderValue"].ToString());

                    }

                    if (group3 != group2 + dsData.Rows[i]["PurchaseLCRefNo"].ToString()) //PurchaseLCRefNo
                    {
                        StartRowGroup3 = ROW;
                        group3 = group2 + dsData.Rows[i]["PurchaseLCRefNo"].ToString();

                        worksheet[ROW, colMasterLCAmount].Number = clsStaticInfo.dbl(dsData.Rows[i]["MasterLCValue"].ToString());
                        worksheet[ROW, colSalesOrderQty].Number = clsStaticInfo.dbl(dsData.Rows[i]["ContractOrderQty"].ToString());
                        worksheet[ROW, colSalesOrderValue].Number = clsStaticInfo.dbl(dsData.Rows[i]["ContractOrderValue"].ToString());
                        worksheet[ROW, colContractFundCommission].Formula = clsStaticInfo.GetxlsCol(colSalesOrderValue) + ROW.ToString() + "*" + (clsStaticInfo.dbl(dsData.Rows[i]["CommissionPercentage"].ToString())).ToString() + "%";
                        worksheet[ROW, colContractFundUtilization].Formula = clsStaticInfo.GetxlsCol(colSalesOrderValue) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colContractFundCommission) + ROW.ToString();

                        worksheet[ROW, colPurchaseLCNo].Text = dsData.Rows[i]["PurchaseLCRefNo"].ToString();

                        worksheet[ROW, colPurchaseLCCurrencyId].Text = dsData.Rows[i]["PurchasePLCurrency"].ToString();

                        worksheet[ROW, colPurchaseOrderDetailTrnQtyRate].Number = clsStaticInfo.dbl(dsData.Rows[i]["POValue"].ToString());
                        worksheet[ROW, colPurchaseLCAmount].Number = clsStaticInfo.dbl(dsData.Rows[i]["PurchaseLcOpeningValue"].ToString()); // PurchaseLcOpeningValue
                        worksheet[ROW, colPartyUserName].Text = dsData.Rows[i]["vendor"].ToString();
                        worksheet[ROW, colLastAmendmentDate].Text = dsData.Rows[i]["LastAmendmentDate"].ToString();

                        //var percentage = clsStaticInfo.dbl(dsData.Rows[i]["PurchaseLcOpeningValue"] + "/" + clsStaticInfo.dbl(dsData.Rows[i]["MasterLCValue"])) + "%";
                        //worksheet[ROW, colPercentage].Text = percentage;

                        worksheet[ROW, colPercentage].Formula = clsStaticInfo.GetxlsCol(colPurchaseLCAmount) + ROW.ToString() + "/" + clsStaticInfo.GetxlsCol(colMasterLCAmount) + ROW.ToString() + "%";

                        worksheet[ROW, colPresentLCValue].Number = clsStaticInfo.dbl(dsData.Rows[i]["PresentLCValue"].ToString());
                        worksheet[ROW, colPurchaseLCLCDate].Text = dsData.Rows[i]["PurchaseLCOpeningDate"].ToString();


                       
                    }
                    //worksheet[StartDataRow, colPurchaseLCAmount, ROW - 1, colPurchaseLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";


                    ROW++;
                }


            
                //if (ROW > startRowGroup1 + 1)
                //{
                //    worksheet[startRowGroup1, colSLNO, ROW - 1, colSLNO].Merge();
                //    worksheet[startRowGroup1, colMasterLCId, ROW - 1, colMasterLCId].Merge();
                //    worksheet[startRowGroup1, colMasterLCRefNo, ROW - 1, colMasterLCRefNo].Merge();
                //    worksheet[startRowGroup1, colMasterLCAmount, ROW - 1, colMasterLCAmount].Merge();
                //    // worksheet[startRowGroup1, colMasterLCCustomerId, ROW - 1, colMasterLCCustomerId].Merge();
                //    worksheet[startRowGroup1, colCurrencyCode, ROW - 1, colCurrencyCode].Merge();
                //    worksheet[startRowGroup1, colPartyId, ROW - 1, colPartyId].Merge();


                //}
                //worksheet[StartDataRow, colMasterLCAmount, ROW - 1, colMasterLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";


                //if (ROW > startRowGroup2 + 1)
                //{
                //    worksheet[startRowGroup2, colContractFundPercentage, ROW - 1, colContractFundPercentage].Merge();
                //    worksheet[startRowGroup2, colContractId, ROW - 1, colContractId].Merge();
                //    worksheet[startRowGroup2, colContractNo, ROW - 1, colContractNo].Merge();
                //    worksheet[startRowGroup2, colMasterLCCustomerId, ROW - 1, colMasterLCCustomerId].Merge(); //new buyer
                //    worksheet[startRowGroup2, colMasterOrderCurrencyId, ROW - 1, colMasterOrderCurrencyId].Merge();
                //    worksheet[startRowGroup2, colSalesOrderQty, ROW - 1, colSalesOrderQty].Merge();
                //    worksheet[startRowGroup2, colSalesOrderValue, ROW - 1, colSalesOrderValue].Merge();
                //    worksheet[startRowGroup2, colContractFundCommission, ROW - 1, colContractFundCommission].Merge();
                //    worksheet[startRowGroup2, colContractFundUtilization, ROW - 1, colContractFundUtilization].Merge();

                //}

                #region Last subtotal
                al.Add(ROW);
                SetHeadText(worksheet, ROW, 1, " Subtotal:");
                worksheet.Range[ROW, 1, ROW, (colMasterLCAmount - 1)].Merge();

                worksheet.Range[ROW, colMasterLCAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colMasterLCAmount) + catFRow + ":" + ru.GetColumnNameForXls(colMasterLCAmount) + (ROW - 1) + ")";
                worksheet.Range[ROW, colSalesOrderQty].Formula = "=SUM(" + ru.GetColumnNameForXls(colSalesOrderQty) + catFRow + ":" + ru.GetColumnNameForXls(colSalesOrderQty) + (ROW - 1) + ")";
                worksheet.Range[ROW, colSalesOrderValue].Formula = "=SUM(" + ru.GetColumnNameForXls(colSalesOrderValue) + catFRow + ":" + ru.GetColumnNameForXls(colSalesOrderValue) + (ROW - 1) + ")";

                worksheet.Range[ROW, colContractFundCommission].Formula = "=SUM(" + ru.GetColumnNameForXls(colContractFundCommission) + catFRow + ":" + ru.GetColumnNameForXls(colContractFundCommission) + (ROW - 1) + ")";

                worksheet.Range[ROW, colContractFundUtilization].Formula = "=SUM(" + ru.GetColumnNameForXls(colContractFundUtilization) + catFRow + ":" + ru.GetColumnNameForXls(colContractFundUtilization) + (ROW - 1) + ")";
                worksheet.Range[ROW, colPurchaseLCAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colPurchaseLCAmount) + catFRow + ":" + ru.GetColumnNameForXls(colPurchaseLCAmount) + (ROW - 1) + ")";
                worksheet.Range[ROW, colPercentage].Formula = "=SUM(" + ru.GetColumnNameForXls(colPercentage) + catFRow + ":" + ru.GetColumnNameForXls(colPercentage) + (ROW - 1) + ")";
                worksheet.Range[ROW, colPresentLCValue].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresentLCValue) + catFRow + ":" + ru.GetColumnNameForXls(colPresentLCValue) + (ROW - 1) + ")";

                worksheet.Range[ROW, colMasterLCAmount, ROW, colPresentLCValue].CellStyle.Font.Bold = true;
                ROW++;
                #endregion

                #region Grand Total
                SetHeadText(worksheet, ROW, 1, "Grand Total:");
                worksheet.Range[ROW, 1, ROW, (colMasterLCAmount - 1)].Merge();


                worksheet.Range[ROW, colMasterLCAmount].Formula = GetFormulaGrandTotal(al, colMasterLCAmount);
                worksheet.Range[ROW, colSalesOrderQty].Formula = GetFormulaGrandTotal(al, colSalesOrderQty);
                worksheet.Range[ROW, colSalesOrderValue].Formula = GetFormulaGrandTotal(al, colSalesOrderValue);
                worksheet.Range[ROW, colContractFundCommission].Formula = GetFormulaGrandTotal(al, colContractFundCommission);
                worksheet.Range[ROW, colContractFundUtilization].Formula = GetFormulaGrandTotal(al, colContractFundUtilization);
                worksheet.Range[ROW, colPurchaseLCAmount].Formula = GetFormulaGrandTotal(al, colPurchaseLCAmount);
                worksheet.Range[ROW, colPercentage].Formula = GetFormulaGrandTotal(al, colPercentage);
                worksheet.Range[ROW, colPresentLCValue].Formula = GetFormulaGrandTotal(al, colPresentLCValue);

                worksheet.Range[ROW, colMasterLCAmount, ROW, colPresentLCValue].CellStyle.Font.Bold = true;
                #endregion

                worksheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);

                worksheet[StartDataRow, colMasterLCAmount, ROW - 1, colMasterLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[StartDataRow, colSalesOrderQty, ROW - 1, colSalesOrderQty].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[StartDataRow, colSalesOrderValue, ROW - 1, colSalesOrderValue].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[StartDataRow, colContractFundCommission, ROW - 1, colContractFundCommission].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[StartDataRow, colContractFundUtilization, ROW - 1, colContractFundUtilization].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[StartDataRow, colPurchaseLCAmount, ROW - 1, colPurchaseLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[StartDataRow, colPercentage, ROW - 1, colPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[StartDataRow, colPresentLCValue, ROW - 1, colPresentLCValue].NumberFormat = "#,##0.00;(#,##0.00)";

                //  worksheet[ROW, colQty].Formula = "SUM("+ clsStaticInfo.GetxlsCol(colQty) + StartDataRow + ":"+ clsStaticInfo.GetxlsCol(colQty) + (ROW-1).ToString() + ")";

                // worksheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                //worksheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeader(ref worksheet, endCol, "Master LC", identity.CompanyId, identity.PlantName, "");
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;


                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                return workbook;

            }
            catch (Exception ex)
            {
                throw (ex);

            }




        }

        public DataTable GetLcData(string MasterLCList)
        {
            try
            {

                string sql = @"select cus.UserName as Customer,  mlc.Id as MasterLCId,  mlc.LCRef as MasterLCRefNo, MLC.Amount as MasterLCValue , cur.Code AS MasterLCcurrency
                , format( MLC.LCDate,'dd-MMM-yyyy') as MasterLCOpeningDate

                ,c.id as ContractId,c.ContractNo, cov.Buyer,cov.ContractOrderQty,cov.ContractOrderValue ,cov.MasterOrderCurrency
                 , cfc.Percentage as CommissionPercentage                     ,cf.Percentage as PurchaseMargin

                 ,plc.id as PurchaseLcId ,plc.LCRef as PurchaseLCRefNo ,  P.UserName as vendor, PLC.Amount as PurchaseLcOpeningValue   ,curPLC.Code AS PurchasePLCurrency                                  
                 ,format(PLC.LCDate, 'dd-MMM-yyyy')As PurchaseLCOpeningDate  , PLCV.Amount as PresentLCValue				                        
                 ,format( PLCV.AmendmentDate, 'dd-MMM-yyyy') AS LastAmendmentDate ,format( PLC.AmendmentDate,'dd-MMM-yyyy')as AmendmentDate , po.POValue
                 from MasterLC MLC			                       
										
                   left join [dbo].[Contract] as c on c.MasterLCId=MLC.Id
                   left join contractfund as cf on cf.ContractId= c.Id and cf.FundUtilization='Purchase'
                   left join contractfund as cfc on cfc.ContractId= c.Id and cfc.FundUtilization='LessCommission'
                   left join scs.Currency cur on cur.id=MLC.CurrencyId
                   left outer join hkp.Party  as Cus on Cus.Id=mlc.CustomerId                                    
                                       
                    left outer join (select  buyer=STUFF((select distinct ','+XB.UserName from 
													trn.MasterOrder XMO 
													
																left outer join trn.MasterOrderItem XMOI on XMO.Id=XMOI.MasterOrderId
																inner join Contract XC on XC.Id=XMOI.ContractId
														left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
															where C.Id=XC.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
																							,
																							
											 c.Id As ContractNo,CurOrder.Code AS MasterOrderCurrency
					,SUM(so.Qty) AS ContractOrderQty,sum(so.qty*so.rate) AS ContractOrderValue from MasterLC MLC
									inner join Contract C on mlc.Id=c.MasterLCId
									left outer join trn.MasterOrderItem moi on moi.ContractId=c.Id

									inner join trn.MasterOrder mo on moi.MasterOrderId=mo.Id 
									
									left outer join trn.SalesOrder SO on so.MasterOrderItemId=moi.Id
									left join scs.Currency CurOrder on CurOrder.id=mo.CurrencyId
									group by c.Id,CurOrder.Code) AS COV on cov.ContractNo=c.Id                                     
                                        
				
			    left outer join PurchaseLC PLC on PLC.ContractId= c.Id
				left join scs.Currency curPLC on curPLC.id=PLC.CurrencyId
				left outer join  PurchaseLCVersion PLCV on PLCV.PurchaseLCId = PLC.Id 
								 and PLCV.ID=(select TOP 1 Id from PurchaseLCVersion where PurchaseLCId=PLC.Id ORDER BY [Version] DESC)
				left join hkp.Party P on p.id= plc.VendorId

			    left outer join (select po.PurchaseLCId,SUM(pod.TransactionQty*pod.TransactionRate) AS POValue from trn.PurchaseOrder PO
				Left outer join  trn.PurchaseOrderDetail POD on pod.InventoryReceiveId=po.Id
				group by po.PurchaseLCId) AS PO on po.PurchaseLCId=plc.Id             

		      --where mlc.id='mlc204'
			   where MLC.Id in(" + MasterLCList + @")
                order by MLC.id, c.id, plc.id";


                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsData, false, "1"); ;

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        string GetFormulaGrandTotal(ArrayList al, int col)
        {
            string _formula = string.Empty;
            ReportUtility ru = new ReportUtility();
            try
            {
                for (int i = 0; i < al.Count; i++)
                {
                    if (_formula.Length == 0)
                    {
                        _formula = "=" + ru.GetColumnNameForXls(col) + al[i];
                    }
                    else
                    {
                        _formula += "+" + ru.GetColumnNameForXls(col) + al[i];
                    }
                }
                return _formula;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, string Text)
        {

            sheet.Range[xlsRow, xlsCol].Text = Text;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);

        }
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, double Number)
        {
            sheet.Range[xlsRow, xlsCol].Number = Number;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
        }
        private void SetHeadText(IWorksheet sheet, int xlsRow, int xlsCol, string text)
        {
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
        }

      
    }
}