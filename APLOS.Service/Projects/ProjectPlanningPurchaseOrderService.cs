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
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Projects
{
    public class ProjectPlanningPurchaseOrderService : Service<ProjectPlanningPurchaseOrder>, IProjectPlanningPurchaseOrderService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IProjectPlanningPORequisitionMaterialMasterService _projectPlanningPORequisitionMaterialMasterService;
        private readonly IProjectPlanningPurchaseOrderDetailService _projectPlanningPurchaseOrderDetailService;

        public ProjectPlanningPurchaseOrderService(
            IRepositoryAsync<ProjectPlanningPurchaseOrder> projectPlanningPurchaseOrderRepository
            , IProjectPlanningPurchaseOrderDetailService projectPlanningPurchaseOrderDetailService
            , IProjectPlanningPORequisitionMaterialMasterService projectPlanningPORequisitionMaterialMasterService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningPurchaseOrderRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _projectPlanningPORequisitionMaterialMasterService = projectPlanningPORequisitionMaterialMasterService;
            _projectPlanningPurchaseOrderDetailService = projectPlanningPurchaseOrderDetailService;
        }

        #endregion Constructor

        //public string InsertAndUpdate(ProjectPlanningPurchaseOrder entity, IEnumerable<ProjectPlanningPurchaseOrderDetail> projectPlanningPurchaseOrderDetail, IEnumerable<ProjectPlanningPurchaseOrderMaterialMaster> projectPlanningPurchaseOrderMaterial, IEnumerable<ProjectPlanningPurchaseOrderMachineType> projectPlanningPurchaseOrderMachineType)
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

        //        _projectPlanningPurchaseOrderDetailService.InsertOrUpdate(projectPlanningPurchaseOrderDetail, entity.Id);
        //        _projectPlanningPurchaseOrderMaterialService.InsertOrUpdate(projectPlanningPurchaseOrderMaterial, entity.Id);
        //        _projectPlanningPurchaseOrderMachineTypeService.InsertOrUpdate(projectPlanningPurchaseOrderMachineType, entity.Id);
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

        public string InsertAndUpdate(ProjectPlanningPurchaseOrder entity)
        {
            var flag = false;
            string pkId = GetPK();
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = pkId;
                    InsertGraph(entity);
                }
                else
                    UpdateGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
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
                if (data != null)
                {
                    _projectPlanningPurchaseOrderDetailService.DeleteWithMaster(Id);
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

        public void DeleteWithChild(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = base.Query(r => r.Id == Id).Select().FirstOrDefault();
                if (data != null)
                {
                    _projectPlanningPORequisitionMaterialMasterService.DeleteWithMaster(Id);
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

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProjectPlanningPurchaseOrder), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        //private void CheckUnique(ProjectPlanningPurchaseOrder entity)
        //{
        //    CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id);
        //}
        public override void Update(ProjectPlanningPurchaseOrder entity)
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

                parameters.CmdText = @"SELECT PPPO.*,PPR.Description AS RequisitionTitle,FT.TotalAmount,FT.TotalQuantity,P.UserName AS Vendor,C.Name AS Currency,PP.Title,PP.Code AS ProjectPlanningCode FROM [MST].[ProjectPlanningPurchaseOrder] AS PPPO
                                        LEFT OUTER JOIN [HKP].[Party] AS P ON PPPO.PartyId = P.Id
                                        LEFT OUTER JOIN SCS.Currency AS C ON PPPO.CurrencyId=C.Id
                                        LEFT OUTER JOIN [MST].[ProjectPlanning] AS PP ON PPPO.ProjectPlanningId = PP.Id
                                        LEFT OUTER JOIN [MST].[ProjectPlanningRequisition] AS PPR ON PPR.ID = PPPO.ProjectPlanningRequisitionId
                                        LEFT OUTER JOIN (SELECT ProjectPlanningPurchaseOrderId, cast(SUM(PPPOM.Amount) as decimal(18,4)) AS TotalAmount,SUM(PPPOM.Quantity) AS TotalQuantity FROM  [MST].[ProjectPlanningPORequisitionMaterialMaster] AS PPPOMF
                                                         LEFT OUTER JOIN (SELECT Id, Quantity,Rate,Quantity*Rate AS Amount FROM [MST].[ProjectPlanningPORequisitionMaterialMaster]) AS PPPOM ON PPPOMF.Id = PPPOM.Id
                                                         LEFT OUTER JOIN [MST].[ProjectPlanningPurchaseOrder] AS PPPO ON PPPOMF.ProjectPlanningPurchaseOrderId=PPPO.Id
                                                         WHERE PPPOMF.ProjectPlanningPurchaseOrderId=PPPO.Id group by ProjectPlanningPurchaseOrderId) AS FT ON PPPO.Id=FT.ProjectPlanningPurchaseOrderId";

                //parameters.CmdText = @"SELECT PPPO.*,P.UserName AS Vendor,C.Name AS Currency,PP.Title,PP.Code AS ProjectPlanningCode FROM [MST].[ProjectPlanningPurchaseOrder] AS PPPO
                //                        LEFT OUTER JOIN [HKP].[Party] AS P ON PPPO.PartyId = P.Id
                //                        LEFT OUTER JOIN SCS.Currency AS C ON PPPO.CurrencyId=C.Id
                //                        LEFT OUTER JOIN [MST].[ProjectPlanning] AS PP ON PPPO.ProjectPlanningId = PP.Id";
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
                string _sql = @"SELECT PPC.Id AS Value, PPC.UserName AS Text FROM HKP.ProjectPlanningPurchaseOrder AS PPC
                                left outer join(SELECT * FROM HKP.CompanyGroupWiseProjectPlanningPurchaseOrder WHERE CompanyGroupId = '" + identity.CompanyGroupId + @"') cgpc
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
                parameters.CmdText = @"	SELECT PPPO.*,PPR.Description AS RequisitionTitle,FT.TotalAmount,FT.TotalQuantity,P.UserName AS Vendor,C.Name AS Currency,PP.Title,PP.Code AS ProjectPlanningCode FROM [MST].[ProjectPlanningPurchaseOrder] AS PPPO
                                        LEFT OUTER JOIN [HKP].[Party] AS P ON PPPO.PartyId = P.Id
                                        LEFT OUTER JOIN SCS.Currency AS C ON PPPO.CurrencyId=C.Id
                                        LEFT OUTER JOIN [MST].[ProjectPlanning] AS PP ON PPPO.ProjectPlanningId = PP.Id
                                        LEFT OUTER JOIN [MST].[ProjectPlanningRequisition] AS PPR ON PPR.ID = PPPO.ProjectPlanningRequisitionId
                                        LEFT OUTER JOIN (SELECT ProjectPlanningPurchaseOrderId, cast(SUM(PPPOM.Amount) as decimal(18,4)) AS TotalAmount,SUM(PPPOM.Quantity) AS TotalQuantity FROM  [MST].[ProjectPlanningPORequisitionMaterialMaster] AS PPPOMF
                                                         LEFT OUTER JOIN (SELECT Id, Quantity,Rate,Quantity*Rate AS Amount FROM [MST].[ProjectPlanningPORequisitionMaterialMaster]) AS PPPOM ON PPPOMF.Id = PPPOM.Id
                                                         LEFT OUTER JOIN [MST].[ProjectPlanningPurchaseOrder] AS PPPO ON PPPOMF.ProjectPlanningPurchaseOrderId=PPPO.Id
                                                         WHERE PPPOMF.ProjectPlanningPurchaseOrderId=PPPO.Id group by ProjectPlanningPurchaseOrderId) AS FT ON PPPO.Id=FT.ProjectPlanningPurchaseOrderId
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

        public GridModel ProjectPlanningRequisitionMaterialMasterSavedList(GridParameter parameters, string projectPlanningRequisitionId, string materialType, string projectPlanningId)
        {
            try
            {
                parameters.CmdText = @"SELECT   PPRM.Id
                                                ,PPRM.Id PPRequisitonMaterialMasterId
                                                ,PPRM.Quantity RequisitionQuantity
                                                ,PPRM.ProjectPlanningMaterialMasterId
                                                ,UOMPP.UserName AS RequisitionUOM
                                                ,PPRM.MaterialMasterId
                                                ,MM.UserName
                                                ,FAM.UserName AS FixedAssetName
												,FAM.AssetType
                                                ,UOMB.UserName AS BaseUom,UOMB.Id BaseUOMId
                                                ,PPRM.AlternativeUomId RequisitionUoMId
												,Isnull(PPOR.RaisedQuantity,0) RaisedQuantity
                                                FROM MST.ProjectPlanningRequisitionMaterialMaster PPRM
												LEFT JOIN (SELECT SUM(RequisitionUoMQuantity) RaisedQuantity,ProjectPlanningRequsitionMaterialMasterId FROM [MST].[ProjectPlanningPORequisitionMaterialMaster] WHERE ProjectPlanningId='" + projectPlanningId + @"'
											    GROUP BY ProjectPlanningRequsitionMaterialMasterId) PPOR ON PPRM.Id=PPOR.ProjectPlanningRequsitionMaterialMasterId
                                                LEFT OUTER JOIN [MST].[MaterialMaster] AS MM  ON PPRM.MaterialMasterId= MM.Id
                                                LEFT OUTER JOIN [SCS].[UnitOfMeasurement] AS UOMPP ON PPRM.AlternativeUomId = UOMPP.Id
                                                LEFT OUTER JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id = MM.AssetMasterId
                                                INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
												LEFT JOIN MST.ProjectPlanningMaterialMaster AS PPMM ON PPRM.ProjectPlanningMaterialMasterId=PPMM.Id
                                                WHERE PPRM.ProjectPlanningRequisitionId='" + projectPlanningRequisitionId + "' AND PPMM.MaterialMasterType='" + materialType + "'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
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