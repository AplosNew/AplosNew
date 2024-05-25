#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Organizations;
using Library.Model.Payrolls;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.Pdf;
using Syncfusion.DocToPDFConverter;
using Syncfusion.OfficeChartToImageConverter;
using System.Text.RegularExpressions;
using clsAttendance;
//using Syncfusion.DocToPDFConverter;
//using Syncfusion.JavaScript.Models;
//using Syncfusion.OfficeChartToImageConverter;
//using Syncfusion.Pdf;
#endregion Using

namespace Library.Service.Employees
{
    public class EmployeeActiveInActiveService : Service<EmployeeInformation>, IEmployeeActiveInActiveService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<EmployeeMobileAppsAuthorization> _employeeAuthService;
        private readonly IRepositoryAsync<EmployeeBudgetCodeHistory> _employeeBudgetCodeHistoryService;
        private readonly IRepositoryAsync<DesignationMaster> _designationMasterRepository;
        private readonly IManpowerBudgetService _manpowerBudgetService;
        private readonly IRepositoryAsync<XLUploadDetail> _xLUploadDetailService;


        public EmployeeActiveInActiveService(
             IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , IRepositoryAsync<EmployeeInformation> employeeInformationRepository
            , IRepositoryAsync<EmployeeMobileAppsAuthorization> employeeAuthService
            , IRepositoryAsync<EmployeeBudgetCodeHistory> employeeBudgetCodeHistoryService
            , IRepositoryAsync<DesignationMaster> designationMasterRepository
             , IManpowerBudgetService manpowerBudgetService
            , ISqlRepository sqlRepository
            , IRepositoryAsync<XLUploadDetail> xLUploadDetailService) : base(employeeInformationRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _employeeAuthService = employeeAuthService;
            _employeeBudgetCodeHistoryService = employeeBudgetCodeHistoryService;
            _designationMasterRepository = designationMasterRepository;
            _manpowerBudgetService = manpowerBudgetService;
            _xLUploadDetailService = xLUploadDetailService;
        }

        #endregion Constructor

        #region Operation

        public void UpdateMaster(EmployeeInformation entity)
        {
            try
            {
                var dblist = Find(entity.SystemId);
                dblist.EmpPicPath = entity.EmpPicPath;
                dblist.GenderID = entity.GenderID;
                dblist.DOB = entity.DOB;
                dblist.CellPhnNo = entity.CellPhnNo;
                dblist.EmailId = entity.EmailId;
                dblist.Salutation = entity.Salutation;
                dblist.FirstName = entity.FirstName;
                dblist.MiddleName = entity.MiddleName;
                dblist.LastName = entity.LastName;
                dblist.NickName = entity.NickName;
                dblist.EmployeeName = entity.FirstName + " " + entity.MiddleName + " " + entity.LastName;
                dblist.BirthdayCelebrationDate = entity.BirthdayCelebrationDate;
                dblist.IsKnownPerson = entity.IsKnownPerson;
                dblist.NumberOfKnownPerson = entity.NumberOfKnownPerson;
                dblist.ApplyingAsFresher = entity.ApplyingAsFresher;
                dblist.NationalID = entity.NationalID;

                var emp = PlantWiseDOJ(dblist.PlantID);
                var nodays = PlantWiseDOJDays(dblist.PlantID);

                var isApproved = base.Query(t => t.SystemId == entity.SystemId).Select(t => t.IsApproved).FirstOrDefault();
                if (isApproved)
                {
                    throw new Exception("Update is not allowed.");
                }

                if (Convert.ToDateTime(entity.DOJ) < Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                {
                    if (emp.Tables[0].Rows.Count > 0)
                    {
                        var start = DateTime.Now;
                        var end = Convert.ToDateTime(entity.DOJ);

                        TimeSpan difference = start - end;
                        var days = Convert.ToInt32(difference.Days);
                        var date = Convert.ToInt32(nodays.Tables[0].Rows[0]["PastDOJDaysAllowed"]);
                        if (date < days)
                        {
                            throw new Exception("Maximum  " + nodays.Tables[0].Rows[0]["PastDOJDaysAllowed"] + " days back is allowed for DOJ.");
                        }
                        //allowed
                        dblist.DOJ = entity.DOJ;
                    }
                    else
                    {
                        throw new Exception("Previous Date of Join is not allowed");
                    }
                }
                else if (Convert.ToDateTime(entity.DOJ) > Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy")))
                {
                    throw new Exception("Future Date of Join is not allowed");
                }
                else
                {
                    dblist.DOJ = entity.DOJ;
                    //Current
                }

                dblist.DateUpdated = DateTime.Now;
                Update(dblist);

                var document = EmployeeDocFile(entity.SystemId);

                _designationMasterRepository.ExecuteSqlCommand(@"UPDATE EmployeeDocument SET FileId='" + entity.SystemId + @"', FileName='" + entity.EmpPicPath + @"' WHERE  Id = '" + document.Tables[0].Rows[0]["Id"].ToString() + @"'");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdatePersonal(EmployeeInformation entity)
        {
            try
            {
                var dblist = Find(entity.SystemId);

                dblist.FatherName = entity.FatherName;
                dblist.MotherName = entity.MotherName;
                dblist.CitizenID = entity.CitizenID;
                dblist.ReligionID = entity.ReligionID;
                dblist.BloodGroupID = entity.BloodGroupID;
                dblist.CivilStatusID = entity.CivilStatusID;
                dblist.SpouseName = entity.SpouseName;
                dblist.SpouseNationalID = entity.SpouseNationalID;
                dblist.SpouseOccupation = entity.SpouseOccupation;
                dblist.NoOfChildren = entity.NoOfChildren;
                dblist.TIN = entity.TIN;
                dblist.MarriagedayCelebrationDate = entity.MarriagedayCelebrationDate;
                dblist.DateUpdated = DateTime.Now;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public void UpdateAddress(EmployeeInformation entity)
        {
            try
            {
                var dblist = Find(entity.SystemId);
                dblist.PresentAddress1 = entity.PresentAddress1;
                dblist.PresentAddress2 = entity.PresentAddress2;
                dblist.ParmanentAddress1 = entity.ParmanentAddress1;
                dblist.ParmanentAddress2 = entity.ParmanentAddress2;
                dblist.PresThanaID = entity.PresThanaID;
                dblist.ParmThanaID = entity.ParmThanaID;
                dblist.PresPostOfficeID = entity.PresPostOfficeID;
                dblist.ParmPostOfficeID = entity.ParmPostOfficeID;
                dblist.PresZipCode = entity.PresZipCode;
                dblist.ParmZipCode = entity.ParmZipCode;
                dblist.PresDistrictID = entity.PresDistrictID;
                dblist.ParmDistrictID = entity.ParmDistrictID;
                dblist.PresCountryID = entity.PresCountryID;
                dblist.ParmCountryID = entity.ParmCountryID;
                dblist.PresCityID = entity.PresCityID;
                dblist.ParmCityID = entity.ParmCityID;
                dblist.PresAreaID = entity.PresAreaID;
                dblist.ParmAreaID = entity.ParmAreaID;
                dblist.EmrCntPer1Name = entity.EmrCntPer1Name;
                dblist.EmrCntPer2Name = entity.EmrCntPer2Name;
                dblist.EmrCntPer1CellNo = entity.EmrCntPer1CellNo;
                dblist.EmrCntPer2CellNo = entity.EmrCntPer2CellNo;
                dblist.ParmanentArea = entity.ParmanentArea;
                dblist.PresentArea = entity.PresentArea;
                dblist.ParmStateId = entity.ParmStateId;
                dblist.PresStateId = entity.PresStateId;
                dblist.EmployeeStatus = "Active";
                dblist.DateUpdated = DateTime.Now;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateSalaryInfo(EmployeeInformation entity)
        {
            try
            {
                var dblist = Find(entity.SystemId);
                dblist.PaymentMode = entity.PaymentMode;
                dblist.PaymentModeEffectiveDate = entity.PaymentModeEffectiveDate;
                dblist.DateUpdated = DateTime.Now;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetEmpDocumentDataList(string companyGroupId, string pId, string plantId)
        {
            try
            {
                var sql = @"SELECT  DISTINCT ED.*,
									CD.UserName DocumentName
									,CD.DocumentType
									,CD.IsSkillBased
									,CDSD.OptionalOrMandatory
									,CD.EmpType
									,CD.ProfileType,CD.DocNumberRequired,CD.DocDateRequired
									,E.UserName AS EmployeeCategory
									,CD.DependateDate
								FROM dbo.EmployeeDocument ED
								LEFT JOIN hkp.ComplianceDocument CD ON ED.ComplianceDocumentId = CD.Id
								LEFT JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CD.Id = CDSD.ComplianceDocumentId
								LEFT JOIN (SELECT  * FROM HKP.DocumentConfigurationDesignationGroup
								Where PlantId='" + plantId + @"' and EmployeeCategoryId = (
										SELECT D.EmployeeCategoryId
										FROM (SELECT * FROM MST.DesignationMaster WHERE CompanyGroupId = '" + companyGroupId + @"'
											) AS D
										LEFT JOIN EmployeeInformation EI ON D.DesignationId = EI.GivenDesignationId
										WHERE EI.SystemId = '" + pId + @"'
										)
								)DD ON CDSD.ComplianceDocumentSetId = DD.ComplianceDocumentSetId
								LEFT JOIN HKP.EmployeeCategory AS E ON DD.EmployeeCategoryId = E.Id
								WHERE ED.EmpSystemID = '" + pId + @"'
									--AND ISNULL(CD.ProfileType,'') NOT IN ('Qualification','Training','Experience','Photo')
									AND E.UserName IS NOT NULL ORDER BY CDSD.OptionalOrMandatory,DocumentName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetSelfDocumentDataList(string companyGroupId, string budgetId, string pId, string plantId)
        {
            try
            {
                var sql = @"SELECT  DISTINCT ED.*,
									CD.UserName DocumentName
									,CD.DocumentType
									,CD.IsSkillBased
									,CDSD.OptionalOrMandatory
									,CD.EmpType
									,CD.ProfileType,CD.DocNumberRequired,CD.DocDateRequired
									,E.UserName AS EmployeeCategory
									,CD.DependateDate
								FROM dbo.EmployeeDocument ED
								LEFT JOIN hkp.ComplianceDocument CD ON ED.ComplianceDocumentId = CD.Id
								LEFT JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CD.Id = CDSD.ComplianceDocumentId
								LEFT JOIN (SELECT  * FROM HKP.DocumentConfigurationDesignationGroup
								Where PlantId='" + plantId + @"' and EmployeeCategoryId = (
										SELECT D.EmployeeCategoryId
										FROM (SELECT * FROM MST.DesignationMaster WHERE CompanyGroupId = '" + companyGroupId + @"'
											) AS D
										LEFT JOIN EmployeeInformation EI ON D.DesignationId = EI.GivenDesignationId
										WHERE EI.BudgetCode = '" + budgetId + @"'
											AND EI.SystemId = '" + pId + @"'
										)
								)DD ON CDSD.ComplianceDocumentSetId = DD.ComplianceDocumentSetId
								LEFT JOIN HKP.EmployeeCategory AS E ON DD.EmployeeCategoryId = E.Id
								WHERE ED.EmpSystemID = '" + pId + @"'
									--AND CD.EmploymentStage = 'PreRecruitment'
									AND CD.DocumentationBy = 'Self'
									AND ISNULL(CD.ProfileType,'') NOT IN ('Qualification','Training','Experience','Photo')
									AND E.UserName IS NOT NULL Order By CDSD.OptionalOrMandatory";
                //AND PD.DueDate IS NOT NULL";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public object GetData(string companyGroupId, string companyId, string plantId, string employeeId)
        {
            try
            {
                var sql = @"SELECT EI.*
								  ,PO.UserName PresThanaName,ParmPO.UserName ParmThanaName,D.UserName PresDistrictName,ParmD.UserName ParmDistrictName
								  ,C.UserName PresCountryName,ParmC.UserName ParmCountryName,ParmP.UserName ParmPostOfficeName, PerP.UserName PresPostOfficeName
                                  ,PerCT.UserName PresCityName,ParCT.UserName ParmCityName,AM.CountryId
								  ,CG.[Image] CompanyGroupLogo, CNT.PhoneLength, COM.IsTINRequiredForSalaryAbove
								  ,CNT.TINCaption, CNT.NIDCaption, CNT.NIDLength, CNT.TINLength, COM.TINRequiredForSalaryAbove
                              FROM dbo.Employeeinformation EI
                              LEFT OUTER JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id
							  LEFT OUTER JOIN scs.PoliceStation PO ON EI.PresThanaID=PO.Id
							  LEFT OUTER JOIN scs.PoliceStation ParmPO ON EI.ParmThanaID=ParmPO.Id
							  LEFT OUTER JOIN SCS.District D ON EI.PresDistrictID = D.Id
							  LEFT OUTER JOIN SCS.District ParmD ON EI.ParmDistrictID = ParmD.Id
		                      LEFT OUTER JOIN SCS.Country C ON EI.PresCountryID = C.ID
		                      LEFT OUTER JOIN SCS.Country ParmC	ON EI.ParmCountryID = ParmC.ID
		                      LEFT OUTER JOIN SCS.PostOffice ParmP ON EI.ParmPostOfficeID = ParmP.ID
		                      LEFT OUTER JOIN SCS.PostOffice PerP ON EI.PresPostOfficeID = PerP.ID
                              LEFT OUTER JOIN SCS.City PerCT ON EI.PresCityID = PerCT.ID
		                      LEFT OUTER JOIN SCS.City ParCT ON EI.ParmCityID = ParCT.ID
                              LEFT OUTER JOIN SCS.[State] ParmS ON EI.ParmStateId = ParmS.Id
							  LEFT OUTER JOIN SCS.[State] PresS ON EI.PresStateId = PresS.Id
							  LEFT OUTER JOIN ORG.Plant PL ON EI.PlantId = PL.Id
							  LEFT OUTER JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
							  LEFT OUTER JOIN SCS.Country CNT ON AM.CountryId=CNT.Id
							  LEFT OUTER JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                              WHERE EI.GroupId='" + companyGroupId + @"' AND EI.CompanyId='" + companyId + @"' AND EI.PlantId='" + plantId + "' AND EI.SystemId='" + employeeId + "'";
                return _sqlRepository.GetData(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetJobData(string empid)
        {
            try
            {
                var sql = @"SELECT JDI.Id, JDI.UserName JobDescription from [MST].[ManpowerBudgetJobDescription] PMBJD
                            LEFT OUTER JOIN [HKP].[JobDescription] JD ON PMBJD.JobDescriptionId=JD.Id
                            LEFT OUTER JOIN [HKP].[JobDescriptionItem] JDI ON JD.JobDescriptionItemId=JDI.Id
                             Where PMBJD.ManpowerBudgetId=(Select EI.BudgetCode From [dbo].[EmployeeInformation] EI Where EI.SystemId='" + empid + "') AND PMBJD.Archive=0";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private DataSet PlantWiseDOJ(string plantId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT  IsPastDOJAllowed FROM dbo.PlantWiseHRMSSetting WHERE PlantId='" + plantId + @"' AND IsPastDOJAllowed=1"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet PlantWiseDOJDays(string plantId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT  PastDOJDaysAllowed FROM dbo.PlantWiseHRMSSetting WHERE PlantId='" + plantId + @"' AND IsPastDOJAllowed=1"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet EmployeeDocFile(string strSystemID)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM EmployeeDocument WHERE ComplianceDocumentId=(SELECT Id FROM HKP.ComplianceDocument WHERE ProfileType ='Photo') AND EmpSystemId ='" + strSystemID + @"'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }


        public IWorkbook EmployeeAppointmentLetterLocal(string companyGroupId, string companyId, string plantId, string empId, string empType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];
                workbook = CreateSheetMain_backup(ref sheet1, report, "Appointment Letter", "Appointment Letter", companyGroupId, companyId, plantId, empId, empType, tempId);
                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
               Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
               ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void EmployeeAppointmentLetterInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                CreateIDCardInWord(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void EmployeeServiceBookInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                CreateServiceBookInWord(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void EmployeeNomineeInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                CreateNomineeInWord(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void EmployeeJoiningLetterInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                CreateJoiningLetterInWord(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void EmployeeAcknowledgementInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {

                CreateAcknowledgementInWord(companyGroupId, companyId, plantId, empId, empType, reportType, tempId);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private void CreateServiceBookInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = 7;
                string reportTypeName = "";

                if (reportType == LetterType.ServiceBook.ToString())
                {
                    reportTypeName = LetterType.ServiceBook.GetDescription();
                }

                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Srv" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }
                //------

                ///-----
                var Templatefile = GetFilePath(plantId, tempId, reportType);
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

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "Srv" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

                bool IsDefLan = false;
                var tokens = (fileName.Substring(("Srv" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }

                DataTable dtEmp = GetEmployeeBasicInfoById(empId, plantId, empType, langID, tempId);
                DataTable dtSalary = SalaryDetailsForSB(empId, langID); // GetGrossAmount(empId);
                DataTable dtDisciplinaryAction = EmployeeDisciplinaryAction(empId, langID); // GetGrossAmount(empId);
                DataTable dtClanderYear = GetCurrentClanderYear(plantId);

                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                foreach (TextSelection item in allresult)
                {
                    string foundText = item.SelectedText;

                    if (replaced.ContainsKey(foundText) == false)
                        replaced.Add(foundText, 0);

                    //for fixed info
                    string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                    if (dtEmp.Columns.Contains(colName))
                    {

                        ////===== def lan 
                        if (IsDefLan == true)
                        {
                            if (IsDefLan == true)
                            {
                                colName = GetBasicInfoInDefaultLng(colName);
                            }
                        }
                        ///=====
                        value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                        if (bplib.clsWebLib.IsNumeric(value))
                            replaced[foundText] = document.Replace(foundText, cnDgt(value, tempId), false, true);
                        else if (bplib.clsWebLib.IsDateOK(value))
                            replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, tempId), false, true);
                        else
                            replaced[foundText] = document.Replace(foundText, value, false, true);
                    }

                }

                //document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), lang["Language"].ToString()), false, true);
                WSection section = document.Sections[0];
                //WTable wTable = (WTable)section.Body.Tables[0];

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeePic"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["EmployeePic"].ToString();
                    string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 120, 120);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[0].Rows[1].Cells[3].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["AuthorizedSignature"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["AuthorizedSignature"].ToString();
                    string picpath = ResourcesPathReader.GetAuthorizedSignaturePath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[3].Rows[0].Cells[1].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }

                }

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["CardHolderSignature"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["CardHolderSignature"].ToString();
                    string picpath = ResourcesPathReader.GetCardHolderSignaturePath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[3].Rows[0].Cells[0].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeFingerPrint"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["EmployeeFingerPrint"].ToString();
                    string picpath = ResourcesPathReader.GetEmployeeFingerPrintForSBPath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[2].Rows[8].Cells[2].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }


                }

                WTable table1 = (WTable)section.Body.Tables[5];

                //TextSelection allresult1 = table1.Find(new Regex("{.*?}"));
                WTableRow copiedRow = table1.Rows[4].Clone();

                var salarydistinctIds = dtSalary.AsEnumerable()
                   .Select(s => new
                   {
                       id = s.Field<string>("SystemId"),
                   })
                   .Distinct().ToList();

                int index = 0;
                foreach (var item in salarydistinctIds)
                {
                    dtSalary.DefaultView.RowFilter = "SystemId='" + item.id + "'";
                    DataView dvr = new DataView(dtSalary.DefaultView.ToTable());

                    double totalOthers = 0;
                   // double gross = 0;

                    WTableRow row;
                    //if (index == 0)
                    //    row = table1.AddRow();
                    //else
                    //{
                    if (index > 0)
                    {
                        row = copiedRow.Clone();
                        table1.Rows.Add(row);
                    }

                    index++;
                    for (int ROW = 0; ROW < dvr.Count; ROW++)
                    {
                        int isReplaced = 0;

                        isReplaced = table1.Replace("{" + dvr[ROW]["SalaryHead"].ToString() + "}", cnDgt(dvr[ROW]["EntryAmount"].ToString(), tempId), false, true);
                        if (isReplaced == 0 && dvr[ROW]["SalaryHead"].ToString().ToUpper() != ("Gross").ToUpper() && dvr[ROW]["HeadType"].ToString() == "E")
                        {
                            totalOthers += Convert.ToDouble(dvr[ROW]["EntryAmount"].ToString());

                        }

                        table1.Replace("{DesignationName}", dvr[ROW]["DesignationName"].ToString(), false, true);
                        table1.Replace("{EffectiveDate}", GetFormatedDate(dvr[ROW]["EffectiveDate"].ToString(), tempId), false, true);

                    }
                    table1.Replace("{Others}", cnDgt(totalOthers.ToString(), tempId), false, true);

                }

                #region Disciplinary 

                WTable table2 = (WTable)section.Body.Tables[7];
                WTableRow copiedRow2 = table2.Rows[1].Clone();

                WTableRow row2;

                for (int ROW = 0; ROW < dtDisciplinaryAction.Rows.Count; ROW++)
                {
                    if (ROW > 0)
                    {
                        row2 = copiedRow2.Clone();
                        table2.Rows.Add(row2);
                    }

                    table2.Replace("{EntryDate}", GetFormatedDate(dtDisciplinaryAction.Rows[ROW]["EntryDate"].ToString(), tempId), false, true);

                    table2.Replace("{Description}", dtDisciplinaryAction.Rows[ROW]["Description"].ToString(), false, true);

                }

                #endregion

                #region LeaveInformation

                WTable table3 = (WTable)section.Body.Tables[6];
                WTableRow copiedRow3 = table3.Rows[2].Clone();
                WTableRow row3;

                for (int i = 0; i < dtClanderYear.Rows.Count; i++)
                {
                    DataTable dtloadLeaveTransactions = loadLeaveTransactions(empId, dtClanderYear.Rows[i]["FromDate"].ToString(), dtClanderYear.Rows[i]["ToDate"].ToString());
                    DataTable dtLoadLeave = loadBf(empId, dtClanderYear.Rows[i]["Id"].ToString());

                    for (int ROW = 0; ROW < dtloadLeaveTransactions.Rows.Count; ROW++)
                    {

                        if (ROW > 0)
                        {
                            row3 = copiedRow3.Clone();
                            table3.Rows.Add(row3);
                        }

                        table3.Replace("{FromDate}", GetFormatedDate(dtloadLeaveTransactions.Rows[ROW]["FromDate"].ToString(), tempId), false, true);

                        table3.Replace("{ToDate}", GetFormatedDate(dtloadLeaveTransactions.Rows[ROW]["ToDate"].ToString(), tempId), false, true);

                        table3.Replace("{LeaveDays}", cnDgt(dtloadLeaveTransactions.Rows[ROW]["LeaveDays"].ToString(), tempId), false, true);

                        table3.Replace("{BroughtForward}", cnDgt(dtLoadLeave.Rows[ROW]["BroughtForward"].ToString(), tempId), false, true);

                    }
                }


                //for (int ROW = 0; ROW < dtloadLeaveTransactions.Rows.Count; ROW++)
                //{
                //    if (ROW > 0)
                //    {
                //        row3 = copiedRow3.Clone();
                //        table3.Rows.Add(row3);
                //    }

                //    table3.Replace("{FromDate}", GetFormatedDate(dtloadLeaveTransactions.Rows[ROW]["FromDate"].ToString(), tempId), false, true);

                //    table3.Replace("{ToDate}", GetFormatedDate(dtloadLeaveTransactions.Rows[ROW]["ToDate"].ToString(), tempId), false, true);

                //    table3.Replace("{LeaveDays}", cnDgt(dtloadLeaveTransactions.Rows[ROW]["LeaveDays"].ToString(), tempId), false, true);

                //    table3.Replace("{BroughtForward}", cnDgt(dtLoadLeave.Rows[ROW]["BroughtForward"].ToString(), tempId), false, true);

                //}

                //for (int ROW = 0; ROW < dtLoadLeave.Rows.Count; ROW++)
                //{
                //    if (ROW > 0)
                //    {
                //        row3 = copiedRow3.Clone();
                //        table3.Rows.Add(row3);
                //    }
                //    table3.Replace("{BroughtForward}", cnDgt(dtLoadLeave.Rows[ROW]["BroughtForward"].ToString(), tempId), false, true);

                //}

                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);

                }

                #endregion

                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-ServiceBook.docx";

                }
                else
                {
                    fileNames = "-ServiceBook.docx";
                }

                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private void xCreateServiceBookInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = 7;
                string reportTypeName = "";

                if (reportType == LetterType.ServiceBook.ToString())
                {
                    reportTypeName = LetterType.ServiceBook.GetDescription();
                }

                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Srv" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }
                //------

                ///-----
                var Templatefile = GetFilePath(plantId, tempId, reportType);
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

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "Srv" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

                bool IsDefLan = false;
                var tokens = (fileName.Substring(("Srv" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }

                ///

                DataTable dtEmp = GetEmployeeBasicInfoById(empId, plantId, empType, langID, tempId);
                DataTable dtSalary = SalaryDetailsForSB(empId, langID); // GetGrossAmount(empId);
                DataTable dtDisciplinaryAction = EmployeeDisciplinaryAction(empId, langID); // GetGrossAmount(empId);

                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                foreach (TextSelection item in allresult)
                {
                    string foundText = item.SelectedText;

                    if (replaced.ContainsKey(foundText) == false)
                        replaced.Add(foundText, 0);

                    //for fixed info
                    string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                    if (dtEmp.Columns.Contains(colName))
                    {

                        ////===== def lan 
                        if (IsDefLan == true)
                        {
                            if (IsDefLan == true)
                            {
                                colName = GetBasicInfoInDefaultLng(colName);
                            }
                        }
                        ///=====
                        value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                        if (bplib.clsWebLib.IsNumeric(value))
                            replaced[foundText] = document.Replace(foundText, cnDgt(value, tempId), false, true);
                        else if (bplib.clsWebLib.IsDateOK(value))
                            replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, tempId), false, true);
                        else
                            replaced[foundText] = document.Replace(foundText, value, false, true);
                    }

                }

                //document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), lang["Language"].ToString()), false, true);
                WSection section = document.Sections[0];
                //WTable wTable = (WTable)section.Body.Tables[0];

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeePic"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["EmployeePic"].ToString();
                    string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 120, 120);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[0].Rows[1].Cells[3].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["AuthorizedSignature"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["AuthorizedSignature"].ToString();
                    string picpath = ResourcesPathReader.GetAuthorizedSignaturePath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[3].Rows[0].Cells[1].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }


                }

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["CardHolderSignature"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["CardHolderSignature"].ToString();
                    string picpath = ResourcesPathReader.GetCardHolderSignaturePath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[3].Rows[0].Cells[0].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }


                }

                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeFingerPrint"].ToString()))
                {
                    var pic = dtEmp.Rows[0]["EmployeeFingerPrint"].ToString();
                    string picpath = ResourcesPathReader.GetEmployeeFingerPrintForSBPath() + pic;
                    //WPicture ImgwPicture = new WPicture(document);
                    if (System.IO.File.Exists(picpath))
                    {
                        try
                        {
                            Image Img = Image.FromFile(picpath);
                            Image newImage = resizeImage(Img, 60, 100);
                            //wPicture.LoadImage(Image.FromFile(picpath));
                            //TextBodyPart textBodyPart = new TextBodyPart(document);

                            section.Tables[2].Rows[8].Cells[2].Paragraphs[0].AppendPicture(newImage);

                            //document.Replace()
                            //document.Replace("{emppic}", textBodyPart, true, true);
                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                    }


                }

                WTable table1 = (WTable)section.Body.Tables[5];

                //TextSelection allresult1 = table1.Find(new Regex("{.*?}"));
                WTableRow copiedRow = table1.Rows[4].Clone();

                var salarydistinctIds = dtSalary.AsEnumerable()
                   .Select(s => new
                   {
                       id = s.Field<string>("SystemId"),
                   })
                   .Distinct().ToList();

                int index = 0;
                foreach (var item in salarydistinctIds)
                {
                    dtSalary.DefaultView.RowFilter = "SystemId='" + item.id + "'";
                    DataView dvr = new DataView(dtSalary.DefaultView.ToTable());




                    double totalOthers = 0;
                    double gross = 0;

                    WTableRow row;
                    //if (index == 0)
                    //    row = table1.AddRow();
                    //else
                    //{
                    if (index > 0)
                    {
                        row = copiedRow.Clone();
                        table1.Rows.Add(row);
                    }


                    index++;
                    for (int ROW = 0; ROW < dvr.Count; ROW++)
                    {
                        int isReplaced = 0;

                        isReplaced = table1.Replace("{" + dvr[ROW]["SalaryHead"].ToString() + "}", cnDgt(dvr[ROW]["EntryAmount"].ToString(), tempId), false, true);
                        if (isReplaced == 0 && dvr[ROW]["SalaryHead"].ToString().ToUpper() != ("Gross").ToUpper() && dvr[ROW]["HeadType"].ToString() == "E")
                        {
                            totalOthers += Convert.ToDouble(dvr[ROW]["EntryAmount"].ToString());

                        }

                        table1.Replace("{DesignationName}", dvr[ROW]["DesignationName"].ToString(), false, true);
                        table1.Replace("{EffectiveDate}", GetFormatedDate(dvr[ROW]["EffectiveDate"].ToString(), tempId), false, true);

                    }
                    table1.Replace("{Others}", cnDgt(totalOthers.ToString(), tempId), false, true);

                }
                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);

                }

                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-ServiceBook.docx";

                }
                else
                {
                    fileNames = "-ServiceBook.docx";
                }

                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private void CreateNomineeInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = 7;
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Nom" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }

                }
                //------

                ///-----
                var Templatefile = GetFilePath(plantId, tempId, reportType);
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

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "Nom" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

                bool IsDefLan = false;

                var tokens = (fileName.Substring(("Nom" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }


                DataTable dtEmp = GetEmployeeBasicInfoById(empId, plantId, empType, langID, tempId);



                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                foreach (TextSelection item in allresult)
                {
                    string foundText = item.SelectedText;

                    if (replaced.ContainsKey(foundText) == false)
                        replaced.Add(foundText, 0);

                    //for fixed info
                    string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                    if (dtEmp.Columns.Contains(colName))
                    {
                        ////===== def lan 
                        if (IsDefLan == true)
                        {
                            if (IsDefLan == true)
                            {
                                colName = GetBasicInfoInDefaultLng(colName);
                            }
                        }
                        ///=====
                        value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                        if (bplib.clsWebLib.IsNumeric(value))
                            replaced[foundText] = document.Replace(foundText, cnDgt(value, tempId), false, true);
                        else if (bplib.clsWebLib.IsDateOK(value))
                            replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, tempId), false, true);
                        else
                            replaced[foundText] = document.Replace(foundText, value, false, true);
                    }

                }

                document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), tempId), false, true);
                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);
                }


                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-Nominee.docx";

                }
                else
                {
                    fileNames = "-Nominee.docx";
                }

                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void CreateJoiningLetterInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = 7;
                //string reportTypeName = "";

                //if (reportType == LetterType.JoiningLetter.ToString())
                //{
                //    reportTypeName = LetterType.JoiningLetter.GetDescription();
                //}
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Joi" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }

                }
                //------

                ///-----
                var Templatefile = GetFilePath(plantId, tempId, reportType);
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

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "Joi" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }
                bool IsDefLan = false;

                var tokens = (fileName.Substring(("Ack" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }

                ///

                DataTable dtEmp = GetEmployeeBasicInfoById(empId, plantId, empType, langID, tempId);



                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] X = document.FindAll(new Regex("{.*?}")).ToArray();
                List<string> allresult = new List<string>();
                for (int i = 0; i < X.Length; i++)
                    allresult.Add(X[i].SelectedText);


                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                for (int i = 0; i < allresult.Count; i++)
                {
                    try
                    {
                        string foundText = allresult[i];

                        if (replaced.ContainsKey(foundText) == false)
                            replaced.Add(foundText, 0);

                        //for fixed info
                        string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                        if (dtEmp.Columns.Contains(colName))
                        {
                            ////===== def lan 
                            if (IsDefLan == true)
                            {
                                colName = GetBasicInfoInDefaultLng(colName);
                            }
                            ///=====
                            value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                            if (bplib.clsWebLib.IsNumeric(value))
                                replaced[foundText] = document.Replace(foundText, cnDgt(value, tempId), false, true);
                            else if (bplib.clsWebLib.IsDateOK(value))
                                replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, tempId), false, true);
                            else
                                replaced[foundText] = document.Replace(foundText, value, false, false);
                        }
                    }
                    catch (Exception)
                    {


                    }



                }

                document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), tempId), false, true);
                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);
                }


                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-JoiningLetter.docx";

                }
                else
                {
                    fileNames = "-JoiningLetter.docx";
                }

                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void CreateAcknowledgementInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = 7;
                bool IsDefLan = false;
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Ack" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }

                }
                //------


                ///-----
                var Templatefile = GetFilePath(plantId, tempId, reportType);
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

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "Ack" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }
                ///

                var tokens = (fileName.Substring(("Ack" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }

                ///
                DataTable dtEmp = GetEmployeeBasicInfoById(empId, plantId, empType, langID, tempId);



                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                //TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));


                TextSelection[] X = document.FindAll(new Regex("{.*?}")).ToArray();
                List<string> allresult = new List<string>();
                for (int i = 0; i < X.Length; i++)
                    allresult.Add(X[i].SelectedText);


                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                for (int i = 0; i < allresult.Count; i++)
                {
                    try
                    {
                        string foundText = allresult[i];

                        if (replaced.ContainsKey(foundText) == false)
                            replaced.Add(foundText, 0);

                        //for fixed info
                        string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                        if (dtEmp.Columns.Contains(colName))
                        {
                            ////===== def lan 
                            if (IsDefLan == true)
                            {
                                if (IsDefLan == true)
                                {
                                    colName = GetBasicInfoInDefaultLng(colName);
                                }
                            }
                            ///=====
                            value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                            if (bplib.clsWebLib.IsNumeric(value))
                                replaced[foundText] = document.Replace(foundText, cnDgt(value, tempId), false, true);
                            else if (bplib.clsWebLib.IsDateOK(value))
                                replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, tempId), false, true);
                            else
                                replaced[foundText] = document.Replace(foundText, value, false, false);
                        }
                    }
                    catch (Exception)
                    {


                    }



                }

                document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), tempId), false, true);
                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);
                }


                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-Acknowledgement.docx";

                }
                else
                {
                    fileNames = "-Acknowledgement.docx";
                }

                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IWorkbook PrintEmployeeIDCard(string empId, string companyGroupId, string companyId, string plantId, string tempId, string empType, string reportType)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];
                workbook = CreateIDCardSheet(ref sheet1, report, "IDCARD", "IDCARD", empId, companyGroupId, companyId, plantId, tempId, empType, reportType);
                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
               Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
               ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private void IterateTextBody(WTextBody textBody, DataTable dt1, DataTable dt2)
        {
            for (int i = 0; i < textBody.ChildEntities.Count; i++)
            {
                IEntity bodyItemEntity = textBody.ChildEntities[i];
                changeFixedColumns(bodyItemEntity, dt1);
                changeSalaryHeads(bodyItemEntity, dt2);
            }

        }

        private void changeFixedColumns(IEntity bodyItemEntity, DataTable dt)
        {
            string key = ""; string value = "";
            for (int COL = 0; COL < dt.Columns.Count; COL++)
            {
                key = "{" + dt.Columns[COL].ColumnName + "}";
                value = dt.Rows[0][dt.Columns[COL].ColumnName].ToString();
                if (((Syncfusion.DocIO.DLS.WParagraph)bodyItemEntity).Text.Contains(key))
                    ((Syncfusion.DocIO.DLS.WParagraph)bodyItemEntity).Text = ((Syncfusion.DocIO.DLS.WParagraph)bodyItemEntity).Text.Replace(key, value);

            }
        }

        private void changeSalaryHeads(IEntity bodyItemEntity, DataTable dt)
        {
            string key = ""; string value = "";
            for (int ROW = 0; ROW < dt.Rows.Count; ROW++)
            {
                key = "{" + dt.Rows[ROW]["SalaryHead"].ToString() + "}";
                value = dt.Rows[ROW]["EntryAmount"].ToString();
                if (((Syncfusion.DocIO.DLS.WParagraph)bodyItemEntity).Text.Contains(key))
                    ((Syncfusion.DocIO.DLS.WParagraph)bodyItemEntity).Text = ((Syncfusion.DocIO.DLS.WParagraph)bodyItemEntity).Text.Replace(key, value);

            }
        }

        private IWorkbook CreateSheetMain_backup(ref IWorksheet sheet1, ReportUtility report, string sheetHeader, string sheetName, string companyGroupId, string companyId, string plantId, string empId, string empType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                var reportType = "";
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "App" + plantId + tempId + ".xls";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }

                //if (lang.Count > 0)
                //{
                //    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                //    langID = dtLangId.Rows[0]["Id"].ToString();
                //}
                //else
                //{
                //    langID = tempId;
                //}

                var dtEmp = GetEmployeeById(empId, plantId, empType, langID, tempId);
                var dtSalary = SalaryDetails(empId); // GetGrossAmount(empId);


                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook1 = null;

                var Templatefile = GetFilePath(plantId, tempId, reportType);
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
                    var newdate = GetFormatedDate(DateTime.Now.ToString("dd-MMM-yyyy"), tempId);

                    workbook1.Worksheets[0].Replace("{Date}", newdate);
                    workbook1.Worksheets[0].Replace("{CompanyName}", dtEmp.Rows[0]["CompanyName"].ToString());
                    workbook1.Worksheets[0].Replace("{Address}", dtEmp.Rows[0]["CompanyAddress"].ToString());
                    workbook1.Worksheets[0].Replace("{EmployeeName}", dtEmp.Rows[0]["EmployeeName"].ToString());
                    workbook1.Worksheets[0].Replace("{FatherName}", dtEmp.Rows[0]["FatherName"].ToString());
                    workbook1.Worksheets[0].Replace("{MotherName}", dtEmp.Rows[0]["MotherName"].ToString());
                    workbook1.Worksheets[0].Replace("{PresentAddress}", dtEmp.Rows[0]["PresentAddress1"].ToString());
                    workbook1.Worksheets[0].Replace("{PermanentAddress}", dtEmp.Rows[0]["ParmanentAddress1"].ToString());
                    workbook1.Worksheets[0].Replace("{CITY}", dtEmp.Rows[0]["PresentCity"].ToString());
                    workbook1.Worksheets[0].Replace("{COUNTRY}", dtEmp.Rows[0]["LPresentCountry"].ToString());
                    workbook1.Worksheets[0].Replace("{FIRSTNAME}", dtEmp.Rows[0]["FirstName"].ToString());
                    workbook1.Worksheets[0].Replace("{Designation}", dtEmp.Rows[0]["DesignationName"].ToString());
                    var doj = GetFormatedDate(dtEmp.Rows[0]["DateOfJoin"].ToString(), tempId);
                    workbook1.Worksheets[0].Replace("{DOJ}", doj);
                    workbook1.Worksheets[0].Replace("{ProbationPeriod}", dtEmp.Rows[0]["confirm"].ToString());
                    workbook1.Worksheets[0].Replace("{Department}", dtEmp.Rows[0]["Department"].ToString());
                    workbook1.Worksheets[0].Replace("{Section}", dtEmp.Rows[0]["Section"].ToString());
                    workbook1.Worksheets[0].Replace("{Unit}", dtEmp.Rows[0]["Unit"].ToString());
                    workbook1.Worksheets[0].Replace("{MedicalAllowance}", "0");
                    workbook1.Worksheets[0].Replace("{FoodAllowance}", "0");


                    if (dtSalary.Rows.Count > 0)
                    {
                        double _totalAmount = 0;
                        for (int i = 0; i < dtSalary.Rows.Count; i++)
                        {

                            if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Basic")
                            {
                                workbook1.Worksheets[0].Replace("{BasicSalary}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                            }
                            if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Conveyance Allowance")
                            {
                                workbook1.Worksheets[0].Replace("{Conveyance}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                            }

                            if (dtSalary.Rows[i]["SalaryHead"].ToString() != null)
                            {
                                if (dtSalary.Rows[i]["SalaryHead"].ToString() == "House Rent")
                                {
                                    workbook1.Worksheets[0].Replace("{HRA}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                    _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                                }
                            }
                            if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Other")
                            {
                                workbook1.Worksheets[0].Replace("{Others}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                            }


                        }//loop
                         //if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Gross")
                         //{
                        workbook1.Worksheets[0].Replace("{Gross}", _totalAmount.ToString());
                        //}


                    }


                    workbook1.Version = ExcelVersion.Excel97to2003;

                }
                else
                {
                    File = "App" + plantId + "English.xls";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                    workbook1 = excelEngine.Excel.Workbooks.Open(strPath);
                    string cn = dtEmp.Rows[0]["CompanyName"].ToString();
                    workbook1.Worksheets[0].Replace("{CompanyName}", cn);
                    workbook1.Worksheets[0].Replace("{Address}", dtEmp.Rows[0]["UtilityName"].ToString());
                    workbook1.Worksheets[0].Replace("{EmployeeName}", dtEmp.Rows[0]["EmployeeName"].ToString());
                    workbook1.Worksheets[0].Replace("{FatherName}", dtEmp.Rows[0]["FatherName"].ToString());
                    workbook1.Worksheets[0].Replace("{MotherName}", dtEmp.Rows[0]["MotherName"].ToString());
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
                    workbook1.Worksheets[0].Replace("{CITY}", dtEmp.Rows[0]["PresentCity"].ToString());
                    workbook1.Worksheets[0].Replace("{COUNTRY}", dtEmp.Rows[0]["LPresentCountry"].ToString());
                    workbook1.Worksheets[0].Replace("{FIRSTNAME}", dtEmp.Rows[0]["FirstName"].ToString());
                    workbook1.Worksheets[0].Replace("{Designation}", dtEmp.Rows[0]["DesignationName"].ToString());
                    workbook1.Worksheets[0].Replace("{DOJ}", dtEmp.Rows[0]["DOJ"].ToString());
                    workbook1.Worksheets[0].Replace("{ProbationPeriod}", dtEmp.Rows[0]["confirm"].ToString());
                    workbook1.Worksheets[0].Replace("{MedicalAllowance}", "0");
                    workbook1.Worksheets[0].Replace("{FoodAllowance}", "0");
                    if (dtSalary.Rows.Count > 0)
                    {
                        double _totalAmount = 0;
                        for (int i = 0; i < dtSalary.Rows.Count; i++)
                        {

                            if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Basic")
                            {
                                workbook1.Worksheets[0].Replace("{BasicSalary}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                            }
                            if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Conveyance Allowance")
                            {
                                workbook1.Worksheets[0].Replace("{Conveyance}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                            }

                            if (dtSalary.Rows[i]["SalaryHead"].ToString() != null)
                            {
                                if (dtSalary.Rows[i]["SalaryHead"].ToString() == "House Rent")
                                {
                                    workbook1.Worksheets[0].Replace("{HRA}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                    _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                                }
                            }
                            if (dtSalary.Rows[i]["SalaryHead"].ToString() == "Other")
                            {
                                workbook1.Worksheets[0].Replace("{Others}", dtSalary.Rows[i]["EntryAmount"].ToString());
                                _totalAmount += Convert.ToDouble(dtSalary.Rows[i]["EntryAmount"].ToString());
                            }
                        }
                        workbook1.Worksheets[0].Replace("{Gross}", _totalAmount.ToString());
                    }
                    workbook1.Worksheets[0].Replace("{Date}", DateTime.Now.ToString("dd-MM-yyyy"));


                }
                return workbook1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void CreateIDCardInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)//, string templatePathHindi, string templatePathEnglish, string templatePathBangla)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "App" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }

                var Templatefile = GetFilePath(plantId, tempId, reportType);
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

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "App" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(filepath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

                bool IsDefLan = false;

                var tokens = (fileName.Substring(("App" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }

                ///

                DataTable dtEmp = GetEmployeeById(empId, plantId, empType, langID, tempId);
                DataTable dtSalary = SalaryDetailsForApp(empId, langID); // GetGrossAmount(empId);


                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);



                TextSelection[] X = document.FindAll(new Regex("{.*?}")).ToArray();
                List<string> allresult = new List<string>();
                for (int i = 0; i < X.Length; i++)
                    allresult.Add(X[i].SelectedText);


                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                for (int i = 0; i < allresult.Count; i++)
                {
                    try
                    {
                        string foundText = allresult[i];

                        if (replaced.ContainsKey(foundText) == false)
                            replaced.Add(foundText, 0);

                        //for fixed info
                        string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                        if (dtEmp.Columns.Contains(colName))
                        {
                            ////===== def lan 
                            if (IsDefLan == true)
                            {
                                colName = GetBasicInfoInDefaultLng(colName);
                            }
                            ///=====
                            value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                            if (bplib.clsWebLib.IsNumeric(value))
                                replaced[foundText] = document.Replace(foundText, cnDgt(value, tempId), false, false);
                            else if (bplib.clsWebLib.IsDateOK(value))
                                replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, tempId), false, false);
                            else
                                replaced[foundText] = document.Replace(foundText, value, false, false);
                        }


                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }



                try
                {


                    WSection section = document.Sections[0];
                    WTable table1 = (WTable)section.Body.Tables[0];

                    //TextSelection allresult1 = table1.Find(new Regex("{.*?}"));
                    WTableRow copiedRow = table1.Rows[4].Clone();

                    double totalOthers = 0;

                    for (int ROW = 0; ROW < dtSalary.Rows.Count; ROW++)
                    {
                        int isReplaced = 0;

                        isReplaced = table1.Replace("{" + dtSalary.Rows[ROW]["SalaryHead"].ToString() + "}", cnDgt(dtSalary.Rows[ROW]["EntryAmount"].ToString(), tempId), false, false);
                        if (isReplaced == 0 && dtSalary.Rows[ROW]["SalaryHead"].ToString().ToUpper() != ("Gross").ToUpper() && dtSalary.Rows[ROW]["HeadType"].ToString() == "E")
                        {
                            totalOthers += Convert.ToDouble(dtSalary.Rows[ROW]["EntryAmount"].ToString());

                        }

                        //table1.Replace("{DesignationName}", dtSalary.Rows[ROW]["DesignationName"].ToString(), false, true);
                        //table1.Replace("{EffectiveDate}", GetFormatedDate(dtSalary.Rows[ROW]["EffectiveDate"].ToString(), tempId), false, true);

                    }
                    table1.Replace("{Others}", cnDgt(totalOthers.ToString(), tempId), false, false);

                    //}
                }
                catch (Exception)
                {
                    for (int ROW = 0; ROW < dtSalary.Rows.Count; ROW++)
                    {
                        int isReplaced = 0;

                        isReplaced = document.Replace("{" + dtSalary.Rows[ROW]["SalaryHead"].ToString() + "}", cnDgt(dtSalary.Rows[ROW]["EntryAmount"].ToString(), tempId), false, false);

                    }


                }



                document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), tempId), false, true);


                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);

                }

                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-appointment-letter.docx";

                }
                else
                {
                    fileNames = "-appointment-letter.docx";
                }
                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string GetBasicInfoInDefaultLng(string colName)
        {
            if (colName == "EmployeeName")
            {
                colName = "EmployeeNameEng";
            }
            if (colName == "MotherName")
            {
                colName = "MotherNameEng";
            }
            if (colName == "MotherName")
            {
                colName = "MotherNameEng";
            }
            return colName;
        }
        public string isBasicInfoInDefaultLng(string colName)
        {
            if (colName == "EmployeeName")
            {
                colName = "EmployeeNameEng";
            }
            if (colName == "MotherName")
            {
                colName = "MotherNameEng";
            }
            if (colName == "MotherName")
            {
                colName = "MotherNameEng";
            }
            return colName;
        }

        public Image resizeImage(Image image, int new_height, int new_width)
        {
            Bitmap new_image = new Bitmap(new_width, new_height);
            Graphics g = Graphics.FromImage((Image)new_image);
            g.InterpolationMode = InterpolationMode.High;
            g.DrawImage(image, 0, 0, new_width, new_height);
            return new_image;
        }

        private IWorkbook CreateIDCardSheet(ref IWorksheet sheet1, ReportUtility report, string sheetHeader, string sheetName, string empId, string companyGroupId, string companyId, string plantId, string tempId, string empType, string reportType)

        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string langName = "";
                string strPath = "";
                var fileName = "";
                //var reportType = "";
                // var dtLangName = "";
                var lang = GetLanguage(plantId, tempId, reportType);
                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    langName = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    langName = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "IdCard" + plantId + langName + ".xlsx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }

                var dtEmp = GetEmployeeDataById(empId, plantId, empType, langID, tempId);

                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook1 = null;
                IWorksheet sheet = null;

                var Templatefile = GetFilePath(plantId, langName, reportType);
                if (Templatefile.Count > 0)
                {
                    fileName = Templatefile["TemplateFileName"].ToString();
                }
                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }
                bool IsDefLan = false;

                var tokens = (fileName.Substring(("IdCard" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }


                if (System.IO.File.Exists(strPath) && langName != "English")
                {
                    workbook1 = excelEngine.Excel.Workbooks.Open(strPath, ExcelOpenType.Automatic, ExcelVersion.Excel2013);

                    sheet = workbook1.Worksheets[0];

                    int COL = 9;
                    int ROW = 1;

                    sheet.HideColumn(COL);

                    FormatTextBox(ref sheet, "BloodGroup", dtEmp.Rows[0]["BloodGroup"].ToString(), 8, ExcelKnownColors.Red);
                    FormatTextBox(ref sheet, "PermanentAddress", dtEmp.Rows[0]["ParmanentAddress1L"].ToString(), 6, ExcelKnownColors.Black);
                    FormatTextBox(ref sheet, "PhoneNumber", cnDgt(dtEmp.Rows[0]["MobileNo"].ToString(), langName), 8, ExcelKnownColors.Black);
                    FormatTextBox(ref sheet, "NID", cnDgt(dtEmp.Rows[0]["NationalID"].ToString(), langName), 8, ExcelKnownColors.Black);


                    ////===== def lan 
                    if (IsDefLan == true)
                    {
                        FormatTextBox(ref sheet, "Name", dtEmp.Rows[0]["EmployeeName"].ToString(), 8, ExcelKnownColors.Black);
                    }
                    else
                    {
                        FormatTextBox(ref sheet, "Name", dtEmp.Rows[0]["EmployeeNameL"].ToString(), 8, ExcelKnownColors.Black);
                    }
                    ///=====
                    FormatTextBox(ref sheet, "DESIG", dtEmp.Rows[0]["DesignationName"].ToString(), 8, ExcelKnownColors.Black);

                    FormatTextBox(ref sheet, "ID", cnDgt(dtEmp.Rows[0]["EmployeeCode"].ToString(), langName), 8, ExcelKnownColors.Black);
                    FormatTextBox(ref sheet, "Department", dtEmp.Rows[0]["Section"].ToString(), 8, ExcelKnownColors.Black);
                    FormatTextBox(ref sheet, "WorkType", dtEmp.Rows[0]["EmploymentType"].ToString(), 8, ExcelKnownColors.Black);

                    var doj = GetFormatedDate(dtEmp.Rows[0]["DateOfJoin"].ToString(), langName);
                    FormatTextBox(ref sheet, "DOJ", doj, 8, ExcelKnownColors.Black);
                    var issudate = GetFormatedDate(DateTime.Now.ToString("dd-MMM-yyyy"), langName);
                    FormatTextBox(ref sheet, "IssueDate", issudate, 8, ExcelKnownColors.Black);

                    int x = sheet.Pictures.Count;
                    var pic = dtEmp.Rows[0]["EmployeePic"].ToString();
                    IPictureShape oldImage = sheet.Pictures["EmpPicture"];
                    int leftPosition = oldImage.Left;
                    int topPosition = oldImage.Top;
                    int height = oldImage.Height;
                    int width = oldImage.Width;
                    oldImage.Remove();
                    string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                    string ImagefileLocation = picpath;

                    //IPictureShape newImage = sheet.Pictures.AddPicture(ImagefileLocation);
                    //newImage.Left = leftPosition;
                    //newImage.Top = topPosition;
                    //newImage.Height = height;
                    //newImage.Width = width;

                    if (System.IO.File.Exists(ImagefileLocation))
                    {
                        IPictureShape newImage = sheet.Pictures.AddPicture(ImagefileLocation);
                        newImage.Left = leftPosition;
                        newImage.Top = topPosition;
                        newImage.Height = height;
                        newImage.Width = width;
                    }

                    workbook1.Version = ExcelVersion.Excel2013;

                }
                else
                {
                    File = "IdCard" + plantId + "English.xlsx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }

                    workbook1 = excelEngine.Excel.Workbooks.Open(strPath, ExcelOpenType.Automatic, ExcelVersion.Excel2013);

                    sheet = workbook1.Worksheets[0];

                    int COL = 9;
                    int ROW = 1;

                    sheet.HideColumn(COL);

                    FormatTextBox(ref sheet, "BloodGroup", dtEmp.Rows[0]["BloodGroup"].ToString(), 8, ExcelKnownColors.Red);
                    FormatTextBox(ref sheet, "PermanentAddress", dtEmp.Rows[0]["ParmanentAddress1"].ToString(), 8, ExcelKnownColors.Black);
                    FormatTextBox(ref sheet, "PhoneNumber", dtEmp.Rows[0]["MobileNo"].ToString(), 8, ExcelKnownColors.Black);
                    FormatTextBox(ref sheet, "NID", dtEmp.Rows[0]["NationalID"].ToString(), 8, ExcelKnownColors.Black);
                    FormatTextBox(ref sheet, "Name", dtEmp.Rows[0]["EmployeeName"].ToString(), 8, ExcelKnownColors.Black);
                    FormatTextBox(ref sheet, "DESIG", dtEmp.Rows[0]["DesignationName"].ToString(), 8, ExcelKnownColors.Black);

                    FormatTextBox(ref sheet, "ID", dtEmp.Rows[0]["EmployeeCode"].ToString(), 8, ExcelKnownColors.Black);
                    FormatTextBox(ref sheet, "Department", dtEmp.Rows[0]["Section"].ToString(), 8, ExcelKnownColors.Black);
                    FormatTextBox(ref sheet, "WorkType", dtEmp.Rows[0]["EmploymentType"].ToString(), 8, ExcelKnownColors.Black);
                    FormatTextBox(ref sheet, "DOJ", dtEmp.Rows[0]["DateOfJoin"].ToString(), 8, ExcelKnownColors.Black);
                    FormatTextBox(ref sheet, "IssueDate", DateTime.Now.ToString("dd-MMM-yyyy"), 8, ExcelKnownColors.Black);

                    int x = sheet.Pictures.Count;
                    var pic = dtEmp.Rows[0]["EmployeePic"].ToString();
                    IPictureShape oldImage = sheet.Pictures["EmpPicture"];
                    int leftPosition = oldImage.Left;
                    int topPosition = oldImage.Top;
                    int height = oldImage.Height;
                    int width = oldImage.Width;
                    oldImage.Remove();
                    string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                    string ImagefileLocation = picpath;



                    if (System.IO.File.Exists(ImagefileLocation))
                    {
                        IPictureShape newImage = sheet.Pictures.AddPicture(ImagefileLocation);
                        newImage.Left = leftPosition;
                        newImage.Top = topPosition;
                        newImage.Height = height;
                        newImage.Width = width;
                    }


                    workbook1.Version = ExcelVersion.Excel2013;
                }

                return workbook1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void FormatTextBox(ref IWorksheet sheet, string TextBoxName, string Text, float FontSize, ExcelKnownColors FontColor)
        {
            Text = Text == "" ? " " : Text;

            ITextBoxShape textbox = sheet.TextBoxes[TextBoxName];
            textbox.Text = Text;
            IRichTextString rtf = textbox.RichText;
            IFont font = sheet.Workbook.CreateFont();
            font.Color = FontColor;
            font.Size = FontSize;
            rtf.SetFont(0, textbox.Text.Length - 1, font);

            textbox.RichText = rtf;
            textbox.Fill.ForeColor = Color.White;
            textbox.Fill.BackColor = Color.Gold;

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

        private DataTable getLanguageId(string username)
        {
            try
            {
                var sql = @"Select Id from SCS.Language where UserName ='" + username.Replace("\r\n", "").Trim() + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable getLanguageName(string Id)
        {
            try
            {
                var sql = @"Select UserName from SCS.Language where Id ='" + Id + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable SalaryDetails(string empId)
        {
            try
            {
                var sql = @"SELECT  SH.SalaryHead, convert(numeric(10,2), SD.EntryAmount) EntryAmount FROM SalaryInfoDefineMaster SM
                            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
                            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
                            WHERE SM.EmpInfoSystemID='" + empId + @"' AND SH.SalaryHead  in ('Basic','Conveyance Allowance','House Rent','Gross') 
                            union
                           SELECT 'Other' SalaryHead,ISNULL(convert(numeric(10,2),Sum(SD.EntryAmount)),0) as 'SalaryDetails' FROM SalaryInfoDefineMaster SM
                           LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
                           LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
                           WHERE SM.EmpInfoSystemID='" + empId + @"' AND SH.SalaryHead not in ('Basic','Conveyance Allowance','House Rent') 
                           AND SH.IsGrossComponent=1 AND SH.IsCTCComponent=0";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable SalaryDetailsForSB(string empId, string languageId)
        {
            try
            {
                var sql = @"SELECT SystemId, FORMAT(EffectiveDate,'dd-MMM-yyyy') EffectiveDate,ISNULL(ISNULL(LegalDesignationName,DesignationName ),GivenDesignationName)  DesignationName,SalaryHead, Convert(decimal(18,0),EntryAmount) EntryAmount,HeadType
                            FROM(
                            SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate
                            ,LocLangGD.Name GivenDesignationName
                            ,LocLangLD.Name LegalDesignationName
                            ,LD.UserName DesignationName---ISNULL(LocalDesignationName,DesignationName) DesignationName,
                            ,SH.SalaryHead,sh.HeadType
                            ,BSH.Name SalaryHeadBangla
                            ,convert(numeric(10,2), SD.EntryAmount) EntryAmount 
                            FROM SalaryInfoDefineMaster SM
                            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
                            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
                            LEFT JOIN EmployeeInformation ei ON EI.SystemId=SM.EmpInfoSystemID
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN hkp.Designation GVDE ON EI.GivenDesignationId = GVDE.Id
                            LEFT JOIN hkp.LegalDesignation LD ON EI.LegalDesignationId = LD.Id
                            LEFT JOIN hkp.LocalLanguage LocLangLD ON LocLangLD.LegalDesignationId = EI.LegalDesignationId and LocLangLD.LanguageId ='" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN hkp.LocalLanguage LocLangGD ON LocLangGD.DesignationId = EI.GivenDesignationId and LocLangGD.LanguageId = '" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=EI.GivenDesignationId AND B.LanguageId='" + languageId + @"'--PL.LanguageId
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL) AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID and BSH.LanguageId='" + languageId + @"'--PL.LanguageId
                            WHERE SM.EmpInfoSystemID='" + empId + @"' AND SM.IsApproved=1
                            UNION
                            SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate
                            ,LocLangGD.Name GivenDesignationName
                            ,LocLangLD.Name LegalDesignationName
                            ,LD.UserName DesignationName---ISNULL(LocalDesignationName,DesignationName) DesignationName,
                            ,SH.SalaryHead,sh.HeadType
                            ,BSH.Name SalaryHeadBangla
                            ,convert(numeric(10,2), SD.EntryAmount) EntryAmount 
                            FROM SalaryInfoBackMaster SM
                            LEFT JOIN SalaryInfoBack SD ON SD.SalaryID=SM.SystemID
                            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
                            LEFT JOIN EmployeeInformation ei ON EI.SystemId=SM.EmpInfoSystemID
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN hkp.Designation GVDE ON EI.GivenDesignationId = GVDE.Id
                            LEFT JOIN hkp.LegalDesignation LD ON EI.LegalDesignationId = LD.Id
                            LEFT JOIN hkp.LocalLanguage LocLangLD ON LocLangLD.LegalDesignationId = EI.LegalDesignationId and LocLangLD.LanguageId ='" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN hkp.LocalLanguage LocLangGD ON LocLangGD.DesignationId = EI.GivenDesignationId and LocLangGD.LanguageId = '" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=EI.GivenDesignationId AND B.LanguageId='" + languageId + @"'--PL.LanguageId 
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL) AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID and BSH.LanguageId='" + languageId + @"'--PL.LanguageId
                            WHERE SM.EmpInfoSystemID='" + empId + @"' ) x ORDER BY EffectiveDate DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable EmployeeDisciplinaryAction(string empId, string languageId)
        {
            try
            {
                var sql = @"select [Id]
                          ,[EmpSystemId]
                          ,[DisciplinaryActionCategoryId]
                          ,[Description]
                          ,FORMAT([EntryDate],'dd-MMM-yyyy') EntryDate
                          ,[AddedBy]
                          ,[AddedDate]
                          ,[AddedFromIP]
                          ,[UpdatedBy]
                          ,[UpdatedDate]
                          ,[UpdatedFromIP] from hkp.EmployeeDisciplinaryAction where EmpSystemId='" + empId + @"'";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable SalaryDetailsForApp(string empId, string languageId)
        {
            try
            {
                var sql = @"SELECT SystemId, FORMAT(EffectiveDate,'dd-MMM-yyyy') EffectiveDate,ISNULL(ISNULL(GivenDesignationName,LegalDesignationName),DesignationName)  DesignationName,SalaryHead, Convert(decimal(18,0),EntryAmount) EntryAmount,HeadType,Grade
                            FROM(
                            SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate
                            ,LocLangGD.Name GivenDesignationName
                            ,LocLangLD.Name LegalDesignationName
                            ,GVDE.UserName DesignationName---ISNULL(LocalDesignationName,DesignationName) DesignationName,
                            ,SH.SalaryHead,sh.HeadType
                            ,BSH.Name SalaryHeadBangla
                            ,convert(numeric(10,2), SD.EntryAmount) EntryAmount ,LSG.UserName Grade
                            FROM SalaryInfoDefineMaster SM
                            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
                            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=SD.SalaryHeadID
                            LEFT JOIN EmployeeInformation ei ON EI.SystemId=SM.EmpInfoSystemID
                            LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                            LEFT JOIN MST.LegalSalaryGradeDesignation LSD ON LSD.LegalDesignationId=EI.LegalDesignationId
                            LEFT JOIN  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=lsd.LegalSalaryGradeId
                            LEFT JOIN hkp.Designation GVDE ON EI.GivenDesignationId = GVDE.Id
                            LEFT JOIN hkp.LegalDesignation LD ON EI.LegalDesignationId = LD.Id
                            LEFT JOIN hkp.LocalLanguage LocLangLD ON LocLangLD.LegalDesignationId = EI.LegalDesignationId and LocLangLD.LanguageId ='" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN hkp.LocalLanguage LocLangGD ON LocLangGD.DesignationId = EI.GivenDesignationId and LocLangGD.LanguageId = '" + languageId + @"'--PL.LanguageId	
                            LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=EI.GivenDesignationId AND B.LanguageId='" + languageId + @"'--PL.LanguageId
                            LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL) AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID and BSH.LanguageId='" + languageId + @"'--PL.LanguageId
                            WHERE SM.EmpInfoSystemID='" + empId + @"'AND SM.IsApproved=1                           
                             ) x ORDER BY EffectiveDate DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetEmployeeDataById(string employeeId, string plantId, string employeementType, string languageId, string tempId)
        {
            try
            {

                string sql = @"SELECT EmployeeName,ParmanentAddress1,
                            ISNULL(FatherNameLocal,FatherName) FatherName,
                            ISNULL(MotherNameLocal,MotherName) MotherName,
                            ISNULL(EmployeeNameLocal,EmployeeName) EmployeeNameL,
                            ISNULL(LocalCompanyName,CompanyName) CompanyName,
                            ISNULL(CompanyAddress,CompanyAddress) CompanyAddress,
                            ISNULL(UtilityName,UtilityName) UtilityName,
                            ISNULL(ParmanentAddress1Local,ParmanentAddress1) ParmanentAddress1L,
                            ISNULL(PresentAddress1Local,PresentAddress1) PresentAddress1,
                            ISNULL(PresentCity,PresentCity) PresentCity,
                            ISNULL(PresentDistrict,PresentDistrict) PresentDistrict,
                            ISNULL(PresentState,PresentState) PresentState,
                            ISNULL(LPresentCountry,LPermanentCountry) LPresentCountry,
                            ISNULL(FirstName,FirstName) FirstName,
                            ISNULL(LocalDesignationName,DesignationName) DesignationName,
                            ISNULL(DOJ,DOJ) DOJ,
                            ISNULL(DateOfJoin,DateOfJoin) DateOfJoin,
                            ISNULL(confirm,confirm) confirm,
                            ISNULL(MobileNo,MobileNo) MobileNo,
                            ISNULL(LocalDepartmentName,Department) Department,
                            ISNULL(LocalSection,Section) Section,
                            ISNULL(Unit,Unit) Unit,
                            ISNULL(DOC,DOC) DOC,
                            ISNULL(NationalID,NationalID) NationalID,
                            ISNULL(EmployeeCode,EmployeeCode) EmployeeCode,
                            ISNULL(BloodGroup,BloodGroup) BloodGroup,
                            ISNULL(EmployeePic,EmployeePic) EmployeePic,
                            ISNULL(EmploymentTypeName,EmploymentType) EmploymentType,
                                    RPTM.TemplateFileName FROM(SELECT TAB2.*, AM.Phone, AM.Email, AM.Website, AM.Address1 FROM (SELECT TAB1.*, LAN.StandardName FROM (SELECT CM.Image CompanyLogo,
                                    CM.UserName CompanyName,AM.Address1 CompanyAddress,E.EmployeeName,
                                    E.FatherName,E.MotherName,e.FatherNameLocal,e.MotherNameLocal,E.EmpPicPath EmployeePic,E.EmployeeCode, Convert(varchar, E.DOJ, 105) DOJ,
                                    REPLACE(CONVERT(VARCHAR(11),E.DOJ,106),' ','-') DateOfJoin,BG.UserName BloodGroup
                                    ,E.NationalID,E.EmploymentType,D.UserName DesignationName, dm.EmployeeCategoryId,ec.UserName EmployeeCategory,L.UserName Line,
			                		E.EmpSignature CardHolderSignature,P.AuthorizedSignature
                                    ,E.CellPhnNo MobileNo,E.ParmanentAddress1,DP.UserName Department,SE.UserName Section,A.[Name] LocalCompanyName, B.[Name] LocalDesignationName,C.[Name] LocalDepartmentName,
			                		N.Name NameLabel,SEL.[Name] LocalSection
                                    ,DN.Name DesignationLabel,DPN.Name DepartmentLabel,LN.Name LineLabel,LET.Name EmploymentTypeLabel, ID.Name IDNoLabel,  PT.Name EmploymentTypeName,
			                		DJ.Name DOJLabel, ET.Name EmergencyTellNoLabel, BGP.Name BloodGroupLabel
                                    ,E.EmployeeNameLocal,LL.UtilityName,NID.Name NIDLabel, E.ParmanentAddress1Local, (PML.Name+' '+LA.Name) ParmanentAddress,LMB.Name MobileNoLabel,
			                		LD.Name LegalDesignationLocal,E.PresentAddress1Local
                                    ,Convert(varchar, DATEADD(year, 5, E.DOJ),105) AS Validity,LNN.Name LineLocal,UN.Username Unit, Convert(varchar, E.DOC, 105) DOC
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
                                    LEFT JOIN ORG.Unit UN ON UN.Id=E.UnitId
			                		LEFT JOIN [SCS].[PlantSetting] P ON P.PlantId=E.PlantId
                                    LEFT JOIN ORG.Department DP ON DP.Id=E.DepartmentId
                                    LEFT JOIN org.Section SE ON SE.Id=E.SectionId
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
                                    LEFT JOIN HKP.LocalLanguage SEL ON SEL.SectionId=E.SectionId AND SEL.LanguageId='" + languageId + @"'
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
                                    "LEFT JOIN  (SELECT * FROM SCS.RptConfigTemplate WHERE Id='" + tempId + "'  and PlantId='" + plantId + @"') AS RPTM ON TAB3.PlantId=RPTM.PlantId";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetEmployeeById(string employeeId, string plantId, string employeementType, string languageId, string tempId)
        {
            try
            {

                string sql = @"SELECT TOP 1 EmpSystemID,
                            ISNULL(FatherNameLocal,FatherName) FatherName,FatherName FatherNameEng,
                            ISNULL(MotherNameLocal,MotherName) MotherName, MotherName MotherNameEng,
                            ISNULL(EmployeeNameLocal,EmployeeName) EmployeeName,EmployeeNameEng,
                            ISNULL(LocalCompanyName,CompanyName) CompanyName,
                            ISNULL(CompanyAddress,CompanyAddress) CompanyAddress,
                            ISNULL(UtilityName,UtilityName) UtilityName,
                            ISNULL(ParmanentAddress1,ParmanentAddress1) ParmanentAddress1,
                            ISNULL(ParmanentAddress2Local,ParmanentAddress1) ParmanentAddressLocal,
                            Grade,		
                            ISNULL(PresentAddress1,PresentAddress1) PresentAddress1,
                            ISNULL(PresentAddress2Local,PresentAddress1) PresentAddressLocal,

                            ISNULL(PresentCity,PresentCity) PresentCity,
                            ISNULL(PresentDistrict,PresentDistrict) PresentDistrict,
                            ISNULL(PresentState,PresentState) PresentState,
                            ISNULL(LPresentCountry,LPermanentCountry) LPresentCountry,
                            ISNULL(FirstName,FirstName) FirstName,
                            ISNULL(LocalDesignationName,DesignationName) DesignationName,
                        --    ISNULL(LegalDesignationLocal,LegalDesignation) DesignationName,
                        --    ISNULL(LocalDesignationName,DesignationName) LocalDesignationName,
                            ISNULL(DOJ,DOJ) DOJ,
                            ISNULL(DateOfJoin,DateOfJoin) DateOfJoin,
                            ISNULL(confirm,confirm) ProbationPeriod,
                            ISNULL(MobileNo,MobileNo) MobileNo,
                            ISNULL(Department,Department) Department,
                            ISNULL(Section,Section) Section,
                            ISNULL(Unit,Unit) Unit,
                            ISNULL(DOC,DOC) DOC,
                            ISNULL(NationalID,NationalID) NationalID,
                            ISNULL(EmployeeCode,EmployeeCode) EmployeeCode,
                            ISNULL(BloodGroup,BloodGroup) BloodGroup,
                            ISNULL(EmployeePic,EmployeePic) EmployeePic,AppliedDate--,
							
                           
                                    --RPTM.TemplateFileName 
                                    FROM(SELECT TAB2.*, AM.Phone, AM.Email, AM.Website, AM.Address1 FROM (SELECT TAB1.*, LAN.StandardName 
                                    FROM (SELECT CM.Image CompanyLogo,E.SystemID as EmpSystemID,
                                    CM.UserName CompanyName,AM.Address1 CompanyAddress,E.EmployeeName,E.EmployeeName EmployeeNameEng,
                                    E.FatherName,E.MotherName,e.FatherNameLocal,e.MotherNameLocal,E.EmpPicPath EmployeePic,E.EmployeeCode, Convert(varchar, E.DOJ, 105) DOJ,
                                    REPLACE(CONVERT(VARCHAR(11),E.DOJ,106),' ','-') DateOfJoin,BG.UserName BloodGroup
                                    ,E.NationalID,E.EmploymentType,D.UserName DesignationName, dm.EmployeeCategoryId,ec.UserName EmployeeCategory,L.UserName Line,
			                		E.EmpSignature CardHolderSignature,P.AuthorizedSignature
                                    ,E.CellPhnNo MobileNo,E.ParmanentAddress1,E.ParmanentAddress2Local,DP.UserName Department,SE.UserName Section,A.[Name] LocalCompanyName, B.[Name] LocalDesignationName,C.[Name] LocalDepartmentName,
			                		N.Name NameLabel
                                    ,DN.Name DesignationLabel,DPN.Name DepartmentLabel,LN.Name LineLabel,LET.Name EmploymentTypeLabel, ID.Name IDNoLabel,  PT.Name EmploymentTypeName,
			                		DJ.Name DOJLabel, ET.Name EmergencyTellNoLabel, BGP.Name BloodGroupLabel
                                    ,E.EmployeeNameLocal,LL.UtilityName,NID.Name NIDLabel, E.ParmanentAddress1Local, (PML.Name+' '+LA.Name) ParmanentAddress,LMB.Name MobileNoLabel,
			                		LD.Name LegalDesignationLocal
									,LSG.ShortName Grade
                                    ,Convert(varchar, DATEADD(year, 5, E.DOJ),105) AS Validity,LNN.Name LineLocal,UN.Username Unit, Convert(varchar, E.DOC, 105) DOC,FORMAT(E.AppliedDate,'dd-MMM-yyyy') AppliedDate
                                    ,PCN.Name LPermanentCountry,PRCN.Name LPresentCountry,E.PresentAddress1,E.PresentAddress2Local
			                		,PD.Name PermanentDistrict,PRD.Name PresentDistrict,PST.Name PermanentState, PRST.Name PresentState,PCT.Name PermanentCity, PRCT.Name PresentCity
                                    ,CASE WHEN DOCDay=0 THEN DOCMonth ELSE DOCDay/30 END AS confirm, PL.LanguageId, PL.Id as 'PlantId', CM.AddressMasterId,E.FirstName,LDN.UserName LegalDesignation  FROM EmployeeInformation E
                                    LEFT JOIN ORG.Company CM ON CM.Id = E.CompanyId
                                    LEFT JOIN MST.AddressMaster AM ON AM.Id = CM.AddressMasterId
                                    LEFT JOIN HKP.BloodGroup BG ON BG.Id = E.BloodGroupID
                                    LEFT JOIN HKP.Designation D ON D.Id = E.GivenDesignationId
                                    LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = e.GivenDesignationId
                                    LEFT JOIN  hkp.LegalDesignation LDN ON LDN.Id=E.LegalDesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id = DM.EmployeeCategoryId
                                    LEFT JOIN ORG.Line L ON L.Id=E.LineId
                                    LEFT JOIN ORG.Unit UN ON UN.Id=E.UnitId
			                		LEFT JOIN [SCS].[PlantSetting] P ON P.PlantId=E.PlantId
                                    LEFT JOIN ORG.Department DP ON DP.Id=E.DepartmentId
                                    LEFT JOIN org.Section SE ON SE.Id=E.SectionId
			                		LEFT JOIN ORG.Plant PL ON PL.Id=E.PlantId
			                		--LEFT JOIN SCS.LegalSalaryGrade LSG on LSG.Id = E.
									LEFT JOIN MST.ManpowerBudget bbb ON e.BudgetCode = bbb.Id
                                    left Join MST.PayrollGroupMaster PGM on PGM.EmployeeId = E.EmployeeId
                                    LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                        AND E.PlantId = gd.PlantId
                                    LEFT JOIN (
                                           		SELECT MAX(EffectiveDate)EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
                                           			FROM MST.LegalSalaryStructure 
                                           			WHERE EffectiveDate <= GETDATE()
                                           		GROUP BY LegalSalaryGradeId, EmployeeLocationId 
                                           		) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = bbb.EmployeeLocationId
                                    LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                    AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                    AND SS.EffectiveDate = S.EffectiveDate
                                    LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 																							
                                    left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id =S.LegalSalaryGradeId

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
                                    "--LEFT JOIN  (SELECT * FROM SCS.RptConfigTemplate WHERE Language='" + tempId + "'  and PlantId='" + plantId + @"') AS RPTM ON TAB3.PlantId=RPTM.PlantId";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetEmployeeBasicInfoById(string employeeId, string plantId, string employeementType, string languageId, string tempId)
        {
            try
            {

                string sql = @"SELECT  EmpSystemID,
                            ISNULL(FatherNameLocal,FatherName) FatherName,  FatherName FatherNameEng,
                            ISNULL(MotherNameLocal,MotherName) MotherName,  MotherName MotherNameEng,
                            ISNULL(EmployeeNameLocal,EmployeeName) EmployeeName, EmployeeName EmployeeNameEng,
                            ISNULL(LocalCompanyName,CompanyName) CompanyName,
                            ISNULL(CompanyAddress,CompanyAddress) CompanyAddress,
                            ISNULL(UtilityName,UtilityName) UtilityName,
                            ISNULL(ParmanentAddress1Local1,ParmanentAddress1) ParmanentAddress1,
                            ISNULL(PresentAddress1Local1,PresentAddress1) PresentAddress1,
                            ISNULL(PresentCity,PresentCity) PresentCity,
                            ISNULL(PresentDistrict,PresentDistrict) PresentDistrict,
                            ISNULL(PresentState,PresentState) PresentState,
                            ISNULL(LPresentCountry,LPermanentCountry) LPresentCountry,
                            ISNULL(FirstName,FirstName) FirstName,
                            ISNULL(LegalDesignationLocal,LegalDesignation) DesignationName,
                            ISNULL(LocalDesignationName,DesignationName) LocalDesignationName,
                            DateOfJoin DOJ,
                            ISNULL(DateOfJoin,DateOfJoin) DateOfJoin,
                            ISNULL(confirm,confirm) confirm,
                            ISNULL(MobileNo,MobileNo) MobileNo,
                            ISNULL(Department,Department) Department,
                            ISNULL(Section,Section) Section,
                            ISNULL(Unit,Unit) Unit,
                            ISNULL(DOC,DOC) DOC,
                            ISNULL(NationalID,NationalID) NationalID,
                            ISNULL(EmployeeCode,EmployeeCode) EmployeeCode,
                            ISNULL(BloodGroup,BloodGroup) BloodGroup,
                            ISNULL(EmployeePic,EmployeePic) EmployeePic,
                            ISNULL(SpouseNameLocal,SpouseName) SpouseName,DOB,SubSection,
                            ISNULL(LocalIdentificationMark,IdentificationMark) IdentificationMark,Age,HightFt,HightInc,AuthorizedSignature,CardHolderSignature,EmployeeFingerPrint --,
                            ,ISNULL((Case When  GenderID ='Male' then     MaleLocal else FemaleLocal end),GenderID) Gender,GenderID
                                    --RPTM.TemplateFileName 
                                    FROM(SELECT TAB2.*, AM.Phone, AM.Email, AM.Website, AM.Address1 FROM (SELECT TAB1.*, LAN.StandardName 
                                    FROM (SELECT CM.Image CompanyLogo,E.SystemID as EmpSystemID,
                                    CM.UserName CompanyName,AM.Address1 CompanyAddress,E.EmployeeName,
                                    E.FatherName,E.MotherName,e.FatherNameLocal,e.MotherNameLocal,E.EmpPicPath EmployeePic,E.EmployeeCode, Convert(varchar, E.DOJ, 105) DOJ,
                                    REPLACE(CONVERT(VARCHAR(11),E.DOJ,106),' ','-') DateOfJoin,BG.UserName BloodGroup
                                    ,E.NationalID,E.EmploymentType,D.UserName DesignationName, dm.EmployeeCategoryId,ec.UserName EmployeeCategory,L.UserName Line,
			                		E.EmpSignature CardHolderSignature,P.AuthorizedSignature
                                    ,E.CellPhnNo MobileNo,E.ParmanentAddress1,DP.UserName Department,SE.UserName Section,A.[Name] LocalCompanyName, B.[Name] LocalDesignationName,C.[Name] LocalDepartmentName,
			                		N.Name NameLabel
                                    ,DN.Name DesignationLabel,DPN.Name DepartmentLabel,LN.Name LineLabel,LET.Name EmploymentTypeLabel, ID.Name IDNoLabel,  PT.Name EmploymentTypeName,
			                		DJ.Name DOJLabel, ET.Name EmergencyTellNoLabel, BGP.Name BloodGroupLabel
                                    ,E.EmployeeNameLocal,LL.UtilityName,NID.Name NIDLabel, E.ParmanentAddress1Local, (PML.Name+' '+LA.Name) ParmanentAddress,LMB.Name MobileNoLabel,
			                		LD.Name LegalDesignationLocal
                                    ,Convert(varchar, DATEADD(year, 5, E.DOJ),105) AS Validity,LNN.Name LineLocal,UN.Username Unit, Convert(varchar, E.DOC, 105) DOC
                                    ,PCN.Name LPermanentCountry,PRCN.Name LPresentCountry,E.PresentAddress1
			                		,PD.Name PermanentDistrict,PRD.Name PresentDistrict,PST.Name PermanentState, PRST.Name PresentState,PCT.Name PermanentCity, PRCT.Name PresentCity
                                    ,CASE WHEN DOCDay=0 THEN DOCMonth ELSE DOCDay/30 END AS confirm, PL.LanguageId, PL.Id as 'PlantId', CM.AddressMasterId,E.FirstName,E.SpouseName,E.SpouseNameLocal,format(E.DOB,'dd-MMM-yyyy') DOB
                                    ,LocalIdentificationMark,IdentificationMark,cast((DATEDIFF(m, DOB, GETDATE())/12) as varchar) Age,FLOOR(Height) AS HightFt,CEILING((Height*12)%12) HightInc,E.PresentAddress1Local PresentAddress1Local1,E.ParmanentAddress1Local ParmanentAddress1Local1,efp.FileName EmployeeFingerPrint,ISNULL( SBL.Name,SB.username)  SubSection,LDN.UserName LegalDesignation 
                                    ,LMM.Name MaleLocal, LMF.Name FemaleLocal,E.GenderID                                    
                                    FROM EmployeeInformation E
                                    LEFT JOIN ORG.Company CM ON CM.Id = E.CompanyId
                                    LEFT JOIN MST.AddressMaster AM ON AM.Id = CM.AddressMasterId
                                    LEFT JOIN HKP.BloodGroup BG ON BG.Id = E.BloodGroupID
                                    LEFT JOIN HKP.Designation D ON D.Id = E.GivenDesignationId
                                    LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = e.GivenDesignationId
                                    LEFT JOIN  hkp.LegalDesignation LDN ON LDN.Id=E.LegalDesignationId
                                    LEFT JOIN HKP.EmployeeCategory EC ON EC.Id = DM.EmployeeCategoryId
                                    LEFT JOIN ORG.Line L ON L.Id=E.LineId
                                    LEFT JOIN ORG.Unit UN ON UN.Id=E.UnitId
			                		LEFT JOIN [SCS].[PlantSetting] P ON P.PlantId=E.PlantId
                                    LEFT JOIN ORG.Department DP ON DP.Id=E.DepartmentId
                                    LEFT JOIN org.Section SE ON SE.Id=E.SectionId
			                		LEFT JOIN ORG.Plant PL ON PL.Id=E.PlantId
                                    LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
									LEFT JOIN EmployeeFingerPrint efp ON efp.EmpSystemID=E.SystemId AND efp.Id=(SELECT TOP 1 Id FROM EmployeeFingerPrint WHERE EmpSystemID=E.SystemId)
			                		LEFT JOIN HKP.LocalLanguage A ON A.CompanyId=E.CompanyId AND A.LanguageId='" + languageId + @"'
                                    LEFT JOIN HKP.LocalLanguage SBL ON SBL.SubSectionId=E.SubSectionId AND SBL.LanguageId='" + languageId + @"'
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
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Male'and LanguageId='" + languageId + @"' ) LMM ON LMM.LanguageId=PL.LanguageId
                                    LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Female'and LanguageId='" + languageId + @"' ) LMF ON LMF.LanguageId=PL.LanguageId
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
                                    "--LEFT JOIN  (SELECT * FROM SCS.RptConfigTemplate WHERE Language='" + tempId + "'  and PlantId='" + plantId + @"') AS RPTM ON TAB3.PlantId=RPTM.PlantId";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Dictionary<string, object> GetFilePath(string plantId, string pkId, string reportType)
        {
            var sql = @"SELECT Id,TemplateFileName FROM SCS.RptConfigTemplate WHERE  Language='" + pkId + "'  AND PlantId='" + plantId + "' and Type='" + reportType + "'";
            return _sqlRepository.GetData(sql);
        }

        public Dictionary<string, object> GetLanguage(string plantId, string pkId, string templateType)
        {
            Library.Service.Enums.LetterType.ServiceBook.GetDescription();
            var sql = @"SELECT Id,Language FROM SCS.RptConfigTemplate WHERE  Id='" + pkId + "'  AND PlantId='" + plantId + "' and type='" + templateType + "'";
            //var sql = "SELECT Id,Language FROM SCS.RptConfigTemplate WHERE  [type]='" + pkId + "'  AND PlantId='" + plantId + "'";
            return _sqlRepository.GetData(sql);
        }

        public IEnumerable<ComboModel> GetCbo(string plantId)
        {
            var sql = @"SELECT Id,FormatName FROM SCS.RptConfigTemplate WHERE Type='Appointment Letter' AND PlantId='" + plantId + "' ORDER BY FormatName";
            return _sqlRepository.GetCombo(sql, "Id", "FormatName");
        }

        public IEnumerable<ComboModel> GetTemplateCbo(string plantId, string type)
        {
            var sql = @"SELECT Id,FormatName FROM SCS.RptConfigTemplate WHERE Type='" + type + @"' AND PlantId='" + plantId + "' ORDER BY FormatName";
            return _sqlRepository.GetCombo(sql, "Id", "FormatName");
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

        public IEnumerable<ComboModel> GetDefaultCbo(string companyGrupId, string plantId)
        {
            var sql = @"SELECT L.Id ,L.UserName FROM ORG.CompanyGroup CG
                        LEFT JOIN SCS.[Language] L ON L.Id=CG.LanguageId
                        WHERE CG.Id='" + companyGrupId + @"'
                        UNION
                        SELECT PL.Id ,PL.UserName  FROM ORG.Plant P
                        LEFT JOIN SCS.[Language] PL ON PL.Id=P.LanguageId
                        WHERE P.Id='" + plantId + @"'
                        ORDER BY UserName";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public bool Login(string id, int pin)
        {
            try
            {
                var emp = Query(t => t.SystemId == id).Select().FirstOrDefault();
                if (emp == null)
                    throw new CustomException("Invalid Employee Id.");
                var accessible = Query(t => t.SystemId == id).Select(t => t.IsAccessible).FirstOrDefault();
                if (!accessible)
                    throw new CustomException("No permission to access.");
                var data = _employeeAuthService.Query(t => t.EmployeeId == id).Select().FirstOrDefault();
                if (data == null)
                    throw new CustomException("Please collect your pin.");
                if (data.PIN != pin)
                    throw new CustomException("Invalid pin.");
                return true;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public IWorkbook JobCard_Report(string employeeId, string fromDate, string toDate, string companyGroupId)
        {
            ExcelEngine excelEngine = null;
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            try
            {
                oRU = new ReportUtility();
                //DataSet dsLocal = GetJobCardInfo(employeeId, fromDate, toDate);

                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                CreateSheet_JobCard(ref sheet1, oRU, "Job Card", "Job Card", employeeId, fromDate, toDate, companyGroupId);

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

        public IWorkbook EmpInfoReport(string companyGroupId, string companyId, string plantId, string employeeId)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Employee Information ";
            workbook.Version = ExcelVersion.Excel2013;

            var dtLocal = GetEmpdata(companyGroupId, companyId, plantId);
            if (dtLocal.Rows.Count == 0)
                throw new Exception("No data found !");

            for (int n = 0; n < dtLocal.Rows.Count; n++)
            {
                int xlsCol = 1;
                int xlsRow = 5;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["DataCollectionDateLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["CardNoLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["AgeLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["NameLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["FatherNameLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["DesignationLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["LocalSectionLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["DesignationLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["LocalSectionLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["DOJLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["NIDLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["SpouseNameLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["MobileNoLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["MotherNameLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["EmploymentTypeLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["LineLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["BloodGroupLabel"].ToString()); xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[n]["EmergencyTellNoLabel"].ToString()); xlsCol++;

                oRU.SetCellText(sheet, 6, 2, dtLocal.Rows[n]["CardNumber"].ToString());
                oRU.SetCellText(sheet, 7, 2, dtLocal.Rows[n]["AgeLabel"].ToString());
                oRU.SetCellText(sheet, 14, 2, dtLocal.Rows[n]["SectionName"].ToString());

                //sheet.Range[oRU.GetColumnNameForXls(2) + row].Text = _Amount;
                //sheet.Range[oRU.GetColumnNameForXls(2) + row + ":" + oRU.GetColumnNameForXls(8) + row].Merge();
                //sheet.Range[oRU.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[oRU.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                //sheet.Range[oRU.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                oRU.CompanyPlantHeader(ref sheet, 5, "Employee Report", companyId, identity.PlantName, null);
                oRU.FreezePage(ref sheet, 1, 1 - 5);
                // oRU.PageAdjustableSetup(ref sheet, 1, rowPrint, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        public IWorkbook EmpRegisterReport(string companyGroupId, string companyId, string plantId)
        {
            var excelEngine = new ExcelEngine();
            var oRU = new ReportUtility();
            var workbook = oRU.GetWorkbook(ref excelEngine, 1);
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Employee Register Information ";
            workbook.Version = ExcelVersion.Excel2013;

            var dtLocal = GetEmpdata(companyGroupId, companyId, plantId);
            if (dtLocal.Rows.Count == 0)
                throw new Exception("No data found !");

            int xlsCol = 1;
            int xlsRow = 5;

            if (dtLocal.Rows.Count > 0)
            {
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["CardNoLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["NameLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["NIDLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["FatherNameLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["MotherNameLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["SpouseNameLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["GenderLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["DOBLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["AgeLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["PresentAddressLabel"].ToString());
                sheet.Range[xlsRow, xlsCol].ColumnWidth = 76;
                xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["ParmanentAddress"].ToString());
                sheet.Range[xlsRow, xlsCol].ColumnWidth = 76; xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["MobileNoLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["DOJLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Grade"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["PayAbleLeavelabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["WorkingTimelabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["BreakTimelabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["WeeklyLeaveDaysLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["RosterRelayLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["MobileNoLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["BloodGroupLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["DivisionLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["LocalSectionLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["StaffCateLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["EmploymentTypeLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Salary"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Heightlbl"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["weightlabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Qualificationlabel"].ToString());
                sheet.Range[xlsRow, xlsCol].ColumnWidth = 24; xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["ExperianceLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Maritalstslabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["NumberOfChildLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["NationalityLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Religionlbl"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Bankacclabel"].ToString());// xlsCol += 1;
                char[] splitchar = { ' ' };
                string NomineeLabel = dtLocal.Rows[0]["Nomineelabl"].ToString().Split(splitchar)[0]; xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, NomineeLabel + " " + dtLocal.Rows[0]["NameLabel"].ToString()); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, NomineeLabel + " " + dtLocal.Rows[0]["PresentAddressLabel"].ToString());
                sheet.Range[xlsRow, xlsCol].ColumnWidth = 76; xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, NomineeLabel + " " + dtLocal.Rows[0]["MobileNoLabel"].ToString()); xlsCol += 1;

                string NameLabel = dtLocal.Rows[0]["NameLabel"].ToString().Split(splitchar)[0];
                string LandOwnr = dtLocal.Rows[0]["LandOwnerNameLabel"].ToString().Split(splitchar)[0];
                string LandOwnrmnam = dtLocal.Rows[0]["LandOwnerNameLabel"].ToString().Split(splitchar)[1];

                string LandOwnrlabel = LandOwnr + " " + LandOwnrmnam + " " + NameLabel;
                string LandOwnrmoblabel = LandOwnr + " " + LandOwnrmnam + " " + dtLocal.Rows[0]["MobileNoLabel"].ToString();
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, LandOwnrlabel); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, LandOwnrmoblabel); xlsCol += 1;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, dtLocal.Rows[0]["Commentlabel"].ToString());
            }
            xlsRow = 6;
            for (int n = 0; n < dtLocal.Rows.Count; n++)
            {
                #region --------data----------

                xlsCol = 1;

                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["EmployeeCode"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["EmployeeNameLocal"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["NationalID"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["FatherNameLocal"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["MotherNameLocal"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["SpouseNameLocal"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["GenderID"].ToString()); xlsCol++;
                var dob = Convert.ToDateTime(dtLocal.Rows[n]["DOB"].ToString());
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dob.ToString("dd-MM-yyyy"))); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["Age"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["PresentAddress1Local"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["ParmanentAddress1Local"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["CellPhnNo"].ToString())); xlsCol++;
                var doj = Convert.ToDateTime(dtLocal.Rows[n]["DOJ"].ToString());
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(doj.ToString("dd-MM-yyyy"))); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, ""); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, ""); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["workingTime"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["BreakTime"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["Weakdays"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, ""); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["CellPhnNo"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["BloodGroup"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["LocalDepartmentName"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["SectionName"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["ProbationerName"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["EmloymentType"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["TotalSalary"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["Height"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["Weight"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["Qualification"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, ""); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["MaritalSts"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["NoOfChildren"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["Nationality"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["Religion"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["BankAccNo"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["NomineeNameLocal"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, dtLocal.Rows[n]["PresentAddress1Local"].ToString()); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, cnDgt(dtLocal.Rows[n]["CellPhnNo"].ToString())); xlsCol++;
                oRU.SetCellText(sheet, xlsRow, xlsCol, ""); xlsCol++;

                xlsRow++;

                #endregion --------data----------
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            oRU.CompanyPlantHeader(ref sheet, 5, "Employee Register Report", companyId, identity.PlantName, null);
            oRU.FreezePage(ref sheet, 1, 6);
            return workbook;
        }

        private void CreateSheet_JobCard(ref IWorksheet sheet1, ReportUtility oRU, string SheetHeader, string SheetName, string employeeId, string fromDate, string toDate, string companyGroupId)
        {
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string sOfficeInTime = "00:00:00";
            string sInTime = "00:00:00";

            try
            {
                var dtLocal = GetJobCardInfo(employeeId, fromDate, toDate);

                if (dtLocal.Rows.Count == 0)
                    throw new CustomException("No Data Found!");

                #region DataSet

                xlsRow = 7;
                string strEmpCode = "";
                int iDate = 0;
                int iShiftIntime = 0;
                int iInTime = 0;
                int iInDevID = 0;
                int iOutTime = 0;
                int iOutDevID = 0;
                int iOTHr = 0;
                int iDayStatus = 0;
                int iLvShortName = 0;
                string strLateBy = "00:00:00";
                int iLateBy = 0;
                int iShiftName = 0;
                int iShiftOuttime = 0;

                if (dtLocal.Rows.Count > 0)
                {
                    for (int i = 0; i < dtLocal.Rows.Count; i++)
                    {
                        if ((string.Compare(strEmpCode.ToUpper(), dtLocal.Rows[i]["EmployeeCode"].ToString().Trim().ToUpper())) != 0)
                        {
                            #region ------------------Column Header------------------

                            xlsCol = 1;
                            xlsRow = 5;
                            sheet1.Range[xlsRow, xlsCol].Text = "Employee Code";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["EmployeeCode"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsCol = 1;
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Employee Name";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["EmployeeName"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsCol = 1;
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Text = "DOJ";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["DOJ"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsCol = 1;
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Unit";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["Unit"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsCol = 1;
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Department";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["Department"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsCol = 1;
                            xlsRow += 1;
                            //sheet1.Range[xlsRow, xlsCol].Text = "Given Designation";
                            //sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["GivenDesignation"].ToString().Trim();
                            //sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal[i]["Designation"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsRow += 1;
                            xlsCol = 5;
                            xlsRow = 6;
                            sheet1.Range[xlsRow, xlsCol].Text = "Division";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["Division"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Section";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["Section"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "SubSection";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["SubSection"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Designation";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dtLocal.Rows[i]["LegalDesignation"].ToString().Trim();
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
                            iShiftName = xlsCol;

                            sheet1.Range[xlsRow, iShiftName].Text = "Shift Name";
                            sheet1.Range[xlsRow, iShiftName].ColumnWidth = 24;
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

                            //xlsCol += 1;
                            //iInDevID = xlsCol;
                            //sheet1.Range[xlsRow, iInDevID].Text = "In Device ID";
                            //sheet1.Range[xlsRow, iInDevID].ColumnWidth = 12;
                            //sheet1.Range[xlsRow, iInDevID].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            //sheet1.Range[xlsRow, iInDevID].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //xlsCol += 1;
                            //iOutDevID = xlsCol;
                            //sheet1.Range[xlsRow, iOutDevID].Text = "Out Device ID";
                            //sheet1.Range[xlsRow, iOutDevID].ColumnWidth = 12;
                            //sheet1.Range[xlsRow, iOutDevID].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            //sheet1.Range[xlsRow, iOutDevID].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                            endXlsCol = xlsCol;

                            #endregion ------------------Column Header------------------
                        }
                        strEmpCode = dtLocal.Rows[i]["EmployeeCode"].ToString().Trim();

                        #region ----------------------Data-----------------------

                        xlsRow += 1;
                        sheet1.Range[xlsRow, iDate].Text = dtLocal.Rows[i]["PDate"].ToString();
                        sheet1.Range[xlsRow, iDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iShiftName].Text = dtLocal.Rows[i]["ShiftName"].ToString();
                        sheet1.Range[xlsRow, iShiftName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iShiftName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iShiftIntime].Text = dtLocal.Rows[i]["ShiftIntime"].ToString();
                        sheet1.Range[xlsRow, iShiftIntime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iShiftIntime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iShiftOuttime].Text = dtLocal.Rows[i]["ShiftOutTime"].ToString();
                        sheet1.Range[xlsRow, iShiftOuttime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iShiftOuttime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iInTime].Text = dtLocal.Rows[i]["InTimeShow"].ToString();
                        sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iOutTime].Text = dtLocal.Rows[i]["OutTimeShow"].ToString();
                        sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (dtLocal.Rows[i]["DayStatus"].ToString().Trim() == "L")
                        {
                            sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Red;
                            sheet1.Range[xlsRow, iDayStatus].Text = "P";
                        }
                        else
                        {
                            sheet1.Range[xlsRow, iDayStatus].Text = dtLocal.Rows[i]["DayStatus"].ToString().Trim();
                        }
                        sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                        sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        // xlsCol += 1;

                        if (dtLocal.Rows[i]["DayStatus"].ToString().Trim() == "L")
                        {
                            #region Late by min

                            sInTime = "00:00:00";
                            if (dtLocal.Rows[i]["InTime"].ToString().Trim() != "")
                            {
                                sInTime = dtLocal.Rows[i]["InTime"].ToString().Trim() + ":00";
                            }
                            else
                            {
                                if (dtLocal.Rows[i]["OutTime"].ToString().Trim() != "")
                                {
                                    sInTime = dtLocal.Rows[i]["OutTime"].ToString().Trim() + ":00";
                                }
                            }
                            sOfficeInTime = "00:00:00";
                            strLateBy = "00:00";
                            if (dtLocal.Rows[i]["ShiftInTime"].ToString().Trim() != "")
                            {
                                sOfficeInTime = dtLocal.Rows[i]["ShiftInTime"].ToString().Trim() + ":00";
                                //sOfficeInTime = dvLocal[i]["ShiftTime"].ToString().Trim();
                                strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                            }

                            #endregion Late by min
                        }
                        else
                        {
                            ///absent by how min

                            #region Absent by how much min

                            if (dtLocal.Rows[i]["DayStatus"].ToString().Trim() == "A")
                            {
                                sInTime = "00:00:00";
                                if (dtLocal.Rows[i]["InTime"].ToString().Trim() != "")
                                {
                                    sInTime = dtLocal.Rows[i]["InTime"].ToString().Trim() + ":00";
                                    sOfficeInTime = "00:00:00";
                                    strLateBy = "00:00";
                                    if (dtLocal.Rows[i]["ShiftInTime"].ToString().Trim() != "")
                                    {
                                        sOfficeInTime = dtLocal.Rows[i]["ShiftInTime"].ToString().Trim() + ":00";
                                        strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                                    }
                                }
                                else
                                {
                                    //if (dvAttn[i]["OutTime"].ToString().Trim() != "")
                                    //{
                                    //    sInTime = dvAttn[i]["OutTime"].ToString().Trim() + ":00";
                                    //}
                                    strLateBy = "";
                                }
                            }
                            else
                            {
                                strLateBy = "";
                            }

                            #endregion Absent by how much min
                        }

                        string dti = dtLocal.Rows[i]["dti"].ToString().Trim();
                        string dto = dtLocal.Rows[i]["dto"].ToString().Trim();
                        string _InTimeShow = dtLocal.Rows[i]["InTimeShow"].ToString().Trim();
                        string _OutTimeShow = dtLocal.Rows[i]["OutTimeShow"].ToString().Trim();
                        //sheet1.Range[xlsRow, iOTHr].Text = iOT;
                        //sheet1.Range[xlsRow, iOTHr].Text = GetDuration(dti, dto, _InTimeShow, _OutTimeShow); ;
                        //sheet1.Range[xlsRow, iOTHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet1.Range[xlsRow, iOTHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iLateBy].Text = strLateBy;
                        sheet1.Range[xlsRow, iLateBy].CellStyle.Font.Color = ExcelKnownColors.Red;
                        sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iLvShortName].Text = dtLocal.Rows[i]["Code"].ToString();
                        sheet1.Range[xlsRow, iLvShortName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iLvShortName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //sheet1.Range[xlsRow, iInDevID].Text = dtLocal.Rows[i]["InDeviceID"].ToString();
                        //sheet1.Range[xlsRow, iInDevID].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet1.Range[xlsRow, iInDevID].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //sheet1.Range[xlsRow, iOutDevID].Text = dtLocal.Rows[i]["OutDeviceID"].ToString();
                        //sheet1.Range[xlsRow, iOutDevID].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        //sheet1.Range[xlsRow, iOutDevID].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

                        #endregion Line Setup
                    }
                    xlsCol = 2;
                    xlsRow += 5;
                    endXlsCol = 7;
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Name = SheetName;
                    oRU.CompanyGroupHeader(ref sheet1, endXlsCol, "Job Card", companyGroupId);

                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].Merge();
                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range["A4" + ":" + oRU.GetColumnNameForXls(endXlsCol) + "4"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet1.Range["A4"].Text = "Employee Job Card Information From Date: " + fromDate + " To Date: " + toDate;
                }

                #endregion DataSet

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<dynamic> ShowJobCard(string employeeId, string fromDate, string toDate)
        {
            //IEnumerable<JobcardVM> result=null;
            try
            {
                var dt = GetJobCardInfo(employeeId, fromDate, toDate);
                var dynamicDt = dt.ToDynamic();
                //List<dynamic> dynamicDt = dt.ToDynamic();
                return dynamicDt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<dynamic> ShowDailyAttendance(string employeeId, string FromDate, string ToDate)
        {
            //IEnumerable<JobcardVM> result=null;
            try
            {
                var dt = GetDailyAttendance(employeeId, FromDate, ToDate);
                var dynamicDt = dt.ToDynamic();
                //List<dynamic> dynamicDt = dt.ToDynamic();
                return dynamicDt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<dynamic> ShowDailyAttendance(string employeeId, string WorkingDate)
        {
            //IEnumerable<JobcardVM> result=null;
            try
            {
                var dt = GetDailyAttendance(employeeId, WorkingDate);
                var dynamicDt = dt.ToDynamic();
                //List<dynamic> dynamicDt = dt.ToDynamic();
                return dynamicDt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable GetDailyAttendance(string employeeId, string FromDate, string ToDate)
        {
            try
            {
                var sql = @"
                              SELECT E.SystemId EmpSystemId,E.EmployeeCode
	                                , E.EmployeeName
	                                , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
                                    , REPLACE(CONVERT(VARCHAR(11), AD.WorkDate, 113), ' ', '-') WorkDate
									 , AD.DayStatus
									 ,CONVERT(varchar(15),CAST(AD.InTime AS TIME),100) InTimeShow
	                                , ARIN.DeviceID InDeviceID
									 ,CONVERT(varchar(15),CAST(AD.OutTime AS TIME),100) OutTimeShow
	                                , AROUT.DeviceID OutDeviceID
									 ,CONVERT(varchar(15),CAST(SD.InTime AS TIME),100) ShiftInTimeShow
                                    ,CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100) ShiftOutTimeShow
                                    , SD.ShiftDefinationDescription ShiftName
									, AD.CountedShortLeave ShortLeave ,lt.Code LeaveType
                                    ,ad.IsManualDayStatus,ad.IsManualInTime,ad.IsManualOutTime
									 , GVD.UserName GivenDesignation,e.PlantId,e.CompanyId,c.UserName Company

                                FROM dbo.EmployeeInformation E
							                INNER JOIN (select * from dbo.AttdnProcessData )AD ON E.SystemID = AD.EmpSystemID
							                LEFT JOIN (SELECT * FROM dbo.ShiftTimeChgMaster WHERE ('" + FromDate + @"' BETWEEN FromDate AND ToDate
                                                                                               or     '" + ToDate + @"' BETWEEN FromDate AND ToDate)
                                                                                        ) AS SFCG
																                ON AD.ShiftSystemID = SFCG.ShiftDefinationID
							                LEFT JOIN dbo.ShiftDefination SD ON AD.ShiftSystemID = SD.SystemID
							                LEFT JOIN dbo.AttdnRawData ARIN ON AD.InTimeRowID = ARIN.RowID
							                LEFT JOIN dbo.AttdnRawData AROUT ON AD.OutTimeRowID = AROUT.RowID
											left join LeaveType lt on lt.Id=ad.LTSystemID
                                            LEFT JOIN
												(
												SELECT LogDownLoadNum
												,min(ptime) ptime
												from AttdnRawData
												where pdate between '" + FromDate + @"' and '" + ToDate + @"'
												group by LogDownLoadNum
												) LIT on LIT.LogDownLoadNum=E.SystemId
                                            LEFT JOIN AttdnRawData ARD ON ARD.LogDownLoadNum=LIT.LogDownLoadNum  AND ARD.PTime=LIT.ptime
							                                       LEFT JOIN org.Unit U ON E.UnitID = U.Id
                                            LEFT JOIN org.Division Dv ON E.DivisionID = Dv.Id
                                            LEFT JOIN org.SubDivision SubDv ON E.SubdivisionID = SubDv.Id
                                            LEFT JOIN org.Department Dp ON E.DepartmentID = Dp.Id
                                            LEFT JOIN org.Section S ON E.SectionID = S.Id
                                            LEFT JOIN org.SubSection SB ON E.SubSectionID = SB.Id
                                            LEFT JOIN org.Line L ON E.LineID = L.Id
                                            LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupID = Dg.Id
                                            LEFT JOIN hkp.Designation D ON E.DesignationSystemID = D.Id
                                            LEFT JOIN hkp.Designation GVD ON E.GivenDesignationId = GVD.Id
                                            left join org.Company c on c.id=e.CompanyId
                                            LEFT JOIN
                                            (
                                            SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
											LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
											)EC ON EC.DesignationId=E.GivenDesignationId

			                    WHERE AD.WorkDate  between '" + FromDate + @"' and '" + ToDate + @"'
								AND E.EmployeeStatus='Active'
								and e.systemid='" + employeeId + @"'
                                ";
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

        private DataTable GetDailyAttendance(string employeeId, string WorkingDate)
        {
            try
            {
                var sql = @"
                              SELECT E.SystemId EmpSystemId,E.EmployeeCode
	                                , E.EmployeeName
	                                , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
                                    , REPLACE(CONVERT(VARCHAR(11), AD.WorkDate, 113), ' ', '-') WorkDate
									 , AD.DayStatus
									 ,CONVERT(varchar(15),CAST(AD.InTime AS TIME),100) InTimeShow
	                                , ARIN.DeviceID InDeviceID
									 ,CONVERT(varchar(15),CAST(AD.OutTime AS TIME),100) OutTimeShow
	                                , AROUT.DeviceID OutDeviceID
									 ,CONVERT(varchar(15),CAST(SD.InTime AS TIME),100) ShiftInTimeShow
                                    ,CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100) ShiftOutTimeShow
                                    , SD.ShiftDefinationDescription ShiftName
									, AD.CountedShortLeave ShortLeave ,lt.Code LeaveType
                                    ,ad.IsManualDayStatus,ad.IsManualInTime,ad.IsManualOutTime
									 , GVD.UserName GivenDesignation,e.PlantId,e.CompanyId,c.UserName Company

                                FROM dbo.EmployeeInformation E
							                INNER JOIN (select * from dbo.AttdnProcessData )AD ON E.SystemID = AD.EmpSystemID
							                LEFT JOIN (SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + WorkingDate + @"' BETWEEN FromDate AND ToDate
                                                                                        ) AS SFCG
																                ON AD.ShiftSystemID = SFCG.ShiftDefinationID
							                LEFT JOIN dbo.ShiftDefination SD ON AD.ShiftSystemID = SD.SystemID
							                LEFT JOIN dbo.AttdnRawData ARIN ON AD.InTimeRowID = ARIN.RowID
							                LEFT JOIN dbo.AttdnRawData AROUT ON AD.OutTimeRowID = AROUT.RowID
											left join LeaveType lt on lt.Id=ad.LTSystemID
                                            LEFT JOIN
												(
												SELECT LogDownLoadNum
												,min(ptime) ptime
												from AttdnRawData
												where pdate= '" + WorkingDate + @"'
												group by LogDownLoadNum
												) LIT on LIT.LogDownLoadNum=E.SystemId
                                            LEFT JOIN AttdnRawData ARD ON ARD.LogDownLoadNum=LIT.LogDownLoadNum  AND ARD.PTime=LIT.ptime
							                                       LEFT JOIN org.Unit U ON E.UnitID = U.Id
                                            LEFT JOIN org.Division Dv ON E.DivisionID = Dv.Id
                                            LEFT JOIN org.SubDivision SubDv ON E.SubdivisionID = SubDv.Id
                                            LEFT JOIN org.Department Dp ON E.DepartmentID = Dp.Id
                                            LEFT JOIN org.Section S ON E.SectionID = S.Id
                                            LEFT JOIN org.SubSection SB ON E.SubSectionID = SB.Id
                                            LEFT JOIN org.Line L ON E.LineID = L.Id
                                            LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupID = Dg.Id
                                            LEFT JOIN hkp.Designation D ON E.DesignationSystemID = D.Id
                                            LEFT JOIN hkp.Designation GVD ON E.GivenDesignationId = GVD.Id
                                            left join org.Company c on c.id=e.CompanyId
                                            LEFT JOIN
                                            (
                                            SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
											LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
											)EC ON EC.DesignationId=E.GivenDesignationId

			                    WHERE AD.WorkDate  = '" + WorkingDate + @"'
								AND E.EmployeeStatus='Active'
								and e.systemid='" + employeeId + @"'
                                ";
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

        public DataTable GetJobCardInfo(string employeeId, string fromDate, string toDate)
        {
            try
            {
                var sql = @"SELECT A.EmployeeCode
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
                            	,A.InTime
                                ,CONVERT(VARCHAR(5), A.ShiftInTime, 108) ShiftInTime
                            	,A.InDeviceID
                            	,A.OutTime
                            	,A.OutDeviceID
                            	,A.IsManual
                            	,A.OTHr
                            	,A.LvShortName
                            	,A.Code
                            	,A.LvDescrip
                            	,A.LeaveType
                                ,dti,dto
                                ,InTimeShow
                                ,OutTimeShow
                                ,ShiftTime = CASE
		                            WHEN ShiftChangeInTime IS NULL
			                            THEN ShiftInTime
		                            ELSE ShiftChangeInTime
		                            END
                                ,ShiftName
                               ,CONVERT(VARCHAR(5), A.ShiftOutTime, 108) ShiftOutTime
                            FROM (
                            	SELECT E.EmployeeCode
                            		,E.EmployeeName
                            		,REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
                            		,D.UserName GivenDesignation
                            		,U.UserName Unit
                            		,Dv.UserName Division
                            		,Dp.UserName Department
                            		,S.UserName Section
                            		,SB.UserName SubSection
                            		,AR.WorkDate PDate
                            		,AR.DayStatus
                            		,CONVERT(VARCHAR(5), AR.InTime, 108) InTime
                                    ,CONVERT(varchar(15),CAST(AR.InTime AS TIME),100) InTimeShow
                                    --,ShiftInTime=case  when cs.InTime is null then sd.InTime else cs.InTime end
                                    ,CONVERT(VARCHAR(5), SD.InTime, 108) ShiftInTime
                            		,ARIN.DeviceID InDeviceID
                            		,CONVERT(VARCHAR(5), AR.OutTime, 108) OutTime
                                    ,CONVERT(varchar(15),CAST(AR.OutTime AS TIME),100) OutTimeShow
                            		,AROUT.DeviceID OutDeviceID
                            		,AR.IsManualInTime IsManual
                            		,AR.OTHr
                            		,LT.UserName LvShortName
                            		,LT.Description LvDescrip
                            		,LT.LeaveType
                                    ,LT.Code
                                    ,Isnull(LG.UserName,'') LegalDesignation
                                    ,AR.InTime dti,AR.OutTime dto
                                    , CONVERT(VARCHAR(5), SFCG.InTime, 108) ShiftChangeInTime
                                    , SD.ShiftDefinationName ShiftName
                                    ,CONVERT(VARCHAR(5), SD.OutTime, 108) ShiftOutTime
                            	
                                FROM dbo.EmployeeInformation E
                            	INNER JOIN dbo.AttdnProcessData AR ON E.SystemID = AR.EmpSystemID
                                --LEFT JOIN (SELECT * FROM dbo.ShiftTimeChgMaster) AS SFCG
                                LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + fromDate + @"' BETWEEN FromDate AND ToDate) AS SFCG

                                                                               
																                ON AR.ShiftSystemID = SFCG.ShiftDefinationID

                            	LEFT JOIN dbo.AttdnRawData ARIN ON AR.InTimeRowID = ARIN.RowID
                            	LEFT JOIN dbo.AttdnRawData AROUT ON AR.OutTimeRowID = AROUT.RowID
                            	LEFT JOIN dbo.LeaveType LT ON AR.LTSystemID = LT.Id
                            	LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            	LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            	LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            	LEFT JOIN ORG.Section S ON E.SectionID = S.Id
                            	LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                                LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
	                            left join EmpDateWiseShiftAssign es on es.EmpSystemID=E.SystemId
                                AND AR.WorkDate=ES.WorkDate
                                left join (
					            SELECT  m.ShiftDefinationID,c.ShiftDate,m.InTime,m.SystemID  FROM [ShiftTimeChgMaster] m
					            left join [ShiftTimeChgChild] c on m.SystemID=c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID=es.ShiftSystemID and cs.ShiftDate=ar.WorkDate

								left join [ShiftDefination] sd on sd.SystemID=es.ShiftSystemID
                            	LEFT JOIN HKP.Designation D ON E.GivenDesignationId = D.Id
                            	WHERE E.SystemID IN (" + employeeId + @")
                            		AND AR.WorkDate BETWEEN '" + fromDate + @"'
                            			AND '" + toDate + @"' AND E.EmployeeStatus='Active'
                            	) A
                            GROUP BY A.EmployeeCode
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
                                ,A.ShiftInTime
                            	,A.InDeviceID
                            	,A.OutTime
                            	,A.OutDeviceID
                            	,A.IsManual
                            	,A.OTHr
                            	,A.LvShortName
                            	,A.LvDescrip
                            	,A.LeaveType
                                ,A.Code
                                ,dti,dto
                                ,InTimeShow
                                ,OutTimeShow
                                ,ShiftChangeInTime
                                ,ShiftName
                                ,A.ShiftOutTime
                                ";
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

        private DataTable GetEmpdata(string companyGroupId, string companyId, string plantId)
        {
            try
            {
                var sql = @"  SELECT CM.Image CompanyLogo
                             ,E.NationalID,E.EmployeeCode, E.CardNumber, REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ, REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB
                             ,A.[Name] LocalCompanyName, B.[Name] LocalDesignationName,C.[Name] LocalDepartmentName,N.Name NameLabel
                             ,DN.Name DesignationLabel,DPN.Name DepartmentLabel,LN.Name LineLabel,LET.Name EmploymentTypeLabel, ID.Name IDNoLabel,  PT.Name EmploymentTypeName, DJ.Name DOJLabel, ET.Name EmergencyTellNoLabel, BGP.Name BloodGroupLabel
                             ,E.EmployeeNameLocal,LL.UtilityName,NID.Name NIDLabel, E.ParmanentAddress1Local, (PML.Name+' '+LA.Name) ParmanentAddress,LMB.Name MobileNoLabel
                             ,E.FatherNameLocal, E.MotherNameLocal,E.PresentAddress1Local,E.SpouseNameLocal,LS.Name LocalSectionLabel,SEC.Name SectionName
                             ,LFN.Name FatherNameLabel,LMN.Name MotherNameLabel,LSN.Name SpouseNameLabel,LPL.Name PresentAddressLabel,LPRL.Name ProbationerName ,LPCL.Name CardNoLabel,SC.Name StaffCategoryLabel,LDOB.Name DOBLabel,E.IsConfirmed
                             ,dcd.Name DataCollectionDateLabel, Age.Name AgeLabel,l.UserName Line, e.CellPhnNo,pml.Name EmloymentType,E.TelePhnNo,e.GenderID,Grade.Name Grade,div.Name Division,Division.Name DivisionLabel
                             ,S.Name Salary,E.Height ,e.Weight ,QL.Name Qualificationlabel,EQ.Name Qualification,Nc.Name NoOfChildrenlbl, ex.Name  ExperianceLabel,ms.Name Maritalstslabel,Ns.Name NationalityLabel,RG.Name Religionlbl,Ba.Name Bankacclabel, Nn.Name Nomineelabl
                             ,ht.Name Heightlbl,W.Name weightlabel,DATEDIFF(year, E.DOB,GetDate()) Age,E.TotalSalary,CV.Name MaritalSts ,E.NoOfChildren,CN.Name Nationality,Rl.Name Religion,E.BankAccNo,NIN.LocalName NomineeNameLocal,ELL.LocalName LandOwnerName,LO.Name LandOwnerNameLabel
                             ,GD.Name GenderLabel,Noc.Name NumberOfChildLabel, RR.Name  RosterRelayLabel,PaL.Name  PayAbleLeavelabel,WT.Name WorkingTimelabel,BT.Name  BreakTimelabel,WLD.Name WeeklyLeaveDaysLabel,BG.UserName BloodGroup,(CONVERT(VARCHAR(5), SD.InTime, 108)+'-'+ CONVERT(VARCHAR(5), SD.outtime, 108) ) workingTime
							 ,(CONVERT(VARCHAR(5), SD.BreakStratTime, 108)+'-'+ CONVERT(VARCHAR(5), SD.BreakEndTime, 108) ) BreakTime, cmt.Name Commentlabel,WD.WeekOff Weakdays,stc.Name StaffCateLabel
                              FROM EmployeeInformation E
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
                            LEFT JOIN (SELECT FixSystemID,EmpSystemId,MAX(EffectiveDate) M FROM  EmployeeShiftAssign
							  WHERE EffectiveDate<=GETDATE()
							  GROUP BY FixSystemID,EmpSystemId
							  ) ESA ON ESA.EmpSystemId = E.SystemId
                             LEFT JOIN
							  (SELECT EmpSystemId,MAX(EffectiveDate) M,
							 WeekOff= CASE AlignWithCC WHEN 1 THEN h.DefaultWeekOff
							   ELSE FstOffDay END FROM  EmployeeWeekOffByDay w
							   left join EmployeeInformation e ON e.SystemId=w.EmpSystemID
							  left join PlantWiseHRMSSetting h ON h.PlantID=e.PlantId
							  WHERE EffectiveDate<=GETDATE()
							  GROUP BY h.DefaultWeekOff,EmpSystemId,AlignWithCC,FstOffDay
							  ) WD ON WD.EmpSystemID =E.SystemId
							  LEFT JOIN ShiftDefination SD ON SD.SystemID = ESA.FixSystemID
							  LEFT JOIN EmployeeNomineeInfo NIN ON NIN.EmpSystemId = E.SystemId
							  LEFT JOIN EmployeeLandLordInfo ELL ON ELL.EmpSystemId = E.SystemId
							  LEFT JOIN HKP.LocalLanguage  Rl ON Rl.LanguageId = PL.LanguageId and Rl.ReligionId=E.ReligionID
                              LEFT JOIN HKP.LocalLanguage  CN ON CN.LanguageId = PL.LanguageId and CN.CountryId=E.CitizenID
							  LEFT JOIN HKP.LocalLanguage  CV ON CV.LanguageId = PL.LanguageId and CV.CivilStatusId=E.CivilStatusID
                              LEFT JOIN (Select TOP(1)* from  EmpAcademicQualificationInformation) EQI ON EQI.EmpSystemID =E.SystemId
							  LEFT JOIN HKP.LocalLanguage A ON A.CompanyId=E.CompanyId AND A.LanguageId=PL.LanguageId
                              LEFT JOIN HKP.LocalLanguage LL ON LL.CompanyId=E.CompanyId AND LL.LanguageId=PL.LanguageId
							  LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=E.GivenDesignationId AND PL.LanguageId=B.LanguageId
							  LEFT JOIN HKP.LocalLanguage C ON C.DepartmentId =E.DepartmentId AND PL.LanguageId=C.LanguageId
                              LEFT JOIN HKP.LocalLanguage SEC ON SEC.SectionId = E.SectionId AND PL.LanguageId = SEC.LanguageId
                              LEFT JOIN HKP.LocalLanguage div ON div.DivisionId = E.DivisionId AND PL.LanguageId = div.LanguageId
                               LEFT JOIN HKP.LocalLanguage EQ ON EQ.QualificationLevelId = EQI.EductLevelSystemID AND PL.LanguageId = EQ.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Name') N ON N.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Designation') DN ON DN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Department') DPN ON DPN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Line') LN ON LN.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmploymentType') LET ON LET.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='IDNo') ID ON ID.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Parmanent') PT ON PT.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='DOJ') DJ ON DJ.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmergencyTellNo') ET ON ET.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='BloodGroup') BGP ON BGP.LanguageId=PL.LanguageId
					          LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='NIDNo') NID ON BGP.LanguageId=PL.LanguageId
	                          LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Permanent') PML ON PML.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Address') LA ON LA.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='MobileNo') LMB ON LMB.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Section') LS ON LS.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='FatherName') LFN ON LFN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='MotherName') LMN ON LMN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='SpouseName') LSN ON LSN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='PresentAddress') LPL ON LPL.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Probationer') LPRL ON LPRL.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='CardNo') LPCL ON LPCL.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='StaffCategory') SC ON SC.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='DOB') LDOB ON LDOB.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='DataCollectionDate') DCD ON DCD.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Age') Age ON Age.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Grade') Grade ON Grade.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Division') Division ON Division.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Salary') S ON S.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='QualificationLabelInfo') QL ON QL.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Experience') ex ON ex.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='MaterialStatus') ms ON ms.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Nationality') Ns ON Ns.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Religion') RG ON RG.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='BankAccountNo') Ba ON Ba.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='NomineeInfo') Nn ON Nn.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='NoOfChildren') Nc ON Nc.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Height') ht ON ht.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Weight') W ON W.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='LanOwnerInfo') LO ON LO.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Gender') GD ON GD.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='NumberOfChild') Noc ON Noc.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='RosterAndRelay') RR ON RR.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='PayableLeave') PaL ON PaL.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='WorkingTime') WT ON WT.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='BreakTime') BT ON BT.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='WeeklyLeaveDays') WLD ON WLD.LanguageId = PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='EmployeeShiftAssign') SA ON SA.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='ShiftDefination') SN ON SN.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Comment') cmt ON cmt.LanguageId = PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='StaffCategory') stc ON stc.LanguageId = PL.LanguageId
                              WHERE E.EmployeeStatus ='Active' and E.CompanyId='" + companyId + @"' and e.PlantId='" + plantId + @"'";

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
            return input;
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

        public string GetFormatedDate(string date, string lng)
        {
            var formateDate = string.Empty;
            var day = cnDgt(date.Substring(0, 2), lng);
            var mon = ChangeMonth(date.Substring(3, 3), lng);
            var year = cnDgt(date.Substring(7, 4), lng);
            return formateDate = day + "-" + mon + "-" + year;
        }

        private string GetDuration(string dti, string dto, string intime, string outtime)
        {
            string res = string.Empty;
            try
            {
                // string vDate = Convert.ToDateTime(sDate).ToString("dd-MMM-yyyy");

                if (string.IsNullOrEmpty(intime) == false && string.IsNullOrEmpty(outtime) == false)
                {
                    string vintime = Convert.ToDateTime(intime).ToString("HH:mm:ss");
                    string vouttime = Convert.ToDateTime(outtime).ToString("HH:mm:ss");
                    var x = (Convert.ToDateTime(dto) - (Convert.ToDateTime(dti)));
                    res = x.ToString().Substring(0, 5);
                    //res = (Convert.ToDateTime(dto)-(Convert.ToDateTime(dti))).ToString().Substring(0, 5);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> GetEmployeeById(string employeeId, string employeementType)
        {
            try
            {
                string _sql = @"SELECT CM.Image CompanyLogo,CM.UserName CompanyName,AM.Address1 CompanyAddress,E.EmployeeName,E.EmpPicPath EmployeePic,E.EmployeeCode, Convert(varchar, E.DOJ, 105) DOJ,BG.UserName BloodGroup
                              , E.NationalID,E.EmploymentType,D.UserName DesignationName, dm.EmployeeCategoryId,ec.UserName EmployeeCategory,L.UserName Line,E.EmpSignature CardHolderSignature,P.AuthorizedSignature
                              ,E.CellPhnNo MobileNo,E.ParmanentAddress1,DP.UserName Department,A.[Name] LocalCompanyName, B.[Name] LocalDesignationName,C.[Name] LocalDepartmentName,N.Name NameLabel
                              ,DN.Name DesignationLabel,DPN.Name DepartmentLabel,LN.Name LineLabel,LET.Name EmploymentTypeLabel, ID.Name IDNoLabel,  PT.Name EmploymentTypeName, DJ.Name DOJLabel, ET.Name EmergencyTellNoLabel, BGP.Name BloodGroupLabel
                              ,E.EmployeeNameLocal,LL.UtilityName,NID.Name NIDLabel, E.ParmanentAddress1Local, (PML.Name+' '+LA.Name) ParmanentAddress,LMB.Name MobileNoLabel,LD.Name LegalDesignationLocal
                              ,Convert(varchar, DATEADD(year, 5, E.DOJ),105) AS Validity,LNN.Name LineLocal,E.EmrCntPer1CellNo
                              ,LDG.UserName LegalDesignation,AM.Phone FROM EmployeeInformation E
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
                              LEFT JOIN HKP.LegalDesignation LDG ON LDG.Id=E.LegalDesignationId
							  LEFT JOIN HKP.LocalLanguage A ON A.CompanyId=E.CompanyId AND A.LanguageId=PL.LanguageId
                              LEFT JOIN HKP.LocalLanguage LL ON LL.CompanyId=E.CompanyId AND LL.LanguageId=PL.LanguageId
							  LEFT JOIN HKP.LocalLanguage B ON B.LegalDesignationId=E.LegalDesignationId AND PL.LanguageId=B.LanguageId
							  --LEFT JOIN HKP.LocalLanguage B ON B.DesignationId=E.GivenDesignationId AND PL.LanguageId=B.LanguageId
							  LEFT JOIN HKP.LocalLanguage C ON C.DepartmentId =E.DepartmentId AND PL.LanguageId=C.LanguageId
                              LEFT JOIN HKP.LocalLanguage LD ON LD.LegalDesignationId=E.LegalDesignationId AND PL.LanguageId=LD.LanguageId
                              LEFT JOIN HKP.LocalLanguage LNN ON LNN.LineId=E.LineId AND PL.LanguageId=LNN.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Name') N ON N.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Designation') DN ON DN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Department') DPN ON DPN.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage wHERE LabelName='Line') LN ON LN.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmploymentType') LET ON LET.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='IDNo') ID ON ID.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='" + employeementType + @"') PT ON PT.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='DOJ') DJ ON DJ.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmergencyTelNo') ET ON ET.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='BloodGroup') BGP ON BGP.LanguageId=PL.LanguageId
					          LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='NIDNo') NID ON BGP.LanguageId=PL.LanguageId
	                          LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Permanent') PML ON PML.LanguageId=PL.LanguageId
							  LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Address') LA ON LA.LanguageId=PL.LanguageId
                              LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='MobileNo') LMB ON LMB.LanguageId=PL.LanguageId
                              WHERE E.SystemID ='" + employeeId + "'";
                return _sqlRepository.GetData(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Dictionary<string, object> GetBudgetInfo(string Id)
        {
            return _manpowerBudgetService.GetManpowerBudgetById(Id);
        }

        private static string GetValue(Dictionary<string, object> dic, string key)
        {
            string value = null;
            if (dic.ContainsKey(key))
            {
                value = dic[key].ToString();
            }

            return value;
        }

        public string GetDesignationGroup(string designationId)
        {
            var _sql = "SELECT DesignationGroupId FROM mst.DesignationMaster WHERE DesignationId='" + designationId + "'";
            return _designationMasterRepository.SqlQuery<string>(_sql).FirstOrDefault();
        }

        private void InitBudgetCode(Dictionary<string, object> dic, ref EmployeeInformation bc)
        {
            bc.PositionId = GetValue(dic, "PositionId");
            bc.DepartmentID = GetValue(dic, "DepartmentId");
            bc.DivisionID = GetValue(dic, "DivisionId");
            bc.EmployeeGroupSystemID = GetValue(dic, "EmployeeGroupId");
            bc.LineID = GetValue(dic, "LineId");
            if (string.IsNullOrEmpty(bc.LineID))
            {
                bc.LineID = null;
            }
            bc.SectionID = GetValue(dic, "SectionId");
            bc.SubdivisionID = GetValue(dic, "SubDivisionId");
            bc.SubSectionID = GetValue(dic, "SubSectionId");
            bc.UnitID = GetValue(dic, "UnitId");
            bc.DesignationSystemID = GetValue(dic, "DesignationId");
            bc.EmploymentType = GetValue(dic, "EmploymentType");
            bc.DesignationGroupID = GetDesignationGroup(bc.DesignationSystemID);

        }

        public void UpdateBudgetCode(EmployeeInformation entity)
        {
            try
            {
                var dblist = Find(entity.SystemId);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                EmployeeBudgetCodeHistory employeeBudgetCodeHistory = new EmployeeBudgetCodeHistory
                {
                    Id = GetAutoNumber(nameof(EmployeeBudgetCodeHistory), PKGeneratorEnum.Auto, null, DateTime.Now),
                    EmpSystemId = entity.SystemId,
                    BudgetId = dblist.BudgetCode,
                    GivenDesignationId = dblist.GivenDesignationId,
                    LegalDesignationId = dblist.LegalDesignationId,
                    AddedBy = identity.Name,
                    AddedDate = DateTime.Now,
                    AddedFromIP = identity.IPAddress
                };
                _employeeBudgetCodeHistoryService.Insert(employeeBudgetCodeHistory);

                dblist.BudgetCode = entity.BudgetCode;
                if (!string.IsNullOrEmpty(entity.GivenDesignationId))
                {
                    dblist.GivenDesignationId = entity.GivenDesignationId;
                }
                if (!string.IsNullOrEmpty(entity.LegalDesignationId))
                {
                    dblist.LegalDesignationId = entity.LegalDesignationId;
                }
                //dblist.LegalDesignationId = entity.LegalDesignationId;
                Dictionary<string, object> dic;
                dic = GetBudgetInfo(entity.BudgetCode);

                InitBudgetCode(dic, ref dblist);

                dblist.DateUpdated = DateTime.Now;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetSectionEmployeeList(GridParameter parameters, string plantId, string companyId, string SectionId)
        {
            try
            {
                parameters.CmdText = @"SELECT Emp.SystemID,EMP.EmployeeName,CONVERT (int, EMP.EmployeeCode) EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        WHERE emp.PlantID='" + plantId + @"'  and EMP.CompanyId='" + companyId + @"' and EMP.EmployeeStatus='Active' and EMP.SectionId='" + SectionId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetEmployeeCbo(string GroupId, string companyId, string plantId)
        {
            try
            {
                var sql = @"SELECT SystemId AS Value, EmployeeName AS Text FROM EmployeeInformation WHERE GroupID='" + GroupId + @"' AND CompanyId='" + companyId + @"' AND PlantId='" + plantId + @"' AND EmployeeStatus='Active' ORDER BY EmployeeName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public IWorkbook AttndReport(string companyGroupId, string employeeId, string plantId)
        {
            throw new NotImplementedException();
        }

        public void Insert(List<XLUploadDetail> entities)
        {
            try
            {
                foreach (var item in entities)
                {
                    var dbdata = Find(item.Id);
                    if (dbdata == null)
                    {
                        item.Id = GetAutoNumber(nameof(XLUploadDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
                        _xLUploadDetailService.Insert(item);
                    }
                    else
                    {
                        _xLUploadDetailService.Update(item);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void xgenerateReport(string CalanderYearId, string FromDate, string ToDate, string plantId, string empID, string reportType, string tempId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = 7;
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Lvr" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }
                //------
                //fileName = "Lvr" + plantId + tempId + ".xlsx";
                //strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                //File = fileName;
                //if (!System.IO.File.Exists(strPath))
                //{
                //    throw new CustomException("File <" + fileName + "> Not Found.");
                //}

                ///-----
                var Templatefile = GetFilePath(plantId, tempId, reportType);
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

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "Lvr" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

                bool IsDefLan = false;

                var tokens = (fileName.Substring(("Lvr" + plantId).Length));
                int charLocation = tokens.IndexOf(".", StringComparison.Ordinal);
                var TemplateLan = tokens.Substring(0, charLocation);

                if (tempId != TemplateLan)
                {
                    IsDefLan = true;
                }

                //Creates a new instance for ExcelEngine
                ExcelEngine excelEngine = new ExcelEngine();

                //Loads or open an existing workbook through Open method of IWorkbooks
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(filepath);

                IWorksheet sheet = workbook.Worksheets[0];
                sheet.ShowColumn(0, true);
                DataTable dsBf, dsTransaction, dsBalance, dsHeader;

                //clsDataContext data = new clsDataContext();
                dsBf = loadBf(empID, CalanderYearId);
                dsTransaction = loadLeaveTransactions(empID, FromDate, ToDate);
                dsBalance = loadOpeningBalance(empID, CalanderYearId);
                dsHeader = GetEmployeeBasicInfoById(empID, plantId, "Permanent", langID, tempId);

                IRange range = sheet.UsedRange;
                IRange columnList = range.Rows[0]; //IRange columnList = range.Rows[5];
                int columnListRow = 1;
                int ColumnTemplateRow = 1;
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
                }

                string PrefixOpeningBalance = "OB";
                string PrefixCurrentTransaction = "LT";
                string PrefixClosingBalance = "CB";

                #region  EmployeeInformation    
                string columnName = "";
                for (int R = 0; R < columnListRow; R++)
                {
                    columnName = "";
                    IRange columnListEmp = range.Rows[R];
                    foreach (DataColumn item in dsHeader.Columns)
                    {
                        ////===== def lan 
                        if (IsDefLan == true)
                        {
                            columnName = GetBasicInfoInDefaultLng(item.ColumnName);

                        }
                        ///=====

                        for (int i = 0; i < range.Rows[R].Cells.Count(); i++)
                        {
                            if (string.IsNullOrEmpty(sheet[R + 1, i + 1].Text))
                                continue;

                            if (sheet[R + 1, i + 1].Text.ToUpper().Trim() == "{" + item.ColumnName.ToUpper() + "}")
                            {

                                //sheet[R + 1, i + 1].Text = dsHeader.Rows[0][item.ColumnName].ToString();
                                if (bplib.clsWebLib.IsNumeric(dsHeader.Rows[0][columnName].ToString()))
                                    sheet[R + 1, i + 1].Text = cnDgt(dsHeader.Rows[0][columnName].ToString(), tempId);
                                else if (bplib.clsWebLib.IsDateOK(dsHeader.Rows[0][columnName].ToString()))
                                    sheet[R + 1, i + 1].Text = GetFormatedDate(dsHeader.Rows[0][columnName].ToString(), tempId);
                                else
                                    sheet[R + 1, i + 1].Text = dsHeader.Rows[0][columnName].ToString();
                            }
                        }
                    }
                }

                #endregion
                int RefROW = ColumnTemplateRow;
                int ROW = ColumnTemplateRow + 1; int COL = 1;
                for (int T = 0; T < dsTransaction.Rows.Count; T++)
                {
                    sheet[ROW, 2].Number = (T + 1);

                    foreach (DataRow item in dsBalance.Rows)
                        item["CurrentTransaction"] = 0;

                    for (int CELL = 0; CELL < columnList.Cells.Count(); CELL++)
                    {
                        string cellValue = columnList.Cells[CELL].Text;
                        if (string.IsNullOrEmpty(cellValue))
                            continue;

                        if (cellValue.ToUpper() == "{BF}")
                        {
                            if (dsBf.Rows.Count > 0)
                            {
                                if (string.IsNullOrEmpty(dsBf.Rows[0]["BroughtForward"].ToString()))
                                {
                                    sheet[ROW, (CELL + 1)].Number = 0;
                                    sheet[ROW, (CELL + 1)].NumberFormat = "###0;";
                                }
                                else
                                {
                                    sheet[ROW, (CELL + 1)].Number = Convert.ToInt32(dsBf.Rows[0]["BroughtForward"].ToString());
                                    sheet[ROW, (CELL + 1)].NumberFormat = "###0;";
                                }
                            }
                        }

                        if (cellValue.ToUpper().Contains("DATE"))
                        {
                            if (dsTransaction.Columns.Contains(cellValue.Replace("{", "").Replace("}", "")))
                            {
                                sheet[ROW, (CELL + 1)].NumberFormat = "@";
                                sheet[ROW, (CELL + 1)].Text = GetFormatedDate(Convert.ToDateTime(dsTransaction.Rows[T][cellValue.Replace("{", "").Replace("}", "")].ToString()).ToString("dd-MMM-yyyy"), tempId);
                            }
                        }

                        for (int OB = 0; OB < dsBalance.Rows.Count; OB++)
                        {
                            string leaveTypeOB = "{" + PrefixOpeningBalance + dsBalance.Rows[OB]["LeaveCode"].ToString() + "}";
                            if (leaveTypeOB.ToUpper() == cellValue.ToUpper())
                            {
                                sheet[ROW, CELL + 1].Number = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString());
                            }
                        }

                        string leaveTypeCL = "{" + PrefixCurrentTransaction + dsTransaction.Rows[T]["Code"].ToString() + "}";
                        if (leaveTypeCL.ToUpper() == cellValue.ToUpper())
                        {
                            sheet[ROW, CELL + 1].Number = dbl(dsTransaction.Rows[T]["LeaveDays"].ToString());

                            dsBalance.DefaultView.RowFilter = "LeaveCode='" + dsTransaction.Rows[T]["Code"].ToString() + "'";
                            if (dsBalance.DefaultView.Count > 0)
                            {
                                dsBalance.DefaultView[0]["CurrentTransaction"] = dbl(dsTransaction.Rows[T]["LeaveDays"].ToString());
                                dsBalance.DefaultView.RowFilter = null;
                                dsBalance.AcceptChanges();
                            }
                        }

                        for (int OB = 0; OB < dsBalance.Rows.Count; OB++)
                        {
                            string leaveTypeOB = "{" + PrefixClosingBalance + dsBalance.Rows[OB]["LeaveCode"].ToString() + "}";
                            if (leaveTypeOB.ToUpper() == cellValue.ToUpper())
                            {
                                dsBalance.Rows[OB]["CurrentYearAllocation"] = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString())
                                - dbl(dsBalance.Rows[OB]["CurrentTransaction"].ToString());

                                sheet[ROW, CELL + 1].Number = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString());
                            }
                        }
                    }
                    ROW++;
                }

                sheet.DeleteRow(RefROW);
                sheet.HideColumn(1);

                workbook.SaveAs("File.xlsx", System.Web.HttpContext.Current.Response, ExcelDownloadType.Open);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IEnumerable<object> GetSuperVisor(string companyid, string plantid)
        {
            var sql = @"SELECT Emp.SystemID,EMP.EmployeeName,CONVERT (int, EMP.EmployeeCode) EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,DV.UserName Division,EC.UserName EmployeeCategory,EMP.EmployeeStatus
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN ORG.Division DV on DV.Id = EMP.DivisionId
                                        LEFT JOIN
									   (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									                ,dg.UserName GivenDesignationGroup
									                from ( SELECT dm.* FROM MST.DesignationMaster DM) DM
									                LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
									                ) EGDSGG on EGDSGG.DesignationId=EMP.GivenDesignationId 
								        LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=EGDSGG.EmployeeCategoryId
                                        WHERE EMP.CompanyId='" + companyid + @"' AND EMP.PlantId='" + plantid + @"' ";

            return _sqlRepository.GetDataCollection(sql);
        }

        private DataTable loadBf(string EmployeeId, string CalanderYearId)
        {
            try
            {
                var sql = @"SELECT EmployeeId, 'Earn' LeaveType,Convert (DECIMAL(10,0),ISNULL (Sum(BroughtForward),0)) BroughtForward,CurrentYearAllocation   
                           from TRN.EmployeeLeaveSummary where LeaveTypeId in (
                            Select Id from LeaveType where LeaveType = 'Earn')
                                 and EmployeeId = '" + EmployeeId + "' and CalanderYearId='" + CalanderYearId + @"'
                                  group by EmployeeId,CurrentYearAllocation";
                var list = _sqlRepository.GetDataTable(sql);

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable loadLeaveTransactions(string EmpSystemID, string FromDate, String ToDate)
        {
            try
            {
                //var sql = @"Select LT.EmpSystemID,L.Code,LT.FromDate,LT.ToDate,LT.LeaveDays,LT.AppliedDate,LT.ApprovedDate
                // from LeaveTransaction LT
                //Left Outer Join LeaveTransactionDetails LD on LT.SystemID=LD.LvTrnsSystemID
                //LEFT OUTER JOIN LeaveTransaction AS T ON t.SystemID=ld.LvTrnsSystemID
                //LEFT OUTER JOIN LeaveType AS L ON L.Id=t.LTSystemID
                //where LT.EmpSystemID='" + EmpSystemID + "' and LD.WorkDate between '" + FromDate + "' and '" + ToDate + "' and LT.IsApproved=1";
                var sql = @"Select LT.EmpSystemID,L.Code, format(LT.FromDate,'dd-MMM-yyyy') FromDate,format(LT.ToDate,'dd-MMM-yyyy') ToDate,LT.LeaveDays,LT.AppliedDate,LT.ApprovedDate
                 from LeaveTransaction LT
                ----Left Outer Join LeaveTransactionDetails LD on LT.SystemID=LD.LvTrnsSystemID
                --LEFT OUTER JOIN LeaveTransaction AS T ON t.SystemID=ld.LvTrnsSystemID
                LEFT OUTER JOIN LeaveType AS L ON L.Id=LT.LTSystemID
                where LT.EmpSystemID='" + EmpSystemID + "' and LT.FromDate between '" + FromDate + "' and '" + ToDate + "' and LT.IsApproved=1";
                var list = _sqlRepository.GetDataTable(sql);

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable loadOpeningBalance(string EmployeeId, string CalanderYearId)
        {
            try
            {
                var sql = @"Select LS.EmployeeId,LS.LeaveTypeId LeaveId,LT.Code LeaveCode, LT.UserName LeaveName,LS.CurrentYearAllocation,0 AS CurrentTransaction from TRN.EmployeeLeaveSummary LS
                           Left Outer Join LeaveType LT on LS.LeaveTypeId=LT.Id
                           where LS.EmployeeId = '" + EmployeeId + "' and LS.CalanderYearId='" + CalanderYearId + "'";
                var list = _sqlRepository.GetDataTable(sql);

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable loadHeader(string EmployeeId)
        {
            try
            {
                var sql = @"select e.EmployeeCode, e.EmployeeName, e.DOJ,l.UserName as Designation
                             from EmployeeInformation AS e
                              left join HKP.LegalDesignation as l on l.Id=e.LegalDesignationId 
                                 where e.SystemId = '" + EmployeeId + "'";
                var list = _sqlRepository.GetDataTable(sql);
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Operation

        public static double dbl(string d)
        {
            return Convert.ToDouble(GetNumericData(d));

        }
        private string GetPK()
        {
            return GetAutoNumber(nameof(EmployeeInformation), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        public static string GetNumericData(string strNumber)
        {
            double d;
            strNumber = strNumber.Replace(",", "");
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Trim() == "")
            { return "0"; }
            else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
            {
                return strNumber;
            }
            else
            {
                return "0";
            }
        }// end function

        public IEnumerable<object> GetClanderYear(string plantId)
        {
            try
            {
                string sqlText = @"SELECT Id, YearNo,
                                    format(FromDate,'dd-MMM-yyyy') AS FromDate,
                                     format(ToDate,'dd-MMM-yyyy') AS ToDate
                                       FROM dbo.YearlyCalendar
                                         WHERE PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(sqlText, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void generateReport(string CalanderYearId, string FromDate, string ToDate, string plantId, string empID, string reportType, string tempId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = 7;
                var lang = GetLanguage(plantId, tempId, reportType);

                if (lang.Count > 0)
                {
                    var dtLangId = getLanguageId(lang["Language"].ToString()); //getLanguageId
                    langID = dtLangId.Rows[0]["Id"].ToString();
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                }
                else
                {
                    langID = tempId;
                    var dtLangName = getLanguageName(langID);
                    tempId = dtLangName.Rows[0]["UserName"].ToString();
                    fileName = "Lvr" + plantId + tempId + ".docx";
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName); // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File <" + fileName + "> Not Found.");
                    }
                }
                //------
                fileName = "Lvr" + plantId + tempId + ".xlsx";
                strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName); // IDCardEng.xlsx
                File = fileName;
                if (!System.IO.File.Exists(strPath))
                {
                    throw new CustomException("File <" + fileName + "> Not Found.");
                }

                ///-----
                var Templatefile = GetFilePath(plantId, tempId, reportType);
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

                string filepath = "";
                if (System.IO.File.Exists(strPath) && tempId != "English")
                {
                    filepath = strPath;
                }
                else
                {
                    File = "Lvr" + plantId + "English.docx";
                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }
                //Creates a new instance for ExcelEngine
                ExcelEngine excelEngine = new ExcelEngine();

                //Loads or open an existing workbook through Open method of IWorkbooks
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(filepath);

                IWorksheet sheet = workbook.Worksheets[0];
                sheet.ShowColumn(0, true);
                DataTable dsBf, dsTransaction, dsBalance, dsHeader;

                //clsDataContext data = new clsDataContext();
                dsBf = loadBf(empID, CalanderYearId);
                dsTransaction = loadLeaveTransactions(empID, FromDate, ToDate);
                dsBalance = loadOpeningBalance(empID, CalanderYearId);
                dsHeader = GetEmployeeBasicInfoById(empID, plantId, "Permanent", langID, tempId);

                IRange range = sheet.UsedRange;
                IRange columnList = range.Rows[0]; //IRange columnList = range.Rows[5];
                int columnListRow = 1;
                int ColumnTemplateRow = 1;
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
                }



                string PrefixOpeningBalance = "OB";
                string PrefixCurrentTransaction = "LT";
                string PrefixClosingBalance = "CB";

                #region EmployeeInformation 

                for (int R = 0; R < columnListRow; R++)
                {
                    IRange columnListEmp = range.Rows[R];
                    foreach (DataColumn item in dsHeader.Columns)
                    {
                        for (int i = 0; i < range.Rows[R].Cells.Count(); i++)
                        {
                            if (string.IsNullOrEmpty(sheet[R + 1, i + 1].Text))
                                continue;

                            if (sheet[R + 1, i + 1].Text.ToUpper().Trim() == "{" + item.ColumnName.ToUpper() + "}")
                            {
                                //sheet[R + 1, i + 1].Text = dsHeader.Rows[0][item.ColumnName].ToString();
                                if (bplib.clsWebLib.IsNumeric(dsHeader.Rows[0][item.ColumnName].ToString()))
                                    sheet[R + 1, i + 1].Text = cnDgt(dsHeader.Rows[0][item.ColumnName].ToString(), tempId);
                                else if (bplib.clsWebLib.IsDateOK(dsHeader.Rows[0][item.ColumnName].ToString()))
                                    sheet[R + 1, i + 1].Text = GetFormatedDate(dsHeader.Rows[0][item.ColumnName].ToString(), tempId);
                                else
                                    sheet[R + 1, i + 1].Text = dsHeader.Rows[0][item.ColumnName].ToString();
                            }
                        }
                    }
                }

                #endregion
                int RefROW = ColumnTemplateRow;
                int ROW = ColumnTemplateRow + 1; int COL = 1;
                for (int T = 0; T < dsTransaction.Rows.Count; T++)
                {
                    sheet[ROW, 2].Number = (T + 1);

                    foreach (DataRow item in dsBalance.Rows)
                        item["CurrentTransaction"] = 0;

                    for (int CELL = 0; CELL < columnList.Cells.Count(); CELL++)
                    {
                        string cellValue = columnList.Cells[CELL].Text;
                        if (string.IsNullOrEmpty(cellValue))
                            continue;

                        if (cellValue.ToUpper() == "{BF}")
                        {
                            if (dsBf.Rows.Count > 0)
                            {
                                if (string.IsNullOrEmpty(dsBf.Rows[0]["BroughtForward"].ToString()))
                                {
                                    sheet[ROW, (CELL + 1)].Number = 0;
                                    //sheet[ROW, (CELL + 1)].NumberFormat = "###0;";
                                }
                                else
                                {
                                    sheet[ROW, (CELL + 1)].Number = Convert.ToInt32(dsBf.Rows[0]["BroughtForward"].ToString());
                                    //sheet[ROW, (CELL + 1)].NumberFormat = "###0;";
                                }
                            }
                        }

                        if (cellValue.ToUpper().Contains("DATE"))
                        {
                            if (dsTransaction.Columns.Contains(cellValue.Replace("{", "").Replace("}", "")))
                            {
                                sheet[ROW, (CELL + 1)].NumberFormat = "@";
                                sheet[ROW, (CELL + 1)].Text = GetFormatedDate(Convert.ToDateTime(dsTransaction.Rows[T][cellValue.Replace("{", "").Replace("}", "")].ToString()).ToString("dd-MMM-yyyy"), tempId);
                            }
                        }

                        for (int OB = 0; OB < dsBalance.Rows.Count; OB++)
                        {
                            string leaveTypeOB = "{" + PrefixOpeningBalance + dsBalance.Rows[OB]["LeaveCode"].ToString() + "}";
                            if (leaveTypeOB.ToUpper() == cellValue.ToUpper())
                            {
                                sheet[ROW, CELL + 1].Number = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString());
                            }
                        }

                        string leaveTypeCL = "{" + PrefixCurrentTransaction + dsTransaction.Rows[T]["Code"].ToString() + "}";
                        if (leaveTypeCL.ToUpper() == cellValue.ToUpper())
                        {
                            sheet[ROW, CELL + 1].Number = dbl(dsTransaction.Rows[T]["LeaveDays"].ToString());

                            dsBalance.DefaultView.RowFilter = "LeaveCode='" + dsTransaction.Rows[T]["Code"].ToString() + "'";
                            if (dsBalance.DefaultView.Count > 0)
                            {
                                dsBalance.DefaultView[0]["CurrentTransaction"] = dbl(dsTransaction.Rows[T]["LeaveDays"].ToString());
                                dsBalance.DefaultView.RowFilter = null;
                                dsBalance.AcceptChanges();
                            }
                        }


                        for (int OB = 0; OB < dsBalance.Rows.Count; OB++)
                        {
                            string leaveTypeOB = "{" + PrefixClosingBalance + dsBalance.Rows[OB]["LeaveCode"].ToString() + "}";
                            if (leaveTypeOB.ToUpper() == cellValue.ToUpper())
                            {
                                dsBalance.Rows[OB]["CurrentYearAllocation"] = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString())
                                - dbl(dsBalance.Rows[OB]["CurrentTransaction"].ToString());

                                sheet[ROW, CELL + 1].Number = dbl(dsBalance.Rows[OB]["CurrentYearAllocation"].ToString());
                            }
                        }
                    }
                    ROW++;
                }

                sheet.DeleteRow(RefROW);
                sheet.HideColumn(1);
                string fileNames = string.Empty;
                if (!string.IsNullOrEmpty(dsHeader.Rows[0]["EmployeeCode"].ToString()))
                {

                    fileNames = dsHeader.Rows[0]["EmployeeCode"].ToString() + "-LeaveRegister-" + Convert.ToDateTime(FromDate).Year + ".xlsx";

                }
                else
                {
                    fileNames = "-LeaveRegister.xlsx";
                }
                workbook.SaveAs(fileNames, System.Web.HttpContext.Current.Response, ExcelDownloadType.Open);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private DataTable GetCurrentClanderYear(string plantId)
        {
            try
            {
                var sql = @"SELECT Id, YearNo,
                                    format(FromDate,'dd-MMM-yyyy') AS FromDate,
                                     format(ToDate,'dd-MMM-yyyy') AS ToDate
                                       FROM dbo.YearlyCalendar
                                         WHERE PlantId='" + plantId + @"'";
                var list = _sqlRepository.GetDataTable(sql);

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable MediasoftFairShopDataExport()
        {
            try
            {

                string sql = @"Select SystemId EmpID, EmployeeCode [RF Card No], c.UserName [Staff Type], 
                                l.UserName Designation, 
                                lk.Name [Designation Bangla], 
                                --d.UserName Designation, 
                                --lc.Name GivenDesignationId, 
                                dp.UserName Department, 
                                ld.Name [Department Bangla], EmployeeName Name, EmployeeNameLocal [Name Bangla], CellPhnNo Phone, EmailId Email, 
                                u.UserName Unit,'0' [FPS Enrollment],
                                (NoOfChildren+1) [Family Members],'2500' [Credit Limit], case when EmployeeStatus='Active' then 'Y' else 'N' end IsActive,NULL SpouseId From EmployeeInformation e
                                left join HKP.EmployeeCategory c on e.EmployeeCategorySystemID = c.Id
                                left join Hkp.LegalDesignation l on e.LegalDesignationId = l.Id
                                left join Hkp.Designation d on e.GivenDesignationId = d.Id
                                left join Org.Department dp on e.DepartmentId = dp.Id
                                left join Org.Unit u on e.UnitId = u.Id
                                left join hkp.LocalLanguage lc on e.GivenDesignationId = lc.DesignationId
                                left join hkp.LocalLanguage lk on e.LegalDesignationId = lk.LegalDesignationId
                                left join hkp.LocalLanguage ld on e.DepartmentId = ld.DepartmentId";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }









        //Service
        public IEnumerable<object> GetListForInActive(string plantId, string CompanyId)
        {
            try
            {



                var Sql = @"--DECLARE @plantId VARCHAR(10)= '20188';
                SELECT EI.SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                        , EI.EmployeeName, Format(EI.DOB,'dd-MMM-yyyy')DOB, EI.EmployeeStatus,FORMAT(EI.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(EI.DOS,'dd-MMM-yyyy')DOS,ld.UserName as LDname,DEG.UserName AS[DesignationName], MB.EntityId,PR.UserName PositionName
                       , DEG.UserName GivenDesignation, DEPT.UserName Department,EI.EmpPicPath
                        FROM dbo.EmployeeInformation AS EI
                        LEFT JOIN HKP.Designation AS DEG ON DEG.Id = EI.DesignationSystemID
						left join [HKP].[LegalDesignation] as ld on  ld.Id=ei.LegalDesignationId
                        --LEFT JOIN HKP.Designation AS DEG ON DEG.Id = EI.DesignationSystemID
                        LEFT JOIN[MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
                       LEFT OUTER JOIN ORG.Position PR ON MB.PositionId= PR.Id

                       LEFT OUTER JOIN ORG.Entity E ON MB.EntityId= E.Id

                       LEFT OUTER JOIN ORG.Department DEPT ON PR.DepartmentId= DEPT.Id

                       WHERE EI.CompanyId= '" + CompanyId + "'AND EI.PlantId= '" + plantId + "' AND EI.EmployeeStatus= 'Separated'  and EI.dos>=DATEADD(month,-6,GETDATE()) order by EI.dos desc ";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        //
        public IEnumerable<object> GetListForActive(string plantId, string CompanyId)
        {
            try
            {
                var Sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        SELECT EI.SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName,ei.EmpPicPath
                        , EI.EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [DesignationName], MB.EntityId,PR.UserName PositionName
                        , DEG.UserName GivenDesignation,DEPT.UserName Department
                        FROM dbo.EmployeeInformation AS EI
                        LEFT JOIN HKP.Designation AS DEG ON DEG.Id=EI.DesignationSystemID
                        LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
                        LEFT OUTER JOIN ORG.Position PR ON MB.PositionId=PR.Id
                        LEFT OUTER JOIN ORG.Entity E ON MB.EntityId=E.Id
                        LEFT OUTER JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                        WHERE EI.CompanyId='" + CompanyId + "' AND EI.PlantId='" + plantId + "' AND EI.EmployeeStatus='Active'";
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }



        public void InActiveToActive(string SystemId,string reason)
        {

            try
            {


          

                // PoValue = "0";
                var Id = GetPK();

                //var Status = "Active";
                var UpdatedBy = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var AddedFromIp = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                var EmployeeId = SystemId;
                var Reason = reason;

                DataTable dtFNF = GetFNFEmployee(SystemId);
                if (dtFNF.Rows.Count > 0)
                {
                    throw new Exception("Full and Final Settlement Employee can't be reactive.");
                }
                //Lock
                DataTable dt = GetEffectiveDateForAttdn(SystemId);
                DateTime FromDate = Convert.ToDateTime(dt.Rows[0]["ApprovedEffectiveDate"].ToString());
                FromDate = FromDate.AddDays(1);

                AttendanceProcessAplos ob = new AttendanceProcessAplos();
                //ob.LockValidation(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), DateTime.Now.ToString("dd-MMM-yyyy"), SystemId);

                if (reason == null)
                {
                    throw new CustomException("Please Enter Reason",Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,ErrorType.ServiceError, null, "", "", false, ModuleEnum.Product.ToString()));
                }
                else
                {
                    string _sql = "Update dbo.EmployeeInformation set DOS=null,DOSBy=null,DOSDate=null,EmployeeStatus='Active' where SystemId='" + SystemId + "'";
                    _sqlRepository.ExecuteSqlCommand(_sql);
                    string _sql1 = "Insert into EmployeeReactivation(Id," +
                    "CompanyGroupId," +
                    "CompanyId," +
                    "PlantId," +
                    "EmployeeId," +
                    "Reason," +
                    "AddedBy," +
                    "AddedDate," +
                    "AddedFromIp," +
                    "UpdatedBy," +
                    "UpdatedDate," +
                    "UpdatedFromIp) " +
                    "values ('" + Id + "'," +
                    "'" + CompanyGroupId + "'," +
                    "'" + CompanyId + "'," +
                    "'" + PlantId + "'," +
                    "'" + SystemId + "'," +
                     "'" + Reason + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                    "'" + AddedFromIp + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                      "'" + AddedFromIp + "')";
                    _sqlRepository.ExecuteSqlCommand(_sql1);
                    #region Attendance process
                    clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                    //DataTable dt = GetEffectiveDateForAttdn(SystemId);
                 
                    //DateTime FromDate = Convert.ToDateTime(dt.Rows[0]["ApprovedEffectiveDate"].ToString());
                    DateTime ToDate = DateTime.Now;
                    while (FromDate <= ToDate)
                    {
                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                        obj.SaveTotal(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), SystemId, false,true);//Main Function for attendace Process
                        FromDate = FromDate.AddDays(1);
                    }


                    #endregion
                }


            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public DataTable GetFNFEmployee(string EmpSystemId)
        {
            try
            {

                string sql = @"select * from EmployeeFullAndFinalSettlement Where EmpSystemId='" + EmpSystemId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable GetEffectiveDateForAttdn(string EmpSystemId)
        {
            try
            {

                string sql = @"SELECT top 1 FORMAT(ApprovedEffectiveDate,'dd-MMM-yyyy')  ApprovedEffectiveDate                                   
                                    FROM [TRN].[Resignation]
                                    where EmployeeId='" + EmpSystemId + "' order by AddedDate desc";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public void ActiveToInActive(string SystemId)
        //{
        //    try
        //    {
        //        //PoValue = "0";
        //        var Id = GetPK();

        //        var Status = "Separated";
        //        var UpdatedBy = "";
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        var ip = identity.IPAddress;
        //        var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
        //        var AddedBy = identity.Name;
        //        var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
        //        var CompanyGroupId = identity.CompanyGroupId;
        //        var CompanyId = identity.CompanyId;
        //        var PlantId = identity.PlantId;
        //        // var res =_inventoryReceiveRepository.SqlQuery<Int32>($"select distinct statusflag=Case when B.POId is not null then 1 else 0 end	from trn.PurchaseOrder A	Left 

        //        //JOIN trn.InventoryReceiveDetail B on B.POID = A.Id where A.Id = '"+PoId+"'").First();               

        //        //if(Convert.ToBoolean(res))
        //        // {
        //        //     throw new CustomException("You can not un Approved the PO? GRN already Received");
        //        // }
        //        // else
        //        // {
        //        string _sql = "Update dbo.EmployeeInformation set DOS=null,DOSBy=null,DOSDate=null,EmployeeStatus='Separated' where SystemId='" + SystemId + "'";
        //        _sqlRepository.ExecuteSqlCommand(_sql);
        //        //string _sql1 = "Insert into TRN.PurchaseOrderApprovalLog(Id," +
        //        //"CompanyGroupId," +
        //        //"CompanyId," +
        //        //"PlantId," +
        //        //"ApprovedBy," +
        //        //"Date," +
        //        //"POValue," +
        //        //"Status," +
        //        //"AddedBy," +
        //        //"AddedDate," +
        //        //"AddedFromIp," +
        //        //"UpdatedBy," +
        //        //"UpdatedDate," +
        //        //"UpdatedFromIp,POID) " +
        //        //"values ('" + Id + "'," +
        //        //"'" + CompanyGroupId + "'," +
        //        //"'" + CompanyId + "'," +
        //        //"'" + PlantId + "'," +
        //        //"'" + AddedBy + "'," +
        //        //"'" + AddedDate + "'," +
        //        //"'" + PoValue + "'," +
        //        //"'" + Status + "'," +
        //        //"'" + AddedBy + "'," +
        //        //"'" + AddedDate + "'," +
        //        //"'" + ip + "'," +
        //        //"'" + UpdatedBy + "'," +
        //        //"'" + updatedDate + "', " +
        //        //"'" + ip + "','" + PoId + "')";
        //        //_sqlRepository.ExecuteSqlCommand(_sql1);
        //        // }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
        //    }
        //}

        //


    }
}

    

      

    
