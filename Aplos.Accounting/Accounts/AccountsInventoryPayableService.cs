using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.Accounts
{
    public class AccountsInventoryPayableService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsInventoryPayableService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        public IEnumerable<object> GetInventoryMaterialRejectPayable(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount,T.InvoiceId,T.IsWrittenOff
					FROM (
						SELECT  'Material' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId
							, MGGL.InventoryGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
							, MGGL.InventoryBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
							, MGGL.InventoryActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
							--,NULL GLGeneralInfoId, NULL GLGeneralInfoCode, NULL GLGeneralInfoName
							--,NULL BudgetMasterId, NULL BudgetCode, NULL BudgetName
							--,NULL ActivityId, NULL ActivityCode, NULL ActivityName
							, SUM(IRD.RejectValue) AS Cr, NULL Dr
							, SUM(IRD.RejectValue) AS Amount
							,NULL AS InvoiceId, 0 IsWrittenOff
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName
					, T.TrnType,T.TaxCategoryId,T.InvoiceId,t.IsWrittenOff
     
					--UNION
					--SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, IRT.TaxCategoryId
					--	, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
					--	, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
					--	, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
					--	--, SUM((ISNULL(IRT.TaxAmount,1)/IRD.TransactionQty)*IRD.ShortageQty) AS  Cr, NULL Dr
					--	--, SUM((ISNULL(IRT.TaxAmount,1)/IRD.TransactionQty)*IRD.ShortageQty) AS Amount
					--	, SUM(ISNULL(IRT.TaxAmount,0)) AS  Cr, NULL Dr
					--	, SUM(ISNULL(IRT.TaxAmount,0)) AS Amount
					--	,NULL AS InvoiceId, 0 IsWrittenOff
					--FROM [TRN].[InventoryReceiveTax] AS IRT
					--LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
					--LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					--LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
					--LEFT JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
					--		AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
					--LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					--LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					--LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					--LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					--LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					--LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					--WHERE IRT.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IRT.InventoryReceiveDetailId<>'' AND IRT.TaxAmount > 0
					--GROUP BY MM.MaterialGroupMasterId, IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount 
						,T.InvoiceId,T.IsWrittenOff
					FROM (
						SELECT 'Vendor' AS OtherName,'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,NULL Cr
						--,SUM(MAT.Cr) +SUM(ISNULL(srv.Amount,0))+SUM(ISNULL(SRV.TotalTaxAmount,0)) AS Dr,
						--SUM(MAT.Cr) +SUM(ISNULL(srv.Amount,0))+SUM(ISNULL(SRV.TotalTaxAmount,0)) AS Amount 
						,SUM(MAT.Cr) AS Dr,
						SUM(MAT.Cr) AS Amount 
						,MAT.InvoiceId, MAT.IsWrittenOff
						FROM (
							SELECT IR.Id, 'Vendor' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
							, MGPGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
							, MGPGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
							, MGPGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
							, NULL Dr, SUM(IRD.RejectValue)   AS  Cr
							, SUM(IRD.RejectValue)  AS Amount
							,IV.Id AS InvoiceId, IV.IsWrittenOff
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						LEFT JOIN [TRN].[Invoice] AS IV ON IV.InventoryReceiveId=IR.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName,IV.Id,IV.IsWrittenOff
						) AS MAT
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.Amount) AS Amount,sum(INS.TotalTaxAmount) AS TotalTaxAmount
						from  [TRN].[InventoryService] AS INS  where InventoryReceiveId=@receiveId group by INS.InventoryReceiveId
						) AS SRV on SRV.InventoryReceiveId=MAT.Id
						GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,MAT.InvoiceId,MAT.IsWrittenOff
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount
					, T.OtherName, T.TrnType,T.TaxCategoryId,T.InvoiceId,T.IsWrittenOff";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetInventoryMaterialShortagePayable(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount,T.InvoiceId,T.IsWrittenOff
					FROM (
						SELECT  'Material' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId
							, MGGL.InventoryGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
							, MGGL.InventoryBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
							, MGGL.InventoryActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
							--,NULL GLGeneralInfoId, NULL GLGeneralInfoCode, NULL GLGeneralInfoName
							--,NULL BudgetMasterId, NULL BudgetCode, NULL BudgetName
							--,NULL ActivityId, NULL ActivityCode, NULL ActivityName
							, SUM(IRD.ShortageValue) AS Cr, NULL Dr
							, SUM(IRD.ShortageValue) AS Amount
							,NULL AS InvoiceId, 0 IsWrittenOff
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName
					, T.TrnType,T.TaxCategoryId,T.InvoiceId,t.IsWrittenOff
     
					--UNION
					--SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, IRT.TaxCategoryId
					--	, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
					--	, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
					--	, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
					--	--, SUM((ISNULL(IRT.TaxAmount,1)/IRD.TransactionQty)*IRD.ShortageQty) AS  Cr, NULL Dr
					--	--, SUM((ISNULL(IRT.TaxAmount,1)/IRD.TransactionQty)*IRD.ShortageQty) AS Amount
					--	, SUM(ISNULL(IRT.TaxAmount,0)) AS  Cr, NULL Dr
					--	, SUM(ISNULL(IRT.TaxAmount,0)) AS Amount
					--	,NULL AS InvoiceId, 0 IsWrittenOff
					--FROM [TRN].[InventoryReceiveTax] AS IRT
					--LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
					--LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					--LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
					--LEFT JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
					--		AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
					--LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					--LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					--LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					--LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					--LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					--LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					--WHERE IRT.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IRT.InventoryReceiveDetailId<>'' AND IRT.TaxAmount > 0
					--GROUP BY MM.MaterialGroupMasterId, IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount 
						,T.InvoiceId,T.IsWrittenOff
					FROM (
						SELECT 'Vendor' AS OtherName,'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,NULL Cr
						--,SUM(MAT.Cr) +SUM(ISNULL(srv.Amount,0))+SUM(ISNULL(SRV.TotalTaxAmount,0)) AS Dr,
						--SUM(MAT.Cr) +SUM(ISNULL(srv.Amount,0))+SUM(ISNULL(SRV.TotalTaxAmount,0)) AS Amount 
						,SUM(MAT.Cr) AS Dr,
						SUM(MAT.Cr) AS Amount 
						,MAT.InvoiceId, MAT.IsWrittenOff
						FROM (
							SELECT IR.Id, 'Vendor' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
							, MGPGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
							, MGPGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
							, MGPGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
							, NULL Dr, SUM(IRD.ShortageValue)   AS  Cr
							, SUM(IRD.ShortageValue)  AS Amount
							,IV.Id AS InvoiceId, IV.IsWrittenOff
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						LEFT JOIN [TRN].[Invoice] AS IV ON IV.InventoryReceiveId=IR.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName,IV.Id,IV.IsWrittenOff
						) AS MAT
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.Amount) AS Amount,sum(INS.TotalTaxAmount) AS TotalTaxAmount
						from  [TRN].[InventoryService] AS INS  where InventoryReceiveId=@receiveId group by INS.InventoryReceiveId
						) AS SRV on SRV.InventoryReceiveId=MAT.Id
						GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,MAT.InvoiceId,MAT.IsWrittenOff
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount
					, T.OtherName, T.TrnType,T.TaxCategoryId,T.InvoiceId,T.IsWrittenOff";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        private Dictionary<string, object> GetInventoryReceive(string receivedId)
        {
            var cmdText = @"select IsNonCreditable,PartyId FROM TRN.[InventoryReceive] where Id = '" + receivedId.ToString() + "'";
            return _sqlRepository.GetData(cmdText);
        }
        private Dictionary<string, object> GetPurchaseReturn(string purchaseReturnId)
        {
            var cmdText = @"select IsNonCreditable,PartyId FROM TRN.[PurchaseReturn] where Id = '" + purchaseReturnId.ToString() + "'";
            return _sqlRepository.GetData(cmdText);
        }
        private Dictionary<string, object> GetCompanyPartyGroup(string partyId, string plantId)
        {
            var cmdText = @"select PartyAccountGroupId FROM HKP.CompanyParty where PartyId = '" + partyId + "' AND PlantId='" + plantId + @"' and PartyType in('Vendor','Director')";
            return _sqlRepository.GetData(cmdText);
        }
        public IEnumerable<object> GetInventoryMaterialWithoutReversChargePayable(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var inventoryReceiveData = GetInventoryReceive(inveReveiveId);
                var companyParty = GetCompanyPartyGroup(inventoryReceiveData["PartyId"].ToString(), plantId);

                if (Convert.ToBoolean(inventoryReceiveData["IsNonCreditable"].ToString()))
                {
                    var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						, T.Cr, T.Amount,T.InventoryReceiveDetailId,T.IsAsset 
					FROM (
						SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.IsAsset,MM.FixedAssetMasterId

							,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
							, SUM(IRD.TotalMaterialTranAmount) AS Dr, NULL Cr
							, SUM(IRD.TotalMaterialTranAmount) AS Amount
                            ,IRD.Id AS  InventoryReceiveDetailId
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,FAG.AssetUnderConstructionGLId,GLF.AccountCode,GLF.UserName
						,FAG.AssetUnderConstructionBudgetMasterId,BF.Code,BF.UserName
						,FAG.AssetUnderConstructionActivityId,AF.Code,AF.UserName,IRD.Id,MM.IsAsset
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.InventoryReceiveDetailId,T.IsAsset
					
                   
                    UNION
					
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount ,NULL InventoryReceiveDetailId,T.IsAsset
                           
					FROM (
						SELECT 'Vendor' AS OtherName,'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,NULL Dr
						,SUM(MAT.Cr)  AS Cr,
						SUM(MAT.Cr)  AS Amount ,MAT.IsAsset
						FROM (
							SELECT IR.Id, 'Vendor' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,GLGeneralInfoId =  CPGL.GLGeneralInfoId     
							,GLGeneralInfoCode = GL.AccountCode
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId = CPGL.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName = B.UserName 
							,ActivityId = CPGL.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName

                            , NULL Dr, SUM(IRD.TotalMaterialTranAmount)   AS  Cr
							, SUM(IRD.TotalMaterialTranAmount)  AS Amount,MM.IsAsset
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[CompanyParty] CP ON IR.PartyId = CP.PartyId AND CP.PlantId=@plantId AND CP.PartyType='Vendor'
						LEFT JOIN HKP.CompanyPartyGL CPGL ON CPGL.CompanyPartyId=CP.Id  and CPGL.PartyGLType='ReconciliationGL'
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON CPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON CPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON CPGL.ActivityId= A.Id

						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY  IR.Id, CPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, CPGL.BudgetMasterId, B.Code, B.UserName, CPGL.ActivityId, A.Code, A.UserName ,MM.IsAsset
						) AS MAT
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.Amount) AS Amount,sum(INS.TotalTaxAmount * IR.ToCurrencyRate) AS TotalTaxAmount
						from  [TRN].[InventoryService] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.InventoryReceiveId 
                        where InventoryReceiveId=@receiveId group by INS.InventoryReceiveId
						) AS SRV on SRV.InventoryReceiveId=MAT.Id
						GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,MAT.IsAsset
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr
					, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset
				UNION ALL
				SELECT T.OtherName, T.TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount , NULL InventoryReceiveDetailId,0 IsAsset
					FROM (
						
							SELECT IR.Id, 'Acceptance' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,PDAD.GLGeneralInfoId ,GL.AccountCode GLGeneralInfoCode ,GL.UserName GLGeneralInfoName
							,PDAD.BudgetMasterId ,B.Code BudgetCode ,B.UserName BudgetName
							,PDAD.ActivityId ,A.Code ActivityCode ,A.UserName ActivityName
							, NULL Dr, SUM(PDAD.TotalMaterialTranAmount)   AS  Cr
							, SUM(PDAD.TotalMaterialTranAmount)  AS Amount
							,0 IsAsset
						FROM TRN.PurchaseDocAcceptance PDA
						LEFT JOIN TRN.PurchaseDocAcceptanceDetail PDAD ON PDAD.PurchaseDocAcceptanceId=PDA.Id
						LEFT JOIN TRN.InventoryReceive IR ON IR.PurchaseDocumentAcceptanceId=PDA.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON PDAD.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON PDAD.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON PDAD.ActivityId= A.Id
						WHERE IR.Id=@receiveId
						GROUP BY  IR.Id, PDAD.GLGeneralInfoId, GL.AccountCode, GL.UserName, PDAD.BudgetMasterId, B.Code, B.UserName, PDAD.ActivityId, A.Code, A.UserName
					) AS T
					GROUP BY  T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType--,T.TaxCategoryId, T.IsAsset
					ORDER BY T.TrnType DESC ";
                    return _sqlRepository.GetDataCollection(sql);
                }
                else
                {
                    var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId, NULL AS TaxCodeId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId,T.BudgetActive,T.BudgetMasterActivityActive,T.EntityId,T.EntityName
					FROM (
						SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId, NULL AS TaxCodeId,MM.FixedAssetMasterId

							,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
							, SUM(IRD.TotalMaterialTranAmount ) AS Dr, NULL Cr
							, SUM(IRD.TotalMaterialTranAmount ) AS Amount
                           ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId,BudgetActive=CASE WHEN BM.Active=1 THEN BM.Active ELSE BMF.Active END
							,BudgetMasterActivityActive=case	WHEN BMA.Active=1 THEN BMA.Active ELSE BMAF.Active END  ,MRM.EntityId,E.UserName EntityName
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId   
						LEFT JOIN [TRN].[MaterialRequsitionMaster] MRM ON MRM.Id=PID.RequisitionId
						LEFT JOIN [ORG].[Entity] AS E ON E.Id= MRM.EntityId
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id
						LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
						LEFT JOIN [MST].[BudgetMasterActivity] BMAF ON BMAF.BudgetMasterId=BMF.Id AND AF.Id=BMAF.ActivityId
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName
						, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,FAG.AssetUnderConstructionGLId,GLF.AccountCode,GLF.UserName
						,FAG.AssetUnderConstructionBudgetMasterId,BF.Code,BF.UserName ,MRM.EntityId,E.UserName
						,FAG.AssetUnderConstructionActivityId,AF.Code,AF.UserName,IRD.Id,BM.Active,BMA.Active,BMF.Active,BMAF.Active
                    ) AS T
					
                   UNION ALL
				   
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId, NULL AS TaxCodeId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive,NULL EntityId,NULL EntityName
					FROM [TRN].[InventoryReceiveTax] AS IRT
					LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
							AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
					WHERE IRD.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IRT.InventoryReceiveDetailId<>'' AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY  IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName,BM.Active ,BMA.Active 
					UNION ALL
					SELECT 'TCS' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, TC.TaxCategoryId,IRT.TaxCodeId
						, TCGL.CreditableGLId  GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.CreditableGLBudgetMasterId BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.CreditableGLActivityId ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive,NULL EntityId,NULL EntityName
					FROM [TRN].[InventoryReceiveAdditionalTax] AS IRT
					LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRT.InventoryReceiveId
					LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TCGL.TaxCodeId=IRT.TaxCodeId
					LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id=TCGL.TaxCodeId
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.CreditableGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.CreditableGLBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.CreditableGLActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
					WHERE IR.Id=@receiveId  AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY  TC.TaxCategoryId, TCGL.CreditableGLId, GL.AccountCode, GL.UserName, TCGL.CreditableGLBudgetMasterId
					, B.Code, B.UserName, TCGL.CreditableGLActivityId, A.Code, A.UserName,IRT.TaxCodeId,BM.Active ,BMA.Active 
					UNION ALL
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId, NULL AS TaxCodeId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRTS.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRTS.TaxAmount) AS Amount
                       ,0 IsAsset, NULL InventoryReceiveDetailId,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive,NULL EntityId,NULL EntityName
					--, IRTS.TaxAmount
					--, IRTS.TaxAmount
					FROM [TRN].[InventoryReceiveTax] AS IRTS
					LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryService] AS INS ON INS.Id=IRTS.InventoryServiceId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
					WHERE INS.InventoryReceiveId=@receiveId --AND IR.IsNonCreditable=0 
					AND IRTS.InventoryServiceId<>'' and INS.IsOtherVendor=0 AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY IRTS.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName,BM.Active ,BMA.Active 
					UNION ALL
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId, NULL AS TaxCodeId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount ,T.IsAsset, NULL InventoryReceiveDetailId,T.BudgetActive,T.BudgetMasterActivityActive,NULL EntityId,NULL EntityName
					FROM (
						SELECT 'Vendor' AS OtherName,'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,NULL Dr
						,SUM(MAT.Cr) +ISNULL(TCS.TCSAmount,0) AS Cr,--+SUM(ISNULL(SRV.TotalTaxAmount,0))
						SUM(MAT.Cr) +ISNULL(TCS.TCSAmount,0) AS Amount --+SUM(ISNULL(SRV.TotalTaxAmount,0))
                        ,0 IsAsset,MAT.BudgetActive,MAT.BudgetMasterActivityActive
						FROM (
							SELECT IR.Id, 'Vendor' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
							,GLGeneralInfoId =  CPGL.GLGeneralInfoId     
							,GLGeneralInfoCode = GL.AccountCode
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId = CPGL.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName = B.UserName 
							,ActivityId = CPGL.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, NULL Dr, SUM(IRD.TotalMaterialTranAmount) + SUM(IRD.TotalTaxAmount)+ SUM(IRD.ChargesTaxTranAmount)  AS  Cr
							, SUM(IRD.TotalMaterialTranAmount) + SUM(IRD.TotalTaxAmount)+ SUM(IRD.ChargesTaxTranAmount) AS Amount,BM2.Active BudgetActive,BMA.Active BudgetMasterActivityActive
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[CompanyParty] CP ON IR.PartyId = CP.PartyId AND CP.PlantId=@plantId AND CP.PartyType in('Vendor','Director')
						LEFT JOIN HKP.CompanyPartyGL CPGL ON CPGL.CompanyPartyId=CP.Id and CPGL.PartyGLType='ReconciliationGL' 
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON CPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON CPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON CPGL.ActivityId= A.Id
						LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM2.Id AND A.Id=BMA.ActivityId
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY  IR.Id, CPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, CPGL.BudgetMasterId, B.Code, B.UserName, CPGL.ActivityId, A.Code, A.UserName,BM2.Active ,BMA.Active 
						,MM.IsAsset,BM2.Active ,BMA.Active 
						) AS MAT
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.Amount) AS Amount,sum(INS.TotalTaxAmount) AS TotalTaxAmount
						from  [TRN].[InventoryService] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.InventoryReceiveId 
                        where InventoryReceiveId=@receiveId and IsOtherVendor=0 group by INS.InventoryReceiveId
						) AS SRV on SRV.InventoryReceiveId=MAT.Id
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.TaxAmount) AS TCSAmount
						from  [TRN].[InventoryReceiveAdditionalTax] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.InventoryReceiveId 
                        where InventoryReceiveId=@receiveId group by INS.InventoryReceiveId
						) AS TCS on TCS.InventoryReceiveId=MAT.Id

						GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,TCS.TCSAmount,MAT.BudgetActive,MAT.BudgetMasterActivityActive
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName
					, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId, T.IsAsset,T.BudgetActive,T.BudgetMasterActivityActive
					
					UNION ALL
					SELECT T.OtherName, T.TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId, NULL AS TaxCodeId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount ,0 IsAsset, NULL InventoryReceiveDetailId,T.BudgetActive,T.BudgetMasterActivityActive,NULL EntityId,NULL EntityName
					FROM (
						
							SELECT IR.Id, 'Acceptance' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
							
                            ,PDAD.GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,PDAD.BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,PDAD.ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName

							, NULL Dr, SUM(IRD.TotalMaterialTranAmount)   AS  Cr
							, SUM(IRD.TotalMaterialTranAmount)  AS Amount
							,0 IsAsset,BM2.Active BudgetActive,BMA.Active BudgetMasterActivityActive
						FROM TRN.PurchaseDocAcceptance PDA
						LEFT JOIN TRN.PurchaseDocAcceptanceDetail PDAD ON PDAD.PurchaseDocAcceptanceId=PDA.Id
						LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.PurchaseDocumentAcceptanceDetailId=PDAD.Id
						LEFT JOIN TRN.GRNAcceptanceMap GRNACC ON GRNACC.PurchaseDocumentAcceptanceId=PDA.Id
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=GRNACC.GRNId
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON PDAD.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON PDAD.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON PDAD.ActivityId= A.Id
						LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM2.Id AND A.Id=BMA.ActivityId
						WHERE IR.Id=@receiveId
						GROUP BY  IR.Id, PDAD.GLGeneralInfoId, GL.AccountCode, GL.UserName, PDAD.BudgetMasterId, B.Code, B.UserName, PDAD.ActivityId, A.Code, A.UserName,BM2.Active ,BMA.Active 
					) AS T
					GROUP BY  T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,t.BudgetActive,T.BudgetMasterActivityActive--,T.TaxCategoryId, T.IsAsset
					UNION ALL
					SELECT T.OtherName, T.TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId, NULL AS TaxCodeId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount ,0 IsAsset, NULL InventoryReceiveDetailId,T.BudgetActive,T.BudgetMasterActivityActive,NULL EntityId,NULL EntityName
					FROM (
						
							SELECT IR.Id, 'LCBase' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
							
                            ,MGGL.ClearingAccountGLId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,MGGL.ClearingAccountBudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGGL.ClearingAccountActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName

							, NULL Dr, SUM(IRD.TotalMaterialTranAmount)+ SUM(IRD.TotalTaxAmount)+ SUM(IRD.ChargesTaxTranAmount)   AS  Cr
							, SUM(IRD.TotalMaterialTranAmount)+ SUM(IRD.TotalTaxAmount)+ SUM(IRD.ChargesTaxTranAmount)  AS Amount
							,0 IsAsset,BM2.Active BudgetActive,BMA.Active BudgetMasterActivityActive
						FROM TRN.InventoryReceiveDetail IRD
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGGL.ClearingAccountGLId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGGL.ClearingAccountBudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ClearingAccountActivityId= A.Id
						LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM2.Id AND A.Id=BMA.ActivityId
						WHERE IR.Id=@receiveId
						GROUP BY  IR.Id, MGGL.ClearingAccountGLId, GL.AccountCode, GL.UserName, MGGL.ClearingAccountBudgetMasterId, B.Code, B.UserName, MGGL.ClearingAccountActivityId, A.Code, A.UserName,BM2.Active ,BMA.Active 
					) AS T
					GROUP BY  T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,t.BudgetActive,T.BudgetMasterActivityActive--,T.TaxCategoryId, T.IsAsset
					ORDER BY T.TrnType DESC ";
                    return _sqlRepository.GetDataCollection(sql);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetInventoryMaterialReversChargePayable(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var inventoryReceiveData = GetInventoryReceive(inveReveiveId);
                var companyParty = GetCompanyPartyGroup(inventoryReceiveData["PartyId"].ToString(), plantId);
                if (Convert.ToBoolean(inventoryReceiveData["IsNonCreditable"].ToString()))
                {
                    var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						, T.Cr, T.Amount,T.InventoryReceiveDetailId,T.IsAsset 
					FROM (
						SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.IsAsset,MM.FixedAssetMasterId

							,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
							, SUM(IRD.TotalMaterialTranAmount) AS Dr, NULL Cr
							, SUM(IRD.TotalMaterialTranAmount) AS Amount
                            ,IRD.Id AS  InventoryReceiveDetailId
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,FAG.AssetUnderConstructionGLId,GLF.AccountCode,GLF.UserName
						,FAG.AssetUnderConstructionBudgetMasterId,BF.Code,BF.UserName
						,FAG.AssetUnderConstructionActivityId,AF.Code,AF.UserName,IRD.Id,MM.IsAsset
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.InventoryReceiveDetailId,T.IsAsset
					
                   
                    UNION
					
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount ,NULL InventoryReceiveDetailId,T.IsAsset
                           
					FROM (
						SELECT 'Vendor' AS OtherName,'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,NULL Dr
						,SUM(MAT.Cr)  AS Cr,
						SUM(MAT.Cr)  AS Amount ,MAT.IsAsset
						FROM (
							SELECT IR.Id, 'Vendor' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,GLGeneralInfoId =  CPGL.GLGeneralInfoId     
							,GLGeneralInfoCode = GL.AccountCode
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId = CPGL.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName = B.UserName 
							,ActivityId = CPGL.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName

                            , NULL Dr, SUM(IRD.TotalMaterialTranAmount)   AS  Cr
							, SUM(IRD.TotalMaterialTranAmount)  AS Amount,MM.IsAsset
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[CompanyParty] CP ON IR.PartyId = CP.PartyId AND CP.PlantId=@plantId AND CP.PartyType='Vendor'
						LEFT JOIN HKP.CompanyPartyGL CPGL ON CPGL.CompanyPartyId=CP.Id and CPGL.PartyGLType='ReconciliationGL' 
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON CPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON CPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON CPGL.ActivityId= A.Id

						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName,MM.IsAsset
						) AS MAT
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.Amount) AS Amount,sum(INS.TotalTaxAmount * IR.ToCurrencyRate) AS TotalTaxAmount
						from  [TRN].[InventoryService] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.InventoryReceiveId 
                        where InventoryReceiveId=@receiveId group by INS.InventoryReceiveId
						) AS SRV on SRV.InventoryReceiveId=MAT.Id
						GROUP BY  IR.Id, CPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, CPGL.BudgetMasterId, B.Code, B.UserName, CPGL.ActivityId, A.Code, A.UserName ,MM.IsAsset
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr
					, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset
UNION
				SELECT T.OtherName, T.TrnType, NULL MaterialGroupMasterId, NULL TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount , NULL InventoryReceiveDetailId,0 IsAsset
					FROM (
						
							SELECT IR.Id, 'Acceptance' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,PDAD.GLGeneralInfoId ,GL.AccountCode GLGeneralInfoCode ,GL.UserName GLGeneralInfoName
							,PDAD.BudgetMasterId ,B.Code BudgetCode ,B.UserName BudgetName
							,PDAD.ActivityId ,A.Code ActivityCode ,A.UserName ActivityName
							, NULL Dr, SUM(PDAD.TotalMaterialTranAmount)   AS  Cr
							, SUM(PDAD.TotalMaterialTranAmount)  AS Amount
							,0 IsAsset
						FROM TRN.PurchaseDocAcceptance PDA
						LEFT JOIN TRN.PurchaseDocAcceptanceDetail PDAD ON PDAD.PurchaseDocAcceptanceId=PDA.Id
						LEFT JOIN TRN.InventoryReceive IR ON IR.PurchaseDocumentAcceptanceId=PDA.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON PDAD.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON PDAD.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON PDAD.ActivityId= A.Id
						WHERE IR.Id=@receiveId
						GROUP BY  IR.Id, PDAD.GLGeneralInfoId, GL.AccountCode, GL.UserName, PDAD.BudgetMasterId, B.Code, B.UserName, PDAD.ActivityId, A.Code, A.UserName
					) AS T
					GROUP BY  T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType--,T.TaxCategoryId, T.IsAsset
					ORDER BY T.TrnType DESC ";
                    return _sqlRepository.GetDataCollection(sql);
                }
                else
                {
                    var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @countryId varchar(10)
                    SET @countryId =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)
                     SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId
	                        , MGGL.InventoryGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
	                        , MGGL.InventoryBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                            , MGGL.InventoryActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
	                        , SUM(IRD.TotalMaterialTranAmount) AS Dr, NULL Cr
		                    , SUM(IRD.TotalMaterialTranAmount) AS Amount,IRD.Id AS  InventoryReceiveDetailId,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive
                    FROM [TRN].[InventoryReceiveDetail] AS IRD
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                    JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
		                    AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                    LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
                    LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
                    LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                    LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
                    WHERE IRD.InventoryReceiveId=@receiveId
                    GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName,IRD.Id,BM.Active,BMA.Active
                    UNION
                    SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, IRT.TaxCategoryId
	                        , TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
	                        , TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                            , TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
	                        , SUM(IRT.TaxAmount) AS  Dr, NULL Cr
		                    , SUM(IRT.TaxAmount) AS Amount,NULL  InventoryReceiveDetailId,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive
                    FROM [TRN].[InventoryReceiveTax] AS IRT
                    JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                    JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
		                    AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                    JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
                    JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
                    LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
                    LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                    LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
                    WHERE IRT.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND TCGL.TaxType='RCM'
                    GROUP BY MM.MaterialGroupMasterId, IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId
					, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName,BM.Active ,BMA.Active 
                    UNION
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRTS.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRTS.TaxAmount) AS Amount,NULL  InventoryReceiveDetailId,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive
						--, IRTS.TaxAmount
						--, IRTS.TaxAmount
					FROM [TRN].[InventoryReceiveTax] AS IRTS
					JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
					JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
					WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND TCGL.TaxType='RCM'
					GROUP BY IRTS.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId
					, A.Code, A.UserName ,BM.Active ,BMA.Active                        
					UNION
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount, NULL InventoryReceiveDetailId,T.BudgetActive,T.BudgetMasterActivityActive
					FROM (
					SELECT 'Vendor' AS OtherName,'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,NULL Dr
						,SUM(MAT.Cr) +SUM(ISNULL(TCS.TCSAmount,0)) AS Cr,--+SUM(ISNULL(SRV.TotalTaxAmount,0))
						SUM(MAT.Cr) +SUM(ISNULL(TCS.TCSAmount,0))  AS Amount --+SUM(ISNULL(SRV.TotalTaxAmount,0))
                        ,0 IsAsset,MAT.Id,MAT.BudgetActive,MAT.BudgetMasterActivityActive
						FROM (
                    SELECT 'Vendor' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
	                    , MGPGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
	                    , MGPGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
	                    , MGPGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                        , NULL Dr, SUM(IRD.TotalMaterialTranAmount) AS  Cr
	                    , SUM(IRD.TotalMaterialTranAmount) AS Amount,NULL  InventoryReceiveDetailId,IR.Id,BM2.Active BudgetActive,BMA.Active BudgetMasterActivityActive
                    FROM [TRN].[InventoryMaterial] AS IM
                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                    LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
                    JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                    JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
                            AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                    LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
                    LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
                    LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
                    LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
                    LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
                    LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM2.Id AND A.Id=BMA.ActivityId
                    WHERE IRD.InventoryReceiveId=@receiveId
                    GROUP BY  MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName,IR.Id,bm2.Active,bma.Active)
					MAT
					LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.TaxAmount) AS TCSAmount
						from  [TRN].[InventoryReceiveAdditionalTax] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.InventoryReceiveId 
                        where InventoryReceiveId=@receiveId group by INS.InventoryReceiveId
						) AS TCS on TCS.InventoryReceiveId=MAT.Id

					GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,MAT.BudgetActive,MAT.BudgetMasterActivityActive
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId, T.IsAsset,T.BudgetActive,T.BudgetMasterActivityActive
					
                    UNION
                    SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, IRT.TaxCategoryId
	                        , TCGL.LiabilityGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
	                        , TCGL.LiabilityBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                            , TCGL.LiabilityActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
	                        , NULL AS  Dr, SUM(IRT.TaxAmount) Cr
		                    , SUM(IRT.TaxAmount) AS Amount,NULL  InventoryReceiveDetailId,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive
                    FROM [TRN].[InventoryReceiveTax] AS IRT
                    JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
                    JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                    JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
		                    AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                    JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
                    JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.LiabilityGLId=GL.Id
                    LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.LiabilityBudgetMasterId= BM.Id
                    LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                    LEFT JOIN [HKP].[Activity] AS A ON TCGL.LiabilityActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
                    WHERE IRT.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND TCGL.TaxType='RCM'
                    GROUP BY MM.MaterialGroupMasterId, IRT.TaxCategoryId, TCGL.LiabilityGLId, GL.AccountCode, GL.UserName, TCGL.LiabilityBudgetMasterId
					, B.Code, B.UserName, TCGL.LiabilityActivityId, A.Code, A.UserName,BM.Active ,BMA.Active 
					UNION
					SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.LiabilityGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.LiabilityBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.LiabilityActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, NULL AS Dr, SUM(IRTS.TaxAmount) AS Cr
						, SUM(IRTS.TaxAmount) AS Amount,NULL  InventoryReceiveDetailId,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive
					FROM [TRN].[InventoryReceiveTax] AS IRTS
					JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
					JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
					WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND TCGL.TaxType='RCM'
					GROUP BY IRTS.TaxCategoryId, TCGL.LiabilityGLId, GL.AccountCode, GL.UserName, TCGL.LiabilityBudgetMasterId, B.Code, B.UserName
					, TCGL.LiabilityActivityId, A.Code, A.UserName,BM.Active ,BMA.Active 
				UNION
					SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.LiabilityGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.LiabilityBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.LiabilityActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, NULL AS Dr, SUM(IRTS.TaxAmount) AS Cr
						, SUM(IRTS.TaxAmount) AS Amount, NULL  InventoryReceiveDetailId,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive
					 FROM[TRN].[InventoryReceiveTax] AS IRTS
					JOIN[TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId = IR.Id
					JOIN[MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId = IRTS.TaxCategoryId
					JOIN[MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId = TC.Id AND IRTS.TaxCategoryId = TC.Id
					LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON TCGL.LiabilityGLId = GL.Id
					LEFT JOIN[MST].[BudgetMaster] AS BM ON TCGL.LiabilityBudgetMasterId = BM.Id
					LEFT JOIN[HKP].[Budget] AS B ON BM.BudgetId = B.Id
					LEFT JOIN[HKP].[Activity] AS A ON TCGL.LiabilityActivityId = A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
					WHERE IRTS.InventoryReceiveId = @receiveId AND IR.IsNonCreditable = 0
					AND IRTS.InventoryServiceId <> '' AND TCGL.InputTaxOutPutTax = 'Input' AND TCGL.TaxType = 'RCM'
					GROUP BY IRTS.TaxCategoryId, TCGL.LiabilityGLId, GL.AccountCode, GL.UserName, TCGL.LiabilityBudgetMasterId, B.Code, B.UserName
					, TCGL.LiabilityActivityId, A.Code, A.UserName,BM.Active ,BMA.Active 
					UNION
					SELECT 'TCS' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, TC.TaxCategoryId
						, TCGL.CreditableGLId  GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.CreditableGLBudgetMasterId BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.CreditableGLActivityId ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        , NULL InventoryReceiveDetailId,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive
					FROM [TRN].[InventoryReceiveAdditionalTax] AS IRT
					LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRT.InventoryReceiveId
					LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TCGL.TaxCodeId=IRT.TaxCodeId
					LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id=TCGL.TaxCodeId
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.CreditableGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.CreditableGLBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.CreditableGLActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
					WHERE IR.Id=@receiveId  AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY  TC.TaxCategoryId, TCGL.CreditableGLId, GL.AccountCode, GL.UserName, TCGL.CreditableGLBudgetMasterId
					, B.Code, B.UserName, TCGL.CreditableGLActivityId, A.Code, A.UserName,BM.Active ,BMA.Active 

					UNION
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
					    , SUM(IRTS.TaxAmount) AS Dr, NULL AS Cr
						, SUM(IRTS.TaxAmount) AS Amount,NULL  InventoryReceiveDetailId,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive
					FROM [TRN].[InventoryReceiveTax] AS IRTS
					JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
					JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
					WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=0
					AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND TCGL.TaxType='RCM'
					GROUP BY IRTS.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName,BM.Active ,BMA.Active ";
                    return _sqlRepository.GetDataCollection(sql);

                }

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetInventoryMaterialForImprestPayable(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var isNonCreditable = GetInventoryReceive(inveReveiveId);
                if (Convert.ToBoolean(isNonCreditable["IsNonCreditable"].ToString()))
                {
                    var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @countryId varchar(10)
                    SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
	                    , T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
	                    , T.BudgetMasterId, T.BudgetCode, T.BudgetName
	                    , T.ActivityId, T.ActivityCode, T.ActivityName
	                    , T.Dr 
                        --+ (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
	                   -- WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ()) AS Dr
	                    , T.Cr, T.Amount + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount 
                        FROM [TRN].[InventoryReceiveTax] AS IRTS 
		                    JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
		                    WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ())AS Amount,T.InventoryReceiveDetailId,T.IsAsset
                    FROM (
	                    SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.IsAsset,MM.FixedAssetMasterId

							,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
							, SUM(IRD.TotalMaterialBooksCurrencyAmount) AS Dr, NULL Cr
							, SUM(IRD.TotalMaterialBooksCurrencyAmount) AS Amount
                            ,IRD.Id AS  InventoryReceiveDetailId
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
                        --LEFT JOIN (SELECT FixedAssetMasterId,AssetUnderConstructionGLId ,AssetUnderConstructionBudgetMasterId,AssetUnderConstructionActivityId
						 --FROM HKP.FixedAssetMasterGL) AS FAG ON FAG.FixedAssetMasterId=MM.FixedAssetMasterId
						LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,FAG.AssetUnderConstructionGLId,GLF.AccountCode,GLF.UserName
						,FAG.AssetUnderConstructionBudgetMasterId,BF.Code,BF.UserName
						,FAG.AssetUnderConstructionActivityId,AF.Code,AF.UserName,IRD.Id 
                    ) AS T
                    GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode
                    , T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.InventoryReceiveDetailId,T.IsAsset
                    UNION
                    
                    SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
	                    , T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
	                    , T.BudgetMasterId, T.BudgetCode, T.BudgetName
	                    , T.ActivityId, T.ActivityCode, T.ActivityName
	                    , T.Dr, T.Cr 
                            --+ (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
		                    --WHERE IRTS.InventoryReceiveId=@receiveId AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ()) AS Cr
	                    , T.Amount ,NULL InventoryReceiveDetailId, 0 IsAsset
                        --+ (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
		                    --WHERE IRTS.InventoryReceiveId=@receiveId AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ())AS Amount
                    FROM (
	                    SELECT 'Vendor' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
		                    , NULL AS GLGeneralInfoId, NULL AS GLGeneralInfoCode, NULL AS GLGeneralInfoName
		                    , NULL AS BudgetMasterId, NULL AS BudgetCode, NULL AS BudgetName
		                    , NULL ActivityId, NULL AS ActivityCode, NULL AS ActivityName
		                    , NULL Dr, SUM(IRD.TotalMaterialBooksCurrencyAmount)  AS  Cr
		                    , SUM(IRD.TotalMaterialBooksCurrencyAmount) AS Amount
	                    FROM [TRN].[InventoryMaterial] AS IM
	                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
	                    LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
	                    JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
	                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
	                    WHERE IRD.InventoryReceiveId=@receiveId
                    ) AS T
                    GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId";
                    return _sqlRepository.GetDataCollection(sql);
                }
                else
                {
                    var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @countryId varchar(10)
                    SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
	                    , T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
	                    , T.BudgetMasterId, T.BudgetCode, T.BudgetName
	                    , T.ActivityId, T.ActivityCode, T.ActivityName
	                    , T.Dr + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
	                    WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ()) AS Dr
	                    , T.Cr, T.Amount + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount 
                        FROM [TRN].[InventoryReceiveTax] AS IRTS 
		                    JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
		                    WHERE IRTS.InventoryReceiveId=@receiveId AND IR.IsNonCreditable=1 AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ())AS Amount,T.InventoryReceiveDetailId,T.IsAsset,T.BudgetActive,T.BudgetMasterActivityActive

                    FROM (
	                    SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.IsAsset,MM.FixedAssetMasterId

							,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
							, SUM(IRD.TotalMaterialBooksCurrencyAmount) AS Dr, NULL Cr
							, SUM(IRD.TotalMaterialBooksCurrencyAmount) AS Amount
                            ,IRD.Id AS  InventoryReceiveDetailId ,BudgetActive=case when MM.IsAsset=0 THEN BM.Active ELSE BMF.Active END
							,BudgetMasterActivityActive=CASE WHEN MM.IsAsset=0 THEN BMA.Active ELSE BMAF.Active END       
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
						LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id
						LEFT JOIN [MST].[BudgetMasterActivity] BMAF ON BMAF.BudgetMasterId=BMF.Id AND AF.Id=BMAF.ActivityId
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,FAG.AssetUnderConstructionGLId,GLF.AccountCode,GLF.UserName
						,FAG.AssetUnderConstructionBudgetMasterId,BF.Code,BF.UserName
						,FAG.AssetUnderConstructionActivityId,AF.Code,AF.UserName,IRD.Id,BM.Active,BMF.Active,BMA.Active,BMAF.Active
                    ) AS T
                    GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode
                    , T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.InventoryReceiveDetailId
					,T.IsAsset,T.BudgetActive,T.BudgetMasterActivityActive
                    UNION
                    SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, IRT.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount * IR.ToCurrencyRate) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount * IR.ToCurrencyRate) AS Amount,NULL InventoryReceiveDetailId,0 IsAsset,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive
					FROM [TRN].[InventoryReceiveTax] AS IRT
					LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
							AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
					WHERE IRD.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IRT.InventoryReceiveDetailId<>''
					GROUP BY MM.MaterialGroupMasterId, IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName,BM.Active ,BMA.Active 
					
					UNION
					SELECT 'Svc' AS OtherName, 'Dr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRTS.TaxAmount * IR.ToCurrencyRate) AS  Dr, NULL Cr
						, SUM(IRTS.TaxAmount * IR.ToCurrencyRate) AS Amount,NULL InventoryReceiveDetailId,0 IsAsset,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive
						--, IRTS.TaxAmount
						--, IRTS.TaxAmount
					FROM [TRN].[InventoryReceiveTax] AS IRTS
					LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
                    LEFT JOIN [TRN].[InventoryService] AS INS ON INS.Id=IRTS.InventoryServiceId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
					WHERE INS.InventoryReceiveId=@receiveId --AND IR.IsNonCreditable=0 
					AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM'
					GROUP BY IRTS.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName,BM.Active ,BMA.Active 
					
					UNION
					SELECT 'TCS' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, TC.TaxCategoryId
						, TCGL.CreditableGLId  GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.CreditableGLBudgetMasterId BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.CreditableGLActivityId ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        , NULL InventoryReceiveDetailId,0 IsAsset,BM.Active BudgetActive,BMA.Active BudgetMasterActivityActive
					FROM [TRN].[InventoryReceiveAdditionalTax] AS IRT
					LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRT.InventoryReceiveId
					LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TCGL.TaxCodeId=IRT.TaxCodeId
					LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id=TCGL.TaxCodeId
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.CreditableGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.CreditableGLBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.CreditableGLActivityId= A.Id
					LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=BM.Id AND A.Id=BMA.ActivityId
					WHERE IR.Id=@receiveId  AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY  TC.TaxCategoryId, TCGL.CreditableGLId, GL.AccountCode, GL.UserName, TCGL.CreditableGLBudgetMasterId
					, B.Code, B.UserName, TCGL.CreditableGLActivityId, A.Code, A.UserName,BM.Active ,BMA.Active 
					UNION
                    SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
	                    , T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
	                    , T.BudgetMasterId, T.BudgetCode, T.BudgetName
	                    , T.ActivityId, T.ActivityCode, T.ActivityName
	                    , T.Dr, T.Cr + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
		                    WHERE IRTS.InventoryReceiveId=@receiveId AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ()) AS Cr
	                    , T.Amount + (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount 
                        FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
		                    WHERE IRTS.InventoryReceiveId=@receiveId AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ())AS Amount,NULL InventoryReceiveDetailId,0 IsAsset,0 BudgetActive,0 BudgetMasterActivityActive
                    FROM (
	                    SELECT 'Vendor' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
		                    , NULL AS GLGeneralInfoId, NULL AS GLGeneralInfoCode, NULL AS GLGeneralInfoName
		                    , NULL AS BudgetMasterId, NULL AS BudgetCode, NULL AS BudgetName
		                    , NULL ActivityId, NULL AS ActivityCode, NULL AS ActivityName
		                    , NULL Dr, SUM(IRD.TotalMaterialBooksCurrencyAmount) + SUM(IRD.TotalTaxAmount)+ ISNULL(TCS.TCSAmount,0) AS  Cr
		                    , SUM(IRD.TotalMaterialBooksCurrencyAmount) + SUM(IRD.TotalTaxAmount)+  ISNULL(TCS.TCSAmount,0)  AS Amount,0 IsAsset
	                    FROM [TRN].[InventoryMaterial] AS IM
	                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
	                    LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
	                    JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
	                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.TaxAmount) AS TCSAmount
						from  [TRN].[InventoryReceiveAdditionalTax] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.InventoryReceiveId 
                        where InventoryReceiveId=@receiveId group by INS.InventoryReceiveId
						) AS TCS on TCS.InventoryReceiveId=@receiveId
	                    WHERE IRD.InventoryReceiveId=@receiveId
						group by TCS.TCSAmount
                    ) AS T
                    GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId";
                    return _sqlRepository.GetDataCollection(sql);

                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public GridModel GetPayableMaterial(GridParameter parameters, string inveReveiveId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"',@companyId varchar(10)='" + identity.CompanyId + @"',@plantId varchar(10)='" + identity.PlantId + @"'
                                        , @totalReceiveAmount DECIMAL(18, 4)=0
	                                  , @totalServiceAmount DECIMAL(18, 4)=0
	                                  , @totalSvcTaxAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
                        SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
                            , MGM.UserName AS MaterialGroupMasterName
                            , IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                            , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                            , ROUND(IRD.TotalMaterialTranAmount/IRD.TransactionQty,4) AS TransactionRate
                            , CU.Code AS CurrencyName, IR.ToCurrencyRate
                            , IRD.TotalMaterialTranAmount AS TrnAmount
                              , IRD.TotalMaterialBooksCurrencyAmount AS BaseAmount
							  ,IRD.TotalTaxAmount TaxAmount
	                        , IRD.ChargesTranAmount	 AS ChargesAmount	                      
	                        ,ISNULL(IRD.MaterialTranAmount,0) + ISNULL(IRD.ChargesTranAmount,0)	 AS TaxableAmount	                      
                            ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
	                        , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
	                        , IRD.CountryId
                             ,PID.TransactionQty AS POQty
                             ,ISNULL(Pre.OtherReceived,0) OtherReceived	                       
                            ,IRD.TransactionQty                         
							,(PID.TransactionQty-IRD.TransactionQty-ISNULL(Pre.OtherReceived,0)) AS Balance                   
					        ,IRD.TransactionUoMId
							,IRD.BaseUOMId 
                            ,MM.IsAsset  
							,HSNC.Code HSNCode
                            ,BudgetMasterId= CASE WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId ELSE MM.BudgetMasterId END
							,MM.ActivityId,FAMBT.FixedAssetMasterId,B.UserName BudgetName,FAM.UserName AS FixedAssetMasterName
                            ,MGPGL.BudgetMasterId VendorBudgetMasterId,E.UserName EntityName,MRM.EntityId
					  from TRN.InventoryMaterial AS IM
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                        LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id and ird.InventoryReceiveId='" + inveReveiveId + @"'
                        LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
						LEFT JOIN [TRN].[MaterialRequsitionMaster] MRM ON MRM.Id=PID.RequisitionId
						LEFT JOIN [ORG].[Entity] AS E ON E.Id= MRM.EntityId
                        LEFT JOIN (select PODetailsId,  Sum(TransactionQty) as OtherReceived 
						from trn.InventoryReceiveDetail where InventoryReceiveId not in('" + inveReveiveId + @"')
                        Group By PODetailsId) AS Pre on pre.PODetailsId=IRD.PODetailsId

                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN HKP.HSNCode AS HSNC ON HSNC.Id=MM.HSNCodeId
                        LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMBT ON FAMBT.BudgetMasterId=MM.BudgetMasterId
                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id=MM.BudgetMasterId
						LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                        LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMBT.FixedAssetMasterId
                        LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMM ON MGGL.InventoryBudgetMasterId= BMM.Id
						LEFT JOIN [HKP].[Budget] AS BBM ON BMM.BudgetId= BBM.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
                        LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GLP ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BMP ON MGPGL.BudgetMasterId= BMP.Id
						LEFT JOIN [HKP].[Budget] AS BP ON BMP.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS AP ON MGPGL.ActivityId= AP.Id
                        WHERE IRD.InventoryReceiveId=@inventoryReceiveId";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetOtherVendorChargesPayableData(string companyId, string plantId, string inveReveiveId,string otherPartyId, bool rcmApplicable)
        {
            try
            {
                var companyParty = GetCompanyPartyGroup(otherPartyId, plantId);
				var sql = "";
                if (rcmApplicable)
                {
					sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						, T.Cr, T.Amount, T.IsAsset--,T.InventoryReceiveDetailId
					FROM (
						SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId
							,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
							, SUM(IRD.AdditionalChargesAmount) AS Dr, NULL Cr
							, SUM(IRD.AdditionalChargesAmount ) AS Amount
                            ,MM.IsAsset--,IRD.Id AS  InventoryReceiveDetailId
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,FAG.AssetUnderConstructionGLId,GLF.AccountCode,GLF.UserName
						,FAG.AssetUnderConstructionBudgetMasterId,BF.Code,BF.UserName
						,FAG.AssetUnderConstructionActivityId,AF.Code,AF.UserName--,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset--, T.InventoryReceiveDetailId
                   
					UNION
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType,NULL MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRTS.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRTS.TaxAmount) AS Amount
                       --, NULL InventoryReceiveDetailId
						,0 IsAsset
					--, IRTS.TaxAmount
					FROM [TRN].[InventoryReceiveTax] AS IRTS
					LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryService] AS INS ON INS.Id=IRTS.InventoryServiceId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE INS.InventoryReceiveId=@receiveId --AND IR.IsNonCreditable=0 
					AND IRTS.InventoryServiceId<>''  and IsOtherVendor=1 AND TCGL.InputTaxOutPutTax='Input' AND TCGL.TaxType='RCM' AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY IRTS.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					
					UNION

					SELECT 'Tax' AS OtherName, 'Cr' AS TrnType,NULL MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.LiabilityGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.LiabilityBudgetMasterId BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.LiabilityActivityId ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, 0 AS  Dr, SUM(IRTS.TaxAmount) Cr
						, SUM(IRTS.TaxAmount) AS Amount
                       --, NULL InventoryReceiveDetailId
					,0 IsAsset
					--, IRTS.TaxAmount
					FROM [TRN].[InventoryReceiveTax] AS IRTS
					LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryService] AS INS ON INS.Id=IRTS.InventoryServiceId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.LiabilityGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.LiabilityBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.LiabilityActivityId= A.Id
					WHERE INS.InventoryReceiveId=@receiveId --AND IR.IsNonCreditable=0 
					AND IRTS.InventoryServiceId<>''  and IsOtherVendor=1 AND TCGL.InputTaxOutPutTax='Input' AND TCGL.TaxType='RCM' AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY IRTS.TaxCategoryId, TCGL.LiabilityGLId, GL.AccountCode, GL.UserName, TCGL.LiabilityBudgetMasterId, B.Code, B.UserName, TCGL.LiabilityActivityId, A.Code, A.UserName
					UNION
					SELECT T.OtherName, T.TrnType,NULL MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount,0 IsAsset --, NULL InventoryReceiveDetailId
                          
					FROM (
						SELECT 'Vendor' AS OtherName,'Cr' AS TrnType, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,NULL Dr
						,SUM(MAT.Cr)   AS Cr, 
						SUM(MAT.Cr)   AS Amount 
                        ,0 IsAsset
						FROM (
							SELECT IR.Id, 'Vendor' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
							 
                            ,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGPGL.GLGeneralInfoId  ELSE FAG.VendorReconGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGPGL.BudgetMasterId  ELSE FAG.VendorReconBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGPGL.ActivityId  ELSE FAG.VendorReconActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END

							, NULL Dr, SUM(IRD.AdditionalChargesAmount)    AS  Cr
							, SUM(IRD.AdditionalChargesAmount)  AS Amount,0 IsAsset
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						--JOIN [TRN].[InventoryService] AS INS ON IRD.InventoryReceiveId=INS.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.OtherPartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id

                        LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAVGL.VendorReconGLId ,FAVGL.VendorReconBudgetMasterId,FAVGL.VendorReconActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT 
						LEFT JOIN HKP.FixedAssetMasterVendorReconGL FAVGL ON 
						FAMBT.FixedAssetMasterId=FAVGL.FixedAssetMasterId  AND FAVGL.PartyAccountGroupId=@partyAccountGruopId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId

						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.VendorReconGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.VendorReconBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.VendorReconActivityId= AF.Id
						
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName
						) AS MAT
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.Amount) AS Amount,sum(INS.TotalTaxAmount) AS TotalTaxAmount
						from  [TRN].[InventoryService] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.InventoryReceiveId 
                        where InventoryReceiveId=@receiveId and IsOtherVendor=1 group by INS.InventoryReceiveId
						) AS SRV on SRV.InventoryReceiveId=MAT.Id
						
						GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName
					) AS T
					GROUP BY  T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode
					, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId, T.IsAsset";
                }
                else
                {
					 sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						, T.Cr, T.Amount, T.IsAsset--,T.InventoryReceiveDetailId
					FROM (
						SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId
							,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
							, SUM(IRD.AdditionalChargesAmount) AS Dr, NULL Cr
							, SUM(IRD.AdditionalChargesAmount ) AS Amount
                            ,MM.IsAsset--,IRD.Id AS  InventoryReceiveDetailId
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,FAG.AssetUnderConstructionGLId,GLF.AccountCode,GLF.UserName
						,FAG.AssetUnderConstructionBudgetMasterId,BF.Code,BF.UserName
						,FAG.AssetUnderConstructionActivityId,AF.Code,AF.UserName--,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset--, T.InventoryReceiveDetailId
                   
					UNION
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType,NULL MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRTS.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRTS.TaxAmount) AS Amount
                       , NULL InventoryReceiveDetailId
					--, IRTS.TaxAmount
					--, IRTS.TaxAmount
					FROM [TRN].[InventoryReceiveTax] AS IRTS
					LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryService] AS INS ON INS.Id=IRTS.InventoryServiceId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE INS.InventoryReceiveId=@receiveId --AND IR.IsNonCreditable=0 
					AND IRTS.InventoryServiceId<>''  and IsOtherVendor=1 AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY IRTS.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT T.OtherName, T.TrnType,NULL MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
						, T.Amount , NULL InventoryReceiveDetailId
                          
					FROM (
						SELECT 'Vendor' AS OtherName,'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,NULL Dr
						,SUM(MAT.Cr)   AS Cr, 
						SUM(MAT.Cr)   AS Amount 
                        ,0 IsAsset
						FROM (
							SELECT IR.Id, 'Vendor' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
							 
                            ,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGPGL.GLGeneralInfoId  ELSE FAG.VendorReconGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGPGL.BudgetMasterId  ELSE FAG.VendorReconBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGPGL.ActivityId  ELSE FAG.VendorReconActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END

							, NULL Dr, SUM(IRD.AdditionalChargesAmount) + SUM(IRD.AdditionalChargesTax)  AS  Cr
							, SUM(IRD.AdditionalChargesAmount) + SUM(IRD.AdditionalChargesTax)  AS Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						--JOIN [TRN].[InventoryService] AS INS ON IRD.InventoryReceiveId=INS.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.OtherPartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id

                        LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAVGL.VendorReconGLId ,FAVGL.VendorReconBudgetMasterId,FAVGL.VendorReconActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT 
						LEFT JOIN HKP.FixedAssetMasterVendorReconGL FAVGL ON 
						FAMBT.FixedAssetMasterId=FAVGL.FixedAssetMasterId  AND FAVGL.PartyAccountGroupId=@partyAccountGruopId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId

						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.VendorReconGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.VendorReconBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.VendorReconActivityId= AF.Id
						
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName
						) AS MAT
						
						GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId, T.IsAsset";
				}
                
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetGRNListForInvPayable(string plantId)
        {
            try
            {
                var sql = @"SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode,isnull(IR.PartyType,'Vendor')PartyType
								, P.UserName AS PartyName,IPP.GSTIN GSTINNo,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDateNew
			                    , CP.UserName AS PartyAccountGroupName
			                    , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                , Particular=CASE WHEN IR.EmployeeId<>'' THEN EI.EmployeeName WHEN IR.PartyId<>'' THEN P.UserName  ELSE P.UserName END
	                            , IR.MaterialStorageId, IR.DocRefNo, IR.DocDate
	                            , IR.GateEntryNo,PG.UserName GateEntryName, REPLACE(CONVERT(CHAR(11), GE.EntryDate, 106),' ','-') AS EntryDate
								, IR.CurrencyId, CU.Code AS CurrencyCode
								, IR.BaseCurrencyId,IR.PaymentTermId,IR.BaseOnDueDate,IR.BaseNoOfDays,IR.MatureDate
	                            , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo
								, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                             , IRD.TransactionQty,IRD.ShortageQty,IRD.ShortageValue, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName,PT.PaymentMode
								, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate, IR.ToCurrencyRate CompanyCurrencyRate
								,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END
								,IR.NoteForAccounts Narration
                                 ,GRNACC.PurchaseDocumentAcceptanceId AcceptanceId, REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AS AcceptanceDate
								, PDA.AcceptanceNo
								,IsFOC=CASE WHEN IR.IsFOC=1 THEN 'YES' ELSE 'NO' END
								,IR.GRNType,IR.OtherPartyId,IR.OtherPartyPlantId,OP.UserName OtherPartyName,IR.OtherPartyDocRefNo,IR.OtherPartyRCMApplicable,ISNULL(PLC.IsAccepptanceFirst,0) IsAccepptanceFirst
								,POId=	STUFF((select distinct ','+PO.Id from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,PODate=	STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,POVendorRefNo=	STUFF((select distinct ','+PO.DocRefNo from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,LCNo=	STUFF((select distinct ','+LC.LCRef from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN DBO.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								 ,PurchaseLCId=	STUFF((select distinct ','+LC.Id from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN DBO.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=	STUFF((select distinct ','+C.ContractNo from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														LEFT JOIN dbo.[Contract] C ON C.Id=LC.ContractId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								 ,CustomerName=	STUFF((select distinct ','+P.UserName from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														LEFT JOIN dbo.[Contract] C ON C.Id=LC.ContractId
														LEFT JOIN HKP.Party P ON P.Id=C.CustomerId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    FROM [TRN].[InventoryReceive] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [HKP].[Party] AS OP ON IR.OtherPartyId=OP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN [TRN].GateEntry GE ON GE.Id=IR.GateEntryNo
					LEFT JOIN dbo.PlantWiseGate PG ON PG.Id=GE.PlantWiseGateId
					LEFT JOIN TRN.GRNAcceptanceMap GRNACC ON GRNACC.GRNId=IR.Id
					LEFT JOIN TRN.PurchaseDocAcceptance PDA ON PDA.Id=GRNACC.PurchaseDocumentAcceptanceId
					LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
					
                     LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty,SUM(ISNULL(A.ShortageQty,0)) AS ShortageQty,SUM(ISNULL(A.ShortageValue,0)) AS ShortageValue, SUM(ROUND(A.TotalMaterialTranAmount,4)) AS TransactionAmount, SUM(ROUND(A.TotalMaterialBooksCurrencyAmount,0)) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                        JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                    WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.IsPaymentHold=0 AND IR.PlantId='" + plantId + @"' AND IR.FixedAssetOrInventory='Inventory' AND IR.OpeningBalanceId IS NULL 
					AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.AuthorizedByStatus='Approved' AND ISNULL(IR.IsFOC,0)=0 AND IR.GRNType!='MaterialTransfer'
                    order by IR.GRNDate desc";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetPostedInventoryTransferList(string column, string value, string plantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        select top 300 * from (SELECT IR.Id,IR.Id GRNNo, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
			                        , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                    , Particular=CASE WHEN IR.EmployeeId<>'' THEN EI.EmployeeName WHEN IR.PartyId<>'' THEN P.UserName  ELSE P.UserName END
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IR.InvoicingPartyPlantId PartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, IR.IsTaxApplicable
                                    , COUNT(*) OVER () AS TotalRows
									,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
									,IR.GateEntryNo,IR.POId,IR.ToCurrencyRate,IR.NoteForAccounts Narration
									,FromVoucherNo = CASE WHEN IR.EmployeeId <>'' THEN VE.VoucherNo ELSE V.VoucherNo END
									,ToVoucherNo = CASE WHEN IR.EmployeeId <>'' THEN VE.VoucherNo ELSE VT.VoucherNo END
									,Ir.VoucherId FromVoucherId,Ir.ToVoucherId
									,VoucherTypeId = CASE WHEN IR.EmployeeId <>'' THEN VE.VoucherTypeId ELSE V.VoucherTypeId END
									,PostingDate= CASE WHEN IR.EmployeeId <>'' THEN REPLACE(CONVERT(CHAR(11), VE.PostingDate, 106),' ','-') ELSE REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') END
                                    ,MS.UserName MaterialStorageName, IR.IsFOC
									,IR.PlantId FromPlantId,IR.ToPlantId,FP.UserName FromPlantName,TP.UserName ToPlantName
                                   
						FROM [TRN].[InventoryReceive] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId=@plantId GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId=@plantId GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN TRN.GRNAcceptanceMap IGD ON IGD.GRNId=IR.Id
                        LEFT JOIN TRN.Invoice IV ON IV.Id=IGD.InvoiceId
						LEFT JOIN TRN.Voucher V ON V.Id=IR.VoucherId
						LEFT JOIN TRN.Voucher VT ON VT.Id=IR.ToVoucherId
						LEFT JOIN TRN.EmployeePayable EP ON EP.InventoryReceiveId=IR.Id
						LEFT JOIN TRN.Voucher VE ON VE.Id=EP.VoucherId
                        LEFT JOIN HKP.MaterialStorage MS ON MS.Id=IR.MaterialStorageId
						LEFT JOIN ORG.Plant TP ON TP.Id=IR.ToPlantId
						LEFT JOIN ORG.Plant FP ON FP.Id=IR.PlantId
                        WHERE IR.PlantId=@plantId AND IR.[Status]='Posting' AND IR.IsPaymentHold=0 AND IR.PlantId=@plantId AND IR.FixedAssetOrInventory='Inventory' AND IR.GRNType='MaterialTransfer' AND IR.OpeningBalanceId IS NULL
						) AS TEMP WHERE " + strkey + " order by PostingDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetAdditionalTaxDetail(string additionalTaxId)
        {
            try
            {
                var sql = @"SELECT  GL.AccountCode+' - '+ GL.UserName GLName,BU.UserName BudgetName,A.UserName ActivityName,0 DrAmount,ATD.Amount CrAmount,ATD.AdditionalTaxId
                            ,ATD.GLGeneralInfoId, ATD.BudgetMasterId, ATD.ActivityId,TC.Id TaxCategoryId,ATD.TaxCodeId,TAC.UserName Particulars,ATD.AType
                            FROM TRN.AdditionalTaxDetail ATD 
                            JOIN TRN.AdditionalTax ATX ON ATX.Id=ATD.AdditionalTaxId
                            LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=ATD.GLGeneralInfoId
                            LEFT JOIN MST.BudgetMaster BM ON BM.Id=ATD.BudgetMasterId
                            LEFT JOIN HKP.Budget BU ON BU.Id=BM.BudgetId
                            LEFT JOIN HKP.Activity A ON A.Id=ATD.ActivityId
							LEFT JOIN MST.TaxCode TAC ON TAC.Id=ATD.TaxCodeId
							LEFT JOIN MST.TaxCategory TC ON TC.Id=TAC.TaxCategoryId
							WHERE ATX.Id='" + additionalTaxId + @"'

                            UNION
							SELECT  GL.AccountCode+' - '+ GL.UserName GLName,BU.UserName BudgetName,A.UserName ActivityName,ATX.TaxAmount DrAmount,0 CrAmount,ATX.Id AdditionalTaxId
							,IVD.GLGeneralInfoId, IVD.BudgetMasterId, IVD.ActivityId,NULL TaxCategoryId,NULL TaxCodeId,'' Particulars,'Dr' AType
                            FROM  TRN.AdditionalTax ATX 
							LEFT JOIN TRN.Invoice IV ON IV.Id=ATX.InvoiceId
							LEFT JOIN TRN.InvoiceDetail IVD ON IVD.InvoiceId=IV.Id
                            LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=IVD.GLGeneralInfoId
                            LEFT JOIN MST.BudgetMaster BM ON BM.Id=IVD.BudgetMasterId
                            LEFT JOIN HKP.Budget BU ON BU.Id=BM.BudgetId
                            LEFT JOIN HKP.Activity A ON A.Id=IVD.ActivityId
							WHERE ATX.Id='" + additionalTaxId + "' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

		public IEnumerable<object> GetShortageQtyDetail(string grnId,string financingTypeId)
		{
			try
			{
				var sql = @"Select * from (SELECT  'Material' AS OtherName,GL.AccountCode+' - '+ GL.UserName GLName,BU.UserName BudgetName,A.UserName ActivityName,0 DrAmount,IRD.ShortageValue CrAmount,IRD.ShortageValue Amount
                            ,IRD.PostDrGLGeneralInfoId GLGeneralInfoId,  IRD.PostDrBudgetMasterId BudgetMasterId, IRD.PostDrActivityId ActivityId,NULL TaxCategoryId,NULL TaxCodeId,'Cr' AType,'Cr' TrnType
                            FROM TRN.InventoryReceive IR 
                            JOIN (SELECT InventoryReceiveId,PostDrGLGeneralInfoId,PostDrBudgetMasterId,PostDrActivityId,SUM(ROUND(ISNULL(ShortageValue,0),2)) ShortageValue
									FROM TRN.InventoryReceiveDetail where ShortageQty>0 
									group by InventoryReceiveId,PostDrGLGeneralInfoId,PostDrBudgetMasterId,PostDrActivityId) IRD ON IRD.InventoryreceiveId=IR.Id
                            LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=IRD.PostDrGLGeneralInfoId
                            LEFT JOIN MST.BudgetMaster BM ON BM.Id=IRD.PostDrBudgetMasterId
                            LEFT JOIN HKP.Budget BU ON BU.Id=BM.BudgetId
                            LEFT JOIN HKP.Activity A ON A.Id=IRD.PostDrActivityId
							WHERE IR.Id='" + grnId + @"'

                            UNION ALL
							SELECT 'DebitNote' AS OtherName,GL.AccountCode+' - '+ GL.UserName GLName,BU.UserName BudgetName,A.UserName ActivityName,0 DrAmount,0 CrAmount,0 Amount
                            ,FGL.AssetGLId GLGeneralInfoId,  FGL.AssetBudgetMasterId BudgetMasterId, FGL.AssetActivityId ActivityId,NULL TaxCategoryId,NULL TaxCodeId,'Dr' AType,'Dr' TrnType
							FROM HKP.FinancingTypeGL FGL
							LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=FGL.AssetGLId
                            LEFT JOIN MST.BudgetMaster BM ON BM.Id=FGL.AssetBudgetMasterId
                            LEFT JOIN HKP.Budget BU ON BU.Id=BM.BudgetId
                            LEFT JOIN HKP.Activity A ON A.Id=FGL.AssetActivityId
							WHERE FGL.FinancingTypeId='" + financingTypeId + @"' 

							UNION ALL
							SELECT 'Tax' AS OtherName, GL.AccountCode+' - '+GL.UserName AS GLName, B.UserName AS BudgetName, A.UserName AS ActivityName
						,  0 DrAmount , SUM(ROUND((IRT.TaxAmount*ird.ShortageQty)/ird.TransactionQty,2)) AS  CrAmount,SUM(ROUND((IRT.TaxAmount*ird.ShortageQty)/ird.TransactionQty,2)) Amount, VD.GLGeneralInfoId AS GLGeneralInfoId, VD.BudgetMasterId, VD.ActivityId
						, IRT.TaxCategoryId,IT.TaxCodeId, 'Cr' AS AType, 'Cr' AS TrnType
                       
					FROM [TRN].[InventoryReceiveDetail] AS IRD
					LEFT JOIN [TRN].[InventoryReceiveTax] AS IRT ON IRT.InventoryReceiveDetailId=IRD.Id  
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN TRN.[VoucherDetail] VD ON VD.Id=IRT.DrVoucherDetailId
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
					WHERE IRD.InventoryReceiveId='" + grnId + @"'  AND IRT.InventoryReceiveDetailId<>''  AND IRD.ShortageQty>0
					GROUP BY  IRT.TaxCategoryId, VD.GLGeneralInfoId, GL.AccountCode, GL.UserName, VD.BudgetMasterId, B.Code, B.UserName,IT.TaxCodeId
					, VD.ActivityId, A.Code, A.UserName
					) x order by x.TrnType desc,x.OtherName ";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetPurchaseOrderDiscount(string plantId, string grnId)
        {
            try
            {
                var sql = @"SELECT DISTINCT pod.InventoryReceiveId POId,IRD.InventoryReceiveId,IR.Id GRNNo,MGM.UserName MaterialGroup,po.DiscountAmount,0 Amount
							,GL.UserName GLName,B.UserName BugetName,A.UserName AcitivityName,MGPGL.GLGeneralInfoId,MGPGL.BudgetMasterId,MGPGL.ActivityId
							FROM trn.InventoryReceiveDetail IRD 
							left join trn.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId
							left join trn.PurchaseOrderDetail POD ON POD.Id=IRD.PODetailsId
							LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=POD.InventoryReceiveId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=POD.InventoryMaterialId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=POD.ArticleId
							LEFT JOIN MST.MaterialGroupMaster  MGM ON MGM.Id=MM.MaterialGroupMasterId
							LEFT JOIN HKP.MaterialGroupGL MGGL ON MGGL.MaterialGroupMasterId=MGM.Id
							LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId='" + plantId + @"' AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
							LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
							LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId 
							AND MGPGL.PartyAccountGroupId= PACG.Id AND GLType='Payable'
							LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
							LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
							LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
							LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
							where IR.Id='" + grnId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetPurchaseOrderDiscountWithAcceptance(string plantId, string purchaseDocAcceptanceId)
        {
            try
            {
                var sql = @"SELECT DISTINCT pod.InventoryReceiveId POId,pdad.PurchaseDocAcceptanceId,pda.AcceptanceNo,MGM.UserName MaterialGroup,po.DiscountAmount,0 Amount
						,GL.UserName GLName,B.UserName BugetName,A.UserName AcitivityName
						FROM trn.PurchaseDocAcceptanceDetail pdad 
						left join trn.PurchaseDocAcceptance pda on pda.Id=pdad.PurchaseDocAcceptanceId
						left join trn.PurchaseOrderDetail pod on pod.Id=pdad.PODetailId join trn.PurchaseOrder po on po.Id=pod.InventoryReceiveId
						LEFT JOIN MST.MaterialMaster MM ON MM.Id=POD.InventoryMaterialId
						LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=POD.ArticleId
						LEFT JOIN MST.MaterialGroupMaster  MGM ON MGM.Id=MM.MaterialGroupMasterId
						LEFT JOIN HKP.MaterialGroupGL MGGL ON MGGL.MaterialGroupMasterId=MGM.Id
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId='" + plantId + @"' AND PartyType='Vendor')AS CP ON pda.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId 
						AND MGPGL.PartyAccountGroupId= PACG.Id AND GLType='Payable'
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
						where pda.Id='" + purchaseDocAcceptanceId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        private Dictionary<string, object> GetCompanyParty(string partyId, string plantId)
        {
            var cmdText = @"select PartyAccountGroupId from hkp.CompanyParty where PartyId='" + partyId + "' AND Plantid='" + plantId + "'";
            return _sqlRepository.GetData(cmdText);
        }
        public IEnumerable<object> GetInventoryMaterialReceivableData(string companyId, string plantId, string inveReveiveId, string partyId, string taxapplicable)
        {
            try
            {
                var companyParty = GetCompanyParty(partyId, plantId);
                var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT  'Inventory' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId
							,ISH.PostDrGLGeneralInfoId GLGeneralInfoId , GL.AccountCode GLGeneralInfoCode ,GL.UserName GLGeneralInfoName
							,ISH.PostDrBudgetMasterId BudgetMasterId ,B.Code BudgetCode  ,B.UserName BudgetName
							,ISH.PostDrActivityId ActivityId ,A.Code ActivityCode ,A.UserName  ActivityName 
							
							, 0 Dr, SUM(ISH.GRNRate * ISH.Qty) AS Cr
							, SUM(ISH.GRNRate * ISH.Qty) AS Amount
                            
						FROM [TRN].[InventorySalesDetail] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN (
								SELECT SDH.InventorySalesDetailId,SUM(RD.MaterialTranRate) GRNRate,SUM(SDH.Qty) Qty
								,RD.PostDrGLGeneralInfoId ,RD.PostDrBudgetMasterId,RD.PostDrActivityId
								FROM TRN.InventorySalesDetail SD 
								JOIN TRN.InventorySalesHistory SDH ON SDH.InventorySalesDetailId=SD.Id
								LEFT JOIN TRN.InventoryReceiveDetail RD ON RD.Id=SDH.InventoryReceiveDetailId
								GROUP BY SDH.InventorySalesDetailId,RD.PostDrGLGeneralInfoId ,RD.PostDrBudgetMasterId,RD.PostDrActivityId
								 ) ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						
						LEFT JOIN[MST].[BudgetMaster] AS BM ON ISH.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Activity] AS A ON ISH.PostDrActivityId= A.Id					

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY ISH.PostDrGLGeneralInfoId,ISH.PostDrBudgetMasterId,ISH.PostDrActivityId,GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,ISD.Id

						UNION		
						
						
                            SELECT  'A/R' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId

                            ,MGPGL.GLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,MGPGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGPGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName

							, SUM(ISD.SalesRate*ISH.Qty)   AS  Dr, 0 Cr
							, SUM(ISD.SalesRate*ISH.Qty)  AS Amount
						FROM [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						--JOIN [TRN].[InventoryService] AS INS ON ISD.InventoryReceiveId=INS.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON IR.CustomerId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id AND GLType='Receivable'
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id

                        LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAVGL.VendorReconGLId ,FAVGL.VendorReconBudgetMasterId,FAVGL.VendorReconActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT 
						LEFT JOIN HKP.FixedAssetMasterVendorReconGL FAVGL ON 
						FAMBT.FixedAssetMasterId=FAVGL.FixedAssetMasterId  AND FAVGL.PartyAccountGroupId=@partyAccountGruopId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId

						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.VendorReconGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.VendorReconBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.VendorReconActivityId= AF.Id

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY  IR.Id, MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName
                    

						union
                        SELECT  OtherName=case when (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 then 'Gain on Sales'  
											when (SUM(ird.MaterialTranRate)-SUM(ISD.SalesRate))*SUM(ISH.Qty)>0 then 'Loss on Sales'
											  end
							,TrnType=case when (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 then 'Cr'  
											when (SUM(ird.MaterialTranRate)-SUM(ISD.SalesRate))*SUM(ISH.Qty)>0 then 'Dr'
											  end
											  , NULL MaterialGroupMasterId
							,GLGeneralInfoId=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN GAD.GLGeneralInfoId ELSE GADL.GLGeneralInfoId END
							,GLGeneralInfoCode=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN GL.AccountCode  ELSE GLL.AccountCode END
							,GLGeneralInfoName=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN GL.UserName   ELSE GLL.UserName  END
							,BudgetMasterId=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN GAD.BudgetMasterId   ELSE GADL.BudgetMasterId  END
							,BudgetCode=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN B.Code   ELSE BL.Code  END
							,BudgetName=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN B.UserName   ELSE BL.UserName  END
							, ActivityId=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN GAD.ActivityId  ELSE GADL.ActivityId END
							,ActivityCode=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN A.Code   ELSE AL.Code  END
							,ActivityName=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN A.UserName   ELSE AL.UserName  END
							
							, Dr=CASE WHEN (SUM(ird.MaterialTranRate)-SUM(ISD.SalesRate))*SUM(ISH.Qty)>0 THEN (SUM(ird.MaterialTranRate)-SUM(ISD.SalesRate))*SUM(ISH.Qty) ELSE 0 END
							, Cr=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty) ELSE 0 END
							, Amount=CASE WHEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty)>0 THEN (SUM(ISD.SalesRate)-SUM(ird.MaterialTranRate))*SUM(ISH.Qty) ELSE (SUM(ird.MaterialTranRate)-SUM(ISD.SalesRate))*SUM(ISH.Qty) END
						FROM [TRN].[InventorySalesDetail] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN ORG.Company C ON C.Id=IR.CompanyId
						LEFT JOIN [HKP].[GeneralAccountDeterminate] GAD ON C.COAId=GAD.COAId and GAD.Id='GainOnInventorySales'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON GAD.BudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id

						LEFT JOIN [HKP].[GeneralAccountDeterminate] GADL ON GADL.COAId=C.COAId and GADL.Id='LossOnInventorySales'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLL ON GADL.GLGeneralInfoId=GLL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMFL ON GADL.BudgetMasterId= BMFL.Id
						LEFT JOIN [HKP].[Budget] AS BL ON BMFL.BudgetId= BL.Id
						LEFT JOIN [HKP].[Activity] AS AL ON GADL.ActivityId= AL.Id

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY ISD.Id,ird.Id,GAD.GLGeneralInfoId ,GL.AccountCode  ,GL.UserName  ,GAD.BudgetMasterId 
							,B.Code  ,B.UserName  ,GAD.ActivityId  ,A.Code  ,A.UserName ,GADL.GLGeneralInfoId 
							,GLL.AccountCode  ,GLL.UserName  ,GADL.BudgetMasterId  ,BL.Code  ,BL.UserName  ,GADL.ActivityId 
							,AL.Code  ,AL.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetGRNListForTransferJournal(string plantId)
        {
            try
            {
                var sql = @"SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId,IR.FromPlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode
								, P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDateNew
			                    , CP.UserName AS PartyAccountGroupName
			                    , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                , Particular=CASE WHEN IR.EmployeeId<>'' THEN EI.EmployeeName WHEN IR.PartyId<>'' THEN P.UserName  ELSE P.UserName END
	                            , IR.MaterialStorageId, IR.DocRefNo, IR.DocDate
	                            , IR.GateEntryNo,PG.UserName GateEntryName, REPLACE(CONVERT(CHAR(11), GE.EntryDate, 106),' ','-') AS EntryDate
								, IR.CurrencyId, CU.Code AS CurrencyCode
								, IR.BaseCurrencyId
                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
	                            , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo
								, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName
								, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate
								,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END,IR.NoteForAccounts Narration
                                ,IR.PurchaseDocumentAcceptanceId AcceptanceId, REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AS AcceptanceDate
								, PDA.AcceptanceNo,PDA.PurchaseLCId,PLC.ContractId,IsFOC=CASE WHEN IR.IsFOC=1 THEN 'YES' ELSE 'NO' END
								,IR.PlantId,FPL.UserName FromPlantName,TPL.UserName ToPlantName,IR.ToPlantId
                    FROM [TRN].[InventoryReceive] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN [TRN].GateEntry GE ON GE.Id=IR.GateEntryNo
					LEFT JOIN dbo.PlantWiseGate PG ON PG.Id=GE.PlantWiseGateId
					LEFT JOIN TRN.PurchaseDocAcceptance PDA ON PDA.Id=IR.PurchaseDocumentAcceptanceId
					LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
					LEFT JOIN ORG.Plant FPL ON FPL.Id=IR.PlantId
					LEFT JOIN ORG.Plant TPL ON TPL.Id=IR.ToPlantId
					LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(ROUND(A.TotalMaterialTranAmount,4)) AS TransactionAmount, SUM(ROUND(A.TotalMaterialBooksCurrencyAmount,0)) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                        JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='20181' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                        WHERE B.PlantId='20181' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                    WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.IsPaymentHold=0 AND IR.PlantId='" + plantId + @"' 
					AND IR.FixedAssetOrInventory='Inventory' AND IR.OpeningBalanceId IS NULL 
				AND IR.RequiredPosting=1 AND IR.GRNType='MaterialTransfer' AND IR.PlantId!=IR.ToPlantId
                    order by IR.GRNDate desc";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetFromPlantInventoryTransferPayable(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var inventoryReceiveData = GetInventoryReceive(inveReveiveId);

                var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId
					FROM (
						SELECT  'Material' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId

							,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
							, NULL Dr, SUM(IRD.TotalMaterialTranAmount - IRD.ShortageValue) AS Cr
							, SUM(IRD.TotalMaterialTranAmount - IRD.ShortageValue) AS Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,FAG.AssetUnderConstructionGLId,GLF.AccountCode,GLF.UserName
						,FAG.AssetUnderConstructionBudgetMasterId,BF.Code,BF.UserName
						,FAG.AssetUnderConstructionActivityId,AF.Code,AF.UserName,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					
                  
                    UNION
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[InventoryReceiveTax] AS IRT
					LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
							AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRD.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IRT.InventoryReceiveDetailId<>'' AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY  IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT 'Svc' AS OtherName, 'Dr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRTS.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRTS.TaxAmount) AS Amount
                       ,0 IsAsset, NULL InventoryReceiveDetailId
					--, IRTS.TaxAmount
					--, IRTS.TaxAmount
					FROM [TRN].[InventoryReceiveTax] AS IRTS
					LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryService] AS INS ON INS.Id=IRTS.InventoryServiceId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE INS.InventoryReceiveId=@receiveId --AND IR.IsNonCreditable=0 
					AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY IRTS.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
                           
						, T.Amount ,T.IsAsset, NULL InventoryReceiveDetailId
                           
					FROM (
						SELECT 'Transfer' AS OtherName,'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,SUM(MAT.Dr) Dr
						,NULL  AS Cr,
						SUM(MAT.Dr)  AS Amount
                        ,0 IsAsset
						FROM (
							SELECT IR.Id, 'Transfer' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						
                            ,GAD.GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,GAD.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,GAD.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName

							, SUM(IRD.TotalMaterialTranAmount) + SUM(IRD.TotalTaxAmount)+ SUM(IRD.ChargesTaxTranAmount)  AS  Dr, NULL Cr
							, SUM(IRD.TotalMaterialTranAmount) + SUM(IRD.TotalTaxAmount)+ SUM(IRD.ChargesTaxTranAmount) AS Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						--JOIN [TRN].[InventoryService] AS INS ON IRD.InventoryReceiveId=INS.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN ORG.Company C ON C.Id=IR.CompanyId
						LEFT JOIN HKP.GeneralAccountDeterminate GAD ON GAD.COAId=C.COAId AND GAD.Id='InventoryTransfer'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id

						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY  IR.Id, GAD.GLGeneralInfoId, GL.AccountCode, GL.UserName, GAD.BudgetMasterId, B.Code, B.UserName, GAD.ActivityId, A.Code, A.UserName
						,MM.IsAsset
						) AS MAT
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.Amount) AS Amount,sum(INS.TotalTaxAmount) AS TotalTaxAmount
						from  [TRN].[InventoryService] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.InventoryReceiveId 
                        where InventoryReceiveId=@receiveId group by INS.InventoryReceiveId
						) AS SRV on SRV.InventoryReceiveId=MAT.Id
						GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId, T.IsAsset
					
					
					ORDER BY T.TrnType DESC ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetToPlantInventoryTransferPayable(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var inventoryReceiveData = GetInventoryReceive(inveReveiveId);

                var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId
					FROM (
						SELECT  'Material' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId

							,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
							, SUM(IRD.TotalMaterialTranAmount - IRD.ShortageValue) AS Dr, NULL Cr
							, SUM(IRD.TotalMaterialTranAmount - IRD.ShortageValue) AS Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id
						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,FAG.AssetUnderConstructionGLId,GLF.AccountCode,GLF.UserName
						,FAG.AssetUnderConstructionBudgetMasterId,BF.Code,BF.UserName
						,FAG.AssetUnderConstructionActivityId,AF.Code,AF.UserName,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					
                  
                    UNION
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[InventoryReceiveTax] AS IRT
					LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
							AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRD.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IRT.InventoryReceiveDetailId<>'' AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY  IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT 'Svc' AS OtherName, 'Dr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRTS.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRTS.TaxAmount) AS Amount
                       ,0 IsAsset, NULL InventoryReceiveDetailId
					--, IRTS.TaxAmount
					--, IRTS.TaxAmount
					FROM [TRN].[InventoryReceiveTax] AS IRTS
					LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryService] AS INS ON INS.Id=IRTS.InventoryServiceId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE INS.InventoryReceiveId=@receiveId --AND IR.IsNonCreditable=0 
					AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY IRTS.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
                           
						, T.Amount ,T.IsAsset, NULL InventoryReceiveDetailId
                           
					FROM (
						SELECT 'Transfer' AS OtherName,'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,NULL Dr
						,SUM(MAT.Cr)  AS Cr,
						SUM(MAT.Cr)  AS Amount
                        ,0 IsAsset
						FROM (
							SELECT IR.Id, 'Transfer' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						
                            ,GAD.GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,GAD.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,GAD.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName

							, NULL Dr, SUM(IRD.TotalMaterialTranAmount) + SUM(IRD.TotalTaxAmount)+ SUM(IRD.ChargesTaxTranAmount)  AS  Cr
							, SUM(IRD.TotalMaterialTranAmount) + SUM(IRD.TotalTaxAmount)+ SUM(IRD.ChargesTaxTranAmount) AS Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						--JOIN [TRN].[InventoryService] AS INS ON IRD.InventoryReceiveId=INS.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN ORG.Company C ON C.Id=IR.CompanyId
						LEFT JOIN HKP.GeneralAccountDeterminate GAD ON GAD.COAId=C.COAId AND GAD.Id='InventoryTransfer'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id

						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY  IR.Id, GAD.GLGeneralInfoId, GL.AccountCode, GL.UserName, GAD.BudgetMasterId, B.Code, B.UserName, GAD.ActivityId, A.Code, A.UserName
						,MM.IsAsset
						) AS MAT
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.Amount) AS Amount,sum(INS.TotalTaxAmount) AS TotalTaxAmount
						from  [TRN].[InventoryService] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.InventoryReceiveId 
                        where InventoryReceiveId=@receiveId group by INS.InventoryReceiveId
						) AS SRV on SRV.InventoryReceiveId=MAT.Id
						GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId, T.IsAsset
					
					
					ORDER BY T.TrnType DESC ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetTransferVendorPayableGLBudgetActivity(string receiveId, string companyId, string plantId, string partyId)
        {
            var companyParty = GetCompanyParty(partyId, plantId);
            var sql = @"DECLARE @receiveId varchar(10)='" + receiveId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + "', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)

                            SELECT distinct IR.Id,IRD.Id AS InventoryReceiveDetailId, 'Vendor' AS OtherName, 'Cr' AS TrnType ,MM.MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGPGL.GLGeneralInfoId  ELSE FAG.VendorReconGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGPGL.BudgetMasterId  ELSE FAG.VendorReconBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGPGL.ActivityId  ELSE FAG.VendorReconActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id

                        LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAVGL.VendorReconGLId ,FAVGL.VendorReconBudgetMasterId,FAVGL.VendorReconActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT 
						LEFT JOIN HKP.FixedAssetMasterVendorReconGL FAVGL ON 
						FAMBT.FixedAssetMasterId=FAVGL.FixedAssetMasterId  AND FAVGL.PartyAccountGroupId=@partyAccountGruopId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId

						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.VendorReconGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.VendorReconBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.VendorReconActivityId= AF.Id

						WHERE IRD.InventoryReceiveId=@receiveId";
            return _sqlRepository.GetDataCollection(sql);
        }

        public GridModel GetPurchaseReturnPostedData(GridParameter parameters, string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            parameters.CmdText = @"SELECT V.VoucherNo, A.Id, A.Id AS AdjustmentNoteId, PR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, PR.InvoicingPartyPlantId PartyPlantId, PP.UserName AS PartyPlantName, V.Id VoucherId, V.PostingDate, V.DocDate
                                , V.DocRefNo, V.CurrencyId, C.Code AS CurrencyCode, VD.Amount, V.IsPark,PR.Id PurchaseReturnNo
								,IsDebitNote=case when PR.IsDebitNote=1 then 'True'  ELSE '' END
                                FROM  [TRN].[Voucher] AS V
								LEFT JOIN TRN.PurchaseReturn PR ON PR.VoucherId=V.Id
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=PR.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=PR.InvoicingPartyPlantId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
								LEFT JOIN (select SUM(DrAmount)Amount,VoucherId from trn.VoucherDetail where DrAmount>0 GROUP BY VoucherId) VD ON VD.VoucherId=V.Id
                                LEFT JOIN [TRN].[AdjustmentNote] AS A ON V.Id=A.VoucherId
                                WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "'AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        public IEnumerable<object> GetPurchaseReturnPostableData(string plantId)
        {
            try
            {
                var sql = @"SELECT PR.Id ,IR.Id GRNNo, REPLACE(CONVERT(CHAR(11), PR.POReturnDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode
								, P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), PR.POReturnDate, 106),' ','-') AS GRNDateNew
			                    , CP.UserName AS PartyAccountGroupName
			                    , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                , Particular=CASE WHEN IR.EmployeeId<>'' THEN EI.EmployeeName WHEN IR.PartyId<>'' THEN P.UserName  ELSE P.UserName END
	                            , IR.MaterialStorageId, PR.DocRefNo, PR.DocDate
	                            , IR.GateEntryNo,PG.UserName GateEntryName, REPLACE(CONVERT(CHAR(11), GE.EntryDate, 106),' ','-') AS EntryDate
								, IR.CurrencyId, CU.Code AS CurrencyCode
								, IR.BaseCurrencyId
	                            , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo
								, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , IRD.TransactionQty--, TU.TransactionUoMId, UoM.UserName AS TransactionUoM
								, IRD.TransactionAmount, IRD.BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName
								, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate
								,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END,IR.NoteForAccounts Narration
                                ,IR.PurchaseDocumentAcceptanceId AcceptanceId
								,IR.GRNType
                    FROM TRN.PurchaseReturn PR
					LEFT JOIN  [TRN].[InventoryReceive] AS IR ON PR.InventoryReceiveId=IR.Id
					LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN [TRN].GateEntry GE ON GE.Id=IR.GateEntryNo
					LEFT JOIN dbo.PlantWiseGate PG ON PG.Id=GE.PlantWiseGateId
					LEFT JOIN (SELECT   A.PurchaseReturnId,SUM(A.TransactionQty) AS TransactionQty, SUM(ROUND(A.TotalMaterialTranAmount,4)) AS TransactionAmount
					, SUM(ROUND(A.TotalMaterialBooksCurrencyAmount,0)) AS BaseAmount 
					 FROM [TRN].[PurchaseReturnDetail] AS A
		             JOIN [TRN].[PurchaseReturn] AS B ON A.PurchaseReturnId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.PurchaseReturnId) AS IRD ON IRD.PurchaseReturnId=PR.Id
                  
                    WHERE PR.PlantId='" + plantId + @"' AND ISNULL(PR.[Status],'')<>'Posting' AND PR.VoucherId IS NULL  AND PR.FixedAssetOrInventory='Inventory' AND PR.OpeningBalanceId IS NULL 
					AND PR.IsApproved=1 AND IR.VoucherId<>''
                    order by IR.GRNDate desc";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public GridModel GetPurchaseReturnMaterial(GridParameter parameters, string companyId, string plantId, string purchaseReturnId)
        {
            try
            {

                parameters.CmdText = @"DECLARE @purchaseReturnId VARCHAR(10)='" + purchaseReturnId + "',@companyId varchar(10)='" + companyId + "',@plantId varchar(10)='" + plantId + @"'
                                        , @totalReceiveAmount DECIMAL(18, 4)=0
	                                  , @totalServiceAmount DECIMAL(18, 4)=0
	                                  , @totalSvcTaxAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@purchaseReturnId)
                        SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@purchaseReturnId)
                        SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@purchaseReturnId AND InventoryServiceId<>'')
                        SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
                            , MGM.UserName AS MaterialGroupMasterName
                            , IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                            , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                            , IRD.MaterialTranRate AS TransactionRate
                            , CU.Code AS CurrencyName, IR.ToCurrencyRate
                            , IRD.TotalMaterialTranAmount AS TrnAmount
                              , IRD.TotalMaterialBooksCurrencyAmount AS BaseAmount
							  ,IRD.TotalTaxAmount TaxAmount
	                        , IRD.ChargesTranAmount	 AS ChargesAmount	                      
	                        ,ISNULL(IRD.MaterialTranAmount,0) + ISNULL(IRD.ChargesTranAmount,0)	 AS TaxableAmount	                      
                            ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
	                        , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
	                        , IRD.CountryId
                            ,IRD.TransactionQty                         
					        ,IRD.TransactionUoMId
							,IRD.BaseUOMId 
                            ,MM.IsAsset  
							,HSNC.Code HSNCode
                            ,BudgetMasterId= CASE WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId ELSE MM.BudgetMasterId END
							,MM.ActivityId,FAMBT.FixedAssetMasterId,B.UserName BudgetName,FAM.UserName AS FixedAssetMasterName
                            ,MGPGL.BudgetMasterId VendorBudgetMasterId
					  from TRN.InventoryMaterial AS IM
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                        LEFT JOIN [TRN].PurchaseReturnDetail AS IRD ON IRD.InventoryMaterialId=IM.Id 

                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN HKP.HSNCode AS HSNC ON HSNC.Id=MM.HSNCodeId
                        LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMBT ON FAMBT.BudgetMasterId=MM.BudgetMasterId
                        LEFT JOIN MST.BudgetMaster AS BM ON BM.Id=MM.BudgetMasterId
						LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                        LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMBT.FixedAssetMasterId
                        LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMM ON MGGL.InventoryBudgetMasterId= BMM.Id
						LEFT JOIN [HKP].[Budget] AS BBM ON BMM.BudgetId= BBM.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
                        LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GLP ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BMP ON MGPGL.BudgetMasterId= BMP.Id
						LEFT JOIN [HKP].[Budget] AS BP ON BMP.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS AP ON MGPGL.ActivityId= AP.Id
                        WHERE IRD.PurchaseReturnId=@purchaseReturnId";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public GridModel GetPurchaseReturnService(GridParameter parameters, string companyId, string plantId, string purchaseReturnId)
        {
            try
            {

                parameters.CmdText = @"SELECT A.Id
                        , A.PurchaseReturnId
                        , A.ServiceMasterId
                        , B.UserName AS ServiceMasterName
                         ,A.Amount,A.Amount GRNServiceAmount
                        , POT.Amount-A.Amount AS  Bal
                        , POT.Amount As POAmount
                        --, A.TotalTaxAmount
                        ,A.POID
						,A.POServiceId,IRT.TaxAmount TotalTaxAmount
                        FROM [TRN].[PurchaseReturnService] AS A 
                        JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                        left JOIN (select Id, Amount from TRN.POService) AS POT on A.POServiceId=POT.Id
                        left join ( Select PurchaseReturnId, sum(TaxAmount) TaxAmount from  trn.PurchaseReturnTax group by PurchaseReturnId) IRT On IRT.PurchaseReturnId=A.Id
                     
                        WHERE A.PurchaseReturnId='" + purchaseReturnId + "'";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetPurchaseReturnMaterialPayable(string companyId, string plantId, string purchaseReturnId, bool isDebitNote)
        {
            try
            {
                var purchaseReturnData = GetPurchaseReturn(purchaseReturnId);
                var companyParty = GetCompanyPartyGroup(purchaseReturnData["PartyId"].ToString(), plantId);
				if (isDebitNote == true)
				{
					if (Convert.ToBoolean(purchaseReturnData["IsNonCreditable"].ToString()))
					{
						var sql = @"DECLARE @receiveId varchar(10)='" + purchaseReturnId + "', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + "', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId
					FROM (
						SELECT  'Material' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId

							,  IRD.PostDrGLGeneralInfoId  GLGeneralInfoId
							, GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,IRD.PostDrBudgetMasterId BudgetMasterId 
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,IRD.PostDrActivityId ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							, NULL Dr
							, SUM(PRD.TotalMaterialTranAmount) AS Cr
							, SUM(PRD.TotalMaterialTranAmount) AS Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId
						FROM TRN.PurchaseReturnDetail PRD
						JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=PRD.InventoryReceiveDetailId
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, IRD.PostDrGLGeneralInfoId, GL.AccountCode, GL.UserName, IRD.PostDrBudgetMasterId, B.Code, B.UserName, IRD.PostDrActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					UNION
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId
					FROM (
						SELECT  'Return' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,NULL FixedAssetMasterId

							,MGGL.DebitNoteGLId GLGeneralInfoId
							,GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,MGGL.DebitNoteBudgetMasterId BudgetMasterId 
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,MGGL.DebitNoteActivityId ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							
							, SUM(PRD.TotalMaterialTranAmount+ISNULL(PRD.TotalTaxAmount,0)+ISNULL(PRD.ChargesTaxTranAmount,0)) AS Dr, NULL Cr
							, SUM(PRD.TotalMaterialTranAmount+ISNULL(PRD.TotalTaxAmount,0)+ISNULL(PRD.ChargesTaxTranAmount,0) ) AS Amount
                            ,MM.IsAsset , NULL InventoryReceiveDetailId,PRD.PurchaseReturnId
						FROM TRN.PurchaseReturnDetail PRD
						JOIN [TRN].[InventoryReceive] AS IR ON IR.Id=PRD.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON PRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						--LEFT JOIN (SELECT VoucherId, GLGeneralInfoId,	BudgetMasterId,	ActivityId,InvoiceDetailId FROM trn.VoucherDetail 
						--					WHERE InvoiceDetailId IS NOT NULL GROUP BY VoucherId, GLGeneralInfoId,	BudgetMasterId,	ActivityId,InvoiceDetailId) AS VD ON VD.VoucherId=IR.VoucherId
						--LEFT JOIN [TRN].[InvoiceDetail] AS IVD ON IVD.Id=VD.InvoiceDetailId
						--LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
						--LEFT JOIN[MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
						--LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						--LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.DebitNoteGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.DebitNoteBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.DebitNoteActivityId= A.Id
						
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.DebitNoteGLId, GL.AccountCode, GL.UserName, MGGL.DebitNoteBudgetMasterId, B.Code, B.UserName
						, MGGL.DebitNoteActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId ,PRD.PurchaseReturnId
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					
                    ORDER BY T.TrnType DESC";
						return _sqlRepository.GetDataCollection(sql);
					}
					else
					{
						var sql = @"DECLARE @receiveId varchar(10)='" + purchaseReturnId + "', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + "', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId
					FROM (
						SELECT  'Material' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId

							,  IRD.PostDrGLGeneralInfoId  GLGeneralInfoId
							, GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,IRD.PostDrBudgetMasterId BudgetMasterId 
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,IRD.PostDrActivityId ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							, NULL Dr
							, SUM(PRD.TotalMaterialTranAmount) AS Cr
							, SUM(PRD.TotalMaterialTranAmount) AS Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId
						FROM TRN.PurchaseReturnDetail PRD
						JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=PRD.InventoryReceiveDetailId
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, IRD.PostDrGLGeneralInfoId, GL.AccountCode, GL.UserName, IRD.PostDrBudgetMasterId, B.Code, B.UserName, IRD.PostDrActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					UNION
					SELECT R.OtherName, R.TrnType, R.MaterialGroupMasterId, R.TaxCategoryId
						, R.GLGeneralInfoId, R.GLGeneralInfoCode, R.GLGeneralInfoName
						, R.BudgetMasterId, R.BudgetCode, R.BudgetName
						, R.ActivityId, R.ActivityCode,R.ActivityName
						, R.Dr
						
						, R.Cr, R.Amount, R.IsAsset,R.InventoryReceiveDetailId
					
					FROM (
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr+ISNULL(TCS.TCSAmount,0) Dr
						
						, T.Cr, T.Amount+ISNULL(TCS.TCSAmount,0) Amount, T.IsAsset,T.InventoryReceiveDetailId,T.PurchaseReturnId
					FROM (
						SELECT  'Return' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,NULL FixedAssetMasterId

							,MGGL.DebitNoteGLId GLGeneralInfoId
							,GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,MGGL.DebitNoteBudgetMasterId BudgetMasterId 
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,MGGL.DebitNoteActivityId ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							
							, SUM(PRD.TotalMaterialTranAmount+ISNULL(PRD.TotalTaxAmount,0)+ISNULL(PRD.ChargesTaxTranAmount,0)) AS Dr, NULL Cr
							, SUM(PRD.TotalMaterialTranAmount+ISNULL(PRD.TotalTaxAmount,0)+ISNULL(PRD.ChargesTaxTranAmount,0) ) AS Amount
                            ,MM.IsAsset , NULL InventoryReceiveDetailId,PRD.PurchaseReturnId
						FROM TRN.PurchaseReturnDetail PRD
						JOIN [TRN].[InventoryReceive] AS IR ON IR.Id=PRD.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON PRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						--LEFT JOIN (SELECT VoucherId, GLGeneralInfoId,	BudgetMasterId,	ActivityId,InvoiceDetailId FROM trn.VoucherDetail 
						--					WHERE InvoiceDetailId IS NOT NULL GROUP BY VoucherId, GLGeneralInfoId,	BudgetMasterId,	ActivityId,InvoiceDetailId) AS VD ON VD.VoucherId=IR.VoucherId
						--LEFT JOIN [TRN].[InvoiceDetail] AS IVD ON IVD.Id=VD.InvoiceDetailId
						--LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
						--LEFT JOIN[MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
						--LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						--LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.DebitNoteGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.DebitNoteBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.DebitNoteActivityId= A.Id
						
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.DebitNoteGLId, GL.AccountCode, GL.UserName, MGGL.DebitNoteBudgetMasterId, B.Code, B.UserName
						, MGGL.DebitNoteActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId ,PRD.PurchaseReturnId
                    ) AS T

					LEFT OUTER JOIN (
						SELECT INS.PurchaseReturnId, sum(INS.TaxAmount) AS TCSAmount
						from  [TRN].[PurchaseReturnAdditionalTax] AS INS
						LEFT JOIN TRN.PurchaseReturn AS IR ON IR.Id=INS.PurchaseReturnId 
                        where PurchaseReturnId=@receiveId group by INS.PurchaseReturnId
						) AS TCS on TCS.PurchaseReturnId=T.PurchaseReturnId

					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount,TCS.TCSAmount,T.PurchaseReturnId, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					)
					R 
					GROUP BY R.MaterialGroupMasterId, R.GLGeneralInfoId, R.GLGeneralInfoCode, R.GLGeneralInfoName, R.BudgetMasterId, R.BudgetCode, R.BudgetName, R.ActivityId
                    , R.ActivityCode, R.ActivityName, R.Dr, R.Cr, R.Amount, R.OtherName, R.TrnType,R.TaxCategoryId,R.IsAsset, R.InventoryReceiveDetailId
					UNION
					SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr
						, SUM(IRT.TaxAmount) AS  Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[PurchaseReturnTax] AS IRT
					LEFT JOIN [TRN].[PurchaseReturnDetail] AS IRD ON IRT.PurchaseReturnDetailId=IRD.Id
                    LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRD.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRD.PurchaseReturnId=@receiveId  AND IRT.PurchaseReturnDetailId<>'' AND ITD.AType='Dr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					UNION
					SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr
						, SUM(IRT.TaxAmount) AS  Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[PurchaseReturnTax] AS IRT
					LEFT JOIN [TRN].[PurchaseReturnService] AS IRD ON IRT.PurchaseReturnServiceId=IRD.Id
                    LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRD.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRD.PurchaseReturnId=@receiveId  AND IRT.PurchaseReturnServiceId<>'' AND ITD.AType='Dr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					
					
					--UNION
					--SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
					--	, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
					--	, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
					--	, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
					--	, NULL Dr
					--	,  SUM(IRT.TaxAmount) AS Cr
					--	, SUM(IRT.TaxAmount) AS Amount
					--    ,0 IsAsset, NULL InventoryReceiveDetailId
					--FROM [TRN].[PurchaseReturnAdditionalTax] AS IRT
					--LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRT.PurchaseReturnId=PR.Id
					--LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					--LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					--LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					--LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					--LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					--LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					--LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					--WHERE IRT.PurchaseReturnId=@receiveId   AND ITD.AType='Dr'
					--GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					
					UNION
					SELECT 'TCS' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, NULL Dr
						,  SUM(IRT.TaxAmount) AS Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[PurchaseReturnAdditionalTax] AS IRT
					
					LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRT.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRT.PurchaseReturnId=@receiveId   AND ITD.AType='Dr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					
                    ORDER BY T.TrnType DESC";
						return _sqlRepository.GetDataCollection(sql);
					}
				}
				else
                {
					if (Convert.ToBoolean(purchaseReturnData["IsNonCreditable"].ToString()))
					{
						var sql = @"DECLARE @receiveId varchar(10)='" + purchaseReturnId + "', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + "', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId,NULL InvoiceId,NULL InvoiceDetailId
					FROM (
						SELECT  'Material' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId

							,  IRD.PostDrGLGeneralInfoId  GLGeneralInfoId
							, GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,IRD.PostDrBudgetMasterId BudgetMasterId 
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,IRD.PostDrActivityId ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							, NULL Dr
							, SUM(PRD.TotalMaterialTranAmount) AS Cr
							, SUM(PRD.TotalMaterialTranAmount) AS Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId
						FROM TRN.PurchaseReturnDetail PRD
						JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=PRD.InventoryReceiveDetailId
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, IRD.PostDrGLGeneralInfoId, GL.AccountCode, GL.UserName, IRD.PostDrBudgetMasterId, B.Code, B.UserName, IRD.PostDrActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					UNION
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId,T.InvoiceId,T.InvoiceDetailId
					FROM (
						SELECT  'Return' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,NULL FixedAssetMasterId

							,VD.GLGeneralInfoId
							,GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,VD.BudgetMasterId 
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,VD.ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							
							, SUM(PRD.TotalMaterialTranAmount) AS Dr, NULL Cr
							, SUM(PRD.TotalMaterialTranAmount ) AS Amount
                            ,MM.IsAsset , NULL InventoryReceiveDetailId,IVD.InvoiceId,VD.InvoiceDetailId
						FROM TRN.PurchaseReturnDetail PRD
						JOIN [TRN].[InventoryReceive] AS IR ON IR.Id=PRD.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON PRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT VoucherId, GLGeneralInfoId,	BudgetMasterId,	ActivityId,InvoiceDetailId FROM trn.VoucherDetail 
											WHERE InvoiceDetailId IS NOT NULL GROUP BY VoucherId, GLGeneralInfoId,	BudgetMasterId,	ActivityId,InvoiceDetailId) AS VD ON VD.VoucherId=IR.VoucherId
						LEFT JOIN [TRN].[InvoiceDetail] AS IVD ON IVD.Id=VD.InvoiceDetailId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, VD.GLGeneralInfoId, GL.AccountCode, GL.UserName, VD.BudgetMasterId, B.Code, B.UserName
						,VD.ActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId ,IVD.InvoiceId,VD.InvoiceDetailId
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId,T.InvoiceId,T.InvoiceDetailId
					
                    ORDER BY T.TrnType DESC";
						return _sqlRepository.GetDataCollection(sql);
					}
					else
					{
						var sql = @"DECLARE @receiveId varchar(10)='" + purchaseReturnId + "', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + "', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId,NULL InvoiceId,NULL InvoiceDetailId
					FROM (
						SELECT  'Material' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId

							,  IRD.PostDrGLGeneralInfoId  GLGeneralInfoId
							, GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,IRD.PostDrBudgetMasterId BudgetMasterId 
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,IRD.PostDrActivityId ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							, NULL Dr
							, SUM(PRD.TotalMaterialTranAmount) AS Cr
							, SUM(PRD.TotalMaterialTranAmount) AS Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId
						FROM TRN.PurchaseReturnDetail PRD
						JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=PRD.InventoryReceiveDetailId
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, IRD.PostDrGLGeneralInfoId, GL.AccountCode, GL.UserName, IRD.PostDrBudgetMasterId, B.Code, B.UserName, IRD.PostDrActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					UNION
					SELECT R.OtherName, R.TrnType, R.MaterialGroupMasterId, R.TaxCategoryId
						, R.GLGeneralInfoId, R.GLGeneralInfoCode, R.GLGeneralInfoName
						, R.BudgetMasterId, R.BudgetCode, R.BudgetName
						, R.ActivityId, R.ActivityCode,R.ActivityName
						, R.Dr
						
						, R.Cr, R.Amount, R.IsAsset,R.InventoryReceiveDetailId,R.InvoiceId,R.InvoiceDetailId
					
					FROM (
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr+ISNULL(TCS.TCSAmount,0) Dr
						
						, T.Cr, T.Amount+ISNULL(TCS.TCSAmount,0) Amount, T.IsAsset,T.InventoryReceiveDetailId,T.PurchaseReturnId,T.InvoiceId,T.InvoiceDetailId
					FROM (
						SELECT  'Return' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,NULL FixedAssetMasterId

							,VD.GLGeneralInfoId
							,GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,VD.BudgetMasterId 
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,VD.ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							
							, SUM(PRD.TotalMaterialTranAmount+ISNULL(PRD.TotalTaxAmount,0)+ISNULL(PRD.ChargesTaxTranAmount,0)) AS Dr, NULL Cr
							, SUM(PRD.TotalMaterialTranAmount+ISNULL(PRD.TotalTaxAmount,0)+ISNULL(PRD.ChargesTaxTranAmount,0) ) AS Amount
                            ,MM.IsAsset , NULL InventoryReceiveDetailId,PRD.PurchaseReturnId,IVD.InvoiceId,VD.InvoiceDetailId
						FROM TRN.PurchaseReturnDetail PRD
						JOIN [TRN].[InventoryReceive] AS IR ON IR.Id=PRD.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON PRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT VoucherId, GLGeneralInfoId,	BudgetMasterId,	ActivityId,InvoiceDetailId FROM trn.VoucherDetail 
											WHERE InvoiceDetailId IS NOT NULL GROUP BY VoucherId, GLGeneralInfoId,	BudgetMasterId,	ActivityId,InvoiceDetailId) AS VD ON VD.VoucherId=IR.VoucherId
						LEFT JOIN [TRN].[InvoiceDetail] AS IVD ON IVD.Id=VD.InvoiceDetailId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
						--LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
						--		AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						--LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.DebitNoteGLId=GL.Id
						--LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.DebitNoteBudgetMasterId= BM.Id
						--LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						--LEFT JOIN [HKP].[Activity] AS A ON MGGL.DebitNoteActivityId= A.Id
						
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, VD.GLGeneralInfoId, GL.AccountCode, GL.UserName, VD.BudgetMasterId, B.Code, B.UserName
						, VD.ActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId ,PRD.PurchaseReturnId,IVD.InvoiceId,VD.InvoiceDetailId
                    ) AS T

					LEFT OUTER JOIN (
						SELECT INS.PurchaseReturnId, sum(INS.TaxAmount) AS TCSAmount
						from  [TRN].[PurchaseReturnAdditionalTax] AS INS
						LEFT JOIN TRN.PurchaseReturn AS IR ON IR.Id=INS.PurchaseReturnId 
                        where PurchaseReturnId=@receiveId group by INS.PurchaseReturnId
						) AS TCS on TCS.PurchaseReturnId=T.PurchaseReturnId

					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount,TCS.TCSAmount,T.PurchaseReturnId, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId,T.InvoiceId,T.InvoiceDetailId
					)
					R 
					GROUP BY R.MaterialGroupMasterId, R.GLGeneralInfoId, R.GLGeneralInfoCode, R.GLGeneralInfoName, R.BudgetMasterId, R.BudgetCode, R.BudgetName, R.ActivityId
                    , R.ActivityCode, R.ActivityName, R.Dr, R.Cr, R.Amount, R.OtherName, R.TrnType,R.TaxCategoryId,R.IsAsset, R.InventoryReceiveDetailId ,R.InvoiceId,R.InvoiceDetailId
					UNION
					SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr
						, SUM(IRT.TaxAmount) AS  Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId,NULL InvoiceId,NULL InvoiceDetailId
					FROM [TRN].[PurchaseReturnTax] AS IRT
					LEFT JOIN [TRN].[PurchaseReturnDetail] AS IRD ON IRT.PurchaseReturnDetailId=IRD.Id
                    LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRD.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRD.PurchaseReturnId=@receiveId  AND IRT.PurchaseReturnDetailId<>'' AND ITD.AType='Dr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					UNION
					SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr
						, SUM(IRT.TaxAmount) AS  Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId,NULL InvoiceId,NULL InvoiceDetailId
					FROM [TRN].[PurchaseReturnTax] AS IRT
					LEFT JOIN [TRN].[PurchaseReturnService] AS IRD ON IRT.PurchaseReturnServiceId=IRD.Id
                    LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRD.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRD.PurchaseReturnId=@receiveId  AND IRT.PurchaseReturnServiceId<>'' AND ITD.AType='Dr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					
					
					--UNION
					--SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
					--	, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
					--	, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
					--	, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
					--	, NULL Dr
					--	,  SUM(IRT.TaxAmount) AS Cr
					--	, SUM(IRT.TaxAmount) AS Amount
					--    ,0 IsAsset, NULL InventoryReceiveDetailId
					--FROM [TRN].[PurchaseReturnAdditionalTax] AS IRT
					--LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRT.PurchaseReturnId=PR.Id
					--LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					--LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					--LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					--LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					--LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					--LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					--LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					--WHERE IRT.PurchaseReturnId=@receiveId   AND ITD.AType='Dr'
					--GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					
					UNION
					SELECT 'TCS' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, NULL Dr
						,  SUM(IRT.TaxAmount) AS Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId,NULL InvoiceId,NULL InvoiceDetailId
					FROM [TRN].[PurchaseReturnAdditionalTax] AS IRT
					
					LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRT.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRT.PurchaseReturnId=@receiveId   AND ITD.AType='Dr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					
                    ORDER BY T.TrnType DESC";
						return _sqlRepository.GetDataCollection(sql);
					}
				}
					
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetPurchaseReturnServicePayable(string companyId, string plantId, string purchaseReturnId)
        {
            try
            {
                var purchaseReturnData = GetPurchaseReturn(purchaseReturnId);
                var companyParty = GetCompanyPartyGroup(purchaseReturnData["PartyId"].ToString(), plantId);



                var sql = @"DECLARE @receiveId varchar(10)='" + purchaseReturnId + "', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + "', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId
					FROM (
						SELECT  'Material' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId

							,  IRD.PostDrGLGeneralInfoId  GLGeneralInfoId
							, GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,IRD.PostDrBudgetMasterId BudgetMasterId 
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,IRD.PostDrActivityId ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							, NULL Dr
							, SUM(PRD.TotalMaterialTranAmount) AS Cr
							, SUM(PRD.TotalMaterialTranAmount) AS Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId
						FROM TRN.PurchaseReturnDetail PRD
						JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=PRD.InventoryReceiveDetailId
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, IRD.PostDrGLGeneralInfoId, GL.AccountCode, GL.UserName, IRD.PostDrBudgetMasterId, B.Code, B.UserName, IRD.PostDrActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					UNION
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId
					FROM (
						SELECT  'Return' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,NULL FixedAssetMasterId

							,  MGGL.DebitNoteGLId  GLGeneralInfoId
							, GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,MGGL.DebitNoteBudgetMasterId BudgetMasterId 
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,MGGL.DebitNoteActivityId ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							
							, SUM(PRD.TotalMaterialTranAmount+ISNULL(PRD.TotalTaxAmount,0)) AS Dr, NULL Cr
							, SUM(PRD.TotalMaterialTranAmount+ISNULL(PRD.TotalTaxAmount,0) ) AS Amount
                            ,MM.IsAsset , NULL InventoryReceiveDetailId
						FROM TRN.PurchaseReturnDetail PRD
						--JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=PRD.InventoryReceiveDetailId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON PRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.DebitNoteGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.DebitNoteBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.DebitNoteActivityId= A.Id
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.DebitNoteGLId, GL.AccountCode, GL.UserName, MGGL.DebitNoteBudgetMasterId, B.Code, B.UserName
						, MGGL.DebitNoteActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId 
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					UNION
					SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr
						, SUM(IRT.TaxAmount) AS  Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[PurchaseReturnTax] AS IRT
					LEFT JOIN [TRN].[PurchaseReturnDetail] AS IRD ON IRT.PurchaseReturnDetailId=IRD.Id
                    LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRD.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRD.PurchaseReturnId=@receiveId  AND IRT.PurchaseReturnDetailId<>'' AND ITD.AType='Dr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					
					UNION
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  SUM(IRT.TaxAmount) AS Dr
						, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[PurchaseReturnTax] AS IRT
					LEFT JOIN [TRN].[PurchaseReturnDetail] AS IRD ON IRT.PurchaseReturnDetailId=IRD.Id
                    LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRD.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRD.PurchaseReturnId=@receiveId  AND IRT.PurchaseReturnDetailId<>'' AND ITD.AType='Cr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					
                    ORDER BY T.TrnType DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetPurchaseReturnMaterialRCMPayable(string companyId, string plantId, string purchaseReturnId, bool isDebitNote)
        {
            try
            {
                var purchaseReturnData = GetPurchaseReturn(purchaseReturnId);
                var companyParty = GetCompanyPartyGroup(purchaseReturnData["PartyId"].ToString(), plantId);
				if (isDebitNote == true)
				{
					var sql = @"DECLARE @receiveId varchar(10)='" + purchaseReturnId + "', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + "', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId
					FROM (
						SELECT  'Material' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId

							,  IRD.PostDrGLGeneralInfoId  GLGeneralInfoId
							, GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,IRD.PostDrBudgetMasterId BudgetMasterId 
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,IRD.PostDrActivityId ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							, NULL Dr
							, SUM(PRD.TotalMaterialTranAmount) AS Cr
							, SUM(PRD.TotalMaterialTranAmount) AS Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId
						FROM TRN.PurchaseReturnDetail PRD
						JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=PRD.InventoryReceiveDetailId
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, IRD.PostDrGLGeneralInfoId, GL.AccountCode, GL.UserName, IRD.PostDrBudgetMasterId, B.Code, B.UserName, IRD.PostDrActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					UNION
SELECT R.OtherName, R.TrnType, R.MaterialGroupMasterId, R.TaxCategoryId
						, R.GLGeneralInfoId, R.GLGeneralInfoCode, R.GLGeneralInfoName
						, R.BudgetMasterId, R.BudgetCode, R.BudgetName
						, R.ActivityId, R.ActivityCode,R.ActivityName
						, R.Dr
						
						, R.Cr, R.Amount, R.IsAsset,R.InventoryReceiveDetailId
					
					FROM (
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr+ISNULL(TCS.TCSAmount,0) Dr
						
						, T.Cr, T.Amount+ISNULL(TCS.TCSAmount,0) Amount, T.IsAsset,T.InventoryReceiveDetailId,T.PurchaseReturnId
					FROM (
						SELECT  'Return' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,NULL FixedAssetMasterId

							,MGGL.DebitNoteGLId GLGeneralInfoId
							,GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,MGGL.DebitNoteBudgetMasterId BudgetMasterId 
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,MGGL.DebitNoteActivityId ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							
							, SUM(PRD.TotalMaterialTranAmount+ISNULL(PRD.TotalTaxAmount,0)+ISNULL(PRD.ChargesTaxTranAmount,0)) AS Dr, NULL Cr
							, SUM(PRD.TotalMaterialTranAmount+ISNULL(PRD.TotalTaxAmount,0)+ISNULL(PRD.ChargesTaxTranAmount,0) ) AS Amount
                            ,MM.IsAsset , NULL InventoryReceiveDetailId,PRD.PurchaseReturnId
						FROM TRN.PurchaseReturnDetail PRD
						JOIN [TRN].[InventoryReceive] AS IR ON IR.Id=PRD.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON PRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						--LEFT JOIN (SELECT VoucherId, GLGeneralInfoId,	BudgetMasterId,	ActivityId,InvoiceDetailId FROM trn.VoucherDetail 
						--					WHERE InvoiceDetailId IS NOT NULL GROUP BY VoucherId, GLGeneralInfoId,	BudgetMasterId,	ActivityId,InvoiceDetailId) AS VD ON VD.VoucherId=IR.VoucherId
						--LEFT JOIN [TRN].[InvoiceDetail] AS IVD ON IVD.Id=VD.InvoiceDetailId
						--LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
						--LEFT JOIN[MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
						--LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						--LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.DebitNoteGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.DebitNoteBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.DebitNoteActivityId= A.Id
						
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.DebitNoteGLId, GL.AccountCode, GL.UserName, MGGL.DebitNoteBudgetMasterId, B.Code, B.UserName
						, MGGL.DebitNoteActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId ,PRD.PurchaseReturnId
                    ) AS T
					LEFT OUTER JOIN (
						SELECT INS.PurchaseReturnId, sum(INS.TaxAmount) AS TCSAmount
						from  [TRN].[PurchaseReturnAdditionalTax] AS INS
						LEFT JOIN TRN.PurchaseReturn AS IR ON IR.Id=INS.PurchaseReturnId 
                        where PurchaseReturnId=@receiveId group by INS.PurchaseReturnId
						) AS TCS on TCS.PurchaseReturnId=T.PurchaseReturnId

					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount,TCS.TCSAmount,T.PurchaseReturnId, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId,T.PurchaseReturnId
					)
					R 
					GROUP BY R.MaterialGroupMasterId, R.GLGeneralInfoId, R.GLGeneralInfoCode, R.GLGeneralInfoName, R.BudgetMasterId, R.BudgetCode, R.BudgetName, R.ActivityId
                    , R.ActivityCode, R.ActivityName, R.Dr, R.Cr, R.Amount, R.OtherName, R.TrnType,R.TaxCategoryId,R.IsAsset, R.InventoryReceiveDetailId
					UNION ALL
					SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr
						, SUM(IRT.TaxAmount) AS  Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[PurchaseReturnTax] AS IRT
					LEFT JOIN [TRN].[PurchaseReturnDetail] AS IRD ON IRT.PurchaseReturnDetailId=IRD.Id
                    LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRD.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRD.PurchaseReturnId=@receiveId  AND IRT.PurchaseReturnDetailId<>'' AND ITD.AType='Dr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					
					UNION  ALL
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  SUM(IRT.TaxAmount) AS Dr
						, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[PurchaseReturnTax] AS IRT
					LEFT JOIN [TRN].[PurchaseReturnDetail] AS IRD ON IRT.PurchaseReturnDetailId=IRD.Id
                    LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRD.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRD.PurchaseReturnId=@receiveId  AND IRT.PurchaseReturnDetailId<>'' AND ITD.AType='Cr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					UNION ALL
					SELECT 'TCS' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, NULL Dr
						,  SUM(IRT.TaxAmount) AS Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[PurchaseReturnAdditionalTax] AS IRT
					
					LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRT.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRT.PurchaseReturnId=@receiveId   AND ITD.AType='Dr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
                    ORDER BY T.TrnType DESC";
					return _sqlRepository.GetDataCollection(sql);
				}
                else
                {
					var sql = @"DECLARE @receiveId varchar(10)='" + purchaseReturnId + "', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + "', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId,NULL InvoiceId,NULL InvoiceDetailId
					FROM (
						SELECT  'Material' AS OtherName, 'Cr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId

							,  IRD.PostDrGLGeneralInfoId  GLGeneralInfoId
							, GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,IRD.PostDrBudgetMasterId BudgetMasterId 
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,IRD.PostDrActivityId ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							, NULL Dr
							, SUM(PRD.TotalMaterialTranAmount) AS Cr
							, SUM(PRD.TotalMaterialTranAmount) AS Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId
						FROM TRN.PurchaseReturnDetail PRD
						JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=PRD.InventoryReceiveDetailId
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, IRD.PostDrGLGeneralInfoId, GL.AccountCode, GL.UserName, IRD.PostDrBudgetMasterId, B.Code, B.UserName, IRD.PostDrActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					UNION
SELECT R.OtherName, R.TrnType, R.MaterialGroupMasterId, R.TaxCategoryId
						, R.GLGeneralInfoId, R.GLGeneralInfoCode, R.GLGeneralInfoName
						, R.BudgetMasterId, R.BudgetCode, R.BudgetName
						, R.ActivityId, R.ActivityCode,R.ActivityName
						, R.Dr
						
						, R.Cr, R.Amount, R.IsAsset,R.InventoryReceiveDetailId,R.InvoiceId,R.InvoiceDetailId
					
					FROM (
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr+ISNULL(TCS.TCSAmount,0) Dr
						
						, T.Cr, T.Amount+ISNULL(TCS.TCSAmount,0) Amount, T.IsAsset,T.InventoryReceiveDetailId,T.PurchaseReturnId,T.InvoiceId,T.InvoiceDetailId
					FROM (
						SELECT  'Return' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,NULL FixedAssetMasterId

							,VD.GLGeneralInfoId
							,GL.AccountCode  GLGeneralInfoCode 
							,GL.UserName GLGeneralInfoName
							,VD.BudgetMasterId
							,B.Code BudgetCode 
							,B.UserName BudgetName 
							,VD.ActivityId
							,A.Code ActivityCode 
							,A.UserName ActivityName 
							
							, SUM(PRD.TotalMaterialTranAmount) AS Dr, NULL Cr
							, SUM(PRD.TotalMaterialTranAmount ) AS Amount
                            ,MM.IsAsset , NULL InventoryReceiveDetailId,PRD.PurchaseReturnId,IVD.InvoiceId,VD.InvoiceDetailId
						FROM TRN.PurchaseReturnDetail PRD
						JOIN [TRN].[InventoryReceive] AS IR ON IR.Id=PRD.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON PRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT VoucherId, GLGeneralInfoId,	BudgetMasterId,	ActivityId,InvoiceDetailId FROM trn.VoucherDetail 
											WHERE InvoiceDetailId IS NOT NULL GROUP BY VoucherId, GLGeneralInfoId,	BudgetMasterId,	ActivityId,InvoiceDetailId) AS VD ON VD.VoucherId=IR.VoucherId
						LEFT JOIN [TRN].[InvoiceDetail] AS IVD ON IVD.Id=VD.InvoiceDetailId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
						WHERE PRD.PurchaseReturnId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, VD.GLGeneralInfoId, GL.AccountCode, GL.UserName, VD.BudgetMasterId, B.Code, B.UserName
						,VD.ActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,PRD.PurchaseReturnId,IVD.InvoiceId,VD.InvoiceDetailId
                    ) AS T
					LEFT OUTER JOIN (
						SELECT INS.PurchaseReturnId, sum(INS.TaxAmount) AS TCSAmount
						from  [TRN].[PurchaseReturnAdditionalTax] AS INS
						LEFT JOIN TRN.PurchaseReturn AS IR ON IR.Id=INS.PurchaseReturnId 
                        where PurchaseReturnId=@receiveId group by INS.PurchaseReturnId
						) AS TCS on TCS.PurchaseReturnId=T.PurchaseReturnId

					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount,TCS.TCSAmount,T.PurchaseReturnId, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId,T.PurchaseReturnId,T.InvoiceId,T.InvoiceDetailId
					)
					R 
					GROUP BY R.MaterialGroupMasterId, R.GLGeneralInfoId, R.GLGeneralInfoCode, R.GLGeneralInfoName, R.BudgetMasterId, R.BudgetCode, R.BudgetName, R.ActivityId
                    , R.ActivityCode, R.ActivityName, R.Dr, R.Cr, R.Amount, R.OtherName, R.TrnType,R.TaxCategoryId,R.IsAsset, R.InventoryReceiveDetailId,R.InvoiceId,R.InvoiceDetailId
					UNION ALL
					SELECT 'Tax' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr
						, SUM(IRT.TaxAmount) AS  Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId,NULL InvoiceId,NULL InvoiceDetailId
					FROM [TRN].[PurchaseReturnTax] AS IRT
					LEFT JOIN [TRN].[PurchaseReturnDetail] AS IRD ON IRT.PurchaseReturnDetailId=IRD.Id
                    LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRD.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRD.PurchaseReturnId=@receiveId  AND IRT.PurchaseReturnDetailId<>'' AND ITD.AType='Dr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					
					UNION  ALL
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  SUM(IRT.TaxAmount) AS Dr
						, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId,NULL InvoiceId,NULL InvoiceDetailId
					FROM [TRN].[PurchaseReturnTax] AS IRT
					LEFT JOIN [TRN].[PurchaseReturnDetail] AS IRD ON IRT.PurchaseReturnDetailId=IRD.Id
                    LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRD.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRD.PurchaseReturnId=@receiveId  AND IRT.PurchaseReturnDetailId<>'' AND ITD.AType='Cr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
					UNION ALL
					SELECT 'TCS' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, ITD.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, ITD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, ITD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, NULL Dr
						,  SUM(IRT.TaxAmount) AS Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId,NULL InvoiceId,NULL InvoiceDetailId
					FROM [TRN].[PurchaseReturnAdditionalTax] AS IRT
					
					LEFT JOIN [TRN].[PurchaseReturn] AS PR ON IRT.PurchaseReturnId=PR.Id
					LEFT JOIN TRN.[InventoryReceive] AS IR ON IR.Id=PR.InventoryReceiveId
					LEFT JOIN TRN.InvoiceTax IT ON IT.VoucherId=IR.VoucherId and IRT.TaxCategoryId=IT.TaxCategoryId
					LEFT JOIN TRN.InvoiceTaxDetail ITD ON ITD.InvoiceTaxId=IT.Id 
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ITD.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON ITD.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON ITD.ActivityId= A.Id
					WHERE IRT.PurchaseReturnId=@receiveId   AND ITD.AType='Dr'
					GROUP BY  IRT.TaxCategoryId, ITD.GLGeneralInfoId, GL.AccountCode, GL.UserName, ITD.BudgetMasterId, B.Code, B.UserName, ITD.ActivityId, A.Code, A.UserName
                    ORDER BY T.TrnType DESC";
					return _sqlRepository.GetDataCollection(sql);
				}
					
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #region ServicePayable
        public IEnumerable<object> GetListForSvcPayable(string plantId)
        {
            try
            {
                var sql = @"SELECT IR.Id,  IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode
								, P.UserName AS PartyName
			                    , CP.UserName AS PartyAccountGroupName
			                   
								, IR.CurrencyId, CU.Code AS CurrencyCode
								, IR.BaseCurrencyId
	                            , IR.PODepended
								, IR.DocRefNo
                                ,Replace(CONVERT(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
                                ,Replace(CONVERT(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS AcknolwdgementDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy,IPP.GSTIN GSTINNo, IR.InvoicingByAddress, IR.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , IRD.TransactionAmount, IRD.BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName
								, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate
								,IR.NoteForAccounts Narration
								,POId=STUFF((SELECT DISTINCT ','+xpo.Id from
									trn.ServicePOMaster xpo
									INNER JOin trn.[ServiceAcknowledgementDetail] xPDAMAP on xpo.Id=xPDAMAP.ServicePOMasterId
									left join [TRN].[ServiceAcknowledgementMaster] xir on xir.Id=xPDAMAP.ServiceAcknowledgementMasterId
									WHERE xir.Id=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,PORefNo=STUFF((SELECT DISTINCT ','+xpo.DocRefNo from
									trn.ServicePOMaster xpo
									INNER JOin trn.[ServiceAcknowledgementDetail] xPDAMAP on xpo.Id=xPDAMAP.ServicePOMasterId
									left join [TRN].[ServiceAcknowledgementMaster] xir on xir.Id=xPDAMAP.ServiceAcknowledgementMasterId
									WHERE xir.Id=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,PODate=STUFF((SELECT DISTINCT ','++REPLACE(CONVERT(CHAR(11), xpo.DocDate, 106),' ','-') from
									trn.ServicePOMaster xpo
									INNER JOin trn.[ServiceAcknowledgementDetail] xPDAMAP on xpo.Id=xPDAMAP.ServicePOMasterId
									left join [TRN].[ServiceAcknowledgementMaster] xir on xir.Id=xPDAMAP.ServiceAcknowledgementMasterId
									WHERE xir.Id=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									
                    FROM [TRN].[ServiceAcknowledgementMaster] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C
					 LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
					
                     LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, SUM(ROUND(A.Amount,4)) AS TransactionAmount, SUM(ROUND(A.TotalAmount,0)) AS BaseAmount 
					 FROM [TRN].[ServiceAcknowledgementDetail] AS A
		                        JOIN [TRN].[ServiceAcknowledgementMaster] AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + plantId + @"' 
								GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
                    WHERE IR.PlantId='" + plantId + @"' 
					AND ISNULL(IR.[Status],'')<>'Posting' AND IR.IsPaymentHold=0   AND IR.ApprovedByStatus='Approved'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetServicePayable(string companyId, string plantId, string serviceAcknowledgementMasterId)
        {
            try
            {
                var sql = @"DECLARE @serviceAcknowledgementMasterId varchar(10)='" + serviceAcknowledgementMasterId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"',@countryId varchar(10)
					
                            SELECT  'Svc' AS OtherName, 'Dr' AS TrnType, MM.Id as MaterialGroupMasterId, NULL AS TaxCategoryId, NULL AS TaxCodeId
							, BM.GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
							, IM.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
							, IM.ActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
							, SUM(IM.Amount) AS Dr, NULL Cr
							, SUM(IM.Amount) AS Amount,  IM.Id ServiceAcknowledgementDetailId,IM.ServiceMasterId,SRM.EntityId,EN.UserName EntityName
						FROM [TRN].[ServiceAcknowledgementDetail] AS IM
						LEFT JOIN [TRN].ServiceAcknowledgementMaster AS IR ON IM.ServiceAcknowledgementMasterId=IR.Id
						LEFT JOIN [TRN].ServicePODetail SPD ON SPD.Id=IM.ServicePODetailId
						LEFT JOIN [TRN].[ServiceRequsitionMaster] SRM ON SRM.Id=SPD.ServiceReqMasterId
						LEFT JOIN [HKP].[ServiceMaster] AS SM ON IM.ServiceMasterId=SM.Id
						LEFT JOIN [HKP].[ServiceGroup] AS MM ON SM.ServiceGroupId=MM.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON IM.BudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON IM.ActivityId= A.Id
						LEFT JOIN [ORG].[Entity] AS EN ON EN.Id= SRM.EntityId
						WHERE IM.ServiceAcknowledgementMasterId=@serviceAcknowledgementMasterId
						GROUP BY MM.Id,BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, IM.BudgetMasterId, B.Code, B.UserName
						, IM.ActivityId, A.Code, A.UserName,IM.Id,IM.ServiceMasterId,SRM.EntityId,EN.UserName
						UNION
						SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId, NULL AS TaxCodeId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,  NULL ServiceAcknowledgementDetailId,'' ServiceMasterId,'' EntityId,'' EntityName
					FROM [TRN].[ServicePOAckTax] AS IRT
					LEFT JOIN [TRN].[ServiceAcknowledgementDetail] AS IRD ON IRT.ServiceAcknowledgementDetailId=IRD.Id
                    LEFT JOIN [TRN].[ServiceAcknowledgementMaster] AS IR ON IRD.ServiceAcknowledgementMasterId=IR.Id
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRD.ServiceAcknowledgementMasterId=@serviceAcknowledgementMasterId AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM'  
					GROUP BY  IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					
                    UNION
						 SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId, IRT.TaxCodeId
						, TCGL.CreditableGLId GLGeneralInfoId , GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.CreditableGLBudgetMasterId BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.CreditableGLActivityId ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,  NULL ServiceAcknowledgementDetailId,'' ServiceMasterId,'' EntityId,'' EntityName
					FROM [TRN].[ServiceAcknowledgementAdditionalTax] AS IRT
                    LEFT JOIN [TRN].[ServiceAcknowledgementMaster] AS IR ON IRT.ServicePOAckMasterId=IR.Id
					LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TCGL.TaxCodeId=IRT.TaxCodeId
					LEFT JOIN [MST].[TaxCode] AS TC ON TCGL.TaxCodeId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.CreditableGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.CreditableGLBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.CreditableGLActivityId= A.Id
					WHERE IRT.ServicePOAckMasterId=@serviceAcknowledgementMasterId AND TC.InputOrOutput='Input' --AND TC.IsRCM=0 
					GROUP BY  IRT.TaxCategoryId, TCGL.CreditableGLId, GL.AccountCode, GL.UserName, TCGL.CreditableGLBudgetMasterId, B.Code, B.UserName, TCGL.CreditableGLActivityId, A.Code, A.UserName,IRT.TaxCodeId

                  
					UNION
					SELECT MAT.OtherName,MAT.TrnType,MAT.MaterialGroupMasterId, 
					MAT.TaxCategoryId, MAT.TaxCodeId
							,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName
							, MAT.Dr, MAT.Cr+ISNULL(TCS.TCSAmount,0)+ISNULL(charges.ChargesAmount,0)+ISNULL(chargestax.ChargesTaxAmount,0) Cr
							, MAT.Amount+ISNULL(TCS.TCSAmount,0)+ISNULL(charges.ChargesAmount,0)+ISNULL(chargestax.ChargesTaxAmount,0) Amount
					,  MAT.ServiceAcknowledgementDetailId,'' ServiceMasterId,'' EntityId,'' EntityName
					FROM (
					SELECT  'Vendor' AS OtherName, 'Cr' AS TrnType, NULL AS MaterialGroupMasterId, NULL AS TaxCategoryId, NULL AS TaxCodeId
							, CPGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
							, CPGL.BudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
							, CPGL.ActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
							, NULL Dr, SUM(IM.Amount+IM.TotalTaxAmount) AS Cr
							, SUM(IM.Amount+IM.TotalTaxAmount) AS Amount,  NULL ServiceAcknowledgementDetailId,IM.ServiceAcknowledgementMasterId
						FROM [TRN].[ServiceAcknowledgementDetail] AS IM
						LEFT JOIN [TRN].ServiceAcknowledgementMaster AS IR ON IM.ServiceAcknowledgementMasterId=IR.Id
						LEFT JOIN [HKP].[ServiceMaster] AS SM ON IM.ServiceMasterId=SM.Id
						LEFT JOIN [HKP].[ServiceGroup] AS MM ON SM.ServiceGroupId=MM.Id
						LEFT JOIN [HKP].[CompanyParty] CP ON IR.PartyId = CP.PartyId AND CP.PlantId=@plantId AND CP.PartyType='Vendor'
						LEFT JOIN HKP.CompanyPartyGL CPGL ON CPGL.CompanyPartyId=CP.Id and CPGL.PartyGLType='ReconciliationGL' 
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON CPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON CPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON CPGL.ActivityId= A.Id

						WHERE IM.ServiceAcknowledgementMasterId=@serviceAcknowledgementMasterId
						GROUP BY CPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, CPGL.BudgetMasterId, B.Code, B.UserName, CPGL.ActivityId, A.Code, A.UserName
						,IM.ServiceAcknowledgementMasterId
						) MAT

						LEFT OUTER JOIN (
						SELECT INS.ServicePOAckMasterId, sum(INS.TaxAmount) AS TCSAmount
						from  [TRN].[ServiceAcknowledgementAdditionalTax] AS INS
						LEFT JOIN TRN.ServiceAcknowledgementMaster AS IR ON IR.Id=INS.ServicePOAckMasterId 
                        where ServicePOAckMasterId=@serviceAcknowledgementMasterId group by INS.ServicePOAckMasterId
						) AS TCS on TCS.ServicePOAckMasterId=MAT.ServiceAcknowledgementMasterId

						LEFT OUTER JOIN (
						SELECT INS.ServiceAcknowledgementMasterId, sum(INS.Amount) AS ChargesAmount--,sum(spt.TaxAmount) chargesTax
						from  [TRN].[ServiceAcknowledgementCharge] AS INS
						LEFT JOIN TRN.ServiceAcknowledgementMaster AS IR ON IR.Id=INS.ServiceAcknowledgementMasterId 
                        where INS.ServiceAcknowledgementMasterId=@serviceAcknowledgementMasterId group by INS.ServiceAcknowledgementMasterId
						) AS charges on charges.ServiceAcknowledgementMasterId=MAT.ServiceAcknowledgementMasterId

						LEFT OUTER JOIN (
						SELECT INS.ServiceAcknowledgementMasterId, sum(INS.TaxAmount) AS ChargesTaxAmount
						from  [TRN].[ServicePOAckTax] AS INS
						LEFT JOIN TRN.ServiceAcknowledgementMaster AS IR ON IR.Id=INS.ServiceAcknowledgementMasterId 
                        where INS.ServiceAcknowledgementMasterId=@serviceAcknowledgementMasterId and INS.ServiceAcknowledgementDetailId is null and INS.ServiceAcknowledgementChargeId<>'' group by INS.ServiceAcknowledgementMasterId
						) AS chargestax on chargestax.ServiceAcknowledgementMasterId=MAT.ServiceAcknowledgementMasterId
						
					UNION
					SELECT MAT.OtherName,MAT.TrnType,MAT.MaterialGroupMasterId, 
					MAT.TaxCategoryId, MAT.TaxCodeId
							,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName
							, MAT.Dr, MAT.Cr+ISNULL(TCS.TCSAmount,0)+ISNULL(charges.ChargesAmount,0)+ISNULL(chargestax.ChargesTaxAmount,0) Cr
							, MAT.Amount+ISNULL(TCS.TCSAmount,0)+ISNULL(charges.ChargesAmount,0)+ISNULL(chargestax.ChargesTaxAmount,0) Amount
					,  MAT.ServiceAcknowledgementDetailId,'' ServiceMasterId,'' EntityId,'' EntityName
					FROM (
					SELECT  'GIRI' AS OtherName, 'Cr' AS TrnType, NULL AS MaterialGroupMasterId, NULL AS TaxCategoryId, NULL AS TaxCodeId
							, MGGL.ClearingAccountGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
							, MGGL.ClearingAccountBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
							, MGGL.ClearingAccountActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
							, NULL Dr, SUM(IM.Amount+IM.TotalTaxAmount) AS Cr
							, SUM(IM.Amount+IM.TotalTaxAmount) AS Amount,  NULL ServiceAcknowledgementDetailId,IM.ServiceAcknowledgementMasterId
						FROM [TRN].[ServiceAcknowledgementDetail] AS IM
						LEFT JOIN [TRN].ServiceAcknowledgementMaster AS IR ON IM.ServiceAcknowledgementMasterId=IR.Id
						LEFT JOIN [HKP].[ServiceMaster] AS SM ON IM.ServiceMasterId=SM.Id
						LEFT JOIN [HKP].[ServiceGroup] AS MM ON SM.ServiceGroupId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[ServiceGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.Id = MGGL.ServiceGroupId

								LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGGL.ClearingAccountGLId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGGL.ClearingAccountBudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ClearingAccountActivityId= A.Id

						WHERE IM.ServiceAcknowledgementMasterId=@serviceAcknowledgementMasterId
						GROUP BY MGGL.ClearingAccountGLId, GL.AccountCode, GL.UserName, MGGL.ClearingAccountBudgetMasterId, B.Code, B.UserName, MGGL.ClearingAccountActivityId, A.Code, A.UserName
						,IM.ServiceAcknowledgementMasterId
						) MAT
						LEFT OUTER JOIN (
						SELECT INS.ServicePOAckMasterId, sum(INS.TaxAmount) AS TCSAmount
						from  [TRN].[ServiceAcknowledgementAdditionalTax] AS INS
						LEFT JOIN TRN.ServiceAcknowledgementMaster AS IR ON IR.Id=INS.ServicePOAckMasterId 
                        where ServicePOAckMasterId=@serviceAcknowledgementMasterId group by INS.ServicePOAckMasterId
						) AS TCS on TCS.ServicePOAckMasterId=MAT.ServiceAcknowledgementMasterId
						LEFT OUTER JOIN (
						SELECT INS.ServiceAcknowledgementMasterId, sum(INS.Amount) AS ChargesAmount--,sum(spt.TaxAmount) chargesTax
						from  [TRN].[ServiceAcknowledgementCharge] AS INS
						LEFT JOIN TRN.ServiceAcknowledgementMaster AS IR ON IR.Id=INS.ServiceAcknowledgementMasterId 
                        where INS.ServiceAcknowledgementMasterId=@serviceAcknowledgementMasterId group by INS.ServiceAcknowledgementMasterId
						) AS charges on charges.ServiceAcknowledgementMasterId=MAT.ServiceAcknowledgementMasterId

						LEFT OUTER JOIN (
						SELECT INS.ServiceAcknowledgementMasterId, sum(INS.TaxAmount) AS ChargesTaxAmount
						from  [TRN].[ServicePOAckTax] AS INS
						LEFT JOIN TRN.ServiceAcknowledgementMaster AS IR ON IR.Id=INS.ServiceAcknowledgementMasterId 
                        where INS.ServiceAcknowledgementMasterId=@serviceAcknowledgementMasterId and INS.ServiceAcknowledgementDetailId is null and INS.ServiceAcknowledgementChargeId<>'' group by INS.ServiceAcknowledgementMasterId
						) AS chargestax on chargestax.ServiceAcknowledgementMasterId=MAT.ServiceAcknowledgementMasterId
						
                        --ORDER BY TrnType DESC 
						UNION ALL
						SELECT  'Charges' AS OtherName, 'Dr' AS TrnType, MM.Id as MaterialGroupMasterId, NULL AS TaxCategoryId, NULL AS TaxCodeId
							, MGGL.ServiceGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
							, MGGL.ServiceBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
							, MGGL.ServiceActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
							, SUM(IM.Amount) AS Dr, NULL Cr
							, SUM(IM.Amount) AS Amount,  NULL ServiceAcknowledgementDetailId,'' ServiceMasterId,'' EntityId,'' EntityName
						FROM [TRN].[ServiceAcknowledgementCharge] AS IM
						LEFT JOIN [TRN].ServiceAcknowledgementMaster AS IR ON IM.ServiceAcknowledgementMasterId=IR.Id
						LEFT JOIN [HKP].[ServiceMaster] AS SM ON IM.ServiceMasterId=SM.Id
						LEFT JOIN [HKP].[ServiceGroup] AS MM ON SM.ServiceGroupId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[ServiceGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.Id = MGGL.ServiceGroupId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.ServiceGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ServiceBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ServiceActivityId= A.Id
						WHERE IM.ServiceAcknowledgementMasterId=@serviceAcknowledgementMasterId
						GROUP BY MM.Id, MGGL.ServiceGLId, GL.AccountCode, GL.UserName, MGGL.ServiceBudgetMasterId, B.Code, B.UserName, MGGL.ServiceActivityId, A.Code, A.UserName,IM.Id
						UNION ALL
						SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId, NULL AS TaxCodeId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,  NULL ServiceAcknowledgementDetailId,'' ServiceMasterId,'' EntityId,'' EntityName
					FROM [TRN].[ServicePOAckTax] AS IRT
					LEFT JOIN [TRN].[ServiceAcknowledgementCharge] AS IRD ON IRT.ServiceAcknowledgementChargeId=IRD.Id
                    LEFT JOIN [TRN].[ServiceAcknowledgementMaster] AS IR ON IRD.ServiceAcknowledgementMasterId=IR.Id
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRD.ServiceAcknowledgementMasterId=@serviceAcknowledgementMasterId AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM'  
					GROUP BY  IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
                        --ORDER BY TrnType DESC ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetServiceDetailGL(string companyId, string plantId, string serviceAcknowledgementMasterId)
        {
            try
            {
                var sql = @"DECLARE @serviceAcknowledgementMasterId varchar(10)='" + serviceAcknowledgementMasterId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"',@countryId varchar(10)
                            SELECT  'Svc' AS OtherName, 'Dr' AS TrnType, MM.Id as MaterialGroupMasterId, NULL AS TaxCategoryId, NULL AS TaxCodeId
							, MGGL.ServiceGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
							, MGGL.ServiceBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
							, MGGL.ServiceActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
							, SUM(IM.Amount) AS Dr, NULL Cr
							, SUM(IM.Amount) AS Amount,  IM.Id ServiceAcknowledgementDetailId
						FROM [TRN].[ServiceAcknowledgementDetail] AS IM
						LEFT JOIN [TRN].ServiceAcknowledgementMaster AS IR ON IM.ServiceAcknowledgementMasterId=IR.Id
						LEFT JOIN [HKP].[ServiceMaster] AS SM ON IM.ServiceMasterId=SM.Id
						LEFT JOIN [HKP].[ServiceGroup] AS MM ON SM.ServiceGroupId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[ServiceGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.Id = MGGL.ServiceGroupId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.ServiceGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ServiceBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ServiceActivityId= A.Id
						WHERE IM.ServiceAcknowledgementMasterId=@serviceAcknowledgementMasterId
						GROUP BY MM.Id, MGGL.ServiceGLId, GL.AccountCode, GL.UserName, MGGL.ServiceBudgetMasterId, B.Code, B.UserName, MGGL.ServiceActivityId, A.Code, A.UserName,IM.Id
                         ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public GridModel GetServiceData(GridParameter parameters, string serviceAcknowledgementMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"SELECT IR.Id,  IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode
								, P.UserName AS PartyName
			                    , CP.UserName AS PartyAccountGroupName
			                   
								, IR.CurrencyId, CU.Code AS CurrencyCode
								, IR.BaseCurrencyId
	                            , IR.PODepended
								,IR.DocRefNo
                                ,Replace(CONVERT(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
                                ,Replace(CONVERT(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS AcknolwdgementDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , SAD.Amount TrnAmount,ROUND(SAD.Amount,4) * ROUND(IR.ToCurrencyRate,4) BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName
								, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate
								,IR.NoteForAccounts Narration
								--,PO.POId
                                ,SM.UserName ServiceMasterName
								,SG.UserName ServiceGroupName,SAD.ServiceAcknowledgementMasterId,ISNULL(SAD.TotalTaxAmount,0) TotalTaxAmount
                     FROM TRN.ServiceAcknowledgementDetail SAD
					LEFT JOIN [TRN].[ServiceAcknowledgementMaster] AS IR ON IR.Id=SAD.ServiceAcknowledgementMasterId
					LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
					LEFT JOIN HKP.ServiceMaster SM ON SM.Id=SAD.ServiceMasterId
					LEFT JOIN HKP.ServiceGroup SG ON SG.Id=SM.ServiceGroupId
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C
					 LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    WHERE IR.Id='" + serviceAcknowledgementMasterId + @"'
					AND ISNULL(IR.[Status],'')<>'Posting' AND IR.IsPaymentHold=0   --AND IR.IsApproved=1";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public GridModel GetServiceAdditionalTax(GridParameter parameters, string serviceAcknowledgementMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"Select * from trn.ServiceAcknowledgementAdditionalTax
                    WHERE ServicePOAckMasterId='" + serviceAcknowledgementMasterId + @"'";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetServicePostingList(string column, string value, string plantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
				select top 100 * from (SELECT IR.Id,  IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode
								, P.UserName AS PartyName
			                    , CP.UserName AS PartyAccountGroupName
			                   
								, IR.CurrencyId, CU.Code AS CurrencyCode
								, IR.BaseCurrencyId
	                            , IR.PODepended
								, IR.DocRefNo
                                ,Replace(CONVERT(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
                                ,Replace(CONVERT(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS AcknolwdgementDate
                                ,Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , IRD.TransactionAmount+IRD.TotalTaxAmount TransactionAmount, IRD.BaseAmount+IRD.TotalTaxAmount BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName
								, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate
								,IR.NoteForAccounts Narration
								,V.VoucherNo,IR.VoucherId, ISNULL(ADT.TaxAmount,0) TDSTax, ADT.VoucherId TDSTaxVoucherId, ADT.Id AdditionalTaxId
                                   ,IsTDSTaxPost=CASE WHEN  ADT.VoucherId IS NULL THEN 'ToBePost' 
													 WHEN VT.IsPark=0 THEN 'TDSPosted'
													 ELSE 'TDSParked' end
									,VT.VoucherNo TDSVoucherNo
									,iv.Id InvoiceId,IW.Id InvoiceWriteOffId
									,V.IsPark

                    FROM [TRN].[ServiceAcknowledgementMaster] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C
					 LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
					LEFT JOIN TRN.Invoice IV ON IV.ServiceAcknowledgementMasterId=IR.Id
                    LEFT JOIN TRN.Voucher V ON V.Id=IR.VoucherId
                     LEFT JOIN (SELECT A.ServiceAcknowledgementMasterId, SUM(ROUND(A.Amount,4)) AS TransactionAmount,SUM(ROUND(A.TotalTaxAmount,0)) TotalTaxAmount, SUM(ROUND(A.TotalAmount,0)) AS BaseAmount 
					 FROM [TRN].[ServiceAcknowledgementDetail] AS A
		                        JOIN [TRN].[ServiceAcknowledgementMaster] AS B ON A.ServiceAcknowledgementMasterId=B.Id WHERE B.PlantId='" + plantId + @"' 
								GROUP BY A.ServiceAcknowledgementMasterId) AS IRD ON IRD.ServiceAcknowledgementMasterId=IR.Id
					
					LEFT JOIN TRN.AdditionalTax ADT ON ADT.ServiceAcknowledgementMasterId=IR.Id
				  LEFT JOIN TRN.Voucher VT ON VT.Id=ADT.VoucherId
					LEFT JOIN TRN.InvoiceWriteOff IW ON IW.VoucherId=VT.Id
                    WHERE IR.PlantId='" + plantId + @"' 
					AND V.Archive=0 AND IR.[Status]='Posting' AND IR.IsPaymentHold=0   --AND IR.IsApproved=1
					) AS TEMP WHERE " + strkey + " order by PostingDate DESC";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

		public IEnumerable<object> GetServiceMasterServiceControlData(string entityId)
		{
			try
			{
				var sql = @"SELECT   SG.UserName AS ServiceGroup,SM.UserName ServiceMaster,SM.ServiceCategory,SM.ServiceSubCategory,HSN.Code HSNCode,SM.HSNCodeId,SM.IsPO,SM.IsApproved,SM.TransactionUoMId
						  ,SC.BudgetLimit,SC.Id,SM.Id ServiceMasterId,SC.ServiceControlId
                                    FROM [HKP].[ServiceMaster] SM
									 LEFT JOIN [HKP].[ServiceGroup] AS SG ON SG.Id=SM.ServiceGroupId
									 left join [HKP].[HSNCode] HSN ON HSN.Id=SM.HSNCodeId
									 join(select * from  [MST].[ServiceControlServiceMaster]) SC on SC.ServiceMasterId=SM.Id";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		#endregion

		public GridModel GetIssueJournalList(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText =
						@"SELECT  V.VoucherNo,II.VoucherId,V.VoucherDate,IID.PolicyAmount,IID.TransactionQty,II.Id IssueNo,II.IssueDate,MS.UserName MaterialStorageName
						,ii.OrderRefNo, IsOrderSpecificy=  CASE WHEN ii.OrderRefNo <> '' THEN 1 ELSE 0 END,II.[Types]
						,SourceNo=II.JWContractId,JW.ContractId,LC.LCRef,Customer=P.Code+' '+P.UserName ,V.IsPark
                        FROM TRN.InventoryIssue II 
                        LEFT JOIN TRN.Voucher V ON V.Id=II.VoucherId
                        LEFT JOIN (SELECT II.VoucherId,II.IssueDate,II.Id,SUM(TransactionQty) TransactionQty,SUM(PolicyAmount) PolicyAmount 
                        FROM TRN.InventoryIssueDetail ID JOIN TRN.InventoryIssue II ON II.Id=ID.InventoryIssueId
						GROUP BY II.VoucherId,II.IssueDate,II.Id) AS IID ON IID.VoucherId=V.Id
						LEFT JOIN HKP.MaterialStorage AS MS ON MS.Id=II.MaterialStorageId
						LEFT JOIN [dbo].[OSTransformationPO] JW ON JW.Id=II.JWContractId
						LEFT join dbo.[Contract] CN ON CN.Id=JW.ContractId
						LEFT JOIN dbo.MasterLC LC ON LC.Id=CN.MasterLCId
						LEFT JOIN HKP.Party P ON P.Id=LC.CustomerId
                        Where V.Archive=0 AND V.SourceType='" + SourceType.IssueJournal + @"' AND V.PlantId= '" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
		public GridModel GetIssueReturnJournalList(GridParameter parameters, string plantId)
		{
			try
			{
				parameters.CmdText =
						@"SELECT  V.VoucherNo,II.VoucherId,V.VoucherDate,IID.TotalAmount,IID.TransactionQty,II.Id IssueReturnNo,II.IssueDate,MS.UserName MaterialStorageName
						,ii.OrderRefNo, IsOrderSpecificy=  CASE WHEN ii.OrderRefNo <> '' THEN 1 ELSE 0 END,II.IssueType  [Types],V.IsPark
                        FROM TRN.InventoryIssueReturn II 
                        LEFT JOIN TRN.Voucher V ON V.Id=II.VoucherId
                        LEFT JOIN (SELECT II.VoucherId,II.IssueDate,II.Id,SUM(ID.Qty) TransactionQty,SUM(ID.TotalAmount) TotalAmount 
                        FROM TRN.InventoryIssueReturnHistory ID JOIN TRN.InventoryIssueReturn II ON II.Id=ID.InventoryIssueReturnId
						GROUP BY II.VoucherId,II.IssueDate,II.Id) AS IID ON IID.VoucherId=V.Id
						LEFT JOIN HKP.MaterialStorage AS MS ON MS.Id=II.MaterialStorageId
                        Where V.Archive=0 AND V.SourceType='" + SourceType.IssueReturnJournal + @"' AND V.PlantId= '" + plantId + "'";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public GridModel GetIssueMaterialGL(GridParameter parameters, string issueId, string companyId)
        {
            try
            {
                parameters.CmdText = @"DECLARE  @issueId varchar(10)='" + issueId + "', @companyId varchar(10)='" + companyId + @"'
                            SELECT IR.Id InventoryIssueId,IRD.Id InventoryIssueDetailId,IR.CompanyGroupId,IRD.CostCenterId
                                ,IR.CompanyId
                                ,Plant.GSTIN 
								,IR.Id PONumber                                 
                                ,REPLACE(Convert(VARCHAR(11), IR.IssueDate, 106), ' ', '-') AS PODate
		                        ,IOM.MaterialMasterId
		                        ,IR.AddedBy
		                        ,IR.AddedDate
		                        ,IR.UpdatedBy
		                        ,IR.UpdatedDate
	                          ,MM.UserName 
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMasterName
	                          ,IOM.ArticleId
	                          ,MMA.StandardName 
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IOM.FirstCharacteristicsValueId
	                          ,FCV.UserName AS FirstCharacteristicsValue
                              ,IOM.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                          ,IOM.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                          ,ROUND(IRD.TransactionQty, 2) TransactionQty
	                          ,ROUND(IRD.PolicyRate, 2) TransactionRate
	                          --,ROUND((IRD.PolicyAmount), 2) AS TrnAmount
	                          ,ROUND((IH.Amount), 2) AS TrnAmount
	                          ,IRD.BaseUOMId
	                          ,TUoM.UserName AS TransactionUoM
							  --,BI.UserName BudgetName
							  --,AI.UserName ActivityName
							  ,GLGeneralInfoId=CASE WHEN IRD.BudgetMasterId<>'' THEN BMI.GLGeneralInfoId ELSE  MGGL.ExpenseGLId END
								,GLGeneralInfoCode=CASE WHEN IRD.BudgetMasterId<>'' THEN IRD.BudgetMasterId ELSE GL.AccountCode END
								,GLGeneralInfoName=CASE WHEN IRD.BudgetMasterId<>'' THEN GLI.UserName ELSE GL.UserName END
								,GLName=CASE WHEN IRD.BudgetMasterId<>'' THEN GLI.AccountCode +'-'+ GLI.UserName ELSE GL.AccountCode +'-'+ GL.UserName END
	                            ,BudgetMasterId=CASE WHEN IRD.BudgetMasterId<>'' THEN IRD.BudgetMasterId ELSE MGGL.ExpenseBudgetMasterId END
								,BudgetCode=CASE WHEN IRD.BudgetMasterId<>'' THEN BI.Code ELSE B.Code END
								,BudgetName=CASE WHEN IRD.BudgetMasterId<>'' THEN BI.UserName ELSE B.UserName END
								,ActivityId=CASE WHEN IRD.ActivityId<>'' THEN IRD.ActivityId ELSE MGGL.ExpenseActivityId END
								,ActivityCode=CASE WHEN IRD.ActivityId<>'' THEN AI.Code ELSE A.Code END
								,ActivityName=CASE WHEN IRD.ActivityId<>'' THEN AI.UserName ELSE A.UserName END
								,PostDrGLGeneralInfoId=IH.PostDrGLGeneralInfoId
								,GAccountCode=IH.GAccountCode
							    ,GUserName=IH.GUserName
	                            , PostDrBudgetMasterId=IH.PostDrBudgetMasterId
								, BCode=IH.BCode
								, BUserName=IH.BUserName
                                , PostDrActivityId=IH.PostDrActivityId
                                , ACode=IH.ACode
								, AUserName=IH.AUserName
								,JWGLGeneralInfoId=GADJW.GLGeneralInfoId
								,JWGLGeneralInfoCode=GGLJW.AccountCode
							    ,JWGLGeneralInfoName=GGLJW.UserName
	                            , JWBudgetMasterId=GADJW.BudgetMasterId
								, JWBCode=GBJW.Code
								, JWBudgetName=GBJW.UserName
                                , JWActivityId=GADJW.ActivityId
                                , JWACode=GAJW.Code
								, JWActivityName=GAJW.UserName


                                ,IRD.BudgetMasterId IssueBudgetMasterId,IRD.ActivityId IssueActivityId
								,MGGL.ExpenseBudgetMasterId,MGGL.ExpenseActivityId
								,GAD.GLGeneralInfoId WIPGLGeneralInfoId
								,GGL.AccountCode WIPGLGeneralInfoCode
								,GGL.UserName WIPGLName
								,GAD.BudgetMasterId WIPBudgetMasterId
								,GB.UserName WIPBudgetName 
								,GAD.ActivityId WIPActivityId
								,GA.UserName WIPActivityName
                              FROM TRN.InventoryIssue IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN trn.InventoryIssueDetail IRD ON IR.Id = IRD.InventoryIssueId						                                   
                         LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
                         LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
                         LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
                         LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId = TUoM.Id
						 LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
		                        AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId=GL.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ExpenseBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BMI ON IRD.BudgetMasterId= BMI.Id
                        LEFT JOIN [HKP].[Budget] AS BI ON BMI.BudgetId= BI.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GLI ON BMI.GLGeneralInfoId=GLI.Id
                        LEFT JOIN [HKP].[Activity] AS AI ON IRD.ActivityId= AI.Id
						LEFT JOIN HKP.GeneralAccountDeterminate GAD ON GAD.COAId=Cmp.COAId and GAD.Id='IssueOfRawMaterialToAnOrder'
						 LEFT JOIN [HKP].[GLGeneralInfo] AS GGL ON GGL.Id=GAD.GLGeneralInfoId
                        LEFT JOIN[MST].[BudgetMaster] AS GBM ON GAD.BudgetMasterId= GBM.Id
                        LEFT JOIN [HKP].[Budget] AS GB ON GBM.BudgetId= GB.Id
                        LEFT JOIN [HKP].[Activity] AS GA ON GAD.ActivityId= GA.Id
						LEFT JOIN HKP.GeneralAccountDeterminate GADJW ON GADJW.COAId=Cmp.COAId and GADJW.Id='IssueOfRawMaterialForJobWork'
						 LEFT JOIN [HKP].[GLGeneralInfo] AS GGLJW ON GGLJW.Id=GADJW.GLGeneralInfoId
                        LEFT JOIN[MST].[BudgetMaster] AS GBMJW ON GADJW.BudgetMasterId= GBMJW.Id
                        LEFT JOIN [HKP].[Budget] AS GBJW ON GBMJW.BudgetId= GBJW.Id
                        LEFT JOIN [HKP].[Activity] AS GAJW ON GADJW.ActivityId= GAJW.Id
						LEFT JOIN (select distinct  InventoryIssueDetailId ,ID.PostDrGLGeneralInfoId, GL.AccountCode GAccountCode, GL.UserName GUserName
						, ID.PostDrBudgetMasterId, B.Code BCode, B.UserName BUserName, ID.PostDrActivityId, A.Code ACode, A.UserName AUserName,SUM(iih.TotalAmount) Amount
						from  [TRN].[InventoryIssueHistory] iih join TRN.InventoryReceiveDetail id on id.Id=iih.InventoryReceiveDetailId
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ID.PostDrGLGeneralInfoId=GL.Id
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON ID.PostDrBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON ID.PostDrActivityId= A.Id
						group by InventoryIssueDetailId ,ID.PostDrGLGeneralInfoId, GL.AccountCode , GL.UserName 
						, ID.PostDrBudgetMasterId, B.Code , B.UserName , ID.PostDrActivityId, A.Code , A.UserName 
						) AS IH ON IH.InventoryIssueDetailId=IRD.Id
                         WHERE IR.Id=@issueId";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

		public GridModel GetIssueReturnMaterialGL(GridParameter parameters, string issueId, string companyId)
		{
			try
			{
				parameters.CmdText = @"DECLARE  @issueId varchar(10)='" + issueId + "', @companyId varchar(10)='" + companyId + @"'
                            SELECT IR.Id InventoryIssueId,IRD.Id InventoryIssueDetailId,IR.CompanyGroupId,IRD.CostCenterId
                                ,IR.CompanyId
                                ,Plant.GSTIN 
								,IR.Id PONumber                                 
                                ,REPLACE(Convert(VARCHAR(11), IR.IssueDate, 106), ' ', '-') AS PODate
		                        ,IOM.MaterialMasterId
		                        ,IR.AddedBy
		                        ,IR.AddedDate
		                        ,IR.UpdatedBy
		                        ,IR.UpdatedDate
	                          ,MM.UserName 
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMasterName
	                          ,IOM.ArticleId
	                          ,MMA.StandardName 
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IOM.FirstCharacteristicsValueId
	                          ,FCV.UserName AS FirstCharacteristicsValue
                              ,IOM.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SecondCharacteristicsValue
	                          ,IOM.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS ThirdCharacteristicsValue
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                          ,ROUND(IRDH.Qty, 2) TransactionQty
	                          ,ROUND(IRDH.Rate, 2) TransactionRate
	                          ,ROUND((IRDH.TotalAmount), 2) AS TrnAmount
	                          ,IRD.BaseUOMId
	                          ,TUoM.UserName AS TransactionUoM
							  ,GLGeneralInfoId=CASE WHEN IRD.BudgetMasterId<>'' THEN BMI.GLGeneralInfoId ELSE  MGGL.ExpenseGLId END
								,GLGeneralInfoCode=CASE WHEN IRD.BudgetMasterId<>'' THEN IRD.BudgetMasterId ELSE GL.AccountCode END
								,GLGeneralInfoName=CASE WHEN IRD.BudgetMasterId<>'' THEN GLI.UserName ELSE GL.UserName END
								,GLName=CASE WHEN IRD.BudgetMasterId<>'' THEN GLI.AccountCode +'-'+ GLI.UserName ELSE GL.AccountCode +'-'+ GL.UserName END
	                            ,BudgetMasterId=CASE WHEN IRD.BudgetMasterId<>'' THEN IRD.BudgetMasterId ELSE MGGL.ExpenseBudgetMasterId END
								,BudgetCode=CASE WHEN IRD.BudgetMasterId<>'' THEN BI.Code ELSE B.Code END
								,BudgetName=CASE WHEN IRD.BudgetMasterId<>'' THEN BI.UserName ELSE B.UserName END
								,ActivityId=CASE WHEN IRD.ActivityId<>'' THEN IRD.ActivityId ELSE MGGL.ExpenseActivityId END
								,ActivityCode=CASE WHEN IRD.ActivityId<>'' THEN AI.Code ELSE A.Code END
								,ActivityName=CASE WHEN IRD.ActivityId<>'' THEN AI.UserName ELSE A.UserName END
								,PostDrGLGeneralInfoId=IH.PostDrGLGeneralInfoId
								,GAccountCode=IH.GAccountCode
							    ,GUserName=IH.GUserName
	                            , PostDrBudgetMasterId=IH.PostDrBudgetMasterId
								, BCode=IH.BCode
								, BUserName=IH.BUserName
                                , PostDrActivityId=IH.PostDrActivityId
                                , ACode=IH.ACode
								, AUserName=IH.AUserName
								,JWGLGeneralInfoId=GADJW.GLGeneralInfoId
								,JWGLGeneralInfoCode=GGLJW.AccountCode
							    ,JWGLGeneralInfoName=GGLJW.UserName
	                            , JWBudgetMasterId=GADJW.BudgetMasterId
								, JWBCode=GBJW.Code
								, JWBudgetName=GBJW.UserName
                                , JWActivityId=GADJW.ActivityId
                                , JWACode=GAJW.Code
								, JWActivityName=GAJW.UserName


                                ,IRD.BudgetMasterId IssueBudgetMasterId,IRD.ActivityId IssueActivityId
								,MGGL.ExpenseBudgetMasterId,MGGL.ExpenseActivityId
								,GAD.GLGeneralInfoId WIPGLGeneralInfoId
								,GGL.AccountCode WIPGLGeneralInfoCode
								,GGL.UserName WIPGLName
								,GAD.BudgetMasterId WIPBudgetMasterId
								,GB.UserName WIPBudgetName 
								,GAD.ActivityId WIPActivityId
								,GA.UserName WIPActivityName
                              FROM TRN.InventoryIssueReturn IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN trn.InventoryIssueReturnHistory IRDH ON IR.Id = IRDH.InventoryIssueReturnId						                                   
                         LEFT JOIN trn.InventoryIssueDetail IRD ON IRD.Id = IRDH.InventoryIssueDetailId						                                   
                         LEFT JOIN trn.InventoryMaterial AS IOM ON IRDH.InventoryMaterialId = IOM.Id
                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
                         LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
                         LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
                         LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId = TUoM.Id
						 LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
		                        AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId=GL.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ExpenseBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BMI ON IRD.BudgetMasterId= BMI.Id
                        LEFT JOIN [HKP].[Budget] AS BI ON BMI.BudgetId= BI.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GLI ON BMI.GLGeneralInfoId=GLI.Id
                        LEFT JOIN [HKP].[Activity] AS AI ON IRD.ActivityId= AI.Id
						LEFT JOIN HKP.GeneralAccountDeterminate GAD ON GAD.COAId=Cmp.COAId and GAD.Id='IssueOfRawMaterialToAnOrder'
						 LEFT JOIN [HKP].[GLGeneralInfo] AS GGL ON GGL.Id=GAD.GLGeneralInfoId
                        LEFT JOIN[MST].[BudgetMaster] AS GBM ON GAD.BudgetMasterId= GBM.Id
                        LEFT JOIN [HKP].[Budget] AS GB ON GBM.BudgetId= GB.Id
                        LEFT JOIN [HKP].[Activity] AS GA ON GAD.ActivityId= GA.Id
						LEFT JOIN HKP.GeneralAccountDeterminate GADJW ON GADJW.COAId=Cmp.COAId and GADJW.Id='IssueOfRawMaterialForJobWork'
						 LEFT JOIN [HKP].[GLGeneralInfo] AS GGLJW ON GGLJW.Id=GADJW.GLGeneralInfoId
                        LEFT JOIN[MST].[BudgetMaster] AS GBMJW ON GADJW.BudgetMasterId= GBMJW.Id
                        LEFT JOIN [HKP].[Budget] AS GBJW ON GBMJW.BudgetId= GBJW.Id
                        LEFT JOIN [HKP].[Activity] AS GAJW ON GADJW.ActivityId= GAJW.Id
						LEFT JOIN (select distinct  InventoryIssueDetailId ,ID.PostDrGLGeneralInfoId, GL.AccountCode GAccountCode, GL.UserName GUserName
						, ID.PostDrBudgetMasterId, B.Code BCode, B.UserName BUserName, ID.PostDrActivityId, A.Code ACode, A.UserName AUserName,SUM(iih.TotalAmount) Amount
						from  [TRN].[InventoryIssueHistory] iih join TRN.InventoryReceiveDetail id on id.Id=iih.InventoryReceiveDetailId
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ID.PostDrGLGeneralInfoId=GL.Id
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON ID.PostDrBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON ID.PostDrActivityId= A.Id
						group by InventoryIssueDetailId ,ID.PostDrGLGeneralInfoId, GL.AccountCode , GL.UserName 
						, ID.PostDrBudgetMasterId, B.Code , B.UserName , ID.PostDrActivityId, A.Code , A.UserName 
						) AS IH ON IH.InventoryIssueDetailId=IRD.Id
                         WHERE IR.Id=@issueId";
				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetInventoryShortage(string companyGroupId, string companyId, string plantId, SourceType sourceType)
        {
            var sql = @"SELECT V.VoucherNo, A.Id, A.Id AS AdjustmentNoteId, A.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, A.PartyPlantId, PP.UserName AS PartyPlantName, A.VoucherId, A.PostingDate, A.DocDate
                                , A.DocRefNo, A.CurrencyId, C.Code AS CurrencyCode, A.Amount, A.IsPark
                                FROM [TRN].[AdjustmentNote] AS A
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=A.PartyPlantId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=A.VoucherId
                                WHERE A.Archive=0 AND A.CompanyGroupId='" + companyGroupId + "'AND A.CompanyId='" + companyId + "' AND A.PlantId='" + plantId + "' AND A.SourceType='" + sourceType + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetOutSourceReceivedList(string plantId)
        {
            try
            {
                var sql = @"SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode
								, P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDateNew
			                    , CP.UserName AS PartyAccountGroupName
			                    , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
                                , Particular=CASE WHEN IR.EmployeeId<>'' THEN EI.EmployeeName WHEN IR.PartyId<>'' THEN P.UserName  ELSE P.UserName END
	                            , IR.MaterialStorageId, IR.DocRefNo, IR.DocDate
	                            , IR.GateEntryNo,PG.UserName GateEntryName, REPLACE(CONVERT(CHAR(11), GE.EntryDate, 106),' ','-') AS EntryDate
								, IR.CurrencyId, CU.Code AS CurrencyCode
								, IR.BaseCurrencyId
	                            , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo
								, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId
								, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName
								, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate, IR.ToCurrencyRate CompanyCurrencyRate
								,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END
								,IR.NoteForAccounts Narration
                                ,IR.PurchaseDocumentAcceptanceId AcceptanceId, REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AS AcceptanceDate
								, PDA.AcceptanceNo
								,IsFOC=CASE WHEN IR.IsFOC=1 THEN 'YES' ELSE 'NO' END
								,IR.GRNType
								,POId=	STUFF((select distinct ','+PO.Id from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,PODate=	STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,POVendorRefNo=	STUFF((select distinct ','+PO.DocRefNo from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,LCNo=	STUFF((select distinct ','+LC.LCRef from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN DBO.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								 ,PurchaseLCId=	STUFF((select distinct ','+LC.Id from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN DBO.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=	STUFF((select distinct ','+C.ContractNo from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														LEFT JOIN dbo.[Contract] C ON C.Id=LC.ContractId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								 ,CustomerName=	STUFF((select distinct ','+P.UserName from
														TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
														LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
														LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
														LEFT JOIN dbo.[Contract] C ON C.Id=LC.ContractId
														LEFT JOIN HKP.Party P ON P.Id=C.CustomerId
														for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    FROM [TRN].[InventoryReceive] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN [TRN].GateEntry GE ON GE.Id=IR.GateEntryNo
					LEFT JOIN dbo.PlantWiseGate PG ON PG.Id=GE.PlantWiseGateId
					LEFT JOIN TRN.PurchaseDocAcceptance PDA ON PDA.Id=IR.PurchaseDocumentAcceptanceId
					LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
					
                     LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(ROUND(A.TotalMaterialTranAmount,4)) AS TransactionAmount, SUM(ROUND(A.TotalMaterialBooksCurrencyAmount,0)) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                        JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                    WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.IsPaymentHold=0 AND IR.PlantId='" + plantId + @"' AND IR.FixedAssetOrInventory='Inventory' AND IR.OpeningBalanceId IS NULL 
					--AND IR.IsApproved=1 AND IR.RequiredPosting=1 
					AND IR.GRNType='GRNBYOS'
                    order by IR.GRNDate desc";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetInventoryOutSourceReceivedJV(string companyId, string plantId, string inveReveiveId)
        {
            try
            {

                var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"'
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId
					FROM (
						SELECT  'OutPutFinishGoodsMaterial' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId

							,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
							, SUM(IRD.TotalMaterialBooksCurrencyAmount) AS Dr, NULL Cr
							, SUM(IRD.TotalMaterialBooksCurrencyAmount) AS Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id
						WHERE IRD.InventoryReceiveId=@receiveId and IRD.MaterialFor='JWOUTPUTMaterial'
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,FAG.AssetUnderConstructionGLId,GLF.AccountCode,GLF.UserName
						,FAG.AssetUnderConstructionBudgetMasterId,BF.Code,BF.UserName
						,FAG.AssetUnderConstructionActivityId,AF.Code,AF.UserName,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                    , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					
					UNION
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr
						
						, T.Cr, T.Amount, T.IsAsset,T.InventoryReceiveDetailId
					FROM (
						SELECT  'OutPutByProductMaterial' AS OtherName, 'Dr' AS TrnType, MM.MaterialGroupMasterId, NULL AS TaxCategoryId,MM.FixedAssetMasterId

							,GLGeneralInfoId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryGLId  ELSE FAG.AssetUnderConstructionGLId END
							,GLGeneralInfoCode =case WHEN MM.IsAsset=0 THEN GL.AccountCode  ELSE GLF.AccountCode END
							,GLGeneralInfoName =case WHEN MM.IsAsset=0 THEN GL.UserName  ELSE GLF.UserName END
							,BudgetMasterId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryBudgetMasterId  ELSE FAG.AssetUnderConstructionBudgetMasterId END
							,BudgetCode =case WHEN MM.IsAsset=0 THEN B.Code  ELSE BF.Code END
							,BudgetName =case WHEN MM.IsAsset=0 THEN B.UserName  ELSE BF.UserName END
							,ActivityId =case WHEN MM.IsAsset=0 THEN MGGL.InventoryActivityId  ELSE FAG.AssetUnderConstructionActivityId END
							,ActivityCode =case WHEN MM.IsAsset=0 THEN A.Code  ELSE AF.Code END
							,ActivityName =case WHEN MM.IsAsset=0 THEN A.UserName  ELSE AF.UserName END
							
							, SUM(IRD.TotalMaterialBooksCurrencyAmount) AS Dr, NULL Cr
							, SUM(IRD.TotalMaterialBooksCurrencyAmount) AS Amount
                            ,MM.IsAsset,IRD.Id AS  InventoryReceiveDetailId
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.InventoryGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.InventoryBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.InventoryActivityId= A.Id
						LEFT JOIN (SELECT FAMBT.BudgetMasterId,FAMG.AssetUnderConstructionGLId ,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId 
						FROM HKP.FixedAssetMasterBudgetTag FAMBT LEFT JOIN HKP.FixedAssetMasterGL FAMG ON FAMBT.FixedAssetMasterId=FAMG.FixedAssetMasterId) AS FAG 
						ON FAG.BudgetMasterId=MM.BudgetMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON FAG.AssetUnderConstructionGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON FAG.AssetUnderConstructionBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON FAG.AssetUnderConstructionActivityId= AF.Id
						WHERE IRD.InventoryReceiveId=@receiveId and IRD.MaterialFor='JWBYPRODUCTMaterial'
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    ,MM.IsAsset,MM.FixedAssetMasterId,FAG.AssetUnderConstructionGLId,GLF.AccountCode,GLF.UserName
						,FAG.AssetUnderConstructionBudgetMasterId,BF.Code,BF.UserName
						,FAG.AssetUnderConstructionActivityId,AF.Code,AF.UserName,IRD.Id
                    ) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId
                     , T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId,T.IsAsset, T.InventoryReceiveDetailId
					
                    UNION
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[InventoryReceiveTax] AS IRT
					LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRT.InventoryReceiveDetailId=IRD.Id
                    LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
					LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN (SELECT C.AddressMasterId, MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
							AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRT.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE IRD.InventoryReceiveId=@receiveId AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IRT.InventoryReceiveDetailId<>'' AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY  IRT.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					SELECT 'TCS' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId, TC.TaxCategoryId
						, TCGL.CreditableGLId  GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.CreditableGLBudgetMasterId BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.CreditableGLActivityId ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRT.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRT.TaxAmount) AS Amount
                        ,0 IsAsset, NULL InventoryReceiveDetailId
					FROM [TRN].[InventoryReceiveAdditionalTax] AS IRT
					LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRT.InventoryReceiveId
					LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TCGL.TaxCodeId=IRT.TaxCodeId
					LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id=TCGL.TaxCodeId
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.CreditableGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.CreditableGLBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.CreditableGLActivityId= A.Id
					WHERE IR.Id=@receiveId  AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY  TC.TaxCategoryId, TCGL.CreditableGLId, GL.AccountCode, GL.UserName, TCGL.CreditableGLBudgetMasterId
					, B.Code, B.UserName, TCGL.CreditableGLActivityId, A.Code, A.UserName
					UNION
					SELECT 'Tax' AS OtherName, 'Dr' AS TrnType, NULL AS MaterialGroupMasterId, IRTS.TaxCategoryId
						, TCGL.GLGeneralInfoId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TCGL.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TCGL.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						, SUM(IRTS.TaxAmount) AS  Dr, NULL Cr
						, SUM(IRTS.TaxAmount) AS Amount
                       ,0 IsAsset, NULL InventoryReceiveDetailId
					--, IRTS.TaxAmount
					--, IRTS.TaxAmount
					FROM [TRN].[InventoryReceiveTax] AS IRTS
					LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
					LEFT JOIN [TRN].[InventoryService] AS INS ON INS.Id=IRTS.InventoryServiceId
					LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=IRTS.TaxCategoryId
					LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id AND IRTS.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.GLGeneralInfoId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.BudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TCGL.ActivityId= A.Id
					WHERE INS.InventoryReceiveId=@receiveId --AND IR.IsNonCreditable=0 
					AND IRTS.InventoryServiceId<>'' AND TCGL.InputTaxOutPutTax='Input' AND ISNULL(TCGL.TaxType,'')<>'RCM' AND IR.PurchaseDocumentAcceptanceId IS NULL
					GROUP BY IRTS.TaxCategoryId, TCGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, TCGL.BudgetMasterId, B.Code, B.UserName, TCGL.ActivityId, A.Code, A.UserName
					UNION
					
					SELECT T.OtherName, T.TrnType, T.MaterialGroupMasterId, T.TaxCategoryId
						, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName
						, T.BudgetMasterId, T.BudgetCode, T.BudgetName
						, T.ActivityId, T.ActivityCode, T.ActivityName
						, T.Dr, T.Cr 
                            --+ (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
							--WHERE IRTS.InventoryReceiveId=@receiveId AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ()) AS Cr
						, T.Amount ,T.IsAsset, NULL InventoryReceiveDetailId
                            --+ (ISNULL((SELECT SUM(IRTS.TaxAmount) AS Amount FROM [TRN].[InventoryReceiveTax] AS IRTS JOIN [TRN].[InventoryReceive] AS IR ON IRTS.InventoryReceiveId=IR.Id
							--WHERE IRTS.InventoryReceiveId=@receiveId AND IRTS.InventoryServiceId<>''),0)/COUNT(*) OVER ())AS Amount
					FROM (
						SELECT 'Vendor' AS OtherName,'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
						, MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName
							, MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName,NULL Dr
						,SUM(MAT.Cr) +SUM(ISNULL(TCS.TCSAmount,0)) AS Cr,--+SUM(ISNULL(SRV.TotalTaxAmount,0))
						SUM(MAT.Cr) +SUM(ISNULL(TCS.TCSAmount,0))  AS Amount --+SUM(ISNULL(SRV.TotalTaxAmount,0))
                        ,0 IsAsset
						FROM (
							SELECT IR.Id, 'Vendor' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,GLGeneralInfoId =GAD.GLGeneralInfoId
							,GLGeneralInfoCode =GL.AccountCode
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId = GAD.BudgetMasterId  
							,BudgetCode =B.Code
							,BudgetName =B.UserName
							,ActivityId =GAD.ActivityId
							,ActivityCode = A.Code
							,ActivityName = A.UserName

							, NULL Dr, SUM(IRD.TotalMaterialTranAmount) + SUM(IRD.TotalTaxAmount)+ SUM(IRD.ChargesTaxTranAmount)  AS  Cr
							, SUM(IRD.TotalMaterialTranAmount) + SUM(IRD.TotalTaxAmount)+ SUM(IRD.ChargesTaxTranAmount) AS Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN ORG.Company C ON C.Id=IR.CompanyId
						LEFT JOIN [HKP].[GeneralAccountDeterminate] GAD ON GAD.COAId=C.COAId AND GAD.Id='ReceiveGoodsFromJobWork'
						--JOIN [TRN].[InventoryService] AS INS ON IRD.InventoryReceiveId=INS.InventoryReceiveId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
						
						JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON GAD.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id

						WHERE IRD.InventoryReceiveId=@receiveId
						GROUP BY  IR.Id, GAD.GLGeneralInfoId, GL.AccountCode, GL.UserName, GAD.BudgetMasterId, B.Code, B.UserName, GAD.ActivityId, A.Code, A.UserName
						,MM.IsAsset
						) AS MAT
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.Amount) AS Amount,sum(INS.TotalTaxAmount) AS TotalTaxAmount
						from  [TRN].[InventoryService] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.InventoryReceiveId 
                        where InventoryReceiveId=@receiveId group by INS.InventoryReceiveId
						) AS SRV on SRV.InventoryReceiveId=MAT.Id
						LEFT OUTER JOIN (
						SELECT INS.InventoryReceiveId, sum(INS.TaxAmount) AS TCSAmount
						from  [TRN].[InventoryReceiveAdditionalTax] AS INS
						LEFT JOIN TRN.InventoryReceive AS IR ON IR.Id=INS.InventoryReceiveId 
                        where InventoryReceiveId=@receiveId group by INS.InventoryReceiveId
						) AS TCS on TCS.InventoryReceiveId=MAT.Id

						GROUP BY  MAT.Id,MAT.GLGeneralInfoId, MAT.GLGeneralInfoCode, MAT.GLGeneralInfoName , MAT.BudgetMasterId, MAT.BudgetCode, MAT.BudgetName
							, MAT.ActivityId, MAT.ActivityCode, MAT.ActivityName
					) AS T
					GROUP BY T.MaterialGroupMasterId, T.GLGeneralInfoId, T.GLGeneralInfoCode, T.GLGeneralInfoName, T.BudgetMasterId, T.BudgetCode, T.BudgetName, T.ActivityId, T.ActivityCode, T.ActivityName, T.Dr, T.Cr, T.Amount, T.OtherName, T.TrnType,T.TaxCategoryId, T.IsAsset
					ORDER BY T.TrnType DESC ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetInventoryOutSourceGIRI(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)= '" + inveReveiveId + @"',@plantId varchar(10)='" + plantId + @"'

						SELECT  'JobWork' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =SVGL.ServiceGLId
							,GLGeneralInfoCode =GLF.AccountCode
							,GLGeneralInfoName =GLF.UserName
							,BudgetMasterId =SVGL.ServiceBudgetMasterId
							,BudgetCode =BF.Code
							,BudgetName =BF.UserName
							,ActivityId =SVGL.ServiceActivityId
							,ActivityCode =AF.Code 
							,ActivityName = AF.UserName 
							, SUM(IRD.TransactionQty*JWTCC.RatePerUnit) AS Dr, NULL Cr
							, SUM(IRD.TransactionQty*JWTCC.RatePerUnit) AS Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [MST].[JobWorkTransformationMaster] JWTM ON JWTM.Id=IRD.OSTransformationPOId
						LEFT JOIN dbo.OSTransformationPODetail JWTCC ON JWTCC.Id=IRD.OSTransformationPODetailId
						LEFT JOIN HKP.ServiceMaster SM ON SM.Id=JWTCC.ServiceId
						LEFT JOIN HKP.ServiceGroup SVG ON SVG.Id=SM.ServiceGroupId
						LEFT JOIN HKP.ServiceGroupGL SVGL ON SVGL.ServiceGroupId=SVG.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON SVGL.ServiceGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON SVGL.ServiceBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON SVGL.ServiceActivityId= AF.Id
						WHERE IRD.InventoryReceiveId=@receiveId and IRD.MaterialFor='JWOUTPUTMaterial' AND IR.PlantId=@plantId
						GROUP BY SVGL.ServiceGLId,SVGL.ServiceBudgetMasterId,SVGL.ServiceActivityId,GLF.AccountCode,GLF.UserName
						,BF.Code,BF.UserName
						,AF.Code,AF.UserName

						UNION ALL

						SELECT  'GRIR' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =SVGL.ClearingAccountGLId
							,GLGeneralInfoCode =GLF.AccountCode
							,GLGeneralInfoName =GLF.UserName
							,BudgetMasterId =SVGL.ClearingAccountBudgetMasterId
							,BudgetCode =BF.Code
							,BudgetName =BF.UserName
							,ActivityId =SVGL.ClearingAccountActivityId
							,ActivityCode =AF.Code 
							,ActivityName = AF.UserName 
							, NULL Dr, SUM(IRD.TransactionQty*JWTCC.RatePerUnit) AS Cr
							, SUM(IRD.TransactionQty*JWTCC.RatePerUnit) AS Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [MST].[JobWorkTransformationMaster] JWTM ON JWTM.Id=IRD.OSTransformationPOId
						LEFT JOIN dbo.OSTransformationPODetail JWTCC ON JWTCC.Id=IRD.OSTransformationPODetailId
						LEFT JOIN HKP.ServiceMaster SM ON SM.Id=JWTCC.ServiceId
						LEFT JOIN HKP.ServiceGroup SVG ON SVG.Id=SM.ServiceGroupId
						LEFT JOIN HKP.ServiceGroupGL SVGL ON SVGL.ServiceGroupId=SVG.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON SVGL.ClearingAccountGLId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON SVGL.ClearingAccountBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON SVGL.ClearingAccountActivityId= AF.Id
						WHERE IRD.InventoryReceiveId=@receiveId and IRD.MaterialFor='JWOUTPUTMaterial' AND IR.PlantId=@plantId
						GROUP BY SVGL.ClearingAccountGLId,SVGL.ClearingAccountBudgetMasterId,SVGL.ClearingAccountActivityId,GLF.AccountCode,GLF.UserName
						,BF.Code,BF.UserName
						,AF.Code,AF.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetInventoryOSServiceMasterData(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)= '" + inveReveiveId + @"',@plantId varchar(10)='" + plantId + @"'
						SELECT top(1) OSPO.CurrencyId,CU.Code CurrencyCode,OSPO.ToCurrencyRate 
						FROM dbo.OSTransformationPO OSPO 
						JOIN TRN.InventoryReceive IR ON IR.TransformationContractId=OSPO.Id
						LEFT JOIN SCS.Currency CU ON CU.Id=OSPO.CurrencyId
						WHERE IR.Id=@receiveId AND IR.PlantId=@plantId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetInventoryOutSourceWIP(string companyId, string plantId, string inveReveiveId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)= '" + inveReveiveId + @"' , @companyId varchar(10)='" + companyId + @"',@plantId varchar(10)='" + plantId + @"'

						SELECT  'CostOfGoodsSold' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =MGGL.ExpenseGLId
							,GLGeneralInfoCode =GL.AccountCode
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =MGGL.ExpenseBudgetMasterId
							,BudgetCode =B.Code
							,BudgetName =B.UserName
							,ActivityId =MGGL.ExpenseActivityId
							,ActivityCode =A.Code 
							,ActivityName = A.UserName 
							, SUM(IRD.TotalMaterialBooksCurrencyAmount) AS Dr, NULL Cr
							, SUM(IRD.TotalMaterialBooksCurrencyAmount) AS Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ExpenseBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id
						WHERE IRD.InventoryReceiveId=@receiveId and IRD.MaterialFor='JWOUTPUTMaterial' AND IR.PlantId=@plantId
						GROUP BY MGGL.ExpenseGLId,GL.AccountCode,GL.UserName,MGGL.ExpenseBudgetMasterId
						,B.Code,B.UserName,MGGL.ExpenseActivityId,A.Code,A.UserName

						UNION

						SELECT  'JobWorkWIP' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =GAD.GLGeneralInfoId
							,GLGeneralInfoCode =GL.AccountCode
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =GAD.BudgetMasterId
							,BudgetCode =B.Code
							,BudgetName =B.UserName
							,ActivityId =GAD.ActivityId
							,ActivityCode =A.Code 
							,ActivityName = A.UserName 
							,NULL  Dr, SUM(IRD.TotalMaterialBooksCurrencyAmount) AS Cr
							, SUM(IRD.TotalMaterialBooksCurrencyAmount) AS Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN ORG.Company C ON C.Id=IR.CompanyId
						LEFT JOIN [HKP].[GeneralAccountDeterminate] GAD ON GAD.COAId=C.COAId AND GAD.Id='IssueOfRawMaterialForJobWork'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id
						WHERE IRD.InventoryReceiveId=@receiveId and IRD.MaterialFor='JWOUTPUTMaterial' AND IR.PlantId=@plantId
						GROUP BY GAD.GLGeneralInfoId,GL.AccountCode,GL.UserName,GAD.BudgetMasterId
						,B.Code,B.UserName,GAD.ActivityId,A.Code,A.UserName";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetOutSourcePostedList(string column, string value, string plantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        select top 100 * from (SELECT IR.Id,IR.Id GRNNo, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate,  P.Code AS PartyCode, P.UserName AS PartyName
                                    , Particular= P.UserName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, CU.Code AS CurrencyCode
	                          ,V.Id VoucherId ,VD.DrAmount Amount
									,IR.GateEntryNo,IR.ToCurrencyRate,IR.NoteForAccounts Narration
									,VoucherNo = V.VoucherNo
									,PostingDate= REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-')
									,[Type] ='JW WIP'
									,V.SourceType
						FROM [TRN].[InventoryReceive] AS IR 
						LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
						LEFT JOIN TRN.Voucher V ON  V.Id=IR.JWWIPVoucherId
						LEFT JOIN(SELECT VoucherId,SUM(DrAmount) DrAmount FROM  TRN.VoucherDetail GROUP BY VoucherId) VD ON VD.VoucherId=V.Id
                        WHERE V.Archive=0 AND IR.PlantId=@plantId AND IR.JWWIPVoucherId<>''

						UNION ALL
						SELECT IR.Id,IR.Id GRNNo, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate,  P.Code AS PartyCode, P.UserName AS PartyName
                                    , Particular= P.UserName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, CU.Code AS CurrencyCode
	                           ,V.Id VoucherId	     ,VD.DrAmount Amount
									,IR.GateEntryNo,IR.ToCurrencyRate,IR.NoteForAccounts Narration
									,VoucherNo = V.VoucherNo
									,PostingDate= REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-')
									,[Type] ='JW ChangeInInv' 
									,V.SourceType
						FROM [TRN].[InventoryReceive] AS IR 
						LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
						LEFT JOIN TRN.Voucher V ON  V.Id=IR.JWChangeInInvVoucherId
						LEFT JOIN(SELECT VoucherId,SUM(DrAmount) DrAmount FROM  TRN.VoucherDetail GROUP BY VoucherId) VD ON VD.VoucherId=V.Id
                        WHERE V.Archive=0 AND IR.PlantId=@plantId AND IR.JWChangeInInvVoucherId<>''

						UNION ALL
						SELECT IR.Id,IR.Id GRNNo, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate,  P.Code AS PartyCode, P.UserName AS PartyName
                                    , Particular= P.UserName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, CU.Code AS CurrencyCode
	                               ,V.Id VoucherId ,VD.DrAmount Amount
									,IR.GateEntryNo,IR.ToCurrencyRate,IR.NoteForAccounts Narration
									,VoucherNo = V.VoucherNo
									,PostingDate= REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-')
									,[Type] ='JW GIRI' 
									,V.SourceType
						FROM [TRN].[InventoryReceive] AS IR 
						LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
						LEFT JOIN TRN.Voucher V ON  V.Id=IR.JWGRIRVoucherId
						LEFT JOIN(SELECT VoucherId,SUM(DrAmount) DrAmount FROM  TRN.VoucherDetail GROUP BY VoucherId) VD ON VD.VoucherId=V.Id
                        WHERE V.Archive=0 AND IR.PlantId=@plantId AND IR.JWGRIRVoucherId<>'') AS TEMP WHERE " + strkey + " order by PostingDate DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #region Post Invoice
        public IEnumerable<object> GetGRNListForPostInvoice(string plantId)
        {
            try
            {
                var sql = @"SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode
		, P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDateNew
		, CP.UserName AS PartyAccountGroupName
		, IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
        , Particular=CASE WHEN IR.EmployeeId<>'' THEN EI.EmployeeName WHEN IR.PartyId<>'' THEN P.UserName  ELSE P.UserName END
	    , IR.MaterialStorageId, IR.DocRefNo, IR.DocDate
	    , IR.GateEntryNo,PG.UserName GateEntryName, REPLACE(CONVERT(CHAR(11), GE.EntryDate, 106),' ','-') AS EntryDate
		, IR.CurrencyId, CU.Code AS CurrencyCode
		, IR.BaseCurrencyId
	    , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo
		, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	    , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId
		, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	    , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
        , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName,PT.PaymentMode
		, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate, IR.ToCurrencyRate CompanyCurrencyRate
		,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END
		,IR.NoteForAccounts Narration
        ,IR.PurchaseDocumentAcceptanceId AcceptanceId, REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AS AcceptanceDate
		, PDA.AcceptanceNo
		,IsFOC=CASE WHEN IR.IsFOC=1 THEN 'YES' ELSE 'NO' END
		,IR.GRNType
		,POId=	STUFF((select distinct ','+PO.Id from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
		,PODate=	STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
		,POVendorRefNo=	STUFF((select distinct ','+PO.DocRefNo from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
		,LCNo=	STUFF((select distinct ','+LC.LCRef from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
								LEFT JOIN DBO.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
								for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			,PurchaseLCId=	STUFF((select distinct ','+LC.Id from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
								LEFT JOIN DBO.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
								for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
		,ContractNo=	STUFF((select distinct ','+C.ContractNo from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
								LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
								LEFT JOIN dbo.[Contract] C ON C.Id=LC.ContractId
								for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			,CustomerName=	STUFF((select distinct ','+P.UserName from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
								LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
								LEFT JOIN dbo.[Contract] C ON C.Id=LC.ContractId
								LEFT JOIN HKP.Party P ON P.Id=C.CustomerId
								for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			, RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
            , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
            , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
FROM [TRN].[InventoryReceive] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
LEFT JOIN (
SELECT C.Id,C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable 
FROM [HKP].[CompanyParty] AS C 
LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
) AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId

		LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
LEFT JOIN [TRN].GateEntry GE ON GE.Id=IR.GateEntryNo
LEFT JOIN dbo.PlantWiseGate PG ON PG.Id=GE.PlantWiseGateId
LEFT JOIN TRN.PurchaseDocAcceptance PDA ON PDA.Id=IR.PurchaseDocumentAcceptanceId
LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
					
LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(ROUND(A.TotalMaterialTranAmount,4)) AS TransactionAmount, SUM(ROUND(A.TotalMaterialBooksCurrencyAmount,0)) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')='Posting' AND ISNULL(IR.VoucherId,'')<>'' AND IR.IsPaymentHold=0 AND IR.PlantId='" + plantId + @"' AND IR.FixedAssetOrInventory='Inventory' AND IR.OpeningBalanceId IS NULL 
AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.GRNType!='MaterialTransfer' AND IR.Id NOT IN(Select InventoryReceiveId FROM [TRN].[Invoice] where ISNULL(InventoryReceiveId,'')<>'')
AND IR.Id NOT IN(Select InventoryReceiveId FROM [TRN].EmployeePayable where ISNULL(InventoryReceiveId,'')<>'')
--AND IR.Id NOT IN(Select distinct InventoryReceiveId from [dbo].[PostGRNInvoiceDetail])
order by IR.GRNDate desc";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetGRNDetailListForPostInvoice(string inventoryReceiveId, string masterId)
        {
            try
            {
                var sql = @"SELECT Activ=CAST (CASE WHEN PID.Id IS NULL THEN 0 ELSE 1 END AS bit),PID.Id,IRD.InventoryReceiveId ,IRD.Id InventoryReceiveDetailId,MGM.UserName AS MaterialGroupMasterName,MM.Id MaterialMasterId
	                        ,MM.UserName MaterialMaster,IRD.MaterialStorageId,IRD.BaseUOMId,IM.ArticleId,ART.StandardName Article,IM.FirstCharacteristicsId,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId,FCV.UserName AS FirstCharacteristicsValue,IM.SecondCharacteristicsId,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId,SCV.UserName AS SecondCharacteristicsValue,IM.ThirdCharacteristicsId,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId,TCV.UserName AS ThirdCharacteristicsValue,0 AS BaseTaxAmount,0 AS TaxAmount,0 AS ChargesAmount
	                         ,0 AS ServiceCharge,0 AS ServiceTax,IRD.CountryId,IRD.TotalMaterialTranAmount,IRD.TotalMaterialBooksCurrencyAmount BooksAmount
							,IRD.TransactionQty GRNQty,ISNULL(PIND.OtherQty,0) OtherQty,PID.TransactionQty
							,TransactionRate=(IRD.TotalMaterialTranAmount/IRD.TransactionQty),ISNULL(PID.TransactionAmount,0) TransactionAmount
							,IRD.TransactionUoMId,TUoM.UserName AS TransactionUoM,CU.Code AS CurrencyName,IR.ToCurrencyRate,Balance=IRD.TransactionQty-(ISNULL(PIND.OtherQty,0)+PID.TransactionQty)				
                        FROM TRN.[InventoryReceiveDetail] AS IRD  
						LEFT JOIN TRN.InventoryReceive AS IR ON IRD.InventoryReceiveId = IR.Id
						LEFT JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId = IM.Id
                        LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=IM.MaterialMasterId
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id                    
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
						LEFT JOIN dbo.PostGRNInvoiceDetail PID ON PID.InventoryReceiveId=IRD.InventoryReceiveId AND PID.InventoryReceiveDetailId=IRD.Id AND PID.PostGRNInvoiceId='" + masterId + @"'
						LEFT JOIN (SELECT InventoryReceiveId,InventoryReceiveDetailId,ISNULL(SUM(TransactionQty),0) OtherQty FROM dbo.PostGRNInvoiceDetail WHERE PostGRNInvoiceId<>'" + masterId + @"'
                            GROUP BY InventoryReceiveId,InventoryReceiveDetailId) PIND ON PIND.InventoryReceiveId=IRD.InventoryReceiveId AND PIND.InventoryReceiveDetailId=IRD.Id
                        Where IRD.InventoryReceiveId " + inventoryReceiveId + "";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetPostInvoiceDetailGL(string inventoryReceiveId)
        {
            try
            {
                string sql = @"SELECT GL.UserName GLGeneralInfoName,GL.AccountCode GLGeneralInfoCode,VD.GLGeneralInfoId
				,B.UserName BudgetName,B.Code BudgetCode,VD.BudgetMasterId,A.UserName ActivityName,VD.ActivityId,VD.CrAmount DrAmount
				from TRN.VoucherDetail VD
				LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=VD.GLGeneralInfoId
				LEFT JOIN MSt.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
				LEFT JOIN HKP.Activity A ON A.Id=VD.ActivityId
				JOIN TRN.Voucher V ON V.Id=VD.VoucherId
				JOIN TRN.InventoryReceive IR ON IR.VoucherId=V.Id
				JOIN HKP.Budget B ON B.Id=BM.BudgetId
				WHERE IR.Id='" + inventoryReceiveId + @"' AND VD.CrAmount>0";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPostInvoiceList(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                string sql = @"select top 100 * from (SELECT PGI.*,P.UserName PartyName,C.Code Currency,FORMAT(PGI.InvoiceDate,'dd-MMM-yyyy') InvDate,PGD.Amount,V.VoucherNo
							,IsPark=CASE WHEN V.IsPark IS NULL THEN 'ToBePost' WHEN V.IsPark=1 then 'Parked'  else 'Posted' end
							FROM [dbo].[PostGRNInvoice] PGI
                            LEFT JOIN HKP.Party P ON P.Id=PGI.PartyId
                            LEFT JOIN SCS.Currency C ON C.Id=PGI.CurrencyId
							LEFT JOIN TRN.Voucher V ON V.Id=PGI.VoucherId
							LEFT JOIN (SELECT PostGRNInvoiceId,SUM(TransactionAmount) Amount 
									FROM dbo.PostGRNInvoiceDetail GROUP BY PostGRNInvoiceId) PGD ON PGD.PostGRNInvoiceId=PGI.Id
							--WHERE  V.Archive=0
							) AS TEMP WHERE " + strkey + "";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetPostableList(string id)
        {
            try
            {
                string sql = @"SELECT PGI.*,P.UserName PartyName,C.Code Currency,FORMAT(PGI.InvoiceDate,'dd-MMM-yyyy') InvDate ,PGD.Amount
							FROM [dbo].[PostGRNInvoice] PGI
                            LEFT JOIN HKP.Party P ON P.Id=PGI.PartyId
                            LEFT JOIN SCS.Currency C ON C.Id=PGI.CurrencyId 
							LEFT JOIN (SELECT SUM(TransactionAmount) Amount,PostGRNInvoiceId 
									FROM dbo.PostGRNInvoiceDetail GROUP BY PostGRNInvoiceId) PGD ON PGD.PostGRNInvoiceId=PGI.Id 
							where PGI.Id='" + id + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPostInvoiceDetailData(string masterId)
        {
            try
            {
                string sql = @"SELECT PGD.Id,IRD.InventoryReceiveId ,IRD.Id InventoryReceiveDetailId,MGM.UserName AS MaterialGroupMasterName,MM.Id MaterialMasterId
							,MM.UserName MaterialMaster,IRD.MaterialStorageId,IRD.BaseUOMId,IM.ArticleId,ART.StandardName Article,IM.FirstCharacteristicsId,FC.UserName AS FirstCharacteristics
							,IM.FirstCharacteristicsValueId,FCV.UserName AS FirstCharacteristicsValue,IM.SecondCharacteristicsId,SC.UserName AS SecondCharacteristics
							,IM.SecondCharacteristicsValueId,SCV.UserName AS SecondCharacteristicsValue,IM.ThirdCharacteristicsId,TC.UserName AS ThirdCharacteristics
							,IM.ThirdCharacteristicsValueId,TCV.UserName AS ThirdCharacteristicsValue,0 AS BaseTaxAmount,0 AS TaxAmount,0 AS ChargesAmount
							,0 AS ServiceCharge,0 AS ServiceTax,IRD.CountryId,IRD.TransactionQty,TransactionAmount=FORMAT((FORMAT((IRD.TotalMaterialTranAmount/IRD.TransactionQty),'N4')*IRD.TransactionQty),'N2')
							,TransactionRate=FORMAT((IRD.TotalMaterialTranAmount/IRD.TransactionQty),'N4')
							,IRD.TransactionUoMId,TUoM.UserName AS TransactionUoM,CU.Code AS CurrencyName,IR.ToCurrencyRate						
						FROM [dbo].[PostGRNInvoiceDetail] PGD
						LEFT JOIN TRN.[InventoryReceiveDetail] AS IRD  ON PGD.InventoryReceiveId = IRD.InventoryReceiveId AND PGD.InventoryReceiveDetailId = IRD.Id
						LEFT JOIN TRN.InventoryReceive AS IR ON IRD.InventoryReceiveId = IR.Id
						LEFT JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId = IM.Id
						LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=IM.MaterialMasterId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id                    
						LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id
						Where PGD.PostGRNInvoiceId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSavedGRNListForPostInvoice(string plantId, string masterId)
        {
            try
            {
                var sql = @"SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode
		, P.UserName AS PartyName,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDateNew
		, CP.UserName AS PartyAccountGroupName
		, IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
        , Particular=CASE WHEN IR.EmployeeId<>'' THEN EI.EmployeeName WHEN IR.PartyId<>'' THEN P.UserName  ELSE P.UserName END
	    , IR.MaterialStorageId, IR.DocRefNo, IR.DocDate
	    , IR.GateEntryNo,PG.UserName GateEntryName, REPLACE(CONVERT(CHAR(11), GE.EntryDate, 106),' ','-') AS EntryDate
		, IR.CurrencyId, CU.Code AS CurrencyCode
		, IR.BaseCurrencyId
	    , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo
		, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	    , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId
		, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	    , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
        , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName,PT.PaymentMode
		, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate, IR.ToCurrencyRate CompanyCurrencyRate
		,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END
		,IR.NoteForAccounts Narration
        ,IR.PurchaseDocumentAcceptanceId AcceptanceId, REPLACE(CONVERT(CHAR(11), PDA.AcceptanceDate, 106),' ','-') AS AcceptanceDate
		, PDA.AcceptanceNo
		,IsFOC=CASE WHEN IR.IsFOC=1 THEN 'YES' ELSE 'NO' END
		,IR.GRNType
		,POId=	STUFF((select distinct ','+PO.Id from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
		,PODate=	STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
		,POVendorRefNo=	STUFF((select distinct ','+PO.DocRefNo from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
		,LCNo=	STUFF((select distinct ','+LC.LCRef from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
								LEFT JOIN DBO.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
								for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			,PurchaseLCId=	STUFF((select distinct ','+LC.Id from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
								LEFT JOIN DBO.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
								for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
		,ContractNo=	STUFF((select distinct ','+C.ContractNo from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
								LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
								LEFT JOIN dbo.[Contract] C ON C.Id=LC.ContractId
								for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			,CustomerName=	STUFF((select distinct ','+P.UserName from
								TRN.InventoryReceiveDetail XVD JOIN TRN.InventoryReceive AS XP ON XP.Id=XVD.InventoryReceiveId AND IR.Id=XVD.InventoryReceiveId
								LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=XVD.POId
								LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
								LEFT JOIN dbo.[Contract] C ON C.Id=LC.ContractId
								LEFT JOIN HKP.Party P ON P.Id=C.CustomerId
								for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			, RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
            , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
            , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
FROM [TRN].[InventoryReceive] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
LEFT JOIN (
SELECT C.Id,C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable 
FROM [HKP].[CompanyParty] AS C 
LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor'
) AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId

		LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
LEFT JOIN [TRN].GateEntry GE ON GE.Id=IR.GateEntryNo
LEFT JOIN dbo.PlantWiseGate PG ON PG.Id=GE.PlantWiseGateId
LEFT JOIN TRN.PurchaseDocAcceptance PDA ON PDA.Id=IR.PurchaseDocumentAcceptanceId
LEFT JOIN dbo.PurchaseLC PLC ON PLC.Id=PDA.PurchaseLCId
					
LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(ROUND(A.TotalMaterialTranAmount,4)) AS TransactionAmount, SUM(ROUND(A.TotalMaterialBooksCurrencyAmount,0)) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')='Posting' AND ISNULL(IR.VoucherId,'')<>'' AND IR.IsPaymentHold=0 AND IR.PlantId='" + plantId + @"' AND IR.FixedAssetOrInventory='Inventory' AND IR.OpeningBalanceId IS NULL 
AND IR.IsApproved=1 AND IR.RequiredPosting=1 AND IR.GRNType!='MaterialTransfer' AND IR.Id NOT IN(Select InventoryReceiveId FROM [TRN].[Invoice] where ISNULL(InventoryReceiveId,'')<>'')
AND IR.Id NOT IN(Select InventoryReceiveId FROM [TRN].EmployeePayable where ISNULL(InventoryReceiveId,'')<>'')
AND IR.Id IN(Select distinct InventoryReceiveId from [dbo].[PostGRNInvoiceDetail]  Where PostGRNInvoiceId='" + masterId + @"')
order by IR.GRNDate desc";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetPostableJVList(string companyId, string plantId, string postGRNInvoiceId, string partyId)
        {
            try
            {

                var companyParty = GetCompanyPartyGroup(partyId, plantId);

                string sql = @"DECLARE @receiveId varchar(10)='" + postGRNInvoiceId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + "', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"'
						SELECT distinct  'Provisonal' AS OtherName, 'Dr' AS TrnType ,NULL MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,GLGeneralInfoId= IRD.PostCRGLGeneralInfoId
							,GLGeneralInfoCode =GL.AccountCode
							,GLGeneralInfoName = GL.UserName
							,BudgetMasterId =IRD.PostCRBudgetMasterId
							,BudgetCode =B.Code
							,BudgetName =B.UserName
							,ActivityId =IRD.PostCRActivityId
							,ActivityCode =A.Code
							,ActivityName =A.UserName
							,SUM(ISNULL(PGD.TransactionAmount,0)) Dr
							,0 Cr
							,SUM(ISNULL(PGD.TransactionAmount,0)) Amount
							,SUM(ISNULL(PGD.TransactionAmount,0)) BaseDrAmount
							,0 BaseCrAmount
						FROM dbo.PostGRNInvoiceDetail PGD 
						LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=PGD.InventoryReceiveDetailId
						LEFT JOIN dbo.PostGRNInvoice PGI ON PGI.Id=PGD.PostGRNInvoiceId
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON IRD.PostCRGLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON IRD.PostCRBudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON IRD.PostCRActivityId= A.Id
						WHERE PGD.PostGRNInvoiceId=@receiveId
						GROUP BY IRD.PostCRGLGeneralInfoId ,GL.AccountCode ,GL.UserName ,IRD.PostCRBudgetMasterId ,B.Code
							,B.UserName ,IRD.PostCRActivityId ,A.Code ,A.UserName
						
						UNION ALL

                        SELECT distinct 'Vendor' AS OtherName, 'Cr' AS TrnType ,MM.MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,GLGeneralInfoId= MGPGL.GLGeneralInfoId
							,GLGeneralInfoCode =GL.AccountCode
							,GLGeneralInfoName = GL.UserName
							,BudgetMasterId =MGPGL.BudgetMasterId
							,BudgetCode =B.Code
							,BudgetName =B.UserName
							,ActivityId =MGPGL.ActivityId
							,ActivityCode =A.Code
							,ActivityName =A.UserName
							,0 Dr
							,SUM(ISNULL(PGD.TransactionAmount,0)) Cr
							,SUM(ISNULL(PGD.TransactionAmount,0)) Amount
							,0 BaseDrAmount
							,SUM(ISNULL(PGD.TransactionAmount,0)) BaseCrAmount
						FROM dbo.PostGRNInvoiceDetail PGD 
						LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=PGD.InventoryReceiveDetailId
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN dbo.PostGRNInvoice PGI ON PGI.Id=PGD.PostGRNInvoiceId
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Vendor')AS CP ON IR.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
						WHERE PGD.PostGRNInvoiceId=@receiveId
						GROUP BY MGPGL.GLGeneralInfoId ,GL.AccountCode ,GL.UserName ,MGPGL.BudgetMasterId ,B.Code
							,B.UserName ,MGPGL.ActivityId ,A.Code ,A.UserName,MM.MaterialGroupMasterId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Post Invoice

    }
}
