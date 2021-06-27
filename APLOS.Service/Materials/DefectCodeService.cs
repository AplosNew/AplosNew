#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
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

namespace Library.Service.Materials
{
    public class DefectCodeService : Service<DefectCode>, IDefectCodeService
    {
        #region Constructor

        private readonly IDefectCodeDetailService _defectCodeDetailService;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public DefectCodeService(
            IRepositoryAsync<DefectCode> charaterValueRepository,
            IPKGeneratorService pkGeneratorService,
            IDefectCodeDetailService defectCodeDetailService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(charaterValueRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _defectCodeDetailService = defectCodeDetailService;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string processId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            parameters.CmdText = @"SELECT d.Id
	                                    , d.CompanyGroupId
	                                    , d.ProcessId
	                                    , d.Code
	                                    , d.[Description]
	                                    , d.Active
	                                    , d.Archive
	                                    , p.UserName AS ProcessName
                                    FROM MST.DefectCode
                                    As d INNER JOIN HKP.Process AS p ON p.Id=d.ProcessId WHERE
                                    d.CompanyGroupId='" + identity.CompanyGroupId + "' AND d.ProcessId='" + processId + "' AND d.Archive=0";
            return _sqlRepository.GetGridData(parameters);
        }

        public void Insert(DefectCode entity, IEnumerable<DefectCodeDetail> defectCodeDetail)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.Id = GetPK();
                entity.CompanyGroupId = identity.CompanyGroupId;
                InsertGraph(entity);
                var detailPk = _pkGeneratorService.GetMaxNumber(nameof(DefectCodeDetail), PKGeneratorEnum.Auto, identity.CompanyGroupId, DateTime.Now);
                if (defectCodeDetail != null)
                {
                    foreach (var item in defectCodeDetail)
                    {
                        item.Id = (detailPk.MaxNumber++).ToString();
                        item.DefectCodeId = entity.Id;
                        _defectCodeDetailService.InsertGraph(item);
                    }
                }
                else
                    throw new CustomException("Please select atleast one defect value..........!");
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        private void Check(DefectCode entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive && r.ProcessId == entity.ProcessId);
            CheckUniqueColumn(UniqueColumnName.Description, entity.Description, r => r.Id != entity.Id && r.Description == entity.Description && r.CompanyGroupId == identity.CompanyGroupId && !r.Archive && r.ProcessId == entity.ProcessId);
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(DefectCode), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void Update(DefectCode entity, IEnumerable<DefectCodeDetail> defectCodeDetail, string[] deletedItems)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                UpdateGraph(entity);
                var detailPk = _pkGeneratorService.GetMaxNumber(nameof(DefectCodeDetail), PKGeneratorEnum.Auto, identity.CompanyGroupId, DateTime.Now);
                if (defectCodeDetail != null)
                {
                    foreach (var item in defectCodeDetail)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.Id = (detailPk.MaxNumber++).ToString();
                            _defectCodeDetailService.InsertGraph(item);
                        }
                        else
                        {
                            _defectCodeDetailService.UpdateGraph(item);
                        }
                    }
                }
                else
                    throw new CustomException("Please select atleast one defect value..........!");
                if (deletedItems != null)
                {
                    _defectCodeDetailService.DeleteGraph(deletedItems);
                }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                DefectCode entity = Find(id);
                _defectCodeDetailService.DeleteGraph(id);
                base.DeleteGraph(entity);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public IEnumerable<object> GetDefectCodeList()
        {
            try
            {
                return from m in base.Query(m => !m.Archive && m.Active).Select().OrderBy(r => r.Code)
                       select new { Text = m.Code, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }
    }
}