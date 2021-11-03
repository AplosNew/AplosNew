using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.IE;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.IEnumerable
{
    public class SubsectionStructureMasterService : Service<SubsectionStructureMaster>, ISubsectionStructureMasterService
    {
        private readonly string _TableName = DbSchema.Transaction + ".[" + DbTable.SubsectionStructureMaster + "]";
        private readonly string _TN_SSDetail = DbSchema.Transaction + ".[" + DbTable.SubsectionStructureDetail + "]";
        private readonly string _TableNameCompany = DbSchema.Organizations + ".[Company]";
        private readonly string _TableNamePlant = DbSchema.Organizations + ".[Plant]";
        private readonly string _Section = DbSchema.Organizations + ".[Section]";
        private readonly string _SubSection = DbSchema.Organizations + ".[SubSection]";
        private readonly string _Department = DbSchema.Organizations + ".[Department]";
        private readonly string _Division = DbSchema.Organizations + ".[Division]";
        private readonly string _Line = DbSchema.Organizations + ".[Line]";
        private readonly string _TableNameUnit = DbSchema.Organizations + ".[Unit]";
        private readonly string _TableNameProcess = DbSchema.HKP + ".[" + DbTable.Process + "]";

        #region Constructor

        private readonly ISubsectionStructureDetailService _subsectionstructuredetailservice;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<SubsectionStructureMaster> _subsectionstructurerepository;
        private readonly IRepositoryAsync<SubsectionStructureDetail> _subsectionstructureDetailRepository;

        public SubsectionStructureMasterService(
            IRepositoryAsync<SubsectionStructureMaster> subsectionstructurerepository,
            IUnitOfWork unitOfWork,
            ISubsectionStructureDetailService subsectionstructuredetailservice,
            IPKGeneratorService pkGeneratorService
            , IRepositoryAsync<SubsectionStructureDetail> subsectionstructureDetailRepository
            , ISqlRepository sqlRepository
            ) :
            base(subsectionstructurerepository, unitOfWork, pkGeneratorService)
        {
            _subsectionstructureDetailRepository = subsectionstructureDetailRepository;
            _subsectionstructurerepository = subsectionstructurerepository;
            _subsectionstructuredetailservice = subsectionstructuredetailservice;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel GetSearchData(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT m.Id
                                 , m.Code
                                 , m.Description
                                 , CONVERT(VARCHAR(5),m.StartTime,108) StartTime
                                 , CONVERT(VARCHAR(5),m.LunchStartTime,108) LunchStartTime
                                 , CONVERT(VARCHAR(5),m.LunchEndTime,108) LunchEndTime
                                 --, m.LunchStartTime
                                 --, m.LunchEndTime
                                 , m.Sequence
                                , m.ApplicableForProduction
                                , m.ApplicableForWIP
                                , m.ApplicableForIncentive
                                , m.ApplicableForBulletin
								 , c.UserName Company
								 , p.UserName Plant
								 , u.UserName Unit
								 , pr.UserName Process
                                 , m.CompanyId
								 , m.UnitId
								 , m.PlantId
								 , m.ProcessId
                                    FROM " + _TableName + @" AS m left outer join
                                    " + _TableNameCompany + @" c  ON c.Id=m.CompanyId left outer join
                                    " + _TableNamePlant + @" p  ON p.Id=m.PlantId left outer join
                                    " + _TableNameUnit + @" u  ON u.Id=m.UnitId left outer join
                                    " + _TableNameProcess + @" pr  ON pr.Id=m.ProcessId
                                    WHERE   m.Archive=0
                                            and m.Companygroupid='" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM {DbSchema.Transaction}.[{DbTable.OperationElement}] WHERE Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public void InsertORUpdate(SubsectionStructureMaster master_ui, out string masterid)
        {
            SubsectionStructureMaster localMaster = null;
            masterid = string.Empty;
            var flag = false;
            try
            {
                OutMaster(master_ui, out localMaster);
                AuditService.Log(localMaster);
                InsertOrUpdateGraph(localMaster);

                _unitOfWork.BeginTransaction();
                flag = true;
                CheckCodeDuplicate(master_ui);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                masterid = localMaster.Id;
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

        private void CheckCodeDuplicate(SubsectionStructureMaster from_ui)
        {
            IEnumerable<object> masterlist = null;
            try
            {
                masterlist = GetMasterListbyplantId(from_ui);
                if (masterlist.Count() > 0)
                {
                    throw new Exception("Same Code:[" + from_ui.Code + "] has been added for this plant...");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CheckDuplicate(SubsectionStructureDetail detail_ui)
        {
            IEnumerable<object> detailList = null;
            try
            {
                detailList = _subsectionstructuredetailservice.GetList(detail_ui.SubsectionStructureMasterId);
                foreach (var item in detailList)
                {
                    var dic = (Dictionary<string, object>)item;
                    if (dic["Id"].ToString() != detail_ui.Id)
                    {
                        if (dic["DivisionId"].ToString() == detail_ui.DivisionId && dic["DepartmentId"].ToString() == detail_ui.DepartmentId && dic["SectionId"].ToString() == detail_ui.SectionId && dic["SubsectionId"].ToString() == detail_ui.SubsectionId && dic["LineId"].ToString() == detail_ui.LineId)
                        {
                            throw new Exception("Same Combination [Division:" + dic["Division"]+ ", Department:" + dic["Department"]+ ", Section:" + dic["Section"]+ ", Subsection:" + dic["Subsection"]+ " and Line:" + dic["Line"]+ "] already exists...");
                        }
                    }//id
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OutMaster(SubsectionStructureMaster from_ui, out SubsectionStructureMaster from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetMaster(from_ui.Id);

                if (from_db.Id == null || from_db.Id == "")
                {
                    from_db = new SubsectionStructureMaster
                    {
                        ModelState = ModelState.Added,
                        //AuditService.Log(from_db);

                        Id = GetPK(),//set pk

                        UnitId = from_ui.UnitId,//set
                        CompanyId = from_ui.CompanyId,
                        CompanyGroupId = identity.CompanyGroupId,
                        ProcessId = from_ui.ProcessId,
                        Archive = false,
                        PlantId = from_ui.PlantId,
                        Description = from_ui.Description,
                        Code = from_ui.Code,
                        LunchEndTime = from_ui.LunchEndTime,
                        LunchStartTime = from_ui.LunchStartTime,
                        Sequence = from_ui.Sequence,
                        StartTime = from_ui.StartTime,
                        ApplicableForProduction = from_ui.ApplicableForProduction,
                        ApplicableForWIP = from_ui.ApplicableForWIP,
                        ApplicableForIncentive = from_ui.ApplicableForIncentive,
                        ApplicableForBulletin = from_ui.ApplicableForBulletin
                    };
                }
                else
                {
                    //AuditService.Log(from_db);

                    from_db.ModelState = ModelState.Modified;

                    from_db.UnitId = from_ui.UnitId;//set
                    from_db.CompanyId = from_ui.CompanyId;
                    //from_db.CompanyGroupId = identity.CompanyGroupId;
                    from_db.ProcessId = from_ui.ProcessId;
                    from_db.Archive = false;
                    from_db.PlantId = from_ui.PlantId;
                    from_db.Description = from_ui.Description;
                    from_db.Code = from_ui.Code;
                    from_db.LunchEndTime = from_ui.LunchEndTime;
                    from_db.LunchStartTime = from_ui.LunchStartTime;
                    from_db.Sequence = from_ui.Sequence;
                    from_db.StartTime = from_ui.StartTime;
                    from_db.ApplicableForProduction = from_ui.ApplicableForProduction;
                    from_db.ApplicableForWIP = from_ui.ApplicableForWIP;
                    from_db.ApplicableForIncentive = from_ui.ApplicableForIncentive;
                    from_db.ApplicableForBulletin = from_ui.ApplicableForBulletin;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OutDetail(SubsectionStructureDetail from_ui, out SubsectionStructureDetail from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetDetail(from_ui.Id);
                ///Check Duplicate

                // detailList = _subsectionstructuredetailservice.GetList(from_ui.SubsectionStructureMasterId);//get all child for this master
                //CheckDuplicate(from_ui);
                // Validation(from_db.Id, from_db.BulletinMasterId, from_db.ZoneId, from_db.ComponentId, from_db.OperationId);

                if (from_db.Id == null || from_db.Id == "")
                {
                    from_db = new SubsectionStructureDetail
                    {
                        ModelState = ModelState.Added,
                        //AuditService.Log(from_db);

                        Id = _subsectionstructuredetailservice.GetPK(),//set pk
                        Archive = false,

                        DepartmentId = from_ui.DepartmentId,
                        DivisionId = from_ui.DivisionId,
                        SubdivisionId = from_ui.SubdivisionId,
                        LineId = from_ui.LineId,
                        SectionId = from_ui.SectionId,
                        SubsectionId = from_ui.SubsectionId,
                        SubsectionStructureMasterId = from_ui.SubsectionStructureMasterId
                    };
                }
                else
                {
                    //AuditService.Log(from_db);

                    from_db.ModelState = ModelState.Modified;
                    from_db.Archive = from_ui.Archive;

                    from_db.DepartmentId = from_ui.DepartmentId;
                    from_db.DivisionId = from_ui.DivisionId;
                    from_db.SubdivisionId = from_ui.SubdivisionId;
                    from_db.LineId = from_ui.LineId;
                    from_db.SectionId = from_ui.SectionId;
                    from_db.SubsectionId = from_ui.SubsectionId;
                    from_db.SubsectionStructureMasterId = from_ui.SubsectionStructureMasterId;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertORUpdateDetail(SubsectionStructureDetail detail)
        {
            SubsectionStructureDetail localDetail = null;
            var flag = false;
            try
            {
                OutDetail(detail, out localDetail);
                AuditService.Log(localDetail);
                _subsectionstructuredetailservice.InsertOrUpdateGraph(localDetail);

                _unitOfWork.BeginTransaction();
                flag = true;
                CheckDuplicate(detail);
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

        private void DelMaster(string id, out SubsectionStructureMaster from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetMaster(id);

                if (from_db.Id != null && from_db.Id != "")
                {
                    from_db.ModelState = ModelState.Modified;
                    from_db.Archive = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DelDetail(string id, out SubsectionStructureDetail from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetDetail(id);

                if (from_db.Id != null && from_db.Id != "")
                {
                    //AuditService.Log(from_db,true);

                    from_db.ModelState = ModelState.Modified;
                    from_db.Archive = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DelChildList(string id, out IEnumerable<SubsectionStructureDetail> from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetChild(id);
                foreach (var ui in from_db)
                {
                    var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                    if (db != null)
                    {
                        db.ModelState = ModelState.Modified;
                        db.Archive = true;
                    }
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteMasterDetail(string masterid)
        {
            SubsectionStructureMaster from_db = null;
            IEnumerable<SubsectionStructureDetail> detaillist = null;
            var flag = false;
            try
            {
                //master
                DelMaster(masterid, out from_db);
                from_db.Archive = true;
                AuditService.Log(from_db);
                InsertOrUpdateGraph(from_db);
                //detail
                DelChildList(masterid, out detaillist);
                foreach (var item in detaillist)
                {
                    item.Archive = true;
                    AuditService.Log(item);
                    _subsectionstructuredetailservice.InsertOrUpdateGraph(item);
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

        public void DeleteDetail(string detailid)
        {
            SubsectionStructureDetail from_db = null;
            var flag = true;
            try
            {
                //master
                DelDetail(detailid, out from_db);
                from_db.Archive = true;
                AuditService.Log(from_db);
                _subsectionstructuredetailservice.InsertOrUpdateGraph(from_db);

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

        private string GetPK()
        {
            return "SSM" + _pkGeneratorService.GetAutoNumber(nameof(SubsectionStructureMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public IEnumerable<object> GetMasterList(string masterid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"SELECT m.Id
                                 , m.Code
                                 , m.Description
                                 , CONVERT(VARCHAR(5),m.StartTime,108) StartTime
                                 , CONVERT(VARCHAR(5),m.LunchStartTime,108) LunchStartTime
                                 , CONVERT(VARCHAR(5),m.LunchEndTime,108) LunchEndTime
                                 --, m.LunchStartTime
                                 --, m.LunchEndTime
                                 , m.Sequence
                                , m.ApplicableForProduction
                                , m.ApplicableForWIP
                                , m.ApplicableForIncentive
                                , m.ApplicableForBulletin
								 , c.UserName Company
								 , p.UserName Plant
								 , u.UserName Unit
								 , pr.UserName Process
                                 , m.CompanyId
								 , m.UnitId
								 , m.PlantId
								 , m.ProcessId
                                    FROM " + _TableName + @" AS m left outer join
                                    " + _TableNameCompany + @" c  ON c.Id=m.CompanyId left outer join
                                    " + _TableNamePlant + @" p  ON p.Id=m.PlantId left outer join
                                    " + _TableNameUnit + @" u  ON u.Id=m.UnitId left outer join
                                    " + _TableNameProcess + @" pr  ON pr.Id=m.ProcessId
                                    WHERE   m.Archive=0
                                            and m.Companygroupid='" + identity.CompanyGroupId + @"'
                                            and m.Id='" + masterid + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetMasterList()
        {
            try
            {
                var _sql = "select * from " + _TableName + "  where archive=0";
                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetMasterListbyplantId(SubsectionStructureMaster ob)
        {
            try
            {
                var _sql = "select * from " + _TableName + "  where archive=0 and plantid='" + ob.PlantId + "' and code='" + ob.Code + "' and Id<>'" + ob.Id + "'";
                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public SubsectionStructureMaster GetMaster(string PK)
        {
            try
            {
                var _sql = "select * from " + _TableName + " where Id='" + PK + "'  and archive=0";
                return _subsectionstructurerepository.SelectQuery(_sql, null).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public SubsectionStructureDetail GetDetail(string PK)
        {
            try
            {
                var _sql = "select * from " + _TN_SSDetail + " where Id='" + PK + "'  and archive=0";
                //return _operationtimecapturedetailservice.SelectQuery(_sql,null);
                return _subsectionstructureDetailRepository.SelectQuery(_sql, null).FirstOrDefault();
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
                return _subsectionstructuredetailservice.GetList(MasterId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<SubsectionStructureDetail> GetChild(string MasterId)
        {
            try
            {
                var _sql = "select * from " + _TN_SSDetail + " where SubsectionStructureMasterId='" + MasterId + "' and archive=0";
                return _subsectionstructureDetailRepository.SqlQuery<SubsectionStructureDetail>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

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

        //getprocesscbo getunitcbo getplantcbo getcountrycbo
        public IEnumerable<object> GetDepartmentListCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"     SELECT d.Id [Value]
                                      ,d.UserName [Text]
                                  FROM " + _Department + @" d
                                    WHERE d.Archive=0 AND d.Active=1 ORDER BY d.UserName";
                return _subsectionstructureDetailRepository.SqlQuery<SubsectionStructureDetail>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetDivisionListCbo()
        {
            try
            {
                var _sql = @"   SELECT d.Id [Value]
                                      ,d.UserName [Text]
                                  FROM " + _Division + @" d
                                    WHERE d.Archive=0 AND d.Active=1 ORDER BY d.UserName";
                return _subsectionstructureDetailRepository.SqlQuery<SubsectionStructureDetail>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetSubsectionListCbo()
        {
            try
            {
                var _sql = @"   SELECT s.Id [Value]
                                      ,s.UserName [Text]
                                  FROM " + _SubSection + @" s
                                    WHERE s.Archive=0 AND s.Active=1 ORDER BY s.UserName";
                return _subsectionstructureDetailRepository.SqlQuery<SubsectionStructureDetail>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetSectionListCbo()
        {
            try
            {
                var _sql = @"   SELECT s.Id [Value]
                                      ,s.UserName [Text]
                                  FROM " + _Section + @" s
                                    WHERE s.Archive=0 AND s.Active=1 ORDER BY s.UserName";
                return _subsectionstructureDetailRepository.SqlQuery<SubsectionStructureDetail>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetLineListCbo()
        {
            try
            {
                var _sql = @" SELECT l.Id [Value]
                                , l.UserName[Text]
                                    FROM " + _Line + @" l
                                    WHERE l.Archive=0 AND l.Active=1 ORDER BY l.UserName";
                return _subsectionstructureDetailRepository.SqlQuery<SubsectionStructureDetail>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}