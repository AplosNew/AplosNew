using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Taxations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Library.Accounting.Accounts
{
    public class AccountsInventorySalesService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsInventorySalesService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
		private Dictionary<string, object> GetCompanyPartyGroup(string partyId, string plantId)
		{
			var cmdText = @"select PartyAccountGroupId FROM HKP.CompanyParty where PartyId = '" + partyId + "' AND PlantId='" + plantId + @"' and PartyType='Vendor'";
			return _sqlRepository.GetData(cmdText);
		}

		private Dictionary<string, object> GetCustomerCompanyPartyGroup(string partyId, string plantId)
		{
			var cmdText = @"select PartyAccountGroupId FROM HKP.CompanyParty where PartyId = '" + partyId + "' AND PlantId='" + plantId + @"' and PartyType='Customer'";
			return _sqlRepository.GetData(cmdText);
		}
		public IEnumerable<object> GetInventoryMaterialReceivableData(string companyId, string plantId, string inveReveiveId, string partyId)
		{
			try
			{
				var companyParty = GetCompanyPartyGroup(partyId, plantId);
				var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT  X.* FROM (
					SELECT  'Inventory' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId,NULL TaxCategoryId,NULL TaxCodeId
							,ISH.PostDrGLGeneralInfoId GLGeneralInfoId , GL.AccountCode GLGeneralInfoCode ,GL.UserName GLGeneralInfoName
							,ISH.PostDrBudgetMasterId BudgetMasterId ,B.Code BudgetCode  ,B.UserName BudgetName
							,ISH.PostDrActivityId ActivityId ,A.Code ActivityCode ,A.UserName  ActivityName 
							
							, 0 Dr, SUM(ISH.BaseRate * ISH.Qty) AS Cr
							, SUM(ISH.BaseRate * ISH.Qty) AS Amount
                            
						FROM [TRN].[InventorySalesDetail] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN (
								SELECT SDH.InventorySalesDetailId,SUM(RD.BooksCurrencyBaseRate) GRNRate,SDH.BaseRate,SUM(SDH.Qty) Qty
								,RD.PostDrGLGeneralInfoId ,RD.PostDrBudgetMasterId,RD.PostDrActivityId
								FROM TRN.InventorySalesDetail SD 
								JOIN TRN.InventorySalesHistory SDH ON SDH.InventorySalesDetailId=SD.Id
								LEFT JOIN TRN.InventoryReceiveDetail RD ON RD.Id=SDH.InventoryReceiveDetailId
								GROUP BY SDH.InventorySalesDetailId,RD.PostDrGLGeneralInfoId ,RD.PostDrBudgetMasterId,RD.PostDrActivityId,SDH.BaseRate
								 ) ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						
						LEFT JOIN[MST].[BudgetMaster] AS BM ON ISH.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Activity] AS A ON ISH.PostDrActivityId= A.Id
						

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY ISH.PostDrGLGeneralInfoId,ISH.PostDrBudgetMasterId,ISH.PostDrActivityId,GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName

						UNION
				
						SELECT X.OtherName,X.TrnType,X.MaterialGroupMasterId,X.TaxCategoryId,X.TaxCodeId,X.GLGeneralInfoId,X.GLGeneralInfoCode,X.GLGeneralInfoName
							,X.BudgetMasterId,X.BudgetCode,X.BudgetName,X.ActivityId,X.ActivityCode,X.ActivityName
							,X.DR+ISNULL(IST.TaxAmount,0)+ISNULL(ISS.SVCAmount,0)+ISNULL(INS.TCSAmount,0) Dr
							,X.Cr
							,X.Amount +ISNULL(IST.TaxAmount,0)+ISNULL(ISS.SVCAmount,0)+ISNULL(INS.TCSAmount,0) Amount	
							FROM (
                            SELECT  'A/R' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId,NULL TaxCategoryId,NULL TaxCodeId

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
							,ISD.InventorySalesId
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
                    ,ISD.InventorySalesId
					) X
					LEFT JOIN(SELECT InventorySalesId,SUM(ISNULL(TaxAmount,0)) TaxAmount FROM  TRN.InventorySalesTax GROUP BY InventorySalesId)
					IST ON IST.InventorySalesId=X.InventorySalesId
					LEFT JOIN(SELECT InventorySalesId,SUM(ISNULL(Amount,0)) SVCAmount FROM  TRN.InventorySalesService GROUP BY InventorySalesId)
					ISS ON ISS.InventorySalesId=X.InventorySalesId
					LEFT outer JOIN ( SELECT InventorySalesId, sum(ISNULL(TaxAmount,0)) AS TCSAmount from  [TRN].[InventorySalesAdditionalTax] group by InventorySalesId) AS INS
                    ON  INS.InventorySalesId=X.InventorySalesId


						UNION
						SELECT  OtherName=case when SUM((ISD.SalesRate-ISH.BaseRate)*ISH.Qty)>0 then 'Gain on Sales'  
											when SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty)<0 then 'Loss on Sales'
											  end
							,TrnType=case when SUM((ISD.SalesRate-ISH.BaseRate)*ISH.Qty)>0 then 'Cr'  
											when SUM((ISD.SalesRate-ISH.BaseRate)*ISH.Qty)<0 then 'Dr'
											  end
											  , NULL MaterialGroupMasterId,NULL TaxCategoryId,NULL TaxCodeId
							,GLGeneralInfoId=CASE WHEN SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty)<0 THEN GAD.GLGeneralInfoId ELSE GADL.GLGeneralInfoId END
							,GLGeneralInfoCode=CASE WHEN SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty)<0 THEN GL.AccountCode  ELSE GLL.AccountCode END
							,GLGeneralInfoName=CASE WHEN SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty)<0 THEN GL.UserName   ELSE GLL.UserName  END
							,BudgetMasterId=CASE WHEN SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty)<0 THEN GAD.BudgetMasterId   ELSE GADL.BudgetMasterId  END
							,BudgetCode=CASE WHEN SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty)<0 THEN B.Code   ELSE BL.Code  END
							,BudgetName=CASE WHEN SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty)<0 THEN B.UserName   ELSE BL.UserName  END
							, ActivityId=CASE WHEN SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty)<0 THEN GAD.ActivityId  ELSE GADL.ActivityId END
							,ActivityCode=CASE WHEN SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty)<0 THEN A.Code   ELSE AL.Code  END
							,ActivityName=CASE WHEN SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty)<0 THEN A.UserName   ELSE AL.UserName  END
							
							, Dr=case when SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty)<0 then 0 else SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty) end
							, Cr=case when SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty)<0 then SUM((ISD.SalesRate-ISH.BaseRate)*ISH.Qty) else 0 end
							, Amount=case when SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty)>0 then SUM((ISH.BaseRate-ISD.SalesRate)*ISH.Qty) else 
							SUM((ISD.SalesRate-ISH.BaseRate)*ISH.Qty)  end
						FROM [TRN].[InventorySalesDetail] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN [HKP].[GeneralAccountDeterminate] GAD ON GAD.PlantId=IR.PlantId and GAD.Id='GainOnInventorySales'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON GAD.BudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id

						LEFT JOIN [HKP].[GeneralAccountDeterminate] GADL ON GADL.PlantId=IR.PlantId and GADL.Id='LossOnInventorySales'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLL ON GADL.GLGeneralInfoId=GLL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMFL ON GADL.BudgetMasterId= BMFL.Id
						LEFT JOIN [HKP].[Budget] AS BL ON BMFL.BudgetId= BL.Id
						LEFT JOIN [HKP].[Activity] AS AL ON GADL.ActivityId= AL.Id

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY GAD.GLGeneralInfoId ,GL.AccountCode  ,GL.UserName  ,GAD.BudgetMasterId 
							,B.Code  ,B.UserName  ,GAD.ActivityId  ,A.Code  ,A.UserName ,GADL.GLGeneralInfoId 
							,GLL.AccountCode  ,GLL.UserName  ,GADL.BudgetMasterId  ,BL.Code  ,BL.UserName  ,GADL.ActivityId 
							,AL.Code  ,AL.UserName
						UNION
						SELECT  OtherName='Tax'  
							,TrnType='Cr'  , NULL MaterialGroupMasterId,NULL TaxCategoryId,NULL TaxCodeId
							,GLGeneralInfoId=TCL.GLGeneralInfoId
							,GLGeneralInfoCode= GL.AccountCode
							,GLGeneralInfoName= GL.UserName 
							,BudgetMasterId= TCL.BudgetMasterId 
							,BudgetCode= B.Code
							,BudgetName= B.UserName
							, ActivityId= TCL.ActivityId
							,ActivityCode=A.Code
							,ActivityName=A.UserName
							, 0 Dr, SUM(ISD.TaxAmount) AS Cr
							, SUM(ISD.TaxAmount) AS Amount
						FROM [TRN].[InventorySalesTax] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN MST.TaxCategory TC ON TC.Id=ISD.TaxCategoryId
						LEFT JOIN MST.TaxCategoryGL TCL ON TCL.TaxCategoryId=TC.Id AND InputTaxOutPutTax='Output'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON TCL.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON TCL.BudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON TCL.ActivityId= A.Id

						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY TCL.GLGeneralInfoId ,GL.AccountCode  ,GL.UserName  ,TCL.BudgetMasterId 
							,B.Code  ,B.UserName  ,TCL.ActivityId  ,A.Code  ,A.UserName ,ISD.TaxCategoryId

							UNION
							SELECT  OtherName='Svc'  
							,TrnType='Cr'  , NULL MaterialGroupMasterId,NULL TaxCategoryId,NULL TaxCodeId
							,GLGeneralInfoId=SGL.ServiceGLId
							,GLGeneralInfoCode= GL.AccountCode
							,GLGeneralInfoName= GL.UserName 
							,BudgetMasterId= SGL.ServiceBudgetMasterId 
							,BudgetCode= B.Code
							,BudgetName= B.UserName
							, ActivityId= SGL.ServiceActivityId
							,ActivityCode=A.Code
							,ActivityName=A.UserName
							, 0 Dr, SUM(ISS.Amount) AS Cr
							, SUM(ISS.Amount) AS Amount
						FROM [TRN].[InventorySalesService] AS ISS
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISS.InventorySalesId=IR.Id
						LEFT JOIN HKP.ServiceMaster SM ON SM.Id=ISS.ServiceMasterId
						LEFT JOIN HKP.ServiceGroup SG ON SG.Id=SM.ServiceGroupId
						LEFT JOIN HKP.ServiceGroupGL SGL ON SGL.ServiceGroupId=SG.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON SGL.ServiceGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON SGL.ServiceBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS B ON BMF.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON SGL.ServiceActivityId= A.Id

						WHERE ISS.InventorySalesId=@receiveId
						GROUP BY SGL.ServiceGLId, GL.AccountCode, GL.UserName, SGL.ServiceBudgetMasterId 
							,B.Code, B.UserName, SGL.ServiceActivityId, A.Code, A.UserName
					UNION
					SELECT 'TCSPayable' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId, IRT.TaxCategoryId,IRT.TaxCodeId
						, TGL.WithholdCreditableGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TGL.WithholdCreditableBudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TGL.WithholdCreditableActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr, SUM(IRT.TaxAmount) AS Cr
						, SUM(IRT.TaxAmount) AS Amount
                        
					FROM [TRN].[InventorySalesAdditionalTax] AS IRT
                    LEFT JOIN [TRN].[InventorySales] AS IR ON IRT.InventorySalesId=IR.Id
					LEFT JOIN MST.TaxCode TCO ON TCO.Id=IRT.TaxCodeId  
					LEFT JOIN MST.TaxCodeGL TGL ON TGL.TaxCodeId=TCO.Id 
					LEFT JOIN [MST].[TaxCategory] AS TC ON IRT.TaxCategoryId=TC.Id
					LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TGL.WithholdCreditableGLId=GL.Id
					LEFT JOIN [MST].[BudgetMaster] AS BM ON TGL.WithholdCreditableBudgetMasterId= BM.Id
					LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					LEFT JOIN [HKP].[Activity] AS A ON TGL.WithholdCreditableActivityId= A.Id
					WHERE IRT.InventorySalesId=@receiveId AND TCO.InputOrOutput='" + TaxCodeInputOutput.Output + @"' 
					GROUP BY  IRT.TaxCategoryId,IRT.TaxCodeId, TGL.WithholdCreditableGLId, GL.AccountCode, GL.UserName, TGL.WithholdCreditableBudgetMasterId, B.Code
					, B.UserName, TGL.WithholdCreditableActivityId, A.Code, A.UserName
							)X
							WHERE X.Amount>0";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetInventoryReceivableSaleDetailGLList(string companyId, string plantId, string inveReveiveId, string partyId)
		{
			try
			{
				var companyParty = GetCustomerCompanyPartyGroup(partyId, plantId);
				var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"'
					SELECT  X.* FROM (
					SELECT  'Sales' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId
							,MGPGL.GLGeneralInfoId , GL.AccountCode GLGeneralInfoCode ,GL.UserName GLGeneralInfoName
							,MGPGL.BudgetMasterId ,B.Code BudgetCode  ,B.UserName BudgetName
							,MGPGL.ActivityId ,A.Code ActivityCode ,A.UserName  ActivityName 
							
							, 0 Dr, SUM(ISH.GRNRate * ISH.Qty) AS Cr
							, SUM(ISH.GRNRate * ISH.Qty) AS Amount
                            ,ISD.Id InventorySalesDetailId
						FROM [TRN].[InventorySalesDetail] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN (
								SELECT SDH.InventorySalesDetailId,SUM(RD.MaterialTranRate) GRNRate,SUM(SDH.Qty) Qty
								
								FROM TRN.InventorySalesDetail SD 
								JOIN TRN.InventorySalesHistory SDH ON SDH.InventorySalesDetailId=SD.Id
								LEFT JOIN TRN.InventoryReceiveDetail RD ON RD.Id=SDH.InventoryReceiveDetailId
								GROUP BY SDH.InventorySalesDetailId
								 ) ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON ISD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON IR.CustomerId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId AND MGPGL.PartyAccountGroupId= PACG.Id AND GLType='Sales'
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY MGPGL.GLGeneralInfoId,MGPGL.BudgetMasterId,MGPGL.ActivityId,GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,ISD.Id
						,ISD.Id 

						UNION ALL
                            SELECT  'A/R' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId

                            ,CPGL.GLGeneralInfoId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,CPGL.BudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,CPGL.ActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName

							, SUM(ISD.SalesRate*ISH.Qty)   AS  Dr, 0 Cr
							, SUM(ISD.SalesRate*ISH.Qty)  AS Amount
							,ISD.Id InventorySalesDetailId
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
						LEFT JOIN HKP.CompanyPartyGL CPGL ON CPGL.CompanyPartyId=CP.Id and CPGL.PartyGLType='ReconciliationGL' 
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON CPGL.GLGeneralInfoId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON CPGL.BudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON CPGL.ActivityId= A.Id

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
						GROUP BY  IR.Id, CPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, CPGL.BudgetMasterId, B.Code, B.UserName, CPGL.ActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName
						,ISD.Id 
							)X
							WHERE X.Amount>0";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetInventoryJVSaleDetailGLList(string companyId, string plantId, string inveReveiveId, string partyId)
		{
			try
			{
				var companyParty = GetCustomerCompanyPartyGroup(partyId, plantId);
				var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)
					SELECT  'Inventory' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId
							,ISH.PostDrGLGeneralInfoId GLGeneralInfoId , GL.AccountCode GLGeneralInfoCode ,GL.UserName GLGeneralInfoName
							,ISH.PostDrBudgetMasterId BudgetMasterId ,B.Code BudgetCode  ,B.UserName BudgetName
							,ISH.PostDrActivityId ActivityId ,A.Code ActivityCode ,A.UserName  ActivityName 
							
							, 0 Dr, SUM(ISH.TotalBaseAmount) AS Cr
							, SUM(ISH.TotalBaseAmount) AS Amount
                            ,ISD.Id InventorySalesDetailId
						FROM [TRN].[InventorySalesDetail] AS ISD
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN (
								SELECT SDH.InventorySalesDetailId,SUM(SDH.TotalBaseAmount) TotalBaseAmount,SUM(SDH.Qty) Qty
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
						
                             SELECT  'CostOfGoodsSold' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId

                            ,MGGL.ExpenseGLId GLGeneralInfoId
							,GL.AccountCode GLGeneralInfoCode
							,GL.UserName GLGeneralInfoName
							,MGGL.ExpenseBudgetMasterId BudgetMasterId
							,B.Code BudgetCode
							,B.UserName BudgetName
							,MGGL.ExpenseActivityId ActivityId
							,A.Code ActivityCode
							,A.UserName ActivityName

							, SUM(ISH.TotalBaseAmount)   AS  Dr, 0 Cr
							, SUM(ISH.TotalBaseAmount)  AS Amount
							,ISD.Id InventorySalesDetailId
						FROM [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IR ON ISD.InventorySalesId=IR.Id
						LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryReceiveDetail] AS INS ON ISH.InventoryReceiveDetailId=INS.Id
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
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId= GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM2 ON MGGL.ExpenseBudgetMasterId= BM2.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM2.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id

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
						GROUP BY  IR.Id, MGGL.ExpenseGLId, GL.AccountCode, GL.UserName, MGGL.ExpenseBudgetMasterId, B.Code, B.UserName, MGGL.ExpenseActivityId, A.Code, A.UserName
						,MM.IsAsset,FAG.VendorReconGLId,GLF.AccountCode,GLF.UserName,FAG.VendorReconBudgetMasterId,BF.Code,BF.UserName,FAG.VendorReconActivityId,AF.Code,AF.UserName,ISD.Id
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

		public IEnumerable<object> GetPostingInvReceivableData(string column, string value, string plantId)
		{
			try
			{
				string strkey = "1=1";
				if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
					strkey = column + " like '%" + value + "%'";
				var sql = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';

                        select top 300 * from (SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.SalesDate, 106),' ','-') AS SalesDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.CustomerId PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
			                        , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IV.PaymentTermId, IV.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IV.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate
	                                , IV.InvoiceNo, REPLACE(CONVERT(CHAR(11), IV.DocDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy
									, IR.InventoryVoucherId,IV.VoucherId,IV.Id InvoiceId,V.IsPark
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable
                                    , COUNT(*) OVER () AS TotalRows
									,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
									,VoucherNo = CASE WHEN IR.EmployeeId <>'' THEN VE.VoucherNo ELSE V.VoucherNo END
									,PostingDate= CASE WHEN IR.EmployeeId <>'' THEN REPLACE(CONVERT(CHAR(11), VE.PostingDate, 106),' ','-') ELSE REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') END
                                    ,MS.UserName MaterialStorageName
									,OI.Id OtherInvoiceId,OI.Amount OtherAmount
                                       ,OtherIsPark=CASE WHEN OI.VoucherId<>'' THEN 'OtherInvoicePosted' WHEN  OI.InvoiceId IS NULL THEN '' ELSE 'OtherInvoiceParked' end
                                        ,OI.VoucherId OtherInvoiceVoucherId,IV.CompanyCurrencyRate,OV.VoucherNo OtherVoucherNo
                        FROM [TRN].[InventorySales] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.CustomerId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Customer') AS CP ON CP.PartyId=IR.CustomerId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventorySalesId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.PolicyAmount) AS TransactionAmount, SUM(A.PolicyAmount) AS BaseAmount 
									FROM [TRN].[InventorySalesDetail] AS A
		                            JOIN [TRN].[InventorySales] AS B ON A.InventorySalesId=B.Id WHERE B.PlantId=@plantId GROUP BY A.InventorySalesId) AS IRD ON IRD.InventorySalesId=IR.Id
                        LEFT JOIN (SELECT A.InventorySalesId, A.TransactionUoMId FROM [TRN].[InventorySalesDetail] AS A JOIN [TRN].[InventorySales] AS B ON A.InventorySalesId=B.Id
		                            WHERE B.PlantId=@plantId GROUP BY A.InventorySalesId, A.TransactionUoMId HAVING COUNT(A.InventorySalesId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventorySalesId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN TRN.Invoice IV ON IV.InventorySalesId=IR.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IV.PaymentTermId=PT.Id
						LEFT JOIN TRN.Voucher V ON V.Id=IV.VoucherId
						LEFT JOIN TRN.OtherInvoice OI ON OI.InvoiceId=IV.Id
						LEFT JOIN TRN.Voucher OV ON OV.Id=OI.VoucherId
						LEFT JOIN TRN.EmployeePayable EP ON EP.InventoryReceiveId=IR.Id
						LEFT JOIN TRN.Voucher VE ON VE.Id=EP.VoucherId
                        LEFT JOIN HKP.MaterialStorage MS ON MS.Id=IR.MaterialStorageId
                        WHERE V.Archive=0 AND IR.PlantId=@plantId AND IR.[Status]='Posting' 
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
		public IEnumerable<object> GetPostingInventorySalesData(string column, string value, string plantId)
		{
			try
			{
				string strkey = "1=1";
				if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
					strkey = column + " like '%" + value + "%'";
				var sql = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';

                        select top 300 * from (SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.SalesDate, 106),' ','-') AS SalesDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.CustomerId PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
			                        , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode
	                                , REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, CP.TaxApplicable
                                    , COUNT(*) OVER () AS TotalRows,IR.InventoryVoucherId VoucherId
									,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
									,VoucherNo = V.VoucherNo 
									,PostingDate=  REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') 
                                    ,MS.UserName MaterialStorageName
                        FROM [TRN].[InventorySales] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.CustomerId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Customer') AS CP ON CP.PartyId=IR.CustomerId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventorySalesId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.PolicyAmount) AS TransactionAmount, SUM(A.PolicyAmount) AS BaseAmount 
									FROM [TRN].[InventorySalesDetail] AS A
		                            JOIN [TRN].[InventorySales] AS B ON A.InventorySalesId=B.Id WHERE B.PlantId=@plantId GROUP BY A.InventorySalesId) AS IRD ON IRD.InventorySalesId=IR.Id
                        LEFT JOIN (SELECT A.InventorySalesId, A.TransactionUoMId FROM [TRN].[InventorySalesDetail] AS A JOIN [TRN].[InventorySales] AS B ON A.InventorySalesId=B.Id
		                            WHERE B.PlantId=@plantId GROUP BY A.InventorySalesId, A.TransactionUoMId HAVING COUNT(A.InventorySalesId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventorySalesId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
						LEFT JOIN TRN.Voucher V ON V.Id=IR.InventoryVoucherId
                        LEFT JOIN HKP.MaterialStorage MS ON MS.Id=IR.MaterialStorageId
                        WHERE V.Archive=0 AND IR.PlantId=@plantId AND IR.[Status]='Posting' 
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

        #region InventorySalesReturn
        public IEnumerable<object> GetSalesDetailDataBySales(string inventorySalesId)
        {
            try
            {
                string sql = @"SELECT ISH.Id HistotyId,''Id,IID.Id InventorySalesDetailId, IID.InventorySalesId InventoryIssueId, IID.InventoryMaterialId, II.MaterialStorageId
		                        , IM.MaterialMasterId, MM.UserName AS MaterialMasterName, IM.ArticleId, AR.StandardName AS ArticleName
		                        , IM.FirstCharacteristicsId, CH1.UserName AS FirstCharacteristics, IM.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicText--FirstCharacteristicsValue
		                        , IM.SecondCharacteristicsId, CH2.UserName AS SecondCharacteristics, IM.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicText--SecondCharacteristicsValue
		                        , IM.ThirdCharacteristicsId, CH3.UserName AS ThirdCharacteristics, IM.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicText--ThirdCharacteristicsValue
		                        ,IRDUM.UserName GRNUoM, IID.TransactionUoMId, IID.BaseUOMId, UoM.UserName AS TransactionUoM, IID.AvgRate, IID.AvgAmount, IID.PolicyRate, IID.PolicyAmount, IID.[Policy]
                                ,CC.UserName CostCenter,C.UserName CountryName,c.Id CountryId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.NoteForAccounts
                                ,IRD.TransactionQty GRNQty,ISH.TotalBaseAmount InventoryAmount,ISD.SalesRate, IID.TransactionQty,ISR.OtherQty,(IID.TransactionQty-isnull(ISR.OtherQty,0)) BalanceQty,ISD.TotalAmount,IST.TaxAmount SalesTaxAmount,0 ReturnAmount,0 TaxAmount,NULL TaxList
                        FROM [TRN].[InventorySalesDetail] AS IID
                        LEFT JOIN [TRN].[InventorySales] AS II ON IID.InventorySalesId=II.Id
                        LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IID.InventoryMaterialId=IM.Id
                        LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN [MST].[MaterialMasterArticle] AS AR ON IM.ArticleId=AR.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH1 ON IM.FirstCharacteristicsId=CH1.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV1 ON IM.FirstCharacteristicsValueId=CHV1.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH2 ON IM.SecondCharacteristicsId=CH2.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON IM.SecondCharacteristicsValueId=CHV2.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH3 ON IM.ThirdCharacteristicsId=CH3.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON IM.ThirdCharacteristicsValueId=CHV3.Id
						LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IID.CostCenterId
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IID.BaseUOMId=UoM.Id
                        LEFT JOIN scs.country C On C.Id=IM.CountryId
                        LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=IID.Id
						LEFT JOIN [TRN].[InventoryReceiveDetail] IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN [SCS].[UnitOfMeasurement] AS IRDUM ON IRD.BaseUOMId=IRDUM.Id
                        JOIN (select InventorySalesHistoryId,Sum(TaxAmount) TaxAmount from trn.inventorySalesTax group by InventorySalesHistoryId) IST ON IST.InventorySalesHistoryId =ISH.Id
                        LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id) ISD ON ISD.Id=IID.Id
						LEFT JOIN (SELECT SR.InventorySalesId,SRD.InventoryMaterialId,sum(SRD.TransactionQty) OtherQty FROM TRN.InventorySalesReturnDetail SRD 
									JOIN TRN.InventorySalesReturn SR ON SR.Id=SRD.InventorySalesReturnId WHERE SR.InventorySalesId='" + inventorySalesId + @"' group by SR.InventorySalesId,SRD.InventoryMaterialId) ISR ON ISR.InventorySalesId=II.Id and ISR.InventoryMaterialId=IID.InventoryMaterialId
						WHERE IID.InventorySalesId='"+ inventorySalesId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

		public IEnumerable<object> GetTaxInfoRowWise(string InventorySalesId)
		{
			try
			{
				string sql = @"SELECT  A.InventorySalesHistoryId,A.Id InventorySalesTaxId,A.InventorySalesId,a.InventorySalesDetailId,A.TaxCategoryId,A.HSNCodeId
								,A.[Percentage],A.TaxAmount SalesTax,0 TaxAmount,B.Code HSNCode,B.[Description]
                                FROM trn.InventorySalesTax A
                                Left JOIN [HKP].[HSNCode] B On A.HSNCodeId=B.Id   
                                where A.InventorySalesId='" + InventorySalesId + "' and a.InventorySalesServiceId is null";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetTaxForUpdateSalesReturn(string salesReturnId, string InventorySalesId)
		{
			try
			{
				string sql = @"SELECT  NULL InventorySalesHistoryId,SRT.Id,ISR.InventorySalesId,SRT.InventorySalesTaxId,ISRD.InventorySalesDetailId,SRT.TaxCategoryId,SRT.HSNCodeId
								,SRT.[Percentage],IST.TaxAmount SalesTax,SRT.TaxAmount,B.Code HSNCode,B.[Description]
                               FROM trn.InventorySalesReturnTax SRT
								left join trn.InventorySalesReturnDetail ISRD ON ISRD.Id=SRT.InventorySalesReturnDetailId
								LEFT JOIN TRN.InventorySalesReturn ISR ON ISR.Id=SRT.InventorySalesReturnId
								LEFT JOIN TRN.InventorySalesTax IST ON IST.Id=SRT.InventorySalesTaxId
                                Left JOIN [HKP].[HSNCode] B On SRT.HSNCodeId=B.Id   
                                WHERE ISR.InventorySalesId='"+ InventorySalesId + "' AND SRT.InventorySalesReturnId='"+ salesReturnId + "' AND SRT.InventorySalesReturnServiceId IS NULL";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}


		public IEnumerable<object> GetSalesDetailDataForUpdateReturn(string salesReturnId,string inventorySalesId)
		{
			try
			{
				string sql = @"SELECT NULL HistotyId,ISRD.Id, ISRD.InventoryReceiveDetailId, IID.Id InventorySalesDetailId, IID.InventorySalesId InventoryIssueId, IID.InventoryMaterialId, II.MaterialStorageId
		                        , IM.MaterialMasterId, MM.UserName AS MaterialMasterName, IM.ArticleId, AR.StandardName AS ArticleName
		                        , IM.FirstCharacteristicsId, CH1.UserName AS FirstCharacteristics, IM.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicText--FirstCharacteristicsValue
		                        , IM.SecondCharacteristicsId, CH2.UserName AS SecondCharacteristics, IM.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicText--SecondCharacteristicsValue
		                        , IM.ThirdCharacteristicsId, CH3.UserName AS ThirdCharacteristics, IM.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicText--ThirdCharacteristicsValue
		                        ,IRDUM.UserName GRNUoM, IID.TransactionUoMId, IID.BaseUOMId, UoM.UserName AS TransactionUoM, IID.AvgRate, IID.AvgAmount, IID.PolicyRate, IID.PolicyAmount, IID.[Policy]
                                ,CC.UserName CostCenter,C.UserName CountryName,c.Id CountryId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.NoteForAccounts
                                ,IRD.TransactionQty GRNQty,ISH.TotalBaseAmount InventoryAmount,ISD.SalesRate, IID.TransactionQty
								,(ISR.OtherQty-ISRD.TransactionQty) OtherQty
								,(IID.TransactionQty-isnull(ISR.OtherQty,0)) BalanceQty,(IID.TransactionQty-isnull(ISR.OtherQty,0)) CurrentBalanceQty
								,ISD.TotalAmount,IST.TaxAmount SalesTaxAmount
								,ISRD.TransactionQty ReturnQty,ISRD.TotalSalesAmount ReturnAmount,ISRT.TaxAmount TaxAmount,NULL TaxList
								, ISRD.TransactionQty TempReturnQty
						FROM [TRN].InventorySalesReturnDetail ISRD
						LEFT JOIN ( SELECT InventorySalesReturnDetailId,SUM(TaxAmount) TaxAmount FROM TRN.InventorySalesReturnTax GROUP BY InventorySalesReturnDetailId ) ISRT ON ISRT.InventorySalesReturnDetailId=ISRD.Id
						LEFT JOIN [TRN].[InventorySalesDetail] AS IID ON IID.Id=ISRD.InventorySalesDetailId
                        LEFT JOIN [TRN].[InventorySales] AS II ON IID.InventorySalesId=II.Id
                        LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IID.InventoryMaterialId=IM.Id
                        LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN [MST].[MaterialMasterArticle] AS AR ON IM.ArticleId=AR.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH1 ON IM.FirstCharacteristicsId=CH1.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV1 ON IM.FirstCharacteristicsValueId=CHV1.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH2 ON IM.SecondCharacteristicsId=CH2.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON IM.SecondCharacteristicsValueId=CHV2.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH3 ON IM.ThirdCharacteristicsId=CH3.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON IM.ThirdCharacteristicsValueId=CHV3.Id
						LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IID.CostCenterId
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IID.BaseUOMId=UoM.Id
                        LEFT JOIN scs.country C On C.Id=IM.CountryId
                        LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=IID.Id
						LEFT JOIN [TRN].[InventoryReceiveDetail] IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN [SCS].[UnitOfMeasurement] AS IRDUM ON IRD.BaseUOMId=IRDUM.Id
                        JOIN (select InventorySalesHistoryId,Sum(TaxAmount) TaxAmount from trn.inventorySalesTax group by InventorySalesHistoryId) IST ON IST.InventorySalesHistoryId =ISH.Id
                        LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id) ISD ON ISD.Id=IID.Id
						LEFT JOIN (SELECT SR.InventorySalesId,SRD.InventoryMaterialId,sum(SRD.TransactionQty) OtherQty FROM TRN.InventorySalesReturnDetail SRD 
									JOIN TRN.InventorySalesReturn SR ON SR.Id=SRD.InventorySalesReturnId WHERE SR.InventorySalesId='" + inventorySalesId + @"' group by SR.InventorySalesId,SRD.InventoryMaterialId) ISR ON ISR.InventorySalesId=II.Id and ISR.InventoryMaterialId=IID.InventoryMaterialId
						WHERE IID.InventorySalesId='" + inventorySalesId + "' AND ISRD.InventorySalesReturnId='"+ salesReturnId + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetInventorySalesReturnData(string plantId)
        {
            try
            {
                string CmdText = @"SELECT E.UserName AS Entity , II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                                , MS.UserName AS MaterialStorage,SUM(IID.TransactionQty) Qty,II.Remarks,II.InventorySalesId,II.InventoryReceiveId
                                FROM [TRN].[InventorySalesReturn] AS II
                                JOIN TRN.InventorySalesReturnDetail AS IID ON IID.InventorySalesReturnId=II.Id
                                JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId=MS.Id
                                Left JOIN [ORG].[Entity] E On E.id=II.EntityId
                                WHERE II.PlantId='" + plantId + @"' AND ISNULL(II.[Status],'') <>'Posting' 
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                                , MS.UserName,E.UserName,II.Remarks,II.Id,II.InventorySalesId,II.InventoryReceiveId";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

		public IEnumerable<object> GetTaxInfo(string Id)
		{
			try
			{
				string sql = @"select A.Id,A.InventorySalesDetailId
							, A.InventorySalesHistoryId
							, A.InventoryReceiveDetailId
							, A.TaxCategoryId
							, A.HSnCodeId
							, HC.Code HSNCode
							, A.Percentage
							, A.TaxAmount
							FROM  TRN.InventorySalesTax A
							LEFT JOIN TRN.InventorySalesHistory B ON B.Id= A.InventorySalesHistoryId
							LEFT JOIN TRN.InventorySalesDetail C ON C.Id= B.InventorySalesDetailId
							LEFT JOIN [TRN].[InventorySales] D ON D.Id= C.InventorySalesId
							LEFT JOIN [HKP].[HSNCode] HC ON HC.Id= A.HSnCodeId
							where D.Id= '" + Id + "'";
				return _sqlRepository.GetDataCollection(sql);

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public IEnumerable<object> GetServiceChargeList(string inventorySalesId)
		{
			try
			{
				var sql = @"SELECT A.Id, A.Id InventorySalesServiceId, A.InventorySalesId
                            , A.ServiceMasterId
                            , B.UserName AS ServiceMasterName
                            , A.Amount
                            ,POT.TaxAmount As SalesServiceTaxAmount,ISNULL(OISRS.OtherServiceAmount,0) OtherServiceAmount,A.Amount-ISNULL(OISRS.OtherServiceAmount,0) BalanceAmount
							,0Amount,0 TotalTaxAmount
                            ,null ChargeTaxList
                            ,A.Description 
                            FROM  [TRN].[InventorySalesService] AS A 
                            INner JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                            left JOIN (select InventorySalesServiceId,Sum(TaxAmount) as TaxAmount  from TRN.InventorySalesTax group by InventorySalesServiceId) AS POT on A.id=POT.InventorySalesServiceId
                            LEFT JOIN (SELECT INS.InventorySalesId,SUM(ISRS.Amount) OtherServiceAmount FROM TRN.InventorySalesReturnService ISRS 
											JOIN TRN.InventorySalesReturn INS ON INS.Id=ISRS.InventorySalesReturnId group by INS.InventorySalesId) OISRS ON OISRS.InventorySalesId=A.InventorySalesId
							WHERE A.InventorySalesId='"+ inventorySalesId + "'";
				return _sqlRepository.GetDataCollection(sql);

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public IEnumerable<object> GetServiceChargeForUpdateList(string salesReturnId, string inventorySalesId)
		{
			try
			{
				var sql = @"SELECT A.Id, A.Id InventorySalesServiceId, A.InventorySalesId
                            , A.ServiceMasterId
                            , B.UserName AS ServiceMasterName
                            , A.Amount
                            ,POT.TaxAmount As SalesServiceTaxAmount,ISNULL(OISRS.OtherServiceAmount,0) OtherServiceAmount
							,A.Amount-ISNULL(OISRS.OtherServiceAmount,0) BalanceAmount,A.Amount-ISNULL(OISRS.OtherServiceAmount,0) CurrentBalanceAmount
							,ISRS.Amount ReturnAmount,ISRS.TotalTaxAmount
                            ,null ChargeTaxList
                            ,A.Description 
                            FROM  TRN.InventorySalesReturnService ISRS 
							LEFT JOIN [TRN].[InventorySalesService] AS A ON A.Id=ISRS.InventorySalesServiceId
                             JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                            left JOIN (select InventorySalesServiceId,Sum(TaxAmount) as TaxAmount  from TRN.InventorySalesTax group by InventorySalesServiceId) AS POT on A.id=POT.InventorySalesServiceId
                            LEFT JOIN (SELECT INS.InventorySalesId,SUM(ISRS.Amount) OtherServiceAmount FROM TRN.InventorySalesReturnService ISRS 
											JOIN TRN.InventorySalesReturn INS ON INS.Id=ISRS.InventorySalesReturnId group by INS.InventorySalesId) OISRS ON OISRS.InventorySalesId=A.InventorySalesId
							WHERE A.InventorySalesId='" + inventorySalesId + "' AND ISRS.InventorySalesReturnId='"+ salesReturnId + @"'
							select * FROM  TRN.InventorySalesReturnService";
				return _sqlRepository.GetDataCollection(sql);

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public IEnumerable<object> GetServiceTaxList(string Id)
		{
			try
			{
				var sql = @"SELECT A.Id, A.Id InventorySalesTaxId, A.InventorySalesServiceId, A.TaxCategoryId, TC.UserName AS TaxCategory, A.HSNCodeId, HN.Code AS HSNCode, A.[Percentage], A.TaxAmount
                            FROM [TRN].[InventorySalesTax] AS A JOIN [MST].[TaxCategory] AS TC ON A.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON A.HSNCodeId=HN.Id
                            WHERE A.InventorySalesId='" + Id + "' AND A.InventoryReceiveDetailId IS NULL ORDER BY TC.[Sequence]";
				return _sqlRepository.GetDataCollection(sql);

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerable<object> GetServiceTaxForUpdate(string salesReturnId, string inventorySalesId)
		{
			try
			{
				var sql = @"SELECT ISRT.Id,ISRT.InventorySalesTaxId,A.InventorySalesServiceId, ISRT.TaxCategoryId, TC.UserName AS TaxCategory, ISRT.HSNCodeId, HN.Code AS HSNCode, A.[Percentage]
						,A.TaxAmount SalesTaxAmount, ISRT.TaxAmount
                            FROM TRN.InventorySalesReturnTax ISRT 
							LEFT JOIN  [TRN].[InventorySalesTax] AS A ON A.Id=ISRT.InventorySalesTaxId
							JOIN [MST].[TaxCategory] AS TC ON ISRT.TaxCategoryId=TC.Id
                            LEFT JOIN [HKP].[HSNCode] AS HN ON ISRT.HSNCodeId=HN.Id
                            WHERE A.InventorySalesId='"+ inventorySalesId + "' AND ISRT.InventorySalesReturnId='"+ salesReturnId + @"' and ISRT.InventorySalesReturnDetailId is null
							ORDER BY TC.[Sequence]";
				return _sqlRepository.GetDataCollection(sql);

			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		#endregion
	}
}
