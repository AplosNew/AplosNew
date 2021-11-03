#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Setups;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.Setups
{
    public class TnaSettingMasterService : Service<TnaSettingMaster>, ITnaSettingMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ITnaSettingDetailService _tnaSettingDetailService;
        public TnaSettingMasterService(
            IRepositoryAsync<TnaSettingMaster> skillCategoryRepository,
            ITnaSettingDetailService tnaSettingDetailService,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(skillCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _tnaSettingDetailService = tnaSettingDetailService;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber("TnaSettingMaster", PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public void InsertOrUpdate(TnaSettingMaster entity,IEnumerable<TnaSettingDetail> entities  )
        {
            var flag = false;
            try
            { if(entities== null)
                _unitOfWork.BeginTransaction();
                flag = true;
                if (string.IsNullOrEmpty(entity.Id))
                    
                {
                    entity.Id = GetPK();
                    InsertGraph(entity);
                }
                else
                {
                    UpdateGraph(entity);
                }
                _tnaSettingDetailService.InsertUpdate(entities,entity.Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }


        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "TnaSettingMaster Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                TnaSettingMaster entity = Find(id);
                // If section row inactive
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public GridModel Query(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT EM.EmployeeCode, EM.EmployeeName,DG.UserName as Given_Designatio , DP.UserName as Department ,
 DV.UserName AS Division,SE.UserName AS Section FROM TnaSettingMaster TS

LEFT JOIN EmployeeInformation EM ON TS.EmployeeInformationId=EM.SystemId 
LEFT JOIN HKP.Designation DG ON EM.GivenDesignationId=DG.Id
LEFT JOIN ORG.Department DP ON EM.DepartmentId=DP.Id
LEFT JOIN ORG.Division DV ON EM.DivisionId=DV.Id
LEFT JOIN ORG.Section SE ON EM.SectionId=SE.Id



 WHERE TS.PlantId='"+plantId+"'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
    }
}