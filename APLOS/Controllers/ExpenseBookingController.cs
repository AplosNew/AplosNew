using Library.Model.Expenses;
using Library.Service.Currencies;
using Library.Service.Expenses;
using Library.Service.ManagementChartOfAccounts;
using Library.Service.Organizations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class ExpenseBookingController : ApiController
    {
        #region Constructor

        private readonly IBudgetMasterService _budgetMasterService;
        private readonly IBudgetMasterActivityService _budgetMasterActivityService;
        private readonly ICurrencyService _currencyService;
        private readonly IExpenseBookingService _expenseBookingService;
        private readonly ICurrencyTransactionService _currencyTransactionService;
        private readonly IEntityService _entityService;

        public ExpenseBookingController(
            ICurrencyService currencyService
            , IExpenseBookingService expenseBookingService
                         , IBudgetMasterService budgetMasterService
            , IBudgetMasterActivityService budgetMasterActivityService
            , ICurrencyTransactionService currencyTransactionService
            , IEntityService entityService

          )
        {
            _expenseBookingService = expenseBookingService;
            this._currencyService = currencyService;
            _budgetMasterService = budgetMasterService;
            _currencyTransactionService = currencyTransactionService;
            _budgetMasterActivityService = budgetMasterActivityService;
            _entityService = entityService;
        }

        #endregion Constructor

        // GET: api/FrApi
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }

        // GET: api/FrApi/5
        public IHttpActionResult GetCboEmployeeBudgetList(string employeeId)
        {
            var result = _budgetMasterService.GetCboEmployeeBudgetList(employeeId);
            return Json(result);
        }

        public IHttpActionResult GetBudgetMasterActivityCbo(string budgetMasterId)
        {
            var result = _budgetMasterActivityService.GetBudgetMasterActivityCbo(budgetMasterId);
            return Json(result);
        }

        public IHttpActionResult GetCboFALinkedList(string budgetMasterId, string activityId, string faLinked)
        {
            var result = _budgetMasterService.GetFALinkedList(budgetMasterId, activityId, faLinked);
            return Json(result);
        }

        public IHttpActionResult GetExpensesEmployeeBudgetPopUpList(string employeeId)
        {
            return Json(_budgetMasterService.GetExpensesEmployeeBudgetPopUpList(employeeId));
        }

        // GET: api/FrApi/5
        public IHttpActionResult GetCboCurrencyList(string companyId)
        {
            var result = new SelectList(_currencyTransactionService.GetCboCurrencyTransaction(companyId), "Value", "Text").Items;

            return Json(result);
        }

        public IHttpActionResult GetCboEntityList(string companyGroupId, string companyId, string plantId)
        {
            var result = new SelectList(_entityService.GetCbo(companyGroupId, companyId, plantId), "Value", "Text");
            return Json(result);
        }

        public IHttpActionResult GetExpenseBookingPendingList(string employeeId)
        {
            var result = _expenseBookingService.GetExpenseBookingPendingList(employeeId);
            return Json(result);
        }

        public void PostExpenseBooking()
        {
            var responseData = Request.Content;
            var readData = responseData.ReadAsStringAsync().Result;
            var objects = JsonConvert.DeserializeObject<Dictionary<string, object>>(readData);
            var expenseBookinOb = GetData<ExpenseBooking>("expenseBookinOb", objects);
            var expenseBookinList = GetListData<ExpenseBookingDetail>("expenseBookingList", objects);
            _expenseBookingService.Insert(expenseBookinOb, expenseBookinList,null);
            Request.CreateResponse(HttpStatusCode.OK);
        }

        private T GetData<T>(string st, Dictionary<string, object> ob)
        {
            var fabricRoll = ob[st];
            var json = JsonConvert.SerializeObject(fabricRoll);
            var fob = JsonConvert.DeserializeObject<T>(json);
            return fob;
        }

        private IEnumerable<T> GetListData<T>(string list, Dictionary<string, object> ob)
        {
            var _list = ob[list];
            var listob = JsonConvert.SerializeObject(_list, Formatting.Indented);
            var lists = JsonConvert.DeserializeObject<IEnumerable<T>>(listob);
            return lists;
        }
    }
}