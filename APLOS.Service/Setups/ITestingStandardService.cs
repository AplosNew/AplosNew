#region Using

using Library.Core;
using Library.Model.Setups;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    public interface ITestingStandardService : IService<TestingStandard>
    {
        IEnumerable<ComboModel> GetCbo(string companyGroupId);

        IEnumerable<object> GetCboWithBuyer(string companyGroupId);

        void DeleteGraph(string Id);

        GridModel FindById(GridParameter parameters, string id);

        string InsertAndUpdate(TestingStandard testingStandard, IEnumerable<TestingStandardDetail> TestingStandardDetail, IEnumerable<TestingStandardBuyer> testingStandardDetailBuyer);

        IWorkbook GetTestingStandardReport(string testing);

        GridModel Query(GridParameter parameters, string companyGroupId);
    }
}