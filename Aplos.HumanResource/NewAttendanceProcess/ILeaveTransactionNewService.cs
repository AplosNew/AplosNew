#region Using

using Library.Core;
using Library.Model.Biometrics;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.HumanResource.NewAttendanceProcess
{
    public interface ILeaveTransactionNewService : IService<LeaveTransaction>
    {
        void SaveAndUpdateData(LeaveTransaction leaveTransaction, string yearId);
        GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string plantId, string employeeId, string yearNo);
        GridModel QueryGetLeaveListForDelete(GridParameter parameters, string companyGroupId, string companyId, string plantId, string employeeId, string yearNo);
        IEnumerable<object> LoadLeaveTypeCbo(string sPlantID, string employeeId);

        IEnumerable<object> EmployeeInfo();

        IEnumerable<object> LoadLvPolicyWiseLeaveTypeCmb(string sPlantID, string strLvPolSysID, string employeeId);

        void SaveData(LeaveTransaction leaveTransaction);
        void SaveLeaveData(LeaveTransaction leaveTransaction);

        IEnumerable<ComboModel> LoadYearCbo(string plantId);

        //IEnumerable<object> LoadGrdAllocatedLvDetails(string employeeId, string calanderYearId);

        IEnumerable<object> LoadGrdAllocatedLvDetails(string companyGroupId, string plantId, string employeeId, string calanderYearId);
        IEnumerable<object> LoadGrdAllocatedLvDetailsNew(string companyGroupId, string plantId, string employeeId, string calanderYearId);
        
        void DeleteGraph(string id);
        void DeleteApprovedLeaveGraph(string id,string EmpSystemid);
    }
}