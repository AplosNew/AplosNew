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
    public class PositionRelationshipController : BaseController
    {
        private readonly IStructureRelationshipService _structureRelationshipService;

        public PositionRelationshipController(IStructureRelationshipService structureRelationshipService)
        {
            _structureRelationshipService = structureRelationshipService;
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityAndPositionRelationship(string companyGroupId, string companyId)
        {
            return Json(_structureRelationshipService.GetEntityAndPositionRelationship(companyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList()
        {
            return Json(_structureRelationshipService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPositionList(GridParameter parameters, string companyGroupId)
        {
            return Json(_structureRelationshipService.Query(parameters, companyGroupId, RelationshipType.Position.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCompanyStructureRelationList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_structureRelationshipService.Query(parameters, identity.CompanyGroupId, RelationshipType.Position.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            return Json(_structureRelationshipService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence(string companyGroupId)
        {
            return Json(_structureRelationshipService.GetAutoSequence(companyGroupId, RelationshipType.Position.ToString()), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(StructureRelationship structureRelationship)
        {
            structureRelationship.RType = RelationshipType.Position.ToString();
            _structureRelationshipService.Insert(structureRelationship);
            return Json(new { StructureRelation = structureRelationship, Sequence = _structureRelationshipService.GetAutoSequence(structureRelationship.CompanyGroupId, structureRelationship.RType), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(StructureRelationship structureRelationship)
        {
            structureRelationship.RType = RelationshipType.Position.ToString();
            _structureRelationshipService.Update(structureRelationship);
            return Json(new { Sequence = _structureRelationshipService.GetAutoSequence(structureRelationship.CompanyGroupId, structureRelationship.RType), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _structureRelationshipService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}