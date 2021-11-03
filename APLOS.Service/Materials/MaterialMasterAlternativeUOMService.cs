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
    public class MaterialMasterAlternativeUOMService : Service<MaterialMasterAlternativeUOM>, IMaterialMasterAlternativeUOMService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<MaterialMasterAlternativeUOM> _materialMasterAlternativeUOMRepository;

        public MaterialMasterAlternativeUOMService(
            IRepositoryAsync<MaterialMasterAlternativeUOM> materialMasterAlternativeUOMRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(materialMasterAlternativeUOMRepository, unitOfWork, pkGeneratorService)
        {
            _materialMasterAlternativeUOMRepository = materialMasterAlternativeUOMRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetMaterialMasterAltUomList(string materialMasterId)
        {
            try
            {
                var _sql = @"SELECT MMAU.*, UOMA.UserName AS AlternativeUOMName,UOMB.UserName AS BaseUOMName
                            ,UsedUomInPO=STUFF((select distinct ', '+Id FROM 
                                                                    TRN.PurchaseOrderDetail										
                                                                    WHERE MMAU.AlternativeUOMId=TransactionUomId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,UsedUomInGRN=STUFF((select distinct ', '+Id FROM 
                                                                    TRN.InventoryReceiveDetail										
                                                                    WHERE MMAU.AlternativeUOMId=TransactionUomId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,UsedUomInBOM=STUFF((select distinct ', '+Id FROM 
                                                                    dbo.BOMDetail										
                                                                    WHERE MMAU.AlternativeUOMId=UomId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            FROM MST.[MaterialMasterAlternativeUOM] AS MMAU 
                            LEFT OUTER JOIN SCS.[UnitOfMeasurement] AS UOMA ON MMAU.AlternativeUOMId=UOMA.Id 
                            LEFT OUTER JOIN SCS.[UnitOfMeasurement] AS UOMB ON MMAU.BaseUOMId=UOMB.Id 
                            WHERE MMAU.Archive = 0 AND MMAU.MaterialMasterId = '" + materialMasterId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void Insert(IEnumerable<MaterialMasterAlternativeUOM> entities, string materiaMasterId)
        {
            try
            {
                var dbList = base.Query(t => t.MaterialMasterId == materiaMasterId).Select().ToList();
                if (entities != null)
                {
                    var pk = _pkGeneratorService.GetMaxNumber(nameof(MaterialMasterAlternativeUOM), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            item.MaterialMasterId = materiaMasterId;
                            InsertGraph(item);
                        }
                        else if (!string.IsNullOrEmpty(item.Id))
                            UpdateGraph(item);
                    }
                }
                if (dbList != null)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbList)
                        {
                            base.DeleteGraph(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                                base.DeleteGraph(item);
                        }
                    }
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public override void Update(MaterialMasterAlternativeUOM charater)
        {
            try
            {
                CheckIdUse(charater.AlternativeUOMId);
                base.Update(charater);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                charater.AddedBy, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets all items in this collection. </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to process all items in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override IQueryFluent<MaterialMasterAlternativeUOM> Query()
        {
            return base.Query(r => !r.Archive);
        }

        public IEnumerable<object> GetAlternativeUOMByMaterialMasterId(string materialMasterId)
        {
            try
            {
                var _sql = @"SELECT '' AS Id, mm.Id AS MaterialMasterId,
                                mm.BaseUOMId AS [Value], uom.UserName AS [Text], uom.StandardName AS AlternativeUOMStandardName,
                                mm.BaseUOMId, uom.UserName AS BaseUOMUserName, uom.StandardName AS BaseUOMStandardName,
                                '1' AS AlternativeUOMFactor, '1' AS BaseUOMFactor
                                FROM " + DbSchema.Masters + ".[" + DbTable.MaterialMaster + @"] mm
                                LEFT OUTER JOIN " + DbSchema.SystemConfigurationAndSetup + @".[UnitOfMeasurement] uom ON mm.BaseUOMId = uom.Id
                                WHERE mm.Id='" + materialMasterId + @"'
                                AND mm.Archive=0
                                UNION ALL
                                SELECT mmau.Id, mmau.MaterialMasterId,
                                mmau.AlternativeUOMId AS Value, auom.UserName AS Text, auom.StandardName AS AlternativeUOMStandardName,
                                mmau.BaseUOMId, buom.UserName AS BaseUOMUserName, buom.StandardName AS BaseUOMStandardName,
                                mmau.AlternativeUOMFactor, mmau.BaseUOMFactor
                                FROM " + DbSchema.Masters + ".[" + DbTable.MaterialMasterAlternativeUOM + @"] mmau
                                LEFT OUTER JOIN " + DbSchema.SystemConfigurationAndSetup + @".[UnitOfMeasurement] auom ON mmau.AlternativeUOMId = auom.Id
                                LEFT OUTER JOIN " + DbSchema.SystemConfigurationAndSetup + @".[UnitOfMeasurement] buom ON mmau.BaseUOMId = buom.Id
                                WHERE mmau.MaterialMasterId='" + materialMasterId + @"'
                                AND mmau.Archive=0";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CheckIdUse(string uomId)
        {
            var sql = "IF EXISTS(SELECT 1 FROM (  " +
                        $"SELECT AlternativeUOMId AS CheckingColumn FROM [{DbSchema.Masters}].[{DbTable.CharacteristicsWisePropertiesUOMFactor}] WHERE Archive=0 " +
                        $") A WHERE CheckingColumn='{uomId}' ) SELECT 1 ELSE SELECT 0 RETURN ";
            var data = Convert.ToBoolean(_materialMasterAlternativeUOMRepository.SqlQuery<int>(sql).Single());
            if (data)
                throw new CustomException("Already characteristics uom factor exist, you can't delete....!");
        }

        public void DeleteGraph(string materialMasterId)
        {
            CheckIdUse(materialMasterId);
            var materialMasterAlt = base.Query(m => m.MaterialMasterId == materialMasterId).Select();
            if (materialMasterAlt != null)
            {
                foreach (var item in materialMasterAlt)
                {
                    base.DeleteGraph(item);
                }
            }
        }
    }
}