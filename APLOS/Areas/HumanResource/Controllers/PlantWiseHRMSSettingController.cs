using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.PlantWiseHRMS;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
	public class PlantWiseHRMSSettingController : BaseController
	{

		#region Constructor
		private readonly ISqlRepository _sqlRepository;
		public PlantWiseHRMSSettingController(ISqlRepository R)
		{
			_sqlRepository = R;
		}
		#endregion Constructor

		#region  -- Pages
		public ActionResult Aplos()
		{
			return View();
		}

		#endregion -- Pages

		#region -- Operations
		[HttpPost, Authorize]
		public ActionResult GetList(string CompanyId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsPlantWiseHRMSSetting pw = new clsPlantWiseHRMSSetting();
				return Json(pw.GetList(CompanyId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}

		[HttpPost, Authorize]
		public ActionResult GetModPlant(string CompanyId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				clsPlantWiseHRMSSetting pw = new clsPlantWiseHRMSSetting();
				return Json(pw.GetModPlant(CompanyId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}
		[HttpPost, Authorize]
		public ActionResult GetPlantList(string PlantID)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				clsPlantWiseHRMSSetting pw = new clsPlantWiseHRMSSetting();
				return Json(pw.GetPlantList(PlantID), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}

		[HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				clsPlantWiseHRMSSetting hr = new clsPlantWiseHRMSSetting();
				hr.Save(data); 
				return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }


        #endregion -- Operations
    }
}