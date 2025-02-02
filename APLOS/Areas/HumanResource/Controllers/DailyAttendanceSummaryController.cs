using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class DailyAttendanceSummaryController : BaseController
    {
        #region Constructor

        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        public DailyAttendanceSummaryController(
              IAttendanceManagementService AttendanceManagementService, IEmployeeProfileService employeeProfileService, ISqlRepository R
            )
        {
            _AttendanceManagementService = AttendanceManagementService;
            _employeeProfileService = employeeProfileService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult DailyAttendanceSummaryNoLine()
        {
            return View();
        }

        #endregion -- Pages
        public void GetAttendanceSummarySql(string WorkDate, out DataSet dsRef)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            clsStaticInfo obs = null;
            try
            {
                string wc = string.Empty;

                obs = new clsStaticInfo();
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
								   
                                   Left join MST.ManpowerBudget MB ON mb.Id = E.BudgetCode
LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
INNER JOIN  ORG.Department AS Dept ON Dept.Id = PR.DepartmentId
								   left join ORG.Line Line on Line.Id = Mb.LineId
                                 left join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
                                 --left join [ORG].[SubSection] ss on ss.Id=PR.SubSectionId 
								   INNER JOIN  ORG.Section AS Sec ON SEC.Id = PR.SectionId
								   INNER JOIN  ORG.SubSection AS SubSec ON SubSEC.Id = PR.SubSectionId
									LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
								    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
									INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
											WHERE
												  PlantId = '" + identity.PlantId + @"'    AND (E.EmployeeStatus != 'Separated' OR ISNULL(E.DOS,'') = '' OR ISNULL(E.DOS,'')>CONVERT(DATE,'" + WorkDate + @"'))
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
Left join MST.ManpowerBudget MB ON mb.Id = E.BudgetCode
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
									   PlantId = '" + identity.PlantId + @"'  
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
Left join MST.ManpowerBudget MB ON mb.Id = E.BudgetCode
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
									   PlantId = '" + identity.PlantId + @"'   
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
									    PlantId = '" + identity.PlantId + @"'  
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
									    PlantId = '" + identity.PlantId + @"'   
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
 ---left join [ORG].[SubSection] ss on ss.Id=e.SubSectionId  

                                inner join AttdnProcessData apd on apd.EmpSystemID=e.SystemId								
									   inner join (select * from [dbo].[LeaveTransaction]
										 where  ('" + WorkDate + @"' Between FromDate and ToDate ))LT on LT.EmpSystemID=e.SystemId and APD.LTSystemID=LT.LTSystemID 
										  inner join [dbo].[LeaveType] lET on  lET.Id = APD.LTSystemId 

									WHERE
									   e.PlantId = '" + identity.PlantId + @"' and lET.LeaveType <> 'Maternity'  	 and apd.WorkDate='" + WorkDate + @"' ";

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
									   e.PlantId = '"+identity.PlantId+@"' and lET.LeaveType='Maternity'  	 and apd.WorkDate='" + WorkDate + @"'
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
        }//End Function
        public void GetAttendanceSummaryNoLineSql(string WorkDate, out DataSet dsRef)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            clsStaticInfo obs = null;
            try
            {
                string wc = string.Empty;

                obs = new clsStaticInfo();
                strSql = @"     SELECT distinct OnRoleEmployee.CompanyId CompanyId,OnRoleEmployee.CompanyName ColumnName,OnRoleEmployee.GroupName GroupName,OnRoleEmployee.CompanyGroupId CompanyGroupId
                             ,OnRoleEmployee.EmpCategory,OnRoleEmployee.Department, OnRoleEmployee.DesignationGroup,OnRoleEmployee.Section,OnRoleEmployee.SubSection
							 ,OnRoleEmployee.catgSeq,OnRoleEmployee.DesGrpSeq,OnRoleEmployee.DeptSeq,OnRoleEmployee.SecSeq,OnRoleEmployee.SubSecq
								,ISNULL(OnRoleEmployee.totalEmployee,0) OnRoleEmployee
								,ISNULL(PresentEmployee.totalPresentEmployee,0) totalPresentEmployee
								,ISNULL(AbsentEmployee.totalAbsentEmployee,0) totalAbsentEmployee
								,ISNULL(LateEmployee.totalLateEmployee,0) totalLateEmployee
								,ISNULL(LeaveEmployee.totalLeaveEmployee,0) totalLeaveEmployee
								,ISNULL(WeekOffEmployee.totalWeekoffEmployee,0)totalWeekoffEmployee
								,ISNULL(MaternityLeave.totalMaternityLeaveEmployee,0)totalMaternityLeaveEmployee
						        --,isnull((PresentEmployee.totalPresentEmployee + LateEmployee.totalLateEmployee),0)totalPresent
					     FROM
						   (SELECT
						   EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,
						   Ld.UserName DesignationGroup,Ld.Id DesigGrpId,Dept.UserName Department, Dept.Id DepartmentId,Sec.UserName Section,Sec.Id SectionId,
						   COUNT(E.SystemId) totalEmployee,SubSec.UserName as  SubSection ,SubSec.Id ssId,
						   C.UserName,cg.Id CompanyGroupId,c.Id CompanyId,c.UserName CompanyName,cg.UserName GroupName 
	                        	,Empc.Sequence catgSeq,Ld.Sequence DesGrpSeq,Sec.Sequence SecSeq,SubSec.Sequence SubSecq,Dept.Sequence DeptSeq 
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
									--INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId
									 inner join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
								 
								    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
											WHERE
												  PlantId = '" + identity.PlantId+"'    AND (E.EmployeeStatus != 'Separated' OR ISNULL(E.DOS,'') = '' OR ISNULL(E.DOS,'')>CONVERT(DATE,'"+WorkDate+@"'))
                                                   AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,'"+WorkDate+@"')  
												GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName
												,EmpC.Username,EmpC.Id
												,Ld.UserName,Ld.Id,Sec.UserName,SubSec.UserName ,SubSec.Id,Sec.Id,Dept.UserName,Dept.Id 
											,Empc.Sequence ,Ld.Sequence ,Sec.Sequence,SubSec.Sequence,Dept.Sequence
										
												) OnRoleEmployee
												LEFT OUTER JOIN
								  ( SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,Ld.UserName DesignationGroup,Ld.Id DesigGrpId,Dept.UserName Department,
						               Dept.Id DepartmentId,Sec.UserName Section,Sec.Id SectionId ,SubSec.UserName as  SubSection ,SubSec.Id ssId
								  ,COUNT(E.SystemId) totalPresentEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Present' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'"+WorkDate+ @"')
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
									--INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId				
									 inner join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId

									WHERE
									   PlantId = '" + identity.PlantId+@"'  
									GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName
												,EmpC.Username,EmpC.Id
												,Ld.UserName,Ld.Id,Sec.UserName,Sec.Id,SubSec.UserName ,SubSec.Id,Dept.UserName,Dept.Id 							
									)
									PresentEmployee
									ON OnRoleEmployee.CompanyGroupId = PresentEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = PresentEmployee.CompanyId
									AND OnRoleEmployee.EmpCategoryId = PresentEmployee.EmpCategoryId AND OnRoleEmployee.DesigGrpId = PresentEmployee.DesigGrpId
									AND OnRoleEmployee.DepartmentId = PresentEmployee.DepartmentId AND OnRoleEmployee.SectionId = PresentEmployee.SectionId
									 AND OnRoleEmployee.ssId = PresentEmployee.ssId

									LEFT OUTER JOIN
									----------------------------
									 (SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,Ld.UserName DesignationGroup,Ld.Id DesigGrpId,Dept.UserName Department,
						               Dept.Id DepartmentId,Sec.UserName Section,Sec.Id SectionId ,SubSec.UserName as  SubSection ,SubSec.Id ssId
									 ,COUNT(E.SystemId) totalAbsentEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Absent' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'"+WorkDate+ @"')
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
									--INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId				
									 inner join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId

									WHERE
									   PlantId = '" + identity.PlantId+@"'   
									GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
											,Ld.UserName,Ld.Id,Sec.UserName,SubSec.UserName ,SubSec.Id,Sec.Id,Dept.UserName,Dept.Id 
									)
									AbsentEmployee ON 
										 OnRoleEmployee.CompanyGroupId = AbsentEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = AbsentEmployee.CompanyId
									AND OnRoleEmployee.EmpCategoryId = AbsentEmployee.EmpCategoryId AND OnRoleEmployee.DesigGrpId = AbsentEmployee.DesigGrpId
									AND OnRoleEmployee.DepartmentId = AbsentEmployee.DepartmentId AND OnRoleEmployee.SectionId = AbsentEmployee.SectionId
									 AND OnRoleEmployee.ssId = AbsentEmployee.ssId

									LEFT OUTER JOIN
									(SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,Ld.UserName DesignationGroup,Ld.Id DesigGrpId,Dept.UserName Department,
						               Dept.Id DepartmentId,Sec.UserName Section,Sec.Id SectionId,SubSec.UserName as  SubSection ,SubSec.Id ssId
									,COUNT(E.SystemId) totalLateEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Late' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'"+WorkDate+ @"')
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
									--INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId				
									 inner join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId

									WHERE
									    PlantId = '" + identity.PlantId+@"'  
									GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
											,Ld.UserName,Ld.Id,Sec.UserName,SubSec.UserName ,SubSec.Id,Sec.Id,Dept.UserName,Dept.Id 
									)
									LateEmployee on
											 OnRoleEmployee.CompanyGroupId = LateEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = LateEmployee.CompanyId
									AND OnRoleEmployee.EmpCategoryId = LateEmployee.EmpCategoryId AND OnRoleEmployee.DesigGrpId = LateEmployee.DesigGrpId
									AND OnRoleEmployee.DepartmentId = LateEmployee.DepartmentId AND OnRoleEmployee.SectionId = LateEmployee.SectionId 
									 AND OnRoleEmployee.ssId = LateEmployee.ssId
										LEFT OUTER JOIN
									(SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,Ld.UserName DesignationGroup,Ld.Id DesigGrpId,Dept.UserName Department,
						               Dept.Id DepartmentId,Sec.UserName Section,Sec.Id SectionId,SubSec.UserName as  SubSection ,SubSec.Id ssId
									,COUNT(E.SystemId) totalWeekoffEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category IN('Holiday', 'Weekend') AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'"+WorkDate+ @"')
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
									--INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId				
									 inner join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId

									WHERE
									    PlantId = '" + identity.PlantId+@"'   
									GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
											,Ld.UserName,Ld.Id,Sec.UserName,Sec.Id,SubSec.UserName,SubSec.Id ,Dept.UserName,Dept.Id 	
									)
									WeekOffEmployee ON 
									
										 OnRoleEmployee.CompanyGroupId = WeekOffEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = WeekOffEmployee.CompanyId
									AND OnRoleEmployee.EmpCategoryId = WeekOffEmployee.EmpCategoryId AND OnRoleEmployee.DesigGrpId = WeekOffEmployee.DesigGrpId
									AND OnRoleEmployee.DepartmentId = WeekOffEmployee.DepartmentId AND OnRoleEmployee.SectionId = WeekOffEmployee.SectionId AND OnRoleEmployee.ssId = WeekOffEmployee.ssId
									
                                             LEFT OUTER JOIN
    
									(SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,Ld.UserName DesignationGroup,Ld.Id DesigGrpId,Dept.UserName Department,
						               Dept.Id DepartmentId,Sec.UserName Section,Sec.Id SectionId ,SubSec.UserName as  SubSection ,SubSec.Id ssId
									,COUNT(E.SystemId) totalLeaveEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Leave' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'"+WorkDate+ @"')
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
									---INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId				
									 inner join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId


                                inner join AttdnProcessData apd on apd.EmpSystemID=e.SystemId								
									   inner join (select * from [dbo].[LeaveTransaction]
										 where  ('" + WorkDate + @"' Between FromDate and ToDate ))LT on LT.EmpSystemID=e.SystemId and APD.LTSystemID=LT.LTSystemID 
										  inner join [dbo].[LeaveType] lET on  lET.Id = APD.LTSystemId 


									WHERE
									   e.PlantId = '" + identity.PlantId+ @"' and lET.LeaveType <> 'Maternity'  	 and apd.WorkDate='" + WorkDate + @"'
									    GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
											,Ld.UserName,Ld.Id,Sec.UserName,Sec.Id,SubSec.UserName ,SubSec.Id ,Dept.UserName,Dept.Id
									)
								LeaveEmployee on


                                    OnRoleEmployee.CompanyGroupId = LeaveEmployee.CompanyGroupId AND OnRoleEmployee.CompanyId = LeaveEmployee.CompanyId

                                    AND OnRoleEmployee.EmpCategoryId = LeaveEmployee.EmpCategoryId AND OnRoleEmployee.DesigGrpId = LeaveEmployee.DesigGrpId

                                    AND OnRoleEmployee.DepartmentId = LeaveEmployee.DepartmentId  AND OnRoleEmployee.SectionId = LeaveEmployee.SectionId AND OnRoleEmployee.ssId = LeaveEmployee.ssId

									left outer join
									(
									SELECT EmpC.UserName EmpCategory,  EmpC.Id EmpCategoryId,Ld.UserName DesignationGroup,Ld.Id DesigGrpId,Dept.UserName Department,
						               Dept.Id DepartmentId,Sec.UserName Section,Sec.Id SectionId ,SubSec.UserName as  SubSection ,SubSec.Id ssId
									,COUNT(E.SystemId) totalMaternityLeaveEmployee,cg.Id CompanyGroupId,cg.UserName GroupName,
									C.Id AS CompanyId,C.UserName CompanyName

									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId

									LEFT OUTER JOIN  (--*
								SELECT * FROM EmployeeInformation
								WHERE SystemId IN (--**
								SELECT DISTINCT EmpSystemID FROM AttdnProcessData  APD
									LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
									WHERE DT.Category = 'Leave' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,'" + WorkDate+ @"')
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
									---INNER JOIN [HKP].DesignationGroup DesGrp ON DesGrp.Id = DesM.DesignationGroupId				
									 inner join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
									  inner join AttdnProcessData apd on apd.EmpSystemID=e.SystemId								
										  left join (select * from [dbo].[LeaveTransaction]
										 where  ('" + WorkDate+@"' Between FromDate and ToDate ))LT on LT.EmpSystemID=e.SystemId and APD.LTSystemID=LT.LTSystemID 
										  left join [dbo].[LeaveType] lET on  lET.Id = APD.LTSystemId 

									WHERE
									   e.PlantId = '"+identity.PlantId+@"' and lET.LeaveType='Maternity'  	 and apd.WorkDate='"+WorkDate+@"'	
									   GROUP BY C.UserName,cg.Id,c.Id,c.UserName,cg.UserName,EmpC.Username,EmpC.Id
											,Ld.UserName,Ld.Id,Sec.UserName,Sec.Id ,SubSec.UserName,SubSec.Id,Dept.UserName,Dept.Id
									) MaternityLeave on 
                                    OnRoleEmployee.CompanyGroupId = MaternityLeave.CompanyGroupId AND OnRoleEmployee.CompanyId = MaternityLeave.CompanyId
                                    AND OnRoleEmployee.EmpCategoryId = MaternityLeave.EmpCategoryId AND OnRoleEmployee.DesigGrpId = MaternityLeave.DesigGrpId
                                    AND OnRoleEmployee.DepartmentId = MaternityLeave.DepartmentId  AND OnRoleEmployee.SectionId = MaternityLeave.SectionId AND OnRoleEmployee.ssId = MaternityLeave.ssId

                                    ORDER BY OnRoleEmployee.catgSeq,OnRoleEmployee.DeptSeq,OnRoleEmployee.SecSeq, OnRoleEmployee.SubSecq,OnRoleEmployee.DesGrpSeq";

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
        }//End Function

        #region -----------------------------------Excel Report--------------------------------------------------
        public ActionResult Getdailyattendance_BACKUP(string WorkDate)//XlsDailyAttendanceSummaryRpt()
        {

            #region Variable

            clsReport objRpt = null;

            DataSet dsAttdnSummary = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ru = new ReportUtility();
                objRpt = new clsReport();
                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();
                para.PlantId = identity.PlantId;

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion Variable

                #region DataSet

                GetAttendanceSummarySql(WorkDate, out dsAttdnSummary);

                DataTable dtAttdnSummary = dsAttdnSummary.Tables[0];

                DataView dvAttendance = new DataView(dsAttdnSummary.Tables[0]);

                if (dsAttdnSummary.Tables[0].Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                xlsRow = 5;
                xlsCol = 1;

                var colEmpCatg = 0;
                var colDepartment = 0;
                var colSec = 0;
                var ColDesigGrp = 0;

                var colOnRole = 0;
                var colPresent = 0;
                var colAbsent = 0;
                var colLate = 0;
                var colLeave = 0;
                var colWeekOffHoliday = 0;
                var colAbsPer = 0;
                var ColLine = 0;

                var ColReMale = 0;
                var ColReFemale = 0;

                var ColPreMale = 0;
                var ColPreFemale = 0;

                #region------------------Column Header------------------
                SetHeadText("Category", sheet1, xlsRow, ref xlsCol, out colEmpCatg, 9);
                SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out colDepartment, 37);
                SetHeadText("Section", sheet1, xlsRow, ref xlsCol, out colSec, 13);
                SetHeadText("Designation", sheet1, xlsRow, ref xlsCol, out ColDesigGrp, 11.71);
                SetHeadText("Line", sheet1, xlsRow, ref xlsCol, out ColLine, 11.71);


                SetHeadText("Recruited", sheet1, xlsRow, ref xlsCol, out colOnRole, 9.14);
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                xlsCol = xlsCol - 1;
                SetHeadText("Male", sheet1, xlsRow + 1, ref xlsCol, out ColReMale, 11.71);
                SetHeadText("Female", sheet1, xlsRow + 1, ref xlsCol, out ColReFemale, 11.71);

                SetHeadText("Present", sheet1, xlsRow, ref xlsCol, out colPresent, 7.29);
                xlsCol = xlsCol - 1;
                SetHeadText("Male", sheet1, xlsRow + 1, ref xlsCol, out ColPreMale, 11.71);
                SetHeadText("Female", sheet1, xlsRow + 1, ref xlsCol, out ColPreFemale, 11.71);


                SetHeadText("Absent", sheet1, xlsRow, ref xlsCol, out colAbsent, 7);
                SetHeadText("Late", sheet1, xlsRow, ref xlsCol, out colLate, 7);
                SetHeadText("Leave", sheet1, xlsRow, ref xlsCol, out colLeave, 7);
                SetHeadText("W.Off", sheet1, xlsRow, ref xlsCol, out colWeekOffHoliday, 15);
                SetHeadText("Abs%", sheet1, xlsRow, ref xlsCol, out colAbsPer, 15);
                int RowHeaderLimit = xlsRow;
                #endregion------------------Column Header------------------

                endXlsCol = (xlsCol - 1);
                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                //Param param = new Param();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;

                string FactoryAddress = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Daily Attendance Summary";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                var strRptDateRange = "";
                strRptDateRange = WorkDate;
                sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                //  var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                //xlsRow--;
                xlsRow--;
                var startXlsRow = xlsRow;
                if (dtAttdnSummary.Rows.Count > 0)
                {
                    string _empcat = string.Empty;
                    string _department = string.Empty;
                    string _section = string.Empty;
                    string _DesignationGroup = string.Empty;
                    string _Line = string.Empty;
                    string _Gender = string.Empty;

                    var isFirst = true;
                    var catFRow = xlsRow;
                    ArrayList al = new ArrayList();
                    var lastEmpCat = string.Empty;
                    for (int i = 0; i <= dtAttdnSummary.Rows.Count - 1; i++)
                    {
                        var catLRow = xlsRow;
                        if (_empcat != dtAttdnSummary.Rows[i]["EmpCategory"].ToString() && string.IsNullOrEmpty(dtAttdnSummary.Rows[i]["EmpCategory"].ToString()) == false)
                        {
                            _empcat = dtAttdnSummary.Rows[i]["EmpCategory"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastEmpCat = _empcat;
                                al.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();
                                sheet1.Range[xlsRow, colOnRole].Formula = "=SUM(" + ru.GetColumnNameForXls(colOnRole) + catFRow + ":" + ru.GetColumnNameForXls(colOnRole) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresent) + catFRow + ":" + ru.GetColumnNameForXls(colPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colAbsent].Formula = "=SUM(" + ru.GetColumnNameForXls(colAbsent) + catFRow + ":" + ru.GetColumnNameForXls(colAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colLate].Formula = "=SUM(" + ru.GetColumnNameForXls(colLate) + catFRow + ":" + ru.GetColumnNameForXls(colLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colLeave) + catFRow + ":" + ru.GetColumnNameForXls(colLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colWeekOffHoliday].Formula = "=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;
                                xlsRow++;
                            }
                            #endregion

                            SetCellText(sheet1, xlsRow, colEmpCatg, _empcat);
                            _department = dtAttdnSummary.Rows[i]["Department"].ToString();
                            SetCellText(sheet1, xlsRow, colDepartment, _department);
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString();
                            SetCellText(sheet1, xlsRow, colSec, _section);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString();
                            SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString();
                            SetCellText(sheet1, xlsRow, ColLine, _Line);

                            if (catFRow < xlsRow)
                            {

                                catFRow = xlsRow;
                            }
                        }
                        else if (_department != dtAttdnSummary.Rows[i]["Department"].ToString())
                        {
                            _department = dtAttdnSummary.Rows[i]["Department"].ToString(); SetCellText(sheet1, xlsRow, colDepartment, _department);
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }
                        else if (_section != dtAttdnSummary.Rows[i]["Section"].ToString())
                        {
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }
                        else if (_DesignationGroup != dtAttdnSummary.Rows[i]["DesignationGroup"].ToString())
                        {
                            _DesignationGroup = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }
                        var Female = ""; var Male = "";

                        DataView dvAttdnSummaryMale = new DataView(dtAttdnSummary)
                        {
                            RowFilter = "EmpCategory = '" + _empcat + "' and Department ='" + _department + "'  and Section = '" + _section + "' and LealDesignation = '" + _DesignationGroup + "' and Line = '" + _Line + "'AND GenderId='Male'"
                        };

                        if (dvAttdnSummaryMale.Count > 0)
                        {


                            Male = dvAttdnSummaryMale[0]["OnRoleEmployee"].ToString();
                        }

                        DataView dvAttdnSummaryFeMale = new DataView(dtAttdnSummary)
                        {
                            RowFilter = "(EmpCategory = '" + _empcat + "' and Department ='" + _department + "'  and Section = '" + _section + "' and LealDesignation = '" + _DesignationGroup + "' and Line = '" + _Line + "' AND GenderId='Female')"
                        };
                        if (dvAttdnSummaryFeMale.Count > 0)
                        {

                            Female = dvAttdnSummaryFeMale[0]["OnRoleEmployee"].ToString();
                        }

                        SetCellText(sheet1, xlsRow, ColReMale, clsStaticInfo.dbl(Male));
                        SetCellText(sheet1, xlsRow, ColReFemale, clsStaticInfo.dbl(Female));
                        //SetCellText(sheet1, xlsRow, colOnRole, Convert.ToDouble(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString()));

                        SetCellText(sheet1, xlsRow, colPresent, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalPresentEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colAbsent, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colLate, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalLateEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colLeave, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalLeaveEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colWeekOffHoliday, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalWeekoffEmployee"].ToString()));

                        SetCellText(sheet1, xlsRow, ColLine, Convert.ToString(dtAttdnSummary.Rows[i]["Line"].ToString()));

                        //SetCellText(sheet1, xlsRow, ColReMale, Convert.ToString(dtAttdnSummary.Rows[i]["GenderID"].ToString()));

                        var ap = Convert.ToDouble(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString()) / Convert.ToDouble(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                        SetCellText(sheet1, xlsRow, colAbsPer, Convert.ToDouble(ap * 100));
                        xlsRow++;
                    }//for emp count

                    #region Last subtotal
                    al.Add(xlsRow);
                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                    sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();
                    sheet1.Range[xlsRow, colOnRole].Formula = "=SUM(" + ru.GetColumnNameForXls(colOnRole) + catFRow + ":" + ru.GetColumnNameForXls(colOnRole) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresent) + catFRow + ":" + ru.GetColumnNameForXls(colPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colAbsent].Formula = "=SUM(" + ru.GetColumnNameForXls(colAbsent) + catFRow + ":" + ru.GetColumnNameForXls(colAbsent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colLate].Formula = "=SUM(" + ru.GetColumnNameForXls(colLate) + catFRow + ":" + ru.GetColumnNameForXls(colLate) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colLeave) + catFRow + ":" + ru.GetColumnNameForXls(colLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colWeekOffHoliday].Formula = "=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;
                    xlsRow++;
                    #endregion

                    #region Grand Total
                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();


                    sheet1.Range[xlsRow, colOnRole].Formula = GetFormulaGrandTotal(al, colOnRole);
                    sheet1.Range[xlsRow, colPresent].Formula = GetFormulaGrandTotal(al, colPresent);
                    sheet1.Range[xlsRow, colAbsent].Formula = GetFormulaGrandTotal(al, colAbsent);
                    sheet1.Range[xlsRow, colLate].Formula = GetFormulaGrandTotal(al, colLate);
                    sheet1.Range[xlsRow, colLeave].Formula = GetFormulaGrandTotal(al, colLeave);
                    sheet1.Range[xlsRow, colWeekOffHoliday].Formula = GetFormulaGrandTotal(al, colWeekOffHoliday);
                    sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;


                    #endregion

                }

                #endregion ----------------------Data-----------------------

                var endXlsRow = xlsRow;
                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var xx = RowHeaderLimit + 1;
                sheet1.UsedRange["A" + xx].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "AttendanceSummary";
                #endregion

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "AttendanceSummary.xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult Getdailyattendance_backup1(string WorkDate)//XlsDailyAttendanceSummaryRpt()
        {

            #region Variable

            clsReport objRpt = null;

            DataSet dsAttdnSummary = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ru = new ReportUtility();
                objRpt = new clsReport();
                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();
                para.PlantId = identity.PlantId;

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion Variable

                #region DataSet

                GetAttendanceSummarySql(WorkDate, out dsAttdnSummary);

                DataTable dtAttdnSummary = dsAttdnSummary.Tables[0];
                if (dtAttdnSummary.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
                DataView dvAttendance = new DataView(dsAttdnSummary.Tables[0]);
                #endregion DataSet
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                xlsRow = 5;
                xlsCol = 1;

                var colEmpCatg = 0;
                var colDepartment = 0;
                var colSec = 0;
                var colSubSec = 0;
                var ColDesigGrp = 0;
                var colOnRole = 0;
                var colPresent = 0;
                var colAbsent = 0;
                var colLate = 0;
                var colLeave = 0;
                var colWeekOffHoliday = 0;
                var colAbsPer = 0;
                var ColLine = 0;
                var ColAbsMale = 0;
                var ColAbsFemale = 0;
                var ColLateMale = 0;
                var ColLateFemale = 0;
                var ColReMale = 0;
                var ColReFemale = 0;
                var ColLeaveMale = 0;
                var ColLeaveFemale = 0;
                var ColPreMale = 0;
                var ColPreFemale = 0;
                var ColWeekOffMale = 0;
                var ColWeekOffFemale = 0;
                var ColPerMale = 0;
                var ColPerFemale = 0;

                #region------------------Column Header------------------
                SetHeadText("Category", sheet1, xlsRow, ref xlsCol, out colEmpCatg, 9);
                SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out colDepartment, 37);
                SetHeadText("Section", sheet1, xlsRow, ref xlsCol, out colSec, 13);
                SetHeadText("SubSection", sheet1, xlsRow, ref xlsCol, out colSubSec, 15);
                SetHeadText("Designation", sheet1, xlsRow, ref xlsCol, out ColDesigGrp, 25);
                SetHeadText("Line", sheet1, xlsRow, ref xlsCol, out ColLine, 11.71);


                SetHeadText("Recruited", sheet1, xlsRow, ref xlsCol, out colOnRole, 9.14);
                xlsCol = xlsCol - 1;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].BorderAround(ExcelLineStyle.Thin);

                SetHeadText("Male", sheet1, xlsRow + 1, ref xlsCol, out ColReMale, 11.71);
                SetHeadText("Female", sheet1, xlsRow + 1, ref xlsCol, out ColReFemale, 11.71);

                SetHeadText("Present", sheet1, xlsRow, ref xlsCol, out colPresent, 7.29);
                xlsCol = xlsCol - 1;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].BorderAround(ExcelLineStyle.Thin);

                SetHeadText("Male", sheet1, xlsRow + 1, ref xlsCol, out ColPreMale, 11.71);
                SetHeadText("Female", sheet1, xlsRow + 1, ref xlsCol, out ColPreFemale, 11.71);

                SetHeadText("Absent", sheet1, xlsRow, ref xlsCol, out colAbsent, 7);
                xlsCol = xlsCol - 1;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].BorderAround(ExcelLineStyle.Thin);

                SetHeadText("Male", sheet1, xlsRow + 1, ref xlsCol, out ColAbsMale, 11.71);
                SetHeadText("Female", sheet1, xlsRow + 1, ref xlsCol, out ColAbsFemale, 11.71);

                SetHeadText("Late", sheet1, xlsRow, ref xlsCol, out colLate, 7);
                xlsCol = xlsCol - 1;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].BorderAround(ExcelLineStyle.Thin);
                SetHeadText("Male", sheet1, xlsRow + 1, ref xlsCol, out ColLateMale, 11.71);
                SetHeadText("Female", sheet1, xlsRow + 1, ref xlsCol, out ColLateFemale, 11.71);

                SetHeadText("Leave", sheet1, xlsRow, ref xlsCol, out colLeave, 7);
                xlsCol = xlsCol - 1;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].BorderAround(ExcelLineStyle.Thin);
                SetHeadText("Male", sheet1, xlsRow + 1, ref xlsCol, out ColLeaveMale, 11.71);
                SetHeadText("Female", sheet1, xlsRow + 1, ref xlsCol, out ColLeaveFemale, 11.71);

                SetHeadText("W.Off", sheet1, xlsRow, ref xlsCol, out colWeekOffHoliday, 15);
                xlsCol = xlsCol - 1;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].BorderAround(ExcelLineStyle.Thin);
                SetHeadText("Male", sheet1, xlsRow + 1, ref xlsCol, out ColWeekOffMale, 11.71);
                SetHeadText("Female", sheet1, xlsRow + 1, ref xlsCol, out ColWeekOffFemale, 11.71);

                SetHeadText("Abs%", sheet1, xlsRow, ref xlsCol, out colAbsPer, 15);
                xlsCol = xlsCol - 1;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].BorderAround(ExcelLineStyle.Thin);
                SetHeadText("Male", sheet1, xlsRow + 1, ref xlsCol, out ColPerMale, 11.71);
                SetHeadText("Female", sheet1, xlsRow + 1, ref xlsCol, out ColPerFemale, 11.71);

                int RowHeaderLimit = xlsRow;
                #endregion------------------Column Header------------------

                endXlsCol = (xlsCol - 1);
                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                //Param param = new Param();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;

                string FactoryAddress = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Daily Attendance Summary";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                var strRptDateRange = "";
                strRptDateRange = WorkDate;
                sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                //  var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                //xlsRow--;
                xlsRow--;
                var startXlsRow = xlsRow;

                string _empcat = string.Empty;
                string _department = string.Empty;
                string _section = string.Empty;
                string _DesignationGroup = string.Empty;
                string _SubSection = string.Empty;
                string _Line = string.Empty;
                string _Gender = string.Empty;

                var isFirst = true;
                var catFRow = xlsRow;
                ArrayList al = new ArrayList();
                var lastEmpCat = string.Empty;
                int StartRow = xlsRow;
                string tempId = "";
                for (int i = 0; i <= dtAttdnSummary.Rows.Count - 1; i++)
                {

                    try
                    {
                        var catLRow = xlsRow;
                        if (_empcat != dtAttdnSummary.Rows[i]["EmpCategory"].ToString() && string.IsNullOrEmpty(dtAttdnSummary.Rows[i]["EmpCategory"].ToString()) == false)
                        {
                            _empcat = dtAttdnSummary.Rows[i]["EmpCategory"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastEmpCat = _empcat;
                                al.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();

                                sheet1.Range[xlsRow, ColReMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColReMale) + catFRow + ":" + ru.GetColumnNameForXls(ColReMale) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, ColReFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColReFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, ColPreMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColPreMale) + catFRow + ":" + ru.GetColumnNameForXls(ColPreMale) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, ColPreFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColPreFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColPreFemale) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, ColAbsMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColAbsMale) + catFRow + ":" + ru.GetColumnNameForXls(ColAbsMale) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, ColAbsFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColAbsFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColAbsFemale) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, ColLateMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColLateMale) + catFRow + ":" + ru.GetColumnNameForXls(ColLateMale) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, ColLateFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColLateFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColLateFemale) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, ColLeaveMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColLeaveMale) + catFRow + ":" + ru.GetColumnNameForXls(ColLeaveMale) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, ColLeaveFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColLeaveFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColLeaveFemale) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, ColWeekOffMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColWeekOffMale) + catFRow + ":" + ru.GetColumnNameForXls(ColWeekOffMale) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, ColWeekOffFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColWeekOffFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColWeekOffFemale) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, ColReMale, xlsRow, ColPerFemale].CellStyle.Font.Bold = true;
                                xlsRow++;
                            }
                            #endregion

                            SetCellText(sheet1, xlsRow, colEmpCatg, _empcat);
                            _department = dtAttdnSummary.Rows[i]["Department"].ToString();
                            SetCellText(sheet1, xlsRow, colDepartment, _department);
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString();
                            SetCellText(sheet1, xlsRow, colSec, _section);

                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString();

                            SetCellText(sheet1, xlsRow, colSubSec, _SubSection);
                            _SubSection = dtAttdnSummary.Rows[i]["SubSection"].ToString();

                            SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);

                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString();
                            SetCellText(sheet1, xlsRow, ColLine, _Line);

                            if (catFRow < xlsRow)
                            {

                                catFRow = xlsRow;
                            }
                        }
                        else if (_department != dtAttdnSummary.Rows[i]["Department"].ToString())
                        {
                            _department = dtAttdnSummary.Rows[i]["Department"].ToString(); SetCellText(sheet1, xlsRow, colDepartment, _department);
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _SubSection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _SubSection);
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);

                        }
                        else if (_section != dtAttdnSummary.Rows[i]["Section"].ToString())
                        {
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _SubSection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _SubSection);
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);

                        }

                        else if (_SubSection != dtAttdnSummary.Rows[i]["SubSection"].ToString())
                        {
                            _SubSection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _SubSection);
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);

                        }

                        else if (_DesignationGroup != dtAttdnSummary.Rows[i]["LealDesignation"].ToString())
                        {
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);


                        }
                        else if (_Line != dtAttdnSummary.Rows[i]["Line"].ToString())
                        {
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);

                        }

                    }
                    catch (Exception)
                    {


                    }


                    try
                    {
                        if (dtAttdnSummary.Rows[i]["GenderID"].ToString().ToUpper() == "MALE")
                        {
                            sheet1[xlsRow, ColReMale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                            sheet1[xlsRow, ColPreMale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalPresentEmployee"].ToString());
                            sheet1[xlsRow, ColAbsMale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString());
                            sheet1[xlsRow, ColLateMale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalLateEmployee"].ToString());
                            sheet1[xlsRow, ColLeaveMale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalWeekoffEmployee"].ToString());
                            sheet1[xlsRow, ColWeekOffMale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalWeekoffEmployee"].ToString());
                            sheet1[xlsRow, ColPerMale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString()) / clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString()) * 100;
                        }
                        else
                        {
                            sheet1[xlsRow, ColReFemale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                            sheet1[xlsRow, ColPreFemale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalPresentEmployee"].ToString());
                            sheet1[xlsRow, ColAbsFemale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString());
                            sheet1[xlsRow, ColLateFemale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalLateEmployee"].ToString());
                            sheet1[xlsRow, ColLeaveFemale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalWeekoffEmployee"].ToString());
                            sheet1[xlsRow, ColWeekOffFemale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalWeekoffEmployee"].ToString());
                            sheet1[xlsRow, ColPerFemale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString()) / clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString()) * 100;

                        }

                    }
                    catch (Exception ex)
                    {


                    }

                    tempId = dtAttdnSummary.Rows[i]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i]["Department"].ToString() + dtAttdnSummary.Rows[i]["Section"].ToString() + dtAttdnSummary.Rows[i]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i]["Line"].ToString();
                    if (tempId.Contains("WorkerProductionSewingOperatorLine-02"))
                    {
                        var dd = tempId;
                    }
                    try
                    {
                        var tempId2 = dtAttdnSummary.Rows[i + 1]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i + 1]["Department"].ToString() + dtAttdnSummary.Rows[i + 1]["Section"].ToString() + dtAttdnSummary.Rows[i + 1]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i + 1]["Line"].ToString();
                        if (tempId != tempId2)
                            xlsRow++;
                    }
                    catch (Exception)
                    {
                        xlsRow++;

                    }

                }//for emp count

                #region Last subtotal
                al.Add(xlsRow);
                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();

                sheet1.Range[xlsRow, ColReMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColReMale) + catFRow + ":" + ru.GetColumnNameForXls(ColReMale) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, ColReFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColReFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow - 1) + ")";

                sheet1.Range[xlsRow, ColAbsMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColAbsMale) + catFRow + ":" + ru.GetColumnNameForXls(ColAbsMale) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, ColAbsFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColAbsFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColAbsFemale) + (xlsRow - 1) + ")";

                sheet1.Range[xlsRow, ColLateMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColLateMale) + catFRow + ":" + ru.GetColumnNameForXls(ColLateMale) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, ColLateFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColLateFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColLateFemale) + (xlsRow - 1) + ")";

                sheet1.Range[xlsRow, ColLeaveMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColLeaveMale) + catFRow + ":" + ru.GetColumnNameForXls(ColLeaveMale) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, ColLeaveFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColLeaveFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColLeaveFemale) + (xlsRow - 1) + ")";

                sheet1.Range[xlsRow, ColWeekOffMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColWeekOffMale) + catFRow + ":" + ru.GetColumnNameForXls(ColWeekOffMale) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, ColWeekOffFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColWeekOffFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColWeekOffFemale) + (xlsRow - 1) + ")";

                sheet1.Range[xlsRow, ColReMale, xlsRow, ColWeekOffFemale].CellStyle.Font.Bold = true;
                xlsRow++;
                #endregion

                #region Grand Total
                SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();

                sheet1.Range[xlsRow, ColReMale].Formula = GetFormulaGrandTotal(al, ColReMale);
                sheet1.Range[xlsRow, ColReFemale].Formula = GetFormulaGrandTotal(al, ColReFemale);

                sheet1.Range[xlsRow, ColAbsMale].Formula = GetFormulaGrandTotal(al, ColAbsMale);
                sheet1.Range[xlsRow, ColAbsFemale].Formula = GetFormulaGrandTotal(al, ColAbsFemale);

                sheet1.Range[xlsRow, ColLateMale].Formula = GetFormulaGrandTotal(al, ColLateMale);
                sheet1.Range[xlsRow, ColLateFemale].Formula = GetFormulaGrandTotal(al, ColLateFemale);

                sheet1.Range[xlsRow, ColLeaveMale].Formula = GetFormulaGrandTotal(al, ColLeaveMale);
                sheet1.Range[xlsRow, ColLeaveFemale].Formula = GetFormulaGrandTotal(al, ColLeaveFemale);

                sheet1.Range[xlsRow, ColWeekOffMale].Formula = GetFormulaGrandTotal(al, ColWeekOffMale);
                sheet1.Range[xlsRow, ColWeekOffFemale].Formula = GetFormulaGrandTotal(al, ColWeekOffFemale);

                sheet1.Range[xlsRow, ColReMale, xlsRow, ColWeekOffFemale].CellStyle.Font.Bold = true;

                #endregion

                #endregion ----------------------Data-----------------------

                var endXlsRow = xlsRow;
                sheet1.Range[StartRow, ColPerMale, xlsRow, ColPerMale].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet1.Range[StartRow, ColPerFemale, xlsRow, ColPerFemale].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet1.IsDisplayZeros = false;

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var xx = RowHeaderLimit + 2;
                sheet1.UsedRange["A" + xx].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "AttendanceSummary";
                #endregion

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "AttendanceSummary.xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Getdailyattendance(string WorkDate)//XlsDailyAttendanceSummaryRpt()
        {

            #region Variable
            clsReport objRpt = null;
            DataSet dsAttdnSummary = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;
            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            #endregion Variable
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ru = new ReportUtility();
                objRpt = new clsReport();
                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();
                para.PlantId = identity.PlantId;
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                #endregion Variable

                #region DataSet
                GetAttendanceSummarySql(WorkDate, out dsAttdnSummary);
                DataTable dtAttdnSummary = dsAttdnSummary.Tables[0];
                if (dtAttdnSummary.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
                DataView dvAttendance = new DataView(dsAttdnSummary.Tables[0]);
                #endregion DataSet
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;
                xlsRow = 5;
                xlsCol = 1;
                var colEmpCatg = 0;
                var colDepartment = 0;
                var colSec = 0;
                var colSubSec = 0;
                var ColDesigGrp = 0;
                var colOnRole = 0;
                var colPresent = 0;
                var colAbsent = 0;
                var colLate = 0;
                var colLeave = 0;
                var colMaternityLeave = 0;
                var colWeekOffHoliday = 0;
                var colAbsPer = 0;
                var ColLine = 0;
                var ColReMale = 0;
                var ColReFemale = 0;

                #region------------------Column Header------------------
                SetHeadText("Category", sheet1, xlsRow, ref xlsCol, out colEmpCatg, 9);
                SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out colDepartment, 37);
                SetHeadText("Section", sheet1, xlsRow, ref xlsCol, out colSec, 13);
                SetHeadText("SubSection", sheet1, xlsRow, ref xlsCol, out colSubSec, 15);
                SetHeadText("Designation", sheet1, xlsRow, ref xlsCol, out ColDesigGrp, 25);
                SetHeadText("Line", sheet1, xlsRow, ref xlsCol, out ColLine, 11.71);


                SetHeadText("On Role", sheet1, xlsRow, ref xlsCol, out colOnRole, 9.14);
                xlsCol = xlsCol - 1;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].BorderAround(ExcelLineStyle.Thin);

                SetHeadText("Male", sheet1, xlsRow + 1, ref xlsCol, out ColReMale, 11.71);
                SetHeadText("Female", sheet1, xlsRow + 1, ref xlsCol, out ColReFemale, 11.71);

                SetHeadText("Present", sheet1, xlsRow, ref xlsCol, out colPresent, 7.29);
                SetHeadText("Absent", sheet1, xlsRow, ref xlsCol, out colAbsent, 7);
                SetHeadText("Late", sheet1, xlsRow, ref xlsCol, out colLate, 7);
                SetHeadText("Leave", sheet1, xlsRow, ref xlsCol, out colLeave, 7);
                SetHeadText("Maternity Leave", sheet1, xlsRow, ref xlsCol, out colMaternityLeave, 10);
                SetHeadText("W.Off", sheet1, xlsRow, ref xlsCol, out colWeekOffHoliday, 15);
                SetHeadText("Abs%", sheet1, xlsRow, ref xlsCol, out colAbsPer, 15);

                int RowHeaderLimit = xlsRow;
                #endregion------------------Column Header------------------

                endXlsCol = (xlsCol - 1);
                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                //Param param = new Param();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;

                string FactoryAddress = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Daily Attendance Summary";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                var strRptDateRange = "";
                strRptDateRange = WorkDate;
                sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------               
                var oRU = new ReportUtility();
                xlsRow = RowIndex;
                xlsRow--;
                var startXlsRow = xlsRow;
                string _empcat = string.Empty;
                string _department = string.Empty;
                string _section = string.Empty;
                string _DesignationGroup = string.Empty;
                string _SubSection = string.Empty;
                string _Line = string.Empty;
                string _Gender = string.Empty;
                var catFRow = xlsRow;
                ArrayList al = new ArrayList();
                var lastEmpCat = string.Empty;
                int StartRow = xlsRow;
                string tempId = "";
                string temp2 = "";
                double onrole = 0;
                double abs = 0;
                for (int i = 0; i <= dtAttdnSummary.Rows.Count - 1; i++)
                {
                    try
                    {
                        var catLRow = xlsRow;
                        if (_empcat != dtAttdnSummary.Rows[i]["EmpCategory"].ToString() && string.IsNullOrEmpty(dtAttdnSummary.Rows[i]["EmpCategory"].ToString()) == false)
                        {

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastEmpCat = _empcat;
                                string strSubTotalFormula = "=(" + ru.GetColumnNameForXls(colAbsent) + (xlsRow) + "/(" + ru.GetColumnNameForXls(ColReMale) + xlsRow + "+" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow) + "))*100";

                                al.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();

                                sheet1.Range[xlsRow, ColReMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColReMale) + catFRow + ":" + ru.GetColumnNameForXls(ColReMale) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, ColReFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColReFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, colPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresent) + catFRow + ":" + ru.GetColumnNameForXls(colPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colAbsent].Formula = "=SUM(" + ru.GetColumnNameForXls(colAbsent) + catFRow + ":" + ru.GetColumnNameForXls(colAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colLate].Formula = "=SUM(" + ru.GetColumnNameForXls(colLate) + catFRow + ":" + ru.GetColumnNameForXls(colLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colLeave) + catFRow + ":" + ru.GetColumnNameForXls(colLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colMaternityLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colMaternityLeave) + catFRow + ":" + ru.GetColumnNameForXls(colMaternityLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colWeekOffHoliday].Formula = "=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colAbsPer].Formula = strSubTotalFormula;  //"=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;

                                xlsRow++;
                            }
                            #endregion

                            _empcat = dtAttdnSummary.Rows[i]["EmpCategory"].ToString();
                            SetCellText(sheet1, xlsRow, colEmpCatg, _empcat);

                            _department = dtAttdnSummary.Rows[i]["Department"].ToString();
                            SetCellText(sheet1, xlsRow, colDepartment, _department);

                            _section = dtAttdnSummary.Rows[i]["Section"].ToString();
                            SetCellText(sheet1, xlsRow, colSec, _section);


                            _SubSection = dtAttdnSummary.Rows[i]["SubSection"].ToString();
                            SetCellText(sheet1, xlsRow, colSubSec, _SubSection);

                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString();
                            SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);

                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString();
                            SetCellText(sheet1, xlsRow, ColLine, _Line);


                            if (catFRow < xlsRow)
                            {

                                catFRow = xlsRow;
                            }
                            temp2 = dtAttdnSummary.Rows[i]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i]["Department"].ToString() + dtAttdnSummary.Rows[i]["Section"].ToString() + dtAttdnSummary.Rows[i]["SubSection"].ToString() + dtAttdnSummary.Rows[i]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i]["Line"].ToString();
                            if (temp2.Contains("StaffProductionSewingGeneralOfficer"))
                            {

                            }

                        }

                        //temp2 = dtAttdnSummary.Rows[i]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i]["Department"].ToString() + dtAttdnSummary.Rows[i]["Section"].ToString() + dtAttdnSummary.Rows[i]["SubSection"].ToString() + dtAttdnSummary.Rows[i]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i]["Line"].ToString();
                        //if (temp2.Contains("StaffProductionSewingGeneralOfficer"))
                        //{

                        //}
                        if (_department != dtAttdnSummary.Rows[i]["Department"].ToString())
                        {

                            _department = dtAttdnSummary.Rows[i]["Department"].ToString(); SetCellText(sheet1, xlsRow, colDepartment, _department);
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _SubSection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _SubSection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);

                        }
                        if (_section != dtAttdnSummary.Rows[i]["Section"].ToString())
                        {
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _SubSection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _SubSection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);
                        }
                        else if (_SubSection != dtAttdnSummary.Rows[i]["SubSection"].ToString())
                        {
                            _SubSection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _SubSection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);
                        }

                        if (_DesignationGroup != dtAttdnSummary.Rows[i]["LealDesignation"].ToString())
                        {
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);
                        }

                        if (_Line != dtAttdnSummary.Rows[i]["Line"].ToString())
                        {
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);
                        }



                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        if (dtAttdnSummary.Rows[i]["GenderID"].ToString().ToUpper() == "MALE")
                        {
                            sheet1[xlsRow, ColReMale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                        }
                        else
                        {
                            sheet1[xlsRow, ColReFemale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                        }


                        if (double.IsNaN(sheet1[xlsRow, colPresent].Number) == false)                        
                        sheet1[xlsRow, colPresent].Number += clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalPresentEmployee"].ToString());
                        else
                            sheet1[xlsRow, colPresent].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalPresentEmployee"].ToString());

                        if (double.IsNaN(sheet1[xlsRow, colAbsent].Number) == false)
                            sheet1[xlsRow, colAbsent].Number += clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString());
                        else
                            sheet1[xlsRow, colAbsent].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString());

                        if (double.IsNaN(sheet1[xlsRow, colLate].Number) == false)
                            sheet1[xlsRow, colLate].Number += clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalLateEmployee"].ToString());
                        else
                            sheet1[xlsRow, colLate].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalLateEmployee"].ToString());

                        if (double.IsNaN(sheet1[xlsRow, colLeave].Number) == false)
                            sheet1[xlsRow, colLeave].Number += clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalLeaveEmployee"].ToString());
                        else
                            sheet1[xlsRow, colLeave].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalLeaveEmployee"].ToString());

                        if (double.IsNaN(sheet1[xlsRow, colMaternityLeave].Number) == false)
                            sheet1[xlsRow, colMaternityLeave].Number += clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalMaternithyEmployee"].ToString());
                        else
                            sheet1[xlsRow, colMaternityLeave].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalMaternithyEmployee"].ToString());

                        if (double.IsNaN(sheet1[xlsRow, colWeekOffHoliday].Number) == false)
                            sheet1[xlsRow, colWeekOffHoliday].Number += clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalWeekoffEmployee"].ToString());
                        else
                            sheet1[xlsRow, colWeekOffHoliday].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalWeekoffEmployee"].ToString());

                        SetCellText(sheet1, xlsRow, ColLine, Convert.ToString(dtAttdnSummary.Rows[i]["Line"].ToString()));

                        string t1 = dtAttdnSummary.Rows[i]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i]["Department"].ToString() + dtAttdnSummary.Rows[i]["Section"].ToString() + dtAttdnSummary.Rows[i]["SubSection"].ToString() + dtAttdnSummary.Rows[i]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i]["Line"].ToString();
                        var t2 = dtAttdnSummary.Rows[i + 1]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i + 1]["Department"].ToString() + dtAttdnSummary.Rows[i + 1]["Section"].ToString() + dtAttdnSummary.Rows[i + 1]["SubSection"].ToString() + dtAttdnSummary.Rows[i + 1]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i + 1]["Line"].ToString();
                        if (t1 != t2)
                        {
                            double abss = Convert.ToDouble(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString());
                            double onrolee = Convert.ToDouble(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                            string strFormula = "=(" + ru.GetColumnNameForXls(colAbsent) + (xlsRow) + "/(" + ru.GetColumnNameForXls(ColReMale) + xlsRow + "+" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow) + "))*100";
                            sheet1.Range[xlsRow, colAbsPer].Formula = strFormula;

                            //var ap = ((abs +  abss)/ (onrolee+onrole));
                            //SetCellText(sheet1, xlsRow, colAbsPer, Convert.ToDouble(ap * 100));
                        }
                        else
                        {
                            onrole += Convert.ToDouble(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                            abs += Convert.ToDouble(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString());
                        }


                    }
                    catch (Exception ex)
                    {
                    }
                    tempId = dtAttdnSummary.Rows[i]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i]["Department"].ToString() + dtAttdnSummary.Rows[i]["Section"].ToString() + dtAttdnSummary.Rows[i]["SubSection"].ToString() + dtAttdnSummary.Rows[i]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i]["Line"].ToString();
                    try
                    {
                        var tempId2 = dtAttdnSummary.Rows[i + 1]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i + 1]["Department"].ToString() + dtAttdnSummary.Rows[i + 1]["Section"].ToString() + dtAttdnSummary.Rows[i + 1]["SubSection"].ToString() + dtAttdnSummary.Rows[i + 1]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i + 1]["Line"].ToString();
                        if (tempId != tempId2)
                            xlsRow++;
                    }
                    catch (Exception)
                    {
                        xlsRow++;
                    }


          
                }//for emp count

                #region Last subtotal
                al.Add(xlsRow);
                string strLastSubTotalFormula = "=(" + ru.GetColumnNameForXls(colAbsent) + (xlsRow) + "/(" + ru.GetColumnNameForXls(ColReMale) + xlsRow + "+" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow) + "))*100";

                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();

                sheet1.Range[xlsRow, ColReMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColReMale) + catFRow + ":" + ru.GetColumnNameForXls(ColReMale) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, ColReFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColReFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow - 1) + ")";

                sheet1.Range[xlsRow, colPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresent) + catFRow + ":" + ru.GetColumnNameForXls(colPresent) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colAbsent].Formula = "=SUM(" + ru.GetColumnNameForXls(colAbsent) + catFRow + ":" + ru.GetColumnNameForXls(colAbsent) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colLate].Formula = "=SUM(" + ru.GetColumnNameForXls(colLate) + catFRow + ":" + ru.GetColumnNameForXls(colLate) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colLeave) + catFRow + ":" + ru.GetColumnNameForXls(colLeave) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colMaternityLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colMaternityLeave) + catFRow + ":" + ru.GetColumnNameForXls(colMaternityLeave) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colWeekOffHoliday].Formula = "=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colAbsPer].Formula = strLastSubTotalFormula;  //"=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";

                sheet1.Range[xlsRow, ColReMale, xlsRow, colAbsPer].CellStyle.Font.Bold = true;

                xlsRow++;
                #endregion

                #region Grand Total
                SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();
                string strLastGrandTotalFormula = "=(" + ru.GetColumnNameForXls(colAbsent) + (xlsRow) + "/(" + ru.GetColumnNameForXls(ColReMale) + xlsRow + "+" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow) + "))*100";

                sheet1.Range[xlsRow, ColReMale].Formula = GetFormulaGrandTotal(al, ColReMale);
                sheet1.Range[xlsRow, ColReFemale].Formula = GetFormulaGrandTotal(al, ColReFemale);


                sheet1.Range[xlsRow, colPresent].Formula = GetFormulaGrandTotal(al, colPresent);
                sheet1.Range[xlsRow, colAbsent].Formula = GetFormulaGrandTotal(al, colAbsent);
                sheet1.Range[xlsRow, colLate].Formula = GetFormulaGrandTotal(al, colLate);
                sheet1.Range[xlsRow, colLeave].Formula = GetFormulaGrandTotal(al, colLeave);
                sheet1.Range[xlsRow, colMaternityLeave].Formula = GetFormulaGrandTotal(al, colMaternityLeave);
                sheet1.Range[xlsRow, colWeekOffHoliday].Formula = GetFormulaGrandTotal(al, colWeekOffHoliday);
                sheet1.Range[xlsRow, colAbsPer].Formula = strLastGrandTotalFormula;  //"=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";

                sheet1.Range[xlsRow, ColReMale, xlsRow, colAbsPer].CellStyle.Font.Bold = true;

                #endregion

                #endregion ----------------------Data-----------------------

                var endXlsRow = xlsRow;
                sheet1.Range[StartRow, colAbsPer, xlsRow, colAbsPer].NumberFormat = ru.NumberFormatDecimalTwo();
                sheet1.Range[StartRow, colAbsPer, xlsRow, colAbsPer].NumberFormat = ru.NumberFormatDecimalTwo();
                sheet1.IsDisplayZeros = false;

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var xx = RowHeaderLimit + 2;
                sheet1.UsedRange["A" + xx].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "AttendanceSummary";
                #endregion

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "AttendanceSummary.xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetdailyattendanceView(string WorkDate)//XlsDailyAttendanceSummaryRpt()
        {

            #region Variable
            clsReport objRpt = null;
            DataSet dsAttdnSummary = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;
            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            #endregion Variable
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ru = new ReportUtility();
                objRpt = new clsReport();
                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();
                para.PlantId = identity.PlantId;
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                #endregion Variable

                #region DataSet
                GetAttendanceSummarySql(WorkDate, out dsAttdnSummary);
                DataTable dtAttdnSummary = dsAttdnSummary.Tables[0];
                if (dtAttdnSummary.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
                DataView dvAttendance = new DataView(dsAttdnSummary.Tables[0]);
                #endregion DataSet
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;
                xlsRow = 5;
                xlsCol = 1;
                var colEmpCatg = 0;
                var colDepartment = 0;
                var colSec = 0;
                var colSubSec = 0;
                var ColDesigGrp = 0;
                var colOnRole = 0;
                var colPresent = 0;
                var colAbsent = 0;
                var colLate = 0;
                var colLeave = 0;
                var colMaternityLeave = 0;
                var colWeekOffHoliday = 0;
                var colAbsPer = 0;
                var ColLine = 0;
                var ColReMale = 0;
                var ColReFemale = 0;

                #region------------------Column Header------------------
                SetHeadText("Category", sheet1, xlsRow, ref xlsCol, out colEmpCatg, 9);
                SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out colDepartment, 37);
                SetHeadText("Section", sheet1, xlsRow, ref xlsCol, out colSec, 13);
                SetHeadText("SubSection", sheet1, xlsRow, ref xlsCol, out colSubSec, 15);
                SetHeadText("Designation", sheet1, xlsRow, ref xlsCol, out ColDesigGrp, 25);
                SetHeadText("Line", sheet1, xlsRow, ref xlsCol, out ColLine, 11.71);


                SetHeadText("On Role", sheet1, xlsRow, ref xlsCol, out colOnRole, 9.14);
                xlsCol = xlsCol - 1;
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].BorderAround(ExcelLineStyle.Thin);

                SetHeadText("Male", sheet1, xlsRow + 1, ref xlsCol, out ColReMale, 11.71);
                SetHeadText("Female", sheet1, xlsRow + 1, ref xlsCol, out ColReFemale, 11.71);

                SetHeadText("Present", sheet1, xlsRow, ref xlsCol, out colPresent, 7.29);
                SetHeadText("Absent", sheet1, xlsRow, ref xlsCol, out colAbsent, 7);
                SetHeadText("Late", sheet1, xlsRow, ref xlsCol, out colLate, 7);
                SetHeadText("Leave", sheet1, xlsRow, ref xlsCol, out colLeave, 7);
                SetHeadText("Maternity Leave", sheet1, xlsRow, ref xlsCol, out colMaternityLeave, 10);
                SetHeadText("W.Off", sheet1, xlsRow, ref xlsCol, out colWeekOffHoliday, 15);
                SetHeadText("Abs%", sheet1, xlsRow, ref xlsCol, out colAbsPer, 15);

                int RowHeaderLimit = xlsRow;
                #endregion------------------Column Header------------------

                endXlsCol = (xlsCol - 1);
                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                //Param param = new Param();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;

                string FactoryAddress = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Daily Attendance Summary";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                var strRptDateRange = "";
                strRptDateRange = WorkDate;
                sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------               
                var oRU = new ReportUtility();
                xlsRow = RowIndex;
                xlsRow--;
                var startXlsRow = xlsRow;
                string _empcat = string.Empty;
                string _department = string.Empty;
                string _section = string.Empty;
                string _DesignationGroup = string.Empty;
                string _SubSection = string.Empty;
                string _Line = string.Empty;
                string _Gender = string.Empty;
                var catFRow = xlsRow;
                ArrayList al = new ArrayList();
                var lastEmpCat = string.Empty;
                int StartRow = xlsRow;
                string tempId = "";
                string temp2 = "";
                double onrole = 0;
                double abs = 0;
                for (int i = 0; i <= dtAttdnSummary.Rows.Count - 1; i++)
                {
                    try
                    {
                        var catLRow = xlsRow;
                        if (_empcat != dtAttdnSummary.Rows[i]["EmpCategory"].ToString() && string.IsNullOrEmpty(dtAttdnSummary.Rows[i]["EmpCategory"].ToString()) == false)
                        {

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastEmpCat = _empcat;
                                string strSubTotalFormula = "=(" + ru.GetColumnNameForXls(colAbsent) + (xlsRow) + "/(" + ru.GetColumnNameForXls(ColReMale) + xlsRow + "+" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow) + "))*100";

                                al.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();

                                sheet1.Range[xlsRow, ColReMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColReMale) + catFRow + ":" + ru.GetColumnNameForXls(ColReMale) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, ColReFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColReFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, colPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresent) + catFRow + ":" + ru.GetColumnNameForXls(colPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colAbsent].Formula = "=SUM(" + ru.GetColumnNameForXls(colAbsent) + catFRow + ":" + ru.GetColumnNameForXls(colAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colLate].Formula = "=SUM(" + ru.GetColumnNameForXls(colLate) + catFRow + ":" + ru.GetColumnNameForXls(colLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colLeave) + catFRow + ":" + ru.GetColumnNameForXls(colLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colMaternityLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colMaternityLeave) + catFRow + ":" + ru.GetColumnNameForXls(colMaternityLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colWeekOffHoliday].Formula = "=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colAbsPer].Formula = strSubTotalFormula;  //"=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;

                                xlsRow++;
                            }
                            #endregion

                            _empcat = dtAttdnSummary.Rows[i]["EmpCategory"].ToString();
                            SetCellText(sheet1, xlsRow, colEmpCatg, _empcat);

                            _department = dtAttdnSummary.Rows[i]["Department"].ToString();
                            SetCellText(sheet1, xlsRow, colDepartment, _department);

                            _section = dtAttdnSummary.Rows[i]["Section"].ToString();
                            SetCellText(sheet1, xlsRow, colSec, _section);


                            _SubSection = dtAttdnSummary.Rows[i]["SubSection"].ToString();
                            SetCellText(sheet1, xlsRow, colSubSec, _SubSection);

                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString();
                            SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);

                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString();
                            SetCellText(sheet1, xlsRow, ColLine, _Line);


                            if (catFRow < xlsRow)
                            {

                                catFRow = xlsRow;
                            }
                            temp2 = dtAttdnSummary.Rows[i]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i]["Department"].ToString() + dtAttdnSummary.Rows[i]["Section"].ToString() + dtAttdnSummary.Rows[i]["SubSection"].ToString() + dtAttdnSummary.Rows[i]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i]["Line"].ToString();
                            if (temp2.Contains("StaffProductionSewingGeneralOfficer"))
                            {

                            }

                        }

                        //temp2 = dtAttdnSummary.Rows[i]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i]["Department"].ToString() + dtAttdnSummary.Rows[i]["Section"].ToString() + dtAttdnSummary.Rows[i]["SubSection"].ToString() + dtAttdnSummary.Rows[i]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i]["Line"].ToString();
                        //if (temp2.Contains("StaffProductionSewingGeneralOfficer"))
                        //{

                        //}
                        if (_department != dtAttdnSummary.Rows[i]["Department"].ToString())
                        {

                            _department = dtAttdnSummary.Rows[i]["Department"].ToString(); SetCellText(sheet1, xlsRow, colDepartment, _department);
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _SubSection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _SubSection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);

                        }
                        if (_section != dtAttdnSummary.Rows[i]["Section"].ToString())
                        {
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _SubSection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _SubSection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);
                        }
                        else if (_SubSection != dtAttdnSummary.Rows[i]["SubSection"].ToString())
                        {
                            _SubSection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _SubSection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);
                        }

                        if (_DesignationGroup != dtAttdnSummary.Rows[i]["LealDesignation"].ToString())
                        {
                            _DesignationGroup = dtAttdnSummary.Rows[i]["LealDesignation"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);
                        }

                        if (_Line != dtAttdnSummary.Rows[i]["Line"].ToString())
                        {
                            _Line = dtAttdnSummary.Rows[i]["Line"].ToString(); SetCellText(sheet1, xlsRow, ColLine, _Line);
                        }



                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        if (dtAttdnSummary.Rows[i]["GenderID"].ToString().ToUpper() == "MALE")
                        {
                            sheet1[xlsRow, ColReMale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                        }
                        else
                        {
                            sheet1[xlsRow, ColReFemale].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                        }


                        if (double.IsNaN(sheet1[xlsRow, colPresent].Number) == false)
                            sheet1[xlsRow, colPresent].Number += clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalPresentEmployee"].ToString());
                        else
                            sheet1[xlsRow, colPresent].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalPresentEmployee"].ToString());

                        if (double.IsNaN(sheet1[xlsRow, colAbsent].Number) == false)
                            sheet1[xlsRow, colAbsent].Number += clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString());
                        else
                            sheet1[xlsRow, colAbsent].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString());

                        if (double.IsNaN(sheet1[xlsRow, colLate].Number) == false)
                            sheet1[xlsRow, colLate].Number += clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalLateEmployee"].ToString());
                        else
                            sheet1[xlsRow, colLate].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalLateEmployee"].ToString());

                        if (double.IsNaN(sheet1[xlsRow, colLeave].Number) == false)
                            sheet1[xlsRow, colLeave].Number += clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalLeaveEmployee"].ToString());
                        else
                            sheet1[xlsRow, colLeave].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalLeaveEmployee"].ToString());

                        if (double.IsNaN(sheet1[xlsRow, colMaternityLeave].Number) == false)
                            sheet1[xlsRow, colMaternityLeave].Number += clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalMaternithyEmployee"].ToString());
                        else
                            sheet1[xlsRow, colMaternityLeave].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalMaternithyEmployee"].ToString());

                        if (double.IsNaN(sheet1[xlsRow, colWeekOffHoliday].Number) == false)
                            sheet1[xlsRow, colWeekOffHoliday].Number += clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalWeekoffEmployee"].ToString());
                        else
                            sheet1[xlsRow, colWeekOffHoliday].Number = clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalWeekoffEmployee"].ToString());

                        SetCellText(sheet1, xlsRow, ColLine, Convert.ToString(dtAttdnSummary.Rows[i]["Line"].ToString()));

                        string t1 = dtAttdnSummary.Rows[i]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i]["Department"].ToString() + dtAttdnSummary.Rows[i]["Section"].ToString() + dtAttdnSummary.Rows[i]["SubSection"].ToString() + dtAttdnSummary.Rows[i]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i]["Line"].ToString();
                        var t2 = dtAttdnSummary.Rows[i + 1]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i + 1]["Department"].ToString() + dtAttdnSummary.Rows[i + 1]["Section"].ToString() + dtAttdnSummary.Rows[i + 1]["SubSection"].ToString() + dtAttdnSummary.Rows[i + 1]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i + 1]["Line"].ToString();
                        if (t1 != t2)
                        {
                            double abss = Convert.ToDouble(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString());
                            double onrolee = Convert.ToDouble(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                            string strFormula = "=(" + ru.GetColumnNameForXls(colAbsent) + (xlsRow) + "/(" + ru.GetColumnNameForXls(ColReMale) + xlsRow + "+" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow) + "))*100";
                            sheet1.Range[xlsRow, colAbsPer].Formula = strFormula;

                            //var ap = ((abs +  abss)/ (onrolee+onrole));
                            //SetCellText(sheet1, xlsRow, colAbsPer, Convert.ToDouble(ap * 100));
                        }
                        else
                        {
                            onrole += Convert.ToDouble(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                            abs += Convert.ToDouble(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString());
                        }


                    }
                    catch (Exception ex)
                    {
                    }
                    tempId = dtAttdnSummary.Rows[i]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i]["Department"].ToString() + dtAttdnSummary.Rows[i]["Section"].ToString() + dtAttdnSummary.Rows[i]["SubSection"].ToString() + dtAttdnSummary.Rows[i]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i]["Line"].ToString();
                    try
                    {
                        var tempId2 = dtAttdnSummary.Rows[i + 1]["EmpCategory"].ToString() + dtAttdnSummary.Rows[i + 1]["Department"].ToString() + dtAttdnSummary.Rows[i + 1]["Section"].ToString() + dtAttdnSummary.Rows[i + 1]["SubSection"].ToString() + dtAttdnSummary.Rows[i + 1]["LealDesignation"].ToString() + dtAttdnSummary.Rows[i + 1]["Line"].ToString();
                        if (tempId != tempId2)
                            xlsRow++;
                    }
                    catch (Exception)
                    {
                        xlsRow++;
                    }



                }//for emp count

                #region Last subtotal
                al.Add(xlsRow);
                string strLastSubTotalFormula = "=(" + ru.GetColumnNameForXls(colAbsent) + (xlsRow) + "/(" + ru.GetColumnNameForXls(ColReMale) + xlsRow + "+" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow) + "))*100";

                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();

                sheet1.Range[xlsRow, ColReMale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColReMale) + catFRow + ":" + ru.GetColumnNameForXls(ColReMale) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, ColReFemale].Formula = "=SUM(" + ru.GetColumnNameForXls(ColReFemale) + catFRow + ":" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow - 1) + ")";

                sheet1.Range[xlsRow, colPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresent) + catFRow + ":" + ru.GetColumnNameForXls(colPresent) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colAbsent].Formula = "=SUM(" + ru.GetColumnNameForXls(colAbsent) + catFRow + ":" + ru.GetColumnNameForXls(colAbsent) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colLate].Formula = "=SUM(" + ru.GetColumnNameForXls(colLate) + catFRow + ":" + ru.GetColumnNameForXls(colLate) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colLeave) + catFRow + ":" + ru.GetColumnNameForXls(colLeave) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colMaternityLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colMaternityLeave) + catFRow + ":" + ru.GetColumnNameForXls(colMaternityLeave) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colWeekOffHoliday].Formula = "=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, colAbsPer].Formula = strLastSubTotalFormula;  //"=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";

                sheet1.Range[xlsRow, ColReMale, xlsRow, colAbsPer].CellStyle.Font.Bold = true;

                xlsRow++;
                #endregion

                #region Grand Total
                SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();
                string strLastGrandTotalFormula = "=(" + ru.GetColumnNameForXls(colAbsent) + (xlsRow) + "/(" + ru.GetColumnNameForXls(ColReMale) + xlsRow + "+" + ru.GetColumnNameForXls(ColReFemale) + (xlsRow) + "))*100";

                sheet1.Range[xlsRow, ColReMale].Formula = GetFormulaGrandTotal(al, ColReMale);
                sheet1.Range[xlsRow, ColReFemale].Formula = GetFormulaGrandTotal(al, ColReFemale);


                sheet1.Range[xlsRow, colPresent].Formula = GetFormulaGrandTotal(al, colPresent);
                sheet1.Range[xlsRow, colAbsent].Formula = GetFormulaGrandTotal(al, colAbsent);
                sheet1.Range[xlsRow, colLate].Formula = GetFormulaGrandTotal(al, colLate);
                sheet1.Range[xlsRow, colLeave].Formula = GetFormulaGrandTotal(al, colLeave);
                sheet1.Range[xlsRow, colMaternityLeave].Formula = GetFormulaGrandTotal(al, colMaternityLeave);
                sheet1.Range[xlsRow, colWeekOffHoliday].Formula = GetFormulaGrandTotal(al, colWeekOffHoliday);
                sheet1.Range[xlsRow, colAbsPer].Formula = strLastGrandTotalFormula;  //"=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";

                sheet1.Range[xlsRow, ColReMale, xlsRow, colAbsPer].CellStyle.Font.Bold = true;

                #endregion

                #endregion ----------------------Data-----------------------

                var endXlsRow = xlsRow;
                sheet1.Range[StartRow, colAbsPer, xlsRow, colAbsPer].NumberFormat = ru.NumberFormatDecimalTwo();
                sheet1.Range[StartRow, colAbsPer, xlsRow, colAbsPer].NumberFormat = ru.NumberFormatDecimalTwo();
                sheet1.IsDisplayZeros = false;

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var xx = RowHeaderLimit + 2;
                sheet1.UsedRange["A" + xx].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "AttendanceSummary";
                #endregion

                workbook.Version = ExcelVersion.Excel97to2003;
                //var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "AttendanceSummary.xls";
                //string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                //workbook.SaveAs(fullPath);

                return RenderReportAsPdf(workbook, "AttendanceSummary");
                ///return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost,Authorize]
        public ActionResult GetdailyattendanceNoLine(string WorkDate)//XlsDailyAttendanceSummaryRpt()
        {
            #region Variable
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            clsReport objRpt = null;

            DataSet dsAttdnSummary = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;

            #endregion Variable

            try
            {
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();
                para.PlantId = identity.PlantId;
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                #endregion Variable

                #region DataSet
                GetAttendanceSummaryNoLineSql(WorkDate, out dsAttdnSummary);
                DataTable dtAttdnSummary = dsAttdnSummary.Tables[0];
                if (dtAttdnSummary.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
                DataView dvAttendance = new DataView(dsAttdnSummary.Tables[0]);
                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                xlsRow = 5;
                xlsCol = 1;

                var colEmpCatg = 0;
                var colDepartment = 0;
                var colSec = 0;
                var colSubSec = 0;
                var ColDesigGrp = 0;

                var colOnRole = 0;
                var colPresent = 0;
                var colAbsent = 0;
                var colLate = 0;
                var colTotalPresent = 0;
                var colLeave = 0;
                var colMaternityLeave = 0;
                var colWeekOffHoliday = 0;
                var colAbsPer = 0;
                
                #region------------------Column Header------------------
                SetHeadText("Category", sheet1, xlsRow, ref xlsCol, out colEmpCatg, 9);
                SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out colDepartment, 37);
                SetHeadText("Section", sheet1, xlsRow, ref xlsCol, out colSec, 13);
                SetHeadText("SubSection", sheet1, xlsRow, ref xlsCol, out colSubSec, 13);
                SetHeadText("Designation", sheet1, xlsRow, ref xlsCol, out ColDesigGrp, 11.71);
                SetHeadText("On Roll", sheet1, xlsRow, ref xlsCol, out colOnRole, 9.14);
                SetHeadText("Present", sheet1, xlsRow, ref xlsCol, out colPresent, 7.29);
                SetHeadText("Late", sheet1, xlsRow, ref xlsCol, out colLate, 7);
                SetHeadText("Total Present", sheet1, xlsRow, ref xlsCol, out colTotalPresent, 10);
                SetHeadText("Absent", sheet1, xlsRow, ref xlsCol, out colAbsent, 7);
                SetHeadText("Leave", sheet1, xlsRow, ref xlsCol, out colLeave, 7);
                SetHeadText("Maternity Leave", sheet1, xlsRow, ref xlsCol, out colMaternityLeave, 10);
                SetHeadText("W.Off", sheet1, xlsRow, ref xlsCol, out colWeekOffHoliday, 15);
                SetHeadText("Abs%", sheet1, xlsRow, ref xlsCol, out colAbsPer, 15);
                int RowHeaderLimit = xlsRow;
                #endregion------------------Column Header------------------

                endXlsCol = (xlsCol - 1);
                int RowIndex = xlsRow + 3;


                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                //Param param = new Param();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;

                string FactoryAddress = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Daily Attendance Summary";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                var strRptDateRange = "";
                strRptDateRange = WorkDate;
                sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                //  var SrNo = 0;
                var x = "";
                int StartRow = xlsRow;

                var oRU = new ReportUtility();

                xlsRow = RowIndex;



                xlsRow--;
                xlsRow--;
                var startXlsRow = xlsRow;
                if (dtAttdnSummary.Rows.Count > 0)
                {
                    string _empcat = string.Empty;
                    string _department = string.Empty;
                    string _section = string.Empty;
                    string _Subsection = string.Empty;
                    string _DesignationGroup = string.Empty;

                    var isFirst = true;
                    var catFRow = xlsRow;
                    ArrayList al = new ArrayList();
                    var lastEmpCat = string.Empty;
                    for (int i = 0; i <= dtAttdnSummary.Rows.Count - 1; i++)
                    {
                        var catLRow = xlsRow;
                        if (_empcat != dtAttdnSummary.Rows[i]["EmpCategory"].ToString() && string.IsNullOrEmpty(dtAttdnSummary.Rows[i]["EmpCategory"].ToString()) == false)
                        {
                            _empcat = dtAttdnSummary.Rows[i]["EmpCategory"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastEmpCat = _empcat;
                                al.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();
                                sheet1.Range[xlsRow, colOnRole].Formula = "=SUM(" + ru.GetColumnNameForXls(colOnRole) + catFRow + ":" + ru.GetColumnNameForXls(colOnRole) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresent) + catFRow + ":" + ru.GetColumnNameForXls(colPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colAbsent].Formula = "=SUM(" + ru.GetColumnNameForXls(colAbsent) + catFRow + ":" + ru.GetColumnNameForXls(colAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colLate].Formula = "=SUM(" + ru.GetColumnNameForXls(colLate) + catFRow + ":" + ru.GetColumnNameForXls(colLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colTotalPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotalPresent) + catFRow + ":" + ru.GetColumnNameForXls(colTotalPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colLeave) + catFRow + ":" + ru.GetColumnNameForXls(colLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colMaternityLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colMaternityLeave) + catFRow + ":" + ru.GetColumnNameForXls(colMaternityLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colWeekOffHoliday].Formula = "=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;
                                xlsRow++;
                            }
                            #endregion
                            SetCellText(sheet1, xlsRow, colEmpCatg, _empcat);
                            _department = dtAttdnSummary.Rows[i]["Department"].ToString();
                            SetCellText(sheet1, xlsRow, colDepartment, _department);
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString();
                            SetCellText(sheet1, xlsRow, colSec, _section);
                            _Subsection = dtAttdnSummary.Rows[i]["SubSection"].ToString();
                            SetCellText(sheet1, xlsRow, colSubSec, _Subsection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString();
                            SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);

                            if (catFRow < xlsRow)
                            {

                                catFRow = xlsRow;
                            }
                        }
                        else if (_department != dtAttdnSummary.Rows[i]["Department"].ToString())
                        {
                            _department = dtAttdnSummary.Rows[i]["Department"].ToString(); SetCellText(sheet1, xlsRow, colDepartment, _department);
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _Subsection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _Subsection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }
                        else if (_section != dtAttdnSummary.Rows[i]["Section"].ToString())
                        {
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _Subsection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _Subsection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }
                        else if (_section != dtAttdnSummary.Rows[i]["SubSection"].ToString())
                        {
                            
                            _Subsection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _Subsection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }
                        else if (_DesignationGroup != dtAttdnSummary.Rows[i]["DesignationGroup"].ToString())
                        {
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }


                        SetCellText(sheet1, xlsRow, colOnRole, Convert.ToDouble(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colPresent, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalPresentEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colAbsent, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colLate, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalLateEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colTotalPresent, Convert.ToDouble(clsStaticInfo.dbl( dtAttdnSummary.Rows[i]["totalPresentEmployee"].ToString()) + clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalLateEmployee"].ToString())));
                        SetCellText(sheet1, xlsRow, colLeave, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalLeaveEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colMaternityLeave, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalMaternityLeaveEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colWeekOffHoliday, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalWeekoffEmployee"].ToString()));

                        var ap = Convert.ToDouble(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString()) / Convert.ToDouble(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                        SetCellText(sheet1, xlsRow, colAbsPer, Convert.ToDouble(ap * 100));
                        xlsRow++;
                    }//for emp count

                    #region Last subtotal
                    al.Add(xlsRow);
                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                    sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();
                    sheet1.Range[xlsRow, colOnRole].Formula = "=SUM(" + ru.GetColumnNameForXls(colOnRole) + catFRow + ":" + ru.GetColumnNameForXls(colOnRole) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresent) + catFRow + ":" + ru.GetColumnNameForXls(colPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colAbsent].Formula = "=SUM(" + ru.GetColumnNameForXls(colAbsent) + catFRow + ":" + ru.GetColumnNameForXls(colAbsent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colLate].Formula = "=SUM(" + ru.GetColumnNameForXls(colLate) + catFRow + ":" + ru.GetColumnNameForXls(colLate) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colTotalPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotalPresent) + catFRow + ":" + ru.GetColumnNameForXls(colTotalPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colLeave) + catFRow + ":" + ru.GetColumnNameForXls(colLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colMaternityLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colMaternityLeave) + catFRow + ":" + ru.GetColumnNameForXls(colMaternityLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colWeekOffHoliday].Formula = "=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;
                    xlsRow++;
                    #endregion

                    #region Grand Total
                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();


                    sheet1.Range[xlsRow, colOnRole].Formula = GetFormulaGrandTotal(al, colOnRole);
                    sheet1.Range[xlsRow, colPresent].Formula = GetFormulaGrandTotal(al, colPresent);
                    sheet1.Range[xlsRow, colAbsent].Formula = GetFormulaGrandTotal(al, colAbsent);
                    sheet1.Range[xlsRow, colLate].Formula = GetFormulaGrandTotal(al, colLate);
                    sheet1.Range[xlsRow, colTotalPresent].Formula = GetFormulaGrandTotal(al, colTotalPresent);
                    sheet1.Range[xlsRow, colLeave].Formula = GetFormulaGrandTotal(al, colLeave);
                    sheet1.Range[xlsRow, colMaternityLeave].Formula = GetFormulaGrandTotal(al, colMaternityLeave);
                    sheet1.Range[xlsRow, colWeekOffHoliday].Formula = GetFormulaGrandTotal(al, colWeekOffHoliday);
                    sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;
                    
                    #endregion

                }

                #endregion ----------------------Data-----------------------

                sheet1.Range[StartRow, colAbsPer, xlsRow, colAbsPer].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet1.Range[StartRow, colAbsPer, xlsRow, colAbsPer].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet1.IsDisplayZeros = false;
                var endXlsRow = xlsRow;
                
                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var xx = RowHeaderLimit + 1;
                sheet1.UsedRange["A" + xx].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "AttendanceSummary";
                #endregion

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "AttendanceSummary.xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                excelEngine = null;
                application = null;
                workbook = null;
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetdailyattendanceNoLineView(string WorkDate)//XlsDailyAttendanceSummaryRpt()
        {
            #region Variable
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            clsReport objRpt = null;

            DataSet dsAttdnSummary = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;

            #endregion Variable

            try
            {
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();
                para.PlantId = identity.PlantId;
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                #endregion Variable

                #region DataSet
                GetAttendanceSummaryNoLineSql(WorkDate, out dsAttdnSummary);
                DataTable dtAttdnSummary = dsAttdnSummary.Tables[0];
                if (dtAttdnSummary.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
                DataView dvAttendance = new DataView(dsAttdnSummary.Tables[0]);
                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                xlsRow = 5;
                xlsCol = 1;

                var colEmpCatg = 0;
                var colDepartment = 0;
                var colSec = 0;
                var colSubSec = 0;
                var ColDesigGrp = 0;

                var colOnRole = 0;
                var colPresent = 0;
                var colAbsent = 0;
                var colLate = 0;
                var colTotalPresent = 0;
                var colLeave = 0;
                var colMaternityLeave = 0;
                var colWeekOffHoliday = 0;
                var colAbsPer = 0;

                #region------------------Column Header------------------
                SetHeadText("Category", sheet1, xlsRow, ref xlsCol, out colEmpCatg, 9);
                SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out colDepartment, 37);
                SetHeadText("Section", sheet1, xlsRow, ref xlsCol, out colSec, 13);
                SetHeadText("SubSection", sheet1, xlsRow, ref xlsCol, out colSubSec, 13);
                SetHeadText("Designation", sheet1, xlsRow, ref xlsCol, out ColDesigGrp, 11.71);
                SetHeadText("On Roll", sheet1, xlsRow, ref xlsCol, out colOnRole, 9.14);
                SetHeadText("Present", sheet1, xlsRow, ref xlsCol, out colPresent, 7.29);
                SetHeadText("Late", sheet1, xlsRow, ref xlsCol, out colLate, 7);
                SetHeadText("Total Present", sheet1, xlsRow, ref xlsCol, out colTotalPresent, 10);
                SetHeadText("Absent", sheet1, xlsRow, ref xlsCol, out colAbsent, 7);
                SetHeadText("Leave", sheet1, xlsRow, ref xlsCol, out colLeave, 7);
                SetHeadText("Maternity Leave", sheet1, xlsRow, ref xlsCol, out colMaternityLeave, 10);
                SetHeadText("W.Off", sheet1, xlsRow, ref xlsCol, out colWeekOffHoliday, 15);
                SetHeadText("Abs%", sheet1, xlsRow, ref xlsCol, out colAbsPer, 15);
                int RowHeaderLimit = xlsRow;
                #endregion------------------Column Header------------------

                endXlsCol = (xlsCol - 1);
                int RowIndex = xlsRow + 3;


                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                //Param param = new Param();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;

                string FactoryAddress = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Daily Attendance Summary";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                var strRptDateRange = "";
                strRptDateRange = WorkDate;
                sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                //  var SrNo = 0;
                var x = "";
                int StartRow = xlsRow;

                var oRU = new ReportUtility();

                xlsRow = RowIndex;



                xlsRow--;
                xlsRow--;
                var startXlsRow = xlsRow;
                if (dtAttdnSummary.Rows.Count > 0)
                {
                    string _empcat = string.Empty;
                    string _department = string.Empty;
                    string _section = string.Empty;
                    string _Subsection = string.Empty;
                    string _DesignationGroup = string.Empty;

                    var isFirst = true;
                    var catFRow = xlsRow;
                    ArrayList al = new ArrayList();
                    var lastEmpCat = string.Empty;
                    for (int i = 0; i <= dtAttdnSummary.Rows.Count - 1; i++)
                    {
                        var catLRow = xlsRow;
                        if (_empcat != dtAttdnSummary.Rows[i]["EmpCategory"].ToString() && string.IsNullOrEmpty(dtAttdnSummary.Rows[i]["EmpCategory"].ToString()) == false)
                        {
                            _empcat = dtAttdnSummary.Rows[i]["EmpCategory"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastEmpCat = _empcat;
                                al.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();
                                sheet1.Range[xlsRow, colOnRole].Formula = "=SUM(" + ru.GetColumnNameForXls(colOnRole) + catFRow + ":" + ru.GetColumnNameForXls(colOnRole) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresent) + catFRow + ":" + ru.GetColumnNameForXls(colPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colAbsent].Formula = "=SUM(" + ru.GetColumnNameForXls(colAbsent) + catFRow + ":" + ru.GetColumnNameForXls(colAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colLate].Formula = "=SUM(" + ru.GetColumnNameForXls(colLate) + catFRow + ":" + ru.GetColumnNameForXls(colLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colTotalPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotalPresent) + catFRow + ":" + ru.GetColumnNameForXls(colTotalPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colLeave) + catFRow + ":" + ru.GetColumnNameForXls(colLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colMaternityLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colMaternityLeave) + catFRow + ":" + ru.GetColumnNameForXls(colMaternityLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colWeekOffHoliday].Formula = "=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;
                                xlsRow++;
                            }
                            #endregion
                            SetCellText(sheet1, xlsRow, colEmpCatg, _empcat);
                            _department = dtAttdnSummary.Rows[i]["Department"].ToString();
                            SetCellText(sheet1, xlsRow, colDepartment, _department);
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString();
                            SetCellText(sheet1, xlsRow, colSec, _section);
                            _Subsection = dtAttdnSummary.Rows[i]["SubSection"].ToString();
                            SetCellText(sheet1, xlsRow, colSubSec, _Subsection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString();
                            SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);

                            if (catFRow < xlsRow)
                            {

                                catFRow = xlsRow;
                            }
                        }
                        else if (_department != dtAttdnSummary.Rows[i]["Department"].ToString())
                        {
                            _department = dtAttdnSummary.Rows[i]["Department"].ToString(); SetCellText(sheet1, xlsRow, colDepartment, _department);
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _Subsection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _Subsection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }
                        else if (_section != dtAttdnSummary.Rows[i]["Section"].ToString())
                        {
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _Subsection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _Subsection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }
                        else if (_Subsection != dtAttdnSummary.Rows[i]["SubSection"].ToString())
                        {

                            _Subsection = dtAttdnSummary.Rows[i]["SubSection"].ToString(); SetCellText(sheet1, xlsRow, colSubSec, _Subsection);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }
                        else if (_DesignationGroup != dtAttdnSummary.Rows[i]["DesignationGroup"].ToString())
                        {
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }


                        SetCellText(sheet1, xlsRow, colOnRole, Convert.ToDouble(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colPresent, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalPresentEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colAbsent, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colLate, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalLateEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colTotalPresent, Convert.ToDouble(clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalPresentEmployee"].ToString()) + clsStaticInfo.dbl(dtAttdnSummary.Rows[i]["totalLateEmployee"].ToString())));
                        SetCellText(sheet1, xlsRow, colLeave, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalLeaveEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colMaternityLeave, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalMaternityLeaveEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colWeekOffHoliday, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalWeekoffEmployee"].ToString()));

                        var ap = Convert.ToDouble(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString()) / Convert.ToDouble(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                        SetCellText(sheet1, xlsRow, colAbsPer, Convert.ToDouble(ap * 100));
                        xlsRow++;
                    }//for emp count

                    #region Last subtotal
                    al.Add(xlsRow);
                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                    sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();
                    sheet1.Range[xlsRow, colOnRole].Formula = "=SUM(" + ru.GetColumnNameForXls(colOnRole) + catFRow + ":" + ru.GetColumnNameForXls(colOnRole) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresent) + catFRow + ":" + ru.GetColumnNameForXls(colPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colAbsent].Formula = "=SUM(" + ru.GetColumnNameForXls(colAbsent) + catFRow + ":" + ru.GetColumnNameForXls(colAbsent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colLate].Formula = "=SUM(" + ru.GetColumnNameForXls(colLate) + catFRow + ":" + ru.GetColumnNameForXls(colLate) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colTotalPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotalPresent) + catFRow + ":" + ru.GetColumnNameForXls(colTotalPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colLeave) + catFRow + ":" + ru.GetColumnNameForXls(colLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colMaternityLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colMaternityLeave) + catFRow + ":" + ru.GetColumnNameForXls(colMaternityLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colWeekOffHoliday].Formula = "=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;
                    xlsRow++;
                    #endregion

                    #region Grand Total
                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();


                    sheet1.Range[xlsRow, colOnRole].Formula = GetFormulaGrandTotal(al, colOnRole);
                    sheet1.Range[xlsRow, colPresent].Formula = GetFormulaGrandTotal(al, colPresent);
                    sheet1.Range[xlsRow, colAbsent].Formula = GetFormulaGrandTotal(al, colAbsent);
                    sheet1.Range[xlsRow, colLate].Formula = GetFormulaGrandTotal(al, colLate);
                    sheet1.Range[xlsRow, colTotalPresent].Formula = GetFormulaGrandTotal(al, colTotalPresent);
                    sheet1.Range[xlsRow, colLeave].Formula = GetFormulaGrandTotal(al, colLeave);
                    sheet1.Range[xlsRow, colMaternityLeave].Formula = GetFormulaGrandTotal(al, colMaternityLeave);
                    sheet1.Range[xlsRow, colWeekOffHoliday].Formula = GetFormulaGrandTotal(al, colWeekOffHoliday);
                    sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;

                    #endregion

                }

                #endregion ----------------------Data-----------------------

                sheet1.Range[StartRow, colAbsPer, xlsRow, colAbsPer].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet1.Range[StartRow, colAbsPer, xlsRow, colAbsPer].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet1.IsDisplayZeros = false;
                var endXlsRow = xlsRow;

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var xx = RowHeaderLimit + 1;
                sheet1.UsedRange["A" + xx].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "AttendanceSummary";
                #endregion

                //workbook.Version = ExcelVersion.Excel97to2003;
                //var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "AttendanceSummary.xls";
                //string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                //workbook.SaveAs(fullPath);
                //return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);

                workbook.Version = ExcelVersion.Excel97to2003;
                //var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "AttendanceSummary.xls";
                //string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                //workbook.SaveAs(fullPath);

                return RenderReportAsPdf(workbook, "AttendanceSummary");
                ///return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                excelEngine = null;
                application = null;
                workbook = null;
            }
        }
        string GetFormulaGrandTotal(ArrayList al, int col)
        {
            string _formula = string.Empty;
            ReportUtility ru = new ReportUtility();
            try
            {
                for (int i = 0; i < al.Count; i++)
                {
                    if (_formula.Length == 0)
                    {
                        _formula = "=" + ru.GetColumnNameForXls(col) + al[i];
                    }
                    else
                    {
                        _formula += "+" + ru.GetColumnNameForXls(col) + al[i];
                    }
                }
                return _formula;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion--------------------------------------------Xls Report End----------------------------------------------------
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, string Text)
        {
            //if (string.IsNullOrEmpty(Text) == false)
            //{
            sheet.Range[xlsRow, xlsCol].Text = Text;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            //}
        }
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, double Number)
        {
            //if (string.IsNullOrEmpty(Text) == false)
            //{
            sheet.Range[xlsRow, xlsCol].Number = Number;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            //}
        }


        private void SetHeadText(IWorksheet sheet, int xlsRow, int xlsCol, string text)
        {
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
        }
        private void SetHeadText(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.AliceBlue;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ColIndex = xlsCol;
            xlsCol += 1;
        }

        private void SetHeaderValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            //sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void CreateDynamicMonthHead(DataTable dtMonthList, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColStart, out List<FiscalYearMonthSequence> list)
        {
            try
            {
                list = new List<FiscalYearMonthSequence>();
                _total_head_count = 0;

                int countGross = 0;
                string grossFormula = "";
                string deductionFormula = "";
                for (int ci = 0; ci < dtMonthList.Rows.Count; ci++)
                {
                    _total_head_count++;
                    countGross++;
                    sheet1.Range[xlsRow, ColStart + countGross].Text = dtMonthList.Rows[ci]["MonthName"].ToString().Substring(0, 3) + "," + dtMonthList.Rows[ci]["MonthYear"].ToString().Substring(2, 2);
                    sheet1.Range[xlsRow, ColStart + countGross].ColumnWidth = 8;
                    sheet1.Range[xlsRow, ColStart + countGross].CellStyle.Font.Bold = true;
                    //sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                    sheet1.Range[xlsRow, ColStart + countGross].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColStart + countGross].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, ColStart + countGross].BorderAround(ExcelLineStyle.Thin);

                    FiscalYearMonthSequence fiscalYearMonthSequence = new FiscalYearMonthSequence();
                    fiscalYearMonthSequence.MonthName = dtMonthList.Rows[ci]["MonthName"].ToString();
                    fiscalYearMonthSequence.MonthNo = dtMonthList.Rows[ci]["MonthNumber"].ToString();
                    fiscalYearMonthSequence.LastDayOfMonth = dtMonthList.Rows[ci]["LastDayOfMonth"].ToString();
                    fiscalYearMonthSequence.MonthYear = dtMonthList.Rows[ci]["MonthYear"].ToString();
                    fiscalYearMonthSequence.XLColIndex = ColStart + countGross;

                    list.Add(fiscalYearMonthSequence);
                    xlsCol += 1;
                }//for         
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
