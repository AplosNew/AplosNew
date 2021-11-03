using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Setups;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.Service.Setups
{
    public class PrerecruitmentUrlService : Service<PrerecruitmentUrl>, IPrerecruitmentUrlService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PrerecruitmentUrlService(
            IRepositoryAsync<PrerecruitmentUrl> prerecruitmentUrlRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(prerecruitmentUrlRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private PKGenerator GetPK()
        {
            return GetMaxNumber(nameof(PrerecruitmentUrl), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public List<Dictionary<string, object>> Query(string companyGroupId, string companyId)
        {
            try
            {
                var sql = @"SELECT ME.Id, ME.Active, ME.[Url], P.CompanyGroupId, P.CompanyId, P.Id AS PlantId,  P.Code As PlantCode, P.UserName As PlantName FROM ORG.Plant AS P
                            OUTER APPLY(SELECT * FROM MMS.PrerecruitmentUrl AS V WHERE P.Id=V.PlantId) AS ME
                            WHERE P.CompanyGroupId='" + companyGroupId + "' AND P.CompanyId='" + companyId + "' ORDER BY P.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Module.ToString()));
            }
        }

        public void Save(IEnumerable<PrerecruitmentUrl> entities)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetPK();
                foreach (PrerecruitmentUrl prerecruitmentUrl in entities)
                {
                    var me = Find(prerecruitmentUrl.Id);
                    if (me != null)
                    {
                        me.Url = prerecruitmentUrl.Url;
                        me.ModelState = ModelState.Modified;
                        AuditService.Log(me);
                    }
                    else if (prerecruitmentUrl.Active)
                    {
                        pk.MaxNumber++;
                        prerecruitmentUrl.Id = pk.MaxNumber.ToString();
                        InsertGraph(prerecruitmentUrl);
                    }
                }
                var companyGroupId = entities.First().CompanyGroupId;
                var companyId = entities.First().CompanyId;
                var dbList = base.Query(t => t.CompanyGroupId == companyGroupId && t.CompanyId == companyId).Select().AsEnumerable();
                if (dbList != null && dbList.Count() > 0)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbList)
                        {
                            Delete(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.CompanyGroupId == item.CompanyGroupId && t.CompanyId == item.CompanyId && t.Active == item.Active))
                                Delete(item);
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                _unitOfWork.Commit();
                flag = false;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Module.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }
}