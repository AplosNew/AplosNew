#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Materials
{
    public class MaterialGridCharacteristicsService : Service<MaterialGridCharacteristics>, IMaterialGridCharacteristicsService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<MaterialGridCharacteristics> _materialGridCharacteristicsRepository;

        public MaterialGridCharacteristicsService(
            IRepositoryAsync<MaterialGridCharacteristics> materialGridCharacteristicsRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(materialGridCharacteristicsRepository, unitOfWork)
        {
            _materialGridCharacteristicsRepository = materialGridCharacteristicsRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> Query(string materialGridId)
        {
            try
            {
                var _sql = @"SELECT MGC.Id, MGC.MaterialGridId, MGC.CharacteristicsId,C.UserName, MGC.Sort, C.IsFreeField, C.IsPreDefinedField, C.IsMandatory, MGC.Active, MGC.Archive
                            FROM HKP.[MaterialGridCharacteristics] AS MGC
                            INNER JOIN HKP.[Characteristics]  AS C ON C.Id= MGC.CharacteristicsId
                            WHERE mgc.MaterialGridId='" + materialGridId + "' AND mgc.Archive=0 ORDER BY mgc.Sort ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public void Insert(IEnumerable<MaterialGridCharacteristics> entity, string materialGridId, string[] deletedItems)
        {
            try
            {
                var pkIncrease = false;
                var pkdetail = _pkGeneratorService.GetMaxNumber(nameof(MaterialGridCharacteristics), PKGeneratorEnum.Auto, null, DateTime.Now);
                var Count = 0;
                if (entity != null)
                {
                    var entityDb_list = base.Query(r => r.MaterialGridId == materialGridId && !r.Archive).Select().AsEnumerable();
                    if (entityDb_list.Count() <= 3 && entity.Count() <= 3)
                    {
                        foreach (var item in entity)
                        {
                            //if (entityDb_list.Any(r => r.Id != item.Id && r.Sort == item.Sort))
                            //    throw new CustomException(string.Format(ResourcesCore.DuplicateSelection, "sort (" + item.Sort + ")"));
                            var data = entityDb_list.FirstOrDefault(r => r.CharacteristicsId == item.CharacteristicsId);
                            if (string.IsNullOrEmpty(item.Id) && data == null)
                            {
                                Count++;
                                item.Id = "MGC" + pkdetail.MaxNumber + "-" + Count;
                                item.MaterialGridId = materialGridId;
                                item.Active = true;
                                pkIncrease = true;
                                InsertGraph(item);
                            }
                            else
                            {
                                data.MaterialGridId = materialGridId;
                                data.Sort = item.Sort;
                                data.Active = true;
                                UpdateGraph(data);
                            }
                        }
                    }
                }

                if (deletedItems != null)
                {
                    CheckIdUse(materialGridId);
                    var ids = base.Query(r => deletedItems.Contains(r.Id)).Select();
                    foreach (var item in ids)
                    {
                        ArchiveGraph(item.Id);
                    }
                }
                if (pkIncrease)
                {
                    pkdetail.MaxNumber++;
                    //_pkGeneratorService.InsertOrUpdateGraph(pkdetail);
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private string GetPK()
        {
            return "MGC" + _pkGeneratorService.GetAutoNumber(nameof(MaterialGridCharacteristics), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(MaterialGridCharacteristics entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteGraph(string materialGridId)
        {
            var data = base.Query(r => r.MaterialGridId == materialGridId && !r.Archive).Select().AsEnumerable();
            if (data != null)
            {
                foreach (var item in data)
                {
                    base.DeleteGraph(item);
                }
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in base.Query(m => !m.Archive).Select()
                       select new { Text = m.CharacteristicsId, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private void CheckIdUse(string id)
        {
            string sql = $"IF EXISTS(SELECT 1 FROM( " +
                            $"SELECT MaterialGridId AS CheckingColumn FROM {DbSchema.Masters}.[{DbTable.MaterialMaster}] WHERE Archive=0 " +
                            $") A WHERE CheckingColumn = '{id}') SELECT 1 ELSE SELECT 0 RETURN ";
            var data = Convert.ToBoolean(_materialGridCharacteristicsRepository.SqlQuery<int>(sql).Single());
            if (data)
                throw new CustomException("Since material master exist, you can't delete!");
        }

        public IEnumerable<object> GetByMatrialGridList(string id)
        {
            try
            {
                string _sql = @"SELECT MGCH.CharacteristicsId
	                                  ,CH.UserName AS MaterialGridName
                                      ,MGCH.Sort
                                FROM HKP.MaterialGridCharacteristics AS MGCH
                                LEFT OUTER JOIN HKP.Characteristics AS CH ON MGCH.CharacteristicsId=CH.Id
                                WHERE MGCH.MaterialGridId='" + id + "' AND MGCH.Archive=0 ORDER BY MGCH.Sort";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }
    }
}