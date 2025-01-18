using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Biometrics;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Attendances.Controllers
{
    public class SandwichAbsentController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly ILeaveTransectionService _leaveTransactionService;
        public SandwichAbsentController(
              IMaternityLeavePolicyService LeavePolicyService,
               ISqlRepository sqlRepository,
               ILeaveTransectionService leaveTransactionService
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
            _leaveTransactionService = leaveTransactionService;
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        #region All Grid 





        [HttpGet]
        public ActionResult GetEmployeeList(string FromDate, string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strSql = string.Empty;



            strSql = @"SELECT 0 CheckBoxSelect 
                     , APD.EmpSystemID
                     , FORMAT(APD.WorkDate ,'dd-MMM-yyyy') AttdnProcDate
                     , APD.DayStatus
                     , FORMAT( x.WorkDate  ,'dd-MMM-yyyy') BeforeWorkDate
                     , x.DayStatus BeforeDayStatus
                     , FORMAT(y.WorkDate,'dd-MMM-yyyy') AfterWorkDate
                     , y.DayStatus AfterDayStatus
                     , EI.EmployeeCode
                     , EI.EmployeeName
                     , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                     , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                     , DG.UserName GivenDesignation
                     , DP.UserName Department
                     , PMB.Code
                     , PR.UserName PositionName
                     , E.UserName EntityName
                     , DSG.UserName Designation
 
                    FROM ( select apd.EmpSystemID, apd.WorkDate, apd.DayStatus
	                       FROM AttdnProcessData AS apd
	                       WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' 
                           AND apd.PlantID='" + identity.PlantId + @"'
	                       AND isnull(apd.DayStatus,'') IN (
                                                            ---DayType
                                                            SELECT DayType  FROM DayType WHERE Category='Weekend' AND  1=(SELECT IsSandwichAbsentInWeekend  From  [dbo].[PlantWiseHRMSSetting] where plantid='" + identity.PlantId + @"')
                                                            UNION
                                                            SELECT DayType FROM DayType WHERE Category='Holiday' AND  1=(SELECT IsSandwichAbsentInHoliday From  [dbo].[PlantWiseHRMSSetting] where plantid='" + identity.PlantId + @"')

                                                           ) 
                           AND apd.EmpSystemID NOT IN (select EmpSystemID  FROM [SCS].[WeeklyAbsentismAssignment] WHERE EmpSystemID=apd.EmpSystemID AND WorkingDate=apd.WorkDate)
                           AND apd.EmpSystemID NOT IN (select EmpSystemID  FROM [TRN].[HolidayAbsentismAssignment] WHERE EmpSystemID=apd.EmpSystemID AND WorkDate=apd.WorkDate)

                    ) AS APD
                    LEFT OUTER JOIN AttdnProcessData AS X ON apd.EmpSystemID=x.EmpSystemID AND isnull(x.WorkDate,'')=(SELECT TOP 1 isnull(WorkDate,'') AS PK 
                                                                                                                      FROM AttdnProcessData WHERE WorkDate<apd.WorkDate 
																								                      AND EmpSystemID=APD.EmpSystemID	AND DayStatus NOT IN (
                                                                                                                                                                                ---DayType
                                                                                                                                                                                SELECT DayType  FROM DayType WHERE Category='Weekend' AND  1=(SELECT IsSandwichAbsentInWeekend  From  [dbo].[PlantWiseHRMSSetting] where plantid='" + identity.PlantId + @"')
                                                                                                                                                                                UNION
                                                                                                                                                                                SELECT DayType FROM DayType WHERE Category='Holiday' AND  1=(SELECT IsSandwichAbsentInHoliday From  [dbo].[PlantWiseHRMSSetting] where plantid='" + identity.PlantId + @"')
                                                                                                                                                                             ) 
                                                                                                                      ORDER BY WorkDate  DESC)
 

                    LEFT OUTER JOIN AttdnProcessData AS Y ON apd.EmpSystemID=Y.EmpSystemID AND isnull(Y.WorkDate,'')=(SELECT TOP 1 isnull(WorkDate,'') AS PK 
                                                                                                                      FROM AttdnProcessData WHERE WorkDate>apd.WorkDate 
																								                      AND EmpSystemID=APD.EmpSystemID	AND DayStatus NOT IN (
                                                                                                                                                                                 ---DayType
                                                                                                                                                                                SELECT DayType  FROM DayType WHERE Category='Weekend' AND  1=(SELECT IsSandwichAbsentInWeekend  From  [dbo].[PlantWiseHRMSSetting] where plantid='" + identity.PlantId + @"')
                                                                                                                                                                                UNION
                                                                                                                                                                                SELECT DayType FROM DayType WHERE Category='Holiday' AND  1=(SELECT IsSandwichAbsentInHoliday From  [dbo].[PlantWiseHRMSSetting] where plantid='" + identity.PlantId + @"')
                                                                                                                                                                              ) 
                                                                                                                      ORDER BY WorkDate  ASC)
                                                                                                  
                                                                                                  
                                                                                                  
                     LEFT JOIN dbo.Employeeinformation EI ON EI.SystemId = apd.EmpSystemID
                     LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                     LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                     LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                     LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                     LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                     LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                     LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                     LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                     LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId							
                     LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId                                                                                                  


                    ---WHERE ISNULL(x.DayStatus,'')='A' AND ISNULL(Y.DayStatus,'')='A'
                    WHERE (ISNULL(x.DayStatus,'')='A' or Isnull(x.LTSystemID,'') in (select Id from LeaveType where LeaveType='Leave Without Pay')) AND (ISNULL(Y.DayStatus,'')='A' or Isnull(y.LTSystemID,'') in (select Id from LeaveType where LeaveType='Leave Without Pay'))
                    ORDER BY APD.EmpSystemID,APD.WorkDate ";





            return Json(_sqlRepository.GetDataCollection(strSql), JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public ActionResult GetAttdnDetails(string EmpsystemId, string FromDate, string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strSql = string.Empty;



            strSql = @"    SELECT apd.EmpSystemID, FORMAT(APD.WorkDate ,'dd-MMM-yyyy') AttdnProcDate, apd.DayStatus
	                       FROM AttdnProcessData AS apd
	                       WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' 
                           AND apd.PlantID='" + identity.PlantId + @"'  AND apd.EmpSystemID='" + EmpsystemId + @"'
                           ORDER BY APD.WorkDate";





            return Json(_sqlRepository.GetDataCollection(strSql), JsonRequestBehavior.AllowGet);
        }








        [HttpPost]
        public JsonResult SaveAbsent(List<SandwichAbsentVM> AbsentList, string FromDate, string ToDate)
        {
            string EmpId = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsWeeklyAbsentismAssignment = null;
            DataSet dsHolidayAbsentismAssignment = null;

            List<SandwichAbsentVM> WeeklyAbsentList = new List<SandwichAbsentVM>();
            List<SandwichAbsentVM> HolidayAbsentList = new List<SandwichAbsentVM>();
            if (AbsentList.Count > 0)
            {
                for (int i = 0; i < AbsentList.Count; i++)
                {

                    SandwichAbsentVM o = new SandwichAbsentVM();
                    o.EmpSystemID = AbsentList[i].EmpSystemID.ToString();
                    o.WorkingDate = AbsentList[i].WorkingDate.ToString();
                    o.DayStatus = AbsentList[i].DayStatus.ToString();
                    if (AbsentList[i].DayStatus.ToString().ToUpper() == "W")
                    {
                        WeeklyAbsentList.Add(o);
                    }
                    if (AbsentList[i].DayStatus.ToString().ToUpper() == "H")
                    {
                        HolidayAbsentList.Add(o);
                    }


                }
            }



            try
            {
                if (WeeklyAbsentList.Count > 0)
                {


                    string sql = @"SELECT [Id]
                                    ,[CompanyGroupId]
                                    ,[PlantId]
                                    ,[EmpSystemID]
                                    ,[WorkingDate]
                                    ,[AddedBy]
                                    ,[AddedDate]
                                    ,[AddedFromIP]
                                    ,[UpdatedBy]
                                    ,[UpdatedDate]
                                    ,[UpdatedFromIP]
                                     FROM [SCS].[WeeklyAbsentismAssignment] WHERE PlantId = '" + identity.PlantId + "' AND WorkingDate BETWEEN '"+FromDate+@"' AND '"+ToDate+@"'  ORDER BY EmpSystemID";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsWeeklyAbsentismAssignment, false, "1");


                    for (int i = 0; i < WeeklyAbsentList.Count; i++)
                    {



                        DataView dvExceptionEmployeeAttendanceUnlock = new DataView(dsWeeklyAbsentismAssignment.Tables[0]);
                        dvExceptionEmployeeAttendanceUnlock.RowFilter = "EmpSystemID='" + WeeklyAbsentList[i].EmpSystemID.ToString() + "' AND PlantId='" + identity.PlantId + "' AND WorkingDate='" + WeeklyAbsentList[i].WorkingDate.ToString() + "'";
                        if (dvExceptionEmployeeAttendanceUnlock.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "WeeklyAbsentismAssignment", out sID);
                            DataRow dr = dsWeeklyAbsentismAssignment.Tables[0].NewRow();
                            dr["Id"] = "WA" + sID;
                            dr["EmpSystemID"] = WeeklyAbsentList[i].EmpSystemID.ToString();
                            dr["CompanyGroupId"] = identity.CompanyGroupId;
                            dr["PlantId"] = identity.PlantId;
                            dr["WorkingDate"] = WeeklyAbsentList[i].WorkingDate.ToString();
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dsWeeklyAbsentismAssignment.Tables[0].Rows.Add(dr);


                        }
                        else
                        {
                            //edit
                            DataRow dr = dvExceptionEmployeeAttendanceUnlock[0].Row;

                            dr.BeginEdit();
                            dr["EmpSystemID"] = WeeklyAbsentList[i].EmpSystemID.ToString();
                            dr["WorkingDate"] = WeeklyAbsentList[i].WorkingDate.ToString();
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();

                        }
                        dvExceptionEmployeeAttendanceUnlock.RowFilter = null;


                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsWeeklyAbsentismAssignment);
                }
                if (HolidayAbsentList.Count > 0)
                {


                    string sql = @"SELECT [Id]
                                    ,[CompanyGroupId]
                                    ,[PlantId]
                                    ,[EmpSystemID]
                                    ,[WorkDate]
                                    ,[AddedBy]
                                    ,[AddedDate]
                                    ,[AddedFromIP]
                                    ,[UpdatedBy]
                                    ,[UpdatedDate]
                                    ,[UpdatedFromIP]
                                     FROM [TRN].[HolidayAbsentismAssignment] WHERE PlantId = '" + identity.PlantId + "' AND WorkDate BETWEEN '"+ FromDate + @"' AND '"+ToDate+@"'  ORDER BY EmpSystemID";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsHolidayAbsentismAssignment, false, "1");


                    for (int i = 0; i < HolidayAbsentList.Count; i++)
                    {



                        DataView dvHolidayAbsentismAssignment = new DataView(dsHolidayAbsentismAssignment.Tables[0]);
                        dvHolidayAbsentismAssignment.RowFilter = "EmpSystemID='" + HolidayAbsentList[i].EmpSystemID.ToString() + "' AND PlantId='" + identity.PlantId + "' AND WorkDate='" + HolidayAbsentList[i].WorkingDate.ToString() + "'";
                        if (dvHolidayAbsentismAssignment.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HolidayAbsentismAssignmentSA", out sID);
                            DataRow dr = dsHolidayAbsentismAssignment.Tables[0].NewRow();
                            dr["Id"] = "HA"+ DateTime.Now.ToString("yy")+ sID;
                            dr["EmpSystemID"] = HolidayAbsentList[i].EmpSystemID.ToString();
                            dr["CompanyGroupId"] = identity.CompanyGroupId;
                            dr["PlantId"] = identity.PlantId;
                            dr["WorkDate"] = HolidayAbsentList[i].WorkingDate.ToString();
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dsHolidayAbsentismAssignment.Tables[0].Rows.Add(dr);


                        }
                        else
                        {
                            //edit
                            DataRow dr = dvHolidayAbsentismAssignment[0].Row;

                            dr.BeginEdit();
                            dr["EmpSystemID"] = HolidayAbsentList[i].EmpSystemID.ToString();
                            dr["WorkDate"] = HolidayAbsentList[i].WorkingDate.ToString();
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();

                        }
                        dvHolidayAbsentismAssignment.RowFilter = null;


                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsHolidayAbsentismAssignment);
                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            return Json(new { Message = AplosMessage.Success });
        }










        [HttpGet, Authorize]
        public ActionResult GetAssignedEmployeeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strSql = string.Empty;



            strSql = @" SELECT * from (
 
                         SELECT  APD.EmpSystemID, APD.id
                         , FORMAT(APD.WorkingDate  ,'dd-MMM-yyyy') AttdnProcDate                     
                         , EI.EmployeeCode
                         , EI.EmployeeName
                         , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , DG.UserName GivenDesignation
                         , DP.UserName Department
                         , PMB.Code
                         , PR.UserName PositionName
                         , E.UserName EntityName
                         , DSG.UserName Designation
                         , 'W' DayStatus
                        FROM  [SCS].[WeeklyAbsentismAssignment]  AS APD                   
                         LEFT JOIN dbo.Employeeinformation EI ON EI.SystemId = apd.EmpSystemID
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                         LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                         LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId							
                         LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId 
                        where apd.PlantID='" + identity.PlantId + @"'
                        --ORDER BY APD.WorkingDate DESC,APD.EmpSystemID 


                        union all

                         SELECT  APD.EmpSystemID, APD.id
                         , FORMAT(APD.WorkDate  ,'dd-MMM-yyyy') AttdnProcDate                     
                         , EI.EmployeeCode
                         , EI.EmployeeName
                         , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , DG.UserName GivenDesignation
                         , DP.UserName Department
                         , PMB.Code
                         , PR.UserName PositionName
                         , E.UserName EntityName
                         , DSG.UserName Designation
                          , 'H' DayStatus
                        FROM  [TRN].[HolidayAbsentismAssignment]  AS APD                   
                         LEFT JOIN dbo.Employeeinformation EI ON EI.SystemId = apd.EmpSystemID
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                         LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                         LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId							
                         LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId 
                        where apd.PlantID='" + identity.PlantId + @"'

                        ) x
                        ORDER BY Convert(date, x.AttdnProcDate) DESC,x.EmpSystemID ";





          string  xstrSql = @"SELECT  APD.EmpSystemID, APD.id
                     , FORMAT(APD.WorkingDate  ,'dd-MMM-yyyy') AttdnProcDate                     
                     , EI.EmployeeCode
                     , EI.EmployeeName
                     , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                     , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                     , DG.UserName GivenDesignation
                     , DP.UserName Department
                     , PMB.Code
                     , PR.UserName PositionName
                     , E.UserName EntityName
                     , DSG.UserName Designation
 
                    FROM  [SCS].[WeeklyAbsentismAssignment]  AS APD                   
                     LEFT JOIN dbo.Employeeinformation EI ON EI.SystemId = apd.EmpSystemID
                     LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                     LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                     LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                     LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                     LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                     LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                     LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                     LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                     LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId							
                     LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId 
					where apd.PlantID='" + identity.PlantId + @"'
                    ORDER BY APD.WorkingDate DESC,APD.EmpSystemID ";

            return Json(_sqlRepository.GetDataCollection(strSql), JsonRequestBehavior.AllowGet);

        }



        [HttpPost]
        public JsonResult DeleteAbsent(string Id,string DayStatus)
        {
            DataSet dsRe = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            string sql = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(Id))
                {

                    if (DayStatus.ToUpper()=="W")
                    {
                        sql = @"Delete  FROM [SCS].[WeeklyAbsentismAssignment] WHERE PlantId = '" + identity.PlantId + "'  and Id='" + Id + @"'";

                    }
                    if (DayStatus.ToUpper() == "H")
                    {
                        sql = @"Delete  FROM [TRN].[HolidayAbsentismAssignment] WHERE PlantId = '" + identity.PlantId + "'  and Id='" + Id + @"'";

                    }
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsRe, false, "1");



                }

            }
            catch (Exception ex)
            {

                throw (ex);
            }
            return Json(new { Message = AplosMessage.Success });
        }





        #endregion





        #endregion -- Operations  
    }


}