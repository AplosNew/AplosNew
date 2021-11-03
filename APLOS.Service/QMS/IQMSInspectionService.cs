using System.Collections.Generic;

using Library.Core;
using Library.Model.QMS;
using Library.Service.Core;

namespace Library.Service.QMS
{
   
    public interface IQMSInspectionService : IService<QMSInspection>
    {

        IEnumerable<ComboModel> GetShiftGroupCbo(string plantId);
        IEnumerable<object> Get(string Id);
        IEnumerable<object> GetProcess();
        IEnumerable<object> GetInspectionLevel(string InspectionMasterId);
        IEnumerable<object> GetInspectionMasterList();
        IEnumerable<object> GetProductionReference();
        IEnumerable<object> GetShiftMaster();
     //   string SaveDetail(string MasterId, IEnumerable<QMSInspectionChild> ChildData);
        
        
        IEnumerable<object> GetInspectionType();
        IEnumerable<object> GetLocationList();
        IEnumerable<object> GetStatusList();
        IEnumerable<object> Getdefectmasterlist();
        IEnumerable<object> Getdefectzonelist();
        IEnumerable<object> Getskilllist();
        IEnumerable<object> GetCustomer();
        IEnumerable<object> LoadAllResPersonDetailsForSelection(string CompanyGroupId);
        string Create(IEnumerable<QMSInspection> DataToSave);
        void Delete(IEnumerable<QMSInspection> DataToDelete);
        IEnumerable<object> GetListInspectionChild(string QMSInspectionId);
        void CreateInspectionChild(IEnumerable<QMSInspectionChild> ChildData, string MasterId);

        List<EmployeeInformation> LoadAllEmpDetailsForSelection(string CompanyGroupId, string Id);
        List<EmployeeInformation> LoadAllDefResPonDetailsForSelection(string CompanyGroupId, string Id);
        List<QMSInspection> GetList(string Date, string LocationId);
        List<QMSInspection> GetDelete(string strkey);
     
    }
}
