#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface ISOPActivityService : IService<SOPActivity>
    {
        void InsertOrUpdate(SOPActivity entity);

        //void InsertOrUpdateDocument(SOPActivityDocument entity);
        void InsertOrUpdateDocument(IEnumerable<SOPActivityDocument> entities);

        void InsertOrUpdateKPI(SOPActivityKPI entity);

        IEnumerable<object> Query(string sopItemId);

        GridModel GetCbo(string sopItemId);

        GridModel GetKPICbo(string sopItemId);

        void Delete(string id);

        void UpdateActivity(string id, string fieldName);

        string GetPk(SOPActivityDocument entity);
    }
}