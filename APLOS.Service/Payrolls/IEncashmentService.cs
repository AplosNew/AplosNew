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
    public interface IEncashmentService
    {
        IWorkbook GetEncashReport(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string YearNo);
        IWorkbook GetEarnLeaveReport(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string YearNo, bool isDetail, bool isActive, bool isSeperated);
        //IWorkbook GetEarnLeaveReport(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string YearNo);


    }
}