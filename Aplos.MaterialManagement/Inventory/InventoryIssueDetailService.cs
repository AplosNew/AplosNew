using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.Products;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using OTSBD;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace Library.MaterialManagement.Inventory
{
    public class InventoryIssueDetailService : Service<InventoryIssueDetail>, IInventoryIssueDetailService
    {
        #region Constructor

        private readonly IRepositoryAsync<InventoryMaterial> _inventoryMaterialRepository;
        private readonly IRepositoryAsync<InventoryReceiveDetail> _receiveDetailRepository;
        private readonly IRepositoryAsync<InventoryIssueHistory> _issueHistoryRepository;
        private readonly IRepositoryAsync<RequisitionIssueDetail> _requisitionIssueDetailRepository;

        private readonly ISqlRepository _sqlRepository;

        public InventoryIssueDetailService(
            IRepositoryAsync<InventoryIssueDetail> issueDetailRepository
            , IRepositoryAsync<InventoryMaterial> inventoryMaterialRepository
            , IRepositoryAsync<InventoryReceiveDetail> receiveDetailRepository
            , IRepositoryAsync<InventoryIssueHistory> issueHistoryRepository
            , IRepositoryAsync<RequisitionIssueDetail> requisitionIssueDetailRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(issueDetailRepository, unitOfWork, pkGeneratorService)
        {
            _issueHistoryRepository = issueHistoryRepository;
            _sqlRepository = sqlRepository;
            _requisitionIssueDetailRepository = requisitionIssueDetailRepository;
            _receiveDetailRepository = receiveDetailRepository;
            _inventoryMaterialRepository = inventoryMaterialRepository;
        }

        #endregion Constructor

        public void InsertRange(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue)
        {
            try
            {
                bool Error = false;
                var uiList = entities.ToList();
                var currentId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueDetail] WHERE InventoryIssueId='{inventoryIssue.Id}'").First();
                var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();

                var specificInvaterialIds = new string[] { };
                var maIds = new string[] { };
                if (specificStockList.IsNotNull())
                {
                    specificInvaterialIds = specificStockList.Select(t => t.InventoryMaterialId).Distinct().ToArray();
                    maIds = inventoryMaterialIds.Except(specificInvaterialIds).Distinct().ToArray();

                    for (int i = uiList.Count() - 1; i >= 0; i--)
                    {
                        var row = uiList.ElementAt(i);
                        if (specificInvaterialIds.Any(t => t == row.InventoryMaterialId))
                            uiList.RemoveAt(i);
                    }
                }
                else maIds = inventoryMaterialIds;
                var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"SELECT MGM.InventoryIssuePolicy AS [Policy], IRD.Id, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryReceiveId, IRD.InventoryMaterialId, IRD.MaterialStorageId, IRD.TransactionQty, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor
                                        , IRD.MaterialTranRate, IRD.MaterialTranAmount, IRD.TotalMaterialTranAmount, COALESCE((IRD.IssueQty),0) AS IssueQty, COALESCE((IRD.BaseIssueQty),0) AS BaseIssueQty,1 RequisitionQty
                                    FROM [TRN].[InventoryReceiveDetail] AS IRD JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId=IM.Id
                                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                    WHERE IR.PlantId='" + inventoryIssue.PlantId + "' AND IRD.MaterialStorageId IN('" + inventoryIssue.MaterialStorageId + "') AND IRD.InventoryMaterialId IN(" + ReturnStringArray(maIds) + @")
                                          AND (ISNULL(IRD.BaseIssueQty,0) IS NOT NULL OR ISNULL(IRD.BaseIssueQty,0) > 0) AND IR.Status='Posting' 
                                          AND CAST(IR.AddedDate AS DATE)<=CAST('" + inventoryIssue.IssueDate + @"' AS DATE) ORDER BY
                                          CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.FIFO + @"' THEN IRD.AddedDate END ASC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.LIFO + @"' THEN IRD.AddedDate END DESC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.WeightedAverage + @"' THEN IRD.AddedDate END ASC").ToList();
                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = "";
                if (receiveDetailList.IsNotNull())
                {
                    foreach (var issue in uiList)
                    {

                        var receiveDetailRow = receiveDetailList.FirstOrDefault(t => t.InventoryMaterialId == issue.InventoryMaterialId);

                        decimal detailtrnAmount = 0;
                        decimal totalGRNQty = 0;
                        /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* (TotalMmaterialTrnAmount/MaterialTrnRate))*/
                        /*Rate= Amount/Sum GRN Qty */
                        if (receiveDetailRow.TransactionUoMId != receiveDetailRow.BaseUOMId)
                            //input.BaseRate = receiveDetailRow.BaseAmount / receiveDetailRow.BaseQty;
                            issue.BaseRate = receiveDetailRow.MaterialTranAmount / receiveDetailRow.BaseQty;
                        else issue.BaseRate = receiveDetailRow.MaterialTranRate;
                        if (issue.TransactionUoMId != issue.BaseUOMId)
                            issue.BaseQty = Convert.ToInt16(issue.TransactionQty * receiveDetailRow.BaseUoMFactor);

                        //foreach (var item in receiveDetailList.Where(r => r.InventoryMaterialId == issue.InventoryMaterialId))
                        //    {
                        //    decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());

                        //    //decimal balaceGRNQty = Convert.ToInt16(_issueHistoryRepository.Query(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId));
                        //    if (item.TransactionUoMId == issue.TransactionUoMId)
                        //        {
                        //        detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty)) * (item.TotalMaterialTranAmount/item.TransactionQty)));
                        //        //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                        //        }
                        //        else
                        //        {
                        //        detailtrnAmount += Convert.ToDecimal((item.MaterialTranAmount - totalIssuedAmount) - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty)));
                        //        //totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                        //    }
                        //    }

                        if (receiveDetailRow.IsNull()) throw new CustomException("Stock 0");
                        currentId++;
                        totalGRNQty = issue.TransactionQty;
                        var detail = new InventoryIssueDetail
                        {
                            Id = MakePK(inventoryIssue.Id, currentId, 2),
                            InventoryIssueId = inventoryIssue.Id,
                            IsAsset = false,
                            //InventoryIssue = inventoryIssue,
                            InventoryMaterialId = issue.InventoryMaterialId,
                            TransactionQty = issue.TransactionQty,
                            BaseQty = issue.BaseQty,
                            BaseUOMId = issue.BaseUOMId,
                            TransactionUoMId = issue.TransactionUoMId,
                            AvgRate = issue.AvgRate,
                            AvgAmount = issue.TransactionQty * issue.AvgRate,
                            Policy = receiveDetailRow.Policy,
                            //PolicyAmount = issue.TransactionQty*(detailtrnAmount / totalGRNQty),
                            PolicyRate = detailtrnAmount / totalGRNQty,
                            BudgetMasterId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                            ActivityId = entities.Where(r => r.MaterialMasterId == issue.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),

                            CostCenterId = issue.CostCenterId,
                            ModelState = ModelState.Added

                            //InventoryReceiveId= receiveDetailRow.InventoryReceiveId,
                            //InventoryReceiveDetailId= receiveDetailRow.InventoryReceiveDetailId

                        };
                        var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{detail.Id}'").First();
                        // single entry (history)
                        //if (input.TransactionQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                        if (issue.BaseQty <= (receiveDetailRow.BaseQty - receiveDetailRow.BaseIssueQty))
                        {
                            historyId++;
                            var history = new InventoryIssueHistory
                            {
                                Id = MakePK(detail.Id, historyId, 2),
                                InventoryIssueDetailId = detail.Id,
                                InventoryReceiveDetailId = receiveDetailRow.Id,
                                Qty = issue.TransactionQty,
                                Rate = Convert.ToDecimal(issue.BaseRate),
                                IsCapitalize = false
                            };
                            detail.PolicyRate = Convert.ToDecimal(issue.BaseRate);
                            detail.PolicyAmount = Convert.ToDecimal(issue.TransactionQty * issue.BaseRate);
                            builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(issue.TransactionQty)) + @"'
                                , BaseIssueQty='" + Convert.ToDecimal(Convert.ToDecimal(receiveDetailRow.BaseIssueQty) + Convert.ToDecimal(issue.TransactionQty)) + "' WHERE Id='" + receiveDetailRow.InventoryReceiveDetailId + "'";
                            rdBuilder.Append(builderSql);
                            AuditService.AddedLog(history);
                            try
                            {
                                _issueHistoryRepository.Insert(history);

                            }
                            catch (Exception)
                            {
                                Error = true;
                                throw;
                            }
                        }
                        // multiple entry (history)
                        else
                        {
                            var rdList = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).ToList();
                            var tqty = receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseQty).Sum()
                                       - receiveDetailList.Where(t => t.InventoryMaterialId == issue.InventoryMaterialId).Select(t => t.BaseIssueQty).Sum();
                            //if (tqty < input.TransactionQty) throw new CustomException("Stock 0");
                            if (tqty < issue.BaseQty) throw new CustomException("Stock 0");
                            decimal policyAmount = 0;
                            //decimal qtyDifference = input.TransactionQty;
                            decimal qtyDifference = Convert.ToDecimal(issue.BaseQty);
                            foreach (var item in rdList)
                            {
                                historyId++;
                                if (item.TransactionUoMId != item.BaseUOMId)
                                    //input.BaseRate = item.BaseAmount / item.BaseQty;
                                    issue.BaseRate = item.TotalMaterialBooksCurrencyAmount / item.BaseQty;
                                //else input.BaseRate = item.TransactionRate;
                                else issue.BaseRate = item.MaterialTranRate;

                                var issueQty = Convert.ToDecimal(item.BaseQty - item.BaseIssueQty); // (10 - 3)//Issueable Qty
                                if (qtyDifference >= issueQty) // (17 >= (10 - 3))
                                {
                                    policyAmount = policyAmount + Convert.ToDecimal(((item.BaseQty - item.BaseIssueQty) * issue.BaseRate));
                                    qtyDifference = Convert.ToDecimal(qtyDifference - issueQty);
                                    issueQty = Convert.ToDecimal(item.BaseIssueQty + issueQty);
                                }
                                else // (6 < 7) (qtyDifference < issueQty)
                                {
                                    //issueQty = Convert.ToDecimal(issueQty - qtyDifference);
                                    issueQty = Convert.ToDecimal(item.BaseIssueQty + qtyDifference);
                                    policyAmount = policyAmount + Convert.ToDecimal((issueQty * issue.BaseRate));
                                    qtyDifference = 0;
                                }
                                var history = new InventoryIssueHistory
                                {
                                    Id = MakePK(detail.Id, historyId, 2),
                                    InventoryIssueDetailId = detail.Id,
                                    InventoryReceiveDetailId = item.Id,
                                    //Qty = Convert.ToDecimal(issueQty - item.BaseIssueQty),//TODO
                                    Qty = Convert.ToDecimal(issueQty),//TODO
                                    Rate = Convert.ToInt32(issue.BaseRate),
                                    IsCapitalize = false
                                };

                                AuditService.AddedLog(history);
                                _issueHistoryRepository.Insert(history);
                                //if (qtyDifference == 0) break;

                                builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET BaseIssueQty='" + Convert.ToDecimal(issueQty) + "',IssueQty='" + Convert.ToDecimal(issueQty) + "'  WHERE Id='" + item.Id + "'";
                                rdBuilder.Append(builderSql);
                            }
                            detail.PolicyRate = Convert.ToDecimal(policyAmount / issue.TransactionQty);
                            detail.PolicyAmount = Convert.ToDecimal(policyAmount);
                        }
                        builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(issue.TotalQty - issue.TransactionQty) + "' WHERE Id='" + issue.InventoryMaterialId + "'";
                        rdBuilder.Append(builderSql);
                        AuditService.AddedLog(detail);
                        //InsertGraph(detail);
                    }
                }
                if (specificStockList.IsNotNull())
                {
                    //foreach (var invMaterialId in specificStockList)
                    //{
                    //    var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId.InventoryMaterialId + "'").FirstOrDefault();
                    //    var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId.InventoryMaterialId).ToList();
                    //    var totalReqQty = 0M;
                    //    decimal policyAmmount = 0;
                    //    currentId++;
                    //    var issueDetail = new InventoryIssueDetail();
                    //    issueDetail.Id = MakePK(inventoryIssue.Id, currentId, 2);
                    //    issueDetail.InventoryIssueId = inventoryIssue.Id;
                    //    //issueDetail.//InventoryIssue = inventoryIssue,
                    //    issueDetail.InventoryMaterialId = invMaterialId.InventoryMaterialId;
                    //    issueDetail.BaseUOMId = stockList.Select(t => t.BaseUOMId).FirstOrDefault();
                    //    issueDetail.AvgRate = invMaterial.AvgRate;
                    //    issueDetail.Policy = "N/A";
                    //    issueDetail.ModelState = ModelState.Added;
                    //    issueDetail.InventoryReceiveId = stockList.Select(t => t.InventoryReceiveId).FirstOrDefault();
                    //    issueDetail.InventoryReceiveDetailId = stockList.Select(t => t.InventoryReceiveDetailId).FirstOrDefault();
                    //    issueDetail.TransactionQty = stockList.Select(t => t.RequisitionQty).FirstOrDefault();
                    //    issueDetail.BaseQty = stockList.Select(t => t.BaseIssueQty).FirstOrDefault();

                    //    var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}'").First();
                    //    foreach (var item in stockList)
                    //    {
                    //        if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

                    //        if (item.TransactionUoMId != item.BaseUOMId)
                    //            item.RequisitionQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
                    //        totalReqQty += item.RequisitionQty;
                    //        historyId++;
                    //        var history = new InventoryIssueHistory
                    //        {
                    //            Id = MakePK(issueDetail.Id, historyId, 2),
                    //            InventoryIssueDetailId = issueDetail.Id,
                    //            InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                    //            Qty = item.RequisitionQty,
                    //            Rate = Convert.ToDecimal(item.BaseRate)
                    //        };
                    //        policyAmmount += history.Qty * history.Rate;
                    //        builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + "' WHERE Id='" + item.InventoryReceiveDetailId + "'";
                    //        rdBuilder.Append(builderSql);
                    //        AuditService.AddedLog(history);
                    //        _issueHistoryRepository.Insert(history);
                    //    }
                    //    issueDetail.PolicyRate = Convert.ToDecimal(policyAmmount / totalReqQty);
                    //    issueDetail.PolicyAmount = Convert.ToDecimal(policyAmmount);
                    //    issueDetail.TransactionQty = totalReqQty;
                    //    issueDetail.AvgAmount = totalReqQty * invMaterial.AvgRate;

                    //    builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - totalReqQty) + "' WHERE Id='" + invMaterialId + "'";
                    //    rdBuilder.Append(builderSql);
                    //    AuditService.AddedLog(issueDetail);
                    //    InsertGraph(issueDetail);
                    //}
                    foreach (var invMaterialId in specificInvaterialIds)
                    {
                        var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
                        var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
                        var totalReqQty = 0M;
                        decimal policyAmmount = 0;

                        decimal detailtrnAmount = 0;
                        decimal totalGRNQty = 0;
                        /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* TrnRate)*/
                        /*Rate= Amount/Sum GRN Qty */

                        foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
                        {
                            decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());

                            if (item.TransactionUoMId == entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault())
                            {
                                detailtrnAmount += Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.TransactionQty - item.IssueQty) - item.RequisitionQty) * (item.TotalMaterialBooksCurrencyAmount / item.TransactionQty)));
                                totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                            }
                            else
                            {
                                detailtrnAmount += Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialBooksCurrencyAmount / item.TransactionQty)));
                                totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                            }
                        }

                        currentId++;
                        var issueDetail = new InventoryIssueDetail
                        {
                            Id = MakePK(inventoryIssue.Id, currentId, 2),
                            InventoryIssueId = inventoryIssue.Id,
                            IsAsset = false,
                            //InventoryIssue = inventoryIssue,
                            InventoryMaterialId = invMaterialId,
                            BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                            TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault(),
                            AvgRate = invMaterial.AvgRate,
                            Policy = "N/A",
                            ModelState = ModelState.Added,
                            TransactionQty = stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),
                            PolicyAmount = detailtrnAmount,
                            PolicyRate = detailtrnAmount / totalGRNQty,
                            BaseQty = totalGRNQty,
                            AvgAmount = totalGRNQty * invMaterial.AvgRate,
                            BudgetMasterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                            ActivityId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                            CostCenterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.CostCenterId).FirstOrDefault(),


                        };

                        var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}'").First();
                        foreach (var item in stockList)
                        {

                            if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

                            if (item.TransactionUoMId != item.BaseUOMId)
                                totalReqQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
                            else
                                totalReqQty = item.RequisitionQty;
                            historyId++;
                            var history = new InventoryIssueHistory
                            {
                                Id = MakePK(issueDetail.Id, historyId, 2),
                                InventoryIssueDetailId = issueDetail.Id,
                                InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                Qty = item.RequisitionQty,
                                Rate = Convert.ToDecimal(item.BaseRate)
                            };
                            policyAmmount += history.Qty * history.Rate;
                            builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' 
                                , BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                            rdBuilder.Append(builderSql);
                            AuditService.AddedLog(history);
                            try
                            {
                                _issueHistoryRepository.Insert(history);

                            }
                            catch (Exception)
                            {
                                Error = true;
                                throw;
                            }

                        }


                        builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - issueDetail.BaseQty) + "' WHERE Id='" + invMaterialId + "'";
                        rdBuilder.Append(builderSql);
                        AuditService.AddedLog(issueDetail);
                        try
                        {
                            InsertGraph(issueDetail);

                        }
                        catch (Exception)
                        {
                            Error = true;
                            throw;
                        }
                    }
                }
                if (Error == false)
                {

                    _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                }







            }
            catch (CustomException)
            {
                throw;
            }
        }

        public IEnumerable<object> GetIssueDetailByIssueId(string issueId)
        {
            try
            {
                var sql = @"SELECT IID.Id, IID.InventoryIssueId, IID.InventoryMaterialId, II.MaterialStorageId
		                        , IM.MaterialMasterId, MM.UserName AS MaterialMasterName, IM.ArticleId, AR.StandardName AS ArticleName
		                        , IM.FirstCharacteristicsId, CH1.UserName AS FirstCharacteristics, IM.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicText--FirstCharacteristicsValue
		                        , IM.SecondCharacteristicsId, CH2.UserName AS SecondCharacteristics, IM.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicText--SecondCharacteristicsValue
		                        , IM.ThirdCharacteristicsId, CH3.UserName AS ThirdCharacteristics, IM.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicText--ThirdCharacteristicsValue
		                        , IID.TransactionQty, IID.BaseUOMId, UoM.UserName AS TransactionUoM, IID.AvgRate, IID.AvgAmount, IID.PolicyRate, IID.PolicyAmount, IID.[Policy]
                                ,CC.UserName CostCenter,C.UserName CountryName,c.Id CountryId,II.VoucherId
                        FROM [TRN].[InventoryIssueDetail] AS IID
                        LEFT JOIN [TRN].[InventoryIssue] AS II ON IID.InventoryIssueId=II.Id
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
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IID.TransactionUoMId=UoM.Id
                        LEFT Join scs.country C On C.Id=IM.CountryId
                        WHERE IID.InventoryIssueId='" + issueId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetIssueWithGl(string companyId, string issueId)
        {
            try
            {
                var sql = @"DECLARE  @issueId varchar(10)='" + issueId + @"', @companyId varchar(10)='" + companyId + @"'
                        Select T.InventoryIssueId,T.MaterialGroupMasterId,T.TrnType,T.GLGeneralInfoCode,T.GLGeneralInfoId,T.GLGeneralInfoName,T.BudgetCode,T.BudgetMasterId,T.BudgetName,T.ActivityCode,T.ActivityId,T.ActivityName,SUM(T.Dr) Dr,SUM(T.Cr) Cr,SUM(T.Amount) Amount
                        FROM (
                        SELECT MM.MaterialGroupMasterId, IID.InventoryIssueId
	                            ,GLGeneralInfoId=CASE WHEN IID.BudgetMasterId<>'' THEN BMI.GLGeneralInfoId ELSE  MGGL.InventoryGLId END
								,GLGeneralInfoCode=CASE WHEN IID.BudgetMasterId<>'' THEN IID.BudgetMasterId ELSE GL.AccountCode END
								,GLGeneralInfoName=CASE WHEN IID.BudgetMasterId<>'' THEN GLI.UserName ELSE GL.UserName END
								,GLName=CASE WHEN IID.BudgetMasterId<>'' THEN GLI.AccountCode +'-'+ GLI.UserName ELSE GL.AccountCode +'-'+ GL.UserName END
	                            ,BudgetMasterId=CASE WHEN IID.BudgetMasterId<>'' THEN IID.BudgetMasterId ELSE MGGL.ExpenseBudgetMasterId END
								,BudgetCode=CASE WHEN IID.BudgetMasterId<>'' THEN BI.Code ELSE B.Code END
								,BudgetName=CASE WHEN IID.BudgetMasterId<>'' THEN BI.UserName ELSE B.UserName END
								,ActivityId=CASE WHEN IID.ActivityId<>'' THEN IID.ActivityId ELSE MGGL.ExpenseActivityId END
								,ActivityCode=CASE WHEN IID.ActivityId<>'' THEN AI.Code ELSE A.Code END
								,ActivityName=CASE WHEN IID.ActivityId<>'' THEN AI.UserName ELSE A.UserName END
	                            , Amount=ROUND(SUM(PolicyAmount),2), ROUND(SUM(PolicyAmount),2) Dr, 0 Cr, 'Dr' AS TrnType
                        FROM  [TRN].[InventoryIssueDetail] AS IID 
                        JOIN [TRN].[InventoryMaterial] AS IM ON IID.InventoryMaterialId=IM.Id
                        JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                        JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
		                        AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId=GL.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ExpenseBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BMI ON IID.BudgetMasterId= BMI.Id
                        LEFT JOIN [HKP].[Budget] AS BI ON BMI.BudgetId= BI.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GLI ON BMI.GLGeneralInfoId=GLI.Id
                        LEFT JOIN [HKP].[Activity] AS AI ON MGGL.ExpenseActivityId= AI.Id
                        WHERE IID.InventoryIssueId=@issueId
                        GROUP BY MM.MaterialGroupMasterId, IID.InventoryIssueId, MGGL.InventoryGLId, MGGL.ExpenseGLId, GL.AccountCode, GL.UserName, MGGL.ExpenseBudgetMasterId
						, B.Code, B.UserName, MGGL.ExpenseActivityId, A.Code, A.UserName,BMI.GLGeneralInfoId,IID.BudgetMasterId,GLI.UserName,GLI.AccountCode,BI.Code, BI.UserName
                        ,AI.Code, AI.UserName,IID.ActivityId
						) AS T
						GROUP BY T.ActivityCode,T.ActivityId,T.ActivityName,T.BudgetCode,T.BudgetMasterId,T.BudgetName,T.GLGeneralInfoCode,T.GLGeneralInfoId,T.GLGeneralInfoName,T.MaterialGroupMasterId
						,T.InventoryIssueId,T.TrnType,T.MaterialGroupMasterId
						
						UNION
					Select T.InventoryIssueId,NULL MaterialGroupMasterId,T.TrnType,T.GLGeneralInfoCode,T.GLGeneralInfoId,T.GLGeneralInfoName,T.BudgetCode,T.BudgetMasterId,T.BudgetName,T.ActivityCode,T.ActivityId,T.ActivityName,SUM(T.Dr) Dr,SUM(T.Cr) Cr,SUM(T.Amount) Amount
						FROM(
						SELECT MM.MaterialGroupMasterId, IID.InventoryIssueId
	                            , IH.PostDrGLGeneralInfoId AS GLGeneralInfoId, IH.GAccountCode AS GLGeneralInfoCode, IH.GUserName AS GLGeneralInfoName, IH.GAccountCode +'-'+ IH.GUserName AS GLName
	                            , IH.PostDrBudgetMasterId AS BudgetMasterId, IH.BCode AS BudgetCode, IH.BUserName AS BudgetName
                                , IH.PostDrActivityId AS ActivityId, IH.ACode AS ActivityCode, IH.AUserName AS ActivityName
	                            , Amount=ROUND(SUM(IID.PolicyAmount),2), 0 Dr, ROUND(SUM(IID.PolicyAmount),2) Cr, 'Cr' AS TrnType
                        FROM 
                         [TRN].[InventoryIssueDetail] AS IID 
                        JOIN [TRN].[InventoryMaterial] AS IM ON IID.InventoryMaterialId=IM.Id
                        JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
						JOIN (select distinct  InventoryIssueDetailId ,ID.PostDrGLGeneralInfoId, GL.AccountCode GAccountCode, GL.UserName GUserName
						, ID.PostDrBudgetMasterId, B.Code BCode, B.UserName BUserName, ID.PostDrActivityId, A.Code ACode, A.UserName AUserName
						from  [TRN].[InventoryIssueHistory] iih join TRN.InventoryReceiveDetail id on id.Id=iih.InventoryReceiveDetailId
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON ID.PostDrGLGeneralInfoId=GL.Id
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON ID.PostDrBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON ID.PostDrActivityId= A.Id
						) AS IH ON IH.InventoryIssueDetailId=IID.Id
                        WHERE IID.InventoryIssueId=@issueId
                        GROUP BY MM.MaterialGroupMasterId, IID.InventoryIssueId, IH.PostDrGLGeneralInfoId, IH.GAccountCode, IH.GUserName, IH.PostDrBudgetMasterId, IH.BCode, IH.BUserName, IH.PostDrActivityId, IH.ACode, IH.AUserName, IH.InventoryIssueDetailId
						) AS T 
						GROUP BY T.ActivityCode,T.ActivityId,T.ActivityName,T.BudgetCode,T.BudgetMasterId,T.BudgetName,T.GLGeneralInfoCode,T.GLGeneralInfoId,T.GLGeneralInfoName,T.InventoryIssueId,T.TrnType--,T.Dr,T.Cr";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetBudgetActivityInIssueMaterial(string materialGroupMasterId)
        {
            try
            {
                var sql = @"SELECT MGGL.MaterialGroupMasterId
	                            , MGGL.InventoryGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName, GL.AccountCode +'-'+ GL.UserName AS GLName
	                            , MGGL.ExpenseBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                                , MGGL.ExpenseActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                        FROM [HKP].[MaterialGroupGL] AS MGGL 
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId=GL.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ExpenseBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id
                        WHERE MGGL.MaterialGroupMasterId='" + materialGroupMasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public IEnumerable<object> GetCostCenterLoadNewFun(string EntityId)
        {
            try
            {
                var sql = @"Select CostCn.Id Value,CostCn.UserName Text from [ORG].[EntityCostCenter] EnCostCn
                LEFT JOIN [ORG].[CostCenter] AS CostCn ON CostCn.Id=EnCostCn.CostCenterId
                LEFT JOIN [ORG].[Entity] AS En ON En.Id=EnCostCn.EntityId
                        WHERE En.Id='" + EntityId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetBudgetActivityInSalesMaterial(string materialGroupMasterId)
        {
            try
            {
                var sql = @"SELECT MGGL.MaterialGroupMasterId
	                            , MGGL.InventoryGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName, GL.AccountCode +'-'+ GL.UserName AS GLName
	                            , MGGL.ExpenseBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                                , MGGL.ExpenseActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                        FROM [HKP].[MaterialGroupGL] AS MGGL 
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId=GL.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ExpenseBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id
                        WHERE MGGL.MaterialGroupMasterId='" + materialGroupMasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetBudgetActivityInScrapMaterial(string materialGroupMasterId)
        {
            try
            {
                var sql = @"SELECT MGGL.MaterialGroupMasterId
	                            , MGGL.InventoryGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName, GL.AccountCode +'-'+ GL.UserName AS GLName
	                            , MGGL.ExpenseBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
                                , MGGL.ExpenseActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                        FROM [HKP].[MaterialGroupGL] AS MGGL 
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON MGGL.ExpenseGLId=GL.Id
                        LEFT JOIN[MST].[BudgetMaster] AS BM ON MGGL.ExpenseBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON MGGL.ExpenseActivityId= A.Id
                        WHERE MGGL.MaterialGroupMasterId='" + materialGroupMasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void RequisitionIssueDetailInsert(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList
            , InventoryIssue inventoryIssue, IEnumerable<RequisitionIssueDetailViewModel> requisitionIssueDetails)
        {
            try
            {
                var uiList = requisitionIssueDetails.ToList();
                var currentId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueDetail] WHERE InventoryIssueId='{inventoryIssue.Id}'").First();
                var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();

                var specificInvaterialIds = new string[] { };
                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = "";

                if (specificStockList.IsNotNull())
                {
                    foreach (var invIssue in entities)
                    {
                        var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invIssue + "'").FirstOrDefault();
                        var totalReqQty = 0M;
                        decimal policyAmmount = 0;
                        currentId++;
                        var issueDetail = new InventoryIssueDetail();

                        issueDetail.Id = MakePK(inventoryIssue.Id, currentId, 2);
                        issueDetail.InventoryIssueId = inventoryIssue.Id;
                        issueDetail.InventoryMaterialId = invIssue.InventoryMaterialId;
                        issueDetail.BaseUOMId = invIssue.BaseUOMId;
                        issueDetail.TransactionUoMId = invIssue.TransactionUoMId;
                        issueDetail.AvgRate = invIssue.AvgRate;
                        issueDetail.Policy = "N/A";
                        issueDetail.ModelState = ModelState.Added;
                        issueDetail.TransactionQty = invIssue.IssueQty;
                        issueDetail.BaseQty = invIssue.IssueQty;
                        issueDetail.InventoryReceiveId = invIssue.InventoryReceiveId;
                        issueDetail.InventoryReceiveDetailId = invIssue.InventoryReceiveDetailId;

                        int requisitionIssueDetailId = 0;
                        foreach (var item in requisitionIssueDetails.Where(r => r.MaterialMasterId == invIssue.MaterialMasterId
                        && r.ArticleId == invIssue.ArticleId && r.FirstCharacteristicsValueId == invIssue.FirstCharacteristicsValueId
                        && r.SecondCharacteristicsValueId == invIssue.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == invIssue.ThirdCharacteristicsValueId))
                        {
                            requisitionIssueDetailId++;
                            var requisitionIssueDetail = new RequisitionIssueDetail()
                            {
                                Id = MakePK(issueDetail.Id, requisitionIssueDetailId, 2),
                                IssueRequestId = item.IssueRequestId,
                                IssueDetailId = issueDetail.Id,
                                IssueMasterId = issueDetail.InventoryIssueId,
                                IssueQty = item.IssueValidQty,
                                IssueRequestMasterId = item.IssueRequestMasterId,
                                IssueRejectedQty = item.IssueRejectedQty
                            };
                            AuditService.AddedLog(requisitionIssueDetail);
                            _requisitionIssueDetailRepository.Insert(requisitionIssueDetail);
                        }
                        //var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}'").First();
                        //foreach (var item in stockList)
                        //{
                        //    if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

                        //    if (item.TransactionUoMId != item.BaseUOMId)
                        //        totalReqQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
                        //    historyId++;
                        //    var history = new InventoryIssueHistory
                        //    {
                        //        Id = MakePK(issueDetail.Id, historyId, 2),
                        //        InventoryIssueDetailId = issueDetail.Id,
                        //        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                        //        Qty = item.RequisitionQty,
                        //        Rate = Convert.ToDecimal(item.BaseRate)
                        //    };
                        //    policyAmmount += history.Qty * history.Rate;
                        //    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' 
                        //        , BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                        //    rdBuilder.Append(builderSql);
                        //    AuditService.AddedLog(history);
                        //    _issueHistoryRepository.Insert(history);
                        //}
                        //issueDetail.BaseQty = totalReqQty;
                        //issueDetail.PolicyRate = Convert.ToDecimal(policyAmmount / totalReqQty);
                        //issueDetail.PolicyAmount = Convert.ToDecimal(policyAmmount);
                        //issueDetail.AvgAmount = totalReqQty * invMaterial.AvgRate;

                        //builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - totalReqQty) + "' WHERE Id='" + invIssue + "'";
                        //rdBuilder.Append(builderSql);
                        AuditService.AddedLog(issueDetail);
                        InsertGraph(issueDetail);
                    }
                }
                //  _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
            }
            catch (CustomException)
            {
                throw;
            }
        }

        public void RequisitionIssueDetailUpdate(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList
           , InventoryIssue inventoryIssue, IEnumerable<RequisitionIssueDetailViewModel> requisitionIssueDetails)
        {
            try
            {
                var uiList = requisitionIssueDetails.ToList();
                //var currentId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueDetail] WHERE InventoryIssueId='{inventoryIssue.Id}'").First();
                var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();

                var specificInvaterialIds = new string[] { };
                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = "";

                if (specificStockList.IsNotNull())
                {
                    foreach (var invIssue in entities)
                    {
                        var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invIssue + "'").FirstOrDefault();
                        var totalReqQty = 0M;
                        decimal policyAmmount = 0;
                        //currentId++;
                        var issueDetail = base.Find(invIssue.IssueDetailId);

                        //issueDetail.Id = MakePK(inventoryIssue.Id, currentId, 2);
                        issueDetail.InventoryIssueId = inventoryIssue.Id;
                        issueDetail.InventoryMaterialId = invIssue.InventoryMaterialId;
                        issueDetail.BaseUOMId = invIssue.BaseUOMId;
                        issueDetail.TransactionUoMId = invIssue.TransactionUoMId;
                        issueDetail.AvgRate = invIssue.AvgRate;
                        issueDetail.Policy = "N/A";
                        issueDetail.ModelState = ModelState.Modified;
                        issueDetail.TransactionQty = invIssue.IssueQty;
                        issueDetail.BaseQty = invIssue.IssueQty;
                        issueDetail.InventoryReceiveId = invIssue.InventoryReceiveId;
                        issueDetail.InventoryReceiveDetailId = invIssue.InventoryReceiveDetailId;

                        //int requisitionIssueDetailId = 0;
                        foreach (var item in requisitionIssueDetails.Where(r => r.MaterialMasterId == invIssue.MaterialMasterId
                        && r.ArticleId == invIssue.ArticleId && r.FirstCharacteristicsValueId == invIssue.FirstCharacteristicsValueId
                        && r.SecondCharacteristicsValueId == invIssue.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == invIssue.ThirdCharacteristicsValueId))
                        {
                            var requisitionIssueDetail = _requisitionIssueDetailRepository.Find(item.Id);

                            requisitionIssueDetail.IssueQty = item.IssueValidQty;
                            requisitionIssueDetail.IssueRejectedQty = item.IssueRejectedQty;
                            requisitionIssueDetail.ModelState = ModelState.Modified;
                            AuditService.UpdatedLog(requisitionIssueDetail);
                            // _requisitionIssueDetailRepository.Insert(requisitionIssueDetail);
                            _requisitionIssueDetailRepository.Update(requisitionIssueDetail);
                        }
                        //var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}'").First();
                        //foreach (var item in stockList)
                        //{
                        //    if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

                        //    if (item.TransactionUoMId != item.BaseUOMId)
                        //        totalReqQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
                        //    historyId++;
                        //    var history = new InventoryIssueHistory
                        //    {
                        //        Id = MakePK(issueDetail.Id, historyId, 2),
                        //        InventoryIssueDetailId = issueDetail.Id,
                        //        InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                        //        Qty = item.RequisitionQty,
                        //        Rate = Convert.ToDecimal(item.BaseRate)
                        //    };
                        //    policyAmmount += history.Qty * history.Rate;
                        //    builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' 
                        //        , BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                        //    rdBuilder.Append(builderSql);
                        //    AuditService.AddedLog(history);
                        //    _issueHistoryRepository.Insert(history);
                        //}
                        //issueDetail.BaseQty = totalReqQty;
                        //issueDetail.PolicyRate = Convert.ToDecimal(policyAmmount / totalReqQty);
                        //issueDetail.PolicyAmount = Convert.ToDecimal(policyAmmount);
                        //issueDetail.AvgAmount = totalReqQty * invMaterial.AvgRate;

                        //builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - totalReqQty) + "' WHERE Id='" + invIssue + "'";
                        //rdBuilder.Append(builderSql);
                        AuditService.UpdatedLog(issueDetail);
                        //InsertGraph(issueDetail);
                        UpdateGraph(issueDetail);
                    }
                }
                //  _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
            }
            catch (CustomException)
            {
                throw;
            }
        }


        public void InsertAssetIssueDetail(IEnumerable<InventoryMaterialViewModel> entities, IEnumerable<InventoryMaterialViewModel> specificStockList, InventoryIssue inventoryIssue)
        {
            try
            {
                var GRNCalculateList = new List<InventoryIssueHistory>();
                var uiList = entities.ToList();
                var currentId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueDetail] WHERE InventoryIssueId='{inventoryIssue.Id}'").First();
                var inventoryMaterialIds = entities.Select(t => t.InventoryMaterialId).ToArray();

                var specificInvaterialIds = new string[] { };
                var maIds = new string[] { };
                if (specificStockList.IsNotNull())
                {
                    specificInvaterialIds = specificStockList.Select(t => t.InventoryMaterialId).Distinct().ToArray();
                    maIds = inventoryMaterialIds.Except(specificInvaterialIds).Distinct().ToArray();

                    for (int i = uiList.Count() - 1; i >= 0; i--)
                    {
                        var row = uiList.ElementAt(i);
                        if (specificInvaterialIds.Any(t => t == row.InventoryMaterialId))
                            uiList.RemoveAt(i);
                    }
                }
                else maIds = inventoryMaterialIds;
                var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"SELECT MGM.InventoryIssuePolicy AS [Policy], IRD.Id, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryReceiveId, IRD.InventoryMaterialId, IRD.MaterialStorageId, IRD.TransactionQty, IRD.TransactionUoMId, IRD.BaseQty, IRD.BaseUOMId, IRD.BaseUoMFactor
                                        , IRD.MaterialTranRate, IRD.MaterialTranAmount, IRD.TotalMaterialTranAmount, COALESCE((IRD.IssueQty),0) AS IssueQty, COALESCE((IRD.BaseIssueQty),0) AS BaseIssueQty,1 RequisitionQty
                                    FROM [TRN].[InventoryReceiveDetail] AS IRD JOIN TRN.InventoryMaterial AS IM ON IRD.InventoryMaterialId=IM.Id
                                    JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                    JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                                    WHERE IR.PlantId='" + inventoryIssue.PlantId + "' AND IRD.MaterialStorageId IN('" + inventoryIssue.MaterialStorageId + "') AND IRD.InventoryMaterialId IN(" + ReturnStringArray(maIds) + @")
                                          --AND (ISNULL(IRD.BaseIssueQty,0) IS NOT NULL OR ISNULL(IRD.BaseIssueQty,0) > 0) 
                                          AND IR.Status='Posting' 
                                          AND ((isnull(IRD.TransactionQty,0)-isnull(IRD.IssueQty,0)-isnull(IRD.PurchaseReturnQty,0)-isnull(IRD.ReductionByAdjustmentQty,0)-isnull(IRD.InventorySalesQty,0)-isnull(IRD.InventoryScrapQty,0))+isnull(IRD.IssueReturnQty,0))>0
                                          AND CAST(IR.AddedDate AS DATE)<=CAST('" + inventoryIssue.IssueDate + @"' AS DATE) ORDER BY
                                          CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.FIFO + @"' THEN IRD.AddedDate END ASC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.LIFO + @"' THEN IRD.AddedDate END DESC
                                        , CASE WHEN MGM.InventoryIssuePolicy='" + InventoryIssuePolicy.WeightedAverage + @"' THEN IRD.AddedDate END ASC").ToList();
                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = "";
                if (specificStockList.IsNotNull())
                {

                    foreach (var invMaterialId in specificInvaterialIds)
                    {
                        var invMaterial = _issueHistoryRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + invMaterialId + "'").FirstOrDefault();
                        var stockList = specificStockList.Where(t => t.InventoryMaterialId == invMaterialId).ToList();
                        var totalReqQty = 0M;
                        decimal policyAmmount = 0;

                        decimal detailtrnAmount = 0;
                        decimal totalGRNQty = 0;
                        /*Amount=MaterialTrnAmount - ((TRN Qty - IssueQty)* TrnRate)*/
                        /*Rate= Amount/Sum GRN Qty */

                        foreach (var item in specificStockList.Where(r => r.InventoryMaterialId == invMaterialId))
                        {
                            //decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT ISNULL(SUM(Qty*Rate),0) FROM [TRN].[InventoryIssueHistory] where  InventoryReceiveDetailId='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());
                            decimal totalIssuedAmount = Convert.ToDecimal(_issueHistoryRepository.SqlQuery<decimal>(@"SELECT totalIssuedAmount=((ISNULL(SUM(ISH.TotalBaseAmount),0)+isnull(sum(PR.TotalMaterialTranAmount),0)+isnull(sum(PSAH.TotalAmount),0)+isnull(sum(IIH.TotalAmount),0)+isnull(sum(InvS.TotalAmount),0)) -isnull(sum(IIR.TotalAmount),0)) 
																																FROM TRN.InventoryReceiveDetail IRD
																																left join trn.InventorySalesHistory ISH ON ISH.InventoryReceiveDetailId=IRD.Id
																																LEFT JOIN trn.PurchaseReturnDetail PR ON PR.InventoryReceiveDetailId=IRD.Id
																																LEFT JOIN trn.PhysicalStockAdjustmentHistory PSAH ON PSAH.InventoryReceiveDetailId=IRD.Id
																																LEFT join [TRN].[InventoryIssueHistory] IIH ON IIH.InventoryReceiveDetailId=IRD.Id
																																LEFT JOIN TRN.InventoryScrapHistory InvS ON InvS.InventoryReceiveDetailId=IRD.Id
																																LEFT join TRN.InventoryIssueReturnHistory IIR ON IIR.InventoryReceiveDetailId=IRD.Id
																															    WHERE  IRD.Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault());


                            if (item.TransactionUoMId == entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault())
                            {
                                detailtrnAmount += Convert.ToDecimal((item.TotalMaterialBooksCurrencyAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - item.RequisitionQty) * (item.TotalMaterialBooksCurrencyAmount / item.TransactionQty)));
                                var newgrn = new InventoryIssueHistory
                                {
                                    TotalAmount = Convert.ToDecimal((item.TotalMaterialTranAmount - totalIssuedAmount) - (((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                    InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                };
                                GRNCalculateList.Add(newgrn);
                                totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                            }
                            else
                            {
                                detailtrnAmount += Convert.ToDecimal(item.TotalMaterialBooksCurrencyAmount - ((((item.TransactionQty - item.IssueQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialBooksCurrencyAmount / item.TransactionQty)));
                                var newgrn = new InventoryIssueHistory
                                {
                                    TotalAmount = Convert.ToDecimal(item.MaterialTranAmount - ((((item.TransactionQty - (item.IssueQty + item.PurchaseReturnQty + item.ReductionByAdjustmentQty + item.InventorySalesQty + item.InventoryScrapQty) - item.IssueReturnQty) * item.BaseUoMFactor) - item.RequisitionQty) * (item.TotalMaterialTranAmount / item.TransactionQty))),
                                    InventoryReceiveDetailId = item.InventoryReceiveDetailId
                                };
                                GRNCalculateList.Add(newgrn);
                                totalGRNQty += Convert.ToDecimal(item.RequisitionQty);
                            }
                        }

                        currentId++;
                        var issueDetail = new InventoryIssueDetail
                        {
                            Id = MakePK(inventoryIssue.Id, currentId, 2),
                            InventoryIssueId = inventoryIssue.Id,
                            IsAsset = true,
                            //InventoryIssue = inventoryIssue,
                            InventoryMaterialId = invMaterialId,
                            BaseUOMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault(),
                            TransactionUoMId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.TransactionUoMId).FirstOrDefault(),
                            AvgRate = Math.Round(invMaterial.AvgRate,4),
                            Policy = "N/A",
                            ModelState = ModelState.Added,
                            TransactionQty = stockList.Sum(r => r.RequisitionQty),//stockList.Select(t => t.RequisitionQty).FirstOrDefault(),
                            PolicyAmount = Math.Round(detailtrnAmount,2),
                            PolicyRate = Math.Round((detailtrnAmount / totalGRNQty),4),
                            BaseQty = totalGRNQty,
                            AvgAmount = Math.Round((totalGRNQty * invMaterial.AvgRate),2),

                            BudgetMasterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.BudgetMasterId).FirstOrDefault(),
                            ActivityId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.ActivityId).FirstOrDefault(),
                            CostCenterId = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.CostCenterId).FirstOrDefault(),
                            Comments = entities.Where(r => r.MaterialMasterId == invMaterial.MaterialMasterId).Select(t => t.Comments).FirstOrDefault(),
                        };

                        var historyId = _issueHistoryRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryIssueHistory] WHERE InventoryIssueDetailId='{issueDetail.Id}'").First();
                        foreach (var item in stockList)
                        {

                            if (item.RequisitionQty > item.StockQty) throw new CustomException("Requisition qty can't greater stock qty.");

                            if (item.TransactionUoMId != item.BaseUOMId)
                                totalReqQty = Convert.ToInt32(item.RequisitionQty * item.BaseUoMFactor);
                            else
                                totalReqQty = item.RequisitionQty;
                            historyId++;
                            var SelectedGRN = GRNCalculateList.Where(r => r.InventoryReceiveDetailId == item.InventoryReceiveDetailId).FirstOrDefault();
                            var history = new InventoryIssueHistory
                            {
                                Id = MakePK(issueDetail.Id, historyId, 2),
                                InventoryIssueDetailId = issueDetail.Id,
                                InventoryReceiveDetailId = item.InventoryReceiveDetailId,
                                Qty = item.RequisitionQty,
                                // Rate = Convert.ToDecimal(item.BaseRate),
                                Rate = Math.Round((SelectedGRN.TotalAmount / item.RequisitionQty),4),
                                TotalAmount = Math.Round(SelectedGRN.TotalAmount,2),
                                BooksCurrencyBaseRate= Math.Round(Convert.ToDecimal(item.BooksCurrencyBaseRate),4),
                                TotalMaterialBooksCurrencyAmount= Math.Round(Convert.ToDecimal(item.RequisitionQty * item.BooksCurrencyBaseRate),2)
                            };
                            //policyAmmount += history.Qty * history.Rate;
                            builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET IssueQty='" + Convert.ToDecimal(item.RequisitionQty + item.IssueQty) + @"' 
                                , BaseIssueQty = '" + (Convert.ToDecimal(Convert.ToDecimal(item.BaseIssueQty) + Convert.ToDecimal(totalReqQty))) + "' WHERE Id = '" + item.InventoryReceiveDetailId + "'";

                            rdBuilder.Append(builderSql);
                            AuditService.AddedLog(history);
                            _issueHistoryRepository.Insert(history);

                        }

                        builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - issueDetail.BaseQty) + "' WHERE Id='" + invMaterialId + "'";
                        rdBuilder.Append(builderSql);
                        AuditService.AddedLog(issueDetail);
                        InsertGraph(issueDetail);
                    }
                }
                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
            }
            catch (CustomException)
            {
                throw;
            }
        }
        public IEnumerable<object> GetAdjustmentDetailByIssueId(string issueId)
        {
            try
            {
                string sql = @"SELECT IID.Id, IID.PhysicalStockAdjustmentMasterID, IID.InventoryMaterialId, II.MaterialStorageId
		                        , IM.MaterialMasterId, MM.UserName AS MaterialMasterName, IM.ArticleId, AR.StandardName AS ArticleName
		                        , IM.FirstCharacteristicsId, CH1.UserName AS FirstCharacteristics, IM.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicsValue
		                        , IM.SecondCharacteristicsId, CH2.UserName AS SecondCharacteristics, IM.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicsValue
		                        , IM.ThirdCharacteristicsId, CH3.UserName AS ThirdCharacteristics, IM.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicsValue
		                        , IID.TransactionQty, IID.BaseUOMId, UoM.UserName AS TransactionUoM, IID.AvgRate, IID.AvgAmount, IID.PolicyRate, IID.PolicyAmount, IID.[Policy]
                                ,CC.UserName CostCenter
                        FROM [TRN].[PhysicalStockAdjustmentDetail] AS IID
                        JOIN [TRN].[PhysicalStockAdjustmentMaster] AS II ON IID.PhysicalStockAdjustmentMasterID=II.Id
                        JOIN [TRN].[InventoryMaterial] AS IM ON IID.InventoryMaterialId=IM.Id
                        JOIN [MST].[MaterialMaster] AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN [MST].[MaterialMasterArticle] AS AR ON IM.ArticleId=AR.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH1 ON IM.FirstCharacteristicsId=CH1.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV1 ON IM.FirstCharacteristicsValueId=CHV1.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH2 ON IM.SecondCharacteristicsId=CH2.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON IM.SecondCharacteristicsValueId=CHV2.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH3 ON IM.ThirdCharacteristicsId=CH3.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON IM.ThirdCharacteristicsValueId=CHV3.Id
						LEFT JOIN [ORG].[CostCenter] AS CC On CC.Id=IID.CostCenterId
                        JOIN [SCS].[UnitOfMeasurement] AS UoM ON IID.BaseUOMId=UoM.Id
                        WHERE IID.PhysicalStockAdjustmentMasterID='" + issueId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetSalesDetailByIssueId(string issueId)
        {
            try
            {
                string sql = @"SELECT ISH.Id HistotyId,IID.Id, IID.InventorySalesId InventoryIssueId, IID.InventoryMaterialId, II.MaterialStorageId
		                        , IM.MaterialMasterId, MM.UserName AS MaterialMasterName, IM.ArticleId, AR.StandardName AS ArticleName
		                        , IM.FirstCharacteristicsId, CH1.UserName AS FirstCharacteristics, IM.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicText--FirstCharacteristicsValue
		                        , IM.SecondCharacteristicsId, CH2.UserName AS SecondCharacteristics, IM.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicText--SecondCharacteristicsValue
		                        , IM.ThirdCharacteristicsId, CH3.UserName AS ThirdCharacteristics, IM.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicText--ThirdCharacteristicsValue
		                        , IID.TransactionQty, IID.BaseUOMId, UoM.UserName AS TransactionUoM, IID.AvgRate, IID.AvgAmount, IID.PolicyRate, IID.PolicyAmount, IID.[Policy]
                                ,CC.UserName CostCenter,C.UserName CountryName,c.Id CountryId,II.ToCurrencyRate, II.DocRefNo, II.DocDate , II.NoteForAccounts
                                 ,ISD.SalesRate,ISD.TotalAmount,IST.TaxAmount
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
                        JOIN (select InventorySalesHistoryId,Sum(TaxAmount) TaxAmount from trn.inventorySalesTax group by InventorySalesHistoryId) IST ON IST.InventorySalesHistoryId =ISH.Id
                        LEFT JOIN (select distinct Id,ROUND(sum(TransactionQty), 2) Qty,ROUND(sum(SalesRate), 2) SalesRate,(ROUND(sum(TransactionQty), 2) * ROUND(sum(SalesRate), 2)) TotalAmount from  TRN.InventorySalesDetail group by Id) ISD ON ISD.Id=IID.Id

                        WHERE IID.InventorySalesId='" + issueId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> GetScrapDetailByIssueId(string issueId)
        {
            try
            {
                string sql = @"SELECT IID.Id, IID.InventoryScrapId InventoryIssueId, IID.InventoryMaterialId, II.MaterialStorageId
		                        , IM.MaterialMasterId, MM.UserName AS MaterialMasterName, IM.ArticleId, AR.StandardName AS ArticleName
		                        , IM.FirstCharacteristicsId, CH1.UserName AS FirstCharacteristics, IM.FirstCharacteristicsValueId, CHV1.UserName AS FirstCharacteristicText--FirstCharacteristicsValue
		                        , IM.SecondCharacteristicsId, CH2.UserName AS SecondCharacteristics, IM.SecondCharacteristicsValueId, CHV2.UserName AS SecondCharacteristicText--SecondCharacteristicsValue
		                        , IM.ThirdCharacteristicsId, CH3.UserName AS ThirdCharacteristics, IM.ThirdCharacteristicsValueId, CHV3.UserName AS ThirdCharacteristicText--ThirdCharacteristicsValue
		                        , IID.TransactionQty, IID.BaseUOMId, UoM.UserName AS TransactionUoM, IID.AvgRate, IID.AvgAmount, IID.PolicyRate, IID.PolicyAmount, IID.[Policy]
                                ,CC.UserName CostCenter,C.UserName CountryName,c.Id CountryId
                                --,ISH.SalesRate,ISH.TotalAmount--,IST.TaxAmount
                        FROM [TRN].[InventoryScrapDetail] AS IID
                        LEFT JOIN [TRN].[InventoryScrap] AS II ON IID.InventoryScrapId=II.Id
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
                        LEFT JOIN TRN.InventoryScrapHistory ISH ON ISH.InventoryScrapDetailId=IID.Id
                       -- LEFT JOIN (select InventoryScrapHistoryId,Sum(TaxAmount) TaxAmount from trn.inventorySalesTax group by InventoryScrapHistoryId) IST ON IST.InventoryScrapHistoryId =ISH.Id
                        WHERE IID.InventoryScrapId='" + issueId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public IEnumerable<object> GetListForMaterialTransferGridFun(string plantId, string POTypeStatus)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var Sql = "";
            if (string.IsNullOrEmpty(POTypeStatus) == true)
            {
                POTypeStatus = "For Checking";
            }
            if (POTypeStatus == "For Checking")
            {
                Sql = @" select * from (
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS IssueDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                        --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
		                            , IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1
                                    ,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
		                            ,isnull(PWG.UserName ,'') GateName
                                    ,IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById
                            FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
		                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation EI1 ON EI.SystemId=IR.AuthorizedBy
                            left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                            Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                            WHERE IR.PlantId='" + identity.PlantId + @"'
                            AND ISNULL(IR.[Status],'')<>'Posting' 
                            AND IR.OpeningBalanceId IS NULL 
                            AND IR.EmployeeId IS NULL 
                            And IR.IsApproved = 1 
                            ANd IR.POId is null 
                            AND IR.GRNType='MaterialTransfer' 
                            AND IR.CheckedByStatus='ForChecked' 

                            UNION ALL
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS IssueDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                        --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
		                            , IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1
                                    ,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
		                            ,isnull(PWG.UserName ,'') GateName
                                    ,IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById
                            FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
		                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation EI1 ON EI.SystemId=IR.AuthorizedBy
                            left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                            Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                            WHERE IR.PlantId='" + identity.PlantId + @"'
                            AND ISNULL(IR.[Status],'')<>'Posting' 
                            AND IR.OpeningBalanceId IS NULL 
                            AND IR.EmployeeId IS NULL 
                            And IR.IsApproved = 1
                            ANd IR.POId is null 
                            AND IR.GRNType='MaterialTransfer' 
                            AND IR.CheckedByStatus IS NULL 
                            AND IR.AuthorizedByStatus='For Approval'

                            UNION ALL
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS IssueDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                        --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
		                            , IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1
                                    ,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
		                            ,isnull(PWG.UserName ,'') GateName
                                    ,IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById
                            FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
		                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation EI1 ON EI.SystemId=IR.AuthorizedBy
                            left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                            Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                            WHERE IR.PlantId='" + identity.PlantId + @"'
                            AND ISNULL(IR.[Status],'')<>'Posting' 
                            AND IR.OpeningBalanceId IS NULL 
                            AND IR.EmployeeId IS NULL 
                            And IR.IsApproved = 1
                            ANd IR.POId is null 
                            AND IR.GRNType='MaterialTransfer' 
                            AND IR.CheckedByStatus IS NULL 
                            AND IR.AuthorizedByStatus IS NULL 
                            )x
                            Order by IssueDate ASC ";



            }
            else if (POTypeStatus == "CheckedHoldRej")
            {
                Sql = @"--DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS IssueDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
                        FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE  (isnull(IR.CheckedByStatus,'')='Hold' Or isnull(IR.CheckedByStatus,'')='Reject') And IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 0 ANd IR.POId is null AND IR.GRNType='MaterialTransfer' Order by IR.GRNDate ASC";

            }
            else if (POTypeStatus == "Checked")
            {
                Sql = @"--DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS IssueDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
                        FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        left  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE  IR.CheckedByStatus='Checked' And IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 0 ANd IR.POId is null AND IR.GRNType='MaterialTransfer' Order by IR.GRNDate ASC";

            }


            else if (POTypeStatus == "For Approval")
            {

                Sql = @"--DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS IssueDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
                        FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE  (isnull(IR.AuthorizedByStatus,'')='Hold' Or isnull(IR. AuthorizedByStatus,'')='Reject')  And IR.CheckedByStatus='Checked' And IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 1 ANd IR.POId is null AND IR.GRNType='MaterialTransfer' Order by IR.GRNDate ASC";

            }
            else if (POTypeStatus == "Approved")
            {

                Sql = @"--DECLARE @plantId VARCHAR(10)='20171';
                            Select * from (
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS IssueDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                        --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
		                            , IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
		                            ,isnull(IR.GateEntryNo,0) GateEntryNo
		                            ,isnull(PWG.UserName ,'') GateName
                            FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
		                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                            left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                            Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                            WHERE  IR. AuthorizedByStatus='Approved' 
                            And IR.CheckedByStatus='Checked' 
                            And IR.PlantId='" + identity.PlantId + @"' 
                            AND ISNULL(IR.[Status],'')<>'Posting' 
                            AND IR.OpeningBalanceId IS NULL 
                            AND IR.EmployeeId IS NULL 
                            And IR.IsApproved = 1 
                            ANd IR.POId is null 
                            AND IR.GRNType='MaterialTransfer' 
                            UNION ALL
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS IssueDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                        --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
		                            , IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
		                            ,isnull(IR.GateEntryNo,0) GateEntryNo
		                            ,isnull(PWG.UserName ,'') GateName
                            FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
		                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                            left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                            Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                            WHERE  IR. AuthorizedByStatus='Approved' 
                            And IR.CheckedByStatus Is NULL
                            And IR.PlantId='" + identity.PlantId + @"'
                            AND ISNULL(IR.[Status],'')<>'Posting' 
                            AND IR.OpeningBalanceId IS NULL 
                            AND IR.EmployeeId IS NULL 
                            And IR.IsApproved = 1 
                            ANd IR.POId is null 
                            AND IR.GRNType='MaterialTransfer' 
                            )x
                            Order by GRNDate ASC";

            }
            else if (POTypeStatus == "Posted")
            {

                Sql = @"--DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS IssueDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
									,MS.UserName as StorageLocation,V.VoucherNo
									,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
									,I.PostingDate
									,I.AddedBy PostedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
                        FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                         LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
                         left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
				  	    left join trn.Voucher V on V.Id=I.VoucherId
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE IR.Status='Posting' 
                        --And IR. AuthorizedByStatus='Approved' And IR.CheckedByStatus='Checked' 
                        And IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')='Posting' 
                        AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL 
                        --And IR.IsApproved = 1 
                        And IR.IsApproved = 1
                        ANd IR.POId is null AND IR.GRNType='MaterialTransfer'
                        --Order by Ir.GRNDate1 ASC
                        ";

            }

            return _sqlRepository.GetDataCollection(Sql);


            //      catch (Exception ex)
            //{
            //    throw new CustomException(ex.Message, ex,
            //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
            //        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            //}
        }




        #region Material Transfer Report

        public void MaterialTransferReport(string CompanyId, string CompanyGroupID, string plantId, string UserId, string grnId)
        {

            var fileName = "";
            var strPath = "";

            var File = "";

            ReportUtility ru = new ReportUtility();

            //tempId = dtLangName.Rows[0]["UserName"].ToString();
            fileName = "MaterialTransfer" + plantId + ".docx";
            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            //makeDictionary();
            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);
            //Gets the paragraph at index 1
            try
            {
                WSection section = document.Sections[0];

                DataTable dtOrderMaster;


                dtOrderMaster = loadGRNMaterialMaster(grnId);

                var invoicePartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["InvoicePartyAddressMasterId"].ToString(), dtOrderMaster.Rows[0]["InvoicingByAddress"].ToString());
                document.Replace("{InvoicingPartyAddress}", invoicePartyAddress, false, false);

                var vendorPartyAddress = ru.GetAddress(dtOrderMaster.Rows[0]["VendorAddressMasterId"].ToString(), "");
                document.Replace("{VendorAddress}", vendorPartyAddress, false, false);

                Dictionary<string, string> columns = new Dictionary<string, string>();

                var poApprovedStatus = "";

                foreach (DataColumn item in dtOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

               // var dsServiceItems = loadGRNServiceMaster(grnId);
                var materialTotal = makeOrderDetailsTable(document, dtOrderMaster, grnId);//Material Details 
                //loadGRNRejectionTable(document, grnId);
                var serviceTotal = 0.00;
                //if (dsServiceItems.Rows.Count > 0)

                //{
                //    document.Replace("{ServiceDetails}", "Service Details", true, true);
                //}
                document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("#,##0.00") + " " + dtOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                document.Replace("{TotalInWords}", ru.InWord((materialTotal + serviceTotal), dtOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();
                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                List<string> strReplace = new List<string>();
                StringCollection strColDistinct = new StringCollection();

                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());             //For Same Name Use
                    string text = strReplace[i].ToUpper();

                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dtOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);
                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);
                }

                //Region that is for Pdf.Document
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Saves the PDF file 
                string Prefix = "MaterialTransfer" + grnId;
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();
            }

            catch (Exception ex)
            {
                throw ex;
            }
            document.Close();
        }

        public DataTable loadGRNMaterialMaster(string OrderMasterID)
        {
            string strSQL;
            try
            {

                strSQL = @"                   SELECT IR.Id grnNumber
                            ,IR.CompanyGroupId
                            ,IR.CompanyId
                            ,Plant.GSTIN
                            ,ir.PODepended
                            ,IR.NoteForAccounts 
                            ,IR.POId PONumber
							,MS.UserName as ToStorageLocation
                            ,REPLACE(Convert(VARCHAR(11), IR.GRNDate, 106), ' ', '-') AS PODate
							 ,GRNType=CASE WHEN IR.GRNType='GRN' then 'GRN Without PO' ELSE 'GRN With PO' END
                            ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                            ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
                            ,IR.InvoicingPartyPlantId
                            ,IRD.TransferedFromGrnId
                            ,INVPARTYPL.UserName InvoicingPartyName
                            ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                            ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                            ,ISNULL(IR.InvoicingByAddress,'') InvoicingByAddress
                            ,IR.DeliveryByAddress
                            ,DPARTYPL.UserName DeliveryParty
                            ,IR.DeliveryPartyPlantId
                            ,IOM.MaterialMasterId
                            ,IR.DocRefNo
                            ,IR.DocDate
                            ,CheckedBy=CASE WHEN IR.CheckedByStatus='Checked' Then eI.EmployeeName else '' END
                            ,AuthorizedBy=CASE When IR.AuthorizedByStatus='Approved'then eI1.EmployeeName else '' END
                            ,AddedBy=CASE When IR.CheckedByStatus='ForChecked' OR IR.CheckedByStatus='Hold' OR IR.CheckedByStatus='Reject' OR IR.CheckedByStatus='Checked'then eI3.EmployeeName else IR.AddedBy END
                            ,IR.AddedDate
                            ,IR.UpdatedBy
                            ,IR.UpdatedDate
                            ,IR.IsApproved
                            ,IR.PartyType
                            ,EMPIN.EmployeeName
                            ,Party.UserName VendorName
                            ,Party.AddressMasterId VendorAddressMasterId
                            ,Party.TINNO VendorGSTIN
                            ,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
                            ,IR.IsNonCreditable
                            ,IR.CurrencyId
                            ,CRNC.Code AS CurrencyName
                            ,IR.ToCurrencyRate
                            ,BASECRNC.Code AS BaseCurrencyName
                            ,PayTerm.UserName PaymentTerm
                            ,MM.UserName MaterialMaster
                            ,MM.MaterialGroupMasterId
                            ,MGM.UserName MaterialGroupMaster
                            ,IOM.ArticleId
                            ,MMA.StandardName Article
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
							
                            ,ROUND(IRD.TransactionQty, 2) POTransactionQty
                            ,ROUND(IRD.MaterialTranRate, 2) TransactionRate
                            ,ROUND((IRD.TransactionQty * IRD.MaterialTranRate), 2) AS TrnAmount
                            ,IRD.TotalMaterialTranAmount BaseAmount
                            ,IRD.TotalTaxAmount AS BaseTaxAmount
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
                            ,IRD.ChargesTranAmount
                            ,IRD.CountryId

                            ,IRD.TransactionUoMId
                            ,TUoM.UserName AS TransactionUoM
                            ,IRD.Id InventoryReceiveDetailId,MRD.MaterialDetail,POD.Description,IRD.Description AS GRDDescrition
                            ,PurOrCheckedStatus= CASE when IR.CheckedByStatus='ForChecked' Then 'To be checked'
                            when IR.CheckedByStatus='Hold' Then 'Hold'
                            when IR.CheckedByStatus='Reject' Then 'Reject'
                            when IR.CheckedByStatus='Checked' Then 'Checked'
                            else ''

                            END
                            ,PurOrApprovedStatus= CASE
                            when IR.AuthorizedByStatus='Reject' Then 'Reject For Approved'
                            when IR.AuthorizedByStatus='Hold' Then 'Hold For Approved'
                            when IR.AuthorizedByStatus='For Approval' Then 'To be Approval'
                            when IR.AuthorizedByStatus='Approved' Then 'Approved'
                            else ''
                            END

                            FROM TRN.InventoryReceive IR
                            LEFT JOIN Hkp.MaterialStorage MS ON MS.Id = IR.MaterialStorageId

                            LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                            LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                            LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                            LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                            LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = IR.BaseCurrencyId
                            LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                            LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                            LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
                            LEFT JOIN trn.inventoryReceiveDetail IRD ON IR.Id = IRD.InventoryReceiveId
                            LEFT JOIN HKP.Party Party ON Party.Id = IR.PartyId
                            LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                            LEFT JOIN MST.MaterialMaster AS MM ON MM.Id = IOM.MaterialMasterId
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MGM.Id = MM.MaterialGroupMasterId
                            LEFT JOIN MST.MaterialMasterArticle AS MMA ON MMA.Id = IOM.ArticleId
                            LEFT JOIN HKP.Characteristics AS FC ON IOM.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IOM.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IOM.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IOM.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IOM.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IOM.ThirdCharacteristicsValueId = TCV.Id
                            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                            LEFT JOIN trn.PurchaseOrderDetail POD ON POD.Id = IRD.PODetailsId
                            Left Join TRN.MaterialRequsitionDetails MRD ON MRD.Id=POD.RequisitionDetailId
                            LEFT JOIN EmployeeInformation AS EMPIN ON EMPIN.SystemId= IR.EmployeeId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                            left join [SEC].[User] U on U.UserId=IR.AddedBy
                            LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=U.EmployeeId
                            WHERE IR.Id ='" + OrderMasterID + @"' and IOM.MaterialMasterId is not NULL

                            Union ALL
                            SELECT IR.Id grnNumber
                            ,IR.CompanyGroupId
                            ,IR.CompanyId
                            ,Plant.GSTIN
                            ,ir.PODepended
                            ,IR.NoteForAccounts 
                            ,IR.POId PONumber
							,MS.UserName as ToStorageLocation
                            ,REPLACE(Convert(VARCHAR(11), IR.GRNDate, 106), ' ', '-') AS PODate
							,GRNType=CASE WHEN IR.GRNType='GRN' then 'GRN Without PO' ELSE 'GRN With PO' END
                            ,REPLACE(Convert(VARCHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate
                            ,REPLACE(Convert(VARCHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
                            ,IR.InvoicingPartyPlantId
                            ,IRD.TransferedFromGrnId
                            ,INVPARTYPL.UserName InvoicingPartyName
                            ,INVPARTYPL.AddressMasterId InvoicePartyAddressMasterId
                            ,INVPARTYPL.GSTIN InvoicingPartyGSTIN
                            ,ISNULL(IR.InvoicingByAddress,'') InvoicingByAddress
                            ,IR.DeliveryByAddress
                            ,DPARTYPL.UserName DeliveryParty
                            ,IR.DeliveryPartyPlantId
                            ,IOM.MaterialMasterId
                            ,IR.DocRefNo
                            ,IR.DocDate
                            ,CheckedBy=CASE WHEN IR.CheckedByStatus='Checked' Then eI.EmployeeName else '' END
                            ,AuthorizedBy=CASE When IR.AuthorizedByStatus='Approved'then eI1.EmployeeName else '' END
                            ,AddedBy=CASE When IR.CheckedByStatus='ForChecked' OR IR.CheckedByStatus='Hold' OR IR.CheckedByStatus='Reject' OR IR.CheckedByStatus='Checked'then eI3.EmployeeName else IR.AddedBy END
                            ,IR.AddedDate
                            ,IR.UpdatedBy
                            ,IR.UpdatedDate
                            ,IR.IsApproved
                            ,IR.PartyType
                            ,EMPIN.EmployeeName
                            ,Party.UserName VendorName
                            ,Party.AddressMasterId VendorAddressMasterId
                            ,Party.TINNO VendorGSTIN
                            ,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
                            ,IR.IsNonCreditable
                            ,IR.CurrencyId
                            ,CRNC.Code AS CurrencyName
                            ,IR.ToCurrencyRate
                            ,BASECRNC.Code AS BaseCurrencyName
                            ,PayTerm.UserName PaymentTerm
                            ,'-' MaterialMaster
                            ,'-' MaterialGroupMasterId
                            ,'-' MaterialGroupMaster
                            ,IOM.ArticleId
                            ,'-' Article
                            ,'-' FirstCharId
                            ,'-' FirstChar
                            ,IOM.FirstCharacteristicsValueId
                            ,'' AS FirstCharacteristicsValue
                            ,IOM.SecondCharacteristicsValueId
                            ,'' AS SecondCharacteristicsValue
                            ,IOM.ThirdCharacteristicsValueId
                            ,'' AS ThirdCharacteristicsValue
                            ,'' SecondCharId
                            ,'' SecondChar
                            ,'' ThirdCharId
                            ,'' ThirdChar
                            ,ROUND(IRD.TransactionQty, 2) POTransactionQty
                            ,ROUND(IRD.MaterialTranRate, 2) TransactionRate
                            ,ROUND((IRD.TransactionQty * IRD.MaterialTranRate), 2) AS TrnAmount
                            ,IRD.TotalMaterialTranAmount BaseAmount
                            ,IRD.TotalTaxAmount AS BaseTaxAmount
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
                            ,IRD.ChargesTranAmount
                            ,IRD.CountryId

                            ,IRD.TransactionUoMId
                            ,TUoM.UserName AS TransactionUoM
                            ,IRD.Id InventoryReceiveDetailId,MRD.MaterialDetail,POD.Description,IRD.Description AS GRDDescrition
                            ,PurOrCheckedStatus= CASE when IR.CheckedByStatus='ForChecked' Then 'To be checked'
                            when IR.CheckedByStatus='Hold' Then 'Hold'
                            when IR.CheckedByStatus='Reject' Then 'Reject'
                            when IR.CheckedByStatus='Checked' Then 'Checked'
                            else ''

                            END
                            ,PurOrApprovedStatus= CASE
                            when IR.AuthorizedByStatus='Reject' Then 'Reject For Approved'
                            when IR.AuthorizedByStatus='Hold' Then 'Hold For Approved'
                            when IR.AuthorizedByStatus='For Approval' Then 'To be Approval'
                            when IR.AuthorizedByStatus='Approved' Then 'Approved'
                            else ''
                            END

                            FROM TRN.InventoryReceive IR

                            LEFT JOIN Hkp.MaterialStorage MS ON MS.Id = IR.MaterialStorageId
                            LEFT JOIN ORG.CompanyGroup CGroup ON CGroup.Id = IR.CompanyGroupId
                            LEFT JOIN ORG.Company Cmp ON Cmp.Id = IR.CompanyId
                            LEFT JOIN ORG.Plant Plant ON Plant.Id = IR.PlantId
                            LEFT JOIN SCS.Currency CRNC ON CRNC.Id = IR.CurrencyId
                            LEFT JOIN SCS.Currency BASECRNC ON BASECRNC.Id = IR.BaseCurrencyId
                            LEFT JOIN MST.PaymentTerm PayTerm ON PayTerm.Id = IR.PaymentTermId
                            LEFT JOIN HKP.PartyPlant INVPARTYPL ON INVPARTYPL.Id = IR.InvoicingPartyPlantId
                            LEFT JOIN HKP.PartyPlant DPARTYPL ON DPARTYPL.Id = IR.DeliveryPartyPlantId
                            LEFT JOIN trn.inventoryReceiveDetail IRD ON IR.Id = IRD.InventoryReceiveId
                            LEFT JOIN HKP.Party Party ON Party.Id = IR.PartyId
                            LEFT JOIN trn.PurchaseOrderDetail POD ON POD.Id = IRD.PODetailsId
                            Left Join TRN.MaterialRequsitionDetails MRD ON MRD.Id=POD.RequisitionDetailId
                            JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId = TUoM.Id
                            LEFT JOIN trn.InventoryMaterial AS IOM ON IRD.InventoryMaterialId = IOM.Id
                            LEFT JOIN EmployeeInformation AS EMPIN ON EMPIN.SystemId= IR.EmployeeId
                            LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IR.AuthorizedBy
                            left join [SEC].[User] U on U.UserId=IR.AddedBy
                            LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=U.EmployeeId
		                    LEFT JOIN(
									select PDAMAP.GRNId
									,PoId=STUFF((select distinct ','+xpo.Id from
									trn.PurchaseOrder xpo
									INNER JOin TRN.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.PoId
									where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									from TRN.POGGRNMap PDAMAP
									LEFT JOIN [TRN].PurchaseOrder IR ON IR.Id = PDAMAP.PoId
									--where PDAMAP.GRNId='2020463'
									group by PDAMAP.GRNId

									)PO ON PO.GRNId = IRD.InventoryReceiveId
                            WHERE IR.Id ='" + OrderMasterID + "' and IOM.MaterialMasterId is NULL";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public double makeOrderDetailsTable(WordDocument document, DataTable dsOrderMaster, string grnId)
        {
            string replaceString = "{materialItems}";
            ReportUtility ru = new ReportUtility();
            DataTable dsOrderItems, dsTax;
            dsOrderItems = loadOrderMasterItems(grnId);
            //dsTax = loadOrderMasterTax(grnId);
            int LasColumnIndex = 11;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            //DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));
            
            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            wTable.TableFormat.IsAutoResized = true;

            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();
            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("RowId");
            range.ApplyCharacterFormat(FontBold);
            int colRowId = COL; COL++;
            wTable.Rows[ROW].Cells[colRowId].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Materials");
            range.ApplyCharacterFormat(FontBold);
            int colMaterialGroup = COL; COL++;
            wTable.Rows[ROW].Cells[colMaterialGroup].Width = 90;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article ");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 90;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colChar1 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar1].Width = 45;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colChar2 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar2].Width = 45;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colChar3 = COL; COL++;
            wTable.Rows[ROW].Cells[colChar3].Width = 45;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Po Material Detail");
            range.ApplyCharacterFormat(FontBold);
            int colDescription = COL; COL++;
            wTable.Rows[ROW].Cells[colDescription].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("GRN Material Detail");
            range.ApplyCharacterFormat(FontBold);
            int colGRNMaterialDetail = COL; COL++;
            wTable.Rows[ROW].Cells[colGRNMaterialDetail].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            //wTable.Rows[ROW].Cells[colQty].Width = 70;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate (" + dsOrderMaster.Rows[0]["CurrencyName"].ToString() + ")");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL; COL++;
            //wTable.Rows[ROW].Cells[colRate].Width = 70;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUoM = COL++;
            wTable.Rows[ROW].Cells[colUoM].Width = 40;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Amount");
            range.ApplyCharacterFormat(FontBold);
            int colATRN = COL; 
            wTable.Rows[ROW].Cells[colUoM].Width = 70;

          
            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                }

                TROW.Cells[colRowId].AddParagraph().AppendText(dsOrderMaster.Rows[i]["InventoryReceiveDetailId"].ToString());
                TROW.Cells[colMaterialGroup].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMaster"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString());
                TROW.Cells[colChar1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());
                TROW.Cells[colChar2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());
                TROW.Cells[colChar3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());
                TROW.Cells[colDescription].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Description"].ToString());
                TROW.Cells[colGRNMaterialDetail].AddParagraph().AppendText(dsOrderMaster.Rows[i]["GRDDescrition"].ToString());
                TROW.Cells[colQty].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["POTransactionQty"].ToString()).ToString("F2"));
                TROW.Cells[colRate].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TransactionRate"].ToString()).ToString("F2"));
                TROW.Cells[colUoM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["TransactionUoM"].ToString().ToString());

                TROW.Cells[colATRN].AddParagraph().AppendText(clsStaticInfo.dbl(dsOrderMaster.Rows[i]["TrnAmount"].ToString()).ToString("#,##0.00"));
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colRate ||C== colMaterialGroup || C == colArticle || C == colChar1 || C == colChar2 || C == colChar3 || /*C == colMaterialDetail ||*/ C == colDescription || C == colGRNMaterialDetail || C == colRowId || C == colRate || C == colUoM || dicTaxes.ContainsValue(C))
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStaticInfo.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
            }
            #endregion Total

            ROW++;
            #region Sub Total
            
                double total = clsStaticInfo.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2") + " (" + ru.InWord(total, dsOrderMaster.Rows[0]["CurrencyId"].ToString()) + ")");

            #endregion Total

            ROW++;
            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            //myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }

            IWParagraphStyle myStyleRightAlign = document.AddParagraphStyle("MyStyleRightAlign");
            //Sets the formatting of the style
            myStyleRightAlign.CharacterFormat.FontSize = 8f;
            myStyleRightAlign.CharacterFormat.TextColor = Color.Black;
            myStyleRightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;



            for (int R = 1; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];



                foreach (WParagraph item in TROW.Cells[colQty].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }


                foreach (WParagraph item in TROW.Cells[colRate].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }

                foreach (WParagraph item in TROW.Cells[colATRN].Paragraphs)
                {
                    item.ApplyStyle("MyStyleRightAlign");
                }

            }

            #endregion paragrpath formats

            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            //for (int i = 0; i < dv.Count; i++)
            //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            //for (int i = 0; i <= colTotalTaxableAmount; i++)
            //	wTable.ApplyVerticalMerge(i, ROW - 1, ROW);

            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section

            #endregion merging section

            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            return total;
        }

        public DataTable loadOrderMasterItems(string OrderMasterID)
        {
            string strSQL;

            try
            {
                strSQL = @"select so.MasterOrderItemId,so.Id AS SOID,CONCAT( mm.[Description],' ',a.StandardName) AS MaterialDesc,
                                so.Qty,uom.UserName AS UOM,SO.Rate,so.Qty*so.Rate AS Amount,isnull(SO.Discount,0) AS Discount
                                  from [TRN].[MasterOrderItem] T
                                INNER JOIN [TRN].[MasterOrder] O ON o.Id=t.MasterOrderId
                                INNER JOIN [TRN].[SalesOrder]  SO ON so.MasterOrderItemId=t.Id
                                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=t.MaterialMasterId
                                LEFT OUTER JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=mm.MaterialGroupMasterId
                                LEFT OUTER JOIN [MST].[MaterialMasterArticle] A ON a.Id=t.ArticleId
                                LEFT OUTER JOIN [SCS].[UnitOfMeasurement] UOM ON uom.Id=o.TotalQtyUOMId
                                where MasterOrderId='" + OrderMasterID + "'";

                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }


    #endregion merging section
      

    }


}
