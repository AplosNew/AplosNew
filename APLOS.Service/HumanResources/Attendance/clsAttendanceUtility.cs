using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

//namespace OTSBD.clsAttendance
//{
//    public enum NotificationType
//    {
//        Attendance,

//        Salary,
//        SalaryDisbursement,
//        SalaryApproval,
//        SalaryApprovalRollback,

//        Promotion,
//        PromotionRollback,

//        Increment,
//        IncrementRollback,

//        GeneralAnnouncement,
//        Holiday,
//        Birthday
//    }
//    public static class DecimalHelper
//    {
//        public static string ToHexString(this Decimal dec)
//        {
//            var sb = new StringBuilder();
//            while (dec > 1)
//            {
//                var r = dec % 16;
//                dec /= 16;
//                sb.Insert(0, ((int)r).ToString("X"));
//            }
//            return sb.ToString();
//        }
//    }//End Function
//    public class TableConvert
//    {
//        static sbyte[] unhex_table =
//      { -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
//       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
//       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
//       , 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,-1,-1,-1,-1,-1,-1
//       ,-1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1
//       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
//       ,-1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1
//       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
//      };

//        public static int Convert(string hexNumber)
//        {
//            int decValue = unhex_table[(byte)hexNumber[0]];
//            for (int i = 1; i < hexNumber.Length; i++)
//            {
//                decValue *= 16;
//                decValue += unhex_table[(byte)hexNumber[i]];
//            }
//            return decValue;
//        }
//    }//End Function
//}
public class clsAttendanceUtility
{
    public void GetUnprocessedDateList(string sGroupID, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {//
            strSql = @"select distinct A.pdate from AttdnRawData A 
                            INNER JOIN
                            (
                            SELECT EmpSystemID, max(EffectiveDate) EffectiveDate FROM dbo.EmployeeShiftAssign
                            where EffectiveDate<='" + sAttnDate + @"'
                            group by EmpSystemID
                            ) B on A.LogDownLoadNum = B.EmpSystemID
                            where A.pdate<='" + sAttnDate + @"' and A.ProcessedFlag=0 
                            and A.groupid='" + sGroupID + @"' 
                           order by A.pdate";

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
    public void GetDateWiseEmployeeList(string sGroupID, string sAttnDate, out DataSet dsRef)
    {
        ConnectionManager.DAL.ConManager objCon;
        string strSql = string.Empty;

        try
        {
            strSql = @"SELECT A.* FROM AttdnRawData A 
                            INNER JOIN
                            (
                             SELECT EmpSystemID, MAX(EffectiveDate) EffectiveDate FROM dbo.EmployeeShiftAssign
                             WHERE EffectiveDate <= '" + sAttnDate + @"'
                             GROUP BY EmpSystemID
                            ) B on A.LogDownLoadNum = B.EmpSystemID
                            WHERE A.LogDownLoadNum IN (SELECT Systemid FROM EmployeeInformation WHERE EmployeeStatus = 'Active')
	                       AND A.ProcessedFlag = 0 AND A.PDate = '" + sAttnDate + @"' 
                           AND A.groupid = '" + sGroupID + "'";

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
}
public class ShortLeaveSetting
{
    public bool IsShortLeaveAllowed { get; set; }
    public bool IsHalfDayPresentAllowed { get; set; }
    public bool IsTowShortLeaveAllowedInaDay { get; set; }
    public int MaxShortLeaveInaMonth { get; set; }
}
public class dicShiftDft
{
    public string SystemID { get; set; } = string.Empty;
    public string GroupID { get; set; } = string.Empty;
    public string PlantID { get; set; } = string.Empty;
    public string ShiftDefinationName { get; set; } = string.Empty;
    public string ShiftDefinationDescription { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int SequenceNo { get; set; } = 0;
    public bool IsActive { get; set; } = false;
    public bool DefaultShift { get; set; } = false;
    public string ShiftType { get; set; } = string.Empty;
    public DateTime? InTime { get; set; }
    public int InTimeStartMargin { get; set; } = 0;
    public int LateMargin { get; set; } = 0;
    public int AbsentEndMargin { get; set; } = 0;
    public DateTime? OutTime { get; set; }
    public int OutTimeEndMargin { get; set; } = 0;
    public int OTStartTime { get; set; } = 0;
    public bool IsGapInclude { get; set; } = false;
    public DateTime? BreakStratTime { get; set; }
    public DateTime? BreakEndTime { get; set; }
    public int BreakPeriod { get; set; } = 0;
    public double WorkingHour { get; set; } = 0.0;

    public bool EarlyIn { get; set; } = false;
    public int EarlyInMargin { get; set; } = 0;
    public int EarlyInRoundMargin { get; set; } = 0;
    public string EarlyInRoundMarginType { get; set; } = string.Empty;

    public bool LateIn { get; set; } = false;
    public int LateInMargin { get; set; } = 0;
    public int LateInRoundMargin { get; set; } = 0;
    public string LateInRoundMarginType { get; set; } = string.Empty;

    public bool EarlyOut { get; set; } = false;
    public int EarlyOutMargin { get; set; } = 0;
    public int EarlyOutRoundMargin { get; set; } = 0;
    public string EarlyOutRoundMarginType { get; set; } = string.Empty;

    public bool LateOut { get; set; } = false;
    public int LateOutMargin { get; set; } = 0;
    public int LateOutRoundMargin { get; set; } = 0;
    public string LateOutRoundMarginType { get; set; } = string.Empty;

    //public decimal ShortLeaveMaxLimit { get; set; } = 0;
    public decimal HalfDayAbsentMaxLimit { get; set; } = 0;
    public bool IncludeBreakTimeInOT { get; set; } = false;
    public bool IsLateInApplicable { get; set; } = false;
    public bool IsEarlyOutApplicable { get; set; } = false;
    public bool IsLunchOutApplicable { get; set; } = false;
    //public bool IsOTOverHalfDay { get; set; } = false;

    public int EarlyOutToleranceMargin { get; set; } = 0;
    public int EarlyOutMaxLimit { get; set; } = 0;
    public int LateInMaxLimit { get; set; } = 0;
}


public static class DecimalHelper
{
    public static string ToHexString(this Decimal dec)
    {
        var sb = new StringBuilder();
        while (dec > 1)
        {
            var r = dec % 16;
            dec /= 16;
            sb.Insert(0, ((int)r).ToString("X"));
        }
        return sb.ToString();
    }
}//End Function 
public class TableConvert
{
    static sbyte[] unhex_table =
              { -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
                   ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
                   ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
                   , 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,-1,-1,-1,-1,-1,-1
                   ,-1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1
                   ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
                   ,-1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1
                   ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
                  };

    public static int Convert(string hexNumber)
    {
        int decValue = unhex_table[(byte)hexNumber[0]];
        for (int i = 1; i < hexNumber.Length; i++)
        {
            decValue *= 16;
            decValue += unhex_table[(byte)hexNumber[i]];
        }
        return decValue;
    }
}//End Function 
public class HROTSetting
{
    public int MinimumOTMinute { get; set; } = 0;
    public int RoundFigureForOT { get; set; } = 0;
    public bool IsRoundOptionApplicable { get; set; } =false;
    public string OTFractionCalculation { get; set; } = "";
    public string OTConsiderOn { get; set; } = "";
    public string OTBaseOnOuttime { get; set; } = "";
    public bool IsPunchBasedOT { get; set; } = false;
    public bool IsPreallocationBasedOT { get; set; } = false; 
    public int PayableMinimumOT { get; set; } = 0;
    public bool IsRemoteAttendanceApprovalRequired { get; set; } = false;

}
public class ParaAttendance
{     
    public string OPN_FLAG { get; set; } = "";
    public string GroupId { get; set; } = "";
    public string sType { get; set; } = "";
    public string sEmpSystemID { get; set; } = "";
    public string sPlantID { get; set; } = "";
    public string sWorkingDate { get; set; } = "";
    public string shiftSystemID { get; set; } = "";
    public string sDate { get; set; } = "";
    public string sTime { get; set; } = "";
    public string sRowID { get; set; } = "";
    public string sDayStatus { get; set; } = "";
    public string sLvTrans { get; set; } = "";
    public decimal iOverTime { get; set; } = 0;
    public decimal iOverTimeIntime { get; set; } = 0;//_OT_inTime
    public decimal iOverTimeOuttime { get; set; } = 0;//_OT_inTime
    public bool bManualTime { get; set; } = false;
    public bool bManualDayStatus { get; set; } = false;//
    public bool IsStatusChanged { get; set; } = false;//
    public bool IsShortLeave { get; set; } = false;//
    public bool IsReversed { get; set; } = false;//
    public bool IsHalfDayLeave { get; set; } = false;//
    public string DayStatusInTimeOnly { get; set; } = "";//
    public int CountedShortLeave { get; set; } =0;//
    public bool IsFirstHalfLeave { get; set; } =false;//IsFirstHalfLeave
    public bool HasManualOutTime { get; set; }
    public string ManualDate { get; set; }
    public bool IsLWP { get; set; }
    public bool IsOTEntitled { get; set; }
    public string DayType { get; set; }
    public string InDate { get; set; }
    public string OutDate { get; set; }
    public string sInRawData { get; set; }
    public string sOutRawData { get; set; }
    public decimal LeaveDuration { get; set; }
    public bool IsOutNUll { get; set; }

}
public class ParaOT
{
    //IsOriginalDateOTApplicable, dsOTPerMinPolicy, IsOTBasedOnPerMinute, sDayType, bOTEntitle, HasManualOutTime, 
    //ManualDate, _PaidHours, _ShiftDft, IsOTOverHalfDay, dtOTSlabEmp, dsOTSlabGen, 
    //sEmpSysID, sDate, sDate, sInTime, sOTStartTime, sMinOT, sOutTime

    public string sEmpSysID { get; set; } = string.Empty;
    public string sDate { get; set; } = string.Empty;
    public string sInTime { get; set; } = string.Empty;
    public string sMinOT { get; set; } = string.Empty;
    public string sOutTime { get; set; } = string.Empty;
    public string sOTStartTime { get; set; } = string.Empty;


    public DataSet dsOTPerMinPolicy { get; set; } 
    public string sDayType { get; set; } = string.Empty;
    public string ManualDate { get; set; } = string.Empty;
    public double _PaidHours { get; set; } = 0;



    public dicShiftDft _ShiftDft { get; set; } 
    public DataTable dtOTSlabEmp { get; set; } 
    public DataSet dsOTSlabGen { get; set; } 
    public bool IsOTOverHalfDay { get; set; } = false;
    public bool IsOriginalDateOTApplicable { get; set; } = false;
    public bool IsOTBasedOnPerMinute { get; set; } = false;
    public bool bOTEntitle { get; set; } = false;
    public bool HasManualOutTime { get; set; } = false;
}
public class ParaShortLeaveHalfDayAbsent
{
    public string sInTime { get; set; }
    public string sOutTime { get; set; }
    public string sWorkingDate { get; set; }
    public string sDate { get; set; }
    public dicShiftDft _ShiftDft { get; set; }
    public bool IsOTOverHalfDay { get; set; }
    public string DayStatus { get; set; }
    public bool IsShortLeave { get; set; }
    public bool IsStatusChanged { get; set; }
    public bool IsReversed { get; set; }
    public int CountShortLeave { get; set; }

    public bool IsShortLeaveAllowed { get; set; }
    public bool IsHalfDayPresentAllowed { get; set; }
    public bool IsTowShortLeaveAllowedInaDay { get; set; }

    public int MaxShortLeaveInaMonth { get; set; }
    public double PaidHours { get; set; }

    public bool ShouldNullifyOTValue { get; set; }
    public bool IsOTentitled { get; set; }
    public bool HasManualOutTime { get; set; }
    public string ManualDate { get; set; }

    //sInTime, sOutTime, sWorkingDate, sDate, _ShiftDft, IsOToverHalfDay, MaxShortLeaveInaMonth PaidHours
    //out _DayStatus, out IsShortLeave, out IsStatusChanged, out IsReversed, out CountShortLeave
}
