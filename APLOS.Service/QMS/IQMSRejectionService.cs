using System.Collections.Generic;

using Library.Core;
using Library.Model.QMS;
using Library.Service.Core;

namespace Library.Service.QMS
{
   
    public interface IQMSRejectionService : IService<QMSRejection>
    {

        IEnumerable<ComboModel> GetShiftGroupCbo(string plantId);
        IEnumerable<object> Get(string Id);
        IEnumerable<object> GetProcess();
        IEnumerable<object> GetShiftMaster();
        IEnumerable<object> GetSKUList();
        IEnumerable<object> GetProductionReference();
        IEnumerable<object> GetDefectMasterList();
        IEnumerable<object> GetLocationList();
        IEnumerable<object> GetGradeList();
        IEnumerable<object> GetCustomer();
        IEnumerable<object> GetRejectionMasterId(string MasterId);
        List<QMSRejection> GetList(string Date,string LocationId);
        List<QMSRejection> GetDelete(string strkey);
        IEnumerable<object> LoadAllResPersonDetailsForSelection(string CompanyGroupId);

        string Create(IEnumerable<QMSRejection> DataToSave);
        void Delete(IEnumerable<QMSRejection> DataToDelete);
        void CreateRejectionChild(IEnumerable<QMSRejectionChild> ChildData, string MasterId);
        IEnumerable<object> GetListRejectionChild(string MasterId);
       // string SaveDetail(string MasterId, IEnumerable<QMSRejectionChild> ChildData);
        List<EmployeeInformation> LoadAllEmpDetailsForSelection(string CompanyGroupId, string Id);

    }
}
