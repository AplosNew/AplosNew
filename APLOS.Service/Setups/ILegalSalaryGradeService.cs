#region Using

using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Core;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Setups
{
    /// <summary>
    ///
    /// </summary>
    public interface ILegalSalaryGradeService : IService<LegalSalaryGrade>
    {
        GridModel Query(GridParameter parameters, string companyGroupId, string plantId);

        IEnumerable<ComboModel> GetCurrencyRuleCbo(string companyGroupId, string plantId);
        List<Dictionary<string, object>> GetCbo();
        IEnumerable<ComboModel> GetCbo(string plantId);

        GridModel SalaryHeadList(GridParameter parameters, string companyGroupId, string currencyRuleId, string[] salaryHeadIds);

        IEnumerable<object> LegalSalaryGradeHeadList(string legalSalaryGradeId);

        decimal GetAutoSequence(string plantId);

        void InsertGraph(LegalSalaryGrade entity, IEnumerable<LegalSalaryGradeHead> legalSalaryGradeHead);

        void UpdateGraph(LegalSalaryGrade entity, IEnumerable<LegalSalaryGradeHead> legalSalaryGradeHead);

        void DeleteGraph(string key);
        void LegalSalaryGradeDelete(string key);
    }
}