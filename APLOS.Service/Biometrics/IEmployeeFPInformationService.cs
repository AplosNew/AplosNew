#region Using

//using Library.Model.Biometics;
using Library.Model.Attendances;
using Library.Model.Biometrics;
using Library.Service.Core;
using Library.ViewModel.HR;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Biometrics
{
    public interface IEmployeeFPInformationService : IService<EmployeeFPInformation>
    {
        IEnumerable<object> GetCheckSameDateLeave(string plantId, string empSystemID, string fromDate, string toDate);

        void Save(EmployeeFPInformation ui);

        IEnumerable<EmployeeProfileVM> GetEmployeeInformation(string PK, string plantid);

        IEnumerable<object> GetAccessControllerList(string Plantid);

        IEnumerable<object> GetAccessControllerEmployeeTag(string Plantid);

        IEnumerable<AccessControllerEmployeeTag> GetAccessControllerEmployeeTagList(string Plantid);

        //IEnumerable<AccessControllerEmployeeTag> GetData(string Plantid);
        IEnumerable<object> GetGroupPrefix(string empid);

        void SaveProximityCard(string empid, string cardid);

        IEnumerable<object> GetShortLeaveSettings(string plantid);

        IEnumerable<EmployeeProfileVM> GetIndviEmployeeInformation(string plantid, string cardNumber);

        IEnumerable<EmployeeProfileVM> GetEmployeeInformation(string emp_pk);

        IEnumerable<object> GetPlantWiseShortLeaveKioskDetails(string plantid);

        IEnumerable<object> GetFPEngineParameterForWithOutBlackListedEmpInfoViaUSBRd(string plantid);

        IEnumerable<EmployeeProfileVM> GetIndviSupVisEmpInfo(string plantId, string cardNumber);

        IEnumerable<object> GetSlvAvailedB4SlvApp(string plantId, string empSystemID, string slvDate);

        IEnumerable<object> GetShortLeaveAllocation(string plantid);

        string GetSLAPK();

        IEnumerable<object> GetEmployeePin(string employeeid, string pin);

        IEnumerable<object> GetCheckMultiTimeSlvINaDay(string plantId, string empSystemID, string slvDate, string strLang);
        IEnumerable<AccessControllerEmployeeTag> GetAccessControllerEmployeeTagDeleteList(string plantid);
        IEnumerable<object> GetAccessControllerEmployeeUnTag(string plantid);
    }
}