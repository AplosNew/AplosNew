using Library.Core;
using Library.Model.IE;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

namespace Library.Service.IEnumerable
{
    public interface IBulletinMasterService : IService<BulletinMaster>
    {
        GridModel GetSearchData(GridParameter parameters, string companyId);

        IEnumerable<object> GetBulletinMasterList();

        //void Insert(BulletinMaster bulletinMaster, IEnumerable<BulletinDetail> bulletinDetail);
        BulletinMaster GetBulletinMaster(string PK);

        //void InsertMaster(BulletinMaster bulletinmaster);
        void InsertORUpdateDetail(BulletinDetail bulletindetail);

        void DeleteMasterDetail(string masterid);

        void DeleteDetail(string detailid);

        void InsertORUpdateMaster(BulletinMaster bulletinmaster, out string masterid);

        IEnumerable<object> GetBulletinDetailList(string companyGroupId, string masterId, string processId);

        IEnumerable<object> GetBulletinMasterList(string masterid);

        GridModel GetBuyerList(GridParameter parameters);

        IWorkbook GetWorkBook(out ExcelEngine excelEngine, string masterid);

        void InsertProcess(string masterId, IEnumerable<BulletinProcess> detail);

        void DeleteProcess(string bulletinProcessId);

        IEnumerable<object> GetBulletinProcessList(string masterId);
    }
}