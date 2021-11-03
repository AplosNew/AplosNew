#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion Using

namespace Library.Service.Materials
{
    public class MaterialMasterCharacteristicsValueService : Service<MaterialMasterCharacteristicsValue>, IMaterialMasterCharacteristicsValueService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public MaterialMasterCharacteristicsValueService(
            IRepositoryAsync<MaterialMasterCharacteristicsValue> materialMasterCharacteristicsValueRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(materialMasterCharacteristicsValueRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00m;
            }
        }

        public IEnumerable<object> Query(string masterId)
        {
            try
            {
                string _sql = @"SELECT A.Id, A.MaterialMasterId, A.CharacteristicsId MaterialMasterCharacteristicsId,  A.Sequence, A.Code, A.ShortName, A.StandardName, A.UserName, A.Remarks, A.Description, A.IsDefault, A.Active, A.Archive
                                FROM [HKP].[CharacteristicsValue] AS A
								LEFT JOIN MST.MaterialMaster M ON A.MaterialMasterId=A.MaterialMasterId
                                WHERE  M.Id='"+masterId+"' ORDER BY A.Sequence";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertOrUpdateGraph(MaterialMasterCharacteristics characteristics, IEnumerable<MaterialMasterCharacteristicsValue> entities, IEnumerable<MaterialMasterCharacteristicsValue> dbList)
        {
            if (entities != null)
            {
                Check(entities);
                foreach (var item in entities)
                {
                    CheckPropertiesAndCharLength(item, characteristics);
                    if (item.Id == 0)
                    {
                        item.MaterialMasterId = characteristics.MaterialMasterId;
                        item.MaterialMasterCharacteristicsId = characteristics.Id;
                        InsertGraph(item);
                    }
                    else
                        UpdateGraph(item);
                }
            }
            if (dbList != null)
            {
                var deleteList = dbList.Where(t => t.MaterialMasterCharacteristicsId == characteristics.Id).ToList();
                foreach (var item in deleteList)
                {
                    if (!entities.Any(t => t.Id == item.Id))
                        base.DeleteGraph(item);
                }
            }
        }

        public void DeleteGraph(IEnumerable<MaterialMasterCharacteristicsValue> characteristicsValueList)
        {
            if (characteristicsValueList != null)
            {
                foreach (var item in characteristicsValueList)
                {
                    base.DeleteGraph(item);
                }
            }
        }

        private void Check(IEnumerable<MaterialMasterCharacteristicsValue> entities)
        {
            // Duplicate Budget activity checking.
            var duplicateCode = entities.GroupBy(x => new { x.Code }).Where(x => x.Skip(1).Any());
            if (duplicateCode.Any())
                throw new CustomException(string.Format(ResourcesCore.DuplicateSelection, "Code (" + duplicateCode.FirstOrDefault().Key + ")"));
            var duplicateUserName = entities.GroupBy(x => new { x.Code }).Where(x => x.Skip(1).Any());
            if (duplicateUserName.Any())
                throw new CustomException(string.Format(ResourcesCore.DuplicateSelection, "UserName (" + duplicateUserName.FirstOrDefault().Key + ")"));
            var duplicateIsDefault = entities.Where(t => t.IsDefault && t.Active);
            if (duplicateIsDefault != null && duplicateIsDefault.Count() > 1)
                throw new CustomException("Default value already set.");
        }

        private static void CheckPropertiesAndCharLength(MaterialMasterCharacteristicsValue entity, MaterialMasterCharacteristics characteristics)
        {
            if (characteristics != null)
            {
                if (characteristics.AttributeProperty == AttributePropertiesEnum.Integer.ToString())
                {
                    if (!int.TryParse(entity.UserName, out int userName))
                        throw new CustomException("User name is not integer");
                }
                else if (characteristics.AttributeProperty == AttributePropertiesEnum.Decimal.ToString())
                {
                    if (!decimal.TryParse(entity.UserName, out decimal userName))
                        throw new CustomException("User name is not decimal");
                }
                else
                {
                    if (characteristics.IsFixedNoOfCharacter &&
                       (characteristics.NoOfCharacter < entity.UserName.Count() || characteristics.NoOfCharacter > entity.UserName.Count()))
                        throw new Exception("User name must be [" + characteristics.NoOfCharacter + "] character");
                }
            }
        }
    }
}