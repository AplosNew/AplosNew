using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Inventory;
using Library.Model.Parties;
using Library.Model.Taxations;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Helpers;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
    }
}
