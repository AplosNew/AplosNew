using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Library.Accounting.Accounts
{
    public class AccountsOpeningBalanceService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsOpeningBalanceService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        public List<Dictionary<string, object>> GetMaterialMasterOBDetailList(string companyId, string plantId, string openinngBalanceId)
        {
            try
            {
                var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + "',@plantId VARCHAR(10)='" + plantId + @"';
                        SELECT 
									distinct IRDD.InventoryMaterialId as InventoryReceivedId,IR.Id InventoryReceivedId,
									FOBD.Id, FOBD.OpeningBalanceId,AGL.AccountCode+' - '+AGL.UserName AS AssetGLName
							       ,FOBD.AccumulatedDepreciationGLId,FOBD.AccumulatedDepreciationBudgetMasterId,FOBD.AccumulatedDepreciationActivityId,AB.UserName BudgetName,AC.UserName AssetActivityName,BM.BudgetCategoryId,BM.BudgetSubCategoryId
								   ,FOBD.FixedAssetMasterId,FOBD.AssetBudgetMasterId,FOBD.AssetActivityId, FOBD.MaterialMasterId, FOBD.BaseUOMId, UOM.UserName AS BaseUoM, FOBD.AssetGLId, FOBD.AccumulatedDepreciationGLId, FOBD.CurrencyId, FOBD.Quantity,FOBD.Quantity QuantityOld
									,FOBD.MaterialMasterId, MM.UserName MaterialMasterName,FOBD.ArticleId, MMA.StandardName ArticleName,FOBD.MaterialStorageId,FOBD.FirstCharacteristicsId,FOBD.FirstCharacteristicsValueId,FOBD.SecondCharacteristicsId,FOBD.SecondCharacteristicsValueId,FOBD.ThirdCharacteristicsId,FOBD.ThirdCharacteristicsValueId
									
									,IR.MaterialStorageId
                                    , FOBD.FirstCharacteristicsId
									, FC.UserName AS FirstCharacteristics
									, FOBD.FirstCharacteristicsValueId
									, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue

									, FOBD.SecondCharacteristicsId
									, FC.UserName AS SecondCharacteristics
									, FOBD.SecondCharacteristicsValueId
									, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue

									, FOBD.ThirdCharacteristicsId
									, FC.UserName AS ThirdCharacteristics
									, FOBD.ThirdCharacteristicsValueId
									, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue
                                    FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                                    LEFT JOIN [TRN].[OpeningBalance] AS FOB ON FOBD.OpeningBalanceId=FOB.Id
									LEFT JOIN [SCS].[UnitOfMeasurement] AS UOM ON UOM.Id=FOBD.BaseUOMId
									LEFT JOIN HKP.GLGeneralInfo AGL ON FOBD.AssetGLId=AGL.Id
									LEFT JOIN MST.BudgetMaster BM ON FOBD.AssetBudgetMasterId=BM.Id
									LEFT JOIN HKP.Budget AB ON BM.BudgetId=AB.Id
                                    LEFT JOIN HKP.Activity AC ON FOBD.AssetActivityId=AC.Id
									LEFT JOIN MST.MaterialMaster MM ON FOBD.MaterialMasterId=MM.Id
									LEFT JOIN MST.MaterialMasterArticle MMA ON FOBD.ArticleId = MMA.Id
                                    LEFT JOIN HKP.Characteristics AS FC ON FOBD.FirstCharacteristicsId=FC.Id
									LEFT JOIN HKP.Characteristics AS SC ON FOBD.SecondCharacteristicsId=SC.Id
									LEFT JOIN HKP.Characteristics AS TC ON FOBD.ThirdCharacteristicsId=TC.Id
									LEFT JOIN HKP.CharacteristicsValue AS FCV ON FOBD.FirstCharacteristicsValueId=FCV.Id
									LEFT JOIN HKP.CharacteristicsValue AS SCV ON FOBD.SecondCharacteristicsValueId=SCV.Id
									LEFT JOIN HKP.CharacteristicsValue AS TCV ON FOBD.ThirdCharacteristicsValueId=TCV.Id
                                    
                                    LEFT JOIN TRN.InventoryReceive IR ON IR.OpeningBalanceId=FOB.Id
									left join trn.InventoryReceiveDetail IRDD ON IRDD.MaterialMasterOpeningBalanceDetailId=FOBD.Id
									WHERE FOB.CompanyId=@companyId AND FOB.PlantId=@plantId AND FOB.Id='" + openinngBalanceId + "'  ";// ORDER BY AGL.AccountCode,AB.UserName,FAM.UserName,ACGL.AccountCode,ACB.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel GetAdvanceJournalList(GridParameter parameters, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT distinct  OB.Id, OB.Id AS OpeningBalanceId, OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, OBD.CurrencyId,  OB.PostingDate, OB.DocRefNo, OB.DocDate
                                , OB.Narration, OB.IsPark, SUM(OBD.DrAmount) DrAmount, SUM(OBD.CrAmount) CrAmount, OB.AddedBy
                                , OB.AddedDate, OB.AddedFromIP, V.VoucherNo
                                FROM [TRN].[OpeningBalanceDetail] AS OBD
								LEFT JOIN TRN.OpeningBalance AS OB ON OB.Id= OBD.OpeningBalanceId
                                LEFT JOIN TRN.Voucher AS V ON V.Id=OB.VoucherId
                                WHERE OB.Archive=0 AND OB.[SourceType]='" + SourceType.OpeningBalance + "' AND OB.CompanyId='" + companyId + "' AND OB.PlantId='" + plantId + @"'
                                GROUP BY OB.Id, OB.CompanyGroupId, OB.CompanyId,  OB.PostingDate, OB.DocRefNo, OB.DocDate, OB.Narration
								, OB.IsPark, OBD.CurrencyId, OB.PlantId, OB.EntityId ,OB.AddedBy, OB.AddedDate, OB.AddedFromIP, V.VoucherNo ";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetOBAdvanceJournalDetail(GridParameter parameters, string companyGroupId, string companyId, string plantId, string openingBalanceId)
        {
            parameters.CmdText = @"SELECT OBD.Id, OBD.OpeningBalanceId, ODC.Id AS OpeningBalanceDetailCurrencyId, OBD.DocRefNo, REPLACE(CONVERT(CHAR(11), OBD.DocDate, 106),' ','-') AS DocDate, OBD.DrAmount, OBD.CrAmount, OBD.CrAmount AS Amount, OBD.GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode
								, GLGI.UserName AS GLGeneralInfoName, OBD.BudgetMasterId,BM.RefNo+' - '+ B.UserName AS BudgetName, OBD.ActivityId, A.UserName AS ActivityName
								, OBD.PartyId, OBD.PartyPlantId, OBD.EmployeeId, OBD.PartyType, OBD.BankMasterId, OBD.CashMasterId, OBD.CurrencyId, OBD.CompanyId, OBD.PlantId, OBD.TransactionTypeId
								,[ParticularName]=CASE 
								WHEN EI.EmployeeName<>'' THEN EI.EmployeeName
								WHEN BA.AccountTitle<>'' THEN BA.AccountTitle
								WHEN EI.EmployeeName<>'' THEN EI.EmployeeName
								WHEN P.UserName<>'' THEN P.UserName
								WHEN CM.UserName<>'' THEN CM.UserName
								WHEN FAM.UserName<>'' THEN FAM.UserName
								WHEN MM.UserName<>'' THEN MM.UserName
								WHEN LOB.LoanBankMaster<>'' THEN LOB.LoanBankMaster
								WHEN LOB.LoanPartyName<>'' THEN LOB.LoanPartyName
								ELSE ''	END
                                ,OBD.FixedAssetMasterId,OBD.FAType
                                ,OBD.MaterialMasterId
                                ,OBD.MaterialMasterOpeningBalanceDetailId
                                ,OBD.LoanOpeningBalanceDetailId
                                ,OBD.SecurityOpeningBalanceDetailId
								,DrDisable=CASE WHEN OBD.PartyType='Customer' AND OBD.CrAmount<> 0  THEN 1 
								                WHEN OBD.PartyType='Vendor' AND OBD.CrAmount<> 0  THEN 1 
								                WHEN OBD.PartyType='FixedAsset' AND OBD.CrAmount<> 0  THEN 1 
												WHEN OBD.PartyType='LoanTaken'   THEN 1 
												WHEN OBD.PartyType='SecurityGiven'   THEN 1 
												WHEN OBD.PartyType='SecurityTaken'   THEN 1 
													ELSE 0
												END

								,CrDisable=CASE WHEN OBD.PartyType='Customer' AND OBD.DrAmount<> 0  THEN 1
												WHEN OBD.PartyType='Vendor' AND OBD.DrAmount<> 0  THEN 1 
												WHEN OBD.PartyType='Bank' AND OBD.DrAmount<> 0  THEN 1 
												WHEN OBD.PartyType='Cash' AND OBD.DrAmount<> 0  THEN 1 
												WHEN OBD.PartyType='FixedAsset' AND OBD.DrAmount<> 0  THEN 1 
												WHEN OBD.PartyType='Material'   THEN 1 
												WHEN OBD.PartyType='LoanGiven'   THEN 1 
												WHEN OBD.PartyType='SecurityGiven'   THEN 1 
												WHEN OBD.PartyType='SecurityTaken'   THEN 1 
													ELSE 0
												END
							   ,BankAmount=CASE WHEN OBD.BankCurrencyId=OBD.CurrencyId AND OBD.PartyType='Bank' THEN 0
												 WHEN OBD.CashCurrencyId=OBD.CurrencyId AND OBD.PartyType='Cash' THEN 0
												    ELSE OBD.BankAmount
												 END
								,OBD.BankCurrencyId,OBD.CashCurrencyId
                                FROM [TRN].[OpeningBalanceDetail] AS OBD
                                LEFT JOIN [TRN].[OpeningBalanceDetailCurrency] AS ODC ON ODC.OpeningBalanceDetailId=OBD.Id
								LEFT JOIN [TRN].[OpeningBalance] AS OB ON OB.Id=OBD.OpeningBalanceId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=OBD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=OBD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=OBD.ActivityId
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=OBD.PartyId
								LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=OBD.PartyPlantId
								LEFT JOIN [MST].BankMaster AS BA ON BA.Id=OBD.BankMasterId
								LEFT JOIN [MST].CashMaster AS CM ON CM.Id=OBD.CashMasterId
                                LEFT JOIN [dbo].EmployeeInformation AS EI ON EI.SystemId=OBD.EmployeeId
                                LEFT JOIN HKP.FixedAssetMasterBudgetTag FMT ON FMT.BudgetMasterId=OBD.BudgetMasterId
                                LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=FMT.FixedAssetMasterId
                                LEFT JOIN [MST].[MaterialMaster] AS MM ON MM.Id=OBD.MaterialMasterId
								LEFT JOIN (SELECT LOBD.Id OpeningBalanceDetailId,LP.UserName LoanPartyName,LBM.AccountTitle LoanBankMaster  FROM TRN.OpeningBalanceDetail LOBD 
												LEFT JOIN HKP.Party LP ON LP.Id=LOBD.PartyId
												LEFT JOIN MST.BankMaster LBM ON LBM.Id=LOBD.BankMasterId) LOB ON LOB.OpeningBalanceDetailId=OBD.LoanOpeningBalanceDetailId
								WHERE OB.CompanyGroupId='" + companyGroupId + "' AND OB.CompanyId='" + companyId + "' AND OB.PlantId='" + plantId + "' AND   OBD.OpeningBalanceId='" + openingBalanceId + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        public GridModel GetInterPlantTransactionTakenList(GridParameter parameters, string sourceType, string companyGroupId, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT OB.Id, OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, E.UserName AS EntityName, OB.VoucherId, VD.VoucherNo, OB.EmployeeTransactionTypeId, OB.FinancingTypeId,
                                OB.SourceType, OB.PartyType, OB.PostingDate, OB.DocDate, OB.DocRefNo, OB.Narration, OB.IsPark, OB.IsPosted, OB.[AddedBy], OB.[AddedDate], OB.[AddedFromIP], X.Amount
                                FROM [TRN].[OpeningBalance] AS OB
								LEFT JOIN [TRN].[Voucher] AS VD ON VD.Id=OB.VoucherId
                                LEFT JOIN [ORG].[Entity] AS E ON E.Id=OB.EntityId
                                LEFT JOIN( SELECT OBDC.OpeningBalanceId, SUM(OBDC.Amount) AS Amount FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
								INNER JOIN [ORG].[Company] AS C ON C.BaseCurrencyId=OBDC.ParallelCurrencyId
								WHERE C.Id='" + companyId + @"'
                                GROUP BY OBDC.OpeningBalanceId
								) AS X ON X.OpeningBalanceId=OB.Id
                                WHERE OB.Archive=0 AND OB.SourceType='" + sourceType + "' AND OB.CompanyGroupId='" + companyGroupId + "' AND OB.CompanyId='" + companyId + "' AND OB.PlantId='" + plantId + "'";
            return _sqlRepository.GetGridData(parameters);
        }

        public GridModel GetInterTransactionGivenList(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            parameters.CmdText = @"SELECT OB.Id, OB.CompanyGroupId, OB.CompanyId, OB.PlantId, OB.EntityId, E.UserName AS EntityName, OB.VoucherId, VD.VoucherNo, OB.EmployeeTransactionTypeId, OB.FinancingTypeId,
                                OB.SourceType, OB.PartyType, OB.PostingDate, OB.DocDate, OB.DocRefNo, OB.Narration, OB.IsPark, OB.IsPosted, OB.[AddedBy], OB.[AddedDate], OB.[AddedFromIP], X.Amount
                                FROM [TRN].[OpeningBalance] AS OB
								LEFT JOIN [TRN].[Voucher] AS VD ON VD.Id=OB.VoucherId
                                LEFT JOIN [ORG].[Entity] AS E ON E.Id=OB.EntityId
                                LEFT JOIN( SELECT OBDC.OpeningBalanceId, SUM(OBDC.Amount) AS Amount FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
								INNER JOIN [ORG].[Company] AS C ON C.BaseCurrencyId=OBDC.ParallelCurrencyId
								WHERE C.Id='" + companyId + @"'
                                GROUP BY OBDC.OpeningBalanceId
								) AS X ON X.OpeningBalanceId=OB.Id
                                WHERE OB.Archive=0 AND OB.SourceType IN ('" + SourceType.InterCompanyTransactionGiven + "', '" + SourceType.InterPlantTransactionGiven + "') AND OB.CompanyGroupId='" + companyGroupId + "' AND OB.CompanyId='" + companyId + "' AND OB.PlantId='" + plantId + "'";
            return _sqlRepository.GetGridData(parameters);
        }
    }
}
