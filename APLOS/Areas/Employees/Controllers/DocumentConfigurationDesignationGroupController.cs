using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
	public class DocumentConfigurationDesignationGroupController : BaseController
	{
		#region -- Constructor

		private readonly IDocumentConfigurationDesignationGroupService _documentConfigurationDesignationGroupService;
		private readonly IDocumentSetAssignDetailService _documentSetAssignDetailService;

		public DocumentConfigurationDesignationGroupController(IDocumentConfigurationDesignationGroupService documentConfigurationDesignationGroupService, IDocumentSetAssignDetailService documentSetAssignDetailService)
		{
			_documentConfigurationDesignationGroupService = documentConfigurationDesignationGroupService;
			_documentSetAssignDetailService = documentSetAssignDetailService;
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
        public ActionResult GetList(GridParameter parameters,string companyId, string plantId)
        {
            return Json(_documentConfigurationDesignationGroupService.Query(parameters, companyId,plantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
		public JsonResult GetDocumentSet(GridParameter parameters, string plantId, string employeeTypeId, string employmentType)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_documentConfigurationDesignationGroupService.QueryAssign(parameters, identity.CompanyGroupId, plantId, employeeTypeId, employmentType), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public JsonResult GetDocumentSetAssignDetailList(string complianceDocumentSetId, string plantId, string employeeTypeId)
		{
			return Json(_documentSetAssignDetailService.Query(complianceDocumentSetId, plantId, employeeTypeId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetDesignationGroupDateList(string plantId, string employeeTypeId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_documentConfigurationDesignationGroupService.GetDesignationGroupDateList(identity.CompanyGroupId, plantId, employeeTypeId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetDocumentList(GridParameter parameters, string plantId, string employeeTypeId, string documentSetType)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_documentConfigurationDesignationGroupService.GetDocumentList(parameters, identity.CompanyGroupId, plantId, employeeTypeId, documentSetType), JsonRequestBehavior.AllowGet);
		}

		[Authorize]
		public JsonResult GetCbo()
		{
			return Json(_documentConfigurationDesignationGroupService.GetCbo(), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetDocumentConfigurationDesignationGroup()
		{
			return Json(_documentConfigurationDesignationGroupService.Query().Select(), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetDocumentConfigurationDesignationGroupById(string id)
		{
			return Json(_documentConfigurationDesignationGroupService.Find(id), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Create(DocumentConfigurationDesignationGroup entity, IEnumerable<DocumentSetAssignDetail> entities)
		{
			_documentConfigurationDesignationGroupService.InsertORUpdateGraph(entity, entities);
			return Json(new { DocumentConfigurationDesignationGroup = entity, Message = AplosMessage.Insert });
		}

		[HttpPost]
		public JsonResult Edit(DocumentConfigurationDesignationGroup documentConfigurationDesignationGroup)
		{
			_documentConfigurationDesignationGroupService.Update(documentConfigurationDesignationGroup);
			return Json(new { Message = AplosMessage.Updated });
		}

		[HttpPost]
		public JsonResult Delete(string id)
		{
		    if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
		    _documentConfigurationDesignationGroupService.Delete(id);
		    return Json(new { Message = AplosMessage.Deleted });
		}

		#endregion -- Operations
	}
}