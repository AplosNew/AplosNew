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
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class PreRecruitmentDocumentService : Service<PreRecruitmentDocument>, IPreRecruitmentDocumentService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<PreRecruitmentDocument> _preRecruitmentDocumentRepository;
        private readonly IPreRecruitmentProofTypeService _preRecruitmentProofTypeService;
        private readonly IRepositoryAsync<MailReceiver> _mailReceiverRepository;
        private readonly ISMTPConfigurationService _smtpConfigurationService;
        private readonly IRepositoryAsync<PreRecruitmentEmployee> _preRecruitmentEmployeeRepository;
        private readonly IRepositoryAsync<CompanyGroup> _companyGroupRepository;
        private readonly IRepositoryAsync<Company> _companyRepository;
        private readonly IRepositoryAsync<ComplianceDocument> _complianceDocumentRepository;
        private readonly IRepositoryAsync<MailLog> _mailLogRepository;
        private readonly IRepositoryAsync<Plant> _plantRepository;

        public PreRecruitmentDocumentService(
              IRepositoryAsync<PreRecruitmentDocument> preRecruitmentDocumentRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , ISMTPConfigurationService smtpConfigurationService
            , IPreRecruitmentProofTypeService preRecruitmentProofTypeService
            , IRepositoryAsync<PreRecruitmentEmployee> preRecruitmentEmployeeRepository
            , IRepositoryAsync<CompanyGroup> companyGroupRepository
            , IRepositoryAsync<Company> companyRepository
            , IRepositoryAsync<ComplianceDocument> complianceDocumentRepository
            , IRepositoryAsync<MailReceiver> mailReceiverRepository
            , IRepositoryAsync<MailLog> mailLogRepository
            , IRepositoryAsync<Plant> plantRepository
            ) : base(preRecruitmentDocumentRepository, unitOfWork, pkGeneratorService)
        {
            _preRecruitmentDocumentRepository = preRecruitmentDocumentRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _smtpConfigurationService = smtpConfigurationService;
            _preRecruitmentProofTypeService = preRecruitmentProofTypeService;
            _preRecruitmentEmployeeRepository = preRecruitmentEmployeeRepository;
            _companyGroupRepository = companyGroupRepository;
            _companyRepository = companyRepository;
            _complianceDocumentRepository = complianceDocumentRepository;
            _mailReceiverRepository = mailReceiverRepository;
            _mailLogRepository = mailLogRepository;
            _plantRepository = plantRepository;
        }

        #endregion Constructor

        public void DeleteCandidateDocument(string id)
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

        public void DueDocumentProcess()
        {
            var sql = @"UPDATE PreRecruitmentDocument SET
						 DueProcessDateTime = CASE WHEN PRD.DueProcessDateTime IS NULL THEN GETDATE() ELSE PRD.DueProcessDateTime END,
						   IsMailSend = CASE WHEN PRD.DueProcessDateTime IS NULL THEN 1 ELSE PRD.IsMailSend END,
							 DueDate =
							CASE
							WHEN CD.DependateDate = 'AppointmentDate'    AND E.ApprovedDateTime IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.ApprovedDateTime)
							WHEN CD.DependateDate = 'AgreedJoinDate'     AND E.AgreedDOJ IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.AgreedDOJ)
							WHEN CD.DependateDate = 'LetterOfIndentDate' AND E.SelectionDateTime IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.SelectionDateTime)
							WHEN CD.DependateDate = 'ProfileSubmit'      AND E.SubmitDateTime IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.SubmitDateTime)
							WHEN CD.DependateDate = 'SelectionDate'      AND E.SelectionDateTime IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.SelectionDateTime)
						END
								FROM PreRecruitmentDocument PRD
							JOIN HKP.ComplianceDocument CD ON CD.Id = PRD.ComplianceDocumentId
							LEFT JOIN
							(
							   SELECT EI.Id AS SystemId, EI.ApprovedDateTime, EI.AgreedDOJ, EI.SelectionDateTime, EI.SubmitDateTime FROM PreRecruitmentEmployee EI WHERE EI.Completed= 0
							   ) E ON E.SystemId = PRD.PreRecruitmentEmployeeId
							WHERE PRD.FileId IS NULL AND  PRD.FileName IS NULL AND PRD.IsCopied = 0";

            sql = @"UPDATE PreRecruitmentDocument SET
						 DueProcessDateTime = CASE WHEN ISNULL(PRD.DueProcessDateTime,'') = '' THEN GETDATE() ELSE PRD.DueProcessDateTime END,
						   IsMailSend = CASE WHEN ISNULL(PRD.DueProcessDateTime,'') = '' THEN 1 ELSE PRD.IsMailSend END,
							 DueDate =
							CASE
							WHEN CD.DependateDate = 'AppointmentDate'    AND ISNULL(E.ApprovedDateTime,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, E.ApprovedDateTime)
							WHEN CD.DependateDate = 'AgreedJoinDate'     AND ISNULL(E.AgreedDOJ,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, E.AgreedDOJ)
							WHEN CD.DependateDate = 'LetterOfIndentDate' AND ISNULL(E.SelectionDateTime,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, E.SelectionDateTime)
							WHEN CD.DependateDate = 'ProfileSubmit'      AND ISNULL(E.SubmitDateTime,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, E.SubmitDateTime)
							WHEN CD.DependateDate = 'SelectionDate'      AND ISNULL(E.SelectionDateTime,'')<>'' THEN DATEADD(DAY, CD.LeadOrLagDays, E.SelectionDateTime)
						END
								FROM PreRecruitmentDocument PRD
							JOIN HKP.ComplianceDocument CD ON CD.Id = PRD.ComplianceDocumentId
							LEFT JOIN
							(
							   SELECT EI.Id AS SystemId, EI.ApprovedDateTime, EI.AgreedDOJ, EI.SelectionDateTime, EI.SubmitDateTime FROM PreRecruitmentEmployee EI WHERE EI.Completed= 0
							   ) E ON E.SystemId = PRD.PreRecruitmentEmployeeId
							WHERE ISNULL(PRD.FileId,'') = '' AND  ISNULL(PRD.FileName,'') = '' AND PRD.IsCopied = 0";

            //var sql = @"DECLARE @totalEmployee AS INT;
            //			DECLARE @i AS INT=1;
            //			DECLARE @employee TABLE
            //			(
            //				Id INT PRIMARY KEY IDENTITY(1,1),
            //				SystemId VARCHAR(30),
            //				ApprovedDateTime DATETIME,
            //				AgreedDOJ DATETIME,
            //				SelectionDateTime DATETIME,
            //				SubmitDateTime DATETIME
            //			)
            //			INSERT INTO @employee
            //			SELECT EI.Id AS SystemId, EI.ApprovedDateTime, EI.AgreedDOJ, EI.SelectionDateTime, EI.SubmitDateTime FROM PreRecruitmentEmployee EI WHERE EI.Completed=0;
            //			SELECT @totalEmployee=COUNT(*) FROM @employee;
            //			WHILE @i<= @totalEmployee
            //			  BEGIN
            //				UPDATE PreRecruitmentDocument SET
            //					   DueProcessDateTime=CASE WHEN ED.DueProcessDateTime IS NULL THEN GETDATE() ELSE ED.DueProcessDateTime END,
            //					   IsMailSend=CASE WHEN ED.DueProcessDateTime IS NULL THEN 1 ELSE ED.IsMailSend END,
            //					   DueDate=
            //						CASE
            //							WHEN CD.DependateDate='AppointmentDate' AND E.ApprovedDateTime IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.ApprovedDateTime)
            //							WHEN CD.DependateDate='AgreedJoinDate' AND E.AgreedDOJ IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.AgreedDOJ)
            //							WHEN CD.DependateDate='LetterOfIndentDate' AND E.SelectionDateTime IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.SelectionDateTime)
            //							WHEN CD.DependateDate='ProfileSubmit' AND E.SubmitDateTime IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.SubmitDateTime)
            //							WHEN CD.DependateDate='SelectionDate' AND E.SelectionDateTime IS NOT NULL THEN DATEADD(DAY, CD.LeadOrLagDays, E.SelectionDateTime)
            //						END
            //					FROM PreRecruitmentDocument ED
            //					JOIN HKP.ComplianceDocument CD ON CD.Id=ED.ComplianceDocumentId
            //					LEFT JOIN @employee E ON E.SystemId=ED.PreRecruitmentEmployeeId
            //					WHERE E.Id=@i AND ED.FileId IS NULL
            //				SET @i = @i + 1;
            //			  END";
            _preRecruitmentDocumentRepository.ExecuteSqlCommand(sql);
        }

        public void DueDocumentMailSendingProcess(string by)
        {
            MailLog log = new MailLog
            {
                AddedBy = "",
                AddedDate = DateTime.Now,
                AddedFromIP = "",
                AppVersion = "",
                CompanyGroupId = "",
                ModelState = ModelState.Added,
                RecordTime = DateTime.Now,
                ServiceName = "PRE-REC Per Employee Document Due Processing",
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
                    EmailSender email = new EmailSender(smtpConfigurationCG.Host, smtpConfigurationCG.Port, smtpConfigurationCG.MailingUserName, smtpConfigurationCG.Password, true);
                    foreach (var company in companyList.Where(r => r.CompanyGroupId == companyGroup.Id))
                    {
                        var employeeList = _preRecruitmentEmployeeRepository.Query(r => r.GroupID == company.CompanyGroupId && r.CompanyId == company.Id && !r.Completed).Select().ToList();
                        foreach (var employee in employeeList)
                        {
                            if (!string.IsNullOrEmpty(employee.Email))
                            {
                                toEmail = employee.FullName + "<" + employee.Email + ">";
                                bccEmail = "mamun.aplos@gmail.com";
                                var path = GetEmployeeDueDocumentList(employee);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    docList = "<table border=1><tr><th align='left'>Sl</th><th align='left'>Document Name</th><th align='left'>Due Date</th></tr>";

                                    try
                                    {
                                        var cmdText = @"SELECT ROW_NUMBER() OVER (ORDER BY EDOC.DueDate) AS RowNum,CD.UserName DocName, REPLACE(CONVERT(VARCHAR(11), PREDOC.DueDate, 106), ' ', '-') DueDate FROM PreRecruitmentDocument AS PREDOC
										  LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = PREDOC.ComplianceDocumentId
											 WHERE PreRecruitmentEmployeeId='" + employee.Id + @"' AND FileName IS NULL AND IsMailSend=0
											 AND DueDate IS NOT NULL AND CompanyGroupId = '" + employee.GroupID + @"'";
                                        DataTable documents = _sqlRepository.GetDataTable(cmdText);

                                        for (int i = 0; i < documents.Rows.Count; i++)
                                        {
                                            docList += "<tr><td>" + documents.Rows[i]["RowNum"] + "</td><td>" + documents.Rows[i]["DocName"] + "</td><td>" + documents.Rows[i]["DueDate"] + "</td></tr>";
                                        }
                                        docList += "</table>";

                                        var message = email.PrepareMessage(smtpConfigurationCG.SenderSystemName + "<" + smtpConfigurationCG.SenderSystemEmail + ">", toEmail, "", bccEmail, "To Be Uploaded Documents List",
                                            "Dear " + employee.FullName + "@,<br><br>You need to submit/upload these documents are listed below ---<br><br>" + docList + ",The Documents are also listed in the Excel File,Please see the attached Excel File.<br><br>Best Regards<br>" + companyGroup.UserName + "Family");
                                        message.Attachments.Add(new Attachment(path));
                                        email.Send(message);
                                        _preRecruitmentDocumentRepository.ExecuteSqlCommand(@"Update dbo.PreRecruitmentDocument set IsMailSend=1 WHERE PreRecruitmentEmployeeId='" + employee.Id + @"' AND FileId IS NULL AND IsMailSend=0
																						AND DueDate IS NOT NULL;");
                                        if (message != null)
                                        {
                                            log.ToList = toEmail;
                                            email.Send(message);
                                            log.IsSuccess = true;
                                            log.HasAttachment = true;
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
                                    log.Remarks = "No data found in Pre-Recruitement documents";
                                }
                            }
                            else
                            {
                                log.Remarks = "Email Id not Found";
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

        public void ProcessDocumentDailyOverDue(DateTime processDate, string by, string ip)
        {
            var companyGroupList = _companyGroupRepository.Query(r => r.Active && !r.Archive).Select().ToList();
            var companyList = _companyRepository.Query(r => r.Active && !r.Archive).Select().ToList();
            var plantList = _plantRepository.Query(r => r.Active && !r.Archive).Select().ToList();
            foreach (var companyGroup in companyGroupList)
            {
                foreach (var company in companyList.Where(r => r.CompanyGroupId == companyGroup.Id))
                {
                    foreach (var plant in plantList.Where(r => r.CompanyGroupId == companyGroup.Id && r.CompanyId == company.Id))
                    {
                        var sql = @"DECLARE @date Date='" + processDate.ToDbDate() + @"';
                                    IF EXISTS (SELECT * FROM [HKP].[DocumentDailyOverDue] WHERE [SourceType]='Pre' AND CONVERT(date, DueDate)=@date and CompanyGroupId='" + companyGroup.Id + @"' and CompanyId='" + company.Id + @"' and PlantId='" + plant.Id + @"')
                                    DELETE FROM [HKP].[DocumentDailyOverDue] WHERE [SourceType]='Pre' AND CONVERT(date, DueDate)=@date and CompanyGroupId='" + companyGroup.Id + @"' and CompanyId='" + company.Id + @"' and PlantId='" + plant.Id + @"';
                                    INSERT INTO [HKP].[DocumentDailyOverDue]([CompanyGroupId],[CompanyId],[PlantId],[ComplianceDocumentId], [SourceType], [DueDate],[AddedBy],[AddedDate],[AddedFromIP],[OverDue],[Completed],[Mandatory],[Optional])
                                               SELECT PREE.GroupID AS CompanyGroupId, PREE.CompanyId, PREE.PlantId,PRDR.ComplianceDocumentId, 'Pre', @date AS DueDate, 'TS' AddedBy, GETDATE() as AddedDate, 'TS' AddedFromIP, 	SUM(case  when PRDR.DueDate is null  then 0 else 1 end) AS OverDue
                                    ,ISNULL((SELECT  COUNT(*) AS Mandatory FROM PreRecruitmentDocument AS PRD JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
	                                    and PRE.GroupID = PREE.GroupID and PRE.CompanyId=PREE.CompanyId and PRE.PlantId=PREE.PlantId WHERE PRD.FileId IS NOT NULL AND  CONVERT(date, PRD.UpdatedDate)<=@date), 0) Completed
                                    ,ISNULL((SELECT  COUNT(*) AS Mandatory FROM PreRecruitmentDocument AS PRD JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
	                                    JOIN [HKP].[ComplianceDocument] AS CD ON CD.Id=PRD.ComplianceDocumentId WHERE PRD.FileId IS NULL AND CONVERT(date, PRD.DueDate)<=@date AND CD.OptionalOrMandatory='Mandatory'
	                                    and PRE.GroupID = PREE.GroupID and PRE.CompanyId=PREE.CompanyId and PRE.PlantId=PREE.PlantId and PRD.ComplianceDocumentId=PRDR.ComplianceDocumentId ), 0) Mandatory
                                    ,ISNULL((SELECT  COUNT(*) AS Mandatory FROM PreRecruitmentDocument AS PRD JOIN PreRecruitmentEmployee AS PRE ON PRE.Id=PRD.PreRecruitmentEmployeeId
	                                    JOIN [HKP].[ComplianceDocument] AS CD ON CD.Id=PRD.ComplianceDocumentId WHERE PRD.FileId IS NULL AND CONVERT(date, PRD.DueDate)<=@date AND CD.OptionalOrMandatory='Optional'
	                                    and PRE.GroupID = PREE.GroupID and PRE.CompanyId=PREE.CompanyId and PRE.PlantId=PREE.PlantId and PRD.ComplianceDocumentId=PRDR.ComplianceDocumentId), 0) Optional
                                    FROM PreRecruitmentDocument AS PRDR
                                    JOIN PreRecruitmentEmployee AS PREE ON PREE.Id=PRDR.PreRecruitmentEmployeeId
                                    WHERE PRDR.FileId IS NULL AND CONVERT(date, PRDR.DueDate)<=@date AND PREE.GroupID='" + companyGroup.Id + @"' and PREE.CompanyId='" + company.Id + @"' and PREE.PlantId='" + plant.Id + @"'and PRDR.IsCopied = 0
                                    GROUP BY PREE.GroupID, PREE.CompanyId, PREE.PlantId,PRDR.ComplianceDocumentId";
                        _complianceDocumentRepository.ExecuteSqlCommand(sql);
                    }
                }
            }
        }

        private string GetEmployeeDueDocumentList(PreRecruitmentEmployee employee)
        {
            try
            {
                var cmdText = @"SELECT CD.UserName DocName, REPLACE(CONVERT(VARCHAR(11), PREDOC.DueDate, 106), ' ', '-') DueDate FROM PreRecruitmentDocument AS PREDOC
								  LEFT JOIN HKP.ComplianceDocument AS CD ON CD.Id = PREDOC.ComplianceDocumentId
								  WHERE PreRecruitmentEmployeeId='" + employee.Id + @"' AND FileName IS NULL AND IsMailSend=0
								  AND DueDate IS NOT NULL AND CompanyGroupId = '" + employee.GroupID + @"'";
                DataTable documents = _sqlRepository.GetDataTable(cmdText);

                #region Variable

                var filePath = "";
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                ReportUtility oRU = new ReportUtility();
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

                    oRU.SetHeaderText(ref sheet1, 4, 1, "Employee Name: " + employee.FullName, ExcelHAlign.HAlignCenter);
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

        public IEnumerable<PreRecruitmentDocument> GetDocumentFile(string id)
        {
            try
            {
                var sql = @"Select * From [dbo].[PreRecruitmentDocument]  Where PreRecruitmentEmployeeId='" + id + "'";
                return _preRecruitmentDocumentRepository.SqlQuery<PreRecruitmentDocument>(sql).AsEnumerable();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdatePreRecruitmentDocument(string id)
        {
            ExecuteSqlCommand("Update dbo.PreRecruitmentDocument set FileName=NULL,FileId=NULL Where Id='" + id + "'");
        }

        public Dictionary<string, object> GetDocFile(string id)
        {
            try
            {
                var sql = @"Select FileId, FileName From [dbo].[PreRecruitmentDocument]  Where Id='" + id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private DataTable GetDocList(string plantId, string givenDesignationIds, string empType)
        {
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
								,DC.ComplianceDocumentSetId
								,DC.ResponsiblePersonId
								,BD.GivenDesignationId
							FROM
							(
							SELECT DISTINCT
									--P.EmploymentType,
									DM.EmployeeCategoryId
									,DM.DesignationId
									,P.GivenDesignationId
								FROM PreRecruitmentEmployee P
								LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
								WHERE P.GivenDesignationId IN (" + givenDesignationIds + @")

								) BD
							LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
							--AND DC.EmploymentType = BD.EmploymentType
							LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
							LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
							LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
							LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
							LEFT OUTER JOIN HKP.ComplianceDocumentPositonCode PC ON CD.Id = PC.ComplianceDocumentId
							LEFT OUTER JOIN ORG.Position PO ON PC.PositionId = PO.Id
							WHERE CD.EmploymentStage = 'PreRecruitment' AND CD.[Type]='EmployeeRelated'
								AND DC.PlantId = '" + plantId + @"'
								AND CD.IsSkillBased = 1
								AND PC.PositionId IN (SELECT PositionId FROM MST.ManpowerBudget WHERE Id IN
								(
								SELECT BudgetId FROM PreRecruitmentEmployee Where GivenDesignationId IN (" + givenDesignationIds + @")
								))
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
								,DC.ComplianceDocumentSetId
								,DC.ResponsiblePersonId
								,BD.GivenDesignationId
							FROM (
						SELECT DISTINCT
									--P.EmploymentType,
									DM.EmployeeCategoryId
									,DM.DesignationId
									,P.GivenDesignationId
								FROM PreRecruitmentEmployee P
								LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
								WHERE P.GivenDesignationId IN (" + givenDesignationIds + @")
								) BD
							LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								--AND DC.EmploymentType = BD.EmploymentType
							LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
							LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
							LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
							LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
							WHERE CD.[Type]='EmployeeRelated'  AND CD.EmploymentStage = 'PreRecruitment'
								AND DC.PlantId = '" + plantId + @"'
								AND CD.IsSkillBased = 0
								AND (CD.EmpType = '" + empType + "'OR CD.EmpType = 'Both')";
                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveDocumentList(string plantId, string empType, List<PreRecruitmentDocument> givenDesignationIdList)
        {
            List<PreRecruitmentDocument> docdb = new List<PreRecruitmentDocument>();
            List<PreRecruitmentProofType> docpfdb = new List<PreRecruitmentProofType>();
            try
            {
                var givenDesignationIds = "''";
                foreach (var item in givenDesignationIdList)
                {
                    if (givenDesignationIds == "''")
                    {
                        givenDesignationIds = "'" + item.GivenDesignationId + "'";
                    }
                    else
                    {
                        givenDesignationIds += ",'" + item.GivenDesignationId + "'";
                    }
                }
                var _pk = GetAutoNumber(nameof(PreRecruitmentDocument), PKGeneratorEnum.Auto, null, DateTime.Now);
                var _pfpk = _preRecruitmentProofTypeService.GetAutoNumber(nameof(PreRecruitmentProofType), PKGeneratorEnum.Auto, null, DateTime.Now);

                var pkCount = 0;
                var pkpfCount = 0;
                DataTable docList = GetDocList(plantId, givenDesignationIds, empType);

                DataView dvPL = new DataView(docList)
                {
                    RowFilter = "OptionalOrMandatory=''"
                };
                DataTable dtPL = dvPL.ToTable();

                DataView dvCD = new DataView(docList)
                {
                    RowFilter = "OptionalOrMandatory <>''"
                };
                DataTable dtCD = dvCD.ToTable();

                foreach (var item in givenDesignationIdList)
                {
                    var empId = item.PreRecruitmentEmployeeId;
                    var givenDesignationId = item.GivenDesignationId;
                    DataView dvList = new DataView(dtCD)
                    {
                        RowFilter = "GivenDesignationId='" + givenDesignationId + "'"
                    };
                    for (int i = 0; i < dvList.Count; i++)
                    {
                        pkCount++;
                        PreRecruitmentDocument ob = new PreRecruitmentDocument
                        {
                            Id = _pk + "-" + pkCount,
                            ComplianceDocumentId = dvList[i]["ComplianceDocumentId"].ToString(),
                            ComplianceDocumentSetId = dvList[i]["ComplianceDocumentSetId"].ToString(),
                            OptionalOrMandatory = dvList[i]["OptionalOrMandatory"].ToString(),
                            PreRecruitmentEmployeeId = empId,
                            ResponsiblePersonId = (dvList[i]["ResponsiblePersonId"].ToString().Trim().Length == 0 ? null : dvList[i]["ResponsiblePersonId"].ToString().Trim())
                        };
                        docdb.Add(ob);
                    }
                }

                foreach (var item in givenDesignationIdList)
                {
                    var empId = item.PreRecruitmentEmployeeId;
                    var givenDesignationId = item.GivenDesignationId;
                    DataView dvList = new DataView(dtPL)
                    {
                        RowFilter = "GivenDesignationId='" + givenDesignationId + "'"
                    };
                    for (int i = 0; i < dvList.Count; i++)
                    {
                        pkpfCount++;
                        PreRecruitmentProofType obj = new PreRecruitmentProofType
                        {
                            Id = _pfpk + "-" + pkpfCount,
                            ComplianceDocumentId = dvList[i]["ComplianceDocumentId"].ToString(),
                            PreRecruitmentEmployeeId = empId
                        };
                        docpfdb.Add(obj);
                    }
                }

                foreach (var item in docdb)
                {
                    base.InsertGraph(item);
                }
                foreach (var item in docpfdb)
                {
                    _preRecruitmentProofTypeService.InsertGraph(item);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void InsertGraph(IEnumerable<PreRecruitmentDocument> entities, string PreRecruitmentEmployeeId)
        {
            try
            {
                if (entities != null)
                {
                    var dbList = Query(r => r.PreRecruitmentEmployeeId == PreRecruitmentEmployeeId).Select().ToList();
                    foreach (var item in entities)
                    {
                        var loList = dbList.FirstOrDefault(r => r.PreRecruitmentEmployeeId == item.PreRecruitmentEmployeeId && r.Id == item.Id);

                        if (loList != null)
                        {
                            loList.FileId = loList.Id;
                            loList.FileName = item.FileName;
                            loList.UpdatedBy = item.PreRecruitmentEmployeeId;
                            loList.UpdatedDate = DateTime.Now;
                            Update(loList);
                        }
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

        public IEnumerable<object> GetDocumentData(string companyGroupId, string budgetId, string plantId, string empType, string pId)
        {
            try
            {
                var sql = @"SELECT PD.Id,PD.FileName,PD.FileId,PD.PreRecruitmentEmployeeId,CD.Id AS ComplianceDocumentId,
                            CD.UserName DocumentName,CD.DocumentType,
                            CD.IsSkillBased,PC.PositionId,CDSD.OptionalOrMandatory,
                            CD.EmpType,E.UserName AS EmployeeCategory
                            FROM HKP.ComplianceDocumentSet AS CDS
                            LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON CDS.Id=DC.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id= CDSD.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
                            LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
                            LEFT OUTER JOIN HKP.ComplianceDocumentPositonCode PC ON CD.Id=PC.ComplianceDocumentId
                            LEFT OUTER JOIN ORG.Position PO ON PC.PositionId=PO.Id
                            LEFT OUTER JOIN  (Select * from  dbo.PreRecruitmentDocument Where PreRecruitmentEmployeeId='" + pId + @"') PD ON CD.Id=PD.ComplianceDocumentId
                            WHERE CD.EmploymentStage='PreRecruitment' AND CD.DocumentationBy='Department'
							and DC.EmployeeCategoryId =
							 (Select D.EmployeeCategoryId From
                         (SELECT * FROM MST.DesignationMaster Where CompanyGroupId='" + companyGroupId + @"') AS D
						 Left outer join dbo.PreRecruitmentEmployee PE ON D.DesignationId=PE.GivenDesignationId
						 WHERE PE.BudgetId= '" + budgetId + @"' AND PE.Id='" + pId + @"')
							--=(Select D.EmployeeCategoryId From
       --                  (SELECT * FROM MST.DesignationMaster Where CompanyGroupId='" + companyGroupId + @"') AS D
       --                  LEFT OUTER JOIN ORG.Position AS P ON P.DesignationId = D.DesignationId
       --                  LEFT OUTER JOIN MST.ManpowerBudget AS M ON M.PositionId=P.Id WHERE M.Id= '" + budgetId + @"')

                         AND DC.PlantId='" + plantId + @"' and CD.IsSkillBased=1 AND PC.PositionId=(select PositionId from MST.ManpowerBudget WHERE Id= '" + budgetId + @"')
                         AND (CD.EmpType='" + empType + @"' or CD.EmpType='Both')
						  UNION
						  SELECT PD.Id,PD.FileName,PD.FileId,PD.PreRecruitmentEmployeeId,CD.Id AS ComplianceDocumentId,
                            CD.UserName DocumentName,CD.DocumentType,
                            CD.IsSkillBased,'' PositionId,CDSD.OptionalOrMandatory,
                            CD.EmpType,E.UserName AS EmployeeCategory
                            FROM HKP.ComplianceDocumentSet AS CDS
                            LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON CDS.Id=DC.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id= CDSD.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
                            LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
                            LEFT OUTER JOIN  (Select * from  dbo.PreRecruitmentDocument Where PreRecruitmentEmployeeId='" + pId + @"') PD ON CD.Id=PD.ComplianceDocumentId
                            WHERE CD.EmploymentStage='PreRecruitment' AND CD.DocumentationBy='Department'
							and DC.EmployeeCategoryId =
							 (Select D.EmployeeCategoryId From
                         (SELECT * FROM MST.DesignationMaster Where CompanyGroupId='" + companyGroupId + @"') AS D
						 Left outer join dbo.PreRecruitmentEmployee PE ON D.DesignationId=PE.GivenDesignationId
						 WHERE PE.BudgetId= '" + budgetId + @"' AND PE.Id='" + pId + @"')
							--=(Select D.EmployeeCategoryId From
       --                  (SELECT * FROM MST.DesignationMaster Where CompanyGroupId='" + companyGroupId + @"') AS D
       --                  LEFT OUTER JOIN ORG.Position AS P ON P.DesignationId = D.DesignationId
       --                  LEFT OUTER JOIN MST.ManpowerBudget AS M ON M.PositionId=P.Id WHERE M.Id= '" + budgetId + @"')

                         AND DC.PlantId='" + plantId + @"' and CD.IsSkillBased=0
                         AND (CD.EmpType='" + empType + @"' or CD.EmpType='Both') Order By CD.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetDocumentDataList(string companyGroupId, string budgetId, string pId, string plantId)
        {
            try
            {
                var sql = @"SELECT distinct  PD.*
									,CD.UserName DocumentName
									,CD.DocumentType
									,CD.IsSkillBased
									,CDSD.OptionalOrMandatory
									,CD.EmpType
									,CD.ProfileType,CD.DocNumberRequired,CD.DocDateRequired
									,E.UserName AS EmployeeCategory
								FROM dbo.PreRecruitmentDocument PD
								LEFT JOIN hkp.ComplianceDocument CD ON PD.ComplianceDocumentId = CD.Id
								LEFT JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CD.Id = CDSD.ComplianceDocumentId
								LEFT JOIN (select  * from hkp.DocumentConfigurationDesignationGroup

								Where PlantId='" + plantId + @"' and EmployeeCategoryId = (
										SELECT D.EmployeeCategoryId
										FROM (SELECT * FROM MST.DesignationMaster WHERE CompanyGroupId = '" + companyGroupId + @"'
											) AS D
										LEFT JOIN dbo.PreRecruitmentEmployee PE ON D.DesignationId = PE.GivenDesignationId
										WHERE PE.BudgetId = '" + budgetId + @"'
											AND PE.Id = '" + pId + @"'
										)
								)DD ON CDSD.ComplianceDocumentSetId = DD.ComplianceDocumentSetId
								LEFT JOIN HKP.EmployeeCategory AS E ON DD.EmployeeCategoryId = E.Id
								WHERE PD.PreRecruitmentEmployeeId = '" + pId + @"'
									AND CD.EmploymentStage = 'PreRecruitment'
									AND CD.DocumentationBy = 'Department'
									AND ISNULL(CD.ProfileType,'') NOT IN ('Qualification','Training','Experience','Photo')
									AND E.UserName IS NOT NULL Order By DocumentName";
                //AND PD.DueDate IS NOT NULL";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetDocumentDataList(string empId)
        {
            try
            {
                var sql = @"SELECT ED.*, CD.UserName DocumentName,CD.EmploymentStage FROM PreRecruitmentDocument ED
							LEFT JOIN HKP.ComplianceDocument CD ON ED.ComplianceDocumentId=CD.Id
							WHERE ED.PreRecruitmentEmployeeId='" + empId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetSubmittedEmployee(GridParameter parameters, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId)
        {
            try
            {
                var str = "";
                if (!isControlAdmin && !isSysAdmin)
                    str = @" AND PRE.BudgetId IN (SELECT Id from mst.ManpowerBudget WHERE EntityId IN (SELECT entityid FROM [HKP].[ApprovalConfiguration] WHERE OrgDocRP='" + employeeId + "'))";
                parameters.CmdText = @"Select PRE.*,E.UserName EntityName,D.UserName Designation,PR.UserName PositionName
									   ,DEG.UserName GivenDesignation, DEPT.UserName AS Department
									 FROM PreRecruitmentEmployee PRE
									 LEFT OUTER JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id
									 LEFT OUTER JOIN ORG.Position PR ON PMB.PositionId=PR.Id
									 LEFT OUTER JOIN HKP.Designation DEG on DEG.Id=PRE.GivenDesignationId
								     LEFT OUTER JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
									 LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id
									 LEFT OUTER JOIN HKP.Designation D ON PR.DesignationId=D.Id
									 Where PRE.GroupID='" + companyGroupId + @"' AND PRE.CompanyId='" + companyId + @"' AND PRE.IsApproved=0 AND PRE.Completed=0"
                                    + str;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void InsertORUpdateMaster(PreRecruitmentDocument entity)
        {
            try
            {
                if (!string.IsNullOrEmpty(entity.FileName))
                {
                    var id = Query(t => t.Id != entity.Id && t.PreRecruitmentEmployeeId == entity.PreRecruitmentEmployeeId && t.FileName == entity.FileName).Select(t => t.Id).FirstOrDefault();
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

        public void InsertORUpdate(PreRecruitmentDocument entity)
        {
            try
            {
                if (!string.IsNullOrEmpty(entity.FileName))
                {
                    var id = Query(t => t.Id != entity.Id && t.PreRecruitmentEmployeeId == entity.PreRecruitmentEmployeeId && t.FileName == entity.FileName).Select(t => t.Id).FirstOrDefault();
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

        public void CreateCandidateDocument(IEnumerable<PreRecruitmentEmployee> entities)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                foreach (var item in entities)
                {
                    var sql = @"DECLARE @employeeId varchar(20)='" + item.Id + @"';
									DECLARE @plantId varchar(20)='" + item.PlantId + @"';
									DECLARE @manpowerBudgetId varchar(20);
									DECLARE @givenDesignationId varchar(20);
									DECLARE @empType varchar(20);
									DELETE FROM PreRecruitmentDocument WHERE PreRecruitmentEmployeeId=@employeeId AND FileName IS NULL;
									SELECT  @ManpowerBudgetId=BudgetId, @givenDesignationId=GivenDesignationId, @empType=EmpType FROM PreRecruitmentEmployee WHERE Id=@employeeId;
									INSERT INTO PreRecruitmentDocument (Id, PreRecruitmentEmployeeId, AddedBy, AddedDate, ComplianceDocumentId, OptionalOrMandatory, ComplianceDocumentSetId, ResponsiblePersonId)
									SELECT @employeeId+'-'+ X.ComplianceDocumentId, @employeeId, '" + identity.Name + @"', GETDATE(), X.ComplianceDocumentId, X.OptionalOrMandatory, X.ComplianceDocumentSetId, X.ResponsiblePersonId from (
									SELECT CD.Id AS ComplianceDocumentId
									,CDSD.OptionalOrMandatory
									,DC.ComplianceDocumentSetId
									,DC.ResponsiblePersonId
								FROM
								(
								SELECT DISTINCT
										--P.EmploymentType
										DM.EmployeeCategoryId
										,DM.DesignationId
										,P.GivenDesignationId
									FROM PreRecruitmentEmployee P
									LEFT JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
									WHERE P.GivenDesignationId=@givenDesignationId
									) BD
								LEFT JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								--AND DC.EmploymentType = BD.EmploymentType
								LEFT JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								LEFT JOIN HKP.ComplianceDocumentPositonCode PC ON CD.Id = PC.ComplianceDocumentId
								LEFT JOIN ORG.Position PO ON PC.PositionId = PO.Id
								LEFT JOIN MST.ManpowerBudget MB ON MB.PositionId=PO.Id
								WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId =@plantId AND CD.IsSkillBased = 1
								AND MB.Id=@manpowerBudgetId AND (CD.EmpType = @empType OR CD.EmpType = 'Both')
                                AND CD.EmploymentStage = 'PreRecruitment'
							UNION
									SELECT  CD.Id AS ComplianceDocumentId
									,CDSD.OptionalOrMandatory
									,DC.ComplianceDocumentSetId
									,DC.ResponsiblePersonId
								FROM (
							SELECT DISTINCT
										--P.EmploymentType
										DM.EmployeeCategoryId
										,DM.DesignationId
										,P.GivenDesignationId
									FROM PreRecruitmentEmployee P
									LEFT JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
									WHERE P.GivenDesignationId=@givenDesignationId
									) BD
								LEFT JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								--AND DC.EmploymentType = BD.EmploymentType
								LEFT JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId = @plantId AND CD.IsSkillBased = 0 AND (CD.EmpType = @empType OR CD.EmpType = 'Both')
                                AND CD.EmploymentStage = 'PreRecruitment'
								)X  WHERE X.ComplianceDocumentId NOT IN(SELECT ComplianceDocumentId from PreRecruitmentDocument ED WHERE ED.PreRecruitmentEmployeeId=@employeeId)";

                    base.ExecuteSqlCommand(sql);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void CreateNewDOcument(IEnumerable<PreRecruitmentDocument> entities, string empId)
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
                                item.PreRecruitmentEmployeeId = empId;
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
								FROM dbo.PreRecruitmentDocument ED
								LEFT JOIN hkp.ComplianceDocument CD ON ED.ComplianceDocumentId = CD.Id
								LEFT JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CD.Id = CDSD.ComplianceDocumentId
								LEFT JOIN (SELECT  * FROM HKP.DocumentConfigurationDesignationGroup
								Where PlantId='" + plantId + @"' and EmployeeCategoryId = (
										SELECT D.EmployeeCategoryId
										FROM (SELECT * FROM MST.DesignationMaster WHERE CompanyGroupId = '" + companyGroupId + @"'
											) AS D
										LEFT JOIN PreRecruitmentEmployee EI ON D.DesignationId = EI.GivenDesignationId
										WHERE EI.Id = '" + pId + @"'
										)
								)DD ON CDSD.ComplianceDocumentSetId = DD.ComplianceDocumentSetId
								LEFT JOIN HKP.EmployeeCategory AS E ON DD.EmployeeCategoryId = E.Id
								WHERE ED.PreRecruitmentEmployeeId = '" + pId + @"'
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

        public IEnumerable<object> GetDocumentList(string plantId, string empType, string butgedCode, string givenDesignationId)
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
										--P.EmploymentType
										DM.EmployeeCategoryId
										,DM.DesignationId
										,P.GivenDesignationId
									FROM PreRecruitmentEmployee P
									LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
									WHERE P.GivenDesignationId='" + givenDesignationId + @"'
									) BD
								LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								--AND DC.EmploymentType = BD.EmploymentType
								LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								LEFT OUTER JOIN HKP.ComplianceDocumentPositonCode PC ON CD.Id = PC.ComplianceDocumentId
								LEFT OUTER JOIN ORG.Position PO ON PC.PositionId = PO.Id
								LEFT JOIN MST.ManpowerBudget MB ON MB.PositionId=PO.Id
								WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId ='" + plantId + @"' AND CD.IsSkillBased = 1
								AND MB.Id='" + butgedCode + @"' AND (CD.EmpType ='" + empType + @"' OR CD.EmpType = 'Both') AND CD.EmploymentStage='PreRecruitment'
							UNION
									SELECT CD.UserName DocumentName, CD.Id AS ComplianceDocumentId
									,CDSD.OptionalOrMandatory
									,DC.ComplianceDocumentSetId
									,DC.ResponsiblePersonId
								FROM (
							SELECT DISTINCT
										--P.EmploymentType
										DM.EmployeeCategoryId
										,DM.DesignationId
										,P.GivenDesignationId
									FROM PreRecruitmentEmployee P
									LEFT OUTER JOIN MST.DesignationMaster DM ON P.GivenDesignationId = DM.DesignationId
									WHERE P.GivenDesignationId='" + givenDesignationId + @"'
									) BD
								LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON DC.EmployeeCategoryId = BD.EmployeeCategoryId
								--AND DC.EmploymentType = BD.EmploymentType
								LEFT OUTER JOIN HKP.ComplianceDocumentSet AS CDS ON CDS.Id = DC.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id = CDSD.ComplianceDocumentSetId
								LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
								LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
								WHERE CD.[Type]='EmployeeRelated' AND DC.PlantId = '" + plantId + @"' AND CD.IsSkillBased = 0 AND (CD.EmpType = '" + empType + @"' OR CD.EmpType = 'Both')
								AND CD.EmploymentStage='PreRecruitment'
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

        #region Salary Fixation Mail

        public void SalaryFixationMail()
        {
            var log = new MailLog
            {
                AddedBy = "",
                AddedDate = DateTime.Now,
                AddedFromIP = "",
                AppVersion = "",
                ModelState = ModelState.Added,
                RecordTime = DateTime.Now,
                ServiceName = "Salary Fixation Mail.",
                UserId = null,
                AttachmentName = null,
                IsSuccess = false,
                SenderName = null,
                MailGenerator = MailGenerator.Scheduler.ToString()
            };
            try
            {
                var empName = "";
                var empEmail = "";
                var empId = "";

                var companyGroupList = _companyGroupRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                var companyList = _companyRepository.Query(r => r.Active && !r.Archive).Select().ToList();
                foreach (var companyGroup in companyGroupList)
                {
                    log.CompanyGroupId = companyGroup.Id;

                    var smtpConfigurationCG = _smtpConfigurationService.Query(r => r.CompanyGroupId == companyGroup.Id).Select().FirstOrDefault();
                    var email = new EmailSender(smtpConfigurationCG.Host, smtpConfigurationCG.Port, smtpConfigurationCG.MailingUserName, smtpConfigurationCG.Password, true);
                    foreach (var company in companyList.Where(r => r.CompanyGroupId == companyGroup.Id))
                    {
                        var sqlStr = @"SELECT Id,FullName,Email FROM PreRecruitmentEmployee WHERE Id IN(
									    SELECT PreReceuitmentEmployeeId FROM[SCS].[SalaryFixationMail]
										 WHERE ISNULL(ismailsent,0)= 0)";

                        DataTable emailList = _sqlRepository.GetDataTable(sqlStr);

                        if (emailList.Rows.Count > 0)
                        {
                            for (int i = 0; i < emailList.Rows.Count; i++)
                            {
                                var toEmail = "";
                                var bccEmail = "";
                                empId = emailList.Rows[i]["Id"].ToString();
                                empName = emailList.Rows[i]["FullName"].ToString();
                                empEmail = emailList.Rows[i]["Email"].ToString();

                                if (empId != string.Empty && empName != string.Empty && empEmail != string.Empty)
                                {
                                    toEmail = empName + "<" + empEmail + ">";
                                    bccEmail = "mamun.aplos@gmail.com";
                                    var message = email.PrepareMessage(smtpConfigurationCG.SenderSystemName + "<" + smtpConfigurationCG.SenderSystemEmail + ">", toEmail, "", bccEmail, "Salary Fixation Mail Send.", "Mail Is In Row");

                                    email.Send(message);
                                    _preRecruitmentEmployeeRepository.ExecuteSqlCommand(@"UPDATE [SCS].[SalaryFixationMail]
																					SET ismailsent = 1
																					WHERE PreReceuitmentEmployeeId = '" + empId + @"'");

                                    log.ToList = toEmail;
                                    log.BccList = bccEmail;
                                    log.IsSuccess = true;
                                    log.Remarks = "Salary Fixation Mail Has been send successfully to " + toEmail + "";
                                    _mailLogRepository.Insert(log);
                                    _unitOfWork.SaveChanges();
                                }
                                else
                                {
                                    log.IsSuccess = false;
                                    log.Remarks = "Mail address not found.";
                                    _mailLogRepository.Insert(log);
                                    _unitOfWork.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            log.IsSuccess = false;
                            log.Remarks = "Mail Not send.";
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

        #endregion Salary Fixation Mail
    }
}