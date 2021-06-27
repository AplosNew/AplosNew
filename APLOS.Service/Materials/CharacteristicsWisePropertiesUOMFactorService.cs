#region Using

using Library.Core;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion Using

namespace Library.Service.Materials
{
    public class CharacteristicsWisePropertiesUOMFactorService : Service<CharacteristicsWisePropertiesUOMFactor>, ICharacteristicsWisePropertiesUOMFactorService
    {
        private string _TableName = DbSchema.Masters + ".[" + DbTable.CharacteristicsWisePropertiesUOMFactor + "]";

        #region Constructor

        private readonly IRepositoryAsync<CharacteristicsWisePropertiesUOMFactor> _charaterValueRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public CharacteristicsWisePropertiesUOMFactorService(
            IRepositoryAsync<CharacteristicsWisePropertiesUOMFactor> charaterValueRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(charaterValueRepository, unitOfWork)
        {
            _charaterValueRepository = charaterValueRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel GetSearchData(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM {DbSchema.Masters}.[{DbTable.CharacteristicsWisePropertiesUOMFactor}] WHERE Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetPK()
        {
            try
            {
                return "CPF" + _pkGeneratorService.GetAutoNumber(nameof(CharacteristicsWisePropertiesUOMFactor), PKGeneratorEnum.Auto, null, DateTime.Now);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<CharacteristicsWisePropertiesUOMFactor> GetList(string MasterId)
        {
            try
            {
                string _sql = "select * from " + _TableName + " where CharacteristicsWisePropertiesMasterId='" + MasterId + "' and archive=0";
                return _charaterValueRepository.SqlQuery<CharacteristicsWisePropertiesUOMFactor>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<CharacteristicsWisePropertiesUOMFactor> GetListByDetailId(string DetailId)
        {
            try
            {
                string _sql = "select * from " + _TableName + " where CharacteristicsWisePropertiesDetailId='" + DetailId + "' and archive=0";
                return _charaterValueRepository.SqlQuery<CharacteristicsWisePropertiesUOMFactor>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}