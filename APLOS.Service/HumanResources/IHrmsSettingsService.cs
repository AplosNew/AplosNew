using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.HumanResources;
using Library.Service.Core;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.HumanResources
{
    public interface IHrmsSettingsService
    {
        void CreateReLockDataEmployeeWise(string lockDate, string[] LockDateWiseEmployeeList, CustomIdentity identity);
        void CreateUnLockDataEmployeeWise(string lockDate, string[] LockDateWiseEmployeeList, CustomIdentity identity);
        //IEnumerable<object> GetLockEmployeeList(string lockDate);
        void CreateLockData(string lockDate);
        void CreateUnLockData(string lockDate, string plantId);
        void GetUnApprovedEmployeeListData(string lockDate, out DataSet dsMaster);
        void CheckAttdenceProcAndShiftAssignData(string lockDate, out DataSet dsMaster);
        void CheckOTConfirmationData(string lockDate, out DataSet dsMaster);
        IEnumerable<object> GetUnApprovedEmployeeListData(string lockDate);
        IEnumerable<object> GetOTConfirmationData(string lockDate);
        IEnumerable<object> GetAttdencenotNotProcData(string lockDate);
        IEnumerable<object> GetShiftNotAssignData(string lockDate);
        string GetLastLockDate();
        string[] GetLockDateList();
        string[] GetUnLockDateList();
        IEnumerable<object> GetAllEmployeeListData(string fromdate, string todate, string plantId);
        IEnumerable<object> GetEmployeeWiseLockData(string empsystemid, string fromdate, string todate, string plantId);
        void  CreateLockDataEmpWise(string lockDate, string[] LockDateWiseEmployeeList, string user, string lockEntryDate);
        IEnumerable<object> GetLockEmployeeListData(string lockdate, string plantId);
        IEnumerable<object> GetTobeLockEmployeeListData(string lockdate, string plantId);        
        void  CreateLockDataDateWise(string lockDate, string[] lockDateList, string user, string lockEntryDate);
        IEnumerable<object> GetLockEmployeeList(string FromDate, string ToDate, CustomIdentity identity);
        ////IEnumerable<object> GetReLockEmployeeList(string lockDate, CustomIdentity identity);
        IEnumerable<object> GetOutPunchMissingData(string lockDate);
        string GetEmpCode(string EmpSystemId);
        //void CreateEmployeeIndividualAttendanceLock(string lockDate, string[] LockDateWiseEmployeeList, CustomIdentity identity);
        void CreateEmployeeIndividualAttendanceUnLock(string lockDate, string[] LockDateWiseEmployeeList, CustomIdentity identity);
        void CreateEmployeeIndividualAttendanceLock(string EmpSystemId, string[] LockDateList, string LockType, CustomIdentity identity);
        void CreateUnLockDataRangeWise(string FromDate, string ToDate, string[] LockDateWiseEmployeeList, CustomIdentity identity);
        IEnumerable<object> GetReLockEmployeeList(string FromDate, string ToDate, CustomIdentity identity);
        IEnumerable<object> GetOutPunchMissingDataForAlert(string lockDate);
        IEnumerable<object> GetOTConfirmationDataForZeroAuto(string lockDate);
        void CheckSalaryLock(string FromDate, string ToDate, string[] LockDateWiseEmployeeList, CustomIdentity identity);
    }
}