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
                                WHERE F.OpeningBalanceId IS NULL AND F.Archive=0 AND F.CompanyGroupId='" + companyGroupId + "'AND F.CompanyId='" + companyId + "' AND F.PlantId='" + plantId + "' AND F.SourceType='" + sourceType + "'";
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

                var sql = @"SELECT  PDA.Id PurchaseDocAcceptanceId,PDA.AcceptanceNo,format(PDA.AcceptanceDate,'dd-MMM-yyyy') AcceptanceDate,V.VoucherNo,Format( V.PostingDate,'dd-MMM-yyyy') as PostingDate
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
							--LEFT JOIN (SELECT PurchaseDocAcceptanceId,SUM(ISNULL(Amount,0)) LoanAccAmount FROM TRN.LoanAgainstAcceptance WHERE ISNULL(VoucherId,'') ='' GROUP BY PurchaseDocAcceptanceId)LAA ON LAA.PurchaseDocAcceptanceId=PDA.Id  
                            WHERE PDA.VoucherId <>'' and V.Plantid='" + plantId + "'  " + dateStatus + @"
							AND ISNULL(I.Amount,0)-ISNULL(I.WrittenOffAmount,0)>0
							AND pda.id NOT in (SELECT PurchaseDocAcceptanceId FROM LoanAgainstAcceptanceDetail )
							ORDER BY I.ActualDueDate ASC ";
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
			var sql = @"SELECT LAA.Id LoanAgainstAcceptanceId,LAA.*, format(LAA.LoanDate,'dd-MMM-yyyy') NewLoanDate,P.UserName PartyName,PP.UserName PartyPlantName ,CU.Code CurrencyCode,U.FullName UserName
						,IVD.GLGeneralInfoId,IVD.BudgetMasterId,IVD.ActivityId,IVD.InvoiceId,IVD.Id InvoiceDetailId,IV.CompanyCurrencyRate,BM.AccountTitle 
						,PDA.AcceptanceNo,PDA.AcceptanceDate,LAAD.BankMasterId
						,PurchaseLCNo= STUFF((select distinct ','+XVD.LCRef from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,LCOpeningDate= STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), XVD.LCDate, 106),' ','-') from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,PINo= STUFF((select distinct ','+XVD.PINo from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,PaymentType= STUFF((select distinct ','+XVD.[Type] from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						FROM LoanAgainstAcceptanceMaster LAA 
						LEFT JOIN LoanAgainstAcceptanceDetail LAAD ON LAA.Id=LAAD.LoanAgainstAcceptanceMasterId
						LEFT JOIN TRN.PurchasedocAcceptance AS PDA ON PDA.Id=LAAD.PurchasedocAcceptanceId
						LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAAD.BankMasterId
						LEFT JOIN SCS.Currency CU ON CU.Id=LAA.CurrencyId
						LEFT JOIN TRN.Invoice IV ON IV.PurchaseDocAcceptanceId=LAAD.PurchaseDocAcceptanceId
						LEFT JOIN TRN.InvoiceDetail IVD ON IVD.InvoiceId=IV.Id
						LEFT JOIN SEC.[USER] U ON U.UserId=LAA.AddedBy
						WHERE LAA.IsPark=1 AND LAA.PlantId='" + plantId + "'  AND LAA.VoucherId IS NULL";
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
