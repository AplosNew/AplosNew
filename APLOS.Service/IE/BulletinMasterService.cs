using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.IE;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.IEnumerable
{
    public class BulletinMasterService : Service<BulletinMaster>, IBulletinMasterService
    {
        #region Constructor

        private readonly IBulletinDetailService _bulletinedetailservice;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<BulletinProcess> _bulletinProcessRepository;
        private readonly IRepositoryAsync<BulletinDetail> _bulletinDetailRepository;
        private readonly IRepositoryAsync<BulletinMaster> _bulletinemasterrepository;

        public BulletinMasterService(
            IRepositoryAsync<BulletinMaster> bulletinemasterrepository
            , IRepositoryAsync<BulletinProcess> bulletinProcessRepository
            , IUnitOfWork unitOfWork
            , IBulletinDetailService bulletinedetailservice
            , IPKGeneratorService pkGeneratorService
            , IRepositoryAsync<BulletinDetail> bulletinDetailRepository
            , ISqlRepository sqlRepository) : base(bulletinemasterrepository, unitOfWork, pkGeneratorService)
        {
            _bulletinemasterrepository = bulletinemasterrepository;
            _bulletinDetailRepository = bulletinDetailRepository;
            _bulletinedetailservice = bulletinedetailservice;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _bulletinProcessRepository = bulletinProcessRepository;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel GetSearchData(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT m.Id, mm.UserName AS MaterialMaster, ma.StandardName AS Article, m.Description
                            , m.MaterialMasterId ,m.MaterialMasterArticleId, m.WorkingHour
                    FROM TRN.[BulletinMaster] AS m
                    LEFT JOIN [MST].[MaterialMaster] mm  ON mm.Id=m.MaterialMasterId
                    LEFT JOIN [MST].[MaterialMasterArticle] ma  ON ma.Id=m.MaterialMasterArticleId
                    LEFT JOIN MST.MaterialMasterArticle AS ART ON m.MaterialMasterArticleId=ART.Id
                    WHERE m.CompanyId='" + companyId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void SaveMaster(BulletinMaster from_ui, out BulletinMaster from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetBulletinMaster(from_ui.Id);
                if (from_db == null)
                {
                    from_db = new BulletinMaster
                    {
                        ModelState = ModelState.Added,
                        //AuditService.Log(from_db);

                        #region Add

                        Id = PK,//set pk

                        CompanyId = identity.CompanyId,
                        Archive = false,
                        CompanyGroupId = identity.CompanyGroupId,
                        Description = from_ui.Description,
                        Sequence = from_ui.Sequence,
                        MaterialMasterId = from_ui.MaterialMasterId,
                        MaterialMasterArticleId = from_ui.MaterialMasterArticleId,
                        WorkingHour = from_ui.WorkingHour
                    };

                    #endregion Add
                }
                else
                {
                    //AuditService.Log(from_db);

                    #region Edit

                    from_db.ModelState = ModelState.Modified;

                    from_db.CompanyId = identity.CompanyId;
                    from_db.Archive = from_ui.Archive;
                    from_db.CompanyGroupId = identity.CompanyGroupId;
                    from_db.Description = from_ui.Description;
                    from_db.Sequence = from_ui.Sequence;
                    from_db.MaterialMasterId = from_ui.MaterialMasterId;
                    from_db.MaterialMasterArticleId = from_ui.MaterialMasterArticleId;
                    from_db.WorkingHour = from_ui.WorkingHour;

                    #endregion Edit
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertORUpdateMaster(BulletinMaster bulletinmaster, out string masterid)
        {
            BulletinMaster localMaster = null;
            masterid = string.Empty;
            var flag = false;
            try
            {
                SaveMaster(bulletinmaster, out localMaster);
                AuditService.Log(localMaster);
                InsertOrUpdateGraph(localMaster);

                _unitOfWork.BeginTransaction();
                flag = true;
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

        private void Validation(string pk, string bulletinmasterid, string zoneid, string componentid, string operationid)
        {
            //has duplicate zone,compo and operation
            var dslocal = GetBulletinMasterList(pk, bulletinmasterid, zoneid, componentid, operationid);
            if (dslocal.Tables[0].Rows.Count > 0)
            {
                var val = "Id:[" + dslocal.Tables[0].Rows[0]["Id"]+ "] has already same Zone, Component and Operation !!!";
                throw (new Exception(val));
            }
        }

        private void GetDBDetail(BulletinDetail from_ui, out BulletinDetail from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = _bulletinedetailservice.Find(from_ui.Id);
                Validation(from_ui.Id, from_ui.BulletinMasterId, from_ui.ZoneId, from_ui.ComponentId, from_ui.OperationId);
                if (from_db == null)
                {
                    from_db = new BulletinDetail
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(from_db);

                    from_db.Id = _bulletinedetailservice.GetPK();
                    from_db.BulletinMasterId = from_ui.BulletinMasterId;
                    from_db.CompanyGroupId = identity.CompanyGroupId;
                    from_db.CompanyId = identity.CompanyId;
                    from_db.ProcessId = from_ui.ProcessId;
                }
                else
                {
                    AuditService.Log(from_db);
                    from_db.ModelState = ModelState.Modified;
                }

                from_db.AllotedManpower = from_ui.AllotedManpower;
                from_db.AllotedWorkstation = from_ui.AllotedWorkstation;
                from_db.ComponentId = from_ui.ComponentId;
                from_db.OperationActionId = from_ui.OperationActionId;
                //from_db.ManpowerBudgetId = from_ui.ManpowerBudgetId;
                from_db.IsDirect = from_ui.IsDirect;
                from_db.IsLastOperation = from_ui.IsLastOperation;
                from_db.IsPrintable = from_ui.IsPrintable;
                from_db.MachineExecutiontype = from_ui.MachineExecutiontype;
                from_db.Sequence = from_ui.Sequence;
                from_db.MaterialMasterArticleId = from_ui.MaterialMasterArticleId;
                from_db.OperationId = from_ui.OperationId;
                from_db.Remark = from_ui.Remark;
                from_db.UserDefinedSPT = from_ui.UserDefinedSPT;
                from_db.ZoneId = from_ui.ZoneId;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertORUpdateDetail(BulletinDetail bulletindetail)
        {
            BulletinDetail localDetail = null;
            var flag = false;
            try
            {
                GetDBDetail(bulletindetail, out localDetail);
                AuditService.Log(localDetail);
                _bulletinedetailservice.InsertOrUpdateGraph(localDetail);

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

        // delete master and child at a time
        private BulletinMaster DelMaster(string id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var from_db = GetBulletinMaster(id);

                if (from_db != null)
                    from_db.ModelState = ModelState.Deleted;
                return from_db;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DelDetail(string id, out BulletinDetail from_db)
        {
            from_db = null;
            try
            {
                from_db = GetBulletinDetail(id);

                if (!string.IsNullOrEmpty(from_db.Id))
                {
                    from_db.ModelState = ModelState.Deleted;
                    // AuditService.Log(from_db);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteMasterDetail(string masterid)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var from_db = DelMaster(masterid);
                var dbData = _bulletinProcessRepository.Query(t => t.BulletinId == masterid).Select().ToList();
                var dbDetailData = _bulletinDetailRepository.Query(t => t.BulletinMasterId == masterid).Select().ToList();
                if (dbDetailData != null)
                {
                    foreach (var item in dbDetailData)
                    {
                        _bulletinDetailRepository.Delete(item);
                    }
                }
                if (dbData != null)
                {
                    foreach (var item in dbData)
                    {
                        _bulletinProcessRepository.Delete(item);
                    }
                }

                DeleteGraph(from_db);
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
            BulletinDetail from_db = null;
            var flag = false;
            try
            {
                //master
                DelDetail(detailid, out from_db);
                _bulletinedetailservice.Delete(from_db);

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

        private string PK => "BM" + _pkGeneratorService.GetAutoNumber(nameof(BulletinMaster), PKGeneratorEnum.Auto, null, DateTime.Now);

        public IEnumerable<object> GetBulletinMasterList(string masterid)
        {
            try
            {
                var sql = @"SELECT m.Id,m.CompanyGroupId, m.CompanyId, m.Description, m.Sequence, m.WorkingHour FROM TRN.[BulletinMaster] AS m WHERE m.Id='" + masterid + @"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataSet GetBulletinMasterList(string pk, string BulletinMasterId, string zoneid, string componentid, string operationid)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT m.Id
                                    FROM [TRN].[BulletinDetail] AS m
                                    WHERE   m.Id<>'" + pk + @"'
                                            and m.ZoneId='" + zoneid + @"'
                                            and m.componentid='" + componentid + @"'
                                            and m.operationid='" + operationid + @"'
                                            and m.BulletinMasterId='" + BulletinMasterId + @"'
                                            and m.Companygroupid='" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetBulletinMasterList()
        {
            try
            {
                var _sql = "select * from [TRN].[BulletinMaster] ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public BulletinMaster GetBulletinMaster(string PK)//TBT
        {
            try
            {
                var _sql = "select * from [TRN].[BulletinMaster] where Id='" + PK + "'";
                return _bulletinemasterrepository.SelectQuery(_sql).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public BulletinDetail GetBulletinDetail(string PK)
        {
            try
            {
                var _sql = "select * from [TRN].[BulletinDetail] where Id='" + PK + "' ";
                return _sqlRepository.GetModelCollection<BulletinDetail>(_sql).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetBulletinDetailList(string companyGroupId, string masterId, string processId)
        {
            try
            {
                return _bulletinedetailservice.GetList(companyGroupId, masterId, processId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private ICollection<BulletinDetail> GetChild(string masterId)
        {
            try
            {
                var _sql = "select * from [TRN].[BulletinDetail] where BulletinMasterId='" + masterId + "'";
                return _bulletinemasterrepository.SqlQuery<BulletinDetail>(_sql).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetBuyerList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                parameters.CmdText = @"SELECT
                                            p.Id,
                                            p.Code,
                                            p.UserName

		                            FROM HKP.[Buyer] AS p left join
		                            (select * from HKP.[CompanyGroupBuyer] where CompanyGroupId='" + identity.CompanyGroupId + @"') c  ON p.Id=c.BuyerId
		                            WHERE   p.Active=1";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataSet XlsReportData(string masterid)
        {
            GridParameter parameters = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"

                                        SELECT bd.[Sequence] SerialNo
	                                        ,z.[UserName] AS [Zone]
	                                        ,c.[UserName] AS Componant
	                                        ,op.[UserName] AS FinalOperationName
	                                        ,mt.Code AS NewCode
	                                        ,'' ActivityCode
	                                        ,'' OperationExecutionType
	                                        ,'' PressorFoot
	                                        ,'' Folder
	                                        ,'' Attachment
	                                        ,bd.MachineExecutiontype AS MachineManual
	                                        ,bd.Remark Remarks
	                                        ,op.Code OperationNo
	                                        ,bd.UserDefinedSPT AS CurrentSMV
	                                        ,bd.AllotedWorkstation AllowtedWorkStation
	                                        ,bd.AllotedManpower AllowtedManpower
                                        FROM [TRN].[BulletinMaster] bm
                                        LEFT JOIN [TRN].[BulletinDetail] bd ON bm.ID = bd.BulletinMasterId
                                        LEFT JOIN [HKP].[FGComponent] c ON bd.ComponentID = c.ID
                                        LEFT JOIN [HKP].[FGZone] z ON bd.ZoneID = z.ID
                                        LEFT JOIN [MSt].[Operation] op ON bd.OperationID = op.ID
                                        LEFT JOIN [MST].[MaterialMasterMachineProcess] mt ON bd.MachineTypeID = mt.ID
                                        --LEFT OUTER JOIN mst.MachineExecutionType ON bd.FGMachineExecutionTypeSystemID= FGMachineExecutionType.SystemID
                                        WHERE bd.BulletinMasterID = '" + masterid + @"'
                                        "
                };
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataSet SP_ONE(string masterid)
        {
            GridParameter parameters = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"
                            SELECT BM.ID
	                            ,BM.Description StylePlanDesc
	                            --,(ISNULL(BM.WorkingHour, 0) * ISNULL(BM.WORKINGHOUR, 0)) AS PLANNEDTARGET
	                            ,BC.SMV
	                            ,BC.MP
	                            --,((ISNULL(BM.WorkingHour, 0) * ISNULL(BM.WORKINGHOUR, 0)) / BC.MP) AS PcsPerMP
	                            ,((60 * ISNULL(BM.WORKINGHOUR, 0) * BC.MP) / BC.SMV) AS FullTarget
	                            ,(60 * ISNULL(BM.WORKINGHOUR, 0)) AS WorkedMinutes
	                            ,((BM.WorkingHour * BM.WORKINGHOUR) / ((60 * BM.WORKINGHOUR * BC.MP) / BC.SMV)) AS PlannedEff
	                            ,'' PRODUCTIONREFID
	                            ,'' DISPLAYPROREFID
	                            ,'' PRODUCTIONREFNO
	                            ,'' FILEREFCOLLECTION
	                            ,mm.CODE OURSTYLEDESC
	                            ,'' AS BUYERSTYLE
	                            ,'' CONTACTNAME
	                            ,b.CODE SHORTNAME
                            FROM [TRN].[BulletinMaster] AS BM
                            LEFT JOIN [MST].[MaterialMaster] mm ON BM.MaterialMasterId = mm.ID
                            --LEFT OUTER JOIN [HKP].[OurStyle] ON BM.OurStyleID=OurStyle.ID
                            LEFT JOIN (	SELECT * FROM [HKP].[Party]) b ON BM.BuyerID = b.ID
                            INNER JOIN (
	                            SELECT BulletinMasterID		,SUM(ISNULL(UserDefinedSPT, 0)) AS SMV		,SUM(ISNULL(AllotedManpower, 0)) AS MP	FROM [TRN].[BulletinDetail]

                                GROUP BY BulletinMasterID
	                            ) AS BC ON BC.BulletinMasterID = BM.ID
                            WHERE BM.ID = '" + masterid + @"'"
                };
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataSet SP_TWO(string masterid)
        {
            GridParameter parameters = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"
                                SELECT isnull(MCH.Code, '') AS CODE
	                                ,isnull(MachineClass.UserName, '') AS CLASS
	                                ,'' SUBCLASS
	                                ,'' ACTIVITYCODE
	                                ,SUM(ISNULL(BC.AllotedWorkstation, 0)) AS ALLOWTEDWORKSTATION
	                                ,SUM(ISNULL(BC.AllotedManpower, 0)) AS ALLOWTEDMANPOWER
                                FROM [TRN].[BulletinMaster] AS BM
                                INNER JOIN (select * from [TRN].[BulletinDetail] ) AS BC ON BC.BulletinMasterID = BM.ID
                                INNER JOIN [MSt].[Operation] ON BC.OperationID = BC.OperationID
                                LEFT JOIN [MST].[MaterialMasterMachineProcess] AS MCH ON BC.MachineTypeID = MCH.ID
                                LEFT JOIN [HKP].[MachineClass] ON MCH.MachineClassID = MachineClass.ID
                                WHERE BM.ID = '" + masterid + @"'
                                GROUP BY MCH.Code
	                                ,MachineClass.UserName"
                };
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetWorkBook(out ExcelEngine excelEngine, string masterid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var listReportData = XlsReportData(masterid);
            var dsSPONE = SP_ONE(masterid);
            var dsSPTWO = SP_TWO(masterid);
            excelEngine = new ExcelEngine();
            var workbook = GenXlsReport(ref excelEngine, masterid, listReportData, dsSPONE, dsSPTWO);
            return workbook;
        }

        public static IWorkbook GenXlsReport(ref ExcelEngine excelEngine, string SystemID, DataSet listReportData, DataSet dsProdRef, DataSet dsFooterData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            #region Declaration

            var _SystemID = SystemID;
            //cStaticInfo xobjStatic = null;
            //cBulletin objStyleWithBulletin = null;
            DataView dvSWP = null;
            DataView dvClass = null;
            DataView dvSubClass = null;
            DataView dvActivity = null;
            DataView dvZone = null;
            DataView dvPart = null;
            DataTable dtSum = null;

            var xlsRow = 1;
            var xlsCol = 1;
            var endXlsCol = 1;
            var tmpXlsRow = 1;
            var startXlsRow = 1;
            var xlsColumnHeader = 1;
            var zoneStartXlsRow = 1;
            var zoneXlsCol = 1;
            var WSCol = 1;
            var WPCol = 1;
            var xlsClassRow = 1;
            var xlsSubClassRow = 1;
            var xlsActivityRow = 1;
            var topSummaryHeadRow = 1;
            var shet2EndxlsCol = 1;

            excelEngine = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            IWorksheet sheet2 = null;
            ReportUtility oRU = null;
            var NumberFormatString = "#,##0.00;(#,##0.00)";

            StringCollection strCollZone = null;
            StringCollection strCollPart = null;

            StringCollection strCollClass = null;
            StringCollection strCollSubClass = null;
            StringCollection strCollActivityCode = null;

            #endregion Declaration

            try
            {
                // Validation

                oRU = new ReportUtility();
                ///All DataSet
                using (
                ///All DataSet
                dvSWP = new DataView
                {
                    Table = listReportData.Tables[0]
                })
                {
                    #region Xls Sheet

                    workbook = oRU.GetWorkbook(ref excelEngine, 2);
                    //excelEngine = new ExcelEngine();
                    //application = excelEngine.Excel;
                    //workbook = application.Workbooks.Create(3);
                    sheet = workbook.Worksheets[0];
                    sheet.Name = "Production Bulletine1";
                    sheet2 = workbook.Worksheets[1];
                    sheet2.Name = "Production Bulletine2";

                    #region Prod Ref Data

                    if (dsProdRef.Tables[0].Rows.Count > 0)
                    {
                        xlsRow = 5;
                        tmpXlsRow = xlsRow;
                        xlsCol = 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Bulletin Desc";
                        sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = dsProdRef.Tables[0].Rows[0]["StylePlanDesc"].ToString();
                        sheet.Range[xlsRow, (xlsCol - 1), xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
                        sheet.Range[xlsRow, (xlsCol - 1), xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    }

                    #endregion Prod Ref Data

                    #region Top Summary Data

                    //Column
                    xlsRow = xlsRow + 2;
                    topSummaryHeadRow = xlsRow;
                    xlsCol = 2;
                    sheet.Range[xlsRow, (xlsCol - 1)].Text = "For The Sketch";
                    sheet.Range[oRU.GetColumnNameForXls(xlsCol - 1) + xlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].Merge();
                    xlsCol = xlsCol + 2;
                    sheet.Range[xlsRow, (xlsCol - 1)].Text = "Part Description";
                    sheet.Range[oRU.GetColumnNameForXls(xlsCol - 1) + xlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].Merge();
                    xlsCol = xlsCol + 1;
                    sheet.Range[xlsRow, xlsCol].Text = "SMV";
                    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    xlsCol = xlsCol + 1;
                    sheet.Range[xlsRow, xlsCol].Text = "MO";
                    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    xlsCol = xlsCol + 1;
                    sheet.Range[xlsRow, xlsCol].Text = "HLP";
                    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    xlsCol = xlsCol + 1;
                    sheet.Range[xlsRow, xlsCol].Text = "IRON";
                    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    xlsCol = xlsCol + 1;
                    sheet.Range[xlsRow, xlsCol].Text = "TotalMP";
                    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    xlsCol = xlsCol + 1;
                    sheet.Range[xlsRow, xlsCol].Text = "Total Work Station";
                    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range["A" + xlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + "" + xlsRow].CellStyle.Font.Bold = true;
                    sheet.Range["A" + xlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + "" + xlsRow].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                    sheet.Range["A" + xlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + "" + xlsRow].BorderAround(ExcelLineStyle.Thin);
                    sheet.Range["A" + xlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + "" + xlsRow].BorderInside(ExcelLineStyle.Thin);

                    strCollZone = new StringCollection();
                    using (dvZone = new DataView
                    {
                        Table = dvSWP.ToTable()
                    })
                    {
                        tmpXlsRow = xlsRow + 1;
                        for (int dvSWPCount = 0; dvSWPCount < dvSWP.Count; dvSWPCount++)
                        {
                            if (!strCollZone.Contains(dvSWP[dvSWPCount]["Zone"].ToString()))
                            {
                                strCollZone.Add(dvSWP[dvSWPCount]["Zone"].ToString());
                                dvZone.RowFilter = "Zone='" + dvSWP[dvSWPCount]["Zone"]+ "'";
                                using (dvPart = new DataView
                                {
                                    Table = dvZone.ToTable()
                                })
                                {
                                    strCollPart = new StringCollection();

                                    xlsRow = xlsRow + 1;
                                    xlsCol = 3;
                                    sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPCount]["Zone"].ToString();//Printing Zone
                                    zoneStartXlsRow = xlsRow;
                                    zoneXlsCol = xlsCol;

                                    for (int ZoneCount = 0; ZoneCount < dvZone.Count; ZoneCount++)
                                    {
                                        if (!strCollPart.Contains(dvZone[ZoneCount]["Componant"].ToString()))
                                        {
                                            strCollPart.Add(dvZone[ZoneCount]["Componant"].ToString());
                                            dvPart.RowFilter = "Componant='" + dvZone[ZoneCount]["Componant"]+ "'";

                                            if (strCollPart.Count > 1)
                                            {
                                                xlsRow = xlsRow + 1;
                                            }
                                            xlsCol = 4;
                                            sheet.Range[xlsRow, xlsCol].Text = dvZone[ZoneCount]["Componant"].ToString();//Printing Componant

                                            dtSum = new DataTable();
                                            dtSum = dvPart.ToTable();

                                            var SMV = dtSum.Compute("SUM(CurrentSMV)", "");
                                            xlsCol = xlsCol + 1;
                                            sheet.Range[xlsRow, xlsCol].Number = Convert.ToDouble(SMV);
                                            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;

                                            double MO = 0, HLP = 0, IRON = 0;
                                            for (int dtSumCount = 0; dtSumCount < dtSum.Rows.Count; dtSumCount++)
                                            {
                                                if (dtSum.Rows[dtSumCount]["OperationExecutionType"].ToString().Trim().ToUpper() == "M")
                                                {
                                                    MO = MO + Convert.ToDouble(dtSum.Rows[dtSumCount]["AllowtedManpower"].ToString().Trim());
                                                }
                                                else if (dtSum.Rows[dtSumCount]["OperationExecutionType"].ToString().Trim().ToUpper() == "H")
                                                {
                                                    HLP = HLP + Convert.ToDouble(dtSum.Rows[dtSumCount]["AllowtedManpower"].ToString().Trim());
                                                }
                                                else if (dtSum.Rows[dtSumCount]["OperationExecutionType"].ToString().Trim().ToUpper() == "I")
                                                {
                                                    IRON = IRON + Convert.ToDouble(dtSum.Rows[dtSumCount]["AllowtedManpower"].ToString().Trim());
                                                }
                                            }

                                            xlsCol = xlsCol + 1;
                                            sheet.Range[xlsRow, xlsCol].Number = MO;//MO
                                            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                                            xlsCol = xlsCol + 1;
                                            sheet.Range[xlsRow, xlsCol].Number = HLP;//HLP
                                            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                                            xlsCol = xlsCol + 1;
                                            sheet.Range[xlsRow, xlsCol].Number = IRON;//IRON
                                            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;

                                            xlsCol = xlsCol + 1;
                                            sheet.Range[xlsRow, xlsCol].Formula = "=SUM(" + oRU.GetColumnNameForXls((xlsCol - 3)) + xlsRow + ":" + oRU.GetColumnNameForXls((xlsCol - 1)) + xlsRow + ")";
                                            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;

                                            var TotalWorkStation = dtSum.Compute("SUM(AllowtedWorkStation)", "");
                                            xlsCol = xlsCol + 1;
                                            sheet.Range[xlsRow, xlsCol].Number = Convert.ToDouble(TotalWorkStation);
                                            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                                        }
                                    }

                                    //Merge Zone
                                    sheet.Range[oRU.GetColumnNameForXls(zoneXlsCol) + zoneStartXlsRow + ":" + oRU.GetColumnNameForXls(zoneXlsCol) + xlsRow].Merge();
                                }
                            }
                        }

                        //Total
                        xlsRow = xlsRow + 1;
                        xlsCol = 3;
                        sheet.Range[xlsRow, xlsCol].Text = "Total";

                        xlsCol = xlsCol + 2;
                        sheet.Range[xlsRow, xlsCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(xlsCol) + tmpXlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ")";
                        sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(xlsCol) + tmpXlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ")";
                        sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(xlsCol) + tmpXlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ")";
                        sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(xlsCol) + tmpXlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ")";
                        sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(xlsCol) + tmpXlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ")";
                        sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(xlsCol) + tmpXlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + (xlsRow - 1) + ")";
                        sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;

                        sheet.Range["C" + xlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + "" + xlsRow].CellStyle.Font.Bold = true;
                        sheet.Range["A" + tmpXlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + "" + xlsRow].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                        sheet.Range["C" + tmpXlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + "" + xlsRow].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range["C" + tmpXlsRow + ":" + oRU.GetColumnNameForXls(xlsCol) + "" + xlsRow].BorderInside(ExcelLineStyle.Hair);

                        //Border For Sketch
                        sheet.Range["A" + tmpXlsRow + ":" + "B" + "" + xlsRow].BorderAround(ExcelLineStyle.Hair);

                        #endregion Top Summary Data

                        #region Bulletine Summary

                        tmpXlsRow = topSummaryHeadRow;
                        xlsCol = xlsCol + 3;
                        sheet.Range[tmpXlsRow, xlsCol].Text = "Bulletine Summary";
                        sheet.Range[oRU.GetColumnNameForXls(xlsCol) + tmpXlsRow + ":" + oRU.GetColumnNameForXls(xlsCol + 1) + tmpXlsRow].Merge();
                        sheet.Range[oRU.GetColumnNameForXls(xlsCol) + tmpXlsRow + ":" + oRU.GetColumnNameForXls(xlsCol + 1) + tmpXlsRow].CellStyle.Font.Bold = true;
                        sheet.Range[oRU.GetColumnNameForXls(xlsCol) + tmpXlsRow + ":" + oRU.GetColumnNameForXls(xlsCol + 1) + tmpXlsRow].BorderAround(ExcelLineStyle.Thin);

                        tmpXlsRow = tmpXlsRow + 1;
                        sheet.Range[tmpXlsRow, xlsCol].Text = "100% Target";
                        sheet.Range[tmpXlsRow, xlsCol].CellStyle.Font.Bold = true;
                        double fulltarget = 0;
                        if (dsProdRef.Tables[0].Rows.Count > 0)
                        {
                            fulltarget = Convert.ToDouble(dsProdRef.Tables[0].Rows[0]["FullTarget"].ToString());
                        }
                        sheet.Range[tmpXlsRow, (xlsCol + 1)].Number = fulltarget;
                        sheet.Range[tmpXlsRow, (xlsCol + 1)].NumberFormat = NumberFormatString;

                        tmpXlsRow = tmpXlsRow + 1;
                        sheet.Range[tmpXlsRow, xlsCol].Text = "Total SMV";
                        sheet.Range[tmpXlsRow, xlsCol].CellStyle.Font.Bold = true;
                        double totalsmv = 0;
                        if (dsProdRef.Tables[0].Rows.Count > 0)
                        {
                            totalsmv = Convert.ToDouble(dsProdRef.Tables[0].Rows[0]["SMV"].ToString());
                        }
                        sheet.Range[tmpXlsRow, (xlsCol + 1)].Number = totalsmv;
                        sheet.Range[tmpXlsRow, (xlsCol + 1)].NumberFormat = NumberFormatString;

                        //tmpXlsRow = tmpXlsRow + 1;
                        //sheet.Range[tmpXlsRow, xlsCol].Text = "Planned Eff%";
                        //sheet.Range[tmpXlsRow, xlsCol].CellStyle.Font.Bold = true;
                        //sheet.Range[tmpXlsRow, (xlsCol + 1)].Text = (Convert.ToDouble(dsProdRef.Tables[0].Rows[0]["PlannedEff"].ToString()) * 100).ToString("F2") + "%";
                        //sheet.Range[tmpXlsRow, (xlsCol + 1)].HorizontalAlignment = ExcelHAlign.HAlignRight;

                        tmpXlsRow = tmpXlsRow + 1;
                        sheet.Range[tmpXlsRow, xlsCol].Text = "Worked Minutes";
                        sheet.Range[tmpXlsRow, xlsCol].CellStyle.Font.Bold = true;
                        double WorkedMinutes = 0;
                        if (dsProdRef.Tables[0].Rows.Count > 0)
                        {
                            WorkedMinutes = Convert.ToDouble(dsProdRef.Tables[0].Rows[0][nameof(WorkedMinutes)].ToString());
                        }
                        sheet.Range[tmpXlsRow, (xlsCol + 1)].Number = WorkedMinutes;
                        sheet.Range[tmpXlsRow, (xlsCol + 1)].NumberFormat = NumberFormatString;

                        //tmpXlsRow = tmpXlsRow + 1;
                        //sheet.Range[tmpXlsRow, xlsCol].Text = "Planned Tgt";
                        //sheet.Range[tmpXlsRow, xlsCol].CellStyle.Font.Bold = true;
                        //sheet.Range[tmpXlsRow, (xlsCol + 1)].Number = Convert.ToDouble(dsProdRef.Tables[0].Rows[0]["PLANNEDTARGET"].ToString());
                        //sheet.Range[tmpXlsRow, (xlsCol + 1)].NumberFormat = NumberFormatString;

                        //tmpXlsRow = tmpXlsRow + 1;
                        //sheet.Range[tmpXlsRow, xlsCol].Text = "Pcs/MP";
                        //sheet.Range[tmpXlsRow, xlsCol].CellStyle.Font.Bold = true;
                        //sheet.Range[tmpXlsRow, (xlsCol + 1)].Number = Convert.ToDouble(dsProdRef.Tables[0].Rows[0]["PcsPerMP"].ToString());
                        //sheet.Range[tmpXlsRow, (xlsCol + 1)].NumberFormat = NumberFormatString;

                        sheet.Range[oRU.GetColumnNameForXls(xlsCol) + (topSummaryHeadRow + 1) + ":" + oRU.GetColumnNameForXls(xlsCol + 1) + tmpXlsRow].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[oRU.GetColumnNameForXls(xlsCol) + (topSummaryHeadRow + 1) + ":" + oRU.GetColumnNameForXls(xlsCol + 1) + tmpXlsRow].BorderInside(ExcelLineStyle.Hair);

                        #endregion Bulletine Summary

                        #region Column Header For Sheet

                        //xlsRow = 5;
                        if (tmpXlsRow > xlsRow)
                        {
                            xlsRow = tmpXlsRow + 3;
                        }
                        else
                        {
                            xlsRow = xlsRow + 3;
                        }
                        xlsColumnHeader = xlsRow;
                        xlsCol = 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Serial No";//Col 1
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Operation No";//Col 2
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Operation Execution Type";//Col 6
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Zone";//Col 3
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Componant";//Col 4
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Final Operation Name";//Col 5
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 30;

                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Machine Code";//Col 7
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 15;
                        //xlsCol = xlsCol + 1;
                        //sheet.Range[xlsRow, xlsCol].Text = "Activity Code";//Col 8
                        //sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Machine/Manual";//Col 6
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Current SMV";//Col 9
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        //xlsCol = xlsCol + 1;
                        //sheet.Range[xlsRow, xlsCol].Text = "Operator Req";//Col 10
                        //sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Alloted MP/Line";//Col 11
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Alloted WS/Line";//Col 12
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        //xlsCol = xlsCol + 1;
                        //sheet.Range[xlsRow, xlsCol].Text = "TGT/Hrs";//Col 13
                        //sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Pressure foots";//Col 14
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Folders";//Col 15
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Attachment";//Col 16
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Remarks";//Col 17
                        sheet.Range["" + oRU.GetColumnNameForXls(xlsCol) + xlsRow].ColumnWidth = 10;

                        endXlsCol = xlsCol;
                        //Formating
                        sheet.Range["A" + xlsRow + ":" + oRU.GetColumnNameForXls(endXlsCol) + "" + xlsRow].CellStyle.Font.Bold = true;
                        sheet.Range["A" + xlsRow + ":" + oRU.GetColumnNameForXls(endXlsCol) + "" + xlsRow].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                        sheet.Range["A" + xlsRow + ":" + oRU.GetColumnNameForXls(endXlsCol) + "" + xlsRow].CellStyle.Borders.LineStyle = ExcelLineStyle.Thin;
                        sheet.Range["A" + xlsRow + ":" + oRU.GetColumnNameForXls(endXlsCol) + "" + xlsRow].CellStyle.Borders[ExcelBordersIndex.DiagonalUp].ShowDiagonalLine = false;
                        sheet.Range["A" + xlsRow + ":" + oRU.GetColumnNameForXls(endXlsCol) + "" + xlsRow].CellStyle.Borders[ExcelBordersIndex.DiagonalDown].ShowDiagonalLine = false;

                        #endregion Column Header For Sheet

                        #region Sheet Data

                        for (int dvSWPRowCount = 0; dvSWPRowCount < dvSWP.Count; dvSWPRowCount++)
                        {
                            xlsRow = xlsRow + 1;

                            xlsCol = 1;
                            sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPRowCount]["SerialNo"].ToString();
                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPRowCount]["OperationNo"].ToString();
                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPRowCount]["OperationExecutionType"].ToString();
                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPRowCount]["Zone"].ToString();
                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPRowCount]["Componant"].ToString();
                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPRowCount]["FinalOperationName"].ToString();

                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPRowCount]["NewCode"].ToString();
                            //xlsCol = xlsCol + 1;
                            //sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPRowCount]["ActivityCode"].ToString();
                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPRowCount]["MachineManual"].ToString();
                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Number = Convert.ToDouble(dvSWP[dvSWPRowCount]["CurrentSMV"].ToString());
                            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                            //xlsCol = xlsCol + 1;
                            //sheet.Range[xlsRow, xlsCol].Number = Convert.ToDouble(dvSWP[dvSWPRowCount]["OperatorReq"].ToString());
                            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Number = Convert.ToDouble(dvSWP[dvSWPRowCount]["AllowtedManpower"].ToString());
                            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Number = Convert.ToDouble(dvSWP[dvSWPRowCount]["AllowtedWorkStation"].ToString());
                            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                            //xlsCol = xlsCol + 1;
                            //sheet.Range[xlsRow, xlsCol].Number = Convert.ToDouble(dvSWP[dvSWPRowCount]["OperationTargetPerHour"].ToString());
                            //sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPRowCount]["PressorFoot"].ToString();
                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPRowCount]["Folder"].ToString();
                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPRowCount]["Attachment"].ToString();
                            xlsCol = xlsCol + 1;
                            sheet.Range[xlsRow, xlsCol].Text = dvSWP[dvSWPRowCount]["Remarks"].ToString();

                            //Border
                            sheet.Range["A" + xlsRow + ":" + oRU.GetColumnNameForXls(endXlsCol) + "" + xlsRow].BorderInside(ExcelLineStyle.Hair);
                            sheet.Range["A" + xlsRow + ":" + oRU.GetColumnNameForXls(endXlsCol) + "" + xlsRow].BorderAround(ExcelLineStyle.Hair);
                        }

                        #endregion Sheet Data

                        #region Sheet2 Data

                        //Column Header
                        //xlsRow = xlsRow + 2;
                        xlsRow = 5;
                        startXlsRow = xlsRow;
                        xlsCol = 1;
                        sheet2.Range[xlsRow, xlsCol].Text = "Machine Class";
                        sheet2.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        xlsCol = xlsCol + 1;
                        sheet2.Range[xlsRow, xlsCol].Text = "Machine Sub Class";
                        sheet2.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        xlsCol = xlsCol + 1;
                        sheet2.Range[xlsRow, xlsCol].Text = "Activity Code";
                        sheet2.Range[xlsRow, xlsCol].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet2.Range[xlsRow, xlsCol].Text = "Machine Code";
                        sheet2.Range[xlsRow, xlsCol].ColumnWidth = 25;
                        xlsCol = xlsCol + 1;
                        sheet2.Range[xlsRow, xlsCol].Text = "Alloted WS/Line";
                        sheet2.Range[xlsRow, xlsCol].ColumnWidth = 10;
                        xlsCol = xlsCol + 1;
                        sheet2.Range[xlsRow, xlsCol].Text = "Alloted WP/Line";
                        sheet2.Range[xlsRow, xlsCol].ColumnWidth = 10;
                        shet2EndxlsCol = xlsCol;

                        sheet2.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                        sheet2.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
                        sheet2.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Thin);

                        strCollClass = new StringCollection();
                        using (dvClass = new DataView
                        {
                            Table = dsFooterData.Tables[0]
                        })
                        {
                            tmpXlsRow = xlsRow + 1;
                            for (int footerCount = 0; footerCount < dsFooterData.Tables[0].Rows.Count; footerCount++)
                            {
                                if (!strCollClass.Contains(dsFooterData.Tables[0].Rows[footerCount]["CLASS"].ToString()))
                                {
                                    strCollClass.Add(dsFooterData.Tables[0].Rows[footerCount]["CLASS"].ToString());

                                    dvClass.RowFilter = "CLASS='" + dsFooterData.Tables[0].Rows[footerCount]["CLASS"]+ "'";
                                    using (dvSubClass = new DataView
                                    {
                                        Table = dvClass.ToTable()
                                    })
                                    {
                                        strCollSubClass = new StringCollection();

                                        xlsRow = xlsRow + 1;
                                        xlsCol = 1;
                                        xlsClassRow = xlsRow;
                                        sheet2.Range[xlsRow, xlsCol].Text = dsFooterData.Tables[0].Rows[footerCount]["CLASS"].ToString();

                                        for (int classCount = 0; classCount < dvClass.Count; classCount++)
                                        {
                                            #region inner

                                            if (!strCollSubClass.Contains(dvClass[classCount]["SUBCLASS"].ToString()))
                                            {
                                                strCollSubClass.Add(dvClass[classCount]["SUBCLASS"].ToString());

                                                dvSubClass.RowFilter = "SUBCLASS='" + dvClass[classCount]["SUBCLASS"]+ "'";
                                                using (dvActivity = new DataView
                                                {
                                                    Table = dvSubClass.ToTable()
                                                })
                                                {
                                                    strCollActivityCode = new StringCollection();

                                                    if (strCollSubClass.Count > 1)
                                                    {
                                                        xlsRow = xlsRow + 1;
                                                    }
                                                    xlsCol = 2;
                                                    xlsSubClassRow = xlsRow;
                                                    sheet2.Range[xlsRow, xlsCol].Text = dvClass[classCount]["SUBCLASS"].ToString();

                                                    for (int activityCount = 0; activityCount < dvSubClass.Count; activityCount++)
                                                    {
                                                        if (!strCollActivityCode.Contains(dvSubClass[activityCount]["ACTIVITYCODE"].ToString()))
                                                        {
                                                            strCollActivityCode.Add(dvSubClass[activityCount]["ACTIVITYCODE"].ToString());
                                                            dvActivity.RowFilter = "ACTIVITYCODE='" + dvSubClass[activityCount]["ACTIVITYCODE"]+ "'";

                                                            if (strCollActivityCode.Count > 1)
                                                            {
                                                                xlsRow = xlsRow + 1;
                                                            }
                                                            xlsCol = 3;
                                                            xlsActivityRow = xlsRow;
                                                            sheet2.Range[xlsRow, xlsCol].Text = dvSubClass[activityCount]["ACTIVITYCODE"].ToString();

                                                            for (int codeCount = 0; codeCount < dvActivity.Count; codeCount++)
                                                            {
                                                                if (codeCount > 0)
                                                                {
                                                                    xlsRow = xlsRow + 1;
                                                                }
                                                                xlsCol = 4;
                                                                sheet2.Range[xlsRow, xlsCol].Text = dvActivity[codeCount]["CODE"].ToString();
                                                                xlsCol = xlsCol + 1;
                                                                WSCol = xlsCol;
                                                                sheet2.Range[xlsRow, xlsCol].Number = Convert.ToDouble(dvActivity[codeCount]["ALLOWTEDWORKSTATION"].ToString().Trim());
                                                                sheet2.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                                                                xlsCol = xlsCol + 1;
                                                                WPCol = xlsCol;
                                                                sheet2.Range[xlsRow, xlsCol].Number = Convert.ToDouble(dvActivity[codeCount]["ALLOWTEDMANPOWER"].ToString().Trim());
                                                                sheet2.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
                                                            }

                                                            //Merge Activity
                                                            sheet2.Range[oRU.GetColumnNameForXls(3) + xlsActivityRow + ":" + oRU.GetColumnNameForXls(3) + xlsRow].Merge();
                                                        }
                                                    }

                                                    //Merge Sub Class
                                                    sheet2.Range[oRU.GetColumnNameForXls(2) + xlsSubClassRow + ":" + oRU.GetColumnNameForXls(2) + xlsRow].Merge();
                                                }
                                            }

                                            #endregion inner
                                        }

                                        //Merge Class
                                        sheet2.Range[oRU.GetColumnNameForXls(1) + xlsClassRow + ":" + oRU.GetColumnNameForXls(1) + xlsRow].Merge();
                                    }
                                }
                            }

                            //Grand Total
                            xlsRow = xlsRow + 1;
                            //xlsCol=1;
                            sheet2.Range[xlsRow, 1].Text = "Grand Total";
                            sheet2.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                            sheet2.Range[xlsRow, WSCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(WSCol) + tmpXlsRow + ":" + oRU.GetColumnNameForXls(WSCol) + (xlsRow - 1) + ")";
                            sheet2.Range[xlsRow, WSCol].NumberFormat = NumberFormatString;
                            sheet2.Range[xlsRow, WSCol].CellStyle.Font.Bold = true;
                            sheet2.Range[xlsRow, WPCol].Formula = "=SUM(" + oRU.GetColumnNameForXls(WPCol) + tmpXlsRow + ":" + oRU.GetColumnNameForXls(WPCol) + (xlsRow - 1) + ")";
                            sheet2.Range[xlsRow, WPCol].NumberFormat = NumberFormatString;
                            sheet2.Range[xlsRow, WPCol].CellStyle.Font.Bold = true;

                            sheet2.Range[startXlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
                            sheet2.Range[startXlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);

                            #endregion Sheet2 Data

                            #region UsedRange Alignment

                            sheet.UsedRange.WrapText = true;
                            sheet.UsedRange.CellStyle.Font.Size = 8;
                            sheet.UsedRange.CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;

                            sheet2.UsedRange.WrapText = true;
                            sheet2.UsedRange.CellStyle.Font.Size = 8;
                            sheet2.UsedRange.CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;

                            #endregion UsedRange Alignment

                            oRU.CompanyHeader(ref sheet, endXlsCol, "Production Bulletine", identity.CompanyId);
                            oRU.CompanyHeader(ref sheet2, shet2EndxlsCol, "Production Bulletine", identity.CompanyId);

                            #region Report Heading

                            //Report Heading
                            // GetFactoryAddInfo(customidentity.CompanyId, out dsHeading);
                            //if (dsHeading != null && dsHeading.Tables[0].Rows.Count > 0)
                            //{
                            //sheet.Range["A1"].RowHeight = 25;
                            //sheet.Range["A1"].CellStyle.Font.Size = 14;
                            //sheet.Range["A1" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "1"].Merge();
                            //sheet.Range["A1" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            //sheet.Range["A1" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "1"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                            //sheet.Range["A1" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "1"].CellStyle.Font.Bold = true;
                            //sheet.Range["A1"].Text = dsHeading.Tables[0].Rows[0]["CompanyName"].ToString();
                            //sheet.Range["A2"].RowHeight = 15;
                            //sheet.Range["A2"].CellStyle.Font.Size = 10;
                            //sheet.Range["A2" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "2"].Merge();
                            //sheet.Range["A2" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            //sheet.Range["A2" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "2"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                            //sheet.Range["A2"].Text = dsHeading.Tables[0].Rows[0]["Address1"].ToString();
                            //sheet.Range["A3"].RowHeight = 15;
                            //sheet.Range["A3"].CellStyle.Font.Size = 10;
                            //sheet.Range["A3" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "3"].Merge();
                            //sheet.Range["A3" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            //sheet.Range["A3" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                            //sheet.Range["A3" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "3"].CellStyle.Font.Bold = true;
                            //sheet.Range["A3"].Text = "Production Bulletine";
                            //End ReportHeading

                            //Report Heading
                            //sheet2.Range["A1"].RowHeight = 25;
                            //sheet2.Range["A1"].CellStyle.Font.Size = 14;
                            //sheet2.Range["A1" + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + "1"].Merge();
                            //sheet2.Range["A1" + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + "1"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            //sheet2.Range["A1" + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + "1"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                            //sheet2.Range["A1" + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + "1"].CellStyle.Font.Bold = true;
                            //sheet2.Range["A1"].Text = dsHeading.Tables[0].Rows[0]["CompanyName"].ToString();
                            //sheet2.Range["A2"].RowHeight = 15;
                            //sheet2.Range["A2"].CellStyle.Font.Size = 10;
                            //sheet2.Range["A2" + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + "2"].Merge();
                            //sheet2.Range["A2" + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + "2"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            //sheet2.Range["A2" + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + "2"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                            //sheet2.Range["A2"].Text = dsHeading.Tables[0].Rows[0]["Address1"].ToString();
                            //sheet2.Range["A3"].RowHeight = 15;
                            //sheet2.Range["A3"].CellStyle.Font.Size = 10;
                            //sheet2.Range["A3" + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + "3"].Merge();
                            //sheet2.Range["A3" + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + "3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            //sheet2.Range["A3" + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + "3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                            //sheet2.Range["A3" + ":" + oRU.GetColumnNameForXls(shet2EndxlsCol) + "3"].CellStyle.Font.Bold = true;
                            //sheet2.Range["A3"].Text = "Production Bulletine";
                            //}

                            #endregion Report Heading

                            #region Freeze Panes

                            //Split and Freeze Panes
                            //sheet.UsedRange["A" + (xlsColumnHeader + 1)].FreezePanes();
                            //sheet.FirstVisibleColumn = 1;
                            //sheet.FirstVisibleRow = xlsColumnHeader;
                            ////End Split and Freeze Panes

                            ////Split and Freeze Panes
                            //sheet2.UsedRange["A6"].FreezePanes();
                            //sheet2.FirstVisibleColumn = 1;
                            //sheet2.FirstVisibleRow = 5;
                            //End Split and Freeze Panes

                            #endregion Freeze Panes

                            oRU.PageSetup(ref sheet, xlsColumnHeader, ExcelPageOrientation.Landscape);
                            oRU.PageSetup(ref sheet2, 5, ExcelPageOrientation.Portrait);
                            //PageSetup(ref sheet, customidentity.UserId, xlsColumnHeader, ExcelPageOrientation.Landscape);
                            //PageSetup(ref sheet, customidentity.UserId, 5, ExcelPageOrientation.Portrait);

                            #region Page Setup

                            //Setting Page Setup
                            //sheet.PageSetup.TopMargin = 1;
                            //sheet.PageSetup.BottomMargin = 1;
                            //sheet.PageSetup.PrintTitleRows = "$" + xlsColumnHeader + ":$" + xlsColumnHeader + "";
                            //sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                            //sheet.PageSetup.RightFooter = "&p";
                            //sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + customidentity.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                            //sheet.PageSetup.LeftMargin = 0.2;
                            //sheet.PageSetup.RightMargin = 0.2;
                            //sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                            //sheet.PageSetup.FitToPagesTall = 0;
                            //sheet.PageSetup.FitToPagesWide = 1;
                            //sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                            //sheet.PageSetup.PrintTitleRows = "$1:$2";
                            //sheet.PageSetup.

                            //Setting Page Setup
                            //sheet2.PageSetup.TopMargin = 1;
                            //sheet2.PageSetup.BottomMargin = 1;
                            //sheet2.PageSetup.PrintTitleRows = "$" + 5 + ":$" + 5 + "";
                            //sheet2.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                            ////sheet.PageSetup.RightFooter = "&p";
                            //sheet2.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + customidentity.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                            //sheet2.PageSetup.LeftMargin = 0.2;
                            //sheet2.PageSetup.RightMargin = 0.2;
                            //sheet2.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                            //sheet2.PageSetup.FitToPagesTall = 0;
                            //sheet2.PageSetup.FitToPagesWide = 1;
                            //sheet2.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                            //sheet2.PageSetup.CenterHorizontally = true;
                            //sheet.PageSetup.PrintTitleRows = "$1:$2";
                            //sheet.PageSetup.

                            #endregion Page Setup

                            //string xlsPassword = ConfigurationSettings.AppSettings["xlsPassword"].ToString();
                            //sheet.Protect(xlsPassword);
                            //sheet2.Protect(xlsPassword);

                            //workbook.Version = ExcelVersion.Excel97to2003;
                            //string strFileName = "Production Bulletine Report " + bplib.clsWebLib.DateData_DBToApp(System.DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + ".xls";
                            //workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, Response, ExcelDownloadType.PromptDialog);
                            //workbook.Close();
                            //excelEngine.Dispose();

                            #endregion Xls Sheet

                            workbook.Version = ExcelVersion.Excel2013;
                            return workbook;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                #region final

                strCollZone = null;
                strCollPart = null;
                strCollClass = null;
                strCollSubClass = null;
                strCollActivityCode = null;
                dvSWP = null;
                dvClass = null;
                dvSubClass = null;
                dvActivity = null;
                dsProdRef = null;
                dsFooterData = null;
                //excelEngine = null;
                //application = null;
                //workbook = null;
                //sheet = null;
                //sheet2 = null;

                #endregion final
            }
        }

        public static IWorkbook Test(ref ExcelEngine excelEngin, DataSet listReportData)
        {
            //excelEngin = new ExcelEngine();
            var application = excelEngin.Excel;
            var workbook = application.Workbooks.Create(1);
            var sheet = workbook.Worksheets[0];
            var _Row = 0;
            for (int i = 0; i < listReportData.Tables[0].Rows.Count; i++)
            {
                _Row += 1;
                sheet.Range["A" + _Row].Text = listReportData.Tables[0].Rows[0]["Zone"].ToString();
            }

            workbook.Version = ExcelVersion.Excel2013;
            return workbook;
        }

        #region Process

        public void InsertProcess(string masterId, IEnumerable<BulletinProcess> processList)
        {
            var flag = false;
            try
            {
                var pk = _pkGeneratorService.GetAutoNumber(nameof(BulletinProcess), PKGeneratorEnum.Auto, null, DateTime.Now);
                if (processList != null)
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    var count = 1;
                    foreach (var item in processList)
                    {
                        item.Id = pk + count;
                        item.BulletinId = masterId;
                        count++;
                        AuditService.AddedLog(item);
                        _bulletinProcessRepository.Insert(item);
                    }
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteProcess(string bulletinProcessId)
        {
            var flag = false;
            try
            {
                var dbData = _bulletinProcessRepository.Find(bulletinProcessId);
                var dbDetailData = _bulletinDetailRepository.Query(t => t.BulletinMasterId == dbData.BulletinId && t.ProcessId == dbData.ProcessId).Select().ToList();
                _unitOfWork.BeginTransaction();
                flag = true;
                if (dbDetailData != null && dbDetailData.Count > 0)
                {
                    foreach (var item in dbDetailData)
                    {
                        _bulletinDetailRepository.Delete(item);
                    }
                }
                _bulletinProcessRepository.Delete(dbData);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<object> GetBulletinProcessList(string masterId)
        {
            try
            {
                var _sql = @"SELECT BP.Id, BP.BulletinId, BP.ProcessId, P.Code, P.ShortName, P.StandardName, P.UserName
                                , MT.[Description] AS MaterialType, P.Active
                        FROM [TRN].[BulletinProcess] AS BP
                        JOIN [HKP].[Process] AS P ON BP.ProcessId=P.Id
                        LEFT JOIN HKP.MaterialType AS MT ON P.MaterialTypeId=MT.Id
                        WHERE BP.BulletinId='" + masterId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Process
    }
}