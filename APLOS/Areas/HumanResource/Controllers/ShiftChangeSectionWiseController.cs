using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.PlantWiseHRMS;
using Library.HumanResource.ShiftChange;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
	public class ShiftChangeSectionWiseController : BaseController
	{

		#region Constructor
		private readonly ISqlRepository _sqlRepository;
		public ShiftChangeSectionWiseController(ISqlRepository R)
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
		[HttpGet, Authorize]
		public ActionResult GetSection()
		{
			try
			{				 
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				clsShiftChangeSectionWise ep = new clsShiftChangeSectionWise();
				return Json(ep.GetSection(identity.PlantId,identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}

		[HttpPost, Authorize]
		public ActionResult GetEmployee(string section)
		{
			try
			{
				string all = "ALL";
				string sSect = section;
				string lblSelectedEmpList = "";
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				clsShiftChangeSectionWise ep = new clsShiftChangeSectionWise();
				return Json(ep.GetEmployee(lblSelectedEmpList, all, all, all, identity.PlantId, all, all, all, sSect, all, all, all, all, all), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}

        [HttpPost, Authorize]
        public ActionResult LoadEmp(string section,string date)
        {
            try
            {
                string all = "ALL";
                string sSect = section;
                string lblSelectedEmpList = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsShiftChangeSectionWise ep = new clsShiftChangeSectionWise();
                return Json(ep.LoadEmployeeDailyShift(lblSelectedEmpList, all, all, date, identity.PlantId, all, all, all, sSect, all, all, all, all, all), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion -- Operations
    }
}