using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Extension.HumanResource.Leave;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Library.Service.Leave
{
    public class clsLeaveApproval
    {
        ISqlRepository _sqlRepository;
        public clsLeaveApproval(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        public clsLeaveApproval()
        {

        }

        public void GetSysIdWiseEmpBasicInfoInformationForLeave(string sGroupID, string sPlantID, bool isControlAdmin, bool isSysAdmin, string employeeId, string companyId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                var str = "";
                str = !isControlAdmin && !isSysAdmin ? @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
                                    (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
                                            where LeaveApproval='" + employeeId + "')))" : @" AND Emp.CompanyId='" + companyId + "'";

                strSql = @"SELECT emp.SystemId EmployeeID,Lvt.SystemID LvTrnMsID, emp.EmployeeCode,emp.BudgetCode,emp.EmployeeName,emp.EmpType,emp.NationalID,Dsgg.UserName GivenDesignation,E.UserName as Entity,
                             REPLACE(CONVERT(VARCHAR(11), emp.DOJ, 113), ' ', '-') DOJ,
							 LT.UserName LeaveName, LT.Description LeaveDescription,
                             REPLACE(CONVERT(VARCHAR(11), LvT.FromDate, 113), ' ', '-') FromDate,
                             REPLACE(CONVERT(VARCHAR(11), LvT.ToDate, 113), ' ', '-') ToDate, LvT.LeaveDays, LvT.LvReason AS Reason, LvT.ComAssignLvSystemID,LVT.LTSystemID
                             FROM
							 dbo.EmployeeInformation emp
							 LEFT outer JOIN dbo.LeaveTransaction LvT on LvT.EmpSystemID = emp.SystemId
                             LEFT OUTER JOIN [MST].[ManpowerBudget] PMB ON EMP.BudgetCode=PMB.Id
							 LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                             LEFT OUTER JOIN [ORG].[Entity] E ON PMB.EntityId=E.Id
                             LEFT outer JOIN dbo.LeaveType LT ON LvT.LTSystemID = LT.Id
							 LEFT outer JOIN [HKP].Designation AS Dsg ON Dsg.ID = Emp.DesignationSystemID
							 LEFT outer JOIN [HKP].Designation AS Dsgg ON Dsgg.ID = Emp.GivenDesignationID
                             WHERE  IsNull(Lvt.IsApproved,0) = 0
							 AND ISNULL(LvT.SystemID,'')<> ''
                             AND LvT.IsCancel=0
							 AND emp.GroupID = '" + sGroupID + @"'
                             AND emp.PlantID = '" + sPlantID + @"'" + str;

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
        }//End Function
        public IEnumerable<object> GetEmpBasicInfoInformationForLeave(string companyGroupId, string plantId, bool isControlAdmin, bool isSysAdmin, string employeeId, string companyId)
        {
            string strSql = string.Empty;
            try
            {
                var str = "";
                str = !isControlAdmin && !isSysAdmin ? @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
                                    (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
                                            where LeaveApproval='" + employeeId + "')))" : @" AND Emp.CompanyId='" + companyId + "'";

                strSql = @"SELECT 0 CheckBoxSelect, emp.SystemId EmployeeID,Lvt.SystemID LvTrnMsID, emp.EmployeeCode,emp.BudgetCode,emp.EmployeeName,emp.EmpType,emp.NationalID,Dsgg.UserName GivenDesignation,E.UserName as Entity,
                             REPLACE(CONVERT(VARCHAR(11), emp.DOJ, 113), ' ', '-') DOJ,
							 LT.UserName LeaveName, LT.Description LeaveDescription
                            , CASE WHEN ISNULL(lvt.FirstApprovingStatus,0) = 1 THEN 'Approved' ELSE 'Not Approved' END FirstApprovingStatus
                            , LVT.FirstApprovingAuthority,ISNULL(EEEI.EmployeeName,'') FAEmployeeName,
                             REPLACE(CONVERT(VARCHAR(11), LvT.FromDate, 113), ' ', '-') FromDate,
                             REPLACE(CONVERT(VARCHAR(11), LvT.ToDate, 113), ' ', '-') ToDate, LvT.LeaveDays, LvT.LvReason AS Reason, LvT.ComAssignLvSystemID,LVT.LTSystemID,LVT.SystemID LvTransSystemID , MP.isNoBenefit , MP.Id as MPolicyId
                        ,(SELECT YearlyCalendar.Id
                                 FROM YearlyCalendar WHERE LvT.FromDate BETWEEN FromDate AND ToDate AND PlantId='" + plantId + @"' ) CalanderYearID
                            ,format(LvT.AppliedDate,'dd-MMM-yyyy') LeaveAppliedDate
                             FROM
							 dbo.EmployeeInformation emp
							 LEFT outer JOIN dbo.LeaveTransaction LvT on LvT.EmpSystemID = emp.SystemId
							 LEFT outer JOIN dbo.EmployeeInformation EEEi on LvT.FirstApprovingAuthority = EEEi.SystemId

                             LEFT OUTER JOIN [MST].[ManpowerBudget] PMB ON EMP.BudgetCode=PMB.Id
							 LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                             LEFT OUTER JOIN [ORG].[Entity] E ON PMB.EntityId=E.Id
                             LEFT outer JOIN dbo.LeaveType LT ON LvT.LTSystemID = LT.Id
							 LEFT outer JOIN [HKP].Designation AS Dsg ON Dsg.ID = PR.DesignationID
							 LEFT outer JOIN [HKP].Designation AS Dsgg ON Dsgg.ID = Emp.GivenDesignationID
							 LEFT JOIN [MST].[MaternityLeavePolicy] MP ON MP.Id=LvT.MaternityLeavePolicyId
                             WHERE  IsNull(Lvt.IsApproved,0) = 0
							 AND ISNULL(LvT.SystemID,'')<> ''
                             AND LvT.IsCancel=0
							 AND emp.GroupID = '" + companyGroupId + @"'
                             AND emp.PlantID = '" + plantId + @"' ORDER BY lvt.DateAdded ";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmpBasicInfoInformationForLeaveMA(string companyGroupId, string plantId, bool isControlAdmin, bool isSysAdmin, string employeeId, string companyId,string FirstApprovingAuthority)
        {
            string strSql = string.Empty;
            try
            {
                var str = "";
                str = !isControlAdmin && !isSysAdmin ? @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
                                    (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
                                            where LeaveApproval='" + employeeId + "')))" : @" AND Emp.CompanyId='" + companyId + "'";

                strSql = @"SELECT 0 CheckBoxSelect, emp.SystemId EmployeeID,Lvt.SystemID LvTrnMsID, emp.EmployeeCode,emp.BudgetCode,emp.EmployeeName,emp.EmpType,emp.NationalID,Dsgg.UserName GivenDesignation,E.UserName as Entity,
                             REPLACE(CONVERT(VARCHAR(11), emp.DOJ, 113), ' ', '-') DOJ,
							 LT.UserName LeaveName, LT.Description LeaveDescription,
                             REPLACE(CONVERT(VARCHAR(11), LvT.FromDate, 113), ' ', '-') FromDate,
                             REPLACE(CONVERT(VARCHAR(11), LvT.ToDate, 113), ' ', '-') ToDate, LvT.LeaveDays, LvT.LvReason AS Reason, LvT.ComAssignLvSystemID,LVT.LTSystemID,LVT.SystemID LvTransSystemID
                        ,(SELECT YearlyCalendar.Id
                                 FROM YearlyCalendar WHERE LvT.FromDate BETWEEN FromDate AND ToDate AND PlantId='" + plantId + @"' ) CalanderYearID
                            ,format(LvT.AppliedDate,'dd-MMM-yyyy') LeaveAppliedDate
                             FROM
							 dbo.EmployeeInformation emp
							 LEFT outer JOIN dbo.LeaveTransaction LvT on LvT.EmpSystemID = emp.SystemId
                             LEFT OUTER JOIN [MST].[ManpowerBudget] PMB ON EMP.BudgetCode=PMB.Id
							 LEFT OUTER JOIN [ORG].[Position] PR ON PMB.PositionId=PR.Id
                             LEFT OUTER JOIN [ORG].[Entity] E ON PMB.EntityId=E.Id
                             LEFT outer JOIN dbo.LeaveType LT ON LvT.LTSystemID = LT.Id
							 LEFT outer JOIN [HKP].Designation AS Dsg ON Dsg.ID = Emp.DesignationSystemID
							 LEFT outer JOIN [HKP].Designation AS Dsgg ON Dsgg.ID = Emp.GivenDesignationID
                             WHERE  IsNull(Lvt.IsApproved,0) = 0
							 AND ISNULL(LvT.SystemID,'')<> ''
                             AND LvT.IsCancel=0
							 --AND emp.GroupID = '" + companyGroupId + @"'
                             --AND emp.PlantID = '" + plantId + @"' 
                             AND FirstApprovingAuthority = '"+ FirstApprovingAuthority + @"' AND LvT.FirstApprovingStatus = 0 ";

               return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetYearlyCalendarInfoCmb(string sGroupID, string sPlantID)
        {
            string strSQL;

            try
            {
                strSQL = @"select Id from dbo.YearlyCalendar where '" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' between FromDate and ToDate AND PlantId='" + sPlantID + @"'";







            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
            return _sqlRepository.GetDataCollection(strSQL);
        }//End Function
        public IEnumerable<object> GetYearlyCalendarInfoCmb(string sGroupID, string sPlantID, string sSystemID)
        {
            string strSQL;

            try
            {
                strSQL = @"SELECT Id FROM
                                    (
                                        SELECT Id
                                        , YearNo
                                        , REPLACE(CONVERT(VARCHAR(11), FromDate, 113), ' ', '-') FromDate
                                        ,REPLACE(CONVERT(VARCHAR(11), ToDate, 113), ' ', '-') ToDate
                                        ,(select Id from dbo.YearlyCalendar where '" + DateTime.Now.ToString("dd-MMM-yyyy") + @"' between FromDate and ToDate AND PlantId='" + sPlantID + @"') CalendarYear
                                        FROM YearlyCalendar WHERE PlantID = '" + sPlantID + @"' AND CompanyGroupId = '" + sGroupID + @"'
                                    ) AS A";

                if (sSystemID.Trim() != "")
                {
                    strSQL = strSQL + " WHERE Id = '" + sSystemID + @"'";
                }

                strSQL = strSQL + " ORDER BY YearNo";





            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
            return _sqlRepository.GetDataCollection(strSQL);
        }//End Function

        public void SaveData(LeaveCustomPara para)
        {
            #region local variables

            DataSet dsLvAllo = null;
            DataTable dtLvAllo = null;
            DataRow drLvAllo = null;
            DataView dvLvAllo = null;

            DataSet dsLvTranslwp = null;
            DataTable dtLvTranslwp = null;
            DataRow drLvTranslwp = null;
            DataView dvLvTranslwp = null;

            DataSet dsLvTrans = null;
            DataTable dtLvTrans = null;
            DataRow drLvTrans = null;
            DataView dvLvTrans = null;

            DataSet dsLvTnsDtl = null;
            DataTable dtLvTnsDtl = null;
            DataRow drLvTnsDtl = null;
            DataView dvLvTnsDtl = null;

            DataSet dsOffDayDet = null;
            DataTable dtOffDayDet = null;
            DataView dvOffDayDet = null;

            DataSet dsAttnData = null;
            DataTable dtAttnData = null;
            DataRow drAttnData = null;
            DataView dvAttnData = null;

            DataSet dsIsAvailLv = null;
            DataSet dsLvCount = null;

            DataSet dsLVPolicy = null;

            DataSet dsEmpTagLvPolMst = null;
            DataTable dtEmpTagLvPolMst = null;
            DataRow drEmpTagLvPolMst = null;
            DataView dvEmpTagLvPolMst = null;

            DataSet dsGrd = null;

            bool bAdvLvTrsAlw = false;
            bool bPrecedingW = false;
            bool bPrecedingH = false;
            bool bSucceedignW = false;
            bool bSucceedignH = false;
            bool bBetweenW = false;
            bool bBetweenH = false;
            bool isAvailExceptionAllowed = false;
            bool isProofDocReq = false;
            bool isMaxAtaRowExp = false;

            DateTime dtFmDate = bplib.clsWebLib.DateData_DBToApp(para.FromDate, bplib.clsWebLib.DB_DATE_FORMAT);
            DateTime dtToDate = bplib.clsWebLib.DateData_DBToApp(para.ToDate, bplib.clsWebLib.DB_DATE_FORMAT);

            DateTime dtLvStartDateForW = dtFmDate;
            DateTime dtLvStartDateForH = dtFmDate;

            DateTime dtLvEndDateForW = dtToDate;
            DateTime dtLvEndDateForH = dtToDate;

            int iSelectLvDays = 0;
            int iPrecedingW = 0;
            int iPrecedingH = 0;
            int iSucceedignW = 0;
            int iSucceedignH = 0;

            clsLeaveTransactionEmpWise objLvTrnt = null;
            clsStaticInfo objStatic = null;

            clsLeaveAllocationEmp objLvAlloEmp = null;
            bplib.clsGenID objGenID = null;

            string idFromDB = "";
            string systemID = "";

            bool DATA_OK = false;
            //int iAppliedLv = 0;
            decimal iAppliedLv = 0;

            #endregion local variables

            try
            {

                objLvTrnt = new clsLeaveTransactionEmpWise();
                objStatic = new clsStaticInfo();
                objLvAlloEmp = new clsLeaveAllocationEmp();

               

                if (DATA_OK == false)
                {
                    var ep = "'" + para.EmpSystemId + "'";

                    AttendanceProcessAplos ob = new AttendanceProcessAplos();
                    if (para.AvoidAttendanceLock == false)
                    {
                        ob.LockValidation(para.PlantId, dtFmDate.ToString("dd-MMM-yyyy"), dtFmDate.ToString("dd-MMM-yyyy"), ep);
                    }
                    
                    if (string.IsNullOrEmpty(para.PlantId) == true)
                    {

                        Exception ex = new Exception("Select Plant First...");
                        throw (ex);
                    }


                    #region Check Leave in Same Date

                    if (objLvTrnt.CheckLvTransactionInSameDate(para.GroupId, para.PlantId, para.LvTransSystemID, para.EmpSystemId, para.FromDate.ToString("dd-MMM-yyyy"), para.ToDate.ToString("dd-MMM-yyyy")) == false)
                    {

                        Exception ex = new Exception("Leave transaction already have in the same date range...");
                        throw (ex);
                    }


                    #endregion Check Leave in Same Date

                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {
                    #region Generate Sr No
                    objGenID = new bplib.clsGenID();
                    if (para.LvTransSystemID == "")
                    {

                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "LeaveTrans", out idFromDB);
                        systemID = "LVTNS-" + idFromDB;
                        para.LvTransSystemID = systemID.Trim();
                    }
                    string _detail_seed = string.Empty;
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "LeaveTransDetail", out _detail_seed);
                    #endregion Generate Sr No

                    #region DataSet

                    #region Leave Policy Variables

                    string strLvPolSysID = "";

                    if (para.lblLeavePolicyId != "")
                    {
                        strLvPolSysID = para.lblLeavePolicyId;
                    }
                    string strYrCalSysID = "";
                    if (para.CalanderYearID != "")
                    {
                        strYrCalSysID = para.CalanderYearID;
                    }

                    objLvTrnt.LeavePolicyDetailInforForSelectedLeaveType(para.GroupId, para.PlantId, para.LvTransSystemID, strLvPolSysID, para.FromDate.ToString("dd-MMM-yyyy"), out dsLVPolicy);
                    if (dsLVPolicy.Tables[0].Rows.Count > 0)
                    {
                        iSelectLvDays = Convert.ToInt32(dsLVPolicy.Tables[0].Rows[0]["LeaveDays"].ToString());
                        bAdvLvTrsAlw = Convert.ToBoolean(dsLVPolicy.Tables[0].Rows[0]["IsExcessAllow"].ToString());

                        bPrecedingW = Convert.ToBoolean(dsLVPolicy.Tables[0].Rows[0]["IsPrecedingWeekoff"].ToString());
                        bPrecedingH = Convert.ToBoolean(dsLVPolicy.Tables[0].Rows[0]["IsPrecedingHoliday"].ToString());
                        bSucceedignW = Convert.ToBoolean(dsLVPolicy.Tables[0].Rows[0]["IsSucceedignWeekoff"].ToString());
                        bSucceedignH = Convert.ToBoolean(dsLVPolicy.Tables[0].Rows[0]["IsSucceedignHoliday"].ToString());
                        bBetweenW = Convert.ToBoolean(dsLVPolicy.Tables[0].Rows[0]["InBetweenWeekoff"].ToString());
                        bBetweenH = Convert.ToBoolean(dsLVPolicy.Tables[0].Rows[0]["InBetweenHoliday"].ToString());
                        isAvailExceptionAllowed = Convert.ToBoolean(dsLVPolicy.Tables[0].Rows[0]["IsAvailExceptionAllowedOnSpecialAppeal"].ToString());
                        isProofDocReq = Convert.ToBoolean(dsLVPolicy.Tables[0].Rows[0]["IsProofDocRequired"].ToString());
                        isMaxAtaRowExp = Convert.ToBoolean(dsLVPolicy.Tables[0].Rows[0]["IsExcessAllow"].ToString());
                    }

                    objLvTrnt.GetYearlyOffDayDetails(para.GroupId, para.PlantId, strYrCalSysID, out dsOffDayDet);
                    dtOffDayDet = dsOffDayDet.Tables[0];

                    if (dsOffDayDet.Tables[0].Rows.Count > 0)
                    {
                        if (bPrecedingW == true)
                        {
                            dtLvStartDateForW = dtFmDate.AddDays(-iSelectLvDays);
                            dvOffDayDet = new DataView();
                            dvOffDayDet.Table = dtOffDayDet;

                            while (dtLvStartDateForW < dtFmDate)
                            {
                                dvOffDayDet.RowFilter = "OffDayDate = '" + dtLvStartDateForW + "' AND OffDayType = 'W'";
                                if (dvOffDayDet.Count > 0)
                                {
                                    iPrecedingW += 1;
                                }
                                else
                                {
                                    iPrecedingW = 0;
                                }
                                dtLvStartDateForW = dtLvStartDateForW.AddDays(1);
                            }
                        }
                        dtLvStartDateForW = dtFmDate.AddDays(-iPrecedingW);
                        if (bPrecedingH == true)
                        {
                            dtLvStartDateForH = dtFmDate.AddDays(-iSelectLvDays);
                            dvOffDayDet = new DataView();
                            dvOffDayDet.Table = dtOffDayDet;

                            while (dtLvStartDateForH < dtFmDate)
                            {
                                dvOffDayDet.RowFilter = "OffDayDate = '" + dtLvStartDateForH + "' AND OffDayType = 'H'";
                                if (dvOffDayDet.Count > 0)
                                {
                                    iPrecedingH += 1;
                                }
                                else
                                {
                                    iPrecedingH = 0;
                                }
                                dtLvStartDateForH = dtLvStartDateForH.AddDays(1);
                            }
                        }
                        dtLvStartDateForH = dtFmDate.AddDays(-iPrecedingH);
                        DateTime dtStartToDate = dtToDate.AddDays(1);
                        if (bSucceedignW == true)
                        {
                            iSucceedignW = 0;

                            dtLvEndDateForW = dtToDate.AddDays(iSelectLvDays);
                            dvOffDayDet = new DataView();
                            dvOffDayDet.Table = dtOffDayDet;

                            while (dtStartToDate < dtLvEndDateForW)
                            {
                                dvOffDayDet.RowFilter = "OffDayDate = '" + dtStartToDate + "' AND OffDayType = 'W'";
                                if (dvOffDayDet.Count > 0)
                                {
                                    iSucceedignW += 1;
                                }
                                else
                                {
                                    break;
                                }
                                dtStartToDate = dtStartToDate.AddDays(1);
                            }
                        }
                        dtLvEndDateForW = dtToDate.AddDays(iSucceedignW);

                        dtStartToDate = dtToDate.AddDays(1);
                        if (bSucceedignH == true)
                        {
                            iSucceedignH = 0;

                            dtLvEndDateForH = dtToDate.AddDays(iSelectLvDays);
                            dvOffDayDet = new DataView();
                            dvOffDayDet.Table = dtOffDayDet;

                            while (dtStartToDate < dtLvEndDateForH)
                            {
                                dvOffDayDet.RowFilter = "OffDayDate = '" + dtStartToDate + "' AND OffDayType = 'H'";
                                if (dvOffDayDet.Count > 0)
                                {
                                    iSucceedignH += 1;
                                }
                                else
                                {
                                    break;
                                }
                                dtStartToDate = dtStartToDate.AddDays(1);
                            }
                        }

                        dtLvEndDateForH = dtToDate.AddDays(iSucceedignH);
                    }

                    #endregion Leave Policy Variables

                    objLvAlloEmp.GetLeaveAllocat(para.GroupId, para.PlantId, strYrCalSysID, strLvPolSysID, out dsLvAllo);
                    dtLvAllo = dsLvAllo.Tables[0];
                    dvLvAllo = new DataView();
                    dvLvAllo.Table = dtLvAllo;

                    objLvTrnt.GetLvTransInfo(para.GroupId, para.PlantId, para.EmpSystemId, "", out dsLvTrans);
                    dtLvTrans = dsLvTrans.Tables[0];
                    dvLvTrans = new DataView();

                    objLvTrnt.GetLeaveTypeCategory(para.LTSystemID, out dsLvTranslwp);
                    if (dsLvTranslwp.Tables[0].Rows.Count < 0)
                    {
                        throw new Exception(" Data not Found.");
                    }


                    objLvTrnt.GetLvTransDetInfo(para.LvTransSystemID, "", out dsLvTnsDtl);
                    dtLvTnsDtl = dsLvTnsDtl.Tables[0];
                    dvLvTnsDtl = new DataView();

                    DateTime fromDate = Convert.ToDateTime(dtFmDate.ToString("dd-MMM-yyyy")).AddDays(-1);
                    DateTime toDate = Convert.ToDateTime(dtToDate.ToString("dd-MMM-yyyy")).AddDays(1);


                    objLvTrnt.GetAttdnData(para.GroupId, para.GroupId, fromDate, toDate, out dsAttnData);

                    objLvTrnt.SelectedLvLeaveDaysCountForEmp(para.GroupId, para.PlantId, para.EmpSystemId, dtFmDate.ToString("dd-MMM-yyyy"), dtToDate.ToString("dd-MMM-yyyy"), para.LvTransSystemID, para.LTSystemID, out dsLvCount);
                    DataSet dsLeaveDayCounted = null;
                    decimal _LeaveDayCounted = 0;
                    bool IsFirstHalfLeave = false;
                    objLvTrnt.LeaveDayCount(para.LvTransSystemID, out dsLeaveDayCounted);
                    if (dsLeaveDayCounted.Tables[0].Rows.Count > 0)
                    {
                        _LeaveDayCounted = Convert.ToDecimal(dsLeaveDayCounted.Tables[0].Rows[0]["LeaveDays"].ToString());
                    }


                    if (dsLvCount.Tables[0].Rows.Count > 0)

                    { iAppliedLv = Convert.ToDecimal(dsLvCount.Tables[0].Rows[0]["LeaveDays"].ToString()); }

                    objLvTrnt.GetLvPolMstTagEmp(para.EmpSystemId, strLvPolSysID, out dsEmpTagLvPolMst);
                    dtEmpTagLvPolMst = dsEmpTagLvPolMst.Tables[0];
                    dvEmpTagLvPolMst = new DataView();

                    #endregion DataSet

                    #region SystemID For Detail Table

                    //int SrNoDet = 0;
                    //int SrNoDetTmp = 0;

                    //if (dsLvTnsDtl.Tables[0].Rows.Count > 0)
                    //{
                    //    for (int j = 0; j < dsLvTnsDtl.Tables[0].Rows.Count; j++)
                    //    {
                    //        int sysIdLen = para.LvTransSystemID.Length + 1;

                    //        SrNoDetTmp = SrNoDet;
                    //        SrNoDet = Convert.ToInt32((dsLvTnsDtl.Tables[0].Rows[j]["SystemID"].ToString()).Substring(sysIdLen));

                    //        if (SrNoDetTmp > SrNoDet)
                    //        {
                    //            SrNoDet = SrNoDetTmp;
                    //        }
                    //    }
                    //}

                    #endregion SystemID For Detail Table

                    #region Save LeaveTransactionDetails

                    int i = 0;
                    int LVcount = 0;
                    int LvAvailed = 0;
                    int ichkDate = 0;

                    TimeSpan ts;
                    DateTime dtFmLTD = dtFmDate;
                    DateTime dtToLTD = dtToDate;

                    if (bPrecedingW == true)
                    {
                        ts = dtFmLTD - dtLvStartDateForW;
                        ichkDate = ts.Days;
                        if (ichkDate > 0)
                        {
                            dtFmLTD = dtLvStartDateForW;
                        }
                    }
                    if (bPrecedingH == true)
                    {
                        ts = dtFmLTD - dtLvStartDateForH;
                        ichkDate = ts.Days;
                        if (ichkDate > 0)
                        {
                            dtFmLTD = dtLvStartDateForH;
                        }
                    }
                    if (bSucceedignW == true)
                    {
                        ts = dtToLTD - dtLvEndDateForW;
                        ichkDate = ts.Days;
                        if (ichkDate < 0)
                        {
                            dtToLTD = dtLvEndDateForW;
                        }
                    }
                    if (bSucceedignH == true)
                    {
                        ts = dtToLTD - dtLvEndDateForH;
                        ichkDate = ts.Days;
                        if (ichkDate < 0)
                        {
                            dtToLTD = dtLvEndDateForH;
                        }
                    }

                    while (dtFmLTD <= dtToLTD)
                    {
                        i += 1;
                        //string strDtlID = para.LvTransSystemID + "-" + (SrNoDet + i).ToString();                        
                        string strDtlID = _detail_seed + "_" + i;
                        string sDayType = "";
                        string sDayStatus = "";
                        string LVDayStatus = "";
                        bool IsAvailed = false;

                        #region DayType, DayStatus AND Leave Count

                        dvOffDayDet = new DataView();
                        dvOffDayDet.Table = dtOffDayDet;
                        dvOffDayDet.RowFilter = "OffDayDate = '" + dtFmLTD + "'";
                        if (dvOffDayDet.Count > 0)
                        {
                            for (int OFD = 0; OFD < dvOffDayDet.Count; OFD++)
                            {
                                sDayType += dvOffDayDet[OFD].Row["OffDayType"].ToString();
                            }
                            TimeSpan ts01 = dtFmDate - dtFmLTD;
                            TimeSpan ts02 = dtToDate - dtFmLTD;
                            int iLessDate = ts01.Days;
                            int iMoreDate = ts02.Days;

                            if (iLessDate > 0)
                            {
                                if (bPrecedingW == true & bPrecedingH == true & sDayType == "WH")
                                {
                                    sDayStatus = "WHLV"; LVDayStatus = "LV"; LVcount += 1;
                                }
                                else if (bPrecedingW == true & bPrecedingH == true & sDayType == "HW")
                                {
                                    sDayStatus = "HWLV"; LVDayStatus = "LV"; LVcount += 1;
                                }
                                else if (bPrecedingW == true & (sDayType == "WH" || sDayType == "HW" || sDayType == "W"))
                                {
                                    sDayType = "W"; sDayStatus = "WLV"; LVDayStatus = "LV"; LVcount += 1;
                                }
                                else if (bPrecedingH == true & (sDayType == "WH" || sDayType == "HW" || sDayType == "H"))
                                {
                                    sDayType = "H"; sDayStatus = "HLV"; LVDayStatus = "LV"; LVcount += 1;
                                }
                            }
                            else if (iMoreDate < 0)
                            {
                                if (bSucceedignW == true & bSucceedignH == true & sDayType == "WH")
                                {
                                    sDayStatus = "WHLV"; LVDayStatus = "LV"; LVcount += 1;
                                }
                                else if (bSucceedignW == true & bSucceedignH == true & sDayType == "HW")
                                {
                                    sDayStatus = "HWLV"; LVDayStatus = "LV"; LVcount += 1;
                                }
                                else if (bSucceedignW == true & (sDayType == "WH" || sDayType == "HW" || sDayType == "W"))
                                {
                                    sDayType = "W"; sDayStatus = "WLV"; LVDayStatus = "LV"; LVcount += 1;
                                }
                                else if (bSucceedignH == true & (sDayType == "WH" || sDayType == "HW" || sDayType == "H"))
                                {
                                    sDayType = "H"; sDayStatus = "HLV"; LVDayStatus = "LV"; LVcount += 1;
                                }
                            }
                            else
                            {
                                dtFmDate = dtFmDate.AddDays(1);
                                if (bBetweenW == true & bBetweenH == true & sDayType == "WH")
                                {
                                    sDayStatus = "WHLV"; LVDayStatus = "LV"; LVcount += 1;
                                }
                                else if (bBetweenW == true & bBetweenH == true & sDayType == "HW")
                                {
                                    sDayStatus = "HWLV"; LVDayStatus = "LV"; LVcount += 1;
                                }
                                else if (bBetweenW == true & (sDayType == "WH" || sDayType == "HW" || sDayType == "W"))
                                {
                                    sDayType = "W"; sDayStatus = "WLV"; LVDayStatus = "LV"; LVcount += 1;
                                }
                                else if (bBetweenH == true & (sDayType == "WH" || sDayType == "HW" || sDayType == "H"))
                                {
                                    sDayType = "H"; sDayStatus = "HLV"; LVDayStatus = "LV"; LVcount += 1;
                                }
                                else
                                { sDayStatus = sDayType; LVDayStatus = "LV"; LVcount += 1; }

                            }
                        }
                        else if (sDayType == "")
                        { sDayType = "NW"; sDayStatus = "LV"; LVDayStatus = "LV"; LVcount += 1; }
                        else
                        { sDayStatus = sDayType; LVDayStatus = sDayType; }

                        #endregion DayType, DayStatus AND Leave Count

                        #region Update AttdnData

                        dtAttnData = dsAttnData.Tables[0];
                        dvAttnData = new DataView();
                        dvAttnData.Table = dtAttnData;
                        dvAttnData.RowFilter = "EmpSystemID = '" + para.EmpSystemId + "' AND WorkDate = '" + dtFmLTD + "'";
                        if (dvAttnData.Count == 1)
                        {


                            if (sDayStatus.Contains("LV"))
                            { LvAvailed += 1; }

                            if (dvAttnData[0].Row["DayStatus"].ToString() == "P")
                            {
                                if (sDayStatus == "WLV")
                                {
                                    sDayStatus = "WLVP";
                                }
                                else if (sDayStatus == "HLV")
                                {
                                    sDayStatus = "HLVP";
                                }
                                else if (sDayStatus == "WHLV")
                                {
                                    sDayStatus = "WHLVP";
                                }
                                else if (sDayStatus == "HWLV")
                                {
                                    sDayStatus = "HWLVP";
                                }
                                else
                                {
                                    sDayStatus = "LVP";
                                }

                                IsAvailed = true;
                            }
                            else if (dvAttnData[0].Row["DayStatus"].ToString() == "L")
                            {
                                if (sDayStatus == "WLV")
                                {
                                    sDayStatus = "WLVL";
                                }
                                else if (sDayStatus == "HLV")
                                {
                                    sDayStatus = "HLVL";
                                }
                                else if (sDayStatus == "WHLV")
                                {
                                    sDayStatus = "WHLVL";
                                }
                                else if (sDayStatus == "HWLV")
                                {
                                    sDayStatus = "HWLVL";
                                }
                                else
                                {
                                    sDayStatus = "LVL";
                                }
                                IsAvailed = true;
                            }
                            else if (dvAttnData[0].Row["DayStatus"].ToString() == "W")
                            {

                                sDayStatus = "WLV";
                                IsAvailed = true;
                            }
                            else if (dvAttnData[0].Row["DayStatus"].ToString() == "WP")
                            {
                                IsAvailed = true;
                                sDayStatus = "WP";
                            }
                            else if (dvAttnData[0].Row["DayStatus"].ToString() == "WL")
                            {
                                IsAvailed = true;
                                sDayStatus = "WL";
                            }
                            else if (dvAttnData[0].Row["DayStatus"].ToString() == "H")
                            {
                                IsAvailed = true;
                                sDayStatus = "HLV";

                            }
                            else if (dvAttnData[0].Row["DayStatus"].ToString() == "HP")
                            {
                                IsAvailed = true;
                                sDayStatus = "HPLV";

                            }
                            else if (dvAttnData[0].Row["DayStatus"].ToString() == "HL")
                            {
                                IsAvailed = true;
                                sDayStatus = "HLLV";

                            }

                            else
                            {
                                IsAvailed = true;
                            }



                            drAttnData = dvAttnData[0].Row;
                            drAttnData.BeginEdit();

                            if (_LeaveDayCounted == (decimal)0.5)
                            {
                                IsAvailed = true;
                                drAttnData["IsHalfDayLeave"] = 1;
                                if (dtLvTnsDtl.Rows.Count > 0)
                                {
                                    var v = dtLvTnsDtl.Rows[0]["IsFirstHalf"].ToString();
                                    IsFirstHalfLeave = bplib.clsWebLib.GetBoolData(v);
                                }
                            }
                            else
                            {
                                IsAvailed = true;
                                drAttnData["DayStatus"] = "LV";

                            }

                            if (IsAvailed == false)
                            {
                                drAttnData["LTSystemID"] = DBNull.Value;
                            }
                            else
                            {
                                drAttnData["LTSystemID"] = bplib.clsWebLib.RetValidLen(para.LTSystemID);
                                if (dsLvTranslwp.Tables[0].Rows[0]["LeaveType"].ToString().ToUpper() == "LEAVE WITHOUT PAY")
                                {
                                    drAttnData["IsLWP"] = true;
                                }
                                else
                                {
                                    drAttnData["IsLWP"] = false;
                                }
                                NullifyNegativeOT(_LeaveDayCounted, IsFirstHalfLeave, ref drAttnData);
                            }
                            //OT nullify

                            drAttnData.EndEdit();
                        }


                        #endregion Update AttdnData

                        #region Update LeaveTrnDetail



                        DataView dvLvTransDet = new DataView();


                        dvLvTransDet.Table = dsLvTnsDtl.Tables[0];
                        dvLvTransDet.RowFilter = "WorkDate = '" + dtFmLTD + "'";

                        if (dvLvTransDet.Count > 0)
                        {
                            drLvTnsDtl = dvLvTransDet[0].Row;
                            drLvTnsDtl.BeginEdit();
                            drLvTnsDtl["LeaveStatus"] = bplib.clsWebLib.RetValidLen(LVDayStatus);

                            drLvTnsDtl["IsAvailed"] = true;

                            drLvTnsDtl.EndEdit();
                        }
                        dtFmLTD = dtFmLTD.AddDays(1);

                        #endregion Update LeaveTrnDetail


                    }

                    #endregion Save LeaveTransactionDetails

                    //lbliLvDays.Text = LVcount.ToString();

                    //lblLvDays.Text = lbliLvDays.Text.ToString();

                    decimal intPreLv = 0;

                    #region Save LeaveTransaction

                    dvLvTrans.Table = dtLvTrans;
                    dvLvTrans.RowFilter = "SystemID = '" + para.LvTransSystemID + "'";
                    if (LVcount > 0)
                    {
                        if (dvLvTrans.Count == 0)
                        {
                            //objStatic.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.CREATE);
                            drLvTrans = dtLvTrans.NewRow();
                            UpdateLeaveTransactionDataRowForApproval("ADDNEW", para.UserId, ref drLvTrans);
                            dtLvTrans.Rows.Add(drLvTrans);
                        }
                        else
                        {
                            //objStatic.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.EDIT);
                            drLvTrans = dvLvTrans[0].Row;
                            intPreLv = Convert.ToDecimal(dvLvTrans[0].Row["LeaveDays"].ToString());
                            drLvTrans.BeginEdit();
                            UpdateLeaveTransactionDataRowForApproval("EDIT", para.UserId, ref drLvTrans);
                            drLvTrans.EndEdit();
                        }
                    }
                    else
                    {
                        //ddlYear.Focus();
                        Exception ex = new Exception("No Normal Woking day found. Please Choose different date...");
                        throw (ex);
                    }

                    #endregion Save LeaveTransaction

                    #region Save LeaveAllocation

                    int LvDtlCount = 0;
                    int iBalanceLv = 0;
                    int canAllowed = 0;


                    #endregion Save LeaveAllocation

                    objLvTrnt.SaveDataSets(para.EmpSystemId.ToString(), para.FromDate.ToString("dd-MMM-yyyy"), para.ToDate.ToString("dd-MMM-yyyy"), dsLvTrans, dsAttnData, dsLvTnsDtl);

                    string _leaveType = string.Empty;
                    if(dsLvTrans.Tables[0].Rows.Count>0)
                    {
                        _leaveType = dsLvTrans.Tables[0].Rows[0]["LTSystemID"].ToString();
                    }
                    else
                    {
                        throw new Exception("Leave Type is missing !!!");
                    }

                    #region AttendanceProcess  
                    clsEmpWiseLeavePolicyInfo _obj_POD = new clsEmpWiseLeavePolicyInfo(para.PlantId);
                    PolicySandwichVM _ps= _obj_POD._getSandwichInfo(para.EmpSystemId, _leaveType);

                    var empids = "'" + para.EmpSystemId + "'";

                  
                     if ((_ps.InBetweenHoliday && _ps.IsNoLeaveOnH ==false && _ps.IsAsperEntryOnH==false) || (_ps.InBetweenWeekoff && _ps.IsNoLeaveOnW==false && _ps.IsAsperEntryOnW==false))//sandwich
                    {
                        clsAttendance.AttendanceProcessAplos oap = new AttendanceProcessAplos();
                        oap.ProcessAttendanceWithSandwich(para.GroupId, para.PlantId, para.UserId, para.FromDate.ToString("dd-MMM-yyyy"), para.ToDate.ToString("dd-MMM-yyyy"), empids);

                    }
                   else// if ((_ps.IsNoLeaveOnH || _ps.IsNoLeaveOnW) || (_ps.IsAsperEntryOnH || _ps.IsAsperEntryOnW))
                    {
                        DateTime FromDate = Convert.ToDateTime(para.FromDate);
                        DateTime ToDate = Convert.ToDateTime(para.ToDate);
                        while (FromDate <= ToDate)
                        {
                            clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                            obj.SaveTotal(para.PlantId, FromDate.ToString("dd-MMM-yyyy"), empids, true);
                            FromDate = FromDate.AddDays(1);
                        }
                    }
                    //else
                    //{
                    //    throw new Exception("Leave Policy is incomplete !!!");
                    //}
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objLvTrnt = null;
                objStatic = null;
            }
        }//End Function
        private void NullifyNegativeOT(decimal _LeaveDayCounted, bool IsFirstHalf, ref DataRow drLocal)
        {
            try
            {
                //drLocal["LTSystemID"]
                var InTimeOT = drLocal["OTIntime"].ToString();
                var OutTimeOT = drLocal["OTOuttime"].ToString();
                if (_LeaveDayCounted == (decimal)0.5)//if halfday leave
                {

                    var tot = Convert.ToDecimal(bplib.clsWebLib.GetNumData(InTimeOT)) + Convert.ToDecimal(bplib.clsWebLib.GetNumData(OutTimeOT));
                    if (tot < 0)
                    {
                        tot = 0;
                    }
                    drLocal["OTHr"] = tot;
                }
                else//if full day leave
                {
                    //OTIntime	OTOuttime
                    //OTHr
                    drLocal["OTHr"] = 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //private void UpdateLeaveTransactionDataRow(string OPN_FLAG, ref DataRow drLocal)
        //{
        //    try
        //    {
        //        if (OPN_FLAG == "ADDNEW")
        //        {
        //            drLocal["SystemID"] = bplib.clsWebLib.RetValidLen(lblLvTransSystemID.Text.Trim());

        //            drLocal["AddedBy"] = bplib.clsWebLib.RetValidLen(((string)Session["USER"]));
        //            drLocal["DateAdded"] = DateTime.Now;
        //        }

        //        drLocal["GroupID"] = bplib.clsWebLib.RetValidLen(lblGroupID.Text.ToString().Trim());
        //        drLocal["PlantID"] = bplib.clsWebLib.RetValidLen(ddlPlant.SelectedValue.ToString().Trim());

        //        drLocal["EmpSystemID"] = bplib.clsWebLib.RetValidLen(lblEmpSystemId.Text.Trim());
        //        drLocal["LTSystemID"] = bplib.clsWebLib.RetValidLen(ddlLeaveType.SelectedValue.ToString().Trim());
        //        //drLocal["ApprovalDate"] = System.DateTime.Now;

        //        drLocal["FromDate"] = lblFromDate.Text.Trim();
        //        drLocal["ToDate"] = lblToDate.Text.Trim();
        //        drLocal["LeaveDays"] = bplib.clsWebLib.GetNumData(lbliLvDays.Text.Trim());
        //        drLocal["AppliedDate"] = lblApplyDate.Text.Trim();
        //        drLocal["LvReason"] = bplib.clsWebLib.RetValidLen(lblReason.Text.Trim(), 200);

        //        drLocal["UpdatedBy"] = bplib.clsWebLib.RetValidLen(((string)Session["USER"]));
        //        drLocal["DateUpdated"] = DateTime.Now;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        //
        //    }
        //}//End Function

        private void UpdateLeaveTransactionDataRowForApproval(string OPN_FLAG, string USER, ref DataRow drLocal)
        {
            try
            {
                drLocal["IsApproved"] = true;
                drLocal["ApprovedBy"] = bplib.clsWebLib.RetValidLen(USER);
                drLocal["ApprovedDate"] = DateTime.Now;
                drLocal["UpdatedBy"] = bplib.clsWebLib.RetValidLen(USER);
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function

        private void UpdateLeaveTransactionDataRowForUnApproval(string OPN_FLAG, string USER, ref DataRow drLocal)
        {
            try
            {
                drLocal["IsApproved"] = false;
                drLocal["ApprovedBy"] = bplib.clsWebLib.RetValidLen(USER);
                drLocal["ApprovedDate"] = DateTime.Now;
                drLocal["UpdatedBy"] = bplib.clsWebLib.RetValidLen(USER);
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function

        public void Reject(LeaveCustomPara para)
        {

            DataSet dsLocal = null;
            DataSet dsLvTrans = null;
            DataTable dtLvTrans = null;
            DataRow drLvTrans = null;
            DataView dvLvTrans = null;


            clsLeaveTransactionEmpWise objLvTrnt = null;
            clsStaticInfo objStatic = null;
            try
            {
                //pmain = PanelRejectLeave;
                if (string.IsNullOrEmpty(para.CancelationReason))
                {
                    throw new Exception("Reject Reason is required.");
                }

                objLvTrnt = new clsLeaveTransactionEmpWise();
                objStatic = new clsStaticInfo();

                objLvTrnt.GetLvTransInfo(para.GroupId, para.PlantId, para.EmpSystemId, "", out dsLvTrans);
                dtLvTrans = dsLvTrans.Tables[0];
                dvLvTrans = new DataView();

                #region Save LeaveTransaction

                dvLvTrans.Table = dtLvTrans;
                dvLvTrans.RowFilter = "SystemID = '" + para.LvTransSystemID + "'";

                if (dvLvTrans.Count == 0)
                {
                    //objStatic.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.CREATE);
                    drLvTrans = dtLvTrans.NewRow();
                    UpdateLeaveTransactionDataRowForReject("ADDNEW", para.UserId, para.CancelationReason, ref drLvTrans);
                    dtLvTrans.Rows.Add(drLvTrans);
                }
                else
                {
                    //objStatic.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.EDIT);
                    drLvTrans = dvLvTrans[0].Row;
                    drLvTrans.BeginEdit();
                    UpdateLeaveTransactionDataRowForReject("EDIT", para.UserId, para.CancelationReason, ref drLvTrans);
                    drLvTrans.EndEdit();
                }




                objLvTrnt.SaveDataSets(dsLvTrans);

                //objLvTrnt.GetSysIdWiseEmpBasicInfoInformationForLeave(para.GroupId, para.CompanyId, para.PlantId, "", out dsLocal);

                //SetList(dsLocal, dgLvTransDtl, PanLvTransDtl);
                //LoadGrdAvailedLvDetails();
                //pmain = PanelFactory;
                //pmain.Visible = true;
                //PanelRejectLeave.Visible = false;
                //lblPanel.Text = string.Empty;
                //LeaveEntryPartClear();
                #endregion Save LeaveTransaction

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }
        //private void SetList(DataSet ds, DataGrid dg, Panel p)
        //{
        //    try
        //    {
        //        if (ds.Tables[0].Rows.Count > 0)
        //        {
        //            dg.DataSource = ds.Tables[0];
        //            dg.DataBind();
        //            p.Visible = true;
        //            dg.Visible = true;
        //        }
        //        else
        //        {
        //            dg.DataSource = null;
        //            dg.DataBind();
        //            p.Visible = false;
        //            dg.Visible = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        private void UpdateLeaveTransactionDataRowForReject(string OPN_FLAG, string USER, string CancelationReason, ref DataRow drLocal)
        {
            try
            {

                drLocal["IsCancel"] = true;
                drLocal["CancelationReason"] = CancelationReason;
                drLocal["CancelationDate"] = DateTime.Now;
                drLocal["CancelBy"] = bplib.clsWebLib.RetValidLen(USER);
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function
    }
    public class LeaveCustomPara
    {

        public string EmpSystemId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string CalanderYearID { get; set; }
        public string SystemID { get; set; }
        public string PlantId { get; set; }
        public string GroupId { get; set; }
        public string LvTransSystemID { get; set; }
        public string lblLeavePolicyId { get; set; }
        public string UserId { get; set; }
        public string LTSystemID { get; set; }
        public string CancelationReason { get; set; }
        public string CompanyId { get; set; }
        public bool AvoidAttendanceLock { get; set; } = false;

    }
    public class LeaveVM
    {
        public bool isNoBenefit { get; set; }

        public bool CheckBoxSelect { get; set; }
        public string EmployeeID { get; set; }
        public string LvTrnMsID { get; set; }
        public string EmployeeCode { get; set; }
        public string BudgetCode { get; set; }
        public string EmployeeName { get; set; }
        public string EmpType { get; set; }
        public string NationalID { get; set; }
        public string GivenDesignation { get; set; }
        public string Entity { get; set; }
        public string DOJ { get; set; }
        public string LeaveName { get; set; }
        public string LeaveDescription { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string LeaveDays { get; set; }
        public string Reason { get; set; }
        public string ComAssignLvSystemID { get; set; }
        public string LTSystemID { get; set; }
        public string LvTransSystemID { get; set; }
        public string CalanderYearID { get; set; }
        public bool MPolicyId { get; set; }
        
        #region New Fields
        public string UserId { get; set; }
        public string PlantId { get; set; }
        public string GroupID { get; set; }
        public string CompanyId { get; set; }
     
        #endregion
    }
}
