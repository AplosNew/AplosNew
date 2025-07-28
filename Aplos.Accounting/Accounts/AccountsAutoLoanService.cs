using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Model.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Library.Accounting.Accounts
{
    public class AccountsAutoLoanService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsAutoLoanService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
		public GridModel LoanQuery(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
		{
			parameters.CmdText = @"SELECT V.VoucherNo, F.FinancingNo, F.TransactionType, F.Id, F.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, F.PartyPlantId, PP.UserName AS PartyPlantName, F.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                , F.VoucherId, F.PostingDate, F.DocDate, F.DocRefNo, F.CurrencyId, C.Code AS CurrencyCode, F.Amount, F.IsWrittenOff, F.WrittenOffAmount, F.IsPark, F.IsPosted
                                FROM [TRN].[Financing] AS F
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=F.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=F.PartyPlantId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=F.EmployeeId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=F.CurrencyId
								LEFT JOIN [TRN].[Voucher] AS V ON V.Id=F.VoucherId
                                WHERE F.OpeningBalanceId IS NULL AND F.Archive=0 AND V.Archive=0 AND F.CompanyGroupId='" + companyGroupId + "'AND F.CompanyId='" + companyId + "' AND F.PlantId='" + plantId + "' AND F.SourceType='" + sourceType + "'";
			return _sqlRepository.GetGridData(parameters);
		}
		public IEnumerable<object> GetAutoLoanAvailableList(string plantId, bool dateRange, string fromDate, string toDate)
        
        {
            try
            {
                string dateStatus = " ";


                if (dateRange == true)
                {
                    dateStatus = " AND V.PostingDate Between '" + fromDate + "' AND '" + toDate + @"'";
                }
                else
                {

                    dateStatus = " AND V.PostingDate <= '" + fromDate + @"' ";

                }

                var sql = @"SELECT  'Acceptance' SourceType,PDA.Id PurchaseDocAcceptanceId,PDA.AcceptanceNo,format(PDA.AcceptanceDate,'dd-MMM-yyyy') AcceptanceDate,V.VoucherNo,I.VoucherId
,Format( V.PostingDate,'dd-MMM-yyyy') as PostingDate
							,P.UserName PartyName, PP.UserName PartyPlantName,PDA.PartyId,PDA.PartyPlantId
							,CurrencyCode= STUFF((select distinct ','+XC.Code from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN SCS.Currency XC ON XC.Id=XVD.CurrencyId
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						,CurrencyId= STUFF((select distinct ','+XVD.CurrencyId from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							, format(I.BaseOnDueDate,'dd-MMM-yyyy')  AS DueDateBaseON
							, format(I.ActualDueDate,'dd-MMM-yyyy')  AS DueDate
							, NoOfDays=DATEDIFF(DAY, '12-Jun-2021',I.BaseOnDueDate)
							,ISNULL(PDAD.MaterialTranAmount,0) AcceptanceAmount
							,ISNULL(PDAD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
							 ,ISNULL(I.WrittenOffAmount,0) SetOff
							 ,ISNULL(I.Amount,0)-ISNULL(I.WrittenOffAmount,0) Balance
							 ,Amount=ISNULL(I.Amount,0)-ISNULL(I.WrittenOffAmount,0),NULL LoanNo,NULL LoanDate
							 ,PurchaseLCNo= STUFF((select distinct ','+XVD.LCRef from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,LCOpeningDate= STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), XVD.LCDate, 106),' ','-') from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,PINo= STUFF((select distinct ','+XVD.PINo from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,LCAmount

							 ,OpeningBank= STUFF((select distinct ','+xbm.AccountTitle from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														left join MST.BankMaster xbm on xbm.Id=XVD.OpeningBankMasterId
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,BankMasterId= STUFF((select distinct ','+XVD.OpeningBankMasterId from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,BenificiaryBank= STUFF((select distinct ','+XVD.BenificiaryBank from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,Tenure= STUFF((select distinct ','+REPLACE(CONVERT(int, XVD.Tenure, 106),' ','-') from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,PaymentType= STUFF((select distinct ','+XVD.[Type] from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,PONo= isnull( STUFF((select distinct ','+xpomap.POId from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN trn.PurchaseDocAcceptancePOMap xpomap on xpomap.PurchaseDocAcceptanceId=xp.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
							,GRNNo= isnull( STUFF((select distinct ','+xgrnmap.GRNId from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN trn.GRNAcceptanceMap xgrnmap on xgrnmap.PurchaseDocumentAcceptanceId=xp.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

							,ContractNo= isnull( STUFF((select distinct ','+XC.ContractNo from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
							,Customer= isnull( STUFF((select distinct ','+XCU.UserName from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
														join HKP.Party XCU ON XCU.Id=XC.CustomerId
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
							 ,MasterLCNo= isnull( STUFF((select distinct ','+XC.MasterLCId from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
							,UDNo= isnull( STUFF((select distinct ','+XC.UDNo from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
							
							
                            FROM TRN.PurchasedocAcceptance AS PDA
                            LEFT JOIN (SELECT PurchaseDocAcceptanceId,SUM(MaterialTranAmount) MaterialTranAmount
										,SUM(TotalMaterialTranAmount) TotalMaterialTranAmount,SUM(ChargesTranAmount) ChargesTranAmount
										,SUM(ChargesTaxTranAmount) ChargesTaxTranAmount
								FROM TRN.PurchasedocAcceptanceDetail GROUP BY PurchaseDocAcceptanceId) AS PDAD ON PDAD.PurchaseDocAcceptanceId=PDA.id
								LEFT JOIN (select Id,sum(Amount) LCAmount from dbo.PurchaseLC group by Id) PLC ON PLC.Id=PDA.PurchaseLCId
							 LEFT JOIN HKP.Party P ON P.Id=PDA.PartyId
                            LEFT JOIN HKP.PartyPlant PP ON PP.Id=PDA.PartyPlantId
							LEFT JOIN TRN.Voucher V ON V.Id=PDA.VoucherId
							LEFT JOIN TRN.Invoice I ON I.PurchaseDocAcceptanceId=PDA.Id 
                            WHERE PDA.VoucherId <>'' and V.Plantid='" + plantId + "'  " + dateStatus + @"
							AND ISNULL(I.Amount,0)-ISNULL(I.WrittenOffAmount,0)>0
							AND pda.id NOT in (SELECT isnull(PurchaseDocAcceptanceId,'') FROM LoanAgainstAcceptanceDetail )
							--ORDER BY I.ActualDueDate ASC
UNION ALL
							SELECT  I.SourceType SourceType,I.Id PurchaseDocAcceptanceId,I.DocRefNo AcceptanceNo,format(V.PostingDate,'dd-MMM-yyyy') AcceptanceDate,V.VoucherNo,I.VoucherId
                               ,isnull( Format( V.PostingDate,'dd-MMM-yyyy'),'') as PostingDate
							,P.UserName PartyName, PP.UserName PartyPlantName,I.PartyId,I.PartyPlantId
							,CurrencyCode= XC.Code  ,I.CurrencyId
							,isnull( format(I.BaseOnDueDate,'dd-MMM-yyyy'),'')  AS DueDateBaseON
							,isnull( format(I.ActualDueDate,'dd-MMM-yyyy'),'')  AS DueDate
							, NoOfDays=DATEDIFF(DAY, GETDATE(), I.ActualDueDate)
				            ,ISNULL(I.Amount,0) AcceptanceAmount
							,ISNULL(I.Amount,0) TotalMaterialTranAmount
							 ,ISNULL(I.WrittenOffAmount,0) SetOff
							 ,ISNULL(I.Amount,0)-ISNULL(I.WrittenOffAmount,0) Balance
							 ,Amount=ISNULL(I.Amount,0)-ISNULL(I.WrittenOffAmount,0) ,NULL LoanNo,NULL LoanDate
							 ,PurchaseLCNo= STUFF((select distinct ','+XVD.LCRef from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	I.PurchaseLCId=XVD.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,LCOpeningDate= STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), XVD.LCDate, 106),' ','-') from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	I.PurchaseLCId=XVD.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,PINo= STUFF((select distinct ','+XVD.PINo from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	I.PurchaseLCId=XVD.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,LCAmount
							 ,OpeningBank= STUFF((select distinct ','+xbm.AccountTitle from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														left join MST.BankMaster xbm on xbm.Id=XVD.OpeningBankMasterId
													where	I.PurchaseLCId=XVD.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,BankMasterId= STUFF((select distinct ','+XVD.OpeningBankMasterId from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	I.PurchaseLCId=XVD.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,BenificiaryBank= STUFF((select distinct ','+XVD.BenificiaryBank from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	I.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,Tenure= STUFF((select distinct ','+REPLACE(CONVERT(int, XVD.Tenure, 106),' ','-') from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	I.PurchaseLCId=XVD.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,PaymentType= STUFF((select distinct ','+XVD.[Type] from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	I.PurchaseLCId=XVD.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,PONo= isnull( STUFF((select distinct ','+xpomap.POId from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN trn.PurchaseDocAcceptancePOMap xpomap on xpomap.PurchaseDocAcceptanceId=xp.Id
													where	I.PurchaseLCId=XVD.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
							,GRNNo= isnull( STUFF((select distinct ','+xgrnmap.GRNId from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN trn.GRNAcceptanceMap xgrnmap on xgrnmap.PurchaseDocumentAcceptanceId=xp.Id
													where	I.PurchaseLCId=XVD.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

							,ContractNo= isnull( STUFF((select distinct ','+XC.ContractNo from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
													where	I.PurchaseLCId=XVD.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
							,Customer= isnull( STUFF((select distinct ','+XCU.UserName from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
														join HKP.Party XCU ON XCU.Id=XC.CustomerId
													where	I.PurchaseLCId=XVD.Id   for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
							 ,MasterLCNo= isnull( STUFF((select distinct ','+XC.MasterLCId from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
													where	I.PurchaseLCId=XVD.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
							,UDNo= isnull( STUFF((select distinct ','+XC.UDNo from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
													where	I.PurchaseLCId=XVD.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
							
							
                            FROM TRN.Invoice I 
						    LEFT JOIN (select Id,sum(Amount) LCAmount from dbo.PurchaseLC group by Id) PLC ON PLC.Id=I.PurchaseLCId
						    LEFT JOIN HKP.Party P ON P.Id=I.PartyId
                            LEFT JOIN HKP.PartyPlant PP ON PP.Id=I.PartyPlantId
							LEFT JOIN TRN.Voucher V ON V.Id=I.VoucherId
							LEFT JOIN SCS.Currency XC ON XC.Id=I.CurrencyId
							WHERE  I.Plantid='" + plantId + "'  " + dateStatus + @"
                            --and I.PostingDate <= '13-Jan-2022' 
                            AND ISNULL(I.Amount,0)-ISNULL(I.WrittenOffAmount,0)>0 
							AND ISNULL(I.PurchaseLCId,'')<>''
							AND I.SourceType in ('InvoiceToAcceptance')
							AND I.Id NOT in (SELECT isnull(InvoiceId,'') FROM LoanAgainstAcceptanceDetail )";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

		public IEnumerable<object> GetAutoLoanPostableList(string plantId)
        {
			var sql = @"SELECT * FROM
						(SELECT 'Acceptance' SourceType,LAA.Id LoanAgainstAcceptanceId,LAA.Id, LAA.CompanyGroupId, LAA.CompanyId, LAA.PlantId, LAA.EntityId, LAA.CurrencyId, 
						LAA.VoucherId, LAA.PartyType, LAA.PartyId, LAA.PartyPlantId, LAA.TransactionType, LAA.PaymentSource, LAA.LoanDate, 
						LAA.LoanNo, LAA.Amount, format(LAA.LoanDate,'dd-MMM-yyyy') NewLoanDate,P.UserName PartyName,PP.UserName PartyPlantName ,CU.Code CurrencyCode,U.FullName UserName
						,(SELECT TOP 1  LAAD.BankMasterId
						FROM LoanAgainstAcceptanceDetail LAAD 
						LEFT JOIN TRN.PurchasedocAcceptance AS PDA ON PDA.Id=LAAD.PurchasedocAcceptanceId
						WHERE LAAD.LoanAgainstAcceptanceMasterId=LAA.Id) BankMasterId
						,(SELECT TOP 1  BM.AccountTitle 
						FROM LoanAgainstAcceptanceDetail LAAD 
						LEFT JOIN TRN.PurchasedocAcceptance AS PDA ON PDA.Id=LAAD.PurchasedocAcceptanceId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAAD.BankMasterId
						WHERE LAAD.LoanAgainstAcceptanceMasterId=LAA.Id)AccountTitle
						,PurchaseLCNo= ISNULL(STUFF((select distinct ','+XVD.LCRef from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN LoanAgainstAcceptanceDetail LAAD ON XP.Id=LAAD.PurchaseDocAcceptanceId
													where	LAAD.LoanAgainstAcceptanceMasterId=LAA.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),(STUFF((select distinct ','+XVD.LCRef from
														dbo.PurchaseLC XVD 
														LEFT JOIN TRN.Invoice I ON XVD.Id=I.PurchaseLCId
														LEFT JOIN LoanAgainstAcceptanceDetail LAADI ON I.Id=LAADI.InvoiceId
													where	LAADI.LoanAgainstAcceptanceMasterId=LAA.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')))
							,PINo= STUFF((select distinct ','+XVD.PINo from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN LoanAgainstAcceptanceDetail LAAD ON XP.Id=LAAD.PurchaseDocAcceptanceId
													where	LAAD.LoanAgainstAcceptanceMasterId=LAA.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						,(select TOP 1 I.CompanyCurrencyRate from  [dbo].[LoanAgainstAcceptanceDetail] ITWLD
						INNER JOIN TRN.Invoice I ON I.Id=ITWLD.InvoiceId WHERE ITWLD.LoanAgainstAcceptanceMasterId=LAA.Id)CompanyCurrencyRate
						,(select TOP 1 VDC.CrAmount  from  [dbo].[LoanAgainstAcceptanceDetail] ITWLD
							INNER JOIN TRN.Invoice I ON I.Id=ITWLD.InvoiceId 
							INNER JOIN TRN.InvoiceDetail ID ON ID.InvoiceId=I.Id 
							INNER JOIN TRN.VoucherDetail VD ON VD.InvoiceDetailId=ID.Id
							INNER JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
							WHERE ITWLD.LoanAgainstAcceptanceMasterId=LAA.Id)BankBookAmount
						FROM LoanAgainstAcceptanceMaster LAA 
						LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN SCS.Currency CU ON CU.Id=LAA.CurrencyId
						LEFT JOIN SEC.[USER] U ON U.UserId=LAA.AddedBy
						WHERE LAA.IsPark=1  AND LAA.VoucherId IS NULL
						UNION ALL
						SELECT 'Invoice' SourceType, LAA.Id LoanAgainstAcceptanceId,LAA.Id, LAA.CompanyGroupId, LAA.CompanyId, LAA.PlantId, LAA.EntityId, LAA.CurrencyId, LAA.VoucherId, 'Vendor' PartyType,LAA.PartyId, LAA.PartyPlantId,'LoanTaken' TransactionType,'Bank' PaymentSource , LAA.LoanDate, LAA.LoanNo,  ITLD.Amount, format(LAA.LoanDate,'dd-MMM-yyyy') NewLoanDate,P.UserName PartyName,PP.UserName PartyPlantName ,CU.Code CurrencyCode,U.FullName UserName
						,LAA.BankMasterId, BM.AccountTitle, XVD.LCRef  PurchaseLCNo,XVD.PINo
						,(select TOP 1 I.CompanyCurrencyRate from  [dbo].[InvoiceTaggingWithLCDetail] ITWLD
						INNER JOIN TRN.Invoice I ON I.Id=ITWLD.InvoiceId WHERE ITWLD.InvoiceTaggingWithLCMasterId=LAA.Id)CompanyCurrencyRate
						,ITLD.Amount BankBookAmount
						FROM InvoiceTaggingWithLCMaster LAA 
						LEFT JOIN (SELECT SUM(Amount) Amount,InvoiceTaggingWithLCMasterId 
						FROM [dbo].[InvoiceTaggingWithLCDetail] GROUP BY InvoiceTaggingWithLCMasterId ) ITLD ON LAA.Id=ITLD.InvoiceTaggingWithLCMasterId
						LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN SCS.Currency CU ON CU.Id=LAA.CurrencyId
						LEFT JOIN SEC.[USER] U ON U.UserId=LAA.AddedBy
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAA.BankMasterId
						LEFT JOIN dbo.PurchaseLC XVD ON XVD.Id=LAA.PurchaseLCId
						WHERE LAA.IsLoan=1 AND  
						 LAA.VoucherId IS NULL)X
						WHERE X.PlantId='" + plantId + "' ";
			return _sqlRepository.GetDataCollection(sql);
		}
		public IEnumerable<object> GetAutoLoanPostableDetailList(string plantId ,string LoanAgainstAcceptanceMasterId, string SourceType)
		{
			string sql = string.Empty;
			if(SourceType== "Acceptance")
            {
				sql = @"SELECT LAA.Id LoanAgainstAcceptanceId,LAA.CurrencyId, format(LAA.LoanDate,'dd-MMM-yyyy') NewLoanDate,P.UserName PartyName,PP.UserName PartyPlantName ,CU.Code CurrencyCode,U.FullName UserName
						,IVD.GLGeneralInfoId,IVD.BudgetMasterId,IVD.ActivityId,IVD.InvoiceId,IVD.Id InvoiceDetailId,IV.Amount
						,IV.CompanyCurrencyRate,BM.AccountTitle 
						,PDA.AcceptanceNo,IV.DocDate InvoieDocDate,LAAD.BankMasterId
						FROM LoanAgainstAcceptanceMaster LAA 
						LEFT JOIN LoanAgainstAcceptanceDetail LAAD ON LAA.Id=LAAD.LoanAgainstAcceptanceMasterId
						INNER JOIN TRN.PurchasedocAcceptance AS PDA ON PDA.Id=LAAD.PurchasedocAcceptanceId
						LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAAD.BankMasterId
						LEFT JOIN SCS.Currency CU ON CU.Id=LAA.CurrencyId
						LEFT JOIN TRN.Invoice IV ON IV.PurchaseDocAcceptanceId=LAAD.PurchaseDocAcceptanceId
						LEFT JOIN TRN.InvoiceDetail IVD ON IVD.InvoiceId=IV.Id
						LEFT JOIN SEC.[USER] U ON U.UserId=LAA.AddedBy
						WHERE LAA.IsPark=1 AND LAA.PlantId='" + plantId + "' AND LAAD.LoanAgainstAcceptanceMasterId='" + LoanAgainstAcceptanceMasterId + @"'  AND LAA.VoucherId IS NULL 
						UNION ALL 
						SELECT LAA.Id LoanAgainstAcceptanceId,LAA.CurrencyId, format(LAA.LoanDate,'dd-MMM-yyyy') NewLoanDate,P.UserName PartyName,PP.UserName PartyPlantName ,CU.Code CurrencyCode,U.FullName UserName
						,IVD.GLGeneralInfoId,IVD.BudgetMasterId,IVD.ActivityId,IVD.InvoiceId,IVD.Id InvoiceDetailId,IV.Amount
						,IV.CompanyCurrencyRate,BM.AccountTitle 
						,IV.DocRefNo  AcceptanceNo,IV.DocDate InvoieDocDate,LAAD.BankMasterId
						FROM LoanAgainstAcceptanceMaster LAA 
						LEFT JOIN LoanAgainstAcceptanceDetail LAAD ON LAA.Id=LAAD.LoanAgainstAcceptanceMasterId
						LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAAD.BankMasterId
						LEFT JOIN SCS.Currency CU ON CU.Id=LAA.CurrencyId
						INNER JOIN TRN.Invoice IV ON IV.Id=LAAD.InvoiceId
						LEFT JOIN TRN.InvoiceDetail IVD ON IVD.InvoiceId=IV.Id
						LEFT JOIN SEC.[USER] U ON U.UserId=LAA.AddedBy
						WHERE LAA.IsPark=1 AND LAA.PlantId='" + plantId + "' AND LAAD.LoanAgainstAcceptanceMasterId='" + LoanAgainstAcceptanceMasterId + @"'  AND LAA.VoucherId IS NULL";
			}
			else
            {
				sql = @"SELECT LAA.Id LoanAgainstAcceptanceId,LAA.CurrencyId, format(LAA.LoanDate,'dd-MMM-yyyy') NewLoanDate,P.UserName PartyName,PP.UserName PartyPlantName ,CU.Code CurrencyCode,U.FullName UserName
						,GLGeneralInfoId=case when IVD.GLGeneralInfoId<>'' then IVD.GLGeneralInfoId else AJD.GLGeneralInfoId end
						,BudgetMasterId=case when IVD.BudgetMasterId<>'' then IVD.BudgetMasterId else AJD.BudgetMasterId end
						,BudgetMasterId=case when IVD.ActivityId<>'' then IVD.ActivityId else AJD.ActivityId end
						 ,IVD.InvoiceId,IVD.Id InvoiceDetailId,LAAD.Amount
						,CompanyCurrencyRate=1--CompanyCurrencyRate=case when IV.CompanyCurrencyRate>0 then IV.CompanyCurrencyRate else 1.00 end
						,BM.AccountTitle ,LAAD.AdjustmentNoteId,LAAD.AdjustmentNoteDetailId
						,AcceptanceNo=case when IV.DocRefNo<>'' then   IV.DocRefNo else  AN.DocRefNo end
						,InvoieDocDate=case when IV.DocDate<>'' then IV.DocDate else AN.DocDate end ,LAAD.OpeningBankMasterId BankMasterId
						FROM InvoiceTaggingWithLCMaster LAA 
						LEFT JOIN InvoiceTaggingWithLCDetail LAAD ON LAA.Id=LAAD.InvoiceTaggingWithLCMasterId
						LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAAD.OpeningBankMasterId
						LEFT JOIN SCS.Currency CU ON CU.Id=LAA.CurrencyId
						LEFT JOIN TRN.Invoice IV ON IV.Id=LAAD.InvoiceId
						LEFT JOIN TRN.InvoiceDetail IVD ON IVD.InvoiceId=IV.Id
						LEFT JOIN TRN.AdjustmentNote AN ON AN.Id=LAAD.AdjustmentNoteId
						LEFT JOIN TRN.AdjustmentNoteDetail AJD ON AJD.Id=LAAD.AdjustmentNoteDetailId
						LEFT JOIN SEC.[USER] U ON U.UserId=LAA.AddedBy
						WHERE LAA.PlantId='" + plantId + "' AND LAAD.InvoiceTaggingWithLCMasterId='" + LoanAgainstAcceptanceMasterId + "'  AND LAA.VoucherId IS NULL";
			}
			
			return _sqlRepository.GetDataCollection(sql);
		}
		public IEnumerable<object> GetMaster(string CompanyGroupId, string CompanyId, string PlantId)
		{
			try
			{
				string strSQL = string.Empty;
				strSQL = @"SELECT distinct m.Id
										,pl.LCRef
										,p.UserName Vendor
										,FORMAT(m.LoanDate, 'dd-MMM-yyyy') LoanDate
										,m.LoanNo
										,c.Code Currency
										,m.Amount
										,CASE 
											WHEN ISNULL(m.VoucherId, '') = ''
												THEN 'Park'
											ELSE 'Post'
											END [Status]
									FROM LoanAgainstAcceptanceMaster m
									LEFT JOIN SCS.Currency AS c ON c.Id = m.CurrencyId
									LEFT JOIN HKP.Party AS p ON p.Id = m.PartyID
									LEFT JOIN (SELECT LoanAgainstAcceptanceMasterId,PurchaseDocAcceptanceId from  LoanAgainstAcceptanceDetail ) d ON d.LoanAgainstAcceptanceMasterId = m.Id
									LEFT JOIN TRN.PurchasedocAcceptance pd ON pd.Id=d.PurchaseDocAcceptanceId
									LEFT JOIN PurchaseLC AS pl ON pl.Id = pd.PurchaseLCId
									WHERE m.PlantId = '" + PlantId + @"'
										AND m.CompanyGroupId = '" + CompanyGroupId + @"'
										AND m.companyId = '" + CompanyId + "'";
				return _sqlRepository.GetDataCollection(strSQL);
			}
			catch (Exception ex)
			{
				throw (ex);
			}

		}//End Function
	}
}
