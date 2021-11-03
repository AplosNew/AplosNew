using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Banks;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Library.Accounting.Accounts
{
    public class AccountsBankService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsBankService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        public GridModel GetBankMasterList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string entityId, BankACType type)
        {
            try
            {
                parameters.CmdText = @"SELECT BM.Id AS BankMasterId, BM.AccountTitle, BM.AccountNumber, BM.GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
                                    , BM.BudgetMasterId, BU.Code AS BudgetCode, BU.UserName AS BudgetName, BM.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                                    , ACT.UserName AS BankAccountTypeName, BM.BankId, BM.Code AS BankCode, B.UserName AS BankName, BM.BankBranchId, BB.Code AS BankBranchCode, BB.UserName AS BankBranchName
                                    , BM.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName, BM.EntityId
                                    FROM [MST].[BankMaster] AS BM
                                    LEFT JOIN [HKP].[GLGeneralInfo] As GL ON GL.Id=BM.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BUM ON BUM.Id=BM.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS BU ON BU.Id=BUM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=BM.ActivityId
                                    LEFT JOIN [HKP].[BankAccountType] AS ACT ON ACT.Id=BM.BankAccountTypeId
                                    LEFT JOIN [HKP].[Bank] AS B ON B.Id=BM.BankId
                                    LEFT JOIN [HKP].[BankBranch] AS BB ON BB.Id=BM.BankBranchId
                                    LEFT JOIN [SCS].Currency AS C ON C.Id=BM.CurrencyId
                                    WHERE BM.Archive=0 AND BM.Active=1 AND BM.CompanyGroupId='" + companyGroupId + "' AND BM.CompanyId='" + companyId + "' AND (BM.PlantId='" + plantId + @"' OR BM.PlantId IS NULL)
                                     AND BM.AccountType='" + type + "' AND (BM.EntityId='" + entityId + "' OR BM.EntityId IS NULL)";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetBankMasterCboListByEntity(string companyGroupId, string companyId, string plantId, string entityId, BankACType bankACType)
        {
            try
            {
                var sql = @"SELECT Id, AccountTitle, AccountNumber, CurrencyId, EntityId FROM [MST].[BankMaster]
                            WHERE Archive=0 AND Active=1 AND CompanyGroupId='" + companyGroupId + "' AND CompanyId='" + companyId + "' AND (PlantId='" + plantId + @"' OR PlantId IS NULL)
                            AND AccountType='" + bankACType + "' AND (EntityId='" + entityId + "' OR EntityId IS NULL) ORDER BY 2 ASC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetBankMasterCboListByPlant(string companyGroupId, string companyId, string plantId, BankACType bankACType)
        {
            try
            {
                var sql = @"SELECT Id, AccountTitle, AccountNumber, CurrencyId, EntityId FROM [MST].[BankMaster]
                            WHERE Archive=0 AND Active=1 AND CompanyGroupId='" + companyGroupId + "' AND CompanyId='" + companyId + "' AND (PlantId='" + plantId + @"' OR PlantId IS NULL)
                            AND AccountType='" + bankACType + "' ORDER BY 2 ASC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetPartyBankCboListByParty(string partyId)
        {
            try
            {
                var sql = @"SELECT PB.Id,PB.CompanyPartyId, PB.Bank+ ' - '+PB.BankAccountNo Bank, PB.BankBranch, PB.BankAccountNo 
                        FROM HKP.PartyBank PB
                        JOIN HKP.CompanyParty CP ON CP.Id=PB.CompanyPartyId
                        JOIN HKP.Party P ON P.Id= CP.PartyId
                        WHERE P.Id='" + partyId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }



        public List<Dictionary<string, object>> GetInvestmentBankMasterCbo(string companyGroupId, string companyId, string plantId, string entityId)
        {
            try
            {
                var sql = @"SELECT Id, AccountTitle, AccountNumber, CurrencyId, EntityId FROM [MST].[BankMaster]
                            WHERE Archive=0 AND Active=1 AND CompanyGroupId='" + companyGroupId + "' AND CompanyId='" + companyId + "' AND (PlantId='" + plantId + @"' OR PlantId IS NULL)
                            AND AccountType in ('HouseBank','Investment') AND (EntityId='" + entityId + "' OR EntityId IS NULL) ORDER BY 2 ASC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        #region Bank Sheet Generation Report

        //Report service level

        private DataTable GetBankSheetGenerationData(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string bankMasterId,string PartyList)
        {
            var sql = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"';
            DECLARE @companyId VARCHAR(10)='" + companyId + @"';
            DECLARE @plantId VARCHAR(10)='" + plantId + @"';
            DECLARE @bankMasterId VARCHAR(10)='" + bankMasterId + @"';
            select X.* from (
                SELECT 
                BeneficiaryAccNo=STUFF((select distinct ','+ PB.BankAccountNo from
            TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
			LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=XP.Id AND CP.PartyType='Vendor' 
			LEFT JOIN HKP.PartyBank PB ON PB.CompanyPartyId=CP.Id
            where XVD.VoucherId=V.Id AND XVD.PartyId<>''  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			
			,BeneficiaryName=STUFF((select distinct ','+ XP.UserName from
            TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
            where XVD.VoucherId=V.Id AND XVD.PartyId<>''  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

            ,PartyId=STUFF((select distinct ','+ XP.Id from
            TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
            where XVD.VoucherId=V.Id AND XVD.PartyId<>''  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

			,VD.CrAmount InstrumentAmount
			,IFSCCode=STUFF((select distinct ','+ PB.BankAccountNo from
            TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
			LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=XP.Id AND CP.PartyType='Vendor' 
			LEFT JOIN HKP.PartyBank PB ON PB.CompanyPartyId=CP.Id
            where XVD.VoucherId=V.Id AND XVD.PartyId<>''  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			,BeneficiaryEmail=STUFF((select distinct ','+ AM.Email from
            TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
			join MST.AddressMaster AM ON AM.Id=XP.AddressMasterId
            where XVD.VoucherId=V.Id AND XVD.PartyId<>''  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS TransactionDate,NULL TransactionTypeNEFT_RTGS
            ,InformationToBeneficiary=STUFF((select distinct ','+xpA.UserName+ ' '+'('+ xp.UserName+')' from
            TRN.VoucherDetail XVD JOIN [HKP].[Party] AS XP ON XP.Id=XVD.PartyId
            JOIN HKP.Activity AS XPA ON XPA.Id=XVD.ActivityId
            where XVD.VoucherId=V.Id AND XVD.PartyId<>'' AND VD.ActivityId!=XVD.ActivityId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

            , V.Narration DebitAcccountNarration
            ,V.VoucherNo PaymentDetails1, VD.CrAmount Paymentdetails2
            ,NULL Paymentdetails3,NULL Paymentdetails4,NULL Paymentdetails5,NULL Paymentdetails6,NULL Paymentdetails7
            ,BN.UserName BeneBankName

            ,CC.CompanyCurrencyDrAmount , CC.CompanyCurrencyCrAmount
            --, OtherSide=CASE
            -- WHEN P.UserName<>'' THEN P.UserName
            -- WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
            -- WHEN CM.UserName<>'' THEN CM.UserName
            -- WHEN EI.EmployeeName <>'' THEN EI.EmployeeName
            -- ELSE A.UserName END
            FROM [TRN].[VoucherDetail] AS VD
            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
            LEFT JOIN [HKP].[Bank] AS BN ON BN.Id=BM.BankId
            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
            LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
            LEFT JOIN [MST].[BudgetMaster] AS BDM ON BDM.Id=VD.BudgetMasterId
            LEFT JOIN [HKP].[Budget] AS B ON B.Id=BDM.BudgetId
            LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=VD.EmployeeId

   --         JOIN (SELECT VoucherId FROM TRN.VoucherDetail VVD
			--WHERE VVD.BankMasterId=@bankMasterId ) VDD ON VDD.VoucherId=VD.VoucherId

            LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount

            FROM [TRN].[VoucherDetailCurrency] AS VDC
            JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
            WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
            ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
            WHERE V.Archive=0 AND V.IsPark=1 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId 
           AND VD.BankMasterId <>'' 
            -- (isnull(VD.BankMasterId,'')='' OR (isnull(VD.BankMasterId,'')<>'' AND ))
            AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"' AND V.SourceType='VendorPayment'

            UNION ALL

			SELECT BeneficiaryAccNo=PB.BankAccountNo,P.UserName BeneficiaryName,  MPD.PartyId,MPD.Amount InstrumentAmount
			,IFSCCode=PB.BankAccountNo,BeneficiaryEmail=am.Email, REPLACE(CONVERT(VARCHAR(11), MU.TentativeDate, 106), ' ', '-') AS TransactionDate,NULL TransactionTypeNEFT_RTGS
			, InformationToBeneficiary=A.UserName+'('+ p.UserName+')'
			, '' DebitAcccountNarration
            , '' PaymentDetails1, MPD.Amount Paymentdetails2
            ,NULL Paymentdetails3,NULL Paymentdetails4,NULL Paymentdetails5,NULL Paymentdetails6,NULL Paymentdetails7
            ,BN.UserName BeneBankName

            , 0 CompanyCurrencyDrAmount,MPD.Amount* IV.CompanyCurrencyRate CompanyCurrencyCrAmount 
           
			FROM TRN.MultiplePaymentDetail MPD 
			JOIN TRN.MultiplePayment MU ON MU.Id=MPD.MultiplePaymentId 
			LEFT JOIN [HKP].[Party] AS P ON P.Id = MPD.PartyId
			LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId = P.Id and CP.PartyType='Vendor' and CP.PlantId=@plantId
            LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id = CP.PartyAccountGroupId and PAG.AccountType='Vendor'
			LEFT JOIN SCS.Currency CU ON CU.Id=CP.CurrencyId
			left join MST.AddressMaster am on am.Id = P.AddressMasterId
            left join SCS.Country C on C.Id = am.CountryId
			left join SCS.[State] S on S.Id = am.StateId
			LEFT JOIN HKP.PartyBank PB ON PB.CompanyPartyId=CP.Id
			LEFT JOIN TRN.Invoice IV ON IV.Id=MPD.InvoiceId
			LEFT JOIN TRN.InvoiceDetail IVD ON IVD.Id=MPD.InvoiceDetailId
			LEFT JOIN [MST].[BudgetMaster] AS BDM ON BDM.Id=IVD.BudgetMasterId
            LEFT JOIN [HKP].[Budget] AS B ON B.Id=BDM.BudgetId
            LEFT JOIN [HKP].[Activity] AS A ON A.Id=IVD.ActivityId
			 LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=MU.BankMasterId
			 LEFT JOIN [HKP].[Bank] AS BN ON BN.Id=BM.BankId
			WHERE MU.IsPark=1 AND MU.TentativeDate BETWEEN '" + fromDate + "' AND '" + toDate + @"' AND MU.SourceType = 'VendorPayment'
			AND CP.PlantId=@plantId and PAG.AccountType='Vendor' and CP.PartyType='Vendor'
                ) X
			where x.PartyId IN (" + PartyList+@")";

            return _sqlRepository.GetDataTable(sql);
        }




        public IWorkbook GetBankSheetGenerationReport(string CompanyGroupId, string CompanyId, string PlantId, string fromDate, string toDate, string bankMasterId,string PartyList)
        {
            // var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtAutoMailReportList = GetBankSheetGenerationData(CompanyGroupId, CompanyId, PlantId,  fromDate,  toDate, bankMasterId, PartyList);

            //DataTable dtCompanyCurrency = _sqlRepository.GetDataTable(@"select CR.* from org.Company c
            //                                            inner join scs.Currency CR ON CR.Id=c.BaseCurrencyId
            //                                            where C.Id='" + CompanyId + "'");

            //if (dtAutoMailReportList.Rows.Count == 0)
            //    throw new Exception("No data found");

            worksheet.Name = "BankSheetGenerationList";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            //worksheet[ROW, COL].Text = "SL. No";
            //int colSLNO = COL;
            //worksheet[ROW, COL].ColumnWidth = 5;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;


            worksheet[ROW, COL].Text = "Beneficiary Acc No";
            int colBeneficiaryAccNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //ROW++;
           // worksheet[ROW+1, 1].Text = "A";
            COL++;

            worksheet[ROW, COL].Text = "Beneficiary Name";
            int colBeneficiaryName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Instrument Amount";
            int colInstrumentAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            //worksheet[ROW, COL].Text = "Account Title";
            //int colAccountTitle = COL;
            //worksheet[ROW, COL].ColumnWidth = 20;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            worksheet[ROW, COL].Text = "IFSC Code";
            int colIFSCCode = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;
            worksheet[ROW, COL].Text = "Beneficiary Email Id";
            int colBeneficiaryEmail = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Transaction Date";
            int colTransactionDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Transaction Type NEFT/RTGS";
            int colTransactionTypeNEFT_RTGS = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Information To Beneficiary";
            int colInformationToBeneficiary = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Debit Acccount Narration";
            int colDebitAcccountNarration = COL;
            worksheet[ROW, COL].ColumnWidth = 50;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Payment Details 1";
            int colPaymentDetails1 = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Payment Details 2";
            int colPaymentdetails2 = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "Payment Details 3";
            int colPaymentdetails3 = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;
            worksheet[ROW, COL].Text = "Payment Details 4";
            int colPaymentdetails4 = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;
            worksheet[ROW, COL].Text = "Payment Details 5";
            int colPaymentdetails5 = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;
            worksheet[ROW, COL].Text = "Payment Details 6";
            int colPaymentdetails6 = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Payment Details 7";
            int colPaymentdetails7 = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "BeneBank Name";
            int colBeneBankName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            ROW++;
            worksheet[ROW, colBeneficiaryAccNo].Text = "A";
            worksheet[ROW, colBeneficiaryName].Text = "C";
            worksheet[ROW, colInstrumentAmount].Text = "N";
            worksheet[ROW, colIFSCCode].Text = "N";


            worksheet[ROW, colBeneficiaryEmail].Text = "A";
            worksheet[ROW, colTransactionDate].Text = "D";
            worksheet[ROW, colTransactionTypeNEFT_RTGS].Text = "A";

            worksheet[ROW, colInformationToBeneficiary].Text = "C";
            worksheet[ROW, colDebitAcccountNarration].Text = "C";
            worksheet[ROW, colPaymentDetails1].Text = "C";
            worksheet[ROW, colPaymentdetails2].Text = "C";
            worksheet[ROW, colPaymentdetails3].Text = "C";
            worksheet[ROW, colPaymentdetails4].Text = "C";
            worksheet[ROW, colPaymentdetails5].Text = "C";
            worksheet[ROW, colPaymentdetails6].Text = "C";
            worksheet[ROW, colPaymentdetails7].Text = "C";
            worksheet[ROW, colBeneBankName].Text = "A";

            ROW++;
            worksheet[ROW, colBeneficiaryAccNo].Text = "30";
            worksheet[ROW, colBeneficiaryName].Text = "40";
            worksheet[ROW, colInstrumentAmount].Text = "20 (17.2)";
            worksheet[ROW, colIFSCCode].Text = "15";

            worksheet[ROW, colBeneficiaryEmail].Text = "100";
            worksheet[ROW, colTransactionDate].Text = "10";
            worksheet[ROW, colTransactionTypeNEFT_RTGS].Text = "1";

            worksheet[ROW, colInformationToBeneficiary].Text = "20";
            worksheet[ROW, colDebitAcccountNarration].Text = "20";
            worksheet[ROW, colPaymentDetails1].Text = "30";
            worksheet[ROW, colPaymentdetails2].Text = "30";
            worksheet[ROW, colPaymentdetails3].Text = "30";
            worksheet[ROW, colPaymentdetails4].Text = "30";
            worksheet[ROW, colPaymentdetails5].Text = "30";
            worksheet[ROW, colPaymentdetails6].Text = "30";
            worksheet[ROW, colPaymentdetails7].Text = "30";
            worksheet[ROW, colBeneBankName].Text = "40";


            ROW++;
            worksheet[ROW, colBeneficiaryAccNo].Text = "Mandatory";
            worksheet[ROW, colBeneficiaryName].Text = "Mandatory";
            worksheet[ROW, colInstrumentAmount].Text = "Mandatory";
            worksheet[ROW, colIFSCCode].Text = "Mandatory";

            worksheet[ROW, colBeneficiaryEmail].Text = "Optional";
            worksheet[ROW, colTransactionDate].Text = "Optional";
            worksheet[ROW, colTransactionTypeNEFT_RTGS].Text = "Optional";

            worksheet[ROW, colInformationToBeneficiary].Text = "Optional";
            worksheet[ROW, colDebitAcccountNarration].Text = "Optional";
            worksheet[ROW, colPaymentDetails1].Text = "Optional";
            worksheet[ROW, colPaymentdetails2].Text = "Optional";
            worksheet[ROW, colPaymentdetails3].Text = "Optional";
            worksheet[ROW, colPaymentdetails4].Text = "Optional";
            worksheet[ROW, colPaymentdetails5].Text = "Optional";
            worksheet[ROW, colPaymentdetails6].Text = "Optional";
            worksheet[ROW, colPaymentdetails7].Text = "Optional";
            worksheet[ROW, colBeneBankName].Text = "Optional";

            worksheet.Range[5, colBeneficiaryAccNo, 8,colBeneBankName ].HorizontalAlignment = ExcelHAlign.HAlignCenter;


            //COL++;
            //worksheet[ROW, COL].Text = "Books Dr Amount";
            //int colCompanyCurrencyDrAmount = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "Books Cr Amount";
            //int colCompanyCurrencyCrAmount = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            int endCol = COL;
            worksheet.Range[5, 1, 5, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[5, 1, 5, endCol].BorderInside(ExcelLineStyle.Hair);
            // sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

            worksheet.Range[5, 1, 5, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[5, 1, 5, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;

            for (int i = 0; i < dtAutoMailReportList.Rows.Count; i++)
            {
                //worksheet[ROW, colSLNO].Number = (i + 1);

                worksheet[ROW, colBeneficiaryAccNo].Text = dtAutoMailReportList.Rows[i]["BeneficiaryAccNo"].ToString();
                worksheet[ROW, colBeneficiaryName].Text = dtAutoMailReportList.Rows[i]["BeneficiaryName"].ToString();
                //worksheet[ROW, colInstrumentAmount].Text = dtAutoMailReportList.Rows[i]["InstrumentAmount"].ToString();

                worksheet[ROW, colInstrumentAmount].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["InstrumentAmount"].ToString());
                worksheet[ROW, colInstrumentAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                // worksheet[ROW, colAccountTitle].Text = dtAutoMailReportList.Rows[i]["AccountTitle"].ToString();
                worksheet[ROW, colIFSCCode].Text = dtAutoMailReportList.Rows[i]["IFSCCode"].ToString();
                worksheet[ROW, colBeneficiaryEmail].Text = dtAutoMailReportList.Rows[i]["BeneficiaryEmail"].ToString();
                worksheet[ROW, colTransactionDate].Text = dtAutoMailReportList.Rows[i]["TransactionDate"].ToString();
                worksheet[ROW, colTransactionTypeNEFT_RTGS].Text = dtAutoMailReportList.Rows[i]["TransactionTypeNEFT_RTGS"].ToString();
                worksheet[ROW, colInformationToBeneficiary].Text = dtAutoMailReportList.Rows[i]["InformationToBeneficiary"].ToString();

                worksheet[ROW, colDebitAcccountNarration].Text = dtAutoMailReportList.Rows[i]["DebitAcccountNarration"].ToString();
                worksheet[ROW, colPaymentDetails1].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["PaymentDetails1"].ToString());
                worksheet[ROW, colPaymentDetails1].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colPaymentdetails2].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["Paymentdetails2"].ToString());
                worksheet[ROW, colPaymentdetails2].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colPaymentdetails3].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["Paymentdetails3"].ToString());
                worksheet[ROW, colPaymentdetails3].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colPaymentdetails4].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["Paymentdetails4"].ToString());
                worksheet[ROW, colPaymentdetails4].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colPaymentdetails5].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["Paymentdetails5"].ToString());
                worksheet[ROW, colPaymentdetails5].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colPaymentdetails6].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["Paymentdetails6"].ToString());
                worksheet[ROW, colPaymentdetails6].NumberFormat = "#,##0.00;(#,##0.00)";

                worksheet[ROW, colPaymentdetails7].Number = clsStaticInfo.dbl(dtAutoMailReportList.Rows[i]["Paymentdetails7"].ToString());
                worksheet[ROW, colPaymentdetails7].NumberFormat = "#,##0.00;(#,##0.00)";


                worksheet[ROW, colBeneBankName].Text = dtAutoMailReportList.Rows[i]["BeneBankName"].ToString();

                //worksheet[ROW, colCompanyCurrencyDrAmount].Text = dtAutoMailReportList.Rows[i]["CompanyCurrencyDrAmount"].ToString();
                //worksheet[ROW, colCompanyCurrencyCrAmount].Text = dtAutoMailReportList.Rows[i]["CompanyCurrencyCrAmount"].ToString();

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;


            ReportUtility reportUtility = new ReportUtility();

           // worksheet[3, 1].Text = "Salary Head GL List";

             reportUtility.PlantHeader(ref worksheet, endCol, "Bank Sheet Generation List", PlantId);
           // reportUtility.CompanyPlantHeader(ref worksheet, endCol, "Bank Sheet Generation", identity.CompanyId, identity.PlantName, "");
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes

            return workbook;
        }
        #endregion Bank Sheet Generation Report


    }
}
