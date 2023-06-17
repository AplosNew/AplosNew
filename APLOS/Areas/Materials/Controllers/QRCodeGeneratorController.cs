using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zen.Barcode;

namespace Aplos.Areas.Materials.Controllers
{
    public class QRCodeGeneratorController : Controller
    {
        
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult Aplos(Dictionary<string, object> data)
        {
            CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;
            System.Drawing.Image barcodeImg = qrCode.Draw(data["Id"].ToString(), 200, 2);

            return View();
        }
    }
}