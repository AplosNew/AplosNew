#region Using

using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using System.Collections.Generic;
using System.Data;

#endregion Using

namespace Library.Service.HumanResources
{
    public interface IWelfareSummaryReportService
    {
        DataTable GetBonusRegisterSql(string companyGroupId, string plantId);
    }
}