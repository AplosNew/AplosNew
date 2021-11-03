#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Projects;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Materials;
using Library.Service.Properties;
using Library.Service.Setups;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Projects
{
    public class ProjectPlanningPORequisitionMaterialMasterService : Service<ProjectPlanningPORequisitionMaterialMaster>, IProjectPlanningPORequisitionMaterialMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaterialMasterService _materialMasterService;
        private readonly IProjectPlanningPORequisitionMaterialMasterArticleService _projectPlanningPORequisitionMaterialMasterArticleService;
        private readonly IRepositoryAsync<ProjectPlanningRequisitionMaterialMaster> _projectPlanningRequisitionMaterialMasterService;
        private readonly IUOMConversionService _uOMConversionService;

        public ProjectPlanningPORequisitionMaterialMasterService(
            IRepositoryAsync<ProjectPlanningPORequisitionMaterialMaster> projectPlanningRequisitionRepository
            , IPKGeneratorService pkGeneratorService
            , IMaterialMasterService materialMasterService
            , IProjectPlanningPORequisitionMaterialMasterArticleService projectPlanningPORequisitionMaterialMasterArticleService
            , IRepositoryAsync<ProjectPlanningRequisitionMaterialMaster> projectPlanningRequisitionMaterialMasterService
            , IUOMConversionService uOMConversionService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningRequisitionRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _materialMasterService = materialMasterService;
            _projectPlanningPORequisitionMaterialMasterArticleService = projectPlanningPORequisitionMaterialMasterArticleService;
            _projectPlanningRequisitionMaterialMasterService = projectPlanningRequisitionMaterialMasterService;
            _uOMConversionService = uOMConversionService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        //public string InsertAndUpdate(IEnumerable<ProjectPlanningPORequisitionMaterialMaster> entity)
        //{
        //    bool flag = false;
        //    string pkId = GetPK();
        //    try
        //    {
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        //CheckUnique(entity);
        //        _unitOfWork.BeginTransaction();
        //        flag = true;
        //        if (string.IsNullOrEmpty(entity.Id))
        //        {
        //            entity.Id = pkId;
        //            base.InsertGraph(entity);
        //        }
        //        else
        //            base.UpdateGraph(entity);

        //        _unitOfWork.SaveChanges();
        //        flag = false;
        //        _unitOfWork.Commit();
        //        return entity.Id;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
        //        entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
        //    }
        //    finally
        //    {
        //        if (flag)
        //            _unitOfWork.Rollback();
        //    }
        //}

        public void InsertORUpdate(ProjectPlanningPurchaseOrder obj, IEnumerable<ProjectPlanningPORequisitionMaterialMaster> entity)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var dbList = base.Query(r => r.ProjectPlanningPurchaseOrderId == obj.Id).Select().ToList();
                decimal dbSumQuantity;
                if (entity != null)
                {
                    decimal uomConvertedValue = 0;
                    decimal baseUoMConvertedValue = 0;
                    var requisitionMaterialMasterId = entity.First().ProjectPlanningRequsitionMaterialMasterId;
                    var dbAllPoMaterialList = base.Query(r => r.ProjectPlanningRequisitionId == obj.ProjectPlanningRequisitionId && r.ProjectPlanningId == obj.ProjectPlanningId).Select().ToList();
                    var pk = GetMaxNumber(nameof(ProjectPlanningPORequisitionMaterialMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entity)
                    {
                        if (item.Quantity <= 0)
                        {
                            throw new CustomException("Quantity must be greater than 0");
                        }
                        if (item.Rate <= 0)
                        {
                            throw new CustomException("Rate must be greater than 0");
                        }
                        var materialMasterName = _materialMasterService.Find(item.MaterialMasterId).UserName;
                        var reQuisitionInfo = _projectPlanningRequisitionMaterialMasterService.Query(t => t.Id == item.ProjectPlanningRequsitionMaterialMasterId).Select().FirstOrDefault();
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            if (Any(r => r.ProjectPlanningRequisitionId == item.ProjectPlanningRequisitionId && r.ProjectPlanningId == item.ProjectPlanningId && r.ProjectPlanningRequsitionMaterialMasterId == item.ProjectPlanningRequsitionMaterialMasterId && r.ProjectPlanningPurchaseOrderId == item.ProjectPlanningPurchaseOrderId))
                            {
                                throw new CustomException(materialMasterName + " This material already added");
                            }
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            item.ProjectPlanningPurchaseOrderId = obj.Id;
                            dbSumQuantity = dbAllPoMaterialList.Where(r => r.ProjectPlanningRequisitionId == obj.ProjectPlanningRequisitionId && r.ProjectPlanningId == item.ProjectPlanningId && r.ProjectPlanningRequsitionMaterialMasterId == item.ProjectPlanningRequsitionMaterialMasterId).Sum(r => r.Quantity);
                            uomConvertedValue = GetUomConversionValue(item.BaseUOMId, item.AlternativeUomId, item.Quantity, item.MaterialMasterId, reQuisitionInfo.AlternativeUomId);
                            baseUoMConvertedValue = GetUomConversionValue(item.AlternativeUomId, item.BaseUOMId, item.Quantity, item.MaterialMasterId);
                            if (uomConvertedValue + dbSumQuantity > reQuisitionInfo.Quantity)
                            {
                                throw new CustomException(materialMasterName + " Po Quantity can not be greater than requisition quantity ");
                            }
                            if (item.BaseUOMId != item.AlternativeUomId)
                            {
                                item.BaseUoMQuantity = baseUoMConvertedValue;
                            }
                            else
                            {
                                item.BaseUoMQuantity = item.Quantity;
                            }
                            if (reQuisitionInfo.AlternativeUomId != item.AlternativeUomId)
                            {
                                item.RequisitionUoMQuantity = uomConvertedValue;
                            }
                            else
                            {
                                item.RequisitionUoMQuantity = item.Quantity;
                            }
                            InsertGraph(item);
                        }
                        else
                        {
                            if (dbList.Any(r => r.ProjectPlanningRequsitionMaterialMasterId == requisitionMaterialMasterId && r.Id == item.Id))
                            {
                                dbSumQuantity = dbAllPoMaterialList.Where(r => r.ProjectPlanningRequisitionId == obj.ProjectPlanningRequisitionId && r.ProjectPlanningId == item.ProjectPlanningId && r.ProjectPlanningRequsitionMaterialMasterId == item.ProjectPlanningRequsitionMaterialMasterId).Sum(r => r.Quantity);
                                var dbItem = dbList.FirstOrDefault(r => r.ProjectPlanningRequsitionMaterialMasterId == requisitionMaterialMasterId && r.Id == item.Id);
                                uomConvertedValue = GetUomConversionValue(item.BaseUOMId, item.AlternativeUomId, item.Quantity, item.MaterialMasterId, reQuisitionInfo.AlternativeUomId);
                                baseUoMConvertedValue = GetUomConversionValue(item.BaseUOMId, item.AlternativeUomId, item.Quantity, item.MaterialMasterId);
                                if ((uomConvertedValue + dbSumQuantity) - dbItem.Quantity > reQuisitionInfo.Quantity)
                                {
                                    throw new CustomException(materialMasterName + "Po Quantity can not be greater than requisition quantity ");
                                }
                                if (item.BaseUOMId != item.AlternativeUomId)
                                {
                                    item.BaseUoMQuantity = baseUoMConvertedValue;
                                }
                                else
                                {
                                    item.BaseUoMQuantity = item.Quantity;
                                }
                                if (reQuisitionInfo.AlternativeUomId != item.AlternativeUomId)
                                {
                                    item.RequisitionUoMQuantity = uomConvertedValue;
                                }
                                else
                                {
                                    item.RequisitionUoMQuantity = item.Quantity;
                                }
                                UpdateGraph(item);
                            }
                            else
                            {
                                throw new CustomException(ServiceResources.RecordNoLonger);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
            //try
            //{
            //    if (entity != null)
            //    {
            //        var pk = base.GetMaxNumber("ProjectPlanningPORequisitionMaterialMaster", PKGeneratorEnum.Auto, null, DateTime.Now);
            //        foreach (var item in entity)
            //        {
            //            if (string.IsNullOrEmpty(item.Id))
            //            {
            //                if (base.Any(r => r.ProjectPlanningRequsitionMaterialMasterId == item.ProjectPlanningRequsitionMaterialMasterId && r.ProjectPlanningMaterialMasterId == item.ProjectPlanningMaterialMasterId))
            //                {
            //                    throw new CustomException(_materialMasterService.Find(item.MaterialMasterId).UserName + " This material already added");
            //                }
            //                pk.MaxNumber++;
            //                item.Id = pk.MaxNumber.ToString();
            //                item.ProjectPlanningPurchaseOrderId = pppOrdere.Id;
            //                item.ProjectPlanningId = pppOrdere.ProjectPlanningId;
            //                base.InsertGraph(item);
            //            }
            //            else if (!string.IsNullOrEmpty(item.Id) && !string.IsNullOrEmpty(item.ProjectPlanningId))
            //            {
            //                base.UpdateGraph(item);
            //            }
            //            //base.InsertOrUpdateGraph(item);
            //        }
            //    }
            //}
            //catch (CustomException)
            //{
            //    throw;
            //}
            //catch (Exception ex)
            //{
            //    throw new CustomException(ex.Message, ex,
            //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
            //        null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            //}
        }

        private decimal GetUomConversionValue(string baseUomId, string selectedUomId, decimal quantity, string materialMasterId, string requisitionUomId = null)
        {
            decimal qv = 0;
            Dictionary<string, object> uomConvertedInfo = (Dictionary<string, object>)GetMaterialUOMValueConversation(baseUomId, selectedUomId, Convert.ToInt32(quantity), materialMasterId, requisitionUomId).FirstOrDefault();
            if (uomConvertedInfo != null)
            {
                if (uomConvertedInfo.ContainsKey("ConvertedQuantity"))
                {
                    var a = uomConvertedInfo["ConvertedQuantity"].ToString();
                    qv = Convert.ToDecimal(a);
                }
            }
            return qv;
        }

        public IEnumerable<object> GetMaterialUOMValueConversation(string baseUomId, string selectedUomId, int quantity, string materialMasterId, string requisitionUomId = null)
        {
            try
            {
                var sql = "";
                if (requisitionUomId != null && baseUomId != selectedUomId)
                {
                    if (baseUomId != requisitionUomId)
                    {
                        sql = @"SELECT BaseUOMFactor*" + quantity + @"/(SELECT BaseUOMFactor FROM MST.MaterialMasterAlternativeUOM
                            WHERE BaseUOMId='" + baseUomId + "' AND AlternativeUOMId='" + requisitionUomId + @"' and MaterialMasterId='" + materialMasterId + @"') ConvertedQuantity  FROM MST.MaterialMasterAlternativeUOM
                            WHERE BaseUOMId='" + baseUomId + "' AND AlternativeUOMId='" + selectedUomId + "' and MaterialMasterId='" + materialMasterId + "'";
                    }
                    else
                    {
                        sql = @"SELECT BaseUOMFactor*" + quantity + @" ConvertedQuantity FROM MST.MaterialMasterAlternativeUOM
                                WHERE BaseUOMId='" + baseUomId + "' AND AlternativeUOMId='" + selectedUomId + "' and MaterialMasterId='" + materialMasterId + "'";
                    }
                }
                if (baseUomId == selectedUomId)
                {
                    if (baseUomId == selectedUomId && selectedUomId == requisitionUomId)
                    {
                        sql = @"SELECT top 1 AlternativeUOMFactor*" + quantity + @" ConvertedQuantity FROM MST.MaterialMasterAlternativeUOM WHERE BaseUOMId='" + selectedUomId + "' and MaterialMasterId='" + materialMasterId + "'";
                    }
                    else
                    {
                        sql = @"SELECT top 1 AlternativeUOMFactor *" + quantity + @"/(SELECT BaseUOMFactor FROM MST.MaterialMasterAlternativeUOM
                            WHERE BaseUOMId='" + baseUomId + "' AND AlternativeUOMId='" + requisitionUomId + "' and MaterialMasterId='" + materialMasterId + "') ConvertedQuantity FROM MST.MaterialMasterAlternativeUOM WHERE BaseUOMId='" + baseUomId + "' and MaterialMasterId='" + materialMasterId + "'";
                    }
                }
                if (requisitionUomId == null)
                {
                    sql = @"select BaseUOMFactor*" + quantity + @" ConvertedQuantity from mst.MaterialMasterAlternativeUOM
                            WHERE BaseUOMId='" + baseUomId + "' AND AlternativeUOMId='" + selectedUomId + "' and MaterialMasterId='" + materialMasterId + "'";
                }
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteGraph(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = base.Query(r => r.Id == Id).Select().FirstOrDefault();
                _projectPlanningPORequisitionMaterialMasterArticleService.DeleteGraph(data.Id);
                if (data != null)
                {
                    base.DeleteGraph(data);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteWithMaster(string Id)
        {
            try
            {
                var data = base.Query(r => r.ProjectPlanningPurchaseOrderId == Id).Select().ToList();
                if (data != null)
                {
                    for (int i = 0; i < data.Count(); i++)
                    {
                        _projectPlanningPORequisitionMaterialMasterArticleService.DeleteGraph(data[i].Id);
                        base.DeleteGraph(data[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProjectPlanningPORequisitionMaterialMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(ProjectPlanningPORequisitionMaterialMaster entity)
        {
            try
            {
                //CheckUnique(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"SELECT PPPO.*,FT.TotalAmount,FT.TotalQuantity,P.UserName AS Vendor,C.Name AS Currency,PP.Title,PP.Code AS ProjectPlanningCode FROM [MST].[ProjectPlanningRequisition] AS PPPO
                                        LEFT OUTER JOIN [HKP].[Party] AS P ON PPPO.PartyId = P.Id
                                        LEFT OUTER JOIN SCS.Currency AS C ON PPPO.CurrencyId=C.Id
                                        LEFT OUTER JOIN [MST].[ProjectPlanning] AS PP ON PPPO.ProjectPlanningId = PP.Id
                                        LEFT OUTER JOIN (SELECT ProjectPlanningRequisitionId, cast(SUM(PPPOM.Amount) as decimal(18,4)) AS TotalAmount,SUM(PPPOM.Quantity) AS TotalQuantity FROM  [MST].[ProjectPlanningRequisitionMaterialMaster] AS PPPOMF
                                                         LEFT OUTER JOIN (SELECT Id, Quantity,Rate,Quantity*Rate AS Amount FROM [MST].[ProjectPlanningRequisitionMaterialMaster]) AS PPPOM ON PPPOMF.Id = PPPOM.Id
                                                         LEFT OUTER JOIN [MST].[ProjectPlanningRequisition] AS PPPO ON PPPOMF.ProjectPlanningRequisitionId=PPPO.Id
                                                         WHERE PPPOMF.ProjectPlanningRequisitionId=PPPO.Id group by ProjectPlanningRequisitionId) AS FT ON PPPO.Id=FT.ProjectPlanningRequisitionId";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT PPC.Id AS Value, PPC.UserName AS Text FROM HKP.ProjectPlanningRequisition AS PPC
                                    left outer join(SELECT * FROM HKP.CompanyGroupWiseProjectPlanningRequisition WHERE CompanyGroupId = '" + identity.CompanyGroupId + @"') cgpc
                                     ON PPC.Id = cgpc.ProductId  WHERE ISNULL(cgpc.Id, '')<> '' AND PPC.Active=1  ORDER BY PPC.UserName ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel FindById(GridParameter parameters, string id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"	SELECT PPPO.*,FT.TotalAmount,FT.TotalQuantity,P.UserName AS Vendor,C.Name AS Currency,PP.Title,PP.Code AS ProjectPlanningCode FROM [MST].[ProjectPlanningRequisition] AS PPPO
                                        LEFT OUTER JOIN [HKP].[Party] AS P ON PPPO.PartyId = P.Id
                                        LEFT OUTER JOIN SCS.Currency AS C ON PPPO.CurrencyId=C.Id
                                        LEFT OUTER JOIN [MST].[ProjectPlanning] AS PP ON PPPO.ProjectPlanningId = PP.Id
                                        LEFT OUTER JOIN (SELECT ProjectPlanningRequisitionId, cast(SUM(PPPOM.Amount) as decimal(18,4)) AS TotalAmount,SUM(PPPOM.Quantity) AS TotalQuantity FROM  [MST].[ProjectPlanningRequisitionMaterialMaster] AS PPPOMF
                                                         LEFT OUTER JOIN (SELECT Id, Quantity,Rate,Quantity*Rate AS Amount FROM [MST].[ProjectPlanningRequisitionMaterialMaster]) AS PPPOM ON PPPOMF.Id = PPPOM.Id
                                                         LEFT OUTER JOIN [MST].[ProjectPlanningRequisition] AS PPPO ON PPPOMF.ProjectPlanningRequisitionId=PPPO.Id
                                                         WHERE PPPOMF.ProjectPlanningRequisitionId=PPPO.Id group by ProjectPlanningRequisitionId) AS FT ON PPPO.Id=FT.ProjectPlanningRequisitionId
                                        WHERE PPPO.Id='" + id + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetCompanyCurrencyCountryWise()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string _sql = @"SELECT CU.Code, C.BaseCurrencyId, CO.CurrencyId FROM ORG.Company AS C
                         INNER JOIN MST.AddressMaster AS AM ON AM.Id=C.AddressMasterId
                         INNER JOIN SCS.Country AS CO ON CO.Id=AM.CountryId
                         INNER JOIN SCS.Currency CU ON CU.Id=C.BaseCurrencyId
                        WHERE C.Id='" + identity.CompanyId + "' ";
            return _sqlRepository.GetDataCollection(_sql, null);
        }

        public IEnumerable<object> GetCoaIdByCompany()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string _sql = @"SELECT COM.COAId,C.UserName AS CoaName FROM [ORG].[Company] AS COM
                            LEFT OUTER JOIN [HKP].[COA] AS C ON COM.COAId = C.Id
                            WHERE COM.Id='" + identity.CompanyId + "' ";
            return _sqlRepository.GetDataCollection(_sql, null);
        }
    }
}