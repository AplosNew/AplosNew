using Library.Model.Attendances;
using Library.Service.Core;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Attendances
{
    public interface IOTManagementService
    {
        IEnumerable<object> LoadEmpMaternityWithOT(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons);
        IEnumerable<object> LoadEmpForOTConfirmation(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons);
        bool ShowOTValueFromHRMSSetting(string companyGroupId, string plantId);
        void SaveData(string ProcDate, DataSet dsGrd);
        IEnumerable<object> LoadConfirmedEmployeeDataForGrid(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons);
        IEnumerable<object> LoadPostDeviationEmployeeDataForGrid(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons);
        IEnumerable<object> LoadMissPunchEmployeeDataForGrid(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons);
        IEnumerable<object> LoadEmpWiseDataForOTConfirmation(string CompanyGroupID, string PlantId, string EmpId, string FDate, string TDate, string sOTValCons);
        DataSet LoadEmpForOTConfirmationAuto(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons);
        DataSet LoadPostDeviationEmployeeDataForGridAuto(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons);
        DataSet LoadEmpMaternityWithOTAuto(string CompanyGroupID, string PlantId, string ProcDate, string sOTValCons);
    }
}