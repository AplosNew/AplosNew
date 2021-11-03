using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.HumanResources
{
    public interface IRestService : IService<AttendanceRest>
    {
        void DeleteMaster(string id);
        IEnumerable<object> GetRestDetailsData(string restId, string plantId);

        GridModel Query(GridParameter parameters, string plantId);

        void Insert(AttendanceRest entity, string plantId, IEnumerable<AttendanceRestDetail> restDetails);

        GridModel GetAllEmployee(GridParameter parameters, string companyGroupId, string companyId, string plantId, string sectionId, string subSectionId, string departmentId, bool isOTEntitle,string AttendanceRestDate);
        GridModel GetAllEmployeeForEx(GridParameter parameters, string companyGroupId, string companyId, string plantId, string sectionId, string subSectionId, string departmentId, bool isOTEntitle, string AttendanceRestDate);

    }
}