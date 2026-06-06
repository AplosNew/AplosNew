using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Securites;
using Library.Service.Securites;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Securities.Controllers
{
    public class UserAccessPlantController : BaseController
    {
        private readonly IUserAccessPlantService _userAccessPlantService;
        private readonly ISqlRepository _sqlRepository;

        public UserAccessPlantController(IUserAccessPlantService userAccessPlantService, ISqlRepository repo)
        {
            _userAccessPlantService = userAccessPlantService;
            _sqlRepository = repo;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCompanyCboByUser(string userId)
        {
            return Json(_userAccessPlantService.GetCompanyCboByUser(userId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(string companyId, string userId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_userAccessPlantService.Query(identity.CompanyGroupId, companyId, userId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPlantList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var flag = false;
            if (identity.IsControlAdmin || identity.IsSysAdmin)
                flag = true;
            var sql = "";
            if (flag)
                sql = @"SELECT P.CompanyId, P.Id AS PlantId, P.Code, P.UserName AS PlantName FROM [ORG].[Plant] AS P WHERE P.CompanyGroupId='" + identity.CompanyGroupId + "' AND P.CompanyId='" + identity.CompanyId + "' AND P.Active=1 AND P.Archive=0";
            else
                sql = @"SELECT A.CompanyId, A.PlantId, P.Code, P.UserName AS PlantName FROM [SEC].[UserAccessPlant] AS A
                            JOIN [ORG].[Plant] AS P ON A.PlantId=P.Id WHERE A.Active=1 AND A.CompanyGroupId='" + identity.CompanyGroupId + "' AND A.CompanyId='" + identity.CompanyId + "' AND A.UserId='" + identity.UserId + "'";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);


            //return Json(_userAccessPlantService.GetPlantList(identity.CompanyGroupId, identity.CompanyId, identity.UserId, flag), JsonRequestBehavior.AllowGet);
        }


       
        [HttpPost]
        public JsonResult Create(IEnumerable<UserAccessPlant> entities)
        {
            _userAccessPlantService.InsertOrUpdateGraph(entities);
            return Json(new { Message = AplosMessage.Success });
        }
    }
}