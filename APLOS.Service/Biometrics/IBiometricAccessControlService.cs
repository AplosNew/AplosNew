using Library.Model.Attendances;
using Library.Service.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Biometrics
{
    public interface IBiometricAccessControlService : IService<AccessControllerList>
    {

        IEnumerable<AccessControllerList> GetBiometricDeviceAsAccessController(string PlantID);

        IEnumerable<AccessControllerEmployeeTag> GetAccCrlRegInfoDeviceWiseForEmp(string PlantID, string strDeviceSystemID);
        IEnumerable<AccessControllerEmployeeTag> GetAccCrlRegInfoEmployeeWise(string EmployeeId);
        IEnumerable<AccessControllerEmployeeTag> GetAccCrlRegInfoDeviceWiseForEmpAndDevice(string EmployeeId, string strDeviceSystemID);

        IEnumerable<AccessControllerEmployeeTag> GetAccCrlRegInfoDeviceWiseForEmp(string PlantID, string strDeviceSystemID, string empIds);
        void DeleteDataSetsForEmp(IEnumerable<AccessControllerEmployeeTag> DataToDelete);
        void SaveDataSetsForEmp(IEnumerable<AccessControllerEmployeeTag> DataToSave);
        void SaveDataSetsForSingleEmp(IEnumerable<AccessControllerEmployeeTag> DataToSave);
        void SaveAdminInfo(Dictionary<string, object> DataToSave);

        List<EmployeeInfomationForAccessControl> SearchEmployeeInformationForDevice(string PlantID, string DeviceSystemID);
        List<EmployeeInfomationForAccessControl> SearchRegisteredEmployeeInformation(string PlantID, string DeviceSystemID);
        List<EmployeeInfomationForAccessControl> GetAllSelectedEmployeesToDelete(string emplIDList);

        List<EmployeeInfomationForAccessControl> GetAllRegisteredEmployeeList(string deviceSystemID);
        List<EmployeeInfomationForAccessControl> GetEmployeeInfoByEmployeeListForUpload(string deviceSystemID, string SystemIDs, string PlantId);

        void ClearDeviceLog(string plantID, string deviceIP);

        List<EmployeeInfomationForAccessControl> SearchAllEmployeeInformation(string strkey, string PlantID);
        List<EmployeeInfomationForAccessControl> GetSingleEmployeeInformation(string employeeid, string PlantID);


        List<FPInformation> GetAllSelectedEmployeesFP(string emplIDList);

    }


}
