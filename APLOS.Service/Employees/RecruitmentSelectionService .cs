#region Using

using Library.Core;
using Library.Crosscutting;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Addresses;
using Library.Model.Employees;
using Library.Model.Organizations;
using Library.Model.Setups;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class RecruitmentSelectionService : Service<PreRecruitmentEmployee>, IRecruitmentSelectionService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<PreRecruitmentEmployee> _preRecruitmentEmployeeRepository;
        private readonly IRepositoryAsync<PrerecruitmentUrl> _prerecruitmentUrlRepository;
        private readonly IRepositoryAsync<Plant> _plantRepository;
        private readonly IRepositoryAsync<SMTPConfiguration> _smtpConfigurationRepository;
        private readonly IManpowerBudgetService _manpowerBudgetService;
        private readonly IPreRecruitmentDocumentService _preRecruitmentDocumentService;

        public RecruitmentSelectionService(
            IRepositoryAsync<PreRecruitmentEmployee> preRecruitmentEmployeeRepository
            , IRepositoryAsync<PrerecruitmentUrl> prerecruitmentUrlRepository
            , IRepositoryAsync<Plant> plantRepository
            , IRepositoryAsync<SMTPConfiguration> smtpConfigurationRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IManpowerBudgetService manpowerBudgetService
            , IPreRecruitmentDocumentService preRecruitmentDocumentService
            ) : base(preRecruitmentEmployeeRepository, unitOfWork, pkGeneratorService)
        {
            _preRecruitmentEmployeeRepository = preRecruitmentEmployeeRepository;
            _prerecruitmentUrlRepository = prerecruitmentUrlRepository;
            _plantRepository = plantRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _manpowerBudgetService = manpowerBudgetService;
            _preRecruitmentDocumentService = preRecruitmentDocumentService;
            _smtpConfigurationRepository = smtpConfigurationRepository;
        }

        #endregion Constructor

        #region Operation

        public GridModel GetData(GridParameter parameters, string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"Select PRE.*,0 Active,E.UserName EntityName,D.UserName Designation,PR.UserName PositionName,DEG.UserName GivenDesignation, DEPT.UserName AS Department
							FROM PreRecruitmentEmployee PRE
							LEFT OUTER JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id
							LEFT OUTER JOIN ORG.Position PR ON PMB.PositionId=PR.Id
							LEFT OUTER JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
							LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id
							LEFT OUTER JOIN HKP.Designation D ON PR.DesignationId=D.Id
							LEFT OUTER JOIN HKP.Designation DEG on DEG.Id=PRE.GivenDesignationId
							Where PRE.GroupID='" + identity.CompanyGroupId + @"' AND PRE.CompanyId='" + identity.CompanyId + @"' AND PRE.PlantId='" + plantId + "' AND isnull(PRE.ReadyForCandidateAccess,0) = 0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetAppData(GridParameter parameters, string plantId, string fd, string td)
        {
            try
            {
                parameters.CmdText = @"SELECT
                                 PRE.Id,PRE.PlantId,PRE.GroupID,PRE.CompanyId,PRE.FullName
                            	,Replace(CONVERT(VARCHAR(11), PRE.DOB, 106), ' ', '-') DOB
                                ,Replace(CONVERT(VARCHAR(11), PRE.AddedDate, 106), ' ', '-') AddedDate
                            	,PRE.BudgetId,PMB.Code,PR.UserName PositionName
                                ,PRE.[SelectionStatus],PRE.[ConfirmationStatus]
                                ,Replace(CONVERT(VARCHAR(11), PRE.AgreedDOJ, 106), ' ', '-') AgreedDOJ
                                ,Replace(CONVERT(VARCHAR(11), PRE.AppAddedDateTime, 106), ' ', '-') AppAddedDateTime
								,PRE.Gender,PRE.Phone,PRE.Email,PRE.EmpType,PRE.Status,PRE.NationalID
								,PRE.TotalSalary,PRE.SpecialReviewAmount,PRE.SpecialReviewDuration
								,PRE.InterviewRankingId,PRE.GivenDesignationId,E.UserName EntityName
								,D.UserName Designation,DG.UserName GivenDesignation
                                ,PR.DesignationId,PRE.IsExceptionalDesigApplicable
								,CNT.PhoneLength ,CNT.TINCaption,CNT.NIDCaption, CNT.NIDLength, CNT.TINLength
								,COM.TINRequiredForSalaryAbove
                            	FROM PreRecruitmentEmployee PRE
                            LEFT JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                            LEFT JOIN HKP.Designation DG ON PRE.GivenDesignationId=DG.Id
							LEFT JOIN ORG.Plant PL ON PRE.PlantId = PL.Id
							LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
							LEFT JOIN SCS.Country CNT ON AM.CountryId=CNT.Id
							LEFT JOIN ORG.Company COM ON PRE.CompanyId=COM.Id
                         WHERE PRE.PlantId='" + plantId + @"'
                         AND (SELECT cast(PRE.AppAddedDateTime as date)) between  '" + fd + "' AND '" + td + @"'
                         AND ISNULL(PRE.ReadyForCandidateAccess,0) = 0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetBudgetCodeList(GridParameter parameters, string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = _manpowerBudgetService.GetManpowerBudgetListSql(identity.CompanyGroupId, identity.CompanyId, plantId);
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public PreRecruitmentEmployee GetMaster(string PK)
        {
            string _sql = "SELECT * FROM PreRecruitmentEmployee WHERE Id='" + PK + "'";
            return _preRecruitmentEmployeeRepository.SelectQuery(_sql, null).FirstOrDefault();
        }

        public void InsertORUpdateMaster(EmailSetup emailSetup, IEnumerable<PreRecruitmentEmployee> entities, string companyId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                if (entities == null || entities.Count() == 0)
                    throw new CustomException("Recipent can not be null.");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var pks = entities.Select(t => t.Id);
                var empType = entities.Select(t => t.EmpType).FirstOrDefault();
                var plantId = entities.Select(t => t.PlantId).FirstOrDefault();

                var plantName = _plantRepository.Query(t => t.Id == plantId).Select(t => t.UserName).FirstOrDefault();
                var url = _prerecruitmentUrlRepository.Query(t => t.CompanyId == companyId && t.PlantId == plantId).Select(t => t.Url).FirstOrDefault();

                if (string.IsNullOrEmpty(url)) throw new CustomException("URL can not be null.");
                var from_db = Query(t => pks.Contains(t.Id)).Select().AsEnumerable();
                var docdb = new List<PreRecruitmentDocument>();
                foreach (var item in entities)
                {
                    if (item.Active)
                    {
                        if (!from_db.Any(t => t.Id == item.Id))
                            throw new CustomException(ServiceResources.RecordNoLonger.ToString());
                        if (item.SelectionStatus == SelectionStatus.Selected.ToString())
                        {
                            var ob = new PreRecruitmentDocument();
                            item.SelectionDateTime = DateTime.Now;
                            item.SelectedBy = identity.Name;
                            item.InitialPIN = new Random().Next(111111, 999999).ToString();
                            item.ReadyForCandidateAccess = true;
                            ob.GivenDesignationId = item.GivenDesignationId;
                            ob.PreRecruitmentEmployeeId = item.Id;
                            docdb.Add(ob);
                        }
                        UpdateGraph(item);
                    }
                }
                _preRecruitmentDocumentService.SaveDocumentList(plantId, empType, docdb);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                var selectedEmp = entities.Where(t => t.SelectionStatus == SelectionStatus.Selected.ToString()).Select(t => t.Id);
                var dbData = Query(a => selectedEmp.Contains(a.Id)).Include(r => r.GivenDesignation).Select().AsEnumerable();
                var dom = _smtpConfigurationRepository.Query(a => a.CompanyGroupId == identity.CompanyGroupId && a.CompanyId == identity.CompanyId).Select().FirstOrDefault();
                if (dom == null)
                    throw new CustomException("This 'company group' has no web address!");
                var cc = emailSetup.CC ?? "";
                emailSetup.Url = url;

                foreach (var item in dbData)
                {
                    var designationName = item.GivenDesignation.UserName;
                    var cgMessage = "You have been selected as " + designationName + " at " + plantName + ".";
                    //emailSetup.Subject = @"Congratulations! " + cgMessage;
                    emailSetup.Subject = "" + plantName + " recruitment system";
                    emailSetup.Message = "";
                    var date = item.SelectionDateTime.Value.Date.AddDays(item.ExpiredDays + 1).ToString("dd-MMM-yyyy");
                    emailSetup.Message = @"Dear " + item.FullName + @",<br /><br /><b>Congratulations!</b><br /><br />"
                        + cgMessage + "<br />Please update your profile by " + date + ", using the following information.<br/>"
                        + "URL: " + emailSetup.Url + "/prerecruitment"
                        + "<br/>ID : " + item.Id + " <br/>OTP : " + item.InitialPIN
                        + "<br/><b>N.B.</b> Google Chrome is recommended browser.";
                    var em = new EmailSender(dom.Host, dom.Port, dom.MailingUserName, dom.Password, dom.IsSSL);
                  em.Send(emailSetup.SenderName + " <" + emailSetup.SenderEmail + ">", item.Email, cc, emailSetup.Subject, emailSetup.Message);


                    //SmtpClient client = new SmtpClient();
                    //client.Port = 587;
                    //client.Host = "cedaartextile.com";
                    //client.EnableSsl = true;
                    //client.Timeout = 100000;
                    //client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //client.UseDefaultCredentials = false;


                    //client.Credentials = new System.Net.NetworkCredential(dom.MailingUserName, dom.Password);
                    //System.Net.Mail.MailMessage reportEmail = new System.Net.Mail.MailMessage(dom.MailingUserName, emailSetup.SenderEmail, emailSetup.Subject, emailSetup.Message);
                    //reportEmail.BodyEncoding = UTF8Encoding.UTF8;
                    //reportEmail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                    //reportEmail.IsBodyHtml = true;
                    //client.Send(reportEmail);

                }
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

        public GridModel GetCbo()
        {
            try
            {
                var sql = @"Select I.Id AS [Value], I.UserName AS [Text] From HKP.InterviewRanking AS I";

                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public IEnumerable<PreRecruitmentEmployee> GetMasterlist(string PKs)
        {
            try
            {
                string _sql = "SELECT * FROM PreRecruitmentEmployee WHERE Id IN (" + PKs + ")";
                return _preRecruitmentEmployeeRepository.SqlQuery<PreRecruitmentEmployee>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GetPKList(IEnumerable<PreRecruitmentEmployee> list, out string masterid)
        {
            masterid = string.Empty;
            try
            {
                foreach (var item in list)
                {
                    if (string.IsNullOrEmpty(masterid))
                    {
                        masterid = "'" + item.Id + "'";
                    }
                    else
                    {
                        masterid += ",'" + item.Id + "'";
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region AppData

        public IEnumerable<object> GetCbo(string companyGroupId)
        {
            try
            {
                var sql = @"SELECT Id [Value], UserName [Text] FROM MST.OperationMaster WHERE CompanyGroupId='"+ companyGroupId + "' ORDER BY UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(PreRecruitmentEmployee), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public override void Insert(PreRecruitmentEmployee entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (entity.DOB != null)
                {
                    DateTime dtDOB = Convert.ToDateTime(entity.DOB);
                    DateTime dtDOJ = Convert.ToDateTime(entity.AgreedDOJ);
                    TimeSpan ts1 = dtDOJ - dtDOB;
                    int days1 = ts1.Days;
                    if (days1 <= 6573)
                    {
                        Exception ex = new Exception("This candidate is below 18 years...");
                        throw (ex);
                    }
                }
                entity.Id = GetPK();
                entity.AppAddedBy = identity.Name;
                entity.AppAddedDateTime = DateTime.Now;
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(PreRecruitmentEmployee entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                entity.AppUpdatedBy = identity.Name;
                entity.AppUpdatedDateTime = DateTime.Now;
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion AppData

        #endregion Operation
    }
}