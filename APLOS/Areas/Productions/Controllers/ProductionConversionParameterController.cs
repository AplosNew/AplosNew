using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using OTSBD;
using Library.Service.Productions;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Data;

namespace Aplos.Areas.Productions.Controllers
{
	public class ProductionConversionParameterController : BaseController
	{
        ProductionConversionParameter PCP = new ProductionConversionParameter();

		#region Constructor
		private readonly SqlRepository _sqlRepository;
		public ProductionConversionParameterController(SqlRepository Repository)
		{
			_sqlRepository = Repository;
            PCP = new ProductionConversionParameter();
		}
		#endregion
		#region Pages
		public ActionResult Aplos()
		{
			return View();
		}
		#endregion

		#region Load Data

		[Authorize, HttpGet]
		public JsonResult getProcesslist()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				return Json(PCP.getProcesslist(), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

        [Authorize, HttpGet]
        public JsonResult getOutputUoMlist()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PCP.getOutputUoMlist(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getUoMlist()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PCP.getUoMlist(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getEntryUoMList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PCP.getEntryUoMList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public JsonResult GetList(string column, string value)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PCP.GetList(column, value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
		public JsonResult Create(Dictionary<string, object> data)
		{
			try
			{
                PCP.Create(data);
				return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}

        [HttpPost]
        public JsonResult delete(string Id)
        {
            try
            {
                PCP.delete(Id);
                return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }




  //      private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
		//{
		//	var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
		//	DataRow dr = dt.NewRow();

		//	foreach (var item in sourceData.Keys)
		//	{
		//		try
		//		{
		//			dr[item] = sourceData[item];
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	dr["AddedBy"] = identity.Name;
		//	dr["AddedDate"] = System.DateTime.Now.ToString();
		//	dr["AddedFromIP"] = identity.IPAddress;
		//	dr["UpdatedBy"] = identity.Name;
		//	dr["UpdatedDate"] = System.DateTime.Now.ToString();
		//	dr["UpdatedFromIP"] = identity.IPAddress;

		//	dt.Rows.Add(dr);
		//}
		//private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
		//{
		//	var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
		//	dr.BeginEdit();

		//	foreach (var item in sourceData.Keys)
		//	{
		//		try
		//		{
		//			dr[item] = sourceData[item];
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	dr["UpdatedBy"] = identity.Name;
		//	dr["UpdatedDate"] = System.DateTime.Now.ToString();
		//	dr["UpdatedFromIP"] = identity.IPAddress;
		//	dr.EndEdit();
		//}

		#endregion

	}
}