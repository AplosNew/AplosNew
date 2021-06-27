#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Machines;
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

namespace Library.Service.Machines
{
    public class CompanyGroupOperationCategoryService : Service<CompanyGroupOperationCategory>, ICompanyGroupOperationCategoryService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public CompanyGroupOperationCategoryService(
            IRepositoryAsync<CompanyGroupOperationCategory> companyGroupOperationCategoryRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(companyGroupOperationCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT  OC.Id
		                                        ,OC.[Sequence]
		                                        ,OC.Code
		                                        ,OC.ShortName
		                                        ,OC.StandardName
		                                        ,OC.UserName
		                                        ,OC.[Description]
		                                        ,OC.Remarks
		                                        ,OC.Active
                                        FROM [" + DbSchema.HKP + @"].[" + DbTable.CompanyGroupOperationCategory + @"] AS CGOC
                                        LEFT OUTER JOIN [" + DbSchema.HKP + @"].[" + DbTable.OperationCategory + @"] AS OC ON CGOC.OperationCategoryId=OC.Id
                                        WHERE CGOC.CompanyGroupId='" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return from m in base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.Active).Include(r => r.OperationCategory).Select().OrderBy(r => r.OperationCategory.UserName)
                       select new { Text = m.OperationCategory.UserName, Value = m.OperationCategoryId };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(CompanyGroupOperationCategory), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public override void InsertGraph(CompanyGroupOperationCategory entity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.Id = GetPK();
                entity.CompanyGroupId = identity.CompanyGroupId;
                base.InsertGraph(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public void UpdateGraph(string operationCategoryId, bool active)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.OperationCategoryId == operationCategoryId).Select().FirstOrDefault();
            if (data_Db != null)
            {
                data_Db.Active = active;
                data_Db.ModelState = ModelState.Modified;
                AuditService.Log(data_Db);
                base.UpdateGraph(data_Db);
            }
        }

        public void DeleteGraph(string operationCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.OperationCategoryId == operationCategoryId).Select().FirstOrDefault();
            if (data_Db != null)
            {
                base.DeleteGraph(data_Db);
            }
        }
    }
}