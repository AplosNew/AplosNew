using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.DocIO.DLS;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ConsecutiveAttendaceAndOTController : BaseController
    {
        #region Constructor

        private readonly Library.HumanResource.Dashboard.HRDashboardService _HRDashboardService;

        private SqlRepository _sqlRepository = new SqlRepository();
        public ConsecutiveAttendaceAndOTController(

            )
        {
            _HRDashboardService = new Library.HumanResource.Dashboard.HRDashboardService();
            _sqlRepository = new SqlRepository();
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult OTHours()
        {
            return View();
        }

        string dayStatusDef = "'Present','Late','Holiday','Weekend'";
        [HttpPost, Authorize]
        public ActionResult ConsecutivePresentStatusDynamic(string CompanyId, string hrFromDate, string hrToDate, string dayCount, string presentComparator, string dayStatus, bool considerInOut, string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (PlantId == "" || PlantId == null)
            {
                PlantId = identity.PlantId;
            }
            dayStatus = dayStatusDef;
            considerInOut = true;

            var jsondata = Json(ModalConsecutivePresentDateList(identity.CompanyGroupId, identity.CompanyId, PlantId, hrFromDate, hrToDate, dayCount, presentComparator, dayStatus, considerInOut), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;

            return jsondata;

        }

        [HttpPost, Authorize]
        public ActionResult PrintPresent(string hrFromDate, string hrToDate, string dayCount, string presentComparator, string dayStatus, bool considerInOut, string PlantId)
        {
            try
            {
                considerInOut = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                dayStatus = dayStatusDef;
                if (PlantId == "" || PlantId == null)
                {
                    PlantId = identity.PlantId;
                }
                var fileName = "attdStatus" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                var workbook = GetEmployeePresentStatusReport(identity.CompanyGroupId, identity.CompanyId, PlantId, hrFromDate, hrToDate, dayCount, presentComparator, identity.UserId, dayStatus, considerInOut);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }
        #endregion -- Pages



        public string ModalConsecutivePresentDateListSql(string companyGroupId, string companyId, string plantId, string hrfromDate, string hrtoDate, string dayCount, string presentComparator, string dayStatus, bool considerInOut)
        {
            string wcConsiderInOut = "";
            if (considerInOut == true)
            {
                wcConsiderInOut = "AND (ISNULL(InTime,'')<>'' and ISNULL(OutTime,'')<>'')";
            }
            string presnetsql = @"select Count(max_Present_days) PresentDaysOccured,EmployeeCodePreFix,EmployeeCodeNumeric,DOSS, EmpSystemID, EmployeeCode,DOJS,Division,Plant,Entity,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys,EmployeeName,CompanyName from (
                                    SELECT EmpSystemID,EmployeeCode,EmployeeCodePreFix,EmployeeCodeNumeric, DOJS,Division,Plant,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys,EmployeeName
                                  , DOSS,CompanyName , count(*) max_Present_days, min(WorkDate) workDate, max(WorkDate) mxworkDate,Entity
                                    FROM (
                                    	SELECT *, sum(xx) OVER (
                                    			PARTITION BY EmpSystemID ORDER BY WorkDate
                                    			) ss
                                    	FROM (
                                    		SELECT ad.WorkDate,ad.InTime,ad.OutTime, ad.EmpSystemID, DT.Category,EI.EmployeeCode, ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric,EI.EmployeeName
                                    		,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJS
                                    		,REPLACE(CONVERT(VARCHAR(11), EI.DOS, 106), ' ', '-') DOSS,C.UserName CompanyName
                                    									,Division.UserName Division ,Plant.UserName Plant ,Unit.UserName Unit ,Department.UserName Department ,Section.UserName Section ,SubSection.UserName SubSection ,ShiftDefination.UserName ShiftDefination ,Line.UserName Line 
                                    									,Ldes.UserName LegalDesignation,ec.UserName EmployeeCategorys,E.UserName Entity
                                    		,CASE 
                                    				WHEN Category = lag(Category) OVER (
                                    						PARTITION BY EmpSystemID ORDER BY WorkDate
                                    						)
                                    					THEN 0
                                    				ELSE 1
                                    				END AS xx
                                    		FROM (	SELECT WorkDate,InTime,OutTime,EmpSystemID
											,DayStatus = Case when ISNULL(InTime,'')<>''  and ISNULL(OutTime,'')<>''  and DayStatus = 'W' then 'WP'
											when ISNULL(InTime,'')<>''  and ISNULL(OutTime,'')<>''  and DayStatus = 'H' then 'HP'
											else DayStatus end
											
											FROM AttdnProcessData where WorkDate BETWEEN '" + hrfromDate + @"' AND '" + hrtoDate + @"' and PlantID = '" + plantId + @"'
                                            )ad
                                    		INNER JOIN DayType dt ON dt.DayType = ad.DayStatus
                                    
                                    
                                    		
                                    	LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = ad.EmpSystemID
                                    									LEFT OUTER JOIN ORG.Company C ON C.Id = EI.CompanyId
                                                                      LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=EI.BudgetCode
                                    									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                                                        LEFT OUTER JOIN ORG.Entity E ON mpb.EntityId=E.Id
                                                                           LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
                                                                            LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                                                                            LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
                                                                            LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                                                            LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                                                            LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                                                            LEFT JOIN [ShiftDefination] ON ShiftDefination.SystemId = MPB.ShiftDefinationId
                                                                            LEFT JOIN [ORG].[Line] ON Line.Id = MPB.LineId
                                    	 LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = EI.LegalDesignationId
                                    								left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                    left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                    left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId		
                                    		WHERE EI.CompanyId = '" + companyId + @"' AND EI.PlantId= '" + plantId + @"' --and EmpSystemID ='2000037'
                                    			AND WorkDate BETWEEN '" + hrfromDate + @"' AND '" + hrtoDate + @"' 
                                    		) x
                                    	) y
                                    --WHERE Category IN ('Present', 'Late')
                                   Where   Category IN (" + dayStatus + @") " + wcConsiderInOut + @"

                                    GROUP BY EmpSystemID,CompanyName, DOSS,DOJS,Division,Plant,Entity,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys,EmployeeCode,EmployeeCodePreFix,EmployeeCodeNumeric,EmployeeName, ss
                                   HAVING COUNT(*) " + presentComparator + " " + dayCount + @"
                                    
                                    ) dd GROUP BY 
                                      EmployeeCodePreFix,EmployeeCodeNumeric,DOSS,CompanyName,Entity, EmpSystemID, EmployeeCode,DOJS,Division,Plant,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys ,EmployeeName
                                    ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric
                                    
                                    ";
            return presnetsql;
        }

        public string ModalConsecutivePresentDateListSqlSaad(string companyGroupId, string companyId, string plantId, string hrfromDate, string hrtoDate, string dayCount, string presentComparator, string dayStatus, bool considerInOut)
        {
            string wcConsiderInOut = "";
            if (considerInOut == true)
            {
                wcConsiderInOut = "AND (ISNULL(InTime,'')<>'' and ISNULL(OutTime,'')<>'')";
            }
            string presnetsql = @"select Count(max_Present_days) PresentDaysOccured,EmployeeCodePreFix,EmployeeCodeNumeric,DOSS, EmpSystemID, EmployeeCode,DOJS,Division,Plant,Entity,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys,EmployeeName,CompanyName from (
                                    SELECT EmpSystemID,EmployeeCode,EmployeeCodePreFix,EmployeeCodeNumeric, DOJS,Division,Plant,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys,EmployeeName
                                  , DOSS,CompanyName , count(*) max_Present_days, min(WorkDate) workDate, max(WorkDate) mxworkDate,Entity
                                    FROM (
                                    	SELECT *, sum(xx) OVER (
                                    			PARTITION BY EmpSystemID ORDER BY WorkDate
                                    			) ss
                                    	FROM (
                                    		SELECT ad.WorkDate,ad.InTime,ad.OutTime, ad.EmpSystemID, DT.Category,EI.EmployeeCode, ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric,EI.EmployeeName
                                    		,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJS
                                    		,REPLACE(CONVERT(VARCHAR(11), EI.DOS, 106), ' ', '-') DOSS,C.UserName CompanyName
                                    									,Division.UserName Division ,Plant.UserName Plant ,Unit.UserName Unit ,Department.UserName Department ,Section.UserName Section ,SubSection.UserName SubSection ,ShiftDefination.UserName ShiftDefination ,Line.UserName Line 
                                    									,Ldes.UserName LegalDesignation,ec.UserName EmployeeCategorys,E.UserName Entity
                                    		,CASE 
                                    				WHEN Category = lag(Category) OVER (
                                    						PARTITION BY EmpSystemID ORDER BY WorkDate
                                    						)
                                    					THEN 0
                                    				ELSE 1
                                    				END AS xx
                                    		FROM (	SELECT WorkDate,InTime,OutTime,EmpSystemID
											,DayStatus = Case when ISNULL(InTime,'')<>''  and ISNULL(OutTime,'')<>''  and DayStatus = 'W' then 'WP'
											when ISNULL(InTime,'')<>''  and ISNULL(OutTime,'')<>''  and DayStatus = 'H' then 'HP'
											else DayStatus end
											
											FROM AttdnProcessData where WorkDate BETWEEN '" + hrfromDate + @"' AND '" + hrtoDate + @"' and PlantID in (" + plantId + @")
                                            )ad
                                    		INNER JOIN DayType dt ON dt.DayType = ad.DayStatus
                                    
                                    
                                    		
                                    	LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = ad.EmpSystemID
                                    									LEFT OUTER JOIN ORG.Company C ON C.Id = EI.CompanyId
                                                                      LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=EI.BudgetCode
                                    									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                                                        LEFT OUTER JOIN ORG.Entity E ON mpb.EntityId=E.Id
                                                                           LEFT JOIN [ORG].[Division] ON Division.Id = PO.DivisionId
                                                                            LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                                                                            LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
                                                                            LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                                                            LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                                                            LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                                                            LEFT JOIN [ShiftDefination] ON ShiftDefination.SystemId = MPB.ShiftDefinationId
                                                                            LEFT JOIN [ORG].[Line] ON Line.Id = MPB.LineId
                                    	 LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = EI.LegalDesignationId
                                    								left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                    left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                    left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId		
                                    		WHERE EI.CompanyId = '" + companyId + @"' AND EI.PlantId in (" + plantId + @") --and EmpSystemID ='2000037'
                                    			AND WorkDate BETWEEN '" + hrfromDate + @"' AND '" + hrtoDate + @"' 
                                    		) x
                                    	) y
                                    --WHERE Category IN ('Present', 'Late')
                                   Where   Category IN (" + dayStatus + @") " + wcConsiderInOut + @"

                                    GROUP BY EmpSystemID,CompanyName, DOSS,DOJS,Division,Plant,Entity,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys,EmployeeCode,EmployeeCodePreFix,EmployeeCodeNumeric,EmployeeName, ss
                                   HAVING COUNT(*) " + presentComparator + " " + dayCount + @"
                                    
                                    ) dd GROUP BY 
                                      EmployeeCodePreFix,EmployeeCodeNumeric,DOSS,CompanyName,Entity, EmpSystemID, EmployeeCode,DOJS,Division,Plant,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys ,EmployeeName
                                    ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric
                                    
                                    ";
            return presnetsql;
        }

        public IEnumerable<object> ModalConsecutivePresentDateList(string companyGroupId, string companyId, string plantId, string hrfromDate, string hrtoDate, string dayCount, string presentComparator, string dayStatus, bool considerInOut)
        {
            try
            {
                string sql = ModalConsecutivePresentDateListSqlSaad(companyGroupId, companyId, plantId, hrfromDate, hrtoDate, dayCount, presentComparator, dayStatus, considerInOut);


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        [HttpGet, Authorize]
        public ActionResult ModalEmployeeWisePresentDateList(string companyId, string plantId, string hrFromDate, string hrToDate, string EmpSystemId, string dayCount, string comparator, string dayStatus, bool considerInOut)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dayStatus = dayStatusDef;
            considerInOut = true;
            return Json(ModalEmployeeWisePresentDateList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, hrFromDate, hrToDate, EmpSystemId, dayCount, comparator, dayStatus, considerInOut), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetWorkingHoursDaily(string companyId, string wrHrFromDate, string wrHrToDate, string hours, string presentComparator, string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string PlantsId = "'" + PlantId.Replace(",", "','") + "'";//replaced with ""
            var jsondata = Json(GetWorkingHoursDailyData(wrHrFromDate, wrHrToDate, identity.CompanyId, PlantsId, presentComparator, hours), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;

            return jsondata;
            //return Json(_HRDashboardService.GetEEmpJobCardInfoWithInDateTimes(wrHrDate, companyId, hours, presentComparator), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult GetWorkingHoursPeriod(string companyId, string wrHrFromDate, string wrHrToDate, string hours, string presentComparator, string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string PlantsId = "'" + PlantId.Replace(",", "','") + "'";//replaced with ""
            var jsondata = Json(GetGetWorkingHoursPeriodData(wrHrFromDate, wrHrToDate, identity.CompanyId, PlantsId, presentComparator, hours), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;

            return jsondata;
            //return Json(_HRDashboardService.GetEEmpJobCardInfoWithInDateTimes(wrHrDate, companyId, hours, presentComparator), JsonRequestBehavior.AllowGet);
        }




        public string GetGetWorkingHoursDailyDataSql(string wrHrFromDate, string ToDate, string companyId, string plantId, string comparator, string workingHour)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"   SELECT A.EmployeeCode
                            	,A.EmployeeName
                                ,A.EmployeeStatus
                            	,A.DOJS
	                            ,A.DOSS
                                ,A.EmployeeCategorys
                            	,A.GivenDesignation
                                ,A.LegalDesignation
                            	,A.Unit
                                ,A.Entity
                            	,A.Division
                            	,A.Department
                            	,A.Section
                            	,A.SubSection
                            	,REPLACE(CONVERT(VARCHAR(11), A.PDate, 113), ' ', '-') PDate
                                ,PDay
                            	,A.DayStatus
                                ,A.IsHalfDayLeave
                            	,A.InTime
                                ,ShiftInTimeShow
								 ,ShiftInTime
                            	,A.InDeviceID
                            	,A.OutTime
                            	,A.OutDeviceID
                            	,A.IsManual
                            	,A.OTHr OverStay
                                ,A.TotalOTHr FinalOT
                            	,A.LvShortName
                            	,A.Code
                            	,A.LvDescrip
                            	,A.LeaveType
                                ,dti,dto
                                ,InTimeShow
                                ,OutTimeShow
                                ,A.OTConsiderOn
                                ,ShiftTime = CASE WHEN ShiftChangeInTime IS NULL THEN ShiftInTime ELSE ShiftChangeInTime END
                                ,ShiftName
								,ShiftType
							    ,ShiftOutTime
                                ,A.IsManualDayStatus,A.IsManualInTime,A.IsManualOutTime, A.ShortLeave,A.IsOTEntitled,A.IsOTComfirm,A.WorkDate,
                                ReConfirm = CASE  WHEN A.IsOTComfirm=0 AND A.WorkDate IS NOT NULL  THEN 1   ELSE 0  END,A.DayCategory
                                ,A.InTimelate,A.OutTimelate
                                ,A.ShiftInTimeLate
                               
	                            --,A.LeaveDuration                               
								--,A.DurationInMin
                                ,DATEDIFF(second, intime, OutTime) / 3600.0 WorkHour
	                                --,A.EO 
									--,A.LIN
									--,A.LO
                                    ,A.Line
                                    --,A.ExtraOT
                            FROM(
                                SELECT E.EmployeeCode
                                    , E.EmployeeName
                                    ,E.EmployeeStatus
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJS
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOS, 113), ' ', '-') DOSS
                                    , D.UserName GivenDesignation
                                    , U.UserName Unit
                                    , Dv.UserName Division
                                    , Dp.UserName Department
                                    , S.UserName Section
                                    ,ar.IsHalfDayLeave
                                    , SB.UserName SubSection
                                    , EN.UserName Entity
                                    ,datename(dw,AR.WorkDate) as PDay
                                    , AR.WorkDate PDate
                                    , AR.DayStatus
                                  
                                    , HR.OTConsiderOn
                                    ---, AR.InTime InTime
                                    ,  InTime=case when dt.OriginalDayType='W' and ar.IsOTEntitled=0 and  ma.InTime IS NOT NUll then ma.InTime 
									               when dt.OriginalDayType='H' and ar.IsOTEntitled=0  and ma.InTime IS NOT NUll  then ma.InTime 
												   when dt.OriginalDayType='W' and ar.IsOTEntitled=1  and  EOT.FromDate IS NOT NUll  then EOT.FromDate 
												   when dt.OriginalDayType='H' and ar.IsOTEntitled=1  and  EOT.FromDate IS NOT NUll  then EOT.FromDate                                                    
									else   ISNULL(ar.InTime,ar.PunchInTime) end

                                    , AR.InTime InTimeShow
                                   	,l.UserName as Line
                            ,ShiftInTimeLate=CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),108)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 108)
						     END
                                    , CONVERT(VARCHAR(5), AR.InTime, 108) InTimelate
                             ,ShiftInTimeShow = CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100)
						     END
                                    , ARIN.DeviceID InDeviceID
                                    ---, AR.OutTime OutTime
                                    , OutTime=case when dt.OriginalDayType='W' and ar.IsOTEntitled=0 and ma.OutTime IS NOT NUll  then ma.OutTime 
									               when dt.OriginalDayType='H' and ar.IsOTEntitled=0 and ma.OutTime IS NOT NUll then ma.OutTime 
												   when dt.OriginalDayType='W' and ar.IsOTEntitled=1 and EOT.ToDate IS NOT NUll  then EOT.ToDate 
												   when dt.OriginalDayType='H' and ar.IsOTEntitled=1 and EOT.ToDate IS NOT NUll  then EOT.ToDate 
                                                   when dt.OriginalDayType='NW' and ar.IsOTEntitled=1 and EOT.ToDate IS NOT NUll then EOT.ToDate 
									else ar.OutTime end



                                    , AR.OutTime OutTimeShow
                                    , CONVERT(VARCHAR(5), AR.OutTime, 108) OutTimelate
                                    , AROUT.DeviceID OutDeviceID
                                    , AR.IsManualInTime IsManual
                                    ---, AR.OTHr 
                                    , ISNULL( AR.OTHr,0) +ISNULL( EOT.Duration,0) OTHr
                                    ,OT.TotalOTHr
                                    , LT.UserName LvShortName
                                    , LT.Description LvDescrip
                                    , LT.LeaveType
                                    , LT.Code
                                    , Isnull(LG.UserName, '') LegalDesignation
                                    , AR.InTime dti, AR.OutTime dto
                                    , CONVERT(VARCHAR(5), cs.InTime, 108) ShiftChangeInTime
                                    , SD.ShiftDefinationName ShiftName
									,sd.ShiftType
                                    --,LEAVE.LeaveDuration	                            
									--,HODD.DurationInMin

		                            --,EO.OffDuration AS EO
									--,EIN.OffDuration AS LIN
									--,LO= Case when LO.InfoType='LUNCHOUT' THEN 'YES' ELSE 'NO' END,ISNULL( EOT.Duration,0)ExtraOT
                                    ,ISNULL(ec.UserName,'') EmployeeCategorys
						   ,ShiftOutTime = CASE                                   
                           WHEN cs.OutTime IS NULL
                           THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
                           ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                           END
                                     ,ShiftInTime = Format(AR.InTime, 'yyyy-MM-dd') + ' ' + CASE 
			                         WHEN cs.InTime IS NULL
			                         	THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
			                         ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
			                         END
                                    , AR.IsManualDayStatus, AR.IsManualInTime, AR.IsManualOutTime, ar.CountedShortLeave ShortLeave,AR.IsOTEntitled,AR.IsOTComfirm,OT.WorkDate,dt.Category DayCategory
                                FROM dbo.EmployeeInformation E

                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                INNER JOIN dbo.AttdnProcessData AR ON E.SystemID = AR.EmpSystemID
	                           --LEFT JOIN (select LET.SystemID,LTD.LeaveDuration,LTD.WorkDate,LET.EmpSystemID from  LeaveTransaction LET 
										--    left join LeaveTransactionDetails LTD ON LTD.LvTrnsSystemID=LET.SystemID	
                                        --where ltd.WorkDate Between '" + wrHrFromDate + @"' and '" + ToDate + @"'
								        -- ) LEAVE ON LEAVE.EmpSystemID=E.SystemId and LEAVE.WorkDate= AR.WorkDate

                                left join (select EmpSystemID,WorkDate,SUM(DurationInMin)AS DurationInMin
		                    From  [dbo].[HourlyOffDuty] 
	                        WHERE  ApproveType='Deducation' AND WorkDate Between '" + wrHrFromDate + @"' and '" + ToDate + @"'
		                    Group BY  EmpSystemID,WorkDate)as HODD on HODD.EmpSystemID=E.SystemId and HODD.WorkDate=AR.WorkDate

                                LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + ToDate + @"' BETWEEN FromDate AND ToDate) AS SFCG
                                ON AR.ShiftSystemID = SFCG.ShiftDefinationID
                                LEFT JOIN dbo.AttdnRawData ARIN ON AR.InTimeRowID = ARIN.RowID
                                LEFT JOIN dbo.AttdnRawData AROUT ON AR.OutTimeRowID = AROUT.RowID
                                LEFT JOIN dbo.LeaveType LT ON AR.LTSystemID = LT.Id
                                LEFT JOIN ORG.Unit U ON EN.UnitID = U.Id
                                LEFT JOIN ORG.Division Dv ON PO.DivisionID = Dv.Id
                                LEFT JOIN ORG.Department Dp ON PO.DepartmentID = Dp.Id

                                  LEFT JOIN ORG.Section S ON PO.SectionID = S.Id
                                LEFT JOIN ORG.SubSection SB ON PO.SubSectionID = SB.Id
								left join org.Line l on l.Id=mpb.LineId

                                LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
                            
                                	left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                left join EmpDateWiseShiftAssign es on es.EmpSystemID = E.SystemId
                                AND AR.WorkDate = ES.WorkDate
                                left join(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m
                                left join[ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = es.ShiftSystemID and cs.ShiftDate = ar.WorkDate
                                left join[ShiftDefination] sd on sd.SystemID = es.ShiftSystemID
                                LEFT JOIN HKP.Designation D ON E.GivenDesignationId = D.Id
                                LEFT JOIN FinalOT OT ON E.SystemId = OT.EmpSystemID and ot.WorkDate=ar.WorkDate
                                LEFT JOIN PlantWiseHRMSSetting hr on HR.PlantID=E.PlantId
                                LEFT JOIN DayType dt on dt.Daytype=AR.DayStatus

                               -- left join AttendanceInfoExtra LO on LO.EmpSystemId=e.SystemId and LO.WorkDate=ar.WorkDate and LO.InfoType='LUNCHOUT'
								--left join AttendanceInfoExtra EO on EO.EmpSystemId=e.SystemId and EO.WorkDate=ar.WorkDate and EO.InfoType='EARLUOUT'
								--left join AttendanceInfoExtra EIN on EIN.EmpSystemId=e.SystemId and EIN.WorkDate=ar.WorkDate and EIN.InfoType='EARLUIN'


								left join (
								SELECT EmpSystemId,FromDate,ToDate,Duration,WorkDate FROM HourlyOT where  OTType IN ('EXTRAOT','OTLIMIT') 
								
								) EOT on EOT.EmpSystemId=AR.EmpSystemID and EOT.WorkDate=ar.WorkDate

								left join AttdnManualData MA on MA.EmpSystemID=ar.EmpSystemID and MA.WorkDate=ar.WorkDate



                                WHERE E.CompanyId = '" + companyId + @"' AND E.PlantId in (" + plantId + @")
                                     AND AR.WorkDate BETWEEN '" + wrHrFromDate + @"'
                                        AND '" + ToDate + @"' AND (EmployeeStatus = 'Active' OR COnvert(date,DOS) >= Convert(Date,'" + wrHrFromDate + @"'))
                                ) A
                                    where DATEDIFF(second, intime, OutTime) / 3600.0 " + comparator + @"  " + workingHour + @" 
                          
                            ORDER BY A.EmployeeCode
                            	,A.PDate ";



                strSql = @"Select  *,DATEDIFF(second, InTime, OutTime) / 3600.0  WorkHour
                            FROM
						(
                                SELECT p.Id PlantId,p.UserName Plant,E.EmployeeCode EmployeeCode
                                    , E.EmployeeName
                                    ,E.EmployeeStatus
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJS
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOS, 113), ' ', '-') DOSS
                                    , U.UserName Unit
                                    , Dv.UserName Division
                                    , Dp.UserName Department
                                    , S.UserName Section
                                    ,ar.IsHalfDayLeave
                                    , SB.UserName SubSection
                                    , EN.UserName Entity
                                    ,datename(dw,AR.WorkDate) as PDay
                                    , AR.WorkDate PDate
                                    , AR.DayStatus
                                  
          
                                   	
									,ISNULL(ar.InTime,EOT.FromDate) AS InTime
									,CASE WHEN ISNULL(ar.OutTime,'')='' THEN EOT.ToDate ELSE 
										CASE WHEN CONVERT(DATETIME,EOT.ToDate)>CONVERT(DATETIME,ar.OutTime) THEN EOT.ToDate ELSE ar.OutTime END END AS OutTime

                                    , AR.InTime InTimeShow
                                   	,l.UserName as Line
                  
                                    , CONVERT(VARCHAR(5), AR.InTime, 108) InTimelate
                  
                                  

						


                                    , AR.OutTime OutTimeShow
                                    , CONVERT(VARCHAR(5), AR.OutTime, 108) OutTimelate
                                    , AR.IsManualInTime IsManual
                                    , ISNULL( AR.OTHr,0) +ISNULL( EOT.Duration,0) OTHr
                                 
                                    , Isnull(LG.UserName, '') LegalDesignation
                                    , AR.InTime dti, AR.OutTime dto
                               
                                    ,ISNULL(ec.UserName,'') EmployeeCategorys
						  
                                    , AR.IsManualDayStatus, AR.IsManualInTime,
									AR.IsManualOutTime, ar.CountedShortLeave ShortLeave,AR.IsOTEntitled,AR.IsOTComfirm
									,dt.Category DayCategory
                                FROM 
								  dbo.AttdnProcessData AR 
									LEFT JOIN	dbo.EmployeeInformation E ON E.SystemID = AR.EmpSystemID
                                    Left join ORG.Plant p on p.Id = e.PlantId
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                                LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                                LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id

                                  LEFT JOIN ORG.Section S ON PO.SectionID = S.Id
                                LEFT JOIN ORG.SubSection SB ON PO.SubSectionID = SB.Id
								left join org.Line l on l.Id=mpb.LineId

                                LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
                            
                                	left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                left join EmpDateWiseShiftAssign es on es.EmpSystemID = E.SystemId
                                   AND CONVERT(DATE,AR.WorkDate) = CONVERT(DATE,ES.WorkDate)
                               
                                LEFT JOIN DayType dt on dt.Daytype=AR.DayStatus

                        

								left join HourlyOT EOT on EOT.EmpSystemId=AR.EmpSystemID and EOT.WorkDate=ar.WorkDate AND  ISNULL(OTType,'') IN ('EXTRAOT','OTLIMIT') 

								left join AttdnManualData MA on MA.EmpSystemID=ar.EmpSystemID and CONVERT(DATE,MA.WorkDate)=CONVERT(DATE,ar.WorkDate)



                                WHERE E.CompanyId = '" + companyId + @"' AND E.PlantId in (" + plantId + @")
                                     AND AR.WorkDate BETWEEN '" + wrHrFromDate + @"'
                                        AND '" + ToDate + @"' 
										AND (E.DOJ<='" + ToDate + @"'and (isnull(E.DOS,'')='' or E.DOS>='" + wrHrFromDate + @"'))
										--and E.EmployeeCode = '10000103'
					 
                                ) A
													Where DATEDIFF(second, intime, OutTime) / 3600.0 " + comparator + @"  " + workingHour + @" 
                  
                         order by EmployeeCode,PDate
                            ";



                return strSql;
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
        public string GetGetWorkingHoursPeriodDataSql(string wrHrFromDate, string ToDate, string companyId, string plantId, string comparator, string workingHour)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {



                strSql = @"SELECT  PlantId,Plant,EmployeeCode , EmployeeName, EmployeeStatus, DOJS,  DOSS,  Unit, Division,  Department
	                , Section,   SubSection,  Entity,   Line
	                , LegalDesignation,  EmployeeCategorys , Sum(WorkHour) WorkHour
                        FROM (
	                    SELECT p.Id PlantId,p.UserName Plant,E.EmployeeCode EmployeeCode, E.EmployeeName, E.EmployeeStatus, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJS
	                    , REPLACE(CONVERT(VARCHAR(11), E.DOS, 113), ' ', '-') DOSS, U.UserName Unit, Dv.UserName Division, Dp.UserName Department
	                    , S.UserName Section, ar.IsHalfDayLeave, SB.UserName SubSection, EN.UserName Entity, datename(dw, AR.WorkDate) AS PDay
	                    , AR.WorkDate PDate, AR.DayStatus, ISNULL(ar.InTime, EOT.FromDate) AS InTime, CASE 
			WHEN ISNULL(ar.OutTime, '') = ''
				THEN EOT.ToDate
			ELSE CASE 
					WHEN CONVERT(DATETIME, EOT.ToDate) > CONVERT(DATETIME, ar.OutTime)
						THEN EOT.ToDate
					ELSE ar.OutTime
					END
			END AS OutTime, AR.InTime InTimeShow, l.UserName AS Line, CONVERT(VARCHAR(5), AR.InTime, 108) InTimelate
			, AR.OutTime OutTimeShow, CONVERT(VARCHAR(5), AR.OutTime, 108) OutTimelate, AR.IsManualInTime IsManual
			, ISNULL(AR.OTHr, 0) + ISNULL(EOT.Duration, 0) OTHr, Isnull(LG.UserName, '') LegalDesignation, AR.InTime dti, AR.OutTime dto
			, ISNULL(ec.UserName, '') EmployeeCategorys, AR.IsManualDayStatus, AR.IsManualInTime, AR.IsManualOutTime, ar.CountedShortLeave ShortLeave
			, AR.IsOTEntitled, AR.IsOTComfirm, dt.Category DayCategory
			,DATEDIFF(second, ISNULL(ar.InTime, EOT.FromDate), CASE 
			WHEN ISNULL(ar.OutTime, '') = ''
				THEN EOT.ToDate
			ELSE CASE 
					WHEN CONVERT(DATETIME, EOT.ToDate) > CONVERT(DATETIME, ar.OutTime)
						THEN EOT.ToDate
					ELSE ar.OutTime
					END
			END) / 3600.0 WorkHour
                                FROM 
								  dbo.AttdnProcessData AR 
									LEFT JOIN	dbo.EmployeeInformation E ON E.SystemID = AR.EmpSystemID
                                    Left join ORG.Plant p on p.Id = e.PlantId
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                                LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                                LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id

                                  LEFT JOIN ORG.Section S ON PO.SectionID = S.Id
                                LEFT JOIN ORG.SubSection SB ON PO.SubSectionID = SB.Id
								left join org.Line l on l.Id=mpb.LineId

                                LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
                            
                                	left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
                                    left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                    left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                left join EmpDateWiseShiftAssign es on es.EmpSystemID = E.SystemId
                                   AND CONVERT(DATE,AR.WorkDate) = CONVERT(DATE,ES.WorkDate)
                               
                                LEFT JOIN DayType dt on dt.Daytype=AR.DayStatus

                        

								left join HourlyOT EOT on EOT.EmpSystemId=AR.EmpSystemID and EOT.WorkDate=ar.WorkDate AND  ISNULL(OTType,'') IN ('EXTRAOT','OTLIMIT') 

								left join AttdnManualData MA on MA.EmpSystemID=ar.EmpSystemID and CONVERT(DATE,MA.WorkDate)=CONVERT(DATE,ar.WorkDate)



                                WHERE E.CompanyId = '" + companyId + @"' AND E.PlantId in (" + plantId + @")
                                     AND AR.WorkDate BETWEEN '" + wrHrFromDate + @"'
                                        AND '" + ToDate + @"' 
										AND (E.DOJ<='" + ToDate + @"'and (isnull(E.DOS,'')='' or E.DOS>='" + wrHrFromDate + @"'))
										--and E.EmployeeCode = '10000103'
					 
                                ) A
								Group By 
                            	PlantId,Plant,EmployeeCode , EmployeeName, EmployeeStatus, DOJS,  DOSS,  Unit, Division,  Department
                            	, Section,   SubSection,  Entity,   Line
                            	, LegalDesignation,  EmployeeCategorys   
                            
                            HAVING SUM(WorkHour) " + comparator + @"  " + workingHour + @"

                            ";



                return strSql;
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

        public IEnumerable<object> GetWorkingHoursDailyData(string wrHrFromDate, string ToDate, string companyId, string plantId, string comparator, string workingHour)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            _sqlRepository = new SqlRepository();

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                strSql = GetGetWorkingHoursDailyDataSql(wrHrFromDate, ToDate, identity.CompanyId, plantId, comparator, workingHour);

                return _sqlRepository.GetDataCollection(strSql);
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


        public IEnumerable<object> GetGetWorkingHoursPeriodData(string wrHrFromDate, string ToDate, string companyId, string plantId, string comparator, string workingHour)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            _sqlRepository = new SqlRepository();

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                strSql = GetGetWorkingHoursPeriodDataSql(wrHrFromDate, ToDate, identity.CompanyId, plantId, comparator, workingHour);

                return _sqlRepository.GetDataCollection(strSql);
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


        [HttpPost, Authorize]
        public ActionResult PrintEmployeeWorkHourReport(string wrHrFromDate, string ToDate, string comparator, string workingHour, string PlantId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string PlantsId = "'" + PlantId.Replace(",", "','") + "'";//replaced with ""
                var fileName = "workHour" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                var workbook = GetEmployeeWorkHourReport(wrHrFromDate, ToDate, identity.CompanyId, PlantsId, comparator, workingHour, identity.Name);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }
        [HttpPost, Authorize]
        public ActionResult PrintEmployeeWorkHourReportPeriod(string wrHrFromDate, string ToDate, string comparator, string workingHour, string PlantId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string PlantsId = "'" + PlantId.Replace(",", "','") + "'";//replaced with ""
                var fileName = "workHour" + workingHour + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                var workbook = GetEmployeeWorkHourReportPeriod(wrHrFromDate, ToDate, identity.CompanyId, PlantsId, comparator, workingHour, identity.Name);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        public IWorkbook GetEmployeeWorkHourReport(string wrHrFromDate, string ToDate, string companyId, string plantId, string comparator, string workingHour, string userId)
        {
            #region Variable
            clsReport objRpt = null;

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            DataTable dtEmployees = null;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            //IWorksheet sheet2 = null;

            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            int endGenericColumn = 0;
            #endregion Variable

            try
            {

                string strPath = "";
                Image companyLogo = null;
                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();

                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet
                string sql = "";
                sql = GetGetWorkingHoursDailyDataSql(wrHrFromDate, ToDate, companyId, plantId, comparator, workingHour);


                //DataTable empDetailDataTable = ModalEmployeeWisePresentDateListDataTable(companyGroupId, companyId, plantId, hrfromDate, hrtoDate, dayCount, presentComparator, dayStatus, considerInOut);

                dtEmployees = _sqlRepository.GetDataTable(sql);
                //Sql Salary Structure 


                //Sql Salary Process 



                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                //sheet2 = workbook.Worksheets[1];

                #region Sheet1

                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                #region Column Variables
                int ColSr = 0;

                #endregion

                //1


                //SR to

                //xlsCol += 1;

                // 9
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);


                SetCellValue("Employee Code", sheet1, xlsRow, ref xlsCol, out int ColemployeeCode);


                SetCellValue("Employee Name", sheet1, xlsRow, 35, ref xlsCol, out int ColName);
                SetCellValue("Plant", sheet1, xlsRow, 35, ref xlsCol, out int ColPlantName);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out int ColDOJ);

                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out int ColDOS);
                SetCellValue("Legal Designation", sheet1, xlsRow, ref xlsCol, out int ColLDG);
                SetCellValue("EmployeeCategory", sheet1, xlsRow, ref xlsCol, out int colEmpCategory);
                SetCellValue("Entity", sheet1, xlsRow, ref xlsCol, out int colEntity);
                SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out int colDepartment);
                SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out int colSection);
                SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out int colSubSection);
                SetCellValue("Work Date", sheet1, xlsRow, ref xlsCol, out int colWorkDate);
                SetCellValue("In Time", sheet1, xlsRow, ref xlsCol, out int colInTime);
                SetCellValue("Out Time", sheet1, xlsRow, ref xlsCol, out int colOutTime);
                SetCellValue("WorkHours", sheet1, xlsRow, ref xlsCol, out int colPresentDaysOccured);

                endXlsCol = xlsCol - 1;

                xlsCol++;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //endXlsCol = endxlsCol;


                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;


                string FactoryAddress = string.Empty;
                try
                {

                    if (companyLogo != null)
                    {
                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);

                    }
                }
                catch (Exception ex)
                {
                }


                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Working Hours Report between " + wrHrFromDate + " To " + ToDate;//"Salary Sheet For The Month Of " + Convert.ToDateTime(wrHrFromDate).ToString("MMMM") + "," + Convert.ToDateTime(toD).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    try
                    {
                        SrNo += 1;


                        //1
                        sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //2                     
                        sheet1.Range[xlsRow, ColPlantName].Text = dtEmployees.Rows[i]["Plant"].ToString();
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, ColemployeeCode].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, ColemployeeCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColemployeeCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJS"].ToString();
                        sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOSS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOSS"].ToString();
                        sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PDate"].ToString()) == false)
                            sheet1.Range[xlsRow, colWorkDate].Text = dtEmployees.Rows[i]["PDate"].ToString();
                        sheet1.Range[xlsRow, colWorkDate].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colWorkDate].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["InTime"].ToString()) == false)
                        {

                            sheet1.Range[xlsRow, colInTime].NumberFormat = "hh:mm AM/PM";
                            if (string.IsNullOrEmpty(dtEmployees.Rows[i]["InTimeShow"].ToString()) == false)
                                sheet1.Range[xlsRow, colInTime].DateTime = Convert.ToDateTime(dtEmployees.Rows[i]["InTimeShow"].ToString());
                            sheet1.Range[xlsRow, colInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, colInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        sheet1.Range[xlsRow, colInTime].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["OutTime"].ToString()) == false)
                        {
                            sheet1.Range[xlsRow, colOutTime].NumberFormat = "hh:mm AM/PM";
                            if (string.IsNullOrEmpty(dtEmployees.Rows[i]["OutTime"].ToString()) == false)
                                sheet1.Range[xlsRow, colOutTime].DateTime = Convert.ToDateTime(dtEmployees.Rows[i]["OutTime"].ToString());
                            sheet1.Range[xlsRow, colOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, colOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        sheet1.Range[xlsRow, colOutTime].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                            sheet1.Range[xlsRow, ColLDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, ColLDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColLDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCategorys"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEmpCategory].Text = dtEmployees.Rows[i]["EmployeeCategorys"].ToString();
                        sheet1.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4.2

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Entity"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEntity].Text = dtEmployees.Rows[i]["Entity"].ToString();
                        sheet1.Range[xlsRow, colEntity].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEntity].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Department"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colDepartment].Text = dtEmployees.Rows[i]["Department"].ToString();
                        sheet1.Range[xlsRow, colDepartment].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colDepartment].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Section"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colSection].Text = dtEmployees.Rows[i]["Section"].ToString();
                        sheet1.Range[xlsRow, colSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colSection].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SubSection"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colSubSection].Text = dtEmployees.Rows[i]["SubSection"].ToString();
                        sheet1.Range[xlsRow, colSubSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colSubSection].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["WorkHour"].ToString()) == false)
                        {
                            sheet1.Range[xlsRow, colPresentDaysOccured].Number = clsStaticInfo.dbl(dtEmployees.Rows[i]["WorkHour"].ToString());
                            sheet1.Range[xlsRow, colPresentDaysOccured].NumberFormat = GetDecimalFormat(false, 2);
                        }
                        //sheet1.Range[xlsRow, colPresentDaysOccured].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colPresentDaysOccured].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        #endregion
                        #region Attendance Data


                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }


                    #endregion

                    xlsRow++;
                }//for emp count
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, endXlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
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
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "WorKHoursInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion
                #endregion


                workbook.Version = ExcelVersion.Excel2016;
                return workbook;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }

        public IWorkbook GetEmployeeWorkHourReportPeriod(string wrHrFromDate, string ToDate, string companyId, string plantId, string comparator, string workingHour, string userId)
        {
            #region Variable
            clsReport objRpt = null;

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            DataTable dtEmployees = null;



            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            //IWorksheet sheet2 = null;

            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            int endGenericColumn = 0;
            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string strPath = "";
                Image companyLogo = null;
                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();

                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet
                string sql = "";
                sql = GetGetWorkingHoursPeriodDataSql(wrHrFromDate, ToDate, companyId, plantId, comparator, workingHour);


                //DataTable empDetailDataTable = ModalEmployeeWisePresentDateListDataTable(companyGroupId, companyId, plantId, hrfromDate, hrtoDate, dayCount, presentComparator, dayStatus, considerInOut);

                dtEmployees = _sqlRepository.GetDataTable(sql);
                //Sql Salary Structure 


                //Sql Salary Process 



                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                //sheet2 = workbook.Worksheets[1];

                #region Sheet1

                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                #region Column Variables
                int ColSr = 0;

                #endregion

                //1


                //SR to

                //xlsCol += 1;

                // 9
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);


                SetCellValue("Employee Code", sheet1, xlsRow, ref xlsCol, out int ColemployeeCode);


                SetCellValue("Employee Name", sheet1, xlsRow, 35, ref xlsCol, out int ColName);
                SetCellValue("Plant", sheet1, xlsRow, 35, ref xlsCol, out int ColPlant);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out int ColDOJ);

                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out int ColDOS);
                SetCellValue("Legal Designation", sheet1, xlsRow, ref xlsCol, out int ColLDG);
                SetCellValue("EmployeeCategory", sheet1, xlsRow, ref xlsCol, out int colEmpCategory);
                SetCellValue("Entity", sheet1, xlsRow, ref xlsCol, out int colEntity);
                SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out int colDepartment);
                SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out int colSection);
                SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out int colSubSection);

                SetCellValue("WorkHours", sheet1, xlsRow, ref xlsCol, out int colPresentDaysOccured);

                endXlsCol = xlsCol - 1;

                xlsCol++;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //endXlsCol = endxlsCol;


                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;


                string FactoryAddress = string.Empty;
                try
                {

                    if (companyLogo != null)
                    {
                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);

                    }
                }
                catch (Exception ex)
                {
                }


                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Working Hours Report between " + wrHrFromDate + " To " + ToDate;//"Salary Sheet For The Month Of " + Convert.ToDateTime(wrHrFromDate).ToString("MMMM") + "," + Convert.ToDateTime(toD).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    try
                    {
                        SrNo += 1;


                        //1
                        sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //2                     
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, ColemployeeCode].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, ColemployeeCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColemployeeCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //3
                        sheet1.Range[xlsRow, ColPlant].Text = dtEmployees.Rows[i]["Plant"].ToString();
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJS"].ToString();
                        sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOSS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOSS"].ToString();
                        sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        //
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                            sheet1.Range[xlsRow, ColLDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, ColLDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColLDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCategorys"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEmpCategory].Text = dtEmployees.Rows[i]["EmployeeCategorys"].ToString();
                        sheet1.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4.2

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Entity"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEntity].Text = dtEmployees.Rows[i]["Entity"].ToString();
                        sheet1.Range[xlsRow, colEntity].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEntity].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Department"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colDepartment].Text = dtEmployees.Rows[i]["Department"].ToString();
                        sheet1.Range[xlsRow, colDepartment].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colDepartment].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Section"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colSection].Text = dtEmployees.Rows[i]["Section"].ToString();
                        sheet1.Range[xlsRow, colSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colSection].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SubSection"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colSubSection].Text = dtEmployees.Rows[i]["SubSection"].ToString();
                        sheet1.Range[xlsRow, colSubSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colSubSection].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["WorkHour"].ToString()) == false)
                        {
                            sheet1.Range[xlsRow, colPresentDaysOccured].Number = clsStaticInfo.dbl(dtEmployees.Rows[i]["WorkHour"].ToString());
                            sheet1.Range[xlsRow, colPresentDaysOccured].NumberFormat = GetDecimalFormat(false, 2);
                        }
                        //sheet1.Range[xlsRow, colPresentDaysOccured].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colPresentDaysOccured].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        #endregion
                        #region Attendance Data


                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }


                    #endregion

                    xlsRow++;
                }//for emp count
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, endXlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
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
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "WorKHoursInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion
                #endregion


                workbook.Version = ExcelVersion.Excel2016;
                return workbook;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }





        public IEnumerable<object> ModalEmployeeWisePresentDateList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string EmpSystemId, string dayCount, string comparator, string dayStatus, bool considerInOut)
        {
            var daYCountStatus = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            string wcConsiderInOut = "";
            if (considerInOut)
            {
                wcConsiderInOut = "OR (ISNULL(InTime,'')<>'' and ISNULL(OutTime,'')<>'')";
            }
            if (companyId == null || companyId == "null")
            {
                company = "";
            }
            else
            {
                company = @"AND  E.CompanyId = '" + companyId + @"'";
            }
            if (plantId == null || plantId == "null")
            {
                plant = "";
            }
            else
            {
                plant = @"AND  E.PlantId = '" + plantId + @"'";
            }
            if (dayCount == null || dayCount == string.Empty || dayCount == "null" || dayCount == "NaN")
            {
                daYCountStatus = "";
            }
            else
            {
                daYCountStatus = "AND DayNumber.DaysCount " + comparator + @" " + dayCount + @"";
            }
            try
            {
                var sql = @"SELECT EmpSystemID,EmployeeCode,EmployeeCodePreFix,EmployeeCodeNumeric,EmployeeName, DOJS,Division,Plant,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys
                                , COUNT(*) max_Present_days, format(min(WorkDate), 'dd-MMM-yyyy') fromDate, format(max(WorkDate),'dd-MMM-yyyy') toDate
                                FROM (
                                	SELECT *, sum(xx) OVER (
                                			PARTITION BY EmpSystemID ORDER BY WorkDate
                                			) ss
                                	FROM (
                                		SELECT ad.WorkDate,ad.InTime,ad.OutTime, ad.EmpSystemID, DT.Category,EI.EmployeeCode,EI.EmployeeName, ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric 
                                		,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJS
                                									,Division.UserName Division ,Plant.UserName Plant ,Unit.UserName Unit ,Department.UserName Department ,Section.UserName Section ,SubSection.UserName SubSection ,ShiftDefination.UserName ShiftDefination ,Line.UserName Line 
                                									,Ldes.UserName LegalDesignation,ec.UserName EmployeeCategorys
                                		,CASE 
                                				WHEN Category = lag(Category) OVER (
                                						PARTITION BY EmpSystemID ORDER BY WorkDate
                                						)
                                					THEN 0
                                				ELSE 1
                                				END AS xx
                                		FROM (	SELECT WorkDate,InTime,OutTime,EmpSystemID
											,DayStatus = Case when ISNULL(InTime,'')<>''  and ISNULL(OutTime,'')<>''  and DayStatus = 'W' then 'WP'
											when ISNULL(InTime,'')<>''  and ISNULL(OutTime,'')<>''  and DayStatus = 'H' then 'HP'
											else DayStatus end
											
											FROM AttdnProcessData where WorkDate BETWEEN '" + hrFromDate + @"' AND '" + hrToDate + @"' and PlantID = '" + plantId + @"'
                                            ) ad
                                		INNER JOIN DayType dt ON dt.DayType = ad.DayStatus   
                                	LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = ad.EmpSystemID
                                									LEFT OUTER JOIN ORG.Company C ON C.Id = EI.CompanyId
                                                                  LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=EI.BudgetCode
                                									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                                                    LEFT OUTER JOIN ORG.Entity E ON mpb.EntityId=E.Id
                                                                       LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
                                                                        LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                                                                        LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
                                                                        LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                                                        LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                                                        LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                                                        LEFT JOIN [ShiftDefination] ON ShiftDefination.SystemId = MPB.ShiftDefinationId
                                                                        LEFT JOIN [ORG].[Line] ON Line.Id = MPB.LineId
                                	 LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = EI.LegalDesignationId
                                								left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId		
                                		WHERE EI.SystemId ='" + EmpSystemId + @"' and
                                			 WorkDate BETWEEN '" + hrFromDate + @"' AND '" + hrToDate + @"' 
                                		) x
                                	) y Where Category IN (" + dayStatus + @") " + wcConsiderInOut + @"

                                GROUP BY EmpSystemID, DOJS, Division, Plant, Unit, Department, Section, SubSection, ShiftDefination, Line, LegalDesignation, EmployeeCategorys, EmployeeCode, EmployeeCodePreFix, EmployeeCodeNumeric, EmployeeName, ss
                                HAVING count(*) " + comparator + @" " + dayCount + @"

                                ";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public DataTable ModalEmployeeWisePresentDateListDataTable(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string dayCount, string comparator, string dayStatus, bool considerInOut)
        {
            string wcConsiderInOut = "";
            if (considerInOut)
            {
                wcConsiderInOut = "AND (ISNULL(InTime,'')<>'' and ISNULL(OutTime,'')<>'')";

            }
            var daYCountStatus = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            if (companyId == null || companyId == "null")
            {
                company = "";
            }
            else
            {
                company = @"AND  E.CompanyId = '" + companyId + @"'";
            }
            if (plantId == null || plantId == "")
            {
                plant = "";
            }
            else
            {
                plant = @"AND  E.PlantId in (" + plantId + @")";
            }
            if (dayCount == null || dayCount == string.Empty || dayCount == "null" || dayCount == "NaN")
            {
                daYCountStatus = "";
            }
            else
            {
                daYCountStatus = "AND DayNumber.DaysCount " + comparator + @" " + dayCount + @"";
            }

            try
            {
                var sql = @"SELECT EmpSystemID,EmployeeCode,EmployeeCodePreFix,EmployeeCodeNumeric,EmployeeName, DOJS,Division,Plant,Entity,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys
                                , COUNT(*) max_Present_days, format(min(WorkDate), 'dd-MMM-yyyy') fromDate, format(max(WorkDate),'dd-MMM-yyyy') toDate
                                FROM (
                                	SELECT *, sum(xx) OVER (
                                			PARTITION BY EmpSystemID ORDER BY WorkDate
                                			) ss
                                	FROM (
                                		SELECT ad.WorkDate,ad.InTime,ad.OutTime, ad.EmpSystemID, DT.Category,EI.EmployeeCode,EI.EmployeeName, ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric 
                                		,REPLACE(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJS
                                									,Division.UserName Division ,Plant.UserName Plant ,Unit.UserName Unit ,Department.UserName Department ,Section.UserName Section ,SubSection.UserName SubSection ,ShiftDefination.UserName ShiftDefination ,Line.UserName Line 
                                									,Ldes.UserName LegalDesignation,ec.UserName EmployeeCategorys,E.UserName Entity
                                		,CASE 
                                				WHEN Category = lag(Category) OVER (
                                						PARTITION BY EmpSystemID ORDER BY WorkDate
                                						)
                                					THEN 0
                                				ELSE 1
                                				END AS xx
                                		FROM (	SELECT WorkDate,InTime,OutTime,EmpSystemID
											,DayStatus = Case when ISNULL(InTime,'')<>''  and ISNULL(OutTime,'')<>''  and DayStatus = 'W' then 'WP'
											when ISNULL(InTime,'')<>''  and ISNULL(OutTime,'')<>''  and DayStatus = 'H' then 'HP'
											else DayStatus end
											
											FROM AttdnProcessData where WorkDate BETWEEN '" + hrFromDate + @"' AND '" + hrToDate + @"' and PlantID in (" + plantId + @")
                                            )  ad
                                		INNER JOIN DayType dt ON dt.DayType = ad.DayStatus                              
                                
                                		
                                	LEFT OUTER JOIN EmployeeInformation EI ON EI.SystemId = ad.EmpSystemID
                                									LEFT OUTER JOIN ORG.Company C ON C.Id = EI.CompanyId
                                                                  LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=EI.BudgetCode
                                									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                                                    LEFT OUTER JOIN ORG.Entity E ON mpb.EntityId=E.Id
                                                                       LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
                                                                        LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                                                                        LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
                                                                        LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                                                        LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                                                        LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                                                        LEFT JOIN [ShiftDefination] ON ShiftDefination.SystemId = MPB.ShiftDefinationId
                                                                        LEFT JOIN [ORG].[Line] ON Line.Id = MPB.LineId
                                	 LEFT OUTER JOIN [HKP].LegalDesignation LDes ON LDes.Id = EI.LegalDesignationId
                                								left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId		
                                		WHERE EI.CompanyId = '" + companyId + @"' AND EI.PlantId in (" + plantId + @")
                                			AND WorkDate BETWEEN '" + hrFromDate + @"' AND '" + hrToDate + @"'  
                                		) x  Where  Category IN (" + dayStatus + @") " + wcConsiderInOut + @"
                                	) y
                            
                                GROUP BY EmpSystemID, DOJS,Division,Plant,Entity,Unit,Department,Section,SubSection,ShiftDefination,Line,LegalDesignation,EmployeeCategorys,EmployeeCode,EmployeeCodePreFix,EmployeeCodeNumeric,EmployeeName, ss
                                HAVING count(*) " + comparator + @" " + dayCount + @"
		                        ";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IWorkbook GetEmployeePresentStatusReport(string companyGroupId, string companyId, string plantId, string hrfromDate, string hrtoDate, string dayCount, string presentComparator, string userId, string dayStatus, bool considerInOut)
        {
            #region Variable
            clsReport objRpt = null;

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            DataTable dtEmployees = null;



            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            IWorksheet sheet2 = null;

            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
           // int endGenericColumn = 0;
            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string strPath = "";
                Image companyLogo = null;
                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();

                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet

                string sql = ModalConsecutivePresentDateListSqlSaad(companyGroupId, companyId, plantId, hrfromDate, hrtoDate, dayCount, presentComparator, dayStatus, considerInOut);


                DataTable empDetailDataTable = ModalEmployeeWisePresentDateListDataTable(companyGroupId, companyId, plantId, hrfromDate, hrtoDate, dayCount, presentComparator, dayStatus, considerInOut);

                dtEmployees = _sqlRepository.GetDataTable(sql);
                //Sql Salary Structure 


                //Sql Salary Process 



                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                sheet1 = workbook.Worksheets[0];
                sheet2 = workbook.Worksheets[1];

                #region Sheet1

                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                #region Column Variables
                int ColSr = 0;

                #endregion

                //1


                //SR to

                //xlsCol += 1;

                // 9
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);


                SetCellValue("Employee Code", sheet1, xlsRow, ref xlsCol, out int ColemployeeCode);


                SetCellValue("Employee Name", sheet1, xlsRow, ref xlsCol, out int ColName);
                SetCellValue("Plant", sheet1, xlsRow, ref xlsCol, out int ColPlant);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out int ColDOJ);

                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out int ColDOS);
                SetCellValue("Legal Designation", sheet1, xlsRow, ref xlsCol, out int ColLDG);
                SetCellValue("EmployeeCategory", sheet1, xlsRow, ref xlsCol, out int colEmpCategory);
                SetCellValue("Entity", sheet1, xlsRow, ref xlsCol, out int colEntity);
                SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out int colDepartment);
                SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out int colSection);
                SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out int colSubSection);
                SetCellValue("PresentDaysOccured", sheet1, xlsRow, ref xlsCol, out int colPresentDaysOccured);



                endXlsCol = xlsCol - 1;





                xlsCol++;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //endXlsCol = endxlsCol;


                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;


                string FactoryAddress = string.Empty;
                try
                {

                    if (companyLogo != null)
                    {
                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);

                    }
                }
                catch (Exception ex)
                {
                }


                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                //sheet1.Range[xlsRow, 3].Text = "Salary Sheet For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region EmpInfo
                    try
                    {
                        SrNo += 1;
                        x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();

                        //1
                        sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //2                     
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, ColemployeeCode].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, ColemployeeCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColemployeeCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, ColPlant].Text = dtEmployees.Rows[i]["Plant"].ToString();

                        //4
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJS"].ToString();
                        sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOSS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOSS"].ToString();
                        sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        //
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                            sheet1.Range[xlsRow, ColLDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, ColLDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColLDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCategorys"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEmpCategory].Text = dtEmployees.Rows[i]["EmployeeCategorys"].ToString();
                        sheet1.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4.2

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Entity"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEntity].Text = dtEmployees.Rows[i]["Entity"].ToString();
                        sheet1.Range[xlsRow, colEntity].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEntity].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Department"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colDepartment].Text = dtEmployees.Rows[i]["Department"].ToString();
                        sheet1.Range[xlsRow, colDepartment].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colDepartment].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Section"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colSection].Text = dtEmployees.Rows[i]["Section"].ToString();
                        sheet1.Range[xlsRow, colSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colSection].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SubSection"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colSubSection].Text = dtEmployees.Rows[i]["SubSection"].ToString();
                        sheet1.Range[xlsRow, colSubSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colSubSection].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PresentDaysOccured"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colPresentDaysOccured].Number = clsStaticInfo.dbl(dtEmployees.Rows[i]["PresentDaysOccured"].ToString());
                        //sheet1.Range[xlsRow, colPresentDaysOccured].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colPresentDaysOccured].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        #endregion
                        #region Attendance Data


                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }


                    #endregion

                    xlsRow++;
                }//for emp count
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, endXlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
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
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "AttdnInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion
                #endregion

                #region sheet2
                if (empDetailDataTable.Rows.Count > 0)
                {

                    sheet2.IsGridLinesVisible = true;

                    #region------------------Column Header------------------
                    xlsRow = 6;
                    xlsCol = 1;

                    #region Column Variables        

                    #endregion

                    //1


                    //SR to

                    //xlsCol += 1;

                    // 9
                    SetCellValue("Sr. No.", sheet2, xlsRow, ref xlsCol, out ColSr);


                    SetCellValue("Employee Code", sheet2, xlsRow, ref xlsCol, out ColemployeeCode);


                    SetCellValue("Employee Name", sheet2, xlsRow, ref xlsCol, out ColName);
                    SetCellValue("DOJ", sheet2, xlsRow, ref xlsCol, out ColDOJ);

                    SetCellValue("Legal Designation", sheet2, xlsRow, ref xlsCol, out ColLDG);
                    SetCellValue("EmployeeCategory", sheet2, xlsRow, ref xlsCol, out colEmpCategory);
                    SetCellValue("Entity", sheet2, xlsRow, ref xlsCol, out colEntity);
                    SetCellValue("Department", sheet2, xlsRow, ref xlsCol, out colDepartment);
                    SetCellValue("Section", sheet2, xlsRow, ref xlsCol, out colSection);
                    SetCellValue("SubSection", sheet2, xlsRow, ref xlsCol, out colSubSection);

                    SetCellValue("FromDate", sheet2, xlsRow, ref xlsCol, out int colFromDate);
                    SetCellValue("ToDate", sheet2, xlsRow, ref xlsCol, out int colToDate);
                    SetCellValue("Day Count", sheet2, xlsRow, ref xlsCol, out int colDayCount);

                    endXlsCol = xlsCol - 1;

                    xlsCol++;
                    sheet2.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                    sheet2.Range[xlsRow - 1, 1].ColumnWidth = 14;
                    sheet2.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                    sheet2.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet2.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet2.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet2.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;
                    sheet2.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet2.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //endXlsCol = endxlsCol;


                    #endregion------------------Column Header------------------

                    RowIndex = xlsRow + 3;

                    #region ******************Report Header******************
                    xlsRow = 1;
                    xlsCol = 1;


                    FactoryAddress = string.Empty;
                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = sheet2.GetColumnWidth(1) + sheet2.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet2.GetRowHeight(1) + sheet2.GetRowHeight(2) + sheet2.GetRowHeight(3) + sheet2.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet2.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }


                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet2.Range[xlsRow, 3].Text = CmpName;
                    sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet2.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                    sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    sheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet2.Range[xlsRow, 3].Text = FactoryName;
                    sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet2.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    xlsRow += 1;
                    //sheet2.Range[xlsRow, 3].Text = "Salary Sheet For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                    sheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;

                    #endregion ******************Report Header******************

                    #region ----------------------Data-----------------------
                    SrNo = 0;
                    x = "";

                    oRU = new ReportUtility();

                    xlsRow = RowIndex;

                    xlsRow--;
                    for (int i = 0; i <= empDetailDataTable.Rows.Count - 1; i++)
                    {
                        #region EmpInfo
                        try
                        {
                            SrNo += 1;
                            x = empDetailDataTable.Rows[i]["EmpSystemID"].ToString().Trim();

                            //1
                            sheet2.Range[xlsRow, ColSr].Number = (SrNo);
                            sheet2.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet2.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //2                     
                            if (string.IsNullOrEmpty(empDetailDataTable.Rows[i]["EmployeeCode"].ToString()) == false)
                                sheet2.Range[xlsRow, ColemployeeCode].Text = empDetailDataTable.Rows[i]["EmployeeCode"].ToString();
                            sheet2.Range[xlsRow, ColemployeeCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet2.Range[xlsRow, ColemployeeCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //3
                            if (string.IsNullOrEmpty(empDetailDataTable.Rows[i]["EmployeeName"].ToString()) == false)
                                sheet2.Range[xlsRow, ColName].Text = empDetailDataTable.Rows[i]["EmployeeName"].ToString();
                            sheet2.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet2.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;


                            if (string.IsNullOrEmpty(empDetailDataTable.Rows[i]["LegalDesignation"].ToString()) == false)
                                sheet2.Range[xlsRow, ColLDG].Text = empDetailDataTable.Rows[i]["LegalDesignation"].ToString();
                            sheet2.Range[xlsRow, ColLDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet2.Range[xlsRow, ColLDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (string.IsNullOrEmpty(empDetailDataTable.Rows[i]["EmployeeCategorys"].ToString()) == false)// EmployeeCategory Need to Make Correct
                                sheet2.Range[xlsRow, colEmpCategory].Text = empDetailDataTable.Rows[i]["EmployeeCategorys"].ToString();
                            sheet2.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet2.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //4.2

                            if (string.IsNullOrEmpty(empDetailDataTable.Rows[i]["Entity"].ToString()) == false)// EmployeeCategory Need to Make Correct
                                sheet2.Range[xlsRow, colEntity].Text = empDetailDataTable.Rows[i]["Entity"].ToString();
                            sheet2.Range[xlsRow, colEntity].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet2.Range[xlsRow, colEntity].VerticalAlignment = ExcelVAlign.VAlignCenter;


                            if (string.IsNullOrEmpty(empDetailDataTable.Rows[i]["Department"].ToString()) == false)
                                sheet2.Range[xlsRow, colDepartment].Text = empDetailDataTable.Rows[i]["Department"].ToString();
                            sheet2.Range[xlsRow, colDepartment].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet2.Range[xlsRow, colDepartment].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (string.IsNullOrEmpty(empDetailDataTable.Rows[i]["Section"].ToString()) == false)
                                sheet2.Range[xlsRow, colSection].Text = empDetailDataTable.Rows[i]["Section"].ToString();
                            sheet2.Range[xlsRow, colSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet2.Range[xlsRow, colSection].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            if (string.IsNullOrEmpty(empDetailDataTable.Rows[i]["SubSection"].ToString()) == false)
                                sheet2.Range[xlsRow, colSubSection].Text = empDetailDataTable.Rows[i]["SubSection"].ToString();
                            sheet2.Range[xlsRow, colSubSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet2.Range[xlsRow, colSubSection].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (string.IsNullOrEmpty(empDetailDataTable.Rows[i]["fromDate"].ToString()) == false)
                                sheet2.Range[xlsRow, colFromDate].Text = empDetailDataTable.Rows[i]["fromDate"].ToString();
                            sheet2.Range[xlsRow, colFromDate].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet2.Range[xlsRow, colFromDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (string.IsNullOrEmpty(empDetailDataTable.Rows[i]["toDate"].ToString()) == false)// EmployeeCategory Need to Make Correct
                                sheet2.Range[xlsRow, colToDate].Text = empDetailDataTable.Rows[i]["toDate"].ToString();
                            sheet2.Range[xlsRow, colToDate].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet2.Range[xlsRow, colToDate].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //4.2

                            if (string.IsNullOrEmpty(empDetailDataTable.Rows[i]["max_Present_days"].ToString()) == false)// EmployeeCategory Need to Make Correct
                                sheet2.Range[xlsRow, colDayCount].Number = clsStaticInfo.dbl(empDetailDataTable.Rows[i]["max_Present_days"].ToString());
                            sheet2.Range[xlsRow, colDayCount].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet2.Range[xlsRow, colDayCount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                        #endregion
                        xlsRow++;
                    }
                    #endregion ----------------------Data-----------------------

                    #region Line Setup
                    if (RowIndex >= (xlsRow - 1))
                    {
                        xlsRow = RowIndex + 2;
                    }

                    sheet2.Range[RowIndex, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet2.Range[RowIndex, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet2.Range[RowIndex, 1, xlsRow - 1, endXlsCol].WrapText = true;
                    #endregion

                    #region Freeze Panes
                    freezePan = RowIndex - 1;
                    sheet2.UsedRange["A" + freezePan].FreezePanes();
                    sheet2.FirstVisibleColumn = 1;
                    sheet2.FirstVisibleRow = 10;
                    #endregion

                    #region UsedRange Alignment
                    sheet2.UsedRange.WrapText = true;
                    sheet2.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet2.PageSetup.TopMargin = 0.5;
                    sheet2.PageSetup.BottomMargin = 0.7;
                    sheet2.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet2.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet2.PageSetup.LeftMargin = 0.5;
                    sheet2.PageSetup.RightMargin = 0.2;
                    sheet2.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet2.PageSetup.FitToPagesTall = 0;
                    sheet2.PageSetup.FitToPagesWide = 1;
                    sheet2.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet2.IsDisplayZeros = false;
                    sheet2.Name = "Detail";
                    sheet2.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    #endregion
                }
                #endregion
                workbook.Version = ExcelVersion.Excel2016;
                return workbook;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }





        [HttpGet, Authorize]
        public ActionResult ModalEmployeeWisePresentStatusDateWiseList(string companyId, string plantId, string hrFromDate, string hrToDate, string EmpSystemId, string dayCount, string comparator, string dayStatus, bool considerInOut)
        {
            considerInOut = true;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dayStatus = dayStatusDef;
            return Json(ModalEmployeeWisePresentStatusDateWiseList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, hrFromDate, hrToDate, EmpSystemId, dayCount, comparator, dayStatus, considerInOut), JsonRequestBehavior.AllowGet);
        }

        public IEnumerable<object> ModalEmployeeWisePresentStatusDateWiseList(string companyGroupId, string companyId, string plantId, string hrFromDate, string hrToDate, string EmpSystemId, string dayCount, string comparator, string dayStatus, bool considerInOut)
        {
            string wcConsiderInOut = "";
            if (considerInOut == true)
            {
                wcConsiderInOut = "OR (ISNULL(apd.InTime,'')<>'' and ISNULL(apd.OutTime,'')<>'')";
            }
            var daYCountStatus = string.Empty;
            var company = string.Empty;
            var plant = string.Empty;
            if (companyId == null || companyId == "null")
            {
                company = "";
            }
            else
            {
                company = @"AND  E.CompanyId = '" + companyId + @"'";
            }
            if (plantId == null || plantId == "null")
            {
                plant = "";
            }
            else
            {
                plant = @"AND  E.PlantId = '" + plantId + @"'";
            }
            if (dayCount == null || dayCount == string.Empty || dayCount == "null" || dayCount == "NaN")
            {
                daYCountStatus = "";
            }
            else
            {
                daYCountStatus = "AND DayNumber.DaysCount " + comparator + @" " + dayCount + @"";
            }

            try
            {
                var sql = @"SELECT 	REPLACE(CONVERT(VARCHAR(11), APD.WorkDate, 106), ' ', '-') WorkDate,APD.WorkDate WD,DATEName(DW, APD.WorkDate) WeekDays,APD.DayStatus,DT.Description
                             ,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.OutTime, 100), 7)) outTime,LTRIM(RIGHT(CONVERT(VARCHAR(25), APD.InTime, 100), 7)) inTime
								FROM ORG.CompanyGroup CG
								LEFT JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
								LEFT JOIN EmployeeInformation E ON e.GroupID = CG.Id and c.Id = E.CompanyId
								LEFT JOIN AttdnProcessData APD ON APD.EmpSystemID = E.SystemId
								INNER JOIN 	DayType	DT ON DT.DayType = APD.DayStatus

								INNER JOIN (
								SELECT COUNT(WorkDate) DaysCount, EmpSystemId FROM AttdnProcessData  APD
								INNER JOIN DayType DT ON DT.DayType = APD.DayStatus Where (DT.Category IN(" + dayStatus + @") " + wcConsiderInOut + @")
                                  AND   CONVERT(DATE, APD.WorkDate) BETWEEN	CONVERT(DATE, '" + hrFromDate + @"') and CONVERT(DATE,'" + hrToDate + @"')
								GROUP BY EmpSystemId
								)  DayNumber ON DayNUmber.EmpSystemID = E.SystemId
								WHERE E.SystemId = '" + EmpSystemId + @"' AND (DT.Category IN(" + dayStatus + @") " + wcConsiderInOut + @")
								AND
								E.GroupID = '" + companyGroupId + @"' " + company + @"  AND CONVERT(DATE, APD.WorkDate) BETWEEN CONVERT(DATE, '" + hrFromDate + @"') and CONVERT(DATE,'" + hrToDate + @"')  
								";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 4;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 12;
            ColIndex = xlsCol;
            xlsCol += 1;
        }

        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, int colValue, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = colValue;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 12;
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        string GetDecimalFormat(bool isInt, int decimalNo)
        {
            try
            {
                var ob = new ReportUtility();
                if (isInt == true)
                {
                    return ob.NumberFormatInt();
                }
                else
                {
                    return ob.GetDynamicDecimalPlace(decimalNo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}