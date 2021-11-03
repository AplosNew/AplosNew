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


    public class IncrementTypeController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public IncrementTypeController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region Aplos
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpPost]
        public ActionResult Save(IncrementType master)
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsIncrementType ob = new clsIncrementType(_sqlRepository);

                master.AddedDate = DateTime.Now;
                master.AddedFromIP = identity.IPAddress;
                master.AddedBy = identity.Name;
                master.UpdatedBy = identity.Name;             
                master.UpdatedDate = DateTime.Now;
                master.UpdatedFromIP= identity.IPAddress;

                ob.SaveIncrementTypeMaster(master);

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
                string sql = @"Delete FROM IncrementType WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult GetIncrementTypeInformation()
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsIncrementType ob = new clsIncrementType(_sqlRepository);
                var data = ob.GetIncrementTypeInfo();
                
                return Json(new { IncrementTypeInfo = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT max(Sequence)+1 Sequence FROM IncrementType ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}