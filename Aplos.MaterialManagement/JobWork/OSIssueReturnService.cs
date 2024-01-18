using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;
using System.Linq;
using Library.Data;
using Library.Service.Enums;
using Library.Service.Logs;
using System.Reflection;
using Library.ViewModel.Materials;
using Library.Model.Inventory;

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
        //            sql = @"select distinct NULL AS LotNumberList, mi.Id,mi.OSTransformationPODetailId, jwi.UserName as JWOutputItem ,mm.Id as MaterialMasterId, mm.UserName as Material
        //                    ,mma.Id as MaterialArticleId, mma.StandardName as Article, InvDetail.InventoryMaterialId
        //                    ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
        //                    ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
        //                    ,SUM(tirc.Quantity) as TIRCQty
        //                    ,(InvDetail.Rate) as Rate
        //                    ,Sum(kk.TotalQuantity) as TIRCTotalQty
        //                     from dbo.OSTransformationPOInputMaterial mi
        //                     left join dbo.JobWorkTransformationIssueReturnChild tirc on tirc.MaterialInputId=mi.Id
        //                     left join MST.MaterialMaster mm on mm.Id=mi.MaterialMasterId
        //                     left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
        //left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
        //                     left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
        //                     left join(select SUM(Quantity) as TotalQuantity,MaterialInputId FROM dbo.JobWorkTransformationIssueReturnChild group by MaterialInputId) kk on kk.MaterialInputId=mi.id
        //                     left join TRN.InventoryMaterial inm on inm.MaterialMasterId=mm.Id and inm.ArticleId=mma.Id
        //                     left join (Select InventoryMaterialId,(sum( MaterialTranAmount)/sum(TransactionQty)) as Rate from TRN.InventoryReceiveDetail group by InventoryMaterialId) InvDetail on InvDetail.InventoryMaterialId=inm.Id
        //                     where mi.OSTransformationPODetailId IN ("+ MPId + ") group by mi.Id, mm.Id, mm.UserName,InvDetail.Rate ,mma.Id, mma.StandardName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity, InvDetail.InventoryMaterialId,mi.OSTransformationPODetailId,jwi.UserName ";
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

        public IEnumerable<object> GetMaterialInputData(IEnumerable<MaterialPlanning> SelectedMaterialPlanningData, string OrderSpecific, string MaterialStorageIdInventory, string IssueDate, string TransIssueId)
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

                if (OrderSpecific == "Yes" && string.IsNullOrEmpty(TransIssueId))
                {

                    sql = @"select mi.Id,mi.Id OSTransformationPOInputMaterialId,
                            mi.OSTransformationPODetailId --mi.OSTransformationPODetailId OSTransformationPOId
,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId ,uom.UserName as MMUnit
,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
,kk.TotalQuantity as TIRCTotalQty
,Sum(0) PlannedQty,0 IssuedQty,0 BalanceQty
,null MaterialStorageId ,uom.Id as TransactionUoMId,uom.Id as BaseUoMId, uom.UserName as TransactionUoM
,Isnull(ab.TotalQty,0)+ISNULL(TIRD.TransferBaseQty,0) TotalQty, Isnull(cd.PostingQty,0)+ISNULL(TIRD.TransferBaseQty,0) PostingQty, Isnull(ef.ApprovedQty,0) ApprovedQty, Isnull(gh.UnApprovedQty,0) UnApprovedQty
,Isnull(cd.PostingQty,0) PostingQuantity--,IRD.BaseUoMFactor

from dbo.OSTransformationPOInputMaterial mi
left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
left join TRN.InventoryMaterial IM on  IM.ArticleId=mi.ArticleId
left join TRN.InventoryReceiveDetail IRD on IRD.InventoryMaterialId=IM.Id
LEFT JOIN (SELECT TIRD.InventoryReceiveId,TIRD.InventoryMaterialId,sum(ISNULL(TIRD.BaseQty,0)) TransferBaseQty FROM TRN.InventoryReceiveDetail TIRD 
										LEFT JOIN TRN.InventoryReceive TIR ON TIR.Id=TIRD.InventoryReceiveId
										WHERE  TIR.[Status] IS NULL AND TIR.IsApproved=1 AND TIR.RequiredPosting=0 AND TIR.GRNType='MaterialTransfer'
										GROUP BY TIRD.InventoryReceiveId,TIRD.InventoryMaterialId
										) TIRD ON TIRD.InventoryMaterialId=IM.Id 
left join(select iid.InventoryMaterialId, SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId,iid.OSTransformationPOId 
			FROM TRN.InventoryIssueDetail iid 
			left join TRN.InventoryIssue II on iid.InventoryIssueId=II.Id 
			left join TRN.InventoryMaterial IM ON IM.Id=iid.InventoryMaterialId
			where iid.OSTransformationPOId in (" + MPId + @")
			group by II.JWContractId,iid.InventoryMaterialId, iid.OSTransformationPOId
			
) kk on kk.JWContractId=mp.OSTransformationPOId  and kk.InventoryMaterialId=Im.Id

Left join(select mi.Id,mi.OSTransformationPODetailId
,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId, uom.UserName as MMUnit
,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
,Sum(kk.TotalQuantity) as TIRCTotalQty
,0 PlannedQty,0 IssuedQty,0 BalanceQty
,0 PostingQuantity
,null MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
,TotalQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
from dbo.OSTransformationPOInputMaterial mi
left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
on iid.InventoryIssueId=II.Id group by II.JWContractId
) kk on kk.JWContractId=mp.OSTransformationPOId
left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=mm.Id AND IM.ArticleId=mi.ArticleId
left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id

WHERE CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE) AND IR.IsApproved=0
AND mi.OSTransformationPODetailId IN (" + MPId + @")
AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"' AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.OSTransformationPODetailId
,uom.UserName,mm.Code,mma.StandardName ,mma.Id

 )ab on ab.MaterialMasterId=mma.MaterialMasterId and 
 ab.ArticleId=mi.ArticleId

 Left JOIN (select mi.Id,mi.OSTransformationPODetailId
                        ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode, mma.StandardName ArticleName,mma.Id ArticleId,uom.UserName as MMUnit
                        ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                        ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
                        ,Sum(kk.TotalQuantity) as TIRCTotalQty
                        ,0 PlannedQty,0 IssuedQty,0 BalanceQty
                        ,0 PostingQuantity
                        ,null MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
                        ,0 TotalQty,  PostingQty =(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 ApprovedQty, 0 UnApprovedQty
                        from dbo.OSTransformationPOInputMaterial mi
                        left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
						left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                        left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                        left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
                        left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
		                        on iid.InventoryIssueId=II.Id group by II.JWContractId
                        ) kk on kk.JWContractId=mp.OSTransformationPOId
                        left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=mm.Id AND IM.ArticleId=mi.ArticleId
                        left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                        left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id


                        WHERE  IR.IsApproved=1
						 AND CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE)    
                         AND mi.OSTransformationPODetailId IN  (" + MPId + @") AND IR.Status='Posting'
                         AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'  
                        group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.OSTransformationPODetailId
						,uom.UserName,mm.Code,mma.StandardName ,mma.Id   
)cd on  cd.MaterialMasterId=mma.MaterialMasterId and 
cd.ArticleId=mi.ArticleId

Left join (select mi.Id,mi.OSTransformationPODetailId
            ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId, uom.UserName as MMUnit
            ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
            ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
            ,Sum(kk.TotalQuantity) as TIRCTotalQty
            ,0 PlannedQty,0 IssuedQty,0 BalanceQty
            ,0 PostingQuantity
            ,null MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
            ,0TotalQty, 0 PostingQty,  ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 UnApprovedQty
            from dbo.OSTransformationPOInputMaterial mi
            left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
			left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
            left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
            left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
            left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
		                        on iid.InventoryIssueId=II.Id group by II.JWContractId
                        ) kk on kk.JWContractId=mp.OSTransformationPOId
            left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=mm.Id AND IM.ArticleId=mi.ArticleId
            left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
            left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
            LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
            left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
            left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
            left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id


            WHERE  IR.IsApproved=1 and IR.Status is null
			    AND CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE)    
                AND mi.OSTransformationPODetailId IN  (" + MPId + @") 
                AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'
               group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.OSTransformationPODetailId
			   ,uom.UserName,mm.Code,mma.StandardName ,mma.Id   
            ---End Of Approved
)ef ON ef.MaterialMasterId=mma.MaterialMasterId and 
ef.ArticleId=mi.ArticleId

left JOIn(
                        select mi.Id,mi.OSTransformationPODetailId
                        ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId, uom.UserName as MMUnit
                        ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                        ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
                        ,Sum(kk.TotalQuantity) as TIRCTotalQty
                        ,0 PlannedQty,0 IssuedQty,0 BalanceQty
                        ,0 PostingQuantity
                        ,null MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
                        ,0 TotalQty, 0 PostingQty, 0 ApprovedQty,  UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))))
                        from dbo.OSTransformationPOInputMaterial mi
                        left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
						left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                        left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                        left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
                        left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
		                        on iid.InventoryIssueId=II.Id group by II.JWContractId
                        ) kk on kk.JWContractId=mp.OSTransformationPOId
                        left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=mm.Id AND IM.ArticleId=mi.ArticleId
                        left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                        left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id


                         WHERE  IR.IsApproved=0 --and IR.Status is null
                         AND CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE)    AND mi.OSTransformationPODetailId IN  (" + MPId + @") 
                         AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
                        group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.OSTransformationPODetailId
						,uom.UserName,mm.Code,mma.StandardName ,mma.Id   
                        )gh on gh.MaterialMasterId=mma.MaterialMasterId and 
						gh.ArticleId=mi.ArticleId

where mi.OSTransformationPODetailId IN (" + MPId + @")
group by uom.Id --,mi.Id
, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity
,mi.Id,mi.OSTransformationPODetailId
,uom.UserName,mm.Code ,mma.StandardName ,mma.Id
,ab.TotalQty,cd.PostingQty,ef.ApprovedQty,gh.UnApprovedQty,TIRD.TransferBaseQty--,IRD.BaseUoMFactor
";

                }
                else if (string.IsNullOrEmpty(TransIssueId))
                {

                    sql = @"SELECT mi.Id,mi.Id OSTransformationPOInputMaterialId,mi.OSTransformationPODetailId
                    , jwi.UserName as JWOutputItem,jwii.Id as JWInputItemId,jwii.UserName as JWInputItem
                    , mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId, uom.UserName as MMUnit
                    , RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                    , BalanceToIssue=case when mi.ArticleId is not null then (mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity ,'0')+ISNULL(IPD.IssuedQty,'0')) else (mp.Quantity * mi.GrossConsumption)-(ISNULL(BB.TotalQty+IPD.IssuedQty,'0')) End
                    , TIRCTotalQty=case when mi.ArticleId is not null then kk.TotalQuantity else BB.TotalQty End
                    , Sum(0) PlannedQty,ISNULL(kk.TotalQuantity,0)+IPD.IssuedQty IssuedQty,0 BalanceQty,JWL.StoreLocationId MaterialStorageId
                    , TransactionUoMId=case when mi.ArticleId is not null then uom.Id else uomm.Id End
                    , BaseUoMId=case when mi.ArticleId is not null then uom.Id else uomm.Id End
                    , TransactionUoM=case when mi.ArticleId is not null then uom.UserName else uomm.UserName End
                    , 0 TotalQty,0 ApprovedQty,0 UnApprovedQty,isSelectedMatInput = Convert(bit, 'True')
                    
                    ,isnull(PQ.PostingQty,0) PostingQty
                    FROM dbo.OSTransformationPOInputMaterial mi
                    LEFT JOIN HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
                    LEFT JOIN MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
                    LEFT JOIN MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                    LEFT JOIN scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                    LEFT JOIN scs.UnitOfMeasurement uomm on uomm.Id=jwii.UOMId
                    LEFT JOIN dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
                    LEFT JOIN HKP.JobWorkLocation JWL ON JWL.Id=mp.MaterialLocationId
                    LEFT JOIN dbo.OSTransformationPO OSPO on OSPO.Id=mp.OSTransformationPOId
                    LEFT JOIN HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                    LEFT JOIN trn.InventoryMaterial IM ON IM.MaterialMasterId=mma.MaterialMasterId and IM.ArticleId=mi.ArticleId
					LEFT JOIN (SELECT OSTransformationPOInputMaterialId,SUM(Qty) IssuedQty FROM [TRN].[IssueProcessDetail] group by OSTransformationPOInputMaterialId) IPD ON IPD.OSTransformationPOInputMaterialId=mi.Id
                    LEFT JOIN (SELECT iid.InventoryMaterialId, SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId, iid.OSTransformationPOId
                    			FROM TRN.InventoryIssueDetail iid 
                    			LEFT JOIN TRN.InventoryIssue II on iid.InventoryIssueId=II.Id 
                    			WHERE iid.OSTransformationPOId in (" + MPId + @")
                    			GROUP BY II.JWContractId,iid.InventoryMaterialId, iid.OSTransformationPOId
                    			) kk on kk.JWContractId=mp.OSTransformationPOId and kk.InventoryMaterialId=Im.Id
                    LEFT JOIN(SELECT SUM(iid.TransactionQty) as TotalQty, II.JWContractId, iid.OSTransformationPOId,iid.JWTCInputId
                    			FROM TRN.InventoryIssueDetail iid 
                    			LEFT JOIN TRN.InventoryIssue II on iid.InventoryIssueId=II.Id 
                    			WHERE iid.OSTransformationPOId in (" + MPId + @")
                    			GROUP BY II.JWContractId, iid.OSTransformationPOId,iid.JWTCInputId
                    			) BB on BB.JWContractId=mp.OSTransformationPOId and BB.OSTransformationPOId=mp.Id and BB.JWTCInputId=mi.JobWorkItemId
                    LEFT JOIN ((select IM.MaterialMasterId,IM.ArticleId,IM.PlantId,IRD.MaterialStorageId,PostingQty=isnull((((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IIH.Qty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))),0)
                                        FROM [TRN].[InventoryReceiveDetail] AS IRD
                                        LEFT JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                                        LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                    					LEFT JOIN (SELECT IH.MaterialStorageId,IM.MaterialMasterId,IM.ArticleId,IM.FirstCharacteristicsValueId,IM.SecondCharacteristicsValueId
                    								,sum(isnull(IH.Qty,0)) Qty FROM  TRN.InventoryIssueHistory IH 
                    								LEFT JOIN TRN.InventoryIssueDetail IID ON IID.Id=IH.InventoryIssueDetailId 
                    								LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IID.InventoryMaterialId 
                    								LEFT JOIN TRN.InventoryIssue II ON II.Id=IID.InventoryIssueId 
                    								GROUP BY IH.MaterialStorageId,IM.MaterialMasterId,IM.ArticleId,IM.FirstCharacteristicsValueId,IM.SecondCharacteristicsValueId
                    								) IIH ON  IIH.MaterialStorageId=IRD.MaterialStorageId AND IIH.MaterialMasterId=IM.MaterialMasterId AND IIH.ArticleId=IM.ArticleId
                                        WHERE    IR.[Status]='Posting' AND IR.IsFOC=0   
                    					GROUP BY IM.MaterialMasterId,IM.ArticleId,IM.PlantId,IRD.MaterialStorageId)
                    ) PQ ON   PQ.ArticleId=IM.ArticleId AND PQ.MaterialStorageId=JWL.StoreLocationId
                    
                    WHERE mi.OSTransformationPODetailId IN (" + MPId + @")
                    GROUP BY  uom.Id ,mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,IPD.IssuedQty
                    ,mi.Id,mi.OSTransformationPODetailId,jwi.UserName,jwii.UserName,uom.UserName,mm.Code,mma.StandardName,mma.Id
                    ,jwii.Id,mi.ArticleId,uomm.Id,uomm.UserName,BB.TotalQty ,JWL.StoreLocationId,OSPO.PlantId,mp.MaterialMasterId,mp.ArticleId,PQ.PostingQty
 ";
                }

                else if (OrderSpecific == "Yes" && !string.IsNullOrEmpty(TransIssueId))
                {
                    sql = @"select --mi.Id,
                           IID.Id as InventoryIssueDetailId, II.Id as IssueId
						   ,IID.TransactionQty,IID.CostCenterId 
                           ,mi.OSTransformationPODetailId --mi.OSTransformationPODetailId OSTransformationPOId
,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId ,uom.UserName as MMUnit
,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
--,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0') - isnull(IID.TransactionQty,'0'))
--,kk.TotalQuantity as TIRCTotalQty 
,TIRCTotalQty= isnull(kk.TotalQuantity,'0') - isnull(IID.TransactionQty,'0')
,Sum(0) PlannedQty,0 IssuedQty,0 BalanceQty
,null MaterialStorageId ,uom.Id as TransactionUoMId,uom.Id as BaseUoMId, uom.UserName as TransactionUoM
,Isnull(ab.TotalQty,0) TotalQty, Isnull(cd.PostingQty,0) PostingQty, Isnull(ef.ApprovedQty,0) ApprovedQty, Isnull(gh.UnApprovedQty,0) UnApprovedQty
,Isnull(cd.PostingQty,0) PostingQuantity--,IRD.BaseUoMFactor

from dbo.OSTransformationPOInputMaterial mi
left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
left join TRN.InventoryMaterial IM on  IM.ArticleId=mi.ArticleId
left join TRN.InventoryReceiveDetail IRD on IRD.InventoryMaterialId=IM.Id
left join(select iid.InventoryMaterialId, SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId,iid.OSTransformationPOId 
			FROM TRN.InventoryIssueDetail iid 
			left join TRN.InventoryIssue II on iid.InventoryIssueId=II.Id 
			left join TRN.InventoryMaterial IM ON IM.Id=iid.InventoryMaterialId
			where iid.OSTransformationPOId in (" + MPId + @")
			group by II.JWContractId,iid.InventoryMaterialId, iid.OSTransformationPOId
			
) kk on kk.JWContractId=mp.OSTransformationPOId  and kk.InventoryMaterialId=Im.Id
left join TRN.InventoryIssue II on II.JWContractId=mp.OSTransformationPOId 
left join TRN.InventoryIssueDetail IID on IID.InventoryIssueId=II.Id and IID.OSTransformationPOId=mp.Id and IID.InventoryMaterialId=IM.Id
left join ORG.CostCenter CC on CC.Id=IID.CostCenterId

Left join(select mi.Id,mi.OSTransformationPODetailId
,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId, uom.UserName as MMUnit
,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
,Sum(kk.TotalQuantity) as TIRCTotalQty
,0 PlannedQty,0 IssuedQty,0 BalanceQty
,0 PostingQuantity
,null MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
,TotalQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
from dbo.OSTransformationPOInputMaterial mi
left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
on iid.InventoryIssueId=II.Id group by II.JWContractId
) kk on kk.JWContractId=mp.OSTransformationPOId
left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=mm.Id AND IM.ArticleId=mi.ArticleId
left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id

WHERE CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE) AND IR.IsApproved=0
AND mi.OSTransformationPODetailId IN (" + MPId + @")
AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"' AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.OSTransformationPODetailId
,uom.UserName,mm.Code,mma.StandardName ,mma.Id

 )ab on ab.MaterialMasterId=mma.MaterialMasterId and 
 ab.ArticleId=mi.ArticleId

 Left JOIN (select mi.Id,mi.OSTransformationPODetailId
                        ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode, mma.StandardName ArticleName,mma.Id ArticleId,uom.UserName as MMUnit
                        ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                        ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
                        ,Sum(kk.TotalQuantity) as TIRCTotalQty
                        ,0 PlannedQty,0 IssuedQty,0 BalanceQty
                        ,0 PostingQuantity
                        ,null MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
                        ,0 TotalQty,  PostingQty =(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 ApprovedQty, 0 UnApprovedQty
                        from dbo.OSTransformationPOInputMaterial mi
                        left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
						left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                        left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                        left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
                        left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
		                        on iid.InventoryIssueId=II.Id group by II.JWContractId
                        ) kk on kk.JWContractId=mp.OSTransformationPOId
                        left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=mm.Id AND IM.ArticleId=mi.ArticleId
                        left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                        left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id


                        WHERE  IR.IsApproved=1
						 AND CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE)    
                         AND mi.OSTransformationPODetailId IN  (" + MPId + @") AND IR.Status='Posting'
                         AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'  
                        group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.OSTransformationPODetailId
						,uom.UserName,mm.Code,mma.StandardName ,mma.Id   
)cd on  cd.MaterialMasterId=mma.MaterialMasterId and 
cd.ArticleId=mi.ArticleId

Left join (select mi.Id,mi.OSTransformationPODetailId
            ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId, uom.UserName as MMUnit
            ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
            ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
            ,Sum(kk.TotalQuantity) as TIRCTotalQty
            ,0 PlannedQty,0 IssuedQty,0 BalanceQty
            ,0 PostingQuantity
            ,null MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
            ,0TotalQty, 0 PostingQty,  ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 UnApprovedQty
            from dbo.OSTransformationPOInputMaterial mi
            left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
			left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
            left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
            left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
            left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
		                        on iid.InventoryIssueId=II.Id group by II.JWContractId
                        ) kk on kk.JWContractId=mp.OSTransformationPOId
            left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=mm.Id AND IM.ArticleId=mi.ArticleId
            left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
            left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
            LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
            left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
            left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
            left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id


            WHERE  IR.IsApproved=1 and IR.Status is null
			    AND CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE)    
                AND mi.OSTransformationPODetailId IN  (" + MPId + @") 
                AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'
               group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.OSTransformationPODetailId
			   ,uom.UserName,mm.Code,mma.StandardName ,mma.Id   
            ---End Of Approved
)ef ON ef.MaterialMasterId=mma.MaterialMasterId and 
ef.ArticleId=mi.ArticleId

left JOIn(
                        select mi.Id,mi.OSTransformationPODetailId
                        ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId, uom.UserName as MMUnit
                        ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                        ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
                        ,Sum(kk.TotalQuantity) as TIRCTotalQty
                        ,0 PlannedQty,0 IssuedQty,0 BalanceQty
                        ,0 PostingQuantity
                        ,null MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
                        ,0 TotalQty, 0 PostingQty, 0 ApprovedQty,  UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))))
                        from dbo.OSTransformationPOInputMaterial mi
                        left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
						left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                        left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                        left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
                        left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
		                        on iid.InventoryIssueId=II.Id group by II.JWContractId
                        ) kk on kk.JWContractId=mp.OSTransformationPOId
                        left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=mm.Id AND IM.ArticleId=mi.ArticleId
                        left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                        left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id


                         WHERE  IR.IsApproved=0 --and IR.Status is null
                         AND CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE)    AND mi.OSTransformationPODetailId IN  (" + MPId + @") 
                         AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
                        group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.OSTransformationPODetailId
						,uom.UserName,mm.Code,mma.StandardName ,mma.Id   
                        )gh on gh.MaterialMasterId=mma.MaterialMasterId and 
						gh.ArticleId=mi.ArticleId

where mi.OSTransformationPODetailId IN (" + MPId + @") and IID.InventoryIssueId='" + TransIssueId + @"'
group by uom.Id --,mi.Id
, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity
,mi.OSTransformationPODetailId
,uom.UserName,mm.Code ,mma.StandardName ,mma.Id
,ab.TotalQty,cd.PostingQty,ef.ApprovedQty,gh.UnApprovedQty--,IRD.BaseUoMFactor
,IID.Id, II.Id,IID.TransactionQty,IID.CostCenterId";
                }

                else
                {
                    sql = @"select IID.Id as InventoryIssueDetailId, II.Id as IssueId
						   ,IID.TransactionQty,IID.CostCenterId, mi.OSTransformationPODetailId --mi.OSTransformationPODetailId OSTransformationPOId
, jwi.UserName as JWOutputItem,jwii.Id as JWInputItemId,jwii.UserName as JWInputItem
,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId, uom.UserName as MMUnit
,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
--,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
--,BalToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(BB.TotalQty,'0'))
--,BalanceToIssue=case when mi.ArticleId is not null then (mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0')) else (mp.Quantity * mi.GrossConsumption)-(ISNULL(BB.TotalQty,'0')) End
,BalanceToIssue=case when mi.ArticleId is not null then (mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0') - isnull(IID.TransactionQty,'0')) else (mp.Quantity * mi.GrossConsumption)-(ISNULL(BB.TotalQty,'0') - isnull(IID.TransactionQty,'0')) End
--,kk.TotalQuantity as TIRCTotalQty
--,BB.TotalQty as TotalIssuedQuantity
--,TIRCTotalQty=case when mi.ArticleId is not null then kk.TotalQuantity else BB.TotalQty End
,TIRCTotalQty=case when mi.ArticleId is not null then (isnull(kk.TotalQuantity,'0') - isnull(IID.TransactionQty,'0')) else (isnull(BB.TotalQty,'0') - isnull(IID.TransactionQty,'0')) End
,Sum(0) PlannedQty,0 IssuedQty,0 BalanceQty
,0 PostingQuantity
,null MaterialStorageId--,uom.Id as TransactionUoMId
,TransactionUoMId=case when mi.ArticleId is not null then uom.Id else uomm.Id End
--,uom.Id as BaseUoMId
,BaseUoMId=case when mi.ArticleId is not null then uom.Id else uomm.Id End
--,uom.UserName as TransactionUoM
,TransactionUoM=case when mi.ArticleId is not null then uom.UserName else uomm.UserName End
,Isnull(ab.TotalQty,0) TotalQty, Isnull(cd.PostingQty,0) PostingQty, Isnull(ef.ApprovedQty,0) ApprovedQty, Isnull(gh.UnApprovedQty,0) UnApprovedQty
from dbo.OSTransformationPOInputMaterial mi
left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
left join MST.MaterialMaster mm on mm.Id=jwii.MaterialMasterId
left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
left join scs.UnitOfMeasurement uomm on uomm.Id=jwii.UOMId
left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
left join trn.InventoryMaterial IM ON IM.MaterialMasterId=jwii.MaterialMasterId and IM.ArticleId=mi.ArticleId
left join trn.InventoryReceiveDetail IRD ON IRD.InventoryMaterialId=IM.Id
left join(select iid.InventoryMaterialId, SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId, iid.OSTransformationPOId
			FROM TRN.InventoryIssueDetail iid 
			left join TRN.InventoryIssue II on iid.InventoryIssueId=II.Id 
			where iid.OSTransformationPOId in (" + MPId + @")
			group by II.JWContractId,iid.InventoryMaterialId, iid.OSTransformationPOId
			) kk on kk.JWContractId=mp.OSTransformationPOId and kk.InventoryMaterialId=Im.Id
left join(select SUM(iid.TransactionQty) as TotalQty, II.JWContractId, iid.OSTransformationPOId,iid.JWTCInputId
			FROM TRN.InventoryIssueDetail iid 
			left join TRN.InventoryIssue II on iid.InventoryIssueId=II.Id 
			where iid.OSTransformationPOId in (" + MPId + @")
			group by II.JWContractId, iid.OSTransformationPOId,iid.JWTCInputId
			) BB on BB.JWContractId=mp.OSTransformationPOId and BB.OSTransformationPOId=mp.Id and BB.JWTCInputId=mi.JobWorkItemId

left join TRN.InventoryIssue II on II.JWContractId=mp.OSTransformationPOId 
left join TRN.InventoryIssueDetail IID on IID.InventoryIssueId=II.Id and IID.OSTransformationPOId=mp.Id and IID.InventoryMaterialId=IM.Id
left join ORG.CostCenter CC on CC.Id=IID.CostCenterId

Left join(select mi.Id,mi.OSTransformationPODetailId, jwi.UserName as JWOutputItem,jwii.Id as JWInputItemId,jwii.UserName as JWInputItem
        ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId, uom.UserName as MMUnit
        ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
        --,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
--,BalToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(BB.TotalQty,'0'))
,BalanceToIssue=case when mi.ArticleId is not null then (mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0')) else (mp.Quantity * mi.GrossConsumption)-(ISNULL(BB.TotalQty,'0')) End
--,kk.TotalQuantity as TIRCTotalQty
--,BB.TotalQty as TotalIssuedQuantity
,TIRCTotalQty=case when mi.ArticleId is not null then kk.TotalQuantity else BB.TotalQty End
        ,0 PlannedQty,0 IssuedQty,0 BalanceQty
        ,0 PostingQuantity
        ,null MaterialStorageId--,uom.Id as TransactionUoMId
,TransactionUoMId=case when mi.ArticleId is not null then uom.Id else uomm.Id End
--,uom.Id as BaseUoMId
,BaseUoMId=case when mi.ArticleId is not null then uom.Id else uomm.Id End
--,uom.UserName as TransactionUoM
        ,TotalQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
        from dbo.OSTransformationPOInputMaterial mi
        left join  HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
        left join MST.MaterialMaster mm on mm.Id=jwii.MaterialMasterId

        left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
        left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
        left join scs.UnitOfMeasurement uomm on uomm.Id=jwii.UOMId
        left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
        left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
        left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
		        on iid.InventoryIssueId=II.Id group by II.JWContractId
        ) kk on kk.JWContractId=mp.OSTransformationPOId

left join(select SUM(iid.TransactionQty) as TotalQty, II.JWContractId, iid.OSTransformationPOId,iid.JWTCInputId
			FROM TRN.InventoryIssueDetail iid 
			left join TRN.InventoryIssue II on iid.InventoryIssueId=II.Id 
			where iid.OSTransformationPOId in (" + MPId + @")
			group by II.JWContractId, iid.OSTransformationPOId,iid.JWTCInputId
			) BB on BB.JWContractId=mp.OSTransformationPOId and BB.OSTransformationPOId=mp.Id and BB.JWTCInputId=mi.JobWorkItemId

        left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=mm.Id AND IM.ArticleId=mi.ArticleId
        left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
        left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
        LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
        left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
        left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id


        WHERE CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE) AND  IR.IsApproved=0
            AND mi.OSTransformationPODetailId IN  (" + MPId + @") 
            AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'  
        group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.OSTransformationPODetailId,jwi.UserName
                    ,jwii.UserName,uom.UserName,mm.Code,mma.StandardName ,mma.Id,jwii.Id,mi.ArticleId,uomm.Id,uomm.UserName,BB.TotalQty

		)ab on ab.MaterialMasterId=jwii.MaterialMasterId and ab.ArticleId=mi.ArticleId

Left JOIN (select mi.Id,mi.OSTransformationPODetailId, jwi.UserName as JWOutputItem,jwii.Id as JWInputItemId,jwii.UserName as JWInputItem
                        ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode, mma.StandardName ArticleName,mma.Id ArticleId,uom.UserName as MMUnit
                        ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                        --,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
--,BalToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(BB.TotalQty,'0'))
,BalanceToIssue=case when mi.ArticleId is not null then (mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0')) else (mp.Quantity * mi.GrossConsumption)-(ISNULL(BB.TotalQty,'0')) End
--,kk.TotalQuantity as TIRCTotalQty
--,BB.TotalQty as TotalIssuedQuantity
,TIRCTotalQty=case when mi.ArticleId is not null then kk.TotalQuantity else BB.TotalQty End
                        ,0 PlannedQty,0 IssuedQty,0 BalanceQty
                        ,0 PostingQuantity
                        ,null MaterialStorageId--,uom.Id as TransactionUoMId
,TransactionUoMId=case when mi.ArticleId is not null then uom.Id else uomm.Id End
--,uom.Id as BaseUoMId
,BaseUoMId=case when mi.ArticleId is not null then uom.Id else uomm.Id End
                        ,0 TotalQty,  PostingQty =(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 ApprovedQty, 0 UnApprovedQty
                        from dbo.OSTransformationPOInputMaterial mi
                        left join  HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
                        left join MST.MaterialMaster mm on mm.Id=jwii.MaterialMasterId

                        left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
                        left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                        left join scs.UnitOfMeasurement uomm on uomm.Id=jwii.UOMId
                        left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
                        left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                        left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
		                        on iid.InventoryIssueId=II.Id group by II.JWContractId
                        ) kk on kk.JWContractId=mp.OSTransformationPOId

left join(select SUM(iid.TransactionQty) as TotalQty, II.JWContractId, iid.OSTransformationPOId,iid.JWTCInputId
			FROM TRN.InventoryIssueDetail iid 
			left join TRN.InventoryIssue II on iid.InventoryIssueId=II.Id 
			where iid.OSTransformationPOId in (" + MPId + @")
			group by II.JWContractId, iid.OSTransformationPOId,iid.JWTCInputId
			) BB on BB.JWContractId=mp.OSTransformationPOId and BB.OSTransformationPOId=mp.Id and BB.JWTCInputId=mi.JobWorkItemId

                        left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=mm.Id AND IM.ArticleId=mi.ArticleId
                        left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                        left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id


                        WHERE  IR.IsApproved=1
						 AND CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE)    
                         AND mi.OSTransformationPODetailId IN  (" + MPId + @") AND IR.Status='Posting'
                         AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
                        group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.OSTransformationPODetailId
,jwi.UserName,jwii.UserName,uom.UserName,mm.Code,mma.StandardName ,mma.Id,jwii.Id,mi.ArticleId,uomm.Id,uomm.UserName,BB.TotalQty
)cd on  cd.MaterialMasterId=jwii.MaterialMasterId and cd.ArticleId=mi.ArticleId

Left join (select mi.Id,mi.OSTransformationPODetailId, jwi.UserName as JWOutputItem,jwii.Id as JWInputItemId,jwii.UserName as JWInputItem
            ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId, uom.UserName as MMUnit
            ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
            --,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
--,BalToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(BB.TotalQty,'0'))
,BalanceToIssue=case when mi.ArticleId is not null then (mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0')) else (mp.Quantity * mi.GrossConsumption)-(ISNULL(BB.TotalQty,'0')) End
--,kk.TotalQuantity as TIRCTotalQty
--,BB.TotalQty as TotalIssuedQuantity
,TIRCTotalQty=case when mi.ArticleId is not null then kk.TotalQuantity else BB.TotalQty End
            ,0 PlannedQty,0 IssuedQty,0 BalanceQty
            ,0 PostingQuantity
            ,null MaterialStorageId--,uom.Id as TransactionUoMId
,TransactionUoMId=case when mi.ArticleId is not null then uom.Id else uomm.Id End
--,uom.Id as BaseUoMId
,BaseUoMId=case when mi.ArticleId is not null then uom.Id else uomm.Id End
            ,0TotalQty, 0 PostingQty,  ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 UnApprovedQty
            from dbo.OSTransformationPOInputMaterial mi
            left join  HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
            left join MST.MaterialMaster mm on mm.Id=jwii.MaterialMasterId

            left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
            left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
left join scs.UnitOfMeasurement uomm on uomm.Id=jwii.UOMId
            left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
            left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
            left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
		            on iid.InventoryIssueId=II.Id group by II.JWContractId
            ) kk on kk.JWContractId=mp.OSTransformationPOId
left join(select SUM(iid.TransactionQty) as TotalQty, II.JWContractId, iid.OSTransformationPOId,iid.JWTCInputId
			FROM TRN.InventoryIssueDetail iid 
			left join TRN.InventoryIssue II on iid.InventoryIssueId=II.Id 
			where iid.OSTransformationPOId in (" + MPId + @")
			group by II.JWContractId, iid.OSTransformationPOId,iid.JWTCInputId
			) BB on BB.JWContractId=mp.OSTransformationPOId and BB.OSTransformationPOId=mp.Id and BB.JWTCInputId=mi.JobWorkItemId

            left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=mm.Id AND IM.ArticleId=mi.ArticleId
            left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
            left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
            LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
            left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
            left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
            left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id


            WHERE  IR.IsApproved=1 and IR.Status is null
			    AND CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE)    
                AND mi.OSTransformationPODetailId IN  (" + MPId + @")  
                AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
               group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.OSTransformationPODetailId
,jwi.UserName,jwii.UserName,uom.UserName,mm.Code,mma.StandardName ,mma.Id,jwii.Id,mi.ArticleId,uomm.Id,uomm.UserName,BB.TotalQty
            ---End Of Approved
)ef ON ef.MaterialMasterId=jwii.MaterialMasterId and ef.ArticleId=mi.ArticleId

left JOIn(
                        select mi.Id,mi.OSTransformationPODetailId, jwi.UserName as JWOutputItem,jwii.Id as JWInputItemId,jwii.UserName as JWInputItem
                        ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode,mma.StandardName ArticleName,mma.Id ArticleId, uom.UserName as MMUnit
                        ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                        --,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
--,BalToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(BB.TotalQty,'0'))
,BalanceToIssue=case when mi.ArticleId is not null then (mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0')) else (mp.Quantity * mi.GrossConsumption)-(ISNULL(BB.TotalQty,'0')) End
--,kk.TotalQuantity as TIRCTotalQty
--,BB.TotalQty as TotalIssuedQuantity
,TIRCTotalQty=case when mi.ArticleId is not null then kk.TotalQuantity else BB.TotalQty End
                        ,0 PlannedQty,0 IssuedQty,0 BalanceQty
                        ,0 PostingQuantity
                        ,null MaterialStorageId--,uom.Id as TransactionUoMId
,TransactionUoMId=case when mi.ArticleId is not null then uom.Id else uomm.Id End
--,uom.Id as BaseUoMId
,BaseUoMId=case when mi.ArticleId is not null then uom.Id else uomm.Id End
                        ,0 TotalQty, 0 PostingQty, 0 ApprovedQty,  UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))))
                        from dbo.OSTransformationPOInputMaterial mi
                        left join  HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
                        left join MST.MaterialMaster mm on mm.Id=jwii.MaterialMasterId

                        left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
                        left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                        left join scs.UnitOfMeasurement uomm on uomm.Id=jwii.UOMId
                        left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
                        left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                        left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid left join TRN.InventoryIssue II
		                        on iid.InventoryIssueId=II.Id group by II.JWContractId
                        ) kk on kk.JWContractId=mp.OSTransformationPOId

left join(select SUM(iid.TransactionQty) as TotalQty, II.JWContractId, iid.OSTransformationPOId,iid.JWTCInputId
			FROM TRN.InventoryIssueDetail iid 
			left join TRN.InventoryIssue II on iid.InventoryIssueId=II.Id 
			where iid.OSTransformationPOId in (" + MPId + @")
			group by II.JWContractId, iid.OSTransformationPOId,iid.JWTCInputId
			) BB on BB.JWContractId=mp.OSTransformationPOId and BB.OSTransformationPOId=mp.Id and BB.JWTCInputId=mi.JobWorkItemId

                        left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=mm.Id AND IM.ArticleId=mi.ArticleId
                        left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
                        left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id


                         WHERE  IR.IsApproved=0 and IR.Status is null
                         AND CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE)    AND mi.OSTransformationPODetailId IN  (" + MPId + @")
                         AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
                        group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.OSTransformationPODetailId
,jwi.UserName,jwii.UserName,uom.UserName,mm.Code,mma.StandardName ,mma.Id,jwii.Id,mi.ArticleId,uomm.Id,uomm.UserName,BB.TotalQty
                        )gh on gh.MaterialMasterId=jwii.MaterialMasterId and gh.ArticleId=mi.ArticleId

where mi.OSTransformationPODetailId IN (" + MPId + @") and IID.InventoryIssueId='" + TransIssueId + @"'
group by ab.MaterialStorageId,gh.UnApprovedQty,ef.ApprovedQty,cd.PostingQty,ab.TotalQty,uom.Id ,mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity
,mi.OSTransformationPODetailId,jwi.UserName,jwii.UserName,uom.UserName,mm.Code,mma.StandardName,mma.Id
,jwii.Id,mi.ArticleId,uomm.Id,uomm.UserName,BB.TotalQty
,IID.Id, II.Id,IID.TransactionQty,IID.CostCenterId";
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
                string sql = @"select mi.Id,mi.OSTransformationPODetailId, jwi.UserName as JWOutputItem,jwii.UserName as JWInputItem
                            ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode, uom.UserName as MMUnit
                            ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
							 ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(KK.TotalIssuedQty,'0'))
                            ,Sum(KK.TotalIssuedQty) as TIRCTotalQty
							,null MaterialStorageId,uom.Id as TransactionUoMid,uom.Id as BaseUoMid
                             from dbo.OSTransformationPOInputMaterial mi
							 left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
							 left join MST.MaterialMaster mm on mm.Id=jwii.MaterialMasterId
							 left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                             left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
							 left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                             left join (select Sum(IID.TransactionQty) as TotalIssuedQty,IID.InventoryMaterialId, IM.MaterialMasterId,IM.ArticleId from TRN.InventoryIssue II inner join TRN.InventoryIssueDetail IID on II.Id=IID.InventoryIssueId
                                        left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
                                        left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
                                        left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
										where II.JWContractId='" + ContractId + @"'
										group by IID.InventoryMaterialId,IM.MaterialMasterId,IM.ArticleId)
										KK on KK.MaterialMasterId=mm.Id
							 where mp.OSTransformationPOId='" + ContractId + @"' and 
							 KK.MaterialMasterId='" + MaterialId + @"' and KK.ArticleId='" + ArticleId + @"' and mi.Id='" + MaterialInputId + @"'
							 group by uom.Id ,mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,KK.TotalIssuedQty,mi.OSTransformationPODetailId,jwi.UserName,jwii.UserName,uom.UserName,mm.Code   ";

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
                               left join dbo.OSTransformationPODetail mp on mp.MaterialLocationId=JL.Id
							   left join HKP.MaterialStorage MS on MS.Id=JL.StoreLocationId
                               where mp.OSTransformationPOId='" + TId + @"' order by JL.LocationName ";

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
                               where JL.Id='" + JLId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> JWDetailsData()
        {
            try
            {
                string sql = @"select II.Id,IID.OSTransformationPOId,IID.InventoryMaterialId,IM.MaterialMasterId,mm.UserName as MaterialName, IM.ArticleId,mma.StandardName as Article,Tuom.UserName as TransactionUoM
                                ,IID.TransactionUoMId--,IID.TransactionQty
                                ,IIH.Qty as TransactionQty,IM.FirstCharacteristicsId
                                ,FC.UserName AS FirstChaName,IM.FirstCharacteristicsValueId,FCV.UserName AS SKU1
                                ,IM.SecondCharacteristicsId,SC.UserName AS SecondChaName,IM.SecondCharacteristicsValueId,SCV.UserName AS SKU2
                                ,IM.ThirdCharacteristicsId,TC.UserName AS ThirdChaName,IM.ThirdCharacteristicsValueId,TCV.UserName AS SKU3
                                ,c.Code as BaseCurrency,BaseRate=round((IRD.MaterialTranRate * IR.ToCurrencyRate),4)
								,Amount=isnull(round((IRD.MaterialTranRate * IR.ToCurrencyRate) * isnull(IIH.Qty,'0'),2),'0')
                                from TRN.InventoryIssue II left join TRN.InventoryIssueDetail IID on IID.InventoryIssueId=II.Id
                                left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
                                left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
                                left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
                                LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
                                LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
                                LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
                                LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
                                LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
                                LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
                                left join SCS.UnitOfMeasurement Tuom on Tuom.Id=IID.TransactionUoMId
                                left join SCS.Currency C on C.Id=II.CurrencyId
								left join TRN.InventoryIssueHistory IIH on IIH.InventoryIssueDetailId=IID.Id
								left join TRN.InventoryReceiveDetail IRD on IRD.Id=IIH.InventoryReceiveDetailId
        						left join TRN.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId
                                where II.Types='InventoryJWIssue'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetGRNRowId(string InventoryIssueDetailId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id,InventoryIssueDetailId,InventoryReceiveDetailId,Qty from TRN.InventoryIssueHistory 
                               where InventoryIssueDetailId='" + InventoryIssueDetailId + @"' ";

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

        public void SaveIssueTransformation(Dictionary<string, object> data, string ContractId, string ContractType, IEnumerable<JobWorkTransformationIssueReturnChild> SelectedQuantityData)
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
                    dr["Id"] = GetTransformationPK();

                    dr["IssueDate"] = data["IssueDate"];
                    dr["EmployeeId"] = data["EmployeeId"];
                    dr["Types"] = data["Types"];
                    dr["IssueType"] = data["IssueType"];
                    dr["MaterialStorageId"] = data["MaterialStorageIdInventory"];
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
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                SaveIssueTransformationChild(SelectedQuantityData, MasterId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetTransformationChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryIssueDetail", out sID);
            return sID;
        }

        public void SaveIssueTransformationChild(IEnumerable<JobWorkTransformationIssueReturnChild> SelectedQuantityData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var JWItemId = "' '";
                var OtMatId = "' '";

                foreach (var empitem in SelectedQuantityData)
                {
                    JWItemId += ",'" + empitem.JWInputItemId + "' ";
                    OtMatId += ",'" + empitem.OSTransformationPOId + "' ";

                }
                con.OpenDataSetThroughAdapter("select * from TRN.InventoryIssueDetail where OSTransformationPOId IN ( " + OtMatId + ") and JWTCInputId IN (" + JWItemId + ") and InventoryIssueId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in SelectedQuantityData)
                {

                    ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPOId='" + item.OSTransformationPOId + "' and JWTCInputId='" + item.JWInputItemId + "' ";

                    if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        //        dr["Id"] = "TC" + GetTransformationChildPK();
                        dr["Id"] = GetTransformationChildPK();

                        dr["InventoryIssueId"] = MasterId;

                        //  dr["MaterialInputId"] = item.Id;
                        //        dr["MaterialMasterId"] = item.InputMaterialId;
                        dr["TransactionQty"] = item.TransactionQty;
                        dr["TransactionUoMId"] = item.TransactionUoMId;
                        dr["BaseUOMId"] = item.BaseUoMId;
                        dr["CostCenterId"] = item.CostCenterId;
                        dr["OSTransformationPOId"] = item.OSTransformationPOId;
                        dr["JWTCInputId"] = item.JWInputItemId;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        //dr["UpdatedBy"] = identity.Name;
                        //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        //dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPOId='" + item.OSTransformationPOId + "' and JWTCInputId='" + item.JWInputItemId + "' ";

                        if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow dr = ExistOrNot.Tables[0].NewRow();
                            dr["Id"] = GetTransformationChildPK();

                            dr["InventoryIssueId"] = MasterId;
                            dr["TransactionQty"] = item.TransactionQty;
                            dr["TransactionUoMId"] = item.TransactionUoMId;
                            dr["BaseUOMId"] = item.BaseUoMId;
                            dr["CostCenterId"] = item.CostCenterId;
                            dr["OSTransformationPOId"] = item.OSTransformationPOId;
                            dr["JWTCInputId"] = item.JWInputItemId;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;

                            ExistOrNot.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            //edit
                            DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["InventoryIssueId"] = MasterId;
                            dr["TransactionQty"] = item.TransactionQty;
                            dr["TransactionUoMId"] = item.TransactionUoMId;
                            dr["BaseUOMId"] = item.BaseUoMId;
                            dr["CostCenterId"] = item.CostCenterId;
                            dr["OSTransformationPOId"] = item.OSTransformationPOId;
                            dr["JWTCInputId"] = item.JWInputItemId;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;


                            dr.EndEdit();
                        }


                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

                //         return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // New Changes

        //public void SaveProcessIssueTransformation(Dictionary<string, object> data, string ContractId, string ContractType, IEnumerable<IssueProcessWIP> SelectedQuantityData)
        private string GetIssueProcessDetailPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "IssueProcessDetail", out sID);
            return sID;
        }
        public void SaveProcessIssueTransformation(IEnumerable<IssueProcessDetail> issueProcessDetaillist, IEnumerable<IssueProcessWIP> specificStockList, InventoryIssue inventoryIssue, string IssueTypeStatus, IEnumerable<InventoryMaterialViewModel> entitiesAll)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from trn.InventoryIssue where Id='" + inventoryIssue.Id + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = GetTransformationPK();

                    dr["IssueDate"] = inventoryIssue.IssueDate;
                    dr["EmployeeId"] = inventoryIssue.EmployeeId;
                    dr["Types"] = inventoryIssue.Types;
                    dr["IssueType"] = inventoryIssue.IssueType;
                    dr["MaterialStorageId"] = inventoryIssue.MaterialStorageId;
                    //dr["IsConfirmed"] = inventoryIssue.IsConfirmed;
                    dr["Remarks"] = inventoryIssue.Remarks;
                    dr["EntityId"] = inventoryIssue.EntityId;
                    dr["JWContractId"] = inventoryIssue.JWContractId;
                    dr["ContractType"] = inventoryIssue.ContractType;
                    dr["IssueCategory"] = inventoryIssue.IssueCategory;

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

                    dr["IssueDate"] = inventoryIssue.IssueDate;
                    dr["EmployeeId"] = inventoryIssue.EmployeeId;
                    dr["Types"] = inventoryIssue.Types;
                    dr["IssueType"] = inventoryIssue.IssueType;
                    dr["MaterialStorageId"] = inventoryIssue.MaterialStorageId;
                    //dr["IsConfirmed"] = inventoryIssue.IsConfirmed;
                    dr["Remarks"] = inventoryIssue.Remarks;
                    dr["EntityId"] = inventoryIssue.EntityId;
                    dr["JWContractId"] = inventoryIssue.JWContractId;
                    dr["ContractType"] = inventoryIssue.ContractType;
                    dr["IssueCategory"] = inventoryIssue.IssueCategory;

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
                inventoryIssue.Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #endregion data update

                DataSet ExistOrNot;
                DataSet dsIssueProWIP = null;
                DataSet dsItemScanChild = null;
                string tempIssueProDetailId = null;

                con.OpenDataSetThroughAdapter("select * from TRN.IssueProcessDetail where InventoryIssueId='" + dsMaster.Tables[0].Rows[0]["Id"].ToString() + "'  ", out ExistOrNot, false, "1");


                foreach (var item in issueProcessDetaillist)
                {
                    ExistOrNot.Tables[0].DefaultView.RowFilter = "InventoryIssueId='" + dsMaster.Tables[0].Rows[0]["Id"].ToString() + "' ";

                    if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = GetIssueProcessDetailPK();

                        dr["InventoryIssueId"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                        dr["OSTransformationPOInputMaterialId"] = item.OSTransformationPOInputMaterialId;
                        dr["MaterialMasterId"] = item.MaterialMasterId;
                        dr["ArticleId"] = item.ArticleId;
                        dr["Qty"] = item.TransactionQty;
                        dr["Source"] = item.Source;
                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                       
                        ExistOrNot.Tables[0].Rows.Add(dr);
                        tempIssueProDetailId = dr["Id"].ToString();
                    }

                    else
                    {
                        //edit
                        DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["InventoryIssueId"] = dsMaster.Tables[0].Rows[0]["Id"].ToString(); 
                        dr["OSTransformationPOInputMaterialId"] = item.OSTransformationPOInputMaterialId;
                        dr["MaterialMasterId"] = item.MaterialMasterId;
                        dr["ArticleId"] = item.ArticleId;
                        dr["Qty"] = item.Qty;
                        dr["Source"] = item.Source;
                        dr["Remarks"] = item.Remarks;

                        dr["Remarks"] = item.Remarks;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;


                        dr.EndEdit();
                    }
                    con.OpenDataSetThroughAdapter("select * from TRN.IssueProcessWIP where 1=2", out dsIssueProWIP, false, "1");
                    foreach (var itemwip in specificStockList.Where(r => r.MaterialMasterId == item.MaterialMasterId && r.ArticleId == item.ArticleId))
                    {

                        DataRow drwip = dsIssueProWIP.Tables[0].NewRow();
                        drwip["Id"] = GetIssueProcessDetailPK();

                        drwip["IssueProcessDetailId"] = tempIssueProDetailId;
                        drwip["ProductionSummaryId"] = itemwip.ProductionSummaryId;
                        drwip["ProductionOrderId"] = itemwip.ProductionOrderId;
                        drwip["LotNumber"] = itemwip.LotNumber;
                        drwip["Qty"] = itemwip.RequisitionQty;
                        drwip["Source"] = itemwip.Source;
                        drwip["Remarks"] = itemwip.Remarks;
                        drwip["AddedBy"] = identity.Name;
                        drwip["AddedDate"] = System.DateTime.Now.ToString();
                        drwip["AddedFromIP"] = identity.IPAddress;

                        dsIssueProWIP.Tables[0].Rows.Add(drwip);
                        if (itemwip.Source == "Scan")
                        {
                            con.OpenDataSetThroughAdapter("select * from itemscanchild where ProductionSummaryId='" + itemwip.ProductionSummaryId + "'", out dsItemScanChild, false, "1");
                            if (dsItemScanChild.Tables[0].Rows.Count > 0)
                            {
                                for (int s = 0; s < dsItemScanChild.Tables[0].Rows.Count; s++)
                                {
                                    DataView dv = new DataView(dsItemScanChild.Tables[0]);
                                    dv.RowFilter = "Id='" + dsItemScanChild.Tables[0].Rows[s]["Id"] + "'";

                                    if (dv.Count > 0)
                                    {
                                        DataRow drmo = dv[0].Row;
                                        drmo.BeginEdit();

                                        drmo["IsDispatch"] = true;

                                        drmo.EndEdit();

                                    }
                                }
                            }
                        }
                        
                        
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster,ExistOrNot, dsIssueProWIP, dsItemScanChild);
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveIssueProcessWIPTransformationChild(IEnumerable<IssueProcessDetail> SelectedQuantityData, string MasterId, IEnumerable<IssueProcessWIP> specificStockList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;
                DataSet dsIssueProWIP=null;
                string tempIssueProDetailId=null;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from TRN.IssueProcessDetail where InventoryIssueId='" + MasterId + "'  ", out ExistOrNot, false, "1");
                

                foreach (var item in SelectedQuantityData)
                {
                    ExistOrNot.Tables[0].DefaultView.RowFilter = "InventoryIssueId='" + MasterId + "' ";

                    if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = GetIssueProcessDetailPK();

                        dr["InventoryIssueId"] = MasterId;
                        dr["OSTransformationPOInputMaterialId"] = item.OSTransformationPOInputMaterialId;
                        dr["MaterialMasterId"] = item.MaterialMasterId;
                        dr["ArticleId"] = item.ArticleId;
                        dr["Qty"] = item.TransactionQty;
                        dr["Source"] = item.Source;
                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        //dr["UpdatedBy"] = identity.Name;
                        //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        //dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);
                        tempIssueProDetailId = dr["Id"].ToString();
                    }

                    else
                    {
                        //edit
                        DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["InventoryIssueId"] = MasterId;
                        dr["OSTransformationPOInputMaterialId"] = item.OSTransformationPOInputMaterialId;
                        dr["MaterialMasterId"] = item.MaterialMasterId;
                        dr["ArticleId"] = item.ArticleId;
                        dr["Qty"] = item.Qty;
                        dr["Source"] = item.Source;
                        dr["Remarks"] = item.Remarks;

                        dr["Remarks"] = item.Remarks;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;


                        dr.EndEdit();
                    }
                    con.OpenDataSetThroughAdapter("select * from TRN.IssueProcessWIP where 1=2", out dsIssueProWIP, false, "1");
                    foreach (var itemwip in specificStockList.Where(r=>r.MaterialMasterId==item.MaterialMasterId && r.ArticleId== item.ArticleId))
                    {
                       
                        DataRow drwip = dsIssueProWIP.Tables[0].NewRow();
                        drwip["Id"] = GetIssueProcessDetailPK();

                        drwip["IssueProcessDetailId"] = tempIssueProDetailId;
                        drwip["ProductionSummaryId"] = itemwip.ProductionSummaryId;
                        drwip["ProductionOrderId"] = itemwip.ProductionOrderId;
                        drwip["LotNumber"] = itemwip.LotNumber;
                        drwip["Qty"] = itemwip.RequisitionQty;
                        drwip["Source"] = itemwip.Source;
                        drwip["Remarks"] = itemwip.Remarks;
                        drwip["AddedBy"] = identity.Name;
                        drwip["AddedDate"] = System.DateTime.Now.ToString();
                        drwip["AddedFromIP"] = identity.IPAddress;

                        dsIssueProWIP.Tables[0].Rows.Add(drwip);

                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot, dsIssueProWIP);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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

        public IEnumerable<object> GetDataByInventoryIssue(string Id, string GRNbyPOCheckStatus, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                if (GRNbyPOCheckStatus == "ForChecked")
                {
                    sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') IssueType
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.IssueDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName
                            ,EI.EmployeeName as ResponsiblePerson,EI.EmployeeCode,II.EmployeeId,II.RefferenceNo
							,SUM(IIH.qty) Qty
							--,SUM(Round(IIH.qty*IIH.Rate,2)) Amount
							,Amount=Sum(IIH.TotalMaterialBooksCurrencyAmount)
							,II.Remarks--,II.Id AS IssueId
							,II.OrderRefNo
							--,C.Id CountryId,c.UserName CountryName
							,II.ContractId,II.ProductionOrderId,Con.ContractNo
                            ,II.Types, II.JWContractId,Tuom.UserName as TransactionUoM
							FROM[TRN].[InventoryIssue] AS II
							left JOIN TRN.InventoryIssueDetail AS IID ON IID.InventoryIssueId= II.Id  AND ISNULL(IID.IsAsset,0)= 0 
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							--left join (Select InventoryIssueDetailId,IssueRequestDetailId,qty, Rate from trn.InventoryIssueHistory ) 
							--IIH ON IIH.InventoryIssueDetailId=IID.Id
						--	left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
						--	left JOIN SCS.Country c ON C.Id=IR.CountryId
							left join dbo.Contract Con On Con.Id=II.ContractId
                            left join SCS.UnitOfMeasurement Tuom on Tuom.Id=IID.TransactionUoMId

							left join TRN.InventoryIssueHistory IIH on IIH.InventoryIssueDetailId=IID.Id
							left join TRN.InventoryReceiveDetail IRD on IRD.Id=IIH.InventoryReceiveDetailId
        					left join TRN.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId
						WHERE II.PlantId= '" + plantId + @"' AND ISNULL(II.[Status],'') <>'Posting'  AND ISNULL(IID.IsAsset,0)= 0  and II.Types='InventoryOSIssue' and II.JWContractId='" + Id + @"'
						GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
						,II.IssueDate, MS.UserName
						,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
						--,C.Id ,c.UserName 
                        ,II.ContractId 
                        ,II.ProductionOrderId,Con.ContractNo,II.Types, II.JWContractId,Tuom.UserName
                        ,II.EmployeeId,II.RefferenceNo
						Order BY II.IssueDate DESC";
                }

                if (GRNbyPOCheckStatus == "Posted")
                {
                    sql = @"SELECT E.UserName AS Entity 
							,isnull(II.IssueType,'') IssueType
							, II.Id, II.CompanyGroupId
							, II.CompanyId, II.PlantId
							, II.EntityId, II.MaterialStorageId
							,FORMAT(II.IssueDate, 'dd-MMM-yyyy') IssueDate
							, MS.UserName AS MaterialStorage 
							,EI.EmployeeCode + ' - ' + EI.EmployeeName EmployeeName 
							,SUM(IIH.qty) Qty
							--,SUM(Round(IIH.qty*IIH.Rate,2)) Amount
							,Amount=isnull(round( Sum((IRD.MaterialTranRate * IR.ToCurrencyRate) * isnull(IIH.Qty,'0')),2),'0')
							,II.Remarks,II.Id AS IssueId
							,II.OrderRefNo
							--,C.Id CountryId,c.UserName CountryName
							,II.ContractId,II.ProductionOrderId,Con.ContractNo
                            ,II.Types, II.JWContractId,Tuom.UserName as TransactionUoM
							FROM[TRN].[InventoryIssue] AS II
							left JOIN TRN.InventoryIssueDetail AS IID ON IID.InventoryIssueId= II.Id  AND ISNULL(IID.IsAsset,0)= 0 
							left JOIN[HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
							left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
							Left JOIN [ORG].[Entity] E On E.id= II.EntityId
							--left join (Select InventoryIssueDetailId,IssueRequestDetailId,qty, Rate from trn.InventoryIssueHistory ) 
							--IIH ON IIH.InventoryIssueDetailId=IID.Id
						--	left join trn.IssueRequest IR On IR.Id=IIH.IssueRequestDetailId
						--	left JOIN SCS.Country c ON C.Id=IR.CountryId
							left join dbo.Contract Con On Con.Id=II.ContractId
                            left join SCS.UnitOfMeasurement Tuom on Tuom.Id=IID.TransactionUoMId

							left join TRN.InventoryIssueHistory IIH on IIH.InventoryIssueDetailId=IID.Id
							left join TRN.InventoryReceiveDetail IRD on IRD.Id=IIH.InventoryReceiveDetailId
        					left join TRN.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId
						WHERE II.PlantId= '" + plantId + @"' AND ISNULL(II.[Status],'')='Posting'  AND ISNULL(IID.IsAsset,0)= 0  and II.Types='InventoryOSIssue' and II.JWContractId='" + Id + @"'
						GROUP BY II.Id, II.CompanyGroupId, II.CompanyId, II.PlantId, II.EntityId, II.MaterialStorageId
						,II.IssueDate, MS.UserName
						,EI.EmployeeCode,EI.EmployeeName,II.IssueType,E.UserName,II.Remarks,II.Id,II.OrderRefNo  
						--,C.Id ,c.UserName 
                        ,II.ContractId ,II.ProductionOrderId,Con.ContractNo,II.Types, II.JWContractId,Tuom.UserName
						Order BY II.IssueDate DESC";
                }


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

        // Edit mode

        public IEnumerable<object> GetOSOutPutInventoryMaterialList(string IssueId, string PKId, string IssueDate, string MaterialStorageIdInventory)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select IID.Id as InventoryIssueDetailId, II.Id as IssueId,IID.TransactionQty,IID.CostCenterId,IID.Comments, vcc.Id as OSTransformationPODetailId,vcc.OSTransformationPOId,vcc.MaterialMasterId,vcc.ArticleId,vcc.Quantity as VCCQuantity, jwi.UserName as JWOutputItem,jwa.UserName as JobWorkActivity
                                , uom.UserName as OutputUnit,OMM.UserName as MaterialMaster, mma.StandardName as ArticleName
							   , c.Code as Currency, emp.EmployeeName as ResponsiblePerson
							   , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,moi.BuyerReferenceNo,moi.OwnReferenceNo,mo.BuyerReferenceNo BuyerOrderNo,mo.OwnReferenceNo AS OwnOrderNo
	                            , SO.Id AS SalesOrderId, Pr.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
								,PM.UserName AS ProductName
								,CN.ContractNo,MLC.LCRef MasterLCNo, owrUom.UserName as MasterOrderUoM
                               ,owr.Id as JWOrderWiseId, owr.OSTransformationPODetailId, owr.OrderType,owr.Quantity as OWRQuantity,owr.PlanQuantity
                               ,IssueActive='Active'
							    ,RequiredQuantity=case when owr.Id is not null then owr.Quantity else vcc.Quantity End
							  	--   ,BalanceToIssue=case when owr.Id is not null then (owr.Quantity)-(ISNULL(OW.TotalQuantity,'0')) else (vcc.Quantity)-(ISNULL(kk.TotalQuantity,'0')) End
							   ,BalanceToIssue=case when owr.Id is not null then (owr.Quantity)-(ISNULL(OW.TotalQuantity,'0')) else (vcc.Quantity)-((ISNULL(kk.TotalQuantity,'0')) - isnull(IID.TransactionQty,'0')) End
							--	 ,TIRCTotalQty=case when owr.Id is not null then ISNULL(OW.TotalQuantity,'0') else ISNULL(kk.TotalQuantity,'0') End
								 ,TIRCTotalQty=case when owr.Id is not null then ISNULL(OW.TotalQuantity,'0') else ISNULL(kk.TotalQuantity,'0') - isnull(IID.TransactionQty,'0') End
								,Sum(0) PlannedQty,0 IssuedQty,0 BalanceQty
                                ,0 PostingQuantity
                               ,null MaterialStorageId,uom.Id as TransactionUoMId,uom.Id as BaseUoMId,uom.UserName as TransactionUoM
							   ,Isnull(ab.TotalQty,0) TotalQty, Isnull(cd.PostingQty,0) PostingQty, Isnull(ef.ApprovedQty,0) ApprovedQty, Isnull(gh.UnApprovedQty,0) UnApprovedQty
                                ,vcc.FirstCharacteristicsId,vcc.FirstCharacteristicsValueId
						   ,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
						   ,vcc.SecondCharacteristicsId,vcc.SecondCharacteristicsValueId
                                ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
								,vcc.ThirdCharacteristicsId,vcc.ThirdCharacteristicsValueId
                                ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue                               
                                from TRN.InventoryIssue II left join dbo.OSTransformationPODetail vcc on II.JWContractId=vcc.OSTransformationPOId 
								left join TRN.InventoryIssueDetail IID on IID.InventoryIssueId=II.Id and IID.OSTransformationPOId=vcc.Id
                               left join HKP.JobWorkItem jwi on jwi.Id=vcc.JobWorkItemMasterId
							   left join hkp.JobWorkActivity jwa on jwa.Id=vcc.JobActivityId
        					   --left join SCS.UnitOfMeasurement uom on uom.Id=vcc.OutputMaterialUOMId
							   left join SCS.UnitOfMeasurement uom on uom.Id=vcc.TransactionUoMId
        					   left join MST.MaterialMasterArticle mma on mma.Id=vcc.ArticleId
							   left join MST.MaterialMaster OMM on OMM.Id=vcc.MaterialMasterId
        					   left join scs.Currency c on c.Id=vcc.CurrencyId
        					   left join dbo.EmployeeInformation emp on emp.SystemId=vcc.ResponsiblePersonId
							   left join dbo.OSTransformationPO vc on vc.Id=vcc.OSTransformationPOId
							   --	   left join dbo.OSTransformationPOMasterOrderItem owr on owr.OSTransformationPODetailId=vcc.Id
							   left join dbo.OSTransformationPOMasterOrderItem owr on owr.OSTransformationPODetailId=vcc.Id
							   left join [TRN].[SalesOrder] AS SO on SO.Id=owr.SalesOrderId
							   left JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
							   left JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
							   LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
							   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
							   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
							   LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
							   LEFT JOIN [HKP].[Party] AS Pr ON MO.PartyId = Pr.Id
							   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
							   LEFT JOIN dbo.[Contract] AS CN ON CN.Id=MOI.ContractId
							   LEFT JOIN dbo.MasterLC AS MLC ON MLC.Id=CN.MasterLCId
							   left join SCS.UnitOfMeasurement owrUom on owrUom.Id=MO.TotalQtyUOMId

                               LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = vcc.FirstCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = vcc.FirstCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = vcc.SecondCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = vcc.SecondCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = vcc.ThirdCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = vcc.ThirdCharacteristicsValueId

								 left join(select SUM(iid.TransactionQty) as TotalQuantity, II.JWContractId FROM TRN.InventoryIssueDetail iid 
								 left join TRN.InventoryIssue II on iid.InventoryIssueId=II.Id group by II.JWContractId
                                  ) kk on kk.JWContractId=vcc.OSTransformationPOId

		                        left join (Select SUM(TransactionQty) as TotalQuantity,OSTransformationPOId,JWOrderWiseId from TRN.InventoryIssueDetail 
								group by OSTransformationPOId,JWOrderWiseId) OW on OW.OSTransformationPOId=vcc.Id and OW.JWOrderWiseId=owr.Id

                               left join ORG.CostCenter CC on CC.Id=IID.CostCenterId
left join (select vcc.Id,vcc.OSTransformationPOId,vcc.MaterialMasterId,vcc.ArticleId,vcc.Quantity as VCCQuantity
                               ,IssueActive='Active'--,IM.Id as InventoryMaterialId
								 ,0 PlannedQty,0 IssuedQty,0 BalanceQty,0 PostingQuantity,null MaterialStorageId
								  ,TotalQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 PostingQty, 0 ApprovedQty, 0 UnApprovedQty
                               ,vcc.FirstCharacteristicsId,vcc.FirstCharacteristicsValueId
						   ,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
						   ,vcc.SecondCharacteristicsId,vcc.SecondCharacteristicsValueId
                                ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
								,vcc.ThirdCharacteristicsId,vcc.ThirdCharacteristicsValueId
                                ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
                               from dbo.OSTransformationPODetail vcc 
								 left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=vcc.MaterialMasterId AND IM.ArticleId=vcc.ArticleId
								 and isnull(IM.FirstCharacteristicsValueId,'')= isnull(vcc.FirstCharacteristicsValueId,'') 
								 and isnull(IM.SecondCharacteristicsValueId,'')= isnull(vcc.SecondCharacteristicsValueId,'')
								 and isnull(IM.ThirdCharacteristicsValueId,'')= isnull(vcc.ThirdCharacteristicsValueId,'')
								left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
								left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
								LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
								left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
								left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id

							   LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id=IM.FirstCharacteristicsId
								LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id=IM.FirstCharacteristicsValueId
								LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id=IM.SecondCharacteristicsId
								LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id=IM.SecondCharacteristicsValueId
								LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id=IM.ThirdCharacteristicsId
								LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id=IM.ThirdCharacteristicsValueId


        WHERE CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE) AND  IR.IsApproved=0
			AND vcc.OSTransformationPOId='" + PKId + @"'
            AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'
        group by
		vcc.Id,vcc.MaterialMasterId,vcc.ArticleId
		,vcc.Quantity
		,vcc.OSTransformationPOId
                                ,FChar.UserName,FCharValue.UserName,SChar.UserName,SCharValue.UserName ,TChar.UserName,TCharValue.UserName
						 ,vcc.FirstCharacteristicsId,vcc.FirstCharacteristicsValueId,vcc.SecondCharacteristicsId,vcc.SecondCharacteristicsValueId
						 ,vcc.ThirdCharacteristicsId,vcc.ThirdCharacteristicsValueId--,IM.Id
)ab on ab.MaterialMasterId=vcc.MaterialMasterId and ab.ArticleId=vcc.ArticleId

left join (select vcc.Id,vcc.OSTransformationPOId,vcc.MaterialMasterId,vcc.ArticleId,vcc.Quantity as VCCQuantity
                               ,IssueActive='Active'--,IM.Id as InventoryMaterialId
								 ,0 PlannedQty,0 IssuedQty,0 BalanceQty,0 PostingQuantity,null MaterialStorageId
								 ,0 TotalQty,  PostingQty =(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 ApprovedQty, 0 UnApprovedQty
                               ,vcc.FirstCharacteristicsId,vcc.FirstCharacteristicsValueId
						   ,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
						   ,vcc.SecondCharacteristicsId,vcc.SecondCharacteristicsValueId
                                ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
								,vcc.ThirdCharacteristicsId,vcc.ThirdCharacteristicsValueId
                                ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
                               from dbo.OSTransformationPODetail vcc
								 left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=vcc.MaterialMasterId AND IM.ArticleId=vcc.ArticleId 
								 and isnull(IM.FirstCharacteristicsValueId,'')= isnull(vcc.FirstCharacteristicsValueId,'') 
								 and isnull(IM.SecondCharacteristicsValueId,'')= isnull(vcc.SecondCharacteristicsValueId,'')
								 and isnull(IM.ThirdCharacteristicsValueId,'')= isnull(vcc.ThirdCharacteristicsValueId,'')
									left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
									left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
									LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
									left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
									left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
									left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id

		                    LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id=IM.FirstCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id=IM.FirstCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id=IM.SecondCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id=IM.SecondCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id=IM.ThirdCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id=IM.ThirdCharacteristicsValueId


       WHERE  IR.IsApproved=1
						 AND CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE)    
                         AND vcc.OSTransformationPOId='" + PKId + @"' AND IR.Status='Posting'
                         AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"' 
        group by
		vcc.Id,vcc.MaterialMasterId,vcc.ArticleId
        ,vcc.Quantity
		,vcc.OSTransformationPOId
                                 ,FChar.UserName,FCharValue.UserName,SChar.UserName,SCharValue.UserName ,TChar.UserName,TCharValue.UserName
						 ,vcc.FirstCharacteristicsId,vcc.FirstCharacteristicsValueId,vcc.SecondCharacteristicsId,vcc.SecondCharacteristicsValueId
						 ,vcc.ThirdCharacteristicsId,vcc.ThirdCharacteristicsValueId--,IM.Id
)cd on cd.MaterialMasterId=vcc.MaterialMasterId and cd.ArticleId=vcc.ArticleId

left join (select vcc.Id,vcc.OSTransformationPOId,vcc.MaterialMasterId,vcc.ArticleId,vcc.Quantity as VCCQuantity
                               ,IssueActive='Active'--,IM.Id as InventoryMaterialId
								 ,0 PlannedQty,0 IssuedQty,0 BalanceQty,0 PostingQuantity,null MaterialStorageId
								,0TotalQty, 0 PostingQty,  ApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0)))), 0 UnApprovedQty
                               ,vcc.FirstCharacteristicsId,vcc.FirstCharacteristicsValueId
						   ,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
						   ,vcc.SecondCharacteristicsId,vcc.SecondCharacteristicsValueId
                                ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
								,vcc.ThirdCharacteristicsId,vcc.ThirdCharacteristicsValueId
                                ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
                               from dbo.OSTransformationPODetail vcc 
								 left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=vcc.MaterialMasterId AND IM.ArticleId=vcc.ArticleId
								 and isnull(IM.FirstCharacteristicsValueId,'')= isnull(vcc.FirstCharacteristicsValueId,'') 
								 and isnull(IM.SecondCharacteristicsValueId,'')= isnull(vcc.SecondCharacteristicsValueId,'')
								 and isnull(IM.ThirdCharacteristicsValueId,'')= isnull(vcc.ThirdCharacteristicsValueId,'')
								left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
								left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
								LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
								left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
								left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id

							   LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id=IM.FirstCharacteristicsId
								LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id=IM.FirstCharacteristicsValueId
								LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id=IM.SecondCharacteristicsId
								LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id=IM.SecondCharacteristicsValueId
								LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id=IM.ThirdCharacteristicsId
								LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id=IM.ThirdCharacteristicsValueId


       WHERE  IR.IsApproved=1 and IR.Status is null
			    AND CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE)    
                AND vcc.OSTransformationPOId='" + PKId + @"'
                AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'
        group by
		vcc.Id,vcc.MaterialMasterId,vcc.ArticleId
		,vcc.Quantity
		,vcc.OSTransformationPOId
                                 ,FChar.UserName,FCharValue.UserName,SChar.UserName,SCharValue.UserName ,TChar.UserName,TCharValue.UserName
						 ,vcc.FirstCharacteristicsId,vcc.FirstCharacteristicsValueId,vcc.SecondCharacteristicsId,vcc.SecondCharacteristicsValueId
						 ,vcc.ThirdCharacteristicsId,vcc.ThirdCharacteristicsValueId--,IM.Id
)ef on ef.MaterialMasterId=vcc.MaterialMasterId and ef.ArticleId=vcc.ArticleId

left join (select vcc.Id,vcc.OSTransformationPOId,vcc.MaterialMasterId,vcc.ArticleId,vcc.Quantity as VCCQuantity
                               ,IssueActive='Active'--,IM.Id as InventoryMaterialId
								 ,0 PlannedQty,0 IssuedQty,0 BalanceQty,0 PostingQuantity,null MaterialStorageId
								,0 TotalQty, 0 PostingQty, 0 ApprovedQty,  UnApprovedQty=(((SUM(ISNULL(IRD.BaseQty,0)) - SUM(ISNULL(IRD.BaseIssueQty, 0))-SUM(ISNULL(IRD.PurchaseReturnQty, 0)))+SUM(ISNULL(IRD.IssueReturnQty, 0))-SUM(ISNULL(IRD.ReductionByAdjustmentQty, 0))-SUM(ISNULL(IRD.InventorySalesQty, 0))-SUM(ISNULL(IRD.InventoryScrapQty, 0))))
                               ,vcc.FirstCharacteristicsId,vcc.FirstCharacteristicsValueId
						   ,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
						   ,vcc.SecondCharacteristicsId,vcc.SecondCharacteristicsValueId
                                ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
								,vcc.ThirdCharacteristicsId,vcc.ThirdCharacteristicsValueId
                                ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
                               from dbo.OSTransformationPODetail vcc 
								 left JOIN [TRN].[InventoryMaterial] AS IM ON IM.MaterialMasterId=vcc.MaterialMasterId AND IM.ArticleId=vcc.ArticleId
								 and isnull(IM.FirstCharacteristicsValueId,'')= isnull(vcc.FirstCharacteristicsValueId,'') 
								 and isnull(IM.SecondCharacteristicsValueId,'')= isnull(vcc.SecondCharacteristicsValueId,'')
								 and isnull(IM.ThirdCharacteristicsValueId,'')= isnull(vcc.ThirdCharacteristicsValueId,'')
								left join [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
								left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
								LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
								left JOIN [SCS].[Currency] AS TCU ON IR.CurrencyId=TCU.Id
								left JOIN [SCS].[Currency] AS BCU ON IR.BaseCurrencyId=BCU.Id
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id

							   LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id=IM.FirstCharacteristicsId
								LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id=IM.FirstCharacteristicsValueId
								LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id=IM.SecondCharacteristicsId
								LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id=IM.SecondCharacteristicsValueId
								LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id=IM.ThirdCharacteristicsId
								LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id=IM.ThirdCharacteristicsValueId


      WHERE  IR.IsApproved=0 and IR.Status is null
                         AND CAST(IR.GRNDate AS DATE)<=CAST('" + IssueDate + @"' AS DATE)    
						 AND vcc.OSTransformationPOId='" + PKId + @"'
                         AND IRD.MaterialStorageId='" + MaterialStorageIdInventory + @"'  AND IM.CompanyGroupId='" + identity.CompanyGroupId + @"' AND IM.CompanyId='" + identity.CompanyId + @"' AND IM.PlantId='" + identity.PlantId + @"'
        group by
		vcc.Id,vcc.MaterialMasterId,vcc.ArticleId
		,vcc.Quantity
		,vcc.OSTransformationPOId
                                 ,FChar.UserName,FCharValue.UserName,SChar.UserName,SCharValue.UserName ,TChar.UserName,TCharValue.UserName
						 ,vcc.FirstCharacteristicsId,vcc.FirstCharacteristicsValueId,vcc.SecondCharacteristicsId,vcc.SecondCharacteristicsValueId
						 ,vcc.ThirdCharacteristicsId,vcc.ThirdCharacteristicsValueId--,IM.Id
)gh on gh.MaterialMasterId=vcc.MaterialMasterId and gh.ArticleId=vcc.ArticleId

where vcc.OSTransformationPOId='" + PKId + @"' and IID.InventoryIssueId='" + IssueId + @"'
group by ab.MaterialStorageId,gh.UnApprovedQty,ef.ApprovedQty,cd.PostingQty,ab.TotalQty,uom.Id ,mm.Id, mm.UserName,vcc.Quantity--,mi.GrossConsumption
,kk.TotalQuantity
,vcc.OSTransformationPOId,jwi.UserName
,uom.UserName,mm.Code,mma.StandardName,mma.Id
,vcc.Id,vcc.MaterialMasterId,vcc.ArticleId,jwa.UserName,OMM.UserName,c.Code,emp.EmployeeName
,owr.Id, owr.OSTransformationPODetailId, owr.OrderType,owr.Quantity,owr.PlanQuantity,Pr.UserName,mo.MasterOrderNo,owruom.UserName
	 , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,moi.BuyerReferenceNo,moi.OwnReferenceNo,mo.BuyerReferenceNo,mo.OwnReferenceNo
	                            , SO.Id, Pr.UserName,B.UserName,PM.Id,MOI.ProductionGrouping
								,PM.UserName
								,CN.ContractNo,MLC.LCRef, owrUom.UserName,OW.TotalQuantity
                                ,FChar.UserName,FCharValue.UserName,SChar.UserName,SCharValue.UserName ,TChar.UserName,TCharValue.UserName
						 ,vcc.FirstCharacteristicsId,vcc.FirstCharacteristicsValueId,vcc.SecondCharacteristicsId,vcc.SecondCharacteristicsValueId
						 ,vcc.ThirdCharacteristicsId,vcc.ThirdCharacteristicsValueId
                         ,II.Id,IID.TransactionQty,IID.CostCenterId,IID.Id,IID.Comments";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> ValAddedMaterialStorageForEdit(string IssueId, string MaterialStorageIdInventory)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select MS.Id as Value, JL.LocationName as Text, MS.UserName as StorageLocation
                                --,II.EmployeeId,emp.EmployeeName as ResponsiblePerson,emp.EmployeeCode
                                --,II.IssueType,II.OrderRefNo,II.RefferenceNo
                                from HKP.MaterialStorage MS left join HKP.JobWorkLocation JL on MS.Id=JL.StoreLocationId
                                left join TRN.InventoryIssue II on II.MaterialStorageId=MS.Id
                               -- left join dbo.EmployeeInformation emp on emp.SystemId=II.EmployeeId       
                                where JL.StoreLocationId='" + MaterialStorageIdInventory + @"' and II.Id='" + IssueId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetProductionSummaryProcess(string articleId)
        {
            try
            {
                string sql = @"SELECT MT.UserName MaterialType,PS.SourceType Source,mm.UserName MaterialMaster,mma.MaterialMasterId,mma.StandardName Article,moi.ArticleId,NULL SKU1,NULL SKU2,PS.ScanQty,PS.Quantity
						,PS.Quantity RequisitionQty,PS.LotNumber,PR.UserName Process,PS.Id ProductionSummaryId ,PS.ProductionOrderId,NULL Characteristics1ValueId,NULL Characteristics2ValueId
                        FROM TRN.ProductionSummary PS
	                    JOIN (select distinct ProductionSummaryId from itemscanchild where ProductionSummaryId<>'' ) isc ON isc.ProductionSummaryId=PS.Id
	                    LEFT JOIN [TRN].[ProductionOrderDetail] POD ON POD.ProductionOrderId=PS.ProductionOrderId
	                    LEFT JOIN [TRN].[SalesOrder] SO ON SO.Id=POD.SalesOrderId
	                    LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id=SO.MasterOrderItemId
	                    LEFT JOIN [MST].[MaterialMasterArticle] mma on mma.Id=MOI.ArticleId
	                    LEFT JOIN [MST].[MaterialMaster] mm on mm.Id=mma.MaterialMasterId
	                    LEFT JOIN [MST].[MaterialGroupMaster] MG on MG.Id=mm.MaterialGroupMasterId
	                    LEFT JOIN [HKP].[MaterialType] MT on MT.Id=MG.MaterialTypeId
	                    LEFT JOIN [HKP].[Process] pr ON pr.Id=PS.ProcessId
	                    WHERE moi.ArticleId='" + articleId + @"' AND  PS.SourceType='Scan' AND PS.Id NOT IN (SELECT ProductionSummaryId FROM [TRN].[IssueProcessWIP])
                        UNION ALL
						SELECT MT.UserName MaterialType,PS.SourceType Source,mm.UserName MaterialMaster,mma.MaterialMasterId,mma.StandardName Article,moi.ArticleId,NULL SKU1,NULL SKU2,PS.ScanQty,PS.Quantity,PS.Quantity RequisitionQty
						,PS.LotNumber,PR.UserName Process,PS.Id ProductionSummaryId ,PS.ProductionOrderId,NULL Characteristics1ValueId,NULL Characteristics2ValueId
						FROM TRN.ProductionSummary PS
	                    LEFT JOIN [TRN].[ProductionOrderDetail] POD ON POD.ProductionOrderId=PS.ProductionOrderId
	                    LEFT JOIN [TRN].[SalesOrder] SO ON SO.Id=POD.SalesOrderId
	                    LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id=SO.MasterOrderItemId
	                    LEFT JOIN [MST].[MaterialMasterArticle] mma on mma.Id=MOI.ArticleId
	                    LEFT JOIN [MST].[MaterialMaster] mm on mm.Id=mma.MaterialMasterId
	                    LEFT JOIN [MST].[MaterialGroupMaster] MG on MG.Id=mm.MaterialGroupMasterId
	                    LEFT JOIN [HKP].[MaterialType] MT on MT.Id=MG.MaterialTypeId
	                    LEFT JOIN [HKP].[Process] pr ON pr.Id=PS.ProcessId
	                    WHERE moi.ArticleId='" + articleId + @"'  AND  PS.SourceType='PB' AND PS.Id NOT IN (SELECT ProductionSummaryId FROM [TRN].[IssueProcessWIP])
						UNION ALL
						SELECT MT.UserName MaterialType,PS.SourceType Source,mm.UserName MaterialMaster,mma.MaterialMasterId,mma.StandardName Article,moi.ArticleId,CV1.UserName SKU1,CV2.UserName SKU2
						,PS.ScanQty,PSD.Qty Quantity,PSD.Qty RequisitionQty,PS.LotNumber,PR.UserName Process,PS.Id ProductionSummaryId ,PS.ProductionOrderId
						,PSD.Characteristics1ValueId,PSD.Characteristics2ValueId
						FROM TRN.ProductionSummary PS
						LEFT JOIN TRN.ProductionSummaryDetail PSD ON PSD.ProductionSummaryId=PS.Id
	                    LEFT JOIN [TRN].[ProductionOrderDetail] POD ON POD.ProductionOrderId=PS.ProductionOrderId
	                    LEFT JOIN [TRN].[SalesOrder] SO ON SO.Id=POD.SalesOrderId
	                    LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id=SO.MasterOrderItemId
	                    LEFT JOIN [MST].[MaterialMasterArticle] mma on mma.Id=MOI.ArticleId
	                    LEFT JOIN [MST].[MaterialMaster] mm on mm.Id=mma.MaterialMasterId
	                    LEFT JOIN [MST].[MaterialGroupMaster] MG on MG.Id=mm.MaterialGroupMasterId
	                    LEFT JOIN [HKP].[MaterialType] MT on MT.Id=MG.MaterialTypeId
	                    LEFT JOIN [HKP].[Process] pr ON pr.Id=PS.ProcessId
						LEFT JOIN HKP.CharacteristicsValue CV1 ON CV1.Id=PSD.Characteristics1ValueId
						LEFT JOIN HKP.CharacteristicsValue CV2 ON CV2.Id=PSD.Characteristics2ValueId
	                    WHERE moi.ArticleId='" + articleId + @"'  AND  PS.SourceType='SKU'
";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
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
    public string JWTransformationPODetailId { get; set; }


    #endregion Scalar Properties
}

public class JobWorkTransformationIssueReturnChild
{

    #region Scalar Properties

    public string Id { get; set; }
    public string CostCenterId { get; set; }
    public string OSTransformationPOId { get; set; }
    public string JWInputItem { get; set; }
    public string JWInputItemId { get; set; }
    public string TransactionUoM { get; set; }

    public string TransactionUoMId { get; set; }

    public string BaseUoMId { get; set; }
    public string TransactionQty { get; set; }
    //public string Remarks { get; set; }
    //public string Value { get; set; }
    //public string LotNumber { get; set; }


    #endregion Scalar Properties
}

public class IssueProcessDetail
{
    public string Id { get; set; }
    public string InventoryIssueId { get; set; }
    public string OSTransformationPOInputMaterialId { get; set; }
    public string MaterialMasterId { get; set; }
    public string ArticleId { get; set; }
    public string Qty { get; set; }
    public string TransactionQty { get; set; }
    public string Source { get; set; }
    public string Remarks { get; set; }
    public bool Active { get; set; }
}

public class IssueProcessWIP
{
    public string Id { get; set; }
    public string IssueProcessDetailId { get; set; }
    public string ProductionSummaryId { get; set; }
    public string ProductionOrderId { get; set; }
    public string MaterialMasterId { get; set; }
    public string ArticleId { get; set; }
    public string LotNumber { get; set; }
    public string Qty { get; set; }
    public string RequisitionQty { get; set; }
    public string Source { get; set; }
    public string Remarks { get; set; }
}