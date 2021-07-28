#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using Library.OrderManagement.Production;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Library.Data;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using System.Text.RegularExpressions;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Aplos.Areas.Commercial.Controllers;
using System.Drawing;
using Library.OrderManagement.Sales;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class PackingInvoiceController : BaseController
    {
        PackingData det = new PackingData();
        clsSales clsSales = new clsSales();
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public PackingInvoiceController(ISqlRepository R)
        {
            _sqlRepository = R;
            det = new PackingData();
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetPackingSOData(string PackingId)
        {
            return Json(clsSales.GetPackingSOData(PackingId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPackingData()
        {
            return Json(det.GetPackingData(), JsonRequestBehavior.AllowGet);
        }


    }
}