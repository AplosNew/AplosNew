#region Using

using Library.Core;
using Library.Crosscutting;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Documents;
using Library.Model.Employees;
using Library.Model.Enums;
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
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class EmployeeDocumentService : Service<EmployeeDocument>, IEmployeeDocumentService
    {
        #region Constructor

        private readonly ISignatureService _signatrueService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<CompanyGroup> _companyGroupRepository;
        private readonly ISMTPConfigurationService _smtpConfigurationService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IRepositoryAsync<Company> _companyRepository;
        private readonly IRepositoryAsync<ComplianceDocument> _complianceDocumentRepository;
        private readonly IRepositoryAsync<EmployeeInformation> _employeeInformationRepository;
        private readonly IRepositoryAsync<MailReceiverServiceMapping> _mailReceiverServiceMappingRepository;
        private readonly IRepositoryAsync<MailReceiverDetail> _mailReceiverDetailRepository;
        private readonly IRepositoryAsync<MailLog> _mailLogRepository;
        private readonly IRepositoryAsync<Resignation> _resignationRepository;
        private readonly IRepositoryAsync<EmployeeDocument> _employeeDocumentRepository;

        public EmployeeDocumentService(
              IRepositoryAsync<EmployeeDocument> employeeDocumentRepository
            , ISignatureService signatrueService
            , ISMTPConfigurationService smtpConfigurationService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<CompanyGroup> companyGroupRepository
            , IRepositoryAsync<Company> companyRepository
            , IRepositoryAsync<ComplianceDocument> complianceDocumentRepository
            , IRepositoryAsync<EmployeeInformation> employeeInformationRepository
            , IRepositoryAsync<MailReceiverDetail> mailReceiverDetailRepository
            , IRepositoryAsync<MailReceiverServiceMapping> mailReceiverServiceMappingRepository
            , IRepositoryAsync<MailLog> mailLogRepository
            , IRepositoryAsync<Resignation> resignationRepository
            ) : base(employeeDocumentRepository, unitOfWork, pkGeneratorService)
        {
            _employeeDocumentRepository = employeeDocumentRepository;
            _unitOfWork = unitOfWork;
            _signatrueService = signatrueService;
            _smtpConfigurationService = smtpConfigurationService;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _sqlRepository = sqlRepository;
            _companyGroupRepository = companyGroupRepository;
            _companyRepository = companyRepository;
            _complianceDocumentRepository = complianceDocumentRepository;
            _employeeInformationRepository = employeeInformationRepository;
            _mailReceiverDetailRepository = mailReceiverDetailRepository;
            _mailReceiverServiceMappingRepository = mailReceiverServiceMappingRepository;
            _mailLogRepository = mailLogRepository;
            _resignationRepository = resignationRepository;
        }

        #endregion Constructor

        private List<MailViewModel> GetAdministrativeMailList()
        {
            var sql = @"SELECT MRD.Id, MRD.UserId, MRD.MailType, ISNULL(U.FullName,MRD.FullName) AS FullName, MRD.Email AS Email, ISNULL(U.Active, CONVERT(BIT, 1)) AS Active  FROM [SCS].[MailReceiverDetail] AS MRD
						LEFT JOIN [SEC].[User] AS U ON U.Id=MRD.UserId
						JOIN [SCS].[MailReceiver] AS MR ON MR.Id = MRD.MailReceiverId
                        WHERE MR.Active = 1 AND MR.MailReceipientType = 'Admin'";
            return _mailReceiverDetailRepository.SqlQuery<MailViewModel>(sql).ToList();
        }

        private List<MailViewModel> GetNormalMaileList(MailReceiverServiceMapping item)
        {
            var sql = @"SELECT MRD.Id, MRD.UserId, MRD.MailType, ISNULL(U.FullName, MRD.FullName) AS FullName, MRD.Email AS Email, ISNULL(U.Active, CONVERT(BIT, 1)) AS Active  FROM [SCS].[MailReceiverDetail] AS MRD
						LEFT JOIN [SEC].[User] AS U ON U.Id=MRD.UserId
						JOIN [SCS].[MailReceiver] AS MR ON MR.Id = MRD.MailReceiverId
                        WHERE MRD.MailReceiverId='" + item.MailReceiverId + "' and MR.Active = 1";
            return _mailReceiverDetailRepository.SqlQuery<MailViewModel>(sql).ToList();
        }

        public void DueDocumentProcess()
        {
            var sql = @"UPDATE EmployeeDocument
						SET DueProcessDateTime = CASE WHEN ED.DueProcessDateTime IS NULL THEN GETDATE() ELSE ED.DueProcessDateTime END,
						 IsMailSend = CASE WHEN ED.DueProcessDateTime IS NULL THEN 1 ELSE ED.IsMailSend END,
					  DueDate =
		              CASE
		              WHEN CD.DependateDate='AsAndWhen' THEN NULL
		              WHEN CD.DependateDate='AppointmentDate' THEN
		                  CASE WHEN E.PreRecruitmentEmployeeId IS NULL AND E.DOJ IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOJ) ELSE
		                      CASE WHEN PRE.ApprovedDateTime IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, PRE.ApprovedDateTime)
		                      END
		                  END
		              WHEN CD.DependateDate='AgreedJoinDate' THEN
		                  CASE WHEN E.PreRecruitmentEmployeeId IS NULL AND E.DOJ IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOJ) ELSE
		                      CASE WHEN PRE.AgreedDOJ IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, PRE.AgreedDOJ)
		                      END
		                  END
		              WHEN CD.DependateDate='ResignationApplyDate' THEN (SELECT TOP(1) CASE WHEN ResignationDate<>'' THEN DATEADD(DAY, CD.LeadOrLagDays,ResignationDate)
		                                                                  ELSE NULL END FROM TRN.Resignation WHERE EmployeeId=E.SystemId  ORDER BY ResignationDate DESC)
		              WHEN CD.DependateDate='ApprovedResignationEffectiveDate' THEN (SELECT TOP(1) CASE WHEN ApprovedEffectiveDate<>'' THEN DATEADD(DAY, CD.LeadOrLagDays,ApprovedEffectiveDate)
		                                                                  ELSE NULL END FROM TRN.Resignation WHERE EmployeeId=E.SystemId  ORDER BY ApprovedEffectiveDate DESC)
		              WHEN CD.DependateDate='JoiningDate' AND E.DOJ IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOJ)
		              WHEN CD.DependateDate='LetterOfIndentDate' THEN
		                  CASE WHEN E.PreRecruitmentEmployeeId IS NULL AND E.DOJ IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOJ) ELSE
		                      CASE WHEN PRE.SelectionDateTime IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, PRE.SelectionDateTime)
		                      END
		                  END
		              WHEN CD.DependateDate='ProfileSubmit' THEN
		                  CASE WHEN E.PreRecruitmentEmployeeId IS NULL AND E.DOJ IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOJ) ELSE
		                      CASE WHEN PRE.SelectionDateTime IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, PRE.SelectionDateTime)
		                      END
		                  END
		              WHEN CD.DependateDate='ProbitionPeriodConfirmationDate' AND E.ProbationConfirmEntryDate IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.ProbationConfirmEntryDate)
		              WHEN CD.DependateDate='PromotionDate' THEN NULL
		              WHEN CD.DependateDate='SeparationDate' AND E.DOS IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOS)
		              WHEN CD.DependateDate='SelectionDate' THEN
		                  CASE WHEN E.PreRecruitmentEmployeeId IS NULL AND E.DOJ IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOJ) ELSE
		                      CASE WHEN PRE.SelectionDateTime IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, PRE.SelectionDateTime)
		                      END
		                  END
		              END
					FROM EmployeeDocument ED
					JOIN HKP.ComplianceDocument CD ON CD.Id=ED.ComplianceDocumentId
					LEFT JOIN
					(
					    SELECT EI.SystemId, EI.PreRecruitmentEmployeeId, EI.DOJ, EI.DOS, EI.ProbationConfirmEntryDate
					    FROM EmployeeInformation EI
					    WHERE EI.EmployeeStatus='Active'
					) E ON E.SystemId=ED.EmpSystemID
					LEFT JOIN PreRecruitmentEmployee PRE ON PRE.Id=E.PreRecruitmentEmployeeId
					WHERE ED.FileId IS NULL   AND ED.FileName IS NULL";
            sql = @"UPDATE EmployeeDocument
						SET DueProcessDateTime = CASE WHEN ISNULL(ED.DueProcessDateTime,'') = '' THEN GETDATE() ELSE ED.DueProcessDateTime END,
						 IsMailSend = CASE WHEN ISNULL(ED.DueProcessDateTime,'') = '' THEN 1 ELSE ED.IsMailSend END,
					  DueDate =
		              CASE
		              WHEN CD.DependateDate='AsAndWhen' THEN NULL
		              WHEN CD.DependateDate='AppointmentDate' THEN
		                  CASE WHEN ISNULL(E.PreRecruitmentEmployeeId,'') = '' AND ISNULL(E.DOJ,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOJ) ELSE
		                      CASE WHEN ISNULL(PRE.ApprovedDateTime,'')<>''  THEN DATEADD(DAY, CD.LeadOrLagDays, PRE.ApprovedDateTime)
		                      END
		                  END
		              WHEN CD.DependateDate='AgreedJoinDate' THEN
		                  CASE WHEN ISNULL(E.PreRecruitmentEmployeeId,'') = '' AND ISNULL(E.DOJ,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOJ) ELSE
		                      CASE WHEN ISNULL(PRE.AgreedDOJ,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, PRE.AgreedDOJ)
		                      END
		                  END
		              WHEN CD.DependateDate='ResignationApplyDate' THEN (SELECT TOP(1) CASE WHEN ISNULL(ResignationDate,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays,ResignationDate)
		                                                                  ELSE NULL END FROM TRN.Resignation WHERE EmployeeId=E.SystemId  ORDER BY ResignationDate DESC)
		              WHEN CD.DependateDate='ApprovedResignationEffectiveDate' THEN (SELECT TOP(1) CASE WHEN ISNULL(ApprovedEffectiveDate,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays,ApprovedEffectiveDate)
		                                                                  ELSE NULL END FROM TRN.Resignation WHERE EmployeeId=E.SystemId  ORDER BY ApprovedEffectiveDate DESC)
		              WHEN CD.DependateDate='JoiningDate' AND ISNULL(E.DOJ,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOJ)
		              WHEN CD.DependateDate='LetterOfIndentDate' THEN
		                  CASE WHEN ISNULL(E.PreRecruitmentEmployeeId,'') = '' AND ISNULL(E.DOJ,'')<>''  THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOJ) ELSE
		                      CASE WHEN ISNULL(PRE.SelectionDateTime,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, PRE.SelectionDateTime)
		                      END
		                  END
		              WHEN CD.DependateDate='ProfileSubmit' THEN
		                  CASE WHEN ISNULL(E.PreRecruitmentEmployeeId,'') = '' AND ISNULL(E.DOJ,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOJ) ELSE
		                      CASE WHEN ISNULL(PRE.SelectionDateTime,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, PRE.SelectionDateTime)
		                      END
		                  END
		              WHEN CD.DependateDate='ProbitionPeriodConfirmationDate' AND ISNULL(E.ProbationConfirmEntryDate,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, E.ProbationConfirmEntryDate)
		              WHEN CD.DependateDate='PromotionDate' THEN NULL
		              WHEN CD.DependateDate='SeparationDate' AND ISNULL(E.DOS,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOS)
		              WHEN CD.DependateDate='SelectionDate' THEN
		                  CASE WHEN ISNULL(E.PreRecruitmentEmployeeId,'')= '' AND ISNULL(E.DOJ,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, E.DOJ) ELSE
		                      CASE WHEN ISNULL(PRE.SelectionDateTime,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, PRE.SelectionDateTime)
		                      END
		                  END
		              END
					FROM EmployeeDocument ED
					JOIN HKP.ComplianceDocument CD ON CD.Id=ED.ComplianceDocumentId
					LEFT JOIN
					(
					    SELECT EI.SystemId, EI.PreRecruitmentEmployeeId, EI.DOJ, EI.DOS, EI.ProbationConfirmEntryDate
					    FROM EmployeeInformation EI
					    WHERE EI.EmployeeStatus='Active'
					) E ON E.SystemId=ED.EmpSystemID
					LEFT JOIN PreRecruitmentEmployee PRE ON PRE.Id=E.PreRecruitmentEmployeeId
					WHERE ISNULL(ED.FileId,'') = ''   AND ISNULL(ED.FileName,'') = ''";
            _employeeDocumentRepository.ExecuteSqlCommand(sql);
        }

        public void DueDocumentMailSendingProcess(string by)
        {
            var log = new MailLog
            {
                AddedBy = "",
                AddedDate = DateTime.Now,
                AddedFromIP = "",
                AppVersion = "",
                ModelState = ModelState.Added,
                RecordTime = DateTime.Now,
                ServiceName = "Employee Wise Documents Due Date Processing.",
                UserId = null,
                AttachmentName = null,
                IsSuccess = false,
                SenderName = null,
                MailGenerator = MailGenerator.Scheduler.ToString()
            };
            try
            {
                var docList = "";
                var toEmail = "";
                var bccEmail = "";
                var companyGroupList = _companyGroupRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                var companyList = _companyRepository.Query(r => r.Active && !r.Archive).Select().ToList();

                foreach (var companyGroup in companyGroupList)
                {
                    log.CompanyGroupId = companyGroup.Id;

                    var smtpConfigurationCG = _smtpConfigurationService.Query(r => r.CompanyGroupId == companyGroup.Id).Select().FirstOrDefault();
                    var email = new EmailSender(smtpConfigurationCG.Host, smtpConfigurationCG.Port, smtpConfigurationCG.MailingUserName, smtpConfigurationCG.Password, true);
                    foreach (var company in companyList.Where(r => r.CompanyGroupId == companyGroup.Id))
                    {
                        var employeeList = _employeeInformationRepository.Query(r => r.GroupID == company.CompanyGroupId && r.CompanyId == company.Id).Select().ToList();
                        var emailList = GetAdministrativeMailList();
                        if (emailList.Count <= 0)
                        {
                            log.CompanyId = company.Id;
                            log.PlantId = null;
                            log.MailReceiverId = null;
                            log.SenderName = null;
                            log.Subject = null;
                            log.IsReciepientListActive = false;
                            log.Remarks = "Reciepient List is not Active";
                        }
                        var ccList = string.Join(";", emailList.Where(r => r.Active && r.MailType == "Cc" && r.Email != string.Empty).Select(r => r.FullName + "<" + r.Email + ">"));
                        log.CcList = ccList;
                        var bccList = string.Join(";", emailList.Where(r => r.Active && r.MailType == "Bcc" && r.Email != string.Empty).Select(r => r.FullName + "<" + r.Email + ">"));
                        log.BccList = bccList;
                        var inActiveList = string.Join(";", emailList.Where(r => !r.Active).Select(r => r.MailType + ":" + r.FullName));
                        var docListbuilder = new StringBuilder();
                        docListbuilder.Append(docList);
                        foreach (var employee in employeeList)
                        {
                            if (!string.IsNullOrEmpty(employee.EmailId))
                            {
                                docList = "<table border=1><tr><th align='left'>Sl</th><th align='left'>Document Name</th><th align='left'>Due Date</th></tr>";

                                var cmdText = @"SELECT ROW_NUMBER() OVER (ORDER BY EDOC.DueDate) AS RowNum,CD.UserName DocName, REPLACE(CONVERT(VARCHAR(11), EDOC.DueDate, 106), ' ', '-') DueDate FROM EmployeeDocument AS EDOC
										  LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = EDOC.ComplianceDocumentId
											 WHERE EmpSystemID = '" + employee.SystemId + @"' AND FileName IS NULL AND IsMailSend=0
											 AND DueDate IS NOT NULL AND CompanyGroupId = '" + employee.GroupID + @"'";
                                var documents = _sqlRepository.GetDataTable(cmdText);

                                for (int i = 0; i < documents.Rows.Count; i++)
                                {
                                    docListbuilder.Append("<tr><td>" + documents.Rows[i]["RowNum"] + "</td><td>" + documents.Rows[i]["DocName"] + "</td><td>" + documents.Rows[i]["DueDate"] + "</td></tr>");
                                }

                                docListbuilder.Append("</table>");

                                var path = GetEmployeeDueDocumentList(employee);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    try
                                    {
                                        toEmail = employee.EmployeeName + "<" + employee.EmailId + ">";

                                        var message = email.PrepareMessage(smtpConfigurationCG.SenderSystemName + "<" + smtpConfigurationCG.SenderSystemEmail + ">", toEmail, ccList, bccList, "To Be Uploaded Documents List", "Dear " + employee.EmployeeName + ",<br><br>You need to submit/upload these documents, the documents are listed below --<br>." + docList + "<br><br>The documents are also listed in the attached Excel Sheet.<br><br>Please see the Attached file below.");
                                        using (var attachment = new Attachment(path))
                                        {
                                            message.Attachments.Add(attachment);
                                        }
                                        email.Send(message);
                                        _employeeInformationRepository.ExecuteSqlCommand(@"UPDATE dbo.EmployeeDocument set IsMailSend = 1
																						   WHERE EmpSystemID ='" + employee.SystemId + @"' AND FileName IS NULL
																						   AND IsMailSend=0
																						   AND DueDate IS NOT NULL;");

                                        if (message != null)
                                        {
                                            log.ToList = toEmail;
                                            log.BccList = bccEmail;
                                            email.Send(message);
                                            log.IsSuccess = true;
                                            log.HasAttachment = false;
                                            log.Remarks = "Documents List Mail of " + employee.EmployeeName + " has been send Successfully";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.IsSuccess = false;
                                        log.HasAttachment = false;
                                        log.Remarks = ex.Message;
                                        continue;
                                    }
                                }
                                else
                                {
                                    log.Remarks = "No data found in Employee Documents";
                                }
                            }
                            else
                            {
                                log.Remarks = "EMail Id not Found in Employee Documents";
                            }
                        }
                        docList = docListbuilder.ToString();
                    }
                }
                _mailLogRepository.Insert(log);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                log.Remarks = ex.Message;
                _mailLogRepository.Insert(log);
                _unitOfWork.SaveChanges();
                throw;
            }
        }

        public void ProcessDocumentDailyOverDue(DateTime processDate, string by, string ip)
        {
            var sql = @"DECLARE @date Date='" + processDate.ToDbDate() + @"';
                        IF EXISTS (SELECT * FROM [HKP].[DocumentDailyOverDue] WHERE [SourceType]='Post' AND CONVERT(date, DueDate)=@date)
                        DELETE FROM [HKP].[DocumentDailyOverDue] WHERE [SourceType]='Post' AND CONVERT(date, DueDate)=@date;
                        INSERT INTO [HKP].[DocumentDailyOverDue]([CompanyGroupId],[CompanyId],[PlantId],[EmployeeTypeOrCategory],[ComplianceDocumentId],[SourceType],[DueDate],[AddedBy],[AddedDate],[AddedFromIP],[OverDue],[Completed],[Mandatory],[Optional])
                        SELECT EII.GroupID AS CompanyGroupId, EII.CompanyId, EII.PlantId,EmpC.Id EmplyeeTypeOrCategory, EDR.ComplianceDocumentId, 'Post', @date AS DueDate, 'TS' AddedBy, GETDATE() as AddedDate, 'TS' AddedFromIP
						,SUM(case  when EDR.DueDate is null  then 0 else 1 end) AS OverDue
						,ISNULL((SELECT sum(case  when ED.FileId is not null  then 1 else 0 end) AS Mandatory FROM EmployeeDocument AS ED JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
							WHERE ED.FileId IS NOT NULL AND CONVERT(date, ED.UpdatedDate)<=@date and ED.ComplianceDocumentId=EDR.ComplianceDocumentId
							AND EI.GroupID=EII.GroupID AND EI.CompanyId=EII.CompanyId AND EI.PlantId=EII.PlantId),0) Completed
						,ISNULL((SELECT  COUNT(*) AS Mandatory FROM EmployeeDocument AS ED JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
							JOIN [HKP].[ComplianceDocument] AS CD ON CD.Id=ED.ComplianceDocumentId
							LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
						LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
						LEFT JOIN [HKP].EmployeeCategory EmpCM ON EmpCM.Id = DesM.EmployeeCategoryId
							WHERE ED.FileId IS NULL AND CONVERT(date, ED.DueDate)<=@date AND ED.OptionalOrMandatory='Mandatory' AND EI.EmployeeStatus = 'Active' AND ED.ComplianceDocumentId=EDR.ComplianceDocumentId
							AND EI.GroupID=EII.GroupID AND EI.CompanyId=EII.CompanyId AND EI.PlantId=EII.PlantId AND EmpCM.Id = EmpC.Id AND EmpCM.Id = EmpC.Id), 0) Mandatory
						,ISNULL((SELECT  COUNT(*) AS Mandatory FROM EmployeeDocument AS ED JOIN EmployeeInformation AS EI ON EI.SystemId=ED.EmpSystemID
							JOIN [HKP].[ComplianceDocument] AS CD ON CD.Id=ED.ComplianceDocumentId
							LEFT JOIN [HKP].Designation GDes ON GDes.Id = EI.GivenDesignationId
						LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EI.GivenDesignationId
						LEFT JOIN [HKP].EmployeeCategory EmpCO ON EmpCO.Id = DesM.EmployeeCategoryId
							WHERE ED.FileId IS NULL AND CONVERT(date, ED.DueDate)<=@date AND ED.OptionalOrMandatory='Optional' AND EI.EmployeeStatus = 'Active' and ED.ComplianceDocumentId=EDR.ComplianceDocumentId
							AND EI.GroupID=EII.GroupID AND EI.CompanyId=EII.CompanyId AND EI.PlantId=EII.PlantId  AND EmpCO.Id = EmpC.Id), 0) Optional
						FROM EmployeeDocument AS EDR
						JOIN EmployeeInformation AS EII ON EII.SystemId=EDR.EmpSystemID
						LEFT JOIN [HKP].Designation GDes ON GDes.Id = EII.GivenDesignationId
						LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EII.GivenDesignationId
						LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
						WHERE EDR.FileId IS NULL AND EDR.DueDate IS NULL OR  CONVERT(date, EDR.DueDate)<=@date AND EII.EmployeeStatus = 'Active'
						GROUP BY EII.GroupID, EII.CompanyId, EII.PlantId, EDR.ComplianceDocumentId,EmpC.UserName,EmpC.Id";

            _complianceDocumentRepository.ExecuteSqlCommand(sql);
        }

        private string GetEmployeeDueDocumentList(EmployeeInformation employee)
        {
            try
            {
                var cmdText = @"SELECT CD.UserName DocName,REPLACE(CONVERT(VARCHAR(11), POSTDOC.DueDate, 106), ' ', '-') DueDate
								FROM EmployeeDocument AS POSTDOC LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = POSTDOC.ComplianceDocumentId
								WHERE EmpSystemID='" + employee.SystemId + @"' AND FileId IS NULL AND IsMailSend=0
								AND DueDate IS NOT NULL AND CompanyGroupId = '" + employee.GroupID + @"'";
                var documents = _sqlRepository.GetDataTable(cmdText);

                #region Variable

                var filePath = "";
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                var oRU = new ReportUtility();
                var xlsRow = 1;
                var xlsCol = 1;

                #endregion Variable

                if (documents.Rows.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;
                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];

                    xlsRow = 5;

                    #region variable

                    var cDocumentName = 0;
                    var cDueDate = 0;

                    #endregion variable

                    var endXlsCol = 0;

                    xlsRow++;
                    xlsCol = 1;
                    var cSl = 0;

                    #region Header

                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sl. No.", 6); cSl = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Document Name", 45); cDocumentName = xlsCol; xlsCol++;

                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Due Date"); cDueDate = xlsCol; xlsCol++;

                    #endregion Header

                    xlsCol--;
                    endXlsCol = xlsCol;
                    xlsRow++;
                    var slCount = 0;
                    for (int i = 0; i < documents.Rows.Count; i++)
                    {
                        slCount++;
                        oRU.SetText(ref sheet1, xlsRow, cSl, slCount.ToString());
                        oRU.SetText(ref sheet1, xlsRow, cDocumentName, documents.Rows[i]["DocName"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cDueDate, documents.Rows[i]["DueDate"].ToString());
                        xlsRow++;
                    }

                    oRU.SetHeaderText(ref sheet1, 4, 1, "Employee Name: " + employee.EmployeeName, ExcelHAlign.HAlignCenter);
                    sheet1.Range[4, 1, 4, endXlsCol].Merge();

                    oRU.MainCompanyGroupHeader(ref sheet1, endXlsCol, "Document Needs To Be Uploaded", employee.GroupID);

                    #region UsedRange Alignment

                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    oRU.PageSetupAuto(ref sheet1, 5, ExcelPageOrientation.Landscape, "TS");
                    sheet1.Name = "DocumentsToBeUploadedByEmployee" + DateTime.Now.ToString("ddMMyyyyHHmmss");

                    workbook.Version = ExcelVersion.Excel97to2003;
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sheet1.Name + ".xls");
                    workbook.SaveAs(filePath);
                    workbook.Close();
                    excelEngine.Dispose();
                }
                return filePath;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(EmployeeDocument), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private IEnumerable<EmployeeDocument> Getlist(string empId)
        {
            try
            {
                return _sqlRepository.GetModelCollection<EmployeeDocument>("SELECT * FROM EmployeeDocument WHERE Id ='" + empId + "'");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<PreRecruitmentDocument> GetOldlist(string empIdOld)
        {
            try
            {
                var _sql = "SELECT * FROM PreRecruitmentDocument WHERE PreRecruitmentEmployeeId ='" + empIdOld + "'";
                return _sqlRepository.GetModelCollection<PreRecruitmentDocument>(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InitData(string empid, string empIdOld, out List<EmployeeDocument> from_db)
        {
            IEnumerable<PreRecruitmentDocument> from_ui = null;
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_ui = GetOldlist(empIdOld);
                from_db = Getlist(empid).ToList();
                foreach (var db in from_db)
                {
                    var ui = from_ui.FirstOrDefault(a => a.Id == db.Id);
                    if (ui == null || ui.Id == null)
                    {
                        db.ModelState = ModelState.Deleted;
                    }
                }
                var _pk = GetPK();
                var pkCount = 0;

                foreach (var ui in from_ui)
                {
                    var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                    if (db == null || db.Id == null)
                    {
                        pkCount++;
                        db = new EmployeeDocument
                        {
                            ModelState = ModelState.Added
                        };
                        AuditService.Log(db);
                        db.Id = "D" + _pk + "-" + pkCount;
                        MoveImage(ui.Id, ui.FileName);
                        db.FileId = ui.FileId;
                        db.FileName = ui.FileName;
                        db.EmpSystemID = empid;
                        db.PreRecruitmentEmployeeId = empIdOld;
                        db.ComplianceDocumentId = ui.ComplianceDocumentId;
                        db.DocDate = ui.DocDate;
                        db.DocNumber = ui.DocNumber;
                        db.AddedDate = ui.AddedDate;
                        db.UpdatedDate = ui.UpdatedDate;
                        db.AddedBy = identity.Name;
                        db.UpdatedBy = identity.Name;
                        db.ComplianceDocumentSetId = ui.ComplianceDocumentSetId;
                        from_db.Add(db);
                    }
                    else
                    {
                        db.ModelState = ModelState.Modified;
                        AuditService.Log(db);
                    }
                    MoveImage(ui.Id, ui.FileName);
                    db.FileId = ui.FileId;
                    db.FileName = ui.FileName;
                    db.EmpSystemID = empid;
                    db.PreRecruitmentEmployeeId = empIdOld;
                    db.ComplianceDocumentId = ui.ComplianceDocumentId;
                    db.DocDate = ui.DocDate;
                    db.DocNumber = ui.DocNumber;
                    db.AddedDate = ui.AddedDate;
                    db.UpdatedDate = ui.UpdatedDate;
                    db.AddedBy = identity.Name;
                    db.UpdatedBy = identity.Name;
                    db.ComplianceDocumentSetId = ui.ComplianceDocumentSetId;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveList(string empid, string empidOld)
        {
            List<EmployeeDocument> from_db = null;
            try
            {
                InitData(empid, empidOld, out from_db);
                foreach (var item in from_db)
                {
                    InsertOrUpdateGraph(item);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<PreRecruitmentDocument> GetPreRecruitmentDocumentList(string PKs)
        {
            try
            {
                return _sqlRepository.GetModelCollection<PreRecruitmentDocument>("SELECT * FROM PreRecruitmentDocument WHERE PreRecruitmentEmployeeId IN (" + PKs + ")");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void MoveImage(string fromName, string toName)
        {
            try
            {
                var Fromdirectory = ResourcesPathReader.GetDocumentSourcePath();
                var Todirectory = ResourcesPathReader.GetDocumentDestinationPath();
                var path = Path.Combine(Fromdirectory, fromName + Path.GetExtension(toName));
                if (File.Exists(path))
                {
                    File.Copy(Path.Combine(Fromdirectory, fromName + Path.GetExtension(toName)), Path.Combine(Todirectory, fromName + Path.GetExtension(toName)), true);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetDocList(string plantId, string budgetIds, string empType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"SELECT CD.Id AS ComplianceDocumentId
									,CD.UserName DocumentName
									,CD.DocumentType
									,CD.IsSkillBased
									,PC.PositionId
									,CDSD.OptionalOrMandatory
									,CD.EmpType
									,E.UserName AS EmployeeCategory
									,BD.BudgetId
									,DC.ComplianceDocumentSetId
								FROM
								(SELECT DISTINCT P.BudgetId
										,MB.EmploymentType
										,DM.EmployeeCategoryId
										,DM.DesignationId
									FROM PreRecruitmentEmployee P
									LEFT OUTER JOIN MST.ManpowerBudget MB ON P.BudgetId = MB.Id
									LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
									WHERE P.BudgetId IN (" + budgetIds + @")) BD
								LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								AND DC.EmploymentType = BD.EmploymentType
								LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								LEFT OUTER JOIN HKP.ComplianceDocumentPositonCode PC ON CD.Id = PC.ComplianceDocumentId
								LEFT OUTER JOIN ORG.Position PO ON PC.PositionId = PO.Id
								WHERE CD.EmploymentStage = 'PostRecruitment' AND CD.[Type]='EmployeeRelated'
										--AND ISNULL(CD.ProfileType,'') NOT IN ('Qualification','Training','Experience','Photo')
									AND DC.PlantId = '" + plantId + @"'
									AND CD.IsSkillBased = 1
									AND PC.PositionId IN (SELECT PositionId FROM MST.ManpowerBudget WHERE Id IN (" + budgetIds + @"))
									AND (CD.EmpType = '" + empType + @"' OR CD.EmpType = 'Both')
									UNION
									SELECT  CD.Id AS ComplianceDocumentId
									,CD.UserName DocumentName
									,CD.DocumentType
									,CD.IsSkillBased
									,'' PositionId
									,CDSD.OptionalOrMandatory
									,CD.EmpType
									,E.UserName AS EmployeeCategory
									,BD.BudgetId
									,DC.ComplianceDocumentSetId
								FROM (SELECT DISTINCT P.BudgetId
										,MB.EmploymentType
										,DM.EmployeeCategoryId
										,DM.DesignationId
									FROM PreRecruitmentEmployee P
									LEFT OUTER JOIN MST.ManpowerBudget MB ON P.BudgetId = MB.Id
									LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
									WHERE P.BudgetId IN (" + budgetIds + @")) BD
								LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
									AND DC.EmploymentType = BD.EmploymentType
								LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								WHERE CD.EmploymentStage = 'PostRecruitment' AND CD.[Type]='EmployeeRelated'
								--AND ISNULL(CD.ProfileType,'') NOT IN ('Qualification','Training','Experience','Photo')
									AND DC.PlantId = '" + plantId + @"'
									AND CD.IsSkillBased = 0
									AND (CD.EmpType = '" + empType + @"'OR CD.EmpType = 'Both')
								UNION
								SELECT  BD.Id AS ComplianceDocumentId
									,BD.UserName DocumentName
									,'' DocumentType
									,'' IsSkillBased
									,'' PositionId
									,'' OptionalOrMandatory
									,'' EmpType
									,E.UserName AS EmployeeCategory
									,BC.BudgetId
									,'' ComplianceDocumentSetId
								FROM (SELECT ComplianceDocumentSetId, CP.UserName,CP.Id from [HKP].[ComplianceDocumentSetProofTypeAssign] CSP
								LEFT OUTER JOIN [HKP].[ComplianceDocumentProofType] CP ON CSP.ComplianceDocumentProofTypeId=CP.Id
								) BD
								LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.ComplianceDocumentSetId = BD.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								LEFT OUTER JOIN (
								SELECT DM.EmployeeCategoryId, MB.EmploymentType,MB.Id BudgetId from MST.ManpowerBudget MB
								LEFT OUTER JOIN ORG.position P ON MB.PositionId=P.Id
								LEFT OUTER JOIN MST.DesignationMaster DM ON P.DesignationId=DM.DesignationId
								WHERE MB.Id IN (" + budgetIds + @")
								) BC ON BC.EmployeeCategoryId=DC.EmployeeCategoryId AND BC.EmploymentType=DC.EmploymentType
								WHERE  DC.PlantId = '" + plantId + "' AND BC.BudgetId is not null";
                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InitPostDocument(IEnumerable<EmployeeInformation> empList, string plantId, string empType)
        {
            var docdb = new List<EmployeeDocument>();
            var budgetIds = "''";
            var builder = new StringBuilder();
            builder.Append(budgetIds);
            foreach (var item in empList)
            {
                if (budgetIds == "''")
                {
                    budgetIds = "'" + item.BudgetCode + "'";
                }
                else
                {
                    builder.Append(",'" + item.BudgetCode + "'");
                }
            }
            budgetIds = builder.ToString();

            var _pk = GetAutoNumber(nameof(EmployeeDocument), PKGeneratorEnum.Auto, null, DateTime.Now);
            var pkCount = 0;
            var docList = GetDocList(plantId, budgetIds, empType);
            foreach (var item in empList)
            {
                var empId = item.SystemId;
                var budgetId = item.BudgetCode;
                using (DataView dvList = new DataView(docList)
                {
                    RowFilter = "BudgetId='" + budgetId + "'"
                })
                {
                    for (int i = 0; i < dvList.Count; i++)
                    {
                        pkCount++;
                        var ob = new EmployeeDocument
                        {
                            Id = _pk + "-" + pkCount,
                            ComplianceDocumentId = dvList[i]["ComplianceDocumentId"].ToString(),
                            ComplianceDocumentSetId = dvList[i]["ComplianceDocumentSetId"].ToString(),
                            EmpSystemID = empId
                        };
                        docdb.Add(ob);
                    }
                }
            }
            foreach (var item in docdb)
            {
                InsertGraph(item);
            }
        }

        public void EmployeeBirthdayWish(string by)
        {
            var log = new MailLog
            {
                AddedBy = "",
                AddedDate = DateTime.Now,
                AddedFromIP = "",
                AppVersion = "",
                CompanyGroupId = "",
                ModelState = ModelState.Added,
                RecordTime = DateTime.Now,
                ServiceName = "EmployeeWiseBirthdaywish",
                UserId = null,
                AttachmentName = "N/A",
                IsSuccess = false,
                MailGenerator = MailGenerator.Scheduler.ToString(),
                Remarks = "BirthDay Wish",
                Subject = "Birthday Wish"
            };

            try
            {
                var emailList = GetAdministrativeMailList();
                var companyGroupList = _companyGroupRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                var companyList = _companyRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                foreach (var companyGroup in companyGroupList)
                {
                    log.CompanyGroupId = companyGroup.Id;
                    var smtpConfigurationCG = _smtpConfigurationService.Query(r => r.CompanyGroupId == companyGroup.Id).Select().FirstOrDefault();
                    var email = new EmailSender(smtpConfigurationCG.Host, smtpConfigurationCG.Port, smtpConfigurationCG.MailingUserName, smtpConfigurationCG.Password, true);
                    var ccList = string.Join(";", emailList.Where(r => r.Active && r.MailType == "Cc" && r.Email != string.Empty).Select(r => r.FullName + "<" + r.Email + ">"));
                    log.CcList = ccList;
                    var bccList = string.Join(";", emailList.Where(r => r.Active && r.MailType == "Bcc" && r.Email != string.Empty).Select(r => r.FullName + "<" + r.Email + ">"));
                    log.BccList = bccList;
                    var inActiveList = string.Join(";", emailList.Where(r => !r.Active).Select(r => r.MailType + ":" + r.FullName));
                    foreach (var company in companyList.Where(r => r.CompanyGroupId == companyGroup.Id))
                    {
                        var employeeList = _employeeInformationRepository.Query(r => r.GroupID == company.CompanyGroupId && r.CompanyId == company.Id && r.EmployeeStatus == "Active").Select().ToList();

                        foreach (var employee in employeeList)
                        {
                            var birthDate = Convert.ToDateTime(employee.BirthdayCelebrationDate);
                            var bdate = birthDate.Day.ToString();
                            var bmonth = birthDate.Month.ToString();
                            var byear = birthDate.Year.ToString();

                            var toDay = DateTime.Now;
                            var bToday = Convert.ToDateTime(toDay.ToString());

                            var tdate = bToday.Day.ToString();
                            var tmonth = bToday.Month.ToString();
                            var tyear = bToday.Year.ToString();

                            if (bdate == tdate && bmonth == tmonth)
                            {
                                log.SenderEmail = smtpConfigurationCG.SenderSystemEmail;
                                log.SenderName = smtpConfigurationCG.SenderSystemName;

                                var birthDayList = employee.EmployeeName;
                                if (!string.IsNullOrEmpty(employee.EmailId))
                                {
                                    try
                                    {
                                        var message = email.PrepareMessage(log.SenderName + "<" + log.SenderEmail + ">",
                                            employee.EmployeeName + "<" + employee.EmailId + ">", ccList, bccList, "Birthday Wish",
                                            "<b>Happy Birthday Dear " + employee.EmployeeName + ".</b><br><br> Wishing you a wonderful year of<br> good health, happiness and success!<br><br> From <br>" + companyGroup.UserName + " Family");

                                        if (message != null)
                                        {
                                            email.Send(message);
                                            log.ToList = employee.EmailId;
                                            log.CcList = ccList;
                                            log.BccList = bccList;
                                            log.IsSuccess = true;
                                            log.HasAttachment = false;
                                            log.Remarks = "BirthDay Wish Mail of " + employee.EmployeeName + " has been send Successfully";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.IsSuccess = false;
                                        log.HasAttachment = false;
                                        log.Remarks = ex.Message;
                                        continue;
                                    }
                                }
                                else
                                {
                                    log.IsSuccess = false;
                                    log.HasAttachment = false;
                                    log.Remarks = "EMail Id not Found";
                                }
                            }
                        }
                    }
                }
                _mailLogRepository.Insert(log);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                log.Remarks = ex.Message;
                _mailLogRepository.Insert(log);
                _unitOfWork.SaveChanges();
                throw;
            }
        }

        public void EmployeeBirthDateList(string addedBy, string ip, string appVersion)
        {
            var ErrorLog = new MailLog
            {
                AddedBy = addedBy,
                AddedDate = DateTime.Now,
                AddedFromIP = ip,
                AppVersion = appVersion,
                CompanyGroupId = null,
                ModelState = ModelState.Added,
                RecordTime = DateTime.Now,
                ServiceName = "Error-BirthDayList",
                UserId = null,
                AttachmentName = null,
                IsSuccess = false,
                SenderName = null,
                MailGenerator = MailGenerator.Scheduler.ToString(),
                Remarks = "Birtday List"
            };
            try
            {
                var companyGroupList = _companyGroupRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                var companyList = _companyRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                var serviceName = MailServiceName.EmployeeBirthdayNotification.ToString();
                var fileName = serviceName + DateTime.Now.ToString("ddMMyyyyHHmmss");

                var birthDateList = "";
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
                        MailGenerator = MailGenerator.Scheduler.ToString(),
                    };

                    var mailServiceList = _mailReceiverServiceMappingRepository.Query(r => r.CompanyGroupId == companyGroup.Id && r.ServiceName == serviceName).Select().ToList();
                    if (mailServiceList.Count <= 0)
                    {
                        log.Remarks = "Mail service not found!";
                        _mailLogRepository.Insert(log);
                        _unitOfWork.SaveChanges();
                    }
                    else
                    {
                        var smtpConfigurationCG = _smtpConfigurationService.Query(r => r.CompanyGroupId == companyGroup.Id).Select().FirstOrDefault();
                        foreach (var item in mailServiceList)
                        {
                            birthDateList = "<table border=1><tr><th align='left'>Plant</th><th align='left'>Department</th><th align='left'>Employee Code</th><th align='left'>Employee Name</th><th align='left'>Designation</th><th align='left'>Employee Category</th></tr>";

                            string cmdText = @"SELECT EmployeeCode,EmployeeName,REPLACE(CONVERT(VARCHAR(11), BirthdayCelebrationDate, 106), ' ', '-') BirthdayCelebrationDate,REPLACE(CONVERT(VARCHAR(11), DOB, 106), ' ', '-') BirthDate,DESIG.UserName Designation
                                                ,Plant.UserName Plant, EC.UserName EmployeeCategory,DP.UserName  Department
                                             FROM EmployeeInformation EI
                                                LEFT JOIN ORG.Plant Plant ON Plant.Id = EI.PlantId
                                             LEFT JOIN hkp.LegalDesignation as DESIG ON DESIG.Id = EI.LegalDesignationId
                                    LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=DESIG.Id
                        LEFT JOIN [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                        LEFT JOIN HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                        LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = EI.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId
												LEFT JOIN org.Department DP ON PO.DepartmentID = DP.Id
                         WHERE DAY(BirthdayCelebrationDate) = DAY(GETdATE()) AND MONTH(BirthdayCelebrationDate) = MONTH(GETdATE()) and EmployeeStatus = 'Active'  ORDER BY EmployeeCode";
                            var documents = _sqlRepository.GetDataTable(cmdText);

                            var birthDateListBuilder = new StringBuilder();
                            birthDateListBuilder.Append(birthDateList);

                            for (int i = 0; i < documents.Rows.Count; i++)
                            {
                                birthDateListBuilder.Append("<tr><td>" + documents.Rows[i]["Plant"] + "</td><td>" + documents.Rows[i]["Department"] + "</td><td>" + documents.Rows[i]["EmployeeCode"] + "</td><td>" + documents.Rows[i]["EmployeeName"] + "</td><td>" + documents.Rows[i]["Designation"] + "</td><td>" + documents.Rows[i]["EmployeeCategory"] + "</td></tr>");
                            }
                            birthDateListBuilder.Append("</table>");

                            birthDateList = birthDateListBuilder.ToString();

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
                                var emailList = GetNormalMaileList(item);
                                if (emailList.Count <= 0)
                                {
                                    log.CompanyId = item.CompanyId;
                                    log.PlantId = item.PlantId;
                                    log.IsReciepientListActive = false;
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

                                var path = GetEmpInfo(item.CompanyGroupId, item.PlantId, fileName);

                                if (!string.IsNullOrEmpty(path))
                                {
                                    try
                                    {
                                        var message = email.PrepareMessage(item.SenderName + "<" + item.SenderEmail + ">", toList, ccList, bccList, item.Subject, "Today's("+DateTime.Now.Date.ToString("dd-MMM-yyyy")+") Birthday List." + birthDateList + "<br>" + item.MessageBody);

                                        using (var attachment = new Attachment(path))
                                        {
                                            message.Attachments.Add(attachment);
                                            email.Send(message);
                                            // Set file name.
                                            log.AttachmentName = fileName + ".xls";
                                            log.IsSuccess = true;
                                            log.IsReciepientListActive = true;
                                            log.IsServiceActive = true;
                                            log.HasAttachment = true;
                                        }
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
                                        log.Remarks = ex.Message;
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
                ErrorLog.Remarks = "Birth Day List - " + ex.Message;
                _mailLogRepository.Insert(ErrorLog);
                _unitOfWork.SaveChanges();
                throw;
            }
        }

        public IEnumerable<OrgStructureListViewModel> OrgStructureList(string CompanyGroupId)
        {
            try
            {
                var strSQL = @"SELECT DISTINCT u.StandardName ColumnName,IsNULL(e.RType,'position') as Rtype,e.Sequence eSequence,p.Sequence pSequence from (
                           SELECT  DISTINCT StandardName from [ORG].[StructureRelationship] as ee where CompanyGroupId='" + CompanyGroupId + @"' and  RType = 'Entity'  union
                           SELECT  DISTINCT StandardName from [ORG].[StructureRelationship] as pp where CompanyGroupId='" + CompanyGroupId + @"' and  RType = 'position' ) u
                           LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Entity' ) e on e.StandardName = u.StandardName
						   LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Position' ) p on p.StandardName = u.StandardName";
                return _employeeInformationRepository.SqlQuery<OrgStructureListViewModel>(strSQL);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GetEntityPosition(string CompanyGroupId, out DataSet dsRef)
        {
            try
            {
                var strSQL = @"SELECT DISTINCT u.StandardName UserName,IsNULL(e.RType,'position') as Rtype,e.Sequence eSequence,p.Sequence pSequence from (
                           SELECT  DISTINCT StandardName from [ORG].[StructureRelationship] as ee where CompanyGroupId='" + CompanyGroupId + @"' and  RType = 'Entity'  union
                           SELECT  DISTINCT StandardName from [ORG].[StructureRelationship] as pp where CompanyGroupId='" + CompanyGroupId + @"' and  RType = 'position' ) u
                           LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Entity' ) e on e.StandardName = u.StandardName
						   LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Position' ) p on p.StandardName = u.StandardName";
                dsRef = _sqlRepository.GetGridData(new GridParameter { ExportType = "Report", CmdText = strSQL }).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetEmpInfo(string companyGroupId, string plantId, string fileName)
        {
            try
            {
                var cListOId = string.Empty;
                var cListId = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";
                var OrgStrList = OrgStructureList(companyGroupId);
                var cList = new StringBuilder();
                var join = new StringBuilder();
                foreach (var item in OrgStrList)
                {
                    if (item.RType == "Entity")
                    {
                        cList.Append("," + item.ColumnName + ".UserName " + "e" + item.ColumnName + " ");
                        if (item.ColumnName == "EmployeeGroup")
                            join.Append("LEFT JOIN [HKP].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = EN." + item.ColumnName + "Id\n");
                        else
                            join.Append("LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = EN." + item.ColumnName + "Id\n");
                    }
                    else
                    {
                        cList.Append("," + item.ColumnName + ".UserName " + "p" + item.ColumnName + " ");
                        join.Append("LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = PO." + item.ColumnName + "Id\n");
                    }
                }
                var strSQL = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
                                    --,e.DOJ
                                    ,Line.UserName Line
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB
									,REPLACE(CONVERT(VARCHAR(11), e.BirthdayCelebrationDate, 106), ' ', '-') BirthdayCelebrationDate
                                    ,REPLACE(CONVERT(VARCHAR(11), e.ApprovedDateTime, 106), ' ', '-') ApprovedDateTime
                                    
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

                                    ,mpb.EntityId,mpb.PositionId
                                    --emp ids
									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName
									--,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
									" + cList + @"
                                    from EmployeeInformation e

                                    --left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    left outer join hkp.Designation edsg on edsg.id=e.DesignationSystemID
                                    left outer join hkp.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									left outer join hkp.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    left outer join hkp.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    left outer join (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									from mst.DesignationMaster dm
									left outer join hkp.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                            --left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=dm.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									and egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    left outer join mst.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Line] ON Line.Id = MPB.LineId
			                                            " + join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId
                                    LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
                                    where " + param + @" and Day(e.BirthdayCelebrationDate) = Day(GETDATE()) and MONTH(e.BirthdayCelebrationDate) = Month(Getdate()) AND e.EmployeeStatus = 'Active'";
                var documents = _sqlRepository.GetDataTable(strSQL);
                return EmployeeBirthDayExcel(companyGroupId, plantId, documents, "BirthDay List of Employees", fileName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string EmployeeBirthDayExcel(string companyGroupId, string plantId, DataTable dtEmpInfo, string SheetHeader, string SheetName)
        {
            try
            {
                #region Variable

                var filePath = "";

                DataTable dtEntity = null;
                DataTable dtPosition = null;

                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                ReportUtility oRU = null;

                var xlsRow = 1;
                var xlsCol = 1;
               // var IsBudgetCodeApplicable = true;

                #endregion Variable

                //objRpt = new clsReport();
                oRU = new ReportUtility();

                GetEntityPosition(companyGroupId, out DataSet dsEntityPosition);

                using (DataView dvEntity = new DataView(dsEntityPosition.Tables[0])
                {
                    RowFilter = "RType = 'Entity'",
                    Sort = "eSequence"
                })
                {
                    dtEntity = dvEntity.ToTable(true, "UserName");

                    using (DataView dvPosition = new DataView(dsEntityPosition.Tables[0])
                    {
                        RowFilter = "RType = 'Position'",
                        Sort = "pSequence"
                    })
                    {
                        dtPosition = dvPosition.ToTable(true, "UserName");

                        using (var dvBC = new DataView(dtEmpInfo))
                        {
                            //var dtBC = dvBC.ToTable(true, "IsPositionCodeApplicable");
                            //for (int i = 0; i < dtBC.Rows.Count; i++)
                            //{
                            //    IsBudgetCodeApplicable = Convert.ToBoolean(dtEmpInfo.Rows[i]["IsPositionCodeApplicable"].ToString());
                            //    if (IsBudgetCodeApplicable)
                            //    {
                            //        break;
                            //    }
                            //}
                            if (dtEmpInfo.Rows.Count > 0)
                            {
                                excelEngine = new ExcelEngine();
                                application = excelEngine.Excel;
                                workbook = application.Workbooks.Create(1);
                                sheet1 = workbook.Worksheets[0];

                                xlsRow = 6;

                                #region variable

                                var cEmployeeCode = 0;
                                var cName = 0;
                                var cBCD = 0;
                                var cBudgetCode = 0;
                                int cLine = 0;

                                var cDOB = 0;

                                var colNum = 0;
                                var cGivenDesignation = 0;

                                #endregion variable

                                var endXlsCol = 0;

                                xlsRow++;
                                xlsCol = 1;
                                var cSl = 0;

                                #region Header

                                oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Employee info", ExcelHAlign.HAlignCenter);
                                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SL", 6); cSl = xlsCol; xlsCol++;
                                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeCode"); cEmployeeCode = xlsCol; xlsCol++;

                                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Name", 30); cName = xlsCol; xlsCol++;
                                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DOB"); cDOB = xlsCol; xlsCol++;
                                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Birthday Celebration Date"); cBCD = xlsCol; xlsCol++;

                                //if (IsBudgetCodeApplicable)
                                //{
                                //    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Budget Code"); cBudgetCode = xlsCol; xlsCol++;

                                //    for (int i = 0; i < dtEntity.Rows.Count; i++)
                                //    {
                                //        oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, dtEntity.Rows[i]["UserName"].ToString(), 25); xlsCol++;
                                //    }
                                //    for (int c = 0; c < dtPosition.Rows.Count; c++)
                                //    {
                                //        oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, dtPosition.Rows[c]["UserName"].ToString(), 25); xlsCol++;
                                //    }
                                //}//IsBudgetCodeApplicable
                                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Line", 25); cLine = xlsCol; xlsCol++;
                                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", 25); cGivenDesignation = xlsCol; xlsCol++;

                                #endregion Header

                                sheet1.Range[xlsRow - 1, cSl, xlsRow - 1, cGivenDesignation].Merge();
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
                                    oRU.SetText(ref sheet1, xlsRow, cBCD, dtEmpInfo.Rows[i]["BirthdayCelebrationDate"].ToString());

                                    //if (Convert.ToBoolean(dtEmpInfo.Rows[i]["IsPositionCodeApplicable"].ToString()))
                                    //{
                                    //    oRU.SetText(ref sheet1, xlsRow, cBudgetCode, dtEmpInfo.Rows[i]["BudgetCode"].ToString());
                                    //    //entity

                                    //    for (int c = 0; c < dtEntity.Rows.Count; c++)
                                    //    {
                                    //        var _colname = dtEntity.Rows[c]["UserName"].ToString();
                                    //        var v = dtEmpInfo.Rows[i]["e" + _colname].ToString();
                                    //        colNum = cBudgetCode + c + 1;
                                    //        oRU.SetText(ref sheet1, xlsRow, colNum, v);
                                    //    }

                                    //    //position
                                    //    for (int c = 0; c < dtPosition.Rows.Count; c++)
                                    //    {
                                    //        var _colname = dtPosition.Rows[c]["UserName"].ToString();
                                    //        oRU.SetText(ref sheet1, xlsRow, colNum + c + 1, dtEmpInfo.Rows[i]["p" + _colname].ToString());
                                    //    }
                                    //}//is bc applicable
                                    oRU.SetText(ref sheet1, xlsRow, cLine, dtEmpInfo.Rows[i]["Line"].ToString());
                                    oRU.SetText(ref sheet1, xlsRow, cGivenDesignation, dtEmpInfo.Rows[i]["GivenDesignation"].ToString());
                                    #endregion Loop
                                    xlsRow++;
                                }

                                oRU.SetHeaderText(ref sheet1, 4, 1, "ON " + DateTime.Now.ToString("dd-MMM-yyyy"), ExcelHAlign.HAlignCenter);
                                sheet1.Range[4, 1, 4, cGivenDesignation].Merge();

                                if (!string.IsNullOrEmpty(plantId))
                                    oRU.PlantHeader(ref sheet1, cGivenDesignation, SheetHeader, plantId);
                                else
                                    oRU.CompanyGroupHeader(ref sheet1, cGivenDesignation, SheetHeader, companyGroupId);

                                #region UsedRange Alignment

                                sheet1.UsedRange.WrapText = true;
                                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                                #endregion UsedRange Alignment

                                oRU.PageSetupAuto(ref sheet1, 5, ExcelPageOrientation.Landscape, "TS");
                                sheet1.Name = SheetName;
                                workbook.Version = ExcelVersion.Excel97to2003;
                                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
                                workbook.SaveAs(filePath);
                                workbook.Close();
                                excelEngine.Dispose();
                            }
                            return filePath;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Dictionary<string, object> GetDocFile(string id)
        {
            try
            {
                var sql = @"Select FileId, FileName From [dbo].[EmployeeDocument]  Where Id='" + id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateEmployeeDocument(string id)
        {
            ExecuteSqlCommand("Update dbo.EmployeeDocument set FileName=NULL,FileId=NULL Where Id='" + id + "'");
        }

        public void InsertORUpdateMaster(EmployeeDocument entity)
        {
            try
            {
                if (!string.IsNullOrEmpty(entity.FileName))
                {
                    var id = Query(t => t.Id != entity.Id && t.EmpSystemID == entity.EmpSystemID && t.FileName == entity.FileName).Select(t => t.Id).FirstOrDefault();
                    if (id != null) throw new CustomException("This file is already exists!!!");
                }

                if (entity != null)
                {
                    var dbdata = Find(entity.Id);
                    dbdata.FileId = entity.Id;
                    dbdata.FileName = entity.FileName;
                    dbdata.DocDate = entity.DocDate;
                    dbdata.DocNumber = entity.DocNumber;
                    dbdata.UpdatedDate = DateTime.Now;
                    Update(dbdata);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void DeleteEmployeeDocument(string id)
        {
            try
            {
                var data = base.Query(r => r.Id == id).Select().FirstOrDefault();
                base.Delete(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetDocumentList(string plantId, string empType, string budgetCode, string givenDesignationId)
        {
            try
            {
                var sql = @"SELECT CD.UserName DocumentName, CD.Id AS ComplianceDocumentId
									,CDSD.OptionalOrMandatory
									,DC.ComplianceDocumentSetId
									,DC.ResponsiblePersonId
								FROM
								(
								SELECT DISTINCT
										P.EmploymentType
										,DM.EmployeeCategoryId
										,DM.DesignationId
										,P.GivenDesignationId
									FROM EmployeeInformation P
									LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
									WHERE P.GivenDesignationId='" + givenDesignationId + @"'
									) BD
								LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								AND DC.EmploymentType = BD.EmploymentType
								LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								LEFT OUTER JOIN HKP.ComplianceDocumentPositonCode PC ON CD.Id = PC.ComplianceDocumentId
								LEFT OUTER JOIN ORG.Position PO ON PC.PositionId = PO.Id
								LEFT JOIN MST.ManpowerBudget MB ON MB.PositionId=PO.Id
								WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId ='" + plantId + @"' AND CD.IsSkillBased = 1
								AND MB.Id='" + budgetCode + @"' AND (CD.EmpType ='" + empType + @"' OR CD.EmpType = 'Both')
							UNION
									SELECT CD.UserName DocumentName, CD.Id AS ComplianceDocumentId
									,CDSD.OptionalOrMandatory
									,DC.ComplianceDocumentSetId
									,DC.ResponsiblePersonId
								FROM (
							SELECT DISTINCT
										P.EmploymentType
										,DM.EmployeeCategoryId
										,DM.DesignationId
										,P.GivenDesignationId
									FROM EmployeeInformation P
									LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
									WHERE P.GivenDesignationId='" + givenDesignationId + @"'
									) BD
								LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								AND DC.EmploymentType = BD.EmploymentType
								LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId = '" + plantId + @"' AND CD.IsSkillBased = 0 AND (CD.EmpType = '" + empType + @"' OR CD.EmpType = 'Both')
								Order By CD.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void CreateNewDOcument(IEnumerable<EmployeeDocument> entities, string empId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (entities != null)
                {
                    foreach (var item in entities)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            var data = Find(empId + "-" + item.ComplianceDocumentId);
                            if (data != null)
                            {
                                throw new CustomException("This document is exists.");
                            }
                            else
                            {
                                item.Id = empId + "-" + item.ComplianceDocumentId;
                                item.EmpSystemID = empId;
                                item.AddedBy = identity.Name;
                                item.AddedDate = DateTime.Now;
                                InsertGraph(item);
                            }
                        }
                    }
                }
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
    }
}