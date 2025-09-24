#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Model.External;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface IResignationService : IService<Resignation>
    {
        Dictionary<string, object> GetFile(string Id);

        GridModel ActiveEmpListByPlantId(GridParameter parameters, string plantID, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId);

        GridModel PendingResignationQueryByPlantId(GridParameter parameters, string plantID, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId);
        IEnumerable<object> MultipleResignationAppliedList(bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string PlantId, string employeeId);
        //IEnumerable<object> MultipleResignationAppliedList(bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId);
        //GridModel MultipleResignationAppliedList(GridParameter parameters, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId);

        //GridModel MultipleResignationPendingList(GridParameter parameters, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId);
        IEnumerable<object> MultipleResignationAppliedPendingList(bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId);
        void Save(Resignation ui, out string masterid);
        void Update(Resignation from_ui);
        void ApprovalUpdate(IEnumerable<Resignation> entities, string name, string ipAddress, string companyGroupId, string companyId);
        void UpdateApprovalStatusUpdate(List<Dictionary<string, string>> entities, string name, string ipAddress, string companyGroupId, string companyId);
        void GetExperience(string empid, out int tYear, out int tMonth);

        GridModel ResignationApprovalQueryByPlantId(GridParameter parameters, string plantID);

        IWorkbook ReportEmployeeInfo(ReportParam status);

        void UpdateResignedEmployees();
        IEnumerable<object> GetCboSeparationType(string PlantId);
        IEnumerable<object> ResignationHistoryByID(string empID);
    }
}