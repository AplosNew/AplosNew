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
	public class ProductionTransformationBookingController : BaseController
	{
        ProductionTransformationBooking PB = new ProductionTransformationBooking();

		#region Constructor
		private readonly SqlRepository _sqlRepository;
		public ProductionTransformationBookingController(SqlRepository Repository)
		{
			_sqlRepository = Repository;
            PB = new ProductionTransformationBooking();
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

                return Json(PB.getProcesslist(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetProcessIdDisplay(string ProcessId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.GetProcessIdDisplay(ProcessId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getWorkCentreCategoryGrouplist()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.getWorkCentreCategoryGrouplist(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getDependantProcesslist(string MasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.getDependantProcesslist(MasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getOutputItemNamelist()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.getOutputItemNamelist(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getEntryQuantityUOMList(string OutputItenNameId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.getEntryQuantityUOMList(OutputItenNameId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetOutputItemParameter(string OutputItenNameId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.GetOutputItemParameter(OutputItenNameId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getOutputItemUOMList(string OutputItenNameId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.getOutputItemUOMList(OutputItenNameId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getInputItemNameList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.getInputItemNameList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getInputUOMList(string InputItenNameId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.getInputUOMList(InputItenNameId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getByProductItemNameList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.getByProductItemNameList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getByProductUOMList(string ByProductNameId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.getByProductUOMList(ByProductNameId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence(string ProductionBookingId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.GetAutoSequence(ProductionBookingId), JsonRequestBehavior.AllowGet);
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

                return Json(PB.GetList(column, value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public JsonResult LoadAllEmpDetails(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.LoadAllEmpDetails(Id), JsonRequestBehavior.AllowGet);
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
                PB.Create(data);
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
                PB.delete(Id);
                return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        // Detail Save

        [HttpPost]
        public JsonResult detailSave(Dictionary<string, object> data, string MasterId)
        {
            try
            {
                PB.detailSave(data, MasterId);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion

        [Authorize, HttpGet]
        public JsonResult GetDetailData(string ProductionBookingId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(PB.GetDetailData(ProductionBookingId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult DelBookingDetails(string Id)
        {
            try
            {
                PB.DelBookingDetails(Id);
                return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


    }
}