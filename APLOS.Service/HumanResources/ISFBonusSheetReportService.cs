#region Using

using Library.Core;
using Library.Model.HumanResources;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Data;
using System.Web;
using static Library.Service.HumanResources.PayRegisterBDReportService;

#endregion Using

namespace Library.Service.HumanResources
{
    public interface ISFBonusSheetReportService 
    {
        IEnumerable<object> GetBonusPoint();
        IEnumerable<object> GetBonusEffectiveDate();
        IWorkbook GetSFBonusSheet(string companyGroupId, string companyId, string plantId, string languageId, string paymentMode, string payGroup, string bonusPointId,string bonusType);
        IEnumerable<object> GetEmpInfo(string companyGroupId, string plantId, string effectiveDate, bool sa, bool ca, string userId);
        IWorkbook GetSFBonusSheetGrid(Dictionary<string, string> parameters, string cutoffdate, string companyId, string plantId, string languageId, string paymentMode, string bonusType, bool isStampDeductApplicable,string reportHeader, string docGrouping);
    }
}