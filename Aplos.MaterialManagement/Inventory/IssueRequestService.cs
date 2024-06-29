#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using Library.Model.Products;
using Library.Service.Core;
using Library.Service.Enums;
using Library.MaterialManagement.Inventory;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Collections.Specialized;
using Library.ViewModel.Materials;
using System.Data;
using Aplos.MaterialManagement.MaterialQuery;
using Library.Service.Extension;

#endregion Using

namespace Library.MaterialManagement.Products
{
    public class IssueRequestService : Service<IssueRequest>, IIssueRequestService
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<PurchaseOrderGroup> _purchaseOrderGroupMaster;
        private readonly IRepositoryAsync<IssueRequest> _IssueRequest;
        private readonly IRepositoryAsync<PurchaseOrderGroupDetails> _purchaseOrderGroupDetails;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<IssueRequest> _issueRequestRepository;
        private readonly IRepositoryAsync<IssueRequestMaster> _issueRequestMasterRepository;
        private readonly IIssueRequestMasterService _issueRequestMasterService;

        private readonly IRepositoryAsync<IssueRequestMasterSalesOrderMap> _issueRequestMasterSalesOrderMap;
        private readonly IRepositoryAsync<IssueRequestMasterProcessMap> _issueRequestMasterProcessMap;
        private readonly IRepositoryAsync<IssueRequestSKUMap> _issueRequestSKUMap;
        private readonly IRepositoryAsync<IssueRequestBOQMap> _issueRequestBOQMap;
        private readonly IInventoryMaterialService _inventoryMaterialMasterService;

        public IssueRequestService(
             IRepositoryAsync<PurchaseOrderGroup> purchaseOrderGroupMaster
             , IRepositoryAsync<IssueRequest> issueRequestRepository
            , IRepositoryAsync<IssueRequest> issueRequest
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<PurchaseOrderGroupDetails> purchaseOrderGroupDetails
            , IIssueRequestMasterService issueRequestMasterService
            , IRepositoryAsync<IssueRequestMaster> issueRequestMasterRepository
            , IRepositoryAsync<IssueRequestMasterSalesOrderMap> issueRequestMasterSalesOrderMap
            , IRepositoryAsync<IssueRequestMasterProcessMap> issueRequestMasterProcessMap
            , IRepositoryAsync<IssueRequestSKUMap> issueRequestSKUMap
            , IRepositoryAsync<IssueRequestBOQMap> issueRequestBOQMap
            , IInventoryMaterialService inventoryMaterialMasterService
            ) : base(issueRequest, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _purchaseOrderGroupMaster = purchaseOrderGroupMaster;
            _IssueRequest = issueRequest;
            _purchaseOrderGroupDetails = purchaseOrderGroupDetails;
            _issueRequestRepository = issueRequestRepository;
            _issueRequestMasterService = issueRequestMasterService;
            _issueRequestMasterRepository = issueRequestMasterRepository;
            _issueRequestMasterSalesOrderMap = issueRequestMasterSalesOrderMap;
            _issueRequestMasterProcessMap = issueRequestMasterProcessMap;
            _issueRequestSKUMap = issueRequestSKUMap;
            _issueRequestBOQMap = issueRequestBOQMap;
            _inventoryMaterialMasterService = inventoryMaterialMasterService;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(PurchaseOrderGroup), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        private string GetIssueRequestMasterSalesOrderMapPK()
        {
            return GetAutoNumber(nameof(IssueRequestMasterSalesOrderMap), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        private string GetIssueRequestMasterProcessMapPK()
        {
            return GetAutoNumber(nameof(IssueRequestMasterProcessMap), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        private string GetIssueRequestSKUMapPK()
        {
            return GetAutoNumber(nameof(IssueRequestSKUMap), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        #region IssueSlip
        private string GetPK1()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(IssueRequest), out sID);
            return sID;
        }
        public void InsertOrUpdateGraphIssueSlipCreate(IssueRequestMaster Issentry, IEnumerable<IssueRequestViewModel> entity, IEnumerable<IssueRequestViewModel> entityGroupData, string IssueSlipType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti
            , IEnumerable<IssueRequestViewModel> SOListSelectedNew, IEnumerable<IssueRequestViewModel> MaterialColorListNew, string ProcessId
            , List<Dictionary<string, object>> machinepopUpDataList)
        {
            var flag = false;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                _unitOfWork.BeginTransaction();
                var currentId1 = _issueRequestRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[IssueRequest]  WHERE IssueRequestMasterId ='{Issentry.Id}'").First();
                var currentBOQMapId = currentId1;
                if (identity.EmployeeId == Issentry.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                {

                    Issentry.AuthorizedBy = Issentry.CheckedBy;
                    Issentry.AuthorizedByStatus = "For Approval";
                    Issentry.CheckedBy = null;
                    Issentry.CheckedByStatus = null;
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                {
                    Issentry.CheckedByStatus = null;
                    Issentry.AuthorizedByStatus = null;
                    Issentry.CheckedBy = null;
                    Issentry.AuthorizedBy = null;
                }
                else
                {
                    Issentry.CheckedBy = Issentry.CheckedBy;
                    Issentry.CheckedByStatus = "ForChecked";
                    Issentry.AuthorizedBy = null;
                    Issentry.AuthorizedByStatus = null;

                }
                if (!string.IsNullOrEmpty(identity.EmployeeId))
                    Issentry.Preparedby = identity.EmployeeId;

                Issentry.IssueSlipType = IssueSlipType;

                _issueRequestMasterService.Insert(Issentry);
                if (SOListSelectedNew.IsNotNull())
                {
                    foreach (var item in SOListSelectedNew)
                    {
                        var itemD = new IssueRequestMasterSalesOrderMap
                        {
                            Id = GetIssueRequestMasterSalesOrderMapPK(),
                            IssueRequestMasterId = Issentry.Id,
                            SalesOrderId = item.SalesOrderId,
                            ModelState = ModelState.Added
                        };
                        AuditService.AddedLog(itemD);
                        _issueRequestMasterSalesOrderMap.Insert(itemD);

                    }


                    var itemcolorD = new IssueRequestMasterProcessMap
                    {
                        Id = GetIssueRequestMasterProcessMapPK(),
                        IssueRequestMasterId = Issentry.Id,
                        ProcessId = ProcessId,
                        ModelState = ModelState.Added
                    };


                    AuditService.AddedLog(itemcolorD);
                    _issueRequestMasterProcessMap.Insert(itemcolorD);

                    foreach (var itemcolor in MaterialColorListNew)
                    {
                        var SKUMapD = new IssueRequestSKUMap
                        {
                            Id = GetIssueRequestSKUMapPK(),
                            IssueRequestMasterId = Issentry.Id,
                            FirstCharacteristicsValueId = itemcolor.FirstCharacteristicsValueId,
                            SecondCharacteristicsValueId = itemcolor.SecondCharacteristicsValueId,
                            ThirdCharacteristicsValueId = itemcolor.ThirdCharacteristicsValueId,
                            RequisitionForQty = itemcolor.RequisitionForQty,
                            MaterialMasterId = itemcolor.MaterialMasterId,
                            ArticleId = itemcolor.ArticleId,
                            SalesOrderId = itemcolor.SalesOrderId,
                            OrderQty = itemcolor.OrderQty,
                            PlanOrderQty = itemcolor.PlanOrderQty,
                            Destination = itemcolor.Destination,
                            PONumber = itemcolor.PONumber,
                            PODate = itemcolor.PODate,
                            ModelState = ModelState.Added
                        };
                        AuditService.AddedLog(SKUMapD);
                        _issueRequestSKUMap.Insert(SKUMapD);
                    }
                }
                var slipDetailId = "";
                var Material = "";
                var Article = "";
                var SKU1 = "";
                var SKU2 = "";
                var SKU3 = "";
                var SalesOrderId = "";
                var TransactionUoMId = "";
                flag = true;
                //List<IDictionary<string, object>> newMachineAllocationDb = new List<IDictionary<string, object>>();
                foreach (var itemDetail in entityGroupData)
                {
                    if (string.IsNullOrEmpty(itemDetail.Id))
                    {
                        var NewId = Issentry.Id + "-";
                        currentId1++;
                        //grndId = NewId + currentId1;
                        var IssueRequstD = new IssueRequest
                        {
                            Id = NewId + currentId1,
                            IssueRequestMasterId = Issentry.Id,
                            RequisitionId = itemDetail.RequisitionNo,
                            RequisitionDetailId = itemDetail.RequisitionDetailId,
                            CostCenterId = itemDetail.CostCenterId,
                            ExpenseActivityId = itemDetail.ExpenseActivityId,
                            RequestedQty = Convert.ToDecimal(itemDetail.RequestedQty),
                            RejectedQty = itemDetail.RejectedQty,
                            BudgetMasterId = itemDetail.BudgetMasterId,
                            GLGeneralInfoId = itemDetail.GLGeneralInfoId,
                            MaterialMasterId = itemDetail.MaterialMasterId,
                            ArticleId = itemDetail.ArticleId,
                            FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                            FirstCharacteristicsValueId = itemDetail.BOQDFirstCharacteristicsValueId,
                            SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                            SecondCharacteristicsValueId = itemDetail.BOQDSecondCharacteristicsValueId,
                            ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                            ThirdCharacteristicsValueId = itemDetail.BOQDThirdCharacteristicsValueId,
                            TransactionUoMId = itemDetail.TransactionUoMId,
                            InventoryMaterialId = itemDetail.InventoryMaterialId,
                            MaterialIssueControlDetailId = itemDetail.MaterialIssueControlDetailId,
                            CountryId = itemDetail.CountryId
                        };
                        try
                        {



                            if (string.IsNullOrEmpty(itemDetail.InventoryMaterialId))
                            {
                                var inventoryMaterialData = _inventoryMaterialMasterService.Query(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId &&
                                r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId).Select(r => r.Id).FirstOrDefault();


                                IssueRequstD.InventoryMaterialId = inventoryMaterialData;

                                if (string.IsNullOrEmpty(inventoryMaterialData))
                                {
                                    InventoryMaterialViewModel inventoryMaterial = new InventoryMaterialViewModel();

                                    inventoryMaterial.Id = null;
                                    inventoryMaterial.CountryId = itemDetail.CountryId;
                                    inventoryMaterial.CompanyGroupId = identity.CompanyGroupId;
                                    inventoryMaterial.CompanyId = identity.CompanyId;
                                    inventoryMaterial.PlantId = identity.PlantId;
                                    inventoryMaterial.MaterialStorageId = null;
                                    inventoryMaterial.OpeningBalanceId = null;
                                    inventoryMaterial.MaterialMasterId = itemDetail.MaterialMasterId;
                                    inventoryMaterial.ArticleId = itemDetail.ArticleId;
                                    inventoryMaterial.FirstCharacteristicsId = itemDetail.FirstCharacteristicsId;
                                    inventoryMaterial.FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId;
                                    inventoryMaterial.SecondCharacteristicsId = itemDetail.SecondCharacteristicsId;
                                    inventoryMaterial.SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId;
                                    inventoryMaterial.ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId;
                                    inventoryMaterial.ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId;
                                    inventoryMaterial.TotalQty = 0;
                                    inventoryMaterial.AvgRate = 0;
                                    inventoryMaterial.ShortageQty = 0;
                                    inventoryMaterial.RejectionQty = 0;
                                    inventoryMaterial.ApprovedQty = 0;


                                    _inventoryMaterialMasterService.InsertOrUpdateFromReceive(inventoryMaterial);
                                    IssueRequstD.InventoryMaterialId = inventoryMaterial.InventoryMaterialId;

                                }

                            }

                            AuditService.AddedLog(IssueRequstD);
                            _issueRequestRepository.Insert(IssueRequstD);

                            slipDetailId = IssueRequstD.Id;
                            Material = IssueRequstD.MaterialMasterId;
                            Article = IssueRequstD.ArticleId;
                            SKU1 = IssueRequstD.FirstCharacteristicsValueId;
                            SKU2 = IssueRequstD.SecondCharacteristicsValueId;
                            SKU3 = IssueRequstD.ThirdCharacteristicsValueId;
                            SalesOrderId = itemDetail.SalesOrderId;
                            TransactionUoMId = IssueRequstD.TransactionUoMId;

                            if (machinepopUpDataList != null)
                            {
                                foreach (var item in machinepopUpDataList.Where(r => r["MaterialMasterId"].ToString() == itemDetail.MaterialMasterId
                                                            && r["ArticleId"].ToString() == itemDetail.ArticleId
                                                            ).ToList())
                                {
                                    item["IssueRequestId"] = IssueRequstD.Id;
                                }
                            }

                        }
                        catch (DivideByZeroException ex)
                        {

                        }
                        finally
                        {

                        }
                    }
                    var FilterentityData = entity.Where(r => r.MaterialMasterId == Material && r.ArticleId == Article && r.BOQDFirstCharacteristicsValueId == SKU1 && r.BOQDSecondCharacteristicsValueId == SKU2 && r.BOQDThirdCharacteristicsValueId == SKU3 && r.SalesOrderId == itemDetail.SalesOrderId && r.TransactionUoMId == TransactionUoMId).ToList();
                    foreach (var itemDetailentity in FilterentityData)
                    {

                        // Insert in receive detail
                        if (string.IsNullOrEmpty(itemDetailentity.Id))
                        {
                            var NewId = Issentry.Id + "-";
                            currentBOQMapId++;
                            //grndId = NewId + currentId1;
                            var IssueRequestBOQMap = new IssueRequestBOQMap
                            {
                                Id = NewId + currentBOQMapId,
                                IssueRequestDetailId = slipDetailId,
                                BOQID = itemDetailentity.BOQId,
                                Qty = Convert.ToDecimal(itemDetailentity.RequestedQty)
                            };
                            try
                            {
                                AuditService.AddedLog(IssueRequestBOQMap);
                                _issueRequestBOQMap.Insert(IssueRequestBOQMap);
                            }
                            catch (DivideByZeroException ex)
                            {

                            }
                            finally
                            {

                            }
                        }
                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                if (machinepopUpDataList != null)
                {
                    SaveIssueMaterailMachineAllocation(machinepopUpDataList);
                }
            }

            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public void CreateOrUpdateMaterialControlIssueSlip(IssueRequestMaster Issentry, IEnumerable<IssueRequestViewModel> entity, IEnumerable<IssueRequestViewModel> entityGroupData, string IssueSlipType, IEnumerable<IssueRequestViewModel> SOListSelectedNew)
        {
            var flag = false;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                _unitOfWork.BeginTransaction();
                var currentId1 = _issueRequestRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[IssueRequest]  WHERE IssueRequestMasterId ='{Issentry.Id}'").First();
                var currentBOQMapId = currentId1;
                if (identity.EmployeeId == Issentry.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }

                else
                {
                    Issentry.CheckedBy = Issentry.CheckedBy;
                    Issentry.CheckedByStatus = "ForChecked";
                    Issentry.AuthorizedBy = null;
                    Issentry.AuthorizedByStatus = null;

                }
                if (!string.IsNullOrEmpty(identity.EmployeeId))
                    Issentry.Preparedby = identity.EmployeeId;

                Issentry.IssueSlipType = IssueSlipType;

                _issueRequestMasterService.Insert(Issentry);

                var slipDetailId = "";
                var Material = "";
                var Article = "";
                var SKU1 = "";
                var SKU2 = "";
                var SKU3 = "";
                var SalesOrderId = "";
                var TransactionUoMId = "";
                flag = true;
                //List<IDictionary<string, object>> newMachineAllocationDb = new List<IDictionary<string, object>>();
                foreach (var itemDetail in entityGroupData)
                {
                    if (string.IsNullOrEmpty(itemDetail.Id))
                    {
                        var NewId = Issentry.Id + "-";
                        currentId1++;
                        //grndId = NewId + currentId1;
                        var IssueRequstD = new IssueRequest
                        {
                            Id = NewId + currentId1,
                            IssueRequestMasterId = Issentry.Id,
                            RequisitionId = itemDetail.RequisitionNo,
                            RequisitionDetailId = itemDetail.RequisitionDetailId,
                            CostCenterId = itemDetail.CostCenterId,
                            ExpenseActivityId = itemDetail.ExpenseActivityId,
                            RequestedQty = Convert.ToDecimal(itemDetail.RequestedQty),
                            RejectedQty = itemDetail.RejectedQty,
                            BudgetMasterId = itemDetail.BudgetMasterId,
                            GLGeneralInfoId = itemDetail.GLGeneralInfoId,
                            MaterialMasterId = itemDetail.MaterialMasterId,
                            ArticleId = itemDetail.ArticleId,
                            FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                            FirstCharacteristicsValueId = itemDetail.BOQDFirstCharacteristicsValueId,
                            SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                            SecondCharacteristicsValueId = itemDetail.BOQDSecondCharacteristicsValueId,
                            ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                            ThirdCharacteristicsValueId = itemDetail.BOQDThirdCharacteristicsValueId,
                            TransactionUoMId = itemDetail.TransactionUoMId,
                            InventoryMaterialId = itemDetail.InventoryMaterialId,
                            MaterialIssueControlDetailId = itemDetail.MaterialIssueControlDetailId,
                            CountryId = itemDetail.CountryId
                        };
                        try
                        {



                            if (string.IsNullOrEmpty(itemDetail.InventoryMaterialId))
                            {
                                var inventoryMaterialData = _inventoryMaterialMasterService.Query(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId &&
                                r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId).Select(r => r.Id).FirstOrDefault();


                                IssueRequstD.InventoryMaterialId = inventoryMaterialData;

                                if (string.IsNullOrEmpty(inventoryMaterialData))
                                {
                                    InventoryMaterialViewModel inventoryMaterial = new InventoryMaterialViewModel();

                                    inventoryMaterial.Id = null;
                                    inventoryMaterial.CountryId = itemDetail.CountryId;
                                    inventoryMaterial.CompanyGroupId = identity.CompanyGroupId;
                                    inventoryMaterial.CompanyId = identity.CompanyId;
                                    inventoryMaterial.PlantId = identity.PlantId;
                                    inventoryMaterial.MaterialStorageId = null;
                                    inventoryMaterial.OpeningBalanceId = null;
                                    inventoryMaterial.MaterialMasterId = itemDetail.MaterialMasterId;
                                    inventoryMaterial.ArticleId = itemDetail.ArticleId;
                                    inventoryMaterial.FirstCharacteristicsId = itemDetail.FirstCharacteristicsId;
                                    inventoryMaterial.FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId;
                                    inventoryMaterial.SecondCharacteristicsId = itemDetail.SecondCharacteristicsId;
                                    inventoryMaterial.SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId;
                                    inventoryMaterial.ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId;
                                    inventoryMaterial.ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId;
                                    inventoryMaterial.TotalQty = 0;
                                    inventoryMaterial.AvgRate = 0;
                                    inventoryMaterial.ShortageQty = 0;
                                    inventoryMaterial.RejectionQty = 0;
                                    inventoryMaterial.ApprovedQty = 0;


                                    _inventoryMaterialMasterService.InsertOrUpdateFromReceive(inventoryMaterial);
                                    IssueRequstD.InventoryMaterialId = inventoryMaterial.InventoryMaterialId;

                                }

                            }

                            AuditService.AddedLog(IssueRequstD);
                            _issueRequestRepository.Insert(IssueRequstD);

                            slipDetailId = IssueRequstD.Id;
                            Material = IssueRequstD.MaterialMasterId;
                            Article = IssueRequstD.ArticleId;
                            SKU1 = IssueRequstD.FirstCharacteristicsValueId;
                            SKU2 = IssueRequstD.SecondCharacteristicsValueId;
                            SKU3 = IssueRequstD.ThirdCharacteristicsValueId;
                            SalesOrderId = itemDetail.SalesOrderId;
                            TransactionUoMId = IssueRequstD.TransactionUoMId;



                        }
                        catch (DivideByZeroException ex)
                        {

                        }
                        finally
                        {

                        }
                    }
                    var FilterentityData = entity.Where(r => r.MaterialMasterId == Material && r.ArticleId == Article && r.BOQDFirstCharacteristicsValueId == SKU1 && r.BOQDSecondCharacteristicsValueId == SKU2 && r.BOQDThirdCharacteristicsValueId == SKU3 && r.SalesOrderId == itemDetail.SalesOrderId && r.TransactionUoMId == TransactionUoMId).ToList();
                    foreach (var itemDetailentity in FilterentityData)
                    {

                        // Insert in receive detail
                        if (string.IsNullOrEmpty(itemDetailentity.Id))
                        {
                            var NewId = Issentry.Id + "-";
                            currentBOQMapId++;
                            //grndId = NewId + currentId1;
                            var IssueRequestBOQMap = new IssueRequestBOQMap
                            {
                                Id = NewId + currentBOQMapId,
                                IssueRequestDetailId = slipDetailId,
                                BOQID = itemDetailentity.BOQId,
                                Qty = Convert.ToDecimal(itemDetailentity.RequestedQty)
                            };
                            try
                            {
                                AuditService.AddedLog(IssueRequestBOQMap);
                                _issueRequestBOQMap.Insert(IssueRequestBOQMap);
                            }
                            catch (DivideByZeroException ex)
                            {

                            }
                            finally
                            {

                            }
                        }
                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

            }

            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public void SaveIssueMaterailMachineAllocation(List<Dictionary<string, object>> dataList)
        {
            try
            {
                string TableNameHead = "TRN.IssueMaterailMachineAllocation";
                DataSet dsMaster;
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from TRN.IssueMaterialMachineAllocation where 1=2", out dsMaster, false, "1");
                string _Id = "";
                int ccount = 0;
                if (dataList != null)
                {
                    foreach (var item in dataList)
                    {
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'"; if (dv.Count == 0)
                        {
                            ccount++; string id = MakePK(item["IssueRequestId"].ToString(), ccount, 2);
                            item["Id"] = id;
                            materialCommonService.AddNewRowD(dsMaster.Tables[0], item);
                        }
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void InsertOrUpdateGraphIssueSlipUpdate(IssueRequestMaster Issentity, IEnumerable<IssueRequestViewModel> entity, string Ids, string IssueSlipType, string CheckedByStatusForNoti, string ApprovedByStatusForNoti)
        {
            var flag = false;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                _unitOfWork.BeginTransaction();

                if (identity.EmployeeId == Issentity.CheckedBy)
                {
                    throw new CustomException("Please select another employee for Check by.");
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "True")
                {

                    Issentity.AuthorizedBy = Issentity.CheckedBy;
                    Issentity.AuthorizedByStatus = "For Approval";
                    Issentity.CheckedBy = null;
                    Issentity.CheckedByStatus = null;
                }
                else if (CheckedByStatusForNoti == "False" && ApprovedByStatusForNoti == "False")
                {
                    Issentity.CheckedByStatus = null;
                    Issentity.AuthorizedByStatus = null;
                    Issentity.CheckedBy = null;
                    Issentity.AuthorizedBy = null;
                }
                else
                {
                    Issentity.CheckedBy = Issentity.CheckedBy;
                    Issentity.CheckedByStatus = "ForChecked";
                    Issentity.AuthorizedBy = null;
                    Issentity.AuthorizedByStatus = null;

                }

                Issentity.Preparedby = identity.EmployeeId;
                Issentity.IssueSlipType = IssueSlipType;

                Issentity.Preparedby = identity.EmployeeId;
                Issentity.IssueSlipType = IssueSlipType;
                Issentity.UpdatedBy = identity.UserId;



                _issueRequestMasterService.Update(Issentity);
                flag = true;
                var currentId1 = _issueRequestRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[IssueRequest]  WHERE IssueRequestMasterId ='{Issentity.Id}'").First();
                var slipDetailId = "";
                var Material = "";
                var Article = "";
                var SKU1 = "";
                var SKU2 = "";
                var SKU3 = "";
                var SalesOrderId = "";
                var TransactionUoMId = "";
                foreach (var itemDetail in entity)
                {
                    if (itemDetail.CostCenterId == "" || itemDetail.CostCenterId == null)
                    {
                        throw new CustomException("Select Cost Center !");
                    }
                    else if (itemDetail.RequestedQty == 0)
                    {
                        throw new CustomException("Input Requested Qty !");
                    }
                    //else if (itemDetail.RejectedQty == 0)
                    //{
                    //    throw new CustomException("Input Rejected Qty !");
                    //}
                    else if (itemDetail.ExpenseActivityId == "")
                    {
                        throw new CustomException("Select Expense Activity !");
                    }

                    else
                    {
                        // Insert in receive detail
                        //if (string.IsNullOrEmpty(itemDetail.Id))
                        //{
                        if (string.IsNullOrEmpty(itemDetail.Id))
                        {
                            var NewId = Issentity.Id + "-";
                            currentId1++;
                            //grndId = NewId + currentId1;
                            var IssueRequstD = new IssueRequest
                            {
                                Id = NewId + currentId1,
                                IssueRequestMasterId = Issentity.Id,
                                RequisitionId = itemDetail.RequisitionNo,
                                RequisitionDetailId = itemDetail.RequisitionDetailId,
                                CostCenterId = itemDetail.CostCenterId,
                                ExpenseActivityId = itemDetail.ExpenseActivityId,
                                RequestedQty = Convert.ToDecimal(itemDetail.RequestedQtyNew),
                                RejectedQty = itemDetail.RejectedQty,
                                BudgetMasterId = itemDetail.BudgetMasterId,
                                GLGeneralInfoId = itemDetail.GLGeneralInfoId,
                                MaterialMasterId = itemDetail.MaterialMasterId,
                                ArticleId = itemDetail.ArticleId,
                                FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = itemDetail.BOQDFirstCharacteristicsValueId,
                                SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = itemDetail.BOQDSecondCharacteristicsValueId,
                                ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = itemDetail.BOQDThirdCharacteristicsValueId,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                InventoryMaterialId = itemDetail.InventoryMaterialId,
                                CountryId = itemDetail.CountryId,
                                UpdatedBy = identity.UserId
                            };
                            try
                            {
                                //InsertGraph(receiveDetail); AuditService.UpdatedLog(receiveDetail);

                                AuditService.AddedLog(IssueRequstD);
                                _issueRequestRepository.Insert(IssueRequstD);

                                slipDetailId = IssueRequstD.Id;
                                Material = IssueRequstD.MaterialMasterId;
                                Article = IssueRequstD.ArticleId;
                                SKU1 = IssueRequstD.FirstCharacteristicsValueId;
                                SKU2 = IssueRequstD.SecondCharacteristicsValueId;
                                SKU3 = IssueRequstD.ThirdCharacteristicsValueId;
                                SalesOrderId = itemDetail.SalesOrderId;
                                TransactionUoMId = IssueRequstD.TransactionUoMId;



                            }
                            catch (DivideByZeroException ex)
                            {

                            }
                            finally
                            {

                            }
                            var FilterentityData = entity.Where(r => r.MaterialMasterId == Material && r.ArticleId == Article && r.BOQDFirstCharacteristicsValueId == SKU1 && r.BOQDSecondCharacteristicsValueId == SKU2 && r.BOQDThirdCharacteristicsValueId == SKU3 && r.SalesOrderId == itemDetail.SalesOrderId && r.TransactionUoMId == TransactionUoMId).ToList();

                            foreach (var itemDetailentity in FilterentityData)
                            {

                                // Insert in receive detail
                                if (string.IsNullOrEmpty(itemDetailentity.Id))
                                {
                                    var NewId1 = Issentity.Id + "-";
                                    currentId1++;
                                    //grndId = NewId + currentId1;
                                    var IssueRequestBOQMap = new IssueRequestBOQMap
                                    {
                                        Id = NewId1 + currentId1,
                                        IssueRequestDetailId = slipDetailId,
                                        BOQID = itemDetailentity.BOQId,
                                        Qty = Convert.ToDecimal(itemDetailentity.RequestedQty)
                                    };
                                    try
                                    {
                                        AuditService.AddedLog(IssueRequestBOQMap);
                                        _issueRequestBOQMap.Insert(IssueRequestBOQMap);
                                    }
                                    catch (DivideByZeroException ex)
                                    {

                                    }
                                    finally
                                    {

                                    }
                                }
                            }//
                        }


                        else
                        {

                            var IssueRequstD = new IssueRequest
                            {
                                Id = itemDetail.Id,
                                IssueRequestMasterId = Ids,
                                RequisitionId = itemDetail.RequisitionNo,
                                RequisitionDetailId = itemDetail.RequisitionDetailId,
                                CostCenterId = itemDetail.CostCenterId,
                                ExpenseActivityId = itemDetail.ExpenseActivityId,
                                RequestedQty = Convert.ToDecimal(itemDetail.RequestedQty),
                                RejectedQty = itemDetail.RejectedQty,
                                BudgetMasterId = itemDetail.BudgetMasterId,
                                GLGeneralInfoId = itemDetail.GLGeneralInfoId,
                                MaterialMasterId = itemDetail.MaterialMasterId,
                                ArticleId = itemDetail.ArticleId,
                                FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                                InventoryMaterialId = itemDetail.InventoryMaterialId,
                                UpdatedBy = identity.UserId,
                                TransactionUoMId = itemDetail.TransactionUoMId
                            };
                            try
                            {
                                AuditService.UpdatedLog(IssueRequstD);
                                _issueRequestRepository.Update(IssueRequstD);

                            }
                            catch (DivideByZeroException ex)
                            {

                            }
                            finally
                            {

                            }
                        }

                        // }
                    }



                }

                // insert in receive tax


                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }
        #endregion
        //private void Check(PurchaseOrderGroup entity)
        //{
        //    CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
        //    CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        //}


        //public void Insert(PurchaseOrderGroup entity)
        //{
        //    try
        //    {
        //        Check(entity);
        //        entity.Id = GetPK();
        //        AuditService.AddedLog(entity);
        //        entity.ModelState = ModelState.Added;
        //        _purchaseOrderGroupMaster.Insert(entity);
        //        _unitOfWork.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
        //    }
        //}

        //public override void Update(PurchaseOrderGroup entity)
        //{
        //    try
        //    {
        //        Check(entity);
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        entity.CompanyGroupId = identity.CompanyGroupId;
        //        base.Update(entity);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
        //    }
        //}

        public void DeleteReq(string id)
        {
            try
            {
                var detail = Convert.ToBoolean(_purchaseOrderGroupMaster.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM [TRN].[PurchaseOrderGroupDetails] WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                if (!detail)
                {
                    var data = base.Find(id);
                    if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                    base.Delete(data);
                    _unitOfWork.SaveChanges();
                }
                else throw new CustomException("Please delete first line item.");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }




        //public decimal GetAutoSequence()
        //{
        //    try
        //    {
        //        return base.Query().Select().Max(r => r.Sequence + 1);
        //    }
        //    catch
        //    {
        //        return 1.00M;
        //    }
        //}


        public IEnumerable<object> GetPurchaseOrderGroupGridData()
        {
            try
            {
                var sql = @"SELECT       POG.Id
	                                    ,POG.CompanyGroupId
	                                    ,POG.Sequence
	                                    ,POG.Code 
	                                    ,POG.UserName
	                                    ,POG.ShortName
	                                    ,POG.StandardName
	                                    ,POG.UserName As PartyName
	                                    ,POG.Description
	                                    ,POG.Remarks
	                                    ,POG.Active
	                                    ,POG.AddedBy
                                       FROM TRN.PurchaseOrderGroup POG   ";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public IEnumerable<object> GetAllPurchaseOrderGroupDetails(string Id)//string ReqDetailId
        {
            try
            {
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"select
                             POGD.Id
                             ,MGM.UserName As MateralMasterGroupName
                             ,MM.Id AS MaterialMasterId
                            ,MM.UserName as MaterialMasterName
                                ,POGD.ArticleId
	                            ,ART.StandardName
	                            ,Pr.UserName As PartyName
	                            ,POGD.FirstCharacteristicsId
	                            ,FC.UserName AS FirstCharacteristics
	                            ,POGD.FirstCharacteristicsValueId
	                            ,FCV.UserName AS FirstCharacteristicsValue
	                            ,POGD.SecondCharacteristicsId
	                            ,SC.UserName AS SecondCharacteristics
	                            ,POGD.SecondCharacteristicsValueId
	                            ,SCV.UserName AS SecondCharacteristicsValue
	                            ,POGD.ThirdCharacteristicsId
	                            ,TC.UserName AS ThirdCharacteristics
	                            ,POGD.ThirdCharacteristicsValueId
	                            ,TCV.UserName AS ThirdCharacteristicsValue
                             FROM 
                            TRn.PurchaseOrderGroupDetails POGD
                            Left JOIn TRn.PurchaseOrderGroup POG ON POG.Id=POGD.PurchaseOrderGroupId
                            Left JOin mst.MaterialMaster MM ON MM.Id=POGD.MaterialMasterId
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle  ART ON ART.Id= POGD.ArticleId
                            LEFT JOIN HKP.Characteristics AS FC ON POGD.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON POGD.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON POGD.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON POGD.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON POGD.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON POGD.ThirdCharacteristicsValueId = TCV.Id
                            LEFT Join [HKP].[Party] As Pr ON POGD.PartyId=Pr.Id
                           Where POG.Id ='" + Id + "' ";
                return _sqlRepository.GetDataCollection(_sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetAllReqdata1()
        {
            throw new NotImplementedException();
        }


        public object SqlQuery<T>(string v)
        {
            throw new NotImplementedException();
        }



        public void UpdateMaterial(IEnumerable<PurchaseOrderGroupDetails> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
        {
            try
            {


                if (entity.IsNotNull())
                {
                    // var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                    foreach (var item1 in entity)
                    {
                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        var ip = identity.IPAddress;
                        var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                        var UpdatedBy = identity.Name;
                        var ReqDetailId = item1.Id;

                        var _sql = "UPDATE [TRN].[PurchaseOrderGroupDetails] SET [TransactionQty] =  '" + Convert.ToDecimal(item1.TransactionQty) + "',[EstimatedRate] = '" + Convert.ToDecimal(item1.EstimatedRate) + "',[TotalAmount] = '" + Convert.ToDecimal(item1.TotalAmount) + "',[UpdatedBy] = '" + identity.UserId + "',[UpdatedDate] = '" + Convert.ToDateTime(DateTime.Now) + "',[UpdatedFromIP] = '" + identity.IPAddress + "' where id = '" + ReqDetailId + "'";
                        _sqlRepository.ExecuteSqlCommand(_sql);
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

        public void DeleteReqDetails(string id)
        {
            try
            {
                //var detail = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.MaterialRequsitionDetails WHERE Id='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                ////var service = Convert.ToBoolean(_inventoryReceiveRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT Id FROM TRN.InventoryService WHERE InventoryReceiveId='" + id + @"') AS A )SELECT 1 ELSE SELECT 0 RETURN").First());
                //if (!detail)
                //{

                var data = _purchaseOrderGroupDetails.Find(id);
                if (data.IsNull()) throw new CustomException(ServiceResources.RecordNoLonger);
                _purchaseOrderGroupDetails.Delete(data.Id);
                _unitOfWork.SaveChanges();
                //}
                //else throw new CustomException("Please delete first line item.");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }



        public decimal GetToCurrencyRate(string currencyId, string baseCurrencyId, DateTime docDate, string companyId)
        {
            try
            {
                decimal toCurrencyRate = 0;
                if (currencyId != baseCurrencyId)
                {
                    var sql = @"SELECT ISNULL((SELECT TOP(1) ISNULL(A.ToCurrencyBankSelling,0) FROM SCS.ExchangeRate AS A WHERE
                                            FromCurrencyCode='" + currencyId + "'   AND A.CompanyId='" + companyId + "' ORDER BY CAST(FromDate AS DATE) DESC), 0)";
                    toCurrencyRate = _purchaseOrderGroupDetails.SqlQuery<decimal>(sql).First();
                }
                return toCurrencyRate;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void Insert(PurchaseOrderGroup entity)
        {
            throw new NotImplementedException();
        }

        public decimal GetAutoSequence()
        {
            throw new NotImplementedException();
        }


        public IEnumerable<object> IssueListData(string IssueStatus, string IssueSlipType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = "";
                if (IssueSlipType == "InventorySlip" || IssueSlipType == "undefined")
                {
                    if (IssueStatus == "ForChecked")
                    {

                        sql = @" select x.Id 
                                 ,x.PreparedBy
                                 ,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate
                                 ,Sum(x.RequestedQty) RequestedQty 
                                 ,Sum(x.RejectedQty) RejectedQty
                                 ,x.CheckedBy
                                 ,CheckedByStatus
                                 ,x.AuthorizedBy
                                 ,AuthorizedByStatus,SalesOrderId
								  ,x.ProcessId,x.ProcessName
								 ,x.CheckedById
									,x.ApprovedById,x.Orderspecific
                                 FROM
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
                                    ,EI1.EmployeeName CheckedBy
                                    ,IRM.CheckedByStatus
                                    ,EI2.EmployeeName AuthorizedBy
                                    ,IRM.AuthorizedByStatus
	                                ,RequestedQty,SalesOrderId
								   ,RejectedQty
									,map.ProcessId
									,p.UserName ProcessName
									,EI1.SystemId CheckedById
									,EI2.SystemId ApprovedById
									,isnull(irm.Orderspecific,'No') Orderspecific
                                FROM TRN.IssueRequestMaster IRM
                                Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                                Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                                Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                                LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                                LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                                LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
								LEFT JOIN(
											SELECT distinct PDAMAP.IssueRequestMasterId
												,SalesOrderId=STUFF((select distinct ','+xPDAMAP.SalesOrderId from
												trn.IssueRequestMaster xpo
												INNER JOin trn.IssueRequestMasterSalesOrderMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
												where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	
												from  trn.IssueRequestMasterSalesOrderMap PDAMAP 
												LEFT JOIN [TRN].IssueRequestMaster IR ON IR.Id = PDAMAP.IssueRequestMasterId
							  
												group by  PDAMAP.IssueRequestMasterId
									)PDA ON PDA.IssueRequestMasterId=IRM.Id
								left join trn.IssueRequestMasterProcessMap map on map.IssueRequestMasterId=IRM.Id
									left JOIn hkp.Process p On p.Id=map.ProcessId
                                Where IRM.CheckedBy IS NOT NULL 
                                AND IRM.CheckedByStatus='ForChecked' 
                                AND IRM.AuthorizedByStatus IS NULL 
                                AND IRM.AuthorizedBy IS null  
                                AND IRM.IssueSlipType='InventorySlip' 
                                And IRM.PreparedBy='" + identity.EmployeeId + @"'

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
                                    ,EI1.EmployeeName CheckedBy
                                    ,IRM.CheckedByStatus
                                    ,EI2.EmployeeName AuthorizedBy
                                    ,IRM.AuthorizedByStatus
	                                ,RequestedQty,SalesOrderId
                                ,RejectedQty
								,map.ProcessId
									,p.UserName ProcessName
								,EI1.SystemId CheckedById
									,EI2.SystemId ApprovedById,isnull(irm.Orderspecific,'No') Orderspecific
                                FROM TRN.IssueRequestMaster IRM
                                Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                                Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                                Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                                LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                                LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                                LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
								LEFT JOIN(
											SELECT distinct PDAMAP.IssueRequestMasterId
												,SalesOrderId=STUFF((select distinct ','+xPDAMAP.SalesOrderId from
												trn.IssueRequestMaster xpo
												INNER JOin trn.IssueRequestMasterSalesOrderMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
												where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	
												from  trn.IssueRequestMasterSalesOrderMap PDAMAP 
												LEFT JOIN [TRN].IssueRequestMaster IR ON IR.Id = PDAMAP.IssueRequestMasterId
							  
												group by  PDAMAP.IssueRequestMasterId
									)PDA ON PDA.IssueRequestMasterId=IRM.Id
								left join trn.IssueRequestMasterProcessMap map on map.IssueRequestMasterId=IRM.Id
									left JOIn hkp.Process p On p.Id=map.ProcessId
                                Where  IRM.CheckedByStatus IS  NULL 
                                AND IRM.AuthorizedByStatus ='For Approval' 
                                AND IRM.IssueSlipType='InventorySlip' 
                                And IRM.PreparedBy='" + identity.EmployeeId + @"'
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
                                    ,EI1.EmployeeName CheckedBy
                                    ,IRM.CheckedByStatus
                                    ,EI2.EmployeeName AuthorizedBy
                                    ,IRM.AuthorizedByStatus
	                                ,RequestedQty,SalesOrderId
                                ,RejectedQty
								,map.ProcessId
									,p.UserName ProcessName
								,EI1.SystemId CheckedById
									,EI2.SystemId ApprovedById,isnull(irm.Orderspecific,'No') Orderspecific
                                FROM TRN.IssueRequestMaster IRM
                                Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                                Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                                Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                                LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                                LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                                LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
								LEFT JOIN(
											SELECT distinct PDAMAP.IssueRequestMasterId
												,SalesOrderId=STUFF((select distinct ','+xPDAMAP.SalesOrderId from
												trn.IssueRequestMaster xpo
												INNER JOin trn.IssueRequestMasterSalesOrderMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
												where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	
												from  trn.IssueRequestMasterSalesOrderMap PDAMAP 
												LEFT JOIN [TRN].IssueRequestMaster IR ON IR.Id = PDAMAP.IssueRequestMasterId
							  
												group by  PDAMAP.IssueRequestMasterId
									)PDA ON PDA.IssueRequestMasterId=IRM.Id
									left join trn.IssueRequestMasterProcessMap map on map.IssueRequestMasterId=IRM.Id
									left JOIn hkp.Process p On p.Id=map.ProcessId
                                Where  IRM.CheckedByStatus IS  NULL 
                                AND IRM.AuthorizedByStatus IS  NULL
                                AND IRM.IssueSlipType='InventorySlip' 
                                And IRM.PreparedBy='" + identity.EmployeeId + @"'
                                )x 
                                Group by Id ,x.PreparedBy,x.AddedDate,x.CheckedBy,x.CheckedBy
                                 ,CheckedByStatus
                                 ,x.AuthorizedBy
                                 ,AuthorizedByStatus,SalesOrderId ,x.ProcessId,x.ProcessName,x.CheckedById
									,x.ApprovedById,x.Orderspecific";
                    }
                    else if (IssueStatus == "HoldReject")
                    {
                        sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty  ,x.CheckedBy
							 ,CheckedByStatus
							 ,x.AuthorizedBy
							 ,AuthorizedByStatus from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                 ,EI1.EmployeeName CheckedBy
                                ,IRM.CheckedByStatus
                                ,EI2.EmployeeName AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                           LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                            LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
                            Where IRM.CheckedBy IS NOT NULL 
                            AND IRM.CheckedByStatus='Hold'OR IRM.CheckedByStatus='Reject' 
                            AND IRM.AuthorizedByStatus IS NULL 
                            AND IRM.IssueSlipType='InventorySlip' 
                            AND IRM.AuthorizedBy IS null 
                            And IRM.PreparedBy='" + identity.EmployeeId + @"'
                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate ,x.CheckedBy
                             ,CheckedByStatus
                             ,x.AuthorizedBy
                             ,AuthorizedByStatus                            
                                                      
                          ";

                    }
                    else
                    {
                        sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty  ,x.CheckedBy
 ,CheckedByStatus
 ,x.AuthorizedBy
 ,AuthorizedByStatus from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
                                ,IRM.AddedBy
                                ,IRM.AddedDate
                                ,IRM.AddedFromIP
                                ,IRM.UpdatedBy
                                ,IRM.UpdatedDate
                                ,IRM.UpdatedFromIP	  
                               -- ,IRM.Preparedby
                                ,EI1.EmployeeName CheckedBy
                                ,IRM.CheckedByStatus
                                ,EI2.EmployeeName AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                            LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
                            Where IRM.CheckedBy IS NOT NULL AND IRM.CheckedByStatus='Checked' 
                            AND IRM.AuthorizedByStatus ='For Approval' AND IRM.IssueSlipType='InventorySlip' 
                            AND IRM.AuthorizedBy IS not null 
                            And IRM.PreparedBy='" + identity.EmployeeId + @"'

                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate ,x.CheckedBy
                             ,CheckedByStatus
                             ,x.AuthorizedBy
                             ,AuthorizedByStatus                            
                                                      
                          ";

                    }


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





        public IEnumerable<object> AssetIssueListData(string IssueStatus, string IssueSlipType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = "";
                if (IssueSlipType == "AssetSlip" || IssueSlipType == "undefined")
                {
                    if (IssueStatus == "ForChecked")
                    {
                        sql = @"  select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty,x.CheckedBy from
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
                             Where IRM.PreparedBy='" + identity.EmployeeId + @"'
						     AND IRM.CheckedByStatus='ForChecked'
						     AND IRM.AuthorizedBy IS null 
                       

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
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
							     Where IRM.PreparedBy='" + identity.EmployeeId + @"'
						     AND IRM.CheckedByStatus IS NULL
						     AND IRM.AuthorizedBy ='For Approval'
                            AND IRM.IssueSlipType='AssetSlip' 

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
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                               Where  IRM.PreparedBy='" + identity.EmployeeId + @"'
						     AND IRM.CheckedByStatus IS NULL
						     AND IRM.AuthorizedBy IS null 
                            AND IRM.IssueSlipType='AssetSlip' 
                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate,x.CheckedBy                             
                                                    
                          ";
                    }
                    else if (IssueStatus == "HoldReject")
                    {
                        sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
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
                         Where IRM.PreparedBy='" + identity.EmployeeId + @"'
                        AND IRM.CheckedByStatus='Hold'OR IRM.CheckedByStatus='Reject' 
                        AND IRM.AuthorizedByStatus IS NULL 
                       AND IRM.IssueSlipType='AssetSlip' 
                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate                             
                          ";

                    }
                    else
                    {
                        sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
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
                            Where IRM.PreparedBy='" + identity.EmployeeId + @"'
                            AND IRM.CheckedByStatus='Checked' 
                            AND IRM.AuthorizedByStatus ='For Approval' 
                            AND IRM.IssueSlipType='AssetSlip' 
                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate                             
                          ";

                    }


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


        public IEnumerable<object> ApprovedIssueSlipGridData(string IssueStatusApproval, string IssueSlipType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = "";
                if (IssueSlipType == "InventorySlip")
                {
                    if (IssueStatusApproval == "Approval")
                    {

                        _sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty  ,x.CheckedBy
                             ,CheckedByStatus
                             ,x.AuthorizedBy
                             ,AuthorizedByStatus from
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
                                ,EI1.EmployeeName CheckedBy
                                ,IRM.CheckedByStatus
                                ,EI2.EmployeeName AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                            LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
                            where IRM.CheckedByStatus='Checked' 
                            AND IRM.IssueSlipType ='InventorySlip' 
                            AND IRM.AuthorizedByStatus='Approved' 
                            And IRM.AuthorizedBy='" + identity.EmployeeId + @"'
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
                                ,EI1.EmployeeName CheckedBy
                                ,IRM.CheckedByStatus
                                ,EI2.EmployeeName AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                            LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
                            where IRM.CheckedByStatus IS NULL
                            AND IRM.IssueSlipType ='InventorySlip' 
                            AND IRM.AuthorizedByStatus='Approved' 
                            And IRM.AuthorizedBy='" + identity.EmployeeId + @"'
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
                                ,EI1.EmployeeName CheckedBy
                                ,IRM.CheckedByStatus
                                ,EI2.EmployeeName AuthorizedBy
                                ,IRM.AuthorizedByStatus
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                            LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                            LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
                            where IRM.CheckedByStatus IS NULL
                            AND IRM.IssueSlipType ='InventorySlip' 
                            AND IRM.AuthorizedByStatus Is NULL 
                            And IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate,x.CheckedBy
                             ,CheckedByStatus
                             ,x.AuthorizedBy
                             ,AuthorizedByStatus";
                    }
                    else
                    {
                        _sql = @"select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty  ,x.CheckedBy
                         ,CheckedByStatus
                         ,x.AuthorizedBy
                         ,AuthorizedByStatus from
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
                            where IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            AND IRM.CheckedByStatus='Checked' 
                            AND IRM.AuthorizedByStatus='Hold' OR  IRM.AuthorizedByStatus='Reject' 
                            AND IRM.IssueSlipType ='AssetSlip'
                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate,x.CheckedBy
                             ,CheckedByStatus
                             ,x.AuthorizedBy
                             ,AuthorizedByStatus";
                    }
                }

                else
                {
                    if (IssueStatusApproval == "Approval")
                    {

                        _sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
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
                              where IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            AND IRM.CheckedByStatus='Checked' 
                            AND IRM.AuthorizedByStatus='Approved'
                            AND IRM.IssueSlipType ='AssetSlip'

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
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                              where IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            AND IRM.CheckedByStatus IS NULL
                            AND IRM.AuthorizedByStatus='Approved'
                            AND IRM.IssueSlipType ='AssetSlip'



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
	                            ,RequestedQty
                            ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                              where IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            AND IRM.CheckedByStatus IS NULL
                            AND IRM.AuthorizedByStatus  IS NULL
                            AND IRM.IssueSlipType ='AssetSlip'



                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate";
                    }
                    else
                    {
                        _sql = @" select x.Id ,x.PreparedBy,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
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
                            where IRM.AuthorizedBy='" + identity.EmployeeId + @"'
                            AND IRM.CheckedByStatus='Checked' 
                            AND IRM.AuthorizedByStatus='Hold' OR  IRM.AuthorizedByStatus='Reject' 
                            AND IRM.IssueSlipType ='AssetSlip'

                            )x 
                            Group by Id ,x.PreparedBy,x.AddedDate";
                    }
                }





                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetSavedPOList(string GRNId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = "";
                _sql = @" 
                                    --DECLARE @plantId VARCHAR(10)='20171';
                                    SELECT ROW_NUMBER()  OVER (ORDER BY  IR.Id) AS SiNo,IR.Id, REPLACE(CONVERT(CHAR(11), IR.PODate, 106),' ','-') AS PODate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
                                    , CP.UserName AS PartyAccountGroupName
                                    , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
                                    --, IR.GateEntryNo
                                    --, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate
                                    , IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
                                    , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
                                    , IR.FixedAssetOrInventory, IR.PODepended,'' PurchaseDocAcceptanceDetailId
                                    --, IR.AlongwithInvoice
                                    --, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
                                    , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
                                    , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState,S1.Id AS InvoicingStateId , S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
                                    , IR.IsApproved, IR.IsPaymentHold, SP.Id AS PlantStateId
                                    ,IPP.UserName As InvoicingByName
                                    ,pgl.CtnId,0'Active',CU.Code Currency
                                    FROM [TRN].[PurchaseOrder] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                    JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                    JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                                    LEFT JOIN [ORG].Plant PL ON PL.Id=IR.PlantId
                                    LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PL.AddressMasterId
                                    LEFT JOIN [SCS].[State] AS SP ON SP.Id=AMP.StateId
                                    LEFT JOIN (SELECT A.InventoryReceiveId,A.QtyStatus, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[PurchaseOrderDetail] AS A
                                    JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId,A.QtyStatus) AS IRD ON IRD.InventoryReceiveId=IR.Id
                                    LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[PurchaseOrderDetail] AS A JOIN [TRN].[PurchaseOrder] AS B ON A.InventoryReceiveId=B.Id

                                    WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                                    LEFT JOIN (Select count(Id) as CtnId,POID from TRN.PurchaseOrderApprovalLog where Status='Approval' group by POID) as pgl  on pgl.POID=IR.Id
                                    LEFT JOIN trn.POGGRNMap Aa ON Aa.POid=IR.Id
                                    WHERE IR.PlantId='" + identity.PlantId + @"'
                                    AND IR.IsClosed=0 and IRD.QtyStatus=0  AND IR.POType='PO' AND pgl.CtnId is not null
                                    AND Aa.GRNID='" + GRNId + @"'
                                    Order by IR.PODate ASC";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> IssueListById(GridParameter parameters, string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"DECLARE @plantId VARCHAR(10) = '" + identity.PlantId + @"';
                            SELECT IR.Id,IR.InventoryMaterialId
                            	,CC.UserName AS CostCenterName
                            	,B.UserName ActivityName
                            	,IR.RequisitionId
                            	,IR.RequisitionDetailId
                            	,En.Username AS EntityName
                            	,MRM.EntityId
                            	,Bu.Code
                            	,Bu.UserName
                            	,Bu1.Code
                            	,Bu1.UserName Activity
                            	,Us.FullName AddedBy
                            	,MRM.Id RequisitionNo
                            	,IR.ArticleId
                            	,Dp.UserName DepartmentName
                            	,MGM.UserName MaterialMasterGroupName
                            	,mm.UserName MaterialMasterName
                            	,mm.Id MaterialMasterId
                            	,ART.StandardName StandardName
                            	,MT.UserName MaterialType
                            	,MRD.FirstCharacteristicsId
                            	,FC.UserName AS FirstCharacteristics
                            	,MRD.FirstCharacteristicsValueId
                            	,FCV.UserName AS FirstCharacteristicsValue
                            	,MRD.SecondCharacteristicsId
                            	,SC.UserName AS SecondCharacteristics
                            	,MRD.SecondCharacteristicsValueId
                            	,SCV.UserName AS SecondCharacteristicsValue
                            	,MRD.ThirdCharacteristicsId
                            	,TC.UserName AS ThirdCharacteristics
                            	,MRD.ThirdCharacteristicsValueId
                            	,TCV.UserName AS ThirdCharacteristicsValue
                            	,IR.ExpenseActivityId
                            	,IR.CostCenterId
                            	,IR.ExpenseActivityId
                            	,IR.BudgetMasterId
                            	,IR.GLGeneralInfoId
                            	,isnull(outIDR.ApprovedQty, 0) ApprovedQty
                            	,isnull(outIDR.RejectionQty, 0) RejectionQty1
                            	,(isnull(GRN.TransactionQty, 0) - (isnull(Issue.IssueQty, 0) - isnull(PurchaseReturnData.Qty, 0) - isnull(InventorySalesData.Qty, 0) - isnull(InventoryTransferData.Qty, 0))) TotalQty
                            	--,ISNULL(outIDR1.TotalQty,0) TotalQty                           
                            	,ISNULL(IssuedQtyOut.IssueQty, 0) AS IssuedQty
                            	,IR.RequestedQty
                            	,IR.RejectedQty
                            	,isnull(IGL1.UserName, '') AS CGL
                            	,isnull(B1.UserName, '') AS CBUdget
                            	,isnull(IA1.UserName, '') AS GLBudgetActivity
                            	,c.UserName CountryName
                            	,C.Id CountryId
                            	,IR.TransactionUoMId
                            	,ISNULL(GRNALLO.TransactionQty, 0) TransactionQty
                            	,Isnull(MMAU.BaseUOMFactor, 0) BaseUOMFactor
                            	,(ISNULL(GRNALLO.TransactionQty, 0)) TotalQty
                            FROM trn.IssueRequest IR
                            LEFT JOIN [TRN].[MaterialRequsitionDetails] AS MRD ON MRD.Id = IR.REquisitionDetailId
                            LEFT JOIN [TRN].[MaterialRequsitionMaster] AS MRM ON MRD.MaterialReqqusitionMasterId = MRM.Id
                            LEFT JOIN [ORG].[CostCenter] CC ON CC.Id = IR.CostCenterId
                            LEFT JOIN hkp.Budget B ON B.Id = IR.ExpenseActivityId
                            LEFT JOIN [ORG].[Entity] AS En ON MRM.EntityId = En.Id
                            LEFT JOIN [HKP].[Budget] AS Bu ON Bu.Id = MRD.ActivityId
                            LEFT JOIN [HKP].[Budget] AS Bu1 ON Bu1.Id = IR.ExpenseActivityId
                            LEFT JOIN MST.MaterialMaster AS MM ON IR.MaterialMasterId = MM.Id
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON IR.ArticleId = ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON IR.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IR.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IR.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IR.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IR.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IR.ThirdCharacteristicsValueId = TCV.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MRD.TransactionUoMId = TUoM.Id
                            LEFT JOIN [SEC].[User] AS Us ON MRM.AddedBy = Us.UserId
                            LEFT JOIN dbo.EmployeeInformation AS Em ON Us.EmployeeId = Em.SystemId
                            LEFT JOIN [ORG].[Department] AS Dp ON Dp.Id = Em.DepartmentId
                            LEFT JOIN (
                            	SELECT B.RequisitionDetailId
                            		,Sum(A.TransactionQty) ApprovedQty
                            		,Sum(A.RejectQty) RejectionQty
                            	FROM TRN.GRNPORequisitionAllocation A
                            	LEFT JOIN trn.PoRequisitionDetail B ON A.POReqDetailsID = B.id
                            	GROUP BY RequisitionDetailId
                            	) outIDR ON outIDR.RequisitionDetailId = IR.RequisitionDetailId
                            LEFT JOIN (
                            	SELECT MRD.MaterialReqqusitionMasterId
                            		,MRD.Id AS RequisitionDetailId
                            		,sum(RID.IssueQty) IssueQty
                            		,sum(RID.IssueRejectedQty) IssueRejectedQty
                            	FROM TRN.RequisitionIssueDetail RID
                            	LEFT JOIN TRN.InventoryIssue II ON II.Id = RID.IssueMasterId
                            	LEFT JOIN TRN.InventoryIssueDetail IID ON IID.Id = RID.IssueDetailId
                            	LEFT JOIN TRN.IssueRequest IR ON IR.Id = RID.IssueRequestId
                            	LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.id = IR.RequisitionDetailId
                            	GROUP BY MRD.MaterialReqqusitionMasterId
                            		,MRD.Id
                            	) IssuedQtyOut ON IssuedQtyOut.RequisitionDetailId = IR.RequisitionDetailId
                            LEFT JOIN (
                            	SELECT IRD.InventoryMaterialId
                            		,Sum(IRD.BaseQty) AS TransactionQty
                            		,SUM(ird.ShortageQty) ShortageQty
                            		,SUM(ird.RejectionQty) RejectionQty
                            	FROM [TRN].[InventoryReceiveDetail] IRD
                            	LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id = IRD.InventoryReceiveId
                            	WHERE IR.PlantId = @plantId
                            	GROUP BY IRD.InventoryMaterialId
                            	) AS GRN ON GRN.InventoryMaterialId = IR.InventoryMaterialId
                            LEFT JOIN (
                            	SELECT IID.InventoryMaterialId
                            		,Sum(IH.Qty) IssueQty
                            		,Sum(IID.PolicyAmount) PolicyAmount
                            	FROM TRN.InventoryIssueDetail IID
                            	LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId = II.Id
                            	LEFT JOIN TRN.InventoryIssueHistory IH ON IH.InventoryIssueDetailId = IID.Id
                            	WHERE II.PlantId = @plantId
                            	GROUP BY IID.InventoryMaterialId
                            	) Issue ON Issue.InventoryMaterialId = IR.InventoryMaterialId
                            --Issue Return
                            LEFT JOIN (
                            	SELECT IH.InventoryMaterialId
                            		,sum(IH.Qty) Qty
                            		,sum(IRD.MaterialTranRate) MaterialTranRate
                            		,(sum(IH.Qty) * sum(IRD.MaterialTranRate)) IssueReturnAmount
                            	FROM trn.InventoryIssueReturnHistory IH
                            	LEFT JOIN trn.InventoryIssueReturn II ON II.Id = IH.InventoryIssueReturnId
                            	LEFT JOIN trn.InventoryReceiveDetail IRD ON IRD.Id = IH.InventoryReceiveDetailId
                            	WHERE II.PlantId = @plantId
                            	GROUP BY IH.InventoryMaterialId
                            	) IssueReturnData ON IssueReturnData.InventoryMaterialId = IR.InventoryMaterialId
                            --Purchase return
                            LEFT JOIN (
                            	SELECT IH.InventoryMaterialId
                            		,sum(IH.TransactionQty) Qty
                            		,sum(IRD.MaterialTranRate) MaterialTranRate
                            		,(sum(IH.TransactionQty) * sum(IRD.MaterialTranRate)) PurchaseReturnAmount
                            	FROM trn.PurchaseReturnDetail IH
                            	LEFT JOIN trn.PurchaseReturn II ON II.Id = IH.PurchaseReturnId
                            	LEFT JOIN trn.InventoryReceiveDetail IRD ON IRD.Id = IH.InventoryReceiveDetailId
                            	WHERE II.PlantId = @plantId
                            	GROUP BY IH.InventoryMaterialId
                            	) PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId = IR.InventoryMaterialId
                            -- InventorySales
                            LEFT JOIN (
                            	SELECT ISD.InventoryMaterialId
                            		,sum(ISH.Qty) Qty
                            		,sum(ISH.BaseRate) Rate
                            		,(sum(ISH.Qty) * sum(ISH.BaseRate)) InventorySalesAmount
                            	FROM [TRN].[InventorySalesHistory] ISH
                            	LEFT JOIN [TRN].[InventorySalesDetail] ISD ON ISD.Id = ISH.InventorySalesDetailId
                            	LEFT JOIN [TRN].[InventorySales] Ins ON Ins.Id = ISD.InventorySalesId
                            	WHERE Ins.PlantId = @plantId
                            	GROUP BY ISD.InventoryMaterialId
                            	) InventorySalesData ON InventorySalesData.InventoryMaterialId = IR.InventoryMaterialId
                            --InventoryTransfer
                            LEFT JOIN (
                            	SELECT IRD.InventoryMaterialId
                            		,sum(IRD.InventoryTransferQty) Qty
                            		,sum(IRD.MaterialTranRate) Rate
                            		,(sum(IRD.InventoryTransferQty) * sum(IRD.MaterialTranRate)) InventoryTransferAmount
                            	FROM [TRN].[InventoryTransferHistory] ITH
                            	LEFT JOIN [TRN].[InventoryReceiveDetail] IRD ON IRD.Id = ITH.InventoryReceiveDetailId
                            	LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id = IRD.InventoryReceiveId
                            	WHERE IR.PlantId = @plantId
                            	GROUP BY IRD.InventoryMaterialId
                            	) InventoryTransferData ON InventoryTransferData.InventoryMaterialId = IR.InventoryMaterialId
                            LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
                            LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id = IR.GLGeneralInfoId
                            LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id = IR.BudgetMasterId
                            LEFT JOIN HKP.Activity IA1 ON IA1.Id = IR.ExpenseActivityId
                            LEFT JOIN hkp.Budget B1 ON B1.Id = IBM1.BudgetId
                            LEFT JOIN scs.country C ON C.Id = Ir.CountryId
                            LEFT JOIN trn.IssueRequestBOQMap boqmAp ON boqmAp.IssueRequestDetailId = IR.Id
                            LEFT JOIN (
                            	SELECT a.SalesOrderId
                            		,b.BOQDetailId
                            		,sum(a.BaseQty) TransactionQty
                            		,UOM.UserName
                            		,UOM.Id StockTransactionUoMId
                            		,a.BaseUoMId
                            	FROM trn.GRNPORequisitionAllocation a
                            	LEFT JOIN trn.POBOQMap b ON b.Id = a.POBOQMapId
                            	LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id = a.BaseUoMId
                            	GROUP BY b.BOQDetailId
                            		,UOM.UserName
                            		,UOM.Id
                            		,a.SalesOrderId
                            		,a.BaseUoMId
                            	) GRNALLO ON GRNALLO.BOQDetailId = boqmAp.BOQID
                            LEFT JOIN (
                            	SELECT a.MaterialMasterId
                            		,a.AlternativeUOMId
                            		,a.BaseUOMId
                            		,Sum(a.BaseUOMFactor) BaseUOMFactor
                            	FROM [MST].[MaterialMasterAlternativeUOM] a
                            	LEFT JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id = a.AlternativeUOMId
                            	GROUP BY a.MaterialMasterId
                            		,a.AlternativeUOMId
                            		,a.BaseUOMId
                            	) AS MMAU ON MMAU.MaterialMasterId = IR.MaterialMasterId
                            	AND MMAU.AlternativeUOMId = TUoM.Id
                            WHERE IR.IssueRequestMasterId = '" + Id + @"'";



                //return _sqlRepository.GetDifferentGridData(parameters);
                var Data = _sqlRepository.GetDataCollection(sql);

                StringCollection strCol = new StringCollection();
                string MaterialMasterList = "''";
                for (int i = 0; i < Data.Count; i++)
                {
                    if (strCol.Contains(Data[i]["MaterialMasterId"].ToString()) == true)
                        continue;
                    strCol.Add(Data[i]["MaterialMasterId"].ToString());
                    MaterialMasterList += ",'" + Data[i]["MaterialMasterId"].ToString() + "'";

                }

                var UOMList = _sqlRepository.GetDataCollection(@"select M.Id AS MaterialMasterId, UOM1.Id AS [Value],UOM1.UserName AS [Text] from (select Id,BaseUOMId UOMId from mst.MaterialMaster
																	union
																	select MaterialMasterId,AlternativeUOMId from mst.MaterialMasterAlternativeUOM
																	) AS M
																	 JOIN scs.UnitOfMeasurement AS uom1 ON uom1.Id=m.UOMId
																	 where m.Id in (" + MaterialMasterList + @")");

                for (int i = 0; i < Data.Count; i++)
                {
                    var temp = UOMList.Where(ee => ee["MaterialMasterId"].ToString() == Data[i]["MaterialMasterId"].ToString()).ToList();
                    Data[i]["uoMList"] = temp;
                }

                return Data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> IssueSlipDetail(string slipstatus, string employeeById)
        {
            try
            {
                string tempQuery = "";
                if (slipstatus == "ForChecked")
                {
                    tempQuery = "WHERE IRM.CheckedBy IS NOT NULL  AND IRM.CheckedByStatus = 'ForChecked' AND IRM.AuthorizedByStatus IS NULL AND IRM.AuthorizedBy IS null AND IRM.IssueSlipType = 'InventorySlip'  AND IRM.CheckedBy='" + employeeById + "'";
                }
                else if (slipstatus == "HoldReject")
                {
                    tempQuery = "WHERE IRM.CheckedBy IS NOT NULL  AND IRM.CheckedByStatus = 'HoldReject' AND IRM.AuthorizedByStatus IS NULL AND IRM.AuthorizedBy IS null AND IRM.IssueSlipType = 'InventorySlip' AND IRM.CheckedBy='" + employeeById + "'";

                }
                else if (slipstatus == "Checked")
                {
                    tempQuery = "WHERE IRM.CheckedBy IS NOT NULL  AND IRM.CheckedByStatus = 'Checked' AND IRM.AuthorizedByStatus='For Approval' AND IRM.IssueSlipType = 'InventorySlip' AND IRM.CheckedBy='" + employeeById + "'";

                }
                else if(slipstatus == "For Approval")
                {
                    tempQuery = "WHERE   IRM.CheckedByStatus = 'Checked' AND IRM.AuthorizedByStatus='For Approval'  AND IRM.IssueSlipType = 'InventorySlip'  AND IRM.AuthorizedBy='" + employeeById + "'";
                }
                 else if (slipstatus == "HoldReject")
                {
                    tempQuery = "WHERE   IRM.CheckedByStatus = 'Checked' AND IRM.AuthorizedByStatus='Reject'  AND IRM.IssueSlipType = 'InventorySlip'  AND IRM.AuthorizedBy='" + employeeById + "'";
                }
                 else if (slipstatus == "Approved")
                {
                    tempQuery = "WHERE   IRM.CheckedByStatus = 'Checked' AND IRM.AuthorizedByStatus='Approved'  AND IRM.IssueSlipType = 'InventorySlip'  AND IRM.AuthorizedBy='" + employeeById + "'";
                }
                var _sql = @"Select  IR.Id ,CC.UserName AS CostCenterName ,B.UserName ActivityName  ,IR.RequisitionId
                                    ,IR.RequisitionDetailId   ,IR.RequestedQty ,IR.RejectedQty  ,En.Username As EntityName
                                    ,MRM.EntityId ,Bu.Code  ,Bu.UserName ,IR.IssueRequestMasterId
                                    ,Bu1.Code ,Bu1.UserName Activity ,Us.FullName AddedBy  ,MRM.Id RequisitionNo  ,MRD.ArticleId
                                    ,Dp.UserName DepartmentName  ,MGM.UserName MaterialMasterGroupName
                                    ,mm.UserName Material ,ART.StandardName ArticleName ,MT.UserName MaterialType  ,MRD.FirstCharacteristicsId
                                    ,FC.UserName AS FirstCharacteristics ,MRD.FirstCharacteristicsValueId ,FCV.UserName AS Sku1 ,MRD.SecondCharacteristicsId
                                    ,SC.UserName AS SecondCharacteristics ,MRD.SecondCharacteristicsValueId
                                    ,SCV.UserName AS Sku2  ,MRD.ThirdCharacteristicsId ,TC.UserName AS ThirdCharacteristics ,MRD.ThirdCharacteristicsValueId
                                    ,TCV.UserName AS Sku3 ,IR.ExpenseActivityId  ,IR.CostCenterId ,IR.ExpenseActivityId  ,IR.BudgetMasterId ,IR.GLGeneralInfoId 
                                    ,IR.IssueRequestMasterId ,isnull(IGL1.UserName,'') AS CGL									
									,isnull(B1.UserName,'') AS CBUdget ,isnull(IA1.UserName,'') AS GLBudgetActivity,TUoM.UserName UOM
                                    from trn.IssueRequest IR
                                    left Join TRN.IssueRequestMaster As IRM on IRM.Id=IR.IssueRequestMasterId
                                    left Join [TRN].[MaterialRequsitionDetails] As MRD on MRD.Id=IR.REquisitionDetailId
                                    Left Join [TRN].[MaterialRequsitionMaster] As MRM On MRD.MaterialReqqusitionMasterId=MRM.Id
                                    Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                                    Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                                    Left Join [ORG].[Entity] As En On MRM.EntityId=En.Id
                                    Left Join [HKP].[Budget] As Bu On Bu.Id=MRD.ActivityId
                                    Left Join [HKP].[Budget] As Bu1 On Bu1.Id=IR.ExpenseActivityId
                                    Left JOIN MST.MaterialMaster AS MM ON IR.MaterialMasterId = MM.Id
                                    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                                    LEFT JOIN MST.MaterialMasterArticle AS ART ON IR.ArticleId = ART.Id
                                    LEFT JOIN HKP.Characteristics AS FC ON IR.FirstCharacteristicsId = FC.Id
                                    LEFT JOIN HKP.Characteristics AS SC ON IR.SecondCharacteristicsId = SC.Id
                                    LEFT JOIN HKP.Characteristics AS TC ON IR.ThirdCharacteristicsId = TC.Id
                                    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IR.FirstCharacteristicsValueId = FCV.Id
                                    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IR.SecondCharacteristicsValueId = SCV.Id
                                    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IR.ThirdCharacteristicsValueId = TCV.Id
                                     LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IR.TransactionUoMId = TUoM.Id
                                    LEFT JOIN [SEC].[User] As Us On IR.AddedBy=Us.UserId
                                    LEFT JOIN dbo.EmployeeInformation As Em On Us.EmployeeId=Em.SystemId
                                    LEFT JOIN [ORG].[Department] AS Dp On Dp.Id=Em.DepartmentId
                                    LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                                    LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IR.GLGeneralInfoId 
									LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IR.BudgetMasterId
									LEFT JOIN HKP.Activity IA1 ON IA1.Id=IR.ExpenseActivityId
									Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId  " + tempQuery + "";

                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #region  IssueSlipChecked and Approval

        public IEnumerable<object> IssueSlipUnChecked(string IssuStatus)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string tempQuery = "";
                if (IssuStatus == "ForChecked")
                {
                    tempQuery = "WHERE IRM.CheckedBy IS NOT NULL  AND IRM.CheckedByStatus = 'ForChecked' AND IRM.AuthorizedByStatus IS NULL AND IRM.AuthorizedBy IS null AND IRM.IssueSlipType = 'InventorySlip'  AND IRM.CheckedBy='" + identity.EmployeeId + "'";
                }
                else if (IssuStatus == "HoldReject")
                {
                    tempQuery = "WHERE IRM.CheckedBy IS NOT NULL  AND IRM.CheckedByStatus = 'HoldReject' AND IRM.AuthorizedByStatus IS NULL AND IRM.AuthorizedBy IS null AND IRM.IssueSlipType = 'InventorySlip' AND IRM.CheckedBy='" + identity.EmployeeId + "'";
                }
                else if (IssuStatus == "Checked")
                {
                    tempQuery = "WHERE IRM.CheckedBy IS NOT NULL  AND IRM.CheckedByStatus = 'Checked' AND IRM.AuthorizedByStatus='For Approval' AND IRM.IssueSlipType = 'InventorySlip' AND IRM.CheckedBy='" + identity.EmployeeId + "'";
                }

                var sql = @" SELECT IRM.Id,IRM.ProductionOrderId, pr.PlannedQty ,MIC.PlanPercentage,RecoveryPercentage=(100+(100-isnull(MIC.PlanPercentage,0))) ,RequestedQty ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            LEFT JOIN (select IssueRequestMasterId,CostCenterId,ExpenseActivityId,sum(RequestedQty) RequestedQty,sum(RejectedQty)RejectedQty 
									FROM  TRN.IssueRequest group by IssueRequestMasterId,ExpenseActivityId,CostCenterId) IR ON IR.IssueRequestMasterId=IRM.Id
                            LEFT JOIN [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
							LEFT JOIN trn.ProductionOrder PR ON PR.Id=IRM.ProductionOrderId
							LEFT JOIN (
							Select distinct IR.IssueRequestMasterId,MIC.POId,MIC.PlanPercentage  FROM TRN.IssueRequest IR
							  left join MaterialIssueControlDetail MICD ON MICD.Id=IR.MaterialIssueControlDetailId
							  left join MaterialIssueControlMaster MIC ON MIC.Id=MICD.MaterialIssueControlMasterId
							) MIC ON MIC.IssueRequestMasterId=IRM.Id
                            LEFT JOIN hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                              " + tempQuery +  " ";

                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public IEnumerable<object> IssueSlipChecked()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @" select x.Id ,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
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
                           Where IRM.CheckedByStatus='Checked' And IRM.CheckedBy='" + identity.EmployeeId + @"')x Group by Id";



                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }



        public void IssueSlipToChecked(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (identity.EmployeeId == AuthorizedBy)
                {
                    throw new CustomException("Please select Another Id");
                }

                var AuthorizedById = "";
                var AuthorizedByStatus = "";

                PoValue = "0";
                var Id = GetPK();
                if (CheckedStataus == "Checked")
                {
                    if (AuthorizedBy == null || AuthorizedBy == "")
                    {
                        throw new CustomException("Select Approved By");
                    }
                    AuthorizedById = AuthorizedBy;
                    AuthorizedByStatus = "For Approval";
                }
                else if (CheckedStataus == "Hold" || CheckedStataus == "Reject")
                {

                    AuthorizedById = null;

                }
                //else
                //{
                //    AuthorizedById = null;

                //}
                var Status = CheckedStataus;
                var UpdatedBy = "";
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                string _sql = "Update TRN.IssueRequestMaster set CheckedByStatus='" + Status + "',AuthorizedBy='" + AuthorizedById + "',AuthorizedByStatus='" + AuthorizedByStatus + "' where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into [TRN].[IssueSlipLog](Id," +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "POValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,ISSUEID) " +
                "values ('" + Id + "'," +
                "'" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + PoValue + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + PoId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        #endregion


        #region  ApprovingIssueSlip




        public IEnumerable<object> IssueSlipUnApproved()

        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @" select x.Id ,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
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
                            Where  IRM.AuthorizedBy='" + identity.EmployeeId + @"' And IRM.AuthorizedByStatus is null
                            )x 
                            Group by Id                          
                          ";


                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public IEnumerable<object> IssueSlipApproved()

        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @" select x.Id ,Sum(x.RequestedQty) RequestedQty ,Sum(x.RejectedQty) RejectedQty from
                            (
                                SELECT IRM.Id
                                ,CC.UserName AS CostCenterName
	                            ,B.UserName ActivityName      
	                            ,IR.RequisitionId
                                ,IR.RequisitionDetailId                           
	                            ,EI.FirstName  PreparedBy	                          
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
                              Where  IRM.AuthorizedBy='" + identity.EmployeeId + @"' And IRM.AuthorizedByStatus='Approval'
                            )x 
                            Group by Id                          
                          ";



                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }



        public void IssueSlipToApproved(string PoId, string PoValue, string CheckedStataus, string AuthorizedBy)
        {
            try
            {
                var IsApproved = 0;

                PoValue = "0";
                //  var Id = GetPK();
                if (CheckedStataus == "Approved")
                {
                    IsApproved = 1;

                }
                else
                {
                    IsApproved = 0;

                }
                var Status = CheckedStataus;
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var ip = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                string _sql = "Update TRN.IssueRequestMaster set AuthorizedByStatus='" + Status + "' where id='" + PoId + "'";
                _sqlRepository.ExecuteSqlCommand(_sql);
                string _sql1 = "Insert into [TRN].[IssueSlipLog](Id," +
                "CompanyGroupId," +
                "CompanyId," +
                "PlantId," +
                "ApprovedBy," +
                "Date," +
                "POValue," +
                "Status," +
                "AddedBy," +
                "AddedDate," +
                "AddedFromIp," +
                "UpdatedBy," +
                "UpdatedDate," +
                "UpdatedFromIp,ISSUEID) " +
                "values ('" + GetPK() + "'," +
                "'" + CompanyGroupId + "'," +
                "'" + CompanyId + "'," +
                "'" + PlantId + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + PoValue + "'," +
                "'" + Status + "'," +
                "'" + AddedBy + "'," +
                "'" + AddedDate + "'," +
                "'" + ip + "'," +
                "'" + UpdatedBy + "'," +
                "'" + updatedDate + "', " +
                "'" + ip + "','" + PoId + "')";
                _sqlRepository.ExecuteSqlCommand(_sql1);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        #endregion

        #region Requisition Issue 
        public IEnumerable<object> RequisitionIssueListData()
        {
            try
            {
                var _sql = @" SELECT x.Id, Replace(CONVERT(VARCHAR(11), x.IssueDate, 106), ' ', '-') IssueDate,Sum(x.BaseQty) IssueQty ,Sum(x.TransactionQty) RejectedQty FROM
                            (
                                SELECT IVS.Id
                                ,IVS.IssueDate
                                ,IVS.AddedBy
                                ,IVS.AddedDate
                                ,IVS.AddedFromIP
                                ,IVS.UpdatedBy
                                ,IVS.UpdatedDate
                                ,IVS.UpdatedFromIP	  
	                            ,IRD.BaseQty
                            ,IRD.TransactionQty
                            FROM TRN.InventoryIssue IVS
                            Left JOin TRN.InventoryIssueDetail IRD ON IRD.InventoryIssueId=IVS.Id
                            )x 
                            GROUP BY Id ,IssueDate                              
                          ";


                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #endregion

        #region Issue Approval

        public IEnumerable<object> IssueSlipUnApproved(string IssuAppStatus)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string tempQuery = "";
                if (IssuAppStatus == "For Approval")
                {
                    tempQuery = "WHERE   IRM.CheckedByStatus = 'Checked' AND IRM.AuthorizedByStatus='For Approval'  AND IRM.IssueSlipType = 'InventorySlip'  AND IRM.AuthorizedBy='" + identity.EmployeeId + "'";
                }
                else if (IssuAppStatus == "HoldReject")
                {
                    tempQuery = "WHERE   IRM.CheckedByStatus = 'Checked' AND IRM.AuthorizedByStatus='Reject'  AND IRM.IssueSlipType = 'InventorySlip'  AND IRM.AuthorizedBy='" + identity.EmployeeId + "'";
                }
                else if (IssuAppStatus == "Approved")
                {
                    tempQuery = "WHERE   IRM.CheckedByStatus = 'Checked' AND IRM.AuthorizedByStatus='Approved'  AND IRM.IssueSlipType = 'InventorySlip'  AND IRM.AuthorizedBy='" + identity.EmployeeId + "'";
                }

                var sql = @" SELECT IRM.Id,IRM.ProductionOrderId, pr.PlannedQty ,MIC.PlanPercentage,RecoveryPercentage=(100+(100-isnull(MIC.PlanPercentage,0))) ,RequestedQty ,RejectedQty
                            FROM TRN.IssueRequestMaster IRM
                            LEFT JOIN (select IssueRequestMasterId,ExpenseActivityId,sum(RequestedQty) RequestedQty,sum(RejectedQty)RejectedQty 
									FROM  TRN.IssueRequest group by IssueRequestMasterId,ExpenseActivityId) IR ON IR.IssueRequestMasterId=IRM.Id
							LEFT JOIN trn.ProductionOrder PR ON PR.Id=IRM.ProductionOrderId
							LEFT JOIN dbo.MaterialIssueControlMaster MIC ON MIC.POId=pr.Id
                            LEFT JOIN hkp.Budget B On B.Id=IR.ExpenseActivityId
                            LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                              " + tempQuery + " ";

                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion

        public IEnumerable<object> IssueListDataByProudctionOrder(string IssueStatus, string IssueSlipType, string productionOrderId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = "";
                if (IssueSlipType == "InventorySlip" || IssueSlipType == "undefined")
                {
                    if (IssueStatus == "ForChecked")
                    {

                        sql = @" select x.Id 
                                 ,x.PreparedBy
                                 ,REPLACE(CONVERT(CHAR(11), x.AddedDate, 106),' ','-') AS AddedDate
                                 ,Sum(x.RequestedQty) RequestedQty 
                                 ,Sum(x.RejectedQty) RejectedQty
                                 ,x.CheckedBy
                                 ,CheckedByStatus
                                 ,x.AuthorizedBy
                                 ,AuthorizedByStatus,SalesOrderId
								  ,x.ProcessId,x.ProcessName
								 ,x.CheckedById
									,x.ApprovedById,x.Orderspecific
                                 FROM
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
                                    ,EI1.EmployeeName CheckedBy
                                    ,IRM.CheckedByStatus
                                    ,EI2.EmployeeName AuthorizedBy
                                    ,IRM.AuthorizedByStatus
	                                ,RequestedQty,SalesOrderId
								   ,RejectedQty
									,map.ProcessId
									,p.UserName ProcessName
									,EI1.SystemId CheckedById
									,EI2.SystemId ApprovedById
									,isnull(irm.Orderspecific,'No') Orderspecific
                                FROM TRN.IssueRequestMaster IRM
                                Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                                Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                                Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                                LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                                LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                                LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
								LEFT JOIN(
											SELECT distinct PDAMAP.IssueRequestMasterId
												,SalesOrderId=STUFF((select distinct ','+xPDAMAP.SalesOrderId from
												trn.IssueRequestMaster xpo
												INNER JOin trn.IssueRequestMasterSalesOrderMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
												where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	
												from  trn.IssueRequestMasterSalesOrderMap PDAMAP 
												LEFT JOIN [TRN].IssueRequestMaster IR ON IR.Id = PDAMAP.IssueRequestMasterId
							  
												group by  PDAMAP.IssueRequestMasterId
									)PDA ON PDA.IssueRequestMasterId=IRM.Id
								left join trn.IssueRequestMasterProcessMap map on map.IssueRequestMasterId=IRM.Id
									left JOIn hkp.Process p On p.Id=map.ProcessId
                                Where IRM.CheckedBy IS NOT NULL 
                                AND IRM.CheckedByStatus='ForChecked' 
                                AND IRM.AuthorizedByStatus IS NULL 
                                AND IRM.AuthorizedBy IS null  
                                AND IRM.IssueSlipType='InventorySlip' 
                                AND IRM.ProductionOrderId='" + productionOrderId + @"' 

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
                                    ,EI1.EmployeeName CheckedBy
                                    ,IRM.CheckedByStatus
                                    ,EI2.EmployeeName AuthorizedBy
                                    ,IRM.AuthorizedByStatus
	                                ,RequestedQty,SalesOrderId
                                ,RejectedQty
								,map.ProcessId
									,p.UserName ProcessName
								,EI1.SystemId CheckedById
									,EI2.SystemId ApprovedById,isnull(irm.Orderspecific,'No') Orderspecific
                                FROM TRN.IssueRequestMaster IRM
                                Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                                Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                                Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                                LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                                LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                                LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
								LEFT JOIN(
											SELECT distinct PDAMAP.IssueRequestMasterId
												,SalesOrderId=STUFF((select distinct ','+xPDAMAP.SalesOrderId from
												trn.IssueRequestMaster xpo
												INNER JOin trn.IssueRequestMasterSalesOrderMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
												where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	
												from  trn.IssueRequestMasterSalesOrderMap PDAMAP 
												LEFT JOIN [TRN].IssueRequestMaster IR ON IR.Id = PDAMAP.IssueRequestMasterId
							  
												group by  PDAMAP.IssueRequestMasterId
									)PDA ON PDA.IssueRequestMasterId=IRM.Id
								left join trn.IssueRequestMasterProcessMap map on map.IssueRequestMasterId=IRM.Id
									left JOIn hkp.Process p On p.Id=map.ProcessId
                                Where  IRM.CheckedByStatus IS  NULL 
                                AND IRM.AuthorizedByStatus ='For Approval' 
                                AND IRM.IssueSlipType='InventorySlip' 
                                AND IRM.ProductionOrderId='" + productionOrderId + @"' 
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
                                    ,EI1.EmployeeName CheckedBy
                                    ,IRM.CheckedByStatus
                                    ,EI2.EmployeeName AuthorizedBy
                                    ,IRM.AuthorizedByStatus
	                                ,RequestedQty,SalesOrderId
                                ,RejectedQty
								,map.ProcessId
									,p.UserName ProcessName
								,EI1.SystemId CheckedById
									,EI2.SystemId ApprovedById,isnull(irm.Orderspecific,'No') Orderspecific
                                FROM TRN.IssueRequestMaster IRM
                                Left JOin TRN.IssueRequest IR ON IR.IssueRequestMasterId=IRM.Id
                                Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                                Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId
                                LEFT JOIN EmployeeInformation EI On EI.SystemId=IRM.Preparedby
                                LEFT JOIN EmployeeInformation EI1 On EI1.SystemId=IRM.CheckedBy
                                LEFT JOIN EmployeeInformation EI2 On EI2.SystemId=IRM.AuthorizedBy
								LEFT JOIN(
											SELECT distinct PDAMAP.IssueRequestMasterId
												,SalesOrderId=STUFF((select distinct ','+xPDAMAP.SalesOrderId from
												trn.IssueRequestMaster xpo
												INNER JOin trn.IssueRequestMasterSalesOrderMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
												where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	
												from  trn.IssueRequestMasterSalesOrderMap PDAMAP 
												LEFT JOIN [TRN].IssueRequestMaster IR ON IR.Id = PDAMAP.IssueRequestMasterId
							  
												group by  PDAMAP.IssueRequestMasterId
									)PDA ON PDA.IssueRequestMasterId=IRM.Id
									left join trn.IssueRequestMasterProcessMap map on map.IssueRequestMasterId=IRM.Id
									left JOIn hkp.Process p On p.Id=map.ProcessId
                                Where  IRM.CheckedByStatus IS  NULL 
                                AND IRM.AuthorizedByStatus IS  NULL
                                AND IRM.IssueSlipType='InventorySlip' 
                                AND IRM.ProductionOrderId='" + productionOrderId + @"' 
                                )x 
                                Group by Id ,x.PreparedBy,x.AddedDate,x.CheckedBy,x.CheckedBy
                                 ,CheckedByStatus
                                 ,x.AuthorizedBy
                                 ,AuthorizedByStatus,SalesOrderId ,x.ProcessId,x.ProcessName,x.CheckedById
									,x.ApprovedById,x.Orderspecific";
                    }
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

    }
}