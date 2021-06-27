using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Attendances;

namespace Aplos.Areas.Payrolls.Controllers
{


    public class SalaryHeadWisePaymentModePolicyController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public SalaryHeadWisePaymentModePolicyController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpPost]
        public ActionResult Save(SalaryHeadPaymentPolicy salaryheadpayment)
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsIncrementType ob = new clsIncrementType(_sqlRepository);

                salaryheadpayment.AddedDate = DateTime.Now;
                salaryheadpayment.AddedFromIP = identity.IPAddress;
                salaryheadpayment.AddedBy = identity.Name;
                salaryheadpayment.UpdatedBy = identity.Name;             
                salaryheadpayment.UpdatedDate = DateTime.Now;
                salaryheadpayment.UpdatedFromIP= identity.IPAddress;
                salaryheadpayment.CompanyGroupId = identity.CompanyGroupId;
                salaryheadpayment.PlantId = salaryheadpayment.PlantId;

                ob.SalaryHeadWisePaymentMode(salaryheadpayment);

                return Json(new { Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }
        [HttpGet]
        public ActionResult Delete(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM SalaryHeadWisePaymentModePolicy WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GetsalaryheadInformation(string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select shpm.PaymentMode,shpm.Amount,sh.SalaryHead,shpm.Id,shpm.SalaryHeadId,p.CompanyId, shpm.PlantId
                                 from SalaryHeadWisePaymentModePolicy shpm
                                 left join SalaryHead sh on sh.SalaryHeadID =shpm.SalaryHeadId 
                                 LEFT JOIN ORG.Plant p on p.Id=shpm.PlantId
                                 where PlantId='" + PlantId + @"' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Authorize]
        public ActionResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select SalaryHeadID,SalaryHead from SalaryHead WHERE GroupID='"+identity.CompanyGroupId+"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Authorize]
        public ActionResult PaymentModeCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"  select distinct PaymentMode from EmployeeInformation ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}