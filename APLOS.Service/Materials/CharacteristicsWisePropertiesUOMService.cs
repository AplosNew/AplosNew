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
    public class CharacteristicsWisePropertiesUOMService : Service<CharacteristicsWisePropertiesUOM>, ICharacteristicsWisePropertiesUOMService
    {
        private string _TableName = DbSchema.Masters + ".[" + DbTable.CharacteristicsWisePropertiesUOM + "]";
        private string _TableNameMaster = DbSchema.Masters + ".[" + DbTable.CharacteristicsWisePropertiesMaster + "]";
        private string _TableNameDetail = DbSchema.Masters + ".[" + DbTable.CharacteristicsWisePropertiesDetail + "]";
        private string _TableNameUOM = DbSchema.SystemConfigurationAndSetup + ".[UnitOfMeasurement]";

        #region Constructor

        private readonly IRepositoryAsync<CharacteristicsWisePropertiesUOM> _charaterValueRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public CharacteristicsWisePropertiesUOMService(
            IRepositoryAsync<CharacteristicsWisePropertiesUOM> charaterValueRepository,
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
            return "CPU" + _pkGeneratorService.GetAutoNumber(nameof(CharacteristicsWisePropertiesUOM), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<CharacteristicsWisePropertiesUOM> GetList(string MasterId)
        {
            try
            {
                var _sql = "select * from " + _TableName + " where CharacteristicsWisePropertiesMasterId='" + MasterId + "' and archive=0";
                return _charaterValueRepository.SqlQuery<CharacteristicsWisePropertiesUOM>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<CharacteristicsWisePropertiesUOM> GetListByDetailId(string detailid)
        {
            try
            {
                var _sql = "select * from " + _TableName + " where CharacteristicsWisePropertiesdetailid='" + detailid + "' and archive=0";
                return _charaterValueRepository.SqlQuery<CharacteristicsWisePropertiesUOM>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetCharValueUOMByMaterialMasterId(string MaterialMasterId)
        {
            try
            {
                var _sql = @"SELECT mm.BaseUOMId AS [Value], uom.UserName AS [Text], uom.StandardName, uom.UserName, uom.Code
                                FROM " + DbSchema.Masters + ".[" + DbTable.MaterialMaster + @"] mm
                                LEFT OUTER JOIN " + DbSchema.SystemConfigurationAndSetup + @".[UnitOfMeasurement] uom ON mm.BaseUOMId = uom.Id
                                WHERE mm.Id='" + MaterialMasterId + @"'
                                AND mm.Archive=0

                                UNION ALL

                                SELECT cwpu.UOMId AS [Value], uom.UserName AS [Text], uom.StandardName, uom.UserName, uom.Code
                                FROM  " + _TableNameDetail + @" cwpd
								LEFT OUTER JOIN " + _TableName + @" cwpu ON cwpd.Id=cwpu.CharacteristicsWisePropertiesDetailId
                                LEFT OUTER JOIN " + _TableNameMaster + @" cwpm ON cwpd.CharacteristicsWisePropertiesMasterId = cwpm.Id
                                LEFT OUTER JOIN " + _TableNameUOM + @" uom ON cwpu.UOMId = uom.Id
                                WHERE cwpm.MaterialMasterId='" + MaterialMasterId + @"'
                                AND cwpd.Archive=0
                                AND cwpu.Archive=0
                                AND cwpm.Archive=0";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetUOMByCharacteristicsValue(string MaterialMasterId, string Characteristics1ValueId, string Characteristics2ValueId, string Characteristics3ValueId)
        {
            try
            {
                var _sql = @"SELECT mm.BaseUOMId AS [Value], uom.UserName AS [Text], uom.StandardName, uom.UserName, uom.Code
                                FROM " + DbSchema.Masters + ".[" + DbTable.MaterialMaster + @"] mm
                                LEFT OUTER JOIN " + DbSchema.SystemConfigurationAndSetup + @".[UnitOfMeasurement] uom ON mm.BaseUOMId = uom.Id
                                WHERE mm.Id='" + MaterialMasterId + @"'
                                AND mm.Archive=0

                                UNION ALL

                                SELECT cwpu.UOMId AS [Value], uom.UserName AS [Text], uom.StandardName, uom.UserName, uom.Code
                                FROM  " + _TableNameDetail + @" cwpd
								LEFT OUTER JOIN " + _TableName + @" cwpu ON cwpd.Id=cwpu.CharacteristicsWisePropertiesDetailId
                                LEFT OUTER JOIN " + _TableNameMaster + @" cwpm ON cwpd.CharacteristicsWisePropertiesMasterId = cwpm.Id
                                LEFT OUTER JOIN " + _TableNameUOM + @" uom ON cwpu.UOMId = uom.Id
                                WHERE cwpm.MaterialMasterId='" + MaterialMasterId + @"'
                                AND ISNULL(cwpd.Characteristics1ValueId,'')= (CASE
                                                                                WHEN ISNULL(cwpd.Characteristics1ValueId,'') = ''
                                                                                    THEN ''
                                                                                ELSE ISNULL('" + Characteristics1ValueId + @"', '')
                                                                              END)
                                AND ISNULL(cwpd.Characteristics2ValueId,'')= (CASE
                                                                                WHEN ISNULL(cwpd.Characteristics2ValueId,'') = ''
                                                                                    THEN ''
                                                                                ELSE ISNULL('" + Characteristics2ValueId + @"', '')
                                                                              END)
                                AND ISNULL(cwpd.Characteristics3ValueId,'')= (CASE
                                                                                WHEN ISNULL(cwpd.Characteristics3ValueId,'') = ''
                                                                                    THEN ''
                                                                                ELSE ISNULL('" + Characteristics3ValueId + @"', '')
                                                                              END)
                                AND cwpd.Archive=0
                                AND cwpu.Archive=0
                                AND cwpm.Archive=0";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetUOMByCharacteristicsValue1st(string MaterialMasterId, string Characteristics1ValueId)
        {
            try
            {
                var _sql = @"SELECT mm.BaseUOMId AS [Value], uom.UserName AS [Text], uom.StandardName, uom.UserName, uom.Code
                                FROM " + DbSchema.Masters + ".[" + DbTable.MaterialMaster + @"] mm
                                LEFT OUTER JOIN " + DbSchema.SystemConfigurationAndSetup + @".[UnitOfMeasurement] uom ON mm.BaseUOMId = uom.Id
                                WHERE mm.Id='" + MaterialMasterId + @"'
                                AND mm.Archive=0

                                UNION ALL

                                SELECT cwpu.UOMId AS [Value], uom.UserName AS [Text], uom.StandardName, uom.UserName, uom.Code
                                FROM  " + _TableNameDetail + @" cwpd
								LEFT OUTER JOIN " + _TableName + @" cwpu ON cwpd.Id=cwpu.CharacteristicsWisePropertiesDetailId
                                LEFT OUTER JOIN " + _TableNameMaster + @" cwpm ON cwpd.CharacteristicsWisePropertiesMasterId = cwpm.Id
                                LEFT OUTER JOIN " + _TableNameUOM + @" uom ON cwpu.UOMId = uom.Id
                                WHERE cwpm.MaterialMasterId='" + MaterialMasterId + @"'
                                AND ISNULL(cwpd.Characteristics1ValueId,'')=ISNULL('" + Characteristics1ValueId + @"', '')
                                AND cwpd.Archive=0
                                AND cwpu.Archive=0
                                AND cwpm.Archive=0";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetUOMByCharacteristicsValue2nd(string MaterialMasterId, string Characteristics1ValueId, string Characteristics2ValueId)
        {
            try
            {
                var _sql = @"SELECT mm.BaseUOMId AS [Value], uom.UserName AS [Text], uom.StandardName, uom.UserName, uom.Code
                                FROM " + DbSchema.Masters + ".[" + DbTable.MaterialMaster + @"] mm
                                LEFT OUTER JOIN " + DbSchema.SystemConfigurationAndSetup + @".[UnitOfMeasurement] uom ON mm.BaseUOMId = uom.Id
                                WHERE mm.Id='" + MaterialMasterId + @"'
                                AND mm.Archive=0

                                UNION ALL

                                SELECT cwpu.UOMId AS [Value], uom.UserName AS [Text], uom.StandardName, uom.UserName, uom.Code
                                FROM  " + _TableNameDetail + @" cwpd
								LEFT OUTER JOIN " + _TableName + @" cwpu ON cwpd.Id=cwpu.CharacteristicsWisePropertiesDetailId
                                LEFT OUTER JOIN " + _TableNameMaster + @" cwpm ON cwpd.CharacteristicsWisePropertiesMasterId = cwpm.Id
                                LEFT OUTER JOIN " + _TableNameUOM + @" uom ON cwpu.UOMId = uom.Id
                                WHERE cwpm.MaterialMasterId='" + MaterialMasterId + @"'
                                AND ISNULL(cwpd.Characteristics1ValueId,'')=ISNULL('" + Characteristics1ValueId + @"', '')
                                AND ISNULL(cwpd.Characteristics2ValueId,'')=ISNULL('" + Characteristics2ValueId + @"', '')
                                AND cwpd.Archive=0
                                AND cwpu.Archive=0
                                AND cwpm.Archive=0";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetUOMByCharacteristicsValue3rd(string MaterialMasterId, string Characteristics1ValueId, string Characteristics2ValueId, string Characteristics3ValueId)
        {
            try
            {
                var _sql = @"SELECT mm.BaseUOMId AS [Value], uom.UserName AS [Text], uom.StandardName, uom.UserName, uom.Code
                                FROM " + DbSchema.Masters + ".[" + DbTable.MaterialMaster + @"] mm
                                LEFT OUTER JOIN " + DbSchema.SystemConfigurationAndSetup + @".[UnitOfMeasurement] uom ON mm.BaseUOMId = uom.Id
                                WHERE mm.Id='" + MaterialMasterId + @"'
                                AND mm.Archive=0

                                UNION ALL

                                SELECT cwpu.UOMId AS [Value], uom.UserName AS [Text], uom.StandardName, uom.UserName, uom.Code
                                FROM  " + _TableNameDetail + @" cwpd
								LEFT OUTER JOIN " + _TableName + @" cwpu ON cwpd.Id=cwpu.CharacteristicsWisePropertiesDetailId
                                LEFT OUTER JOIN " + _TableNameMaster + @" cwpm ON cwpd.CharacteristicsWisePropertiesMasterId = cwpm.Id
                                LEFT OUTER JOIN " + _TableNameUOM + @" uom ON cwpu.UOMId = uom.Id
                                WHERE cwpm.MaterialMasterId='" + MaterialMasterId + @"'
                                AND ISNULL(cwpd.Characteristics1ValueId,'')=ISNULL('" + Characteristics1ValueId + @"', '')
                                AND ISNULL(cwpd.Characteristics2ValueId,'')=ISNULL('" + Characteristics2ValueId + @"', '')
                                AND ISNULL(cwpd.Characteristics3ValueId,'')=ISNULL('" + Characteristics3ValueId + @"', '')
                                AND cwpd.Archive=0
                                AND cwpu.Archive=0
                                AND cwpm.Archive=0";

                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}