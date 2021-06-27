#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class TnaSettingMasterController : BaseController
    {
        #region Constructor
        private readonly ITnaSettingMasterService _tnaSettingMasterService;
       private readonly ITnaSettingDetailService _tnaSettingDetailService;
        public TnaSettingMasterController(ITnaSettingMasterService tnaSettingMasterService,ITnaSettingDetailService tnaSettingDetailService)
        {
            _tnaSettingMasterService = tnaSettingMasterService;
            _tnaSettingDetailService = tnaSettingDetailService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string plantId)
        {
            return Json(_tnaSettingMasterService.Query(parameters,plantId), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult Create(TnaSettingMaster tnaSettingMaster,IEnumerable<TnaSettingDetail> tnaSettingDetails)
        {
            _tnaSettingMasterService.InsertOrUpdate(tnaSettingMaster , tnaSettingDetails);
            return Json(new { TnaSettingMaster = tnaSettingMaster, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(TnaSettingMaster tnaSettingMaster)
        {
            _tnaSettingMasterService.Update(tnaSettingMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id, string plantId, string joblocationId)
        {
            _tnaSettingMasterService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
      
    }
}