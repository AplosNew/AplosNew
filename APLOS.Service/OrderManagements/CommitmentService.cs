#region Using

using Library.Core;
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
using System.Linq;
#endregion Using

namespace Library.Service.OrderManagements
{
    public class CommitmentService : Service<Commitment>, ICommitmentService
    {
        #region Constructor

        private readonly ICommitmentMonthService _commitmentMonthService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<CommitmentValueAddedProcess> _commitmentValueAddedProcessRepository;

        public CommitmentService(
            IRepositoryAsync<Commitment> commitmentRepository
            , IRepositoryAsync<CommitmentValueAddedProcess> commitmentValueAddedProcessRepository
            , ICommitmentMonthService commitmentMonthService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(commitmentRepository, unitOfWork, pkGeneratorService)
        {
            _commitmentMonthService = commitmentMonthService;
            _commitmentValueAddedProcessRepository = commitmentValueAddedProcessRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string entityId)
        {
            try
            {
                parameters.CmdText = @"SELECT C.Id, C.FOB, C.CM, C.SPT, C.Efficiency, C.Target, REPLACE(CONVERT(VARCHAR(11), C.LSD, 113), ' ', '-') LSD,REPLACE(CONVERT(VARCHAR(11), C.ClosingDate, 113), ' ', '-') ClosingDate
                                     ,B.UserName BuyerMaster, BDP.UserName DepartmentName,BDV.UserName DivisionName,BB.UserName BuyerBrand,CN.Code Currency, --BS.UserName BuyerStyle,
                                     S.UserName Season,BP.UserName BuyerProgram,C.EntityId,C.BuyerBrandId,C.BuyerMasterId,C.BuyerProgramId,C.Buyer
									 ,C.CurrencyId,C.ProductMasterId, B.Id BuyerId,B.UserName BuyerName,C.Year,C.SeasonId,C.Remarks,PM.UserName ProductMaster
                                     FROM [TRN].[Commitment] C
                                     LEFT JOIN [ORG].[Entity]  E ON C.EntityId=E.Id
                                     LEFT JOIN [HKP].[BuyerBrand] BB ON C.BuyerBrandId=BB.Id
                                     LEFT JOIN [SCS].[Currency] CN ON C.CurrencyId=CN.Id
                                     --LEFT JOIN [HKP].[BuyerStyle] BS ON C.BuyerStyleId=BS.Id
                                     LEFT JOIN [HKP].[Season] S ON C.SeasonId=S.Id
                                     LEFT JOIN [HKP].[BuyerProgram] BP ON C.BuyerProgramId=BP.Id
                                     LEFT JOIN [MST].[BuyerMaster] BM ON C.BuyerMasterId=BM.Id
                                     LEFT JOIN [MST].[ProductMaster] PM ON C.ProductMasterId=PM.Id
                                     LEFT JOIN [HKP].[BuyerDepartment] BDP ON BM.BuyerDepartmentId=BDP.Id
                                     LEFT JOIN [HKP].[BuyerDivision] BDV ON BM.BuyerDivisionId=BDV.Id
                                     LEFT JOIN HKP.Buyer B ON BM.BuyerId= B.Id
                                     WHERE C.EntityId = '" + entityId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public GridModel GetMaterialMasterList(GridParameter parameters, string groupId)
        {
            try
            {
                parameters.CmdText = @"SELECT MGP.UserName AS MaterialGroup, MT.[Description] AS MaterialType, MM.Id ,MM.Code ,MM.UserName [FinishedGoods], UOMB.UserName AS BaseUoM, PM.UserName AS ProductMaster
                        FROM [TRN].[ProductDefinition] AS PD
                        LEFT JOIN [MST].[MaterialMaster] AS MM ON PD.MaterialMasterId= MM.Id
                        LEFT JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                        LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                        LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                        LEFT JOIN hkp.ProductCategory pc on pc.Id=pm.ProductCategoryId
                        LEFT JOIN hkp.ProductSubCategory psc on psc.Id=pm.ProductSubCategoryId
                        LEFT JOIN[SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                        WHERE MM.CompanyGroupId='" + groupId + "' AND MM.Archive=0 AND MM.Active=1";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }


        public GridModel GetProductMasterList(GridParameter parameters, string groupId)
        {
            try
            {
                parameters.CmdText = @"SELECT PM.Id, PM.UserName, PM.StandardName,PC.UserName ProductCategory, PSC.UserName ProductSubCategory , P.UserName Process
                                     FROM MST.ProductMaster PM
                                     LEFT JOIN HKP.ProductCategory PC ON PC.Id=PM.ProductCategoryId
                                     LEFT JOIN HKP.ProductSubCategory PSC ON PSC.Id=PM.ProductSubCategoryId
                                     LEFT JOIN HKP.Process P ON  P.Id= PM.BaseProcessId
                                     WHERE PM.CompanyGroupId='" + groupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetSalesGroupCbo(string entityId)
        {
            try
            {
                var sql = @"SELECT SG.Id, SG.UserName FROM [SEC].[UserSalesGroup] AS USG
                            LEFT JOIN [ORG].[SalesGroup] AS SG ON USG.SalesGroupId=SG.Id
                            JOIN (SELECT SORGP.SalesOrganisationId FROM [ORG].[SalesOrganisationPlant] AS SORGP
                            LEFT JOIN [ORG].[Entity] AS E ON E.PlantId=SORGP.PlantId WHERE E.Id='" + entityId + @"' AND SORGP.Active=1)
                            AS SP ON SG.SalesOrganizationId=SP.SalesOrganisationId";
                return _sqlRepository.GetCombo(sql, "Id", "UserName");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }


        public IEnumerable<ComboModel> GetCbo()
        {
            try
            {
                var sql = @"SELECT C.Id,(B.UserName+' - '+BP.UserName) BuyerMaster FROM [TRN].[Commitment] C
									 LEFT JOIN [MST].[BuyerMaster] BM ON C.BuyerMasterId=BM.Id
									 LEFT JOIN HKP.Buyer B ON BM.BuyerId= B.Id
									 LEFT JOIN [HKP].[BuyerProgram] BP ON C.BuyerProgramId=BP.Id";
                return _sqlRepository.GetCombo(sql, "Id", "BuyerMaster");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private string GetPK() => GetAutoNumber(nameof(Commitment), PKGeneratorEnum.Yearly, null, DateTime.Now);

        public void Insert(Commitment entity, IEnumerable<CommitmentMonth> monthList, IEnumerable<CommitmentValueAddedProcess> cvAddedList)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                base.InsertGraph(entity);
                _commitmentMonthService.InsertGraph(entity.Id, monthList);
                InsertOrUpdateValueAddedProcess(cvAddedList, entity.Id);
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
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void Update(Commitment entity, IEnumerable<CommitmentMonth> monthList, IEnumerable<CommitmentValueAddedProcess> cvAddedList)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var dbData = base.Find(entity.Id);
                if (dbData == null) throw new CustomException("This data not exist.");
                base.UpdateGraph(entity);
                _commitmentMonthService.UpdateGraph(entity.Id, monthList);
                InsertOrUpdateValueAddedProcess(cvAddedList, entity.Id);
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
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void InsertOrUpdateValueAddedProcess(IEnumerable<CommitmentValueAddedProcess> entities, string pk)
        {
            var dbList = _commitmentValueAddedProcessRepository.Query(t => t.CommitmentId == pk).Select();
            if (entities != null)
            {
                var count = _commitmentValueAddedProcessRepository.CreateChildPk(t => t.CommitmentId == pk, x => x.Id, pk).ToInt();
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = pk + "-" + count;
                        item.CommitmentId = pk;
                        AuditService.AddedLog(item);
                        _commitmentValueAddedProcessRepository.Insert(item);
                        count++;
                    }
                    else
                    {
                        if (_commitmentValueAddedProcessRepository.Any(t => t.CommitmentId == pk))
                        {
                            AuditService.UpdatedLog(item);
                            _commitmentValueAddedProcessRepository.Update(item);
                        }
                        else
                            throw new CustomException(ServiceResources.RecordNoLonger.ToString());
                    }
                }
            }
        }

        public void DeleteMaster(string id)
        {
            var flag = false;
            try
            {
                var dbData = base.Find(id);
                if (dbData == null) throw new CustomException("This data not exist.");
                _unitOfWork.BeginTransaction();
                flag = true;
                _commitmentMonthService.DeleteMonth(id);
                DeleteCommitmentValueProcess(id);
                base.Delete(dbData);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteCommitmentValueProcess(string masterId)
        {
            var dbList = _commitmentValueAddedProcessRepository.Query(t => masterId.Contains(t.CommitmentId)).Select().ToList();
            if (dbList != null)
            {
                foreach (var item in dbList)
                {
                    _commitmentValueAddedProcessRepository.Delete(item);
                }
            }
        }

        public void DeleteProcess(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = _commitmentValueAddedProcessRepository.Find(Id);
                if (data != null)
                {
                    _commitmentValueAddedProcessRepository.Delete(data);
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

        public IEnumerable<object> QueryCommitmentValueAdded(string masterId)
        {
            try
            {
                var sql = @"SELECT CV.*,P.UserName ProcessName,SP.UserName SubProcessName FROM [TRN].[CommitmentValueAddedProcess] CV
                                        LEFT JOIN  HKP.Process P ON CV.ProcessId=p.Id
                                        LEFT JOIN HKP.SubProcess SP ON CV.SubProcessId=SP.Id
                                        WHERE CV.CommitmentId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetCommitmentData()
        {
            try
            {
                var sql = @"SELECT C.Id ,B.UserName BuyerDefinition, BDP.UserName DepartmentName,BDV.UserName DivisionName
                          ,B.UserName BuyerName,PM.UserName ProductMaster
                          FROM [TRN].[Commitment] C
                          LEFT JOIN [MST].[BuyerMaster] BM ON C.BuyerMasterId=BM.Id
                          LEFT JOIN [MST].[ProductMaster] PM ON C.ProductMasterId=PM.Id
                          LEFT JOIN [HKP].[BuyerDepartment] BDP ON BM.BuyerDepartmentId=BDP.Id
                          LEFT JOIN [HKP].[BuyerDivision] BDV ON BM.BuyerDivisionId=BDV.Id
                          LEFT JOIN HKP.Buyer B ON BM.BuyerId= B.Id Order by B.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }
    }
}