#region using
using Aplos.Controllers;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Data;
using Library.Service.Materials;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;

#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialIssueControlController : BaseController
    {
        #region -- Constructor
        private readonly ISqlRepository _sqlRepository;
        public MaterialIssueControlController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region Pages
        
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetList()
        {
            return null;// Json(_fgzoneService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        
        [HttpPost]
        public JsonResult Create()
        {
            //  _fgzoneService.Insert(fgzone);
            return null;// Json(new { MaterialIssueControl = fgzone, Sequence = _fgzoneService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                //_fgzoneService.Archive(id);
                return null;// Json(new { Sequence = _fgzoneService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}