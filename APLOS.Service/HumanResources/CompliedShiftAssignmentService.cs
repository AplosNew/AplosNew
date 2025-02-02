using Library.Core;
using Library.Crosscutting;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Model.Logs;
using Library.Model.Organizations;
using Library.Model.Setups;
using Library.Service.Addresses;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.Organizations;
using Library.ViewModel.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Library.Service.HumanResources
{
    public class CompliedShiftAssignmentService : Service<CompliedShiftAssignment>, ICompliedShiftAssignmentService
    {
        #region Constructor
        private readonly IRepositoryAsync<MailReceiver> _mailReceiverRepository;
        private readonly IRepositoryAsync<MailReceiverDetail> _mailReceiverDetailRepository;
        private readonly IRepositoryAsync<MailReceiverServiceMapping> _mailReceiverServiceMappingRepository;
        private readonly ISMTPConfigurationService _smtpConfigurationService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<MailLog> _mailLogRepository;
        private readonly IRepositoryAsync<CompanyGroup> _companyGroupRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;

        public CompliedShiftAssignmentService(
            IRepositoryAsync<CompliedShiftAssignment> compliedShiftActualShiftTagRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IRepositoryAsync<MailLog> mailLogRepository
            , IRepositoryAsync<CompanyGroup> companyGroupRepository
            , IRepositoryAsync<Company> companyRepository
            , IRepositoryAsync<MailReceiver> mailReceiverRepository
            , IRepositoryAsync<MailReceiverDetail> mailReceiverDetailRepository
            , ISMTPConfigurationService smtpConfigurationService
            , IRepositoryAsync<MailReceiverServiceMapping> mailReceiverServiceMappingRepository

            , IUnitOfWork unitOfWork) : base(compliedShiftActualShiftTagRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _mailLogRepository = mailLogRepository;
            _companyGroupRepository = companyGroupRepository;
            _companyRepository = companyRepository;
            _mailReceiverRepository = mailReceiverRepository;
            _mailReceiverDetailRepository = mailReceiverDetailRepository;
            _smtpConfigurationService = smtpConfigurationService;
            _mailReceiverServiceMappingRepository = mailReceiverServiceMappingRepository;
        }
        #endregion Constructor
        private List<MailViewModel> GetMaileList(MailReceiverServiceMapping item)
        {
            var sql = @"SELECT MRD.Id, MRD.UserId, MRD.MailType, ISNULL(U.FullName, MRD.FullName) AS FullName, ISNULL(U.Email, MRD.Email) AS Email, ISNULL(U.Active, CONVERT(BIT, 1)) AS Active  FROM [SCS].[MailReceiverDetail] AS MRD
						LEFT JOIN [SEC].[User] AS U ON U.Id=MRD.UserId
						JOIN [SCS].[MailReceiver] AS MR ON MR.Id = MRD.MailReceiverId
                        WHERE MRD.MailReceiverId='" + item.MailReceiverId + "' and MR.Active = 1";
            return _mailReceiverDetailRepository.SqlQuery<MailViewModel>(sql).ToList();
        }

        public void GetEntityPosition(string CompanyGroupId, out DataSet dsRef)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"SELECT DISTINCT u.StandardName UserName,IsNULL(e.RType,'position') as Rtype,e.Sequence eSequence,p.Sequence pSequence from (
                           SELECT  DISTINCT StandardName from [ORG].[StructureRelationship] as ee where CompanyGroupId='" + CompanyGroupId + @"' and  RType = 'Entity'  union
                           SELECT  DISTINCT StandardName from [ORG].[StructureRelationship] as pp where CompanyGroupId='" + CompanyGroupId + @"' and  RType = 'position' ) u
                           LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Entity' ) e on e.StandardName = u.StandardName
						   LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Position' ) p on p.StandardName = u.StandardName"
                };
                dsRef = _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<OrgStructureListViewModel> OrgStructureList(string CompanyGroupId)
        {
            try
            {
                var strSQL = @"  SELECT DISTINCT u.StandardName ColumnName,IsNULL(e.RType,'position') as Rtype,e.Sequence eSequence,p.Sequence pSequence from (
                           SELECT  DISTINCT StandardName from [ORG].[StructureRelationship] as ee where CompanyGroupId='" + CompanyGroupId + @"' and  RType = 'Entity'  union
                           SELECT  DISTINCT StandardName from [ORG].[StructureRelationship] as pp where CompanyGroupId='" + CompanyGroupId + @"' and  RType = 'position' ) u
                           LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Entity' ) e on e.StandardName = u.StandardName
						   LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Position' ) p on p.StandardName = u.StandardName";
                return _mailReceiverDetailRepository.SqlQuery<OrgStructureListViewModel>(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void InsertOrUpdateGraph(IEnumerable<CompliedShiftAssignment> entities)
        {
            try
            {
                if (entities == null)
                {
                    throw new CustomException("No data to save.");
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                foreach (var item in entities)
                {
                    Check(item);
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.PlantId = identity.PlantId;
                        item.Id = GetAutoNumber(nameof(CompliedShiftAssignment), PKGeneratorEnum.Auto, null, DateTime.Now);
                        base.Insert(item);
                    }
                    else
                    {
                        base.Update(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public DataSet GetWorkDate(string empId, string workDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter

            {
                ExportType = "DATASET",
                CmdText = @"SELECT Id from [dbo].[CompliedShiftAssignment] Where EmpSystemId='" + @empId + "' AND WorkDate='" + workDate + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }


        public override void Insert(CompliedShiftAssignment entity)
        {
            try
            {
                if (entity != null)
                {
                    var checkEffectiveDate = GetWorkDate(entity.EmpSystemID, entity.WorkDate.ToString());
                    if (checkEffectiveDate.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Another shift is exists on this date " + entity.WorkDate + "");
                    }
                    entity.Id = GetAutoNumber(nameof(CompliedShiftAssignment), PKGeneratorEnum.Auto, null, DateTime.Now);
                    base.Insert(entity);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string workDate, string compliedShiftId, string plantId)
        {
            try
            {
                parameters.searchBy = "EmployeeCode";
                parameters.sort = "EmployeeCode";
                parameters.order = "ASC";
                parameters.CmdText = @"SELECT CS.ShiftName,E.EmployeeName, Convert(Int,E.EmployeeCode) EmployeeCode,C.* 
                                     FROM [dbo].[CompliedShiftAssignment] C
                                     LEFT JOIN HKP.CompliedShift CS ON CS.Id=C.CompliedShiftId
                                     --LEFT JOIN ShiftDefination SD ON SD.SystemID=C.ActualShiftId
                                     LEFT JOIN EmployeeInformation E ON E.SystemId=C.EmpSystemId                                     
                                     WHERE C.PlantId='" + plantId + @"' AND C.CompliedShiftId='" + compliedShiftId + @"' AND C.WorkDate='" + workDate + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetSectionCbo(string sGroupID, bool sysId, string userId)
        {
            if (sysId)
            {
                var sql = @"SELECT Id,UserName FROM [ORG].Section
                           WHERE Id IN (SELECT SectionId FROM [ORG].[CompanyGroupSection] WHERE CompanyGroupId = '" + sGroupID + @"') ORDER BY UserName";
                return _sqlRepository.GetCombo(sql, "Id", "UserName");

            }
            else
            {
                var sql = @"SELECT ID,UserName FROM (SELECT ID,UserName FROM [ORG].Section
                      WHERE Id IN(SELECT SectionId FROM[ORG].[CompanyGroupSection] WHERE CompanyGroupId = '" + sGroupID + @"')
                      AND Id IN(SELECT SectionId FROM [SEC].[UserSection] AS US
                     INNER JOIN(SELECT Id FROM [SEC].[User] WHERE Id = '" + userId + @"')U ON U.Id = US.UserId)
					 )A ORDER BY UserName";
                return _sqlRepository.GetCombo(sql, "Id", "UserName");
            }
        }

        public IEnumerable<ComboModel> GetCompliedShiftCbo(string plantId)
        {
            var sql = @"SELECT Id,ShiftName FROM [HKP].[CompliedShift] Where PlantId='" + plantId + "'";
            return _sqlRepository.GetCombo(sql, "Id", "ShiftName");
        }


        public IEnumerable<ComboModel> GetActualShiftCbo(string plantId)
        {
            var sql = @"SELECT SystemID,ShiftDefinationDescription FROM ShiftDefination Where PlantId='" + plantId + "'";
            return _sqlRepository.GetCombo(sql, "SystemID", "ShiftDefinationDescription");
        }

        public IEnumerable<ComboModel> GetCompliedShiftGroupingCbo(string plantId)
        {
            var sql = @"Select Id, Description from [MST].[CompliedShiftGrouping] Where PlantId='" + plantId + "' ORDER BY Description";
            return _sqlRepository.GetCombo(sql, "Id", "Description");
        }

        private List<Dictionary<string, object>> CheckEmployee(string empSystemId, DateTime workDate, string compliedShift)
        {
            var sql = @"SELECT EmpSystemId,CompliedShiftId,WorkDate FROM [dbo].[CompliedShiftAssignment] Where EmpSystemId='" + empSystemId + @"' AND WorkDate='" + workDate + @"' AND CompliedShiftId<>'" + compliedShift + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        private void Check(CompliedShiftAssignment entity)
        {

            //var compliedShift = "";
            //var empSystemId = "";
            var list = CheckEmployee(entity.EmpSystemID, entity.WorkDate, entity.CompliedShiftId);
            if (list.Count > 0)
            {
                throw new CustomException("This Employee already exists for the selected date...");
            }

            //foreach (var item in CheckEmployee(entity.EmpSystemID, entity.WorkDate, entity.CompliedShiftId))
            //{
            //    var dic = (Dictionary<string, object>)item;
            //    DateTime workDate = Convert.ToDateTime(dic["WorkDate"]);
            //    compliedShift = dic["CompliedShiftId"].ToString();
            //    empSystemId = dic["EmpSystemId"].ToString();

            //    if (entity.EmpSystemID == empSystemId && entity.WorkDate == workDate && entity.CompliedShiftId == compliedShift)
            //    {
            //        throw new CustomException("This Employee already exists");
            //    }
            //}
        }

        public GridModel GetAllEmployee(GridParameter parameters, string plantId, string sectionId, string workDate, string compliedShiftGroupId)
        {
            try
            {
                var sec = "";
                var shift = "";
                if (sectionId != "null" && sectionId != "undefined")
                {
                    sec = @"AND pr.SectionId='" + sectionId + @"'";
                }
                if (compliedShiftGroupId != "null" && compliedShiftGroupId != "undefined")
                {
                    shift = @"AND S.ShiftSystemID IN (Select ActualShiftId from [MST].[CompliedShiftGroupDetail] Where CompliedShiftGroupingId='" + compliedShiftGroupId + @"')";
                }

                parameters.CmdText = @"SELECT 0 Active, S.WorkDate,S.EmpSystemID,EMP.EmployeeName,Convert(Int,EMP.EmployeeCode) EmployeeCode,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                       PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.ShiftSystemID ActualShiftId
                                       ,SE.UserName Section,SS.UserName SubSection,PL.UserName Plant
                                       FROM EmpDateWiseShiftAssign S
                                       left join EmployeeInformation EMP ON EMP.SystemId=S.EmpSystemID
                                       LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                       LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                       LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                       LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                       LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                       LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                       LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                       LEFT JOIN ORG.Section SE ON SE.Id=pr.SectionId
                                       LEFT JOIN ORG.SubSection SS ON SS.Id=pr.SubSectionId
                                       WHERE S.PlantID='" + plantId + @"'
                                       AND WorkDate='" + workDate + @"' " + sec + " " + shift + " " +
                                       "AND EmpSystemID NOT IN (select EmpSystemId from CompliedShiftAssignment where WorkDate = '" + workDate + @"')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetUnAssignEmployee(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT EMP.SystemId,EMP.EmployeeName,Convert(Int,EMP.EmployeeCode) EmployeeCode, FORMAT(EMP.DOJ,'dd-MMM-yyyy') DOJ 
									  ,SE.UserName Section,SS.UserName SubSection,PL.UserName Plant, DEPT.UserName Department, D.UserName Designation, DEG.UserName GivenDesignation
									  FROM EmployeeInformation EMP
                                       LEFT JOIN ORG.Department DEPT ON EMP.DepartmentId=DEPT.Id
									   LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                       LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                       LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                       LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                       LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                       LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                       LEFT JOIN ORG.Section SE ON SE.Id=pr.SectionId
                                       LEFT JOIN ORG.SubSection SS ON SS.Id=pr.SubSectionId
                                       WHERE EMP.EmployeeStatus='Active' AND EMP.SystemId NOT IN (SELECT EmpSystemId FROM CompliedShiftAssignment) AND EMP.PlantID='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetCbo(string empId)
        {
            try
            {
                string _sql = @"SELECT R.Id,R.CompliedShiftRosterMasterID AS [Value] FROM [dbo].[CompliedEmployeeRoster] R
                          -- , RM.ShiftRosterName UserName LEFT JOIN [dbo].[CompliedShiftRosterMaster] RM ON RM.Id=R.CompliedShiftRosterMasterID
                            WHERE R.EmpSystemId='" + empId + "'";

                return _sqlRepository.GetGridData(new GridParameter { CmdText = _sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetCompliedRosterCbo(string plantId)
        {
            string _sql = @"SELECT RM.Id, RM.ShiftRosterName UserName FROM  [dbo].[CompliedShiftRosterMaster] RM where RM.PlantID='" + plantId + "'";
            return _sqlRepository.GetCombo(_sql, "Id", "UserName");
        }

        public IEnumerable<object> GetEmployeeFixedShift(string empId, string plantId, string fromDate, string toDate)
        {
            try
            {
                var sql = @"SELECT CS.ShiftName,CSA.Id,CSA.CompliedShiftId,CSA.PlantId,CSA.EmpSystemId,
                            REPLACE(CONVERT(VARCHAR(11), CSA.WorkDate, 113), ' ', '-') WorkDate FROM [dbo].[CompliedShiftAssignment] CSA
                            LEFT JOIN [HKP].[CompliedShift] CS ON CS.Id=CSA.CompliedShiftId
                            WHERE CSA.EmpSystemId='" + empId + @"'  AND CSA.PlantId='" + plantId + @"' AND CSA.WorkDate between '" + fromDate + @"' AND  '" + toDate + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetDailyComplianceReport(string plantId, string plantName, string companyId, string workDate)
        {
            try
            {

                var xlsRow = 6;
                var xlsCol = 0;
                int endXlsCol = 1;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Daily Attendance";

                var dataList = GetDailyCompliance(plantId, workDate);
                if (dataList.Count == 0)
                {
                    throw new CustomException("No processed data found.");
                }
                string strSubSec = "0";
                string strSec = "0";
                string strUnit = "0";
                int strCount = 0;
                int intRow = 0;
                for (int i = 0; i <= dataList.Count - 1; i++)
                {

                    xlsCol = 1;

                    if ((string.Compare(strSubSec.ToUpper(), dataList[i]["SubSectionID"].ToString().Trim().ToUpper())) != 0
                        || (string.Compare(strSec.ToUpper(), dataList[i]["SectionID"].ToString().Trim().ToUpper())) != 0
                        || (string.Compare(strUnit.ToUpper(), dataList[i]["UnitID"].ToString().Trim().ToUpper())) != 0)
                    {
                        xlsRow += intRow;
                        intRow = 1;
                        strCount = 0;

                        sheet.Range[xlsRow, 1].Text = "Unit :-" + dataList[i]["Unit"].ToString();
                        sheet.Range[xlsRow, 1, xlsRow, 3].Merge();
                        sheet.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                        sheet.Range[xlsRow, 1].CellStyle.Font.Size = 12;
                        sheet.Range[xlsRow, 1, xlsRow, 3].RowHeight = 21;
                        sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet.Range[xlsRow, 4].Text = "Section :-" + dataList[i]["Section"].ToString();
                        sheet.Range[xlsRow, 4, xlsRow, 6].Merge();
                        sheet.Range[xlsRow, 4].CellStyle.Font.Bold = true;
                        sheet.Range[xlsRow, 4].CellStyle.Font.Size = 12;
                        sheet.Range[xlsRow, 4, xlsRow, 6].RowHeight = 21;
                        sheet.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet.Range[xlsRow + 1, 1].Text = "Sub Section :-" + dataList[i]["SubSection"].ToString();
                        sheet.Range[xlsRow + 1, 1, xlsRow + 1, 3].Merge();
                        sheet.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;
                        sheet.Range[xlsRow + 1, 1].CellStyle.Font.Size = 12;
                        sheet.Range[xlsRow + 1, 1, xlsRow + 1, 3].RowHeight = 21;
                        sheet.Range[xlsRow + 1, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsRow += 2;

                        #region ------------------Column Header------------------
                        xlsCol = 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Sl No.";
                        sheet.Range[xlsRow, xlsCol].ColumnWidth = 4.70;
                        sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Employee Code";
                        sheet.Range[xlsRow, xlsCol].ColumnWidth = 13;
                        sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Employee Name";
                        sheet.Range[xlsRow, xlsCol].ColumnWidth = 25;
                        sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Shift Name";
                        sheet.Range[xlsRow, xlsCol].ColumnWidth = 25;
                        sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Shift InTime";
                        sheet.Range[xlsRow, xlsCol].ColumnWidth = 11;
                        sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Shift OutTime";
                        sheet.Range[xlsRow, xlsCol].ColumnWidth = 12;
                        sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //xlsCol += 1;
                        //sheet.Range[xlsRow, xlsCol].Text = "Least Punch Time";
                        //sheet.Range[xlsRow, xlsCol].ColumnWidth = 15;
                        //sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet.Range[xlsRow, xlsCol].Text = "InTime";
                        sheet.Range[xlsRow, xlsCol].ColumnWidth = 7;
                        sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet.Range[xlsRow, xlsCol].Text = "OutTime";
                        sheet.Range[xlsRow, xlsCol].ColumnWidth = 7;
                        sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet.Range[xlsRow, xlsCol].Text = "Day Status";
                        sheet.Range[xlsRow, xlsCol].ColumnWidth = 8;
                        sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //xlsCol += 1;
                        //sheet.Range[xlsRow, xlsCol].Text = "Late By";
                        //sheet.Range[xlsRow, xlsCol].ColumnWidth = 7;
                        //sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //xlsCol += 1;
                        //sheet.Range[xlsRow, xlsCol].Text = "Duration";
                        //sheet.Range[xlsRow, xlsCol].ColumnWidth = 7;
                        //sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //xlsCol += 1;
                        //sheet.Range[xlsRow, xlsCol].Text = "Short Leave";
                        //sheet.Range[xlsRow, xlsCol].ColumnWidth = 7;
                        //sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //xlsCol += 1;
                        //sheet.Range[xlsRow, xlsCol].Text = "Leave Type";
                        //sheet.Range[xlsRow, xlsCol].ColumnWidth = 7;
                        //sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                        sheet.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                        endXlsCol = xlsCol;
                        xlsCol = 1;
                        xlsRow += 1;
                        #endregion ------------------Column Header------------------
                    }
                    //strSubSec = dataList[i]["SubSection"].ToString().Trim();//
                    strSubSec = dataList[i]["SubSectionID"].ToString().Trim();//SubSectionID
                    strSec = dataList[i]["SectionID"].ToString().Trim();
                    strUnit = dataList[i]["UnitID"].ToString().Trim();
                    if (strSubSec.ToUpper() == "GENERAL")
                    {

                    }
                    #region ----------------------Data-----------------------

                    strCount += 1;
                    sheet.Range[xlsRow, xlsCol].Number = strCount;
                    sheet.Range[xlsRow, xlsCol].RowHeight = 13;
                    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet.Range[xlsRow, xlsCol].Text = dataList[i]["EmployeeCode"].ToString().Trim();
                    sheet.Range[xlsRow, xlsCol].RowHeight = 13;
                    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet.Range[xlsRow, xlsCol].Text = dataList[i]["EmployeeName"].ToString().ToUpper();
                    sheet.Range[xlsRow, xlsCol].RowHeight = 13;
                    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet.Range[xlsRow, xlsCol].Text = dataList[i]["ShiftName"].ToString().Trim();
                    sheet.Range[xlsRow, xlsCol].RowHeight = 13;
                    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet.Range[xlsRow, xlsCol].Text = dataList[i]["ShiftInTimeShow"].ToString().Trim();
                    sheet.Range[xlsRow, xlsCol].RowHeight = 13;
                    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsCol += 1;
                    sheet.Range[xlsRow, xlsCol].Text = Convert.ToDateTime(dataList[i]["ShiftOutTimeShow"]).ToString("HH:mm tt").Trim();
                    sheet.Range[xlsRow, xlsCol].RowHeight = 13;
                    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (dataList[i]["DayStatus"].ToString().Trim() == "LV" || dataList[i]["DayStatus"].ToString().Trim() == "W" || dataList[i]["DayStatus"].ToString().Trim() == "H")
                    {
                        xlsCol += 1;
                        sheet.Range[xlsRow, xlsCol].Text = "";
                        sheet.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet.Range[xlsRow, xlsCol].Text = "";
                        sheet.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    }
                    else
                    {
                        xlsCol += 1;
                        sheet.Range[xlsRow, xlsCol].Text = dataList[i]["InTimeShow"].ToString().Trim();
                        sheet.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;

                        try
                        {
                            if (Convert.ToDateTime(dataList[i]["PDate"].ToString()).ToString("dd-MMM-yyyy") == DateTime.Now.ToString("dd-MMM-yyyy"))//DateTime.Now.ToString("dd-MMM-yyyy"))//DateTime.Now.ToString("dd-MMM-yyyy"))
                            {
                                if (dataList[i]["InTimeShow"].ToString() != "" && dataList[i]["OutTimeShow"].ToString() == "")
                                {
                                    if (Convert.ToDateTime(dataList[i]["ShiftOutTimeShow"].ToString()) < DateTime.Now)
                                    {
                                        Random rand = new Random((int)clsStaticInfo.dbl(dataList[i]["EmpSystemId"].ToString() + DateTime.Now.ToDbDate()));//Need Date wise seed  
                                        dataList[i]["OutTimeShow"] = Convert.ToDateTime(dataList[i]["ShiftOutTimeShow"].ToString()).AddMinutes(rand.Next(0, 10)).ToString("HH:mm");
                                    }
                                }
                            }

                            sheet.Range[xlsRow, xlsCol].Text = Convert.ToDateTime(dataList[i]["OutTimeShow"]).ToString("HH:mm tt").Trim();
                            sheet.Range[xlsRow, xlsCol].RowHeight = 13;
                            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        catch (Exception)
                        {

                            
                        }
                    }

                    xlsCol += 1;
                    if (dataList[i]["DayStatus"].ToString().Trim() == "L")
                    {
                        sheet.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Blue;
                        sheet.Range[xlsRow, xlsCol].Text = "P";
                    }
                    else
                    {
                        sheet.Range[xlsRow, xlsCol].Text = dataList[i]["DayStatus"].ToString().Trim().Replace("RST", "P");
                    }
                    sheet.Range[xlsRow, xlsCol].RowHeight = 13;
                    sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsRow += 1;

                    #endregion ----------------------Data-----------------------

                    #region Line Setup
                    sheet.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].WrapText = true;
                    #endregion
                }

                #region Freeze Panes
                sheet.IsDisplayZeros = false;
                sheet.UsedRange["A5"].FreezePanes();
                sheet.FirstVisibleColumn = 1;
                sheet.FirstVisibleRow = 4;
                #endregion

                sheet.Range[11, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[11, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[11, 4, xlsRow, 4].WrapText = true;
                //sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.PlantHeader(ref sheet, endXlsCol, "Daily Attendance " + workDate + "", plantId);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(endXlsCol) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IWorkbook GetMonthlyDailyShiftReport(string PlantId, string PlantName, string CompanyId, string userName, string yearId, string monthId, string complianceShiftList)
        {
            #region Variable

            ReportUtility reportUtility = new ReportUtility();
            DataView dvDaily = null;
            DateTime dtFrmDt = DateTime.Now;
            DateTime dtEndDate = DateTime.Now;

            ParaMontlyAttendance objm = new ParaMontlyAttendance();

            string m = reportUtility.GetMonthName(monthId);
            dtFrmDt = Convert.ToDateTime("01-" + m + "-" + yearId);

            if (Convert.ToInt32(DateTime.Now.Month) != Convert.ToInt32(monthId))
            {
                dtEndDate = dtFrmDt.AddMonths(1).AddDays(-1);
            }
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

            #endregion Variable
            try
            {

                string _ShiftCode = "ShiftCode";

                objm.AMonth = monthId;
                objm.AYear = yearId;
                objm.PlantId = PlantId;
                objm.FDate = dtFrmDt.ToString("dd-MMM-yyyy");
                objm.TDate = dtEndDate.ToString("dd-MMM-yyyy");
                var dataList = GetMonthlyAttendance(PlantId, yearId, monthId, dtFrmDt.ToString("dd-MMM-yyyy"), complianceShiftList);

                if (dataList.Count == 0)
                {
                    throw new CustomException("No processed data found.");
                }

                Dictionary<string, List<DataRow>> dicShift = GetEmployeeShift(PlantId, dtFrmDt.ToString("dd-MMM-yyyy"), dtEndDate.ToString("dd-MMM-yyyy"));

                if (dataList.Count == 0)
                    throw new Exception("No data found");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                xlsRow = 5;

                #region Variables              
                int strCount = 0; int iSrNo = 0; int iEmpCode = 0; int iEmpName = 0;
                int iDOJ = 0; int iDOS = 0; int iDiv = 0; int iUnit = 0; int iDepart = 0;
                int iSec = 0; int iDesig = 0; int iGDesig = 0;
                #endregion

                #region ------------------Column Header------------------

                #region ------------------Details Header-----------------

                xlsRow += 1;

                xlsCol = 1;
                iSrNo = xlsCol;
                sheet1.Range[xlsRow, iSrNo].Text = "Sl No.";
                sheet1.Range[xlsRow, iSrNo].ColumnWidth = 10;
                sheet1.Range[xlsRow, iSrNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iSrNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iEmpCode = xlsCol;
                sheet1.Range[xlsRow, iEmpCode].Text = "Employee Code";
                sheet1.Range[xlsRow, iEmpCode].ColumnWidth = 30;
                sheet1.Range[xlsRow, iEmpCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iEmpName = xlsCol;
                sheet1.Range[xlsRow, iEmpName].Text = "Employee Name";
                sheet1.Range[xlsRow, iEmpName].ColumnWidth = 45;
                sheet1.Range[xlsRow, iEmpName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iEmpName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //xlsCol += 1;
                //iDOJ = xlsCol;
                //sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                //sheet1.Range[xlsRow, iDOJ].ColumnWidth = 9.20;
                //sheet1.Range[xlsRow, iDOJ].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1.Range[xlsRow, iDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //xlsCol += 1;
                //iDOS = xlsCol;
                //sheet1.Range[xlsRow, iDOS].Text = "DOS";
                //sheet1.Range[xlsRow, iDOS].ColumnWidth = 9.20;
                //sheet1.Range[xlsRow, iDOS].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1.Range[xlsRow, iDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //xlsCol += 1;
                //iUnit = xlsCol;
                //sheet1.Range[xlsRow, iUnit].Text = "Unit";
                //sheet1.Range[xlsRow, iUnit].ColumnWidth = 9;
                //sheet1.Range[xlsRow, iUnit].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1.Range[xlsRow, iUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iDepart = xlsCol;
                sheet1.Range[xlsRow, iDepart].Text = "Department";
                sheet1.Range[xlsRow, iDepart].ColumnWidth = 60;
                sheet1.Range[xlsRow, iDepart].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iDepart].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //xlsCol += 1;
                //iSec = xlsCol;
                //sheet1.Range[xlsRow, iSec].Text = "Section";
                //sheet1.Range[xlsRow, iSec].ColumnWidth = 15;
                //sheet1.Range[xlsRow, iSec].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1.Range[xlsRow, iSec].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //xlsCol += 1;
                //iDiv = xlsCol;
                //sheet1.Range[xlsRow, iDiv].Text = "Division";
                //sheet1.Range[xlsRow, iDiv].ColumnWidth = 9.20;
                //sheet1.Range[xlsRow, iDiv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1.Range[xlsRow, iDiv].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iDesig = xlsCol;
                sheet1.Range[xlsRow, iDesig].Text = "Designation";
                sheet1.Range[xlsRow, iDesig].ColumnWidth = 35;
                sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //xlsCol += 1;
                //iGDesig = xlsCol;
                //sheet1.Range[xlsRow, iGDesig].Text = "Given Designation";
                //sheet1.Range[xlsRow, iGDesig].ColumnWidth = 15;
                //sheet1.Range[xlsRow, iGDesig].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //sheet1.Range[xlsRow, iGDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //xlsCol += 1;
                //iGDesig = xlsCol;
                //List<SwapColumn> _list2 = GetColDisplayName(dsDaily);
                //List<SwapColumn> _list2 = new List<SwapColumn>();
                xlsCol = iDesig;
                int StartDayCol = xlsCol;
                while (dtFrmDt <= dtEndDate)
                {
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = dtFrmDt.ToString("dd");
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    dtFrmDt = dtFrmDt.AddDays(1);
                }
                #endregion ------------------Details Header-------------------------

                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Size = 24;

                endXlsCol = xlsCol;
                xlsRow += 1;
                int _StartRow = xlsRow;
                #endregion ------------------Column Header------------------

                for (int i = 0; i <= dataList.Count - 1; i++)
                {

                    #region ----------------------Data-----------------------

                    strCount += 1;
                    sheet1.Range[xlsRow, iSrNo].Number = strCount;

                    sheet1.Range[xlsRow, iEmpCode].Text = dataList[i]["EmployeeCode"].ToString().Trim();
                    sheet1.Range[xlsRow, iEmpName].Text = dataList[i]["EmployeeName"].ToString().ToUpper();
                    sheet1.Range[xlsRow, iDepart].Text = dataList[i]["Department"].ToString().Trim();
                    sheet1.Range[xlsRow, iDesig].Text = dataList[i]["GivenDesignation"].ToString().Trim();



                    if (dicShift.ContainsKey(dataList[i]["EmployeePK"].ToString()))
                    {

                        List<DataRow> drData = dicShift[dataList[i]["EmployeePK"].ToString()];
                        foreach (DataRow item in drData)
                        {
                            sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].Text = item["Code"].ToString();
                        }
                    }


                    xlsRow += 1;
                    #endregion ----------------------Data-----------------------
                }

                #region Line Setup
                sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].WrapText = true;
                sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].CellStyle.Font.Size = 25;

                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;

                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region ******************Report Header******************


                #endregion ******************Report Header******************

                #region Freeze Panes
                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;
                sheet1.Zoom = 37;
                #endregion



                sheet1.Range[11, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[11, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[11, 4, xlsRow, 4].WrapText = true;
                //sheet.UsedRange.WrapText = true;
                // sheet1.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.PlantHeader(ref sheet1, endXlsCol, "Monthly Shift Information of " + m + "," + yearId, PlantId);
                sheet1.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(endXlsCol) + 5].Merge();
                sheet1.Range[1, 1, 4, endXlsCol].CellStyle.Font.Size = 40;
                sheet1.Range[1, 1, 4, endXlsCol].RowHeight = 65;

                sheet1.Range[1, 1, 4, endXlsCol].CellStyle.Font.FontName = "Aerial Narrow";

                reportUtility.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);

                sheet1.PageSetup.LeftMargin = 0.4;
                sheet1.PageSetup.RightMargin = 0.2;

                return workbook;
            }

            catch (Exception ex)
            {
                throw ex;
            }


        }

        public IWorkbook GetMonthlyDailyAttendanceReport(string PlantId, string PlantName, string CompanyId, string userName, string yearId, string monthId, string dayStatusReportType)
        {
            #region Variable

            ReportUtility reportUtility = new ReportUtility();
            DateTime dtFrmDt = DateTime.Now;
            DateTime dtEndDate = DateTime.Now;

            ParaMontlyAttendance objm = new ParaMontlyAttendance();

            string m = reportUtility.GetMonthName(monthId);
            dtFrmDt =new DateTime((int)clsStaticInfo.dbl(yearId), (int)clsStaticInfo.dbl(monthId), 1);
            dtEndDate = dtFrmDt.AddMonths(1).AddDays(-1);

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

            #endregion Variable
            try
            {
                clsReport objRpt = null;
                DataSet dsMonthlyAttnSumm = null;
                DataView dvMonthlyAttnSumm = null;


                int colTotalDays = 0;
                int colTotalPayDays = 0;
                int colTotalHoliDays = 0;
                int colTotalWeekOffDays = 0;
                int colTotalPresentDays = 0;
                int colTotalPresentLateDays = 0;
                int colTotalExtraAbsentDays = 0;
                int colTotalAbsent = 0;
                int colTotalLeave = 0;
                int colTotalLeaveEL = 0;
                int colTotalLeaveCL = 0;
                int colTotalLeaveSL = 0;
                int colTotalLWP = 0;
                int colSection = 0;


                double totalEl = 0.00;
                double totalCL = 0.00;
                double totalSL = 0.00;


                string DayType = "";
                objRpt = new clsReport();
                var dataList = GetMonthlyAttendance(PlantId, yearId, monthId, dtFrmDt.ToString("dd-MMM-yyyy"), "");
                if (dataList.Count == 0)
                {
                    throw new CustomException("No processed data found.");
                }
                objRpt.GetComplianceMonthlyAttnSummaryRptForDetails(PlantId, monthId, yearId, out dsMonthlyAttnSumm);
                dvMonthlyAttnSumm = new DataView();
                dvMonthlyAttnSumm.Table = dsMonthlyAttnSumm.Tables[0];

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(PlantId, Convert.ToInt32(monthId), Convert.ToInt32(yearId), out dsExtraAbsent);
                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);

                objm.AMonth = monthId;
                objm.AYear = yearId;
                objm.PlantId = PlantId;
                objm.FDate = dtFrmDt.ToString("dd-MMM-yyyy");
                objm.TDate = dtEndDate.ToString("dd-MMM-yyyy");


                Dictionary<string, List<DataRow>> dicShift = GetMonthlyDailyAttendancedec(dayStatusReportType, objm);
                Dictionary<string, List<DataRow>> dicLeave = GetMonthlyLeaveDetail(objm);


                if (dataList.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;
                    application.DefaultVersion = ExcelVersion.Excel2016;
                    workbook = application.Workbooks.Create(1);

                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;
                    workbook.Version = ExcelVersion.Excel2016;
                    xlsRow = 5;

                    #region Variables              
                    int strCount = 0; int iSrNo = 0; int iEmpCode = 0; int iEmpName = 0; int iAttendanceInfo = 0;
                    #endregion

                    #region ------------------Column Header------------------

                    #region ------------------Details Header-----------------

                    xlsRow += 1;

                    xlsCol = 1;
                    iSrNo = xlsCol;
                    sheet1.Range[xlsRow, iSrNo].Text = "Sl";
                    sheet1.Range[xlsRow, iSrNo].ColumnWidth = 20;
                    sheet1.Range[xlsRow, iSrNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iSrNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iSrNo, xlsRow + 1, iSrNo].Merge();


                    xlsCol += 1;
                    iEmpCode = xlsCol;
                    sheet1.Range[xlsRow, iEmpCode].Text = "Employee Information";
                    sheet1.Range[xlsRow, iEmpCode].ColumnWidth = 170;
                    sheet1.Range[xlsRow, iEmpCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iEmpCode, xlsRow + 1, iEmpCode].Merge();
                    xlsCol += 1;
                    colSection = xlsCol;
                    sheet1.Range[xlsRow, colSection].Text = "Section";
                    sheet1.Range[xlsRow, colSection].ColumnWidth = 170;
                    sheet1.Range[xlsRow, colSection].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, colSection].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, colSection, xlsRow + 1, colSection].Merge();


                    sheet1.Range[xlsRow, 1, xlsRow + 1, iAttendanceInfo + 2].CellStyle.Font.Size = 60;

                    //List<SwapColumn> _list2 = GetColDisplayName(dsDaily);
                    xlsCol = colSection;
                    int StartDayCol = colSection;
                    while (dtFrmDt <= dtEndDate)
                    {
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dtFrmDt.ToString("dd");
                        sheet1.Range[xlsRow, xlsCol].ColumnWidth = 18;

                        if (dayStatusReportType == "AllStatusInfo")
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 31;

                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        var dtDate = dtFrmDt.ToString("dd");
                        var dayType = "Ddd";
                        var quote = "\"";
                        var dayFormula = "=TEXT(DATE(" + yearId + ", " + monthId + "," + dtDate + " ), " + quote + "" + dayType + quote + ")";
                        sheet1.Range[xlsRow + 1, xlsCol].Formula = dayFormula;
                        sheet1.Range[xlsRow + 1, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow + 1, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow + 1, xlsCol].CellStyle.Font.Bold = true;


                        //var ob = _list2.Find(r => r.ValueMember == dtFrmDt.ToString("dd"));

                        //if (ob != null)
                        //{
                        //    ob.ColIndex = xlsCol;

                        //}//if
                        dtFrmDt = dtFrmDt.AddDays(1);
                    }

                    if (dayStatusReportType == "AllStatusInfo")
                    {
                        xlsCol += 1;
                        iAttendanceInfo = xlsCol;
                        sheet1.Range[xlsRow, iAttendanceInfo].Text = "Attendance Information";
                        sheet1.Range[xlsRow, iAttendanceInfo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iAttendanceInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, iAttendanceInfo, xlsRow + 1, iAttendanceInfo + 1].Merge();
                        sheet1.Range[xlsRow, iAttendanceInfo].ColumnWidth = 85;
                        sheet1.Range[xlsRow, iAttendanceInfo + 1].ColumnWidth = 85;
                        endXlsCol = iAttendanceInfo - 1;
                    }
                    #endregion ------------------Details Header-------------------------

                    if (dayStatusReportType != "AllStatusInfo")
                    {
                        xlsCol += 1;
                        colTotalDays = xlsCol;
                        sheet1.Range[xlsRow, colTotalDays].Text = "Total Days";
                        sheet1.Range[xlsRow, colTotalDays].ColumnWidth = 22;
                        sheet1.Range[xlsRow, colTotalDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colTotalDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotalDays, xlsRow + 1, colTotalDays].Merge();

                        xlsCol += 1;
                        colTotalPayDays = xlsCol;
                        sheet1.Range[xlsRow, colTotalPayDays].Text = "Pay Days";
                        sheet1.Range[xlsRow, colTotalPayDays].ColumnWidth = 22;
                        sheet1.Range[xlsRow, colTotalPayDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colTotalPayDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotalPayDays, xlsRow + 1, colTotalPayDays].Merge();

                        xlsCol += 1;
                        colTotalHoliDays = xlsCol;
                        sheet1.Range[xlsRow, colTotalHoliDays].Text = "Total Holidays";
                        sheet1.Range[xlsRow, colTotalHoliDays].ColumnWidth = 35;
                        sheet1.Range[xlsRow, colTotalHoliDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colTotalHoliDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotalHoliDays, xlsRow + 1, colTotalHoliDays].Merge();

                        xlsCol += 1;
                        colTotalWeekOffDays = xlsCol;
                        sheet1.Range[xlsRow, colTotalWeekOffDays].Text = "Total Weekdays";
                        sheet1.Range[xlsRow, colTotalWeekOffDays].ColumnWidth = 50;
                        sheet1.Range[xlsRow, colTotalWeekOffDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colTotalWeekOffDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotalWeekOffDays, xlsRow + 1, colTotalWeekOffDays].Merge();

                        xlsCol += 1;
                        colTotalPresentDays = xlsCol;
                        sheet1.Range[xlsRow, colTotalPresentDays].Text = "Total Present";
                        sheet1.Range[xlsRow, colTotalPresentDays].ColumnWidth = 42;
                        sheet1.Range[xlsRow, colTotalPresentDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colTotalPresentDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotalPresentDays, xlsRow + 1, colTotalPresentDays].Merge();


                        xlsCol += 1;
                        colTotalAbsent = xlsCol;
                        sheet1.Range[xlsRow, colTotalAbsent].Text = "Total Absent";
                        sheet1.Range[xlsRow, colTotalAbsent].ColumnWidth = 42;
                        sheet1.Range[xlsRow, colTotalAbsent].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colTotalAbsent].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotalAbsent, xlsRow + 1, colTotalAbsent].Merge();

                        //xlsCol += 1;
                        //colTotalPresentLateDays = xlsCol;
                        //sheet1.Range[xlsRow, colTotalPresentLateDays].Text = "Total Late";
                        //sheet1.Range[xlsRow, colTotalPresentLateDays].ColumnWidth = 35;
                        //sheet1.Range[xlsRow, colTotalPresentLateDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet1.Range[xlsRow, colTotalPresentLateDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //sheet1.Range[xlsRow, colTotalPresentLateDays, xlsRow + 1, colTotalPresentLateDays].Merge();





                        xlsCol += 1;
                        colTotalLeaveEL = xlsCol;
                        sheet1.Range[xlsRow + 1, colTotalLeaveEL].Text = "EL";
                        sheet1.Range[xlsRow + 1, colTotalLeaveEL].ColumnWidth = 35;
                        sheet1.Range[xlsRow + 1, colTotalLeaveEL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow + 1, colTotalLeaveEL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        colTotalLeaveCL = xlsCol;
                        sheet1.Range[xlsRow + 1, colTotalLeaveCL].Text = "CL";
                        sheet1.Range[xlsRow + 1, colTotalLeaveCL].ColumnWidth = 35;
                        sheet1.Range[xlsRow + 1, colTotalLeaveCL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow + 1, colTotalLeaveCL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        colTotalLeaveSL = xlsCol;
                        sheet1.Range[xlsRow + 1, colTotalLeaveSL].Text = "SL";
                        sheet1.Range[xlsRow + 1, colTotalLeaveSL].ColumnWidth = 35;
                        sheet1.Range[xlsRow + 1, colTotalLeaveSL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow + 1, colTotalLeaveSL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, colTotalLeaveEL].Text = "Total Leave";
                        sheet1.Range[xlsRow, colTotalLeaveEL].ColumnWidth = 35;
                        sheet1.Range[xlsRow, colTotalLeaveEL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colTotalLeaveEL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotalLeaveEL, xlsRow, colTotalLeaveSL].Merge();

                        xlsCol += 1;
                        colTotalLWP = xlsCol;
                        sheet1.Range[xlsRow, colTotalLWP].Text = "Total LWP";
                        sheet1.Range[xlsRow, colTotalLWP].ColumnWidth = 35;
                        sheet1.Range[xlsRow, colTotalLWP].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colTotalLWP].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotalLWP, xlsRow + 1, colTotalLWP].Merge();
                        xlsCol += 1;
                        colTotalExtraAbsentDays = xlsCol;
                        sheet1.Range[xlsRow, colTotalExtraAbsentDays].Text = "Extra Absent";
                        sheet1.Range[xlsRow, colTotalExtraAbsentDays].ColumnWidth = 35;
                        sheet1.Range[xlsRow, colTotalExtraAbsentDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colTotalExtraAbsentDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotalExtraAbsentDays, xlsRow + 1, colTotalExtraAbsentDays].Merge();
                        xlsCol += 1;
                        endXlsCol = xlsCol + 1;
                    }


                    sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, iEmpCode + 1, xlsRow + 1, xlsCol].CellStyle.Font.Size = 40;
                    sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].CellStyle.Font.Bold = true;
                    xlsRow = xlsRow + 1;


                    xlsRow += 1;
                    int _StartRow = xlsRow;
                    #endregion ------------------Column Header------------------

                    for (int i = 0; i <= dataList.Count - 1; i++)
                    {
                      
                        totalCL = 0.00;
                        totalEl = 0.00;
                        totalSL = 0.00;

                        #region ----------------------Data-----------------------
                        strCount += 1;
                        sheet1.Range[xlsRow, iSrNo].Number = strCount;
                        //sheet1.Range[xlsRow, iSrNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet1.Range[xlsRow, iSrNo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iEmpCode].Text = "Emp. Code :" + dataList[i]["EmployeeCode"].ToString().Trim() + Environment.NewLine + "Name :" + dataList[i]["EmployeeName"].ToString().ToUpper() + Environment.NewLine + "F. Name :" + dataList[i]["EmployeeFatherName"].ToString().ToUpper() + Environment.NewLine + "Dept :" + dataList[i]["Department"].ToString().Trim() + Environment.NewLine + "Desg :" + dataList[i]["GivenDesignation"].ToString().Trim();

                        sheet1.Range[xlsRow, colSection].Text = dataList[i]["Section"].ToString().Trim();


                        string ecode = dataList[i]["EmployeeCode"].ToString().Trim();
                        string _SystemId = dataList[i]["EmployeePK"].ToString().Trim();
                        decimal _ExtraAbsent = 0;
                        dvExtraAbsent.RowFilter = "EmpSystemID='" + _SystemId + "' ";
                        _ExtraAbsent = dvExtraAbsent.Count;
                        var DaysInaMonth = bplib.clsWebLib.GetNumData(dataList[i]["TotalProcDate"].ToString().Trim());
                        var TotalAbsent = bplib.clsWebLib.GetNumData(dataList[i]["TotalAbsent"].ToString().Trim());
                        var TotalLWP = bplib.clsWebLib.GetNumData(dataList[i]["TotalLWP"].ToString().Trim());
                        double _pay_days = Convert.ToDouble(DaysInaMonth) - (Convert.ToDouble(TotalAbsent) + Convert.ToDouble(TotalLWP) + Convert.ToDouble(_ExtraAbsent));
                        double _pre = Convert.ToDouble(bplib.clsWebLib.GetNumData(dataList[i]["TotalPresent"].ToString().Trim()));
                        double _Late = Convert.ToDouble(bplib.clsWebLib.GetNumData(dataList[i]["TotalLate"].ToString().Trim()));
                        double _absent = Convert.ToDouble(bplib.clsWebLib.GetNumData(dataList[i]["TotalAbsent"].ToString().Trim()));
                        double TPresentAndLate = _pre + _Late;

                        if (dayStatusReportType == "AllStatusInfo")
                        {

                            sheet1.Range[xlsRow, iAttendanceInfo].Text = "Total Days:" + DaysInaMonth + Environment.NewLine
                                                                          + "Pay Days:" + _pay_days + Environment.NewLine + "Present :" + TPresentAndLate + Environment.NewLine + "Late:"
                                                                          + _Late + Environment.NewLine + "Absent:" + _absent;
                            sheet1.Range[xlsRow, iAttendanceInfo + 1].Text = "Leave:" + bplib.clsWebLib.GetNumData(dataList[i]["TotalLv"].ToString().Trim()) + Environment.NewLine + "LWP:" + TotalLWP + Environment.NewLine
                                                                         + "Holidays:" + dataList[i]["TotalHoliDay"].ToString().Trim() + Environment.NewLine + "WeekOff:" + dataList[i]["TotalWeekOff"].ToString().Trim()
                                                                          + Environment.NewLine + "Extra Absentism :" + _ExtraAbsent;
                            //sheet1.Range[xlsRow, iAttendanceInfo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            //sheet1.Range[xlsRow, iAttendanceInfo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, iAttendanceInfo + 1].CellStyle.Font.Size = 55;
                            sheet1.Range[xlsRow, iSrNo, xlsRow, iAttendanceInfo].CellStyle.Font.Size = 60;
                        }
                        else
                        {

                            sheet1.Range[xlsRow, colTotalDays].Text = dataList[i]["TotalProcDate"].ToString().Trim();


                            sheet1.Range[xlsRow, colTotalPayDays].Text = _pay_days.ToString();

                            //sheet1.Range[xlsRow, colTotalLeave].Text = bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalLV"].ToString().Trim());





                            sheet1.Range[xlsRow, colTotalHoliDays].Text = dataList[i]["TotalHoliDay"].ToString().Trim();
                            sheet1.Range[xlsRow, colTotalWeekOffDays].Text = dataList[i]["TotalWeekOff"].ToString().Trim();


                            sheet1.Range[xlsRow, colTotalPresentDays].Text = TPresentAndLate.ToString().Trim();

                            double _extraAbsent = Convert.ToDouble(bplib.clsWebLib.GetNumData(dataList[i]["TotalLate"].ToString().Trim()));
                            sheet1.Range[xlsRow, colTotalAbsent].Text = _absent.ToString().Trim();
                            sheet1.Range[xlsRow, iSrNo, xlsRow, iEmpCode].CellStyle.Font.Size = 60;

                        }

                        string _m = reportUtility.GetMonthName(monthId);
                        dtFrmDt = Convert.ToDateTime("01-" + _m + "-" + yearId);
                        xlsCol = iEmpCode;

                        if (dicShift.ContainsKey(dataList[i]["EmployeePK"].ToString()))
                        {
                            List<DataRow> drData = dicShift[dataList[i]["EmployeePK"].ToString()];
                            foreach (DataRow item in drData)
                            {
                                if (string.IsNullOrEmpty(item["Dstatus"].ToString()))
                                {
                                    DayType = item["DayStatus"].ToString();
                                }
                                else
                                {
                                    DayType = item["Dstatus"].ToString();


                                }
                                if (item["Category"].ToString() == "Leave")
                                {
                                    DayType = item["LEAVE"].ToString();
                                }
                                if (dayStatusReportType == "AllStatusInfo")
                                {
                                    if (Convert.ToDateTime(item["PDate"].ToString()).ToString("dd-MMM-yyyy") == DateTime.Now.ToString("dd-MMM-yyyy"))//DateTime.Now.ToString("dd-MMM-yyyy"))//DateTime.Now.ToString("dd-MMM-yyyy"))
                                    {
                                        if (item["InTime"].ToString() != "" && item["OutTime"].ToString() == "")
                                        {
                                            if(Convert.ToDateTime(item["ShiftOutTime"].ToString()) < DateTime.Now)
                                            {
                                                Random rand = new Random((int)clsStaticInfo.dbl(dataList[i]["EmployeePK"].ToString() + DateTime.Now.ToDbDate()));
                                                item["OutTime"] = Convert.ToDateTime(item["ShiftOutTime"]).AddMinutes(rand.Next(0,10)).ToString("HH:mm");
                                            }
                                        }
                                    }
                                    sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].Text = DayType + Environment.NewLine + item["ShiftCode"].ToString() + Environment.NewLine + item["InTime"].ToString() + Environment.NewLine + item["OutTime"].ToString();


                                }
                                else
                                {
                                    sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].Text = DayType;
                                    sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                }



                                if (DayType == "P")
                                {
                                    sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Interior.Color = System.Drawing.Color.Green;
                                    sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Font.Color = ExcelKnownColors.White;
                                }
                                else if (DayType == "A")
                                {
                                    sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Interior.Color = System.Drawing.Color.Red;
                                    sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Font.Color = ExcelKnownColors.White;
                                }
                                else if (DayType == "L" || DayType == "RST" || DayType == "LVL" || DayType == "WL" || DayType == "HL")
                                {
                                    sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Interior.Color = System.Drawing.Color.Blue;
                                    sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Font.Color = ExcelKnownColors.White;
                                }
                                else if (item["Category"].ToString() == "Leave")
                                {

                                    sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Interior.Color = System.Drawing.Color.Yellow;
                                    sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Font.Color = ExcelKnownColors.Black;
                                }


                            }

                            if (dicLeave.ContainsKey(dataList[i]["EmployeePK"].ToString()))
                            {
                                if (dataList[i]["EmployeePK"].ToString() == "1800080")
                                {
                                    string dd = "dde";
                                }

                                List<DataRow> drLeaveData = dicLeave[dataList[i]["EmployeePK"].ToString()];
                                foreach (DataRow item in drLeaveData)
                                {
                                    if (item["Code"].ToString() == "CL")
                                    {
                                        totalCL = Convert.ToDouble(item["LeaveDuration"].ToString());
                                    }
                                    if (item["Code"].ToString() == "EL")
                                    {
                                        totalEl = Convert.ToDouble(item["LeaveDuration"].ToString());
                                    }
                                    if (item["Code"].ToString() == "SL")
                                    {
                                        totalSL = Convert.ToDouble(item["LeaveDuration"].ToString());
                                    }
                                }
                            }



                        }

                        if (dayStatusReportType != "AllStatusInfo")
                        {
                            sheet1.Range[xlsRow, colTotalLeaveEL].Text = bplib.clsWebLib.GetNumData(totalEl.ToString());
                            sheet1.Range[xlsRow, colTotalLeaveCL].Text = bplib.clsWebLib.GetNumData(totalCL.ToString());
                            sheet1.Range[xlsRow, colTotalLeaveSL].Text = bplib.clsWebLib.GetNumData(totalSL.ToString());
                        }



                        xlsRow += 1;

                        #endregion ----------------------Data-----------------------

                    }





                    #region Line Setup
                    if (dayStatusReportType == "AllStatusInfo")
                    {
                        sheet1.Range[_StartRow, 1, xlsRow - 1, iAttendanceInfo + 1].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_StartRow, 1, xlsRow - 1, iAttendanceInfo + 1].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_StartRow, 1, xlsRow - 1, iAttendanceInfo + 1].WrapText = true;
                    }
                    else
                    {
                        sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].WrapText = true;
                    }
                    sheet1.Range[_StartRow, iSrNo, xlsRow, iEmpCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[_StartRow, iSrNo, xlsRow, iEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[_StartRow, iEmpCode + 1, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[_StartRow, iEmpCode + 1, xlsRow, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[_StartRow, iSrNo, xlsRow, iEmpCode].CellStyle.Font.Size = 45;

                    //sheet1.Range[_StartRow, iEmpCode, xlsRow, iEmpCode].CellStyle.Font.Size = 45;
                    sheet1.Range[_StartRow, iEmpCode + 1, xlsRow - 1, endXlsCol - 2].CellStyle.Font.Size = 60;
                    sheet1.Range[_StartRow, endXlsCol - 2, xlsRow - 1, endXlsCol].CellStyle.Font.Size = 55;


                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    //sheet1.UsedRange.CellStyle.Font.Size = 24;
                    //sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    //sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    #region ******************Report Header******************
                    xlsRow = 1;
                    sheet1.Range[xlsRow, endXlsCol - 5, xlsRow, endXlsCol - 1].Merge();
                    sheet1.Range[xlsRow, endXlsCol - 5].Text = "Color Indication";
                    sheet1.Range[xlsRow, endXlsCol - 5].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, endXlsCol - 5].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                    sheet1.Range[xlsRow, endXlsCol - 5].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, endXlsCol - 5].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow + 1, endXlsCol - 5].Text = "Present";
                    sheet1.Range[xlsRow + 1, endXlsCol - 4].CellStyle.Interior.Color = System.Drawing.Color.Green;
                    sheet1.Range[xlsRow + 1, endXlsCol - 5].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow + 1, endXlsCol - 5].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow + 1, endXlsCol - 3].Text = "Absent";
                    sheet1.Range[xlsRow + 1, endXlsCol - 2].CellStyle.Interior.Color = System.Drawing.Color.Red;
                    sheet1.Range[xlsRow + 1, endXlsCol - 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow + 1, endXlsCol - 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow + 2, endXlsCol - 5].Text = "Leave";
                    sheet1.Range[xlsRow + 2, endXlsCol - 4].CellStyle.Interior.Color = System.Drawing.Color.Yellow;
                    sheet1.Range[xlsRow + 2, endXlsCol - 5].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow + 2, endXlsCol - 5].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow + 2, endXlsCol - 3].Text = "Half Day Leave";
                    sheet1.Range[xlsRow + 2, endXlsCol - 3].WrapText = true;
                    //sheet1.Range[xlsRow + 2, endXlsCol - 2].CellStyle.Font.Size = 8;
                    sheet1.Range[xlsRow + 2, endXlsCol - 2].CellStyle.Font.Color = ExcelKnownColors.Yellow;
                    sheet1.Range[xlsRow + 2, endXlsCol - 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow + 2, endXlsCol - 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow + 2, endXlsCol - 2].Text = "Yellow Font";
                    sheet1.Range[xlsRow + 2, endXlsCol - 2].WrapText = true;
                    //sheet1.Range[xlsRow + 2, endXlsCol - 1].CellStyle.Font.Size = 8;
                    sheet1.Range[xlsRow + 2, endXlsCol - 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow + 2, endXlsCol - 2].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    sheet1.Range[xlsRow + 3, endXlsCol - 3].Text = "Late";
                    sheet1.Range[xlsRow + 3, endXlsCol - 2].CellStyle.Interior.Color = System.Drawing.Color.Blue;

                    sheet1.Range[xlsRow + 3, endXlsCol - 5].Text = "Out T Miss:";
                    sheet1.Range[xlsRow + 3, endXlsCol - 5].WrapText = true;
                    ////sheet1.Range[xlsRow + 3, endXlsCol - 4].CellStyle.Font.Size = 8;
                    sheet1.Range[xlsRow + 3, endXlsCol - 4].CellStyle.Interior.Color = System.Drawing.Color.Violet;

                    //sheet1.Range[xlsRow + 4, endXlsCol - 4].Text = "Manual Attdn:";
                    //sheet1.Range[xlsRow + 4, endXlsCol - 4].WrapText = true;
                    //sheet1.Range[xlsRow + 4, endXlsCol - 4].CellStyle.Font.Size = 8;
                    //sheet1.Range[xlsRow + 4, endXlsCol - 3].CellStyle.Interior.Color = System.Drawing.Color.Orange;

                    sheet1.Range[xlsRow + 4, endXlsCol - 3].Text = "Short Leave";
                    sheet1.Range[xlsRow + 4, endXlsCol - 3].WrapText = true;
                    //sheet1.Range[xlsRow + 4, endXlsCol - 2].CellStyle.Font.Size = 8;
                    sheet1.Range[xlsRow + 4, endXlsCol - 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow + 4, endXlsCol - 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow + 4, endXlsCol - 2].Text = "Maganta Font";
                    sheet1.Range[xlsRow + 4, endXlsCol - 2].WrapText = true;
                    //sheet1.Range[xlsRow + 4, endXlsCol 2- 1].CellStyle.Font.Size = 8;
                    sheet1.Range[xlsRow + 4, endXlsCol - 2].CellStyle.Font.Color = ExcelKnownColors.Magenta;
                    sheet1.Range[xlsRow + 4, endXlsCol - 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow + 4, endXlsCol - 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, endXlsCol - 5, xlsRow + 4, endXlsCol - 2].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, endXlsCol - 5, xlsRow + 4, endXlsCol - 2].CellStyle.Font.Size = 30;

                    #endregion ******************Report Header******************

                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;
                    sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                    #endregion

                    //sheet1.Range[11, 1, xlsRow, endXlsCol - 4].BorderInside(ExcelLineStyle.Hair);
                    //sheet1.Range[11, 1, xlsRow, endXlsCol - 4].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[11, 4, xlsRow, 4].WrapText = true;

                    //sheet1.UsedRange.CellStyle.Font.Size = 8;
                    reportUtility.PlantHeader(ref sheet1, endXlsCol - 6, "Monthly Attendance Information of " + m + "," + yearId, PlantId);
                    //  sheet1.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(endXlsCol - 5) + 5].Merge();
                    //sheet1.Range[1, 1, 4, endXlsCol - 5].Merge();
                    sheet1.Range[1, 1, 4, endXlsCol - 6].CellStyle.Font.Size = 70;
                    sheet1.Range[1, 1, 4, endXlsCol - 6].RowHeight = 75;
                    sheet1.Zoom = 37;
                    sheet1.Range[1, 1, 4, endXlsCol].CellStyle.Font.FontName = "Arial Narrow";
                    reportUtility.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);
                    sheet1.PageSetup.PrintTitleRows = "$" + 6 + ":$" + 7 + "";
                }
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<Dictionary<string, object>> GetMonthlyAttendance(string plantId, string yearId, string monthid, string monthStartDate, string complianceShiftList)
        {
            var wc = "";
            if (!string.IsNullOrEmpty(complianceShiftList))
            {
                wc = "where A.EmployeePK IN  (SELECT EmpSystemId FROM CompliedShiftDateWise WHERE CompliedShiftId IN (" + complianceShiftList + @") and Month(WorkDate) = " + monthid + @" and Year(WOrkdate) = " + yearId + @" )";
            }
            var dMonth = monthStartDate;
            var sql = @"SELECT A.* FROM
                                    (SELECT E.SystemId EmployeePK,E.FatherName EmployeeFatherName,CONVERT (int,E.EmployeeCode) EmployeeCode, E.EmployeeName, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ,REPLACE(CONVERT(VARCHAR(11), E.DOS, 113), ' ', '-') DOS, E.EmpType,
                                            DG.UserName GivenDesignation, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
                                            S.UserName Section, SB.UserName SubSection, L.UserName Line, REPLACE(CONVERT(VARCHAR(11), ADM.FromDate, 113), ' ', '-') FromDate,
                                            REPLACE(CONVERT(VARCHAR(11), ADM.ToDate, 113), ' ', '-') ToDate, ADM.MonthNo, ADM.YearNo,
                                            ADM.TotalProcDate, ADM.TotalPresent, ADM.TotalLate, ADM.TotalAbsent, ADM.TotalLv, ADM.TotalLWP, ADM.TotalMLv, ADM.TotalOTHr,
                                            ADM.TotalNormalOTHr, ADM.TotalExtraOTHr, ADM.TotalHoliDay, ADM.TotalWeekOff, ADM.TotalWeekOffHoliDay,SLeave.ShortLeave
                                    FROM dbo.EmployeeInformation E
                                                INNER JOIN dbo.AttdnDataMonthlySummary ADM ON E.SystemID = ADM.EmpSystemID
                                                
												LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=E.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department]  DP ON DP.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division]  DV ON DV.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] S ON S.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] SB ON SB.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] U ON U.Id = EN.UnitId                                    
                                    LEFT OUTER JOIN ORG.Line L on L.id=mpb.LineId
                                                LEFT JOIN HKP.Designation DG ON DG.Id=E.GivenDesignationId
                                            LEFT JOIN (
												SELECT EmpSystemID,sum(CountedShortLeave) ShortLeave,DATEPART(year,workdate) _year,DATEPART(month,workdate) _month
												 FROM AttdnProcessData
													WHERE PlantID='" + plantId + @"' --and DATEPART(year,workdate)=2018 and DATEPART(month,workdate)=9
													group by EmpSystemID,DATEPART(year,workdate),DATEPART(month,workdate)
													--having sum(CountedShortLeave)>2
												) SLeave on adm.MonthNo=SLeave._month and adm.YearNo=SLeave._year and e.SystemId=SLeave.EmpSystemID

                                    WHERE ADM.PlantID = '" + plantId + @"' AND ADM.MonthNo = '" + monthid + @"' AND ADM.YearNo = '" + yearId + @"' --and e.SystemId=1800001 
                        AND (EmployeeStatus = 'Active' OR COnvert(date,DOS) >= Convert(Date,'" + dMonth + @"'))

                        ) A  " + wc + @"
                        GROUP BY A.EmployeeCode, A.EmployeeName, A.DOJ,A.DOS, A.EmpType, A.GivenDesignation,A.Unit, A.Division, A.Department,
		                            A.Section, A.SubSection, A.Line, A.FromDate, A.ToDate, A.MonthNo, A.YearNo,
                                    A.TotalProcDate, A.TotalPresent, A.TotalLate, A.TotalAbsent, A.TotalLv,A.TotalLWP, A.TotalMLv, A.TotalOTHr,
                                    A.TotalNormalOTHr, A.TotalExtraOTHr, A.TotalHoliDay, A.TotalWeekOff, A.TotalWeekOffHoliDay,A.EmployeePK,A.ShortLeave,A.EmployeeFatherName
                        ORDER BY  A.EmployeeCode";

            if (_sqlRepository.GetDataCollection(sql).Count > 0)
            {

                return _sqlRepository.GetDataCollection(sql);
            }
            else
            {
                throw new Exception("No Data Found.");
            }
        }

        private DataTable GetMonthlyDailyShift(string shiftCode, ParaMontlyAttendance objm, string compliedShiftList)
        {
            var wc = "";
            if (!string.IsNullOrEmpty(compliedShiftList))
            {
                wc = "AND CS.Id IN(" + compliedShiftList + @")";
            }
            var strSql = @"DECLARE @sql_ nvarchar(max)

                                    select  EmployeePK,WorkDate, isnull(ShiftCode, '')  ShiftCode 
                                    INTO #tempOT
                                    from 
                                    (
                                    SELECT A.* FROM
	                                (SELECT E.systemId EmployeePK,E.EmployeeCode, E.EmployeeName, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ,
                                           LD.UserName Designation, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
                                            S.UserName Section, SB.UserName SubSection, L.UserName Line,  REPLACE(CONVERT(VARCHAR(11), AD.WorkDate, 113), ' ', '-') PDate,
                                             AD.DayStatus, CS.Code ShiftCode,  CONVERT(VARCHAR(5), AD.InTime, 108) InTime, ARIN.DeviceID InDeviceID, CONVERT(VARCHAR(5), AD.OutTime, 108) OutTime,
                                            AROUT.DeviceID OutDeviceID,  LT.UserName LvShortName
											,AD.WorkDate, DD.UserName GivenDesignation
                                    FROM dbo.EmployeeInformation E
                                                INNER JOIN AttdnProcessFinalData AD ON E.SystemID = AD.EmpSystemID
                                                LEFT JOIN dbo.AttdnRawData ARIN ON AD.ShiftID = ARIN.RowID
 LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                                                LEFT JOIN HKP.CompliedShift CS ON AD.ShiftID = CS.Id
                                                LEFT JOIN dbo.AttdnRawData AROUT ON AD.ShiftID = AROUT.RowID
                                                LEFT JOIN dbo.LeaveType LT ON AD.ShiftID = LT.Id
                                                LEFT JOIN ORG.Unit U ON EN.UnitID = U.Id
                                                LEFT JOIN ORG.Division Dv ON PR.DivisionID = Dv.Id
                                                LEFT JOIN ORG.Department Dp ON PR.DepartmentID = Dp.Id
                                                LEFT JOIN ORG.Section S ON PR.SectionID = S.Id
                                                LEFT JOIN ORG.SubSection SB ON pr.SubSectionID = SB.Id
                                                Left join HKP.LegalDesignation LD ON LD.Id = e.LegalDesignationId
                                                LEFT JOIN ORG.Line L ON mb.LineID = L.Id
												LEFT JOIN HKP.Designation DD ON E.GivenDesignationId = DD.Id
                                    WHERE AD.PlantID = '" + objm.PlantId + @"' AND AD.WorkDate BETWEEN '" + objm.FDate + @"' AND '" + objm.TDate + @"' --AND e.SystemId=1800001
                                    AND (E.EmployeeStatus='Active' or E.dos>'" + objm.FDate + @"' or e.dos is null) " + wc + @"
            ) A
                         GROUP BY A.EmployeeCode, A.EmployeeName, A.DOJ, A.Designation, A.Unit, A.Division, A.Department,
		                            A.Section, A.SubSection, A.Line, A.PDate, A.DayStatus, A.InTime, A.InDeviceID, A.OutTime,
                                    A.OutDeviceID, A.LvShortName, WorkDate, GivenDesignation, A.EmployeePK,A.ShiftCode 


                            ) TT
	                            DECLARE @sql nvarchar(max),
                                    @col nvarchar(max)

                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(REPLACE(CONVERT(VARCHAR(11), WorkDate, 113), ' ', '-'))	
                                FROM #tempOT 
                                FOR XML PATH ('')
                            )

                            SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                                MAX([ShiftCode]) FOR [WorkDate] IN ('+STUFF(@col,1,1,'')+')
                            ) as pvt)'

                            EXEC sp_executesql @sql
                            drop table #tempOT";

           
            if (_sqlRepository.GetDataTable(strSql).Rows.Count > 0)
            {

                return _sqlRepository.GetDataTable(strSql);
            }
            else
            {
                throw new Exception("No data found");
            }
        }
        private Dictionary<string, List<DataRow>> GetEmployeeShift(string plantId, string fromDate, string toDate)
        {
            string sql = @"SELECT ad.EmpSystemID,ad.WorkDate,CS.Code,DATEPART(day,ad.WorkDate) AS D
                                    FROM AttdnProcessFinalData AD 
                                                LEFT JOIN dbo.AttdnRawData ARIN ON AD.ShiftID = ARIN.RowID

                                                LEFT JOIN HKP.CompliedShift CS ON AD.ShiftID = CS.Id
                                                LEFT JOIN dbo.AttdnRawData AROUT ON AD.ShiftID = AROUT.RowID
                                                LEFT JOIN dbo.LeaveType LT ON AD.ShiftID = LT.Id
                                            
                                    WHERE AD.PlantID = '" + plantId + @"' AND AD.WorkDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' --AND e.SystemId=1800001
									order by ad.EmpSystemID";
            DataTable dt = _sqlRepository.GetDataTable(sql);
            Dictionary<string, List<DataRow>> dicShift = new Dictionary<string, List<DataRow>>();
            List<DataRow> _data = new List<DataRow>();
            string empId = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                {
                    _data = new List<DataRow>();
                    dicShift.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                }
                _data.Add(dt.Rows[i]);

                empId = dt.Rows[i]["EmpSystemID"].ToString();
            }

            return dicShift;
        }

        private Dictionary<string, List<DataRow>> GetMonthlyDailyAttendancedec(string attdnType, ParaMontlyAttendance objm)
        {
            string strSql = @" SELECT A.* FROM
	                                (SELECT E.systemId EmployeePK,E.FatherName,E.EmployeeCode, E.EmployeeName, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ,
                                            LD.UserName Designation, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
                                            S.UserName Section, SB.UserName SubSection, L.UserName Line,  REPLACE(CONVERT(VARCHAR(11), AD.WorkDate, 113), ' ', '-') PDate,
                                              AD.DayStatus,DT.Category,
											  ISNULL(Case when DT.Category = 'Late' then  'P' 
											  when DT.Category = 'Present' and AD.DayStatus <> 'OD' then 'P' 
											  else case when DT.Category = 'Present' and AD.DayStatus = 'OD' THEN  'OD' else AD.DayStatus END
											  end,'') as Dstatus										 
											   ,DATEPART(day,ad.WorkDate) AS D	
											 , CS.Code ShiftCode
											 	,Convert(Datetime,CONCAT(Format(AD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), CS.InTime, 108))) ShiftInTime
									--,Convert(Datetime,CONCAT(Format(AD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), CS.OutTime, 108))) ShiftOutTime
						
									,Case When Convert(Datetime,CONCAT(Format(AD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), CS.OutTime, 108))) < Convert(Datetime,CONCAT(Format(AD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), CS.InTime, 108))) then DateAdd(day,1,Convert(Datetime,CONCAT(Format(AD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), CS.OutTime, 108)))) Else Convert(Datetime,CONCAT(Format(AD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), CS.OutTime, 108))) END ShiftOutTime
								 , CONVERT(VARCHAR(5), AD.InTime, 108) InTime,  CONVERT(VARCHAR(5), AD.OutTime, 108) OutTime,
                                             LT.UserName LvShortName, LT.Code LEAVE
											, AD.WorkDate, DD.UserName GivenDesignation
                                    FROM dbo.EmployeeInformation E
 LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                                                INNER JOIN AttdnProcessFinalData AD ON E.SystemID = AD.EmpSystemID
                                                INNER JOIN AttdnProcessData APD ON E.SystemID = APD.EmpSystemID and APD.WorkDate = AD.WorkDate
												left Join DayType DT oN DT.DayType = AD.DayStatus
                                                LEFT JOIN dbo.LeaveType LT ON APD.LTSystemID = LT.Id												
												 LEFT JOIN HKP.CompliedShift CS ON AD.ShiftID = CS.Id
                                                --LEFT JOIN dbo.LeaveType LT ON APD.LTSystemID = LT.Id
                                                LEFT JOIN ORG.Unit U ON En.UnitID = U.Id
                                                LEFT JOIN ORG.Division Dv ON pr.DivisionID = Dv.Id
                                                LEFT JOIN ORG.Department Dp ON pr.DepartmentID = Dp.Id
                                                LEFT JOIN ORG.Section S ON pr.SectionID = S.Id
                                                LEFT JOIN ORG.SubSection SB ON pr.SubSectionID = SB.Id
                                                LEFT JOIN ORG.Line L ON mb.LineID = L.Id
                                                Left join HKP.LegalDesignation LD ON LD.Id = e.LegalDesignationId
												LEFT JOIN HKP.Designation DD ON E.GivenDesignationId = DD.Id
                                    WHERE AD.PlantID = '" + objm.PlantId + @"' AND AD.WorkDate BETWEEN '" + objm.FDate + @"' AND '" + objm.TDate + @"' --AND e.SystemId=1800001
                                    AND (E.EmployeeStatus='Active' or E.dos>'" + objm.FDate + @"' or e.dos is null)  ";

            strSql = strSql + @") A  order by A.EmployeeCode,A.WorkDate";
            DataTable dt = _sqlRepository.GetDataTable(strSql);

            Dictionary<string, List<DataRow>> dicShift = new Dictionary<string, List<DataRow>>();
            List<DataRow> _data = new List<DataRow>();
            string empId = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (empId != dt.Rows[i]["EmployeePK"].ToString())
                {
                    _data = new List<DataRow>();
                    dicShift.Add(dt.Rows[i]["EmployeePK"].ToString(), _data);
                }
                _data.Add(dt.Rows[i]);

                empId = dt.Rows[i]["EmployeePK"].ToString();
            }

            return dicShift;
        }

        private Dictionary<string, List<DataRow>> GetMonthlyLeaveDetail(ParaMontlyAttendance objm)
        {
            string strSql = @" SELECT e.SystemID EmployeePK,Lt.Code,sum(LTD.LeaveDuration) AS LeaveDuration FROM dbo.EmployeeInformation E
                                INNER JOIN AttdnProcessFinalData AD ON E.SystemID = AD.EmpSystemID
                                INNER JOIN AttdnProcessData APD ON E.SystemID = APD.EmpSystemID and APD.WorkDate = AD.WorkDate
                                --LEFT JOIN dbo.AttdnRawData ARIN ON AD.ShiftID = ARIN.RowID
                                inner JOIN dbo.LeaveType LT ON APD.LTSystemID = LT.Id												
                                inner JOIN  LeaveTransaction LTR on LTR.EmpSystemID = E.SystemId and Lt.Id = LTR.LTSystemID	and LTR.IsApproved =1	 --and LTR.WorkDate between '01-Dec-2019' AND '31-Dec-2019'										
                                inner JOIN  LeaveTransactionDetails LTD ON LTR.SystemID = LTD.LvTrnsSystemID and APD.WorkDate = LTD.WorkDate  and LTD.IsAvailed = 1  --and LTD.WorkDate BETWEEN '01-Dec-2019' AND '31-Dec-2019' 
                                WHERE AD.PlantID = '" + objm.PlantId + @"' AND AD.WorkDate BETWEEN '" + objm.FDate + @"' AND '" + objm.TDate + @"' 
                                AND (E.EmployeeStatus='Active' or E.dos>'" + objm.FDate + @"' or e.dos is null)          
                                                                group by   e.SystemID,Lt.Code,E.EmployeeCode    
                                order by e.SystemID    
                                   ";

            DataTable dt = _sqlRepository.GetDataTable(strSql);

            Dictionary<string, List<DataRow>> dicShift = new Dictionary<string, List<DataRow>>();
            List<DataRow> _data = new List<DataRow>();
            string empId = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (empId != dt.Rows[i]["EmployeePK"].ToString())
                {
                    _data = new List<DataRow>();
                    dicShift.Add(dt.Rows[i]["EmployeePK"].ToString(), _data);
                }
                _data.Add(dt.Rows[i]);

                empId = dt.Rows[i]["EmployeePK"].ToString();
            }

            return dicShift;
        }


        private DataTable GetMonthlyDailyAttendance(string attdnType, ParaMontlyAttendance objm)
        {
            string statusString = "";
            if (attdnType == "DayStatus")
            {
                statusString = @", CASE WHEN(ISNULL(DayStatus,'') = 'W' OR ISNULL(DayStatus,'') = 'H') THEN
                                     ISNULL(ISNULL(DayStatus, ''), '')
									 WHEN ISNULL(DayStatus,'') = 'LV' THEN
                                     ISNULL(ISNULL(DayStatus,'') + ISNULL(LEAVE, ''), '')
                                     WHEN ISNULL(DayStatus,'') != 'LV' THEN
                                     ISNULL(ISNULL(DayStatus, ''), '') END   ";
            }
            if (attdnType == "AllStatusInfo")
            {
                statusString = @", CASE WHEN(ISNULL(DayStatus,'') = 'W' OR ISNULL(DayStatus,'') = 'H') THEN
                                     ISNULL(ISNULL(DayStatus, '') + ',' + ISNULL(ShiftCode, '') + ',' + '' + ',' + '' + ',', '')
                                     WHEN ISNULL(DayStatus,'') = 'LV' THEN
                                        ISNULL(ISNULL(DayStatus,'') + ','+ISNULL(LEAVE, '') + ',' + ISNULL(ShiftCode, '') + ',' + '' + ',' + '' + ',', '')
                                     
                                     WHEN ISNULL(DayStatus,'') != 'LV' THEN
                                     ISNULL(ISNULL(DayStatus, '') + ',' + ISNULL(ShiftCode, '') + ',' + ISNULL(InTime, '') + ',' + ISNULL(OutTime, '') + ',', '') END";
            }
            var strSql = @"DECLARE @sql_ nvarchar(max)

                                    SELECT  EmployeePK,WorkDate
                                    " + statusString + @" DayStatus
                                    INTO #tempOT
                                    from 
                                    (
                                    SELECT A.* FROM
	                                (SELECT E.systemId EmployeePK,E.EmployeeCode, E.EmployeeName, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ,
                                            LD.UserName Designation, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
                                            S.UserName Section, SB.UserName SubSection, L.UserName Line,  REPLACE(CONVERT(VARCHAR(11), AD.WorkDate, 113), ' ', '-') PDate,
                                             AD.DayStatus, CS.Code ShiftCode,  CONVERT(VARCHAR(5), AD.InTime, 108) InTime, ARIN.DeviceID InDeviceID, CONVERT(VARCHAR(5), AD.OutTime, 108) OutTime,
                                            AROUT.DeviceID OutDeviceID, LT.UserName LvShortName, LT.Code LEAVE
											,AD.WorkDate, DD.UserName GivenDesignation
                                    FROM dbo.EmployeeInformation E
                                                INNER JOIN AttdnProcessFinalData AD ON E.SystemID = AD.EmpSystemID
                                                INNER JOIN AttdnProcessData APD ON E.SystemID = APD.EmpSystemID and APD.WorkDate = AD.WorkDate

                                                LEFT JOIN dbo.AttdnRawData ARIN ON AD.ShiftID = ARIN.RowID
 LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                                                LEFT JOIN HKP.CompliedShift CS ON AD.ShiftID = CS.Id
                                                LEFT JOIN dbo.AttdnRawData AROUT ON AD.ShiftID = AROUT.RowID
                                                LEFT JOIN dbo.LeaveType LT ON APD.LTSystemID = LT.Id
                                                LEFT JOIN ORG.Unit U ON EN.UnitID = U.Id
                                                LEFT JOIN ORG.Division Dv ON PR.DivisionID = Dv.Id
                                                LEFT JOIN ORG.Department Dp ON PR.DepartmentID = Dp.Id
                                                LEFT JOIN ORG.Section S ON PR.SectionID = S.Id
                                                LEFT JOIN ORG.SubSection SB ON PR.SubSectionID = SB.Id
                                                LEFT JOIN ORG.Line L ON MB.LineID = L.Id
Left join HKP.LegalDesignation LD ON LD.Id = e.LegalDesignationId
												LEFT JOIN HKP.Designation DD ON E.GivenDesignationId = DD.Id
                                    WHERE AD.PlantID = '" + objm.PlantId + @"' AND AD.WorkDate BETWEEN '" + objm.FDate + @"' AND '" + objm.TDate + @"' --AND e.SystemId=1800001
                                    AND (E.EmployeeStatus='Active' or E.dos>'" + objm.FDate + @"' or e.dos is null) ";



            strSql = strSql + @") A
                         GROUP BY A.EmployeeCode, A.EmployeeName, A.DOJ, A.Designation, A.Unit, A.Division, A.Department,
		                            A.Section, A.SubSection, A.Line, A.PDate, A.DayStatus, A.InTime, A.InDeviceID, A.OutTime,
                                    A.OutDeviceID, A.LvShortName, WorkDate, GivenDesignation, A.EmployeePK,A.ShiftCode,A.LEAVE


                            ) TT
	                            DECLARE @sql nvarchar(max),
                                    @col nvarchar(max)

                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(REPLACE(CONVERT(VARCHAR(11), WorkDate, 113), ' ', '-'))	
                                FROM #tempOT 
                                FOR XML PATH ('')
                            )

                            SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                                MAX([DayStatus]) FOR [WorkDate] IN ('+STUFF(@col,1,1,'')+')
                            ) as pvt)'

                            EXEC sp_executesql @sql
                            drop table #tempOT";
            if (_sqlRepository.GetDataTable(strSql).Rows.Count > 0)
            {

                return _sqlRepository.GetDataTable(strSql);
            }
            else
            {
                throw new Exception("No Attendance Data Found");
            }
        }
        private List<Dictionary<string, object>> GetDailyCompliance(string plantId, string workDate)
        {
            var sql = @"SELECT EmpSystemId, EmployeeCode, EmployeeName, DOJ, DOS
                        	,dti,dto,PDate,DayStatus,InTime,ShiftName,ShiftInTimeShow,ShiftOutTimeShow,InTimeShow,OutTimeShow,LeastPunchTime,OutTime,ShiftInTime,PlantID,'0' Duration,'0' LateBy
                        	,Designation,GivenDesignation,EmpCategory,Line,SubSection,Section,Department,Division,Unit
                        FROM (
                        	SELECT E.SystemId EmpSystemId,CONVERT(int, E.EmployeeCode) EmployeeCode,E.EmployeeName
                        		,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
                                ,REPLACE(CONVERT(VARCHAR(11), E.DOS, 113), ' ', '-') DOS
                        		,AD.InTime dti,AD.OutTime dto
                        		,REPLACE(CONVERT(VARCHAR(11), AD.WorkDate, 113), ' ', '-') PDate
                        		,AD.DayStatus
                        		,CONVERT(VARCHAR(15), CAST(LIT.ptime AS TIME), 100) + ' (' + ARD.PType + ')' LeastPunchTime
                        		,CONVERT(VARCHAR(5), AD.InTime, 108) InTime
                        		,CONVERT(VARCHAR(15), CAST(AD.InTime AS TIME), 100) InTimeShow
                        		,CONVERT(VARCHAR(5), AD.OutTime, 108) OutTime
                        	    ,AD.OutTime  OutTimeShow
                                --,CONVERT(VARCHAR(15), CAST(AD.OutTime AS TIME), 100) OutTimeShow
                        		,CONVERT(VARCHAR(5), SD.InTime, 108) ShiftInTime
                        		,CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100) ShiftInTimeShow
                        	    ,CASE When Convert(Datetime,CONCAT(Format(AD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), SD.OutTime, 108))) < Convert(Datetime,CONCAT(Format(AD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), SD.InTime, 108))) then DateAdd(day,1,Convert(Datetime,CONCAT(Format(AD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), SD.OutTime, 108)))) Else Convert(Datetime,CONCAT(Format(AD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), SD.OutTime, 108))) END ShiftOutTimeShow
                        		,SD.ShiftName,AD.PlantID,E.GivenDesignationId
                        		,LD.UserName Designation,GVD.UserName GivenDesignation,L.UserName Line
                        		,U.UserName Unit,Dv.UserName Division,SubDv.UserName SubDivision
                        		,Dp.UserName Department,S.UserName Section,SB.UserName SubSection
                        		,EC.UserName AS EmpCategory FROM dbo.EmployeeInformation E
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                        	INNER JOIN (SELECT * FROM dbo.AttdnProcessFinalData) AD ON E.SystemID = AD.EmpSystemID
                        	LEFT JOIN HKP.CompliedShift SD ON AD.ShiftID = SD.Id
                        	LEFT JOIN (SELECT LogDownLoadNum,min(ptime) ptime FROM AttdnRawData	WHERE pdate = '" + workDate + @"'
                        		GROUP BY LogDownLoadNum) LIT ON LIT.LogDownLoadNum = E.SystemId
                        	LEFT JOIN AttdnRawData ARD ON ARD.LogDownLoadNum = LIT.LogDownLoadNum AND ARD.PTime = LIT.ptime
                        	LEFT JOIN org.Unit U ON EN.UnitID = U.Id
                        	LEFT JOIN org.Division Dv ON PR.DivisionID = Dv.Id
                        	LEFT JOIN org.SubDivision SubDv ON PR.SubdivisionID = SubDv.Id
                        	LEFT JOIN org.Department Dp ON PR.DepartmentID = Dp.Id
                        	LEFT JOIN org.Section S ON PR.SectionID = S.Id
                        	LEFT JOIN org.SubSection SB ON PR.SubSectionID = SB.Id
                        	LEFT JOIN org.Line L ON MB.LineID = L.Id
Left join HKP.LegalDesignation LD ON LD.Id = E.LegalDesignationId
                        	LEFT JOIN hkp.Designation GVD ON E.GivenDesignationId = GVD.Id
                        		LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                            LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
                        	WHERE AD.WorkDate = '" + workDate + @"' AND (EmployeeStatus = 'Active' OR COnvert(date,E.DOS) >= Convert(Date,'" + workDate + @"'))
) A
                        WHERE PlantID = '" + plantId + @"'
                        GROUP BY EmpSystemId,EmployeeCode,EmployeeName,DOJ,DOS,dti,dto,PDate,DayStatus,InTime,LeastPunchTime
                        	,OutTime,ShiftName,ShiftInTimeShow,ShiftOutTimeShow,InTimeShow
                        	,OutTimeShow,ShiftInTime,PlantID,Designation
                        	,GivenDesignation,EmpCategory,Line,SubSection,Section,Department
                        	,Division,Unit 
                        	ORDER BY  DayStatus ,EmployeeCode
                    
";
            if (_sqlRepository.GetDataCollection(sql).Count > 0)
            {
                return _sqlRepository.GetDataCollection(sql);

            }
            else
            {
                throw new Exception("No data found");
            }
        }
        /// <summary>
        /// SQL for Getting Employee Information of Shifts with max shift 
        /// </summary>
        /// <param name="rotationDate"></param>
        /// <returns></returns>
        private DataTable GetEmpInfo(DateTime rotationDate)
        {
            try
            {
                var cmdText = @"SELECT CER.EmpSystemId,EmpInfo.PlantId,CER.CompliedShiftRosterMasterID,CSER.CompliedShiftId,CSER.ShiftSequence,yy.sSeq MaxSeq
                                 FROM CompliedEmployeeRoster CER 
								    LEFT JOIN EmployeeInformation EmpInfo ON  CER.EmpSystemId = EmpInfo.SystemId
	                                LEFT JOIN CompliedShiftRosterChild CSER ON CER.CompliedShiftRosterMasterID = CSER.CompliedShiftRosterMasterID
	                                LEFT JOIN(Select max(ShiftSequence) sSeq,CompliedShiftRosterMasterID  from CompliedShiftRosterChild group by CompliedShiftRosterMasterID) yy ON CER.CompliedShiftRosterMasterID = yy.CompliedShiftRosterMasterID

	                                INNER JOIN EmployeeInformation EI ON EI.SystemId = CER.EmpSystemId
	                                  WHERE EmpSystemId NOT IN(
		                                SELECT TOP 1 WITH TIES EmpSystemId
			                                FROM CompliedShiftAssignment
				                                WHERE WorkDate >=  Convert(Date,'" + rotationDate.ToString("yyyy-MM-dd") + @"') --GETDATE()
			                                ORDER BY ROW_NUMBER() 
		                                OVER(PARTITION BY EmpSystemId ORDER BY WorkDate DESC))
                                AND EI.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataTable(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        /// <param name="rotationDate">to make distinction of  calling method from TS or Applications</param>
        /// <param name="requestType"></param>
        public void CompliedshiftChange(string addedBy, string ip, string appVersion, DateTime rotationDate, string requestType)
        {
            var rotateDate = Convert.ToDateTime(rotationDate.ToString("dd-MMM-yyyy"));  //Roster Rotate Date
            var dtEmpInfo = GetEmpInfo(rotateDate);
            DataTable dtUniqueEmpSystemId = new DataTable();
            dtUniqueEmpSystemId = dtEmpInfo.DefaultView.ToTable(true, "EmpSystemId");
            List<string> empSystemId = new List<string>();

            for (int i = 0; i < dtUniqueEmpSystemId.Rows.Count; i++)
            {
                //GET THE LATEST COPLIEDSHIFT INFO PER EmpSystemId
                var latestCompShInfo = @"SELECT B.WorkDate,B.EmpSystemId,CSA.CompliedShiftId,CSRC.ShiftSequence,yy.sSeq MaxSeq--,CSRC.CompliedShiftRosterMasterID
                                            FROM CompliedShiftAssignment CSA INNER JOIN
                                                (
                                                SELECT  MAX(WorkDate) WorkDate
                                                ,EmpSystemId 
	                                                FROM CompliedShiftAssignment
                                                  WHERE CONVERT(DATE,WorkDate) < CONVERT(DATE,'" + rotateDate.ToString("dd-MMM-yyyy") + @"')  --GETDATE() 
                                                  GROUP BY EmpSystemId  
                                                ) B ON b.EmpSystemId= CSA.EmpSystemId
											
											    INNER JOIN CompliedEmployeeRoster CER on CER.EmpSystemId = CSA.EmpSystemId
												INNER JOIN CompliedShiftRosterChild CSRC 
												ON 
											    CSRC.CompliedShiftId = CSA.CompliedShiftId and 
												CSRC.CompliedShiftRosterMasterID = CER.CompliedShiftRosterMasterID
	                                           LEFT JOIN(Select max(ShiftSequence) sSeq,CompliedShiftRosterMasterID  from CompliedShiftRosterChild group by CompliedShiftRosterMasterID) 
                                                   yy ON CER.CompliedShiftRosterMasterID = yy.CompliedShiftRosterMasterID
												INNER JOIN CompliedShiftRosterMaster CSRM 
												ON CSRM.Id = CSRC.CompliedShiftRosterMasterID                                            
											AND b.WorkDate = csa.WorkDate WHERE  b.EmpSystemId is not null 
                                            AND b.EmpSystemId  = '" + dtUniqueEmpSystemId.Rows[i]["EmpSystemId"] + "'";
                var dtCmpShId = _sqlRepository.GetDataTable(latestCompShInfo);//latest complianceShiftId,ComplienceshiftSequence,EmpSystemId

                #region Data Insertion               
                CompliedShiftAssignment ent2 = new CompliedShiftAssignment();
                var newSeq = 0;
                DataTable dtEmpInfoSeqFitered;
                DataTable dtEmpInfoShiftFitered;

                if (dtCmpShId.Rows.Count > 0) //dtCmpShId=> DataTable Compliance Shift Id
                {
                    #region GetSequence for Insertion
                    using (var dvEmpInfo = new DataView(dtEmpInfo)
                    {

                        RowFilter = "EmpSystemId = '" + Convert.ToString(dtCmpShId.Rows[0]["EmpSystemId"]) + @"'",
                    })
                    {
                        dtEmpInfoSeqFitered = dvEmpInfo.ToTable();
                    }
                    if (Convert.ToInt32(dtCmpShId.Rows[0]["ShiftSequence"]) >= Convert.ToInt32(dtCmpShId.Rows[0]["MaxSeq"]))
                    {
                        newSeq = Convert.ToInt32(dtEmpInfoSeqFitered.Compute("min([ShiftSequence])", string.Empty));
                    }
                    else
                    {
                        newSeq = Convert.ToInt32(dtCmpShId.Rows[0]["ShiftSequence"]) + 1;
                    }
                    #endregion

                    #region GetShiftId Insertion
                    using (var dvEmpInfo = new DataView(dtEmpInfo)
                    {
                        RowFilter = "EmpSystemId = '" + Convert.ToString(dtCmpShId.Rows[0]["EmpSystemId"]) + @"' AND ShiftSequence = " + newSeq + "",
                    })
                    {
                        dtEmpInfoShiftFitered = dvEmpInfo.ToTable();
                    }
                    var newShift = dtEmpInfoShiftFitered.Rows[0]["CompliedShiftId"];
                    #endregion


                    ent2.Id = GetAutoNumber(nameof(CompliedShiftAssignment), PKGeneratorEnum.Auto, null, DateTime.Now);
                    ent2.PlantId = dtEmpInfoShiftFitered.Rows[0]["PlantId"].ToString();
                    ent2.CompliedShiftId = Convert.ToString(newShift);
                    ent2.EmpSystemID = dtEmpInfoShiftFitered.Rows[0]["EmpSystemId"].ToString();

                    ent2.WorkDate = rotateDate;
                    ent2.AddedBy = "TS";
                    ent2.AddedDate = DateTime.Now;
                    ent2.AddedFromIP = "TS";
                    ent2.ModelState = ModelState.Added;
                    base.Insert(ent2);
                    empSystemId.Add(dtEmpInfoShiftFitered.Rows[0]["EmpSystemId"].ToString());
                }
                #endregion
            }

            if (requestType == "TS")
            {
                SendShiftChangeMail(empSystemId, addedBy, ip, appVersion, rotateDate.ToString());
            }
        }

        public void SendShiftChangeMail(List<string> empSystemId, string addedBy, string ip, string appVersion, string rotateDate)
        {
            var Errorlog = new MailLog
            {
                AddedBy = addedBy,
                AddedDate = DateTime.Now,
                AddedFromIP = ip,
                AppVersion = appVersion,
                CompanyGroupId = null,
                ModelState = ModelState.Added,
                RecordTime = DateTime.Now,
                ServiceName = "ERROR-ComplientShiftRotationChangeNotification",
                UserId = null,
                AttachmentName = null,
                IsSuccess = false,
                SenderName = null,
                MailGenerator = MailGenerator.Scheduler.ToString()
            };
            try
            {
                var companyGroupList = _companyGroupRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                var companyList = _companyRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                var serviceName = MailServiceName.CompliedShiftChangeNotification.ToString();
                var fileName = serviceName + DateTime.Now.ToString("ddMMyyyyHHmmss");
                foreach (var companyGroup in companyGroupList)
                {
                    var log = new MailLog
                    {
                        AddedBy = addedBy,
                        AddedDate = DateTime.Now,
                        AddedFromIP = ip,
                        AppVersion = appVersion,
                        CompanyGroupId = companyGroup.Id,
                        ModelState = ModelState.Added,
                        RecordTime = DateTime.Now,
                        ServiceName = serviceName,
                        UserId = null,
                        AttachmentName = null,
                        IsSuccess = false,
                        SenderName = null,
                        MailGenerator = MailGenerator.Scheduler.ToString()
                    };
                    var mailServiceList = _mailReceiverServiceMappingRepository.Query(r => r.CompanyGroupId == companyGroup.Id && r.ServiceName == serviceName).Select().ToList();
                    if (mailServiceList.Count <= 0)
                    {
                        log.Remarks = "Mail service not found!";
                        _mailLogRepository.Insert(log);
                        _unitOfWork.SaveChanges();
                        break;
                    }
                    else
                    {
                        var smtpConfigurationCG = _smtpConfigurationService.Query(r => r.CompanyGroupId == companyGroup.Id).Select().FirstOrDefault();
                        foreach (var item in mailServiceList)
                        {
                            log.MailReceiverId = item.MailReceiverId;
                            log.SenderName = item.SenderName;
                            log.SenderEmail = item.SenderEmail;
                            log.Subject = item.Subject;
                            if (item.Active)
                            {
                                EmailSender email = null;
                                if (!string.IsNullOrEmpty(item.PlantId))
                                {
                                    var smtpConfigurationC = _smtpConfigurationService.Query(r => r.CompanyGroupId == companyGroup.Id && r.CompanyId == item.CompanyId).Select().FirstOrDefault();
                                    if (null == smtpConfigurationC)
                                        log.Remarks = string.Format(ResourcesCore.SMTPConfigNotFound.ToString(), "Company");
                                    else
                                        email = new EmailSender(smtpConfigurationC.Host, smtpConfigurationC.Port, smtpConfigurationC.MailingUserName, smtpConfigurationC.Password, true);
                                }
                                else
                                {
                                    if (null == smtpConfigurationCG)
                                        log.Remarks = string.Format(ResourcesCore.SMTPConfigNotFound.ToString(), "Company Group");
                                    else
                                        email = new EmailSender(smtpConfigurationCG.Host, smtpConfigurationCG.Port, smtpConfigurationCG.MailingUserName, smtpConfigurationCG.Password, true);
                                }

                                var emailList = GetMaileList(item);
                                if (emailList.Count <= 0)
                                {
                                    log.CompanyId = item.CompanyId;
                                    log.PlantId = item.PlantId;
                                    log.MailReceiverId = item.MailReceiverId;
                                    log.SenderName = item.SenderName;
                                    log.Subject = item.Subject;
                                    log.IsReciepientListActive = false;
                                    log.Remarks = "Reciepient List is not Active";
                                }
                                var toList = string.Join(";", emailList.Where(r => r.Active && r.MailType == "To" && r.Email != string.Empty).Select(r => r.FullName + "<" + r.Email + ">"));
                                log.ToList = toList;
                                var ccList = string.Join(";", emailList.Where(r => r.Active && r.MailType == "Cc" && r.Email != string.Empty).Select(r => r.FullName + "<" + r.Email + ">"));
                                log.CcList = ccList;
                                var bccList = string.Join(";", emailList.Where(r => r.Active && r.MailType == "Bcc" && r.Email != string.Empty).Select(r => r.FullName + "<" + r.Email + ">"));
                                log.BccList = bccList;
                                var inActiveList = string.Join(";", emailList.Where(r => !r.Active).Select(r => r.MailType + ":" + r.FullName));
                                if (toList == "")
                                {
                                    log.IsReciepientListActive = true;
                                    log.IsServiceActive = true;
                                    log.InactiveUsers = inActiveList;
                                    log.ToAddressProblem = "To List is Empty";
                                    var tmissingEmailList = string.Join(";", emailList.Where(r => r.Email == string.Empty).Select(r => r.MailType + ":" + r.FullName));
                                    if (tmissingEmailList == string.Empty)
                                        log.MissingEMails = null;
                                    else
                                        log.MissingEMails = tmissingEmailList.Substring(0, 500);
                                }
                                if (inActiveList == string.Empty)
                                    log.InactiveUsers = null;
                                else
                                    log.InactiveUsers = inActiveList;
                                var missingEmailList = string.Join(";", emailList.Where(r => r.Email == string.Empty).Select(r => r.MailType + ":" + r.FullName));
                                if (missingEmailList == string.Empty)
                                    log.MissingEMails = null;
                                else
                                    log.MissingEMails = missingEmailList;

                                var path = CreateShiftChangeListforXL(empSystemId, item.CompanyGroupId, item.PlantId, fileName, rotateDate);

                                if (!string.IsNullOrEmpty(path))
                                {
                                    try
                                    {
                                        var message = email.PrepareMessage(item.SenderName + "<" + item.SenderEmail + ">", toList, ccList, bccList, item.Subject, item.MessageBody);
                                        message.Attachments.Add(new Attachment(path));
                                        email.Send(message);
                                        log.AttachmentName = fileName + ".xls";
                                        log.IsSuccess = true;
                                        log.IsReciepientListActive = true;
                                        log.IsServiceActive = true;
                                        log.HasAttachment = true;
                                        log.Remarks = "Mail has been send successfully.";
                                    }
                                    catch (Exception ex)
                                    {
                                        log.IsSuccess = false;
                                        log.Remarks = ex.Message;
                                        continue;
                                    }
                                }
                                else if (item.IsSendMailIfEmptyData)
                                {
                                    try
                                    {
                                        var message = email.PrepareMessage(item.SenderName + "<" + item.SenderEmail + ">", toList, ccList, bccList, item.Subject, "No data to show.");
                                        email.Send(message);

                                        log.AttachmentName = null;
                                        log.Remarks = "Mail send with: No data found.";
                                        log.IsSuccess = true;
                                        log.IsReciepientListActive = true;
                                        log.IsServiceActive = true;
                                        log.HasAttachment = false;
                                    }
                                    catch (Exception ex)
                                    {
                                        log.IsSuccess = false;
                                        log.Remarks = serviceName + " - " + ex.Message;
                                        continue;
                                    }
                                }
                                else
                                {
                                    log.Remarks = "Mail not send for: No data found and Not permitted to send Email.";
                                    log.AttachmentName = null;
                                    log.IsSuccess = true;
                                    log.IsReciepientListActive = true;
                                    log.IsServiceActive = true;
                                    log.HasAttachment = false;
                                }
                            }
                            else
                            {
                                log.Remarks = "Service is inactive";
                            }
                        }
                    }
                    _mailLogRepository.Insert(log);
                    _unitOfWork.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Errorlog.Remarks = ex.Message;
                _mailLogRepository.Insert(Errorlog);
                _unitOfWork.SaveChanges();
                throw;
            }
        }
        //(item.CompanyGroupId, item.PlantId, fileName);

        public string CreateShiftChangeListforXL(List<string> empSystemId, string companyGroupId, string plantId, string fileName, string rotateDate)
        {
            try
            {
                //get ds from sql
                //var dtEmpInfo = GetShiftChangeedEmpInfo(empSystemId, companyGroupId, plantId);
                //set ds for excel
                return GetShiftChangedEmployeeListExcel(companyGroupId, plantId, "Employess of Rotated Shift", fileName, rotateDate);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetShiftChangeedEmpInfo(string companyGroupId, string plantId, string shiftId, String effectiveDate)
        {
            try
            {
                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

                var OrgStrList = OrgStructureList(companyGroupId);
                foreach (var item in OrgStrList)
                {
                    if (item.RType == "Entity")
                    {
                        cList += "," + item.ColumnName + ".UserName " + "e" + item.ColumnName + " ";

                        if (item.ColumnName == "EmployeeGroup")
                        {
                            Join += "LEFT JOIN [HKP].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = EN." + item.ColumnName + "Id\n";
                        }
                        else
                        {
                            Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = EN." + item.ColumnName + "Id\n";
                        }
                    }
                    else
                    {
                        cList += "," + item.ColumnName + ".UserName " + "p" + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = PO." + item.ColumnName + "Id\n";
                    }
                }

                var cmdText = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,Convert(INt,e.EmployeeCode) EmployeeCodeS,mpb.Code BudgetCode,e.EmployeeName
                                    --,e.DOJ
									,CSHIFT.ShiftName,  REPLACE(CONVERT(VARCHAR(11), CSA.WorkDate, 106), ' ', '-') shiftWorkDate
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB
                                    ,REPLACE(CONVERT(VARCHAR(11), e.ApprovedDateTime, 106), ' ', '-') ApprovedDateTime
                                    ,CONVERT(date, e.ApprovedDateTime) vApprovedDateTime
									,ISNULL(e.IsApproved,0) IsApproved
                                    --Resignation
								    ,ISNULL(rsg.ApprovalStatus,'') rsgApprovalStatus
									,REPLACE(CONVERT(VARCHAR(11), rsg.ApprovedEffectiveDate, 106), ' ', '-') resignationApprovedEffectiveDate
                                    ,CONVERT(DATE, rsg.ApprovedEffectiveDate) resignationApprovedEffectiveDateS
									,REPLACE(CONVERT(VARCHAR(11), rsg.ResignationDate, 106), ' ', '-') ApplicantResignationDate
                                    --,REPLACE(CONVERT(VARCHAR(11), e.DOSDate, 106), ' ', '-') ApplicantSeparationDate
								    ,CONVERT(DATE, rsg.ApprovedDate) resignationApprovedEntryDateS
									,CONVERT(date,rsg.AddedDate) resignationAddedDate
								    ,CONVERT(DATE, rsg.EffectiveDate) EffectiveDateS
									,REPLACE(CONVERT(VARCHAR(11),rsg.EffectiveDate,106),' ','-') RsgSelfEffectiveDate
                                    ,ProbationPeriod= case when e.DOCIsDay=1 then e.DOCDay
									ELSE e.DOCMonth*30 end
									,e.DOCDay,e.DOCMonth
                                    ,e.EmployeeStatus EmployeeStatus
									,e.DOJ+(CASE WHEN e.DOCIsDay=1 THEN e.DOCDay
												ELSE e.DOCMonth*30 END) DOCs
									,Replace(CONVERT(VARCHAR(11),
									e.DOJ+(case when e.DOCIsDay=1 then e.DOCDay
												else e.DOCMonth*30 end)
									 , 106), ' ', '-') DOC
                                    ,Replace(CONVERT(VARCHAR(11),e.DOC,106),' ','-') empDOC
                                    ,Replace(CONVERT(VARCHAR(11),(e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end - ProbationPeriodAlertBeforeDays)), 106), ' ', '-') AlermStartDate

                                    ,DATEDIFF(day, GETDATE(), (e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end))) DaysToGO
									,DATEDIFF(day, GETDATE(), rsg.ApprovedEffectiveDate) RsgDaysToGO
	                                ,e.DOJ + (case when e.DOCIsDay=1 then e.DOCDay
									else e.DOCMonth*30 end - ProbationPeriodAlertBeforeDays)  AlermStartDateCmp

                                        ,Replace(CONVERT(VARCHAR(11),PRE.ConfirmationDate,106),' ','-') PREConfirmationDate
									 ,convert(date,PRE.ConfirmationDate) PREConfirmationDateExc
									  , pre.Completed preCompleted
                                    ,isnull(e.IsConfirmed,0) IsConfirmedProbation
									--Probation Confirmation Date
									,CONVERT(DATE,e.ProbationConfirmEntryDate) ProbationConfirmEntryDate
                                    ,mpb.EntityId,mpb.PositionId,ISNULL(hs.IsPositionCodeApplicable,0) IsPositionCodeApplicable
									--Increment Due list
									--,SINDD.NextDueDate IncrementNextDueDate,SINDD.EffectiveDate IncrementEffectiveDate
                                    --emp ids
                                     ,e.DepartmentId,e.DivisionId,e.LineId
                                    ,e.PlantId,e.UnitId,e.SectionId,e.SubDivisionId,e.SubSectionId,e.DesignationGroupId
                                    --emp info
                                    ,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    ,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,LD.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
                                    ,EmpC.UserName empCategory,EmpC.Sequence CatgSequence, ELoc.UserName EmployeeLocation
									" + cList + @"

                                    FROM EmployeeInformation e
                                    LEFT OUTER JOIN ORG.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=e.LineId
                                    LEFT OUTER JOIN ORG.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN ORG.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN ORG.Section es on es.id=e.SectionId
                                    LEFT OUTER JOIN ORG.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN ORG.Unit eu on eu.id=e.UnitId
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                           -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=dm.DesignationGroupId
                                   -- left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								   LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT OUTER JOIN HKP.EmployeeLocation ELoc on mpb.EmployeeLocationId=ELoc.Id
			                                       " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId
                                    LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
			                      	LEFT OUTER JOIN CompliedShiftAssignment CSA ON CSA.EmpSystemId = E.SystemId
									LEFT OUTER JOIN HKP.CompliedShift CSHIFT ON CSHIFT.Id = CSA.CompliedShiftId
                                    WHERE e.EmployeeStatus = 'Active' AND " + param + @"
                                    AND CSA.CompliedShiftId = '" + shiftId + "' AND CSA.WorkDate = CONVERT(dATE,'" + Convert.ToDateTime(effectiveDate).ToString("yyyy-MM-dd") + "')";

                //if (dayStatus == "Work Off")
                //{
                //    cmdText += "AND  DT.DayType IN ('W','H','WP','HP','WA','HA')";
                //}
                //else
                //{
                //    cmdText += "AND  DT.Category = '" + dayStatus + "'";
                //}
                cmdText += " ORDER BY EmployeeCodeS ASC";
                return _sqlRepository.GetDataTable(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetShiftChangedEmployeeListExcel(string companyGroupId, string plantId, string SheetHeader, string SheetName, string rotateDate)
        {
            try
            {
                #region Variable
                //clsReport objRpt = null;
                var filePath = "";

                DataTable dtEntity = null;
                DataTable dtPosition = null;

                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                ReportUtility oRU = null;

                //StringCollection dayStatus = null;

                var xlsRow = 1;
                var xlsCol = 1;
                var IsBudgetCodeApplicable = true;

                #endregion Variable
                //objRpt = new clsReport();
                oRU = new ReportUtility();
                //plantId = "20188";
                var dtCompliedShift = _sqlRepository.GetDataTable(@"SELECT DISTINCT CS.* FROM CompliedShiftAssignment CSA LEFT JOIN HKP.CompliedShift  CS ON CSA.CompliedShiftId = CS.Id WHERE CSA.PlantId = '" + plantId + @"' ORDER BY CS.ShiftName");
                // cmdText += "AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,GETDATE()) ORDER BY EmployeeCodeS ASC";


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(dtCompliedShift.Rows.Count);

                if (dtCompliedShift.Rows.Count > 0)
                {
                    for (int dsi = 0; dsi < dtCompliedShift.Rows.Count; dsi++)
                    {

                        var daylyAttdnEmpInfo = GetShiftChangeedEmpInfo(companyGroupId, plantId, dtCompliedShift.Rows[dsi]["Id"].ToString(), rotateDate);

                        //sheet1 = workbook.Worksheets[Ec];
                        GetEntityPosition(companyGroupId, out DataSet dsEntityPosition);

                        var dvEntity = new DataView(dsEntityPosition.Tables[0])
                        {
                            RowFilter = "RType = 'Entity'",
                            Sort = "eSequence"
                        };
                        dtEntity = dvEntity.ToTable(true, "UserName");

                        var dvPosition = new DataView(dsEntityPosition.Tables[0])
                        {
                            RowFilter = "RType = 'Position'",
                            Sort = "pSequence"
                        };
                        dtPosition = dvPosition.ToTable(true, "UserName");

                        var dvBC = new DataView(daylyAttdnEmpInfo);
                        var dtBC = dvBC.ToTable(true, "IsPositionCodeApplicable");
                        for (int i = 0; i < dtBC.Rows.Count; i++)
                        {
                            IsBudgetCodeApplicable = Convert.ToBoolean(daylyAttdnEmpInfo.Rows[i]["IsPositionCodeApplicable"].ToString());
                            if (IsBudgetCodeApplicable)
                            {
                                break;
                            }
                        }
                        sheet1 = workbook.Worksheets[dsi];
                        sheet1.Name = dtCompliedShift.Rows[dsi]["ShiftName"].ToString();
                        if (daylyAttdnEmpInfo.Rows.Count > 0)
                        {
                            xlsRow = 5;


                            #region variable
                            var cEmployeeCode = 0; var cBudgetCode = 0; var cName = 0; var cDOJ = 0; var cDOB = 0;
#pragma warning disable CS0219 // The variable 'cLeaveType' is assigned but its value is never used
                            var cDesignation = 0; var cGivenDesignation = 0; var cLD = 0; var cLeaveType = 0;
#pragma warning restore CS0219 // The variable 'cLeaveType' is assigned but its value is never used
#pragma warning disable CS0219 // The variable 'cDayStatus' is assigned but its value is never used
                            var cDayStatus = 0; var cEmpCatg = 0; var cEmpLocation = 0; var cSl = 0;
#pragma warning restore CS0219 // The variable 'cDayStatus' is assigned but its value is never used
                            var endXlsCol = 0;
                            var colNum = 0;
                            #endregion variable

                            xlsRow++;
                            xlsCol = 1;

                            #region Header
                            oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sl. No.", 6); cSl = xlsCol; xlsCol++;
                            oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeCode"); cEmployeeCode = xlsCol; xlsCol++;
                            oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Name", 30); cName = xlsCol; xlsCol++;
                            oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOB"); cDOB = xlsCol; xlsCol++;
                            oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOJ"); cDOJ = xlsCol; xlsCol++;
                            oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Emp Category"); cEmpCatg = xlsCol; xlsCol++;
                            oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Emp Location"); cEmpLocation = xlsCol; xlsCol++;
                            //if (dayStatus[dsi] == "Leave")
                            //{
                            //    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "LeaveType"); cLeaveType = xlsCol; xlsCol++;
                            //}
                            //if (dayStatus[dsi] == "Work Off")
                            //{
                            //    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Day Status"); cDayStatus = xlsCol; xlsCol++;
                            //}
                            if (IsBudgetCodeApplicable)
                            {
                                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "BudgetCode"); cBudgetCode = xlsCol; xlsCol++;

                                for (int i = 0; i < dtEntity.Rows.Count; i++)
                                {
                                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, dtEntity.Rows[i]["UserName"].ToString(), 25); xlsCol++;
                                }
                                for (int c = 0; c < dtPosition.Rows.Count; c++)
                                {
                                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, dtPosition.Rows[c]["UserName"].ToString(), 25); xlsCol++;
                                }
                            }//IsBudgetCodeApplicable


                            oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", 25); cDesignation = xlsCol; xlsCol++;
                            oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "GivenDesignation", 25); cGivenDesignation = xlsCol; xlsCol++;
                            oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Legal Designation", 25); cLD = xlsCol; xlsCol++;

                            #endregion Header

                            var fPanRow = xlsRow + 1;//Freeze pan starting rows

                            xlsCol--;
                            endXlsCol = xlsCol;
                            xlsRow++;
                            var slCount = 0;
                            for (int i = 0; i < daylyAttdnEmpInfo.Rows.Count; i++)
                            {
                                slCount++;
                                #region Loop

                                oRU.SetText(ref sheet1, xlsRow, cSl, slCount.ToString());
                                oRU.SetText(ref sheet1, xlsRow, cEmployeeCode, daylyAttdnEmpInfo.Rows[i]["EmployeeCode"].ToString());

                                oRU.SetText(ref sheet1, xlsRow, cName, daylyAttdnEmpInfo.Rows[i]["EmployeeName"].ToString());
                                oRU.SetText(ref sheet1, xlsRow, cDOB, daylyAttdnEmpInfo.Rows[i]["DOB"].ToString());
                                oRU.SetText(ref sheet1, xlsRow, cDOJ, daylyAttdnEmpInfo.Rows[i]["DOJ"].ToString());
                                oRU.SetText(ref sheet1, xlsRow, cEmpCatg, daylyAttdnEmpInfo.Rows[i]["empCategory"].ToString());
                                oRU.SetText(ref sheet1, xlsRow, cEmpLocation, daylyAttdnEmpInfo.Rows[i]["EmployeeLocation"].ToString());

                                //if (dayStatus[dsi] == "Leave")
                                //{
                                //    oRU.SetText(ref sheet1, xlsRow, cLeaveType, daylyAttdnEmpInfo.Rows[i]["LeaveType"].ToString());
                                //}
                                //if (dayStatus[dsi] == "Work Off")
                                //{
                                //    oRU.SetText(ref sheet1, xlsRow, cDayStatus, daylyAttdnEmpInfo.Rows[i]["DayType"].ToString());
                                //}
                                if (Convert.ToBoolean(daylyAttdnEmpInfo.Rows[i]["IsPositionCodeApplicable"].ToString()))
                                {
                                    oRU.SetText(ref sheet1, xlsRow, cBudgetCode, daylyAttdnEmpInfo.Rows[i]["BudgetCode"].ToString());
                                    //entity

                                    for (int c = 0; c < dtEntity.Rows.Count; c++)
                                    {
                                        var _colname = dtEntity.Rows[c]["UserName"].ToString();
                                        var v = daylyAttdnEmpInfo.Rows[i]["e" + _colname].ToString();
                                        colNum = cBudgetCode + c + 1;
                                        oRU.SetText(ref sheet1, xlsRow, colNum, v);
                                    }

                                    //position

                                    for (int c = 0; c < dtPosition.Rows.Count; c++)
                                    {
                                        var _colname = dtPosition.Rows[c]["UserName"].ToString();
                                        oRU.SetText(ref sheet1, xlsRow, colNum + c + 1, daylyAttdnEmpInfo.Rows[i]["p" + _colname].ToString());
                                    }
                                }//is bc applicable

                                oRU.SetText(ref sheet1, xlsRow, cEmpLocation, daylyAttdnEmpInfo.Rows[i]["EmployeeLocation"].ToString());
                                oRU.SetText(ref sheet1, xlsRow, cDesignation, daylyAttdnEmpInfo.Rows[i]["Designation"].ToString());
                                oRU.SetText(ref sheet1, xlsRow, cGivenDesignation, daylyAttdnEmpInfo.Rows[i]["GivenDesignation"].ToString());
                                if (daylyAttdnEmpInfo.Rows[i]["Designation"].ToString().ToUpper() != daylyAttdnEmpInfo.Rows[i]["GivenDesignation"].ToString().ToUpper())
                                {
                                    sheet1.Range[xlsRow, cDesignation].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                    sheet1.Range[xlsRow, cDesignation].CellStyle.Font.Color = ExcelKnownColors.White;
                                    sheet1.Range[xlsRow, cGivenDesignation].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                    sheet1.Range[xlsRow, cGivenDesignation].CellStyle.Font.Color = ExcelKnownColors.White;
                                }

                                oRU.SetText(ref sheet1, xlsRow, cLD, daylyAttdnEmpInfo.Rows[i]["LegalDesignation"].ToString());

                                #endregion Loop
                                xlsRow++;
                            }

                            oRU.SetHeaderText(ref sheet1, 4, 1, dtCompliedShift.Rows[dsi]["ShiftName"].ToString() + " Shift", ExcelHAlign.HAlignCenter);
                            sheet1.Range[4, 1, 4, endXlsCol].Merge();
                            var attdnHeader = SheetHeader + " On " + DateTime.Now.ToString("dd-MMM-yyyy");
                            if (!string.IsNullOrEmpty(plantId))
                                oRU.PlantHeader(ref sheet1, endXlsCol, attdnHeader, plantId);
                            else
                                oRU.MainCompanyGroupHeader(ref sheet1, endXlsCol, attdnHeader, companyGroupId);

                            #region UsedRange Alignment
                            sheet1.UsedRange.WrapText = true;
                            sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                            sheet1.UsedRange["A" + fPanRow].FreezePanes();
                            #endregion UsedRange Alignment

                            oRU.PageSetupAuto(ref sheet1, 5, ExcelPageOrientation.Landscape, "TS");

                        }
                        //else
                        //{
                        //    throw new Exception("No Employee found");
                        //}
                    }
                    workbook.Version = ExcelVersion.Excel97to2003;
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + plantId + ".xls");
                    workbook.SaveAs(filePath);
                    workbook.Close();
                    excelEngine.Dispose();
                    return filePath;
                }
                else
                {
                    throw new Exception("Shift not found");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        List<SwapColumn> GetColDisplayName(DataTable dslocal)
        {
            List<SwapColumn> list = null;
            try
            {
                list = new List<SwapColumn>();
                for (int i = 0; i < dslocal.Columns.Count; i++)
                {
                    var c = dslocal.Columns[i].ColumnName;
                    if (c.ToUpper() != "EMPLOYEEPK")
                    {
                        string _date = Convert.ToDateTime(c).ToString("dd-MMM-yyyy");
                        string _day = Convert.ToDateTime(c).ToString("dd");
                        SwapColumn ob = new SwapColumn();
                        ob.DisplayMember = _date;
                        ob.ValueMember = _day;
                        list.Add(ob);
                    }//if
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook GetEmployeeJobCardReport(string PlantId, string PlantName, string CompanyId, string userName, string fromDate, string toDate, string emp)
        {
            ReportUtility reportUtility = new ReportUtility();
            DataView dvBioDvAC = null;
            StringCollection sEmpCodeColl = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string sOfficeInTime = "00:00:00";
            string sInTime = "00:00:00";

            DateTime dtFrmDate = Convert.ToDateTime(fromDate);
            DateTime dtToDate = Convert.ToDateTime(toDate);
            TimeSpan tsFromToDate = dtToDate - dtFrmDate;
            int daysFromTo = tsFromToDate.Days;
            if (daysFromTo < 0)
            {
                Exception ex = new Exception("Please check the access From Date, cannot more than access To Date...");
                throw (ex);
            }

            #region DataSet

            var dsBioDvAC = GetEmpJobCardInfoWithInDateTimeCompiled(emp, fromDate, toDate);

            var dsPayDays = GetJobCardPayDays(emp, fromDate);
            DataSet ds = new DataSet();
            ds.Tables.Add(dsPayDays);
            var ListPayDays = ds.Tables[0].ToList<PayDaysReport>();

            #endregion DataSet

            if (dsBioDvAC.Rows.Count > 0)
            {
                sEmpCodeColl = new StringCollection();

                for (int i = 0; i <= dsBioDvAC.Rows.Count - 1; i++)
                {
                    if (sEmpCodeColl.Contains(dsBioDvAC.Rows[i]["EmployeeCode"].ToString().Trim()) == false)
                    {
                        sEmpCodeColl.Add(dsBioDvAC.Rows[i]["EmployeeCode"].ToString().Trim());
                    }
                }

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(sEmpCodeColl.Count);
                for (int Ec = 0; Ec < sEmpCodeColl.Count; Ec++)
                {
                    dvBioDvAC = new DataView(dsBioDvAC);

                    dvBioDvAC.RowFilter = "EmployeeCode = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";
                    if (dvBioDvAC.Count > 0)
                    {
                        sheet1 = workbook.Worksheets[Ec];
                        sheet1.IsGridLinesVisible = true;

                        xlsRow = 6;

                        string strEmpCode = "";

                        int iDate = 0;
                        int iShiftIntime = 0;
                        int iInTime = 0;
                        int iOutTime = 0;
                        int iDayStatus = 0;
                        int iPayDays = 0;
                        int iLvShortName = 0;//
                        string strLateBy = "00:00:00";
                        int iLateBy = 0;
                        int iShiftName = 0;
                        int iShiftCode = 0;
                        int iShiftOuttime = 0;
                        for (int i = 0; i < dvBioDvAC.Count; i++)
                        {
                            if ((string.Compare(strEmpCode.ToUpper(), dvBioDvAC[i]["EmployeeCode"].ToString().Trim().ToUpper())) != 0)
                            {

                                #region ------------------Column Header------------------

                                xlsCol = 1;
                                xlsRow = 6;
                                sheet1.Range[xlsRow, xlsCol].Text = "Employee Code";
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["EmployeeCode"].ToString().Trim();
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                xlsCol = 1;
                                xlsRow += 1;
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Text = "DOJ";
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["DOJ"].ToString().Trim();
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                xlsCol = 1;
                                xlsRow += 1;
                                sheet1.Range[xlsRow, xlsCol].Text = "Unit";
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Unit"].ToString().Trim();
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                xlsCol = 1;
                                xlsRow += 1;
                                sheet1.Range[xlsRow, xlsCol].Text = "Department";
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Department"].ToString().Trim();
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                xlsCol = 1;
                                xlsRow += 1;
                                sheet1.Range[xlsRow, xlsCol].Text = "Given Designation";
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["GivenDesignation"].ToString().Trim();
                                //sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Designation"].ToString().Trim();
                                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                xlsRow += 1;
                                xlsCol = 5;
                                xlsRow = 6;

                                sheet1.Range[xlsRow, xlsCol].Text = "Employee Name";
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["EmployeeName"].ToString().Trim();
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                xlsRow += 1;

                                sheet1.Range[xlsRow, xlsCol].Text = "Division";
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Division"].ToString().Trim();
                                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                xlsRow += 1;
                                sheet1.Range[xlsRow, xlsCol].Text = "Section";
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Section"].ToString().Trim();
                                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                xlsRow += 1;
                                sheet1.Range[xlsRow, xlsCol].Text = "SubSection";
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["SubSection"].ToString().Trim();
                                sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                xlsRow += 1;
                                sheet1.Range[xlsRow, xlsCol].Text = "Legal Designation";
                                sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["LegalDesignation"].ToString().Trim();
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                xlsCol = 1;
                                iDate = xlsCol;
                                xlsRow += 2;
                                sheet1.Range[xlsRow, iDate].Text = "Date";
                                sheet1.Range[xlsRow, iDate].ColumnWidth = 12;
                                sheet1.Range[xlsRow, iDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                xlsCol += 1;
                                iShiftCode = xlsCol;
                                sheet1.Range[xlsRow, iShiftCode].Text = "Shift Code";
                                sheet1.Range[xlsRow, iShiftCode].ColumnWidth = 12;
                                sheet1.Range[xlsRow, iShiftCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iShiftCode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                xlsCol += 1;
                                iShiftName = xlsCol;
                                sheet1.Range[xlsRow, iShiftName].Text = "Shift Name";
                                sheet1.Range[xlsRow, iShiftName].ColumnWidth = 12;
                                sheet1.Range[xlsRow, iShiftName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iShiftName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                xlsCol += 1;
                                iShiftIntime = xlsCol;
                                sheet1.Range[xlsRow, iShiftIntime].Text = "Shift InTime";
                                sheet1.Range[xlsRow, iShiftIntime].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iShiftIntime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iShiftIntime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                xlsCol += 1;
                                iShiftOuttime = xlsCol;
                                sheet1.Range[xlsRow, iShiftOuttime].Text = "Shift OutTime";
                                sheet1.Range[xlsRow, iShiftOuttime].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iShiftOuttime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iShiftOuttime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                xlsCol += 1;
                                iInTime = xlsCol;
                                sheet1.Range[xlsRow, iInTime].Text = "InTime";
                                sheet1.Range[xlsRow, iInTime].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                xlsCol += 1;
                                iOutTime = xlsCol;
                                sheet1.Range[xlsRow, iOutTime].Text = "OutTime";
                                sheet1.Range[xlsRow, iOutTime].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                //xlsCol += 1;
                                //iOTHr = xlsCol;
                                //sheet1.Range[xlsRow, iOTHr].Text = "Duration";
                                //sheet1.Range[xlsRow, iOTHr].ColumnWidth = 9;
                                //sheet1.Range[xlsRow, iOTHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                //sheet1.Range[xlsRow, iOTHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                xlsCol += 1;
                                iDayStatus = xlsCol;
                                sheet1.Range[xlsRow, iDayStatus].Text = "Day Status";
                                sheet1.Range[xlsRow, iDayStatus].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                xlsCol += 1;
                                iPayDays = xlsCol;
                                sheet1.Range[xlsRow, iPayDays].Text = "Pay Days";
                                sheet1.Range[xlsRow, iPayDays].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iPayDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iPayDays].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                xlsCol += 1;
                                iLateBy = xlsCol;
                                sheet1.Range[xlsRow, iLateBy].Text = "Late By";
                                sheet1.Range[xlsRow, iLateBy].ColumnWidth = 7;
                                sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                xlsCol += 1;
                                iLvShortName = xlsCol;
                                sheet1.Range[xlsRow, iLvShortName].Text = "LV";
                                sheet1.Range[xlsRow, iLvShortName].ColumnWidth = 6;
                                sheet1.Range[xlsRow, iLvShortName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iLvShortName].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                endXlsCol = xlsCol;

                                #endregion ------------------Column Header------------------
                            }
                            strEmpCode = dvBioDvAC[i]["EmployeeCode"].ToString().Trim();

                            #region ----------------------Data-----------------------

                            xlsRow += 1;
                            sheet1.Range[xlsRow, iDate].Text = dvBioDvAC[i]["PDate"].ToString();
                            sheet1.Range[xlsRow, iDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iShiftCode].Text = dvBioDvAC[i]["ShiftCode"].ToString();
                            sheet1.Range[xlsRow, iShiftCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iShiftCode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iShiftName].Text = dvBioDvAC[i]["ShiftName"].ToString();
                            sheet1.Range[xlsRow, iShiftName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iShiftName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iShiftIntime].Text = dvBioDvAC[i]["ShiftIntime"].ToString();
                            sheet1.Range[xlsRow, iShiftIntime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iShiftIntime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iShiftOuttime].Text = dvBioDvAC[i]["ShiftOutTime"].ToString();
                            sheet1.Range[xlsRow, iShiftOuttime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iShiftOuttime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "LV" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "H")
                            {
                                sheet1.Range[xlsRow, iInTime].Text = "";
                                sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iOutTime].Text = "";
                                sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            }
                            else
                            {
                                sheet1.Range[xlsRow, iInTime].Text = dvBioDvAC[i]["InTimeShow"].ToString();
                                sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                if (Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString()).ToString("dd-MMM-yyyy") ==  DateTime.Now.ToString("dd-MMM-yyyy"))//DateTime.Now.ToString("dd-MMM-yyyy"))//DateTime.Now.ToString("dd-MMM-yyyy"))
                                {
                                    if (dvBioDvAC[i]["InTimeShow"].ToString() != "" && dvBioDvAC[i]["OutTimeShow"].ToString() == "")
                                    {
                                        if (Convert.ToDateTime(dvBioDvAC[i]["ShiftOutTime"].ToString()) < DateTime.Now)
                                        {
                                            Random rand = new Random((int)clsStaticInfo.dbl(dvBioDvAC[i]["EmployeePK"].ToString() + DateTime.Now.ToDbDate()));//Need Date wise seed  
                                            dvBioDvAC[i]["OutTimeShow"] = Convert.ToDateTime(dvBioDvAC[i]["ShiftOutTime"].ToString()).AddMinutes(rand.Next(0, 10)).ToString("HH:mm");
                                        }
                                    }
                                }
                                sheet1.Range[xlsRow, iOutTime].Text = dvBioDvAC[i]["OutTimeShow"].ToString();
                                sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }

                            if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "L")
                            {
                                sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Blue;
                                sheet1.Range[xlsRow, iDayStatus].Text = "P";
                            }
                            else
                            {
                                sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim().Replace("RST", "P");
                            }
                            sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                            sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            #region Comn

                            #endregion comn

                            if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "L")
                            {
                                #region Late by min

                                sInTime = "00:00:00";
                                if (dvBioDvAC[i]["InTime"].ToString().Trim() != "")
                                {
                                    sInTime = dvBioDvAC[i]["InTime"].ToString().Trim() + ":00";
                                }
                                else
                                {
                                    if (dvBioDvAC[i]["OutTime"].ToString().Trim() != "")
                                    {
                                        sInTime = dvBioDvAC[i]["OutTime"].ToString().Trim() + ":00";
                                    }
                                }
                                sOfficeInTime = "00:00:00";
                                strLateBy = "00:00";

                                if (dvBioDvAC[i]["ShiftInTime"].ToString().Trim() != "" && sInTime != "00:00:00")
                                {
                                    sOfficeInTime = dvBioDvAC[i]["ShiftInTime"].ToString().Trim() + ":00";
                                    strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                                }

                                #endregion Late by min
                            }
                            else
                            {
                                ///absent by how min

                                #region Absent by how much min

                                if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "A")
                                {
                                    sInTime = "00:00:00";
                                    if (dvBioDvAC[i]["InTime"].ToString().Trim() != "")
                                    {
                                        sInTime = dvBioDvAC[i]["InTime"].ToString().Trim() + ":00";
                                        sOfficeInTime = "00:00:00";
                                        strLateBy = "00:00";

                                    }
                                    else
                                    {

                                        strLateBy = "";
                                    }
                                }
                                else
                                {
                                    strLateBy = "";
                                }

                                #endregion Absent by how much min
                            }

                            //paid days

                            DateTime _ddd = Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString());

                            var _data = ListPayDays.Where(r => r.EmployeeCode == dvBioDvAC[i]["EmployeeCode"].ToString() && r.WorkDate == _ddd).FirstOrDefault();
                            if (_data != null)
                            {
                                sheet1.Range[xlsRow, iPayDays].Number = Convert.ToDouble(_data.DayValue);
                                sheet1.Range[xlsRow, iPayDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iPayDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }

                            string dti = dvBioDvAC[i]["dti"].ToString().Trim();
                            string dto = dvBioDvAC[i]["dto"].ToString().Trim();
                            string _InTimeShow = dvBioDvAC[i]["InTimeShow"].ToString().Trim();
                            string _OutTimeShow = dvBioDvAC[i]["OutTimeShow"].ToString().Trim();

                            #region Duration of working Hour


                            #endregion Duration of working Hour
                            if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "LV" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "H")
                            {
                                sheet1.Range[xlsRow, iLateBy].Text = "";
                                sheet1.Range[xlsRow, iLateBy].CellStyle.Font.Color = ExcelKnownColors.Blue;
                                sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }
                            else
                            {
                                sheet1.Range[xlsRow, iLateBy].Text = strLateBy;
                                sheet1.Range[xlsRow, iLateBy].CellStyle.Font.Color = ExcelKnownColors.Blue;
                                sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }


                            sheet1.Range[xlsRow, iLvShortName].Text = dvBioDvAC[i]["Code"].ToString();
                            sheet1.Range[xlsRow, iLvShortName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iLvShortName].VerticalAlignment = ExcelVAlign.VAlignCenter;


                            #endregion ----------------------Data-----------------------

                            #region Line Setup

                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

                            #endregion Line Setup


                            #region Line Setup
                            int _StartRow = xlsRow;
                            #endregion

                            #region UsedRange Alignment
                            sheet1.UsedRange.WrapText = true;
                            sheet1.UsedRange.CellStyle.Font.Size = 8;
                            sheet1.Range["A1"].CellStyle.Font.Size = 14;
                            sheet1.Range["A2"].CellStyle.Font.Size = 10;
                            sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                            #endregion UsedRange Alignment

                            #region ******************Report Header******************

                            //xlsRow += 1;
                            //sheet1.Range[xlsRow, xlsCol].Text = "Employee Job Card Information From : " + fromDate + " To : " + toDate;
                            //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                            //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                            //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                            //sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                            //sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            //sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                            #endregion ******************Report Header******************

                            reportUtility.CompanyPlantHeader(ref sheet1, endXlsCol, "Employee Job Card Information From : " + fromDate + " To : " + toDate, CompanyId, PlantName, null);

                            #region Freeze Panes
                            sheet1.IsDisplayZeros = false;
                            sheet1.UsedRange["A13"].FreezePanes();
                            sheet1.FirstVisibleColumn = 1;
                            sheet1.FirstVisibleRow = 12;
                            #endregion



                            sheet1.Range[11, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                            sheet1.Range[11, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[11, 4, xlsRow, 4].WrapText = true;
                            sheet1.UsedRange.CellStyle.Font.Size = 10;
                            sheet1.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(endXlsCol) + 5].Merge();
                            reportUtility.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);

                            sheet1.Name = sEmpCodeColl[Ec].ToString().Trim();
                        }
                    }


                }
            }
            else
            {
                throw new Exception("No Data Found");
            }
            return workbook;
        }


        public DataTable GetEmpJobCardInfoWithInDateTimeCompiled(string strEmpCode, string FromDate, string ToDate)
        {
            var strSql = @"SELECT A.EmployeePK, A.EmployeeCode
                            	,A.EmployeeName
                            	,A.DOJ
                            	,A.GivenDesignation
                                ,A.LegalDesignation
                            	,A.Unit
                            	,A.Division
                            	,A.Department
                            	,A.Section
                            	,A.SubSection
                            	,REPLACE(CONVERT(VARCHAR(11), A.PDate, 113), ' ', '-') PDate
                            	,A.DayStatus
                                ,A.ShiftCode 
                            	,A.aShiftName ShiftName
								,CONVERT(VARCHAR(5), A.aShiftInTime, 108) ShiftInTime
                            	,A.InTime
                                ,A.aShiftOutTime ShiftOutTime
                            	,A.OutTime
                            	
                            	,A.LvShortName
                            	,A.Code
                            	,A.LvDescrip
                            	,A.LeaveType
                                ,dti,dto
                                ,InTimeShow
                                ,OutTimeShow
                              
                              
                            FROM(
                                SELECT E.SystemId EmployeePK, E.EmployeeCode
                                    , E.EmployeeName
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
                                    , D.UserName GivenDesignation
                                    , U.UserName Unit
                                    , Dv.UserName Division
                                    , Dp.UserName Department
                                    , S.UserName Section
                                    , SB.UserName SubSection
                                    , APFD.WorkDate PDate
                                    , APFD.DayStatus
                                    , CONVERT(VARCHAR(5), APFD.InTime, 108) InTime
                                    , CONVERT(varchar(15), CAST(APFD.InTime AS TIME), 100) InTimeShow
                                    , CONVERT(VARCHAR(5), APFD.OutTime, 108) OutTime
                                    , CONVERT(varchar(15), CAST(APFD.OutTime AS TIME), 100) OutTimeShow  
                                    , HS.ShiftName aShiftName
									,Convert(Datetime,CONCAT(Format(APFD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), HS.InTime, 108))) aShiftInTime
						
									,Case When Convert(Datetime,CONCAT(Format(APFD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), HS.OutTime, 108))) < Convert(Datetime,CONCAT(Format(APFD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), HS.InTime, 108))) then DateAdd(day,1,Convert(Datetime,CONCAT(Format(APFD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), HS.OutTime, 108)))) Else Convert(Datetime,CONCAT(Format(APFD.WorkDate,'dd-MMM-yyyy'),' ', CONVERT(VARCHAR(5), HS.OutTime, 108))) END aShiftOutTime
								 
							
									, HS.Code ShiftCode  
                                    , LT.UserName LvShortName
                                    , LT.Description LvDescrip
                                    , LT.LeaveType
                                    , LT.Code
                                    , Isnull(LG.UserName, '') LegalDesignation
                                    , APFD.InTime dti, APFD.OutTime dto
                                    , CONVERT(VARCHAR(5), SFCG.InTime, 108) ShiftChangeInTime
                                    , SD.ShiftDefinationName ShiftName
                                    , CONVERT(VARCHAR(5), SD.OutTime, 108) ShiftOutTime
                                     FROM dbo.EmployeeInformation E

                                INNER JOIN AttdnProcessFinalData APFD ON E.SystemID = APFD.EmpSystemID
                                LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE  '" + FromDate + @"' BETWEEN FromDate AND ToDate) AS SFCG
                                ON APFD.ShiftID = SFCG.ShiftDefinationID

                                LEFT Join HKP.CompliedShift HS ON APFD.ShiftID = HS.Id

                                LEFT JOIN dbo.LeaveType LT ON APFD.EmpSystemID = LT.Id

                                LEFT JOIN ORG.Unit U ON E.UnitID = U.Id

                                LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id

                                LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id

                                LEFT JOIN ORG.Section S ON E.SectionID = S.Id

                                LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                                LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id

                                left join CompliedShiftAssignment es on es.EmpSystemID = E.SystemId
                                AND APFD.WorkDate = ES.WorkDate
                                left join( SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID  FROM[ShiftTimeChgMaster] m
                                left join[ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = es.CompliedShiftID and cs.ShiftDate = APFD.WorkDate

                                left join[ShiftDefination] sd on sd.SystemID = es.CompliedShiftID

                                LEFT JOIN HKP.Designation D ON E.GivenDesignationId = D.Id

                                WHERE E.SystemID IN(" + strEmpCode + @")
                                      AND APFD.WorkDate BETWEEN '" + FromDate + @"'
                                      AND '" + ToDate + @"' AND (EmployeeStatus = 'Active' OR DOS >= '" + FromDate + @"')
                            ) A
                            GROUP BY A.EmployeePK, A.EmployeeCode
                            	,A.EmployeeName
                            	,A.DOJ
                            	,A.GivenDesignation
                                ,A.LegalDesignation
                            	,A.Unit
                            	,A.Division
                            	,A.Department
                            	,A.Section
                            	,A.SubSection
                            	,PDate
                            	,A.DayStatus
                            	,A.InTime
								,A.aShiftName
								,A.ShiftCode
                                ,A.aShiftInTime
								,A.aShiftOutTime
                            	,A.OutTime
                            	
                            	,A.LvShortName
                            	,A.LvDescrip
                            	,A.LeaveType
                                ,A.Code
                                ,dti,dto
                                ,InTimeShow
                                ,OutTimeShow
                                ,ShiftChangeInTime
                                ,ShiftName
                               
                            ORDER BY A.EmployeeCode
                            	,A.PDate";

            if (_sqlRepository.GetDataTable(strSql).Rows.Count > 0)
            {
                return _sqlRepository.GetDataTable(strSql);

            }
            else
            {
                throw new Exception("No data found");
            }



        }//End Function

        public DataTable GetJobCardPayDays(string empCode, string WorkDate)
        {
            ReportUtility ob = new ReportUtility();
            var strSql = @"SELECT EmpSystemID, WorkDate ,EmployeeCode
           , DayValue = ISNULL(TotalPresent, 0) + ISNULL(TotalLate, 0) + ISNULL(TotalLv, 0) + ISNULL(TotalMLv, 0) + ISNULL(TotalWeekOff, 0)
           + ISNULL(TotalCompAssignLv, 0) + ISNULL(TotalHoliDay, 0) + ISNULL(TotalWeekOffHoliDay, 0)

                            FROM(SELECT EmpSystemID, WorkDate, EmployeeCode,

                                        " + ob.GetAttSum() + @"

                                        OTHr

                                  FROM dbo.AttdnProcessData a
                             left join  employeeInformation ei on  ei.SystemId =a.EmpSystemID
                                WHERE  ei.EmployeeCode in (" + empCode + @")
                                    AND MONTH(WorkDate) = MONTH('" + WorkDate + @"')
                                    AND YEAR(WorkDate) = YEAR('" + WorkDate + @"')) A
                                ";


            //if (_sqlRepository.GetDataTable(strSql).Rows.Count > 0)
            //{
            return _sqlRepository.GetDataTable(strSql);

            //}
            //else
            //{
            //    throw new Exception("No data found");
            //}
        }

    }
    public class SwapColumn
    {
        public string ValueMember { get; set; } = string.Empty;
        public string DisplayMember { get; set; } = string.Empty;
        public int ColIndex { get; set; } = 0;
    }
    public class ReturnResult
    {
        public bool Status { get; set; }
        public IWorkbook Workbook { get; set; }
        public string Message { get; set; }
    }
    public class ParaMontlyAttendance
    {
        public string UnitId { get; set; }
        public string DivisionId { get; set; }
        public string DepartmentId { get; set; }
        public string SectionId { get; set; }
        public string SubsectionId { get; set; }
        public string LineId { get; set; }
        public string EmpCat { get; set; }
        public string DesignationId { get; set; }
        public string DesignationGroupId { get; set; }
        public string EntityId { get; set; }
        //public string JoblocationId { get; set; }
        //public string ShiftId { get; set; }
        public string PlantId { get; set; }
        public string AMonth { get; set; }
        public string AYear { get; set; }

        public string FDate { get; set; }
        public string TDate { get; set; }


    }

    class PayDaysReport
    {
        public string EmployeeCode { get; set; }
        public DateTime WorkDate { get; set; }
        public decimal DayValue { get; set; }
    }
}
