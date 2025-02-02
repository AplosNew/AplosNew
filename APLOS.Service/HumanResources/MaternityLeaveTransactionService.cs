using ConnectionManager;
using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Biometrics;
using Library.Service.Biometrics;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.Organizations;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Library.Service.HumanResources
{
    public class MaternityLeaveTransactionService : Service<LeaveTransaction>, IMaternityLeaveTransactionService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<LeaveTransaction> _maternityLeaveTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISignatureService _signatrueService;
        private readonly ILeaveTransactionDetailsService _leaveTransactionDetailsService;

        public MaternityLeaveTransactionService(
            IRepositoryAsync<LeaveTransaction> maternityLeaveTransactionRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , ISignatureService signatrueService
             , ILeaveTransactionDetailsService leaveTransactionDetailsService
            , IUnitOfWork unitOfWork) : base(maternityLeaveTransactionRepository, unitOfWork, pkGeneratorService)
        {
            _maternityLeaveTransactionRepository = maternityLeaveTransactionRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _signatrueService = signatrueService;
            _leaveTransactionDetailsService = leaveTransactionDetailsService;
        }

        #endregion Constructor

        public void Save(LeaveTransaction entity)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;


                var CheckLeave = GetLeaveCheck(entity.SystemID, entity.FromDate, entity.ToDate, entity.EmpSystemID);
                if (CheckLeave.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("Another leave already taken:" + Convert.ToDateTime(entity.FromDate).ToString("dd-MMM-yyyy") + " This Range");
                }


                if (string.IsNullOrEmpty(entity.SystemID))
                {
                    var CheckUnApproveProfile = GetUnApproveLeave(entity.EmpSystemID, entity.SystemID);
                    if (CheckUnApproveProfile.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Previous Leave Not Approved.");
                    }
                }

                var consequtiveLeave = GetCosequtiveLeave(entity.SystemID, entity.EmpSystemID);
                if (consequtiveLeave.Tables[0].Rows.Count > 0)
                {
                    if (entity.ExpectedDelivaryDate < Convert.ToDateTime(consequtiveLeave.Tables[0].Rows[0]["ToDate"]))
                        throw new CustomException("Can apply after '" + consequtiveLeave.Tables[0].Rows[0]["GapeBetweenConsecutiveIssue"] + "' days (" + consequtiveLeave.Tables[0].Rows[0]["ToDate"] + ")");
                }

                if (!string.IsNullOrEmpty(entity.SystemID))
                {
                    var approved = base.Query(t => t.SystemID == entity.SystemID).Select(t => t.IsApproved).FirstOrDefault();
                    if (approved == false)
                    {
                        _leaveTransactionDetailsService.ExecuteSqlCommand(@"DELETE FROM [dbo].LeaveTransactionDetails WHERE LvTrnsSystemID ='" + entity.SystemID + "'");
                    }
                    else
                    {
                        throw new CustomException("Data update is not allowed.");
                    }
                }

                if (string.IsNullOrEmpty(entity.SystemID))
                {

                    var pk = _signatrueService.GetAutoNumber("LT-", DateTime.Now).ToString();
                    entity.SystemID = "LT-" + DateTime.Now.ToString("yy") + "-" + pk;

                    var fDate = Convert.ToDateTime(entity.FromDate);
                    var tDate = Convert.ToDateTime(entity.ToDate);

                    //TimeSpan diff = tDate - fDate;
                    //entity.LeaveDays = Convert.ToDecimal(diff.Days + 1);
                    entity.LvReason = "Maternity Leave.";
                    entity.AppliedDate = DateTime.Now;
                    var child = GetCheck(entity.MaternityLeavePolicyId, entity.EmpSystemID);
                    if (child.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Baby No :" + child.Tables[0].Rows[0]["ChildNo"] + " already exists.");

                    }

                    entity.DateAdded = DateTime.Now;
                    var lvtypeId = GetLeaveType();
                    if (lvtypeId.Tables[0].Rows.Count > 0)
                    {
                        entity.LTSystemID = lvtypeId.Tables[0].Rows[0]["Id"].ToString();
                    }
                    Insert(entity);
                }
                else
                {
                    entity.DateUpdated = DateTime.Now;
                    Update(entity);

                }
                LeaveTransactionDetails details = new LeaveTransactionDetails
                {
                    LvTrnsSystemID = entity.SystemID,
                    AddedBy = entity.AddedBy,
                    DateAdded = DateTime.Now//------------
                };

                _leaveTransactionDetailsService.InsertGraph(null, new List<string>(), new List<string>(), details, entity.FromDate, Convert.ToDateTime(entity.ToDate), 1, false);

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<object> Query(string empId)
        {
            try
            {
                string CmdText = @"SELECT FORMAT(l.FromDate,'dd-MMM-yyyy')FromDate,format(L.ToDate,'dd-MMM-yyyy')ToDate,L.LeaveDays,format(L.ExpectedDelivaryDate,'dd-MMM-yyyy')ExpectedDelivaryDate,
                                    Approved = case when L.IsApproved =0 THEN 'NO' ELSE 'YES' END,L.MaternityLeavePolicyId,L.EmpSystemID,L.SystemID,L.IsApproved
                                  	,FORMAT(mlp.EffectiveDate, 'dd-MMM-yyyy') EffectiveDate,mlp.ChildNo,L.LTSystemID
                                  	,format(DateAdd(DAY, - mlp.MaternityStartDay, L.ExpectedDelivaryDate), 'dd-MMM-yyyy') AS MaternityStartDay
                                  	,format(DateAdd(DAY, mlp.MaternityEndDay, L.ExpectedDelivaryDate), 'dd-MMM-yyyy') AS MaternityEndDay                                 	
                                      FROM LeaveTransaction L
                                      LEFT JOIN [MST].[MaternityLeavePolicy] AS mlp ON mlp.Id = L.MaternityLeavePolicyId
                                      LEFT JOIN LeaveType LT ON LT.Id = L.LTSystemID
                                        WHERE L.EmpSystemID = '" + empId + @"' and LT.LeaveType='Maternity'                                       	
                                                ";
                return _sqlRepository.GetDataCollection(CmdText, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void DeleteGraph(string id)
        {

            var from_db = Find(id);

            var detailsData = _leaveTransactionDetailsService.Query(r => r.LvTrnsSystemID == id).Select().ToList();
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                if (from_db.IsApproved)
                {
                    throw new CustomException("Leave already approved.");
                }

                foreach (var item in detailsData)
                {
                    _leaveTransactionDetailsService.Delete(item.SystemID);
                }

                base.DeleteGraph(from_db);
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public DataSet GetCheck(string MaternityLeavePolicyId, string empId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT MLP.ChildNo FROM LeaveTransaction L
                            LEFT JOIN  [MST].[MaternityLeavePolicy] MLP on MLP.Id=l.MaternityLeavePolicyId
                            WHERE L.MaternityLeavePolicyId ='" + MaternityLeavePolicyId + @"' AND EmpSystemId ='" + empId + @"' "
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetLeaveCheck(string Id, DateTime fromDate, DateTime? toDate, string empId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @" select * from LeaveTransaction where SystemID<> '" + Id + "' and (('" + fromDate + "' between FromDate and ToDate) or ('" + toDate + "' between FromDate and ToDate) )and EmpSystemID='" + empId + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetUnApproveLeave(string EmpSystemID, string SystemID) ////GetUnApproveLeave
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"  select lt.SystemID from LeaveTransaction lt
					  left join LeaveType ly on ly.Id=lt.LTSystemID
					   where EmpSystemID='" + EmpSystemID + "'  and SystemID<>'" + SystemID + "' and IsApproved=0   AND ly.LeaveType='Maternity'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetLeaveType()
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT Id FROM LeaveType Where LeaveType='Maternity'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetCheckApproved(string id)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT IsApproved FROM LeaveTransaction Where SystemId ='" + id + "' "
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        public IEnumerable<object> GetFemaleEmployee(string plantId)
        {
            try
            {
                string CmdText = @"SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                              Where EMP.PlantId='" + plantId + "' AND EMP.GenderID='Female' AND EMP.EmployeeStatus='Active' ORDER BY EmployeeCodePreFix ,EmployeeCodeNumeric ";
                return _sqlRepository.GetDataCollection(CmdText, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> getemployeeDelete(string plantId, string CompanyId)
        {
            try
            {
                var Today = DateTime.Now;
                string FirstDayOfTheMonth = "01-" + Convert.ToDateTime(Today).ToString("MMM") + "-" + Convert.ToDateTime(Today).ToString("yyyy");
                string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");

                string CmdText = @"SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy') DOC
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                         FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                              Where EMP.PlantId='" + plantId + @"' 
                                AND EMP.EmployeeStatus='Active'  And
                            (EMP.DOJ <= '" + LastDayOfTheMonth + @"') and (DOS IS NULL OR EMP.DOS>='" + FirstDayOfTheMonth + @"') And EMP.CompanyId='" + CompanyId + @"'
                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

                return _sqlRepository.GetDataCollection(CmdText, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> getFixedOTemployee(string YearNo, string MonthNo, string plantId, string CompanyId)
        {
            try
            {

                string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
                string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");

                string CmdText = @"   SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy') DOC
										,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        ,ew.MinimumOT 
                                        ,Allow= Case when ew.IsExcessAllowed=1 then 'YES' ELSE 'NO' END
										from EmployeeWiseFixedOTSetting ew
                                        left join  EmployeeInformation EMP on emp.SystemId = ew.EmpSystemId										
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                              Where EMP.PlantId='" + plantId + @"' AND EMP.EmployeeStatus='Active' 	and 
							  (EMP.DOJ <= '" + LastDayOfTheMonth + @"') and (DOS IS NULL OR EMP.DOS>='" + FirstDayOfTheMonth + @"') And EMP.CompanyId='" + CompanyId + @"'
							   ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public IEnumerable<object> getChildNo(string Id, string PlantId)
        {
            try
            {
                var sql = @"select MaternityStartDay,
                             MaternityEndDay,CanAvailAfterDOJ,
                            MaternityLeaveStartDay,
                             MaternityLeaveEndDay  from [MST].[MaternityLeavePolicy] where Id='" + Id + "' and plantId='" + PlantId + "' ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public IWorkbook LeaveReport(string fromDate, string toDate, string plantId, string employeeCodeString, string companyGroupId)
        {
            ExcelEngine excelEngine = null;
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheetLeave(ref sheet1, oRU, "Leave Info", employeeCodeString, "Leave Info", fromDate, toDate, plantId, companyGroupId);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IWorkbook ShortLeaveReport(string date, string companyGroupId, string plantId, string employeeCodeString)
        {
            ExcelEngine excelEngine = null;
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                string sheetHeader = "Short Leave Info";
                string sheetName = "Short Leave Info";
                string leaveStatus = "ShortLeave";
                CreateShortandHalfDayLeaveXls(ref sheet1, oRU, sheetHeader, employeeCodeString, sheetName, date, plantId, companyGroupId, leaveStatus);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IWorkbook HalfDayLeaveReport(string date, string companyGroupId, string plantId, string employeeCodeString)
        {
            ExcelEngine excelEngine = null;
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                string sheetHeader = "Half Day Leave Info";
                string sheetName = "Half Day Leave Info";
                string leaveStatus = "HalfDayLeave";

                CreateShortandHalfDayLeaveXls(ref sheet1, oRU, sheetHeader, employeeCodeString, sheetName, date, plantId, companyGroupId, leaveStatus);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IWorkbook EmpEncashReport(string year, string plantId, string companyGroupId)
        {
            ExcelEngine excelEngine = null;
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheetLEncash(ref sheet1, oRU, "Earn Leave Report", "Earn Leave Report", year, plantId, companyGroupId);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IWorkbook EmpEncashReportOld(string fromDate, string toDate, string plantId, string companyGroupId)
        {
            ExcelEngine excelEngine = null;
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheetLEncashOld(ref sheet1, oRU, "Leave Encashment", "Leave Encashment", fromDate, toDate, plantId, companyGroupId);

                workbook.Version = ExcelVersion.Excel2013;
                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public void Listofdate(ref IWorksheet sheet1, ref int xlsColIndex, int xlsRow, DataTable dt, out List<listofdate> list)
        {
            try
            {
                List<DateTime> L_date = new List<DateTime>();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var c = (dt.Columns[i].ColumnName);
                    if (c.ToUpper() != "EMPLOYEEID" && c.ToUpper() != "LEAVETYPEID")
                    {
                        var c2 = Convert.ToDateTime(dt.Columns[i].ColumnName);
                        L_date.Add(c2);
                    }
                }

                L_date.Sort((a, b) => a.CompareTo(b));


                ReportUtility oru = new ReportUtility();
                list = new List<listofdate>();
                //for (int i = 0; i < dt.Columns.Count; i++)
                for (int i = 0; i < L_date.Count; i++)
                {
                    var c = L_date[i].ToString("dd-MMM-yyyy");
                    if (c.ToUpper() != "EMPLOYEEID" && c.ToUpper() != "LEAVETYPEID")
                    {
                        xlsColIndex++;
                        listofdate ob = new listofdate();
                        ob.ColIndex = xlsColIndex;
                        ob.Text = c;
                        list.Add(ob);
                        oru.SetHeaderText(ref sheet1, xlsRow, xlsColIndex, ob.Text, 6);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string SalaryHead(string plantId, out string salaryHeadLists)
        {
            string strsql = "";
            string stringSalaryHeadId = "";
            strsql = @"SELECT LTD.LvEncashmentFormulaDesID from LeavePolicyDetail LTD 
                LEFT JOIN LeaveType lt on lt.Id = LTD.LTSystemID
                LEFT JOIN LeavePolicyMaster lpm on lpm.SystemID = LTD.lpmSystemId
                WHERE LeaveType = 'Earn'  and lpm.PlantID = '" + plantId + @"'";
            DataTable dtLeaveEnc = _sqlRepository.GetDataTable(strsql);
            string subject = dtLeaveEnc.Rows[0]["LvEncashmentFormulaDesID"].ToString();
            string[] allTexts = subject.Split(' ');
            string resultString = "''";
            for (int i = 0; i < allTexts.Length; i++)
            {
                if (allTexts[i].Trim() != "")
                    resultString += ",'" + allTexts[i] + "'";
            }


            salaryHeadLists = subject;

            return resultString;
        }
        /// <summary>
        /// IsProrataPreviousyear
        /// IsProratacurrentyear
        /// IsAvailExceptionAllowedOnSpecialAppeal
        /// LeaveType
        /// CurrentYearAllocation
        /// AppliedDays
        /// PreviousYearCarryForward
        /// AppliedDays
        /// DaysCanBeSanctioned
        /// </summary>
        /// <param name="IsProrataPreviousyear"></param>
        /// <param name="IsProratacurrentyear" ></param>
        /// <param name="IsAvailExceptionAllowedOnSpecialAppeal"></param>
        public void DaysCanbeSanctioned(DataRow SourceRow, out decimal LeaveDays, out decimal Balance)
        {
            try
            {
                LeaveDays = 0;
                Balance = 0;
                bool proDataPrevYear = Convert.ToBoolean(SourceRow["IsProrataPreviousyear"].ToString());
                bool proDataCurrentYear = Convert.ToBoolean(SourceRow["IsProratacurrentyear"].ToString());
                bool isAvailExceptionAllowed = Convert.ToBoolean(SourceRow["IsAvailExceptionAllowedOnSpecialAppeal"].ToString());


                //drLocal["AppliedDays"] = SourceRow["AppliedDays"].ToString().Trim();
                //drLocal["Availed"] = SourceRow["Availed"].ToString().Trim();

                if (SourceRow["LeaveType"].ToString().Trim().ToUpper() != "EARN")
                {
                    if (proDataCurrentYear == false)
                    {
                        #region 01
                        if (proDataPrevYear == false)
                        {
                            LeaveDays = Convert.ToDecimal(SourceRow["CurrentYearAllocation"].ToString().Trim());
                            Balance = Convert.ToDecimal(SourceRow["CurrentYearAllocation"].ToString().Trim()) - Convert.ToDecimal(SourceRow["AppliedDays"].ToString().Trim());
                        }
                        else
                        {
                            LeaveDays = Convert.ToDecimal(SourceRow["CurrentYearAllocation"].ToString().Trim()) + Convert.ToDecimal(SourceRow["PreviousYearCarryForward"].ToString().Trim());
                            Balance = Convert.ToDecimal(SourceRow["CurrentYearAllocation"].ToString().Trim()) + Convert.ToDecimal(SourceRow["PreviousYearCarryForward"].ToString().Trim()) - Convert.ToDecimal(SourceRow["AppliedDays"].ToString().Trim());
                        }
                        #endregion
                    }
                    else
                    {
                        #region 02
                        //if (isAvailExceptionAllowed == false)
                        //{
                        if (proDataPrevYear == false)
                        {
                            LeaveDays = Convert.ToDecimal(SourceRow["DaysCanBeSanctioned"].ToString().Trim());
                            Balance = Convert.ToDecimal(SourceRow["DaysCanBeSanctioned"].ToString().Trim()) - Convert.ToDecimal(SourceRow["AppliedDays"].ToString().Trim());
                        }
                        else
                        {
                            LeaveDays = Convert.ToDecimal(SourceRow["DaysCanBeSanctioned"].ToString().Trim()) + Convert.ToDecimal(SourceRow["PreviousYearCarryForward"].ToString().Trim());
                            Balance = Convert.ToDecimal(SourceRow["DaysCanBeSanctioned"].ToString().Trim()) + Convert.ToDecimal(SourceRow["PreviousYearCarryForward"].ToString().Trim()) - Convert.ToDecimal(SourceRow["Applied"].ToString().Trim());
                        }
                        //}
                        //else
                        //{
                        //    if (proDataPrevYear == false)
                        //    {
                        //        LeaveDays = Convert.ToDecimal(SourceRow["CurrentYearAllocation"].ToString().Trim());
                        //        Balance = Convert.ToDecimal(SourceRow["CurrentYearAllocation"].ToString().Trim()) - Convert.ToDecimal(SourceRow["AppliedDays"].ToString().Trim());
                        //    }
                        //    else
                        //    {
                        //        LeaveDays = Convert.ToDecimal(SourceRow["CurrentYearAllocation"].ToString().Trim()) + Convert.ToDecimal(SourceRow["PreviousYearCarryForward"].ToString().Trim());
                        //        Balance = Convert.ToDecimal(SourceRow["CurrentYearAllocation"].ToString().Trim()) + Convert.ToDecimal(SourceRow["PreviousYearCarryForward"].ToString().Trim()) - Convert.ToDecimal(SourceRow["AppliedDays"].ToString().Trim());
                        //    }
                        //}
                        #endregion
                    }
                }
                else
                {
                    LeaveDays = Convert.ToDecimal(SourceRow["DaysCanBeSanctioned"].ToString().Trim());
                    Balance = Convert.ToDecimal(SourceRow["DaysCanBeSanctioned"].ToString().Trim()) - Convert.ToDecimal(SourceRow["AppliedDays"].ToString().Trim());
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
                return _maternityLeaveTransactionRepository.SqlQuery<OrgStructureListViewModel>(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CreateSheetLeave(ref IWorksheet sheet1, ReportUtility oRU, string SheetHeader, string employeeCodeString, string SheetName, string fromDate, string toDate, string plantId, string companyGroupId)
        {
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            try
            {
                if (employeeCodeString == "undefined")
                {
                    employeeCodeString = null;
                }
                var dtEmp = GetEmpInfo(fromDate, toDate, plantId, employeeCodeString);

                if (dtEmp.Rows.Count == 0)
                    throw new CustomException("No Leave found for any emploees between the selected date range !");
                var dtLeaveDetail = GetLeaveDetaiInfo(fromDate, toDate, employeeCodeString);
                if (dtLeaveDetail.Rows.Count < 0)
                    throw new CustomException("No Leave found for any emploees between the selected date range !");
                xlsRow = 4;
                List<listofdate> _list = null;

                if (dtEmp.Rows.Count > 0)
                {
                    #region ------------------Column Header------------------
                    xlsCol = 1;
                    xlsRow += 1;
                    int c_ec = 0;
                    int E_Name = 0;
                    int E_LType = 0;
                    int E_DCS = 0;
                    int E_LAB = 0;
                    int E_LAA = 0;
                    int E_TLA = 0;
                    int E_TLB = 0;

                    c_ec = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Code"); xlsCol += 1;
                    //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].ColumnWidth = 08;
                    E_Name = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Name"); xlsCol += 1;
                    //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].ColumnWidth = 16;
                    E_LType = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Leave Type"); xlsCol += 1;
                    //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].ColumnWidth = 06;
                    E_DCS = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Can Be Sanctioned"); xlsCol += 1;
                    //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].ColumnWidth = 10;
                    E_LAB = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Availed Before");
                    //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].ColumnWidth = 08;

                    Listofdate(ref sheet1, ref xlsCol, xlsRow, dtLeaveDetail, out _list); xlsCol += 1;
                    E_LAA = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Availed After"); xlsCol += 1;
                    //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].ColumnWidth = 08;
                    E_TLA = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Total Availed"); xlsCol += 1;
                    //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].ColumnWidth = 06;
                    E_TLB = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Balance");
                    //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].ColumnWidth = 06;

                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightGreen;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header-----------------
                    string Empid = string.Empty;
                    int lrow = 0;
                    bool Isfst = true;
                    int frow = 0;
                    for (int i = 0; i < dtEmp.Rows.Count; i++)//e lt
                    {
                        #region --------data----------
                        if (Empid != dtEmp.Rows[i]["EmployeeId"].ToString() && i != 0)
                        {
                            lrow = xlsRow;
                            Isfst = true;

                            sheet1.Range[frow, 1, lrow, 1].Merge();
                            sheet1.Range[frow, 2, lrow, 2].Merge();
                        }
                        else
                        {
                            if ((dtEmp.Rows.Count - 1) == i)
                            {
                                lrow = xlsRow + 1;
                                sheet1.Range[frow, 1, lrow, 1].Merge();
                                sheet1.Range[frow, 2, lrow, 2].Merge();
                            }
                        }
                        Empid = dtEmp.Rows[i]["EmployeeId"].ToString();
                        var LvType = dtEmp.Rows[i]["LeaveTypeId"].ToString();

                        xlsRow += 1;
                        xlsCol = 1;
                        if (Isfst)
                        {
                            frow = xlsRow;
                            Isfst = false;
                        }
                        oRU.SetCellText(sheet1, xlsRow, c_ec, dtEmp.Rows[i]["EmployeeCode"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_Name, dtEmp.Rows[i]["EmployeeName"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_LType, dtEmp.Rows[i]["Code"].ToString());
                        var DCS = Convert.ToDouble(dtEmp.Rows[i]["DaysCanBeSanctioned"]);

                        decimal dcbs = 0;
                        decimal bln = 0;
                        DaysCanbeSanctioned(dtEmp.Rows[i], out dcbs, out bln);

                        oRU.SetCellText(sheet1, xlsRow, E_DCS, Convert.ToDecimal(dcbs).ToString());

                        var TLB = Convert.ToDouble(dtEmp.Rows[i]["LAB"]);
                        var TLA = Convert.ToDouble(dtEmp.Rows[i]["LAA"]);
                        oRU.SetCellText(sheet1, xlsRow, E_LAB, TLB, true);
                        oRU.SetCellText(sheet1, xlsRow, E_LAA, TLA, true);

                        string LVafterAailed = "=sum(" + oRU.GetColumnNameForXls(E_LAB) + xlsRow + ":" + oRU.GetColumnNameForXls(E_LAA) + xlsRow + ")";
                        oRU.SetFormula(ref sheet1, xlsRow, E_TLA, LVafterAailed, true);

                        var balance = "=" + oRU.GetColumnNameForXls(E_DCS) + xlsRow + "-" + oRU.GetColumnNameForXls(E_TLA) + xlsRow + "";

                        oRU.SetFormula(ref sheet1, xlsRow, E_TLB, balance, true);

                        if (dtLeaveDetail != null && dtLeaveDetail.Rows.Count > 0)
                        {
                            DataView dvLeaveD = new DataView(dtLeaveDetail);
                            dvLeaveD.RowFilter = "EmployeeId='" + Empid + "' and LeaveTypeId='" + LvType + "'";
                            if (dvLeaveD.Count > 0)
                            {
                                DataTable dtleaveD = dvLeaveD.ToTable();
                                for (int d = 0; d < _list.Count; d++)
                                {
                                    var ob = _list[d];
                                    xlsCol = ob.ColIndex;
                                    var v = dtleaveD.Rows[0][ob.Text].ToString();
                                    if (v == null || v.Length == 0)
                                    {
                                        v = "0";
                                    }
                                    oRU.SetCellText(sheet1, xlsRow, xlsCol, Convert.ToDouble(v), true);
                                }//for
                            }//count 
                        }//null check
                        #endregion --------data----------
                    }// emp + ltype
                    xlsCol = 2;
                    xlsRow += 5;
                    //endXlsCol = 5;
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Name = SheetName;

                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].Merge();
                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                    var LeaveReportHeader = "Employee Leave Information From Date: " + fromDate + " To Date: " + toDate;
                    oRU.PlantHeader(ref sheet1, endXlsCol, LeaveReportHeader, plantId);
                    oRU.PageSetup(ref sheet1, 4, ExcelPageOrientation.Portrait);

                    #region UsedRange Alignment
                    //sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment
                }//emp count
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CreateSheetLEncash(ref IWorksheet sheet1, ReportUtility oRU, string SheetHeader, string SheetName, string year, string plantId, string companyGroupId)
        {
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            var currentYear = DateTime.Now.ToString("yyyy");
            string toDate = DateTime.Now.ToString("dd-MMM-yyyy");
            string fromDate = "01-Jan-" + year;
            try
            {
                string salaryHeadId = "";
                string salaryHeads = "";

                salaryHeadId = SalaryHead(plantId, out salaryHeads);


                var dtEmp = GetEmpEncashInfo(year, plantId);
                string payRollGroup = "";
                DataSet dsSlrProc = null;
                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmpSalaryInformationRpt(plantId, toDate, payRollGroup, salaryHeadId, out dsSlrProc);
                xlsRow = 4;

                if (dtEmp.Rows.Count > 0)
                {
                    #region ------------------Column Header------------------
                    xlsCol = 1;
                    xlsRow += 1;
                    int c_ec = 0;
                    int E_Name = 0;
                    int E_FNAME = 0;
                    int E_DOB = 0;
                    int E_DOJ = 0;
                    int E_BRATE = 0;
                    int E_ELB = 0;
                    int E_Amount = 0;
                    double wagesRate = 0.00;
                    c_ec = xlsCol;
                    sheet1.Range[5, xlsCol].RowHeight = 20;

                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Code"); xlsCol += 1;
                    E_Name = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Name"); xlsCol += 1;
                    E_FNAME = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Father Name"); xlsCol += 1;
                    E_DOB = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Date of Birth"); xlsCol += 1;
                    E_DOJ = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Date of Joining"); xlsCol += 1;
                    E_BRATE = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Wages"); xlsCol += 1;
                    E_ELB = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Earn Leave Balance"); xlsCol += 1;
                    E_Amount = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Amount");

                    //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightGreen;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header-----------------
                    string strReplace = salaryHeads;
                    for (int i = 0; i < dtEmp.Rows.Count; i++)//e lt
                    {
                        #region --------data----------

                        strReplace = salaryHeads;
                        xlsRow += 1;
                        xlsCol = 1;


                        if (dicEmpSalry.ContainsKey(dtEmp.Rows[i]["EmployeeId"].ToString()))
                        {
                            List<DataRow> drSalary = dicEmpSalry[dtEmp.Rows[i]["EmployeeId"].ToString()];
                            wagesRate = 0.00;

                            for (int ic = 0; ic < drSalary.Count; ic++)
                            {

                                strReplace = strReplace.Replace(drSalary[ic]["SalaryHeadID"].ToString().ToUpper(), drSalary[ic]["EntryAmount"].ToString());
                                wagesRate += clsStaticInfo.dbl(drSalary[ic]["EntryAmount"].ToString());
                            }
                            object value = null;
                            try
                            {

                                DataTable dt = new DataTable();
                                value = dt.Compute(strReplace, "");

                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }
                            finally
                            {
                                oRU.SetText(ref sheet1, xlsRow, E_BRATE, wagesRate);
                                oRU.SetText(ref sheet1, xlsRow, E_Amount, clsStaticInfo.dbl(value.ToString()) * clsStaticInfo.dbl(dtEmp.Rows[i]["ELbalance"].ToString()), false);

                            }

                        }



                        //oRU.SetCellText(sheet1, xlsRow, c_ec, dtEmp.Rows[i]["LeaveTypeId"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, c_ec, dtEmp.Rows[i]["EmployeeCode"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_Name, dtEmp.Rows[i]["EmployeeName"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_FNAME, dtEmp.Rows[i]["FatherName"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_DOB, dtEmp.Rows[i]["DOB"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_DOJ, dtEmp.Rows[i]["DOJ"].ToString());
                        sheet1.Range[xlsRow, E_BRATE].NumberFormat = oRU.NumberFormatDecimalTwo();
                        oRU.SetText(ref sheet1, xlsRow, E_ELB, clsStaticInfo.dbl(dtEmp.Rows[i]["ELbalance"].ToString()));
                        sheet1.Range[xlsRow, E_ELB].NumberFormat = oRU.NumberFormatDecimalTwo();

                        sheet1.Range[xlsRow, E_Amount].NumberFormat = oRU.NumberFormatDecimalTwo();

                        #endregion --------data----------
                    }// emp + ltype
                    xlsCol = 2;
                    xlsRow += 5;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.UsedRange.WrapText = true;
                    sheet1.Name = SheetName;



                    sheet1.Range[xlsRow, xlsCol].NumberFormat = oRU.NumberFormatDecimalTwo();

                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].Merge();
                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;

                    oRU.PlantHeader(ref sheet1, endXlsCol, SheetHeader, plantId);
                    oRU.PageSetup(ref sheet1, 4, ExcelPageOrientation.Portrait);


                    #region UsedRange Alignment
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment
                }//emp count
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, List<DataRow>> GetEmpSalaryInformationRpt(string plantId, string effectiveDate, string payRollGroup, string salaryHeadId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();

            string strSql = string.Empty;
            clsStaticInfo obs = null;
            try
            {

                obs = new clsStaticInfo();
                strSql = @"SELECT * FROM
                          (
                           SELECT E.SystemID EmpSystemID,  E.EmployeeCode EmployeeCode, E.EmployeeName, REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB,
	                              E.FatherName, E.MotherName, E.EmpType EmployeeType, E.EmploymentType EmploymentNature, E.NationalID,
	                              E.GenderID GenderName, REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ,
                                  REPLACE(Convert(VARCHAR(11), E.DOS, 106), ' ', '-') AS DOS,
	                              REPLACE(Convert(VARCHAR(11), E.DOC, 106), ' ', '-') AS DOC, DG.UserName DesignationGroup, D.UserName Designation,ISNULL(LG.UserName,'') LegalDesignation,
								  D.UserName GivenDesignation, L.UserName Line, U.UserName Unit, Dv.UserName Division, Dp.UserName Department,
								  S.UserName Section, SB.UserName SubSection, EC.UserName AS EmpCategory, Cm.UserName CompanyName, CAM.Address1,
	                              CAM.Address2, E.EmployeeCategorySystemID, E.UnitID, E.DivisionID, E.DepartmentID, E.DesignationSystemID,
	                              E.SectionID, E.SubSectionID, E.LineID, E.DesignationGroupID, E.SubSecStrucSystemID, E.EmployeeStatus,
	                              P.UserName PlantName, (PAM.[Address1] + ', ' + PAM.[Address2] + ', ' + PAMC.UserName + ' - ' + PAM.Postcode) FactoryAddress,
	                              GC.UserName GroupName, (CGAM.[Address1] + ', ' + CGAM.[Address2] + ', ' + CT.UserName + ' - ' + CGAM.Postcode + ', Contact: ' + CGAM.Phone) GroupAddress,
	                              E.PlantID, BK.UserName BankNameShort, E.BankAccNo, 
								  EmpSlr.SalaryHeadID, SH.SalaryHead, ISNULL(PSH.Sequence, 99) Sequence, SH.HeadType, ISNULL(SH.HeadCategory,'') HeadCategory, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount,
	                              EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, EmpSlr.AmtDefinitionCurrencyID, EmpSlr.AmtDefinationRate
	                              , EmpSlr.EmpInfoSystemID, MW.SalaryHeadValue                                
	                            ,CRC.IntegerInDisb, CRC.DecimalNo, MW.Grade,CRC.IsDecimalInDisb IsDecimal
                                ,ISNULL(E.GenderID,'') Gender,ISNULL(LSalGr.Code,'') GradeCode


											,ISNULL(PG.UserName,'') PayRollGroup

                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
				            FROM (SELECT * FROM EmployeeInformation  WHERE (EmployeeStatus != 'Separated' or DOS is null or DOS >='" + effectiveDate + @"')) AS E

                                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                                            LEFT JOIN ORG.Section S ON E.SectionID = S.Id
                                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                                           LEFT JOIN[MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode

                                            LEFT JOIN ORG.Line L ON MB.LineID = L.Id
											LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                            LEFT JOIN HKP.DesignationGroup DG ON E.DesignationGroupID = Dg.Id
                                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                                            LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
                                            LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LG.Id and E.PlantId = LSGD.PlantId
                                            LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = LSGD.LegalSalaryGradeId  and E.PlantId = LSalGr.PlantId
												
											LEFT JOIN ORG.Plant AS p ON E.PlantId = p.Id
											LEFT JOIN ORG.Company AS Cm ON E.CompanyID = Cm.Id
											LEFT JOIN ORG.CompanyGroup AS GC ON E.GroupID = GC.Id
											LEFT JOIN HKP.Bank AS BK ON E.BankSystemID = BK.Id
											LEFT JOIN MST.AddressMaster AS CAM ON Cm.AddressMasterId = CAM.Id
											LEFT JOIN MST.AddressMaster AS PAM ON P.AddressMasterId = PAM.Id
											LEFT JOIN MST.AddressMaster AS CGAM ON GC.AddressMasterId = CGAM.Id
											LEFT JOIN SCS.City AS PAMC ON PAM.CityId = PAMC.Id
											LEFT JOIN SCS.City AS CT ON CGAM.CityId = CT.Id
                                            LEFT JOIN
													(
													 SELECT ECT.Id, ECT.UserName, DM.DesignationId 
													  FROM [HKP].[EmployeeCategory] ECT
																	LEFT JOIN MST.DesignationMaster DM ON ECT.Id = DM.EmployeeCategoryId
													) EC ON EC.DesignationId = E.GivenDesignationId
											LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + effectiveDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = E.SystemId

										
												INNER JOIN (
													SELECT * FROM
																(
																 --SELECT MST.EmpInfoSystemID, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
																	--	EmpSlr.AmtDefinitionCurrencyID AmtDefinationCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID
																-- FROM SalaryInfoDefine EmpSlr
																	--	INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID 
                                                                   Select SalaryDetails.* from  ( SELECT MAX(EffectiveDate) EffectiveDate,EmpInfoSystemID--,SalaryHead,SalaryHeadID,EntryCurrencyID
	 FROM (
	           SELECT MST.EmpInfoSystemID,SH.SalaryHead, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
				EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID,MST.EffectiveDate
					FROM SalaryInfoDefine EmpSlr
					INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID AND MST.IsApproved = 1 
					left outer join SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
					--where EmpInfoSystemID = '1800118'
					UNION
					SELECT SBM.EmpInfoSystemID,SH.SalaryHead,SIB.SalaryHeadID,SIB.EntryCurrencyID,SIB.EntryAmount,SIB.DefineCurrencyID,SIB.DefineAmount
					,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, SIB.AmtDefinitionRate, SBM.SalaryRuleMasterSystemID,SBM.EffectiveDate from SalaryInfoBack SIB
					INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID 
					left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID
					--where EmpInfoSystemID = '1800118'
                        )dd where EffectiveDate <= '" + effectiveDate + @"' 					

					GROUP BY EmpInfoSystemID) effDateSalary


					Inner JOIN
					
            ( SELECT EmpInfoSystemID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount 
			,AmtDefinitionCurrencyID , AmtDefinationRate, SalaryRuleMasterSystemID,EffectiveDate
	            FROM (
	           SELECT MST.EmpInfoSystemID,SH.SalaryHead, EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount, 
				EmpSlr.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionRate AmtDefinationRate, MST.SalaryRuleMasterSystemID,MST.EffectiveDate
					FROM SalaryInfoDefine EmpSlr
					INNER JOIN SalaryInfoDefineMaster MST ON EmpSlr.SalaryID = MST.SystemID AND MST.IsApproved = 1
					LEFT OUTER JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
				--	WHERE EmpInfoSystemID = '1800118'
					UNION
					SELECT SBM.EmpInfoSystemID,SH.SalaryHead,SIB.SalaryHeadID,SIB.EntryCurrencyID,SIB.EntryAmount,SIB.DefineCurrencyID,SIB.DefineAmount
					,SIB.AmtDefinitionCurrencyID AmtDefinitionCurrencyID, SIB.AmtDefinitionRate, SBM.SalaryRuleMasterSystemID,SBM.EffectiveDate from SalaryInfoBack SIB
					INNER JOIN SalaryInfoBackMaster SBM ON SIB.SalaryID = SBM.SystemID 
					left outer join SalaryHead SH ON SH.SalaryHeadID = SIB.SalaryHeadID
				--	where EmpInfoSystemID = '1800118'
                )dd where EffectiveDate <= '" + effectiveDate + @"'  ) SalaryDetails ON effDateSalary.EffectiveDate= SalaryDetails.EffectiveDate and effDateSalary.EmpInfoSystemID = SalaryDetails.EmpInfoSystemID



                                                                  -----------------------AND MST.IsApproved = 1---------------------
																) A
																
													) EmpSlr ON E.SystemID = EmpSlr.EmpInfoSystemID
										LEFT JOIN SalaryHead SH ON SH.SalaryHeadID = EmpSlr.SalaryHeadID
										LEFT JOIN (SELECT * FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId='" + plantId + @"') PSH ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
										
										LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EmpSlr.SalaryRuleMasterSystemID 
										--LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID	AND SRG.SalaryHeadID = SH.SalaryHeadID									
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = SH.SalaryHeadID

                                        
                         ) A  where  ISNULL(EmpInfoSystemID,'')<>'' AND PlantID = '" + plantId + @"' AND
                            Convert(date ,DOJ) <='" + effectiveDate + @"' AND (DOS IS NULL OR DOS >='" + effectiveDate + @"') AND SalaryHeadID in (" + salaryHeadId + @") ";




                strSql = strSql + @" ORDER BY EmployeeCode";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpSystemID"].ToString();
                }

                return dicBonus;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function  

        private void CreateShortandHalfDayLeaveXls(ref IWorksheet sheet1, ReportUtility oRU, string SheetHeader, string employeeCodeString, string SheetName, string date, string plantId, string companyGroupId, string LeaveStatus)
        {

            try
            {
                #region Variable
                DataTable dtEntity = null;
                DataTable dtPosition = null;
                var xlsRow = 1; var xlsCol = 1; var IsBudgetCodeApplicable = true;
                #endregion Variable
                oRU = new ReportUtility();
                GetEntityPosition(companyGroupId, out DataSet dsEntityPosition);
                var dtEmpInfo = GetShortLeaveAndHalfDayLeaveInfo(date, companyGroupId, plantId, employeeCodeString, LeaveStatus);//SQL Query Function
                using (var dvEntity = new DataView(dsEntityPosition.Tables[0])
                {
                    RowFilter = "RType = 'Entity'",
                    Sort = "eSequence"
                })
                {
                    dtEntity = dvEntity.ToTable(true, "UserName");
                    using (var dvPosition = new DataView(dsEntityPosition.Tables[0])
                    {
                        RowFilter = @"RType = 'Position'",
                        Sort = "pSequence"
                    })
                    {
                        dtPosition = dvPosition.ToTable(true, "UserName");

                        using (var dvBC = new DataView(dtEmpInfo))
                        {
                            var dtBC = dvBC.ToTable(true, "IsPositionCodeApplicable");
                            for (int i = 0; i < dtBC.Rows.Count; i++)
                            {
                                IsBudgetCodeApplicable = Convert.ToBoolean(dtEmpInfo.Rows[i]["IsPositionCodeApplicable"].ToString());
                                if (IsBudgetCodeApplicable)
                                {
                                    break;
                                }
                            }
                            if (dtEmpInfo.Rows.Count > 0)
                            {
                                xlsRow = 5;
                                #region variable
                                var cEmployeeCode = 0; var cBudgetCode = 0; var cName = 0; var cDOJ = 0; var cDOB = 0;
                                var cDesignation = 0; var cGivenDesignation = 0; var cLD = 0;
                                var endXlsCol = 0; var colNum = 0; var cSl = 0;
                                #endregion variable

                                xlsRow++;
                                xlsCol = 1;

                                #region Header
                                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sl. No.", 6); cSl = xlsCol; xlsCol++;
                                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeCode"); cEmployeeCode = xlsCol; xlsCol++;

                                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Name", 30); cName = xlsCol; xlsCol++;
                                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOB"); cDOB = xlsCol; xlsCol++;
                                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOJ"); cDOJ = xlsCol; xlsCol++;

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
                                xlsCol--;
                                endXlsCol = xlsCol;
                                xlsRow++;
                                var slCount = 0;
                                for (int i = 0; i < dtEmpInfo.Rows.Count; i++)
                                {
                                    slCount++;
                                    #region Loop

                                    oRU.SetText(ref sheet1, xlsRow, cSl, slCount.ToString());
                                    oRU.SetText(ref sheet1, xlsRow, cEmployeeCode, dtEmpInfo.Rows[i]["EmployeeCode"].ToString());
                                    oRU.SetText(ref sheet1, xlsRow, cName, dtEmpInfo.Rows[i]["EmployeeName"].ToString());
                                    oRU.SetText(ref sheet1, xlsRow, cDOB, dtEmpInfo.Rows[i]["DOB"].ToString());
                                    oRU.SetText(ref sheet1, xlsRow, cDOJ, dtEmpInfo.Rows[i]["DOJ"].ToString());
                                    if (Convert.ToBoolean(dtEmpInfo.Rows[i]["IsPositionCodeApplicable"].ToString()))
                                    {
                                        oRU.SetText(ref sheet1, xlsRow, cBudgetCode, dtEmpInfo.Rows[i]["BudgetCode"].ToString());
                                        for (int c = 0; c < dtEntity.Rows.Count; c++)
                                        {
                                            var _colname = dtEntity.Rows[c]["UserName"].ToString();
                                            var v = dtEmpInfo.Rows[i]["e" + _colname].ToString();
                                            colNum = cBudgetCode + c + 1;
                                            oRU.SetText(ref sheet1, xlsRow, colNum, v);
                                        }
                                        for (int c = 0; c < dtPosition.Rows.Count; c++)
                                        {
                                            var _colname = dtPosition.Rows[c]["UserName"].ToString();
                                            oRU.SetText(ref sheet1, xlsRow, colNum + c + 1, dtEmpInfo.Rows[i]["p" + _colname].ToString());
                                        }
                                    }//is bc applicable
                                    oRU.SetText(ref sheet1, xlsRow, cDesignation, dtEmpInfo.Rows[i]["Designation"].ToString());
                                    oRU.SetText(ref sheet1, xlsRow, cGivenDesignation, dtEmpInfo.Rows[i]["GivenDesignation"].ToString());
                                    if (dtEmpInfo.Rows[i]["Designation"].ToString().ToUpper() != dtEmpInfo.Rows[i]["GivenDesignation"].ToString().ToUpper())
                                    {
                                        sheet1.Range[xlsRow, cDesignation].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                        sheet1.Range[xlsRow, cDesignation].CellStyle.Font.Color = ExcelKnownColors.White;
                                        sheet1.Range[xlsRow, cGivenDesignation].CellStyle.ColorIndex = ExcelKnownColors.Red;
                                        sheet1.Range[xlsRow, cGivenDesignation].CellStyle.Font.Color = ExcelKnownColors.White;
                                    }
                                    oRU.SetText(ref sheet1, xlsRow, cLD, dtEmpInfo.Rows[i]["LegalDesignation"].ToString());
                                    #endregion Loop
                                    xlsRow++;
                                }

                                oRU.SetHeaderText(ref sheet1, 4, 1, "On " + DateTime.Now.ToString("dd-MMM-yyyy"), ExcelHAlign.HAlignCenter);
                                sheet1.Range[4, 1, 4, endXlsCol].Merge();

                                if (!string.IsNullOrEmpty(plantId))
                                    oRU.PlantHeader(ref sheet1, endXlsCol, SheetHeader, plantId);
                                else
                                    oRU.MainCompanyGroupHeader(ref sheet1, endXlsCol, SheetHeader, companyGroupId);

                                #region UsedRange Alignment
                                sheet1.UsedRange.WrapText = true;
                                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                                #endregion UsedRange Alignment

                                oRU.PageSetupAuto(ref sheet1, 5, ExcelPageOrientation.Landscape, "TS");
                                sheet1.Name = SheetName;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
        private DataTable GetEmpInfo(string fromDate, string toDate, string plantId, string employeeCodeString)
        {
            try
            {
                string wc = string.Empty;

                if (!string.IsNullOrEmpty(employeeCodeString))
                {
                    wc = " AND EI.EmployeeCode IN(" + employeeCodeString + @")";
                }
                var sql = @"select   EI.EmployeeCode
		                            ,EI.EmployeeName
                                    ,ei.SystemId
		                            ,L.Code
		                            ,isnull (LS.DaysCanBeSanctioned ,0) DaysCanBeSanctioned	
		                            ,L.LeaveType	
		                            ,ls.LeaveTypeId
		                            ,ls.EmployeeId
                                    ,isnull(b.LAB,0) LAB
                                    ,isnull(A.LAA,0) LAA
									,LD.IsProrataPreviousyear
                                    ,LD.IsProratacurrentyear
                                    ,LD.IsAvailExceptionAllowedOnSpecialAppeal
                                    ,LS.CurrentYearAllocation
                                    ,LS.PreviousYearCarryForward PreviousYearCarryForward
                                    ,LS.AppliedDays
                 from (select * from TRN.EmployeeLeaveSummary where CalanderYearId
				 in
						(
						select id from YearlyCalendar where PlantId='" + plantId + @"' and '" + fromDate + @"' between FromDate and ToDate
						)
				 ) LS
                 Left join EmployeeInformation EI on EI.SystemId=ls.EmployeeId
                 LEFT JOIN LeaveType  L ON L.Id=LS.LeaveTypeId
                 left join (
				 select m.DesignationId,d.LTSystemID,IsProrataPreviousyear,IsAvailExceptionAllowedOnSpecialAppeal,IsProratacurrentyear
				  from mst.DesignationMaster m
				  left join scs.DesignationMasterConfiguration c on m.id=c.DesignationMasterId and c.PlantId='" + plantId + @"'
				  left join LeavePolicyDetail d on d.LPMSystemID=c.LeavePolicyMasterId
				 ) LD ON  LD.LTSystemID=LS.LeaveTypeId and ld.DesignationId=ei.GivenDesignationId
                  -----------------------START---------LAB----------
                  left join 
                 (select LT.EmpSystemID x,ls.LeaveTypeId LTSystemID,sum(d.LeaveDuration) LAB_OLD ,ls.EmployeeId EmpSystemID,ls.CurrentYearAvailedOpeningBalance
				 ,isnull(sum(d.LeaveDuration),0)+ls.CurrentYearAvailedOpeningBalance LAB
				 from   (select * from TRN.EmployeeLeaveSummary where CalanderYearId
				 in
						(
						select id from YearlyCalendar where PlantId='" + plantId + @"' and '" + fromDate + @"' between FromDate and ToDate
						)
				 ) LS
				 LEFT JOIN LeaveTransaction LT  ON LS.LeaveTypeId=LT.LTSystemID and ls.EmployeeId=lt.EmpSystemID
                 left join (select * from LeaveTransactionDetails where WorkDate < '" + fromDate + @"' AND IsAvailed=1  ) D on D.LvTrnsSystemID=LT.SystemID
                 group by  LT.EmpSystemID,ls.LeaveTypeId,ls.EmployeeId,ls.CurrentYearAvailedOpeningBalance  )  B on b.EmpSystemID=ls.EmployeeId and b.LTSystemID=ls.LeaveTypeId 
                left join 
                      -----------------LAA-----------
                  (select LT.EmpSystemID,lt.LTSystemID,sum(d.LeaveDuration) LAA 
                  from  LeaveTransaction LT  
                  left join LeaveTransactionDetails D on D.LvTrnsSystemID=LT.SystemID
                         where d.WorkDate between  '" + toDate + @"' and (
						select ToDate from YearlyCalendar where PlantId='" + plantId + @"' and '" + fromDate + @"' between FromDate and ToDate
						)
                   AND D.IsAvailed=1
                  group by  LT.EmpSystemID,lt.LTSystemID) A on A.EmpSystemID=ls.EmployeeId and A.LTSystemID=ls.LeaveTypeId
                  where EI.PlantId = '" + plantId + @"' and ls.EmployeeId in 
				    (select	lt.EmpSystemID
					from  LeaveTransaction LT  
					left join LeaveTransactionDetails D on D.LvTrnsSystemID=LT.SystemID
					where d.WorkDate BETWEEN DOJ AND '" + toDate + @"' AND D.IsAvailed=1
					)
                " + wc + @"
                  order by ls.EmployeeId ";
                var list = _sqlRepository.GetDataTable(sql);
                if (list.IsNull())
                    throw new CustomException("No Data Found");
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DataTable XGetEmpEncashInfo(string fromDate, string toDate, string plantId)
        {
            var currentYear = DateTime.Now.ToString("yyyy");
            var fDate = "01-Jan-" + currentYear;
            try
            {
                var sql = @"select          
                                    ls.EmployeeId
		                            ,EI.EmployeeName
									,convert (int, EI.EmployeeCode) EmployeeCode 
									,EI.FatherName
									,REPLACE(Convert(VARCHAR(11), EI.DOB, 106), ' ', '-') AS  DOB
                                    ,REPLACE (Convert(VARCHAR(11), Ei.DOJ, 106), ' ', '-') AS  DOJ 
                                    ,SD.EntryAmount  
                                    ,ei.SystemId
		                            ,L.Code
		                            ,isnull (LS.DaysCanBeSanctioned ,0) DaysCanBeSanctioned	
		                            ,L.LeaveType	
		                            ,ls.LeaveTypeId
									,isnull (Btn.AVL,0)  availebal
									,(DaysCanBeSanctioned-isnull(Btn.AVL,0)) ELbalance
									,(SD.EntryAmount )/26*(DaysCanBeSanctioned-isnull (Btn.AVL,0)) Amount
									,LD.IsProrataPreviousyear
                                    ,LD.IsProratacurrentyear
                                    ,LD.IsAvailExceptionAllowedOnSpecialAppeal
                                    ,LS.CurrentYearAllocation
                                    ,LS.PreviousYearCarryForward PreviousYearCarryForward
                                    ,LS.AppliedDays
                     from (select * from TRN.EmployeeLeaveSummary where CalanderYearId
				 in
						(
						select id from YearlyCalendar where PlantId='" + plantId + @"' and '" + fDate + @"' between FromDate and ToDate
						)
				 ) LS
                  Left join EmployeeInformation EI on EI.SystemId=ls.EmployeeId
                  LEFT JOIN LeaveType  L ON L.Id=LS.LeaveTypeId
				  left join  SalaryInfoDefineMaster sm on  sm.EmpInfoSystemID=EI.SystemId
		          left join SalaryInfoDefine sd on sd.SalaryID=sm.SystemID
		          left join SalaryHead sh on sh.SalaryHeadID=sd.SalaryHeadID

                  left join (
				  select m.DesignationId,d.LTSystemID,IsProrataPreviousyear,IsAvailExceptionAllowedOnSpecialAppeal,IsProratacurrentyear
				  from mst.DesignationMaster m
				  left join scs.DesignationMasterConfiguration c on m.id=c.DesignationMasterId and c.PlantId='20188'
				  left join LeavePolicyDetail d on d.LPMSystemID=c.LeavePolicyMasterId
				 ) LD ON  LD.LTSystemID=LS.LeaveTypeId and ld.DesignationId=ei.GivenDesignationId

		         -------------*************** availed between --------start----------		 
                  left join 
					(
						select LT.EmpSystemID x,ls.LeaveTypeId LTSystemID,ls.EmployeeId EmpSystemID,ls.CurrentYearAvailedOpeningBalance
						,isnull(sum(d.LeaveDuration),0)+ls.CurrentYearAvailedOpeningBalance AVL
						    from (select * from TRN.EmployeeLeaveSummary where CalanderYearId
				 in
						(
						select id from YearlyCalendar where PlantId='" + plantId + @"' and '" + fDate + @"' between FromDate and ToDate
						)
				 ) LS
						LEFT JOIN LeaveTransaction LT  ON LS.LeaveTypeId=LT.LTSystemID and ls.EmployeeId=lt.EmpSystemID
						left join (select * from LeaveTransactionDetails where WorkDate between '" + fDate + @"' AND '" + toDate + @"' AND IsAvailed=1  ) D on D.LvTrnsSystemID=LT.SystemID
					  
						group by  LT.EmpSystemID,ls.LeaveTypeId,ls.EmployeeId ,ls.CurrentYearAvailedOpeningBalance
				    ) 
					  Btn on Btn.EmpSystemID=ls.EmployeeId and Btn.LTSystemID=ls.LeaveTypeId 
		      -------------*************** availed between -------- end----------	 
                left join 

                  (select LT.EmpSystemID,lt.LTSystemID,sum(d.LeaveDuration) LAA from  LeaveTransaction LT  
                  left join LeaveTransactionDetails D on D.LvTrnsSystemID=LT.SystemID
                  where d.WorkDate >  '" + toDate + @"' AND D.IsAvailed=1 
                  group by  LT.EmpSystemID,lt.LTSystemID) A on A.EmpSystemID=ls.EmployeeId and A.LTSystemID=ls.LeaveTypeId
                  where EI.PlantId = '" + plantId + @"' and ls.EmployeeId in 

				    (select	lt.EmpSystemID
					from  LeaveTransaction LT  
					left join LeaveTransactionDetails D on D.LvTrnsSystemID=LT.SystemID
					where d.WorkDate BETWEEN '" + fDate + @"' AND '" + toDate + @"' AND D.IsAvailed=1
					)
                 and   SH.HeadCategory ='Basic'   AND L.LeaveType='Earn'
				 
                 order by EI.EmployeeCode";


                var list = _sqlRepository.GetDataTable(sql);
                if (list.IsNull())
                    throw new CustomException("No Data Found");
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetEmpEncashInfo(string year, string plantId)
        {
            var currentYear = year;
            var toDate = DateTime.Now.ToString("dd-MMM-yyyy");
            var fromDate = "1-Jan-" + year;
            //fromDate = "01-Jan-" + currentYear;
            try
            {
                var sql = @"select          
                                    ls.EmployeeId
		                            ,EI.EmployeeName
								    ,ISNULL(EI.EmployeeCode,'') EmployeeCode 
									,EI.FatherName
									,REPLACE(Convert(VARCHAR(11), EI.DOB, 106), ' ', '-') AS  DOB
                                    ,REPLACE (Convert(VARCHAR(11), Ei.DOJ, 106), ' ', '-') AS  DOJ 
                                    ,SD.EntryAmount  
                                    ,ei.SystemId
		                            ,L.Code
		                            ,isnull (LS.DaysCanBeSanctioned ,0) DaysCanBeSanctioned	
		                            ,L.LeaveType	
		                            ,ls.LeaveTypeId
									,isnull (Btn.AVL,0)  availebal
                            --,ELbalance=ISNULL(let.BroughtForward,0)+ISNULL(LET.DaysCanBeSanctioned,0)-ISNULL(LET.AvailedLeave,0)-ISNULL(let.Days,0)-ISNULL(let.YearEndLapse,0)
				
				,ELbalance=CASE WHEN L.LeaveType='Earn' THEN
                            CASE WHEN
                            -----------------------------------DOJorDOC start -----------------------------------------------------------
                            CASE WHEN ltd.LvAvailedOnDOJ=1 THEN
                            CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter, ei.DOJ )
                            WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter, ei.DOJ )
                            WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter, ei.DOJ ) END
                            WHEN ltd.LvAvailedOnDOC=1 THEN
                            CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter, ei.DOC )
                            WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter, ei.DOC )
                            WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter, ei.DOC )
                            END
                            END
                            ---------------------------------------DOJorDOC start end-------------------------------------------------------
                            
                            > '" + toDate + @"' then
                            (CASE WHEN LS.IsEncashed =1 THEN ISNULL(LS.CarryForward, 0)+ISNULL(LS.EncashedInbetween, 0) ELSE ISNULL(LS.BroughtForward, 0)+isnull(LS.CarryForwardOpeningBalance,0) END)+isnull(LS.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(LS.EncashedInbetween,0)------No
                            ELSE (CASE WHEN LS.IsEncashed =1 THEN ISNULL(LS.CarryForward, 0)+ISNULL(LS.EncashedInbetween, 0) ELSE ISNULL(LS.BroughtForward, 0)+isnull(LS.CarryForwardOpeningBalance,0) END)+isnull(LS.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(LS.EncashedInbetween,0) END---Yes
                            ELSE (CASE WHEN LS.IsEncashed =1 THEN ISNULL(LS.CarryForward, 0)+ISNULL(LS.EncashedInbetween, 0) ELSE ISNULL(LS.BroughtForward, 0)+isnull(LS.CarryForwardOpeningBalance,0) END)+isnull(LS.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(LS.EncashedInbetween,0) END -- isnull (Btn.AVL,0) ---No
                            


									--,(DaysCanBeSanctioned-isnull(Btn.AVL,0)) ELbalance
									,Amount = (SD.EntryAmount )/26*(
									CASE WHEN L.LeaveType='Earn' THEN
                            CASE WHEN
                            -----------------------------------DOJorDOC start -----------------------------------------------------------
                            CASE WHEN ltd.LvAvailedOnDOJ=1 THEN
                            CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter, ei.DOJ )
                            WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter, ei.DOJ )
                            WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter, ei.DOJ ) END
                            WHEN ltd.LvAvailedOnDOC=1 THEN
                            CASE WHEN ltd.CanAvailUOM='Year' THEN DateAdd(YEAR,LvCanAvailAfter, ei.DOC )
                            WHEN ltd.CanAvailUOM='Month' THEN DateAdd(MONTH,LvCanAvailAfter, ei.DOC )
                            WHEN ltd.CanAvailUOM='Day' THEN DateAdd(DAY,LvCanAvailAfter, ei.DOC )
                            END
                            END
                            ---------------------------------------DOJorDOC start end-------------------------------------------------------
                            
                            > '" + toDate + @"' then
                            (CASE WHEN LS.IsEncashed =1 THEN ISNULL(LS.CarryForward, 0)+ISNULL(LS.EncashedInbetween, 0) ELSE ISNULL(LS.BroughtForward, 0)+isnull(LS.CarryForwardOpeningBalance,0) END)+isnull(LS.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(LS.EncashedInbetween,0)------No
                            ELSE (CASE WHEN LS.IsEncashed =1 THEN ISNULL(LS.CarryForward, 0)+ISNULL(LS.EncashedInbetween, 0) ELSE ISNULL(LS.BroughtForward, 0)+isnull(LS.CarryForwardOpeningBalance,0) END)+isnull(LS.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(LS.EncashedInbetween,0) END---Yes
                            ELSE (CASE WHEN LS.IsEncashed =1 THEN ISNULL(LS.CarryForward, 0)+ISNULL(LS.EncashedInbetween, 0) ELSE ISNULL(LS.BroughtForward, 0)+isnull(LS.CarryForwardOpeningBalance,0) END)+isnull(LS.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)-isnull(LS.EncashedInbetween,0) END -- isnull (Btn.AVL,0) ---No
                            
									) 
									
                                    ,LD.IsProratacurrentyear
                                    ,LD.IsAvailExceptionAllowedOnSpecialAppeal
                                    ,LS.CurrentYearAllocation
                                    ,LS.PreviousYearCarryForward PreviousYearCarryForward
                                    ,LS.AppliedDays
                     FROM (SELECT * FROM TRN.EmployeeLeaveSummary WHERE CalanderYearId
				 in
						(
						SELECT id FROM YearlyCalendar WHERE PlantId='" + plantId + @"' AND '" + fromDate + @"' BETWEEN FromDate AND ToDate
						)
				 ) LS
                  Left join EmployeeInformation EI on EI.SystemId = ls.EmployeeId
                  --Left join[dbo].[LeaveEncashmentTransaction] as let on let.EmpSystemId = EI.SystemId
LEFT JOIN LeaveType  L ON L.Id=LS.LeaveTypeId
left join SalaryInfoDefineMaster sm on  sm.EmpInfoSystemID=EI.SystemId
left join SalaryInfoDefine sd on sd.SalaryID=sm.SystemID
left join SalaryHead sh on sh.SalaryHeadID=sd.SalaryHeadID



left join(
select m.DesignationId, d.LTSystemID,  IsAvailExceptionAllowedOnSpecialAppeal, IsProratacurrentyear
from mst.DesignationMaster m

left join scs.DesignationMasterConfiguration c on m.id= c.DesignationMasterId and c.PlantId= '" + plantId + @"'

left join LeavePolicyDetail d on d.LPMSystemID= c.LeavePolicyMasterId
) LD ON  LD.LTSystemID=LS.LeaveTypeId and ld.DesignationId=ei.GivenDesignationId

left outer join(
                            --***********LV**********************

SELECT DC.LeavePolicyMasterId , lpm.PolicyName , e.SystemId EmpId, d.*
FROM

EmployeeInformation e

LEFT join MST.DesignationMaster DM ON e.GivenDesignationId= dm.DesignationId

LEFT JOIN SCS.DesignationMasterConfiguration DC

ON DM.Id= DC.DesignationMasterId AND dc.plantid= e.plantid

LEFT JOIN LeavePolicyDetail d ON d.LPMSystemID= dc.LeavePolicyMasterId

LEFT JOIN LeavePolicyMaster AS lpm ON lpm.SystemID= dc.LeavePolicyMasterId

where dc.plantid= '" + plantId + @"'
-- * ******************LV * **********************

) ltd on ltd.LTSystemID = L.Id AND ltd.EmpId=ei.SystemId
                            

		         -------------*************** availed between --------start----------		 
                  LEFT JOIN
                    (
                        SELECT LT.EmpSystemID x, ls.LeaveTypeId LTSystemID, ls.EmployeeId EmpSystemID, ls.CurrentYearAvailedOpeningBalance

                        , ISNULL(SUM(d.LeaveDuration),0)+LS.CurrentYearAvailedOpeningBalance AVL

                            FROM(SELECT* FROM TRN.EmployeeLeaveSummary WHERE CalanderYearId
                 IN

                        (
                        SELECT id FROM YearlyCalendar WHERE PlantId= '" + plantId + @"' AND '" + fromDate + @"' between FromDate and ToDate
                        )
				 ) LS

                        LEFT JOIN LeaveTransaction LT ON LS.LeaveTypeId=LT.LTSystemID and ls.EmployeeId=lt.EmpSystemID
                       LEFT JOIN(SELECT * from LeaveTransactionDetails WHERE WorkDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND IsAvailed = 1) D on D.LvTrnsSystemID=LT.SystemID

                     GROUP BY LT.EmpSystemID, ls.LeaveTypeId, ls.EmployeeId , ls.CurrentYearAvailedOpeningBalance

                 )

                      Btn on Btn.EmpSystemID=ls.EmployeeId and Btn.LTSystemID=ls.LeaveTypeId


                  LEFT JOIN(
                            select
                            tt.UserName LeaveType, t.EmpSystemID, t.LTSystemID, sum(isnull(d.LeaveDuration,0)) LeaveDuration
                              from
                            LeaveTransaction t
                            LEFT JOIN
                            (--detail
                            SELECT SUM(LeaveDuration) LeaveDuration, LvTrnsSystemID FROM LeaveTransactionDetails
                            WHERE IsAvailed= 1
                            AND WorkDate BETWEEN
                            (SELECT FromDate FROM YearlyCalendar WHERE YearNo = '" + currentYear + @"' and PlantId = '" + plantId + @"')
                            AND (SELECT ToDate FROM YearlyCalendar WHERE YearNo = '" + currentYear + @"' and PlantId = '" + plantId + @"')
                            GROUP BY LvTrnsSystemID
                            )--detail
                            d on t.SystemID=d.LvTrnsSystemID

                            left join LeaveType tt on tt.id= t.LTSystemID
                            where t.IsApproved= 1
                            group by tt.UserName , t.EmpSystemID, t.LTSystemID
                            ) kk on kk.LTSystemID=ls.LeaveTypeId and kk.EmpSystemID=ls.EmployeeId
			  -------------*************** availed between -------- end----------	 
                LEFT JOIN

                  (SELECT LT.EmpSystemID, lt.LTSystemID, sum(d.LeaveDuration) LAA from LeaveTransaction LT
                  LEFT JOIN LeaveTransactionDetails D on D.LvTrnsSystemID= LT.SystemID
                  WHERE d.WorkDate >  '" + toDate + @"' AND D.IsAvailed= 1
                  GROUP BY  LT.EmpSystemID, lt.LTSystemID) A on A.EmpSystemID=ls.EmployeeId and A.LTSystemID=ls.LeaveTypeId
                   where EI.PlantId = '" + plantId + @"' and ls.EmployeeId in

                     (SELECT lt.EmpSystemID
                     FROM  LeaveTransaction LT
 
                     LEFT JOIN LeaveTransactionDetails D on D.LvTrnsSystemID= LT.SystemID
 
                     WHERE d.WorkDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND D.IsAvailed= 1-- - and lt.EmpSystemID = '1800296'
                     )
                 AND SH.HeadCategory ='Basic'   AND L.LeaveType= 'Earn'


              ORDER BY EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric";


                var list = _sqlRepository.GetDataTable(sql);
                if (list.IsNull())
                    throw new CustomException("No Data Found");
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }



        private void CreateSheetLEncashOld(ref IWorksheet sheet1, ReportUtility oRU, string SheetHeader, string SheetName, string fromDate, string toDate, string plantId, string companyGroupId)
        {
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            try
            {
                var dtEmp = GetEmpEncashInfoOld(fromDate, toDate, plantId);

                xlsRow = 4;

                if (dtEmp.Rows.Count > 0)
                {
                    #region ------------------Column Header------------------
                    xlsCol = 1;
                    xlsRow += 1;
                    int c_ec = 0;
                    int E_Name = 0;
                    int E_FNAME = 0;
                    int E_DOB = 0;
                    int E_DOJ = 0;
                    int E_BRATE = 0;
                    int E_ELB = 0;
                    int E_Amount = 0;

                    c_ec = xlsCol;
                    sheet1.Range[5, xlsCol].RowHeight = 20;

                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Code"); xlsCol += 1;
                    E_Name = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Name"); xlsCol += 1;
                    E_FNAME = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Father Name"); xlsCol += 1;
                    E_DOB = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Date of Birth"); xlsCol += 1;
                    E_DOJ = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Date of Joining"); xlsCol += 1;
                    E_BRATE = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Basic Rate"); xlsCol += 1;
                    E_ELB = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Earn Leave Balance"); xlsCol += 1;
                    E_Amount = xlsCol;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Amount");

                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightGreen;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header-----------------

                    for (int i = 0; i < dtEmp.Rows.Count; i++)//e lt
                    {
                        #region --------data----------
                        xlsRow += 1;
                        xlsCol = 1;
                        //oRU.SetCellText(sheet1, xlsRow, c_ec, dtEmp.Rows[i]["LeaveTypeId"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, c_ec, dtEmp.Rows[i]["EmployeeCode"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_Name, dtEmp.Rows[i]["EmployeeName"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_FNAME, dtEmp.Rows[i]["FatherName"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_DOB, dtEmp.Rows[i]["DOB"].ToString());
                        oRU.SetCellText(sheet1, xlsRow, E_DOJ, dtEmp.Rows[i]["DOJ"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, E_BRATE, Convert.ToInt32(dtEmp.Rows[i]["EntryAmount"]));
                        sheet1.Range[xlsRow, E_BRATE].NumberFormat = oRU.NumberFormatDecimalTwo();
                        oRU.SetText(ref sheet1, xlsRow, E_ELB, Convert.ToInt32(dtEmp.Rows[i]["ELbalance"]));
                        sheet1.Range[xlsRow, E_ELB].NumberFormat = oRU.NumberFormatDecimalTwo();
                        oRU.SetText(ref sheet1, xlsRow, E_Amount, Convert.ToInt32(dtEmp.Rows[i]["Amount"]));
                        sheet1.Range[xlsRow, E_Amount].NumberFormat = oRU.NumberFormatDecimalTwo();

                        #endregion --------data----------
                    }// emp + ltype
                    xlsCol = 2;
                    xlsRow += 5;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.UsedRange.WrapText = true;
                    sheet1.Name = SheetName;



                    sheet1.Range[xlsRow, xlsCol].NumberFormat = oRU.NumberFormatDecimalTwo();

                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].Merge();
                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet1.Range["A4"].Text = "Employee Leave Encashment From Date: " + fromDate + " To Date: " + toDate;
                    oRU.CompanyGroupHeader(ref sheet1, endXlsCol, "Leave Encashment", companyGroupId);
                    oRU.PageSetup(ref sheet1, 4, ExcelPageOrientation.Portrait);


                    #region UsedRange Alignment
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment
                }//emp count
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private DataTable GetEmpEncashInfoOld(string fromDate, string toDate, string plantId)
        {
            var currentYear = DateTime.Now.ToString("yyyy");
            var fDate = "01-Jan-" + currentYear;
            try
            {
                var sql = @"select          
                                    ls.EmployeeId
		                            ,EI.EmployeeName
									, EI.EmployeeCode EmployeeCode 
									,EI.FatherName
									,REPLACE(Convert(VARCHAR(11), EI.DOB, 106), ' ', '-') AS  DOB
                                    ,REPLACE (Convert(VARCHAR(11), Ei.DOJ, 106), ' ', '-') AS  DOJ 
                                    ,SD.EntryAmount  
                                    ,ei.SystemId
		                            ,L.Code
		                            ,isnull (LS.DaysCanBeSanctioned ,0) DaysCanBeSanctioned	
		                            ,L.LeaveType	
		                            ,ls.LeaveTypeId
									,isnull (Btn.AVL,0)  availebal
									,(DaysCanBeSanctioned-isnull(Btn.AVL,0)) ELbalance
									,(SD.EntryAmount )/26*(DaysCanBeSanctioned-isnull (Btn.AVL,0)) Amount
									--,LD.IsProrataPreviousyear
                                    ,LD.IsProratacurrentyear
                                    ,LD.IsAvailExceptionAllowedOnSpecialAppeal
                                    ,LS.CurrentYearAllocation
                                    ,LS.PreviousYearCarryForward PreviousYearCarryForward
                                    ,LS.AppliedDays
                     from (select * from TRN.EmployeeLeaveSummary where CalanderYearId
				 in
						(
						select id from YearlyCalendar where PlantId='" + plantId + @"' and '" + fDate + @"' between FromDate and ToDate
						)
				 ) LS
                  LEFT JOIN EmployeeInformation EI on EI.SystemId=ls.EmployeeId
                  LEFT JOIN LeaveType  L ON L.Id=LS.LeaveTypeId
				  LEFT JOIN  SalaryInfoDefineMaster sm on  sm.EmpInfoSystemID=EI.SystemId
		          LEFT JOIN SalaryInfoDefine sd on sd.SalaryID=sm.SystemID
		          LEFT JOIN SalaryHead sh on sh.SalaryHeadID=sd.SalaryHeadID

                  LEFT JOIN (
				  SELECT m.DesignationId,d.LTSystemID--,IsProrataPreviousyear
                    ,IsAvailExceptionAllowedOnSpecialAppeal,IsProratacurrentyear
				  FROM mst.DesignationMaster m
				  LEFT JOIN scs.DesignationMasterConfiguration c on m.id=c.DesignationMasterId and c.PlantId='20188'
				  LEFT JOIN LeavePolicyDetail d on d.LPMSystemID=c.LeavePolicyMasterId
				 ) LD ON  LD.LTSystemID=LS.LeaveTypeId and ld.DesignationId=ei.GivenDesignationId

		         -------------*************** availed between --------start----------		 
                  left join 
					(
						select LT.EmpSystemID x,ls.LeaveTypeId LTSystemID,ls.EmployeeId EmpSystemID,ls.CurrentYearAvailedOpeningBalance
						,isnull(sum(d.LeaveDuration),0)+ls.CurrentYearAvailedOpeningBalance AVL
						    from (select * from TRN.EmployeeLeaveSummary where CalanderYearId
				 in
						(
						select id from YearlyCalendar where PlantId='" + plantId + @"' and '" + fDate + @"' between FromDate and ToDate
						)
				 ) LS
						LEFT JOIN LeaveTransaction LT  ON LS.LeaveTypeId=LT.LTSystemID and ls.EmployeeId=lt.EmpSystemID
						left join (select * from LeaveTransactionDetails where WorkDate between '" + fDate + @"' AND '" + toDate + @"' AND IsAvailed=1  ) D on D.LvTrnsSystemID=LT.SystemID
					  
						group by  LT.EmpSystemID,ls.LeaveTypeId,ls.EmployeeId ,ls.CurrentYearAvailedOpeningBalance
				    ) 
					  Btn on Btn.EmpSystemID=ls.EmployeeId and Btn.LTSystemID=ls.LeaveTypeId 
		      -------------*************** availed between -------- end----------	 
                left join 

                  (select LT.EmpSystemID,lt.LTSystemID,sum(d.LeaveDuration) LAA from  LeaveTransaction LT  
                  left join LeaveTransactionDetails D on D.LvTrnsSystemID=LT.SystemID
                  where d.WorkDate >  '" + toDate + @"' AND D.IsAvailed=1 
                  group by  LT.EmpSystemID,lt.LTSystemID) A on A.EmpSystemID=ls.EmployeeId and A.LTSystemID=ls.LeaveTypeId
                  where EI.PlantId = '" + plantId + @"' and ls.EmployeeId in 

				    (select	lt.EmpSystemID
					from  LeaveTransaction LT  
					left join LeaveTransactionDetails D on D.LvTrnsSystemID=LT.SystemID
					where d.WorkDate BETWEEN '" + fDate + @"' AND '" + toDate + @"' AND D.IsAvailed=1
					)
                 and   SH.HeadCategory ='Basic'   AND L.LeaveType='Earn'
				 
                 order by EI.EmployeeCode";


                var list = _sqlRepository.GetDataTable(sql);
                if (list.IsNull())
                    throw new CustomException("No Data Found");
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private DataTable GetLeaveDetaiInfo(string fromDate, string toDate, string employeeCodeString)
        {
            try
            {
                string wc = string.Empty;

                if (!string.IsNullOrEmpty(employeeCodeString))
                {
                    wc = " and LT.EmpSystemID in (" + employeeCodeString + @")";
                }
                var sql = @"DECLARE @sql_ nvarchar(max)
                                    select  EmployeeId,LeaveTypeId, WorkDate,LeaveDuration--,SystemID
                                    INTO #tempOT
                                    from 
                                    (
                                    SELECT A.* FROM
	                                (
									select   EI.EmployeeCode
											,EI.EmployeeName
											,L.Code
											,LS.DaysCanBeSanctioned	
											,L.LeaveType	
											,d.WorkDate
											,d.SystemID
											,d.LeaveDuration 
											,ls.LeaveTypeId
											,ls.EmployeeId
 from TRN.EmployeeLeaveSummary LS
 left join LeaveTransaction LT  On LS.EmployeeId=LT.EmpSystemID AND LS.LeaveTypeId=LT.LTSystemID
 Left join EmployeeInformation EI on EI.SystemId=LT.EmpSystemID
 left join LeaveTransactionDetails D on D.LvTrnsSystemID=LT.SystemID
 LEFT JOIN LeaveType  L ON L.Id=LS.LeaveTypeId
 where d.WorkDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'  AND D.IsAvailed=1 " + wc + @"
									) A
                                    GROUP BY EmployeeCode,EmployeeName,LeaveType,Code,DaysCanBeSanctioned,LeaveDuration,WorkDate,SystemID,EmployeeId,LeaveTypeId
                            ) TT
	                            DECLARE @sql nvarchar(max),
                                    @col nvarchar(max)
                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(REPLACE(CONVERT(VARCHAR(11), WorkDate, 113), ' ', '-'))	
                                FROM #tempOT 
                                FOR XML PATH ('')  )
                            SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                                MAX([LeaveDuration]) FOR [WorkDate] IN ('+STUFF(@col,1,1,'')+')
                            ) as pvt)'
                            
                            EXEC sp_executesql @sql
                       
                            drop table #tempOT";
                var list = _sqlRepository.GetDataTable(sql);
                if (list.IsNull() || list.Rows.Count == 0)
                    throw new CustomException("No Data Found");
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private DataTable GetShortLeaveAndHalfDayLeaveInfo(string date, string companyGroupId, string plantId, string employeeCodeString, string leaveStatus)
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

                var cmdText = @"SELECT e.SystemId,e.EmployeeId,DT.DayType,e.EmployeeCode,Convert(INt,e.EmployeeCode) EmployeeCodeS,mpb.Code BudgetCode,e.EmployeeName
                                    --,e.DOJ
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
                                     ,PR.DepartmentId,PR.DivisionId,PMB.LineId
                                    ,e.PlantId,eN.UnitId,PR.SectionId,PR.SubDivisionId,PR.SubSectionId,DM.DesignationGroupId
                                    --emp info
                                    ,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    ,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
                                    ,EmpC.UserName empCategory,EmpC.Sequence CatgSequence,LT.UserName LeaveType,ELoc.UserName EmployeeLocation
									" + cList + @"

                                    FROM EmployeeInformation e
LEFT JOIN MST.ManpowerBudget mpb ON E.BudgetCode=mpb.Id
                                    LEFT JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN MST.DesignationMaster DesM ON E.GivenDesignationId = DesM.DesignationId
                                    LEFT JOIN HKP.EmployeeCategory EmpC ON EmpC.Id=DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN ORG.Department edept on edept.id=PO.DepartmentId
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=MPB.LineId
                                    LEFT OUTER JOIN ORG.Division ediv on ediv.id=PO.DivisionId
                                    LEFT OUTER JOIN ORG.SubDivision esdiv on esdiv.id=PO.SubDivisionId
                                    LEFT OUTER JOIN ORG.Section es on es.id=PO.SectionId
                                    LEFT OUTER JOIN ORG.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN ORG.Unit eu on eu.id=EN.UnitId
                                   -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=PR.DesignationID
                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=DesM.DesignationGroupId
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId
                                                                       
                                    LEFT OUTER JOIN HKP.EmployeeLocation ELoc on mpb.EmployeeLocationId=ELoc.Id
			                                       " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId
                                    LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
			                        LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId                                    
									LEFT OUTER JOIN LeaveType LT ON LT.Id = APD.LTSystemID

									LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
                                    WHERE e.EmployeeStatus = 'Active' AND " + param + @" ";

                cmdText += "AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + date + @"')";

                if (leaveStatus == "ShortLeave")
                {
                    cmdText += " AND APD.CountedShortLeave > 0";
                }
                else if (leaveStatus == "HalfDayLeave")
                {
                    cmdText += " AND APD.CountedShortLeave > 0";
                }
                cmdText += "APD.CountedShortLeave > 0 ORDER BY EmployeeCodeS ASC";

                var list = _sqlRepository.GetDataTable(cmdText);
                if (list.Rows.Count == 0)
                    throw new CustomException("No Data Found");
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public class listofdate
        {
            public string Text { get; set; }
            public int ColIndex { get; set; }
        }


        public IEnumerable<object> GetClanderYear(string plantId)
        {
            try
            {
                string sqlText = @"SELECT Id, YearNo FROM dbo.YearlyCalendar WHERE PlantId='" + plantId + "'";

                return _sqlRepository.GetDataCollection(sqlText, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IEnumerable<object> GetPolicyData(string EffectiveDate, String plantId)
        {
            try
            {
                string sqlText = @"  select p.* from
                                (
                                select max(EffectiveDate) ed,PlantId from [MST].[MaternityLeavePolicy] 
                                where '" + EffectiveDate + @"'>= (EffectiveDate) 
                                group by PlantId
                                ) x
                                left join [MST].[MaternityLeavePolicy] p on p.EffectiveDate=x.ed and p.PlantId=x.PlantId
								where x.PlantId='" + plantId + @"'
								order by EffectiveDate DESC,ChildNo";

                return _sqlRepository.GetDataCollection(sqlText);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


        private DataSet GetCosequtiveLeave(string SystemID, string EmpSystemID)
        {
            try
            {
                GridParameter parameters = null;
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"SELECT FORMAT(DATEADD(day, MLP.GapeBetweenConsecutiveIssue, MAX(ExpectedDelivaryDate)),'dd-MMM-yyyy') AS ToDate,MLP.GapeBetweenConsecutiveIssue
                                ,format(L.ExpectedDelivaryDate,'dd-MMM-yyyy') ExpectedDelivaryDate
                                        FROM LeaveTransaction L
                                LEFT JOIN LeaveType LT ON LT.Id=L.LTSystemID
                                LEFT JOIN [MST].[MaternityLeavePolicy] MLP ON MLP.Id=L.MaternityLeavePolicyId
                                WHERE L.SystemID<> '" + SystemID + @"' AND  L.EmpSystemID='" + EmpSystemID + "' AND LT.LeaveType='Maternity' GROUP BY ToDate,MLP.GapeBetweenConsecutiveIssue,ExpectedDelivaryDate"
                };
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public string GetFormatedDate(string date, string lng)
        {
            var formateDate = string.Empty;
            var day = cnDgt(date.Substring(0, 2), lng);
            var mon = ChangeMonth(date.Substring(3, 3), lng);
            var year = cnDgt(date.Substring(7, 4), lng);
            return formateDate = day + "-" + mon + "-" + year;
        }

        public string cnDgt(string input)
        {
            return input.Replace('0', '০')
                     .Replace('1', '১')
                     .Replace('2', '২')
                     .Replace('3', '৩')
                     .Replace('4', '৪')
                     .Replace('5', '৫')
                     .Replace('6', '৬')
                     .Replace('7', '৭')
                     .Replace('8', '৮')
                     .Replace('9', '৯');
        }

        public string cnDgt(string input, string lng)
        {
            if (lng == "Bengali")
            {
                return input.Replace('0', '০')
                    .Replace('1', '১')
                    .Replace('2', '২')
                    .Replace('3', '৩')
                    .Replace('4', '৪')
                    .Replace('5', '৫')
                    .Replace('6', '৬')
                    .Replace('7', '৭')
                    .Replace('8', '৮')
                    .Replace('9', '৯');
            }
            else if (lng == "Hindi")
            {
                return input.Replace('0', '०')
                    .Replace('1', '१')
                    .Replace('2', '२')
                    .Replace('3', '३')
                    .Replace('4', '४')
                    .Replace('5', '५')
                    .Replace('6', '६')
                    .Replace('7', '७')
                    .Replace('8', '८')
                    .Replace('9', '९');
            }
            else if (lng == "English")
            {
                return input.Replace('0', '0')
                    .Replace('1', '1')
                    .Replace('2', '2')
                    .Replace('3', '3')
                    .Replace('4', '4')
                    .Replace('5', '5')
                    .Replace('6', '6')
                    .Replace('7', '7')
                    .Replace('8', '8')
                    .Replace('9', '9');
            }
            return input;
        }


        #region new process




        public void CreateMaternityLeaveReportSheet(string reportType, string SystemId, string LanguageId, string plantId, string UserName, string LeaveTransactionId, string fromDate)
        {
            try
            {
                fromDate = Convert.ToDateTime(fromDate).AddMonths(-1).ToString("dd-MMM-yyyy");
                #region Query 
                var cmdText1 = @"SELECT EI.SystemId,EI.EmployeeNameLocal
                                    ,EI.EmployeeCode,LD.Name LegalDesignationLocal,SEC.Name Sectionlocal
                                    ,CAST(DATEDIFF(yy, EI.DOB, t.FromDate) AS varchar(4)) as [Year]
                                    ,CAST(DATEDIFF(mm, DATEADD(yy, DATEDIFF(yy, EI.DOB, t.FromDate), EI.DOB), t.FromDate) AS varchar(2)) as [Month]
                                    , CAST(DATEDIFF(dd, DATEADD(mm, DATEDIFF(mm, DATEADD(yy, DATEDIFF(yy, EI.DOB, t.FromDate), EI.DOB), t.FromDate), DATEADD(yy, DATEDIFF(yy, EI.DOB, t.FromDate), EI.DOB)), t.FromDate) AS varchar(2)) AS [Day]
                                    , EI.EmployeeName
                                    , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                                    , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                                    , DG.UserName GivenDesignation
                                    , DP.UserName Department
                                    , DSG.UserName LegalDesignation
                                    ,s.UserName Section
                                    ,ss.UserName Subsection
                                    ,ll.UserName Line
                                    ,format(t.fromdate,'dd-MMM-yyyy') LeaveStartDate
                                    ,FORMAT(t.FromDate,'dd-MMM-yyy') FromDate
                                    --============================new 
                                    ,format(mbm.BeforePaymentDate,'dd-MMM-yyyy') BeforePaymentDate
                                    ,mbm.LeaveDays
                                    ,mbm.IsPaidBefore IsBefore
                                    ,mbm.IsPaidAfter IsAfter
                                    --,isnull(mbm.AfterAmount,0)+isnull(mbm.BeforeAmount,0) TotalEarn
                                    ,format(mbm.AfterPaymentDate,'dd-MMM-yyyy') AfterPaymentDate
                                    ,AfterPercentageAmount=mbm.AfterAmount
                                    ,Rate=mbm.WageRate
                                    ,mbm.TotalWorkingDays TotalWorkingDays
                                    ,isnull(c.EntryAmount,0) TotalEarn
                                    ,TotlaEarning=mbm.WageRate*mbm.LeaveDays
                                    ,BeforePercentageAmount=mbm.BeforeAmount
                                    --============================ 
                                    
                                    FROM dbo.Employeeinformation EI
                                    LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    left join [ORG].[Plant] p on p.Id=EI.PlantId
                                    LEFT JOIN HKP.LegalDesignation DSG ON ei.LegalDesignationId=DSG.Id
                                    LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                                    LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId	
                                    LEFT JOIN org.Section s ON s.id=PR.SectionId
                                    LEFT JOIN org.SubSection ss ON ss.Id=PR.SubSectionId
                                    left join org.Line ll on ll.id=PMB.LineId
                                    LEFT JOIN HKP.LocalLanguage LD ON LD.LegalDesignationId=EI.LegalDesignationId 
                                    LEFT JOIN HKP.LocalLanguage SEC ON SEC.SectionId = PR.SectionId 
                                    left join MaternityBenefitMaster mbm on mbm.EmpSystemId=ei.SystemId
                                    LEFT JOIN (select sum(WorkingDays)as workingDays,sum(TotalEarnedAmount+EncashAmount+OtherAmount) as TotalEarn ,MaternityBenefitMasterId From MaternityBenefitDetail group by MaternityBenefitMasterId) mbd on mbd.MaternityBenefitMasterId=mbm.Id
                                    left join LeaveTransaction t on t.SystemID=mbm.LeaveTransactionId
                                   left join (
									select top 1 SP.* from SalaryProcChild SP
									left join SalaryProcMaster SPM on SPM.SystemID=SP.SlrProcMstSystemID
									where
									SalaryHeadID in (select SalaryHeadID from SalaryHead where HeadCategory in( 'Gross')) and EmpInfoSystemID='" + SystemId + @"'
									and MonthNo=MONTH('" + fromDate + @"') and YearNo=year('" + fromDate + @"')
									)c on c.EmpInfoSystemID='" + SystemId + @"'

                                    where ei.SystemId ='" + SystemId + @"' and EI.PlantId='" + plantId + @"' and mbm.LeaveTransactionId='" + LeaveTransactionId + @"'";
                
                var cmdText2 = @"select
                            YearNo,MonthNo
                            ,left( DateName( month , DateAdd( month , MonthNo , -1 )),3) [MonthName]
                            --,[MonthName]
                            ,SalaryProcessMasterId
                            ,isnull(StructureAmount,0) EarnedAmount
                            ,isnull(StructureAmount,0) StructureAmount
							,WorkingDays
							,(isnull(StructureAmount,0)/WorkingDays)*112 TotalAmount
                            from (--x
                            select m.MonthNo,m.YearNo
                            ,DateName( month , DateAdd( month , m.MonthNo , -1 )) [MonthName]
                            ,c.EntryAmount StructureAmount,c.DisbusmentAmount 
                            ,c.SlrProcMstSystemID SalaryProcessMasterId
	                      ,mbm.TotalWorkingDays WorkingDays
                            from
                            (
                            select * from SalaryProcChild where
                            SalaryHeadID in (select SalaryHeadID from SalaryHead where HeadCategory in( 'Gross'))
                            )c                           
                           
                            left join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
                            left join SalaryProceAttdnData att on att.SlrProcMstSystemID=m.SystemID and att.EmpSystemID='" + SystemId + @"'
                            left join (select c.*,m.EffectiveDate from BonusPaymentActual c left join [BonusPaymentActualMaster] m on m.SystemID=c.BnsMstSystemID
                            )b on b.EmpSystemID='" + SystemId + @"'  and month(b.EffectiveDate)=m.MonthNo and YEAR(b.EffectiveDate)=m.YearNo
                            
							left join MaternityBenefitMaster mbm on mbm.empsystemid=c.EmpInfoSystemID

                            where
                            c.SlrProcMstSystemID in (
                            select SystemID from SalaryProcMaster  Where MonthNo=MONTH('" + fromDate + @"') and YearNo=year('" + fromDate + @"')
                            )
                            and c.EmpInfoSystemID='" + SystemId + @"'
                            ) x";

                #endregion

                var GetMaternityLeaveReport = _sqlRepository.GetDataTable(cmdText1);
                DataTable dsHeader = _sqlRepository.GetDataTable(cmdText2);

                string File = "Mlr" + UserName + plantId + ".xlsx";
                string filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(filepath);

                IWorksheet sheet = workbook.Worksheets[0];
                sheet.ShowColumn(0, true);

                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();

                //clsDataContext data = new clsDataContext();

                IRange range = sheet.UsedRange;
                IRange columnList = range.Rows[0]; //IRange columnList = range.Rows[5];
                int columnListRow = 1;
                int ColumnTemplateRow = 1;
                int ColumnTemplateRowT = 1;
                int ColumnTemplateRowM = 1;
                for (int i = 0; i < range.Rows.Length; i++)
                {

                    if (string.IsNullOrEmpty(range["A" + (i + 1)].Text))
                        continue;
                    if (range["A" + (i + 1)].Text.ToUpper() == "COLUMNLIST")
                    {
                        columnListRow = (i + 1);

                    }
                    if (range["A" + (i + 1)].Text.ToUpper() == "REFROW")
                    {
                        columnList = range.Rows[i];
                        ColumnTemplateRow = (i + 1);
                    }
                    if (range["A" + (i + 1)].Text.ToUpper() == "REFROWT")
                    {
                        columnList = range.Rows[i];
                        ColumnTemplateRowT = (i + 1);
                    }
                    if (range["A" + (i + 1)].Text.ToUpper() == "REFROWM")
                    {
                        columnList = range.Rows[i];
                        ColumnTemplateRowM = (i + 1);
                    }

                }


                #region EmployeeInformation 

                for (int R = columnListRow; R <= columnListRow + 1; R++)
                {
                    IRange columnListEmp = range.Rows[R];
                    foreach (DataColumn item in GetMaternityLeaveReport.Columns)
                    {
                        for (int i = 0; i < range.Rows[R].Cells.Count(); i++)
                        {
                            if (string.IsNullOrEmpty(sheet[ColumnTemplateRow, i + 1].Text))
                                continue;

                            try
                            {
                                if (sheet[ColumnTemplateRow, i + 1].Text.ToUpper().Trim() == "{" + item.ColumnName.ToUpper() + "}")
                                {
                                    if (bplib.clsWebLib.IsNumeric(GetMaternityLeaveReport.Rows[0][item.ColumnName].ToString()))
                                        sheet[ColumnTemplateRow, i + 1].Number = clsStaticInfo.dbl(GetMaternityLeaveReport.Rows[0][item.ColumnName].ToString());

                                    else
                                        sheet[ColumnTemplateRow, i + 1].Text = GetMaternityLeaveReport.Rows[0][item.ColumnName].ToString();

                                    if (item.ColumnName.ToUpper() == "DOJ".ToUpper())
                                    {
                                        sheet[ColumnTemplateRow, i + 1].Text = GetFormatedDate(GetMaternityLeaveReport.Rows[0][item.ColumnName].ToString(), UserName);
                                    }
                                    if (item.ColumnName.ToUpper() == "FromDate".ToUpper())
                                    {
                                        sheet[ColumnTemplateRow, i + 1].Text = GetFormatedDate(GetMaternityLeaveReport.Rows[0][item.ColumnName].ToString(), UserName);
                                    }
                                    if (item.ColumnName.ToUpper() == "BeforePaymentDate".ToUpper())
                                    {
                                        sheet[ColumnTemplateRow, i + 1].Text = GetFormatedDate(GetMaternityLeaveReport.Rows[0][item.ColumnName].ToString(), UserName);
                                    }

                                }
                            }
                            catch (Exception)
                            {


                            }

                            if (string.IsNullOrEmpty(sheet[ColumnTemplateRowT, i + 1].Text))
                                continue;

                            try
                            {
                                if (clsStaticInfo.nullrecorder(sheet[ColumnTemplateRowT, i + 1].Text).ToUpper().Trim() == "{" + item.ColumnName.ToUpper() + "}")
                                {

                                    if (bplib.clsWebLib.IsNumeric(GetMaternityLeaveReport.Rows[0][item.ColumnName].ToString()))
                                    {
                                        sheet[ColumnTemplateRowT, i + 1].Number = clsStaticInfo.dbl(GetMaternityLeaveReport.Rows[0][item.ColumnName].ToString());
                                    }
                                    else
                                    {
                                        sheet[ColumnTemplateRowT, i + 1].Text = (GetMaternityLeaveReport.Rows[0][item.ColumnName].ToString());

                                    }
                                    if (item.ColumnName.ToUpper() == "AfterPaymentDate".ToUpper())
                                    {
                                        sheet[ColumnTemplateRowT, i + 1].Text = GetFormatedDate(GetMaternityLeaveReport.Rows[0][item.ColumnName].ToString(), UserName);
                                    }


                                }
                            }
                            catch (Exception)
                            {


                            }

                        }
                    }
                }

                #endregion
                CreateMaternityLeaveReportSheet(sheet, dsHeader, UserName);

                sheet.HideColumn(1);
                string fileNames = "MaternityLeaveReport.xlsx";
                workbook.SaveAs(fileNames, System.Web.HttpContext.Current.Response, ExcelDownloadType.Open);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public string ChangeMonth(string input, string lng)
        {
            if (lng == "Bengali")
            {
                return input
                    .Replace("Jan", "জানুয়ারি")
                    .Replace("Feb", "ফেব্রুয়ারি")
                    .Replace("Mar", "মার্চ")
                    .Replace("Apr", "এপ্রিল")
                    .Replace("May", "মে")
                    .Replace("Jun", "জুন")
                    .Replace("Jul", "জুলাই")
                    .Replace("Aug", "আগস্ট")
                    .Replace("Sep", "সেপ্টেম্বর")
                    .Replace("Oct", "অক্টোবর")
                    .Replace("Nov", "নভেম্বর")
                    .Replace("Dec", "ডিসেম্বর");
            }
            else if (lng == "Hindi")
            {
                return input
                    .Replace("Jan", "जनवरी")
                    .Replace("Feb", "फरवरी")
                    .Replace("Mar", "मार्च")
                    .Replace("Apr", "अप्रैल")
                    .Replace("May", "मई")
                    .Replace("Jun", "जून")
                    .Replace("Jul", "जुलाई")
                    .Replace("Aug", "अगस्त")
                    .Replace("Sep", "सितम्बर")
                    .Replace("Oct", "अक्तूबर")
                    .Replace("Nov", "नवम्बर")
                    .Replace("Dec", "दिसम्बर");
            }
            return input;
        }


        public void CreateMaternityLeaveReportSheet(IWorksheet sheet, DataTable dsHeader, string UserName)
        {
            try
            {
                IRange range = sheet.UsedRange;
                IRange columnList = range.Rows[0]; //IRange columnList = range.Rows[5];
                int columnListRow = 1;
                int ColumnTemplateRow = 1;

                for (int i = 0; i < range.Rows.Length; i++)
                {

                    if (string.IsNullOrEmpty(range["A" + (i + 1)].Text))
                        continue;
                    if (range["A" + (i + 1)].Text.ToUpper() == "COLUMNLISTM")
                    {
                        columnListRow = (i + 1);

                    }
                    if (range["A" + (i + 1)].Text.ToUpper() == "REFROWM")
                    {
                        columnList = range.Rows[i];
                        ColumnTemplateRow = (i + 1);
                    }
                }
                int lastCol = 6;
                int xlsROW = ColumnTemplateRow + 1;
                for (int R = 0; R < dsHeader.Rows.Count; R++)
                {
                    IRange columnListEmp = range.Rows[ColumnTemplateRow];
                    foreach (DataColumn item in dsHeader.Columns)
                    {
                        for (int i = 0; i < sheet.Rows[ColumnTemplateRow].Cells.Count(); i++)
                        {
                            if (string.IsNullOrEmpty(sheet[ColumnTemplateRow, i + 1].Text))
                                continue;

                            if (i > lastCol)
                                lastCol = i;
                            if (sheet[ColumnTemplateRow, i + 1].Text.ToUpper().Trim() == "{" + item.ColumnName.ToUpper() + "}")
                            {
                                if (bplib.clsWebLib.IsNumeric(dsHeader.Rows[R][item.ColumnName].ToString()))
                                    sheet[xlsROW, i + 1].Number = clsStaticInfo.dbl(dsHeader.Rows[R][item.ColumnName].ToString());
                                else
                                    sheet[xlsROW, i + 1].Text = ChangeMonth(dsHeader.Rows[R][item.ColumnName].ToString(), UserName);

                            }
                        }
                    }
                    xlsROW++;

                }

                sheet.Range[xlsROW, 2, xlsROW, 3].Text = "সর্বমোট";
                sheet.Range[xlsROW, 2, xlsROW, 3].Merge();
                sheet.Range[xlsROW, 2, xlsROW, 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                for (int i = 4; i <= lastCol + 1; i++)
                {
                    sheet[xlsROW, i].Formula = "SUM(" + clsStaticInfo.GetxlsCol(i) + ColumnTemplateRow.ToString() + ":" + clsStaticInfo.GetxlsCol(i) + (xlsROW - 1).ToString() + ")";
                }

                sheet.Range[xlsROW, 7].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[xlsROW, 7].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[xlsROW, 2, xlsROW, 7].CellStyle.Font.Bold = true;
                sheet.Range[xlsROW, 2, xlsROW, 7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[xlsROW, 2, xlsROW, 7].BorderAround(ExcelLineStyle.Hair);

                int RefROW = ColumnTemplateRow;
                sheet.HideColumn(1);
                sheet.DeleteRow(RefROW);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }
}
#endregion