#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;
using System;
using Library.Data;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class RecipeMaterialController : BaseController
    {
        #region Constructor
        //private readonly IButtonRecipeMasterService _masterService;

        private readonly IRecipeMaterialService _recipeMaterialService;

        public RecipeMaterialController( IRecipeMaterialService recipeMaterialService)
        {
            //_masterService = masterService;
            _recipeMaterialService = recipeMaterialService;
        }
        #endregion

        #region -- Pages

     
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        #region Operation

        [HttpGet, Authorize]
        public JsonResult GetRecipeList(string masterId)
        {
            return Json(_recipeMaterialService.GetRecipeMaterialListNew(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetRecipeCbo(string entityId)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_recipeMaterialService.GetRecipeCbo(entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateRecipeMaterial(RecipeMaterial recipeMaterial)
        {
            try
            {
                _recipeMaterialService.Insert(recipeMaterial);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return Json(new { RecipeMaterial = recipeMaterial, Message = AplosMessage.Success });
        }
        //[HttpPost]
        //public JsonResult CreateRecipeMaterialFG(RecipeMaterial recipeMaterial)
        //{
        //    try
        //    {
        //        //validation MaterialMasterId
        //        var IsDuplicateEntryAllowed = _recipeMaterialService.ShouldValidation(recipeMaterial.RecipeGlobalMasterId, recipeMaterial.MaterialMasterId,recipeMaterial.ArticleId);
        //        if (IsDuplicateEntryAllowed)
        //        {
        //            _recipeMaterialService.Insert(recipeMaterial);
        //        }
        //        else
        //        {
        //            //if (!string.IsNullOrEmpty(recipeMaterial.ArticleId))
        //            //    throw new CustomException("Selected Material has no attribute and Article has attribute, so it can not be added again..."); 
        //            //else
        //            var name= _recipeMaterialService.GetMaterialAtricleName(recipeMaterial.RecipeGlobalMasterId, recipeMaterial.MaterialMasterId, recipeMaterial.ArticleId);
        //                throw new CustomException("Selected Article already exists in  " + name + " Group.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return Json(new { RecipeMaterial = recipeMaterial, Message = AplosMessage.Success });
        //}

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _recipeMaterialService.DeleteRecipeMaterial(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion
    }
}