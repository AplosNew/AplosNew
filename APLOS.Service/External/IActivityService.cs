#region Using

using Library.Core;
using Library.Model.External;
using Library.Service.Core;

#endregion Using

namespace Library.Service.External
{
    public interface IActivityService : IService<ActivityEmp>
    {
        void InsertOrUpdate(ActivityEmp entity);

        void InsertOrUpdateDocument(DocumentActivity entity, string docPk);

        void InsertOrUpdateKPI(KPI entity);

        GridModel Query(GridParameter parameters, string employeeId);

        GridModel GetCbo(string employeeId);

        GridModel GetKPICbo(string employeeId);

        void Delete(string id);

        void UpdateActivity(string id, string fieldName);

        string GetPk(DocumentActivity entity);
    }
}