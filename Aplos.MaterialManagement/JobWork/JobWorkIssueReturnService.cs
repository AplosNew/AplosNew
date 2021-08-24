using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;

using Library.Data;
using Library.Service.Enums;
using Library.Service.Logs;
using System.Reflection;

namespace Library.MaterialManagement.JobWork
{

    public class JobWorkIssueReturn
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public JobWorkIssueReturn()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        //       FOR JOB WORK MODULE

        //public IEnumerable<object> GetMaterialInputData(IEnumerable<MaterialPlanning> SelectedMaterialPlanningData)
        //{
        //    try
        //    {
        //        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        var MPId = "' '";

        //        foreach (var get in SelectedMaterialPlanningData)
        //        {
        //            MPId += ",'" + get.Id + "' ";

        //        }

        //        string sql = "";
        //        if (!string.IsNullOrEmpty(MPId))
        //        {
        //            sql = @"select distinct NULL AS LotNumberList, mi.Id,mi.JobWorkTransformationContractChildMasterId, jwi.UserName as JWOutputItem ,mm.Id as MaterialMasterId, mm.UserName as Material
        //                    ,mma.Id as MaterialArticleId, mma.StandardName as Article, InvDetail.InventoryMaterialId
        //                    ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
        //                    ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
        //                    ,SUM(tirc.Quantity) as TIRCQty
        //                    ,(InvDetail.Rate) as Rate
        //                    ,Sum(kk.TotalQuantity) as TIRCTotalQty
        //                     from dbo.JobWorkTransformationContractChild3 mi
        //                     left join dbo.JobWorkTransformationIssueReturnChild tirc on tirc.MaterialInputId=mi.Id
        //                     left join MST.MaterialMaster mm on mm.Id=mi.MaterialMasterId
        //                     left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
        //left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
        //                     left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
        //                     left join(select SUM(Quantity) as TotalQuantity,MaterialInputId FROM dbo.JobWorkTransformationIssueReturnChild group by MaterialInputId) kk on kk.MaterialInputId=mi.id
        //                     left join TRN.InventoryMaterial inm on inm.MaterialMasterId=mm.Id and inm.ArticleId=mma.Id
        //                     left join (Select InventoryMaterialId,(sum( MaterialTranAmount)/sum(TransactionQty)) as Rate from TRN.InventoryReceiveDetail group by InventoryMaterialId) InvDetail on InvDetail.InventoryMaterialId=inm.Id
        //                     where mi.JobWorkTransformationContractChildMasterId IN ("+ MPId + ") group by mi.Id, mm.Id, mm.UserName,InvDetail.Rate ,mma.Id, mma.StandardName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity, InvDetail.InventoryMaterialId,mi.JobWorkTransformationContractChildMasterId,jwi.UserName ";
        //        }
        //            var SqlData = _sqlRepository.GetDataCollection(sql);
        //            StringCollection strCol = new StringCollection();
        //            string MaterialMasterList = "''";
        //            string MaterialMasterArticleList = "''";
        //            for (int i = 0; i < SqlData.Count; i++)
        //            {
        //                if (strCol.Contains(SqlData[i]["MaterialMasterId"].ToString()) == true && strCol.Contains(SqlData[i]["MaterialArticleId"].ToString()) == true)
        //                    continue;
        //                strCol.Add(SqlData[i]["MaterialMasterId"].ToString());
        //                strCol.Add(SqlData[i]["MaterialArticleId"].ToString());
        //                MaterialMasterList += ",'" + SqlData[i]["MaterialMasterId"].ToString() + "'";
        //                MaterialMasterArticleList += ",'" + SqlData[i]["MaterialArticleId"].ToString() + "'";

        //            }

        //            var LotNoList = _sqlRepository.GetDataCollection(@"select IRD.LotNo Text, IRD.LotNo Value,IM.MaterialMasterId, IM.ArticleId from trn.InventoryReceiveDetail IRD
        //                                                                   left join trn.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
        //                                                                   where IM.MaterialMasterId IN (" + MaterialMasterList + ") and IM.ArticleId IN (" + MaterialMasterArticleList + ") ");

        //            for (int i = 0; i < SqlData.Count; i++)
        //            {
        //                var temp = LotNoList.Where(ee => ee["MaterialMasterId"].ToString() == SqlData[i]["MaterialMasterId"].ToString() && ee["ArticleId"].ToString() == SqlData[i]["MaterialArticleId"].ToString()).ToList();

        //                SqlData[i]["LotNumberList"] = temp;
        //            }

        //            return SqlData;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //}

        public IEnumerable<object> GetMaterialInputData(IEnumerable<MaterialPlanning> SelectedMaterialPlanningData, string OrderSpecific, string MaterialStorageIdInventory, string IssueDate)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var MPId = "' '";
                string sql = "";
                foreach (var get in SelectedMaterialPlanningData)
                {
                    MPId += ",'" + get.Id + "' ";

                }

                if (OrderSpecific == "Yes")
                {
                    //             sql = @"select mi.Id,mi.JobWorkTransformationContractChildMasterId
                    //                     ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId ,uom.UserName as MMUnit
                    //                     ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                    //                     ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
                    //                     --,SUM(tirc.Quantity) as TIRCQty
                    //                     ,Sum(kk.TotalQuantity) as TIRCTotalQty
                    //                     ,0 PlannedQty,0 IssuedQty,0 BalanceQty
                    //                     ,0 TotalQty
                    //,0 PostingQty
                    //,0 PostingQuantity
                    //,0 ApprovedQty
                    //,0 UnApprovedQty,null MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
                    //                      from dbo.JobWorkTransformationContractChild3 mi
                    //left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
                    //left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                    // left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                    //                      left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
                    //                      left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
                    //                      on iid.InventoryIssueId=II.Id group by II.JWContractId) kk on kk.JWContractId=mp.JobWorkTransformationContractMasterId
                    //                      where mi.JobWorkTransformationContractChildMasterId IN (" + MPId + @")
                    // group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.JobWorkTransformationContractChildMasterId
                    // ,uom.UserName,mm.Code ,mma.StandardName ,mma.Id";
                    sql = @"SELECT -- t.Id,
                            t.JobWorkTransformationContractChildMasterId
                            ,t.InputMaterialId,t.MaterialMasterId,t.MaterialMaster,t.InputMaterialCode,t.ArticleName,t.ArticleId ,t.MMUnit
                            ,sum(t.RequiredQuantity) RequiredQuantity,t.BalanceToIssue,t.TIRCTotalQty,t.PlannedQty,t.IssuedQty,t.BalanceQty,t.MaterialStorageId,t.TransactionUoMid TransactionUoMId,t.BaseUoMid BaseUoMId
                            ,t.TotalQty TotalQty
                            ,t.PostingQty PostingQty
                            ,t.PostingQty PostingQuantity
                            ,t.ApprovedQty ApprovedQty
                            ,t.UnApprovedQty UnApprovedQty
                            ,t.BaseUoMFactor,t.TransactionUoM   
                            FROM(
                            SELECT mi.Id,mi.JobWorkTransformationContractChildMasterId,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId ,uom.UserName as MMUnit,
                            IRD.MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid,Sum(kk.TotalQuantity) as TIRCTotalQty,RequiredQuantity=Sum(ISNULL((mp.Quantity * mi.GrossConsumption),0))
                            ,BalanceToIssue=Sum(ISNULL((mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0')),0))
                            ,0 PlannedQty,0 IssuedQty,0 BalanceQty,IRD.BaseUoMFactor,uom.UserName as TransactionUoM  
                            ,TotalQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
                            FROM [TRN].[InventoryReceiveDetail] AS IRD
                            JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                            JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                            JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id

                            JOIN dbo.JobWorkTransformationContractChild3 mi ON mi.ArticleId=IM.ArticleId
                            left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
                            left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                            left join MST.MaterialMaster mm1 on mm1.Id=IM.MaterialMasterId

                            LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                            left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                            left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
                            left join(select iid.InventoryMaterialId, iid.Id,SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId 
											FROM TRN.InventoryIssueDetail iid 
											left join TRN.InventoryIssue II
			                            on iid.InventoryIssueId=II.Id group by II.JWContractId,iid.Id,iid.InventoryMaterialId
			                            ) kk on kk.JWContractId=mp.JobWorkTransformationContractMasterId and kk.InventoryMaterialId=IM.Id


                            WHERE CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE) 
                            AND mi.JobWorkTransformationContractChildMasterId IN (" + MPId + @")
                            AND IRD.MaterialStorageId='"+ MaterialStorageIdInventory + @"' AND IM.CompanyGroupId='"+ identity.CompanyGroupId+ @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
                            GROUP By mi.Id,mi.JobWorkTransformationContractChildMasterId
                            ,mm.Id  ,mm.Id ,mm.UserName  ,mm.Code  ,mma.StandardName ,mma.Id,uom.UserName 
                            ,IRD.MaterialStorageId,uom.Id  ,uom.Id  ,IRD.BaseUoMFactor,uom.UserName  

                            UNION ALL
                            SELECT mi.Id,mi.JobWorkTransformationContractChildMasterId
                            ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId ,uom.UserName as MMUnit,
                            IRD.MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
                            ,Sum(kk.TotalQuantity) as TIRCTotalQty,RequiredQuantity=Sum(ISNULL((mp.Quantity * mi.GrossConsumption),0))
                            ,BalanceToIssue=Sum(ISNULL((mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0')),0))
                            ,0 PlannedQty,0 IssuedQty,0 BalanceQty,IRD.BaseUoMFactor,uom.UserName as TransactionUoM  
                            ,0 TotalQty, PostingQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0 ApprovedQty, 0 UnApprovedQty
                            FROM [TRN].[InventoryReceiveDetail] AS IRD
                            left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                            left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                            left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                            left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id

                            JOIN dbo.JobWorkTransformationContractChild3 mi ON mi.ArticleId=IM.ArticleId
                            left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
                            left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                            left join MST.MaterialMaster mm1 on mm1.Id=IM.MaterialMasterId

                            LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id

                            left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                            left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
                           left join(select iid.InventoryMaterialId, iid.Id,SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId 
											FROM TRN.InventoryIssueDetail iid 
											left join TRN.InventoryIssue II
			                            on iid.InventoryIssueId=II.Id group by II.JWContractId,iid.Id,iid.InventoryMaterialId
			                            ) kk on kk.JWContractId=mp.JobWorkTransformationContractMasterId and kk.InventoryMaterialId=IM.Id


                            WHERE CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE) 
                            AND mi.JobWorkTransformationContractChildMasterId IN (" + MPId + @")
                            AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"' AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
                            AND IR.Status is not NULL
                            GROUP By mi.Id,mi.JobWorkTransformationContractChildMasterId
                            ,mm.Id  ,mm.Id ,mm.UserName  ,mm.Code  ,mma.StandardName ,mma.Id,uom.UserName 
                            ,IRD.MaterialStorageId,uom.Id  ,uom.Id ,IRD.BaseUoMFactor ,uom.UserName 
                            UNION ALL
                            SELECT mi.Id,mi.JobWorkTransformationContractChildMasterId
                            ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId ,uom.UserName as MMUnit,
                            IRD.MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
                            ,Sum(kk.TotalQuantity) as TIRCTotalQty,RequiredQuantity=Sum(ISNULL((mp.Quantity * mi.GrossConsumption),0))
                            ,BalanceToIssue=Sum(ISNULL((mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0')),0))
                            ,0 PlannedQty,0 IssuedQty,0 BalanceQty		,IRD.BaseUoMFactor,uom.UserName as TransactionUoM     
                            ,0 TotalQty,0 PostingQty,ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 UnApprovedQty
                            FROM [TRN].[InventoryReceiveDetail] AS IRD
                            left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                            left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                            left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                            left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id


                            JOIN dbo.JobWorkTransformationContractChild3 mi ON mi.ArticleId=IM.ArticleId
                            left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
                            left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                            left join MST.MaterialMaster mm1 on mm1.Id=IM.MaterialMasterId

                            LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                            left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                            left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
                            left join(select iid.InventoryMaterialId, iid.Id,SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId 
											FROM TRN.InventoryIssueDetail iid 
											left join TRN.InventoryIssue II
			                            on iid.InventoryIssueId=II.Id group by II.JWContractId,iid.Id,iid.InventoryMaterialId
			                            ) kk on kk.JWContractId=mp.JobWorkTransformationContractMasterId and kk.InventoryMaterialId=IM.Id


                            WHERE CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE) 
                            AND mi.JobWorkTransformationContractChildMasterId IN (" + MPId + @")
                            AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"' AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'  
                            AND IR.IsApproved=1
                            GROUP By mi.Id,mi.JobWorkTransformationContractChildMasterId
                            ,mm.Id  ,mm.Id ,mm.UserName  ,mm.Code  ,mma.StandardName ,mma.Id,uom.UserName 
                            ,IRD.MaterialStorageId,uom.Id  ,uom.Id  ,IRD.BaseUoMFactor,uom.UserName 
                            UNION ALL
                            SELECT mi.Id,mi.JobWorkTransformationContractChildMasterId
                            ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId ,uom.UserName as MMUnit,
                            IRD.MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
                            ,Sum(kk.TotalQuantity) as TIRCTotalQty,RequiredQuantity=Sum(ISNULL((mp.Quantity * mi.GrossConsumption),0))
                            ,BalanceToIssue=Sum(ISNULL((mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0')),0))
                            ,0 PlannedQty,0 IssuedQty,0 BalanceQty,IRD.BaseUoMFactor,uom.UserName as TransactionUoM  
                            ,0 TotalQty,0 PostingQty,0 ApprovedQty, UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))))
                            FROM [TRN].[InventoryReceiveDetail] AS IRD
                            left JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                            left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                            left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                            left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id

                            JOIN dbo.JobWorkTransformationContractChild3 mi ON mi.ArticleId=IM.ArticleId
                            left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
                            left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                            left join MST.MaterialMaster mm1 on mm1.Id=IM.MaterialMasterId

                            LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                            left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                            left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
                            left join(select iid.InventoryMaterialId, iid.Id,SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId 
											FROM TRN.InventoryIssueDetail iid 
											left join TRN.InventoryIssue II
			                            on iid.InventoryIssueId=II.Id group by II.JWContractId,iid.Id,iid.InventoryMaterialId
			                            ) kk on kk.JWContractId=mp.JobWorkTransformationContractMasterId and kk.InventoryMaterialId=IM.Id

                            WHERE CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE) 
                            AND mi.JobWorkTransformationContractChildMasterId IN (" + MPId + @")
                            AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"' AND IR.IsApproved=0 AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
                            GROUP By mi.Id,mi.JobWorkTransformationContractChildMasterId
                            ,mm.Id  ,mm.Id ,mm.UserName  ,mm.Code  ,mma.StandardName ,mma.Id,uom.UserName 
                            ,IRD.MaterialStorageId,uom.Id  ,uom.Id ,IRD.BaseUoMFactor ,uom.UserName 
                            ) AS t	where  t.PostingQty<>0 		
                            Group By --t.Id,
                            t.JobWorkTransformationContractChildMasterId
                            ,t.InputMaterialId,t.MaterialMasterId,t.MaterialMaster,t.InputMaterialCode,t.ArticleName,t.ArticleId ,t.MMUnit
                            ,t.RequiredQuantity,t.BalanceToIssue,t.TIRCTotalQty,t.PlannedQty,t.IssuedQty,t.BalanceQty,t.MaterialStorageId,t.TransactionUoMid,t.BaseUoMid,t.BaseUoMFactor,t.TransactionUoM  
                            ,t.TotalQty 
                            ,t.PostingQty 
                            ,t.PostingQty                             
                            ,t.UnApprovedQty 
                            ,t.BaseUoMFactor, t.ApprovedQty";

                }
                else
                {
                    sql = @"select mi.Id,mi.JobWorkTransformationContractChildMasterId, jwi.UserName as JWOutputItem,jwii.UserName as JWInputItem
                            ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode, uom.UserName as MMUnit
                            ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                            ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
                            --,SUM(tirc.Quantity) as TIRCQty
                            ,Sum(kk.TotalQuantity) as TIRCTotalQty
                            ,0 PlannedQty,0 IssuedQty,0 BalanceQty
                            ,0 TotalQty
							,0 PostingQty
							,0 PostingQuantity
							,0 ApprovedQty
							,0 UnApprovedQty,null MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
                             from dbo.JobWorkTransformationContractChild3 mi
                             --left join dbo.JobWorkTransformationIssueReturnChild tirc on tirc.MaterialInputId=mi.Id
							 left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
							 left join MST.MaterialMaster mm on mm.Id=jwii.MaterialMasterId
							 left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                             left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
							 left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                             left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
                             on iid.InventoryIssueId=II.Id group by II.JWContractId) kk on kk.JWContractId=mp.JobWorkTransformationContractMasterId
                             where mi.JobWorkTransformationContractChildMasterId IN (" + MPId + @")
							 group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.JobWorkTransformationContractChildMasterId,jwi.UserName,jwii.UserName,uom.UserName,mm.Code  ";

                }

               

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetIssuedDetailList(string ArticleId, string MaterialId, string MaterialInputId, string ContractId)
        {
            try
            {
                string sql = @"select mi.Id,mi.JobWorkTransformationContractChildMasterId, jwi.UserName as JWOutputItem,jwii.UserName as JWInputItem
                            ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode, uom.UserName as MMUnit
                            ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
							 ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(KK.TotalIssuedQty,'0'))
                            ,Sum(KK.TotalIssuedQty) as TIRCTotalQty
							,null MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
                             from dbo.JobWorkTransformationContractChild3 mi
							 left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
							 left join MST.MaterialMaster mm on mm.Id=jwii.MaterialMasterId
							 left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                             left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
							 left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                             left join (select Sum(IID.TransactionQty) as TotalIssuedQty,IID.InventoryMaterialId, IM.MaterialMasterId,IM.ArticleId from TRN.InventoryIssue II inner join TRN.InventoryIssueDetail IID on II.Id=IID.InventoryIssueId
                                        left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
                                        left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
                                        left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
										where II.JWContractId='"+ ContractId + @"'
										group by IID.InventoryMaterialId,IM.MaterialMasterId,IM.ArticleId)
										KK on KK.MaterialMasterId=mm.Id
							 where mp.JobWorkTransformationContractMasterId='"+ ContractId + @"' and 
							 KK.MaterialMasterId='"+ MaterialId + @"' and KK.ArticleId='"+ ArticleId + @"' and mi.Id='"+ MaterialInputId + @"'
							 group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,KK.TotalIssuedQty,mi.JobWorkTransformationContractChildMasterId,jwi.UserName,jwii.UserName,uom.UserName,mm.Code   ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetLotNoRate(string LotNumber)
        {
            try
            {
                string sql = @"select IRD.Id, IRD.MaterialTranRate, IRD.InventoryMaterialId, IM.MaterialMasterId, IM.ArticleId from trn.InventoryReceiveDetail IRD
                               left join trn.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                               where IRD.LotNo='" + LotNumber + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> getentitylist()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id as Value, UserName as Text from ORG.Entity where PlantId='" + identity.PlantId + "' order by UserName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> gejobworklocation(string TId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select MS.Id as Value, JL.LocationName as Text, MS.UserName as StorageLocation 
                               from HKP.JobWorkLocation JL
                               left join dbo.JobWorkTransformationContractChild mp on mp.MaterialLocationId=JL.Id
							   left join HKP.MaterialStorage MS on MS.Id=JL.StoreLocationId
                               where mp.JobWorkTransformationContractMasterId='" + TId + @"' order by JL.LocationName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> getalljobworklocation()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id as Value, LocationName as Text
                               from HKP.JobWorkLocation order by LocationName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> getStoragloc(string JLId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select MS.Id as Value, JL.LocationName as Text, MS.UserName as StorageLocation 
                               from 
							   HKP.MaterialStorage MS left join HKP.JobWorkLocation JL on MS.Id=JL.StoreLocationId
                               where JL.Id='"+ JLId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetTransformationPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryIssue", out sID);
            return sID;
        }

        public void SaveIssueTransformation(Dictionary<string, object> data, string ContractId, string ContractType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where PositionCodeId='" + data["PositionCodeId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same Position Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from trn.InventoryIssue where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "IT" + GetTransformationPK();

                    dr["IssueDate"] = data["Date"];
                    dr["EmployeeId"] = data["EmployeeId"];
                    dr["Types"] = data["Types"];
                    dr["IssueType"] = data["IssueType"];
                    dr["MaterialStorageId"] = data["JobWorkLocationId"];
                    dr["IsConfirmed"] = data["IsConfirmed"];
                    dr["Remarks"] = data["Remarks"];
                    dr["EntityId"] = data["EntityId"];
                    dr["JWContractId"] = ContractId;
                    dr["ContractType"] = ContractType;

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = identity.PlantId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["IssueDate"] = data["Date"];
                    dr["EmployeeId"] = data["EmployeeId"];
                    dr["Types"] = data["Types"];
                    dr["IssueType"] = data["IssueType"];
                    dr["MaterialStorageId"] = data["JobWorkLocationId"];
                    dr["IsConfirmed"] = data["IsConfirmed"];
                    dr["Remarks"] = data["Remarks"];
                    dr["EntityId"] = data["EntityId"];
                    dr["JWContractId"] = ContractId;
                    dr["ContractType"] = ContractType;

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = identity.PlantId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }
                data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // New Changes

        public IEnumerable<object> GetCostCenterLoadNewFun(string EntityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Select CostCn.Id Value,CostCn.UserName Text from [ORG].[EntityCostCenter] EnCostCn
                LEFT JOIN [ORG].[CostCenter] AS CostCn ON CostCn.Id=EnCostCn.CostCenterId
                LEFT JOIN [ORG].[Entity] AS En ON En.Id=EnCostCn.EntityId
                WHERE En.Id='" + EntityId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetDataByInventoryIssue(string Id, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                var sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') issuetype
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.IssueDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
							,SUM(IIH.qty) Qty
							,SUM(Round(IIH.qty*IIH.Rate,2)) Amount
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							,C.Id CountryId,c.UserName CountryName,II.ContractId,II.ProductionOrderId,Con.ContractNo
                            ,II.Types, II.JWContractId
							FROM[TRN].[InventoryIssue] AS II
							left JOIN TRN.InventoryIssueDetail AS IID ON IID.InventoryIssueId= II.Id AND IID.IsAsset= 0
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							left join (Select InventoryIssueDetailId,IssueRequestDetailId,qty, Rate from trn.InventoryIssueHistory ) IIH ON IIH.InventoryIssueDetailId=IID.Id
							left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
							left JOIN SCS.Country c ON C.Id=IR.CountryId
							left join dbo.Contract Con On Con.Id=II.ContractId
						WHERE II.PlantId= '" + plantId + @"' AND ISNULL(II.[Status],'') <>'Posting' AND IID.IsAsset= 0 and II.Types='InventoryJWIssue' and II.JWContractId='"+ Id + @"'
						GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
						,II.IssueDate, MS.UserName
						,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
						,C.Id ,c.UserName ,II.ContractId ,II.ProductionOrderId,Con.ContractNo,II.Types, II.JWContractId
						Order BY II.IssueDate DESC";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        // VALUE ADDED NEW CODE

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryIssue", out sID);
            return sID;
        }

        public void Create(Dictionary<string, object> data, string ContractId, string ContractType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where PositionCodeId='" + data["PositionCodeId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same Position Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from trn.InventoryIssue where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "I" + GetPK();

                    dr["IssueDate"] = data["Date"];
                    dr["EmployeeId"] = data["EmployeeId"];
                    dr["Types"] = data["Types"];
                    dr["IssueType"] = data["IssueType"];
                    dr["MaterialStorageId"] = data["MaterialStorageId"];
                    dr["IsConfirmed"] = data["IsConfirmed"];
                    dr["Remarks"] = data["Remarks"];
                    dr["EntityId"] = data["EntityId"];
                    dr["JWContractId"] = ContractId;
                    dr["ContractType"] = ContractType;

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = identity.PlantId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["IssueDate"] = data["Date"];
                    dr["EmployeeId"] = data["EmployeeId"];
                    dr["Types"] = data["Types"];
                    dr["IssueType"] = data["IssueType"];
                    dr["MaterialStorageId"] = data["MaterialStorageId"];
                    dr["IsConfirmed"] = data["IsConfirmed"];
                    dr["Remarks"] = data["Remarks"];
                    dr["EntityId"] = data["EntityId"];
                    dr["JWContractId"] = ContractId;
                    dr["ContractType"] = ContractType;

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = identity.PlantId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }
                data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
public class MaterialPlanning
{

    #region Scalar Properties

    public string Id { get; set; }
    public string JobWorkItem { get; set; }
    public string MaterialType { get; set; }
    public string ArticleCode { get; set; }

    public string OutputUnit { get; set; }
    public string Quantity { get; set; }
    public string OrderSpecific { get; set; }
    public string MaterialLocation { get; set; }

    public string MaterialStorageIdInventory { get; set; } 
    

    #endregion Scalar Properties
}