using Library.Core;
using Library.Model.IE;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.IEnumerable
{
    public interface ISubsectionStructureMasterService : IService<SubsectionStructureMaster>
    {
        GridModel GetSearchData(GridParameter parameters);

        SubsectionStructureMaster GetMaster(string PK);
        void DeleteMasterDetail(string masterid);

        void DeleteDetail(string detailid);
        void InsertORUpdate(SubsectionStructureMaster master_ui, out string masterid);

        void InsertORUpdateDetail(SubsectionStructureDetail detail);

        IEnumerable<object> GetDetailList(string MasterId);

        IEnumerable<object> GetMasterList(string masterid);

        decimal GetAutoSequence();

        IEnumerable<object> GetDepartmentListCbo();

        IEnumerable<object> GetDivisionListCbo();

        IEnumerable<object> GetSubsectionListCbo();

        IEnumerable<object> GetSectionListCbo();

        IEnumerable<object> GetLineListCbo();
    }
}