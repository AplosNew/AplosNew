using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.HumanResources;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Library.Service.HumanResources
{
    public class CompliedShiftGroupingService : Service<CompliedShiftGrouping>, ICompliedShiftGroupingService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<CompliedShiftGroupDetail> _compliedShiftGroupDetailRepository;
        private readonly ICompliedShiftGroupDetailService _compliedShiftGroupDetailService;

        public CompliedShiftGroupingService(
            IRepositoryAsync<CompliedShiftGrouping> compliedShiftRepository,
            IRepositoryAsync<CompliedShiftGroupDetail> compliedShiftGroupDetailRepository
            , ICompliedShiftGroupDetailService compliedShiftGroupDetailService
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) : base(compliedShiftRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _compliedShiftGroupDetailService = compliedShiftGroupDetailService;
            _compliedShiftGroupDetailRepository = compliedShiftGroupDetailRepository;
        }

        #endregion ConstructorCompliedShiftGrouping


        private void Check(CompliedShiftGrouping entity)
        {
            var code = base.Query(r => r.Id != entity.Id && r.Code == entity.Code && r.PlantId == entity.PlantId).Select().FirstOrDefault();
            if (code != null)
            {
                throw new CustomException("Code already exist");
            }

            //CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.PlantId == entity.PlantId);
            //CheckUniqueColumn(UniqueColumnName.UserName, entity.Description, r => r.Id != entity.Id && r.Description == entity.Description && r.PlantId == entity.PlantId);
        }


        public void InsertOrUpdateGraph(CompliedShiftGrouping entity, IEnumerable<CompliedShiftGroupDetail> details)
        {
            var flag = false;
            try
            {
                //if (details == null)
                //    throw new CustomException("Please select shift");
                _unitOfWork.BeginTransaction();
                flag = true;
                Check(entity);
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPK();
                    entity.ModelState = ModelState.Added;
                    AuditService.Log(entity);
                }
                else
                {
                    entity.ModelState = ModelState.Modified;
                    AuditService.Log(entity);
                }
                if (details != null)
                {
                    _compliedShiftGroupDetailService.InsertOrUpdate(details, entity.Id); 
                }
                InsertOrUpdateGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
        private string GetPK()
        {
            return GetAutoNumber(nameof(CompliedShiftGrouping), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public override void Update(CompliedShiftGrouping entity)
        {
            try
            {
                Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel QueryshiftDefination(GridParameter parameters, string groupId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT SystemID, ShiftDefinationName, ShiftType, UserName, ShiftDefinationDescription
										, CONVERT(VARCHAR(8), CONVERT(TIME, InTime)) AS InTime
										, CONVERT(VARCHAR(8), CONVERT(TIME, OutTime)) AS OutTime
							FROM ShiftDefination WHERE GroupID='" + groupId + "' AND PlantID='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> QueryDetail(string compliedShiftGroupId)
		{
            try
            {
               var sql = @"SELECT M.Id, M.CompliedShiftGroupingId, M.ActualShiftId
										, SD.ShiftDefinationName, SD.ShiftType, SD.UserName, SD.ShiftDefinationDescription
										, CONVERT(VARCHAR(8), CONVERT(TIME, InTime)) AS InTime
										, CONVERT(VARCHAR(8), CONVERT(TIME, OutTime)) AS OutTime
									FROM MST.CompliedShiftGroupDetail M
									LEFT JOIN ShiftDefination SD ON M.ActualShiftId=SD.SystemID
                                    WHERE CompliedShiftGroupingId='" + compliedShiftGroupId + "' ORDER BY CONVERT(TIME, InTime)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public GridModel Query(GridParameter parameters, string companyGroupId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM MST.CompliedShiftGrouping WHERE PlantId='" + plantId + "' AND CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCbo(string plantId)
        {
            try
            {
                return from m in base.Query(r => r.PlantId == plantId).Select().OrderBy(r => r.Description)
                       select new { Text = m.Description, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                var dbList = _compliedShiftGroupDetailRepository.Query(t => t.CompliedShiftGroupingId == id).Select().AsEnumerable();
                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var item in dbList)
                {
                    _compliedShiftGroupDetailRepository.Delete(item);
                }
                base.Delete(id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public void DeleteDetail(string id)
        {
            var flag = false;
            try
            {
                var dbList = _compliedShiftGroupDetailRepository.Query(t => t.Id == id).Select().FirstOrDefault();
                if (dbList != null)
                {
                    _compliedShiftGroupDetailRepository.Delete(dbList.Id);
					_unitOfWork.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

    }
}