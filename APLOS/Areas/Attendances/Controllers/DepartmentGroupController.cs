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

namespace Aplos.Areas.Attendances.Controllers
{


    public class DepartmentGroupController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public DepartmentGroupController(ISqlRepository sqlRepository)
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
        public ActionResult Save(DepartmentGroup master,string DepartmentGroupId, List<DepartmentGroupDetails> DepartmentIdList)
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsShiftInfo ob = new clsShiftInfo(_sqlRepository);
                master.AddedDate = DateTime.Now;
                master.AddedFromIP = identity.IPAddress;
                master.AddedBy = identity.Name;
                master.UpdatedBy = identity.Name;
                master.CompanyId = identity.CompanyId;
                master.UpdatedDate = DateTime.Now;

                ob.SaveDepartmentMasterAndDetails(master, DepartmentGroupId, DepartmentIdList);

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
                string sql = @"Delete FROM DepartmentGroup WHERE Id='" + Id + @"'";
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
        public ActionResult GetDepartmentInformation()
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsShiftInfo ob = new clsShiftInfo(_sqlRepository);
                var data = ob.GetDepartmentInfo(identity.CompanyGroupId, identity.PlantId);
                
                return Json(new { DepartmentInfo = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDepartmentInformationEdit(string Id)
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsShiftInfo ob = new clsShiftInfo(_sqlRepository);
                var data = ob.GetDepartmentInfoEdit(Id,identity.CompanyId, identity.PlantId);

                return Json(new { DepartmentInfoedit = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDepartmenthkp()
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsShiftInfo ob = new clsShiftInfo(_sqlRepository);
                var data = ob.GetDepartmenthkp(identity.CompanyId);

                return Json(new { DepartmenthkpInfo = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT max(Sequence)+1 Sequence FROM DepartmentGroup WHERE CompanyId='" + identity.CompanyId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}