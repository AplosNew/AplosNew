using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Helpers;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;

namespace Aplos.MaterialManagement.MaterialQuery
{
    public class InventoryIssueQueryService
	{
        private readonly ISqlRepository _sqlRepository;
        public InventoryIssueQueryService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }


        public GridModel GetReceivableMaterial(GridParameter parameters, string inveReveiveId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"',@companyId varchar(10)='" + identity.CompanyId + @"',@plantId varchar(10)='" + identity.PlantId + @"'
                                        , @totalReceiveAmount DECIMAL(18, 4)=0
	                                  , @totalServiceAmount DECIMAL(18, 4)=0
	                                  , @totalSvcTaxAmount DECIMAL(18, 4)=0
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
                        SELECT IM.Id, ISD.Id AS InventoryReceiveDetailId
                            , MGM.UserName AS MaterialGroupMasterName
                            , IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue                         
                            , ISD.TransactionUoMId, TUoM.UserName AS TransactionUoM
                            , ISH.SalesRate AS TransactionRate
                            , CU.Code AS CurrencyName, IVS.ToCurrencyRate
                            , ISH.Amount
                            ,ISH.Qty                         
							                  
					        ,ISD.TransactionUoMId
							,ISD.BaseUOMId 
                            ,MM.IsAsset  
							,HSNC.Code HSNCode
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
						LEFT JOIN [TRN].[InventorySalesDetail] ISD ON ISD.InventoryMaterialId=IM.Id AND ISD.InventorySalesId=@inventoryReceiveId
                        JOIN [SCS].[UnitOfMeasurement] AS TUoM ON ISD.TransactionUoMId=TUoM.Id
						JOIN TRN.InventorySales IVS ON IVS.Id=ISD.InventorySalesId
                        JOIN [SCS].[Currency] AS CU ON IVS.CurrencyId=CU.Id
                        LEFT JOIN HKP.HSNCode AS HSNC ON HSNC.Id=MM.HSNCodeId
						LEFT JOIN (
								SELECT SDH.InventorySalesDetailId,SUM(SDH.Qty) Qty,sum(SDH.Qty*SD.SalesRate) Amount,SD.SalesRate SalesRate
								FROM TRN.InventorySalesDetail SD 
								JOIN TRN.InventorySalesHistory SDH ON SDH.InventorySalesDetailId=SD.Id
								LEFT JOIN TRN.InventoryReceiveDetail RD ON RD.Id=SDH.InventoryReceiveDetailId
								GROUP BY SDH.InventorySalesDetailId,SD.SalesRate
								 ) ISH ON ISH.InventorySalesDetailId=ISD.Id
                        WHERE ISD.InventorySalesId=@inventoryReceiveId";
                return _sqlRepository.GetDifferentGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetInventortGLBudgetActivityData(string companyId, string plantId, string inveReveiveId, string partyId)
        {
            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                string partyType = "Vendor";
                var companyParty = materialCommonService.GetCompanyParty(companyId, plantId, partyId, partyType);
                var sql = @"DECLARE @receiveId varchar(10)='" + inveReveiveId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"', @partyAccountGruopId varchar(10)='" + companyParty["PartyAccountGroupId"].ToString() + @"',@countryId varchar(10)

						SELECT  'CostOfGoodsSold' AS OtherName, 'Dr' AS TrnType, NULL MaterialGroupMasterId
							,GLGeneralInfoId =MGGL.InventoryGLId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =MGGL.InventoryBudgetMasterId 
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = MGGL.InventoryActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							
							, SUM(ISH.Qty*IRD.BooksCurrencyBaseRate) AS Dr
							, NULL Cr
							, SUM(ISH.Qty*IRD.BooksCurrencyBaseRate) AS Amount
                            --,IRD.Id AS  InventoryReceiveDetailId
						FROM  [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IVS ON ISD.InventorySalesId=IVS.Id
						LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ExpenseBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id
						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY MM.MaterialGroupMasterId, MGGL.InventoryGLId, GL.AccountCode, GL.UserName, MGGL.InventoryBudgetMasterId, B.Code, B.UserName, MGGL.InventoryActivityId, A.Code, A.UserName
					    UNION
						SELECT  'Inventory' AS OtherName, 'Cr' AS TrnType, NULL MaterialGroupMasterId

							,GLGeneralInfoId =IRD.PostDrGLGeneralInfoId 
							,GLGeneralInfoCode =GL.AccountCode
							,GLGeneralInfoName =GL.UserName 
							,BudgetMasterId =IRD.PostDrBudgetMasterId 
							,BudgetCode =B.Code 
							,BudgetName =B.UserName
							,ActivityId =IRD.PostDrActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName 
							, NULL Dr
							, SUM(ISH.Qty*IRD.BooksCurrencyBaseRate) AS Cr
							, SUM(ISH.Qty*IRD.BooksCurrencyBaseRate) AS Amount
                            --,IRD.Id AS  InventoryReceiveDetailId
						FROM  [TRN].[InventorySalesDetail] AS ISD 
						LEFT JOIN [TRN].[InventorySales] AS IVS ON ISD.InventorySalesId=IVS.Id
						LEFT JOIN TRN.InventorySalesHistory ISH ON ISH.InventorySalesDetailId=ISD.Id
						LEFT JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
						WHERE ISD.InventorySalesId=@receiveId
						GROUP BY  IRD.PostDrGLGeneralInfoId, GL.AccountCode, GL.UserName, IRD.PostDrBudgetMasterId, B.Code, B.UserName, IRD.PostDrActivityId, A.Code, A.UserName ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> IssueReturnForUpdateQuery(string Id, string toDate, string CostCenterId)
        {
            var sql = @"Select a.Id IssueREturnHistoryId
                              ,a.InventoryIssueReturnId
                            --,InventoryMaterialId
                            --,InventoryReceiveDetailId
                            --,Qty,CostCenterId
                            --,StorageLocationId
                            --,BaseUOMId
                            --,TransactionUoMId 
                            ,a.CostCenterId CostCenterId
                            ,cc.UserName CostCenterName
                            , a.InventoryReceiveDetailId
                            ,IM.Id InventoryMaterialId
                            ,a.Id As IssueReturnId
                            --, REPLACE(CONVERT(CHAR(11), C.IssueDate, 106),' ','-') AS IssueDate
                            ,a.IssueRequestDetailId
                            ,IM.MaterialMasterId, MM.UserName AS MaterialMasterName, IM.ArticleId, AR.StandardName AS ArticleName
                            ,IM.FirstCharacteristicsId, CH1.UserName AS Sku1, IM.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicsValue
                            ,IM.SecondCharacteristicsId, CH2.UserName AS Sku2, IM.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicsValue
                            ,IM.ThirdCharacteristicsId, CH3.UserName AS Sku3, IM.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicsValue			
                            ,a.BaseUOMId, UoM.UserName AS TransactionUoM
                            ,IIH.qty AS IssuedQty
                            ,Isnull(IIH.IssueReturnQty,0)  IssueReturnQty
                            
                            ,Isnull(a.qty,0) TransactionQty
                            ,Isnull(a.qty,0) oldReturnQty
                            --,(Isnull(IIH.qty ,0)-(Isnull(IIH.IssueReturnQty,0)+Isnull(a.qty,0))) Balance
                            ,(Isnull(IIH.qty ,0)-(Isnull(IIH.IssueReturnQty,0))) Balance
                            ,0 Active
                            ,a.StorageLocationId,MS.UserName MaterialStorage
                            FROM TRN.InventoryIssueReturnHistory a
                            left join [TRN].[InventoryMaterial] AS IM ON IM.Id=a.InventoryMaterialId
                            left JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                            LEFT JOIN [MST].[MaterialMasterArticle] AS AR ON IM.ArticleId=AR.Id
                            LEFT JOIN [HKP].[Characteristics] AS CH1 ON IM.FirstCharacteristicsId=CH1.Id
                            LEFT JOIN [HKP].[CharacteristicsValue] AS CHV1 ON IM.FirstCharacteristicsValueId=CHV1.Id
                            LEFT JOIN [HKP].[Characteristics] AS CH2 ON IM.SecondCharacteristicsId=CH2.Id
                            LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON IM.SecondCharacteristicsValueId=CHV2.Id
                            LEFT JOIN [HKP].[Characteristics] AS CH3 ON IM.ThirdCharacteristicsId=CH3.Id
                            LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON IM.ThirdCharacteristicsValueId=CHV3.Id
                            LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=a.CostCenterId
                            left JOIN [SCS].[UnitOfMeasurement] AS UoM ON a.BaseUOMId=UoM.Id
                            left join [HKP].[MaterialStorage] MS on MS.id=a.StorageLocationId
                            LEFT JOIN trn.InventoryIssueReturn IIR ON IIR.Id=a.InventoryIssueReturnId
                            LEFT join(select sum(qty) qty,sum(IssueReturnQty) IssueReturnQty,InventoryReceiveDetailId from trn.InventoryIssueHistory group by InventoryReceiveDetailId) IIH On IIH.InventoryReceiveDetailId=a.InventoryReceiveDetailId
                            
                            Where IIR.Id='" + Id + @"'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetCheckedApprovedListQuery(string tabType)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";


                if (tabType == "UnCheckedList")
                {
                    sql = @"
                        SELECT E.UserName AS Entity 
                        ,isnull(II.IssueType,'') issuetype
                        , II.Id, II.CompanyGroupId
                        , II.CompanyId, II.PlantId
                        , II.EntityId, II.MaterialStorageId
                        ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                        , MS.UserName AS MaterialStorage 
                        ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                       ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount
                        ,II.Remarks,II.Id AS IssueId
                        ,II.OrderRefNo
                        ,EI1.EmployeeName CheckedByName
                        ,II.CheckedByStatus
                        ,EI2.EmployeeName ApprovedByName
                        ,II.ApprovedByStatus
						,P.UserName CustomerName
						,P.Code CustomerCode
                        FROM[TRN].[InventorySales] AS II
                         left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                        left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                        left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                        Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                        left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                        left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                        WHERE II.CheckedBy='" + identity.EmployeeId + @"' AND II.CheckedByStatus ='For Checking' 
                        AND II.ApprovedByStatus IS NULL
                        
                        AND ISNULL(II.[Status],'') <>'Posting' 

                        GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus
	,P.UserName,P.Code";
                }
                else if (tabType == "HoldRejectCheckedList")
                {
                    sql = @"SELECT E.UserName AS Entity 
                            ,isnull(II.IssueType,'') issuetype
                            , II.Id, II.CompanyGroupId
                            , II.CompanyId, II.PlantId
                            , II.EntityId, II.MaterialStorageId
                            ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                            , MS.UserName AS MaterialStorage 
                            ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                        ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount
                            ,II.Remarks,II.Id AS IssueId
                            ,II.OrderRefNo
                            ,EI1.EmployeeName CheckedByName
                            ,II.CheckedByStatus
                            ,EI2.EmployeeName ApprovedByName
                            ,II.ApprovedByStatus
							,P.UserName CustomerName
						    ,P.Code CustomerCode
                            FROM[TRN].[InventorySales] AS II
                            left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                            left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                            left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                            Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                            left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                            left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                            WHERE II.CheckedByStatus ='Hold' OR II.CheckedByStatus ='Reject' 
                            AND II.ApprovedByStatus IS NULL
                            AND II.CheckedBy='" + identity.EmployeeId + @"'
                            AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus 
							,P.UserName,P.Code
							";
                }
                else if (tabType == "CheckedList")
                {
                    sql = @"SELECT E.UserName AS Entity 
                            ,isnull(II.IssueType,'') issuetype
                            , II.Id, II.CompanyGroupId
                            , II.CompanyId, II.PlantId
                            , II.EntityId, II.MaterialStorageId
                            ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                            , MS.UserName AS MaterialStorage 
                            ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                         ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount
                            ,II.Remarks,II.Id AS IssueId
                            ,II.OrderRefNo
                            ,EI1.EmployeeName CheckedByName
                            ,II.CheckedByStatus
                            ,EI2.EmployeeName ApprovedByName
                            ,II.ApprovedByStatus
,P.UserName CustomerName
						    ,P.Code CustomerCode
                            FROM[TRN].[InventorySales] AS II
                            left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                            left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                            left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                            Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                            left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                            left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                            WHERE   II.CheckedBy= '" + identity.EmployeeId + @"'  
                            AND II.CheckedByStatus ='Checked' 
                            AND II.ApprovedByStatus= 'For Approval'                            
                            AND ISNULL(II.[Status],'') <>'Posting' 
                           GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus 
,P.UserName,P.Code
							";
                }
                else if (tabType == "UnApprovedList")
                {
                    sql = @"SELECT * FROM(
                                SELECT E.UserName AS Entity 
                                ,isnull(II.IssueType,'') issuetype
                                , II.Id, II.CompanyGroupId
                                , II.CompanyId, II.PlantId
                                , II.EntityId, II.MaterialStorageId
                                ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                                , MS.UserName AS MaterialStorage 
                                ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                          ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount  
                                ,II.Remarks,II.Id AS IssueId
                                ,II.OrderRefNo
                                ,EI1.EmployeeName CheckedByName
                                ,II.CheckedByStatus
                                ,EI2.EmployeeName ApprovedByName
                                ,II.ApprovedByStatus
,P.UserName CustomerName
						    ,P.Code CustomerCode
                                FROM[TRN].[InventorySales] AS II
                                left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                                left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                                left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                                Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                                left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                                left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                                WHERE   II.ApprovedBy= '" + identity.EmployeeId + @"' 
                                AND II.CheckedByStatus ='Checked' 
                                AND II.ApprovedByStatus ='For Approval'                                
                                AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus
							,P.UserName,P.Code

					UNION ALL

                                SELECT E.UserName AS Entity 
                                ,isnull(II.IssueType,'') issuetype
                                , II.Id, II.CompanyGroupId
                                , II.CompanyId, II.PlantId
                                , II.EntityId, II.MaterialStorageId
                                ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                                , MS.UserName AS MaterialStorage 
                                ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                            ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount
                                ,II.Remarks,II.Id AS IssueId
                                ,II.OrderRefNo
                                ,EI1.EmployeeName CheckedByName
                                ,II.CheckedByStatus
                                ,EI2.EmployeeName ApprovedByName
                                ,II.ApprovedByStatus
,P.UserName CustomerName
						    ,P.Code CustomerCode
                                FROM[TRN].[InventorySales] AS II
                                left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                                left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                                left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                                Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                                left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                                left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                                WHERE  II.ApprovedBy= '" + identity.EmployeeId + @"' 
                                AND II.CheckedByStatus IS NULL
                                AND II.ApprovedByStatus ='For Approval'                               
                                AND ISNULL(II.[Status],'') <>'Posting' 
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus
							,P.UserName,P.Code
                                )X
                                Order BY IssueDate DESC";
                }
                else if (tabType == "HoldRejectApprovedList")
                {
                    sql = @"SELECT E.UserName AS Entity 
                            ,isnull(II.IssueType,'') issuetype
                            , II.Id, II.CompanyGroupId
                            , II.CompanyId, II.PlantId
                            , II.EntityId, II.MaterialStorageId
                            ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                            , MS.UserName AS MaterialStorage 
                            ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                            ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount
                            ,II.Remarks,II.Id AS IssueId
                            ,II.OrderRefNo
                            ,EI1.EmployeeName CheckedByName
                            ,II.CheckedByStatus
                            ,EI2.EmployeeName ApprovedByName
                            ,II.ApprovedByStatus
,P.UserName CustomerName
						    ,P.Code CustomerCode
                            FROM[TRN].[InventorySales] AS II
                             left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                            left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                            left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                            Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                            left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                            left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                            WHERE  II.ApprovedBy= '" + identity.EmployeeId + @"' 
                            AND II.CheckedByStatus ='Checked' 
                            AND (II.ApprovedByStatus ='Hold' OR II.ApprovedByStatus ='Reject')                             
                            AND ISNULL(II.[Status],'') <>'Posting' 
                            GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus,P.UserName,P.Code							";
                }
                else if (tabType == "ApprovedList")
                {
                    sql = @"SELECT E.UserName AS Entity 
                            ,isnull(II.IssueType,'') issuetype
                            , II.Id, II.CompanyGroupId
                            , II.CompanyId, II.PlantId
                            , II.EntityId, II.MaterialStorageId
                            ,FORMAT(II.SalesDate, 'dd-MMM-yyyy') IssueDate
                            , MS.UserName AS MaterialStorage 
                            ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                          ,sum(IID.Qty) Qty
                        ,sum(IId.SalesRate*IID.Qty) Amount
                            ,II.Remarks,II.Id AS IssueId
                            ,II.OrderRefNo
                            ,EI1.EmployeeName CheckedByName
                            ,II.CheckedByStatus
                            ,EI2.EmployeeName ApprovedByName
                            ,II.ApprovedByStatus
,P.UserName CustomerName
						    ,P.Code CustomerCode
                            FROM[TRN].[InventorySales] AS II
                              left JOIN (select InventorySalesId ,IsAsset,TransactionQty  Qty
										,ROUND(SalesRate, 2) SalesRate--,(sum(TransactionQty) * ROUND(sum(SalesRate), 4)) /sum(TransactionQty) TotalAmount 
										from TRN.InventorySalesDetail --where InventorySalesId='202199'
										--group by InventorySalesId ,IsAsset
										) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
                            left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                            left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                            Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                            left join dbo.EmployeeInformation AS EI1 ON EI1.SystemId = II.CheckedBy
                            left join dbo.EmployeeInformation AS EI2 ON EI2.SystemId = II.ApprovedBy
						left join hkp.Party P on P.Id=II.CustomerId

                            WHERE  II.ApprovedBy= '" + identity.EmployeeId + @"' 
                            AND II.CheckedByStatus ='Checked' 
                            AND II.ApprovedByStatus= 'Approved'                            
                            AND ISNULL(II.[Status],'') <>'Posting' 
                            GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
                            ,II.SalesDate, MS.UserName
                            ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo,EI1.EmployeeName 
							,II.CheckedByStatus
							,EI2.EmployeeName 
							,II.ApprovedByStatus,P.UserName,P.Code
							";//II.PlantId= '" + identity.PlantId + @"'  AND 
                }


                return _sqlRepository.GetDataCollection(sql) ;

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetDataByInventoryScrapQuery(string tabType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                if (tabType == "1")
                {
                    sql = @" Select * from(SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
			                AND II.CheckedByStatus='For Checking'
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id ,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus									

			                UNION ALL
			                SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id
							 LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"'
			                         AND II.CheckedByStatus IS NULL
			                         AND II.ApprovedByStatus IS NULL
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id ,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus				
							UNION ALL
			                SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id
							 LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
			                         AND II.CheckedByStatus IS NULL
			                         AND II.ApprovedByStatus ='For Approval'
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id ,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus)x  
							Order BY IssueDate DESC";
                }
                else if (tabType == "2")
                {
                    sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND (II.CheckedByStatus='Hold' OR II.CheckedByStatus='Reject')                           
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX))
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus";
                }
                else if (tabType == "3")
                {
                    sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND II.CheckedByStatus='Checked' AND II.ApprovedByStatus='For Approval'    
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX))
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus";
                }
                else if (tabType == "4")
                {
                    sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND II.CheckedByStatus='Checked' AND (II.ApprovedByStatus='Hold' OR II.ApprovedByStatus='Reject')
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX))
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus";
                }
                else if (tabType == "5")
                {
                    sql = @"select * from(SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND II.CheckedByStatus='Checked' AND II.ApprovedByStatus='Approved' 
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX))  
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
                            UNION ALL
                            SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND II.CheckedByStatus IS NULL AND II.ApprovedByStatus='Approved' 
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id ,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
                             UNION ALL
                            SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND II.CheckedByStatus IS NULL AND II.ApprovedByStatus IS NULL
							AND ISNULL(II.[Status],'') <>'Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
                             )x
							Order BY IssueDate DESC";
                }
                if (tabType == "6")
                {
                    sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'	,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus

							FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
                            LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
							WHERE II.PlantId= '" + identity.PlantId + @"' 
							AND II.CheckedByStatus='Checked' AND II.ApprovedByStatus='Approved' 
							AND ISNULL(II.[Status],'') ='Posting' 
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus

							Order BY II.ScrapDate DESC";
                }
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }

        }
        private DataTable GetInventoryScrapReportData(string CompanyGroupId, string CompanyId, string PlantId, string fromDate, string toDate)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				if (fromDate != "" && toDate != "")
				{
					var sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							,c.UserName as Company
							,p.UserName as Plant
							
							
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, format(II.DocDate,'dd-MMM-yyyy')DocDate, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus


							--,MT.UserName MaterialType
							--,MGM.UserName AS MaterialGroupMasterName
							,IM.MaterialMasterId
							,MM.UserName MaterialMasterName
						-- , IM.ArticleId
							, ART.StandardName ArticleName
							
							--, IM.FirstCharacteristicsId
							--, FC.UserName AS FirstCharacteristics
							--, IM.FirstCharacteristicsValueId
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							--, IM.SecondCharacteristicsId
							--, SC.UserName AS SecondCharacteristics
							--, IM.SecondCharacteristicsValueId
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							--, IM.ThirdCharacteristicsId
							--, TC.UserName AS ThirdCharacteristics
							--, IM.ThirdCharacteristicsValueId
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						
						    ,Posted=CASE WHEN II.[Status]='Posted' then 'YES' else 'NO' END



							FROM [TRN].[InventoryScrap] AS II
							left join org.company c on c.id= ii.companyid
							left join org.Plant p on p.id= ii.PlantId
							
							

							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy



							LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IID.InventoryMaterialId
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							left JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id



							WHERE II.PlantId='" + identity.PlantId + @"' 
                         AND convert(Date,II.ScrapDate) BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
			               -- AND II.CheckedByStatus='For Checking'
							--AND ISNULL(II.[Status],'') <>'Posting' 
							--AND II.ScrapDate Between '1-Jan-2020' ANd '1-Jan-2020'
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id 
							,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,
							 II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , 
							EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus	
							,c.UserName  
							,p.UserName

							,IM.MaterialMasterId
							,MM.UserName 
						-- , IM.ArticleId
							, ART.StandardName 
							
							, ISNULL(FCV.UserName,'')  
							--, IM.SecondCharacteristicsId
							--, SC.UserName AS SecondCharacteristics
							--, IM.SecondCharacteristicsValueId
							, ISNULL(SCV.UserName,'')  
							--, IM.ThirdCharacteristicsId
							--, TC.UserName AS ThirdCharacteristics
							--, IM.ThirdCharacteristicsValueId
							, ISNULL(TCV.UserName,''),II.[Status] ";
					return _sqlRepository.GetDataTable(sql);
				}


				else
				{
					var sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							,c.UserName as Company
							,p.UserName as Plant
							
							
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, format(II.DocDate,'dd-MMM-yyyy')DocDate, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'
                            ,EI.EmployeeName CheckedByName,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus


							--,MT.UserName MaterialType
							--,MGM.UserName AS MaterialGroupMasterName
							,IM.MaterialMasterId
							,MM.UserName MaterialMasterName
						-- , IM.ArticleId
							, ART.StandardName ArticleName
							
							--, IM.FirstCharacteristicsId
							--, FC.UserName AS FirstCharacteristics
							--, IM.FirstCharacteristicsValueId
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							--, IM.SecondCharacteristicsId
							--, SC.UserName AS SecondCharacteristics
							--, IM.SecondCharacteristicsValueId
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							--, IM.ThirdCharacteristicsId
							--, TC.UserName AS ThirdCharacteristics
							--, IM.ThirdCharacteristicsValueId
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
						
						    ,Posted=CASE WHEN II.[Status]='Posted' then 'YES' else 'NO' END



							FROM [TRN].[InventoryScrap] AS II
							left join org.company c on c.id= ii.companyid
							left join org.Plant p on p.id= ii.PlantId
							
							

							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy



							LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IID.InventoryMaterialId
							left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id



							WHERE II.PlantId='" + identity.PlantId + @"' 

                            AND convert(Date,II.ScrapDate) <= '" + toDate + @"'
			               -- AND II.CheckedByStatus='For Checking'
							--AND ISNULL(II.[Status],'') <>'Posting' 
							--AND II.ScrapDate Between '1-Jan-2020' ANd '1-Jan-2020'
							GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id 
							,II.ToCurrencyRate, II.DocRefNo, II.DocDate ,
							 II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
                            ,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , 
							EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus	
							,c.UserName  
							,p.UserName

							,IM.MaterialMasterId
							,MM.UserName 
							, ART.StandardName 
							, ISNULL(FCV.UserName,'')  
							, ISNULL(SCV.UserName,'')  
							, ISNULL(TCV.UserName,''),II.[Status] ";
					return _sqlRepository.GetDataTable(sql);
				}

			}

			catch (Exception ex)
			{
				throw ex;
			}
		}
        public IWorkbook InventoryScrapReportList(string companyGroupId, string companyId, string plantId, string FromDate, string ToDate)
        {

            //Start EmployeeAdvanceDueList

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtInventoryScrapReportList = GetInventoryScrapReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, FromDate, ToDate);



            if (dtInventoryScrapReportList.Rows.Count == 0)
                throw new Exception("No data found");
            // throw new Exception("To date must be above or equal to From Date.");

            worksheet.Name = "InventroyScrapReport";
            var _rowd = 4;
            if (FromDate != "" && ToDate != "")
            {


                worksheet[_rowd, 4].Text = ToDate + " " + "To" + " " + ToDate;

                worksheet.UsedRange.CellStyle.Font.Size = 8;
                //sheet1.UsedRange.CellStyle.Font.Bold = true;
                worksheet.Range[_rowd, 3, _rowd, 5].Merge();
                //sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            }

            else
            {

                worksheet[_rowd, 4].Text = ToDate;
                worksheet.UsedRange.CellStyle.Font.Size = 8;
                worksheet.UsedRange.CellStyle.Font.Bold = false;
                worksheet.Range[_rowd, 3, _rowd, 4].Merge();
                //sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            }

            var _rows = 5;
            worksheet[_rows, 5].Text = "Report Ref No: ";
            worksheet.Range[_rows, 3, _rows, 6].CellStyle.Font.Size = 8;
            worksheet.Range[_rows, 3, _rows, 6].Merge();
            worksheet.UsedRange.CellStyle.Font.Bold = false;
            _rows++;

            int COL = 1; int ROW = 7;
            int startCol = COL;

            worksheet[ROW, COL].Text = "SL.No";
            int colSLNO = COL;
            worksheet[ROW, COL].ColumnWidth = 7;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "Entity";
            int colEntity = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "Company";
            int colCompany = COL;
            worksheet[ROW, COL].ColumnWidth = 18;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "Plant";
            int colPlant = COL;
            worksheet[ROW, COL].ColumnWidth = 18;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "Type";
            int colissuetype = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "Material Storage";
            int colMaterialStorage = COL;
            worksheet[ROW, COL].ColumnWidth = 18;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "Issue Date";
            int colIssueDate = COL;
            worksheet[ROW, COL].ColumnWidth = 14;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;



            worksheet[ROW, COL].Text = "Remarks";
            int colRemarks = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "DocRefNo";
            int colDocRefNo = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "Doc Date";
            int colDocDate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "NoteForAccounts";
            int colNoteForAccounts = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;


            worksheet[ROW, COL].Text = "Checked By";
            int colCheckedByName = COL;
            worksheet[ROW, COL].ColumnWidth = 17;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "Checked Status";
            int colCheckedByStatus = COL;
            worksheet[ROW, COL].ColumnWidth = 17;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "Approved By";
            int colApprovedByName = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "Approved Status";
            int colApprovedByStatus = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW - 1, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "Material Master";
            int colMaterialMasterName = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "Article";
            int colArticleName = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;


            worksheet[ROW, COL].Text = "SKU1";
            int colFirstCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;


            worksheet[ROW, COL].Text = "SKU2";
            int colSecondCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "SKU3";
            int colThirdCharacteristicsValue = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;


            worksheet[ROW, COL].Text = "Posted";
            int colPosted = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Qty";
            int colQty = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            COL++;

            worksheet[ROW, COL].Text = "Amount";
            int colAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 10f;

            worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            ROW++;

            for (int i = 0; i < dtInventoryScrapReportList.Rows.Count; i++)
            {

                worksheet[ROW, colSLNO].Number = (i + 1);

                worksheet[ROW, colEntity].Text = dtInventoryScrapReportList.Rows[i]["Entity"].ToString();
                worksheet[ROW, colCompany].Text = dtInventoryScrapReportList.Rows[i]["Company"].ToString();

                worksheet[ROW, colPlant].Text = dtInventoryScrapReportList.Rows[i]["Plant"].ToString();
                worksheet[ROW, colissuetype].Text = dtInventoryScrapReportList.Rows[i]["issuetype"].ToString();
                worksheet[ROW, colIssueDate].Text = dtInventoryScrapReportList.Rows[i]["IssueDate"].ToString();
                worksheet[ROW, colMaterialStorage].Text = dtInventoryScrapReportList.Rows[i]["MaterialStorage"].ToString();
                worksheet[ROW, colQty].Number = clsStaticInfo.dbl(dtInventoryScrapReportList.Rows[i]["Qty"].ToString());
                worksheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtInventoryScrapReportList.Rows[i]["Amount"].ToString());
                worksheet[ROW, colRemarks].Text = dtInventoryScrapReportList.Rows[i]["Remarks"].ToString();


                worksheet[ROW, colDocRefNo].Text = dtInventoryScrapReportList.Rows[i]["DocRefNo"].ToString();
                worksheet[ROW, colDocDate].Text = dtInventoryScrapReportList.Rows[i]["DocDate"].ToString();
                worksheet[ROW, colNoteForAccounts].Text = dtInventoryScrapReportList.Rows[i]["NoteForAccounts"].ToString();
                worksheet[ROW, colCheckedByName].Text = dtInventoryScrapReportList.Rows[i]["CheckedByName"].ToString();
                worksheet[ROW, colApprovedByName].Text = dtInventoryScrapReportList.Rows[i]["ApprovedByName"].ToString();
                worksheet[ROW, colCheckedByStatus].Text = dtInventoryScrapReportList.Rows[i]["CheckedByStatus"].ToString();
                worksheet[ROW, colApprovedByStatus].Text = dtInventoryScrapReportList.Rows[i]["ApprovedByStatus"].ToString();
                worksheet[ROW, colMaterialMasterName].Text = dtInventoryScrapReportList.Rows[i]["MaterialMasterName"].ToString();
                worksheet[ROW, colArticleName].Text = dtInventoryScrapReportList.Rows[i]["ArticleName"].ToString();
                worksheet[ROW, colFirstCharacteristicsValue].Text = dtInventoryScrapReportList.Rows[i]["FirstCharacteristicsValue"].ToString();
                worksheet[ROW, colSecondCharacteristicsValue].Text = dtInventoryScrapReportList.Rows[i]["SecondCharacteristicsValue"].ToString();
                worksheet[ROW, colThirdCharacteristicsValue].Text = dtInventoryScrapReportList.Rows[i]["ThirdCharacteristicsValue"].ToString();

                worksheet[ROW, colPosted].Text = dtInventoryScrapReportList.Rows[i]["Posted"].ToString();
                // worksheet[row, colpurchaseprice].numberformat = clsstaticinfo.numberformat();




                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            //worksheet.UsedRange.CellStyle.Font.Size = 8f;



            ReportUtility reportUtility = new ReportUtility();

            reportUtility.PlantHeader(ref worksheet, endCol, "Inventory Scrap", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 3, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A8"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 8;

            #endregion Freeze Panes


            return workbook;
        }

        public IEnumerable<object> GetCheckedApprovedListScrapQuery(string tabType)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";


                if (tabType == "UnCheckedList")
                {
                    sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.CheckedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus='For Checking' 
							AND II.ApprovedByStatus IS NULL 
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus
					Order BY II.ScrapDate DESC";
                }
                else if (tabType == "HoldRejectCheckedList")
                {
                    sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.CheckedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus='Hold' OR II.CheckedByStatus='Reject'
							AND II.ApprovedByStatus IS NULL 
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus
					Order BY II.ScrapDate DESC";
                }
                else if (tabType == "CheckedList")
                {
                    sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.CheckedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus='Checked' 
							AND II.ApprovedByStatus ='For Approval'
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus
					Order BY II.ScrapDate DESC";
                }
                else if (tabType == "UnApprovedList")
                {
                    sql = @"SELECT * FROM(
                                SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.ApprovedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus='Checked' 
							AND II.ApprovedByStatus ='For Approval'
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus
					
							

					UNION ALL

                                SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.ApprovedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus IS NULL
							AND II.ApprovedByStatus ='For Approval'
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus				
							
                                )X
                                Order BY IssueDate DESC";
                }
                else if (tabType == "HoldRejectApprovedList")
                {
                    sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.ApprovedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus ='Checked'
							AND (II.ApprovedByStatus ='Hold' OR II.ApprovedByStatus ='Reject')
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus
					Order BY II.ScrapDate DESC";
                }
                else if (tabType == "ApprovedList")
                {
                    sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.ScrapDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,SUM(IID.TransactionQty) Qty
							,SUM(IID.PolicyAmount) Amount
							,II.Remarks,II.Id AS IssueId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.CurrencyId
							,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts',EI.EmployeeName CheckedByName
							,II.CheckedBy,EI1.EmployeeName ApprovedByName, EI1.ApprovedBy,II.CheckedByStatus,II.ApprovedByStatus
					FROM[TRN].[InventoryScrap] AS II
							left JOIN TRN.InventoryScrapDetail AS IID ON IID.InventoryScrapId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId=IID.Id	
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=II.CheckedBy
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=II.ApprovedBy
					WHERE II.PlantId= '" + identity.PlantId + @"' AND II.ApprovedBy= '" + identity.EmployeeId + @"'
							AND II.CheckedByStatus ='Checked'
							AND II.ApprovedByStatus ='Approved'
							AND ISNULL(II.[Status],'') <>'Posting' 
					GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
							,II.ScrapDate, MS.UserName
							,II.IssueType,E.UserName,II.Remarks,II.Id,II.ToCurrencyRate, II.DocRefNo, II.DocDate 
							, II.CurrencyId,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 
							,EI.EmployeeName ,II.CheckedBy,EI1.EmployeeName , EI1.ApprovedBy
							,II.CheckedByStatus,II.ApprovedByStatus
					Order BY II.ScrapDate DESC";
                }


                return  _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

		public GridModel Querywithoutpo(GridParameter parameters, string inveReveiveId)
		{
			try
			{
				parameters.CmdText = @"DECLARE @inventoryReceiveId VARCHAR(10)='" + inveReveiveId + @"'
				                             , @totalReceiveAmount DECIMAL(18, 4)=0
				                             , @totalServiceAmount DECIMAL(18, 4)=0
				                             , @totalSvcTaxAmount DECIMAL(18, 4)=0
				                  SET @totalReceiveAmount=(SELECT ISNULL(SUM(ISNULL(MaterialTranAmount, 0)),1) FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId=@inventoryReceiveId)
				                  SET @totalServiceAmount=(SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[InventoryService] WHERE InventoryReceiveId=@inventoryReceiveId)
				                  SET @totalSvcTaxAmount=(SELECT ISNULL(SUM(ISNULL(TaxAmount, 0)),0) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveId=@inventoryReceiveId AND InventoryServiceId<>'')
				                  SELECT IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
				                      , MGM.UserName AS MaterialGroupMasterName
				                      , IM.MaterialMasterId, MM.UserName MaterialMasterName
				                      , IM.ArticleId, ART.StandardName ArticleName

				                      , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristicText
				                      , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue

				                      , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristicText
				                      , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue

				                      , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristicText
				                      , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue

				                      , IRD.TransactionQty
				                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
				                      , IRD.MaterialTranRate AS TransactionRate
				                      , CU.Code AS CurrencyName, IR.ToCurrencyRate
				                       , (IRD.MaterialTranAmount) AS TrnAmount
				                      , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                         
				                      , IRD.TotalTaxAmount AS BaseTaxAmount
				                   , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
				                   , IRD.ChargesTranAmount AS ChargesAmount
				                   ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
	                               , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                   ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                   , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                   , IRD.CountryId
				                    , IRD.TotalMaterialTranAmount AS TotalMaterialTranAmount  
                                    ,null TaxList
                                    ,IRD.InventoryReceiveId,IRD.BaseUOMId,IRD.InventoryMaterialId, IRD.MaterialStorageId
                                    ,isnull(IRD.ShortageQty,0) ShortageQty,isnull(IRD.RejectionQty,0) RejectionQty,isnull(IRD.ApprovedQty,0) ApprovedQty
                                    ,(IRD.TransactionQty-isnull(IRD.ShortageQty,0)) AS NetQty
                                    ,IRD.BaseQty
									,IRD.BaseUoMFactor,IRD.Description
                                    ,null DataChangeFlag,IRD.ShortRejFlag
                                   ,IRD.ShortageRatePercent ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent RejectionRate,IRD.RejectValue RejectionValue,IRD.RejectClamPercent RejectionClamRate
                                   ,C.Id CountryId,C.UserName CountryName,IRD.LotNumber,IRD.Diameter,IRD.Type
				                  FROM TRN.InventoryMaterial AS IM
				                  JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
				                  LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
				                  LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
				                  LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
				                  LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
				                  LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
				                  LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
				                  LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
				                  LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
				                  JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
				                  JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
				                  JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
				                  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                  left Join [SCS].[Country] AS C ON C.Id=IM.CountryId
				                  WHERE IRD.InventoryReceiveId=@inventoryReceiveId And Im.MaterialMasterId is not null --Order BY IRD.Id ASC

                                  UNION ALL

								SELECT  IM.Id, IRD.Id AS InventoryReceiveDetailId,IRD.id as RCBDetailsID,IRD.PODetailsId,IRD.POId
				                      , '' AS MaterialGroupMasterName
				                      , '' MaterialMasterId
									  ,'' UserName
				                      , '' ArticleId
									  , '' StandardName
				                      , IM.FirstCharacteristicsId, '' AS FirstCharacteristics
				                      , IM.FirstCharacteristicsValueId, '' AS FirstCharacteristicsValue
				                      , IM.SecondCharacteristicsId, '' AS SecondCharacteristics
				                      , IM.SecondCharacteristicsValueId, '' AS SecondCharacteristicsValue
				                      , IM.ThirdCharacteristicsId, '' AS ThirdCharacteristics
				                      , IM.ThirdCharacteristicsValueId, '' AS ThirdCharacteristicsValue
				                      , IRD.TransactionQty
				                      , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM
				                      , IRD.MaterialTranRate AS TransactionRate
				                      , CU.Code AS CurrencyName, IR.ToCurrencyRate
				                       , (IRD.MaterialTranAmount) AS TrnAmount
				                      , IRD.ToTalMaterialBooksCurrencyAmount AS BaseAmount
                                         
				                      , IRD.TotalTaxAmount AS BaseTaxAmount
				                   , TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)
				                   , IRD.ChargesTranAmount AS ChargesAmount
				                   ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
	                               , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                   ,ServiceCharge=(@totalServiceAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                   , ServiceTax=(@totalSvcTaxAmount/ISNULL(NULLIF(@totalReceiveAmount,0), 1))*IRD.MaterialTranAmount
				                   , IRD.CountryId
				                    , IRD.TotalMaterialTranAmount AS TotalMaterialTranAmount  
                                    ,null TaxList
                                    ,IRD.InventoryReceiveId,IRD.BaseUOMId,IRD.InventoryMaterialId, IRD.MaterialStorageId
                                    ,isnull(IRD.ShortageQty,0) ShortageQty,isnull(IRD.RejectionQty,0) RejectionQty,isnull(IRD.ApprovedQty,0) ApprovedQty
                                    ,(IRD.TransactionQty-isnull(IRD.ShortageQty,0)) AS NetQty
                                    ,IRD.BaseQty
									,IRD.BaseUoMFactor,IRD.Description
                                    ,null DataChangeFlag,IRD.ShortRejFlag
                                    ,IRD.ShortageRatePercent ShortageRate,IRD.ShortageValue,IRD.RejectRatePercent RejectionRate,IRD.RejectValue RejectionValue,IRD.RejectClamPercent RejectionClamRate
                                    ,C.Id CountryId,C.UserName CountryName,IRD.LotNumber,IRD.Diameter,IRD.Type
				                  FROM TRN.InventoryMaterial AS IM
				                  --JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
				                  --LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
				                  --LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
				                  --LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
				                  --LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
				                  --LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
				                  --LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
				                  --LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
				                  --LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
				                  JOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
				                  JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
				                  JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
				                  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                  left Join [SCS].[Country] AS C ON C.Id=IM.CountryId
				                  WHERE IRD.InventoryReceiveId=@inventoryReceiveId And Im.MaterialMasterId is null Order BY IRD.Id ASC";

				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> MaterialSalesDetails(string inveReveiveId, string POID)

		{
			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				var sql = @"SELECT IR.Id IssueNo
							,IR.CompanyGroupId ,IR.CompanyId ,Plant.GSTIN  ,null PODepended 
							,IR.Id PONumber   ,IR.IssueRequestMasterId ,REPLACE(Convert(VARCHAR(11), IR.SalesDate, 106), ' ', '-') AS PODate
							,null BaseOnDueDate ,NULL AS MatureDate ,null InvoicingPartyPlantId	 ,null InvoicingPartyName
							,null InvoicePartyAddressMasterId ,null InvoicingPartyGSTIN ,null InvoicingByAddress ,null DeliveryByAddress
							,null DeliveryPartya ,null DeliveryPartyPlantId	 ,IOM.MaterialMasterId ,null DocRefNo  ,null DocDate
							,IR.AddedBy ,IR.AddedDate ,IR.UpdatedBy ,IR.UpdatedDate ,null IsApproved ,null PartyType ,null VendorName
							,null VendorAddressMasterId ,null VendorGSTIN ,null IsNonCreditable ,IR.CurrencyId ,CUR.Code CurrencyName
							,null as ToCurrencyRate ,null AS BaseCurrencyName ,NULL PaymentTerm ,MM.UserName Materials ,MM.MaterialGroupMasterId
							,MGM.UserName MaterialGroupMaster ,IOM.ArticleId ,MMA.StandardName Article ,FC.Id FirstCharId ,FC.UserName FirstChar
							,IOM.FirstCharacteristicsValueId ,FCV.UserName AS SKU1 ,IOM.SecondCharacteristicsValueId ,SCV.UserName AS SKU2
							,IOM.ThirdCharacteristicsValueId
							,TCV.UserName AS SKU3
							,SC.Id SecondCharId
							,SC.UserName SecondChar
							,TC.Id ThirdCharId
							,TC.UserName ThirdChar
							--,ROUND(IRD.BaseQty, 2) Qty
							--,ROUND(IRD.PolicyRate, 2) TransactionRate
							--,ROUND((IRD.PolicyAmount), 2) AS TrnAmount
							,ROUND(ISH.Qty, 2) Qty	
							,ROUND(ISH.SalesRate, 2) TransactionRate
							,ROUND(ISH.TotalAmount,2) TrnAmount
							,null BaseAmount
							,null AS BaseTaxAmount
							,TaxAmount = (
								SELECT SUM(TaxAmount)
								FROM [TRN].[PurchaseOrderTax]
								WHERE InventoryReceiveDetailId = IRD.Id
								)
							,ServiceTaxAmount = (
								SELECT SUM(TotalTaxAmount)
								FROM [TRN].[POService]
								WHERE InventoryReceiveId = IOM.Id
								)
							,null ChargesTranAmount
							,null CountryId
							,IRD.BaseUOMId
							,TUoM.UserName AS UOM
							,IRD.Id InventoryReceiveDetailId
							,IR.IssueType,E.UserName AS Entity,IR.Remarks,EI.EmployeeName,CC.UserName CostCenter,IRD.Comments,ROUND(ISH.Qty, 2) Qty,ISH.TotalAmount,ISH.TotalAmount/Qty SalesRate
							FROM TRN.InventorySales IR
						LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
						LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
						LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
						LEFT JOIN trn.InventorySalesDetail IRD ON IR.Id = IRD.InventorySalesId		
						--LEFT JOIN trn.InventorySalesHistory IIH ON IIH.InventorySalesDetailId = IRD.Id
                         --LEFT JOIN (select InventorySalesDetailId,ROUND(sum(Qty), 2) Qty,
								--ROUND(sum(SalesRate), 2) SalesRate,
								--ROUND((sum(TotalAmount)), 2) AS TotalAmount 
							--from  TRN.InventorySalesHistory 
							--group by InventorySalesDetailId
							--)  ISH ON ISH.InventorySalesDetailId=IRD.Id
						 				                                   
						 LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id)  ISH ON ISH.Id=IRD.Id
			                                   
						LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
						left JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
						left JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
						left JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
						LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
						LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.CurrencyId
						Left JOIN [ORG].[Entity] E On E.id=IR.EntityId
						LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId = TUoM.Id
						LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IRD.CostCenterId
						--WHERE IR.Id IS NULL";

				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> MaterialScrapDetails(string inveReveiveId, string POID)

		{
			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				var sql = @" SELECT IR.Id IssueNo
							, IR.CompanyGroupId ,IR.CompanyId ,Plant.GSTIN  ,null PODepended  ,IR.Id PONumber  
							, IR.IssueRequestMasterId  ,REPLACE(Convert(VARCHAR(11), IR.ScrapDate, 106), ' ', '-') AS PODate
							, null BaseOnDueDate ,NULL AS MatureDate ,null InvoicingPartyPlantId
							, null InvoicingPartyName ,null InvoicePartyAddressMasterId ,null InvoicingPartyGSTIN
							, null InvoicingByAddress
							,null DeliveryByAddress
							,null DeliveryPartya
							,null DeliveryPartyPlantId		
							,IOM.MaterialMasterId
							,null DocRefNo 
							,null DocDate
							,IR.AddedBy
							,IR.AddedDate
							,IR.UpdatedBy
							,IR.UpdatedDate
							,null IsApproved
							,null PartyType
							,null VendorName
							,null VendorAddressMasterId
							,null VendorGSTIN
							,null IsNonCreditable
							-- ,'' CurrencyId
							,IR.CurrencyId
							--,null AS CurrencyName
							,CUR.Code CurrencyName
							,null as ToCurrencyRate
							,null AS BaseCurrencyName
							,NULL PaymentTerm
							,MM.UserName Materials
							,MM.MaterialGroupMasterId
							,MGM.UserName MaterialGroupMaster
							,IOM.ArticleId
							,MMA.StandardName Article
							,FC.Id FirstCharId
							,FC.UserName FirstChar
							,IOM.FirstCharacteristicsValueId
							,FCV.UserName AS SKU1
							,IOM.SecondCharacteristicsValueId
							,SCV.UserName AS SKU2
							,IOM.ThirdCharacteristicsValueId
							,TCV.UserName AS SKU3
							,SC.Id SecondCharId
							,SC.UserName SecondChar
							,TC.Id ThirdCharId
							,TC.UserName ThirdChar
							--,ROUND(IRD.BaseQty, 2) Qty
							--,ROUND(IRD.PolicyRate, 2) TransactionRate
							--,ROUND((IRD.PolicyAmount), 2) AS TrnAmount
							,null BaseAmount
							,null AS BaseTaxAmount
							--,TaxAmount = (
							--	SELECT SUM(TaxAmount)
							--	FROM [TRN].[PurchaseOrderTax]
							--	WHERE InventoryReceiveDetailId = IRD.Id
							--	)
							--,ServiceTaxAmount = (
							--	SELECT SUM(TotalTaxAmount)
							--	FROM [TRN].[POService]
							--	WHERE InventoryReceiveId = IOM.Id
							--	)
							,null ChargesTranAmount
							,null CountryId
							--,IRD.BaseUOMId
							,TUoM.UserName AS UOM
							--,IRD.Id InventoryReceiveDetailId
							--,IR.IssueType,E.UserName AS Entity,IR.Remarks,EI.EmployeeName,CC.UserName CostCenter,IRD.Comments,IIH.SalesRate,IIH.TotalAmount
                           ,IIH.Qty
						FROM TRN.InventoryScrap IR
						LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
						LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
						LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
						LEFT JOIN trn.InventoryScrapDetail IRD ON IR.Id = IRD.InventoryScrapId		
						LEFT JOIN trn.InventoryScrapHistory IIH ON IIH.InventoryScrapDetailId = IRD.Id	
						 				                                   
						LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
						left JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
						left JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
						left JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
						LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
						LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.CurrencyId
						Left JOIN [ORG].[Entity] E On E.id=IR.EntityId
						--LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
						left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId = TUoM.Id
						--LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IRD.CostCenterId
						--WHERE IR.Id IS NULL";

				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> MaterialAdjustmentDetailsData(string inveReveiveId, string POID)

		{
			try
			{

				var sql = @"   SELECT IR.Id IssueNo
                                ,IR.CompanyGroupId
                                ,IR.CompanyId
                                ,Plant.GSTIN 
								,null PODepended 
								,IR.Id PONumber  
	                            ,IR.IssueRequestMasterId  
                                ,REPLACE(Convert(VARCHAR(11), IR.IssueDate, 106), ' ', '-') AS PODate
                                ,null BaseOnDueDate
                                ,NULL AS MatureDate
		                        ,null InvoicingPartyPlantId
		                       
		                             ,null InvoicingPartyName
                                                ,null InvoicePartyAddressMasterId
                                                ,null InvoicingPartyGSTIN
                                                ,null InvoicingByAddress
		                        ,null DeliveryByAddress
		                        ,null DeliveryParty
		                        ,null DeliveryPartyPlantId		
		                        ,IOM.MaterialMasterId
		                        ,null DocRefNo 
		                        ,null DocDate
		                        ,IR.AddedBy
		                        ,IR.AddedDate
		                        ,IR.UpdatedBy
		                        ,IR.UpdatedDate
		                        ,null IsApproved
		                        ,null PartyType
		                        ,null VendorName
                                ,null VendorAddressMasterId
                                ,null VendorGSTIN
                                --,Case When null IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
		                        ,null IsNonCreditable
		                       -- ,'' CurrencyId
                                ,IR.CurrencyId
	                            --,null AS CurrencyName
                                ,CUR.Code CurrencyName
	                            ,null as ToCurrencyRate
		                        ,null AS BaseCurrencyName
		                        ,NULL PaymentTerm
	                          ,MM.UserName Materials
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
	                          ,IOM.ArticleId
	                          ,MMA.StandardName Article
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IOM.FirstCharacteristicsValueId
	                          ,FCV.UserName AS SKU1
                              ,IOM.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SKU2
	                          ,IOM.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS SKU3
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                            ,ROUND(IRD.BaseQty, 2) Qty
	                          ,ROUND(IRD.PolicyRate, 2) TransactionRate
	                          ,ROUND((IRD.PolicyAmount), 2) AS TrnAmount
	                          ,null BaseAmount
	                          ,null AS BaseTaxAmount
	                          ,TaxAmount = (
		                            SELECT SUM(TaxAmount)
		                            FROM [TRN].[PurchaseOrderTax]
		                            WHERE InventoryReceiveDetailId = IRD.Id
		                            )
	                          ,ServiceTaxAmount = (
		                            SELECT SUM(TotalTaxAmount)
		                            FROM [TRN].[POService]
		                            WHERE InventoryReceiveId = IOM.Id
		                            )
	                          ,null ChargesTranAmount
	                          ,null CountryId
	                          ,IRD.BaseUOMId
	                          ,TUoM.UserName AS UOM
                              ,IRD.Id InventoryReceiveDetailId
							  ,IR.IssueType,E.UserName AS Entity,IR.Remarks,EI.EmployeeName,CC.UserName CostCenter
                              FROM TRN.PhysicalStockAdjustmentMaster IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN trn.PhysicalStockAdjustmentDetail IRD ON IR.Id = IRD.PhysicalStockAdjustmentMasterID	
                         LEFT JOIN trn.PhysicalStockAdjustmentHistory IIH ON IIH.PhysicalStockAdjustmentDetailId = IRD.Id	
						 				                                   
                         LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
                         INNER JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         INNER JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
                         LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
                         LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.CurrencyId
						 Left JOIN [ORG].[Entity] E On E.id=IR.EntityId
						 LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                         JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId = TUoM.Id
						 LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IRD.CostCenterId
						--WHERE IR.Id IS NULL
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
		public IEnumerable<object> GetIssueReturnRegister(string fromDate, string toDate, string Type)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				sql = @"SELECT II.Id AS IssueId
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate
	                        ,En.UserName AS Entityname
	                        ,MS.UserName AS MaterialStorageName
	                        ,II.STATUS
	                        ,v.VoucherNo
	                        ,IRH.Id IssueDetailId
	                        ,IRH.InventoryIssueReturnId
	                        --,IID.InventoryMaterialId
	                        ,MT.UserName MaterialType
	                        ,MGM.UserName AS MaterialGroupMasterName
	                        ,IM.MaterialMasterId
	                        ,MM.UserName MaterialMasterName
	                        -- , IM.ArticleId
	                        ,ART.StandardName ArticleName
	                        ,IsAsset = CASE 
		                        WHEN MM.IsAsset = 0
			                        THEN 'No'
		                        ELSE 'Yes'
		                        END
	                        --, IM.FirstCharacteristicsId
	                        ,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId
	                        ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
	                        ,IM.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId
	                        ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
	                        ,IM.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId
	                        ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
	                        ,IRH.InventoryReceiveDetailId
                            ,CC.UserName CostCenterName,EI.EmployeeName,TUOM.UserName AS UOM
							,IIH.StandardName StorageLocation,IRH.Qty,IRH.IssueRequestDetailId
                        FROM TRN.[InventoryIssueReturn] II
                        LEFT JOIN [TRN].InventoryIssueReturnHistory IRH ON II.Id = IRH.InventoryIssueReturnId	
                        LEFT JOIN ORG.CostCenter CC ON CC.Id=IRH.CostCenterId
                        LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                        LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IRH.InventoryMaterialId
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                        LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
                        LEFT join dbo.EmployeeInformation EI ON EI.SystemId=II.EmployeeId
						LEFT JOIN [HKP].[MaterialStorage] IIH ON IIH.id = IRH.StorageLocationId
                        LEFT JOIN [SCS].[UnitOfMeasurement] TUOM ON TUOM.id = IRH.BaseUOMId
                        where II.PlantId='" + identity.PlantId + "'AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";
				return _sqlRepository.GetDataCollection(sql);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public IEnumerable<object> GetDataByPhysicalStockAdjustment(string plantId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = @"SELECT E.UserName AS Entity ,isnull(II.IssueType,'') issuetype, II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,FORMAT(II.IssueDate, 'dd-MMM-yyyy') IssueDate, MS.UserName AS MaterialStorage
									 ,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName,SUM(IID.TransactionQty) Qty,SUM(IID.PolicyAmount) Amount,II.Remarks,II.Id AS IssueId,II.OrderRefNo
                                    FROM[TRN].PhysicalStockAdjustmentMaster
        AS II
                                JOIN TRN.PhysicalStockAdjustmentDetail AS IID ON IID.PhysicalStockAdjustmentMasterID= II.Id AND IID.IsAsset= 0
                                JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                                left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                                Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                                WHERE II.PlantId= '" + identity.PlantId + @"' AND ISNULL(II.[Status],'') <>'Posting' 
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,II.IssueDate, MS.UserName
									 ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  Order BY II.Id DESC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}
		public IEnumerable<object> MaterialIssueDetailsData1(string inveReveiveId, string POID)

		{
			try
			{
				var sql = @" SELECT IR.Id IssueNo
                                ,IR.CompanyGroupId
                                ,IR.CompanyId
                                ,Plant.GSTIN 
								,null PODepended 
								,IR.Id PONumber  
	                            ,IR.IssueRequestMasterId  
                                ,REPLACE(Convert(VARCHAR(11), IR.IssueDate, 106), ' ', '-') AS PODate
                                ,null BaseOnDueDate
                                ,NULL AS MatureDate
		                        ,null InvoicingPartyPlantId
		                       
		                             ,null InvoicingPartyName
                                                ,null InvoicePartyAddressMasterId
                                                ,null InvoicingPartyGSTIN
                                                ,null InvoicingByAddress
		                        ,null DeliveryByAddress
		                        ,null DeliveryParty
		                        ,null DeliveryPartyPlantId		
		                        ,IOM.MaterialMasterId
		                        ,null DocRefNo 
		                        ,null DocDate
		                        ,IR.AddedBy
		                        ,IR.AddedDate
		                        ,IR.UpdatedBy
		                        ,IR.UpdatedDate
		                        ,null IsApproved
		                        ,null PartyType
		                        ,null VendorName
                                ,null VendorAddressMasterId
                                ,null VendorGSTIN
                                --,Case When null IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
		                        ,null IsNonCreditable
		                       -- ,'' CurrencyId
                                ,IR.CurrencyId
	                            --,null AS CurrencyName
                                ,CUR.Code CurrencyName
	                            ,null as ToCurrencyRate
		                        ,null AS BaseCurrencyName
		                        ,NULL PaymentTerm
	                          ,MM.UserName Materials
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
	                          ,IOM.ArticleId
	                          ,MMA.StandardName Article
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IOM.FirstCharacteristicsValueId
	                          ,FCV.UserName AS SKU1
                              ,IOM.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SKU2
	                          ,IOM.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS SKU3
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                             ,ROUND(IIH.Qty, 2) Qty
	                          ,ROUND(IIH.Rate,2) TransactionRate
	                          ,ROUND((IIH.Qty*IIH.Rate), 2) AS TrnAmount
	                          ,null BaseAmount
	                          ,null AS BaseTaxAmount
	                          ,TaxAmount = (
		                            SELECT SUM(TaxAmount)
		                            FROM [TRN].[PurchaseOrderTax]
		                            WHERE InventoryReceiveDetailId = IRD.Id
		                            )
	                          ,ServiceTaxAmount = (
		                            SELECT SUM(TotalTaxAmount)
		                            FROM [TRN].[POService]
		                            WHERE InventoryReceiveId = IOM.Id
		                            )
	                          ,null ChargesTranAmount
	                          ,null CountryId
	                          ,IRD.BaseUOMId
	                          ,TUoM.UserName AS UOM
                              ,IRD.Id InventoryReceiveDetailId
							  ,IR.IssueType,E.UserName AS Entity,IR.Remarks,EI.EmployeeName,CC.UserName CostCenter,IRD.Comments
                              ,IsNULL(V.VoucherNo,'') VoucherNo ,IsPark=case when IR.VoucherId<>'' then 0 else 1 end
                              FROM TRN.InventoryIssue IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN trn.InventoryIssueDetail IRD ON IR.Id = IRD.InventoryIssueId		
                          LEFT JOIN (Select InventoryIssueDetailId,IssueRequestDetailId,Sum(Qty) Qty,sum(qty*rate)/Sum(Qty) Rate, sum(qty*rate) TrnAmount from trn.InventoryIssueHistory group by InventoryIssueDetailId,IssueRequestDetailId)IIH ON IIH.InventoryIssueDetailId = IRD.Id		
						 				                                   
                         LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
                         INNER JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         INNER JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
                         LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
                         LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.CurrencyId
						 Left JOIN [ORG].[Entity] E On E.id=IR.EntityId
						 LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                         JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId = TUoM.Id
						 LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IRD.CostCenterId
                         LEFT JOIN TRN.Voucher V ON V.Id=IR.VoucherId
						--WHERE IR.Id IS NULL
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
		public IEnumerable<object> MaterialIssueDetailsData(string inveReveiveId, string POID)
		{
			try
			{
				var sql = @"  SELECT IR.Id IssueNo
                                ,IR.CompanyGroupId
                                ,IR.CompanyId
                                ,Plant.GSTIN 
								,null PODepended 
								,IR.Id PONumber  
	                            ,IR.IssueRequestMasterId  
                                ,REPLACE(Convert(VARCHAR(11), IR.IssueDate, 106), ' ', '-') AS PODate
                                ,null BaseOnDueDate
                                ,NULL AS MatureDate
		                        ,null InvoicingPartyPlantId
		                             ,null InvoicingPartyName
                                                ,null InvoicePartyAddressMasterId
                                                ,null InvoicingPartyGSTIN
                                                ,null InvoicingByAddress
		                        ,null DeliveryByAddress
		                        ,null DeliveryParty
		                        ,null DeliveryPartyPlantId		
		                        ,IOM.MaterialMasterId
		                        ,null DocRefNo 
		                        ,null DocDate
		                        ,IR.AddedBy
		                        ,IR.AddedDate
		                        ,IR.UpdatedBy
		                        ,IR.UpdatedDate
		                        ,null IsApproved
		                        ,null PartyType
		                        ,null VendorName
                                ,null VendorAddressMasterId
                                ,null VendorGSTIN
                                --,Case When null IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
		                        ,null IsNonCreditable
                                ,IR.CurrencyId
                                ,CUR.Code CurrencyName
	                            ,null as ToCurrencyRate
		                        ,null AS BaseCurrencyName
		                        ,NULL PaymentTerm
	                          ,MM.UserName Materials
	                          ,MM.MaterialGroupMasterId
	                          ,MGM.UserName MaterialGroupMaster
	                          ,IOM.ArticleId
	                          ,MMA.StandardName Article
	                          ,FC.Id FirstCharId
	                          ,FC.UserName FirstChar
                              ,IOM.FirstCharacteristicsValueId
	                          ,FCV.UserName AS SKU1
                              ,IOM.SecondCharacteristicsValueId
	                          ,SCV.UserName AS SKU2
	                          ,IOM.ThirdCharacteristicsValueId
	                          ,TCV.UserName AS SKU3
	                          ,SC.Id SecondCharId
	                          ,SC.UserName SecondChar
	                          ,TC.Id ThirdCharId
	                          ,TC.UserName ThirdChar
	                          ,ROUND(IRD.Qty, 2) Qty
	                          ,null ChargesTranAmount
	                          ,null CountryId
                              ,IRD.Id InventoryReceiveDetailId,IRD.IsCapitalize,TUOM.UserName UOM,
							  IR.IssueType,E.UserName AS Entity,IR.Remarks,EI.EmployeeName,CC.UserName CostCenter, IIH.StandardName StorageLocation
                              FROM TRN.[InventoryIssueReturn] IR
                         LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                         LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                         LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                         LEFT JOIN [TRN].InventoryIssueReturnHistory IRD ON IR.Id = IRD.InventoryIssueReturnId		
						LEFT JOIN [TRN].[VoucherDetail]	VD ON VD.Id=IRD.CapitalizeVoucherDetailId
						 LEFT JOIN [HKP].[MaterialStorage] IIH ON IIH.id = IRD.StorageLocationId
                         LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                         INNER JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
                         INNER JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                         INNER JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
                         LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
                         LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
                         LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
                         LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
                         LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
                         LEFT JOIN [SCS].[Currency] AS CUR ON CUR.Id=IR.CurrencyId
						 Left JOIN [ORG].[Entity] E On E.id=IR.EntityId
						 LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
						 LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IRD.CostCenterId
                         LEFT JOIN [SCS].[UnitOfMeasurement] TUOM ON TUOM.id = IRD.BaseUOMId
						--WHERE IR.Id IS NULL
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
		public IEnumerable<object> GetApprovedIssueSlip()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = @"SELECT * FROM (
                                select x.Id,x.ProcessName,x.SalesOrderId,x.ProductionOrderId,x.PreparedBy,x.BuyerItemReferenceNo,x.OwnItemReferenceNo,x.BuyerOrderReferenceNo,x.OwnOrderReferenceNo,x.CustomerName,x.BUyerName,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate
                                ,Sum(x.RequestedQty) RequestedQty ,sum(isnull(x.IssueQty,0)) IssueQty,Balance=Sum(isnull(x.RequestedQty,0))-sum(isnull(x.IssueQty,0)),Sum(x.RejectedQty) RejectedQty
                                ,Orderspecific=CASE WHEN Orderspecific='Yes' Then 'Yes' else 'No' End from(
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty,IIH.IssueQty
                            ,RejectedQty,IRM.Orderspecific
							,p.UserName ProcessName
							,IRMSO.SalesOrderId
							,ISNULL(IRM.ProductionOrderId,'') ProductionOrderId
							,concatData1.BuyerItemReferenceNo
							,concatData1.OwnItemReferenceNo
							,concatData1.BuyerOrderReferenceNo
							,concatData1.OwnOrderReferenceNo
							,concatData1.CustomerName
							,concatData1.BUyerName
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            left JOIN [TRN].[IssueRequestMasterProcessMap] IRMPM ON IRMPM.IssueRequestMasterId=IRM.Id
							left JOIN HKP.Process p ON p.Id=IRMPM.ProcessId
							left join [TRN].[IssueRequestMasterSalesOrderMap] IRMSO ON IRMSO.IssueRequestMasterId=IRM.Id
							LEFT JOIN  (select IssueRequestDetailId,SUM(Qty) IssueQty from TRN.InventoryIssueHistory	group by IssueRequestDetailId)	IIH ON IIH.IssueRequestDetailId=IR.Id
							
							LEFT JOIN(
							SELECT distinct PDAMAP.IssueRequestMasterId
								,SalesOrderId=STUFF((select distinct ','+xpo.SalesOrderId from
								[TRN].[ProductionOrderDetail] xpo
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on xpo.SalesOrderId=xPDAMAP.SalesOrderId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


							  from  [TRN].[IssueRequestMasterSalesOrderMap] PDAMAP 
							  LEFT JOIN [TRN].[ProductionOrderDetail] IR ON IR.SalesOrderId = PDAMAP.SalesOrderId
							  --LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  --left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.IssueRequestMasterId
							)concatData ON concatData.IssueRequestMasterId = IRM.Id

							LEFT JOIN(
							    SELECT distinct PDAMAP.IssueRequestMasterId
								,BuyerItemReferenceNo=STUFF((select distinct ','+xpo.BuyerReferenceNo from
								[TRN].[MasterOrderItem] xpo
								left join  trn.SalesOrder item on item.MasterOrderItemId=xpo.Id
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on item.Id=xPDAMAP.SalesOrderId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,OwnItemReferenceNo=STUFF((select distinct ','+xpo.OwnReferenceNo from
								[TRN].[MasterOrderItem] xpo
								left join  trn.SalesOrder item on item.MasterOrderItemId=xpo.Id
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on item.Id=xPDAMAP.SalesOrderId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,BuyerOrderReferenceNo=STUFF((select distinct ','+MO.BuyerReferenceNo from
								[TRN].[MasterOrderItem] xpo
								left join trn.MasterOrder MO ON MO.id=xpo.MasterOrderId
								left join  trn.SalesOrder item on item.MasterOrderItemId=xpo.Id
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on item.Id=xPDAMAP.SalesOrderId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,OwnOrderReferenceNo=STUFF((select distinct ','+MO.OwnReferenceNo from
								[TRN].[MasterOrderItem] xpo
								left join trn.MasterOrder MO ON MO.id=xpo.MasterOrderId
								left join  trn.SalesOrder item on item.MasterOrderItemId=xpo.Id
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on item.Id=xPDAMAP.SalesOrderId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

										,CustomerName=STUFF((select distinct ','+party.UserName from
								[TRN].[MasterOrderItem] xpo
								left join trn.MasterOrder MO ON MO.id=xpo.MasterOrderId
								left join hkp.Party party on party.Id=MO.PartyId
								left join  trn.SalesOrder item on item.MasterOrderItemId=xpo.Id
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on item.Id=xPDAMAP.SalesOrderId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,BUyerName=STUFF((select distinct ','+Buyer.UserName from
								[TRN].[MasterOrderItem] xpo
								left join trn.MasterOrder MO ON MO.id=xpo.MasterOrderId
								left join hkp.Buyer Buyer on Buyer.Id=MO.BuyerId
								left join  trn.SalesOrder item on item.MasterOrderItemId=xpo.Id
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on item.Id=xPDAMAP.SalesOrderId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

							  from  [TRN].[IssueRequestMasterSalesOrderMap] PDAMAP 
							  LEFT JOIN trn.SalesOrder IR ON IR.Id = PDAMAP.SalesOrderId
							  --LEFT JOIN [TRN].[MasterOrderItem] C ON C.Id=IR.MasterOrderItemId
							  --left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.IssueRequestMasterId
							)concatData1 ON concatData.IssueRequestMasterId = IRM.Id

                           Where IRM.CheckedBy IS NOT NULL 
						   AND IRM.CheckedByStatus='Checked' 
						   AND IRM.AuthorizedByStatus='Approved' 
						   AND IRM.AuthorizedBy IS NOT null  
						   AND IRM.IssueSlipType='InventorySlip'
						   AND IRM.PlantId='" + identity.PlantId + @"'
                           --Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='Checked' OR IRM.CheckedByStatus='Approval'AND IRM.AuthorizedByStatus IS Not NULL  AND IRM.AuthorizedBy IS null OR IRM.AuthorizedBy IS NOT null And IRM.PreparedBy=''
                           --Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='ForChecked' AND IRM.AuthorizedByStatus IS NULL AND IRM.IssueSlipType='AssetSlip' AND IRM.AuthorizedBy IS null --And IRM.PreparedBy=''
                           UNION ALL
						   SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty,IIH.IssueQty
                                ,RejectedQty
								,IRM.Orderspecific
								,p.UserName ProcessName
								,concatData.SalesOrderId
								,ISNULL(IRM.ProductionOrderId,'') ProductionOrderId
								,concatData1.BuyerItemReferenceNo
								,concatData1.OwnItemReferenceNo
								,concatData1.BuyerOrderReferenceNo
							,concatData1.OwnOrderReferenceNo
							,concatData1.CustomerName
							,concatData1.BUyerName
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
							left JOIN [TRN].[IssueRequestMasterProcessMap] IRMPM ON IRMPM.IssueRequestMasterId=IRM.Id
							left JOIN HKP.Process p ON p.Id=IRMPM.ProcessId
                            left join [TRN].[IssueRequestMasterSalesOrderMap] IRMSO ON IRMSO.IssueRequestMasterId=IRM.Id
							LEFT JOIN  (select IssueRequestDetailId,SUM(Qty) IssueQty from TRN.InventoryIssueHistory	group by IssueRequestDetailId)	IIH ON IIH.IssueRequestDetailId=IR.Id					

							LEFT JOIN(
							SELECT distinct PDAMAP.IssueRequestMasterId
								,SalesOrderId=STUFF((select distinct ','+xpo.SalesOrderId from
								[TRN].[ProductionOrderDetail] xpo
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on xpo.SalesOrderId=xPDAMAP.SalesOrderId

								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

							  from  [TRN].[IssueRequestMasterSalesOrderMap] PDAMAP 
							  LEFT JOIN [TRN].[ProductionOrderDetail] IR ON IR.SalesOrderId = PDAMAP.SalesOrderId
							  --LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  --left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.IssueRequestMasterId
							)concatData ON concatData.IssueRequestMasterId = IRM.Id
							LEFT JOIN(
							    SELECT distinct PDAMAP.IssueRequestMasterId
								,BuyerItemReferenceNo=STUFF((select distinct ','+xpo.BuyerReferenceNo from
								[TRN].[MasterOrderItem] xpo
								left join  trn.SalesOrder item on item.MasterOrderItemId=xpo.Id
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on item.Id=xPDAMAP.SalesOrderId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,OwnItemReferenceNo=STUFF((select distinct ','+xpo.OwnReferenceNo from
								[TRN].[MasterOrderItem] xpo
								left join  trn.SalesOrder item on item.MasterOrderItemId=xpo.Id
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on item.Id=xPDAMAP.SalesOrderId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,BuyerOrderReferenceNo=STUFF((select distinct ','+MO.BuyerReferenceNo from
								[TRN].[MasterOrderItem] xpo
								left join trn.MasterOrder MO ON MO.id=xpo.MasterOrderId
								left join  trn.SalesOrder item on item.MasterOrderItemId=xpo.Id
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on item.Id=xPDAMAP.SalesOrderId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,OwnOrderReferenceNo=STUFF((select distinct ','+MO.OwnReferenceNo from
								[TRN].[MasterOrderItem] xpo
								left join trn.MasterOrder MO ON MO.id=xpo.MasterOrderId
								left join  trn.SalesOrder item on item.MasterOrderItemId=xpo.Id
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on item.Id=xPDAMAP.SalesOrderId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,CustomerName=STUFF((select distinct ','+party.UserName from
								[TRN].[MasterOrderItem] xpo
								left join trn.MasterOrder MO ON MO.id=xpo.MasterOrderId
								left join hkp.Party party on party.Id=MO.PartyId
								left join  trn.SalesOrder item on item.MasterOrderItemId=xpo.Id
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on item.Id=xPDAMAP.SalesOrderId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,BUyerName=STUFF((select distinct ','+Buyer.UserName from
								[TRN].[MasterOrderItem] xpo
								left join trn.MasterOrder MO ON MO.id=xpo.MasterOrderId
								left join hkp.Buyer Buyer on Buyer.Id=MO.BuyerId
								left join  trn.SalesOrder item on item.MasterOrderItemId=xpo.Id
								INNER JOin [TRN].[IssueRequestMasterSalesOrderMap] xPDAMAP on item.Id=xPDAMAP.SalesOrderId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

							  from  [TRN].[IssueRequestMasterSalesOrderMap] PDAMAP 
							  LEFT JOIN trn.SalesOrder IR ON IR.Id = PDAMAP.SalesOrderId
							  --LEFT JOIN [TRN].[MasterOrderItem] C ON C.Id=IR.MasterOrderItemId
							  --left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.IssueRequestMasterId
							)concatData1 ON concatData1.IssueRequestMasterId = IRM.Id

                           Where IRM.CheckedBy IS  NULL 
						   AND IRM.CheckedByStatus IS NULL
						   AND IRM.AuthorizedByStatus='Approved' 
						   AND IRM.AuthorizedBy IS NOT null  
						   AND IRM.IssueSlipType='InventorySlip'
						   AND IRM.PlantId='" + identity.PlantId + @"'
                           )x 
                            Group by Id ,x.PreparedBy,x.AddedDate ,Orderspecific,x.ProcessName 
							,x.SalesOrderId,x.ProductionOrderId,x.BuyerItemReferenceNo,x.OwnItemReferenceNo,x.BuyerOrderReferenceNo,x.OwnOrderReferenceNo,x.CustomerName,x.BUyerName
							) y
							where y.Balance>0";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}

		public IEnumerable<object> GetAssetIssueSlip()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.EmployeeName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,IRM.CheckedBy
                                ,IRM.CheckedByStatus
                                ,IRM.AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                             
                           Where IRM.CheckedBy IS NOT NULL 
						   AND IRM.CheckedByStatus='Checked' 
						   AND IRM.AuthorizedByStatus='Approved' 
						   AND IRM.AuthorizedBy IS NOT null  
						   AND IRM.IssueSlipType='AssetSlip'
						   AND IRM.PlantId='" + identity.PlantId + @"'
                           --Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='Checked' OR IRM.CheckedByStatus='Approval'AND IRM.AuthorizedByStatus IS Not NULL  AND IRM.AuthorizedBy IS null OR IRM.AuthorizedBy IS NOT null And IRM.PreparedBy='" + identity.EmployeeId + @"'
                           --Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='ForChecked' AND IRM.AuthorizedByStatus IS NULL AND IRM.IssueSlipType='AssetSlip' AND IRM.AuthorizedBy IS null --And IRM.PreparedBy='" + identity.EmployeeId + @"'
                           )x 
                            Group by Id ,x.PreparedBy,x.AddedDate                             
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

		public IEnumerable<object> GetApprovedIssueSlipDetails(string Id, string StorageLocationId, string OrderSpecific)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			var sql = "";
			try
			{
				sql = @"SELECT MGM.UserName MaterialMasterGroupName
                                	,IR.MaterialMasterId ,mm.UserName Material ,IR.ArticleId ,ART.StandardName ArticleName
                                	,MT.UserName MaterialType ,IR.FirstCharacteristicsId ,FC.UserName AS FirstCharacteristics ,IR.FirstCharacteristicsValueId
                                	,FCV.UserName AS Sku1 ,IR.SecondCharacteristicsId ,SC.UserName AS SecondCharacteristics ,IR.SecondCharacteristicsValueId
                                	,SCV.UserName AS Sku2 ,IR.ThirdCharacteristicsId ,TC.UserName AS ThirdCharacteristics ,IR.ThirdCharacteristicsValueId
                                	,TCV.UserName AS Sku3 ,C.UserName CountryName ,C.Id CountryId ,TUoM.Id BaseUOMId ,TUoM.Id TransactionUoMId ,TUoM.UserName UOM
                                	,TUoM.UserName TransactionUoM ,IR.CostCenterId ,CC.UserName AS CostCenterName ,IR.GLGeneralInfoId ,IGL1.UserName GLName
                                	,IR.BudgetMasterId ,B1.UserName BudgetName ,IR.ExpenseActivityId ,IA1.UserName ActivityName
                                	,IRM.Id IssueRequestMasterId ,IR.Id IssueRequest,MM.IsAsset
                                	,Convert(BIT, 0) 'check' ,IR.RequestedQty RequestedQty ,sum(ABC.Qty) RequestIssuedQty,sum(IDRM.Qty) IssuedQty
                                	,Sum(Isnull(PostingQty.PostingQty, 0)) PostingQty
                                	,BalanceQty = Isnull(IR.RequestedQty, 0)   - SUM(ISNULL(ABC.Qty, 0))
                                    ,TempBalanceQty = Isnull(IR.RequestedQty, 0)  - SUM(ISNULL(ABC.Qty, 0))
                                	,BaseUOMFactor = CASE  WHEN AlternativeUOM.BaseUOMFactor IS NULL THEN 1 ELSE AlternativeUOM.BaseUOMFactor END
                                FROM trn.IssueRequest IR
                                LEFT JOIN TRN.IssueRequestMaster IRM ON IRM.Id = IR.IssueRequestMasterId
                                LEFT JOIN MST.MaterialMaster AS MM ON IR.MaterialMasterId = MM.Id
                                LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                                LEFT JOIN MST.MaterialMasterArticle AS ART ON IR.ArticleId = ART.Id
                                LEFT JOIN HKP.Characteristics AS FC ON IR.FirstCharacteristicsId = FC.Id
                                LEFT JOIN HKP.Characteristics AS SC ON IR.SecondCharacteristicsId = SC.Id
                                LEFT JOIN HKP.Characteristics AS TC ON IR.ThirdCharacteristicsId = TC.Id
                                LEFT JOIN HKP.CharacteristicsValue AS FCV ON IR.FirstCharacteristicsValueId = FCV.Id
                                LEFT JOIN HKP.CharacteristicsValue AS SCV ON IR.SecondCharacteristicsValueId = SCV.Id
                                LEFT JOIN HKP.CharacteristicsValue AS TCV ON IR.ThirdCharacteristicsValueId = TCV.Id
                                LEFT JOIN [SCS].[Country] AS C ON C.Id = IR.CountryId
                                LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON Ir.TransactionUoMId = TUoM.Id
                                LEFT JOIN [SEC].[User] AS Us ON IR.AddedBy = Us.UserId
                                LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                                LEFT JOIN [ORG].[CostCenter] CC ON CC.Id = IR.CostCenterId
                                LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id = IR.GLGeneralInfoId
                                LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id = IR.BudgetMasterId
                                LEFT JOIN hkp.Budget B1 ON B1.Id = IBM1.BudgetId
                                LEFT JOIN HKP.Activity IA1 ON IA1.Id = IR.ExpenseActivityId
                                LEFT JOIN [MST].[MaterialMasterAlternativeUOM] AlternativeUOM ON AlternativeUOM.AlternativeUOMId = IR.TransactionUoMId
                                	AND AlternativeUOM.MaterialMasterId = mm.Id
                                LEFT JOIN (
                                	SELECT IRD.InventoryMaterialId
                                		,TUoM.Id UoM
                                		,0 TotalQty
                                		,PostingQty = (((SUM(ISNULL(IRD.BaseQty, 0)) - SUM(ISNULL(II.IssueQty, 0)) - SUM(ISNULL(IRD.PurchaseReturnQty, 0))) + SUM(ISNULL(IRD.IssueReturnQty, 0)) - SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0)) - SUM(ISNULL(IRD.InventorySalesQty, 0)) - SUM(ISNULL(IRD.InventoryScrapQty, 0))))
                                		,0 ApprovedQty
                                		,0 UnApprovedQty
                                		,IM.MaterialMasterId
                                		,IM.ArticleId
                                		,IM.FirstCharacteristicsValueId
                                		,IM.SecondCharacteristicsValueId
                                		,IM.ThirdCharacteristicsValueId
                                		--,IRD.MaterialStorageId
                                		,IM.PlantId
                                	FROM [TRN].[InventoryReceiveDetail] AS IRD
                                	JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId = IM.Id
                                	JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id
                                	LEFT JOIN [HKP].[Party] AS P ON IR.PartyId = P.Id
                                	JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId = TCU.Id
                                	JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId = BCU.Id
                                	JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                                	LEFT JOIN (
                                		SELECT IID.InventoryMaterialId
                                			,IH.InventoryReceiveDetailId
                                			--,II.MaterialStorageId
                                			,Sum(ISNULL(IH.Qty, 0)) IssueQty
                                			,Sum(ISNULL(IH.TotalMaterialBooksCurrencyAmount, 0)) IssueAmount
                                			,IID.IsAsset
                                		FROM TRN.InventoryIssueDetail IID
                                		LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId = II.Id
                                		LEFT JOIN TRN.InventoryIssueHistory IH ON IH.InventoryIssueDetailId = IID.Id
                                		WHERE II.PlantId = '" + identity.PlantId + @"'
                                		GROUP BY IID.InventoryMaterialId
                                			,IID.IsAsset
                                			,IH.InventoryReceiveDetailId
                                			--,II.MaterialStorageId
                                		) II ON II.InventoryReceiveDetailId = IRD.Id
                                		--AND II.MaterialStorageId = IRD.MaterialStorageId
                                	WHERE IM.CompanyGroupId = '" + identity.CompanyGroupId + @"'
                                		AND IM.CompanyId = '" + identity.CompanyId + @"'
                                		AND IM.PlantId = '" + identity.PlantId + @"'
                                		--AND IRD.MaterialStorageId = '" + StorageLocationId + @"'
                                		AND IR.[Status] = 'Posting'
                                	GROUP BY IRD.InventoryMaterialId
                                		,IM.MaterialMasterId
                                		,IM.ArticleId
                                		,IM.FirstCharacteristicsValueId
                                		,IM.SecondCharacteristicsValueId
                                		,IM.ThirdCharacteristicsValueId
                                		--,IRD.MaterialStorageId
                                		,TUoM.Id
                                		,IM.PlantId
                                	) PostingQty ON PostingQty.InventoryMaterialId = IR.InventoryMaterialId
                                LEFT JOIN (
                                	SELECT isnull(sum(c.Qty), 0) Qty
                                		,c.IssueRequestDetailId
                                	FROM trn.InventoryIssue a
                                	LEFT JOIN trn.InventoryIssueDetail b ON b.InventoryIssueId = a.id
                                	LEFT JOIN trn.InventoryIssueHistory c ON c.InventoryIssueDetailId = b.Id
                                	LEFT JOIN trn.IssueRequest IR ON IR.Id = c.IssueRequestDetailId
                                	LEFT JOIN TRN.IssueRequestMaster IRM ON IRM.Id = IR.IssueRequestMasterId				
                                	GROUP BY c.IssueRequestDetailId
                                	) ABC ON ABC.IssueRequestDetailId = IR.Id
                                LEFT JOIN (
                                	SELECT aa.Id
                                		,sum(cc.Qty) Qty
                                	FROM trn.IssueRequest aa
                                	LEFT JOIN trn.IssueRequestMaster dd ON dd.id = aa.IssueRequestMasterId
                                	LEFT JOIN [TRN].[IssueRequestBOQMap] bb ON bb.IssueRequestDetailId = aa.id
                                	LEFT JOIN [TRN].[IssueDetailAndIssueRequestMap] cc ON cc.IssueRequestBOQMapId = bb.Id
                                	WHERE cc.IssueRequestBOQMapId IS NOT NULL --and  dd.Id='2150'
                                	GROUP BY aa.Id
                                	) IDRM ON IDRM.Id = IR.id
                                WHERE IRM.Id = '" + Id + @"'
                                GROUP BY MGM.UserName ,IR.MaterialMasterId ,IR.RequestedQty ,mm.UserName ,IR.ArticleId ,ART.StandardName
                                	,MT.UserName ,IR.FirstCharacteristicsId ,FC.UserName ,IR.FirstCharacteristicsValueId ,FCV.UserName ,IR.SecondCharacteristicsId
                                	,SC.UserName ,IR.SecondCharacteristicsValueId ,SCV.UserName ,IR.ThirdCharacteristicsId ,TC.UserName ,IR.ThirdCharacteristicsValueId
                                	,TCV.UserName ,C.UserName ,C.Id ,TUoM.Id ,TUoM.Id ,TUoM.UserName ,IR.CostCenterId ,CC.UserName ,IR.GLGeneralInfoId
                                	,IGL1.UserName ,IR.BudgetMasterId ,B1.UserName ,IR.ExpenseActivityId ,IA1.UserName ,IRM.Id ,IR.Id,MM.IsAsset
                                	,AlternativeUOM.BaseUOMFactor";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}

		public IEnumerable<object> GetApprovedIssueSlipBOQDetails(string Id, string StorageLocationId, string OrderSpecific)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			var sql = "";
			try
			{
				if (OrderSpecific == "Yes")
				{
					sql = @"Select 
	                MGM.UserName MaterialMasterGroupName
                    ,IR.MaterialMasterId
	                ,mm.UserName Material
	                ,IR.ArticleId
	                ,ART.StandardName ArticleName
	                ,MT.UserName MaterialType
	                ,IR.FirstCharacteristicsId
	                ,FC.UserName AS FirstCharacteristics
	                ,IR.FirstCharacteristicsValueId
	                ,FCV.UserName AS Sku1
	                ,IR.SecondCharacteristicsId
	                ,SC.UserName AS SecondCharacteristics
	                ,IR.SecondCharacteristicsValueId
	                ,SCV.UserName AS Sku2
	                ,IR.ThirdCharacteristicsId
	                ,TC.UserName AS ThirdCharacteristics
	                ,IR.ThirdCharacteristicsValueId
	                ,TCV.UserName AS Sku3,C.UserName CountryName,C.Id CountryId
							
	                ,IR.CostCenterId
	                ,CC.UserName AS CostCenterName
	                ,IR.GLGeneralInfoId 
	                ,IGL1.UserName GLName									
	                ,IR.BudgetMasterId									
	                ,B1.UserName BudgetName
	                ,IR.ExpenseActivityId
	                ,IA1.UserName ActivityName	
	                ,IRM.Id IssueRequestMasterId
	                ,IR.Id IssueRequest,Convert(bit,0)  'check'	
	                ,bd.SalesOrderId
	                ,TUoM.Id BaseUOMId,GRNALLO.BaseUoM
	                ,TUoM.Id TransactionUoMId							
	                ,TUoM.UserName TransactionUoM,GRNALLO.MaterialStorageId	
	                ,MM.IssueByUoM
                    ,0 TrasactopmUomQty
	                ,'' IssueTransactionUoMId
	                ,'' IssueTransactionUoM
	                ,Isnull(IR.RequestedQty,0) RequestedQty
	                ,Isnull(IDRM.Qty,0) IssuedQty
	                ,sum((ISNULL(GRNALLO.TransactionQty,0)*GRNALLO.BaseUoMFactor)/CASE WHEN AlternativeUOM.BaseUOMFactor is NULL then 1 else  AlternativeUOM.BaseUOMFactor end) PostingQty
	                ,BalanceQty=Isnull(IR.RequestedQty,0)-ISNULL(IDRM.Qty,0)
					,BaseUOMFactor= CASE WHEN AlternativeUOM.BaseUOMFactor is NULL then 1 else  AlternativeUOM.BaseUOMFactor end
                FROM trn.IssueRequest IR									
                LEFT JOIN TRN.IssueRequestMaster IRM ON IRM.Id=IR.IssueRequestMasterId                                    
                Left JOIN MST.MaterialMaster AS MM ON IR.MaterialMasterId = MM.Id
                LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                LEFT JOIN MST.MaterialMasterArticle AS ART ON IR.ArticleId = ART.Id
                LEFT JOIN HKP.Characteristics AS FC ON IR.FirstCharacteristicsId = FC.Id
                LEFT JOIN HKP.Characteristics AS SC ON IR.SecondCharacteristicsId = SC.Id
                LEFT JOIN HKP.Characteristics AS TC ON IR.ThirdCharacteristicsId = TC.Id
                LEFT JOIN HKP.CharacteristicsValue AS FCV ON IR.FirstCharacteristicsValueId = FCV.Id
                LEFT JOIN HKP.CharacteristicsValue AS SCV ON IR.SecondCharacteristicsValueId = SCV.Id
                LEFT JOIN HKP.CharacteristicsValue AS TCV ON IR.ThirdCharacteristicsValueId = TCV.Id
                LEFT JOIN [SCS].[Country] AS C ON C.Id = IR.CountryId
                LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON Ir.TransactionUoMId = TUoM.Id
                LEFT JOIN [SEC].[User] As Us On IR.AddedBy=Us.UserId                                   
                LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id

                Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IR.GLGeneralInfoId 
                LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IR.BudgetMasterId
                Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
                LEFT JOIN HKP.Activity IA1 ON IA1.Id=IR.ExpenseActivityId
							
                LEFT JOIN(
		                SELECT isnull(sum(c.Qty),0) Qty ,c.IssueRequestDetailId	 
		                from trn.InventoryIssue  a          
		                LEFT JOIN trn.InventoryIssueDetail b ON b.InventoryIssueId=a.id
		                Left JOIN  trn.InventoryIssueHistory c On c.InventoryIssueDetailId=b.Id 
		                Left JOIN trn.IssueRequest IR	ON IR.Id=c.IssueRequestDetailId	
		                LEFT JOIN TRN.IssueRequestMaster IRM ON IRM.Id=IR.IssueRequestMasterId   
		                --where IRM.Id='2067'				
		                GROUp BY c.IssueRequestDetailId
		                )ABC ON ABC.IssueRequestDetailId=IR.Id
                Left Join (select aa.Id, sum(cc.Qty) Qty  
		                from trn.IssueRequest aa
		                left join trn.IssueRequestMaster dd on dd.id=aa.IssueRequestMasterId
		                left join [TRN].[IssueRequestBOQMap] bb on bb.IssueRequestDetailId=aa.id
		                left join  [TRN].[IssueDetailAndIssueRequestMap] cc on cc.IssueRequestBOQMapId=bb.Id 
		                where cc.IssueRequestBOQMapId is not null --and  dd.Id='2150'
		                group by aa.Id
                ) IDRM ON IDRM.Id=IR.id
                Left Join [TRN].[IssueRequestBOQMap] IRBOQMAP ON IRBOQMAP.IssueRequestDetailId=IR.Id
                left join BOQDetail bd on bd.id=IRBOQMAP.BOQID
                left join (
		                select a.Id,  IRD.MaterialStorageId,a.SalesOrderId
		                ,b.BOQDetailId,sum(a.TransactionQty) TransactionQty 
		                ,sum(a.BaseQty) BaseQty 
		                ,UOM.UserName,a.POBOQMapId,IRBM.IssueRequestDetailId
		                ,UOM.Id StockTransactionUoMId,a.BaseUoMId,IRD.BaseUoMFactor,UOM.UserName BaseUoM
		                from trn.GRNPORequisitionAllocation a
		                left Join trn.InventoryReceiveDetail IRD ON IRD.Id=a.InventoryReceiveDetailId
		                left join trn.POBOQMap b ON b.Id=a.POBOQMapId 
		                left join [TRN].[IssueRequestBOQMap] IRBM ON IRBM.BOQID=b.BOQDetailId
		                LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=a.BaseUoMId
		                where IRBM.IssueRequestDetailId<>'' --and a.SalesOrderId='212160301' --and b.BOQDetailId='21223-25'
		                group by a.Id, IRBM.IssueRequestDetailId,a.POBOQMapId,b.BOQDetailId
		                ,UOM.UserName,UOM.Id,a.SalesOrderId,a.BaseUoMId,IRD.MaterialStorageId
		                ,IRD.BaseUoMFactor
	                ) GRNALLO ON GRNALLO.BOQDetailId=IRBOQMAP.BOQID 
                    left join [MST].[MaterialMasterAlternativeUOM] AlternativeUOM ON AlternativeUOM.AlternativeUOMId=IR.TransactionUoMId And AlternativeUOM.MaterialMasterId=mm.Id

                Where IRM.Id='" + Id + @"'  and IR.Id=GRNALLO.IssueRequestDetailId
                Group BY
                MGM.UserName 
                ,IR.MaterialMasterId
                ,mm.UserName 
                ,IR.ArticleId
                ,ART.StandardName 
                ,MT.UserName 
                ,IR.FirstCharacteristicsId
                ,FC.UserName 
                ,IR.FirstCharacteristicsValueId
                ,FCV.UserName 
                ,IR.SecondCharacteristicsId
                ,SC.UserName 
                ,IR.SecondCharacteristicsValueId
                ,SCV.UserName 
                ,IR.ThirdCharacteristicsId
                ,TC.UserName 
                ,IR.ThirdCharacteristicsValueId
                ,TCV.UserName ,C.UserName ,C.Id 
							
                ,IR.CostCenterId
                ,CC.UserName 
                ,IR.GLGeneralInfoId 
                ,IGL1.UserName 									
                ,IR.BudgetMasterId									
                ,B1.UserName 
                ,IR.ExpenseActivityId
                ,IA1.UserName 	
                ,IRM.Id 
                ,IR.Id 	
                ,bd.SalesOrderId
                ,TUoM.Id 
                ,TUoM.Id 							
                ,TUoM.UserName 							
                ,MM.IssueByUoM,IR.RequestedQty     
                ,Isnull(IR.RequestedQty,0) 
                ,Isnull(IDRM.Qty,0) 
                ,IDRM.Qty,GRNALLO.MaterialStorageId	,AlternativeUOM.BaseUOMFactor,GRNALLO.BaseUoM
                Order by mm.UserName ASC";
				}
				else
				{

					sql = @"SELECT MGM.UserName MaterialMasterGroupName
                                	,IR.MaterialMasterId ,mm.UserName Material ,IR.ArticleId ,ART.StandardName ArticleName
                                	,MT.UserName MaterialType ,IR.FirstCharacteristicsId ,FC.UserName AS FirstCharacteristics ,IR.FirstCharacteristicsValueId
                                	,FCV.UserName AS Sku1 ,IR.SecondCharacteristicsId ,SC.UserName AS SecondCharacteristics ,IR.SecondCharacteristicsValueId
                                	,SCV.UserName AS Sku2 ,IR.ThirdCharacteristicsId ,TC.UserName AS ThirdCharacteristics ,IR.ThirdCharacteristicsValueId
                                	,TCV.UserName AS Sku3 ,C.UserName CountryName ,C.Id CountryId ,TUoM.Id BaseUOMId ,TUoM.Id TransactionUoMId ,TUoM.UserName UOM
                                	,TUoM.UserName TransactionUoM ,IR.CostCenterId ,CC.UserName AS CostCenterName ,IR.GLGeneralInfoId ,IGL1.UserName GLName
                                	,IR.BudgetMasterId ,B1.UserName BudgetName ,IR.ExpenseActivityId ,IA1.UserName ActivityName
                                	,IRM.Id IssueRequestMasterId ,IR.Id IssueRequest,MM.IsAsset
                                	,Convert(BIT, 0) 'check' ,IR.RequestedQty RequestedQty ,sum(IDRM.Qty) IssuedQty
                                	,Sum(Isnull(PostingQty.PostingQty, 0)) PostingQty
                                	,BalanceQty = Isnull(IR.RequestedQty, 0) - SUM(ISNULL(IDRM.Qty, 0))
                                    ,TempBalanceQty = Isnull(IR.RequestedQty, 0) - SUM(ISNULL(IDRM.Qty, 0))
                                	,BaseUOMFactor = CASE  WHEN AlternativeUOM.BaseUOMFactor IS NULL THEN 1 ELSE AlternativeUOM.BaseUOMFactor END
                                FROM trn.IssueRequest IR
                                LEFT JOIN TRN.IssueRequestMaster IRM ON IRM.Id = IR.IssueRequestMasterId
                                LEFT JOIN MST.MaterialMaster AS MM ON IR.MaterialMasterId = MM.Id
                                LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                                LEFT JOIN MST.MaterialMasterArticle AS ART ON IR.ArticleId = ART.Id
                                LEFT JOIN HKP.Characteristics AS FC ON IR.FirstCharacteristicsId = FC.Id
                                LEFT JOIN HKP.Characteristics AS SC ON IR.SecondCharacteristicsId = SC.Id
                                LEFT JOIN HKP.Characteristics AS TC ON IR.ThirdCharacteristicsId = TC.Id
                                LEFT JOIN HKP.CharacteristicsValue AS FCV ON IR.FirstCharacteristicsValueId = FCV.Id
                                LEFT JOIN HKP.CharacteristicsValue AS SCV ON IR.SecondCharacteristicsValueId = SCV.Id
                                LEFT JOIN HKP.CharacteristicsValue AS TCV ON IR.ThirdCharacteristicsValueId = TCV.Id
                                LEFT JOIN [SCS].[Country] AS C ON C.Id = IR.CountryId
                                LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON Ir.TransactionUoMId = TUoM.Id
                                LEFT JOIN [SEC].[User] AS Us ON IR.AddedBy = Us.UserId
                                LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                                LEFT JOIN [ORG].[CostCenter] CC ON CC.Id = IR.CostCenterId
                                LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id = IR.GLGeneralInfoId
                                LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id = IR.BudgetMasterId
                                LEFT JOIN hkp.Budget B1 ON B1.Id = IBM1.BudgetId
                                LEFT JOIN HKP.Activity IA1 ON IA1.Id = IR.ExpenseActivityId
                                LEFT JOIN [MST].[MaterialMasterAlternativeUOM] AlternativeUOM ON AlternativeUOM.AlternativeUOMId = IR.TransactionUoMId
                                	AND AlternativeUOM.MaterialMasterId = mm.Id
                                LEFT JOIN (
                                	SELECT IRD.InventoryMaterialId
                                		,TUoM.Id UoM
                                		,0 TotalQty
                                		,PostingQty = (((SUM(ISNULL(IRD.BaseQty, 0)) - SUM(ISNULL(II.IssueQty, 0)) - SUM(ISNULL(IRD.PurchaseReturnQty, 0))) + SUM(ISNULL(IRD.IssueReturnQty, 0)) - SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0)) - SUM(ISNULL(IRD.InventorySalesQty, 0)) - SUM(ISNULL(IRD.InventoryScrapQty, 0))))
                                		,0 ApprovedQty
                                		,0 UnApprovedQty
                                		,IM.MaterialMasterId
                                		,IM.ArticleId
                                		,IM.FirstCharacteristicsValueId
                                		,IM.SecondCharacteristicsValueId
                                		,IM.ThirdCharacteristicsValueId
                                		--,IRD.MaterialStorageId
                                		,IM.PlantId
                                	FROM [TRN].[InventoryReceiveDetail] AS IRD
                                	JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId = IM.Id
                                	JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id
                                	LEFT JOIN [HKP].[Party] AS P ON IR.PartyId = P.Id
                                	JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId = TCU.Id
                                	JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId = BCU.Id
                                	JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                                	LEFT JOIN (
                                		SELECT IID.InventoryMaterialId
                                			,IH.InventoryReceiveDetailId
                                			--,II.MaterialStorageId
                                			,Sum(ISNULL(IH.Qty, 0)) IssueQty
                                			,Sum(ISNULL(IH.TotalMaterialBooksCurrencyAmount, 0)) IssueAmount
                                			,IID.IsAsset
                                		FROM TRN.InventoryIssueDetail IID
                                		LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId = II.Id
                                		LEFT JOIN TRN.InventoryIssueHistory IH ON IH.InventoryIssueDetailId = IID.Id
                                		WHERE II.PlantId = '" + identity.PlantId + @"'
                                		GROUP BY IID.InventoryMaterialId
                                			,IID.IsAsset
                                			,IH.InventoryReceiveDetailId
                                			--,II.MaterialStorageId
                                		) II ON II.InventoryReceiveDetailId = IRD.Id
                                		--AND II.MaterialStorageId = IRD.MaterialStorageId
                                	WHERE IM.CompanyGroupId = '" + identity.CompanyGroupId + @"'
                                		AND IM.CompanyId = '" + identity.CompanyId + @"'
                                		AND IM.PlantId = '" + identity.PlantId + @"'
                                		--AND IRD.MaterialStorageId = '" + StorageLocationId + @"'
                                		AND IR.[Status] = 'Posting'
                                	GROUP BY IRD.InventoryMaterialId
                                		,IM.MaterialMasterId
                                		,IM.ArticleId
                                		,IM.FirstCharacteristicsValueId
                                		,IM.SecondCharacteristicsValueId
                                		,IM.ThirdCharacteristicsValueId
                                		--,IRD.MaterialStorageId
                                		,TUoM.Id
                                		,IM.PlantId
                                	) PostingQty ON PostingQty.InventoryMaterialId = IR.InventoryMaterialId
                                LEFT JOIN (
                                	SELECT isnull(sum(c.Qty), 0) Qty
                                		,c.IssueRequestDetailId
                                	FROM trn.InventoryIssue a
                                	LEFT JOIN trn.InventoryIssueDetail b ON b.InventoryIssueId = a.id
                                	LEFT JOIN trn.InventoryIssueHistory c ON c.InventoryIssueDetailId = b.Id
                                	LEFT JOIN trn.IssueRequest IR ON IR.Id = c.IssueRequestDetailId
                                	LEFT JOIN TRN.IssueRequestMaster IRM ON IRM.Id = IR.IssueRequestMasterId				
                                	GROUP BY c.IssueRequestDetailId
                                	) ABC ON ABC.IssueRequestDetailId = IR.Id
                                LEFT JOIN (
                                	SELECT aa.Id
                                		,sum(cc.Qty) Qty
                                	FROM trn.IssueRequest aa
                                	LEFT JOIN trn.IssueRequestMaster dd ON dd.id = aa.IssueRequestMasterId
                                	LEFT JOIN [TRN].[IssueRequestBOQMap] bb ON bb.IssueRequestDetailId = aa.id
                                	LEFT JOIN [TRN].[IssueDetailAndIssueRequestMap] cc ON cc.IssueRequestBOQMapId = bb.Id
                                	WHERE cc.IssueRequestBOQMapId IS NOT NULL --and  dd.Id='2150'
                                	GROUP BY aa.Id
                                	) IDRM ON IDRM.Id = IR.id
                                WHERE IRM.Id = '" + Id + @"'
                                GROUP BY MGM.UserName ,IR.MaterialMasterId ,IR.RequestedQty ,mm.UserName ,IR.ArticleId ,ART.StandardName
                                	,MT.UserName ,IR.FirstCharacteristicsId ,FC.UserName ,IR.FirstCharacteristicsValueId ,FCV.UserName ,IR.SecondCharacteristicsId
                                	,SC.UserName ,IR.SecondCharacteristicsValueId ,SCV.UserName ,IR.ThirdCharacteristicsId ,TC.UserName ,IR.ThirdCharacteristicsValueId
                                	,TCV.UserName ,C.UserName ,C.Id ,TUoM.Id ,TUoM.Id ,TUoM.UserName ,IR.CostCenterId ,CC.UserName ,IR.GLGeneralInfoId
                                	,IGL1.UserName ,IR.BudgetMasterId ,B1.UserName ,IR.ExpenseActivityId ,IA1.UserName ,IRM.Id ,IR.Id,MM.IsAsset
                                	,AlternativeUOM.BaseUOMFactor";
				}
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}


		public GridModel GetDeletableIssueList(GridParameter parameters, string plantId)
		{
			try
			{
				parameters.CmdText = @"SELECT E.UserName AS Entity ,II.IssueType, II.Id,V.VoucherNo,II.VoucherId, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,FORMAT(II.IssueDate,'dd-MMM-yyyy') IssueDate, MS.UserName AS MaterialStorage
									 ,EI.EmployeeCode+' - '+EI.EmployeeName EmployeeName,SUM(IID.TransactionQty) Qty,SUM(IID.PolicyAmount) Amount,II.Remarks,II.Id AS IssueId
                                FROM [TRN].[InventoryIssue] AS II
                                JOIN TRN.InventoryIssueDetail AS IID ON IID.InventoryIssueId=II.Id AND IID.IsAsset=0
                                JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId=MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId=II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id=II.EntityId
								Left JOIN TRN.Voucher V on V.Id=II.VoucherId
                                WHERE II.PlantId='" + plantId + @"' 
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,II.IssueDate, MS.UserName
									 ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,V.VoucherNo,II.VoucherId";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}
		public IEnumerable<object> GetAssetIssueSlipWithGRN(string plantId, string materialStorageId)
		{
			try
			{
				var sql = @"SELECT IR.Id GRNNo,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END, IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName,
                                     FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate, P.Code AS PartyCode, P.UserName AS PartyName, UoM.UserName AS TransactionUoM,CU.Code AS CurrencyCode
                                     , IR.IsNonCreditable, FORMAT(IR.MatureDate,'dd-MMM-yyyy') MatureDate, FORMAT(IR.EntryDate,'dd-MMM-yyyy') EntryDate, FORMAT(IR.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate, IR.DeliveryByAddress
                                     , DPP.UserName AS DeliveryBy, CP.UserName AS PartyAccountGroupName, PT.UserName AS PaymentTermName, IR.GateEntryNo, IR.InvoicingByAddress, IPP.UserName AS InvoicingBy,IR.ToCurrencyRate
                                     , MGM.UserName AS MaterialGroupMasterName
									 ,IR.POId, IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId
									  ,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty,IRD.BaseUOMId,IRD.TransactionUoMId
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty
						 ,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,ISNULL(IRD.BooksCurrencyBaseRate,0) BooksCurrencyBaseRate
						 ,ISNULL(IRD.TrnCurrencyBaseRate,0) TrnCurrencyBaseRate
						 , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, CU.Code AS TCurrency, IRD.MaterialTranAmount
                       , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
                           , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
							,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						    ,GL.UserName GLName,GL.Id GLGeneralInfoId,IRD.PostDrBudgetMasterId BudgetMasterId,B.UserName BudgetName,IRD.PostDrActivityId ActivityId,A.UserName ActivityName
							, IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                            ,IReq.Id IssueRequestDatailId,IReq.IssueRequestMasterId
									 FROM TRN.InventoryReceiveDetail IRD 
                                     LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
                                     LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                                     LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
								LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
							    JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							    LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							    LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							    LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							    LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                     LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IRD.TransactionUoMId=UoM.Id
									 LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
                                     JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                     LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                                      LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                                     			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                     JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                                     LEFT JOIN [TRN].[IssueRequest] AS IReq ON MM.Id=IReq.MaterialMasterId 
                                        AND IReq.ArticleId=ART.Id 
                                     LEFT JOIN [TRN].[IssueRequestMaster] AS IRM ON IRM.Id =IReq.IssueRequestMasterId 
                                     LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                                     WHERE IRD.IsAsset=1 AND IRD.CapitalizeVoucherDetailId IS NULL AND IR.PlantId='" + plantId + @"' 
                                    AND IRM.IssueSlipType='AssetSlip' 
                                    AND (ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0))>0 AND IRD.MaterialStorageId='" + materialStorageId + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}
		public IEnumerable<object> GetGRNFixedAssetList(string plantId, string materialStorageId, string issueDate)
		{
			try
			{
				var sql = @"SELECT IR.Id GRNNo,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END, IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName,
                                     FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate, P.Code AS PartyCode, P.UserName AS PartyName, UoM.UserName AS TransactionUoM,CU.Code AS CurrencyCode
                                     , IR.IsNonCreditable, FORMAT(IR.MatureDate,'dd-MMM-yyyy') MatureDate, FORMAT(IR.EntryDate,'dd-MMM-yyyy') EntryDate, FORMAT(IR.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate, IR.DeliveryByAddress
                                     , DPP.UserName AS DeliveryBy, CP.UserName AS PartyAccountGroupName, PT.UserName AS PaymentTermName, IR.GateEntryNo, IR.InvoicingByAddress, IPP.UserName AS InvoicingBy,IR.ToCurrencyRate
                                     , MGM.UserName AS MaterialGroupMasterName,IRD.BaseUoMFactor
									 ,IR.POId, IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId
									  ,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty,IRD.BaseUOMId,IRD.TransactionUoMId
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						,ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty, ISNULL(IRD.PurchaseReturnQty,0) PurchaseReturnQty,ISNULL(IRD.IssueReturnQty,0) IssueReturnQty,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty,ISNULL(IRD.InventorySalesQty,0) InventorySalesQty,ISNULL(IRD.InventoryScrapQty,0) InventoryScrapQty,Isnull(InventoryTransferQty,0) InventoryTransferQty
						 ,((((((ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0)-ISNULL(IRD.PurchaseReturnQty,0))+ISNULL(IRD.IssueReturnQty,0))-ISNULL(IRD.ReductionByAdjustmentQty,0))-ISNULL(IRD.InventorySalesQty,0))-ISNULL(IRD.InventoryScrapQty,0))-Isnull(InventoryTransferQty,0)) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,ISNULL(IRD.BooksCurrencyBaseRate,0) BooksCurrencyBaseRate
						 ,ISNULL(IRD.TrnCurrencyBaseRate,0) TrnCurrencyBaseRate
						 , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, CU.Code AS TCurrency, IRD.MaterialTranAmount,IR.ToCurrencyRate
                       , BaseRate=TrnCurrencyBaseRate
                           , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
							,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						    ,GL.UserName GLName,GL.Id GLGeneralInfoId,IRD.PostDrBudgetMasterId BudgetMasterId,B.UserName BudgetName,IRD.PostDrActivityId ActivityId,A.UserName ActivityName
							, IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue, isnull(C.UserName,'') CountryName, C.Id CountryId
							, ISNULL(PO1.PoId,'') PurchaseOrderId,REPLACE(CONVERT(CHAR(11), PO1.PODate, 106),' ','-')  PurchaseOrderDate,ISNULL(PLC.LCRef,'') LCRef,ISNULL(BMTbl.AccountTitle,'') AccountTitle,ISNULL(PDA.AcceptanceNo,'') AcceptanceNo,PDA.AcceptanceDate,PO.DocRefNo PODocRefNo,PO.PODate,IR.DocRefNo GRNDocRefNo,IR.GRNDate
								FROM TRN.InventoryReceiveDetail IRD 
								LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
								LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
								LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
								LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
							    JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							    LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							    LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							    LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							    LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IRD.TransactionUoMId=UoM.Id
								LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
								LEFT JOIN [MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
								LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
								LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
                                JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                                LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                                     			            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                                LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                                left JOIN SCS.Country C On C.Id=IM.countryId
								LEFT JOIN(
										select PDAMAP.GRNId, REPLACE(Convert(VARCHAR(11), IR.PODate, 106), ' ', '-') AS PODate
										,PoId=STUFF((select distinct ','+xpo.Id from
										trn.PurchaseOrder xpo
										INNER JOin TRN.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.PoId
										where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

										from TRN.POGGRNMap PDAMAP
										LEFT JOIN [TRN].PurchaseOrder IR ON IR.Id = PDAMAP.PoId
										
										group by PDAMAP.GRNId, IR.podate
										)PO1 ON PO1.GRNId = IRD.InventoryReceiveId
								 --LEFT JOIN [TRN].[GateEntry] GTE ON GTE.ID= IR.GateEntryNo

								Left Join TRN.PurchaseOrder PO ON PO.Id=IRD.POId
								Left join dbo.PurchaseLC PLC ON PLC.Id=PO.PurchaseLCId
								LEFT JOIN [MST].[BankMaster] BMTbl ON BMTbl.Id=PLC.OpeningBankMasterId
								Left Join TRN.GRNAcceptanceMap APOMap ON APOMap.GRNId=IR.Id
								left JOIN TRN.PurchaseDocAcceptance PDA ON PDA.Id=APOMap.PurchaseDocumentAcceptanceId
                                     WHERE IRD.IsAsset=1 AND IRD.CapitalizeVoucherDetailId IS NULL  AND IR.VoucherId<>'' AND IR.PlantId='" + plantId + @"' 
                                    --AND (ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0))>0 
									AND ((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)-isnull(IRD.PurchaseReturnQty,0)-isnull(IRD.ReductionByAdjustmentQty,0)-isnull(IRD.InventorySalesQty,0)-isnull(IRD.InventoryScrapQty,0))+isnull(IRD.IssueReturnQty,0))>0
								    AND IRD.MaterialStorageId='" + materialStorageId + "'  AND IR.GRNDate<= '" + issueDate + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}
		public IEnumerable<object> GetDataByInventoryIssue(string plantId)
		{
			try
			{

				var sql = @"SELECT * FROM (
                            SELECT II.Id,II.IssueDate IssueDate1,E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							,  II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.IssueDate, 'dd-MMM-yyyy') IssueDate
							
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,IIH.Qty
							,IIh.TotalAmount Amount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							,IIH.CountryId,IIH.CountryName
							,II.ContractId,II.ProductionOrderId,Con.ContractNo
                            ,IsNULL(V.VoucherNo,'') VoucherNo ,IsPark=case when II.VoucherId<>'' then 0 else 1 end
							FROM [TRN].[InventoryIssue] AS II
							left join (
									SELECT IID.InventoryIssueId,IID.IsAsset,C.UserName CountryName,C.Id CountryId,SUM(IIH.Qty) Qty, SUM(IIH.TotalAmount) TotalAmount
									FROM trn.InventoryIssueHistory IIH 
									JOIN TRN.InventoryIssueDetail IID ON IID.Id=IIH.InventoryIssueDetailId
									left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
									left JOIN SCS.Country C ON C.Id=IR.CountryId
									WHERE IID.IsAsset= 0
									GROUP BY IID.InventoryIssueId,IID.IsAsset,C.UserName,C.Id
									) IIH ON IIH.InventoryIssueId=II.Id
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							
							left join dbo.Contract Con On Con.Id=II.ContractId
                            LEFT JOIN TRN.Voucher V ON V.Id=II.VoucherId
						WHERE II.PlantId= '" + plantId + @"'
						AND IIH.IsAsset= 0)X
						Order BY 2 DESC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}

		public IEnumerable<object> GetInventoryIssueByProductionOrder(string plantId, string productionOrderId)
		{
			try
			{

				var sql = @"SELECT * FROM (
                            SELECT II.Id,II.IssueDate IssueDate1,E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							,  II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.IssueDate, 'dd-MMM-yyyy') IssueDate
							
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,IIH.Qty
							,IIh.TotalAmount Amount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							,C.Id CountryId,c.UserName CountryName,II.ContractId,II.ProductionOrderId,Con.ContractNo
                            ,IsNULL(V.VoucherNo,'') VoucherNo ,IsPark=case when II.VoucherId<>'' then 0 else 1 end
							FROM[TRN].[InventoryIssue] AS II
							left join (
									SELECT IID.InventoryIssueId,IID.IsAsset,IIH.IssueRequestDetailId,SUM(IIH.Qty) Qty, SUM(IIH.TotalAmount) TotalAmount
									FROM trn.InventoryIssueHistory IIH JOIN TRN.InventoryIssueDetail IID ON IID.Id=IIH.InventoryIssueDetailId
									WHERE IID.IsAsset= 0
									GROUP BY IID.InventoryIssueId,IIH.IssueRequestDetailId,IID.IsAsset
									) IIH ON IIH.InventoryIssueId=II.Id
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
							left JOIN SCS.Country c ON C.Id=IR.CountryId
							left join dbo.Contract Con On Con.Id=II.ContractId
                            LEFT JOIN TRN.Voucher V ON V.Id=II.VoucherId
						WHERE II.PlantId= '" + plantId + @"' and II.ProductionOrderId='" + productionOrderId + @"'
						AND IIH.IsAsset= 0)X
						Order BY 2 DESC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}
		public IEnumerable<object> GetInventoryIssueBOQ(string plantId)
		{
			try
			{

				var sql = @"SELECT * FROM (
                            SELECT II.Id,II.IssueDate IssueDate1,E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							,  II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.IssueDate, 'dd-MMM-yyyy') IssueDate
							
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,IIH.Qty
							,IIh.TotalAmount Amount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							,C.Id CountryId,c.UserName CountryName,II.ContractId,II.ProductionOrderId,Con.ContractNo,II.VoucherId
							FROM[TRN].[InventoryIssue] AS II
							left join (
									SELECT IID.InventoryIssueId,IID.IsAsset,IIH.IssueRequestDetailId,SUM(IIH.Qty) Qty, SUM(IIH.TotalAmount) TotalAmount
									FROM trn.InventoryIssueHistory IIH JOIN TRN.InventoryIssueDetail IID ON IID.Id=IIH.InventoryIssueDetailId
									WHERE IID.IsAsset= 0
									GROUP BY IID.InventoryIssueId,IIH.IssueRequestDetailId,IID.IsAsset
									) IIH ON IIH.InventoryIssueId=II.Id
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
							left JOIN SCS.Country c ON C.Id=IR.CountryId
							left join dbo.Contract Con On Con.Id=II.ContractId
						WHERE II.PlantId= '" + plantId + @"' AND II.Types='InventoryBOQIssue'
						AND IIH.IsAsset= 0)X
						Order BY 2 DESC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}
		public IEnumerable<object> GetDataByInventoryReturnIssue(string plantId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = @"SELECT E.UserName AS Entity ,isnull(II.IssueType,'') issuetype, II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,FORMAT(II.IssueDate, 'dd-MMM-yyyy') IssueDate, MS.UserName AS MaterialStorage
									 --,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
									 ,SUM(IID.Qty) Qty,
									 --SUM(IID.PolicyAmount) Amount
									 II.Remarks,II.Id AS IssueId,II.OrderRefNo
                                    FROM[TRN].[InventoryIssueReturn] AS II
                                left JOIN [TRN].InventoryIssueReturnHistory AS IID ON IID.InventoryIssueReturnId= II.Id -- AND IID.IsAsset= 0
                                left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
                                left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
                                Left JOIN [ORG].[Entity] E On E.id= II.EntityId
                                WHERE II.PlantId= '" + plantId + @"' AND ISNULL(II.[Status],'') <>'Posting' 
                                GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
	                                 ,II.IssueDate, MS.UserName
									 ,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  Order BY II.Id DESC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
			}
		}

		public DataTable GetIssueRegister(string fromDate, string toDate, string Type)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				var temp = "";

				if (Type == "Posted")
				{
					temp = "and v.VoucherNo is not null";
				}
				if (Type == "NonPosted")
				{
					temp = "and v.VoucherNo is null";
				}

				sql = @"SELECT II.Id AS IssueId
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate
	                        ,En.UserName AS Entityname
                            ,HSNC.Code HSNCode,BUoM.UserName AS BaseUOM
                            ,'' PostingDate,'' PostedBy,II.Remarks
	                        ,MS.UserName AS MaterialStorageName
	                        ,v.VoucherNo,V.IsPark,IID.Id IssueDetailId ,IID.InventoryIssueId
	                        ,MT.UserName MaterialType,II.IssueType ,MGM.UserName AS MaterialGroupMasterName
	                        ,IM.MaterialMasterId ,MM.UserName MaterialMasterName
	                        ,ART.StandardName ArticleName
	                        ,IsAsset = CASE  WHEN MM.IsAsset = 0 THEN 'No' ELSE 'Yes' END
	                        ,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId
	                        ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
	                        ,IM.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId
	                        ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
	                        ,IM.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId
	                        ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
	                        ,IID.TransactionQty
	                        ,TUoM.UserName AS UOM
	                        ,Round(IID.AvgRate,2) AvgRate
	                        ,Round(IID.AvgAmount,2) AvgAmount
	                        ,Round(IID.PolicyRate,2) PolicyRate
	                        ,Round(IID.PolicyAmount,2) PolicyAmount
	                        ,IID.Policy
	                        ,IID.BaseQty
	                        ,IID.InventoryReceiveId
	                        ,IID.InventoryReceiveDetailId

                            ,GLCode=case when v.Id <>'' then  ISNULL(IGL.AccountCode,'') else ISNULL(IGLNP.AccountCode,'') end
                            ,GL=case when v.Id <>'' then ISNULL(IGL.UserName,'') else ISNULL(IGLNP.UserName,'') end
							,Activity=case when v.Id <>'' then ISNULL(IA.UserName,'') else ISNULL(IANP.UserName,'') end
							,Budget=case when v.Id <>'' then isnull(B.UserName,'') else ISNULL(BNP.UserName,'') end
							,BudgetRefNo=case when v.Id <>'' then isnull(IBM.RefNo,'') else ISNULL(IBMNP.RefNo,'') end

                            ,CGLCode=case when v.Id <>'' then  ISNULL(IGL1.AccountCode,'') else ISNULL(IIH.CGLCode,'') end
							,CGL=case when v.Id <>''  then  ISNULL(IGL1.UserName,'') else ISNULL(IIH.CGL,'') end
							,CActivity=case when v.Id <>'' then  ISNULL(IA1.UserName,'') else ISNULL(IIH.CActivity,'') end
							,CBUdget=case when v.Id <>'' then  ISNULL(B1.UserName,'') else ISNULL(IIH.CBUdget,'') end
                            ,CBudgetRefNo=case when v.Id <>'' then  ISNULL(IBM1.RefNo,'') else ISNULL(IIH.CBudgetRefNo,'') end

                            ,CC.UserName CostCenterName,EI.EmployeeName,D.UserName DepartmentName

                            ,Level1=case when v.Id <>'' then C1.UserName else ISNULL(C1NP.UserName,'') end
							,Level2=case when v.Id <>'' then C2.UserName else ISNULL(C2NP.UserName,'') end
							,Level3=case when v.Id <>'' then C3.UserName else ISNULL(C3NP.UserName,'') end
							,Level4=case when v.Id <>'' then C4.UserName else ISNULL(C4NP.UserName,'') end

                            ,CLevel1=case when v.Id <>'' then CC1.UserName else ISNULL(IIH.CRLevel1,'') end
							,CLevel2=case when v.Id <>'' then CC2.UserName else ISNULL(IIH.CRLevel2,'') end
							,CLevel3=case when v.Id <>'' then CC3.UserName else ISNULL(IIH.CRLevel3,'') end
							,CLevel4=case when v.Id <>'' then CC4.UserName else ISNULL(IIH.CRLevel4,'') end
                            
                            ,Status =case when II.VoucherId<>'' then 'Posted' else 'NonPosted' end
                           ,PO.Id PONo
                        FROM trn.InventoryIssue II
                        LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId
                        LEFT JOIN ORG.CostCenter CC ON CC.Id=IID.CostCenterId
                        LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                        LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IID.InventoryMaterialId
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
                        LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
                        LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
						LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IID.PostDrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IID.PostDrBudgetMasterId
						LEFT JOIN HKP.Activity IA ON IA.Id=IID.PostDrActivityId
						Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
						LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IID.PostCrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IID.PostCrBudgetMasterId
						LEFT JOIN HKP.Activity IA1 ON IA1.Id=IID.PostCrActivityId
						Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
                        LEFT JOIN HKP.COALevel1 C1 ON C1.Id=IGL.COALevel1Id
						LEFT JOIN HKP.COALevel2 C2 ON C2.Id=IGL.COALevel2Id
						LEFT JOIN HKP.COALevel3 C3 ON C3.Id=IGL.COALevel3Id
						LEFT JOIN HKP.COALevel4 C4 ON C4.Id=IGL.COALevel4Id
                        LEFT JOIN HKP.COALevel1 CC1 ON CC1.Id=IGL1.COALevel1Id
						LEFT JOIN HKP.COALevel2 CC2 ON CC2.Id=IGL1.COALevel2Id
						LEFT JOIN HKP.COALevel3 CC3 ON CC3.Id=IGL1.COALevel3Id
						LEFT JOIN HKP.COALevel4 CC4 ON CC4.Id=IGL1.COALevel4Id
                        LEFT join dbo.EmployeeInformation EI ON EI.SystemId=II.EmployeeId
                        LEFT join [ORG].[Department] D ON D.Id=EI.DepartmentId
						LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IID.BaseUOMId = BUoM.Id
                        left join [TRN].[ProductionOrder] PO on PO.Id=II.ProductionOrderId
						--NotPosted---
						LEFT JOIN MST.BudgetMaster IBMNP ON IBMNP.Id=IID.BudgetMasterId
						LEFT JOIN HKP.GLGeneralInfo IGLNP ON IGLNP.Id=IBMNP.GLGeneralInfoId 
						LEFT JOIN HKP.Activity IANP ON IANP.Id=IID.ActivityId
						Left JOIN hkp.Budget BNP On BNP.Id=IBMNP.BudgetId
						LEFT JOIN HKP.COALevel1 C1NP ON C1NP.Id=IGLNP.COALevel1Id
						LEFT JOIN HKP.COALevel2 C2NP ON C2NP.Id=IGLNP.COALevel2Id
						LEFT JOIN HKP.COALevel3 C3NP ON C3NP.Id=IGLNP.COALevel3Id
						LEFT JOIN HKP.COALevel4 C4NP ON C4NP.Id=IGLNP.COALevel4Id
						LEFT JOIN (SELECT DISTINCT IIH.InventoryIssueDetailId,IGL1.AccountCode CGLCode,isnull(IGL1.UserName,'') AS CGL
							,isnull(IA1.UserName,'') AS CActivity
							,isnull(B1.UserName,'') AS CBUdget ,IBM1.RefNo CBudgetRefNo
							,C1.UserName CRLevel1,C2.UserName CRLevel2,C3.UserName CRLevel3,C4.UserName CRLevel4
							FROM TRN.InventoryIssueHistory IIH 
					LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
					LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IRD.PostDrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IRD.PostDrBudgetMasterId
					LEFT JOIN HKP.Activity IA1 ON IA1.Id=IRD.PostDrActivityId
					Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
					LEFT JOIN HKP.COALevel1 C1 ON C1.Id=IGL1.COALevel1Id
						LEFT JOIN HKP.COALevel2 C2 ON C2.Id=IGL1.COALevel2Id
						LEFT JOIN HKP.COALevel3 C3 ON C3.Id=IGL1.COALevel3Id
						LEFT JOIN HKP.COALevel4 C4 ON C4.Id=IGL1.COALevel4Id
				) IIH ON  IIH.InventoryIssueDetailId=IID.Id
						--NotPosted
                    where II.PlantId='" + identity.PlantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' " + temp + @"";

				return _sqlRepository.GetDataTable(sql);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		// OutSource

		public IEnumerable<object> GetOSIssueRegister(string fromDate, string toDate, string Type)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				if (Type == "Posted")
				{
					sql = @"SELECT II.Id AS IssueId,ospo.Id as PONumber
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate
	                        ,En.UserName AS Entityname
	                        ,MS.UserName AS MaterialStorageName
	                        ,II.STATUS
	                        ,v.VoucherNo
	                        ,IID.Id IssueDetailId
	                        ,IID.InventoryIssueId
	                        ,MT.UserName MaterialType,II.IssueType
	                        ,MGM.UserName AS MaterialGroupMasterName
	                        ,IM.MaterialMasterId
	                        ,MM.UserName MaterialMasterName
	                        ,ART.StandardName ArticleName
	                        ,IsAsset = CASE  WHEN MM.IsAsset = 0 THEN 'No' ELSE 'Yes' END
	                        ,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId
	                        ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
	                        ,IM.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId
	                        ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
	                        ,IM.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId
	                        ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
	                        ,IID.TransactionQty
	                        ,TUoM.UserName AS UOM
	                        ,Round(IID.AvgRate,2) AvgRate
	                        ,Round(IID.AvgAmount,2) AvgAmount
	                        ,Round(IID.PolicyRate,2) PolicyRate
	                        ,Round(IID.PolicyAmount,2) PolicyAmount
	                        ,IID.Policy
	                        ,IID.BaseQty
	                        ,IID.InventoryReceiveId
	                        ,IID.InventoryReceiveDetailId
                            ,ISNULL(IGL.UserName,'') AS GL
							,ISNULL(IA.UserName,'') Activity
							,isnull(B.UserName,'') AS Budget
							,isnull(IGL1.UserName,'') AS CGL
							,isnull(IA1.UserName,'') AS CActivity
							,isnull(B1.UserName,'') AS CBUdget
                            ,CC.UserName CostCenterName,EI.EmployeeName
                            ,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName--,MLC.LCRef
                            ,PLC.LCRef as PurchaseLCNo,pod.ReferenceNo
                        FROM trn.InventoryIssue II
                        LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId
                        LEFT JOIN ORG.CostCenter CC ON CC.Id=IID.CostCenterId
                        LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                        LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IID.InventoryMaterialId
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
                        LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
						LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IID.PostDrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IID.PostDrBudgetMasterId
						LEFT JOIN HKP.Activity IA ON IA.Id=IID.PostDrActivityId
						Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
						LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IID.PostCrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IID.PostCrBudgetMasterId
						LEFT JOIN HKP.Activity IA1 ON IA1.Id=IID.PostCrActivityId
						Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
                        LEFT join dbo.EmployeeInformation EI ON EI.SystemId=II.EmployeeId
                        left join dbo.OSTransformationPO ospo on ospo.Id=II.JWContractId
						left join [dbo].[Contract] Ct on Ct.Id=ospo.ContractId
						left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
						left join dbo.PurchaseLC PLC on PLC.Id=ospo.PurchaseLCId
                        left join dbo.OSTransformationPODetail pod on pod.OSTransformationPOId=ospo.Id and pod.Id=IID.OSTransformationPOId
                    where v.VoucherNo is not null ANd II.PlantId='" + identity.PlantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
                     and II.Types='InventoryOSIssue' ";

				}
				else
				{
					sql = @"SELECT II.Id AS IssueId,ospo.Id as PONumber
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate
	                        ,En.UserName AS Entityname
	                        ,MS.UserName AS MaterialStorageName
	                        ,II.STATUS
	                        ,v.VoucherNo
	                        ,IID.Id IssueDetailId
	                        ,IID.InventoryIssueId
	                        ,MT.UserName MaterialType,II.IssueType
	                        ,MGM.UserName AS MaterialGroupMasterName
	                        ,IM.MaterialMasterId
	                        ,MM.UserName MaterialMasterName
	                        ,ART.StandardName ArticleName
	                        ,IsAsset = CASE   WHEN MM.IsAsset = 0 THEN 'No' ELSE 'Yes' END
	                        ,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId
	                        ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
	                        ,IM.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId
	                        ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
	                        ,IM.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId
	                        ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
	                        ,IID.TransactionQty
	                        ,TUoM.UserName AS UOM
	                        ,Round(IID.AvgRate,2) AvgRate
	                        ,Round(IID.AvgAmount,2) AvgAmount
	                        ,Round(IID.PolicyRate,2) PolicyRate
	                        ,Round(IID.PolicyAmount,2) PolicyAmount
	                        ,IID.Policy
	                        ,IID.BaseQty
	                        ,IID.InventoryReceiveId
	                        ,IID.InventoryReceiveDetailId
                            ,ISNULL(IGL.UserName,'') AS GL
							,ISNULL(IA.UserName,'') Activity
							,isnull(B.UserName,'') AS Budget
							,isnull(IGL1.UserName,'') AS CGL
							,isnull(IA1.UserName,'') AS CActivity
							,isnull(B1.UserName,'') AS CBUdget
                            ,CC.UserName CostCenterName
                            ,EI.EmployeeName
                            ,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName--,MLC.LCRef
                            ,PLC.LCRef as PurchaseLCNo,pod.ReferenceNo
                        FROM trn.InventoryIssue II
                        LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId
                        LEFT JOIN ORG.CostCenter CC ON CC.Id=IID.CostCenterId
                        LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                        LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IID.InventoryMaterialId
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
                        LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
						LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IID.PostDrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IID.PostDrBudgetMasterId
						LEFT JOIN HKP.Activity IA ON IA.Id=IID.PostDrActivityId
						Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
						LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IID.PostCrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IID.PostCrBudgetMasterId
						LEFT JOIN HKP.Activity IA1 ON IA1.Id=IID.PostCrActivityId
						Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
                        LEFT join dbo.EmployeeInformation EI ON EI.SystemId=II.EmployeeId
                        left join dbo.OSTransformationPO ospo on ospo.Id=II.JWContractId
						left join [dbo].[Contract] Ct on Ct.Id=ospo.ContractId
						left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
						left join dbo.PurchaseLC PLC on PLC.Id=ospo.PurchaseLCId
                        left join dbo.OSTransformationPODetail pod on pod.OSTransformationPOId=ospo.Id and pod.Id=IID.OSTransformationPOId
                    where v.VoucherNo is null ANd II.PlantId='" + identity.PlantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
                     and II.Types='InventoryJWIssue' ";

				}

				return _sqlRepository.GetDataCollection(sql);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}
		

		public IEnumerable<object> GetOSIssueRegisterBYGRN(string fromDate, string toDate, string Type)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				if (Type == "Posted")
				{

					sql = @"SELECT II.Id AS IssueId,IID.Id as IssueDetailId
                            ,OSPOType=case when ospo.POType='OSValueAddedPO' then 'ValueAdded' else 'Transformation' End
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate	 
	                        ,MT.UserName MaterialType
	                        ,MGM.UserName AS MaterialGroupMasterName
	                        ,IM.MaterialMasterId
	                        ,MM.UserName MaterialMasterName	                      
	                        ,ART.StandardName ArticleName	                        
	                        ,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId
	                        ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
	                        ,IM.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId
	                        ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
	                        ,IM.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId
	                        ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
							,IIH.InventoryReceiveDetailId 
							,IRD.Id GRNDetailId
							,IRD.TransactionQty GRNQty
							--,TUoM1.UserName AS GRNUOM
							,GRNUOM=case when IRD.BaseUOMId is not null then TUoM1.UserName else TUoM2.UserName End
							,TUoM2.UserName as TrnUoM
							,IRD.MaterialTranRate GRNRate
							,C.Code AS TransactionCurrency
							,Ir.ToCurrencyRate CurrencyConvRate
							,IRD.TotalMaterialBooksCurrencyAmount TrnAmtBDT
							,IRD.BaseQty GRNBaseQty
                        	,round(IRD.BooksCurrencyBaseRate,4) BaseRate
                        	,(IRD.BaseQty * IRD.BooksCurrencyBaseRate) BaseAmtBDT
							,MS.UserName as MaterialStorage
							,isnull(IIH1.Qty,0) OtherIssuedQty
							,isnull(IIH.Qty,0) CurrentIssueQty
							,TUoM.UserName AS IssueUOM							
	                        ,TotalIssued=(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0))						
							,Balance=(Isnull(IRD.BaseQty,0)-(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0)))
	                        

                           ,ISNULL(IGL.UserName,'') AS GL
							,ISNULL(IA.UserName,'') Activity
							,isnull(B.UserName,'') AS Budget
							,isnull(IGL1.UserName,'') AS CGL
							,isnull(IA1.UserName,'') AS CActivity
							,isnull(B1.UserName,'') AS CBUdget
                            ,CC.UserName CostCenterName
                            ,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName--,MLC.LCRef
                            ,PLC.LCRef as PurchaseLCNo,ospo.Id as PONumber,pod.ReferenceNo
                        FROM trn.InventoryIssue II
                        LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId	
                        LEFT JOIN ORG.CostCenter CC ON CC.Id=IID.CostCenterId
                        LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                        LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IID.InventoryMaterialId
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                       
                        LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
						LEFT JOIN trn.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
						LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
						LEFT JOIN(select Sum(Qty) Qty,InventoryIssueDetailId from  trn.InventoryIssueHistory group by InventoryIssueDetailId) IIH1 ON IIH1.InventoryIssueDetailId=IID.Id --AND  IID.InventoryIssueId !=II.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
					   LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM1 ON IRD.BaseUOMId = TUoM1.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM2 ON IRD.TransactionUoMId = TUoM2.Id


                      LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IID.PostDrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IID.PostDrBudgetMasterId
						LEFT JOIN HKP.Activity IA ON IA.Id=IID.PostDrActivityId
						Left JOIN hkp.Budget B On B.Id=IBM.BudgetId


						LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IID.PostCrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IID.PostCrBudgetMasterId
						LEFT JOIN HKP.Activity IA1 ON IA1.Id=IID.PostCrActivityId
						Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
                        left join dbo.OSTransformationPO ospo on ospo.Id=II.JWContractId
						left join [dbo].[Contract] Ct on Ct.Id=ospo.ContractId
						left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
						left join dbo.PurchaseLC PLC on PLC.Id=ospo.PurchaseLCId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id = IRD.InventoryReceiveId
						LEFT JOIN SCS.Currency C ON C.Id = IR.CurrencyId
                        left join dbo.OSTransformationPODetail pod on pod.OSTransformationPOId=ospo.Id and pod.Id=IID.OSTransformationPOId

                    where v.VoucherNo is not null ANd II.PlantId='" + identity.PlantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
                     and II.Types='InventoryOSIssue' ";

				}
				else
				{


					sql = @"SELECT II.Id AS IssueId,IID.Id as IssueDetailId
                            ,OSPOType=case when ospo.POType='OSValueAddedPO' then 'ValueAdded' else 'Transformation' End
	                        ,REPLACE(CONVERT(CHAR(11), II.IssueDate, 106), ' ', '-') IssueDate	 
	                        ,MT.UserName MaterialType
	                        ,MGM.UserName AS MaterialGroupMasterName
	                        ,IM.MaterialMasterId
	                        ,MM.UserName MaterialMasterName	                      
	                        ,ART.StandardName ArticleName	                        
	                        ,FC.UserName AS FirstCharacteristics
	                        ,IM.FirstCharacteristicsValueId
	                        ,ISNULL(FCV.UserName, '') AS FirstCharacteristicsValue
	                        ,IM.SecondCharacteristicsId
	                        ,SC.UserName AS SecondCharacteristics
	                        ,IM.SecondCharacteristicsValueId
	                        ,ISNULL(SCV.UserName, '') AS SecondCharacteristicsValue
	                        ,IM.ThirdCharacteristicsId
	                        ,TC.UserName AS ThirdCharacteristics
	                        ,IM.ThirdCharacteristicsValueId
	                        ,ISNULL(TCV.UserName, '') AS ThirdCharacteristicsValue
							,IIH.InventoryReceiveDetailId 
							,IRD.Id GRNDetailId
							,IRD.TransactionQty GRNQty
							--,TUoM1.UserName AS GRNUOM
							,GRNUOM=case when IRD.BaseUOMId is not null then TUoM1.UserName else TUoM2.UserName End
							,TUoM2.UserName as TrnUoM
							,IRD.MaterialTranRate GRNRate
							,C.Code AS TransactionCurrency
							,Ir.ToCurrencyRate CurrencyConvRate
							,IRD.TotalMaterialBooksCurrencyAmount TrnAmtBDT
							,IRD.BaseQty GRNBaseQty
                        	,round(IRD.BooksCurrencyBaseRate,4) BaseRate
                        	,(IRD.BaseQty * IRD.BooksCurrencyBaseRate) BaseAmtBDT    
                             ,MS.UserName as MaterialStorage
							,isnull(IIH1.Qty,0) OtherIssuedQty
							,isnull(IIH.Qty,0) CurrentIssueQty
							,TUoM.UserName AS IssueUOM							
	                        ,TotalIssued=(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0))						
							,Balance=(Isnull(IRD.BaseQty,0)-(isnull(IIH1.Qty,0) + ISNULL(IIH.Qty,0)))
	                        

                           ,ISNULL(IGL.UserName,'') AS GL
							,ISNULL(IA.UserName,'') Activity
							,isnull(B.UserName,'') AS Budget
							,isnull(IGL1.UserName,'') AS CGL
							,isnull(IA1.UserName,'') AS CActivity
							,isnull(B1.UserName,'') AS CBUdget
                            ,CC.UserName CostCenterName
                            ,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName--,MLC.LCRef
                            ,PLC.LCRef as PurchaseLCNo,ospo.Id as PONumber,pod.ReferenceNo
                        FROM trn.InventoryIssue II
                        LEFT JOIN trn.InventoryIssueDetail IID ON II.Id = IId.InventoryIssueId	
                        LEFT JOIN ORG.CostCenter CC ON CC.Id=IID.CostCenterId
                        LEFT JOIN ORG.Entity En ON II.EntityId = En.Id
                        LEFT JOIN HKP.MaterialStorage MS ON II.MaterialStorageId = MS.Id
                        LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id = IID.InventoryMaterialId
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                       
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        --LEFT JOIN trn.Invoice AS I ON I.InventoryReceiveId = II.Id
                        LEFT JOIN trn.Voucher V ON V.Id = II.VoucherId
						LEFT JOIN trn.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
						LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
						LEFT JOIN(select Sum(Qty) Qty,InventoryIssueDetailId from  trn.InventoryIssueHistory group by InventoryIssueDetailId) IIH1 ON IIH1.InventoryIssueDetailId=IID.Id --AND  IID.InventoryIssueId !=II.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
					   LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM1 ON IRD.BaseUOMId = TUoM1.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM2 ON IRD.TransactionUoMId = TUoM2.Id


                      LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IID.PostDrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IID.PostDrBudgetMasterId
						LEFT JOIN HKP.Activity IA ON IA.Id=IID.PostDrActivityId
						Left JOIN hkp.Budget B On B.Id=IBM.BudgetId


						LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IID.PostCrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IID.PostCrBudgetMasterId
						LEFT JOIN HKP.Activity IA1 ON IA1.Id=IID.PostCrActivityId
						Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
                        left join dbo.OSTransformationPO ospo on ospo.Id=II.JWContractId
						left join [dbo].[Contract] Ct on Ct.Id=ospo.ContractId
						left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
					--	LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
						left join dbo.PurchaseLC PLC on PLC.Id=ospo.PurchaseLCId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id = IRD.InventoryReceiveId
						LEFT JOIN SCS.Currency C ON C.Id = IR.CurrencyId
                      --  left join dbo.OSTransformationPO PO on PO.Id=II.JWContractId
                        left join dbo.OSTransformationPODetail pod on pod.OSTransformationPOId=ospo.Id and pod.Id=IID.OSTransformationPOId

                    where v.VoucherNo is null ANd II.PlantId='" + identity.PlantId + "' AND convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
                    and II.Types='InventoryJWIssue' ";

				}

				return _sqlRepository.GetDataCollection(sql);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public IEnumerable<object> GetIssueRegisterDetail(string id)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				var sql = @"select Id,InventoryIssueDetailId,InventoryReceiveDetailId,Qty,Round(Rate,4) Rate,AddedBy,REPLACE(CONVERT(CHAR(11), AddedDate, 106), ' ', '-') AddedDate  from trn.InventoryIssueHistory where InventoryIssueDetailId='" + id + "'";


				return _sqlRepository.GetDataCollection(sql);
			}

			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

	}
}
