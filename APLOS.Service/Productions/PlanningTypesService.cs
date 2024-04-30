#region Using
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Productions;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Productions
{
    /// <summary>
    ///  Class ProductService.
    /// </summary>
    public partial class PlanningTypesService : Service<PlanningTypes>, IPlanningTypesService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public PlanningTypesService(
            IRepositoryAsync<PlanningTypes> PlanningTypesRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(PlanningTypesRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void DeleteGraph(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Find(Id);
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
            return GetAutoNumber(nameof(PlanningTypes), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private bool CheckUnique(string companyGroupId, string planningType)
        {
            //CheckUniqueColumn(UniqueColumnName.Code, entity.PlanningType, r => r.PlanningType == entity.PlanningType &&  r.CompanyGroupId == entity.CompanyGroupId && r.Id != entity.Id);
            try
            {
                var _sql = @"SELECT * FROM dbo.PlanningTypes Where CompanyGroupId='" + companyGroupId + @"' AND PlanningType='" + planningType + "'";
                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool CheckUnique(string companyGroupId, string planningType, string id)
        {
            try
            {
                var _sql = @"SELECT * FROM dbo.PlanningTypes Where CompanyGroupId='" + companyGroupId + @"' AND PlanningType='" + planningType + @"' AND Id <>'"+ id + "'";
                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CheckUnique(PlanningTypes entity)
        {
            try
            {
                var _sql = @"SELECT * FROM dbo.PlanningTypes Where CompanyGroupId='" + entity.CompanyGroupId + @"' AND PlanningType='" + entity.PlanningType + @"' AND BaseProcessId='" + entity.BaseProcessId + @"' AND PlantId='" + entity.PlantId + @"' AND Id <>'" + entity.Id + "'";
                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    throw new CustomException("Planning Types " + entity.PlanningType + " is already exists.");
                }

                var sql = @"SELECT * FROM dbo.PlanningTypes Where CompanyGroupId='" + entity.CompanyGroupId + @"' AND UserName='" + entity.UserName + @"' AND BaseProcessId='" + entity.BaseProcessId + @"' AND PlantId='" + entity.PlantId + @"' AND Id <>'" + entity.Id + "'";
                var dbuserName = _sqlRepository.GetDataCollection(sql, null);

                if (dbuserName.Count > 0)
                {
                    throw new CustomException("User Name " + entity.UserName + " is already exists.");
                }
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override void Insert(PlanningTypes entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                
                CheckUnique(entity);
                entity.CompanyGroupId = identity.CompanyGroupId;
                entity.Id = "PT" + GetPK();

                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {

            }
        }

        public override void Update(PlanningTypes entity)
        {
            try
            {
                
                CheckUnique(entity);

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
                parameters.CmdText = @"SELECT PT.*,P.UserName BaseProcess,PN.UserName Plant,C.UserName Company,C.Id CompanyId,SP.UserName SubProcess,EN.UserName Entity FROM dbo.PlanningTypes PT
                                    LEFT JOIN HKP.Process P ON P.Id=PT.BaseProcessId
                                    LEFT JOIN HKP.SubProcess SP ON SP.Id=PT.SubProcessId
									LEFT JOIN [ORG].[Plant] PN ON PN.Id=PT.PlantId
                                    LEFT JOIN [ORG].[Company] C ON C.Id=PN.CompanyId
                                    LEFT JOIN [ORG].[Entity] EN ON EN.Id=PT.EntityId
                                    WHERE PT.CompanyGroupId='" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel GetShiftList(GridParameter parameters, string sGroupID, string sPlantID, string[] ShiftDefinationIDs, string wcids)
        {
            try
            {
                parameters.sort = "ShiftDefinationName";
                parameters.CmdText = @"SELECT 0 Flag,SystemID ShiftDefinationID, ShiftDefinationName, ShiftDefinationDescription, ShiftType, SequenceNo ShiftSequence, CONVERT(VARCHAR(10), InTime, 108) AS InTime,
                                        InTimeStartMargin, LateMargin, AbsentEndMargin, CONVERT(VARCHAR(10), OutTime, 108) AS OutTime,
                                        OutTimeEndMargin, OTStartTime, CONVERT(VARCHAR(10), BreakStratTime, 108) AS BreakStratTime,
                                        CONVERT(VARCHAR(10), BreakEndTime, 108) AS BreakEndTime, BreakPeriod, WorkingHour, IsActive, DefaultShift, IsGapInclude
                                FROM ShiftDefination WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' AND SystemID NOT IN (" + ReturnStringArray(ShiftDefinationIDs) + ") AND SystemID IN(SELECT  ShiftDefinationId FROM WorkCenterWiseShift WHERE WorkCenterMasterId IN("+ wcids + "))";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}