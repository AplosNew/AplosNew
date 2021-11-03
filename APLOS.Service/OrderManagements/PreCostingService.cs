#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class PreCostingService : Service<PreCosting>, IPreCostingService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IRepositoryAsync<PreCostingDetail> _preCostingDetailRepository;

        public PreCostingService(
            IRepositoryAsync<PreCosting> preCostingRepository,
            IPKGeneratorService pkGeneratorService,
            IRepositoryAsync<PreCostingDetail> preCostingDetailRepository,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(preCostingRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
            _preCostingDetailRepository = preCostingDetailRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT PC.*,B.UserName BuyerName,MM.UserName FinishedGoods,C.UserName CriticalName FROM [TRN].[PreCosting] PC
                                        LEFT JOIN HKP.Buyer B ON PC.BuyerId = B.Id
                                        LEFT JOIN HKP.Critical C ON PC.CriticalId=C.Id
                                        LEFT JOIN MST.MaterialMaster MM ON PC.MaterialMasterId=MM.Id
                                        WHERE PC.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetPreCostingDetailList(string preCostingId)
        {
            try
            {
                string _sql = @"SELECT PD.*,MG.UserName MaterialGroupMasterName,MG.Code,MT.Description MaterialTypeName,UOM.UserName BaseUOM FROM [TRN].[PreCostingDetail] PD
                                LEFT JOIN MST.MaterialGroupMaster MG ON PD.MaterialGroupMasterId=MG.Id
								LEFT JOIN HKP.MaterialType MT ON MG.MaterialTypeId=MT.Id
                                LEFT JOIN SCS.UnitOfMeasurement UOM ON MG.BaseUoMId=UOM.Id
                                WHERE PD.PreCostingId='" + preCostingId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IEnumerable<Object> getUomList()
        {
            var sql = @"SELECT DISTINCT UOM1.Id AS UoMID,UOM1.UserName AS UoM, MM.Id
                        FROM MST.MaterialGroupMaster AS MM
                        LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOM1 ON MM.BaseUOMId = UOM1.Id
                        UNION
                        SELECT DISTINCT UOM2.Id AS UoMID,UOM2.UserName AS UoM,
                         MM.Id
                        FROM MST.MaterialGroupMaster AS MM
                        LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOM2 ON MM.BaseUoMId = UOM2.Id
                        WHERE MM.BaseUoMId IS NOT NULL
                        UNION
                        SELECT DISTINCT UOM3.Id AS UoMID,UOM3.UserName AS UoM, MMALT.MaterialGroupMasterId AS Id
                        FROM  MST.MaterialGroupAlternativeUoM AS MMALT
                        LEFT OUTER JOIN SCS.UnitOfMeasurement AS UOM3 ON MMALT.AlternativeUOMId = UOM3.Id";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public GridModel GetFinishGoodsWithCompanyGroup(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT MM.Id,MM.UserName FinishedGoods,MG.UserName MaterialGroupName,PM.UserName ProductMasterName
                             FROM TRN.ProductDefinition PD
                             JOIN MST.MaterialMaster MM ON MM.Id= PD.MaterialMasterId
                             left join mst.MaterialGroupMaster MG ON MM.MaterialGroupMasterId = MG.Id
                             left join mst.ProductMaster PM ON PD.ProductMasterId=PM.Id
                            WHERE MM.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetProductPreCostingWithCompanyGroup(GridParameter parameters, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT MM.Id,MM.UserName FinishedGoods,MG.UserName MaterialGroupName,PM.UserName ProductMasterName FROM TRN.ProductDefinition PD
                             JOIN MST.MaterialMaster MM ON MM.Id= PD.MaterialMasterId
                             left join mst.MaterialGroupMaster MG ON MM.MaterialGroupMasterId = MG.Id
                             left join mst.ProductMaster PM ON PD.ProductMasterId=PM.Id
                            WHERE MM.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            return base.GetAutoNumber(nameof(PreCosting), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public void InsertAndUpdate(PreCosting entity)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                string pkId = GetPK();
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = pkId;
                    base.InsertGraph(entity);
                }
                else
                {
                    base.UpdateGraph(entity);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void PreCostingDetailInsertAndUpdate(IEnumerable<PreCostingDetail> entities)
        {
            var flag = false;
            try
            {
                if (entities == null)
                    throw new CustomException("Please add any row.");
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = base.GetMaxNumber(nameof(PreCostingDetail), PKGeneratorEnum.Auto, identity.CompanyGroupId, DateTime.Now);
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        item.ModelState = ModelState.Added;
                        AuditService.Log(item);
                    }
                    else
                    {
                        item.ModelState = ModelState.Modified;
                        AuditService.Log(item);
                    }
                    _preCostingDetailRepository.InsertOrUpdateGraph(item);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException cx)
            {
                throw cx;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new CustomException(string.Format(ResourcesCore.IsNull, "PreCosting Id"));
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Find(id);
                if (data != null)
                {
                    IEnumerable<PreCostingDetail> commitmentPreCosting = _preCostingDetailRepository.Query(r => r.PreCostingId == data.Id).Select();
                    if (commitmentPreCosting != null)
                    {
                        _preCostingDetailRepository.ExecuteSqlCommand("DELETE FROM TRN.PreCostingDetail Where PreCostingId='" + data.Id + "'");
                    }
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

        public void DeletePreCostingDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new CustomException(string.Format(ResourcesCore.IsNull, "Id"));
            try
            {
                _preCostingDetailRepository.Delete(id);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<Object> GetPreCostingCalculation(string plantId, string fgId)
        {
            var sql = @"SELECT SUM(ecc.NoOfWorkStation) TotalWorkStationi,SUM(Cost) TotalHourlyCost  from SCS.WorkcenterMaster wcm
left join scs.WorkCenterMasterProductPriority wcmp on wcm.Id= wcmp.WorkCenterMasterId
                        join scs.EntityComponentCosting ecc on wcm.EntityId=ecc.EntityId
                        left join (
                        SELECT sum( MonthlyFixedCost) + sum(SemiVariableCost) Cost,EntityComponentCostingId FROM scs.EntityComponentCostingDetail WHERE FirstOption=1 group by EntityComponentCostingId
                        )a on ecc.Id=a.EntityComponentCostingId
                        where wcm.PlantId='" + plantId + "' and wcmp.MaterialMasterId='" + fgId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<Object> GetPreCostingCalculationWithEntity(string plantId, string fgId)
        {
            var sql = @"SELECT wcm.EntityId,E.UserName EntityName,ecc.NoOfWorkStation WorkStation,HourlyFixedCost,HourlyVariableCost  from SCS.WorkcenterMaster wcm
                        left join scs.WorkCenterMasterProductPriority wcmp on wcm.Id= wcmp.WorkCenterMasterId
                        join scs.EntityComponentCosting ecc on wcm.EntityId=ecc.EntityId
                        left join (
                        SELECT sum( MonthlyFixedCost) HourlyFixedCost,sum(SemiVariableCost) HourlyVariableCost,EntityComponentCostingId FROM scs.EntityComponentCostingDetail WHERE FirstOption=1 group by EntityComponentCostingId
                        )a on ecc.Id=a.EntityComponentCostingId
						LEFT JOIN ORG.Entity E ON ECC.EntityId=E.Id
                        where wcm.PlantId='" + plantId + "' and wcmp.MaterialMasterId='" + fgId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<Object> GetPlantWithWorkCenter(string companyId)
        {
            var sql = @"SELECT DISTINCT PlantId Value, p.UserName Text FROM SCS.WorkCenterMaster wcm
                        left join org.Plant p on wcm.PlantId=p.Id
                        where wcm.CompanyId='" + companyId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<Object> GetFGNoOfWorkStation(string finishGoodId)
        {
            var sql = @"SELECT SUM(PDE.NoOfWorkStation) FGNoOfWorkStation,SUM(PDE.EfficencyPercentage)/100 EfficencyPercentage,PDE.SPT FGSPT FROM [TRN].[ProductDefinition] PD
                        LEFT JOIN TRN.ProductDefinitionEfficency PDE ON PD.Id=PDE.ProductDefinitionId
                        WHERE PD.MaterialMasterId='" + finishGoodId + "' GROUP BY PDE.SPT ";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<Object> GetMaterialGroupArticlePrdProcessGroupList(string materialGroupArticleId)
        {
            var sql = @"SELECT MGAP.Id,MGPP.ProductionProcessGroupId,MGPP.InputId,MGPP.Sequence,PPG.UserName ProductionProcessGroupName,MGAP.Wastage
                        FROM  [MST].[MaterialGroupArticlePrdProcessGroup] MGAP
                        LEFT JOIN [MST].[MaterialGroupProductionProcessGroup] MGPP ON MGAP.MaterialGroupProductionProcessGroupId=MGPP.Id
                         LEFT JOIN HKP.ProductionProcessGroup PPG ON MGPP.ProductionProcessGroupId=PPG.Id
                         WHERE  MGAP. MaterialGroupArticleId='"+ materialGroupArticleId + "' ORDER BY MGPP.Sequence ASC";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<Object> GetMaterialGroupProcessCritia(string materialGroupArticleId)
        {
            var sql = @"Select * from [MST].[MaterialGroupProcessCriteria]
	                    WHERE MaterialGroupArticleId='" + materialGroupArticleId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }
    }
}