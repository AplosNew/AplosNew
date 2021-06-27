#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class MailReceiverController : BaseController
    {
        #region Constructor

        private readonly IMailReceiverService _mailReceiverService;
        private SqlRepository _sqlRepository = new SqlRepository();

        public MailReceiverController(IMailReceiverService mailReceiverService)
        {
            _mailReceiverService = mailReceiverService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult MailRecipientService()
        {
            return View();
        }

        [Authorize]
        public ActionResult AdministrativeMailRecipient()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo(string companyGroupId)
        {
            return Json(_mailReceiverService.GetCbo(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPlant()
        {
            string strSql = @"select com.ShortName + ' (' + Plant.UserName + ')' Text, Plant.Id Value FROM ORG.Plant Plant
                                     INNER JOIN ORG.Company COM ON COM.Id = Plant.CompanyId";




            return Json(_sqlRepository.GetDataCollection(strSql), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_mailReceiverService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GeAdmintList(GridParameter parameters)
        {
            return Json(_mailReceiverService.AdminQuery(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetTaggingUser(string mailReceiverId)
        {
            return Json(_mailReceiverService.GetTaggingUser(mailReceiverId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAdminCcUser()
        {
            return Json(_mailReceiverService.GetAdminCcUser(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAdminBccUser()
        {
            return Json(_mailReceiverService.GetAdminBccUser(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(MailReceiver entity, IEnumerable<MailReceiverDetail> details)
        {
            _mailReceiverService.Insert(entity, details);
            return Json(new { MailReceiver = entity, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(MailReceiver entity, IEnumerable<MailReceiverDetail> details)
        {
            _mailReceiverService.Update(entity, details);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _mailReceiverService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult DeleteDetail(int id)
        {
            _mailReceiverService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public JsonResult MailReceiverServiceMappingGetList(GridParameter parameters)
        {
            return Json(_mailReceiverService.QueryMailReceiverMapping(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult MailReceiverServiceMappingCreate(MailReceiverServiceMapping entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            _mailReceiverService.InsertMailReceiverMapping(entity);
            return Json(new { MailReceiver = entity, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult MailReceiverServiceMappingUpdate(MailReceiverServiceMapping entity)
        {
            _mailReceiverService.UpdateMailReceiverMapping(entity);
            return Json(new { MailReceiver = entity, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult MailReceiverServiceMappingDelete(string id)
        {
            _mailReceiverService.DeleteMapping(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}