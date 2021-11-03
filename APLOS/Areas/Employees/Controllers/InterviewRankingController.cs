#region Using
using Library.Core;
using Aplos.Properties;
using Aplos.Controllers;
using System.Web.Mvc;
using Library.Service.Employees;
using Library.Model.Employees;
#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class InterviewRankingController : BaseController
    {
        #region Constructor
        private readonly IInterviewRankingService _interviewRankingService;

        public InterviewRankingController(IInterviewRankingService interviewRankingService)
        {
            this._interviewRankingService = interviewRankingService;
        }
        #endregion      

        #region -- Pages
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        //[Authorize]
        //public JsonResult GetInterviewRankingCbo()
        //{
        //    return Json(new SelectList(_interviewRankingService.GetCboList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_interviewRankingService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetInterviewRanking(string id)
        {
            return Json(_interviewRankingService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_interviewRankingService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(InterviewRanking interviewRanking)
        {
                _interviewRankingService.Insert(interviewRanking);
                return Json(new { InterviewRanking = interviewRanking, Sequence = _interviewRankingService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(InterviewRanking interviewRanking)
        {
                _interviewRankingService.Update(interviewRanking);
                return Json(new { Sequence = _interviewRankingService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
                _interviewRankingService.Delete(id);
                return Json(new { Sequence = _interviewRankingService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}