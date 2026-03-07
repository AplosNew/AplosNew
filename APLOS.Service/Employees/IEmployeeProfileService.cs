using Library.Core;
using Library.Model.Employees;
using Library.Model.Payrolls;
using Library.Service.Core;
using Syncfusion.Pdf;
using Syncfusion.Presentation;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Employees
{
    public interface IEmployeeProfileService : IService<EmployeeInformation>
    {
        void InsetOrUpdateMaster(EmployeeInformation entity);
        void UpdateGivenDesignation();
        IPresentation PrintEmployeeIDCardPpt(string empId, string companyGroupId, string companyId, string plantId, string tempId, string empType, string reportType, string issuDate, string workTypeId);
        IPresentation EmployeeMultipleIDCardPpt(string empId, string companyGroupId, string companyId, string plantId, string tempId, string issuDate, string workTypeId, List<Dictionary<string, object>> dataList, bool IsCurrentIssueDate);

        IEnumerable<object> GetAllEmployeeDataWithWorkType(string companyId, string plantId);
        PdfDocument EmployeeMultipleIDCard(string empId, string companyGroupId, string companyId, string plantId, string tempId, string issuDate, string workTypeId, List<Dictionary<string, object>> dataList);
        IEnumerable<object> GetOperationVariation(string companyGroupId/*, string empSystemId*/);
        IEnumerable<object> GetOperationMaster(string companyGroupId/*, string empSystemId*/);
        IEnumerable<object> GetOperationVariationCbo(string companyGroupId);
        decimal GetAutoSequence(string empSystemId);
        IEnumerable<object> GetIssueIdCardByEmployee(string employeeId);
        IEnumerable<object> GetWarningLetterByEmployee(string employeeId);
        void InsertOrUpdatedependantInfo(EmployeeDependantInfo entity);

        void InsertOrUpdateLandLordInfo(EmployeeLandLordInfo entity);

        IEnumerable<ComboModel> GetProfessionCbo();
        void DeleteNominee(string id);
        void DeleteDependant(string id);
        void DeleteLandLoard(string id);

        void InsertOrUpdate(EmployeeNomineeInfo entity);
        IEnumerable<ComboModel> GetRelationCbo();
        void UpdateRelativeInfo(EmployeeInformation entity, string name);
        void UpdateLocalInfo(EmployeeInformation entity, string name);
        object GetLocalLanguageLabel(string plantId);
        IEnumerable<ComboModel> GetTemplateCbo(string plantId, string type);
        PdfDocument PrintEmployeeIDCard(string empId, string companyGroupId, string companyId, string plantId, string tempId, string empType, string reportType, string issuDate, string workTypeId);
        IEnumerable<ComboModel> GetDefaultCbo(string companyGrupId, string plantId);
        void Insert(List<XLUploadDetail> entities);

        IWorkbook EmployeeAppointmentLetterLocal(string companyGroupId, string companyId, string plantId, string empId, string empType, string tempId);
        void EmployeeAppointmentLetterInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId);
        void EmployeeFixationFormInWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId);
        //IWorkbook EmployeeConfirmationLetterLocal(string companyId, string plantId, string empId, string empType, string tempId);
        IEnumerable<ComboModel> GetCbo(string plantId);

       

        IEnumerable<object> GetSectionEmployeeList(string plantId, string companyId, string SectionId);

        void UpdateBudgetCode(EmployeeInformation entity);

        void UpdateMaster(EmployeeInformation entity, string name);

        void UpdatePersonal(EmployeeInformation entity, string name);

        void UpdateAddress(EmployeeInformation entity, string name);

        void UpdateEmployment(EmployeeInformation entity, string name);

        void UpdateAdvanceInfo(EmployeeInformation entity, string name, string IP);

        bool Login(string id, int pin);

        IWorkbook JobCard_Report(string employeeId, string fromDate, string toDate, string companyGroupId);

        IWorkbook EmpInfoReport(string companyGroupId, string companyId, string plantId, string employeeId);

        IWorkbook EmpRegisterReport(string companyGroupId, string companyId, string plantId, string radioValue);

        Dictionary<string, object> GetEmployeeById(string employeeId, string employeementType);

        IEnumerable<dynamic> ShowJobCard(string employeeId, string fromDate, string toDate);

        IEnumerable<dynamic> ShowDailyAttendance(string employeeId, string workingDate);

        IEnumerable<dynamic> ShowDailyAttendance(string employeeId, string FromDate, string ToDate);
        IEnumerable<object> GetSuperVisor(string companyid, string plantid);

        void EmployeeServiceBookInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId);
        void EmployeeNomineeInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId);
        void EmployeeJoiningLetterInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId);
        void EmployeeAcknowledgementInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId);
        void ConfirmationletterInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId);
        void GetEmployeePersonalFileInMSWord(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId)
        //void generateReport( string CalanderYearId, string FromDate, string ToDate,string plantId, string EmpSystemID, string empType, string reportType, string tempId);
        //void generateReport(string CalanderYearId, string FromDate, string ToDate, string plantId, string empID, string reportType,string EmployeeType, string tempId);

        void generateReport(string CalanderYearId, string FromDate, string ToDate, string plantId, string empID, string reportType, string tempId);

        IEnumerable<object> GetClanderYear(string PlantId);
        DataTable MediasoftFairShopDataExport();
        void CreateLockData(string lockDate);
        void SaveApprovedEmployeeData(DataSet dsGrd);

        void SaveUnApprovedEmployeeData(DataSet dsGrd);


        void EmployeeIncrementHistory(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId);
        void EmployeeExitInterview(string companyGroupId, string companyId, string plantId, string empId, string empType, string reportType, string tempId);

    }
}