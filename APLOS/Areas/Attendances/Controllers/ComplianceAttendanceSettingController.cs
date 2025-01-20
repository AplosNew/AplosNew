using Aplos.Controllers;
using Aplos.Properties;
using ConnectionManager.DAL;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Attendances.Controllers
{
    public class ComplianceAttendanceSettingController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private ConManager objCon;

        public ComplianceAttendanceSettingController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        [Authorize]
        public ActionResult JobCardCompliance()
        {
            return View();
        }

        
        public ActionResult BuyerJobCardCompliance()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        #region Master Grid Load------start

        [HttpPost]
        public ActionResult getComplianceAttendancelist(string PlantID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select c.*,p.CompanyId from ComplianceAttendanceSetting c
                            left join ORG.Plant p on p.Id = c.PlantID
                            where PlantID ='" + PlantID + @"' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        [HttpPost]
        public ActionResult Save(ComplianceAttendanceSetting ComplianceAttendance)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            SaveCompliance(ComplianceAttendance);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        public void SaveCompliance(ComplianceAttendanceSetting ComplianceAttendance)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM ComplianceAttendanceSetting WHERE Id='" + ComplianceAttendance.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                DataSet dsDefaultValidation;
                string sql1 = "select * From ComplianceAttendanceSetting where PlantId='" + identity.PlantId + @"' and Id <> '" + ComplianceAttendance.Id + @"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsDefaultValidation, false, "1");
                if (dsDefaultValidation.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("More then one setting is not allow..");
                    throw (ex);
                }

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ComplianceAttendanceSetting", out sID);
                    dr["Id"] = "CA" + sID;
                    dr["MaxOTPerDay"] = ComplianceAttendance.MaxOTPerDay;
                    dr["MaxExtraOTPerDay"] = ComplianceAttendance.MaxExtraOTPerDay;
                    dr["IsNoPunchOnWeekOffForOTEntitle"] = ComplianceAttendance.IsNoPunchOnWeekOffForOTEntitle;
                    dr["IsNoPunchOnWeekOffForOTNotEntitle"] = ComplianceAttendance.IsNoPunchOnWeekOffForOTNotEntitle;
                    dr["IsNoPunchOnHolidayForOTEntitle"] = ComplianceAttendance.IsNoPunchOnHolidayForOTEntitle;
                    dr["IsNoPunchOnHolidayForOTNotEntitle"] = ComplianceAttendance.IsNoPunchOnHolidayForOTNotEntitle;
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["PlantId"] = ComplianceAttendance.PlantId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["MaxOTPerDay"] = ComplianceAttendance.MaxOTPerDay;
                    dr["MaxExtraOTPerDay"] = ComplianceAttendance.MaxExtraOTPerDay;
                    dr["IsNoPunchOnWeekOffForOTEntitle"] = ComplianceAttendance.IsNoPunchOnWeekOffForOTEntitle;
                    dr["IsNoPunchOnWeekOffForOTNotEntitle"] = ComplianceAttendance.IsNoPunchOnWeekOffForOTNotEntitle;
                    dr["IsNoPunchOnHolidayForOTEntitle"] = ComplianceAttendance.IsNoPunchOnHolidayForOTEntitle;
                    dr["IsNoPunchOnHolidayForOTNotEntitle"] = ComplianceAttendance.IsNoPunchOnHolidayForOTNotEntitle;
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["PlantId"] = ComplianceAttendance.PlantId;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult Delete(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM ComplianceAttendanceSetting WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        public class ComplianceAttendanceSetting : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public int MaxOTPerDay { get; set; }
            public int MaxExtraOTPerDay { get; set; }
            public bool IsNoPunchOnWeekOffForOTEntitle { get; set; }
            public bool IsNoPunchOnWeekOffForOTNotEntitle { get; set; }
            public bool IsNoPunchOnHolidayForOTEntitle { get; set; }
            public bool IsNoPunchOnHolidayForOTNotEntitle { get; set; }
            public string CompanyGroupId { get; set; }
            public string PlantId { get; set; }
            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            [NeverUpdate]
            public string AddedFromIP { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }
            #endregion Audit Properties
        }

        #endregion -- Operations  

        #region Compliance Job Card Report-----------------

        [HttpPost, Authorize]
        public ActionResult GetEmployeeInformation(string fromDate, string toDate, string criteria)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string wcManual = "";
                string Apjoin = "";
                if (criteria == "MANUALOUTTUIME")
                {
                    wcManual = " AND AP.IsOTEntitled = 1  AND AP.IsManualOutTime = 1";
                    Apjoin = @"INNER JOIN AttdnProcessData AP ON AP.EmpSystemID = E.SystemId
                        INNER JOIN AttdnManualData MA ON AP.EmpSystemID = MA.EmpSystemID AND AP.WorkDate = MA.WorkDate";
                }
                var cmdText = @"SELECT    * fROM(  SELECT   dISTINCT        [CheckBoxSelect] = Convert(bit, 'True'),
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus 
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,E.PlantId
                                    FROM EmployeeInformation e
                                    LEFT OUTER JOIN ORG.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=e.LineId
                                    LEFT OUTER JOIN ORG.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN ORG.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN ORG.Section es on es.id=e.SectionId
                                    LEFT OUTER JOIN ORG.SubSection ess on ess.id=e.SubSectionId
                                    LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN ORG.Unit eu on eu.id=e.UnitId
                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId
                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    " + Apjoin + @"
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                  WHERE DOJ<='" + toDate + @"' AND (DOS is null OR DOS>= '" + fromDate + "') and e.plantId='" + identity.PlantId + @"' and e.GroupID='" + identity.CompanyGroupId + @"' " + wcManual + @"
                                     ) DD ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

                JsonResult json = Json(_sqlRepository.GetDataCollection(cmdText), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void GetEmpJobCardInfoWithInDateTimes(string EmpIdLoop, string FromDate, string ToDate, string plantId, out DataSet dsRef)
        {

            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT A.EmployeeCode,A.EmployeeCodeNumeric
                            	,A.EmployeeName
                                ,A.EmployeeStatus
                            	,A.DOJ
                            	,A.GivenDesignation
                                ,A.LegalDesignation
                            	,A.Unit
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
                            	,A.OriginalDayType
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
                                ,A.GradeCode
	                            ,A.LeaveDuration                               
								,A.DurationInMin

	                                ,A.EO 
									,A.LIN
									,A.LO
                                    ,A.Line,A.WDate
,A.MaxOTPerDay,A.IsNoPunchOnHolidayForOTEntitle,A.IsNoPunchOnHolidayForOTNotEntitle,A.IsNoPunchOnWeekOffForOTEntitle,A.IsNoPunchOnWeekOffForOTNotEntitle
,A.SystemId
                            FROM(
                                SELECT E.EmployeeCode,E.EmployeeCodeNumeric
                                    , E.EmployeeName
                                    ,E.EmployeeStatus
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOS, 113), ' ', '-') DOS
                                    ,E.SystemId
                                    , D.UserName GivenDesignation
                                    , U.UserName Unit
                                    , Dv.UserName Division
                                    , Dp.UserName Department
                                    , S.UserName Section
                                    ,ar.IsHalfDayLeave
                                    , SB.UserName SubSection
                                    ,datename(dw,AR.WorkDate) as PDay,AR.WorkDate WDate
                                    , AR.WorkDate PDate
                                    , AR.DayStatus
                                    , LSalGr.Code GradeCode
                                    , HR.OTConsiderOn
                                    , AR.InTime InTime
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
                                    , AR.OutTime OutTime
                                    , AR.OutTime as OutTimeShow
                                    , CONVERT(VARCHAR(5), AR.OutTime, 108) OutTimelate
                                    , AROUT.DeviceID OutDeviceID
                                    , AR.IsManualInTime IsManual
                                    , AR.OTHr 
                                    ,OT.TotalOTHr
                                    , LT.UserName LvShortName
                                    , LT.Description LvDescrip
                                    , LT.LeaveType
                                    , dt.OriginalDayType
                                    , LT.Code
                                    , Isnull(LG.UserName, '') LegalDesignation
                                    , AR.InTime dti, AR.OutTime dto
                                    , CONVERT(VARCHAR(5), cs.InTime, 108) ShiftChangeInTime
                                    , SD.ShiftDefinationName ShiftName
									,sd.ShiftType
                                    ,LEAVE.LeaveDuration	                            
									,HODD.DurationInMin

		                            ,EO.OffDuration AS EO
									,EIN.OffDuration AS LIN
									,LO= Case when LO.InfoType='LUNCHOUT' THEN 'YES' ELSE 'NO' END

						   ,ShiftOutTime = CASE                                   
                           WHEN cs.OutTime IS NULL
                           THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
                           ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                           END
                                     ,ShiftInTime = Format(AR.WorkDate, 'yyyy-MM-dd') + ' ' + CASE 
			                         WHEN cs.InTime IS NULL
			                         	THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
			                         ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
			                         END
                                    , AR.IsManualDayStatus, AR.IsManualInTime, AR.IsManualOutTime,
ar.CountedShortLeave ShortLeave,AR.IsOTEntitled,AR.IsOTComfirm,OT.WorkDate,dt.Category DayCategory
,ISNULL(CAS.MaxOTPerDay,0)MaxOTPerDay,ISNULL(CAS.IsNoPunchOnHolidayForOTEntitle,0)IsNoPunchOnHolidayForOTEntitle,ISNULL(CAS.IsNoPunchOnHolidayForOTNotEntitle,0)IsNoPunchOnHolidayForOTNotEntitle,ISNULL(CAS.IsNoPunchOnWeekOffForOTEntitle,0)IsNoPunchOnWeekOffForOTEntitle,ISNULL(CAS.IsNoPunchOnWeekOffForOTNotEntitle,0)IsNoPunchOnWeekOffForOTNotEntitle
                                FROM dbo.EmployeeInformation E

                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
left join [dbo].[ComplianceAttendanceSetting] CAS ON CAS.CompanyGroupId=mpb.CompanyGroupId and cas.PlantId=e.PlantId
                                INNER JOIN dbo.AttdnProcessData AR ON E.SystemID = AR.EmpSystemID
	                           LEFT JOIN (select LET.SystemID,LTD.LeaveDuration,LTD.WorkDate,LET.EmpSystemID from  LeaveTransaction LET 
										    left join LeaveTransactionDetails LTD ON LTD.LvTrnsSystemID=LET.SystemID	
                                        where ltd.WorkDate Between '" + FromDate + @"' and '" + ToDate + @"'
								         ) LEAVE ON LEAVE.EmpSystemID=E.SystemId and LEAVE.WorkDate= AR.WorkDate

                                left join (select EmpSystemID,WorkDate,SUM(DurationInMin)AS DurationInMin
		                    From  [dbo].[HourlyOffDuty] 
	                        WHERE  ApproveType='Deducation' AND WorkDate Between '" + FromDate + @"' and '" + ToDate + @"'
		                    Group BY  EmpSystemID,WorkDate)as HODD on HODD.EmpSystemID=E.SystemId and HODD.WorkDate=AR.WorkDate

                                LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + FromDate + @"' BETWEEN FromDate AND ToDate) AS SFCG
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
                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LG.Id and LSGD.PlantId='" + plantId + @"'
                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = LSGD.LegalSalaryGradeId
                                --left join EmpDateWiseShiftAssign es on es.EmpSystemID = E.SystemId
                                --AND AR.WorkDate = ES.WorkDate
                                left join(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m
                                left join[ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = AR.ShiftSystemID and cs.ShiftDate = ar.WorkDate
                                left join[ShiftDefination] sd on sd.SystemID = AR.ShiftSystemID
                                LEFT JOIN HKP.Designation D ON E.GivenDesignationId = D.Id
                                LEFT JOIN FinalOT OT ON E.SystemId = OT.EmpSystemID and ot.WorkDate=ar.WorkDate
                                LEFT JOIN PlantWiseHRMSSetting hr on HR.PlantID=E.PlantId
                                LEFT JOIN DayType dt on dt.Daytype=AR.DayStatus

                                left join AttendanceInfoExtra LO on LO.EmpSystemId=e.SystemId and LO.WorkDate=ar.WorkDate and LO.InfoType='LUNCHOUT'
								left join AttendanceInfoExtra EO on EO.EmpSystemId=e.SystemId and EO.WorkDate=ar.WorkDate and EO.InfoType='EARLUOUT'
								left join AttendanceInfoExtra EIN on EIN.EmpSystemId=e.SystemId and EIN.WorkDate=ar.WorkDate and EIN.InfoType='EARLUIN'

                                WHERE E.SystemID in (" + EmpIdLoop + @")
                                    AND AR.WorkDate BETWEEN '" + FromDate + @"'
                                        AND '" + ToDate + @"' AND (EmployeeStatus = 'Active' OR Convert(date,DOS) >= Convert(Date,'" + FromDate + @"'))
                                ) A
                           
                            ORDER BY A.EmployeeCode
                            	,A.PDate
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
        }//End Function
        private void GetEmpJobCardMonthlySummary(string EmpIdLoop, string FromDate, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @" SELECT EmpSystemID,EmployeeCode, WorkDate ,EmployeeCode, ISNULL(TotalPresent, 0) TotalPresent, ISNULL(TotalLv, 0) TotalLv,ISNULL(TotalHoliDay,0)TotalHoliDay,ISNULL(TotalWeekOff, 0)TotalWeekOff
                                ,ISNULL(TotalLWP, 0) TotalLWP,ISNULL(TotalMLv, 0) TotalMLv,ISNULL(TotalMLv, 0) TotalMLv,isnull(TotalAbsent,0)TotalAbsent,ISNULL(TotalLate,0)TotalLate
                                , DayValue = ISNULL(TotalPresent, 0) + ISNULL(TotalLate, 0) + ISNULL(TotalLv, 0) + ISNULL(TotalMLv, 0) + ISNULL(TotalWeekOff, 0)
                                + ISNULL(TotalCompAssignLv, 0) + ISNULL(TotalHoliDay, 0) + ISNULL(TotalWeekOffHoliDay, 0),Category,DayStatus
                                FROM(SELECT EmpSystemID, WorkDate, EmployeeCode,Category,DayStatus,

                               --TotalPresent = CASE WHEN Category = 'Present' and LTSystemID is null THEN 1
                               --WHEN Category = 'Present' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                               --WHEN Category = 'Late' and LTSystemID is null THEN 1
                               --WHEN Category = 'Leave' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                               --WHEN Category = 'Half Day' and LTSystemID is not null THEN (1-LeaveDuration)
                               --WHEN Category = 'Half Day' and LTSystemID is null THEN 0.5
                               --ELSE 0 END,
                                TotalPresent = CASE WHEN DayStatus = 'P' or DayStatus = 'PW' THEN 1 
                                ELSE 0 END,

                               TotalLate = CASE WHEN DayStatus = 'L'THEN 1
                                ELSE 0 END,
                                
                                TotalAbsent = CASE WHEN Category = 'Absent' and LTSystemID is null THEN 1
                                WHEN Category = 'Absent' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                                WHEN Category = 'Absent' and LTSystemID is not null and LeaveDuration=1 THEN 1
                                WHEN Category = 'Half Day' and LTSystemID is null THEN 0.5
                                ELSE 0 END,
                                
                                TotalLv = CASE WHEN LTSystemID is not null and Category<>'Leave' and LeaveDuration<1 and IsLWP=0 THEN LeaveDuration
                                WHEN LTSystemID is not null and Category='Leave' and IsLWP=0 THEN LeaveDuration
                                ELSE 0 END,
                                
                                TotalLWP = CASE WHEN LTSystemID is not null and Category<>'Leave' and LeaveDuration<1 and IsLWP=1 THEN LeaveDuration
                                WHEN LTSystemID is not null and Category='Leave' and IsLWP=1 THEN LeaveDuration
                                ELSE 0 END,
                                
                                TotalMLv = 0,
                                TotalCompAssignLv = 0,

                              --TotalWeekOff = CASE WHEN OriginalDayType = 'W' and c.IsNoPunchOnWeekOffForOTEntitle=1 and a.IsOTEntitled=0 THEN 1
								                       -- WHEN OriginalDayType = 'W' and c.IsNoPunchOnWeekOffForOTNotEntitle=1 and a.IsOTEntitled=1 THEN 1
								                       -- WHEN OriginalDayType = 'CW' and c.IsNoPunchOnWeekOffForOTNotEntitle=1 and a.IsOTEntitled=1 THEN 1
								                      --  WHEN OriginalDayType = 'CW' and c.IsNoPunchOnWeekOffForOTNotEntitle=1 and a.IsOTEntitled=0 THEN 1                               
                                --ELSE 0 END,

                               -- TotalWeekOff = CASE WHEN Category = 'Weekend' and c.IsNoPunchOnWeekOffForOTEntitle=1 and a.IsOTEntitled=0 THEN 1
								                       --WHEN Category = 'Weekend' and c.IsNoPunchOnWeekOffForOTNotEntitle=1 and a.IsOTEntitled=1 THEN 1
                                --ELSE 0 END,
                                
                                TotalWeekOff = CASE WHEN DayStatus = 'W' OR DayStatus = 'WL' OR DayStatus = 'CW' OR DayStatus = 'CWL' OR DayStatus = 'CWP' OR DayStatus = 'WP' THEN 1
                                ELSE 0 END,

                                TotalHoliDay = CASE WHEN p.OriginalDayType = 'H' AND C.IsNoPunchOnHolidayForOTEntitle=1 AND A.IsOTEntitled=0 THEN 1
														WHEN p.OriginalDayType = 'H' AND C.IsNoPunchOnHolidayForOTNotEntitle=1 AND A.IsOTEntitled=1 THEN 1
                                ELSE 0 END,
                                
                                TotalWeekOffHoliDay = 0,
                                OTHr
                                FROM dbo.AttdnProcessData a
                                left join daytype p on a.DayStatus=p.DayType
                                left join employeeInformation ei on ei.SystemId =a.EmpSystemID
	                            left join ComplianceAttendanceSetting c on c.CompanyGroupId=ei.GroupID and c.PlantId = ei.PlantId
                                WHERE  ei.SystemId in( " + EmpIdLoop + @")
                                and WorkDate between '" + FromDate + @"' AND '" + ToDate + @"'
                                ) A  ";
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

        [HttpGet]
        public ActionResult GetComplianceJobCardReport(ReportFormat reportFormat, string[] employeeId, string fromDate, string toDate, bool chkAdditionInfo)
        {
            try
            {
                string EmpIdLoop = "";
                foreach (string item in employeeId)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "" + item + ""; ;
                    }

                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = GetComplianceJobCardReport(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, EmpIdLoop, fromDate, toDate, chkAdditionInfo);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + " Job Card Report";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        PdfDocument document = new PdfDocument();
                        ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings();
                        settings.TemplateDocument = document;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document = converter1.Convert(settings);
                        }
                        document.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
                        return null;
                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public void GetExtraAbsentCount(string fromDate, string toDate, string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @" SELECT waa.WorkingDate,waa.EmpSystemID,e.EmployeeCode
                            FROM [SCS].[WeeklyAbsentismAssignment] waa
		                    left join EmployeeInformation e on e.SystemId= waa.EmpSystemID
                              WHERE waa.WorkingDate between '" + fromDate + "' and '" + toDate + "' and  waa.plantid='" + plantid + @"' 
                            UNION
                            SELECT ha.WorkDate WorkingDate,ha.EmpSystemID,e.EmployeeCode
                            FROM [trn].[HolidayAbsentismAssignment] ha
		                    left join EmployeeInformation e on e.SystemId=ha.EmpSystemID
                              where ha.WorkDate between '" + fromDate + "' and '" + toDate + "'  and ha.plantid='" + plantid + @"'
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

        public void GetManualOutTimeForOTReport(string strEmpCode, string FromDate, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId
                        	,EI.EmployeeCode
                        	,EI.EmployeeName
                        	,PMB.Code BudgetCode
                        	,LGD.userName LegalDesignation
                        	,DSG.UserName Designation
                        	,DP.UserName Department
                        	,se.UserName Section
                        	,Sus.UserName SubSection
                        	,E.UserName EntityName
                        	,PR.UserName PositionName
                        	,FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                        	--,FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                        	--, CONVERT(VARCHAR(5), cs.InTime, 108) ShiftChangeInTime
                        	,SD.ShiftDefinationName ShiftName
                        	,sd.ShiftType
                        	,ShiftOutTime = CASE 
                        		WHEN cs.OutTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.OutTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                        		END
                        	,ShiftInTime = CASE 
                        		WHEN cs.InTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
                        		END
                        	,ISNULL(AP.InTime, '') InTime
                        	,ISNULL(AP.OutTime, '') OutTime
                        
                        	,ISNULL(MA.UpdatedBy, MA.AddedBy) ManualAttdnUser
                        	,ISNULL(ISNULL(MA.DateUpdated, MA.DateAdded), '') ManualAttdnDate
                        	,AP.IsOTComfirm
                        	,OTF.NormalOTHr ComfirmedOT
                        	,AP.OTHr CalOT
                        	,ISNULL(AP.PunchInTime, '') PunchInTime
                        	,ISNULL(AP.PunchOutTime, '') PunchOutTime
                        FROM AttdnProcessData AP
                        LEFT JOIN AttdnManualData MA ON AP.EmpSystemID = MA.EmpSystemID
                        	AND AP.WorkDate = MA.WorkDate
                        
                        LEFT JOIN FinalOT OTF ON AP.EmpSystemID = OTF.EmpSystemID
                        	AND AP.WorkDate = OTF.WorkDate
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        LEFT JOIN EmpDateWiseShiftAssign es ON es.EmpSystemID = EI.SystemId
                        	AND AP.WorkDate = ES.WorkDate
                        Left join DayType DT ON DT.DayType = AP.DayStatus
                        LEFT JOIN (
                        	SELECT m.ShiftDefinationID
                        		,c.ShiftDate
                        		,m.InTime
                        		,m.SystemID
                        		,m.OutTime
                        	FROM [ShiftTimeChgMaster] m
                        	LEFT JOIN [ShiftTimeChgChild] c ON m.SystemID = c.STCMasterSystemID
                        	) CS ON cs.ShiftDefinationID = es.ShiftSystemID
                        	AND cs.ShiftDate = AP.WorkDate
                        LEFT JOIN [ShiftDefination] sd ON sd.SystemID = es.ShiftSystemID
                        LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode = PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                        LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
                        LEFT JOIN HKP.Designation DSG ON PR.DesignationId = DSG.Id
                        LEFT JOIN HKP.Designation DeG ON DeG.Id = EI.GivenDesignationId
                        LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                        LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                        WHERE EI.EmployeeStatus = 'Active'
                        	AND AP.IsOTEntitled = 1
                        	AND AP.IsManualOutTime = 1
                        	AND AP.WorkDate BETWEEN '" + FromDate + @"'
                        		AND '" + ToDate + @"'
                        	AND EI.SystemId " + strEmpCode + @"
                        ORDER BY AP.WorkDate
                        	,convert(INT, EI.EmployeeCode)";


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
        public void GetJobCardPayDays(string EmpIdLoop, string FromDate, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                clsCrossModule ob = new clsCrossModule();
                strSql = @"SELECT EmpSystemID, WorkDate ,EmployeeCode
           , DayValue = ISNULL(TotalPresent, 0) + ISNULL(TotalLate, 0) + ISNULL(TotalLv, 0) + ISNULL(TotalMLv, 0) + ISNULL(TotalWeekOff, 0)
           + ISNULL(TotalCompAssignLv, 0) + ISNULL(TotalHoliDay, 0) + ISNULL(TotalWeekOffHoliDay, 0)
                            FROM(SELECT EmpSystemID, WorkDate, EmployeeCode,
                                        " + ob.GetAttSum() + @"
                                        OTHr
                                  FROM dbo.AttdnProcessData a
                        left join daytype p on a.DayStatus=p.DayType
                             left join  employeeInformation ei on  ei.SystemId =a.EmpSystemID
                                WHERE  ei.SystemId in( " + EmpIdLoop + @")
                                    and WorkDate between '" + FromDate + @"' AND '" + ToDate + @"'
                                --    AND MONTH(WorkDate) = MONTH('" + FromDate + @"')
                                --   AND YEAR(WorkDate) = YEAR('" + ToDate + @"')
                                                                        ) A";

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
        }
        public void GetWeeklyAbsentismAssignment(string ePlant, string eCode, string eFrmDate, string eToDate, out DataSet dsWeeklyAbsnt)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT  REPLACE(CONVERT(VARCHAR(11),WorkingDate,106),' ','-') WorkingDate,EmpSystemID,EmployeeCode
                                            FROM [SCS].[WeeklyAbsentismAssignment] S
                                            LEFT JOIN EmployeeInformation EI on EI.SystemId = S.EmpSystemID
                                            where WorkingDate BETWEEN '" + eFrmDate + @"' AND '" + eToDate + @"'  AND S.plantid= '" + ePlant + @"' AND EmpSystemID in( " + eCode + ")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsWeeklyAbsnt, false, false, "", "1");
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
        public void SelectedPlantWiseCompany(string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT p.UserName PlantName,c.UserName CompanyName ,ISNULL(a.Address1,'')+','+ ISNULL(a.Address2,'') Address1, a.Phone,a.Email
                                ,cm.Address1 cAddress1 ,cm.Address2 cAddress2
                                FROM org.Plant p
							LEFT OUTER JOIN org.Company c on c.Id=p.CompanyId
							LEFT OUTER JOIN mst.AddressMaster a on a.Id=p.AddressMasterId
							LEFT OUTER JOIN mst.AddressMaster cm on cm.Id=c.AddressMasterId
							WHERE p.Id='" + sPlantID + "'";

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
        public DataTable SelectedPlantWiseCompanyDT(string sPlantID)
        {
            string strSql = "";
            try
            {
                strSql = @"SELECT p.UserName PlantName,c.UserName CompanyName ,ISNULL(a.Address1,'')+','+ ISNULL(a.Address2,'') Address1, a.Phone,a.Email
                                ,cm.Address1 cAddress1 ,cm.Address2 cAddress2
                                FROM org.Plant p
							LEFT OUTER JOIN org.Company c on c.Id=p.CompanyId
							LEFT OUTER JOIN mst.AddressMaster a on a.Id=p.AddressMasterId
							LEFT OUTER JOIN mst.AddressMaster cm on cm.Id=c.AddressMasterId
							WHERE p.Id='" + sPlantID + "'";
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

        public IWorkbook GetComplianceJobCardReport(string username, string companyGroupId, string companyId, string plantId, string plantName, string EmpIdLoop, string fromDate, string toDate, bool chkAdditionInfo)
        {

            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsBioDvAC = null;
            DataTable dtBioDvAC = null;
            DataView dvBioDvAC = null;
            DataView dvSummary = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsMonthlySummary = null;

            DataSet dsPayDays = null;
            DataTable dtMonthlySummary = null;
            DataView dvPayDays = null;

            StringCollection sEmpCodeColl = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            var workbook = oru.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            IWorksheet sheet1 = null;

            DataSet dsWeeklyAbsnt = null;
            DataTable dtWeeklyAbsnt = null;
            DataView dvWeeklyAbsnt = null;
            DataSet dsValidation = null;
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string sOfficeInTime = "00:00";
            string sInTime = "00:00";
            string freezeRow = "";
            int StartRow = xlsRow;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                #region Validation
                if (string.IsNullOrEmpty(fromDate) == true || bplib.clsWebLib.IsDateOK(fromDate) == false)
                {

                    Exception ex = new Exception("Please define access From Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }
                if (string.IsNullOrEmpty(fromDate) == true || bplib.clsWebLib.IsDateOK(fromDate) == false)
                {

                    Exception ex = new Exception("Please define access To Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }
                DateTime dtFrmDate = bplib.clsWebLib.DateData_DBToApp(fromDate, bplib.clsWebLib.DB_DATE_FORMAT);
                DateTime dtToDate = bplib.clsWebLib.DateData_DBToApp(fromDate, bplib.clsWebLib.DB_DATE_FORMAT);
                TimeSpan tsFromToDate = dtToDate - dtFrmDate;
                int daysFromTo = tsFromToDate.Days;
                if (daysFromTo < 0)
                {
                    Exception ex = new Exception("Please check the access From Date, cannot more than access To Date...");
                    throw (ex);
                }

                //string sql1 = "select * From ComplianceAttendanceSetting where PlantId ='" + identity.PlantId + @"' ";
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql1, out dsValidation, false, "1");
                //if (dsValidation.Tables[0].Rows.Count < 1)
                //{
                //    Exception ex = new Exception("OT Settings are incomplete .");
                //    throw (ex);
                //}
                #endregion Validation

                objRpt = new clsReport();
                dvPayDays = new DataView();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                #region DataSet

                GetEmpJobCardInfoWithInDateTimes(EmpIdLoop, fromDate, toDate, plantId, out dsBioDvAC);
                dtBioDvAC = dsBioDvAC.Tables[0];

                GetEmpJobCardMonthlySummary(EmpIdLoop, fromDate, toDate, out dsMonthlySummary);
                dtMonthlySummary = dsMonthlySummary.Tables[0];

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                DataView dvExtraAbsentDate = null;
                GetExtraAbsentCount(fromDate, toDate, plantId, out dsExtraAbsent);
                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);
                dvExtraAbsentDate = new DataView(dsExtraAbsent.Tables[0]);

                GetJobCardPayDays(EmpIdLoop, fromDate, toDate, out dsPayDays);
                var ListPayDays = dsPayDays.Tables[0].ToList<PayDaysReport>();
                ParaMontlyAttendance objm = new ParaMontlyAttendance();
                dvWeeklyAbsnt = new DataView();
                GetWeeklyAbsentismAssignment(plantId, EmpIdLoop, fromDate, toDate, out dsWeeklyAbsnt);
                dtWeeklyAbsnt = dsWeeklyAbsnt.Tables[0];
                dvWeeklyAbsnt.Table = dtWeeklyAbsnt;
                SelectedPlantWiseCompany(plantId, out dsCmp);
                SelectedPlant(plantId, out dsFactory);
                #endregion DataSet

                if (dsBioDvAC.Tables[0].Rows.Count > 0)
                {
                    sEmpCodeColl = new StringCollection();
                    for (int i = 0; i <= dsBioDvAC.Tables[0].Rows.Count - 1; i++)
                    {
                        if (sEmpCodeColl.Contains(dsBioDvAC.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim()) == false)
                        {
                            sEmpCodeColl.Add(dsBioDvAC.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim());
                        }
                    }

                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;
                    workbook = application.Workbooks.Create(sEmpCodeColl.Count);
                    for (int Ec = 0; Ec < sEmpCodeColl.Count; Ec++)
                    {
                        dvBioDvAC = new DataView();
                        dvBioDvAC.Table = dtBioDvAC;

                        dvSummary = new DataView();
                        dvSummary.Table = dtMonthlySummary;

                        dvBioDvAC.RowFilter = "EmployeeCode = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";
                        dvSummary.RowFilter = "EmployeeCode = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";
                        dvExtraAbsent.RowFilter = "EmployeeCode = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";

                        if (dvBioDvAC.Count > 0)
                        {
                            sheet1 = workbook.Worksheets[Ec];
                            sheet1.IsGridLinesVisible = true;
                            xlsRow = 6;
                            string strEmpCode = "";
                            int iDate = 0;
                            int iShiftIntime = 0;
                            int iInTime = 0;
                            int iOutTime = 0;
                            int iTotalOT = 0;
                            int iDayStatus = 0;
                            int iODD = 0;
                            int iLvShortName = 0;
                            string strLateBy = "00:00:00";
                            int iLateBy = 0;
                            int iShiftName = 0;
                            int iShiftOuttime = 0;
                            var iDay = 0;
                            var iOverStay = 0;
                            var OTtotal = 0.00;
                            var OTOverstay = 0.00;
                            var OTOverstayNew = 0.00;
                            var OTOverstay1 = 0.00;
                            var OTOverstay2 = 0.00;
                            var total = 0.00;
                            string employeeName = "";
                            object chequeAmount;
                            object OverStay;
                            object totalPresentDays;
                            object totalAbsentDays;
                            object totalLateDays;
                            object totalLeaveDays;
                            object totalWeekOFFDays;
                            object totalHolidays;
                            object totalODD;
                            object totalDays;
                            object totalHalfDays;
                            object totalHalfDaysLeave;
                            object totalLeaveAbsentDays;
                            object totalAbsentLeaveDays;
                            object totalExtraAbsent;
                            object toTotalLWP;

                            chequeAmount = dvBioDvAC.ToTable().Compute(@"Sum(FinalOT)", "");
                            OverStay = dvBioDvAC.ToTable().Compute(@"Sum(OverStay)", "");
                            totalPresentDays = dvSummary.ToTable().Compute(@"Sum(TotalPresent)", null);
                            totalHalfDays = 0;
                            totalHalfDaysLeave = 0;
                            totalLeaveAbsentDays = 0;
                            totalAbsentLeaveDays = 0;
                            totalAbsentDays = dvSummary.ToTable().Compute(@"SUM(TotalAbsent)", null);
                            totalExtraAbsent = dvExtraAbsent.ToTable().Compute(@"Count(WorkingDate)", null);
                            totalLateDays = dvSummary.ToTable().Compute(@"SUM(TotalLate)", null);
                            totalLeaveDays = dvSummary.ToTable().Compute(@"SUM(TotalLv)", null);
                            totalWeekOFFDays = dvSummary.ToTable().Compute(@"SUM(TotalWeekOff)", null);
                            totalHolidays = dvSummary.ToTable().Compute(@"SUM(TotalHoliDay)", null);
                            totalODD = dvBioDvAC.ToTable().Compute(@"SUM(DurationInMin)", null);
                            totalDays = dvSummary.ToTable().Compute(@"COUNT(DayValue)", null);
                            toTotalLWP = dvSummary.ToTable().Compute(@"SUM(TotalLWP)", null);

                            for (int i = 0; i < dvBioDvAC.Count; i++)
                            {


                                if ((string.Compare(strEmpCode.ToUpper(), dvBioDvAC[i]["EmployeeCode"].ToString().Trim().ToUpper())) != 0)
                                {

                                    #region ------------------Column Header------------------
                                    employeeName = dvBioDvAC[i]["EmployeeName"].ToString().Trim();
                                    xlsCol = 1;
                                    xlsRow = 5;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Emp Code";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["EmployeeCode"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Text = "Emp Name";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["EmployeeName"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "DOJ";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["DOJ"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Designation";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["LegalDesignation"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Grade";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["GradeCode"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Current Status";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["EmployeeStatus"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    xlsCol = 5;
                                    xlsRow = 5;

                                    sheet1.Range[xlsRow, xlsCol].Text = "Unit";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Unit"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;

                                    sheet1.Range[xlsRow, xlsCol].Text = "Division";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Division"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Department";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Department"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Section";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Section"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "SubSection";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["SubSection"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    xlsRow = 5;
                                    xlsCol = 9;

                                    #region Total
                                    //-----Total------
                                    sheet1.Range[1, 10].Text = "Job Card Summary";
                                    sheet1.Range[1, 10, 1, 10 + 2].Merge();

                                    sheet1.Range[1, 10, 1, 9 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[1, 10, 1, 9 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[1, 10, 1, 9 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[1, 10, 1, 9 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //1 += 1;

                                    sheet1.Range[2, 10].Text = "Present Days";
                                    sheet1.Range[2, 10, 2, 10 + 1].Merge();

                                    sheet1.Range[2, 10 + 2].Text = (Convert.ToDouble(totalPresentDays) + (Convert.ToDouble(totalHalfDays) * 0.5)).ToString();
                                    sheet1.Range[2, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[2, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[2, 10, 2, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[2, 10, 2, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[2, 10, 2, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //xlsRow += 1;

                                    sheet1.Range[3, 10].Text = "Leave Days / LWP";
                                    sheet1.Range[3, 10, 3, 10 + 1].Merge();

                                    sheet1.Range[3, 10 + 2].Text = (Convert.ToDouble(totalLeaveDays) + (Convert.ToDouble(totalHalfDaysLeave) * 0.5) + (Convert.ToDouble(totalAbsentLeaveDays) * 0.5)).ToString() + " / " + toTotalLWP;
                                    sheet1.Range[3, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[3, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[3, 10, 3, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[3, 10, 3, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[3, 10, 3, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //xlsRow += 1;
                                    sheet1.Range[4, 10].Text = "Absent Days/Extra Ab";
                                    sheet1.Range[4, 10, 4, 10 + 1].Merge();

                                    sheet1.Range[4, 10 + 2].Text = (Convert.ToDouble(totalAbsentDays) + (Convert.ToDouble(totalLeaveAbsentDays) * 0.5) - (Convert.ToDouble(totalAbsentLeaveDays) * 0.5)).ToString() + " / " + totalExtraAbsent;
                                    sheet1.Range[4, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[4, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[4, 10, 4, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[4, 10, 4, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[4, 10, 4, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    ///xlsRow += 1;
                                    sheet1.Range[5, 10].Text = "Total Weekoffs";
                                    sheet1.Range[5, 10, 5, 10 + 1].Merge();

                                    sheet1.Range[5, 10 + 2].Text = totalWeekOFFDays.ToString();
                                    sheet1.Range[5, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[5, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[5, 10, 5, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[5, 10, 5, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[5, 10, 5, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //xlsRow += 1;
                                    sheet1.Range[6, 10].Text = "Late";
                                    sheet1.Range[6, 10, 6, 10 + 1].Merge();

                                    sheet1.Range[6, 10 + 2].Text = totalLateDays.ToString();
                                    sheet1.Range[6, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[6, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[6, 10, 6, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[6, 10, 6, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[6, 10, 6, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //xlsRow += 1;
                                    sheet1.Range[7, 10].Text = "Holidays";
                                    sheet1.Range[7, 10, 7, 10 + 1].Merge();

                                    sheet1.Range[7, 10 + 2].Text = totalHolidays.ToString();
                                    sheet1.Range[7, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[7, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[7, 10, 7, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[7, 10, 7, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[7, 10, 7, 10 + 2].BorderAround(ExcelLineStyle.Hair);

                                    if (chkAdditionInfo == true)
                                    {
                                        //xlsRow += 1;
                                        sheet1.Range[8, 10].Text = "ODD(Hours):";
                                        sheet1.Range[8, 10, 8, 10 + 1].Merge();


                                        string zot = string.Empty;
                                        oru.GetOT(dsBioDvAC.Tables[0].Rows[i]["OTConsiderOn"].ToString(), totalODD.ToString(), out zot);
                                        sheet1.Range[8, 10 + 2].Text = zot;

                                        sheet1.Range[8, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                        sheet1.Range[8, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[8, 10, 8, 10 + 2].CellStyle.Font.Bold = true;
                                        sheet1.Range[8, 10, 8, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        sheet1.Range[8, 10, 8, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    
                                        //xlsRow += 1;
                                        sheet1.Range[9, 10].Text = "Total OT Hour";
                                        sheet1.Range[9, 10, 9, 10 + 1].Merge();

                                    }

                                    // ----End Total-- -
                                    #endregion

                                    xlsRow = 11;
                                    xlsCol = 1;
                                    iDate = xlsCol;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, iDate].Text = "Date";
                                    sheet1.Range[xlsRow, iDate].ColumnWidth = 11;
                                    sheet1.Range[xlsRow, iDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iDay = xlsCol;
                                    sheet1.Range[xlsRow, iDay].Text = "Day";
                                    sheet1.Range[xlsRow, iDay].ColumnWidth = 5;
                                    sheet1.Range[xlsRow, iDay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDay].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    xlsCol += 1;
                                    iShiftName = xlsCol;
                                    sheet1.Range[xlsRow, iShiftName].Text = "Shift Name";
                                    sheet1.Range[xlsRow, iShiftName].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, iShiftName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iShiftName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iShiftIntime = xlsCol;
                                    sheet1.Range[xlsRow, iShiftIntime].Text = "Shift InTime";
                                    sheet1.Range[xlsRow, iShiftIntime].ColumnWidth = 8;
                                    sheet1.Range[xlsRow, iShiftIntime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iShiftIntime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iShiftOuttime = xlsCol;
                                    sheet1.Range[xlsRow, iShiftOuttime].Text = "Shift OutTime";
                                    sheet1.Range[xlsRow, iShiftOuttime].ColumnWidth = 9;
                                    sheet1.Range[xlsRow, iShiftOuttime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iShiftOuttime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                     xlsCol += 1;
                                     iInTime = xlsCol;
                                     sheet1.Range[xlsRow, iInTime].Text = "InTime";
                                     sheet1.Range[xlsRow, iInTime].ColumnWidth = 8;
                                     sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                     sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                     xlsCol += 1;
                                     iOutTime = xlsCol;
                                     sheet1.Range[xlsRow, iOutTime].Text = "OutTime";
                                     sheet1.Range[xlsRow, iOutTime].ColumnWidth = 8;
                                     sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                     sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                 
                                   
                                                                        
                                    xlsCol += 1;
                                    iDayStatus = xlsCol;
                                    sheet1.Range[xlsRow, iDayStatus].Text = "Day Status";
                                    sheet1.Range[xlsRow, iDayStatus].ColumnWidth = 6.5;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iLateBy = xlsCol;
                                    sheet1.Range[xlsRow, iLateBy].Text = "Late By";
                                    sheet1.Range[xlsRow, iLateBy].ColumnWidth = 9.5;
                                    sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    xlsCol += 1;
                                    iLvShortName = xlsCol;
                                    sheet1.Range[xlsRow, iLvShortName].Text = "LV";
                                    sheet1.Range[xlsRow, iLvShortName].ColumnWidth = 9;
                                    sheet1.Range[xlsRow, iLvShortName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iLvShortName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    if (chkAdditionInfo == true)
                                    {
                                        xlsCol += 1;
                                        iODD = xlsCol;
                                        sheet1.Range[xlsRow, iODD].Text = "ODD";
                                        sheet1.Range[xlsRow, iODD].ColumnWidth = 9;
                                        sheet1.Range[xlsRow, iODD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iODD].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    }

                                    if (chkAdditionInfo == true)
                                    {
                                        xlsCol += 1;
                                        iOverStay = xlsCol;
                                        sheet1.Range[xlsRow, iOverStay].Text = "Over Stay";
                                        sheet1.Range[xlsRow, iOverStay].ColumnWidth = 9;
                                        sheet1.Range[xlsRow, iOverStay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iOverStay].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    }
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    endXlsCol = xlsCol;

                                    freezeRow = xlsRow.ToString();
                                    #endregion ------------------Column Header------------------
                                }
                                strEmpCode = dvBioDvAC[i]["EmployeeCode"].ToString().Trim();

                                #region ----------------------Data-----------------------

                                xlsRow += 1;
                                sheet1.Range[xlsRow, iDate].Text = dvBioDvAC[i]["PDate"].ToString();
                                sheet1.Range[xlsRow, iDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iShiftName].Text = dvBioDvAC[i]["ShiftName"].ToString();
                                sheet1.Range[xlsRow, iShiftName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iShiftName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (!string.IsNullOrEmpty(dvBioDvAC[i]["ShiftInTimeShow"].ToString()))
                                {
                                    sheet1.Range[xlsRow, iShiftIntime].Text = dvBioDvAC[i]["ShiftInTimeShow"].ToString(); 
                                }
                                sheet1.Range[xlsRow, iShiftIntime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iShiftIntime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iShiftOuttime].Text = dvBioDvAC[i]["ShiftOutTime"].ToString();
                                sheet1.Range[xlsRow, iShiftOuttime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iShiftOuttime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                //if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTEntitle"]))
                                //{
                                //    continue;
                                //}
                                //if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsNoPunchOnHolidayForOTEntitle"]))
                                //{
                                //    continue;
                                //}

                                if (chkAdditionInfo == true)
                                {
                                    if (!string.IsNullOrEmpty(dvBioDvAC[i]["DurationInMin"].ToString()))
                                    {
                                        string yot = string.Empty;
                                        oru.GetOT(dsBioDvAC.Tables[0].Rows[i]["OTConsiderOn"].ToString(), dsBioDvAC.Tables[0].Rows[i]["DurationInMin"].ToString(), out yot);

                                        sheet1.Range[xlsRow, iODD].Text = yot;
                                        sheet1.Range[xlsRow, iODD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iODD].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }

                                }
                                if (dvBioDvAC[i]["PDate"].ToString() == "11-Dec-2020")
                                {

                                }
                                sheet1.Range[xlsRow, iDay].Text = dvBioDvAC[i]["PDay"].ToString().Substring(0, 3);
                                sheet1.Range[xlsRow, iDay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iDay].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                {
                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "CW";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                    }
                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }
                                else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                {



                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "CW";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                    }
                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                }
                                else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                {
                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    
                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "CW";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                    }
                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                }
                                else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                {
                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "CW";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                    }
                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                }

                                else if (dvBioDvAC[i]["DayStatus"].ToString().Trim().Contains("LV") || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W")
                                {
                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "CW"; 
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                    }
                                    //sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["OriginalDayType"].ToString().Trim();
                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }
                                

                                else
                                {
                                    if (dvBioDvAC[i]["InTimeShow"].ToString() != "")
                                    {
                                        sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                        sheet1.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dvBioDvAC[i]["InTimeShow"].ToString());
                                        sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }
                                    if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsManualInTime"].ToString().Trim()))
                                    {
                                        sheet1.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Dark_blue;
                                    }
                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim().Contains("LV"))
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "LV";
                                    }
                                    else
                                    {
                                        if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "L")
                                        {
                                            sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Dark_blue;
                                            sheet1.Range[xlsRow, iDayStatus].Text = "L";
                                        }
                                        else
                                        {

                                            if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                            {
                                                sheet1.Range[xlsRow, iDayStatus].Text = "CW";
                                            }
                                            else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                            {
                                                sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                            }
                                            else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                            {
                                                sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                            }
                                            else
                                            {
                                                sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                            }
                                        }
                                    }

                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                }
                                if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CW")
                                {
                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "CW";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                    }

                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }
                                if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                {
                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }
                                else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                {
                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }
                                else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                {
                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter; ;
                                }
                                else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                {
                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }

                                else if (dvBioDvAC[i]["DayStatus"].ToString().Trim().Contains("LV") || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                {
                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                }
                                else
                                {

                                    if (dvBioDvAC[i]["OutTimeShow"].ToString() != "")
                                    {

                                        DateTime NewRealOutTime;
                                        string TakeDate = Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                        string ot = Convert.ToDateTime(dvBioDvAC[i]["ShiftOutTime"].ToString().Trim()).ToString("hh:mm tt");

                                        //check night shift
                                        string _sOUTtime = TakeDate + " " + ot;
                                        string _sINtime = TakeDate + " " + Convert.ToDateTime(dvBioDvAC[i]["ShiftInTime"].ToString().Trim()).ToString("hh:mm tt");
                                        if (Convert.ToDateTime(_sOUTtime) < Convert.ToDateTime(_sINtime))
                                        {
                                            TakeDate = Convert.ToDateTime(TakeDate).AddDays(1).ToString("dd-MMM-yyyy");
                                        }

                                        string TateandTime = TakeDate + " " + ot;
                                        int minutesadd = Convert.ToInt32(dvBioDvAC[i]["MaxOTPerDay"].ToString().Trim());
                                        DateTime NewOutTime = Convert.ToDateTime(TateandTime).AddMinutes(minutesadd);
                                        DateTime RealOutTime = Convert.ToDateTime(dvBioDvAC[i]["OutTimeShow"].ToString().Trim());

                                        if (Convert.ToDateTime(RealOutTime) > Convert.ToDateTime(NewOutTime))
                                        {
                                            //long WorkDateTickCount = Convert.ToDateTime(Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString()).ToString("dd-MMM-yyyy")).Ticks;
                                            //int EmployeeSystemId = (int)Convert.ToInt64(dvBioDvAC[i]["SystemId"].ToString());

                                            long WorkDateTickCount = Convert.ToInt64(Convert.ToDateTime(dvBioDvAC[i]["WDate"].ToString()).ToString("yyMMddHHmmss"));
                                            int EmployeeSystemId = (int)Convert.ToInt64(dvBioDvAC[i]["EmployeeCodeNumeric"].ToString());

                                            WorkDateTickCount += EmployeeSystemId;

                                            Random rnd = new Random((int)(WorkDateTickCount));
                                            int RandomMinutes = rnd.Next(0, 15);
                                            NewRealOutTime = Convert.ToDateTime(NewOutTime).AddMinutes(RandomMinutes);
                                        }

                                        else
                                        {
                                            NewRealOutTime = Convert.ToDateTime(dvBioDvAC[i]["OutTimeShow"].ToString().Trim());
                                        }
                                        DateTime RandomTime = Convert.ToDateTime(NewRealOutTime);
                                        DateTime ShiftTime = Convert.ToDateTime(TateandTime);
                                        TimeSpan span = RandomTime - ShiftTime;
                                        double totalMinutes = span.TotalMinutes;

                                        sheet1.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                        sheet1.Range[xlsRow, iOutTime].DateTime = NewRealOutTime;
                                        sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }

                                    if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsManualOutTime"].ToString().Trim()))
                                    {
                                        sheet1.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Dark_blue;
                                    }
                                }


                                if (dvBioDvAC[i]["DayStatus"].ToString() == "W" || dvBioDvAC[i]["DayStatus"].ToString() == "H" || dvBioDvAC[i]["DayStatus"].ToString() == "LV" || dvBioDvAC[i]["DayStatus"].ToString() == "CW" || dvBioDvAC[i]["DayStatus"].ToString() == "A" || dvBioDvAC[i]["DayStatus"].ToString() == "AH" || dvBioDvAC[i]["DayStatus"].ToString() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString() == "WP" || dvBioDvAC[i]["DayStatus"].ToString() == "WL" || dvBioDvAC[i]["DayStatus"].ToString() == "CWL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                {
                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].Text = "";

                                }
                                else
                                {
                                    sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                    if (!string.IsNullOrEmpty(dvBioDvAC[i]["InTimeShow"].ToString()))
                                    {
                                        sheet1.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dvBioDvAC[i]["InTimeShow"].ToString()); 
                                    }

                                    if (dvBioDvAC[i]["OutTimeShow"].ToString() != "")
                                    {

                                        DateTime NewRealOutTime;
                                        string TakeDate = Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                        string ot = Convert.ToDateTime(dvBioDvAC[i]["ShiftOutTime"].ToString().Trim()).ToString("hh:mm tt");

                                        //check night shift
                                        string _sOUTtime = TakeDate + " " + ot;
                                        string _sINtime = TakeDate + " " + Convert.ToDateTime(dvBioDvAC[i]["ShiftInTime"].ToString().Trim()).ToString("hh:mm tt");
                                        if (Convert.ToDateTime(_sOUTtime) < Convert.ToDateTime(_sINtime))
                                        {
                                            TakeDate = Convert.ToDateTime(TakeDate).AddDays(1).ToString("dd-MMM-yyyy");
                                        }

                                        string TateandTime = TakeDate + " " + ot;
                                        int minutesadd = Convert.ToInt32(dvBioDvAC[i]["MaxOTPerDay"].ToString().Trim());
                                        DateTime NewOutTime = Convert.ToDateTime(TateandTime).AddMinutes(minutesadd);
                                        DateTime RealOutTime = Convert.ToDateTime(dvBioDvAC[i]["OutTimeShow"].ToString().Trim());

                                        if (Convert.ToDateTime(RealOutTime) > Convert.ToDateTime(NewOutTime))
                                        {
                                            //long WorkDateTickCount = Convert.ToDateTime(Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString()).ToString("dd-MMM-yyyy")).Ticks;
                                            //int EmployeeSystemId = (int)Convert.ToInt64(dvBioDvAC[i]["SystemId"].ToString());

                                            long WorkDateTickCount = Convert.ToInt64(Convert.ToDateTime(dvBioDvAC[i]["WDate"].ToString()).ToString("yyMMddHHmmss"));
                                            int EmployeeSystemId = (int)Convert.ToInt64(dvBioDvAC[i]["EmployeeCodeNumeric"].ToString());

                                            WorkDateTickCount += EmployeeSystemId;

                                            Random rnd = new Random((int)(WorkDateTickCount));
                                            int RandomMinutes = rnd.Next(0, 15);
                                            NewRealOutTime = Convert.ToDateTime(NewOutTime).AddMinutes(RandomMinutes);
                                        }

                                        else
                                        {
                                            NewRealOutTime = Convert.ToDateTime(dvBioDvAC[i]["OutTimeShow"].ToString().Trim());
                                        }
                                        DateTime RandomTime = Convert.ToDateTime(NewRealOutTime);
                                        DateTime ShiftTime = Convert.ToDateTime(TateandTime);
                                        TimeSpan span = RandomTime - ShiftTime;
                                        double totalMinutes = span.TotalMinutes;

                                        sheet1.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                        sheet1.Range[xlsRow, iOutTime].DateTime = NewRealOutTime;
                                        sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }

                                }
                                sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                var Date = dvBioDvAC[i]["PDate"].ToString().Trim();
                                var EmpCode = dvBioDvAC[i]["EmployeeCode"].ToString().Trim();

                                dvWeeklyAbsnt.RowFilter = "EmployeeCode = '" + EmpCode + "' AND  WorkingDate = '" + Date + "'";


                                if (dvWeeklyAbsnt.Count > 0)
                                {
                                    sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Red;
                                }

                                #region Extra Absent Colore

                                dvExtraAbsentDate.RowFilter = "EmployeeCode = '" + EmpCode + "' AND  WorkingDate = '" + Date + "'";

                                bool IsExtraAbsent = false;
                                if (dvExtraAbsentDate.Count > 0)
                                {
                                    IsExtraAbsent = true;
                                }
                                if (IsExtraAbsent)
                                {
                                    sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Red;
                                    sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Bold = true;

                                }
                                #endregion

                                if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsManualDayStatus"].ToString().Trim()))
                                {
                                    sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Dark_blue;

                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    sheet1.Range[xlsRow, iLateBy].Text = "";
                                    sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }

                                if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "L")
                                {
                                    #region Late by min                                  
                                    sInTime = "00:00:00";
                                    if (dvBioDvAC[i]["InTimelate"].ToString().Trim() != "")
                                    {
                                        sInTime = dvBioDvAC[i]["InTimelate"].ToString().Trim() + ":00";
                                    }
                                    else
                                    {
                                        if (dvBioDvAC[i]["OutTimelate"].ToString().Trim() != "")
                                        {
                                            sInTime = dvBioDvAC[i]["OutTimelate"].ToString().Trim() + ":00";
                                        }
                                    }
                                    sOfficeInTime = "00:00:00";
                                    strLateBy = "00:00";
                                    if (dvBioDvAC[i]["ShiftInTimeLate"].ToString().Trim() != "" && sInTime != "00:00:00")
                                    {
                                        sOfficeInTime = dvBioDvAC[i]["ShiftInTimeLate"].ToString().Trim();
                                        strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                                    }

                                    #endregion Late by min
                                }
                                else
                                {
                                    ///absent by how min

                                    #region Absent by how much min

                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "A")
                                    {
                                        sInTime = "00:00:00";
                                        if (dvBioDvAC[i]["InTimelate"].ToString().Trim() != "")
                                        {
                                            sInTime = dvBioDvAC[i]["InTimelate"].ToString().Trim() + ":00";
                                            sOfficeInTime = "00:00:00";
                                            strLateBy = "00:00";
                                            if (dvBioDvAC[i]["ShiftInTimeLate"].ToString().Trim() != "" && sInTime != "00:00:00")
                                            {
                                                sOfficeInTime = dvBioDvAC[i]["ShiftInTimeLate"].ToString().Trim();
                                                strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                                            }
                                        }
                                        else
                                        {
                                            strLateBy = "";
                                        }
                                    }
                                    else
                                    {
                                        strLateBy = "";
                                    }

                                    #endregion Absent by how much min
                                }

                                //paid days

                                DateTime _ddd = Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString());

                                string dti = dvBioDvAC[i]["dti"].ToString().Trim();
                                string dto = dvBioDvAC[i]["dto"].ToString().Trim();
                                string _InTimeShow = dvBioDvAC[i]["InTimeShow"].ToString().Trim();
                                string _OutTimeShow = dvBioDvAC[i]["OutTimeShow"].ToString().Trim();

                                sheet1.Range[xlsRow, iLateBy].Text = strLateBy;
                                sheet1.Range[xlsRow, iLateBy].CellStyle.Font.Color = ExcelKnownColors.Dark_blue;
                                sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (!string.IsNullOrEmpty(dvBioDvAC[i]["Code"].ToString()))
                                {
                                    sheet1.Range[xlsRow, iLvShortName].Text = dvBioDvAC[i]["Code"].ToString() + "(" + dvBioDvAC[i]["LeaveDuration"].ToString() + ")";
                                    sheet1.Range[xlsRow, iLvShortName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iLvShortName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }

                                var sl = dvBioDvAC[i]["ShortLeave"].ToString();
                                if (sl == "0")
                                {
                                    sl = null;
                                }

                                if (chkAdditionInfo == true)
                                {
                                    string yot = string.Empty;//OTConsiderOn
                                    string overstay = string.Empty;
                                    if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsOTEntitled"].ToString()) == true)
                                    {

                                        if (!string.IsNullOrEmpty(dvBioDvAC[i]["DayCategory"].ToString()))
                                        {
                                            if (dvBioDvAC[i]["DayCategory"].ToString() == "Present" || dvBioDvAC[i]["DayCategory"].ToString() == "Late")
                                            {

                                                if (dvBioDvAC[i]["OutTimeShow"].ToString() != "")
                                                {
                                                    DateTime NewRealOutTime;
                                                    string TakeDate = Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                                    string ot = Convert.ToDateTime(dvBioDvAC[i]["ShiftOutTime"].ToString().Trim()).ToString("hh:mm tt");

                                                    //check night shift
                                                    string _sOUTtime = TakeDate + " " + ot;
                                                    string _sINtime = TakeDate + " " + Convert.ToDateTime(dvBioDvAC[i]["ShiftInTime"].ToString().Trim()).ToString("hh:mm tt");
                                                    if (Convert.ToDateTime(_sOUTtime) < Convert.ToDateTime(_sINtime))
                                                    {
                                                        TakeDate = Convert.ToDateTime(TakeDate).AddDays(1).ToString("dd-MMM-yyyy");
                                                    }

                                                    string TateandTime = TakeDate + " " + ot;
                                                    int minutesadd = Convert.ToInt32(dvBioDvAC[i]["MaxOTPerDay"].ToString().Trim());
                                                    DateTime NewOutTime = Convert.ToDateTime(TateandTime).AddMinutes(minutesadd);
                                                    DateTime RealOutTime = Convert.ToDateTime(dvBioDvAC[i]["OutTimeShow"].ToString().Trim());
                                                    double totalMinutes;

                                                    if (Convert.ToDateTime(RealOutTime) > Convert.ToDateTime(NewOutTime) && (dvBioDvAC[i]["OriginalDayType"].ToString() != "H" && dvBioDvAC[i]["OriginalDayType"].ToString() != "W"))
                                                    {
                                                        long WorkDateTickCount = Convert.ToDateTime(Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString()).ToString("dd-MMM-yyyy")).Ticks;
                                                        int EmployeeSystemId = (int)Convert.ToInt64(dvBioDvAC[i]["SystemId"].ToString());
                                                        WorkDateTickCount += EmployeeSystemId;

                                                        Random rnd = new Random((int)(WorkDateTickCount));
                                                        int RandomMinutes = rnd.Next(0, 15);
                                                        NewRealOutTime = Convert.ToDateTime(NewOutTime).AddMinutes(RandomMinutes);
                                                        DateTime RandomTime = Convert.ToDateTime(NewRealOutTime);
                                                        DateTime ShiftTime = Convert.ToDateTime(TateandTime);
                                                        TimeSpan span = RandomTime - ShiftTime;
                                                        totalMinutes = span.TotalMinutes;
                                                        oru.GetOT(dsBioDvAC.Tables[0].Rows[0]["OTConsiderOn"].ToString(), minutesadd.ToString(), out overstay);
                                                        if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                                        {
                                                            overstay = "";
                                                            OTOverstay1 += 0.00;

                                                        }
                                                        else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                                        {
                                                            overstay = "";
                                                            OTOverstay1 += 0.00;


                                                        }
                                                        else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                                        {
                                                            overstay = "";
                                                            OTOverstay1 += 0.00;


                                                        }
                                                        else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                                        {
                                                            overstay = "";
                                                            OTOverstay1 += 0.00;

                                                        }

                                                        else if (dvBioDvAC[i]["DayStatus"].ToString().Trim().Contains("LV") || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CW" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                                        {
                                                            OTOverstay1 += 0.00;
                                                        }
                                                        else
                                                        {
                                                            OTOverstay1 += clsStaticInfo.dbl(minutesadd);

                                                        }

                                                    }
                                                    else
                                                    {
                                                        NewRealOutTime = Convert.ToDateTime(dvBioDvAC[i]["OutTimeShow"].ToString().Trim());
                                                        oru.GetOT(dsBioDvAC.Tables[0].Rows[0]["OTConsiderOn"].ToString(), dvBioDvAC[i]["OverStay"].ToString(), out overstay);
                                                        if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                                        {
                                                            overstay = "";
                                                            OTOverstay2 += 0.00;

                                                        }
                                                        else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                                        {
                                                            overstay = "";
                                                            OTOverstay2 += 0.00;


                                                        }
                                                        else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                                        {
                                                            overstay = "";
                                                            OTOverstay2 += 0.00;


                                                        }
                                                        else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                                        {
                                                            overstay = "";
                                                            OTOverstay2 += 0.00;

                                                        }

                                                        else if (dvBioDvAC[i]["DayStatus"].ToString().Trim().Contains("LV") || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                                        {
                                                            OTOverstay2 += 0.00;
                                                        }
                                                        else
                                                        {
                                                            OTOverstay2 += clsStaticInfo.dbl(dvBioDvAC[i]["OverStay"].ToString());


                                                        }


                                                    }

                                                }
                                            }
                                        }
                                    }

                                    if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                    {
                                        overstay = "";


                                    }
                                    else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                    {
                                        overstay = "";



                                    }
                                    else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                    {
                                        overstay = "";



                                    }
                                    else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                    {
                                        overstay = "";


                                    }

                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim().Contains("LV") || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        overstay = "";

                                    }




                                    sheet1.Range[xlsRow, iOverStay].Text = overstay;
                                    sheet1.Range[xlsRow, iOverStay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOverStay].VerticalAlignment = ExcelVAlign.VAlignCenter;






                                }

                                #endregion ----------------------Data-----------------------
                                #region Line Setup

                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

                                #endregion Line Setup
                            }
                            xlsRow += 1;
                            OTOverstay += clsStaticInfo.dbl(OTOverstay1 + OTOverstay2);

                            if (chkAdditionInfo == true)
                            {
                                string GTotalOt = string.Empty;
                                oru.GetOT(dsBioDvAC.Tables[0].Rows[0]["OTConsiderOn"].ToString(), OTOverstay.ToString(), out GTotalOt);
                                sheet1.Range[xlsRow, iODD + 1].Text = GTotalOt;
                                sheet1.Range[xlsRow, iODD + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iODD + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iODD].Text = "Total ";
                                sheet1.Range[xlsRow, iODD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iODD].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, iODD, xlsRow, iODD + 1].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, iODD, xlsRow, iODD + 1].BorderInside(ExcelLineStyle.Hair);

                                sheet1.Range[9, 10 + 2].Text = GTotalOt;
                                sheet1.Range[9, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[9, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[9, 10, 9, 10 + 2].CellStyle.Font.Bold = true;
                                sheet1.Range[9, 10, 9, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[9, 10, 9, 10 + 2].BorderAround(ExcelLineStyle.Hair);

                            }

                            xlsRow += 3;
                            xlsRow += 5;

                            sheet1.Range[xlsRow, iDate].Text = employeeName;
                            sheet1.Range[xlsRow, iDate, xlsRow, iShiftName].Merge();
                            sheet1.Range[xlsRow, iDate, xlsRow, iShiftName].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, iDate, xlsRow, iShiftName].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thick;

                            sheet1.Range[xlsRow, iDate, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iDate, xlsRow, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.IsDisplayZeros = false;

                            #region ******************Report Header******************
                            try
                            {
                                string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                                Image companyLogo = Image.FromFile(strPath);
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
                            catch (Exception)
                            {


                            }
                            xlsRow = 1;
                            xlsCol = 1;


                            FactoryName = string.Empty;

                            string FactoryAddress = string.Empty;

                            if (dsCmp.Tables[0].Rows.Count > 0)
                            {
                                CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                            }
                            else
                            {
                                CmpName = "";
                            }
                            sheet1.Range[xlsRow, 3].Text = CmpName;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 17;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 20;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                            xlsRow += 1;
                            if (dsFactory.Tables[0].Rows.Count > 0)
                            {
                                //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                                FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                            }
                            else
                            {
                                FactoryName = "";
                            }
                            sheet1.Range[xlsRow, 3].Text = FactoryName;
                            //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 25;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                            xlsRow += 1;
                            if (dsFactory.Tables[0].Rows.Count > 0)
                            {
                                FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                            }
                            else
                            {
                                FactoryAddress = "";
                            }
                            sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 15;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                            xlsRow += 1;
                            sheet1.Range[xlsRow, 3].Text = "Employee Job Card Information From Date: " + fromDate + " To Date: " + toDate;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 20;
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                            #endregion ******************Report Header******************

                            #region Freeze Panes
                            if (chkAdditionInfo == true)
                            {
                                sheet1.IsDisplayZeros = false;
                                sheet1.UsedRange["A13"].FreezePanes();
                            }
                            else
                            {
                                sheet1.IsDisplayZeros = false;
                                sheet1.UsedRange["A13"].FreezePanes();
                            }

                            #endregion Freeze Panes

                            #region UsedRange Alignment

                            sheet1.UsedRange.WrapText = true;
                            sheet1.UsedRange.CellStyle.Font.Size = 8;
                            sheet1.Range["A1"].CellStyle.Font.Size = 14;
                            sheet1.Range["A2"].CellStyle.Font.Size = 10;
                            sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                            #endregion UsedRange Alignment

                            #region Page Setup
                            sheet1.PageSetup.TopMargin = 0.5;
                            sheet1.PageSetup.BottomMargin = 0.7;
                            sheet1.PageSetup.PrintTitleRows = "$1:$11";
                            sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                            sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                            sheet1.PageSetup.LeftMargin = 0.5;
                            sheet1.PageSetup.RightMargin = 0.2;
                            sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                            sheet1.PageSetup.FitToPagesTall = 0;
                            sheet1.PageSetup.FitToPagesWide = 1;
                            sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                            sheet1.IsDisplayZeros = false;

                            sheet1.Name = sEmpCodeColl[Ec].ToString().Trim();

                            #endregion Page Setup

                        }

                    }

                    return workbook;
                }
                else
                {
                    Exception ex = new Exception("No data found...");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                dsBioDvAC = null;
                dvBioDvAC = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }


        }

        [HttpGet, Authorize]
        public ActionResult GetBuyerComplianceJobCardReport(ReportFormat reportFormat, string[] employeeId, string fromDate, string toDate, bool chkAdditionInfo)
        {
            try
            {
                string EmpIdLoop = "";
                foreach (string item in employeeId)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "" + item + ""; ;
                    }

                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = GetBuyerComplianceJobCardReport(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, EmpIdLoop, fromDate, toDate, chkAdditionInfo);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        PdfDocument document = new PdfDocument();
                        ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings();
                        settings.TemplateDocument = document;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document = converter1.Convert(settings);
                        }
                        document.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
                        return null;
                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        public IWorkbook GetBuyerComplianceJobCardReport(string username, string companyGroupId, string companyId, string plantId, string plantName, string EmpIdLoop, string fromDate, string toDate, bool chkAdditionInfo)
        {

            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsBioDvAC = null;
            DataTable dtBioDvAC = null;
            DataView dvBioDvAC = null;
            DataView dvSummary = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsMonthlySummary = null;

            DataSet dsPayDays = null;
            DataTable dtMonthlySummary = null;
            DataView dvPayDays = null;

            StringCollection sEmpCodeColl = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            var workbook = oru.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            IWorksheet sheet1 = null;

            DataSet dsWeeklyAbsnt = null;
            DataTable dtWeeklyAbsnt = null;
            DataView dvWeeklyAbsnt = null;
            DataSet dsValidation = null;
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string sOfficeInTime = "00:00";
            string sInTime = "00:00";
            string freezeRow = "";
            int StartRow = xlsRow;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                #region Validation
                if (string.IsNullOrEmpty(fromDate) == true || bplib.clsWebLib.IsDateOK(fromDate) == false)
                {

                    Exception ex = new Exception("Please define access From Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }
                if (string.IsNullOrEmpty(fromDate) == true || bplib.clsWebLib.IsDateOK(fromDate) == false)
                {

                    Exception ex = new Exception("Please define access To Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }
                DateTime dtFrmDate = bplib.clsWebLib.DateData_DBToApp(fromDate, bplib.clsWebLib.DB_DATE_FORMAT);
                DateTime dtToDate = bplib.clsWebLib.DateData_DBToApp(fromDate, bplib.clsWebLib.DB_DATE_FORMAT);
                TimeSpan tsFromToDate = dtToDate - dtFrmDate;
                int daysFromTo = tsFromToDate.Days;
                if (daysFromTo < 0)
                {
                    Exception ex = new Exception("Please check the access From Date, cannot more than access To Date...");
                    throw (ex);
                }

                //string sql1 = "select * From ComplianceAttendanceSetting where PlantId ='" + identity.PlantId + @"' ";
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql1, out dsValidation, false, "1");
                //if (dsValidation.Tables[0].Rows.Count < 1)
                //{
                //    Exception ex = new Exception("OT Settings are incomplete .");
                //    throw (ex);
                //}
                #endregion Validation

                objRpt = new clsReport();
                dvPayDays = new DataView();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                #region DataSet

                GetEmpJobCardInfoWithInDateTimes(EmpIdLoop, fromDate, toDate, plantId, out dsBioDvAC);
                dtBioDvAC = dsBioDvAC.Tables[0];

                GetEmpJobCardMonthlySummary(EmpIdLoop, fromDate, toDate, out dsMonthlySummary);
                dtMonthlySummary = dsMonthlySummary.Tables[0];

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                DataView dvExtraAbsentDate = null;
                GetExtraAbsentCount(fromDate, toDate, plantId, out dsExtraAbsent);
                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);
                dvExtraAbsentDate = new DataView(dsExtraAbsent.Tables[0]);

                GetJobCardPayDays(EmpIdLoop, fromDate, toDate, out dsPayDays);
                var ListPayDays = dsPayDays.Tables[0].ToList<PayDaysReport>();
                ParaMontlyAttendance objm = new ParaMontlyAttendance();
                dvWeeklyAbsnt = new DataView();
                GetWeeklyAbsentismAssignment(plantId, EmpIdLoop, fromDate, toDate, out dsWeeklyAbsnt);
                dtWeeklyAbsnt = dsWeeklyAbsnt.Tables[0];
                dvWeeklyAbsnt.Table = dtWeeklyAbsnt;
                SelectedPlantWiseCompany(plantId, out dsCmp);
                SelectedPlant(plantId, out dsFactory);
                #endregion DataSet

                if (dsBioDvAC.Tables[0].Rows.Count > 0)
                {
                    sEmpCodeColl = new StringCollection();
                    for (int i = 0; i <= dsBioDvAC.Tables[0].Rows.Count - 1; i++)
                    {
                        if (sEmpCodeColl.Contains(dsBioDvAC.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim()) == false)
                        {
                            sEmpCodeColl.Add(dsBioDvAC.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim());
                        }
                    }

                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;
                    workbook = application.Workbooks.Create(sEmpCodeColl.Count);
                    for (int Ec = 0; Ec < sEmpCodeColl.Count; Ec++)
                    {
                        dvBioDvAC = new DataView();
                        dvBioDvAC.Table = dtBioDvAC;

                        dvSummary = new DataView();
                        dvSummary.Table = dtMonthlySummary;

                        dvBioDvAC.RowFilter = "EmployeeCode = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";
                        dvSummary.RowFilter = "EmployeeCode = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";
                        dvExtraAbsent.RowFilter = "EmployeeCode = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";

                        if (dvBioDvAC.Count > 0)
                        {
                            sheet1 = workbook.Worksheets[Ec];
                            sheet1.IsGridLinesVisible = true;
                            xlsRow = 6;
                            string strEmpCode = "";
                            int iDate = 0;
                            int iShiftIntime = 0;
                            int iInTime = 0;
                            int iOutTime = 0;
                            int iTotalOT = 0;
                            int iDayStatus = 0;
                            int iODD = 0;
                            int iLvShortName = 0;
                            string strLateBy = "00:00:00";
                            int iLateBy = 0;
                            int iShiftName = 0;
                            int iShiftOuttime = 0;
                            var iDay = 0;
                            var iOverStay = 0;
                            var OTtotal = 0.00;
                            var OTOverstay = 0.00;
                            var OTOverstayNew = 0.00;
                            var OTOverstay1 = 0.00;
                            var OTOverstay2 = 0.00;
                            var total = 0.00;
                            string employeeName = "";
                            object chequeAmount;
                            object OverStay;
                            object totalPresentDays;
                            object totalAbsentDays;
                            object totalLateDays;
                            object totalLeaveDays;
                            object totalWeekOFFDays;
                            object totalHolidays;
                            object totalODD;
                            object totalDays;
                            object totalHalfDays;
                            object totalHalfDaysLeave;
                            object totalLeaveAbsentDays;
                            object totalAbsentLeaveDays;
                            object totalExtraAbsent;
                            object toTotalLWP;

                            chequeAmount = dvBioDvAC.ToTable().Compute(@"Sum(FinalOT)", "");
                            OverStay = dvBioDvAC.ToTable().Compute(@"Sum(OverStay)", "");
                            totalPresentDays = dvSummary.ToTable().Compute(@"Sum(TotalPresent)", null);
                            totalHalfDays = 0;
                            totalHalfDaysLeave = 0;
                            totalLeaveAbsentDays = 0;
                            totalAbsentLeaveDays = 0;
                            totalAbsentDays = dvSummary.ToTable().Compute(@"SUM(TotalAbsent)", null);
                            totalExtraAbsent = dvExtraAbsent.ToTable().Compute(@"Count(WorkingDate)", null);
                            totalLateDays = dvSummary.ToTable().Compute(@"SUM(TotalLate)", null);
                            totalLeaveDays = dvSummary.ToTable().Compute(@"SUM(TotalLv)", null);
                            totalWeekOFFDays = dvSummary.ToTable().Compute(@"SUM(TotalWeekOff)", null);
                            totalHolidays = dvSummary.ToTable().Compute(@"SUM(TotalHoliDay)", null);
                            totalODD = dvBioDvAC.ToTable().Compute(@"SUM(DurationInMin)", null);
                            totalDays = dvSummary.ToTable().Compute(@"COUNT(DayValue)", null);
                            toTotalLWP = dvSummary.ToTable().Compute(@"SUM(TotalLWP)", null);

                            for (int i = 0; i < dvBioDvAC.Count; i++)
                            {


                                if ((string.Compare(strEmpCode.ToUpper(), dvBioDvAC[i]["EmployeeCode"].ToString().Trim().ToUpper())) != 0)
                                {

                                    #region ------------------Column Header------------------
                                    employeeName = dvBioDvAC[i]["EmployeeName"].ToString().Trim();
                                    xlsCol = 1;
                                    xlsRow = 5;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Emp Code";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["EmployeeCode"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Text = "Emp Name";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["EmployeeName"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "DOJ";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["DOJ"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Designation";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["LegalDesignation"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Grade";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["GradeCode"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Current Status";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["EmployeeStatus"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    xlsCol = 5;
                                    xlsRow = 5;

                                    sheet1.Range[xlsRow, xlsCol].Text = "Unit";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Unit"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;

                                    sheet1.Range[xlsRow, xlsCol].Text = "Division";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Division"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Department";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Department"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Section";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Section"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "SubSection";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["SubSection"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    xlsRow = 5;
                                    xlsCol = 9;

                                    #region Total
                                    //-----Total------
                                    sheet1.Range[1, 10].Text = "Job Card Summary";
                                    sheet1.Range[1, 10, 1, 10 + 2].Merge();

                                    sheet1.Range[1, 10, 1, 9 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[1, 10, 1, 9 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[1, 10, 1, 9 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[1, 10, 1, 9 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //1 += 1;

                                    sheet1.Range[2, 10].Text = "Present Days";
                                    sheet1.Range[2, 10, 2, 10 + 1].Merge();

                                    sheet1.Range[2, 10 + 2].Text = (Convert.ToDouble(totalPresentDays) + (Convert.ToDouble(totalHalfDays) * 0.5)).ToString();
                                    sheet1.Range[2, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[2, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[2, 10, 2, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[2, 10, 2, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[2, 10, 2, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //xlsRow += 1;

                                    sheet1.Range[3, 10].Text = "Leave Days / LWP";
                                    sheet1.Range[3, 10, 3, 10 + 1].Merge();

                                    sheet1.Range[3, 10 + 2].Text = (Convert.ToDouble(totalLeaveDays) + (Convert.ToDouble(totalHalfDaysLeave) * 0.5) + (Convert.ToDouble(totalAbsentLeaveDays) * 0.5)).ToString() + " / " + toTotalLWP;
                                    sheet1.Range[3, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[3, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[3, 10, 3, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[3, 10, 3, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[3, 10, 3, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //xlsRow += 1;
                                    sheet1.Range[4, 10].Text = "Absent Days/Extra Ab";
                                    sheet1.Range[4, 10, 4, 10 + 1].Merge();

                                    sheet1.Range[4, 10 + 2].Text = (Convert.ToDouble(totalAbsentDays) + (Convert.ToDouble(totalLeaveAbsentDays) * 0.5) - (Convert.ToDouble(totalAbsentLeaveDays) * 0.5)).ToString() + " / " + totalExtraAbsent;
                                    sheet1.Range[4, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[4, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[4, 10, 4, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[4, 10, 4, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[4, 10, 4, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    ///xlsRow += 1;
                                    sheet1.Range[5, 10].Text = "Total Weekoffs";
                                    sheet1.Range[5, 10, 5, 10 + 1].Merge();

                                    sheet1.Range[5, 10 + 2].Text = totalWeekOFFDays.ToString();
                                    sheet1.Range[5, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[5, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[5, 10, 5, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[5, 10, 5, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[5, 10, 5, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //xlsRow += 1;
                                    sheet1.Range[6, 10].Text = "Late";
                                    sheet1.Range[6, 10, 6, 10 + 1].Merge();

                                    sheet1.Range[6, 10 + 2].Text = totalLateDays.ToString();
                                    sheet1.Range[6, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[6, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[6, 10, 6, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[6, 10, 6, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[6, 10, 6, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //xlsRow += 1;
                                    sheet1.Range[7, 10].Text = "Holidays";
                                    sheet1.Range[7, 10, 7, 10 + 1].Merge();

                                    sheet1.Range[7, 10 + 2].Text = totalHolidays.ToString();
                                    sheet1.Range[7, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[7, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[7, 10, 7, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[7, 10, 7, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[7, 10, 7, 10 + 2].BorderAround(ExcelLineStyle.Hair);

                                    if (chkAdditionInfo == true)
                                    {
                                        //xlsRow += 1;
                                        sheet1.Range[8, 10].Text = "ODD(Hours):";
                                        sheet1.Range[8, 10, 8, 10 + 1].Merge();


                                        string zot = string.Empty;
                                        oru.GetOT(dsBioDvAC.Tables[0].Rows[i]["OTConsiderOn"].ToString(), totalODD.ToString(), out zot);
                                        sheet1.Range[8, 10 + 2].Text = zot;

                                        sheet1.Range[8, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                        sheet1.Range[8, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[8, 10, 8, 10 + 2].CellStyle.Font.Bold = true;
                                        sheet1.Range[8, 10, 8, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        sheet1.Range[8, 10, 8, 10 + 2].BorderAround(ExcelLineStyle.Hair);

                                        //xlsRow += 1;
                                        sheet1.Range[9, 10].Text = "Total OT Hour";
                                        sheet1.Range[9, 10, 9, 10 + 1].Merge();

                                    }

                                    // ----End Total-- -
                                    #endregion

                                    xlsRow = 11;
                                    xlsCol = 1;
                                    iDate = xlsCol;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, iDate].Text = "Date";
                                    sheet1.Range[xlsRow, iDate].ColumnWidth = 11;
                                    sheet1.Range[xlsRow, iDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iDay = xlsCol;
                                    sheet1.Range[xlsRow, iDay].Text = "Day";
                                    sheet1.Range[xlsRow, iDay].ColumnWidth = 5;
                                    sheet1.Range[xlsRow, iDay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDay].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    xlsCol += 1;
                                    iShiftName = xlsCol;
                                    sheet1.Range[xlsRow, iShiftName].Text = "Shift Name";
                                    sheet1.Range[xlsRow, iShiftName].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, iShiftName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iShiftName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iShiftIntime = xlsCol;
                                    sheet1.Range[xlsRow, iShiftIntime].Text = "Shift InTime";
                                    sheet1.Range[xlsRow, iShiftIntime].ColumnWidth = 8;
                                    sheet1.Range[xlsRow, iShiftIntime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iShiftIntime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iShiftOuttime = xlsCol;
                                    sheet1.Range[xlsRow, iShiftOuttime].Text = "Shift OutTime";
                                    sheet1.Range[xlsRow, iShiftOuttime].ColumnWidth = 9;
                                    sheet1.Range[xlsRow, iShiftOuttime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iShiftOuttime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iInTime = xlsCol;
                                    sheet1.Range[xlsRow, iInTime].Text = "InTime";
                                    sheet1.Range[xlsRow, iInTime].ColumnWidth = 8;
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iOutTime = xlsCol;
                                    sheet1.Range[xlsRow, iOutTime].Text = "OutTime";
                                    sheet1.Range[xlsRow, iOutTime].ColumnWidth = 8;
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;



                                    xlsCol += 1;
                                    iDayStatus = xlsCol;
                                    sheet1.Range[xlsRow, iDayStatus].Text = "Day Status";
                                    sheet1.Range[xlsRow, iDayStatus].ColumnWidth = 6.5;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iLateBy = xlsCol;
                                    sheet1.Range[xlsRow, iLateBy].Text = "Late By";
                                    sheet1.Range[xlsRow, iLateBy].ColumnWidth = 9.5;
                                    sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    xlsCol += 1;
                                    iLvShortName = xlsCol;
                                    sheet1.Range[xlsRow, iLvShortName].Text = "LV";
                                    sheet1.Range[xlsRow, iLvShortName].ColumnWidth = 9;
                                    sheet1.Range[xlsRow, iLvShortName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iLvShortName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    if (chkAdditionInfo == true)
                                    {
                                        xlsCol += 1;
                                        iODD = xlsCol;
                                        sheet1.Range[xlsRow, iODD].Text = "ODD";
                                        sheet1.Range[xlsRow, iODD].ColumnWidth = 9;
                                        sheet1.Range[xlsRow, iODD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iODD].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    }

                                    if (chkAdditionInfo == true)
                                    {
                                        xlsCol += 1;
                                        iOverStay = xlsCol;
                                        sheet1.Range[xlsRow, iOverStay].Text = "Over Stay";
                                        sheet1.Range[xlsRow, iOverStay].ColumnWidth = 9;
                                        sheet1.Range[xlsRow, iOverStay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iOverStay].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    }
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    endXlsCol = xlsCol;

                                    freezeRow = xlsRow.ToString();
                                    #endregion ------------------Column Header------------------
                                }
                                strEmpCode = dvBioDvAC[i]["EmployeeCode"].ToString().Trim();

                                #region ----------------------Data-----------------------

                                xlsRow += 1;
                                sheet1.Range[xlsRow, iDate].Text = dvBioDvAC[i]["PDate"].ToString();
                                sheet1.Range[xlsRow, iDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iShiftName].Text = dvBioDvAC[i]["ShiftName"].ToString();
                                sheet1.Range[xlsRow, iShiftName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iShiftName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iShiftIntime].Text = dvBioDvAC[i]["ShiftInTimeShow"].ToString();
                                sheet1.Range[xlsRow, iShiftIntime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iShiftIntime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iShiftOuttime].Text = dvBioDvAC[i]["ShiftOutTime"].ToString();
                                sheet1.Range[xlsRow, iShiftOuttime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iShiftOuttime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                //if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTEntitle"]))
                                //{
                                //    continue;
                                //}
                                //if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsNoPunchOnHolidayForOTEntitle"]))
                                //{
                                //    continue;
                                //}

                                if (chkAdditionInfo == true)
                                {
                                    if (!string.IsNullOrEmpty(dvBioDvAC[i]["DurationInMin"].ToString()))
                                    {
                                        string yot = string.Empty;
                                        oru.GetOT(dsBioDvAC.Tables[0].Rows[i]["OTConsiderOn"].ToString(), dsBioDvAC.Tables[0].Rows[i]["DurationInMin"].ToString(), out yot);

                                        sheet1.Range[xlsRow, iODD].Text = yot;
                                        sheet1.Range[xlsRow, iODD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iODD].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }

                                }
                                if (dvBioDvAC[i]["PDate"].ToString() == "11-Dec-2020")
                                {

                                }
                                sheet1.Range[xlsRow, iDay].Text = dvBioDvAC[i]["PDay"].ToString().Substring(0, 3);
                                sheet1.Range[xlsRow, iDay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iDay].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                {
                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "CW";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                    }
                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }
                                else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                {



                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "CW";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                    }
                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                }
                                else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                {
                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "CW";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                    }
                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                }
                                else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                {
                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "CW";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                    }
                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                }

                                else if (dvBioDvAC[i]["DayStatus"].ToString().Trim().Contains("LV") || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W")
                                {
                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "CW";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                    }
                                    //sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["OriginalDayType"].ToString().Trim();
                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }


                                else
                                {
                                    if (dvBioDvAC[i]["InTimeShow"].ToString() != "")
                                    {
                                        sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                        sheet1.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dvBioDvAC[i]["InTimeShow"].ToString());
                                        sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }
                                    if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsManualInTime"].ToString().Trim()))
                                    {
                                        sheet1.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Dark_blue;
                                    }
                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim().Contains("LV"))
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "LV";
                                    }
                                    else
                                    {
                                        if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "L")
                                        {
                                            sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Dark_blue;
                                            sheet1.Range[xlsRow, iDayStatus].Text = "L";
                                        }
                                        else
                                        {

                                            if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                            {
                                                sheet1.Range[xlsRow, iDayStatus].Text = "CW";
                                            }
                                            else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                            {
                                                sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                            }
                                            else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                            {
                                                sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                            }
                                            else
                                            {
                                                sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                            }
                                        }
                                    }

                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                }
                                if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CW")
                                {
                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "CW";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "W";
                                    }
                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = "H";
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();

                                    }

                                    sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }
                                if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                {
                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }
                                else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                {
                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }
                                else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                {
                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter; ;
                                }
                                else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                {
                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }

                                else if (dvBioDvAC[i]["DayStatus"].ToString().Trim().Contains("LV") || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                {
                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                }
                                else
                                {

                                    if (dvBioDvAC[i]["OutTimeShow"].ToString() != "")
                                    {

                                        DateTime NewRealOutTime;
                                        string TakeDate = Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                        string ot = Convert.ToDateTime(dvBioDvAC[i]["ShiftOutTime"].ToString().Trim()).ToString("hh:mm tt");

                                        //check night shift
                                        string _sOUTtime = TakeDate + " " + ot;
                                        string _sINtime = TakeDate + " " + Convert.ToDateTime(dvBioDvAC[i]["ShiftInTime"].ToString().Trim()).ToString("hh:mm tt");
                                        if (Convert.ToDateTime(_sOUTtime) < Convert.ToDateTime(_sINtime))
                                        {
                                            TakeDate = Convert.ToDateTime(TakeDate).AddDays(1).ToString("dd-MMM-yyyy");
                                        }

                                        string TateandTime = TakeDate + " " + ot;
                                        //int minutesadd = Convert.ToInt32(dvBioDvAC[i]["MaxOTPerDay"].ToString().Trim());
                                        int minutesadd = 240;
                                        DateTime NewOutTime = Convert.ToDateTime(TateandTime).AddMinutes(minutesadd);
                                        DateTime RealOutTime = Convert.ToDateTime(dvBioDvAC[i]["OutTimeShow"].ToString().Trim());

                                        if (Convert.ToDateTime(RealOutTime) > Convert.ToDateTime(NewOutTime))
                                        {
                                            //long WorkDateTickCount = Convert.ToDateTime(Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString()).ToString("dd-MMM-yyyy")).Ticks;
                                            //int EmployeeSystemId = (int)Convert.ToInt64(dvBioDvAC[i]["SystemId"].ToString());

                                            long WorkDateTickCount = Convert.ToInt64(Convert.ToDateTime(dvBioDvAC[i]["WDate"].ToString()).ToString("yyMMddHHmmss"));
                                            int EmployeeSystemId = (int)Convert.ToInt64(dvBioDvAC[i]["EmployeeCodeNumeric"].ToString());

                                            WorkDateTickCount += EmployeeSystemId;

                                            Random rnd = new Random((int)(WorkDateTickCount));
                                            int RandomMinutes = rnd.Next(0, 15);
                                            NewRealOutTime = Convert.ToDateTime(NewOutTime).AddMinutes(RandomMinutes);
                                        }

                                        else
                                        {
                                            NewRealOutTime = Convert.ToDateTime(dvBioDvAC[i]["OutTimeShow"].ToString().Trim());
                                        }
                                        DateTime RandomTime = Convert.ToDateTime(NewRealOutTime);
                                        DateTime ShiftTime = Convert.ToDateTime(TateandTime);
                                        TimeSpan span = RandomTime - ShiftTime;
                                        double totalMinutes = span.TotalMinutes;

                                        sheet1.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                        sheet1.Range[xlsRow, iOutTime].DateTime = NewRealOutTime;
                                        sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }

                                    if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsManualOutTime"].ToString().Trim()))
                                    {
                                        sheet1.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Dark_blue;
                                    }
                                }


                                if (dvBioDvAC[i]["DayStatus"].ToString() == "W" || dvBioDvAC[i]["DayStatus"].ToString() == "H" || dvBioDvAC[i]["DayStatus"].ToString() == "LV" || dvBioDvAC[i]["DayStatus"].ToString() == "CW" || dvBioDvAC[i]["DayStatus"].ToString() == "A" || dvBioDvAC[i]["DayStatus"].ToString() == "AH" || dvBioDvAC[i]["DayStatus"].ToString() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString() == "WP" || dvBioDvAC[i]["DayStatus"].ToString() == "WL" || dvBioDvAC[i]["DayStatus"].ToString() == "CWL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                {
                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].Text = "";

                                }
                                else
                                {
                                    sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                    if (!string.IsNullOrEmpty(dvBioDvAC[i]["InTimeShow"].ToString()))
                                    {
                                        sheet1.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dvBioDvAC[i]["InTimeShow"].ToString()); 
                                    }

                                    if (dvBioDvAC[i]["OutTimeShow"].ToString() != "")
                                    {

                                        DateTime NewRealOutTime;
                                        string TakeDate = Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                        string ot = Convert.ToDateTime(dvBioDvAC[i]["ShiftOutTime"].ToString().Trim()).ToString("hh:mm tt");

                                        //check night shift
                                        string _sOUTtime = TakeDate + " " + ot;
                                        string _sINtime = TakeDate + " " + Convert.ToDateTime(dvBioDvAC[i]["ShiftInTime"].ToString().Trim()).ToString("hh:mm tt");
                                        if (Convert.ToDateTime(_sOUTtime) < Convert.ToDateTime(_sINtime))
                                        {
                                            TakeDate = Convert.ToDateTime(TakeDate).AddDays(1).ToString("dd-MMM-yyyy");
                                        }

                                        string TateandTime = TakeDate + " " + ot;
                                        int minutesadd = 240;
                                        //int minutesadd = Convert.ToInt32(dvBioDvAC[i]["MaxOTPerDay"].ToString().Trim());
                                        DateTime NewOutTime = Convert.ToDateTime(TateandTime).AddMinutes(minutesadd);
                                        DateTime RealOutTime = Convert.ToDateTime(dvBioDvAC[i]["OutTimeShow"].ToString().Trim());

                                        if (Convert.ToDateTime(RealOutTime) > Convert.ToDateTime(NewOutTime))
                                        {
                                            //long WorkDateTickCount = Convert.ToDateTime(Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString()).ToString("dd-MMM-yyyy")).Ticks;
                                            //int EmployeeSystemId = (int)Convert.ToInt64(dvBioDvAC[i]["SystemId"].ToString());

                                            long WorkDateTickCount = Convert.ToInt64(Convert.ToDateTime(dvBioDvAC[i]["WDate"].ToString()).ToString("yyMMddHHmmss"));
                                            int EmployeeSystemId = (int)Convert.ToInt64(dvBioDvAC[i]["EmployeeCodeNumeric"].ToString());

                                            WorkDateTickCount += EmployeeSystemId;

                                            Random rnd = new Random((int)(WorkDateTickCount));
                                            int RandomMinutes = rnd.Next(0, 15);
                                            NewRealOutTime = Convert.ToDateTime(NewOutTime).AddMinutes(RandomMinutes);
                                        }

                                        else
                                        {
                                            NewRealOutTime = Convert.ToDateTime(dvBioDvAC[i]["OutTimeShow"].ToString().Trim());
                                        }
                                        DateTime RandomTime = Convert.ToDateTime(NewRealOutTime);
                                        DateTime ShiftTime = Convert.ToDateTime(TateandTime);
                                        TimeSpan span = RandomTime - ShiftTime;
                                        double totalMinutes = span.TotalMinutes;

                                        sheet1.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                        sheet1.Range[xlsRow, iOutTime].DateTime = NewRealOutTime;
                                        sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }

                                }
                                sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                var Date = dvBioDvAC[i]["PDate"].ToString().Trim();
                                var EmpCode = dvBioDvAC[i]["EmployeeCode"].ToString().Trim();

                                dvWeeklyAbsnt.RowFilter = "EmployeeCode = '" + EmpCode + "' AND  WorkingDate = '" + Date + "'";


                                if (dvWeeklyAbsnt.Count > 0)
                                {
                                    sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Red;
                                }

                                #region Extra Absent Colore

                                dvExtraAbsentDate.RowFilter = "EmployeeCode = '" + EmpCode + "' AND  WorkingDate = '" + Date + "'";

                                bool IsExtraAbsent = false;
                                if (dvExtraAbsentDate.Count > 0)
                                {
                                    IsExtraAbsent = true;
                                }
                                if (IsExtraAbsent)
                                {
                                    sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Red;
                                    sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Bold = true;

                                }
                                #endregion

                                if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsManualDayStatus"].ToString().Trim()))
                                {
                                    sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Dark_blue;

                                    sheet1.Range[xlsRow, iOutTime].Text = "";
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    sheet1.Range[xlsRow, iInTime].Text = "";
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    sheet1.Range[xlsRow, iLateBy].Text = "";
                                    sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }

                                if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "L")
                                {
                                    #region Late by min                                  
                                    sInTime = "00:00:00";
                                    if (dvBioDvAC[i]["InTimelate"].ToString().Trim() != "")
                                    {
                                        sInTime = dvBioDvAC[i]["InTimelate"].ToString().Trim() + ":00";
                                    }
                                    else
                                    {
                                        if (dvBioDvAC[i]["OutTimelate"].ToString().Trim() != "")
                                        {
                                            sInTime = dvBioDvAC[i]["OutTimelate"].ToString().Trim() + ":00";
                                        }
                                    }
                                    sOfficeInTime = "00:00:00";
                                    strLateBy = "00:00";
                                    if (dvBioDvAC[i]["ShiftInTimeLate"].ToString().Trim() != "" && sInTime != "00:00:00")
                                    {
                                        sOfficeInTime = dvBioDvAC[i]["ShiftInTimeLate"].ToString().Trim();
                                        strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                                    }

                                    #endregion Late by min
                                }
                                else
                                {
                                    ///absent by how min

                                    #region Absent by how much min

                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "A")
                                    {
                                        sInTime = "00:00:00";
                                        if (dvBioDvAC[i]["InTimelate"].ToString().Trim() != "")
                                        {
                                            sInTime = dvBioDvAC[i]["InTimelate"].ToString().Trim() + ":00";
                                            sOfficeInTime = "00:00:00";
                                            strLateBy = "00:00";
                                            if (dvBioDvAC[i]["ShiftInTimeLate"].ToString().Trim() != "" && sInTime != "00:00:00")
                                            {
                                                sOfficeInTime = dvBioDvAC[i]["ShiftInTimeLate"].ToString().Trim();
                                                strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                                            }
                                        }
                                        else
                                        {
                                            strLateBy = "";
                                        }
                                    }
                                    else
                                    {
                                        strLateBy = "";
                                    }

                                    #endregion Absent by how much min
                                }

                                //paid days

                                DateTime _ddd = Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString());

                                string dti = dvBioDvAC[i]["dti"].ToString().Trim();
                                string dto = dvBioDvAC[i]["dto"].ToString().Trim();
                                string _InTimeShow = dvBioDvAC[i]["InTimeShow"].ToString().Trim();
                                string _OutTimeShow = dvBioDvAC[i]["OutTimeShow"].ToString().Trim();

                                sheet1.Range[xlsRow, iLateBy].Text = strLateBy;
                                sheet1.Range[xlsRow, iLateBy].CellStyle.Font.Color = ExcelKnownColors.Dark_blue;
                                sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (!string.IsNullOrEmpty(dvBioDvAC[i]["Code"].ToString()))
                                {
                                    sheet1.Range[xlsRow, iLvShortName].Text = dvBioDvAC[i]["Code"].ToString() + "(" + dvBioDvAC[i]["LeaveDuration"].ToString() + ")";
                                    sheet1.Range[xlsRow, iLvShortName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iLvShortName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }

                                var sl = dvBioDvAC[i]["ShortLeave"].ToString();
                                if (sl == "0")
                                {
                                    sl = null;
                                }

                                if (chkAdditionInfo == true)
                                {
                                    string yot = string.Empty;//OTConsiderOn
                                    string overstay = string.Empty;
                                    if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsOTEntitled"].ToString()) == true)
                                    {

                                        if (!string.IsNullOrEmpty(dvBioDvAC[i]["DayCategory"].ToString()))
                                        {
                                            if (dvBioDvAC[i]["DayCategory"].ToString() == "Present" || dvBioDvAC[i]["DayCategory"].ToString() == "Late")
                                            {

                                                if (dvBioDvAC[i]["OutTimeShow"].ToString() != "")
                                                {
                                                    DateTime NewRealOutTime;
                                                    string TakeDate = Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                                    string ot = Convert.ToDateTime(dvBioDvAC[i]["ShiftOutTime"].ToString().Trim()).ToString("hh:mm tt");

                                                    //check night shift
                                                    string _sOUTtime = TakeDate + " " + ot;
                                                    string _sINtime = TakeDate + " " + Convert.ToDateTime(dvBioDvAC[i]["ShiftInTime"].ToString().Trim()).ToString("hh:mm tt");
                                                    if (Convert.ToDateTime(_sOUTtime) < Convert.ToDateTime(_sINtime))
                                                    {
                                                        TakeDate = Convert.ToDateTime(TakeDate).AddDays(1).ToString("dd-MMM-yyyy");
                                                    }

                                                    string TateandTime = TakeDate + " " + ot;
                                                    int minutesadd = 240;
                                                    //int minutesadd = Convert.ToInt32(dvBioDvAC[i]["MaxOTPerDay"].ToString().Trim());
                                                    DateTime NewOutTime = Convert.ToDateTime(TateandTime).AddMinutes(minutesadd);
                                                    DateTime RealOutTime = Convert.ToDateTime(dvBioDvAC[i]["OutTimeShow"].ToString().Trim());
                                                    double totalMinutes;

                                                    if (Convert.ToDateTime(RealOutTime) > Convert.ToDateTime(NewOutTime) && (dvBioDvAC[i]["OriginalDayType"].ToString() != "H" && dvBioDvAC[i]["OriginalDayType"].ToString() != "W"))
                                                    {
                                                        long WorkDateTickCount = Convert.ToDateTime(Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString()).ToString("dd-MMM-yyyy")).Ticks;
                                                        int EmployeeSystemId = (int)Convert.ToInt64(dvBioDvAC[i]["SystemId"].ToString());
                                                        WorkDateTickCount += EmployeeSystemId;

                                                        Random rnd = new Random((int)(WorkDateTickCount));
                                                        int RandomMinutes = rnd.Next(0, 15);
                                                        NewRealOutTime = Convert.ToDateTime(NewOutTime).AddMinutes(RandomMinutes);
                                                        DateTime RandomTime = Convert.ToDateTime(NewRealOutTime);
                                                        DateTime ShiftTime = Convert.ToDateTime(TateandTime);
                                                        TimeSpan span = RandomTime - ShiftTime;
                                                        totalMinutes = span.TotalMinutes;
                                                        oru.GetOT(dsBioDvAC.Tables[0].Rows[0]["OTConsiderOn"].ToString(), minutesadd.ToString(), out overstay);
                                                        if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                                        {
                                                            overstay = "";
                                                            OTOverstay1 += 0.00;

                                                        }
                                                        else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                                        {
                                                            overstay = "";
                                                            OTOverstay1 += 0.00;


                                                        }
                                                        else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                                        {
                                                            overstay = "";
                                                            OTOverstay1 += 0.00;


                                                        }
                                                        else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                                        {
                                                            overstay = "";
                                                            OTOverstay1 += 0.00;

                                                        }

                                                        else if (dvBioDvAC[i]["DayStatus"].ToString().Trim().Contains("LV") || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CW" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                                        {
                                                            OTOverstay1 += 0.00;
                                                        }
                                                        else
                                                        {
                                                            OTOverstay1 += clsStaticInfo.dbl(minutesadd);

                                                        }

                                                    }
                                                    else
                                                    {
                                                        NewRealOutTime = Convert.ToDateTime(dvBioDvAC[i]["OutTimeShow"].ToString().Trim());
                                                        oru.GetOT(dsBioDvAC.Tables[0].Rows[0]["OTConsiderOn"].ToString(), dvBioDvAC[i]["OverStay"].ToString(), out overstay);
                                                        if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                                        {
                                                            overstay = "";
                                                            OTOverstay2 += 0.00;

                                                        }
                                                        else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                                        {
                                                            overstay = "";
                                                            OTOverstay2 += 0.00;


                                                        }
                                                        else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                                        {
                                                            overstay = "";
                                                            OTOverstay2 += 0.00;


                                                        }
                                                        else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                                        {
                                                            overstay = "";
                                                            OTOverstay2 += 0.00;

                                                        }

                                                        else if (dvBioDvAC[i]["DayStatus"].ToString().Trim().Contains("LV") || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                                        {
                                                            OTOverstay2 += 0.00;
                                                        }
                                                        else
                                                        {
                                                            OTOverstay2 += clsStaticInfo.dbl(dvBioDvAC[i]["OverStay"].ToString());


                                                        }


                                                    }

                                                }
                                            }
                                        }
                                    }

                                    if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                    {
                                        overstay = "";


                                    }
                                    else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                    {
                                        overstay = "";



                                    }
                                    else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == false)
                                    {
                                        overstay = "";



                                    }
                                    else if (dvBioDvAC[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dvBioDvAC[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString().Trim()) == true)
                                    {
                                        overstay = "";


                                    }

                                    else if (dvBioDvAC[i]["DayStatus"].ToString().Trim().Contains("LV") || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "W" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "CWL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "WL" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HP" || dvBioDvAC[i]["DayStatus"].ToString().Trim() == "HL")
                                    {
                                        overstay = "";

                                    }




                                    sheet1.Range[xlsRow, iOverStay].Text = overstay;
                                    sheet1.Range[xlsRow, iOverStay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOverStay].VerticalAlignment = ExcelVAlign.VAlignCenter;






                                }

                                #endregion ----------------------Data-----------------------
                                #region Line Setup

                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

                                #endregion Line Setup
                            }
                            xlsRow += 1;
                            OTOverstay += clsStaticInfo.dbl(OTOverstay1 + OTOverstay2);

                            if (chkAdditionInfo == true)
                            {
                                string GTotalOt = string.Empty;
                                oru.GetOT(dsBioDvAC.Tables[0].Rows[0]["OTConsiderOn"].ToString(), OTOverstay.ToString(), out GTotalOt);
                                sheet1.Range[xlsRow, iODD + 1].Text = GTotalOt;
                                sheet1.Range[xlsRow, iODD + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iODD + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iODD].Text = "Total ";
                                sheet1.Range[xlsRow, iODD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iODD].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, iODD, xlsRow, iODD + 1].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, iODD, xlsRow, iODD + 1].BorderInside(ExcelLineStyle.Hair);

                                sheet1.Range[9, 10 + 2].Text = GTotalOt;
                                sheet1.Range[9, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[9, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[9, 10, 9, 10 + 2].CellStyle.Font.Bold = true;
                                sheet1.Range[9, 10, 9, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[9, 10, 9, 10 + 2].BorderAround(ExcelLineStyle.Hair);

                            }

                            xlsRow += 3;
                            xlsRow += 5;

                            sheet1.Range[xlsRow, iDate].Text = employeeName;
                            sheet1.Range[xlsRow, iDate, xlsRow, iShiftName].Merge();
                            sheet1.Range[xlsRow, iDate, xlsRow, iShiftName].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, iDate, xlsRow, iShiftName].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thick;

                            sheet1.Range[xlsRow, iDate, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iDate, xlsRow, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.IsDisplayZeros = false;

                            #region ******************Report Header******************
                            try
                            {
                                string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                                Image companyLogo = Image.FromFile(strPath);
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
                            catch (Exception)
                            {


                            }
                            xlsRow = 1;
                            xlsCol = 1;


                            FactoryName = string.Empty;

                            string FactoryAddress = string.Empty;

                            if (dsCmp.Tables[0].Rows.Count > 0)
                            {
                                CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                            }
                            else
                            {
                                CmpName = "";
                            }
                            sheet1.Range[xlsRow, 3].Text = CmpName;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 17;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 20;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                            xlsRow += 1;
                            if (dsFactory.Tables[0].Rows.Count > 0)
                            {
                                //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                                FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                            }
                            else
                            {
                                FactoryName = "";
                            }
                            sheet1.Range[xlsRow, 3].Text = FactoryName;
                            //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 25;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                            xlsRow += 1;
                            if (dsFactory.Tables[0].Rows.Count > 0)
                            {
                                FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                            }
                            else
                            {
                                FactoryAddress = "";
                            }
                            sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 15;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                            xlsRow += 1;
                            sheet1.Range[xlsRow, 3].Text = "Employee Job Card Information From Date: " + fromDate + " To Date: " + toDate;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 20;
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                            #endregion ******************Report Header******************

                            #region Freeze Panes
                            if (chkAdditionInfo == true)
                            {
                                sheet1.IsDisplayZeros = false;
                                sheet1.UsedRange["A13"].FreezePanes();
                            }
                            else
                            {
                                sheet1.IsDisplayZeros = false;
                                sheet1.UsedRange["A13"].FreezePanes();
                            }

                            #endregion Freeze Panes

                            #region UsedRange Alignment

                            sheet1.UsedRange.WrapText = true;
                            sheet1.UsedRange.CellStyle.Font.Size = 8;
                            sheet1.Range["A1"].CellStyle.Font.Size = 14;
                            sheet1.Range["A2"].CellStyle.Font.Size = 10;
                            sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                            #endregion UsedRange Alignment

                            #region Page Setup
                            sheet1.PageSetup.TopMargin = 0.5;
                            sheet1.PageSetup.BottomMargin = 0.7;
                            sheet1.PageSetup.PrintTitleRows = "$1:$11";
                            sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                            sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                            sheet1.PageSetup.LeftMargin = 0.5;
                            sheet1.PageSetup.RightMargin = 0.2;
                            sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                            sheet1.PageSetup.FitToPagesTall = 0;
                            sheet1.PageSetup.FitToPagesWide = 1;
                            sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                            sheet1.IsDisplayZeros = false;

                            sheet1.Name = sEmpCodeColl[Ec].ToString().Trim();

                            #endregion Page Setup

                        }

                    }

                    return workbook;
                }
                else
                {
                    Exception ex = new Exception("No data found...");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                dsBioDvAC = null;
                dvBioDvAC = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }


        }

      
        private void GetBuyerEmpJobCardInfoWithInDateTimes(string EmpIdLoop, string FromDate, string ToDate, string plantId, out DataSet dsRef)
        {

            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT A.EmployeeCode,A.EmployeeCodeNumeric
                            	,A.EmployeeName
                                ,A.EmployeeStatus
                            	,A.DOJ
                            	,A.GivenDesignation
                                ,A.LegalDesignation
                            	,A.Unit
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
                                ,A.OTHrs
                            	,A.LvShortName
                            	,A.Code
                            	,A.LvDescrip
                            	,A.LeaveType
                            	,A.OriginalDayType
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
                                ,A.GradeCode
	                            ,A.LeaveDuration                               
								,A.DurationInMin

	                                ,A.EO 
									,A.LIN
									,A.LO
                                    ,A.Line,A.WDate
,A.MaxOTPerDay,A.IsNoPunchOnHolidayForOTEntitle,A.IsNoPunchOnHolidayForOTNotEntitle,A.IsNoPunchOnWeekOffForOTEntitle,A.IsNoPunchOnWeekOffForOTNotEntitle
,A.SystemId,A.TotalPresent
                            FROM(
                                SELECT E.EmployeeCode,E.EmployeeCodeNumeric
                                    , E.EmployeeName
                                    ,E.EmployeeStatus
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOS, 113), ' ', '-') DOS
                                    ,E.SystemId
                                    , D.UserName GivenDesignation
                                    , U.UserName Unit
                                    , Dv.UserName Division
                                    , Dp.UserName Department
                                    , S.UserName Section
                                    ,ar.IsHalfDayLeave
                                    , SB.UserName SubSection
                                    ,datename(dw,AR.WorkDate) as PDay,AR.WorkDate WDate
                                    , AR.WorkDate PDate
                                    , AR.DayStatus
                                    , LSalGr.Code GradeCode
                                    , HR.OTConsiderOn
                                    , AR.InTime InTime
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
                                    , AR.OutTime OutTime
                                    , AR.OutTime as OutTimeShow
                                    , CONVERT(VARCHAR(5), AR.OutTime, 108) OutTimelate
                                    , AROUT.DeviceID OutDeviceID
                                    , AR.IsManualInTime IsManual
                                    , AR.OTHr 
                                    ,OT.TotalOTHr
                                    ,OTHrs=CASE WHEN (CASE WHEN OT.TotalOTHr>240 THEN 240 ELSE OT.TotalOTHr END)<121 THEN 0 ELSE ((CASE WHEN OT.TotalOTHr>240 THEN 240 ELSE OT.TotalOTHr END)-120)/60 END
                                    , LT.UserName LvShortName
                                    , LT.Description LvDescrip
                                    , LT.LeaveType
                                    , dt.OriginalDayType
                                    , LT.Code
                                    , Isnull(LG.UserName, '') LegalDesignation
                                    , AR.InTime dti, AR.OutTime dto
                                    , CONVERT(VARCHAR(5), cs.InTime, 108) ShiftChangeInTime
                                    , SD.ShiftDefinationName ShiftName
									,sd.ShiftType
                                    ,LEAVE.LeaveDuration	                            
									,HODD.DurationInMin

		                            ,EO.OffDuration AS EO
									,EIN.OffDuration AS LIN
									,LO= Case when LO.InfoType='LUNCHOUT' THEN 'YES' ELSE 'NO' END

						   ,ShiftOutTime = CASE                                   
                           WHEN cs.OutTime IS NULL
                           THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
                           ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                           END
                                     ,ShiftInTime = Format(AR.WorkDate, 'yyyy-MM-dd') + ' ' + CASE 
			                         WHEN cs.InTime IS NULL
			                         	THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
			                         ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
			                         END
                                    , AR.IsManualDayStatus, AR.IsManualInTime, AR.IsManualOutTime,
ar.CountedShortLeave ShortLeave,AR.IsOTEntitled,AR.IsOTComfirm,OT.WorkDate,dt.Category DayCategory
,CAS.MaxOTPerDay,CAS.IsNoPunchOnHolidayForOTEntitle,CAS.IsNoPunchOnHolidayForOTNotEntitle,CAS.IsNoPunchOnWeekOffForOTEntitle,CAS.IsNoPunchOnWeekOffForOTNotEntitle
 ,TotalPresent = CASE WHEN DT.Category = 'Present' and LTSystemID is null THEN 1
											WHEN DT.Category = 'Present' and LTSystemID is not null and LEAVE.LeaveDuration<1 THEN (1-LEAVE.LeaveDuration)
											WHEN DT.Category = 'Late' and LTSystemID is null THEN 1
											WHEN DT.Category = 'Leave' and LTSystemID is not null and LEAVE.LeaveDuration<1 THEN (1-LEAVE.LeaveDuration)
											WHEN DT.Category = 'Half Day' and LTSystemID is not null THEN (1-LEAVE.LeaveDuration)
											WHEN DT.Category = 'Half Day' and LTSystemID is null THEN 0.5
											ELSE 0 END
                                FROM dbo.EmployeeInformation E

                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
left join [dbo].[ComplianceAttendanceSetting] CAS ON CAS.CompanyGroupId=mpb.CompanyGroupId and cas.PlantId=e.PlantId
                                INNER JOIN dbo.AttdnProcessData AR ON E.SystemID = AR.EmpSystemID
	                           LEFT JOIN (select LET.SystemID,LTD.LeaveDuration,LTD.WorkDate,LET.EmpSystemID from  LeaveTransaction LET 
										    left join LeaveTransactionDetails LTD ON LTD.LvTrnsSystemID=LET.SystemID	
                                        where ltd.WorkDate Between '" + FromDate + @"' and '" + ToDate + @"'
								         ) LEAVE ON LEAVE.EmpSystemID=E.SystemId and LEAVE.WorkDate= AR.WorkDate

                                left join (select EmpSystemID,WorkDate,SUM(DurationInMin)AS DurationInMin
		                    From  [dbo].[HourlyOffDuty] 
	                        WHERE  ApproveType='Deducation' AND WorkDate Between '" + FromDate + @"' and '" + ToDate + @"'
		                    Group BY  EmpSystemID,WorkDate)as HODD on HODD.EmpSystemID=E.SystemId and HODD.WorkDate=AR.WorkDate

                                LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + FromDate + @"' BETWEEN FromDate AND ToDate) AS SFCG
                                ON AR.ShiftSystemID = SFCG.ShiftDefinationID
                                LEFT JOIN dbo.AttdnRawData ARIN ON AR.InTimeRowID = ARIN.RowID
                                LEFT JOIN dbo.AttdnRawData AROUT ON AR.OutTimeRowID = AROUT.RowID
                                LEFT JOIN dbo.LeaveType LT ON AR.LTSystemID = LT.Id
                                LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                                LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                                LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id

                                  LEFT JOIN ORG.Section S ON PO.SectionID = S.Id
                                LEFT JOIN ORG.SubSection SB ON PO.SubSectionID = SB.Id
								left join org.Line l on l.Id=mpb.LineId

                                LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LG.Id and LSGD.PlantId='" + plantId + @"'
                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = LSGD.LegalSalaryGradeId
                                --left join EmpDateWiseShiftAssign es on es.EmpSystemID = E.SystemId
                                --AND AR.WorkDate = ES.WorkDate
                                left join(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m
                                left join[ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = AR.ShiftSystemID and cs.ShiftDate = ar.WorkDate
                                left join[ShiftDefination] sd on sd.SystemID = AR.ShiftSystemID
                                LEFT JOIN HKP.Designation D ON E.GivenDesignationId = D.Id
                                LEFT JOIN FinalOT OT ON E.SystemId = OT.EmpSystemID and ot.WorkDate=ar.WorkDate
                                LEFT JOIN PlantWiseHRMSSetting hr on HR.PlantID=E.PlantId
                                LEFT JOIN DayType dt on dt.Daytype=AR.DayStatus

                                left join AttendanceInfoExtra LO on LO.EmpSystemId=e.SystemId and LO.WorkDate=ar.WorkDate and LO.InfoType='LUNCHOUT'
								left join AttendanceInfoExtra EO on EO.EmpSystemId=e.SystemId and EO.WorkDate=ar.WorkDate and EO.InfoType='EARLUOUT'
								left join AttendanceInfoExtra EIN on EIN.EmpSystemId=e.SystemId and EIN.WorkDate=ar.WorkDate and EIN.InfoType='EARLUIN'

                                WHERE E.SystemID in (" + EmpIdLoop + @")
                                    AND AR.WorkDate BETWEEN '" + FromDate + @"'
                                        AND '" + ToDate + @"' AND (EmployeeStatus = 'Active' OR COnvert(date,DOS) >= Convert(Date,'" + FromDate + @"'))
                                ) A
                           
                            ORDER BY A.EmployeeCode
                            	,A.PDate
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
        }//End Function

        private void GetBuyerEmpJobCardMonthlySummary(string EmpIdLoop, string FromDate, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @" SELECT EmpSystemID,EmployeeCode, WorkDate ,EmployeeCode, ISNULL(TotalPresent, 0) TotalPresent, ISNULL(TotalLv, 0) TotalLv,ISNULL(TotalHoliDay,0)TotalHoliDay,ISNULL(TotalWeekOff, 0)TotalWeekOff
                                ,ISNULL(TotalLWP, 0) TotalLWP,ISNULL(TotalMLv, 0) TotalMLv,ISNULL(TotalMLv, 0) TotalMLv,isnull(TotalAbsent,0)TotalAbsent,ISNULL(TotalLate,0)TotalLate
                                , DayValue = ISNULL(TotalPresent, 0) + ISNULL(TotalLate, 0) + ISNULL(TotalLv, 0) + ISNULL(TotalMLv, 0) + ISNULL(TotalWeekOff, 0)
                                + ISNULL(TotalCompAssignLv, 0) + ISNULL(TotalHoliDay, 0) + ISNULL(TotalWeekOffHoliDay, 0),Category,DayStatus
                                FROM(SELECT EmpSystemID, WorkDate, EmployeeCode,Category,DayStatus,

                                TotalPresent = CASE WHEN Category = 'Present' and LTSystemID is null THEN 1
                                WHEN Category = 'Present' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                                WHEN Category = 'Late' and LTSystemID is null THEN 1
                                WHEN Category = 'Leave' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                                WHEN Category = 'Half Day' and LTSystemID is not null THEN (1-LeaveDuration)
                                WHEN Category = 'Half Day' and LTSystemID is null THEN 0.5
                                ELSE 0 END,
    
                                TotalLate = CASE WHEN Category = 'Late' and LTSystemID is null THEN 1
                                WHEN Category = 'Late' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                                WHEN Category = 'Late' and LTSystemID is not null and LeaveDuration=1 THEN 1
                                ELSE 0 END,
                                
                                TotalAbsent = CASE WHEN Category = 'Absent' and LTSystemID is null THEN 1
                                WHEN Category = 'Absent' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                                WHEN Category = 'Absent' and LTSystemID is not null and LeaveDuration=1 THEN 1
                                WHEN Category = 'Half Day' and LTSystemID is null THEN 0.5
                                ELSE 0 END,
                                
                                TotalLv = CASE WHEN LTSystemID is not null and Category<>'Leave' and LeaveDuration<1 and IsLWP=0 THEN LeaveDuration
                                WHEN LTSystemID is not null and Category='Leave' and IsLWP=0 THEN LeaveDuration
                                ELSE 0 END,
                                
                                TotalLWP = CASE WHEN LTSystemID is not null and Category<>'Leave' and LeaveDuration<1 and IsLWP=1 THEN LeaveDuration
                                WHEN LTSystemID is not null and Category='Leave' and IsLWP=1 THEN LeaveDuration
                                ELSE 0 END,
                                
                                TotalMLv = 0,
                                TotalCompAssignLv = 0,

                               -- TotalWeekOff = CASE WHEN OriginalDayType = 'W' and c.IsNoPunchOnWeekOffForOTEntitle=1 and a.IsOTEntitled=0 THEN 1
								                       -- WHEN OriginalDayType = 'W' and c.IsNoPunchOnWeekOffForOTNotEntitle=1 and a.IsOTEntitled=1 THEN 1
								                       -- WHEN OriginalDayType = 'CW' and c.IsNoPunchOnWeekOffForOTNotEntitle=1 and a.IsOTEntitled=1 THEN 1
								                       -- WHEN OriginalDayType = 'CW' and c.IsNoPunchOnWeekOffForOTNotEntitle=1 and a.IsOTEntitled=0 THEN 1

                                TotalWeekOff = CASE WHEN Category = 'Weekend' and c.IsNoPunchOnWeekOffForOTEntitle=1 and a.IsOTEntitled=0 THEN 1
								                       WHEN Category = 'Weekend' and c.IsNoPunchOnWeekOffForOTNotEntitle=1 and a.IsOTEntitled=1 THEN 1
                                ELSE 0 END,
                                
                                TotalHoliDay = CASE WHEN p.OriginalDayType = 'H' AND C.IsNoPunchOnHolidayForOTEntitle=1 AND A.IsOTEntitled=0 THEN 1
														WHEN p.OriginalDayType = 'H' AND C.IsNoPunchOnHolidayForOTNotEntitle=1 AND A.IsOTEntitled=1 THEN 1
                                ELSE 0 END,
                                
                                TotalWeekOffHoliDay = 0,
                                OTHr
                                FROM dbo.AttdnProcessData a
                                left join daytype p on a.DayStatus=p.DayType
                                left join employeeInformation ei on ei.SystemId =a.EmpSystemID
	                            left join ComplianceAttendanceSetting c on c.CompanyGroupId=ei.GroupID and c.PlantId = ei.PlantId
                                WHERE  ei.SystemId in( " + EmpIdLoop + @")
                                and WorkDate between '" + FromDate + @"' AND '" + ToDate + @"'
                                ) A  ";
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
        public void GetBuyerExtraAbsentCount(string fromDate, string toDate, string plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @" SELECT waa.WorkingDate,waa.EmpSystemID,e.EmployeeCode
                            FROM [SCS].[WeeklyAbsentismAssignment] waa
		                    left join EmployeeInformation e on e.SystemId= waa.EmpSystemID
                              WHERE waa.WorkingDate between '" + fromDate + "' and '" + toDate + "' and  waa.plantid='" + plantid + @"' 
                            UNION
                            SELECT ha.WorkDate WorkingDate,ha.EmpSystemID,e.EmployeeCode
                            FROM [trn].[HolidayAbsentismAssignment] ha
		                    left join EmployeeInformation e on e.SystemId=ha.EmpSystemID
                              where ha.WorkDate between '" + fromDate + "' and '" + toDate + "'  and ha.plantid='" + plantid + @"'
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
        public void GetBuyerJobCardPayDays(string EmpIdLoop, string FromDate, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                clsCrossModule ob = new clsCrossModule();
                strSql = @"SELECT EmpSystemID, WorkDate ,EmployeeCode
           , DayValue = ISNULL(TotalPresent, 0) + ISNULL(TotalLate, 0) + ISNULL(TotalLv, 0) + ISNULL(TotalMLv, 0) + ISNULL(TotalWeekOff, 0)
           + ISNULL(TotalCompAssignLv, 0) + ISNULL(TotalHoliDay, 0) + ISNULL(TotalWeekOffHoliDay, 0)
                            FROM(SELECT EmpSystemID, WorkDate, EmployeeCode,
                                        " + ob.GetAttSum() + @"
                                        OTHr
                                  FROM dbo.AttdnProcessData a
                        left join daytype p on a.DayStatus=p.DayType
                             left join  employeeInformation ei on  ei.SystemId =a.EmpSystemID
                                WHERE  ei.SystemId in( " + EmpIdLoop + @")
                                    and WorkDate between '" + FromDate + @"' AND '" + ToDate + @"'
                                --    AND MONTH(WorkDate) = MONTH('" + FromDate + @"')
                                --   AND YEAR(WorkDate) = YEAR('" + ToDate + @"')
                                                                        ) A";

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
        }
        public void GetBuyerWeeklyAbsentismAssignment(string ePlant, string eCode, string eFrmDate, string eToDate, out DataSet dsWeeklyAbsnt)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT  REPLACE(CONVERT(VARCHAR(11),WorkingDate,106),' ','-') WorkingDate,EmpSystemID,EmployeeCode
                                            FROM [SCS].[WeeklyAbsentismAssignment] S
                                            LEFT JOIN EmployeeInformation EI on EI.SystemId = S.EmpSystemID
                                            where WorkingDate BETWEEN '" + eFrmDate + @"' AND '" + eToDate + @"'  AND S.plantid= '" + ePlant + @"' AND EmpSystemID in( " + eCode + ")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsWeeklyAbsnt, false, false, "", "1");
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
        public void SelectedBuyerPlantWiseCompany(string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT p.UserName PlantName,c.UserName CompanyName ,ISNULL(a.Address1,'')+','+ ISNULL(a.Address2,'') Address1, a.Phone,a.Email
                                ,cm.Address1 cAddress1 ,cm.Address2 cAddress2
                                FROM org.Plant p
							LEFT OUTER JOIN org.Company c on c.Id=p.CompanyId
							LEFT OUTER JOIN mst.AddressMaster a on a.Id=p.AddressMasterId
							LEFT OUTER JOIN mst.AddressMaster cm on cm.Id=c.AddressMasterId
							WHERE p.Id='" + sPlantID + "'";

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
        public void SelectedBuyerPlant(string sPlantID, out DataSet dsRef)
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

        class PayDaysReport
        {
            public string EmployeeCode { get; set; }
            public DateTime WorkDate { get; set; }
            public decimal DayValue { get; set; }
        }
        #endregion
    }
}