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
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.Setups
{
    public class PlantWiseLetterTemplateService : Service<PlantWiseLetterTemplate>, IPlantWiseLetterTemplateService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public PlantWiseLetterTemplateService(
            IRepositoryAsync<PlantWiseLetterTemplate> PlantWiseLetterTemplateRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(PlantWiseLetterTemplateRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(PlantWiseLetterTemplate), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(PlantWiseLetterTemplate entity)
        {
            try
            {
                entity.Id = "PCL-" + GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(PlantWiseLetterTemplate entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string plantId,string letterType)
        {
            try
            {
                parameters.CmdText = @"SELECT PTC.*,P.UserName Plant FROM [SCS].[PlantWiseLetterTemplate] PTC
										LEFT JOIN ORG.Plant AS P ON PTC.PlantId=P.Id Where PTC.PlantId='" + plantId + "'and PTC.LetterType='" + letterType + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetTermsAndConditionsByPreRecruitmentEmployee(string preRecruitmentEmployeeId)
        {
            try
            {
                var sql = @"SELECT * FROM [TRN].[EmployeeWiseTermsAndConditions] WHERE PreRecruitmentEmployeeId ='" + preRecruitmentEmployeeId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}