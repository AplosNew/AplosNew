#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.IE;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Machines;
using Library.Service.Organizations;
using Library.Service.Processes;
using Library.Service.Skills;
using Library.Service.Systems;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
#endregion Using

namespace Library.Service.IE
{
    public class OperationMasterService : Service<OperationMaster>, IOperationMasterService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IOperationActivityService _OperationActivityService;
        private readonly IOperationTypeService _OperationTypeService;
        private readonly IOperationCategoryService _OperationCategoryService;
        private readonly ISkillService _SkillService;
        private readonly IMachineMasterService _MachineMasterService;
        private readonly ILegalDesignationService _legalDesignationService;
        private readonly IProcessService _ProcessService;
        private readonly ISkillGroupingService _SkillGroupingService;





        public OperationMasterService(
            IRepositoryAsync<OperationMaster> OperationMasterRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IOperationActivityService OperationActivityService
            , IOperationTypeService OperationTypeService
            , IOperationCategoryService OperationCategoryService
            , ISkillService SkillService
            , IProcessService ProcessService
            , IMachineMasterService MachineMasterService
            , ILegalDesignationService legalDesignationService
            , ISkillGroupingService SkillGroupingService
            //, IOperationMasterService OperationMasterService
            , IUnitOfWork unitOfWork) :
            base(OperationMasterRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _OperationActivityService = OperationActivityService;
            _OperationTypeService = OperationTypeService;
            _OperationCategoryService = OperationCategoryService;
            _SkillService = SkillService;
            _MachineMasterService = MachineMasterService;
            _legalDesignationService = legalDesignationService;
            _ProcessService = ProcessService;
            _SkillGroupingService = SkillGroupingService;


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
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(OperationMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void Check(OperationMaster entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
            CheckUniqueColumn(UniqueColumnName.StandardName, entity.StandardName, r => r.Id != entity.Id && r.StandardName == entity.StandardName);
            //CheckUniqueColumn(UniqueColumnName.Sequence, entity.Sequence.ToString(), r => r.Id != entity.Id && r.Sequence == entity.Sequence);
        }

        //public override void Insert(SizeGroup entity)
        //{
        //    try
        //    {
        //        Check(entity);
        //        entity.Id ="SG-"+ GetPK();
        //        base.Insert(entity);
        //    }
        //    catch (Exception ex)
        //    {
        //        //throw new CustomException(ex.Message, ex,
        //        //Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
        //        //ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //        throw ex;
        //    }
        //}

        //public override void Update(SizeGroup entity)
        //{
        //    try
        //    {
        //        Check(entity);
        //        base.Update(entity);
        //    }
        //    catch (Exception ex)
        //    {
        //        //throw new CustomException(ex.Message, ex,
        //        //Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
        //        //ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //        throw ex;

        //    }
        //}

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM [HKP].[SizeGroup]";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCboCompanyGroup()
        {
            try
            {

                return from m in _OperationActivityService.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }



        public IEnumerable<object> GetCboOperationType()
        {
            try
            {

                return from m in _OperationTypeService.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        // OperationSkillService


        public IEnumerable<object> GetCboOperationCategory()
        {
            try
            {

                return from m in _OperationCategoryService.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCboSkill()
        {
            try
            {

                return from m in _SkillService.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id, SkillGroupId = m.SkillGroupId };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCboSkillCboByMachine(string Id)
        {
            try
            {
                var sql = @"select SK.id SkillId,SK.UserName from [HKP].[Skill] SK
                            Left Join [MST].[MachineMaster] MM On MM.skillId=Sk.Id 
                            where MM.Id='" + Id + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCboMachineMaster()
        {
            try
            {
                var sql = @"Select UserName As Text, Id As Value from mst.MachineMaster";
                return _sqlRepository.GetDataCollection(sql);

                //return from m in _MachineMasterService.Query().Select().OrderBy(r => r.UserName)
                //       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCboSkillGrouping()
        {
            try
            {

                var sql = @"Select Code +' - '+UserName As Text, Id As Value from [SCS].[SkillGrouping]";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetCbolegalDesignation()
        {
            try
            {

                return from m in _legalDesignationService.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetCboProcess()
        {
            try
            {

                return from m in _ProcessService.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetOperationMaster()
        {
            try
            {
                var sql = @"SELECT OM.*                                ,CG.StandardName AS CompanyGroup                                ,OA.UserName AS OperationActivity                                ,OT.UserName AS OperationType                                ,OC.UserName AS OperationCategory                                ,S.Id SkillId,S.UserName AS Skill                                ,MM.UserName AS MachineMaster                                ,SG.UserName AS SkillGroup                                ,LD.UserName AS LegalDesignation                                ,DG.UserName AS DesignationGroup                                ,p.UserName As Process                                From [MST].[OperationMaster] OM                                LEFT JOIN [ORG].CompanyGroup CG ON CG.Id=OM.CompanyGroupId                                LEFT JOIN [HKP].[OperationActivity] OA ON OA.Id=OM.OperationActivityId                                LEFT JOIN [HKP].[OperationType] OT ON OT.Id=OM.OperationTypeId                                LEFT JOIN [HKP].[OperationCategory] OC ON OC.Id=OM.OperationCategoryId                                LEFT JOIN [HKP].[Skill] S On S.Id=OM.SkillId                                LEFT JOIN [MST].[MachineMaster] MM ON MM.Id=OM.MachineMasterId                                LEFT JOIN [SCS].[SkillGrouping] SG ON SG.Id=OM.SkillGroupId                                LEFT JOIN [HKP].[DesignationGroup] DG ON DG.Id=OM.DesignationGroupId                                LEFT JOIN [HKP].[LegalDesignation] LD ON LD.Id=OM.LegalDesignationId                                LEFT JOIN [HKP].[Process] P ON P.Id=OM.ProcessId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public IEnumerable<object> GetDataByMasterOrderId(string id)
        {
            try
            {
                var sql = @"SELECT * from [MST].[OperationMaster] where Id='" + id + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetSkillMasterMachineData(string masterId)
        {
            try
            {
                string sql = @"SELECT SM.*,MGP.UserName AS MaterialGroupMasterName,MT.UserName MaterialTypeName,M.Code MaterialCode,M.UserName MaterialMasterName
, MMA.Code, MMA.StandardName,HSNCode = CASE WHEN HC.Code <> '' THEN ISNULL(HC.Code, NULL) ELSE ISNULL(MHC.Code, NULL) END,M.IsAsset,BPM.BusinessProcessName
      FROM dbo.SkillMasterMachine SM
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id = SM.ArticleId
LEFT JOIN[MST].[MaterialMaster] M ON M.Id = MMA.MaterialMasterId
LEFT JOIN[MST].[MaterialGroupMaster] AS MGP ON M.MaterialGroupMasterId = MGP.Id
LEFT JOIN[HKP].[MaterialType] AS MT ON MGP.MaterialTypeId = MT.Id
LEFT JOIN[HKP].[HSNCode] HC ON HC.id = MMA.HSNCodeId
LEFT JOIN[HKP].[HSNCode] MHC ON MHC.id = M.HSNCodeId
LEFT JOIN(SELECT distinct MBP.MaterialMasterId, BP.BusinessProcessName FROM [MST].[MaterialMasterBusinessProcess] AS MBP
JOIN[SCS].[BusinessProcess] AS BP ON MBP.BusinessProcessId = BP.Id WHERE BP.BusinessProcessName = 'MachineDefinition') BPM ON BPM.MaterialMasterId = M.Id
Where SM.SkillMasterId = '" + masterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Operation Master Reports


        public IWorkbook CreateOperationMasterReports(string companyId, string plantId)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new Helpers.ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 2);
                var sheet1 = workbook.Worksheets[0];
                var sheet2 = workbook.Worksheets[1];
                var Head = "Operation Master Reports";
                //Library.Service.IE.IWorksheet s1 = (Library.Service.IE.IWorksheet)sheet1;
                //Library.Service.IE.IWorksheet s2 = (Library.Service.IE.IWorksheet)sheet2;
                CreateOperationMasterReports(ref sheet1, ref sheet2, report, Head, "Summary", companyId, plantId);
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateOperationMasterReports(ref Syncfusion.XlsIO.IWorksheet sheet1, ref Syncfusion.XlsIO.IWorksheet sheet2, ReportUtility report, string sheet1Name, string sheet2Name, string companyId, string plantId)
        {
            var cmdText = @"
                        SELECT OM.Id
	                    ,OM.CompanyGroupId
	                    ,OM.Sequence
	                    ,OM.Code OperationCode
	                    ,OM.ShortName
	                    ,OM.StandardName
	                    ,OM.UserName
	                    --,OM.OperationActivityId
	                    ,OA.Code OperationActivityCode
	                    ,OA.UserName OperationActivity
	                    ,OM.OperationCategoryId
	                    ,OC.Code OperationCategoryCode
	                    ,OC.UserName OperationCategory
	                    ,OM.OperationTypeId
	                    ,OT.Code OperationTypeCode
	                    ,OT.UserName OperationType
	                    ,OM.SkillId
	                    ,Skill = CASE 
		                    WHEN SKO.UserName IS NULL
			                    THEN SKM.UserName
		                    WHEN SKM.UserName IS NULL
			                    THEN SKO.UserName
		                    ELSE SKM.UserName
		                    END
	                    ,OM.Type
	                    ,OM.MachineMasterId
	                   ,MM.UserName MachineMasterName
	                    ,MM.Code MachineCode
	                    ,MM.UserName Machine
	                    --,Om.SkillGroupId
	                    ,Om.UserName SkillGroupName
	                    ,SKG.Code SkillGroupCode
	                    ,SKG.UserName SkillGroup
	                    ,SKG.[Grouping]
	                    ,OM.LegalDesignationId
	                    ,OLDG.Code LegalDesignationCode
	                    ,OLDG.UserName LegalDesignation
	                    ,Om.ProcessId
	                    ,PR.Code ProcessCode
	                    ,PR.UserName Process
	                    ,OMB.Caption
	                    ,PO.UserName Position
	                    ,OMB.ManpowerBudget
	                    ,OM.ProposedSalary
	                    ,OM.Active
	                    ,OM.Remarks
	                    ,CONCAT(EN.UserName, ' ', Li.ShortName) AS EntityName
	                    ,PO.UserName PositionName
                    FROM MST.OperationMaster OM
                    LEFT JOIN [HKP].[OperationActivity] OA ON OA.Id = OM.OperationActivityId
                    LEFT JOIN [HKP].[OperationCategory] OC ON OC.Id = OM.OperationCategoryId
                    LEFT JOIN [HKP].[OperationType] OT ON OT.Id = OM.OperationTypeId
                    LEFT JOIN MST.MachineMaster MM ON MM.Id = OM.MachineMasterId
                    LEFT JOIN [HKP].[Skill] SKO ON SKO.Id = OM.SkillId
                    LEFT JOIN [HKP].[Skill] SKM ON SKM.Id = MM.SkillId
                    LEFT JOIN [HKP].[SkillProcess] SP ON SP.SkillId = SKO.Id
                    LEFT JOIN [HKP].[Process] P ON P.Id = SP.ProcessId
                    LEFT JOIN SCS.SkillGrouping SKG ON SKG.Id = OM.SkillGroupId
                    LEFT JOIN HKP.LegalDesignation OLDG ON OLDG.Id = OM.LegalDesignationId
                    LEFT JOIN [HKP].[Process] PR ON PR.Id = OM.ProcessId
                    LEFT JOIN [MST].OperationPositionMPBudget OMB ON OMB.OperationMasterId = OM.Id
                    LEFT JOIN [ORG].[Entity] EN ON EN.ID= OMB.EntityId
                    LEFT JOIN [ORG].[Line] Li ON Li.ID = EN.LineId
                    LEFT JOIN [ORG].[Position] PO ON PO.Id = OMB.PositionId";
            var inventoryMaterialList = _sqlRepository.GetDataTable(cmdText);
            var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();
            if (inventoryMaterialList.Rows.Count == 0)
                throw new Exception("No Data Found !!!");

            var _row = 5;

            //_row++;
            var _rowL = _row;
            var row = _row + 1;


            var sheet1headreColIndex = 1;
            //var sheet2headreColIndex = 1;
            _rowL += 1;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Operation Code");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Short Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Standard Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "User Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Operation Activity");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Operation Type");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Operation Category");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Type");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Machine Master Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Proposed Salary");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Skill Group Name");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Skill Group ");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Legal Designation");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Process ");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Active");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Entity Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Position Name");
            sheet1headreColIndex++;
            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Caption");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Man power Budget");
            sheet1headreColIndex++;

            report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Remarks");
            sheet1headreColIndex++;





            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
            {
                _rowL++;
                report.SetText(ref sheet1, _rowL, 1, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OperationCode"].ToString()));
                report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["ShortName"].ToString());
                report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["StandardName"].ToString());
                report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["UserName"].ToString());
                report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["OperationActivity"].ToString());
                report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["OperationType"].ToString());
                report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["OperationCategory"].ToString());
                report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["Type"].ToString());
                report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["MachineMasterName"].ToString());
                report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ProposedSalary"].ToString()));
                report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["SkillGroupName"].ToString());
                report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["SkillGroup"].ToString());
                report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["LegalDesignation"].ToString());
                report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["Process"].ToString());
                report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["Active"].ToString());
                report.SetText(ref sheet1, _rowL, 16, inventoryMaterialList.Rows[n]["EntityName"].ToString());
                report.SetText(ref sheet1, _rowL, 17, inventoryMaterialList.Rows[n]["PositionName"].ToString());
                report.SetText(ref sheet1, _rowL, 18, inventoryMaterialList.Rows[n]["Caption"].ToString());
                report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ManpowerBudget"].ToString()));
                report.SetText(ref sheet1, _rowL, 20, inventoryMaterialList.Rows[n]["Remarks"].ToString());


            }

            //#region sumCalc

            //_rowL++;
            //sheet1.Range[_rowL, 1, _rowL, 5].Merge();
            //sheet2.Range[_rowL, 1, _rowL, 5].Merge();
            //report.SetText(ref sheet1, _rowL, 1, "Total :", true);
            //report.SetText(ref sheet2, _rowL, 1, "Total :", true);

            //var totalCountNeed = 5;
            //var sumdrcrCol = 6;
            //for (int i = 1; i <= totalCountNeed; i++)
            //{
            //    if (i < 3)
            //    {
            //        sheet1.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
            //        sheet1.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
            //        sheet1.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
            //        sheet1.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
            //    }

            //    sheet2.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
            //    sheet2.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
            //    sheet2.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
            //    sheet2.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);


            //    sumdrcrCol++;
            //}
            //#endregion sumCalc

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            //sheet2.Range[(row), 1, _rowL, sheet2headreColIndex].BorderAround(ExcelLineStyle.Hair);


            _rowL++;
            //if (inventoryServiceList.Rows.Count != 0)
            //{
            //    _rowL++;
            //    var serviceHeadreColIndex = 1;
            //    report.SetHeaderText(ref sheet1, _rowL, serviceHeadreColIndex, "Service", 32);
            //    serviceHeadreColIndex++;
            //    report.SetHeaderText(ref sheet1, _rowL, serviceHeadreColIndex, "Amount (TRN)", 32, ExcelHAlign.HAlignRight);
            //    serviceHeadreColIndex++;
            //    report.SetHeaderText(ref sheet1, _rowL, serviceHeadreColIndex, "Total Tax", 26, ExcelHAlign.HAlignRight);


            //    for (int n = 0; n < inventoryServiceList.Rows.Count; n++)
            //    {
            //        _rowL++;
            //        report.SetText(ref sheet1, _rowL, 1, inventoryServiceList.Rows[n]["ServiceMasterName"].ToString());
            //        report.SetText(ref sheet1, _rowL, 2, Convert.ToDouble(inventoryServiceList.Rows[n]["Amount"].ToString()));
            //        report.SetText(ref sheet1, _rowL, 3, Convert.ToDouble(inventoryServiceList.Rows[n]["TotalTaxAmount"].ToString()));
            //    }

            //}
            //#region sum

            //_rowL++;
            //report.SetText(ref sheet1, _rowL, 1, "Total :", true);

            //var loopCount = 2;
            //var colNo = 2;
            //for (int i = 1; i <= loopCount; i++)
            //{
            //    sheet1.Range[_rowL, colNo].Formula = "=SUM(" + report.GetColumnNameForXls(colNo) + Row_Total_Start + ":" + report.GetColumnNameForXls(colNo) + (_rowL - 1) + ")";
            //    sheet1.Range[_rowL, colNo].NumberFormat = report.NumberFormatDecimalTwo();
            //    sheet1.Range[_rowL, colNo].CellStyle.Font.Bold = true;
            //    sheet1.Range[_rowL, colNo].BorderAround(ExcelLineStyle.Hair);
            //    colNo++;
            //}
            //#endregion sumCalc

            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
            sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

            //#region Signature

            //_rowL = _rowL + 4;
            //sheet1.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            //sheet1.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            //sheet1.Range[_rowL, 6].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

            //report.SetText(ref sheet1, _rowL, 1, "Prepared By", true);
            //report.SetText(ref sheet1, _rowL, 3, "Checked By", true);
            //report.SetText(ref sheet1, _rowL, 6, "Authorized By", true);

            //sheet2.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            //sheet2.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            //sheet2.Range[_rowL, 6].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

            //report.SetText(ref sheet2, _rowL, 1, "Prepared By", true);
            //report.SetText(ref sheet2, _rowL, 3, "Checked By", true);
            //report.SetText(ref sheet2, _rowL, 6, "Authorized By", true);

            //#endregion Signature

            sheet1.Name = sheet1Name;
            sheet1.UsedRange.WrapText = true;
            sheet1.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet1, sheet1headreColIndex, sheet1Name, companyId, plantName, null);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);

            //sheet2.Name = sheet2Name;
            //sheet2.UsedRange.WrapText = true;
            //sheet2.UsedRange.CellStyle.Font.Size = 8;
            //report.CompanyPlantHeader(ref sheet2, sheet2headreColIndex, sheet2Name, companyId, plantName, null);
            //report.PageSetup(ref sheet2, 5, ExcelPageOrientation.Landscape);
        }





        #endregion Operation Master Reports


    }
}