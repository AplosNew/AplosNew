using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Enums;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Organizations.Controllers
{
    public class CostCenterController : BaseController
    {
        #region -- Constructor

        private readonly ICostCenterService _costCenterService;

        public CostCenterController(ICostCenterService costCenterService)
        {
            _costCenterService = costCenterService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_costCenterService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_costCenterService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCostCenterUnSelectedList(GridParameter parameters, string costCenterIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_costCenterService.QueryWithUnSelected(parameters, new JavaScriptSerializer().Deserialize<string[]>(costCenterIds), identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_costCenterService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCostCenter()
        {
            return Json(_costCenterService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCostCenterById(string id)
        {
            return Json(_costCenterService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CostCenter costCenter)
        {
            if (costCenter.CostType == CostCenterType.Department.ToString() && string.IsNullOrEmpty(costCenter.DepartmentId))
                   throw new CustomException("Please Select Department !");
            if (costCenter.CostType == CostCenterType.Employee.ToString() && string.IsNullOrEmpty(costCenter.EmployeeId))
                throw new CustomException("Please Select Employee !");
            if (costCenter.CostType == CostCenterType.Unit.ToString() && string.IsNullOrEmpty(costCenter.UnitId))
                throw new CustomException("Please Select Unit !");
            if (costCenter.CostType == CostCenterType.Line.ToString() && string.IsNullOrEmpty(costCenter.LineId))
                throw new CustomException("Please Select Line !");

            _costCenterService.Insert(costCenter);
            return Json(new { CostCenter = costCenter, Sequence = _costCenterService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(CostCenter costCenter)
        {
            if (costCenter.CostType == CostCenterType.Department.ToString() && string.IsNullOrEmpty(costCenter.DepartmentId))
                throw new CustomException("Please Select Department !");
            if (costCenter.CostType == CostCenterType.Employee.ToString() && string.IsNullOrEmpty(costCenter.EmployeeId))
                throw new CustomException("Please Select Employee !");
            if (costCenter.CostType == CostCenterType.Unit.ToString() && string.IsNullOrEmpty(costCenter.UnitId))
                throw new CustomException("Please Select Unit !");
            if (costCenter.CostType == CostCenterType.Line.ToString() && string.IsNullOrEmpty(costCenter.LineId))
                throw new CustomException("Please Select Line !");
            _costCenterService.Update(costCenter);
            return Json(new { Sequence = _costCenterService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _costCenterService.Delete(id);
                return Json(new { Sequence = _costCenterService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations
    }
}