#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;
using Library.Model.Setups;
using Library.Core;

#endregion Using

namespace Library.Service.Employees
{
    public class RptConfigTemplateService : Service<RptConfigTemplate>, IRptConfigTemplateService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ICompanyGroupSOPCategoryService _companyGroupSOPCategoryService;

        public RptConfigTemplateService(
            IRepositoryAsync<RptConfigTemplate> SOPCategoryRepository,
            IPKGeneratorService pkGeneratorService,
            ICompanyGroupSOPCategoryService companyGroupSOPCategoryService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(SOPCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _companyGroupSOPCategoryService = companyGroupSOPCategoryService;
        }

        #endregion Constructor


        private string GetPK()
        {
            return GetAutoNumber(nameof(RptConfigTemplate), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<ComboModel> GetPlantCbo()
        {
            var sql = @"select Id, StandardName from ORG.Plant ORDER BY StandardName";
            return _sqlRepository.GetCombo(sql, "Id", "StandardName");
        }

        public IEnumerable<ComboModel> GetLanguageCbo()
        {
            var sql = @"select Id, StandardName from SCS.Language ORDER BY StandardName";
            return _sqlRepository.GetCombo(sql, "Id", "StandardName");
        }

        public GridModel GetConfigTemplate(GridParameter parameters)
        {
            try
            {
                //parameters.CmdText = @"Select * From [SCS].[RptConfigTemplate]";
                parameters.CmdText = @"Select RC.*, P.StandardName AS 'PlantName' From [SCS].[RptConfigTemplate] AS RC LEFT JOIN [ORG].Plant AS P ON RC.PlantId=P.Id";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Insert(RptConfigTemplate entity)
        {
            try
            {
                if (entity!=null)
                {
                    entity.Id = GetPK();
                    base.Insert(entity);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public override void Update(RptConfigTemplate entity)
        {
            try
            {
                if (entity.Id != "")
                {
                    base.Update(entity);
                }
                
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

    }
}