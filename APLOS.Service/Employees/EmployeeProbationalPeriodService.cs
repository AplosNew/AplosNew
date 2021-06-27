#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Organizations;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.IO;

#endregion Using

namespace Library.Service.Employees
{
    public class EmployeeProbationalPeriodService : Service<EmployeeProbationalPeriod>, IEmployeeProbationalPeriodService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<EmployeeProbationalPeriod> _employeeProbationalPeriodRepository;
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IRepositoryAsync<Plant> _plantRepository;

        public EmployeeProbationalPeriodService(
            IRepositoryAsync<EmployeeProbationalPeriod> employeeProbationalPeriodRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , IEmployeeInformationService employeeInformationService
            , ISqlRepository sqlRepository
            , IRepositoryAsync<Plant> plantRepository
            ) : base(employeeProbationalPeriodRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _employeeProbationalPeriodRepository = employeeProbationalPeriodRepository;
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _employeeInformationService = employeeInformationService;
            _plantRepository = plantRepository;
        }

        #endregion Constructor

        public void UpdateStatus(string Id)
        {
            try
            {
                var dblist = _employeeInformationService.Find(Id);
          
                dblist.EmployeeStatus = "TBS";
                dblist.DateUpdated = DateTime.Now;
                _employeeInformationService.Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public void UpdateStatusActive(string Id)
        {
            try
            {
                var dblist = _employeeInformationService.Find(Id);

                dblist.EmployeeStatus = "Active";
                dblist.DateUpdated = DateTime.Now;
                _employeeInformationService.Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        private string GetPK()
        {
            return "P" + GetAutoNumber(nameof(EmployeeProbationalPeriod), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void OutMaster(EmployeeProbationalPeriod from_ui, out EmployeeProbationalPeriod from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = Find(from_ui.Id);
                if (from_db == null || from_db.Id == null || from_db.Id == "")
                {
                    from_db = new EmployeeProbationalPeriod
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(from_db);

                    #region Add

                    from_db.Id = GetPK();//set pk
                    from_db.ConfirmAfterDays = from_ui.ConfirmAfterDays;

                    #endregion Add
                }
                else
                {
                    #region Edit

                    from_db.ModelState = ModelState.Modified;
                    AuditService.Log(from_db);

                    from_db.ConfirmAfterDays = from_ui.ConfirmAfterDays;

                    #endregion Edit
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Save(EmployeeProbationalPeriod ui, out string masterid)
        {
            EmployeeProbationalPeriod localMaster = null;
            masterid = string.Empty;

            var flag = false;
            try
            {
                OutMaster(ui, out localMaster);
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

        public IEnumerable<EmployeeProbationalPeriod> GetMasterlist(string PKs)
        {
            try
            {
                var _sql = "SELECT * FROM trn.EmployeeProbationalPeriod WHERE Id IN (" + PKs + ")";
                return _employeeProbationalPeriodRepository.SqlQuery<EmployeeProbationalPeriod>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<EmployeeInformation> GetEmployeeInformationlist(string PKs)
        {
            try
            {
                string _sql = "SELECT * FROM EmployeeInformation WHERE SystemId IN (" + PKs + ")";
                return _employeeProbationalPeriodRepository.SqlQuery<EmployeeInformation>(_sql).AsEnumerable();
                // return _preRecruitmentEmployeeRepository.SqlQuery<PreRecruitmentEmployee>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> ProbationQueryByID(string empID)
        {
            try
            {
                var _sql = @"select * ,Replace(CONVERT(VARCHAR(11), DOJ1, 106), ' ', '-') DateOfConfirmation
	                                  ,Replace(CONVERT(VARCHAR(11), TetativeConfirmationDate1, 106), ' ', '-') TetativeConfirmationDate
	                                  ,Replace(CONVERT(VARCHAR(11), NextConfirmationDate1, 106), ' ', '-') NextConfirmationDate
                                 from (
		                                SELECT   emp.DOJ as DOJ1
                                                ,emp.EmployeeName
		                                        ,epp.ConfirmAfterDays
		                                        ,(emp.DOJ + epp.ConfirmAfterDays) as TetativeConfirmationDate1
		                                        ,epp.ExtendedDays
                                                ,(emp.DOJ + epp.ConfirmAfterDays + epp.ExtendedDays) as NextConfirmationDate1
		                                        ,epp.Remarks
                                                FROM dbo.EmployeeInformation AS emp
                                                Left outer join [TRN].[EmployeeProbationalPeriod] epp on emp.SystemId = epp.EmployeeId
                                                where epp.EmployeeId ='" + empID + "') x ";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private string GetEmpIds(IEnumerable<EmployeeProbationalPeriod> entities)
        {
            string empCodes = "''";
            try
            {
                foreach (var item in entities)
                {
                    if (empCodes == "''")
                    {
                        empCodes = "'" + item.EmployeeId + "'";
                    }
                    else
                    {
                        empCodes += ",'" + item.EmployeeId + "'";
                    }
                }
                return empCodes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ConfirmedEmployeeInfo(IEnumerable<EmployeeInformation> empInfoList)
        {
            string pks = string.Empty;
            try
            {
                foreach (var db in empInfoList)
                {
                    db.ModelState = ModelState.Modified;
                    //db.IsConfirmed = true;

                    _employeeInformationService.InsertOrUpdateGraph(db);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel EmployeeQuery(GridParameter parameters, bool isControlAdmin, bool isSysAdmin, string companyId, string employeeId, string plantId)
        {
            try
            {
                var str = "";
                str = !isControlAdmin && !isSysAdmin ? @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
                                    (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
                                            where ProbationRP='" + employeeId + "')))" : @" AND Emp.CompanyId='" + companyId + "'";

                parameters.CmdText = @"SELECT emp.SystemId as EmployeeId
			                                    , ''Id
                                                , emp.DOCDay
                                                , emp.EmployeeCode
                                                , emp.EmployeeName
                                                , EMP.EmailId Email
		                                        , Replace(CONVERT(VARCHAR(11), EMP.DOB, 106), ' ', '-') DOB
                                                , E.UserName as Entity
                                                , emp.PlantId
	                                            , D.UserName Designation
		                                        , DEG.UserName GivenDesignation
	                                            , REPLACE(CONVERT(VARCHAR(11), emp.DOJ, 106), ' ', '-') DOJ
                                                , REPLACE(CONVERT(VARCHAR(11), (emp.DOJ + ISNULL(emp.DOCDay,0)), 106), ' ', '-') DOC
                                                , (emp.DOJ + ISNULL(emp.DOCDay,0)) DOCSort
                                                , REPLACE(CONVERT(VARCHAR(11),GETDATE(), 106), ' ', '-') Today
	                                            , c.UserName as EmployeeCategory
                                                , co.UserName as company
                                                FROM dbo.EmployeeInformation AS EMP
                                                --LEFT OUTER JOIN (SELECT TOP(1) ExtendedDays,Id,EmployeeId FROM [TRN].EmployeeProbationalPeriod
				                                --        WHERE EmployeeId IN(select systemid from EmployeeInformation e where e.BudgetCode in
                                                --            (select Id from mst.ManpowerBudget where EntityId in
                                                --                (select entityid from [HKP].[ApprovalConfiguration]
                                                --                    where ProbationRP =
                                                --                    (select EmployeeId from [SEC].[User] where UserId='identity.Name')
                                                --                )
                                                --            )
                                                --        )  ORDER BY CAST(AddedDate AS DATETIME) DESC) AS p ON EMP.SystemId = p.EmployeeId
                                                LEFT OUTER JOIN [MST].[ManpowerBudget] PMB ON EMP.BudgetCode=PMB.Id
												LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                                                LEFT OUTER JOIN [ORG].[Entity] E ON PMB.EntityId=E.Id
												LEFT OUTER JOIN [HKP].[Designation] DEG ON EMP.GivenDesignationId=DEG.Id
		                                        LEFT OUTER JOIN [HKP].[Designation] D ON PR.DesignationId=D.Id
                                                LEFT OUTER JOIN [HKP].[EmployeeCategory] c ON c.id=EMP.EmployeeCategorySystemID
                                                LEFT OUTER JOIN [ORG].[Company] co ON Emp.CompanyId = co.Id
                                                where EMP.IsConfirmed = 0 AND EMP.EmployeeStatus='Active' AND EMP.PlantId='" + plantId + @"'" + str;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel EmployeeColorQueryByDate(GridParameter parameters, bool isControlAdmin, bool isSysAdmin, string companyId, string employeeId, bool a, bool t, bool f, string plantId)
        {
            try
            {
                var str = "";
                str = !isControlAdmin && !isSysAdmin ? @" AND emp.BudgetCode IN (SELECT Id from mst.ManpowerBudget WHERE EntityId IN (SELECT entityid FROM [HKP].[ApprovalConfiguration] WHERE ProbationRP='" + employeeId + "'))" : @" AND Emp.CompanyId='" + companyId + "'";

                var wc = string.Empty;
                wc = a ? " Where( [Status]='Due'" : " Where( [Status]=''";
                if (t)
                {
                    wc += " or [Status]='Present'";
                }
                else
                {
                    wc += " or [Status]=''";
                }
                if (f)
                {
                    wc += " or [Status]='Future'";
                }
                else
                {
                    wc += " or [Status]=''";
                }
                if (!string.IsNullOrEmpty(wc))
                {
                    wc = wc + ")";
                }
                parameters.CmdText = @"Select  * from ( SELECT emp.SystemId as EmployeeId
			                                    ,''Id
                                                ,emp.DOCDay
                                                ,emp.DOCMonth     
                                                ,emp.EmployeeCode
                                                ,emp.EmployeeName
                                                ,EMP.EmailId Email
		                                        ,Replace(CONVERT(VARCHAR(11), EMP.DOB, 106), ' ', '-') DOB
                                                ,E.UserName as Entity
                                                ,emp.PlantId
	                                            ,D.UserName Designation
		                                        ,DEG.UserName GivenDesignation
	                                            ,REPLACE(CONVERT(VARCHAR(11), emp.DOJ, 106), ' ', '-') DOJ
                                                ,REPLACE(CONVERT(VARCHAR(11), (emp.DOJ +
												(case when ISNULL(emp.DOCIsDay,0) = 1 then emp.DOCDay
													  else ISNULL(emp.DOCMonth,0)*30 end)), 106), ' ', '-') DOC
                                                --,(emp.DOJ + ISNULL(emp.DOCDay,0)) DOCSort
                                                ,(emp.DOJ +
												(case when ISNULL(emp.DOCIsDay,0) = 1 then emp.DOCDay
													  else ISNULL(emp.DOCMonth,0)*30 end)) DOCSort
                                                ,REPLACE(CONVERT(VARCHAR(11),GETDATE(), 106), ' ', '-') Today
	                                            ,c.UserName as EmployeeCategory
                                                ,co.UserName as company
												--Present

												,case when (emp.DOJ + (case when ISNULL(emp.DOCIsDay, 0) = 1 then emp.DOCDay
										            else ISNULL(emp.DOCMonth, 0) * 30 end)) < CONVERT(DATE, GETDATE()) then 'Due'
										                when (emp.DOJ + (case when ISNULL(emp.DOCIsDay, 0) = 1 then emp.DOCDay
										            else ISNULL(emp.DOCMonth, 0) * 30 end)) = CONVERT(DATE, GETDATE()) then 'Present'
										            --else 'Future'
                                                WHEN (emp.DOJ + (case when ISNULL(emp.DOCIsDay, 0) = 1 then emp.DOCDay
										            else
													ISNULL(emp.DOCMonth, 0) * 30 end)) < CONVERT(DATE, GETDATE()+hr.ProbationPeriodAlertBeforeDays)
													 then 'Future'
										            end AS [Status]
                                                ,hr.IsPastDOCAllowed
                                                ,hr.pastDOCdaysAllowed
                                                FROM dbo.EmployeeInformation AS EMP
                                                LEFT OUTER JOIN PlantWiseHRMSSetting hr on hr.PlantID = emp.PlantId
                                                LEFT OUTER JOIN [MST].[ManpowerBudget] PMB ON EMP.BudgetCode=PMB.Id
												LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                                                LEFT OUTER JOIN [ORG].[Entity] E ON PMB.EntityId=E.Id
												LEFT OUTER JOIN [HKP].[Designation] DEG ON EMP.GivenDesignationId=DEG.Id
		                                        LEFT OUTER JOIN [HKP].[Designation] D ON PR.DesignationId=D.Id
                                                LEFT OUTER JOIN [HKP].[EmployeeCategory] c ON c.id=EMP.EmployeeCategorySystemID
                                                LEFT OUTER JOIN [ORG].[Company] co ON Emp.CompanyId = co.Id
                                                where emp.IsConfirmed = 0 AND EMP.EmployeeStatus = 'Active' AND EMP.PlantId='" + plantId + @"'
												" + str + @"
												)X " + wc + "";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetCbo(string plantId)
        {
            var sql = @"select Id,FormatName from SCS.RptConfigTemplate where Type='Confirmation Letter' AND PlantId='" + plantId + "' ORDER BY FormatName";
            return _sqlRepository.GetCombo(sql, "Id", "FormatName");
        }



        public GridModel GetConfirmedEmployeeData(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT EMP.*, E.UserName as Entity, D.UserName Designation
		                                        , DEG.UserName GivenDesignation , c.UserName as EmployeeCategory, co.UserName as company
                                                FROM EmployeeInformation AS EMP
                                                LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID = emp.PlantId
                                                LEFT JOIN [MST].[ManpowerBudget] PMB ON EMP.BudgetCode=PMB.Id
												LEFT JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                                                LEFT JOIN [ORG].[Entity] E ON PMB.EntityId=E.Id
												LEFT JOIN [HKP].[Designation] DEG ON EMP.GivenDesignationId=DEG.Id
		                                        LEFT JOIN [HKP].[Designation] D ON PR.DesignationId=D.Id
                                                LEFT JOIN [HKP].[EmployeeCategory] c ON c.id=EMP.EmployeeCategorySystemID
                                                LEFT JOIN [ORG].[Company] co ON Emp.CompanyId = co.Id
                                                WHERE EMP.EmployeeStatus = 'Active' AND EMP.IsConfirmed=1 AND EMP.PlantId='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetInActivemployeeData(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT EMP.*, E.UserName as Entity, D.UserName Designation
		                                        , DEG.UserName GivenDesignation , c.UserName as EmployeeCategory, co.UserName as company
                                                FROM EmployeeInformation AS EMP
                                                LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID = emp.PlantId
                                                LEFT JOIN [MST].[ManpowerBudget] PMB ON EMP.BudgetCode=PMB.Id
												LEFT JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                                                LEFT JOIN [ORG].[Entity] E ON PMB.EntityId=E.Id
												LEFT JOIN [HKP].[Designation] DEG ON EMP.GivenDesignationId=DEG.Id
		                                        LEFT JOIN [HKP].[Designation] D ON PR.DesignationId=D.Id
                                                LEFT JOIN [HKP].[EmployeeCategory] c ON c.id=EMP.EmployeeCategorySystemID
                                                LEFT JOIN [ORG].[Company] co ON Emp.CompanyId = co.Id
                                                WHERE EMP.EmployeeStatus = 'TBS'  AND EMP.PlantId='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataSet PlantWiseDOC(string plantId)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = @"select  IsPastDOCAllowed from dbo.PlantWiseHRMSSetting where PlantId='" + plantId + @"'  ";

                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InitEmployeeInfo(IEnumerable<EmployeeProbationalPeriod> entities, IEnumerable<EmployeeInformation> empInfoList)
        {
            var pks = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _plantId = string.Empty;
                foreach (var db in empInfoList)
                {
                    db.ModelState = ModelState.Modified;
                    var empid = db.SystemId;
                    var ui = entities.FirstOrDefault(a => a.EmployeeId == empid);
                    var ep = empInfoList.FirstOrDefault(e => e.EmployeeId == empid);

                    var PrevDay = 0;
                    if (db.DOCIsDay)
                    {
                        PrevDay = db.DOCDay;
                    }
                    if (db.DOCIsMonth)
                    {
                        PrevDay = db.DOCMonth * 30;
                    }

                    // var emp = PlantWiseDOC(db.PlantID);
                    // var targetDate = Convert.ToDateTime(db.DOJ).AddDays(PrevDay + ui.ConfirmAfterDays);
                    //var plantName = _plantRepository.Query(t => t.Id == db.PlantID).Select(t => t.UserName).FirstOrDefault();
                    if (ui.ApprovalStatus == "Confirm")
                    {
                        if (ui.ConfirmAfterDays == 0)
                        {
                            db.DOC = ui.NewDOC;
                            db.DOCDay = (int)Convert.ToDateTime(ui.NewDOC).Subtract(Convert.ToDateTime(db.DOJ)).TotalDays;
                            db.DOCBy = identity.UserId;
                            db.ProbationConfirmEntryDate = DateTime.Now;
                            db.IsConfirmed = true;
                        }
                    }

                    db.DOCDay = PrevDay + ui.ConfirmAfterDays;
                    db.DOCMonth = 0;
                    db.DOCIsDay = true;
                    db.DOCIsMonth = false;
                    db.UpdatedBy = identity.Name;
                    db.DateUpdated = DateTime.Now;

                    _employeeInformationService.InsertOrUpdateGraph(db);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void ProbationalUpdate(IEnumerable<EmployeeProbationalPeriod> entities)
        {
            var pks = string.Empty;
            var flag = false;
            try
            {
                const string _pks = "''";
                var from_dblist = GetMasterlist(_pks);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _plantId = string.Empty;
                var _emp_pks = GetEmpIds(entities);
                var empInfoList = GetEmployeeInformationlist(_emp_pks);

                var _Id = GetPK();
                var _count = 0;
                foreach (var item in entities)
                {
                    var db = from_dblist.FirstOrDefault(a => a.Id == item.Id);
                    if (db == null || db.Id == null)
                    {
                        var _emp = empInfoList.FirstOrDefault(a => a.SystemId == item.EmployeeId);
                        if (_emp != null)
                        {
                            var PrevDay = 0;

                            if (_emp.DOCIsDay)
                            {
                                PrevDay = _emp.DOCDay;
                            }
                            if (_emp.DOCIsMonth)
                            {
                                PrevDay = _emp.DOCMonth * 30;
                            }

                            _count++;
                            db = new EmployeeProbationalPeriod
                            {
                                Id = _Id + "-" + _count,
                                ModelState = ModelState.Added
                            };
                            AuditService.AddedLog(db);
                            db.ConfirmAfterDays = PrevDay;
                            db.ExtendedDays = item.ConfirmAfterDays;
                            db.Remarks = item.Remarks;

                            db.EmployeeId = item.EmployeeId;
                            db.CompanyGroupId = identity.CompanyGroupId;
                            db.CompanyId = identity.CompanyId;
                            db.PlantId = item.PlantId;
                            InsertOrUpdateGraph(db);
                        }
                        else
                        {
                            throw new Exception("employee [" + item.EmployeeId + "] not found");
                        }
                    }
                }
                InitEmployeeInfo(entities, empInfoList);
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        
        public IWorkbook EmployeeConfirmation(string companyGroupId, string companyId, string plantId, string empId, string empType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];
                //CreateSheetMainLocal(ref sheet1, report, "Appointment Letter", "Appointment Letter", companyId, plantId, empId, empType, tempId); //, templatePathHindi, templatePathEnglish, templatePathBangla);
                workbook = CreateSheetConfirmaion(ref sheet1, report, "Confirmation Letter", "Confirmation Letter", companyGroupId,companyId, plantId, empId, empType, tempId); //, templatePathHindi, templatePathEnglish, templatePathBangla);
                                                                                                                                                                 // workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
               Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
               ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        private IWorkbook CreateSheetConfirmaion(ref IWorksheet sheet1, ReportUtility report, string sheetHeader, string sheetName, string companyGroupId, string companyId, string plantId, string empId, string empType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook1 = null;
                string File = "";
                string strPath = "";
                var fileName = "";
                var langID = "";
                var lang = GetLanguage(plantId, tempId);
                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                }
                else
                {
                    langID = tempId;
                }

                var dtEmp = GetEmployeeById(empId, plantId, empType, langID, tempId);
                var grossAmount = GetGrossAmount(empId);// GetGrossAmount(empId);

                var Templatefile = GetFilePath(plantId, tempId);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {

                    workbook1 = excelEngine.Excel.Workbooks.Open(strPath);
                    workbook1.Worksheets[0].Replace("{Date}", DateTime.Now.ToString("dd-MM-yyyy"));
                    workbook1.Worksheets[0].Replace("{Website}", dtEmp.Rows[0]["Website"].ToString());
                    workbook1.Worksheets[0].Replace("{Email}", dtEmp.Rows[0]["Email"].ToString());
                    workbook1.Worksheets[0].Replace("{PhoneNumber}", dtEmp.Rows[0]["Phone"].ToString());
                        workbook1.Worksheets[0].Replace("{CompanyName}", dtEmp.Rows[0]["CompanyName"].ToString());
                        workbook1.Worksheets[0].Replace("{Address}", dtEmp.Rows[0]["CompanyAddress"].ToString());
                        workbook1.Worksheets[0].Replace("{EmployeeName}", dtEmp.Rows[0]["EmployeeName"].ToString());

                    string address = "";
                    if (dtEmp.Rows[0]["PresentCity"].ToString() != "")
                    {
                        address = dtEmp.Rows[0]["PresentCity"].ToString() + @", " + dtEmp.Rows[0]["PresentDistrict"].ToString() + @", " + dtEmp.Rows[0]["PresentState"].ToString() + @", " + dtEmp.Rows[0]["LPresentCountry"].ToString();
                    }
                    else
                    {
                        address = dtEmp.Rows[0]["ParmanentAddress1"].ToString();
                    }

                    workbook1.Worksheets[0].Replace("{EmployeeAddress}", address);
                    workbook1.Worksheets[0].Replace("{GivenDesignation}", dtEmp.Rows[0]["DesignationName"].ToString());
                    workbook1.Worksheets[0].Replace("{DOJ}", dtEmp.Rows[0]["DOJ"].ToString());
                    workbook1.Worksheets[0].Replace("{DOC}", dtEmp.Rows[0]["DOC"].ToString());
                    workbook1.Worksheets[0].Replace("{Section}", dtEmp.Rows[0]["DOC"].ToString());
                    workbook1.Worksheets[0].Replace("{Gross}", grossAmount.Rows[0]["EntryAmount"].ToString());



                }
                else
                {
                    File = "Con" + plantId + "English.xls";
                    var Cgdata = GetDefaultCompanyGroupLanguage(companyGroupId);
                    var pdata = GetDefaultPlantLanguage(plantId);

                    if (Cgdata["UserName"].ToString() == tempId || pdata["UserName"].ToString() == tempId)
                    {
                        strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);

                        File = "Con" + dtEmp.Rows[0]["PlantId"].ToString() + "English.xls";
                        strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                        workbook1 = excelEngine.Excel.Workbooks.Open(strPath);
                        workbook1.Worksheets[0].Replace("{CompanyName}", dtEmp.Rows[0]["CompanyName"].ToString());
                        workbook1.Worksheets[0].Replace("{Address}", dtEmp.Rows[0]["CompanyAddress"].ToString());
                        workbook1.Worksheets[0].Replace("{EmployeeName}", dtEmp.Rows[0]["EmployeeName"].ToString());
                        workbook1.Worksheets[0].Replace("{Website}", dtEmp.Rows[0]["Website"].ToString());
                        workbook1.Worksheets[0].Replace("{Email}", dtEmp.Rows[0]["Email"].ToString());
                        workbook1.Worksheets[0].Replace("{Phone}", dtEmp.Rows[0]["Phone"].ToString());
                        string address = "";
                        if (dtEmp.Rows[0]["PresentCity"].ToString() != "")
                        {
                            address = dtEmp.Rows[0]["PresentCity"].ToString() + @", " + dtEmp.Rows[0]["PresentDistrict"].ToString() + @", " + dtEmp.Rows[0]["PresentState"].ToString() + @", " + dtEmp.Rows[0]["LPresentCountry"].ToString();
                        }
                        else
                        {
                            address = dtEmp.Rows[0]["PresentAddress1"].ToString();
                        }

                        workbook1.Worksheets[0].Replace("{EmployeeAddress}", address);
                        workbook1.Worksheets[0].Replace("{GivenDesignation}", dtEmp.Rows[0]["DesignationName"].ToString());
                        workbook1.Worksheets[0].Replace("{DOJ}", dtEmp.Rows[0]["DOJ"].ToString());
                        workbook1.Worksheets[0].Replace("{DOC}", dtEmp.Rows[0]["DOC"].ToString());
                        workbook1.Worksheets[0].Replace("{Section}", dtEmp.Rows[0]["DOC"].ToString());
                        workbook1.Worksheets[0].Replace("{Gross}", grossAmount.Rows[0]["EntryAmount"].ToString());

                        workbook1.Worksheets[0].Replace("{Date}", DateTime.Now.ToString("dd-MM-yyyy"));

                        workbook1.Version = ExcelVersion.Excel97to2003;
                    }

                    else
                    {

                        File = "Con" + plantId + "English.xls";
                        strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                        workbook1 = excelEngine.Excel.Workbooks.Open(strPath);
                        workbook1.Worksheets[0].Replace("{CompanyName}", dtEmp.Rows[0]["CompanyName"].ToString());
                        workbook1.Worksheets[0].Replace("{Address}", dtEmp.Rows[0]["CompanyAddress"].ToString());
                        workbook1.Worksheets[0].Replace("{EmployeeName}", dtEmp.Rows[0]["EmployeeName"].ToString());
                        workbook1.Worksheets[0].Replace("{Website}", dtEmp.Rows[0]["Website"].ToString());
                        workbook1.Worksheets[0].Replace("{Email}", dtEmp.Rows[0]["Email"].ToString());
                        workbook1.Worksheets[0].Replace("{Phone}", dtEmp.Rows[0]["Phone"].ToString());
                        string address = "";
                        if (dtEmp.Rows[0]["PresentCity"].ToString() != "")
                        {
                            address = dtEmp.Rows[0]["PresentCity"].ToString() + @", " + dtEmp.Rows[0]["PresentDistrict"].ToString() + @", " + dtEmp.Rows[0]["PresentState"].ToString() + @", " + dtEmp.Rows[0]["LPresentCountry"].ToString();
                        }
                        else
                        {
                            address = dtEmp.Rows[0]["PresentAddress1"].ToString();
                        }

                        workbook1.Worksheets[0].Replace("{EmployeeAddress}", address);
                        workbook1.Worksheets[0].Replace("{GivenDesignation}", dtEmp.Rows[0]["DesignationName"].ToString());
                        workbook1.Worksheets[0].Replace("{DOJ}", dtEmp.Rows[0]["DOJ"].ToString());
                        workbook1.Worksheets[0].Replace("{DOC}", dtEmp.Rows[0]["DOC"].ToString());
                        workbook1.Worksheets[0].Replace("{Section}", dtEmp.Rows[0]["DOC"].ToString());
                        workbook1.Worksheets[0].Replace("{Gross}", grossAmount.Rows[0]["EntryAmount"].ToString());

                        workbook1.Worksheets[0].Replace("{Date}", DateTime.Now.ToString("dd-MM-yyyy"));
                    }
                        workbook1.Version = ExcelVersion.Excel97to2003;
                    }
                    return workbook1;
                }
            
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Dictionary<string, object> GetFilePath(string plantId, string pkId)
        {
            var sql = @"SELECT Id,TemplateFileName FROM SCS.RptConfigTemplate WHERE  Id='" + pkId + "'  AND PlantId='" + plantId + "'";
            return _sqlRepository.GetData(sql);
        }

        public Dictionary<string, object> GetLanguage(string plantId, string pkId)
        {
            var sql = @"SELECT Id,Language FROM SCS.RptConfigTemplate WHERE  Id='" + pkId + "'  AND PlantId='" + plantId + "'";
            return _sqlRepository.GetData(sql);
        }
        private DataTable getLanguageId(string username)
        {
            try
            {
                var sql = @"Select Id from SCS.Language where UserName ='" + username + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Dictionary<string, object> GetDefaultCompanyGroupLanguage(string companyGrupId)
        {
            var sql = @"SELECT CG.LanguageId Id,L.UserName FROM ORG.CompanyGroup CG
                        LEFT JOIN SCS.[Language] L ON L.Id=CG.LanguageId
                        WHERE CG.Id='" + companyGrupId + @"'
                        ORDER BY UserName";
            return _sqlRepository.GetData(sql, null);
        }
        public Dictionary<string, object> GetDefaultPlantLanguage(string plantId)
        {
            var sql = @"
                        SELECT P.LanguageId Id,PL.UserName  FROM ORG.Plant P
                        LEFT JOIN SCS.[Language] PL ON PL.Id=P.LanguageId
                        WHERE P.Id='" + plantId + @"'
                        ORDER BY UserName";
            return _sqlRepository.GetData(sql, null);
        }
        private DataTable GetEmpInfo(string empSystemID)
        {
            try
            {
                var sql = @"Select E.*,D.UserName GivenDesignation From dbo.EmployeeInformation E
                            LEFT JOIN HKP.Designation D ON E.GivenDesignationId=D.Id Where E.SystemId= '" + empSystemID + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }//End Function

        public DataTable GetEmployeeById(string employeeId, string plantId, string employeementType, string languageId, string tempId)
        {
            try
            {

                string sql = @"SELECT 
            ISNULL(FatherNameLocal,FatherName) FatherName,
            ISNULL(MotherNameLocal,MotherName) MotherName,
            ISNULL(EmployeeNameLocal,EmployeeName) EmployeeName,
            ISNULL(LocalCompanyName,CompanyName) CompanyName,
            ISNULL(CompanyAddress,CompanyAddress) CompanyAddress,
            ISNULL(UtilityName,UtilityName) UtilityName,
            ISNULL(ParmanentAddress1,ParmanentAddress1) ParmanentAddress1,
            ISNULL(PresentAddress1,PresentAddress1) PresentAddress1,
            ISNULL(PresentCity,PresentCity) PresentCity,
            ISNULL(PresentDistrict,PresentDistrict) PresentDistrict,
            ISNULL(PresentState,PresentState) PresentState,
            ISNULL(LPresentCountry,LPermanentCountry) LPresentCountry,
            ISNULL(FirstName,FirstName) FirstName,
            ISNULL(LocalDesignationName,DesignationName) DesignationName,
            ISNULL(DOJ,DOJ) DOJ,
            ISNULL(confirm,confirm) confirm,
            ISNULL(MobileNo,MobileNo) MobileNo,
            ISNULL(DOC,DOC) DOC,
            ISNULL(Website,Website) Website,
            ISNULL(Email,Email) Email,
            ISNULL(Phone,Phone) Phone,
                     RPTM.TemplateFileName FROM(SELECT TAB2.*, AM.Phone, AM.Email, AM.Website, AM.Address1 FROM (SELECT TAB1.*, LAN.StandardName FROM (SELECT CM.Image CompanyLogo,
                    CM.UserName CompanyName,AM.Address1 CompanyAddress,E.EmployeeName,
                    E.FatherName,E.MotherName,e.FatherNameLocal,e.MotherNameLocal,E.EmpPicPath EmployeePic,E.EmployeeCode, Convert(varchar, E.DOJ, 105) DOJ,BG.UserName BloodGroup
                                              , E.NationalID,E.EmploymentType,D.UserName DesignationName, dm.EmployeeCategoryId,ec.UserName EmployeeCategory,L.UserName Line,
							  E.EmpSignature CardHolderSignature,P.AuthorizedSignature
                              ,E.CellPhnNo MobileNo,E.ParmanentAddress1,DP.UserName Department,A.[Name] LocalCompanyName, B.[Name] LocalDesignationName,C.[Name] LocalDepartmentName,
							  N.Name NameLabel
                              ,DN.Name DesignationLabel,DPN.Name DepartmentLabel,LN.Name LineLabel,LET.Name EmploymentTypeLabel, ID.Name IDNoLabel,  PT.Name EmploymentTypeName,
							   DJ.Name DOJLabel, ET.Name EmergencyTellNoLabel, BGP.Name BloodGroupLabel
                              ,E.EmployeeNameLocal,LL.UtilityName,NID.Name NIDLabel, E.ParmanentAddress1Local, (PML.Name+' '+LA.Name) ParmanentAddress,LMB.Name MobileNoLabel,
							  LD.Name LegalDesignationLocal
                              ,Convert(varchar, DATEADD(year, 5, E.DOJ),105) AS Validity,LNN.Name LineLocal, Convert(varchar, E.DOC, 105) DOC
                              ,PCN.Name LPermanentCountry,PRCN.Name LPresentCountry,E.PresentAddress1
							  ,PD.Name PermanentDistrict,PRD.Name PresentDistrict,PST.Name PermanentState, PRST.Name PresentState,PCT.Name PermanentCity, PRCT.Name PresentCity
                              ,CASE WHEN DOCDay=0 THEN DOCMonth ELSE DOCDay/30 END AS confirm, PL.LanguageId, PL.Id as 'PlantId', CM.AddressMasterId,E.FirstName FROM EmployeeInformation E
                              LEFT JOIN ORG.Company CM ON CM.Id = E.CompanyId
                              LEFT JOIN MST.AddressMaster AM ON AM.Id = CM.AddressMasterId
                              LEFT JOIN HKP.BloodGroup BG ON BG.Id = E.BloodGroupID
                              LEFT JOIN HKP.Designation D ON D.Id = E.GivenDesignationId
                              LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = e.GivenDesignationId
                              LEFT JOIN HKP.EmployeeCategory EC ON EC.Id = DM.EmployeeCategoryId
                              LEFT JOIN ORG.Line L ON L.Id=E.LineId
							  LEFT JOIN [SCS].[PlantSetting] P ON P.PlantId=E.PlantId
                              LEFT JOIN ORG.Department DP ON DP.Id=E.DepartmentId
							  LEFT JOIN ORG.Plant PL ON PL.Id=E.PlantId
							  LEFT JOIN HKP.LocalLanguage A ON A.CompanyId=E.CompanyId AND A.LanguageId='" + languageId + @"'
                              LEFT JOIN HKP.LocalLanguage LL ON LL.CompanyId=E.CompanyId AND LL.LanguageId='" + languageId + @"'
							  LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=E.GivenDesignationId AND PL.LanguageId='" + languageId + @"'
							  LEFT JOIN HKP.LocalLanguage C ON C.DepartmentId =E.DepartmentId AND PL.LanguageId='" + languageId + @"'
                              LEFT JOIN HKP.LocalLanguage LD ON LD.LegalDesignationId=E.LegalDesignationId AND PL.LanguageId='" + languageId + @"'
                              LEFT JOIN HKP.LocalLanguage LNN ON LNN.LineId=E.LineId AND PL.LanguageId='" + languageId + @"'
                              LEFT JOIN HKP.LocalLanguage PCN ON PCN.CountryId=E.ParmCountryID AND PL.LanguageId='" + languageId + @"'
							  LEFT JOIN HKP.LocalLanguage PRCN ON PRCN.CountryId=E.ParmCountryID AND PL.LanguageId='" + languageId + @"'
							  LEFT JOIN HKP.LocalLanguage PD ON PD.DistrictId=E.ParmDistrictID AND PL.LanguageId='" + languageId + @"'
							  LEFT JOIN HKP.LocalLanguage PRD ON PRD.DistrictId=E.PresDistrictID AND PL.LanguageId='" + languageId + @"'
							  LEFT JOIN HKP.LocalLanguage PST ON PST.StateId=E.ParmStateId AND PL.LanguageId='" + languageId + @"'
							  LEFT JOIN HKP.LocalLanguage PRST ON PRST.StateId=E.PresStateId AND PL.LanguageId='" + languageId + @"'
							  LEFT JOIN HKP.LocalLanguage PCT ON PCT.CityId=E.ParmCityID AND PL.LanguageId='" + languageId + @"'
							  LEFT JOIN HKP.LocalLanguage PRCT ON PRCT.CityId=E.PresCityID AND PL.LanguageId='" + languageId + @"'

                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Name' and LanguageId='" + languageId + @"' ) N ON N.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Designation'and LanguageId='" + languageId + @"' ) DN ON DN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Department'and LanguageId='" + languageId + @"' ) DPN ON DPN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Line'and LanguageId='" + languageId + @"' ) LN ON LN.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmploymentType'and LanguageId='" + languageId + @"' ) LET ON LET.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='IDNo'and LanguageId='" + languageId + @"' ) ID ON ID.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='" + employeementType + @"'and LanguageId='" + languageId + @"' ) PT ON PT.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='DOJ'and LanguageId='" + languageId + @"' ) DJ ON DJ.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmergencyTelNo'and LanguageId='" + languageId + @"' ) ET ON ET.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='BloodGroup'and LanguageId='" + languageId + @"' ) BGP ON BGP.LanguageId=PL.LanguageId
					          LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='NIDNo'and LanguageId='" + languageId + @"' ) NID ON BGP.LanguageId=PL.LanguageId
	                          LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Permanent'and LanguageId='" + languageId + @"' ) PML ON PML.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Address'and LanguageId='" + languageId + @"' ) LA ON LA.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='MobileNo'and LanguageId='" + languageId + @"' ) LMB ON LMB.LanguageId=PL.LanguageId
                                WHERE E.SystemID ='" + employeeId + "') TAB1 " +
                                "LEFT JOIN SCS.Language AS LAN ON LAN.Id=TAB1.LanguageId) TAB2 LEFT JOIN MST.AddressMaster AS AM ON AM.Id=TAB2.AddressMasterId) TAB3 " +
                                "LEFT JOIN  (SELECT * FROM SCS.RptConfigTemplate WHERE Language='" + tempId + "'  and PlantId='" + plantId + @"') AS RPTM ON TAB3.PlantId=RPTM.PlantId";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetConfirmationLetterData(string plantId)
        {
            try
            {
                var sql = @"SELECT * FROM [SCS].[PlantWiseLetterTemplate] WHERE LetterType='" + LetterType.ConfirmationLetter + @"' AND PlantId='" + plantId + @"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetGrossAmount(string empId)
        {
            try
            {
                var sql = @"SELECT  convert(numeric(10,2), SD.EntryAmount) EntryAmount FROM SalaryInfoDefineMaster SM
                            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
                            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
                            WHERE SM.EmpInfoSystemID='" + empId + @"' AND SH.HeadCategory='GROSS'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}