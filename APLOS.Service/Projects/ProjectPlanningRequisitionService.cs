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
    public class ProjectPlanningRequisitionService : Service<ProjectPlanningRequisition>, IProjectPlanningRequisitionService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IProjectPlanningRequisitionMaterialMasterService _projectPlanningRequisitionMaterialService;
        private readonly IProjectPlanningRequisitionMaterialMasterArticleService _projectPlanningRequisitionMaterialArticleService;
        private readonly IProjectPlanningPurchaseOrderService _projectPlanningPurchaseOrderService;

        public ProjectPlanningRequisitionService(
            IRepositoryAsync<ProjectPlanningRequisition> projectPlanningRequisitionRepository
            , IProjectPlanningRequisitionMaterialMasterService projectPlanningRequisitionMaterialService
            , IProjectPlanningRequisitionMaterialMasterArticleService projectPlanningRequisitionMaterialArticleService
            , IProjectPlanningPurchaseOrderService projectPlanningPurchaseOrderService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningRequisitionRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _projectPlanningRequisitionMaterialService = projectPlanningRequisitionMaterialService;
            _projectPlanningRequisitionMaterialArticleService = projectPlanningRequisitionMaterialArticleService;
            _projectPlanningPurchaseOrderService = projectPlanningPurchaseOrderService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public string InsertAndUpdate(ProjectPlanningRequisition entity)
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
            return GetAutoNumber(nameof(ProjectPlanningRequisition), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        //private void CheckUnique(ProjectPlanningRequisition entity)
        //{
        //    CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id);
        //}
        public override void Update(ProjectPlanningRequisition entity)
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

        public GridModel QueryGraph(GridParameter parameters, string projectPlanningId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"SELECT PPPO.*,P.UserName AS Vendor,PP.Title AS ProjectPlanningTitle,PP.Code AS ProjectPlanningCode FROM [MST].[ProjectPlanningRequisition] AS PPPO
                                        LEFT OUTER JOIN [HKP].[Party] AS P ON PPPO.PartyId = P.Id
                                        LEFT OUTER JOIN [MST].[ProjectPlanning] AS PP ON PPPO.ProjectPlanningId = PP.Id
														 WHERE PPPO.ProjectPlanningId='" + projectPlanningId + "'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                parameters.CmdText = @"SELECT PPPO.*,FT.TotalQuantity,P.UserName AS Vendor,PP.Title AS ProjectPlanningTitle,PP.Code AS ProjectPlanningCode FROM [MST].[ProjectPlanningRequisition] AS PPPO
                                        LEFT OUTER JOIN [HKP].[Party] AS P ON PPPO.PartyId = P.Id
                                        LEFT OUTER JOIN [MST].[ProjectPlanning] AS PP ON PPPO.ProjectPlanningId = PP.Id
                                        LEFT OUTER JOIN (SELECT ProjectPlanningRequisitionId,SUM(PPPOM.Quantity) AS TotalQuantity FROM  [MST].[ProjectPlanningRequisitionMaterialMaster] AS PPPOMF
                                                         LEFT OUTER JOIN (SELECT Id, Quantity FROM [MST].[ProjectPlanningRequisitionMaterialMaster]) AS PPPOM ON PPPOMF.Id = PPPOM.Id
                                                         LEFT OUTER JOIN [MST].[ProjectPlanningRequisition] AS PPPO ON PPPOMF.ProjectPlanningRequisitionId=PPPO.Id
                                                         WHERE PPPOMF.ProjectPlanningRequisitionId=PPPO.Id group by ProjectPlanningRequisitionId) AS FT ON PPPO.Id=FT.ProjectPlanningRequisitionId";

                //parameters.CmdText = @"SELECT PPPO.*,FT.TotalQuantity,P.UserName AS Vendor,C.Name AS Currency,PP.Title AS ProjectPlanningTitle,PP.Code AS ProjectPlanningCode FROM [MST].[ProjectPlanningRequisition] AS PPPO
                //                         LEFT OUTER JOIN [HKP].[Party] AS P ON PPPO.PartyId = P.Id
                //                         LEFT OUTER JOIN SCS.Currency AS C ON PPPO.CurrencyId=C.Id
                //                         LEFT OUTER JOIN [MST].[ProjectPlanning] AS PP ON PPPO.ProjectPlanningId = PP.Id
                //                         LEFT OUTER JOIN (SELECT ProjectPlanningRequisitionId,SUM(PPPOM.Quantity) AS TotalQuantity FROM  [MST].[ProjectPlanningRequisitionMaterialMaster] AS PPPOMF
                //                                          LEFT OUTER JOIN (SELECT Id, Quantity FROM [MST].[ProjectPlanningRequisitionMaterialMaster]) AS PPPOM ON PPPOMF.Id = PPPOM.Id
                //                                          LEFT OUTER JOIN [MST].[ProjectPlanningRequisition] AS PPPO ON PPPOMF.ProjectPlanningRequisitionId=PPPO.Id
                //                                          WHERE PPPOMF.ProjectPlanningRequisitionId=PPPO.Id group by ProjectPlanningRequisitionId) AS FT ON PPPO.Id=FT.ProjectPlanningRequisitionId";

                //parameters.CmdText = @"SELECT PPPO.*,P.UserName AS Vendor,C.Name AS Currency,PP.Title,PP.Code AS ProjectPlanningCode FROM [MST].[ProjectPlanningRequisition] AS PPPO
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
                //parameters.CmdText = @"	SELECT PPPO.*,FT.TotalQuantity,P.UserName AS Vendor,C.Name AS Currency,PP.Title ProjectPlanningTitle,PP.Code AS ProjectPlanningCode FROM [MST].[ProjectPlanningRequisition] AS PPPO
                //                        LEFT OUTER JOIN [HKP].[Party] AS P ON PPPO.PartyId = P.Id
                //                        LEFT OUTER JOIN SCS.Currency AS C ON PPPO.CurrencyId=C.Id
                //                        LEFT OUTER JOIN [MST].[ProjectPlanning] AS PP ON PPPO.ProjectPlanningId = PP.Id
                //                        LEFT OUTER JOIN (SELECT ProjectPlanningRequisitionId,SUM(PPPOM.Quantity) AS TotalQuantity FROM  [MST].[ProjectPlanningRequisitionMaterialMaster] AS PPPOMF
                //                                         LEFT OUTER JOIN (SELECT Id, Quantity FROM [MST].[ProjectPlanningRequisitionMaterialMaster]) AS PPPOM ON PPPOMF.Id = PPPOM.Id
                //                                         LEFT OUTER JOIN [MST].[ProjectPlanningRequisition] AS PPPO ON PPPOMF.ProjectPlanningRequisitionId=PPPO.Id
                //                                         WHERE PPPOMF.ProjectPlanningRequisitionId=PPPO.Id group by ProjectPlanningRequisitionId) AS FT ON PPPO.Id=FT.ProjectPlanningRequisitionId
                //                        WHERE PPPO.Id='" + id + "'";
                parameters.CmdText = @"	SELECT PPPO.*,FT.TotalQuantity,
                                        P.UserName AS Vendor,
                                        PP.Title ProjectPlanningTitle,
                                        PP.Code AS ProjectPlanningCode FROM [MST].[ProjectPlanningRequisition] AS PPPO
                                        LEFT OUTER JOIN [HKP].[Party] AS P ON PPPO.PartyId = P.Id
                                        LEFT OUTER JOIN [MST].[ProjectPlanning] AS PP ON PPPO.ProjectPlanningId = PP.Id
                                        LEFT OUTER JOIN (SELECT ProjectPlanningRequisitionId,SUM(PPPOM.Quantity) AS TotalQuantity FROM  [MST].[ProjectPlanningRequisitionMaterialMaster] AS PPPOMF
                                                         LEFT OUTER JOIN (SELECT Id, Quantity FROM [MST].[ProjectPlanningRequisitionMaterialMaster]) AS PPPOM ON PPPOMF.Id = PPPOM.Id
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

        public IEnumerable<object> GetMaterialMasterAttributeValueList(string materialMasterId)
        {
            string _sql = @"SELECT DISTINCT A.MaterialAttributeId,A.MaterialAttributeValueId,A.MaterialMasterAttributeValueId
                                ,[Text]= CASE WHEN A.MaterialAttributeValueId<>'' THEN B.UserName
			                                WHEN A.MaterialMasterAttributeValueId<>'' THEN C.UserName
			                                ELSE A.MaterialAttributeValueFreeText END
                                FROM MST.MaterialMasterArticleValue AS A
                                LEFT OUTER JOIN HKP.MaterialAttributeValue AS B ON B.Id=A.MaterialAttributeValueId
                                LEFT OUTER JOIN MST.MaterialMasterAttributeValue AS C ON C.Id=A.MaterialMasterAttributeValueId
                                where A.MaterialMasterId='" + materialMasterId + "'";
            return _sqlRepository.GetDataCollection(_sql);
        }

        public void DeleteMasterWithChild(string Id)
        {
            var flag = false;
            try
            {
                var data = base.Query(r => r.Id == Id).Select().FirstOrDefault();
                if (data != null)
                {
                    var poHas = _projectPlanningPurchaseOrderService.Query(r => r.ProjectPlanningRequisitionId == Id).Select().FirstOrDefault();
                    if (poHas != null)
                    {
                        throw new CustomException("This requisition is already used on purchaseorder " + poHas.Id);
                    }
                    else
                    {
                        _unitOfWork.BeginTransaction();
                        flag = true;
                        _projectPlanningRequisitionMaterialService.DeleteWithMaster(Id);
                        base.DeleteGraph(data);
                        _unitOfWork.SaveChanges();
                        flag = false;
                        _unitOfWork.Commit();
                    }
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
    }
}