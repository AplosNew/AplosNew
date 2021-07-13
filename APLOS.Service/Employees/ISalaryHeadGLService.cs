#region Using

using Library.Core;
using Library.Model.Employees;
using Library.Model.Organizations;
using Library.Service.Core;
using Syncfusion.XlsIO;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Employees
{
    public interface ISalaryHeadGLService : IService<SalaryHeadGL>
    {
        IEnumerable<object> GetSalaryHeadGl(string loanTypeTakenId);

        void InsertOrUpdate(IEnumerable<SalaryHeadGL> entities);

        IEnumerable<object> GetSalaryHead(string plantid, string manPowerBudgetId);
        IEnumerable<object> GetSalaryHeadData();
        IEnumerable<object> GetSalaryHeadGL(string plantid, string salaryHeadId);

        GridModel GetAllList(GridParameter parameters, string coaId);

        GridModel GetAssingList(GridParameter parameters, string coaId);

        GridModel GetNotAssingList(GridParameter parameters, string coaId);

        GridModel GetManPowerBudgetList(GridParameter parameters, string companyId);

        GridModel GetManPowerBudgetSavedList(GridParameter parameters, string plantid);

        GridModel GetBudgetListWithGL(GridParameter parameters, string id);

        GridModel GetActivityListWithBudget(GridParameter parameters, string id);

        IEnumerable<object> CoaInfo(string companyId);
        GridModel GetSearchWithCombine(GridParameter parameters, string coaId);
        GridModel GetSearchWithCombineSalaryHead(GridParameter parameters, string coaId);
        GridModel GetSearchWithCombineWithAssing(GridParameter parameters, string coaId);
        GridModel GetSearchWithCombineWithNotAssing(GridParameter parameters, string coaId);
        List<Dictionary<string, object>> GetSalaryHeadGLCombine(string coaId);

        IWorkbook GetSalaryHeadGlReport(/*string CompanyGroupId, string CompanyId, string PlantId*/);
    }
}