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
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Setups
{
    public class EntityConfigService : Service<EntityConfig>, IEntityConfigService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public EntityConfigService(
            IRepositoryAsync<EntityConfig> EntityConfigRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(EntityConfigRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetEntityConfigParameterList()
        {
            return Enum.GetValues(typeof(EntityConfigParameter)).Cast<EntityConfigParameter>().Select(v => new
            {
                Id = "",
                Text = v.ToString(),
                Applicable = false
            });
        }
        public void InsertOrUpdateGraph(IEnumerable<EntityConfig> entities,string entityId)
        {
            var flag = false;
            string sID = null;
            try
            {
                //if (entities == null)
                //    throw new CustomException("Data can not null.");
                _unitOfWork.BeginTransaction();
                flag = true;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                var dbList = Query(t => t.EntityId == entityId).Select().AsEnumerable();
                //var pk = GetMaxNumber(nameof(EntityConfig), PKGeneratorEnum.Yearly, null, DateTime.Now);

               

                if (entities != null)
                {
                    foreach (var item in entities)
                    {
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(EntityConfig), out sID);

                        if (string.IsNullOrEmpty(item.Id))
                        {
                            //pk.MaxNumber++;
                            item.Id = sID;
                            InsertGraph(item);
                        }
                        else
                        {
                            UpdateGraph(item);
                        }
                    } 
                }
                if (dbList != null)
                {
                    var deleteList = dbList.Where(t => t.EntityId == entityId).ToList();
                    if (entities != null)
                    {
                        foreach (var item in deleteList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                                Delete(item);
                        }
                    }
                    else
                    {
                        ExecuteSqlCommand("DELETE FROM dbo.EntityConfig Where EntityId='"+ entityId + "'");
                    }
                }
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
      

        public void DeleteGraph(string Id)
        {
           
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "EntityConfig Id"));

                EntityConfig entity = Find(Id);
                // If section row inactive
                base.Delete(entity);
                
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
        }
       
        public IEnumerable<object> Query(string entityId)
        {
            try
            {
                string CmdText = @"SELECT EC.*, Applicable=CASE WHEN EC.Id IS NULL THEN 0 ELSE 1 END FROM dbo.EntityConfig EC Where EntityId='" + entityId + "'";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in base.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public List<ComboModel> GetCboProduction(string companyGroupId)
        {
            try
            {
                if (string.IsNullOrEmpty(companyGroupId))
                    throw new CustomException(ResourcesCore.InvalidCompanyGroup);
                var sql = @"SELECT DISTINCT E.Id, E.UserName FROM [dbo].[EntityConfig] EC
                          LEFT JOIN [ORG].[Entity] AS E ON EC.EntityId=E.Id
                          WHERE E.CompanyGroupId='" + companyGroupId + @"' AND EC.IsProductionEntity=1 
                          AND E.Active=1 AND E.Archive=0";
                return _sqlRepository.GetCombo(sql, "Id", "UserName");
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
        }

    }
}