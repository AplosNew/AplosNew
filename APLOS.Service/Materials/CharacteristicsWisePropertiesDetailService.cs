#region Using

using Library.Core;
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
    public class CharacteristicsWisePropertiesDetailService : Service<CharacteristicsWisePropertiesDetail>, ICharacteristicsWisePropertiesDetailService
    {
        private string _TableName = DbSchema.Masters + ".[" + DbTable.CharacteristicsWisePropertiesDetail + "]";
        private string _TableNameMaster = DbSchema.Masters + ".[" + DbTable.CharacteristicsWisePropertiesMaster + "]";
        private string _TableNameUOM = DbSchema.SystemConfigurationAndSetup + ".[UnitOfMeasurement]";
        private string _CV = DbSchema.HKP + ".[" + DbTable.CharacteristicsValue + "]";

        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<CharacteristicsWisePropertiesDetail> _charaterValueRepository;

        public CharacteristicsWisePropertiesDetailService(
            IRepositoryAsync<CharacteristicsWisePropertiesDetail> charaterValueRepository,
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
                parameters.CmdText = $"SELECT * FROM {DbSchema.HKP}.[{DbTable.CharacteristicsWisePropertiesUOM}] WHERE Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public string GetPK()
        {
            return "CPD" + _pkGeneratorService.GetAutoNumber(nameof(CharacteristicsWisePropertiesDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<CharacteristicsWisePropertiesDetail> GetList(string MasterId)
        {
            try
            {
                var _sql = "select * from " + _TableName + " where CharacteristicsWisePropertiesMasterId='" + MasterId + "' and archive=0";
                return _charaterValueRepository.SqlQuery<CharacteristicsWisePropertiesDetail>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public CharacteristicsWisePropertiesDetail GetDetail(string PK)
        {
            try
            {
                var _sql = "select * from " + _TableName + " where Id='" + PK + "' and Archive=0";
                return _charaterValueRepository.SelectQuery(_sql, null).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetDetailList(string MasterId)
        {
            try
            {
                var _sql = @"
                                SELECT
	                                 cv1.[Description] CharacteristicsValue1
	                                ,cv2.[Description] CharacteristicsValue2
	                                ,cv3.[Description] CharacteristicsValue3
                                    ,cv1.[Code] CharacteristicsValue1Code
	                                ,cv2.[Code] CharacteristicsValue2Code
	                                ,cv3.[Code] CharacteristicsValue3Code
                                        ,d.Characteristics1ValueId
                                        ,d.Characteristics2ValueId
                                        ,d.Characteristics3ValueId
                                        ,d.Id

                                FROM " + _TableName + @" d
                                LEFT JOIN " + _CV + @" cv1 ON cv1.Id = d.Characteristics1ValueId
                                LEFT JOIN " + _CV + @" cv2 ON cv2.Id = d.Characteristics2ValueId
                                LEFT JOIN " + _CV + @" cv3 ON cv3.Id = d.Characteristics3ValueId
                                WHERE d.CharacteristicsWisePropertiesMasterId = '" + MasterId + "' and d.Archive=0 Order by cv1.[Description],cv2.[Description],cv3.[Description]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetDetailById(string id)
        {
            try
            {
                var _sql = @"
                                SELECT d.Id
	                                ,cv1.[Description] Characteristics1Value
	                                ,cv2.[Description] Characteristics2Value
	                                ,cv3.[Description] Characteristics3Value
                                    ,d.Characteristics1ValueId
                                    ,d.Characteristics2ValueId
                                    ,d.Characteristics3ValueId
                                    , d.CharacteristicsWisePropertiesMasterId

                                FROM " + _TableName + @" d
                                LEFT JOIN " + _CV + @" cv1 ON cv1.Id = d.Characteristics1ValueId
                                LEFT JOIN " + _CV + @" cv2 ON cv2.Id = d.Characteristics2ValueId
                                LEFT JOIN " + _CV + @" cv3 ON cv3.Id = d.Characteristics3ValueId
                                WHERE d.Id = '" + id + "'  and d.Archive=0 Order by cv1.[Description],cv2.[Description],cv3.[Description]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetDetailByMaterialMasterId(string MaterialMasterId)
        {
            try
            {
                var _sql = @"";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetDetailByCharacteristicsValue(string MaterialMasterId, string Characteristics1ValueId, string Characteristics2ValueId, string Characteristics3ValueId)
        {
            try
            {
                var _sql = @"SELECT cwpu.UOMId AS Value, uom.UserName AS Text, uom.StandardName, uom.ShortName, uom.Code
                                FROM " + _TableName + @" cwpu
                                LEFT OUTER JOIN " + _TableNameMaster + @" cwpm ON cwpu.CharacteristicsWisePropertiesMasterId = cwpm.Id
                                LEFT OUTER JOIN " + _TableNameUOM + @" uom ON cwpu.UOMId = uom.Id
                                WHERE cwpm.MaterialMasterId='" + MaterialMasterId + @"'
                                AND ISNULL(cwpm.Characteristics1ValueId,'')=ISNULL('" + Characteristics1ValueId + @"', '')
                                AND ISNULL(cwpm.Characteristics2ValueId,'')=ISNULL('" + Characteristics2ValueId + @"', '')
                                AND ISNULL(cwpm.Characteristics3ValueId,'')=ISNULL('" + Characteristics3ValueId + @"', '')
                                AND cwpu.Archive=0
                                AND cwpm.Archive=0";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}