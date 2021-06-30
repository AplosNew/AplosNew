using Syncfusion.XlsIO;
using System.Collections.Generic;

namespace Library.MaterialManagement.Reports
{
    public interface IInventoryReceiveReportService
    {
        IWorkbook GetInventoryReceiveReport(string companyId, string plantId, string inventoryReceiveId);
    }
}