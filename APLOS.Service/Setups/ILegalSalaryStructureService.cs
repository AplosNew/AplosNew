#region Using

using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    /// <summary>
    ///
    /// </summary>
    public interface ILegalSalaryStructureService : IService<LegalSalaryStructure>
    {
        GridModel Query(GridParameter parameters, string legalSalaryGradeId);

        IEnumerable<object> GetHeadList(string legalSalaryGradeId);

        IEnumerable<object> GetHeadEdit(string id);

        void InsertOrUpdateGraph(LegalSalaryStructure entity, IEnumerable<LegalSalaryStructureValue> valueList);

        void DeleteGraph(string key);

        IWorkbook GetLegalSalaryReport(string effectiveDate, string plantId);
    }
}