#region Using

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
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Setups
{
    public class PrdOrdSettingService : Service<PrdOrdSetting>, IPrdOrdSettingService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<PrdOrdSetting> _baseRepository;

        public PrdOrdSettingService(
            IRepositoryAsync<PrdOrdSetting> baseRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository) : base(baseRepository, unitOfWork, pkGeneratorService)
        {
            _baseRepository = baseRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(PrdOrdSetting), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public IEnumerable<PrdOrdSetting> GetList(string groupId, string companyId, string plantId)
        {
            try
            {
                var data = new List<PrdOrdSetting>();
                if (!string.IsNullOrEmpty(plantId))
                {
                    var sql = @"SELECT A.Id, A.CompanyGroupId, A.CompanyId, A.PlantId, A.ColumnSequence, A.ColumnName, A.MargeAllowed,
                            [dbo].[INSERT_SPACE_BEFORE_CAPITAL_LETTERS](A.ColumnName) AS ColumnAlias, A.AddedBy, A.AddedDate, A.AddedFromIP, A.UpdatedBy, A.UpdatedDate, A.UpdatedFromIP FROM SCS.PrdOrdSetting AS A WHERE A.PlantId='" + plantId + "' ORDER BY A.ColumnSequence";
                    data = _baseRepository.SqlQuery<PrdOrdSetting>(sql).ToList();
                    if (data.Count == 0)
                        data = CreateTable(groupId, companyId, plantId);
                }
                return data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private List<PrdOrdSetting> CreateTable(string groupId, string companyId, string plantId)
        {
            try
            {
                var list = new List<PrdOrdSetting>();
                var seq = 1;
                foreach (var item in Enum.GetValues(typeof(PrdOrdSettingEnum)))
                {
                    var model = new PrdOrdSetting
                    {
                        Id = null,
                        CompanyGroupId = groupId,
                        CompanyId = companyId,
                        PlantId = plantId,
                        ColumnSequence = seq,
                        ColumnName = item.ToString(),
                        ColumnAlias = EnumService.GetDescription(item),
                        MargeAllowed = false,
                        AddedBy = null,
                        AddedDate = DateTime.Now,
                        AddedFromIP = null,
                        UpdatedBy = null,
                        UpdatedDate = null,
                        UpdatedFromIP = null
                    };
                    list.Add(model);
                    seq++;
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        #region Operation

        public void InsertOrUpdateGraph(IEnumerable<PrdOrdSetting> entities)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = GetPK();
                        
                        InsertGraph(item);
                    }
                    else
                        UpdateGraph(item);
                }
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #endregion Operation
    }
}