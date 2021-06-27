#region Using

using Library.Core;
using Library.Crosscutting.Security;
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
using System.Threading;

#endregion Using

namespace Library.Service.Materials
{
    public class CharacteristicsWisePropertiesMasterService : Service<CharacteristicsWisePropertiesMaster>, ICharacteristicsWisePropertiesMasterService
    {
        private readonly string _TableName = DbSchema.Masters + ".[" + DbTable.CharacteristicsWisePropertiesMaster + "]";
        private readonly string _CwpUOM = DbSchema.Masters + ".[" + DbTable.CharacteristicsWisePropertiesUOM + "]";
        private readonly string _CwpfUOM = DbSchema.Masters + ".[" + DbTable.CharacteristicsWisePropertiesUOMFactor + "]";
        private readonly string _MM = DbSchema.Masters + ".[" + DbTable.MaterialMaster + "]";
        private readonly string _C = DbSchema.HKP + ".[" + DbTable.Characteristics + "]";
        private readonly string _UOM = DbSchema.SystemConfigurationAndSetup + ".[UnitOfMeasurement]";

        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly ICharacteristicsWisePropertiesUOMService _characteristicswisepropertiesuomservice;
        private readonly ICharacteristicsWisePropertiesUOMFactorService _characteristicswisepropertiesuomfactorservice;
        private readonly ICharacteristicsWisePropertiesDetailService _characteristicswisepropertiesdetailservice;
        private readonly IRepositoryAsync<CharacteristicsWisePropertiesMaster> _characteristicsWisePropertiesMaster;

        public CharacteristicsWisePropertiesMasterService(
            IRepositoryAsync<CharacteristicsWisePropertiesMaster> characteristicsWisePropertiesMaster,
            IPKGeneratorService pkGeneratorService,
            ICharacteristicsWisePropertiesUOMService characteristicswisepropertiesuomservice,
            ICharacteristicsWisePropertiesUOMFactorService characteristicswisepropertiesuomfactorservice,
            ICharacteristicsWisePropertiesDetailService characteristicswisepropertiesdetailservice,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(characteristicsWisePropertiesMaster, unitOfWork)
        {
            _characteristicsWisePropertiesMaster = characteristicsWisePropertiesMaster;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
            _characteristicswisepropertiesuomservice = characteristicswisepropertiesuomservice;
            _characteristicswisepropertiesuomfactorservice = characteristicswisepropertiesuomfactorservice;
            _characteristicswisepropertiesdetailservice = characteristicswisepropertiesdetailservice;
        }

        #endregion Constructor

        public GridModel GetSearchData(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"
                                    SELECT m.Id
	                                    ,m.MaterialMasterId
	                                    ,m.Characteristics1Id
	                                    ,m.Characteristics2Id
	                                    ,m.Characteristics3Id
	                                    ,mm.[Description] MaterialMasterDescription
	                                    ,mm.Code MaterialMasterCode
	                                    ,c1.[Description] Characteristics1
	                                    ,c2.[Description] Characteristics2
	                                    ,c3.[Description] Characteristics3

                                    FROM " + _TableName + @" m
                                    LEFT JOIN " + _MM + @" mm ON mm.Id = m.MaterialMasterId
                                    --*******Characteristics***********
                                    LEFT JOIN " + _C + @" c1 ON c1.Id = m.Characteristics1Id
                                    LEFT JOIN " + _C + @" c2 ON c2.Id = m.Characteristics2Id
                                    LEFT JOIN " + _C + @" c3 ON c3.Id = m.Characteristics3Id
                                    Where m.Archive=0 and m.CompanyGroupId='" + identity.CompanyGroupId + @"' ";
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

        public IEnumerable<object> GetList(string masterid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"
                                    SELECT m.Id
	                                    ,m.MaterialMasterId
	                                    ,m.Characteristics1Id
	                                    ,m.Characteristics2Id
	                                    ,m.Characteristics3Id
                                        ,'xyz' SelectedCharacteristics
                                        , Characteristics1Selected = CASE isnull(m.Characteristics1Id,'')
                                                WHEN '' THEN 0
                                                ELSE 1
                                                END
                                        , Characteristics2Selected = CASE isnull(m.Characteristics2Id,'')
                                                 WHEN '' THEN 0
                                                 ELSE 1
                                                 END
                                        , Characteristics3Selected = CASE isnull(m.Characteristics3Id,'')
                                                 WHEN '' THEN 0
                                                 ELSE 1
                                                 END

	                                    ,mm.[Description] Description
	                                    ,mm.Code Code
	                                    ,c1.[Alias] Characteristics1
	                                    ,c2.[Alias] Characteristics2
	                                    ,c3.[Alias] Characteristics3

                                    FROM " + _TableName + @" m
                                    LEFT JOIN " + _MM + @" mm ON mm.Id = m.MaterialMasterId
                                    --*******Characteristics***********
                                    LEFT JOIN " + _C + @" c1 ON c1.Id = m.Characteristics1Id
                                    LEFT JOIN " + _C + @" c2 ON c2.Id = m.Characteristics2Id
                                    LEFT JOIN " + _C + @" c3 ON c3.Id = m.Characteristics3Id
                                    Where m.Archive=0 and m.CompanyGroupId='" + identity.CompanyGroupId + @"' and m.Id='" + masterid + @"'
                                  ";
                //LEFT OUTER JOIN[" + DbSchema.Materials + @"].[" + DbTable.MaterialType+ @"] AS MT ON MM.MaterialTypeId = MT.Id
                //                    LEFT OUTER JOIN[" +DbSchema.Masters+ @"].[" + DbTable.MaterialGroupMaster+ @"] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                //                    LEFT OUTER JOIN[" +DbSchema.Materials+ @"].[" + DbTable.MaterialGrid+ @"] AS MG ON MM.MaterialGridId = MG.Id

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetMasterId(string materialmasterid)
        {
            try
            {
                var _sql = "select Id from " + _TableName + " where MaterialmasterId='" + materialmasterid + "' and archive=0";
                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertORUpdate(CharacteristicsWisePropertiesMaster master, out string masterid)
        {
            masterid = string.Empty;
            CharacteristicsWisePropertiesMaster localMaster = null;
            var flag = false;
            try
            {
                OutMaster(master, out localMaster);
                ValidationMaster(localMaster);
                masterid = master.Id;
                AuditService.Log(localMaster);
                InsertOrUpdateGraph(localMaster);
                masterid = localMaster.Id;
                if (localMaster == null)
                {
                    throw new Exception("Master Data can not be blank...");
                }
                //if (localDetail == null)
                //{
                //    throw new Exception("Detail Data can not be blank...");
                //}
                //if (localUOMFactorList == null && localUOMList==null)
                //{
                //    throw new Exception("Both [Alternative UOM] and [UOM Conversion Factor] can not be blank...");
                //}

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertORUpdate(CharacteristicsWisePropertiesDetail detail, IEnumerable<CharacteristicsWisePropertiesUOMFactor> uomfactorList, IEnumerable<CharacteristicsWisePropertiesUOM> uomList)
        {
            var masterid = string.Empty;
            CharacteristicsWisePropertiesDetail localDetail = null;
            List<CharacteristicsWisePropertiesUOMFactor> localUOMFactorList = null;
            List<CharacteristicsWisePropertiesUOM> localUOMList = null;
            var flag = false;
            try
            {
                masterid = detail.CharacteristicsWisePropertiesMasterId;

                //OutMaster(master, out localMaster);
                //masterid = master.Id;
                //AuditService.Log(localMaster, localMaster.Archive);
                //base.InsertOrUpdateGraph(localMaster);
                ///Detail
                OutDetail(masterid, detail, out localDetail);
                AuditService.Log(localDetail);
                _characteristicswisepropertiesdetailservice.InsertOrUpdateGraph(localDetail);

                ///UOMFactor
                if (uomfactorList != null)
                {
                    GetDB_UOMFactor(masterid, localDetail.Id, uomfactorList, out localUOMFactorList);
                    foreach (var localob in localUOMFactorList)
                    {
                        if (!string.IsNullOrEmpty(localob.BaseUOMFactor) && !string.IsNullOrEmpty(localob.AlternativeUOMId) && !string.IsNullOrEmpty(localob.AlternativeUOMFactor) && !string.IsNullOrEmpty(localob.BaseUOMId))
                        {
                            AuditService.Log(localob);
                            _characteristicswisepropertiesuomfactorservice.InsertOrUpdateGraph(localob);
                        }
                    }
                }
                ///UOM (optional)
                if (uomList != null)
                {
                    GetDB_UOM(masterid, localDetail.Id, uomList, out localUOMList);
                    foreach (var localob in localUOMList)
                    {
                        AuditService.Log(localob);
                        _characteristicswisepropertiesuomservice.InsertOrUpdateGraph(localob);
                    }
                }

                if (localDetail == null)
                {
                    throw new Exception("Detail Data can not be blank...");
                }
                if (localUOMFactorList == null && localUOMList == null)
                {
                    throw new Exception("Both [Alternative UOM] and [UOM Conversion Factor] can not be blank...");
                }

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void OutMaster(CharacteristicsWisePropertiesMaster from_ui, out CharacteristicsWisePropertiesMaster from_db)
        {
            IEnumerable<object> masterList = null;
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetMaster(from_ui.Id);
                masterList = GetMaster(from_ui.Id, from_ui.MaterialMasterId);

                ///Validation
                ///
                if (masterList.Count() > 0)
                {
                    throw new Exception("This Material Master:[" + from_ui.MaterialMasterId + "] has already been taken...");
                }

                if (from_db.Id == null || from_db.Id == "")
                {
                    from_db = new CharacteristicsWisePropertiesMaster
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(from_db);

                    #region Add

                    from_db.Id = GetPK();//set pk

                    from_db.Characteristics1Id = (string.IsNullOrEmpty(from_ui.Characteristics1) ? null : from_ui.Characteristics1Id);
                    from_db.Characteristics3Id = (string.IsNullOrEmpty(from_ui.Characteristics3) ? null : from_ui.Characteristics3Id);
                    from_db.Characteristics2Id = (string.IsNullOrEmpty(from_ui.Characteristics2) ? null : from_ui.Characteristics2Id);
                    from_db.Archive = false;
                    from_db.CompanyGroupId = identity.CompanyGroupId;
                    from_db.CompanyId = identity.CompanyId;
                    from_db.MaterialMasterId = from_ui.MaterialMasterId;

                    #endregion Add
                }
                else
                {
                    #region Edit

                    from_db.ModelState = ModelState.Modified;
                    AuditService.Log(from_db);

                    from_db.Characteristics1Id = (string.IsNullOrEmpty(from_ui.Characteristics1) ? null : from_ui.Characteristics1Id);
                    from_db.Characteristics3Id = (string.IsNullOrEmpty(from_ui.Characteristics3) ? null : from_ui.Characteristics3Id);
                    from_db.Characteristics2Id = (string.IsNullOrEmpty(from_ui.Characteristics2) ? null : from_ui.Characteristics2Id);

                    from_db.Archive = false;
                    from_db.CompanyGroupId = identity.CompanyGroupId;
                    from_db.CompanyId = identity.CompanyId;
                    from_db.MaterialMasterId = from_ui.MaterialMasterId;

                    #endregion Edit
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void GetDB_UOMFactor(string MasterId, string DetailId, IEnumerable<CharacteristicsWisePropertiesUOMFactor> from_ui, out List<CharacteristicsWisePropertiesUOMFactor> from_db)
        {
            var _count = 0;
            from_db = null;
            try
            {
                from_db = _characteristicswisepropertiesuomfactorservice.GetListByDetailId(DetailId).ToList<CharacteristicsWisePropertiesUOMFactor>();
                if (from_db == null)
                {
                    from_db = new List<CharacteristicsWisePropertiesUOMFactor>();
                }

                var _PK = _characteristicswisepropertiesuomfactorservice.GetPK();
                foreach (var ui in from_ui)
                {
                    var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                    if (db == null)//new
                    {
                        if (!ui.Archive)///first time to be inserted and same time it is been deleted
                        {
                            _count += 1;
                            db = new CharacteristicsWisePropertiesUOMFactor
                            {
                                Id = _PK + "-" + _count,
                                ModelState = ModelState.Added,

                                Archive = false,
                                AlternativeUOMFactor = ui.AlternativeUOMFactor,
                                AlternativeUOMId = ui.AlternativeUOMId,
                                BaseUOMFactor = ui.BaseUOMFactor
                            };
                            ;
                            db.BaseUOMId = ui.BaseUOMId;
                            db.CharacteristicsWisePropertiesMasterId = MasterId;
                            db.CharacteristicsWisePropertiesDetailId = DetailId;
                            from_db.Add(db);
                        }
                    }
                    else
                    {
                        db.ModelState = ModelState.Modified;

                        db.Archive = ui.Archive; ;
                        db.AlternativeUOMFactor = ui.AlternativeUOMFactor;
                        db.AlternativeUOMId = ui.AlternativeUOMId;
                        db.BaseUOMFactor = ui.BaseUOMFactor; ;
                        db.BaseUOMId = ui.BaseUOMId;
                        db.CharacteristicsWisePropertiesDetailId = DetailId;
                        db.CharacteristicsWisePropertiesMasterId = MasterId;
                    }
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ValidationUOM(IEnumerable<CharacteristicsWisePropertiesUOM> from_ui, IEnumerable<CharacteristicsWisePropertiesUOM> from_db)
        {
            try
            {
                if (from_ui != null)
                {
                    foreach (var ui in from_ui)
                    {
                        var db = from_db.FirstOrDefault(a => a.Id != ui.Id && a.UOMId == ui.UOMId && !a.Archive);
                        if (db != null && db.Id != null)
                        {
                            var _id = db.Id;
                            var uidb = from_ui.FirstOrDefault(m => m.Id == _id && m.Archive);
                            if (uidb != null)
                            {//if inserted id is archived in ui and inserted again
                            }
                            else
                            {
                                throw (new Exception("UOMID:[" + ui.UOMId + "] already exists..."));
                            }
                        }//if
                    }//for
                }//null
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ValidationDetail(CharacteristicsWisePropertiesDetail from_ui, IEnumerable<CharacteristicsWisePropertiesDetail> from_dblist)
        {
            try
            {
                if (from_ui != null)
                {
                    foreach (var db in from_dblist)
                    {
                        if (db.Id != from_ui.Id && db.Characteristics1ValueId == from_ui.Characteristics1ValueId && db.Characteristics2ValueId == from_ui.Characteristics2ValueId && db.Characteristics3ValueId == from_ui.Characteristics3ValueId)
                        {
                            throw (new Exception("Detail Data already exists..."));
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ValidationMaster(CharacteristicsWisePropertiesMaster from_ui)
        {
            if (from_ui.MaterialMasterId == null || from_ui.MaterialMasterId == "")
            {
                throw new Exception("Masterial Master Id can not be blank...");
            }
            if ((from_ui.Characteristics1Id == null || from_ui.Characteristics1Id == "") && (from_ui.Characteristics2Id == null || from_ui.Characteristics2Id == "") && (from_ui.Characteristics3Id == null || from_ui.Characteristics3Id == ""))
            {
                throw new Exception("Minimum One Characteristics should be selected...");
            }
        }

        private void GetDB_UOM(string MasterId, string DetailId, IEnumerable<CharacteristicsWisePropertiesUOM> from_ui, out List<CharacteristicsWisePropertiesUOM> from_db)
        {
            var _count = 0;
            from_db = null;
            try
            {
                from_db = _characteristicswisepropertiesuomservice.GetListByDetailId(DetailId).ToList();
                if (from_db == null)
                {
                    from_db = new List<CharacteristicsWisePropertiesUOM>();
                }
                ValidationUOM(from_ui, from_db);
                var _PK = _characteristicswisepropertiesuomservice.GetPK();
                foreach (var ui in from_ui)
                {
                    var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                    if (db == null)//new
                    {
                        if (!ui.Archive)///first time to be inserted and same time it is been deleted
                        {
                            _count += 1;
                            db = new CharacteristicsWisePropertiesUOM
                            {
                                Id = _PK + "-" + _count,
                                ModelState = ModelState.Added,

                                Archive = false,
                                CharacteristicsWisePropertiesMasterId = MasterId,
                                CharacteristicsWisePropertiesDetailId = DetailId,
                                UOMId = ui.UOMId
                            };
                            from_db.Add(db);
                        }
                    }
                    else
                    {
                        db.ModelState = ModelState.Modified;

                        db.Archive = ui.Archive; ;
                        db.CharacteristicsWisePropertiesDetailId = DetailId;
                        db.CharacteristicsWisePropertiesMasterId = MasterId;
                        db.UOMId = ui.UOMId;
                    }
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            return "CPM" + _pkGeneratorService.GetAutoNumber(nameof(CharacteristicsWisePropertiesMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public CharacteristicsWisePropertiesMaster GetMaster(string PK)//TBT
        {
            try
            {
                var _sql = "select * from " + _TableName + " where Id='" + PK + "' and Archive=0";
                return _characteristicsWisePropertiesMaster.SelectQuery(_sql, null).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OutDetail(string masterid, CharacteristicsWisePropertiesDetail from_ui, out CharacteristicsWisePropertiesDetail from_db)
        {
            IEnumerable<CharacteristicsWisePropertiesDetail> detaillist = null;
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                detaillist = _characteristicswisepropertiesdetailservice.GetList(masterid);
                ValidationDetail(from_ui, detaillist);
                from_db = detaillist.FirstOrDefault(a => a.Id == from_ui.Id);

                if (from_db == null || from_db.Id == null || from_db.Id == "")
                {
                    from_db = new CharacteristicsWisePropertiesDetail
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(from_db);

                    #region Add

                    from_db.Id = _characteristicswisepropertiesdetailservice.GetPK();//set pk

                    from_db.Characteristics1ValueId = from_ui.Characteristics1ValueId;
                    from_db.Characteristics2ValueId = from_ui.Characteristics2ValueId;
                    from_db.Archive = false;
                    from_db.Characteristics3ValueId = from_ui.Characteristics3ValueId;
                    from_db.MaterialMasterId = from_ui.MaterialMasterId;
                    from_db.CharacteristicsWisePropertiesMasterId = from_ui.CharacteristicsWisePropertiesMasterId;

                    #endregion Add
                }
                else
                {
                    #region Edit

                    from_db.ModelState = ModelState.Modified;
                    AuditService.Log(from_db);

                    from_db.Characteristics1ValueId = from_ui.Characteristics1ValueId;
                    from_db.Characteristics2ValueId = from_ui.Characteristics2ValueId;
                    from_db.Archive = false;
                    from_db.Characteristics3ValueId = from_ui.Characteristics3ValueId;
                    from_db.MaterialMasterId = from_ui.MaterialMasterId;
                    from_db.CharacteristicsWisePropertiesMasterId = from_ui.CharacteristicsWisePropertiesMasterId;

                    #endregion Edit
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetMaster(string PK, string MaterialMasterID)
        {
            try
            {
                var _sql = "select * from " + _TableName + " where Id<>'" + PK + "' and MaterialMasterId='" + MaterialMasterID + "' and Archive=0";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetUOMList(string detailid)
        {
            try
            {
                var sql = @"
                            SELECT
	                            u.Id,u.CharacteristicsWisePropertiesMasterId,CharacteristicsWisePropertiesDetailId,u.UOMId,u.Archive
	                            ,uom.[StandardName] UOM,uom.[Code] UOMCode
                            FROM  " + _CwpUOM + @" u
                            LEFT JOIN " + _UOM + @" uom ON uom.Id = u.UOMId
                                    WHERE   u.Archive=0
                                            and u.CharacteristicsWisePropertiesDetailId='" + detailid + @"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetUOMFactorList(string detailid)
        {
            try
            {
                var sql = @"SELECT
                            f.Id,	f.CharacteristicsWisePropertiesMasterId,f.AlternativeUOMId,f.AlternativeUOMFactor,f.BaseUOMId,f.BaseUOMFactor,f.Archive
	                            ,auom.[StandardName] FAUOM
	                            ,buom.[StandardName] BaseUOM
                            FROM  " + _CwpfUOM + @" f
                            LEFT JOIN " + _UOM + @" auom ON auom.Id = f.AlternativeUOMId
                            LEFT JOIN " + _UOM + @" buom ON buom.Id = f.BaseUOMId
                                    WHERE   f.Archive=0
                                            and f.CharacteristicsWisePropertiesDetailId='" + detailid + @"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetList()
        {
            try
            {
                var _sql = "select * from " + _TableName + " where archive=0";
                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DelDetail(string masterid, out IEnumerable<CharacteristicsWisePropertiesDetail> from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = _characteristicswisepropertiesdetailservice.GetList(masterid);

                if (from_db == null)
                {
                    throw (new Exception("No Detail Data found against MasterId:[" + masterid + "]"));
                }

                foreach (var ui in from_db)
                {
                    var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                    if (db != null)
                    {
                        db.ModelState = ModelState.Modified;
                        AuditService.Log(db);
                        db.Archive = true;
                    }
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DelMaster(string id, out CharacteristicsWisePropertiesMaster from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetMaster(id);

                if (from_db == null || from_db.Id == "")
                {
                    throw (new Exception("No Data found against ID:[" + id + "]"));
                }
                from_db.ModelState = ModelState.Modified;
                AuditService.Log(from_db);
                from_db.Archive = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DelUomList(string id, out IEnumerable<CharacteristicsWisePropertiesUOM> from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = _characteristicswisepropertiesuomservice.GetList(id).ToList<CharacteristicsWisePropertiesUOM>();
                foreach (var ui in from_db)
                {
                    var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                    if (db != null)
                    {
                        db.ModelState = ModelState.Modified;
                        AuditService.Log(db);
                        db.Archive = true;
                    }
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DelUomFactorList(string id, out IEnumerable<CharacteristicsWisePropertiesUOMFactor> from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = _characteristicswisepropertiesuomfactorservice.GetList(id).ToList<CharacteristicsWisePropertiesUOMFactor>();
                foreach (var ui in from_db)
                {
                    var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                    if (db != null)
                    {
                        db.ModelState = ModelState.Modified;
                        AuditService.Log(db);
                        db.Archive = true;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteMasterDetail(string masterid)
        {
            CharacteristicsWisePropertiesMaster from_db = null;
            IEnumerable<CharacteristicsWisePropertiesDetail> detaillist = null;
            IEnumerable<CharacteristicsWisePropertiesUOM> uomlist = null;
            IEnumerable<CharacteristicsWisePropertiesUOMFactor> uomfactorlist = null;
            var flag = false;
            try
            {
                #region Data

                //master
                DelMaster(masterid, out from_db);
                InsertOrUpdateGraph(from_db);
                //detail
                DelDetail(masterid, out detaillist);
                //_characteristicswisepropertiesdetailservice.InsertOrUpdateGraph(detaillist);
                //DelUomList(masterid, out uomlist);
                foreach (var item in detaillist)
                {
                    _characteristicswisepropertiesdetailservice.InsertOrUpdateGraph(item);
                }
                //uom
                DelUomList(masterid, out uomlist);
                foreach (var item in uomlist)
                {
                    _characteristicswisepropertiesuomservice.InsertOrUpdateGraph(item);
                }
                //uomfactor
                DelUomFactorList(masterid, out uomfactorlist);
                foreach (var item in uomfactorlist)
                {
                    _characteristicswisepropertiesuomfactorservice.InsertOrUpdateGraph(item);
                }

                #endregion Data

                #region Transaction

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                #endregion Transaction
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
                from_db = null;
                uomlist = null;
                uomfactorlist = null;
            }
        }

        public void DeleteDetail(string detailid)
        {
            CharacteristicsWisePropertiesDetail from_db_detail = null;
            IEnumerable<CharacteristicsWisePropertiesUOM> uomlist = null;
            IEnumerable<CharacteristicsWisePropertiesUOMFactor> uomfactorlist = null;
            var flag = false;
            try
            {
                #region Data

                //detail
                DelDetailById(detailid, out from_db_detail);
                _characteristicswisepropertiesdetailservice.InsertOrUpdateGraph(from_db_detail);
                //uom
                DelUomListByDetailId(detailid, out uomlist);
                foreach (var item in uomlist)
                {
                    _characteristicswisepropertiesuomservice.InsertOrUpdateGraph(item);
                }
                //uomfactor
                DelUomFactorListByDetailId(detailid, out uomfactorlist);
                foreach (var item in uomfactorlist)
                {
                    _characteristicswisepropertiesuomfactorservice.InsertOrUpdateGraph(item);
                }

                #endregion Data

                #region Transaction

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                #endregion Transaction
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
                from_db_detail = null;
                uomlist = null;
                uomfactorlist = null;
            }
        }

        private void DelUomFactorListByDetailId(string detailid, out IEnumerable<CharacteristicsWisePropertiesUOMFactor> from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = _characteristicswisepropertiesuomfactorservice.GetListByDetailId(detailid).ToList<CharacteristicsWisePropertiesUOMFactor>();
                foreach (var ui in from_db)
                {
                    var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                    if (db != null)
                    {
                        db.ModelState = ModelState.Modified;
                        AuditService.Log(db);
                        db.Archive = true;
                    }
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DelUomListByDetailId(string detailid, out IEnumerable<CharacteristicsWisePropertiesUOM> from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //from_db = GetUOMList(id);
                from_db = _characteristicswisepropertiesuomservice.GetListByDetailId(detailid).ToList<CharacteristicsWisePropertiesUOM>();
                foreach (var ui in from_db)
                {
                    var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                    if (db != null)
                    {
                        db.ModelState = ModelState.Modified;
                        AuditService.Log(db);
                        db.Archive = true;
                    }
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DelDetailById(string detailid, out CharacteristicsWisePropertiesDetail from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = _characteristicswisepropertiesdetailservice.GetDetail(detailid);

                if (from_db == null || from_db.Id == "")
                {
                    throw (new Exception("No Detail Data found against Detail:[" + detailid + "]"));
                }
                from_db.ModelState = ModelState.Modified;
                AuditService.Log(from_db);
                from_db.Archive = true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}