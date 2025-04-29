using Library.Core;
using Library.Crosscutting.Security;
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
using System.IO;
using System.Reflection;
using System.Threading;

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
        public GridModel GetAllBankMasterLists(GridParameter parameters, string companyGroupId, string companyId, string plantId)
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
                                    WHERE BM.Archive=0 AND BM.Active=1 AND BM.CompanyGroupId='" + companyGroupId + "' AND BM.CompanyId='" + companyId + "' AND (BM.PlantId='" + plantId + @"' OR BM.PlantId IS NULL) ";
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

        public List<Dictionary<string, object>> GetNegotiatingBankMasterCboListByPlant(string companyGroupId, string companyId, string plantId)
        {
            try
            {
                var sql = @"SELECT Id, AccountTitle, AccountNumber, CurrencyId, EntityId FROM [MST].[BankMaster]
                            WHERE Archive=0 AND Active=1 AND IsNegotiatingBank=1 AND CompanyGroupId='" + companyGroupId + "' AND CompanyId='" + companyId + "' AND (PlantId='" + plantId + @"' OR PlantId IS NULL) ORDER BY 2 ASC";
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
                            AND AccountType in ('Investment') AND (EntityId='" + entityId + "' OR EntityId IS NULL) ORDER BY 2 ASC";
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

        #region Bank Ledger Report Company Level
        public IWorkbook GetBankLedgerReportCompanyLevel(string companyGroupId, string companyId, string bankMasterId, string fromDate, string toDate, bool extended)
        {
            try
            {
                var row = 6;
                var colLast = 0;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";
                AccountsCommonService accountsCommonService = new AccountsCommonService(_sqlRepository);

                int xlsCol = 1;
                int colVoucherNo = 0;
                int colPostingDate = 0;
                //int colAccountName = 0;
                int colNarration = 0;
                int colParticulars = 0;

                int colDebit = 0;
                int colCredit = 0;
                int colBlance = 0;
                int colDrCr = 0;
                int colVoucherDetailId = 0;
                int colReconcileDate = 0;
                int colReconciliationStatus = 0;
                //int colLast = xlsCol;

                // Get BankMaster data
                var bankMaster = GetBankMaster(bankMasterId);

                // Set Header
                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank");
                // sheet.Range[row, 1, row, 2].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankMaster["BankName"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Branch");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, bankMaster["BankBranchName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Account No");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankMaster["AccountNumber"].ToString());


                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Account Title");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, bankMaster["AccountTitle"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank Currency");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                var bankCurrencyId = bankMaster["CurrencyCode"].ToString();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankCurrencyId);

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "GL");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, bankMaster["GLGeneralInfoCode"] + " - " + bankMaster["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetHeaderText(ref sheet, row, 5, "Bank Currency", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ":" + reportUtility.GetColumnNameForXls(7) + row].Merge();
                sheet.Range[row, 5, row, 7].BorderAround(ExcelLineStyle.Thin);

                colLast = 8;

                accountsCommonService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(colLast) + row + ":" + reportUtility.GetColumnNameForXls(10) + row].Merge();
                    sheet.Range[row, 8, row, 10].BorderAround(ExcelLineStyle.Thin);
                    colLast = 11;
                }

                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Voucher No", 13); colVoucherNo = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Posting Date", 11); colPostingDate = xlsCol; xlsCol++;
                //reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Account Name", 32); colAccountName = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 25); colParticulars = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Narration", 25); colNarration = xlsCol; xlsCol++;


                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colCredit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Balance", 15, ExcelHAlign.HAlignRight); colBlance = xlsCol; xlsCol++;


                int colCompanyDr = 0;
                int colCompanyCr = 0;
                int colCompanyBlance = 0;
                // int colDrCr = 0;

                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colCompanyDr = xlsCol; xlsCol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colCompanyCr = xlsCol; xlsCol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Balance", 15, ExcelHAlign.HAlignRight); colCompanyBlance = xlsCol; xlsCol++;
                }
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Dr/Cr", 5, ExcelHAlign.HAlignRight); colDrCr = xlsCol;


                if (extended == true)
                {
                    xlsCol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Id", 15, ExcelHAlign.HAlignCenter); colVoucherDetailId = xlsCol; xlsCol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Reconciliation Date", 20); colReconcileDate = xlsCol; xlsCol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Reconciliation Status", 20); colReconciliationStatus = xlsCol;

                }

                row++;
                reportUtility.SetText(ref sheet, row, 2, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(colPostingDate) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

                // Get bank opening balance data.
                var obVal = GetBankOpeningBalanceLedgerData(companyGroupId, companyId, bankMasterId, fromDate);
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    reportUtility.SetText(ref sheet, row, 7, Convert.ToDouble(obVal[0]["OB"]), true);
                    sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                        reportUtility.SetText(ref sheet, row, 10, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                    sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    colLast = 8;
                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                    {
                        colLast = 11;
                    }
                    sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                }
                sheet.Range[row, colLast].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[row, colLast].VerticalAlignment = ExcelVAlign.VAlignTop;
                row++;
                int StartRow = row;
                // Get bank transaction data.

                int col = 0;
                var ledgerData = GetBankLedgerData(companyGroupId, companyId, bankMasterId, fromDate, toDate);
                if (ledgerData.Rows.Count > 0)
                {
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDateTime(ledgerData.Rows[i]["PostingDate"].ToString()).ToString("dd-MMM-yyyy")); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["OtherSide"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString()); col++;
                        sheet.Range[row, 4].WrapText = true;
                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode == bankCurrencyId)
                        {
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                        }
                        else
                        {
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(5) + row + "-" + reportUtility.GetColumnNameForXls(6) + row + ")";
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        col++;
                        colLast = col;
                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                        {
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                            sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(10) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(8) + row + "-" + reportUtility.GetColumnNameForXls(9) + row + ")"; col++;
                            sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            colLast = col;
                        }
                        
                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, colLast].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[row, colLast].VerticalAlignment = ExcelVAlign.VAlignTop;
                        if (extended == true)
                        {
                            col++;

                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherDetailId"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["ReconcileDate"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["ReconciliationStatus"].ToString());

                        }
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 2, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();
                sheet.Range[row, colDebit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colDebit) + StartRow + ":" + reportUtility.GetColumnNameForXls(colDebit) + (row - 1) + ")";
                sheet.Range[row, colDebit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet.Range[row, colCredit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colCredit) + StartRow + ":" + reportUtility.GetColumnNameForXls(colCredit) + (row - 1) + ")";
                sheet.Range[row, colCredit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet.Range[row, 7].Formula = "=" + reportUtility.GetColumnNameForXls(7) + (row - 1);
                sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, 7].CellStyle.Font.Bold = true;
                colLast = 8;
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                {
                    sheet.Range[row, 10].Formula = "=" + reportUtility.GetColumnNameForXls(10) + (row - 1);
                    sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 10].CellStyle.Font.Bold = true;
                    colLast = 11;
                }
                sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                sheet.Range[row, colLast].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[row, colLast].VerticalAlignment = ExcelVAlign.VAlignTop;
                // row++;

                sheet.Range[12, 5, row, 5].WrapText = true;
                //sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyHeader(ref sheet, colLast, "Bank Ledger", companyId);
                reportUtility.SetText(ref sheet, 5, 4, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);

                sheet.Range[reportUtility.GetColumnNameForXls(1) + 6 + ":" + reportUtility.GetColumnNameForXls(colLast) + 6].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetBankLedgerData(string companyGroupId, string companyId, string bankMasterId, string fromDate, string toDate)
        {
            var cmdText = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"';
                    DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                    DECLARE @bankMasterId VARCHAR(10)='" + bankMasterId + @"';
                    SELECT V.VoucherNo, V.PostingDate, V.CurrencyId,
                    GLT.DrAmount AS DrAmount,
                    GLT.CrAmount AS CrAmount
                    , CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount, V.Narration,v.AddedDate
                    ,OtherSide = concat( STUFF((select distinct ','+XPP.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.PartyPlant XPP ON XPP.Id=XVD.PartyPlantId
                    where XVD.VoucherId=V.Id AND XVD.PartyPlantId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XEI.AccountTitle from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join mst.BankMaster XEI ON XEI.id=XVD.BankMasterId
					LEFT JOIN HKP.Bank BX ON BX.Id=XEI.BankId
                    where XVD.VoucherId=V.Id AND XVD.BankMasterId !=VD.BankMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    
                    ,STUFF((select distinct ','+XEI.EmployeeName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join dbo.EmployeeInformation XEI ON XEI.SystemId=XVD.EmployeeId
                    where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XCM.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join MST.CashMaster XCM ON XCM.Id=XVD.CashMasterId
                    where XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XA.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.Activity XA ON XA.Id=XVD.ActivityId
                    where XVD.VoucherId=V.Id AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL AND XVD.PartyPlantId IS NULL for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                    ,GLT.VoucherDetailId,case when isnull(GLT.ReconcileDate,'')<>'' then FORMAT(GLT.ReconcileDate,'dd-MMM-yyyy') else '' end ReconcileDate
					,case when isnull(GLT.ReconcileDate,'')<>'' then 'Yes' else 'No' end ReconciliationStatus
                    FROM [TRN].[GLTransactionDetail] AS GLT
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=GLT.VoucherDetailId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                    LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                    LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                    LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                    LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                    FROM [TRN].[VoucherDetailCurrency] AS VDC
                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND VD.BankMasterId=@bankMasterId AND V.SourceType!='OpeningBalance'
                    AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"'
                    
                    UNION ALL
                    
                    SELECT V.VoucherNo, V.PostingDate, V.CurrencyId,
                    GLT.DrAmount AS DrAmount,
                    GLT.CrAmount AS CrAmount
                    , CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount, V.Narration,v.AddedDate
                    ,OtherSide = concat( STUFF((select distinct ','+XPP.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.PartyPlant XPP ON XPP.Id=XVD.PartyPlantId
                    where XVD.VoucherId=V.Id AND XVD.PartyPlantId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XEI.EmployeeName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join dbo.EmployeeInformation XEI ON XEI.SystemId=XVD.EmployeeId
                    where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XEI.AccountTitle from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join mst.BankMaster XEI ON XEI.id=XVD.BankMasterId
					LEFT JOIN HKP.Bank BX ON BX.Id=XEI.BankId
                    where XVD.VoucherId=V.Id AND XVD.BankMasterId !=VD.BankMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    
                    ,STUFF((select distinct ','+XCM.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join MST.CashMaster XCM ON XCM.Id=XVD.CashMasterId
                    where XVD.VoucherId=V.Id AND XVD.CashMasterId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XA.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.Activity XA ON XA.Id=XVD.ActivityId
                    where XVD.VoucherId=V.Id AND XVD.BankMasterId IS NULL AND XVD.EmployeeId IS NULL AND XVD.PartyPlantId IS NULL for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                    ,GLT.VoucherDetailId,case when isnull(GLT.ReconcileDate,'')<>'' then FORMAT(GLT.ReconcileDate,'dd-MMM-yyyy') else '' end ReconcileDate
					,case when isnull(GLT.ReconcileDate,'')<>'' then 'Yes' else 'No' end ReconciliationStatus
                    FROM [TRN].[GLTransactionDetail] AS GLT
                    LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=GLT.VoucherDetailId
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                    LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                    LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                    LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                    LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
                    FROM [TRN].[VoucherDetailCurrency] AS VDC
                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND VD.BankMasterId=@bankMasterId
                    AND V.SourceType='OpeningBalance'
                    AND V.PostingDate > '" + fromDate + @"'
                    ORDER BY V.PostingDate,V.Addeddate, V.VoucherNo ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public Dictionary<string, object> GetBankMaster(string bankMasterId)
        {
            var sql = @"SELECT BM.Id, BM.AccountTitle, BM.AccountNumber, BM.CurrencyId, C.Code AS CurrencyCode, B.UserName AS BankName, BB.UserName AS BankBranchName, GLGI.AccountCode AS GLGeneralInfoCode
                    , GLGI.UserName AS GLGeneralInfoName, BGM.RefNo, BG.UserName AS BudgetName, A.UserName AS ActivityName
                    FROM [MST].[BankMaster] AS BM
                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=BM.CurrencyId
                    LEFT JOIN [HKP].[Bank] AS B ON B.Id=BM.BankId
                    LEFT JOIN [HKP].[BankBranch] AS BB ON BB.Id=BM.BankBranchId
                    LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
                    LEFT JOIN [MST].[BudgetMaster] AS BGM ON BGM.Id=BM.BudgetMasterId
                    LEFT JOIN [HKP].[Budget] AS BG ON BG.Id=BGM.BudgetId
                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=BM.ActivityId
                    WHERE BM.Id='" + bankMasterId + "'";
            return _sqlRepository.GetData(sql);
        }

        public List<Dictionary<string, object>> GetBankOpeningBalanceLedgerData(string companyGroupId, string companyId, string bankMasterId, string fromDate)
        {
            var sql = @"SELECT SUM(DrAmount) - SUM(CrAmount) AS OB
                        , CompanyCurrencyId, SUM(CompanyCurrencyDrAmount)-SUM(CompanyCurrencyCrAmount) AS CompanyCurrencyOB
                        , CompanyGroupCurrencyId, SUM(CompanyGroupCurrencyDrAmount)-SUM(CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyOB
                        , HardCurrencyId, SUM(HardCurrencyDrAmount)-SUM(HardCurrencyCrAmount) AS HardCurrencyOB FROM (
                        SELECT SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        , GC.CompanyGroupCurrencyId, SUM(GC.CompanyGroupCurrencyDrAmount) AS CompanyGroupCurrencyDrAmount, SUM(GC.CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyCrAmount
                        , HC.HardCurrencyId, SUM(HC.HardCurrencyDrAmount) AS HardCurrencyDrAmount, SUM(HC.HardCurrencyCrAmount) AS HardCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.BankMasterId=VD.BankMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.DrAmount AS CompanyGroupCurrencyDrAmount, VDC.CrAmount AS CompanyGroupCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS GC ON GC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS HardCurrencyId, VDC.DrAmount AS HardCurrencyDrAmount, VDC.CrAmount AS HardCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS HC ON HC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND VD.BankMasterId='" + bankMasterId + "' AND V.PostingDate < '" + fromDate.ToDbDate() + @"'
                        GROUP BY CC.CompanyCurrencyId, GC.CompanyGroupCurrencyId, HC.HardCurrencyId
                        UNION
                        SELECT SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId, SUM(CC.CompanyCurrencyDrAmount) AS CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount) AS CompanyCurrencyCrAmount
                        , GC.CompanyGroupCurrencyId, SUM(GC.CompanyGroupCurrencyDrAmount) AS CompanyGroupCurrencyDrAmount, SUM(GC.CompanyGroupCurrencyCrAmount) AS CompanyGroupCurrencyCrAmount
                        , HC.HardCurrencyId, SUM(HC.HardCurrencyDrAmount) AS HardCurrencyDrAmount, SUM(HC.HardCurrencyCrAmount) AS HardCurrencyCrAmount
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.BankMasterId=VD.BankMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.DrAmount AS CompanyGroupCurrencyDrAmount, VDC.CrAmount AS CompanyGroupCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS GC ON GC.VoucherDetailId=VD.Id
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS HardCurrencyId, VDC.DrAmount AS HardCurrencyDrAmount, VDC.CrAmount AS HardCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS HC ON HC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND VD.BankMasterId='" + bankMasterId + "' AND V.PostingDate ='" + fromDate.ToDbDate() + @"' AND V.SourceType='OpeningBalance'
                        GROUP BY CC.CompanyCurrencyId, GC.CompanyGroupCurrencyId, HC.HardCurrencyId
                        ) AS X GROUP BY X.CompanyCurrencyId, X.CompanyGroupCurrencyId, X.HardCurrencyId";
            return _sqlRepository.GetDataCollection(sql);
        }
        #endregion


        public string PostDateChequeReport(string POId, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "PostDateChequeReport";
                sheet = workbook.Worksheets[0];
                DataTable data;
                PostDateChequeSQL(POId, out data);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "PDC No";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPDCNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Bank Name";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBankName = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Name";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPartyName = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCurrency = COL;
                COL++;

                sheet[ROW, COL].Text = "Doc Ref No";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDocRefNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Posting Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPostingDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Doc Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDocDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Payment Date";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPaymentDate = COL;
                COL++;

                sheet[ROW, COL].Text = "BaseDate";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBaseDate = COL;
                COL++;

                sheet[ROW, COL].Text = "PO Id";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPOId = COL;
                COL++;

                sheet[ROW, COL].Text = "Remainder Days";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRemainderDays = COL;
                COL++;

                sheet[ROW, COL].Text = "Days";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColDays = COL;
                COL++;

                sheet[ROW, COL].Text = "Cheque No";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColChequeNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColResponsiblePerson = COL;
                COL++;

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRemarks = COL;

                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColPDCNo].Text = data.Rows[i]["PDCNo"].ToString();
                    sheet[ROW, ColBankName].Text = data.Rows[i]["BankName"].ToString();
                    sheet[ROW, ColPartyName].Text = data.Rows[i]["PartyName"].ToString();
                    sheet[ROW, ColCurrency].Text = data.Rows[i]["Currency"].ToString();
                    sheet[ROW, ColDocRefNo].Text = data.Rows[i]["DocRefNo"].ToString();
                    sheet[ROW, ColPostingDate].Text = data.Rows[i]["PostingDate"].ToString();
                    sheet[ROW, ColDocDate].Text = data.Rows[i]["DocDate"].ToString();
                    sheet[ROW, ColPaymentDate].Text = data.Rows[i]["PaymentDate"].ToString();
                    sheet[ROW, ColBaseDate].Text = data.Rows[i]["BaseDate"].ToString();
                    sheet[ROW, ColPOId].Text = data.Rows[i]["POId"].ToString();
                    sheet[ROW, ColRemainderDays].Text = data.Rows[i]["RemainderDays"].ToString();
                    sheet[ROW, ColDays].Text = data.Rows[i]["Days"].ToString();
                    sheet[ROW, ColChequeNo].Text = data.Rows[i]["ChequeNo"].ToString();
                    sheet[ROW, ColResponsiblePerson].Text = data.Rows[i]["ResponsiblePerson"].ToString();
                    sheet[ROW, ColAmount].Text = data.Rows[i]["Amount"].ToString();
                    sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Post Date Cheque Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


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




                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void PostDateChequeSQL(string POId, out DataTable data)
        {
            try
            {
                string strSQL = @"select PDC.Id PDCNo,PDC.BankMasterId,BM.AccountTitle BankName,PDC.PartyId,P.UserName PartyName,PDC.CurrencyId,C.[Name] Currency,PDC.DocRefNo
                            ,format(PDC.PostingDate,'dd-MMM-yyyy') PostingDate,PDC.POId,PDC.[Days],PDC.RemainderDays,PDC.ChequeNo
							,EI.SystemId ResponsiblePersonId,EI.EmployeeName ResponsiblePerson,EI.EmployeeCode ResponsiblePersonCode
							,format(PDC.DocDate,'dd-MMM-yyyy') DocDate,format(PDC.PaymentDate,'dd-MMM-yyyy') PaymentDate,format(PDC.BaseDate,'dd-MMM-yyyy')BaseDate,PDC.Amount,PDC.Remarks
                            from TRN.PostDepositCheque PDC
							left join MST.BankMaster BM on BM.Id=PDC.BankMasterId
							left join HKP.Party P on P.Id=PDC.PartyId
							left join SCS.Currency C on C.Id=PDC.CurrencyId
							left join EmployeeInformation EI on EI.SystemId=PDC.ResponsiblePersonId
							where PDC.Id='" + POId + @"'";

                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public string CurrentFundPositionReport(DateTime PostingDate, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "CurrentFundPositionReport";
                sheet = workbook.Worksheets[0];
                DataTable data;
                CurrentFundPositionSQL(PostingDate, out data);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "SL No";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColSLNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Bank CashName";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBank_CashName = COL;
                COL++;

                sheet[ROW, COL].Text = "Account Number";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColAccountNumber = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCurrency = COL;
                COL++;

                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Limit Amount";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColLimitAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Available Amount";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColTotalAvailableAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "PDC Over Due";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPDCOverDue = COL;
                COL++;

                sheet[ROW, COL].Text = "PDC In Next 7 Days";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColPDCInNext_7_Days = COL;
                COL++;

                //sheet[ROW, COL].Text = "Payment Over Due";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColPaymentOverdue = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Payment Over Due In Next 7 Days";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColPaymentOverdueInNext_7_Days = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Surplus Short As On Date";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColSurplus_Short_AsOnDate = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Short Surplus In Next 7 Days";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColShort_SurplusInNext_7_Days = COL;
                //COL++;

                sheet[ROW, COL].Text = "Remarks";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColRemarks = COL;

                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColSLNo].Text = data.Rows[i]["SLNo"].ToString();
                    sheet[ROW, ColCategory].Text = data.Rows[i]["Category"].ToString();
                    sheet[ROW, ColBank_CashName].Text = data.Rows[i]["Bank_CashName"].ToString();
                    sheet[ROW, ColAccountNumber].Text = data.Rows[i]["AccountNumber"].ToString();
                    sheet[ROW, ColCurrency].Text = data.Rows[i]["Currency"].ToString();
                    sheet[ROW, ColAmount].Number = clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());
                    sheet[ROW, ColLimitAmount].Number = clsStaticInfo.dbl(data.Rows[i]["LimitAmount"].ToString());
                    sheet[ROW, ColTotalAvailableAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalAvailableAmount"].ToString());
                    sheet[ROW, ColPDCOverDue].Number = clsStaticInfo.dbl(data.Rows[i]["PDCOverDue"].ToString());
                    sheet[ROW, ColPDCInNext_7_Days].Number = clsStaticInfo.dbl(data.Rows[i]["PDCInNext_7_Days"].ToString());
                    //sheet[ROW, ColPaymentOverdue].Number = clsStaticInfo.dbl(data.Rows[i]["PaymentOverdue"].ToString());
                    //sheet[ROW, ColPaymentOverdueInNext_7_Days].Number = clsStaticInfo.dbl(data.Rows[i]["PaymentOverdueInNext_7_Days"].ToString());
                    //sheet[ROW, ColSurplus_Short_AsOnDate].Number = clsStaticInfo.dbl(data.Rows[i]["Surplus_Short_AsOnDate"].ToString());
                    //sheet[ROW, ColShort_SurplusInNext_7_Days].Number = clsStaticInfo.dbl(data.Rows[i]["Short_SurplusInNext_7_Days"].ToString());
                    sheet[ROW, ColRemarks].Text = data.Rows[i]["Remark"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Current Fund Position Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


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




                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CurrentFundPositionSQL(DateTime PostingDate, out DataTable data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string strSQL = @"SELECT ROW_NUMBER() OVER (ORDER BY  AccountTitle) AS SLNo,Category,AccountTitle Bank_CashName,AccountNumber,Currency,SUM(DrAmount) - SUM(CrAmount) AS Amount
                         ,LimitAmount,0 TotalAvailableAmount,'' Remark,PDCOverDue,PDCInNext_7_Days,0 PaymentOverdue,0 PaymentOverdueInNext_7_Days
						 ,0 Surplus_Short_AsOnDate,0 Short_SurplusInNext_7_Days
						FROM (
                        SELECT  'Bank' Category,BM.AccountTitle,BM.AccountNumber,CU.Code Currency,BM.LimitAmount,SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId
                         ,PDCOverDue=  SUM(CASE WHEN DATEDIFF(DAY, GETDATE(),PDC.PostingDate)<0 THEN PDC.Amount else 0 end) OVER (partition by VD.BankMasterId) 
                         ,PDCInNext_7_Days=  SUM(CASE WHEN DATEDIFF(DAY, GETDATE(),PDC.PostingDate)>=0 AND DATEDIFF(DAY, GETDATE(),PDC.PostingDate)<7 THEN PDC.Amount else 0 end) OVER (partition by VD.BankMasterId) 
						 
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
						LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
						LEFT JOIN TRN.PostDepositCheque PDC ON PDC.BankMasterId=BM.Id
						LEFT JOIN SCS.Currency CU ON CU.Id=BM.CurrencyId
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.BankMasterId=VD.BankMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + identity.CompanyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"'
						--AND VD.BankMasterId='20199' 
						AND V.PostingDate <= '" + PostingDate + @"' and VD.BankMasterId<>''
                        GROUP BY CC.CompanyCurrencyId ,BM.AccountTitle,BM.AccountNumber,CU.Code,BM.LimitAmount,PDC.PostingDate,VD.BankMasterId,PDC.Amount

						UNION
						SELECT 'Cash' Category,CM.UserName AccountTitle, '' AccountNumber,CU.Code Currency,0 LimitAmount,SUM(GLTD.DrAmount) AS DrAmount, SUM(GLTD.CrAmount) AS CrAmount
                        , CC.CompanyCurrencyId
                         ,0 PDCOverDue,0PDCInNext_7_Days
                        FROM [TRN].[Voucher] AS V
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=V.Id
						LEFT JOIN MST.CashMaster CM ON CM.Id=VD.CashMasterId
						LEFT JOIN SCS.Currency CU ON CU.Id=CM.CurrencyId
                        LEFT JOIN [TRN].[GLTransactionDetail] AS GLTD ON GLTD.VoucherDetailId=VD.Id AND GLTD.CashMasterId=VD.CashMasterId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + identity.CompanyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' 
						--AND VD.BankMasterId='20199' 
						AND V.PostingDate <= '" + PostingDate + @"' and VD.CashMasterId<>''
                        GROUP BY CC.CompanyCurrencyId ,CM.UserName,CU.Code
						  ) AS X GROUP BY X.CompanyCurrencyId,X.AccountTitle,x.Currency,x.LimitAmount,x.Category,x.AccountNumber,x.PDCOverDue,X.PDCInNext_7_Days";

                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

    }
}
