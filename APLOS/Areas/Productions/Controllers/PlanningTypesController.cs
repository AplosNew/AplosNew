#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;
using Library.Crosscutting.Security;
using System.Threading;
using System;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class PlanningTypesController : BaseController
    {
        #region Constructor
        /// <summary>   The PlanningTypesService service. </summary>
        private readonly IPlanningTypesService _planningTypesService;
        private readonly ISqlRepository _sqlRepository;
        public PlanningTypesController(IPlanningTypesService planningTypesService, ISqlRepository R)
        {
            _planningTypesService = planningTypesService;
            _sqlRepository = R;
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

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT PlanningType AS [Value], UserName AS [Text] FROM [dbo].[PlanningTypes]"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetAllEntity(string CompanyId)
        {
            try
            {

                string sql = @"";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (identity.IsSysAdmin)
                {
                    sql = @"SELECT distinct E.Id,E.PlantId,P.UserName AS PlantName,e.Code,e.UserName AS UserName
                        FROM [ORG].[Entity] E
                            LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E.Id
                            LEFT JOIN org.Plant AS p ON p.Id=e.PlantId
                            LEFT JOIN org.Company AS c ON c.Id=e.CompanyId
                            WHERE ECC.IsProductionEntity=1 AND E.[Active]=1 AND e.CompanyId='" + CompanyId + @"'
                        ORDER BY e.Code";

                    return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                }

                sql = @"SELECT  distinct E2.Id,e2.PlantId,P.UserName AS PlantName,e2.Code,e2.UserName AS UserName  FROM [SEC].[UserEntity] E
                        LEFT JOIN org.Entity AS e2 ON e2.Id=e.EntityId
                        LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E2.Id
                        LEFT JOIN org.Plant AS p ON p.Id=e.PlantId
                        LEFT JOIN org.Company AS c ON c.Id=e.CompanyId
                        WHERE E.UserId='" + identity.UserId + @"' AND ECC.IsProductionEntity=1 AND E2.[Active]=1 ORDER BY E2.Code";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);


                throw new Exception("No entity configurations was found in the system for the current user");
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_planningTypesService.Query(parameters), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(PlanningTypes planningTypes)
        {
            _planningTypesService.Insert(planningTypes);
            return Json(new { PlanningTypes= planningTypes, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PlanningTypes planningTypes)
        {
            _planningTypesService.Update(planningTypes);
            return Json(new {Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _planningTypesService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}