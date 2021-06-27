using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Enums;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class EntityRelationshipController : BaseController
    {
        private readonly IStructureRelationshipService _structureRelationshipService;

        public EntityRelationshipController(IStructureRelationshipService companyStructureRelationshipService)
        {
            _structureRelationshipService = companyStructureRelationshipService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetList()
        {
            return Json(_structureRelationshipService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCompanyStructureRelationList(GridParameter parameters, string companyGroupId, string companyId)
        {
            return Json(_structureRelationshipService.Query(parameters, companyGroupId, companyId, RelationshipType.Entity.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCompanyStructureRelationForData(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_structureRelationshipService.Query(parameters, identity.CompanyGroupId, companyId, RelationshipType.Entity.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            return Json(_structureRelationshipService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence(string companyGroupId, string companyId)
        {
            return Json(_structureRelationshipService.GetAutoSequence(companyGroupId, companyId, RelationshipType.Entity.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(StructureRelationship structureRelationship)
        {
            structureRelationship.RType = RelationshipType.Entity.ToString();
            _structureRelationshipService.Insert(structureRelationship);
            return Json(new { StructureRelation = structureRelationship, Sequence = _structureRelationshipService.GetAutoSequence(structureRelationship.CompanyGroupId, structureRelationship.CompanyId, structureRelationship.RType), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(StructureRelationship structureRelationship)
        {
            _structureRelationshipService.Update(structureRelationship);
            structureRelationship.RType = RelationshipType.Entity.ToString();
            return Json(new { Sequence = _structureRelationshipService.GetAutoSequence(structureRelationship.CompanyGroupId, structureRelationship.CompanyId, structureRelationship.RType), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _structureRelationshipService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}