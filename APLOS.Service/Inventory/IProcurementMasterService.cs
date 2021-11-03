using Library.Core;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.ViewModel.OrderManagements;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Inventory
{
    public interface IProcurementMasterService : IService<ProcurementMaster>
    {

        IEnumerable<object> GetDataByProcurementMasterId();
        object SqlQuery<T>(string v);
        object GetAutoSequence();
        void DeleteReq(string id);

        

        IEnumerable<object> GetMaterialTypeCbo();
        IEnumerable<object> GetQualityStdCbo();

        IWorkbook CreateProcurementMasterReportSheet(string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount);




    }
}