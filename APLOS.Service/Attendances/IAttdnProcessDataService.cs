using Library.Model.Attendances;
using Library.Service.Core;
using Syncfusion.XlsIO;

namespace Library.Service.Attendances
{
    public interface IAttdnProcessDataService : IService<AttdnProcessData>
    {
        IWorkbook AttndReport(string fromDate, string toDate, string companyGroupId,string companyId,  string plantId);
        void SaveTotal(string _plantid);
    }
}