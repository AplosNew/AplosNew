#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialMasterMachineProcessController : BaseController
    {
        #region -- Constrator
        private readonly IMaterialMasterMachineProcessService _baseService;
        private readonly IMaterialMasterArticleService _articleService;
        private readonly ISqlRepository _sqlRepository;
        public MaterialMasterMachineProcessController(IMaterialMasterMachineProcessService baseService, IMaterialMasterArticleService articleService, ISqlRepository R)
        {
            _baseService = baseService;
            _articleService = articleService;
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Machines
        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_baseService.GetMaterialMasterList(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetDetailList(string materialMasterId)
        {
            return Json(_baseService.GetDetailList(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetArticleList(string materialMasterId)
        {
            return Json(_articleService.Query(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMachineMasterData()
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT MM.Id
                                  ,CG.StandardName As CompanyGroup
                                  ,MM.Sequence
                                  ,MM.Code
                                  ,MM.ShortName
	                              ,MM.StandardName
                                  ,MM.UserName
	                              ,MC.UserName AS MachineCategory
	                              ,MSC.UserName AS MachineSubCategory
	                              ,SK.UserName AS Skill
                                  ,MM.Description
                                  ,MM.Remarks
                                  ,MM.ProductionMachineQty
                                  ,MM.SampleMachineQty
                                  ,MM.TrainingMachineQty
                                  ,MM.RentMachineQty
                                  ,MM.OtherMachineQty
                                  ,MM.Active
                              FROM MST.MachineMaster As MM
                             LEFT JOIN ORG.CompanyGroup AS CG on CG.ID=MM.CompanyGroupID
                             LEFT JOIN  HKP.MachineCategory AS MC on MC.Id=MM.MachineCategoryId
                             LEFT JOIN HKP. MachineSubCategory AS MSC  on MSC.ID=MM.MachineSubCategoryID
                             LEFT JOIN HKP.Skill AS SK ON SK.ID=MM.SkillId";
                return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost]
        public JsonResult Edit(string materialMasterId, string skillId, IEnumerable<MaterialMasterMachineProcess> entities
            , IEnumerable<MaterialMasterArticle> articleList)
        {
            _baseService.InsertUpdateOrDeleteGraph(materialMasterId, skillId, entities, articleList);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _baseService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion
    }
}
