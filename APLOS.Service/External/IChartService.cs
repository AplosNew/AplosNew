#region Using

using Library.Model.External;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.External
{
    public interface IChartService : IService<Employee>
    {
        IEnumerable<object> GetDetailList(IEnumerable<ChartColumnList> chartColumnList, int seq, string cgid);

        IEnumerable<object> GetGroupWiseColumnList(string companyGroupId);

        IEnumerable<object> GetGroupWiseCList(string companyGroupId);

        #region Modal Interface

        IEnumerable<object> NotLoggedInEmployeeList(IEnumerable<ChartColumnList> chartColumnList, int seq, string cgid);

        IEnumerable<object> SubmittedEmployeeList(IEnumerable<ChartColumnList> chartColumnList, int seq, string cgid);

        IEnumerable<object> NotSubmittedEmployeeList(IEnumerable<ChartColumnList> chartColumnList, int seq, string cgid);

        IEnumerable<object> StNotLoggedInEmployeeList(IEnumerable<ChartColumnList> chartColumnList, int seq, string cgid);

        IEnumerable<object> StSubmittedEmployeeList(IEnumerable<ChartColumnList> chartColumnList, int seq, string cgid);

        IEnumerable<object> StNotSubmittedEmployeeList(IEnumerable<ChartColumnList> chartColumnList, int seq, string cgid);

        #endregion Modal Interface

        IEnumerable<object> TotalActivity(string companyGroupId);

        IEnumerable<object> FirstLoggedIn(string companyGroupId);

        IEnumerable<object> DayWiseSubmit(string companyGroupId);

        IEnumerable<object> TotalDocument(string companyGroupId);
    }
}