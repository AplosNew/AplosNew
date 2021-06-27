#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Enums;
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

namespace Library.Service.Skills
{
    public class CompanyGroupSkillCategoryService : Service<CompanyGroupSkillCategory>, ICompanyGroupSkillCategoryService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public CompanyGroupSkillCategoryService(
            IRepositoryAsync<CompanyGroupSkillCategory> companyGroupSkillCategoryRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(companyGroupSkillCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT SC.[Sequence]
		                                            ,SC.Code
		                                            ,SC.ShortName
		                                            ,SC.StandardName
		                                            ,SC.UserName
		                                            ,SC.[Description]
		                                            ,SC.Remarks
		                                            ,SC.Active
		                                            ,SC.Id
                                            FROM [" + DbSchema.HKP + @"].[CompanyGroupSkillCategory] AS CGSC
                                            INNER JOIN [" + DbSchema.HKP + @"].[SkillCategory] AS SC ON SC.Id=CGSC.SkillCategoryId
                                            WHERE CGSC.CompanyGroupId='" + identity.CompanyGroupId + "'";
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
                return from m in base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.Active).Include(r => r.SkillCategory).Select().OrderBy(r => r.SkillCategory.UserName)
                       select new { Text = m.SkillCategory.UserName, Value = m.SkillCategoryId };
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
            return GetAutoNumber("CompanyGroupSkillCategory", PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public override void InsertGraph(CompanyGroupSkillCategory entity)
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
        }

        public void UpdateGraph(string skillCategoryId, bool active)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CompanyGroupSkillCategory data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.SkillCategoryId == skillCategoryId).Select().FirstOrDefault();
            if (data_Db != null)
            {
                data_Db.Active = active;
                data_Db.ModelState = ModelState.Modified;
                AuditService.Log(data_Db);
                base.UpdateGraph(data_Db);
            }
        }

        public void DeleteGraph(string skillCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            CompanyGroupSkillCategory data_Db = base.Query(r => r.CompanyGroupId == identity.CompanyGroupId && r.SkillCategoryId == skillCategoryId).Select().FirstOrDefault();
            if (data_Db != null)
            {
                base.DeleteGraph(data_Db);
            }
        }
    }
}