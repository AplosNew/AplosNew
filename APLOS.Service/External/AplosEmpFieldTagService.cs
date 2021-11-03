#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.External;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion Using

namespace Library.Service.External
{
    public class AplosEmpFieldTagService : Service<AplosEmpFieldTag>, IAplosEmpFieldTagService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public AplosEmpFieldTagService(
            IRepositoryAsync<AplosEmpFieldTag> aplosEmpFieldTagRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
             ) : base(aplosEmpFieldTagRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(AplosEmpFieldTag), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void Insert(IEnumerable<AplosEmpFieldTag> entity)
        {
            try
            {
                foreach (var item in entity)
                {
                    if (item.Id != 0)
                    {
                        base.Update(item);
                    }
                    else
                    {
                        base.Insert(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(AplosEmpFieldTag entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, int companyGroupId)
        {
            try
            {
                parameters.sort = "Id";
                parameters.order = "asc";
                parameters.CmdText = @"SELECT * FROM dbo.AplosEmpFieldTag where CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetCompanyGroupCbo()
        {
            try
            {
                var sql = @"Select CG.Id AS [Value], CG.Name AS [Text] From  [dbo].[CompanyGroup]AS CG ";

                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }
    }
}