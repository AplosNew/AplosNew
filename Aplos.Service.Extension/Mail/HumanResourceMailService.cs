using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using Library.Model.Accounts;
using Library.Service.Core;
using Library.Model.Inventory;
using Library.Service.Systems;
using Library.Data;
using Syncfusion.XlsIO;
using Library.ViewModel.Organizations;
using Library.Core;
using Library.Data.Repositories;
using Library.Model.Setups;
using Library.ViewModel.Setups;
using System.Web.UI.WebControls;

namespace Library.Service.Extension.Mail
{
    //public interface IRrportUtility
    // {
    //     void PageSetup(ref IWorksheet sheet, int xlsColumnHeader, ExcelPageOrientation po);
    //     void PlantHeader(ref IWorksheet sheet, int lastCol, string sheetHeader, string plantId);
    // }
    public class HumanResourceMailService
    {
        SqlRepository _sqlRepository;
        private readonly IRepositoryAsync<MailReceiverDetail> _mailReceiverDetailRepository;
        public HumanResourceMailService(IRepositoryAsync<MailReceiverDetail> mailReceiverDetailRepository)
        {
            _mailReceiverDetailRepository = mailReceiverDetailRepository;
            _sqlRepository = new SqlRepository();
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

        public IEnumerable<OrgStructureListViewModel> GetOrgStructureList(string CompanyGroupId)
        {
            try
            {
                var strSQL = @"  SELECT DISTINCT u.StandardName ColumnName,IsNULL(e.RType,'position') as Rtype from (
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

        public IEnumerable<OrgStructureListViewModel> OrgStructureListColList(string CompanyGroupId, string CompanyId)
        {
            try
            {
                string wcCompanyEntity = "";
                string wcCompanyPosition = "";

                if (!string.IsNullOrEmpty(CompanyId) && CompanyId != "undefined")
                {
                    wcCompanyEntity = " AND CompanyId = '" + CompanyId + @"'";
                    wcCompanyPosition = " AND t.CompanyId = '" + CompanyId + @"'";
                }
                var strSQL = @"SELECT StandardName, UserName ColumnName, RType,Sequence
									   FROM ORG.StructureRelationship
									   WHERE RType = 'Entity'  AND CompanyGroupId = '" + CompanyGroupId + @"' " + wcCompanyEntity + @"
							   UNION
							   SELECT StandardName, UserName ColumnName, RType,Sequence FROM ORG.StructureRelationship  AS k
								      WHERE RType = 'Position' AND NOT EXISTS (
																	SELECT 1
																	FROM ORG.StructureRelationship  AS t
																	WHERE t.standardname = k.standardname
									       AND t.rtype = 'Entity'  AND t.CompanyGroupId = '" + CompanyGroupId + @"' " + wcCompanyPosition + @")
 										    UNION
									 SELECT LN.* FROM (SELECT 'Line' StandardName, 'Line'  ColumnName,'Z' RType, 99 Sequence) AS LN
										INNER JOIN (
										SELECT CASE WHEN ISNULL(m.id,'')='' THEN 'NOLine' else 'Line' END AS HasLine FROM (select 'HasLine' AS Line) AS K
										LEFT OUTER JOIN (SELECT TOP 1 *  FROM ORg.Line) AS M ON 1=1										
										) AS AC ON AC.HasLine=LN.StandardName
										   ORDER BY RType,Sequence";
                DataTable dt = _sqlRepository.GetDataTable(strSQL);
                string id = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (id == dt.Rows[i]["StandardName"].ToString())
                    {
                        id = dt.Rows[i]["StandardName"].ToString();
                        dt.Rows[i].Delete();
                    }
                    else
                    {
                        id = dt.Rows[i]["StandardName"].ToString();
                    }
                }
                dt = dt.DefaultView.ToTable();


                var orgStrctList = (from rw in dt.AsEnumerable()
                                    select new OrgStructureListViewModel()
                                    {
                                        StandardName = rw["StandardName"].ToString(),
                                        ColumnName = rw["ColumnName"].ToString(),
                                        RType = rw["RType"].ToString(),
                                        Sequence = Convert.ToInt32(rw["Sequence"])

                                    }).ToList();

                return orgStrctList;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetProbationPeriodAlertBeforDaysInfo(string groupId, string plantId, out DataSet dsProbationPeriodAlertBeforeDaysInfo)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                var param = string.Empty;
                if (!string.IsNullOrEmpty(groupId) && !string.IsNullOrEmpty(plantId))
                    param = "HS.GroupID='" + groupId + "' AND HS.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(groupId) && string.IsNullOrEmpty(plantId))
                    param = "HS.GroupID='" + groupId + "'";
                parameters.CmdText = @"select
                     ProbationPeriodAlertBeforeDays,IncrementAlertBeforeDays
                    from PlantWiseHRMSSetting as HS where " + param + @"";
                dsProbationPeriodAlertBeforeDaysInfo = _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GetResignationAlertBeforDaysInfo(string groupId, string plantId, out DataSet dsResignationAlertBeforeDaysInfo)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                var param = string.Empty;
                if (!string.IsNullOrEmpty(groupId) && !string.IsNullOrEmpty(plantId))
                    param = "HS.GroupID='" + groupId + "' AND HS.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(groupId) && string.IsNullOrEmpty(plantId))
                    param = "HS.GroupID='" + groupId + "'";
                parameters.CmdText = @"select
                     ResignationAlertBeforeDays
                    from PlantWiseHRMSSetting as HS where " + param + @"";
                dsResignationAlertBeforeDaysInfo = _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region ********************SQLGeneratingFunction***************

        public DataTable GetEmpInfo(string companyGroupId, string plantId)
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
                var cmdText = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
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
                                     ,e.DepartmentId,e.DivisionId,e.LineId
                                    ,e.PlantId,e.UnitId,e.SectionId,e.SubDivisionId,e.SubSectionId,e.DesignationGroupId
                                    --emp info
                                    ,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    ,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
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
                                   -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
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
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
			                                       " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId
                                    LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
                                    WHERE e.EmployeeStatus = 'Active' AND " + param + @"";
                return _sqlRepository.GetDataTable(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetManualAttendanceInfo(string companyGroupId, string plantId)
        {
            var strSql = "";
            try
            {
                strSql = @"Select AM.empsystemid,Convert(int,e.EmployeeCode ) EmployeeCode, EmployeeName, ISNULL(GD.UserName,LD.UserName) Designation,ISNULL(LG.UserName,'') LegalDesignation,u.UserName Unit,D.StandardName Department, 
                                            s.UserName Section,sb.UserName Sub_section, L.USERNAME Line,  REPLACE(CONVERT(VARCHAR(11), AM.WorkDate, 113), ' ', '-') WorkDate, 
                                            REPLACE(CONVERT(VARCHAR(11), AM.DateAdded, 113), ' ', '-') EntryDate,
											A.DayStatus, 
											CONVERT(varchar(15),CAST(A.InTime AS TIME),100) InTime,
											CONVERT(varchar(15),CAST(A.OutTime AS TIME),100) OutTime,IsManualDayStatus, IsManualInTime, IsManualOutTime
                                            FROM EmployeeInformation E
                                            LEFT JOIN ORG.Department D on D.Id = E.DepartmentId 
                                            LEFT JOIN AttdnProcessData A on A.EmpSystemID = E.SystemId
                                            LEFT JOIN AttdnManualData AM on AM.EmpSystemID = E.SystemId
											And AM.WorkDate = A.WorkDate
                                            LEFT JOIN ORG.Section S ON E.SectionID = S.Id
                                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                                            LEFT JOIN ORG.Unit u on u.Id=e.UnitId
                                            LEFT JOIN ORG.Line L on L.Id= E.LineId
                                            LEFT JOIN HKP.Designation LD on E.LegalDesignationId = LD.Id
                                            LEFT JOIN HKP.Designation GD on  E.GivenDesignationId = GD.Id  
                                            LEFT JOIN HKP.LegalDesignation LG on  E.LegalDesignationId = LG.Id  
                                            WHERE 
											(IsManualDayStatus=1 or IsManualInTime=1 or IsManualOutTime=1) and 
											CONVERT(date,AM.DateAdded) = Convert(Date,(GetDate()-1)) AND E.PlantID = '" + plantId + @"' AND E.GroupID = '" + companyGroupId + @"'                                            
										    ORDER BY EmployeeCode";
                return _sqlRepository.GetDataTable(strSql);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void GetIncrementEmpInfo(string companyGroupId, string plantId, out DataSet dsEmpInfo)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
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
                parameters.CmdText = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
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
                                    ,mpb.EntityId,mpb.PositionId,hs.IsPositionCodeApplicable
									--Increment Due list
									,DATEDIFF(day, GETDATE(), (sindd.NextDueDate)) IncDaysToGO
									,Tem.NextDueDate sIncrementNextDueDate
									,REPLACE(CONVERT(VARCHAR(11), sindd.NextDueDate, 106), ' ', '-') IncrementNextDueDate
									,REPLACE(CONVERT(VARCHAR(11), sindd.EffectiveDate, 106), ' ', '-') IncrementEffectiveDate
                                    --emp ids
                                     ,e.DepartmentId,e.DivisionId,e.LineId
                                    ,e.PlantId,e.UnitId,e.SectionId,e.SubDivisionId,e.SubSectionId,e.DesignationGroupId
                                    --emp info
                                    ,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    ,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
									       " + cList + @"
                                    FROM EmployeeInformation e
                                    LEFT OUTER JOIN org.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN org.Line eL on eL.id=e.LineId
                                    LEFT OUTER JOIN org.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN org.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN org.Section es on es.id=e.SectionId
                                    LEFT OUTER JOIN org.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN org.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN org.Unit eu on eu.id=e.UnitId
                                    -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN hkp.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN hkp.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN hkp.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN hkp.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    LEFT OUTER JOIN (SELECT dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN hkp.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                            -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=dm.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN mst.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
			                                            " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId
                                   	LEFT OUTER JOIN
									(select Max(NextDueDate) NextDueDate,EmpSystemId
									from DBO.SalaryIncrementNextDueDate
									group By EmpSystemId
									) Tem on Tem.EmpSystemId = e.SystemId
									LEFT OUTER  JOIN dbo.SalaryIncrementNextDueDate sindd on sindd.Id=
									(
										SELECT top 1 Id 
										--,  EffectiveDate
									--,EmpSystemId,NextDueDate  
									FROM     DBO.SalaryIncrementNextDueDate
									WHERE    EmpSystemId = e.SystemId and Convert(date,EffectiveDate) < Convert(Date,GETDATE()) order by EffectiveDate DESC
									)
								    --LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
								    LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
                                    where " + param + @"
									AND e.EmployeeStatus = 'Active'";
                dsEmpInfo = _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetAttendanceFromAppInfo(string companyGroupId, string plantId)
        {

            try
            {
                string sqlTxt = string.Empty;
                ////parameters = new GridParameter
                //{
                //    ExportType = "DATASET";
                //};
                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

                var OrgStrList = GetOrgStructureList(companyGroupId);
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
                sqlTxt = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
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

                                    ,mpb.EntityId,mpb.PositionId
					
                                    --emp ids
                                     ,e.DepartmentId,e.DivisionId,e.LineId
                                    ,e.PlantId,e.UnitId,e.SectionId,e.SubDivisionId,e.SubSectionId,e.DesignationGroupId
                                    --emp info
                                    ,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    ,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
                                    ,PDate=REPLACE(CONVERT(VARCHAR(11),PDate, 106), ' ', '-') 
									,inTime=FORMAT( inTime, 'hh.mm tt')
									,OutTime=FORMAT( OutTime, 'hh.mm tt')
									,ISNULL(AttdnAPP.Latitude,'') Latitude,ISNULL(AttdnAPP.Longitude,'') Longitude,ISNULL(AttdnAPP.INLocationDesc,'') INLocationDesc,ISNULL(AttdnAPP.OutLocationDesc,'') OutLocationDesc
                                     " + cList + @"
                                    FROM EmployeeInformation e
                                    INNER JOIN dbo.AttdnRawDataFromApp AttdnAPP ON E.SystemID = AttdnAPP.EmployeeId
                                    
                                    LEFT JOIN org.Unit U ON E.UnitID = U.Id
                                    LEFT OUTER JOIN org.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN org.Line eL on eL.id=e.LineId
                                    LEFT OUTER JOIN org.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN org.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN org.Section es on es.id=e.SectionId
                                    LEFT OUTER JOIN org.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN org.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN org.Unit eu on eu.id=e.UnitId
                                    -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN hkp.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN hkp.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN hkp.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN hkp.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    LEFT OUTER JOIN (SELECT dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN hkp.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                            -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=dm.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN mst.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
			                                            " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId
                                  
								    LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
                                    WHERE " + param + @"
                                    AND AttdnAPP.PDate = '" + DateTime.Now.AddDays(-1).ToString("dd-MMM-yyyy") + @"' AND e.EmployeeStatus = 'Active'
                                    --AND AttdnAPP.PDate = '" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' AND e.EmployeeStatus = 'Active'
									";
                return _sqlRepository.GetDataTable(sqlTxt);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetUnApproveEmployeeInfo(string empId,string companyGroupId, string plantId)
        {

            try
            {
                string sqlTxt = string.Empty;
                sqlTxt = @"SELECT EI.SystemID,EI.EmployeeCode, EI.EmployeeName,
	                        Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOB,
	                        Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJ
   	                        ,DP.UserName Department, PR.UserName PositionName,	E.UserName EntityName,
	                        DSG.UserName Designation, se.UserName Section, Sus.UserName SubSection,
	                        LGD.userName LegalDesignation,PMB.Code,ISNULL(PG.UserName,'') PayrollGroup,EC.UserName EmployeeCategory
	                        ,EI.ApprovalAuthorityId,ATH.EmailId
                        FROM dbo.Employeeinformation EI                             
                        LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
                        LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id						
                        LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                        LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                        LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
                        LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD on LGD.Id=EI.LegalDesignationId							  
                        LEFT JOIN ORG.Section AS Se ON Se.Id= PR.SectionID 
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID 
                        LEFT JOIN  [MST].[PayrollGroupMaster] PGM ON PGM.EmployeeId=EI.SystemId
                        LEFT JOIN  [HKP].[PayrollGroup] PG ON PG.Id=PGM.PayrollGroupId
                        LEFT JOIN (
                                    SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
			                        LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
	                        )EC ON EC.DesignationId=EI.GivenDesignationId
	                        LEFT JOIN dbo.Employeeinformation ATH ON ATH.SystemId=EI.ApprovalAuthorityId
                        WHERE EI.EmployeeStatus !='Separated' AND EI.IsApproved =0 AND EI.PlantId='"+ plantId + "' AND  EI.GroupId='"+ companyGroupId + "' AND EI.ApprovalAuthorityId='"+empId+"'";
                return _sqlRepository.GetDataTable(sqlTxt);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetMissedPunchInfo(string companyGroupId, string plantId)
        {

            try
            {
                string sqlTxt = string.Empty;
                ////parameters = new GridParameter
                //{
                //    ExportType = "DATASET";
                //};
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
                sqlTxt = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
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

                                    ,mpb.EntityId,mpb.PositionId,hs.IsPositionCodeApplicable
					
                                    --emp ids
                                     ,e.DepartmentId,e.DivisionId,e.LineId
                                    ,e.PlantId,e.UnitId,e.SectionId,e.SubDivisionId,e.SubSectionId,e.DesignationGroupId
                                    --emp info
                                    ,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    ,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
                                         ,AD.WorkDate PDate, AD.DayStatus ,CONVERT(VARCHAR(5), AD.InTime, 108) InTime
								, SD.ShiftDefinationName ShiftName ,CONVERT(varchar(15),CAST(SD.InTime AS TIME),100) ShiftInTimeShow  ,CONVERT(varchar(15),CAST(AD.InTime AS TIME),100) InTimeShow
								, CONVERT(VARCHAR(5), AD.OutTime, 108) OutTime
                                    ,CONVERT(varchar(15),CAST(AD.OutTime AS TIME),100) OutTimeShow, CONVERT(VARCHAR(15),CAST(LIT.ptime AS TIME),100)  +' ('+ ARD.PType+')' LeastPunchTime
									       " + cList + @"
                                    FROM EmployeeInformation e
                                     INNER JOIN dbo.AttdnProcessData AD ON E.SystemID = AD.EmpSystemID
							                LEFT JOIN (SELECT * FROM dbo.ShiftTimeChgMaster WHERE '01-Apr-2019' BETWEEN FromDate AND ToDate) AS SFCG
																                ON AD.ShiftSystemID = SFCG.ShiftDefinationID
							                LEFT JOIN dbo.ShiftDefination SD ON AD.ShiftSystemID = SD.SystemID
							                LEFT JOIN dbo.AttdnRawData ARIN ON AD.InTimeRowID = ARIN.RowID
							                LEFT JOIN dbo.AttdnRawData AROUT ON AD.OutTimeRowID = AROUT.RowID
                                            LEFT JOIN
												(
												SELECT LogDownLoadNum
												,min(ptime) ptime
												from AttdnRawData
												where pdate=CONVERT(DATE,GetDate()-1) --and PType='IN'--and LogDownLoadNum='1800004'
												group by LogDownLoadNum
												) LIT on LIT.LogDownLoadNum=E.SystemId
                                            LEFT JOIN AttdnRawData ARD ON ARD.LogDownLoadNum=LIT.LogDownLoadNum  AND ARD.PTime=LIT.ptime
							                                       LEFT JOIN org.Unit U ON E.UnitID = U.Id
                                    LEFT OUTER JOIN org.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN org.Line eL on eL.id=e.LineId
                                    LEFT OUTER JOIN org.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN org.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN org.Section es on es.id=e.SectionId
                                    LEFT OUTER JOIN org.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN org.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN org.Unit eu on eu.id=e.UnitId
                                    -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN hkp.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN hkp.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN hkp.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN hkp.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    LEFT OUTER JOIN (SELECT dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN hkp.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                            -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=dm.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN mst.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
			                                            " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId
                                  
								    LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
                                    WHERE " + param + @"
                                      AND CONVERT(DATE,AD.WorkDate)  = CONVERT(DATE,GETDATE()-1) AND E.EmployeeStatus='Active' 
                                           AND (  (AD.InTime IS NULL AND AD.OutTime IS NOT NULL) OR (AD.OutTime IS NULL AND AD.InTime IS NOT NULL))
                                AND AD.DayStatus <>'W'
									AND e.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataTable(sqlTxt);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetYesterdayMissedPunchInfoDetailWithSheets(string companyGroupId, string plantId, string attdnStatus, string attdnDate)
        {

            try
            {

                var wc = "";
                if (attdnStatus.ToUpper() == "ABSENT")
                {
                    wc = @" And AD.InTime IS NULL AND AD.OutTime IS NULL and AD.DayStatus ='A'

                                           and ARIN.PTime IS NULL and AROUT.PTime is null ";
                }

                if (attdnStatus.ToUpper().Replace(" ", "") == "INPUNCHMISSING")
                {
                    wc = @"And AD.InTime IS NULL AND AD.OutTime IS not NULL and AD.DayStatus ='A'
										   and ARIN.PTime IS NULL and AROUT.PTime is not null  ";
                }

                if (attdnStatus.ToUpper().Replace(" ", "") == "OUTPUNCHMISSING")
                {
                    wc = @"And AD.InTime IS not NULL AND AD.OutTime IS NULL and AD.DayStatus ='P'
										   and ARIN.PTime IS not NULL and AROUT.PTime is null ";
                }
                if (attdnStatus.ToUpper().Replace(" ", "") == "ABSENTMARGIN")
                {
                    wc = @"And ((AD.InTime IS NOT NULL AND AD.OutTime IS NOT NULL and AD.DayStatus ='A')
										   OR
						        (AD.InTime IS NOT NULL AND AD.OutTime IS NULL and AD.DayStatus ='A'))";
                }

                string sqlTxt = string.Empty;
                ////parameters = new GridParameter
                //{
                //    ExportType = "DATASET";
                //};
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
                sqlTxt = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
                                    --,e.DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB
                                    ,REPLACE(CONVERT(VARCHAR(11), e.ApprovedDateTime, 106), ' ', '-') ApprovedDateTime
                                    ,CONVERT(date, e.ApprovedDateTime) vApprovedDateTime
									,ISNULL(e.IsApproved,0) IsApproved
                                    , EmpC.UserName empCategory,ELoc.UserName EmployeeLocation
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

                                    ,mpb.EntityId,mpb.PositionId,hs.IsPositionCodeApplicable
					
                                    --emp ids
                                     ,e.DepartmentId,e.DivisionId,e.LineId
                                    ,e.PlantId,e.UnitId,e.SectionId,e.SubDivisionId,e.SubSectionId,e.DesignationGroupId
                                    --emp info
                                    ,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    ,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
                                         ,AD.WorkDate PDate, AD.DayStatus ,CONVERT(VARCHAR(5), AD.InTime, 108) InTime
								, SD.ShiftDefinationName ShiftName ,CONVERT(varchar(15),CAST(SD.InTime AS TIME),100) ShiftInTimeShow  ,CONVERT(varchar(15),CAST(AD.InTime AS TIME),100) InTimeShow
								, CONVERT(VARCHAR(5), AD.OutTime, 108) OutTime
                                    ,CONVERT(varchar(15),CAST(AD.OutTime AS TIME),100) OutTimeShow, CONVERT(VARCHAR(15),CAST(LIT.ptime AS TIME),100)  +' ('+ ARD.PType+')' LeastPunchTime
									       " + cList + @"
                                    FROM EmployeeInformation e
                                     INNER JOIN dbo.AttdnProcessData AD ON E.SystemID = AD.EmpSystemID
							                LEFT JOIN (SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + attdnDate + @"' BETWEEN FromDate AND ToDate) AS SFCG
																                ON AD.ShiftSystemID = SFCG.ShiftDefinationID
							                LEFT JOIN dbo.ShiftDefination SD ON AD.ShiftSystemID = SD.SystemID
							                LEFT JOIN dbo.AttdnRawData ARIN ON AD.InTimeRowID = ARIN.RowID
							                LEFT JOIN dbo.AttdnRawData AROUT ON AD.OutTimeRowID = AROUT.RowID
                                            LEFT JOIN
												(
												SELECT LogDownLoadNum
												,min(ptime) ptime
												from AttdnRawData
												where pdate='" + attdnDate + @"' --and PType='IN'--and LogDownLoadNum='1800004'
												group by LogDownLoadNum
												) LIT on LIT.LogDownLoadNum=E.SystemId
                                            LEFT JOIN AttdnRawData ARD ON ARD.LogDownLoadNum=LIT.LogDownLoadNum  AND ARD.PTime=LIT.ptime
							                                       LEFT JOIN org.Unit U ON E.UnitID = U.Id
                                    LEFT OUTER JOIN org.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN org.Line eL on eL.id=e.LineId
                                    LEFT OUTER JOIN org.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN org.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN org.Section es on es.id=e.SectionId
                                    LEFT OUTER JOIN org.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN org.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN org.Unit eu on eu.id=e.UnitId
                                    -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN hkp.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN hkp.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN hkp.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN hkp.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    
						
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
                                    LEFT OUTER JOIN mst.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT OUTER JOIN HKP.EmployeeLocation ELoc on mpb.EmployeeLocationId=ELoc.Id
			                                            " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId
                                  
								    LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
                                    WHERE " + param + @"
                                      AND CONVERT(DATE,AD.WorkDate)  = '" + attdnDate + @"' AND E.EmployeeStatus='Active' 
                                           " + wc + @"
                                AND AD.DayStatus NOT IN (Select Category from DayType where Category in ('Holiday', 'Weekend'))
									AND e.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataTable(sqlTxt);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable GetDailyAttendanceEmpInfo(string companyGroupId, string companyId, string plantId, string dayStatus, string attendanceDate)
        {
            try
            {
                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

                var OrgStrList = OrgStructureListColList(companyGroupId, companyId);
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
                    if (item.RType == "Position")
                    {
                        cList += "," + item.ColumnName + ".UserName " + "p" + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = PO." + item.ColumnName + "Id\n";
                    }
                    if (item.RType == "Z")//For Line
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = MPB." + item.ColumnName + "Id\n";
                    }
                }
                var cmdText = @"SELECT e.SystemId,e.EmployeeId,DT.DayType,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
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
                                     ,e.DepartmentId,e.DivisionId,e.LineId
                                    ,e.PlantId,e.UnitId,e.SectionId,e.SubDivisionId,e.SubSectionId,e.DesignationGroupId
                                    --emp info
                                    --,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    --,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
                                    ,EmpC.UserName empCategory,EmpC.Sequence CatgSequence,LT.UserName LeaveType,ELoc.UserName EmployeeLocation
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
                                   -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
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
			                        LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId                                    
									LEFT OUTER JOIN LeaveType LT ON LT.Id = APD.LTSystemID

									LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
                                    WHERE e.EmployeeStatus = 'Active' AND " + param + @" ";

                if (dayStatus == "Work Off")
                {
                    cmdText += "AND  DT.DayType IN ('W','H','WP','HP','WA','HA')";
                }
                else
                {
                    cmdText += "AND  DT.Category = '" + dayStatus + "'";
                }
                cmdText += "AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + attendanceDate + @"') ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric ASC";
                return _sqlRepository.GetDataTable(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable GetDailyAttendanceEmp(string companyGroupId, string companyId, string plantId, string dayStatus, string attendanceDate, string shift, string Entity, string Dept, string Ydate, string Sec, string SSec, string empCategoryList, string designationList, string lineList, string Dstatus)
        {
            try
            {
                string shiftInfo = string.Empty;
                string DeptName = string.Empty;
                string Section = string.Empty;
                string SubSection = string.Empty;
                string EmpCat = string.Empty;
                string DesList = string.Empty;
                string Line = string.Empty;
                string DayS = string.Empty;
                if (string.IsNullOrEmpty(shift) || shift.ToUpper() == "ALL")
                {

                }
                else
                {
                    shiftInfo = " and APD.ShiftSystemID = '" + shift + "'";
                }
                if (string.IsNullOrEmpty(Dept) || Dept.ToUpper() == "ALL")
                {

                }
                else
                {
                    DeptName = " and Department.Id In (" + Dept + ")";
                }
                if (string.IsNullOrEmpty(Sec) || Sec.ToUpper() == "ALL")
                {

                }
                else
                {
                    Section = " and Section.Id In ( " + Sec + ")";
                }
                if (string.IsNullOrEmpty(SSec) || SSec.ToUpper() == "ALL")
                {

                }
                else
                {
                    SubSection = " and SubSection.Id In ( " + SSec + ")";
                }

                if (string.IsNullOrEmpty(empCategoryList) || empCategoryList.ToUpper() == "ALL")
                {

                }
                else
                {
                    EmpCat = " and EmpC.Id In ( " + empCategoryList + ")";
                }

                if (string.IsNullOrEmpty(designationList) || designationList.ToUpper() == "ALL")
                {

                }
                else
                {
                    DesList = " and ld.Id In ( " + designationList + ")";
                }
                if (string.IsNullOrEmpty(lineList) || lineList.ToUpper() == "ALL")
                {

                }
                else
                {
                    Line = " and isnull(eL.Id,'') in ( " + lineList + ")";
                }

                //if (Dstatus != null)
                //{
                //    if (Dstatus.ToUpper() != "ALL" && Dstatus != "null" && Dstatus != "" && Dstatus != "''")
                //    {
                //        if (Dstatus == "'Other'")
                //        {
                //            DayS = " and DT.Category not in( 'Present','Late','Absent','Leave','Weekend')";
                //        }
                //        else
                //        {
                //            DayS = " and DT.Category in (" + Dstatus + ")";
                //        }
                //    }
                //}

                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

                var OrgStrList = OrgStructureListColList(companyGroupId, companyId);
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
                    if (item.RType == "Position")
                    {
                        cList += "," + item.ColumnName + ".UserName " + "p" + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = PO." + item.ColumnName + "Id\n";
                    }
                    if (item.RType == "Z")//For Line
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = MPB." + item.ColumnName + "Id\n";
                    }
                }
                var cmdText = @"SELECT e.SystemId,e.EmployeeId,DT.DayType,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName,edept.UserName Depertment,EN.UserName Entity
                                    --,e.DOJ
                                    ,FORMAT(CAST(APD.InTime AS datetime2), N'hh:mm tt')Intime
                                    , e.FatherName FatherName,sdf.UserName ShiftName 
                                    ,FORMAT(CAST(sdf.InTime AS datetime2), N'hh:mm tt')  ShiftInTime
									,FORMAT(CAST(sdf.OutTime AS datetime2), N'hh:mm tt') ShiftOutTime
									,Lc CurrentMonthLate
									,lcA CurrentMonthAbsent
									,APDY.DayStatus YesterdayDaystatus
									,APDY.OTHr YesterdayOverStayHour
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
                                    --,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    --,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
                                    ,EmpC.UserName empCategory,EmpC.Sequence CatgSequence,LT.UserName LeaveType,ELoc.UserName EmployeeLocation
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
                                   -- left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
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
			                        LEFT OUTER JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId  
                                   -- left join ShiftDefination sdf on sdf.SystemId = 
									LEFT OUTER JOIN LeaveType LT ON LT.Id = APD.LTSystemID
                                    LEFT OUTER JOIN AttdnProcessData APDY ON APDY.EmpSystemID = E.SystemId  and APDY.WorkDate='" + Ydate + @"'
                                    left join ShiftDefination sdf on sdf.SystemId = APD.ShiftSystemID
                                    left join (
									select count(atdnd.WorkDate)Lc, atdnd.EmpSystemID from AttdnProcessData atdnd
									left join DayType DT on DT.DayType= atdnd.DayStatus
									where MONTH(atdnd.WorkDate)=MONTH('" + attendanceDate + @"')  and dt.Category = 'Late'
									Group By EmpSystemID
									) lc on lc.EmpSystemID = E.SystemID
									left join (
									select count(atdnd.WorkDate)LcA, atdnd.EmpSystemID from AttdnProcessData atdnd
									left join DayType DT on DT.DayType= atdnd.DayStatus
									where MONTH(atdnd.WorkDate)=MONTH('" + attendanceDate + @"')  and dt.Category = 'Absent'
									Group By EmpSystemID
									) lcA on lcA.EmpSystemID = E.SystemID

									LEFT OUTER JOIN DayType DT ON DT.DayType = APD.DayStatus
                                    WHERE e.EmployeeStatus = 'Active' " + shiftInfo + " " + Section + " " + SubSection + " " + EmpCat + " " + DesList + " " + Line + " and EN.Id in (" + Entity + @") " + DeptName + "  AND " + param + @" ";

                if (dayStatus == "Work Off")
                {
                    cmdText += " AND  DT.DayType IN ('W','H','WP','HP','WA','HA')";
                }
                else
                {
                    cmdText += " AND  DT.Category = '" + dayStatus + "'";
                }
                cmdText += " AND CONVERT(DATE, APD.WorkDate) = CONVERT(DATE,'" + attendanceDate + @"') ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric ASC";
                return _sqlRepository.GetDataTable(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetDAttendanceEmployee(string companyGroupId, string companyId, string plantId, string dayStatus, string attendanceDate, string shift, string Entity, string Dept, string Ydate, string Sec, string SSec, string empCategoryList, string designationList, string lineList, string Dstatus, string JobLocation)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string secSQL = string.Empty;
            string xxy = string.Empty;
            string XJobLocation = string.Empty;
            clsStaticInfo obs = null;
            string ShiftIds_WC = "";
            try
            {
                string xx = "'" + dayStatus.Replace('"', ' ').Trim() + "'";

                if (shift != "ALL" && shift != "''")
                {
                    ShiftIds_WC = " and sd.SystemID in ('" + shift + "') ";
                }

                //if (xx == "'Other'")
                //{
                //    xxy += " AND  dt.Category in( 'Half Day','Holiday','Working Day')";
                //}
                //else
                //{
                xxy += " AND  DT.Category = " + xx + "";
                XJobLocation += " And J.SystemID in ('" + JobLocation + "')";
                //}

                obs = new clsStaticInfo();
                strSql = @" select e.SystemId
                                            from EmployeeInformation e
                                            left join mst.ManpowerBudget mp on mp.id=e.BudgetCode
											left join org.Entity en on en.id=mp.EntityId    
											left join ORG.Position p on p.Id = mp.PositionId
											left join org.Department dep on dep.Id = p.DepartmentId
											left join org.Section s on s.Id = p.SectionId
											left join org.SubSection ss on ss.Id = p.SubSectionId                                       
                                            LEFT JOIN org.Line L ON L.Id = mp.LineId
                                            LEFT JOIN hkp.LegalDesignation LG ON e.LegalDesignationId = LG.Id 
											left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id
											left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId
											left join HKP.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId
											where   e.PlantId='" + plantId + @"' and e.DOJ <= ( '" + attendanceDate + @"') and (e.DOS is null or e.DOS >= '" + attendanceDate + @"')";


                if (Dept != "ALL")
                {
                    strSql = strSql + @" AND dep.Id in ( '" + Dept + "')";
                }
                if (Sec != "ALL")
                {
                    strSql = strSql + @" AND s.Id in ('" + Sec + "')";
                }
                if (SSec != "ALL")
                {
                    strSql = strSql + @" AND ss.Id in ('" + SSec + "')";
                }

                if (empCategoryList != "ALL" && !string.IsNullOrEmpty(empCategoryList))
                {
                    strSql = strSql + @" AND ec.Id in ('" + empCategoryList + "')";
                }

                if (Entity != "ALL")
                {
                    strSql = strSql + @" AND en.Id in ('" + Entity + "')";
                }
                if (lineList != "ALL" && lineList != "''" && !string.IsNullOrEmpty(lineList))
                {
                    strSql = strSql + @" AND isnull(L.Id,'') in ('" + lineList + "')";
                }
                if (designationList != "ALL" && designationList != "''" && !string.IsNullOrEmpty(designationList))
                {
                    strSql = strSql + @" AND LG.Id in ('" + designationList + "')";
                }

                secSQL = @"SELECT e.SystemId,e.EmployeeCode,e.FatherName,el.UserName [Location]
								,dep.username Department,en.UserName Entity,ec.UserName empCategory
                                , e.EmployeeName,Lc CurrentMonthLate,LT.UserName LeaveType
									,lcA CurrentMonthAbsent
								,sd.UserName ShiftName
                                , ShiftIn  = CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100)
						     END
								,ShiftOut = CASE                                   
                           WHEN cs.OutTime IS NULL
                           THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
                           ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                           END
                                , FORMAT(CAST(ap.InTime AS datetime2), N'hh:mm tt') InTime
								,FORMAT(CAST( ap.OutTime AS datetime2), N'hh:mm tt') OutTime
	                            ,  REPLACE(CONVERT(VARCHAR(11), ap.WorkDate, 113), ' ', '-') PDate
	                            , ap.DayStatus
	                            , ap.OTHr TodaysOT
                        , LG.UserName Designation
                         , kk.PrvDayStatus
						,kk.YesterdayOTHr,ap.IsManualInTime,ap.IsManualOutTime,hr.OTConsiderOn

                        from EmployeeInformation e

                        left join AttdnProcessData ap on ap.EmpSystemID = e.SystemId
                        left join DayType DT on DT.DayType = ap.DayStatus
                        LEFT JOIN LeaveType LT ON LT.Id = AP.LTSystemID
                        LEFT JOIN dbo.ShiftDefination SD ON ap.ShiftSystemID = SD.SystemID
                        LEFT OUTER JOIN ShiftTimeChgMaster AS cs ON ap.WorkDate BETWEEN cs.FromDate AND cs.ToDate AND sd.SystemID=cs.ShiftDefinationID
                                            left join mst.ManpowerBudget mp on mp.id = e.BudgetCode
                                            left join hkp.EmployeeLocation el on el.Id = mp.EmployeeLocationId
                                            left join org.Entity en on en.id = mp.EntityId

                                            left join ORG.Position p on p.Id = mp.PositionId

                                            left join org.Department dep on dep.Id = p.DepartmentId

                                            left join org.Section s on s.Id = p.SectionId
                                            LEFT JOIN PlantWiseHRMSSetting hr on HR.PlantID=e.PlantId
                                            left join org.SubSection ss on ss.Id = p.SubSectionId
                                            LEFT JOIN org.Line L ON L.Id = mp.LineId
                                            LEFT JOIN hkp.LegalDesignation LG ON e.LegalDesignationId = LG.Id

                                            left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id

                                            left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId

                                            left join HKP.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId

                                            LEFT JOIN JobLocation J ON J.SystemID = e.JobLocationID

                                    left join (
									select count(atdnd.WorkDate)Lc, atdnd.EmpSystemID from AttdnProcessData atdnd
									left join DayType DT on DT.DayType= atdnd.DayStatus
									where MONTH(atdnd.WorkDate)=MONTH('" + attendanceDate + @"') and YEAR(atdnd.WorkDate)=YEAR('" + attendanceDate + @"')  and dt.Category = 'Late'
									Group By EmpSystemID
									) lc on lc.EmpSystemID = E.SystemID
									left join (
									select count(atdnd.WorkDate)LcA, atdnd.EmpSystemID from AttdnProcessData atdnd
									left join DayType DT on DT.DayType= atdnd.DayStatus
									where MONTH(atdnd.WorkDate)=MONTH('" + attendanceDate + @"') and YEAR(atdnd.WorkDate)=YEAR('" + attendanceDate + @"')  and dt.Category = 'Absent'
									Group By EmpSystemID
									) lcA on lcA.EmpSystemID = E.SystemID

                                            left join(select yap.DayStatus PrvDayStatus, yap.OTHr YesterdayOTHr, yap.EmpSystemID from AttdnProcessData yap where yap.WorkDate = '" + Ydate + @"') kk on kk.EmpSystemID = e.SystemId

                                  where  ap.WorkDate='" + attendanceDate + @"' and e.SystemId in (" + strSql + ")  " + ShiftIds_WC + " " + xxy + " " + XJobLocation + "";


                return _sqlRepository.GetDataTable(secSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }



        public Dictionary<string, List<DataRow>> GetMontlyAttdnSummary(string companyGroupId, string plantId, string attendanceDate)
        {
            try
            {
                Dictionary<string, List<DataRow>> dicMonthlyAttdnSummary = new Dictionary<string, List<DataRow>>();
                var cmdText = @"SELECT [EmpSystemID], [MonthNo], [YearNo], ADMS.[GroupID], ADMS.[PlantID], [FromDate], [ToDate], [TotalProcDate]
                            ,[TotalPresent], [TotalLate], [TotalAbsent], [TotalLv], [TotalMLv], [TotalCompAssignLv], [TotalWeekOff]
                            , [TotalHoliDay], [TotalWeekOffHoliDay], [TotalOTHr], [TotalNormalOTHr], [TotalExtraOTHr], [IsDisbusted]
                            , [TotalLWP]
                            FROM [dbo].[AttdnDataMonthlySummary] ADMS 
                            INNER JOIN EmployeeInformation EEI ON ADMS.EmpSystemID = EEI.SystemId
                         where MonthNo = MONTH('" + attendanceDate + @"') and YearNo = YEAR('" + attendanceDate + @"')
                        Order by EmployeeCodePreFix,EmployeeCodeNumeric";

                DataTable dt = _sqlRepository.GetDataTable(cmdText);
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicMonthlyAttdnSummary.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpSystemID"].ToString();
                }

                return dicMonthlyAttdnSummary;

            }
            catch (Exception)
            {
                throw;
            }
        }

        //private void GetDailyAttendaceNotification(string companyGroupId, string plantId, out DataSet dsEmpInfo)
        //{
        //	try
        //	{
        //	}

        //	catch (Exception)
        //	{
        //		throw;
        //	}
        //}

        public void GetSeparatedEmpInfo(string companyGroupId, string plantId, out DataSet dsEmpInfo)
        {
            GridParameter parameters = null;
            try
            {
                parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
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
                parameters.CmdText = @"SELECT e.SystemId,e.EmployeeId,e.EmployeeCode,mpb.Code BudgetCode,e.EmployeeName
                                    --,e.DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-') DOJ
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOB, 106), ' ', '-') DOB
                                    ,REPLACE(CONVERT(VARCHAR(11), e.ApprovedDateTime, 106), ' ', '-') ApprovedDateTime
                                    ,CONVERT(date, e.ApprovedDateTime) vApprovedDateTime
									,ISNULL(e.IsApproved,0) IsApproved
                                    --Resignation
								    ,ISNULL(RSG.ApprovalStatus,'') rsgApprovalStatus
									,REPLACE(CONVERT(VARCHAR(11), rsg.ApprovedEffectiveDate, 106), ' ', '-') resignationApprovedEffectiveDate
                                    ,CONVERT(DATE, rsg.ApprovedEffectiveDate) resignationApprovedEffectiveDateS
									,REPLACE(CONVERT(VARCHAR(11), rsg.ResignationDate, 106), ' ', '-') ApplicantResignationDate
                                    ,REPLACE(CONVERT(VARCHAR(11), e.DOSDate, 106), ' ', '-') ApplicantSeparationDate
								    ,CONVERT(DATE, rsg.ApprovedDate) resignationApprovedEntryDateS
									,CONVERT(date,rsg.AddedDate) resignationAddedDate
								    ,CONVERT(DATE, rsg.EffectiveDate) EffectiveDateS
									,CONVERT(DATE, e.DOSDate) ApplicantSeparationDateEX
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

                                    ,mpb.EntityId,mpb.PositionId,hs.IsPositionCodeApplicable
                                    --emp ids
                                     ,e.DepartmentId,e.DivisionId,e.LineId
                                    ,e.PlantId,e.UnitId,e.SectionId,e.SubDivisionId,e.SubSectionId,e.DesignationGroupId
                                    --emp info
                                    ,edept.UserName Department,eL.UserName Line,ediv.UserName Division,esdiv.UserName Subdivision
                                    ,eu.UserName Unit,ep.UserName Plant
                                    ,ess.UserName Subsection,es.UserName Section
									,edsg.UserName Designation,edsgg.UserName DesignationGroup
									,egdsg.UserName GivenDesignation,egdsgg.GivenDesignationGroup
                                    --,srm.SalaryRuleName,egdsgg.SalaryRuleName GivenSalaryRuleName
                                    ,ld.UserName LegalDesignation
									,PO.Code PositionCode,EN.Code EntityCode
											" + cList + @"
                                    FROM EMPLOYEEINFORMATION E
                                    LEFT OUTER JOIN org.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN org.Line eL on eL.id=e.LineId
                                    LEFT OUTER JOIN org.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN org.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN org.Section es on es.id=e.SectionId
                                    LEFT OUTER JOIN org.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN org.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN org.Unit eu on eu.id=e.UnitId
                                    --left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=e.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
                                    LEFT OUTER JOIN hkp.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN hkp.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN hkp.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN hkp.LegalDesignation  ld on ld.Id=e.LegalDesignationId

                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									from mst.DesignationMaster dm
									left outer join hkp.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                            --left outer join [ORG].[PlantDesignationGroupSalaryRule] srs on srs.DesignationGroupId=dm.DesignationGroupId
                                    --left outer join SalaryRuleMaster srm on srm.SystemId=srs.SalaryRuleMasterId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									and egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN mst.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
			                            " + Join + @"
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    LEFT OUTER JOIN PlantWiseHRMSSetting hs on hs.PlantID=e.PlantId
                                    LEFT OUTER JOIN TRN.Resignation rsg on rsg.EmployeeId=e.SystemId
                                    LEFT OUTER JOIN dbo.PreRecruitmentEmployee pre on pre.Id=e.PreRecruitmentEmployeeId
								    where " + param + @" ";
                dsEmpInfo = _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable GetAttendanceSummarySql(ParamList para, string workDate, string sUnitID, string sDivID, string sDepID, string sSecID, string sSubSecID)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            clsStaticInfo obs = null;
            try
            {
                obs = new clsStaticInfo();
                string wc = string.Empty;
                //if (para.JobLocationId != "ALL")
                //{
                //    wc = " and JobLocationID='" + para.JobLocationId + "' ";
                //}
                if (sUnitID != "ALL")
                {
                    wc = wc + @" AND UnitId = '" + sUnitID + "'";
                }
                if (sDivID != "ALL")
                {
                    wc = wc + @" AND DivisionId = '" + sDivID + "'";
                }
                if (sDepID != "ALL")
                {
                    wc = wc + @" AND DepartmentId = '" + sDepID + "'";
                }
                if (sSecID != "ALL")
                {
                    wc = wc + @" AND SectionId = '" + sSecID + "'";
                }
                if (sSubSecID != "ALL")
                {
                    wc = wc + @" AND SubSectionId = '" + sSubSecID + "'";
                }
                obs = new clsStaticInfo();
                strSql = @"SELECT distinct OnRoleEmployee.CompanyId CompanyId,OnRoleEmployee.CompanyName ColumnName,OnRoleEmployee.GroupName GroupName,OnRoleEmployee.CompanyGroupId CompanyGroupId
                             ,OnRoleEmployee.EmpCategory,OnRoleEmployee.Department, OnRoleEmployee.DesignationGroup,OnRoleEmployee.Section
							 ,OnRoleEmployee.catgSeq,OnRoleEmployee.DesGrpSeq,OnRoleEmployee.DeptSeq,OnRoleEmployee.SecSeq
								,ISNULL(OnRoleEmployee.totalEmployee,0) OnRoleEmployee
								,ISNULL(PresentEmployee.totalPresentEmployee,0) totalPresentEmployee
								,ISNULL(AbsentEmployee.totalAbsentEmployee,0) totalAbsentEmployee
								,ISNULL(LateEmployee.totalLateEmployee,0) totalLateEmployee
								,ISNULL(LeaveEmployee.totalLeaveEmployee,0) totalLeaveEmployee
								,ISNULL(WeekOffEmployee.totalWeekoffEmployee,0)totalWeekoffEmployee
						
					     FROM
						   (SELECT
						   EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,
						   DesGrp.UserName DesignationGroup,DesGrp.Id DesigGrpId,Dept.UserName Department, Dept.Id DepartmentId,Sec.UserName Section,Sec.Id SectionId,
						   COUNT(E.SystemId) totalEmployee,
						   C.UserName,cg.Id CompanyGroupId,c.Id CompanyId,c.UserName CompanyName,cg.UserName GroupName 
	                        	,Empc.Sequence catgSeq,DesGrp.Sequence DesGrpSeq,Sec.Sequence SecSeq,Dept.Sequence DeptSeq
						    	FROM  ORG.CompanyGroup CG
											LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
											LEFT OUTER JOIN EmployeeInformation 
											E ON e.GroupID = CG.Id and c.Id=E.CompanyId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
								   INNER JOIN  ORG.Department AS Dept ON Dept.Id = PR.DepartmentId
								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
											WHERE
												  PlantId = '" + para.PlantId + @"'    AND (E.EmployeeStatus != 'Separated' OR ISNULL(E.DOS,'') = '' OR ISNULL(E.DOS,'')>CONVERT(DATE,'" + workDate + @"'))
                                                   AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + workDate + @"')  " + wc + @"
												GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName
												,EmpC.Username,EmpC.Id
												,DesGrp.UserName,DesGrp.Id,Sec.UserName,Sec.Id,Dept.UserName,Dept.Id 
											,Empc.Sequence ,DesGrp.Sequence ,Sec.Sequence,Dept.Sequence
										
												) OnRoleEmployee
												LEFT OUTER JOIN
								  ( SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,DesGrp.UserName DesignationGroup,DesGrp.Id DesigGrpId,Dept.UserName Department,
						               Dept.Id DepartmentId,Sec.UserName Section,Sec.Id SectionId
								  ,COUNT(E.SystemId) totalPresentEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Present' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + workDate + @"')
								)--**
                                " + wc + @"
								)--*
								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
								   INNER JOIN  ORG.Department AS Dept ON Dept.Id = PR.DepartmentId
								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
									 	LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId				

									WHERE
									   PlantId = '" + para.PlantId + @"'  
									GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName
												,EmpC.Username,EmpC.Id
												,DesGrp.UserName,DesGrp.Id,Sec.UserName,Sec.Id,Dept.UserName,Dept.Id 							
									)
									PresentEmployee
									ON OnRoleEmployee.CompanyGroupId = PresentEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = PresentEmployee.CompanyId
									AND OnRoleEmployee.EmpCategoryId = PresentEmployee.EmpCategoryId AND OnRoleEmployee.DesigGrpId = PresentEmployee.DesigGrpId
									AND OnRoleEmployee.DepartmentId = PresentEmployee.DepartmentId AND OnRoleEmployee.SectionId = PresentEmployee.SectionId 

									LEFT OUTER JOIN
									----------------------------
									 (SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,DesGrp.UserName DesignationGroup,DesGrp.Id DesigGrpId,Dept.UserName Department,
						               Dept.Id DepartmentId,Sec.UserName Section,Sec.Id SectionId
									 ,COUNT(E.SystemId) totalAbsentEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Absent' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + workDate + @"')
								)--**
                                " + wc + @"
								)--*
								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
								INNER JOIN  ORG.Department AS Dept ON Dept.Id = PR.DepartmentId
								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
									 	LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId				

									WHERE
									   PlantId = '" + para.PlantId + @"'   
									GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
											,DesGrp.UserName,DesGrp.Id,Sec.UserName,Sec.Id,Dept.UserName,Dept.Id 
									)
									AbsentEmployee ON 
										 OnRoleEmployee.CompanyGroupId = AbsentEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = AbsentEmployee.CompanyId
									AND OnRoleEmployee.EmpCategoryId = AbsentEmployee.EmpCategoryId AND OnRoleEmployee.DesigGrpId = AbsentEmployee.DesigGrpId
									AND OnRoleEmployee.DepartmentId = AbsentEmployee.DepartmentId AND OnRoleEmployee.SectionId = AbsentEmployee.SectionId 

									LEFT OUTER JOIN
									(SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,DesGrp.UserName DesignationGroup,DesGrp.Id DesigGrpId,Dept.UserName Department,
						               Dept.Id DepartmentId,Sec.UserName Section,Sec.Id SectionId
									,COUNT(E.SystemId) totalLateEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Late' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + workDate + @"')
								)--**
                                " + wc + @"
								)--*
								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
								INNER JOIN  ORG.Department AS Dept ON Dept.Id = PR.DepartmentId
								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
									 	LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId				

									WHERE
									    PlantId = '" + para.PlantId + @"'  
									GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
											,DesGrp.UserName,DesGrp.Id,Sec.UserName,Sec.Id,Dept.UserName,Dept.Id 
									)
									LateEmployee on
											 OnRoleEmployee.CompanyGroupId = LateEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = LateEmployee.CompanyId
									AND OnRoleEmployee.EmpCategoryId = LateEmployee.EmpCategoryId AND OnRoleEmployee.DesigGrpId = LateEmployee.DesigGrpId
									AND OnRoleEmployee.DepartmentId = LateEmployee.DepartmentId AND OnRoleEmployee.SectionId = LateEmployee.SectionId 
										LEFT OUTER JOIN
									(SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,DesGrp.UserName DesignationGroup,DesGrp.Id DesigGrpId,Dept.UserName Department,
						               Dept.Id DepartmentId,Sec.UserName Section,Sec.Id SectionId
									,COUNT(E.SystemId) totalWeekoffEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category IN('Holiday', 'Weekend') AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + workDate + @"')
								)--**
                                " + wc + @"
								)--*
								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
								INNER JOIN  ORG.Department AS Dept ON Dept.Id =PR.DepartmentId
								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
									 	LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId				

									WHERE
									    PlantId = '" + para.PlantId + @"'   
									GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
											,DesGrp.UserName,DesGrp.Id,Sec.UserName,Sec.Id,Dept.UserName,Dept.Id 	
									)
									WeekOffEmployee ON 
									
										 OnRoleEmployee.CompanyGroupId = WeekOffEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = WeekOffEmployee.CompanyId
									AND OnRoleEmployee.EmpCategoryId = WeekOffEmployee.EmpCategoryId AND OnRoleEmployee.DesigGrpId = WeekOffEmployee.DesigGrpId
									AND OnRoleEmployee.DepartmentId = WeekOffEmployee.DepartmentId AND OnRoleEmployee.SectionId = WeekOffEmployee.SectionId 
									
                                             LEFT OUTER JOIN
    
									(SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,DesGrp.UserName DesignationGroup,DesGrp.Id DesigGrpId,Dept.UserName Department,
						               Dept.Id DepartmentId,Sec.UserName Section,Sec.Id SectionId
									,COUNT(E.SystemId) totalLeaveEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Leave' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + workDate + @"')
								)--**
                                " + wc + @"
								)--*
								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
								INNER JOIN  ORG.Department AS Dept ON Dept.Id = PR.DepartmentId

								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
									 	LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId				

									WHERE
									   PlantId = '" + para.PlantId + @"' ";

                strSql += @"GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
											,DesGrp.UserName,DesGrp.Id,Sec.UserName,Sec.Id,Dept.UserName,Dept.Id
									)
								LeaveEmployee ON

                                    OnRoleEmployee.CompanyGroupId = LeaveEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = LeaveEmployee.CompanyId

                                    AND OnRoleEmployee.EmpCategoryId = LeaveEmployee.EmpCategoryId AND OnRoleEmployee.DesigGrpId = LeaveEmployee.DesigGrpId

                                    AND OnRoleEmployee.DepartmentId = LeaveEmployee.DepartmentId  AND OnRoleEmployee.SectionId = LeaveEmployee.SectionId 

                                    ORDER BY OnRoleEmployee.catgSeq,OnRoleEmployee.DeptSeq,OnRoleEmployee.SecSeq,OnRoleEmployee.DesGrpSeq";

                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function GetEmpSalaryStructureSql()
        public DataTable SelectedPlant(string sPlantID)
        {
            string strSql = "";
            try
            {
                strSql = @"SELECT P.UserName,AM.Address1+','+ ISNULL(AM.Address2,'') Address1 FROM ORG.Plant P
                            LEFT OUTER JOIN MST.AddressMaster AM ON P.AddressMasterId=AM.Id
                             WHERE P.Id = '" + sPlantID + @"'";
                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//end of function



        public class SwapColumn : BaseModel
        {
            public string ValueMember { get; set; } = string.Empty;
            public string DisplayMember { get; set; } = string.Empty;
            public int ColIndex { get; set; } = 0;
        }
        public void GetDayType(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = "select * from daytype where Category in ('Present','Late') ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function
        public void GetExtraAbsent(string plantid, int smonth, int syear, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT WorkingDate,EmpSystemID
                              FROM [SCS].[WeeklyAbsentismAssignment]
                              where month(WorkingDate)=" + smonth + " and YEAR(WorkingDate)=" + syear + " and plantid='" + plantid + @"' 
                            union

                            SELECT WorkDate WorkingDate,EmpSystemID
                              FROM [trn].[HolidayAbsentismAssignment]
                              where month(WorkDate)=" + smonth + " and YEAR(WorkDate)=" + syear + " and plantid='" + plantid + @"'
                            ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function
        public void GetHalfLeaveInfo(string plantid, string fromdate, string todate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select t.EmpSystemID,d.WorkDate,d.LeaveDuration from LeaveTransaction t
                                left join LeaveTransactionDetails d on d.LvTrnsSystemID = t.SystemID
                                where d.WorkDate between '" + fromdate + @"' and '" + todate + @"' 
                                and d.IsAvailed = 1 
                                and d.LeaveDuration = 0.5
                                and plantid='" + plantid + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function



        public void GetAttendanceInfoExtra(string plantId, string fromdate, string todate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT EmpSystemId,InfoType FROM AttendanceInfoExtra 
                        WHERE InfoType IN ('LATEIN','EARLYOUT')
                    AND WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'
                     AND PlantId = '" + plantId + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end function



        // public void SelectedPlantWiseCompany(string sPlantID, out DataSet dsRef)
        // {
        //     ConnectionManager.DAL.ConManager objCon;
        //     string strSql = "";

        //     try
        //     {
        //         strSql = @"SELECT p.UserName PlantName,c.UserName CompanyName ,ISNULL(a.Address1,'')+','+ ISNULL(a.Address2,'') Address1, a.Phone,a.Email
        //                         ,cm.Address1 cAddress1 ,cm.Address2 cAddress2
        //                         FROM org.Plant p
        //LEFT OUTER JOIN org.Company c on c.Id=p.CompanyId
        //LEFT OUTER JOIN mst.AddressMaster a on a.Id=p.AddressMasterId
        //LEFT OUTER JOIN mst.AddressMaster cm on cm.Id=c.AddressMasterId
        //WHERE p.Id='" + sPlantID + "'";

        //         objCon = new ConnectionManager.DAL.ConManager("1");
        //         objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
        //     }
        //     catch (Exception ex)
        //     {
        //         throw (ex);
        //     }
        //     finally
        //     {
        //         objCon = null;
        //     }
        // }//end of function
        public void SelectedPlant(string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT P.UserName,AM.Address1+','+ ISNULL(AM.Address2,'') Address1 FROM ORG.Plant P
                            LEFT OUTER JOIN MST.AddressMaster AM ON P.AddressMasterId=AM.Id
                             WHERE P.Id = '" + sPlantID + @"'";
                //strSql = @"SELECT * FROM ORG.Plant WHERE Id = '" + sPlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function
        public void SelectedPlantWiseCompany(string sPlantID, out DataSet dsRef)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET"
            };

            try
            {
                parameters.CmdText = @"SELECT p.UserName PlantName,c.UserName CompanyName,a.Address1,a.Phone,a.Email
								,cm.Address1 cAddress1 ,cm.Address2 cAddress2
								FROM ORG.Plant p
							LEFT OUTER JOIN ORG.Company c ON c.Id=p.CompanyId
							LEFT OUTER JOIN MST.AddressMaster a ON a.Id=p.AddressMasterId
							LEFT OUTER JOIN MST.AddressMaster cm ON cm.Id=c.AddressMasterId
							where p.Id='" + sPlantID + "'";
                dsRef = _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//end of function


        public void SelectedCompanyGroup(out DataSet dsRef)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET"
            };

            try
            {
                parameters.CmdText = @"SELECT * FROM ORG.CompanyGroup";
                dsRef = _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//end of function

        public DataTable SelectedPlantWiseCompanyDataTable(string sPlantID)
        {
            string sqlText = "";

            try
            {
                sqlText = @"SELECT p.UserName PlantName,c.UserName CompanyName,a.Address1,a.Phone,a.Email
								,cm.Address1 cAddress1 ,cm.Address2 cAddress2
								FROM ORG.Plant p
							LEFT OUTER JOIN ORG.Company c ON c.Id=p.CompanyId
							LEFT OUTER JOIN MST.AddressMaster a ON a.Id=p.AddressMasterId
							LEFT OUTER JOIN MST.AddressMaster cm ON cm.Id=c.AddressMasterId
							where p.Id='" + sPlantID + "'";
                return _sqlRepository.GetDataTable(sqlText);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//end of function
        public DataTable GetAttendanceSummarySql(string WorkDate, string plantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            clsStaticInfo obs = null;
            try
            {
                string wc = string.Empty;


                strSql = @"SELECT  OnRoleEmployee.CompanyId CompanyId,OnRoleEmployee.CompanyName ColumnName,OnRoleEmployee.GroupName GroupName,OnRoleEmployee.CompanyGroupId CompanyGroupId
                             ,OnRoleEmployee.EmpCategory,OnRoleEmployee.Department
--, OnRoleEmployee.DesignationGroup
,OnRoleEmployee.Section,OnRoleEmployee.SubSection
							 ,OnRoleEmployee.catgSeq
                                    ,OnRoleEmployee.DeptSeq
                                    ,OnRoleEmployee.LineSeq
                                    ,OnRoleEmployee.SecSeq
                                     ,OnRoleEmployee.SubSecq
                             ,ISNULL(OnRoleEmployee.Line,'') Line,ISNULL(OnRoleEmployee.GenderID,'') GenderID,ISNULL(OnRoleEmployee.LealDesignation,'')LealDesignation
								,ISNULL(OnRoleEmployee.totalEmployee,0) OnRoleEmployee
								,ISNULL(PresentEmployee.totalPresentEmployee,0) totalPresentEmployee
								,ISNULL(AbsentEmployee.totalAbsentEmployee,0) totalAbsentEmployee
								,ISNULL(LateEmployee.totalLateEmployee,0) totalLateEmployee
								,ISNULL(LeaveEmployee.totalLeaveEmployee,0) totalLeaveEmployee
								,ISNULL(WeekOffEmployee.totalWeekoffEmployee,0)totalWeekoffEmployee		
                                ,isnull (MaternityLeave.totalMaternithyEmployee,0)totalMaternithyEmployee
					     FROM
						   (SELECT
						   EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,
                            isnull(Line.UserName,'') Line ,isnull(Line.Id,'') LineId,isnull(e.GenderID,'')GenderID 
                            ,isnull(Ld.UserName,'') AS LealDesignation,isnull(Ld.Id,'') LDID,isnull(Sec.UserName,'') Section,isnull(Sec.Id,'') SectionId
						   --,DesGrp.UserName DesignationGroup,DesGrp.Id DesigGrpId
,Dept.UserName Department, Dept.Id DepartmentId,SubSec.UserName as  SubSection ,SubSec.Id ssId,
						   COUNT(E.SystemId) totalEmployee,
						   C.UserName,cg.Id CompanyGroupId,c.Id CompanyId,c.UserName CompanyName,cg.UserName GroupName 
	                        	,Empc.Sequence catgSeq
                                    --,DesGrp.Sequence DesGrpSeq
                                    ,Sec.Sequence SecSeq
                                    ,Dept.Sequence DeptSeq
                                     ,Line.Sequence LineSeq
	                                    ,SubSec.Sequence SubSecq
						    	FROM  ORG.CompanyGroup CG
											LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
											LEFT OUTER JOIN EmployeeInformation 
											E ON e.GroupID = CG.Id and c.Id=E.CompanyId
								   INNER JOIN  ORG.Department AS Dept ON Dept.Id = E.DepartmentId
                                   Left join MST.ManpowerBudget MB ON mb.Id = E.BudgetCode
LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
								   left join ORG.Line Line on Line.Id = Mb.LineId
                                 left join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
                                 
								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
											WHERE
												  PlantId = '" + plantId + @"'    AND (E.EmployeeStatus != 'Separated' OR ISNULL(E.DOS,'') = '' OR ISNULL(E.DOS,'')>CONVERT(DATE,'" + WorkDate + @"'))
                                                   AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'" + WorkDate + @"')  " + wc + @" 
--and  Dept.UserName='Production' and  EmpC.UserName='Staff' and Sec.UserName='Sewing'
												GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName
												,EmpC.Username,EmpC.Id
												--,DesGrp.UserName,DesGrp.Id
,Sec.UserName,Sec.Id,Dept.UserName,Dept.Id ,SubSec.UserName ,SubSec.Id
											,Empc.Sequence ,SubSec.Sequence, Line.Sequence
                                        --,DesGrp.Sequence
                                        ,Sec.Sequence,Dept.Sequence,line.UserName,Line.Id,e.GenderID,Ld.UserName,Ld.Id											
												) OnRoleEmployee
												LEFT OUTER JOIN
								  ( SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId
                                --,DesGrp.UserName DesignationGroup,DesGrp.Id DesigGrpId
                                            ,Dept.UserName Department,
						               Dept.Id DepartmentId
                                --,Sec.UserName Section,Sec.Id SectionId
								  ,COUNT(E.SystemId) totalPresentEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName,SubSec.UserName as  SubSection ,SubSec.Id ssId,
                                isnull(Line.UserName,'') Line ,isnull(Line.Id,'') LineId,isnull(e.GenderID,'')GenderID 
                                ,isnull(Ld.UserName,'') AS LealDesignation,isnull(Ld.Id,'') LDID,isnull(Sec.UserName,'') Section,isnull(Sec.Id,'') SectionId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Present' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + WorkDate + @"')
								)--**
                                " + wc + @"
								)--*
								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
								   INNER JOIN  ORG.Department AS Dept ON Dept.Id = PR.DepartmentId
								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
									 	LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId	
								   left join ORG.Line Line on Line.Id = Mb.LineId
                                  left join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
									WHERE
									   PlantId = '" + plantId + @"'  
									GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName
												,EmpC.Username,EmpC.Id
												--,DesGrp.UserName,DesGrp.Id
                            ,Sec.UserName,Sec.Id,Dept.UserName,Dept.Id 
                                ,Line.UserName,Line.Id,E.GenderID,Ld.UserName,Ld.Id,SubSec.UserName ,SubSec.Id
									)
									PresentEmployee
									ON OnRoleEmployee.CompanyGroupId = PresentEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = PresentEmployee.CompanyId
									AND OnRoleEmployee.EmpCategoryId = PresentEmployee.EmpCategoryId 
--AND OnRoleEmployee.DesigGrpId = PresentEmployee.DesigGrpId
									AND OnRoleEmployee.DepartmentId = PresentEmployee.DepartmentId AND OnRoleEmployee.SectionId = PresentEmployee.SectionId 
                                    AND OnRoleEmployee.LineId = PresentEmployee.LineId AND OnRoleEmployee.GenderID = PresentEmployee.GenderID 
                                    AND OnRoleEmployee.LDID = PresentEmployee.LDID AND OnRoleEmployee.ssId = PresentEmployee.ssId
									LEFT OUTER JOIN
									----------------------------
									 (SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId
--,DesGrp.UserName DesignationGroup,DesGrp.Id DesigGrpId
,Dept.UserName Department,
						               Dept.Id DepartmentId
                                        --,Sec.UserName Section,Sec.Id SectionId
									 ,COUNT(E.SystemId) totalAbsentEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName
                                    ,SubSec.UserName as  SubSection ,SubSec.Id ssId,
                                    isnull(Line.UserName,'') Line ,isnull(Line.Id,'') LineId,isnull(e.GenderID,'')GenderID ,isnull(Ld.UserName,'') AS LealDesignation,isnull(Ld.Id,'') LDID,isnull(Sec.UserName,'') Section,isnull(Sec.Id,'') SectionId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Absent' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + WorkDate + @"')
								)--**
                                " + wc + @"
								)--*
								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
								INNER JOIN  ORG.Department AS Dept ON Dept.Id = PR.DepartmentId
								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
									 	LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId	
								   left join ORG.Line Line on Line.Id = Mb.LineId
                                        left join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
									WHERE
									   PlantId = '" + plantId + @"'   
									GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
											--,DesGrp.UserName,DesGrp.Id
                                ,Sec.UserName,Sec.Id,Dept.UserName,Dept.Id 
                                    ,Line.UserName,Line.Id,e.GenderID,Ld.UserName,Ld.Id ,SubSec.UserName ,SubSec.Id
									)
									AbsentEmployee ON 
										 OnRoleEmployee.CompanyGroupId = AbsentEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = AbsentEmployee.CompanyId
									AND OnRoleEmployee.EmpCategoryId = AbsentEmployee.EmpCategoryId 
--AND OnRoleEmployee.DesigGrpId = AbsentEmployee.DesigGrpId
									AND OnRoleEmployee.DepartmentId = AbsentEmployee.DepartmentId AND OnRoleEmployee.SectionId = AbsentEmployee.SectionId 
                                AND OnRoleEmployee.LineId = AbsentEmployee.LineId AND OnRoleEmployee.GenderID = AbsentEmployee.GenderID AND OnRoleEmployee.LDID = AbsentEmployee.LDID AND OnRoleEmployee.ssId = AbsentEmployee.ssId
									LEFT OUTER JOIN
									(SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId
--,DesGrp.UserName DesignationGroup,DesGrp.Id DesigGrpId
,Dept.UserName Department,
						               Dept.Id DepartmentId
                                    --,Sec.UserName Section,Sec.Id SectionId
									,COUNT(E.SystemId) totalLateEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName,SubSec.UserName as  SubSection ,SubSec.Id ssId,
                            isnull(Line.UserName,'') Line ,isnull(Line.Id,'') LineId,isnull(e.GenderID,'')GenderID ,isnull(Ld.UserName,'') AS LealDesignation,isnull(Ld.Id,'') LDID
                            ,isnull(Sec.UserName,'') Section,isnull(Sec.Id,'') SectionId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Late' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + WorkDate + @"')
								)--**
                                " + wc + @"
								)--*
								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
								INNER JOIN  ORG.Department AS Dept ON Dept.Id = PR.DepartmentId
								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
									 	LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId	
								   left join ORG.Line Line on Line.Id = Mb.LineId
                                   left join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
									WHERE
									    PlantId = '" + plantId + @"'  
									GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
											--,DesGrp.UserName,DesGrp.Id
,Sec.UserName,Sec.Id,Dept.UserName,Dept.Id 
                                ,Line.UserName,Line.Id ,e.GenderID ,Ld.UserName,Ld.Id
                                --,ss.UserName,ss.Id 
                                ,SubSec.UserName ,SubSec.Id
									)
									LateEmployee on
											 OnRoleEmployee.CompanyGroupId = LateEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = LateEmployee.CompanyId
									AND OnRoleEmployee.EmpCategoryId = LateEmployee.EmpCategoryId 
--AND OnRoleEmployee.DesigGrpId = LateEmployee.DesigGrpId
									AND OnRoleEmployee.DepartmentId = LateEmployee.DepartmentId AND OnRoleEmployee.SectionId = LateEmployee.SectionId 
			AND OnRoleEmployee.LineId = LateEmployee.LineId 	AND OnRoleEmployee.GenderID = LateEmployee.GenderID AND OnRoleEmployee.LDID = LateEmployee.LDID AND OnRoleEmployee.ssId = LateEmployee.ssId
										LEFT OUTER JOIN
									(SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId
--,DesGrp.UserName DesignationGroup,DesGrp.Id DesigGrpId
,Dept.UserName Department,
						               Dept.Id DepartmentId
--,Sec.UserName Section,Sec.Id SectionId
									,COUNT(E.SystemId) totalWeekoffEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName,SubSec.UserName as  SubSection ,SubSec.Id ssId,
isnull(Line.UserName,'') Line ,isnull(Line.Id,'') LineId,isnull(e.GenderID,'')GenderID ,isnull(Ld.UserName,'') AS LealDesignation,isnull(Ld.Id,'') LDID,isnull(Sec.UserName,'') Section,isnull(Sec.Id,'') SectionId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category IN('Holiday', 'Weekend') AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + WorkDate + @"')
								)--**
                                " + wc + @"
								)--*
								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
								INNER JOIN  ORG.Department AS Dept ON Dept.Id = PR.DepartmentId
								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
									 	LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId	
left join ORG.Line Line on Line.Id = Mb.LineId
 left join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId

									WHERE
									    PlantId = '" + plantId + @"'   
									GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
											--,DesGrp.UserName,DesGrp.Id
,Sec.UserName,Sec.Id,Dept.UserName,Dept.Id 
,Line.UserName,Line.Id,E.GenderID ,e.GenderID,Ld.UserName,Ld.Id, SubSec.UserName,SubSec.Id
									)
									WeekOffEmployee ON 
									
										 OnRoleEmployee.CompanyGroupId = WeekOffEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = WeekOffEmployee.CompanyId
									AND OnRoleEmployee.EmpCategoryId = WeekOffEmployee.EmpCategoryId 
--AND OnRoleEmployee.DesigGrpId = WeekOffEmployee.DesigGrpId
									AND OnRoleEmployee.DepartmentId = WeekOffEmployee.DepartmentId AND OnRoleEmployee.SectionId = WeekOffEmployee.SectionId 
		AND OnRoleEmployee.LineId = WeekOffEmployee.LineId AND OnRoleEmployee.GenderID = WeekOffEmployee.GenderID 	AND OnRoleEmployee.LDID = WeekOffEmployee.LDID 	AND OnRoleEmployee.ssId = WeekOffEmployee.ssId 								
                                             LEFT OUTER JOIN
    
									(SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId
--,DesGrp.UserName DesignationGroup,DesGrp.Id DesigGrpId
,Dept.UserName Department,
						               Dept.Id DepartmentId
--,Sec.UserName Section,Sec.Id SectionId
									,COUNT(E.SystemId) totalLeaveEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName,
SubSec.UserName as  SubSection ,SubSec.Id ssId,
isnull(Line.UserName,'') Line ,isnull(Line.Id,'') LineId,isnull(e.GenderID,'')GenderID ,isnull(Ld.UserName,'') AS LealDesignation,isnull(Ld.Id,'') LDID,isnull(Sec.UserName,'') Section,isnull(Sec.Id,'') SectionId

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Leave' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + WorkDate + @"')
								)--**
                                " + wc + @"
								)--*
								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
								INNER JOIN  ORG.Department AS Dept ON Dept.Id = PR.DepartmentId

								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
									 	LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId	
  left join ORG.Line Line on Line.Id = Mb.LineId
 left join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId

                                inner join AttdnProcessData apd on apd.EmpSystemID=e.SystemId								
									   inner join (select * from [dbo].[LeaveTransaction]
										 where  ('" + WorkDate + @"' Between FromDate and ToDate ))LT on LT.EmpSystemID=e.SystemId and APD.LTSystemID=LT.LTSystemID 
										  inner join [dbo].[LeaveType] lET on  lET.Id = APD.LTSystemId 

									WHERE
									   e.PlantId = '" + plantId + @"' and lET.LeaveType <> 'Maternity'  	 and apd.WorkDate='" + WorkDate + @"' ";

                strSql += @"GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
											--,DesGrp.UserName,DesGrp.Id
,Sec.UserName,Sec.Id,Dept.UserName,Dept.Id
	,Line.UserName,Line.Id,E.GenderID ,Ld.UserName,Ld.Id,SubSec.UserName ,SubSec.Id 
									)
								LeaveEmployee on


                                    OnRoleEmployee.CompanyGroupId = LeaveEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = LeaveEmployee.CompanyId

                                    AND OnRoleEmployee.EmpCategoryId = LeaveEmployee.EmpCategoryId 
--AND OnRoleEmployee.DesigGrpId = LeaveEmployee.DesigGrpId

                                    AND OnRoleEmployee.DepartmentId = LeaveEmployee.DepartmentId  AND OnRoleEmployee.SectionId = LeaveEmployee.SectionId 
AND OnRoleEmployee.LineId = LeaveEmployee.LineId AND OnRoleEmployee.GenderID = LeaveEmployee.GenderID  AND OnRoleEmployee.LDID = LeaveEmployee.LDID   AND OnRoleEmployee.ssId = LeaveEmployee.ssId

									  LEFT OUTER JOIN(
									  SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId
											,Dept.UserName Department, Dept.Id DepartmentId
									,COUNT(E.SystemId) totalMaternithyEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName,
							SubSec.UserName as  SubSection ,SubSec.Id ssId,
							isnull(Line.UserName,'') Line ,isnull(Line.Id,'') LineId,isnull(e.GenderID,'')GenderID ,isnull(Ld.UserName,'') AS LealDesignation
							,isnull(Ld.Id,'') LDID,isnull(Sec.UserName,'') Section,isnull(Sec.Id,'') SectionId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Leave' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + WorkDate + @"')
								)--**                               
								)--*
								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
								INNER JOIN  ORG.Department AS Dept ON Dept.Id = PR.DepartmentId
								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
							LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId	
								  left join ORG.Line Line on Line.Id = Mb.LineId
								 left join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
								 inner join AttdnProcessData apd on apd.EmpSystemID=e.SystemId								
										  left join (select * from [dbo].[LeaveTransaction]
										 where  ('" + WorkDate + @"' Between FromDate and ToDate ))LT on LT.EmpSystemID=e.SystemId and APD.LTSystemID=LT.LTSystemID 
										  left join [dbo].[LeaveType] lET on  lET.Id = APD.LTSystemId 
									WHERE
									   e.PlantId = '" + plantId + @"' and lET.LeaveType='Maternity'  	 and apd.WorkDate='" + WorkDate + @"'
									    GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
						,Sec.UserName,Sec.Id,Dept.UserName,Dept.Id
							,Line.UserName,Line.Id,E.GenderID ,Ld.UserName,Ld.Id,SubSec.UserName ,SubSec.Id 
									  )MaternityLeave on 
									   OnRoleEmployee.CompanyGroupId = MaternityLeave.CompanyGroupId AND OnRoleEmployee.CompanyId = MaternityLeave.CompanyId
                                    AND OnRoleEmployee.EmpCategoryId = MaternityLeave.EmpCategoryId
                                    AND OnRoleEmployee.DepartmentId = MaternityLeave.DepartmentId  AND OnRoleEmployee.SectionId = MaternityLeave.SectionId 
									AND OnRoleEmployee.LineId = MaternityLeave.LineId AND OnRoleEmployee.GenderID = MaternityLeave.GenderID 
									 AND OnRoleEmployee.LDID = MaternityLeave.LDID   AND OnRoleEmployee.ssId = MaternityLeave.ssId

                                     ORDER BY 
                                OnRoleEmployee.EmpCategory,
												OnRoleEmployee.Department ,
												OnRoleEmployee.Section,
												OnRoleEmployee.SubSection,
												OnRoleEmployee.LealDesignation,
												Line,GenderID";

                return _sqlRepository.GetDataTable(strSql);
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

        public DataTable GetDailyAttnRpt(string sPlantID, string WDate, string sUnitID, string sDivID, string sDepID, string sSecID, string sSubSecID, string sLineID, string sDesigGrpID, string sDesigID, string sEmpCatID)
        {
            var EntityTables = @"LEFT JOIN org.Unit U ON E.UnitID = U.Id
                     LEFT JOIN org.Division Dv ON E.DivisionID = Dv.Id
                     LEFT JOIN org.Department Dp ON E.DepartmentID = Dp.Id
                     LEFT JOIN org.Section S ON E.SectionID = S.Id
                     LEFT JOIN org.SubSection SB ON E.SubSectionID = SB.Id
                     LEFT JOIN org.Line L ON E.LineID = L.Id
                     LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupID = Dg.Id
                     LEFT JOIN hkp.Designation D ON E.DesignationSystemID = D.Id
                     LEFT JOIN hkp.Designation GVD ON E.GivenDesignationId = GVD.Id
                     LEFT JOIN hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id ";

            var EntityColumns = @", DG.UserName DesignationGroup
		                , D.UserName Designation
		                , GVD.UserName GivenDesignation
		                , L.UserName Line
		                , U.UserName Unit
		                , Dv.UserName Division
		                , Dp.UserName Department
		                , S.UserName Section
		                , SB.UserName SubSection
		                , EC.UserName AS EmpCategory ";

            var EntityAlias = @", DesignationGroup
	                    , Designation
                        , GivenDesignation
		                , EmpCategory
		                , Line
		                , SubSection
		                , Section
		                , Department
		                , Division
		                , Unit ";

            var strSql = string.Empty;

            try
            {
                strSql = @"SELECT EmployeeCode
	                            , EmployeeName
	                            , DOJ
	                            , DesignationGroupID
	                            , UnitID
	                            , DivisionID
	                            , DepartmentID
	                            , SectionID
	                            , SubSectionID
	                            , LineID
	                            , EmpCategoryID
	                            , PDate
	                            , DayStatus
	                            , InTime
                                ,ShiftName
							    ,ShiftInTimeShow
							    ,InTimeShow
							    ,OutTimeShow
							    --,CONVERT(VARCHAR(5),dateadd(MINUTE,-LeastInTime, ShiftInTime), 108) LeastEntryTime
							    ,LeastInTime
	                            , InDeviceID
	                            , OutTime
	                            , OutDeviceID
	                            , OTHr
	                            , ShiftTime = CASE
		                            WHEN ShiftChangeInTime IS NULL
			                            THEN ShiftInTime
		                            ELSE ShiftChangeInTime
		                            END
	                            , PlantID
                                " + EntityAlias + @"
			                FROM
                            (
                              SELECT E.EmployeeCode
	                                , isnull(E.FirstName,'') +' ' +isnull(E.MiddleName,'')+' ' +isnull(E.LastName,'') EmployeeName
	                                , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
	                                , E.DesignationGroupID

	                                , E.UnitID
	                                , E.DivisionID
	                                , E.DepartmentID
	                                , E.SectionID
	                                , E.SubSectionID
	                                , E.LineID
	                                , REPLACE(CONVERT(VARCHAR(11), AD.WorkDate, 113), ' ', '-') PDate
	                                , E.EmployeeCategorySystemID EmpCategoryID
	                                , AD.DayStatus
                                    ,CONVERT(VARCHAR(15),CAST(LIT.ptime AS TIME),100) LeastInTime
	                                , CONVERT(VARCHAR(5), AD.InTime, 108) InTime
                                    ,CONVERT(varchar(15),CAST(AD.InTime AS TIME),100) InTimeShow
	                                , ARIN.DeviceID InDeviceID
	                                , CONVERT(VARCHAR(5), AD.OutTime, 108) OutTime
                                    ,CONVERT(varchar(15),CAST(AD.OutTime AS TIME),100) OutTimeShow
	                                , AROUT.DeviceID OutDeviceID
	                                , AD.OTHr
	                                , CONVERT(VARCHAR(5), SFCG.InTime, 108) ShiftChangeInTime
                                    --,sd.InTimeStartMargin LeastInTime
	                                , CONVERT(VARCHAR(5), SD.InTime, 108) ShiftInTime
                                    ,CONVERT(varchar(15),CAST(SD.InTime AS TIME),100) ShiftInTimeShow
                                    , SD.ShiftDefinationName ShiftName
	                                , AD.PlantID
                                    ,E.GivenDesignationId
                                    " + EntityColumns + @"

                                FROM dbo.EmployeeInformation E
							                INNER JOIN dbo.AttdnProcessData AD ON E.SystemID = AD.EmpSystemID
							                LEFT JOIN (SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + WDate + @"' BETWEEN FromDate AND ToDate) AS SFCG
																                ON AD.ShiftSystemID = SFCG.ShiftDefinationID
							                LEFT JOIN dbo.ShiftDefination SD ON AD.ShiftSystemID = SD.SystemID
							                LEFT JOIN dbo.AttdnRawData ARIN ON AD.InTimeRowID = ARIN.RowID
							                LEFT JOIN dbo.AttdnRawData AROUT ON AD.OutTimeRowID = AROUT.RowID
                                            left join
												(
												select LogDownLoadNum
												,min(ptime) ptime
												from AttdnRawData
												where pdate='" + WDate + @"' --and LogDownLoadNum='1800004'
												group by LogDownLoadNum
												) LIT on LIT.LogDownLoadNum=E.SystemId

							                " + EntityTables + @"

			                    WHERE AD.WorkDate  = '" + WDate + @"' AND E.EmployeeStatus='Active'
                            ) A  WHERE PlantID  = '" + sPlantID + @"'";

                if (sUnitID != "ALL")
                {
                    strSql = strSql + @" AND UnitID = '" + sUnitID + "'";
                }
                if (sDivID != "ALL")
                {
                    strSql = strSql + @" AND DivisionID = '" + sDivID + "'";
                }
                if (sDepID != "ALL")
                {
                    strSql = strSql + @" AND DepartmentID = '" + sDepID + "'";
                }
                if (sSecID != "ALL")
                {
                    strSql = strSql + @" AND SectionID = '" + sSecID + "'";
                }
                if (sSubSecID != "ALL")
                {
                    strSql = strSql + @" AND SubSectionID = '" + sSubSecID + "'";
                }
                if (sLineID != "ALL")
                {
                    strSql = strSql + @" AND LineID = '" + sLineID + "'";
                }
                if (sDesigGrpID != "ALL")
                {
                    strSql = strSql + @" AND DesignationGroupID = '" + sDesigGrpID + "'";
                }
                if (sDesigID != "ALL")
                {
                    strSql = strSql + @" AND GivenDesignationId = '" + sDesigID + "'";
                }
                if (sEmpCatID != "ALL")
                {
                    strSql = strSql + @" AND EmpCategoryID = '" + sEmpCatID + "'";
                }

                strSql = strSql + @"
                        GROUP BY  EmployeeCode
	                            , EmployeeName
	                            , DOJ
	                            , DesignationGroupID
	                            , UnitID
	                            , DivisionID
	                            , DepartmentID
	                            , SectionID
	                            , SubSectionID
	                            , LineID
	                            , EmpCategoryID
	                            , PDate
	                            , DayStatus
	                            , InTime
								,LeastInTime
	                            , InDeviceID
	                            , OutTime
	                            , OutDeviceID
	                            , OTHr
	                            , ShiftChangeInTime
                                ,ShiftName
								,ShiftInTimeShow
								,InTimeShow
								,OutTimeShow
	                            , ShiftInTime
	                            , PlantID
                                " + EntityAlias + @"
                        ORDER BY Unit, Section, SubSection, DayStatus, EmployeeCode";

                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function
        #endregion ********************SQLGeneratingFunction***************



    }

    public class ParamList
    {
        public string EmployeeId { get; set; }
        public string UnitId { get; set; }
        public string DivisionId { get; set; }
        public string DepartmentId { get; set; }
        public string SectionId { get; set; }
        public string SubSectionId { get; set; }
        public string LineId { get; set; }
        public string PlantId { get; set; }
        public string SubSecStrucId { get; set; }
        public string EmpCategorId { get; set; }
        public string DesignationGroupId { get; set; }
        public string DesignationId { get; set; }
        public string FromDate { get; set; }
        public string EmpStatus { get; set; }
        public string SalaryProcessId { get; set; }
        public string CompanyGroupId { get; set; }
        public string CompanyId { get; set; }
        public string ToDate { get; set; }
        public string PayGroup { get; set; }
        public string SystemID { get; set; }
        public string LanguageId { get; set; }
        public string SystemAdmin { get; set; }
        public string ControlAdmin { get; set; }
    }

    public class clsEntityDropdownlist
    {
        public DropDownList ddlUnit { get; set; }
        public DropDownList ddlDivision { get; set; }
        public DropDownList ddlDepartment { get; set; }
        public DropDownList ddlSection { get; set; }
        public DropDownList ddlSubSection { get; set; }
        public DropDownList ddlLine { get; set; }
        public DropDownList ddlPlant { get; set; }
        public DropDownList ddlSubSecStruc { get; set; }
        public DropDownList ddlEmpCategor { get; set; }
        public DropDownList ddlDesignationGroup { get; set; }
        public DropDownList ddlDesignation { get; set; }
        public DropDownList ddlJobLocation { get; set; }
        public DropDownList ddlEntity { get; set; }
        public DropDownList ddlPayGroup { get; set; }


    }
    public class clsShiftShowLabel
    {
        public Label lblEmpCode { get; set; }
        public Label lblEmpName { get; set; }
        public Label lblEmpDateOJ { get; set; }
        public Label lblDesignationGroup { get; set; }
        public Label lblDesignation { get; set; }
        public Label lblDepartment { get; set; }
        public Label lblCurrentShift { get; set; }
        public Label lblCurrentWeekOff { get; set; }
        public Label lblShiftName { get; set; }
        public Label lblDayStatus { get; set; }
        public Label lblCurrentEffectiveDate { get; set; }
        public Label lblLeastPunchTime { get; set; }
        public Label lblJbLc { get; set; }
        public Label lblJbLcSystemID { get; set; }
        public Label lblJbLcPlantID { get; set; }
    }

    public class ParaAttendanceReport
    {
        public string UnitId { get; set; }
        public string DivisionId { get; set; }
        public string DepartmentId { get; set; }
        public string SectionId { get; set; }
        public string SubsectionId { get; set; }
        public string LineId { get; set; }
        public string EmpCat { get; set; }
        public string DesignationId { get; set; }
        public string EntityId { get; set; }
        public string JoblocationId { get; set; }
        public string DesignationGroupId { get; set; }
        public string ShiftId { get; set; }
        public string PlantId { get; set; }
        public string ADate { get; set; }
        //string sUnit = ddlUnit.SelectedValue.ToString().Trim();
        //string sDevi = ddlDivision.SelectedValue.ToString().Trim();
        //string sDept = ddlDepartment.SelectedValue.ToString().Trim();
        //string sSect = ddlSection.SelectedValue.ToString().Trim();
        //string sSbSe = ddlSubSection.SelectedValue.ToString().Trim();
        //string sLine = ddlLine.SelectedValue.ToString().Trim();
        ////string sSbSeStr = this.ddlSubSecStruc.SelectedValue.ToString().Trim();
        //string sEmpC = ddlEmpCategor.SelectedValue.ToString().Trim();
        //string sDeGr = ddlDesignationGroup.SelectedValue.ToString().Trim();
        //string sDesi = ddlDesignation.SelectedValue.ToString().Trim();
        //string sEntity = ddlEntity.SelectedValue.ToString().Trim();
        //string sJoblocation = ddlJobLocation.SelectedValue.ToString().Trim();
    }

}
