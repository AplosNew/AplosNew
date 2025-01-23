using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll.Allowance;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.HumanResources.PayRegisterBDReportService;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class DailyDayStatusReportController : BaseController
    {
        #region Constructor

        private readonly IManpowerAttendanceSummary _manpowerAttendanceSummary;
        private readonly ISqlRepository _sqlRepository;

        public DailyDayStatusReportController(ISqlRepository sqlRepository,
              IManpowerAttendanceSummary manpowerAttendanceSummary
            )
        {
            _sqlRepository = sqlRepository;
            _manpowerAttendanceSummary = manpowerAttendanceSummary;
        }

        #endregion Constructor

        #region -- Pages

        
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region --GET Employee--

        [HttpPost, Authorize]
        public ActionResult GetEmpInfo(string workDate, string PrevWorkDate, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var cmdText = @"Select   ei.FatherName,ec.UserName as EmployeeCategory,FORMAT(CAST(AD.intime AS datetime2), N'hh:mm tt')intime,FORMAT(CAST(AD.outtime AS datetime2), N'hh:mm tt')outtime,sd.UserName as ShiftName,en.UserName as Entity,
                    Dp.UserName Department, ad.seq,ad.ds,
                                    S.UserName Section,
                                    SS.UserName SubSection,                                   
									L.UserName Line,
                                    ISNULL(LG.UserName,'') Designation, 
                                    EI.SystemId EmpSystemId,
                                    EI.EmployeeCode, EI.EmployeeName ,
                                    TodayStatus=case 
									when  AD.SEQ=1 THEN AD.DS when isnull(EI.EmployeeCurrentStatus,'') <>'' then EI.EmployeeCurrentStatus else AD.DayStatus 																		
									END,
									PrvDayStatus=case
									when   ADp.DayStatus='LV' THEN ADp.DS when isnull(EI.EmployeeCurrentStatus,'') <>'' then EI.EmployeeCurrentStatus else ADp.DayStatus 	
									end,
	                               -- AD.DayStatus TodayStatus
									 AD.NormalOTHr OTHr,
	                                ADP.DayStatus PrvDayStatus, 
                                    ---isnull(ADP.NormalOTHr,0) YesterdayOTHr,
                                    ADP.NormalOTHr YesterdayOTHr,
                                    ISNULL(EI.LineID,'') LineID,
									EI.SubSectionId,l.Sequence,hr.OTConsiderOn
                                    ,AD.IsOTEntitled IsOTEntitledToday, ADP.IsOTEntitled IsOTEntitledYesterday, ISNULL(ADP.IsOTComfirm,0) IsTodayOTComfirm, ISNULL(AD.IsOTComfirm,0) IsYesterDayOTComfirm
                                    ,ToDayReConfirm = CASE WHEN AD.IsOTComfirm=0 AND AD.FIOTWorkDate IS NOT NULL THEN 1 ELSE 0  END
                                    ,YesterDayReConfirm= CASE WHEN ADP.IsOTComfirm=0 AND ADP.FOTWorkDate IS NOT NULL THEN 1 ELSE 0  END,ADP.YesterDayDayCategory,AD.ToDayDayCategory
					        FROM (select *
							,TBSSEQ=case when isnull(EmployeeCurrentStatus,'')='LONG ABSENTEEISM' then 2 when isnull(EmployeeCurrentStatus,'')='TBS' then 3 else 0 end
							 from EmployeeInformation							
							) EI
							
							left outer join MST.ManpowerBudget as m on m.Id=ei.BudgetCode
LEFT JOIN ORG.Position PR ON m.PositionId=PR.Id
                            left outer join ORG.Entity  as en on en.Id=m.EntityId
                            LEFT JOIN PlantWiseHRMSSetting hr on HR.PlantID=ei.PlantId
					        LEFT JOIN HKP.LegalDesignation LG on EI.LegalDesignationId = LG.Id
                            LEFT JOIN HKP.Designation GD on GD.Id = EI.GivenDesignationId
                            LEFT JOIN ORG.Section S ON S.Id = pr.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.ID = pr.SubSectionId
							LEFT JOIN (select dm.EmployeeCategoryId,d.LegalDesignationId from mst.DesignationMasterLegalDesignation d
							 left join MST.DesignationMaster as dm on dm.Id=d.DesignationMasterId
							 ) kk on kk.LegalDesignationId=ei.LegalDesignationId
							LEFT JOIN ORG.Line L ON L.Id = m.LineId
							left join HKP.EmployeeCategory as EC on ec.Id=kk.EmployeeCategoryId
                            LEFT JOIN  EmployeeOTEntitle OT ON OT.EmpSystemID=EI.SystemId
					        INNER JOIN (SELECT APD.*, FIOT.NormalOTHr, FIOT.WorkDate FIOTWorkDate,dt.Category ToDayDayCategory,Dt.Category
                                            ,SEQ=case when  LTSystemid in (select  id from leavetype where LeaveType='Maternity') then 1
													 when isnull(MaternityStatus,'')<>''  then 1 else 0 end
											--,DS=(select  code from leavetype where LeaveType='Maternity' and id=LTSystemid)
											,DS=case when LTSystemid in (select  id from leavetype where LeaveType='Maternity') then (select  code from leavetype where LeaveType='Maternity' and id=LTSystemid)
											when isnull(MaternityStatus,'')<>'' then MaternityStatus else null end 
                             from dbo.AttdnProcessData APD
							LEFT JOIN FINALOT FIOT on FIOT.EmpSystemID = APD.EmpSystemID AND FIOT.WorkDate=APD.WorkDate
							LEFT JOIN DayType dt on dt.Daytype=APD.DayStatus
							WHERE APD.WorkDate  = '" + workDate + @"' 
							) AD ON AD.EmpSystemID = EI.SystemID
							left outer join dbo.ShiftDefination as sd on sd.SystemID=AD.ShiftSystemID
					        LEFT JOIN (select a.*, fot.NormalOTHr, FOT.WorkDate FOTWorkDate,dty.Category YesterDayDayCategory 
                            ,DS=case when LTSystemid in (select  id from leavetype where LeaveType='Maternity') then (select  code from leavetype where LeaveType='Maternity' and id=LTSystemid)
											when isnull(MaternityStatus,'')<>'' then MaternityStatus else null end
                            from dbo.AttdnProcessData a 
							LEFT JOIN FINALOT FOT on FOT.EmpSystemID = a.EmpSystemID AND FOT.WorkDate=a.WorkDate
                            LEFT JOIN DayType dty on dty.Daytype=a.DayStatus
                                    WHERE a.WorkDate  = '" + PrevWorkDate + @"'
									) ADP ON ADP.EmpSystemID = EI.SystemID 
                                    LEFT JOIN ORG.Department DP on Dp.Id = EI.DepartmentId
                            
                                    WHERE   (EI.EmployeeStatus = 'Active' OR Convert(date,DOS) >= '"+ workDate + @"') AND  EI.Plantid='"+identity.PlantId+@"'  
                            ORDER BY EI.TBSSEQ,AD.SEQ,EmployeeCodePreFix,EmployeeCodeNumeric";


            var jsondata = Json(_sqlRepository.GetDataCollection(cmdText), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        #endregion

        #region --REPORT--

        [HttpPost, Authorize]
        public ActionResult GetDailyDayStatusReport(ReportFormat reportFormat, string workDate,  string PrevWorkDate,List<string> empParameters)
        {
            try
            {
                string EmpList = "' '"; //= "'" + empParameters.Replace(",", "','") + "'";//replaced with ""

                foreach (var item in empParameters)
                {
                    EmpList += ",'" + item + "'";
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var reportFileName = "DailyDayStatus";
                var workbook = _manpowerAttendanceSummary.ExcelDailyDayStatusReport(identity.PlantId, PrevWorkDate, identity.CompanyId, workDate);
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName);

                    case ReportFormat.Excel:
                        //return RenderReportAsExcel(workbook, reportFileName);
                        workbook.SaveAs(reportFileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
                        return null;
                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }            
        }

        #endregion


    }
}