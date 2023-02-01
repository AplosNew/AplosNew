using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Library.Accounting.Accounts
{
    public class AccountsDocAcceptanceService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsDocAcceptanceService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        public IEnumerable<object> GetAcceptanceNonPostedList(string plantId)
        {
            try
            {
                var sql = @"SELECT PDA.Id,REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AS AcceptanceDate
                            ,REPLACE(CONVERT(CHAR(11), PDA.DueDate, 106),' ','-') AS DueDate		
                            ,REPLACE(CONVERT(CHAR(11), PDA.InvoiceDate, 106),' ','-') AS InvoiceDate		
                            ,PDA.Remarks,P.UserName PartyName, PP.UserName PartyPlantName
                            ,PDA.AcceptanceNo, PDA.PurchaseLCId, PLC.CurrencyId,ToCurrencyRate = CASE WHEN PDA.AcceptanceRate=0 THEN PLC.Rate ELSE PDA.AcceptanceRate END, CU.Code CurrencyName
                            ,ISNULL(PDA.AcceptanceAmount,0) Amount
                            ,ISNULL(PDA.AcceptanceAmount,0) AcceptanceAmount
							,ISNULL(PDAD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
							,PDA.PurchaseLCId,PDA.PartyId,PDA.PartyPlantId
							,PDAS.ServiceAmount,V.VoucherNo,PDA.VoucherId
							 ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
							,IsChargePark=CASE WHEN  PDAS.PurchaseDocAcceptanceId IS NULL THEN '' 
										WHEN PDAS.VoucherId<>'' THEN 'Post'
										ELSE 'Park' END
                            ,IsNonCreditable=case when PDA.IsNonCreditable=1 then 'Yes' else 'No' end
							,PBM.AccountTitle LCOpeningBank
							,REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') AS LCOpeningDate
							,REPLACE(CONVERT(CHAR(11), PLC.ExpiryDate, 106),' ','-') AS LCExpiryDate
                            ,PurchaseLCNo= STUFF((select distinct ','+XVD.LCRef from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,Tenure= STUFF((select distinct ','+REPLACE(CONVERT(int, XVD.Tenure, 106),' ','-') from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,PaymentType= STUFF((select distinct ','+XVD.[Type] from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,ContractNo= STUFF((select distinct ','+XC.ContractNo from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,AcceptanceFirst =case when PLC.IsAccepptanceFirst=1 then 'Yes' else 'No' end
                            FROM TRN.PurchasedocAcceptance AS PDA
                            LEFT JOIN (SELECT PurchaseDocAcceptanceId,SUM(TotalMaterialTranAmount) TotalMaterialTranAmount,SUM(ChargesTranAmount) ChargesTranAmount
										,SUM(ChargesTaxTranAmount) ChargesTaxTranAmount
								FROM TRN.PurchasedocAcceptanceDetail GROUP BY PurchaseDocAcceptanceId) AS PDAD ON PDAD.PurchaseDocAcceptanceId=PDA.Id
                            LEFT JOIN(select VoucherId,PurchaseDocAcceptanceId,SUM(ISNULL(Amount,0)) ServiceAmount
							 FROM  TRN.PurchaseDocAcceptanceCharges GROUP BY VoucherId,PurchaseDocAcceptanceId) AS PDAS ON PDAS.PurchaseDocAcceptanceId=PDA.id
                            LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
                            LEFT JOIN MST.BankMaster PBM ON PBM.Id=PLC.OpeningBankMasterId
                            LEFT JOIN SCS.Currency CU ON CU.Id=PLC.CurrencyId
                            LEFT JOIN HKP.Party P ON P.Id=PDA.PartyId
                            LEFT JOIN HKP.PartyPlant PP ON PP.Id=PDA.PartyPlantId
							LEFT JOIN TRN.Voucher V ON V.Id=PDA.VoucherId
                            WHERE PDA.VoucherId IS NULL AND PDA.PlantId='" + plantId + @"'
                            ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetAcceptancePostedList(string plantId)
        {
            try
            {
                var sql = @"SELECT  PDA.Id,REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AS AcceptanceDate
                            ,REPLACE(CONVERT(CHAR(11), PDA.DueDate, 106),' ','-') AS DueDate		
                            ,REPLACE(CONVERT(CHAR(11), PDA.InvoiceDate, 106),' ','-') AS InvoiceDate		
                            ,PDA.Remarks,P.UserName PartyName, PP.UserName PartyPlantName
                            ,PDA.AcceptanceNo,PDA.PurchaseLCId,PDAS.CurrencyId, 1 ToCurrencyRate,CU.Code CurrencyName
                            ,ISNULL(PDA.AcceptanceAmount,0) Amount
							,ISNULL(PDAD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
							,PDA.PurchaseLCId,PDA.PartyId,PDA.PartyPlantId
							,PDAS.ServiceAmount,V.VoucherNo,PDA.VoucherId
							 ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
							,IsChargePark=CASE WHEN  PDAS.PurchaseDocAcceptanceId IS NULL THEN '' 
										WHEN PDAS.VoucherId<>'' THEN 'Post'
										ELSE 'Park' END
                            ,IsNonCreditable=case when PDA.IsNonCreditable=1 then 'Yes' else 'No' end
                            ,PurchaseLCNo= STUFF((select distinct ','+XVD.LCRef from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,ContractNo= isnull( STUFF((select distinct ','+XC.ContractNo from
														dbo.PurchaseLC XVD Left join TRN.PurchasedocAcceptance AS XP ON XP.PurchaseLCId=XVD.Id
														LEFT JOIN dbo.[Contract] XC ON XC.Id=XVD.ContractId
													where	PDA.Id=XP.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
,V.IsPark
                            FROM TRN.PurchasedocAcceptance AS PDA
                            LEFT JOIN (SELECT PurchaseDocAcceptanceId
										,SUM(TotalMaterialTranAmount) TotalMaterialTranAmount,SUM(ChargesTranAmount) ChargesTranAmount
										,SUM(ChargesTaxTranAmount) ChargesTaxTranAmount
								FROM TRN.PurchasedocAcceptanceDetail GROUP BY PurchaseDocAcceptanceId) AS PDAD ON PDAD.PurchaseDocAcceptanceId=PDA.id
                            LEFT JOIN(select VoucherId,PurchaseDocAcceptanceId,SUM(ISNULL(Amount,0)) ServiceAmount,CurrencyId
							 FROM  TRN.PurchaseDocAcceptanceCharges GROUP BY VoucherId,PurchaseDocAcceptanceId,CurrencyId) AS PDAS ON PDAS.PurchaseDocAcceptanceId=PDA.id
                            --LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=PDAD.POId
                            LEFT JOIN SCS.Currency CU ON CU.Id=PDAS.CurrencyId
                            LEFT JOIN HKP.Party P ON P.Id=PDA.PartyId
                            LEFT JOIN HKP.PartyPlant PP ON PP.Id=PDA.PartyPlantId
							LEFT JOIN TRN.Voucher V ON V.Id=PDA.VoucherId
                            WHERE V.Archive=0 AND PDA.VoucherId <>'' AND PDA.PlantId='" + plantId + @"'
                            order by V.PostingDate desc
                            ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetAcceptanceChargesNonPostedList(string plantId)
        {
            try
            {
                var sql = @"SELECT 
	                        PDA.Id
	                        ,REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AS AcceptanceDate		
	                        ,PDA.Remarks
	                        ,PDA.AcceptanceNo
	                        ,PDAD.Amount,PDA.PurchaseLCId,PDAD.CurrencyId
							,C.Code CurrencyName
							,PDA.PurchaseLCId,PDA.PartyId,PDA.PartyPlantId
							,P.UserName PartyName,OBM.AccountTitle LCOpeningBank
							,IsAcceptancePark=CASE WHEN  PDA.VoucherId<>'' THEN 'Post' ELSE 'Park' END
							,PDAS.AcceptanceAmount
							,REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') AS LCOpeningDate	
							,REPLACE(CONVERT(CHAR(11), PLC.ExpiryDate, 106),' ','-') AS LCExpiryDate
                            ,PDA.AcceptanceRate ToCurrencyRate, PDA.IsNonCreditable
	                        FROM trn.PurchasedocAcceptance AS PDA
	                        LEFT JOIN(select distinct VoucherId,PurchasedocAcceptanceId,CurrencyId
							,SUM(isnull(Amount,0)) Amount from trn.PurchasedocAcceptanceCharges 
							group by VoucherId,PurchasedocAcceptanceId,CurrencyId ) AS PDAD ON PDAD.PurchaseDocAcceptanceId=PDA.id
	                        LEFT JOIN SCS.Currency C ON C.Id=PDAD.CurrencyId
							LEFT JOIN HKP.Party P ON P.Id=PDA.PartyId
							LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
							LEFT JOIN MST.BankMaster OBM ON OBM.Id=PLC.OpeningBankMasterId
							 LEFT JOIN(select PurchaseDocAcceptanceId,SUM(ISNULL(MaterialTranAmount,0)) AcceptanceAmount
							 FROM  trn.PurchaseDocAcceptanceDetail GROUP BY PurchaseDocAcceptanceId) AS PDAS ON PDAS.PurchaseDocAcceptanceId=PDA.id
							WHERE PDAD.VoucherId IS NULL AND PDAD.PurchaseDocAcceptanceId<>'' AND PDA.PlantId='" + plantId + @"'
                            ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetAcceptanceChargesPostedList(string plantId)
        {
            try
            {
                var sql = @"SELECT 
	                        PDA.Id
	                        ,REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AS AcceptanceDate		
	                        ,PDA.Remarks
	                        ,PDA.AcceptanceNo
	                        ,PDAD.Amount,PDA.PurchaseLCId,PDAD.CurrencyId
							,C.Code CurrencyName
							,PDA.PurchaseLCId,PDA.PartyId,PDA.PartyPlantId
							,P.UserName PartyName,OBM.AccountTitle LCOpeningBank
							,IsAcceptancePark=CASE WHEN  PDA.VoucherId<>'' THEN 'Post' ELSE 'Park' END
							,PDAS.AcceptanceAmount
							,V.VoucherNo,PDAD.VoucherId,PDA.AcceptanceRate ToCurrencyRate
	                        FROM trn.PurchasedocAcceptance AS PDA
	                        LEFT JOIN(select distinct VoucherId,PurchasedocAcceptanceId,CurrencyId
							,SUM(isnull(Amount,0)) Amount from TRN.PurchasedocAcceptanceCharges
							GROUP BY VoucherId,PurchasedocAcceptanceId,CurrencyId ) AS PDAD ON PDAD.PurchaseDocAcceptanceId=PDA.id
	                        LEFT JOIN SCS.Currency C ON C.Id=PDAD.CurrencyId
							LEFT JOIN HKP.Party P ON P.Id=PDA.PartyId
							LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
							LEFT JOIN MST.BankMaster OBM ON OBM.Id=PLC.OpeningBankMasterId
							LEFT JOIN(select PurchaseDocAcceptanceId,SUM(ISNULL(MaterialTranAmount,0)) AcceptanceAmount
							FROM  TRN.PurchaseDocAcceptanceDetail GROUP BY PurchaseDocAcceptanceId) AS PDAS ON PDAS.PurchaseDocAcceptanceId=PDA.id
							LEFT JOIN TRN.Voucher V ON V.Id=PDAD.VoucherId
							WHERE V.Archive=0 AND PDAD.VoucherId<>'' AND PDA.PlantId='" + plantId + @"'
                            ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetAcceptancePOServicePostedList(string plantId)
        {
            try
            {
                var sql = @"SELECT   PDA.Id
	                        ,REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AS AcceptanceDate		
	                        ,PDA.Remarks
	                        ,PDA.AcceptanceNo
	                        ,PDAD.Amount,PDA.PurchaseLCId,PDAD.CurrencyId
							,C.Code CurrencyName
							,PDA.PurchaseLCId,PDA.PartyId,PDA.PartyPlantId
							,P.UserName PartyName,OBM.AccountTitle LCOpeningBank
							,IsAcceptancePark=CASE WHEN  PDA.VoucherId<>'' THEN 'Post' ELSE 'Park' END
							,PDAS.AcceptanceAmount
							,REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') AS LCOpeningDate	
							,REPLACE(CONVERT(CHAR(11), PLC.ExpiryDate, 106),' ','-') AS LCExpiryDate
                            ,PDA.AcceptanceRate ToCurrencyRate, PDA.IsNonCreditable
	                        FROM trn.PurchasedocAcceptance AS PDA
	                        LEFT JOIN(select distinct VoucherId,PurchasedocAcceptanceId,CurrencyId
							,SUM(isnull(Amount,0)) Amount from trn.PurchaseDocAcceptanceService 
							group by VoucherId,PurchasedocAcceptanceId,CurrencyId ) AS PDAD ON PDAD.PurchaseDocAcceptanceId=PDA.id
	                        LEFT JOIN SCS.Currency C ON C.Id=PDAD.CurrencyId
							LEFT JOIN HKP.Party P ON P.Id=PDA.PartyId
							LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
							LEFT JOIN MST.BankMaster OBM ON OBM.Id=PLC.OpeningBankMasterId
							 LEFT JOIN(select PurchaseDocAcceptanceId,SUM(ISNULL(MaterialTranAmount,0)) AcceptanceAmount
							 FROM  trn.PurchaseDocAcceptanceDetail GROUP BY PurchaseDocAcceptanceId) AS PDAS ON PDAS.PurchaseDocAcceptanceId=PDA.id
							WHERE PDAD.VoucherId<>'' AND PDAD.PurchaseDocAcceptanceId<>'' AND PDA.PlantId='" + plantId + @"'
                            ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }


        }
        public IEnumerable<object> GetAcceptancePOServiceNonPostedList(string plantId)
        {
            try
            {
                var sql = @"SELECT   PDA.Id
	                        ,REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AS AcceptanceDate		
	                        ,PDA.Remarks
	                        ,PDA.AcceptanceNo
	                        ,PDAD.Amount,PDA.PurchaseLCId,PDAD.CurrencyId
							,C.Code CurrencyName
							,PDA.PurchaseLCId,PDA.PartyId,PDA.PartyPlantId
							,P.UserName PartyName,OBM.AccountTitle LCOpeningBank
							,IsAcceptancePark=CASE WHEN  PDA.VoucherId<>'' THEN 'Post' ELSE 'Park' END
							,PDAS.AcceptanceAmount
							,REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') AS LCOpeningDate	
							,REPLACE(CONVERT(CHAR(11), PLC.ExpiryDate, 106),' ','-') AS LCExpiryDate
                            ,PDA.AcceptanceRate ToCurrencyRate, PDA.IsNonCreditable
	                        FROM trn.PurchasedocAcceptance AS PDA
	                        LEFT JOIN(select distinct VoucherId,PurchasedocAcceptanceId,CurrencyId
							,SUM(isnull(Amount,0)) Amount from trn.PurchaseDocAcceptanceService 
							group by VoucherId,PurchasedocAcceptanceId,CurrencyId ) AS PDAD ON PDAD.PurchaseDocAcceptanceId=PDA.id
	                        LEFT JOIN SCS.Currency C ON C.Id=PDAD.CurrencyId
							LEFT JOIN HKP.Party P ON P.Id=PDA.PartyId
							LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
							LEFT JOIN MST.BankMaster OBM ON OBM.Id=PLC.OpeningBankMasterId
							 LEFT JOIN(select PurchaseDocAcceptanceId,SUM(ISNULL(MaterialTranAmount,0)) AcceptanceAmount
							 FROM  trn.PurchaseDocAcceptanceDetail GROUP BY PurchaseDocAcceptanceId) AS PDAS ON PDAS.PurchaseDocAcceptanceId=PDA.id
							WHERE PDAD.VoucherId IS NULL AND PDAD.PurchaseDocAcceptanceId<>'' AND PDA.PlantId='" + plantId + @"'
                            ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
    }
}