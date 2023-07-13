using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.ManagementChartOfAccounts;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CompanyGroupBudgetController : BaseController
    {
        #region Constructor

        private readonly ICompanyGroupBudgetService _companyGroupBudgetService;
        private readonly ISqlRepository _sqlRepository;
        public CompanyGroupBudgetController(
            ICompanyGroupBudgetService companyGroupBudgetService, ISqlRepository R)
        {
            _companyGroupBudgetService = companyGroupBudgetService;
            _sqlRepository = R;
        }

        #endregion Constructor

        [HttpGet]
        public ActionResult GetList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            string sql = @"select top 100 * from (select * from hkp.Budget where Archive=0) AS TEMP WHERE " + strkey + " ORDER BY TEMP.AddedDate desc";
             
            JsonResult json = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;            
        }
       
    }
}