using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Employees;
using Library.Service.Helpers;
using Syncfusion.Presentation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Zen.Barcode;

namespace Aplos.Areas.Materials.Controllers
{
    public class QRCodeGeneratorController : Controller
    {
        private readonly SqlRepository _sqlRepository;
        public QRCodeGeneratorController()
        {
            _sqlRepository = new SqlRepository();
        }
        public ActionResult Aplos()
        {
            return View();
        }

        public IPresentation CreateQRCode(Dictionary<string, object> data, string ShadeText)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string langName = "";
                string strPath = "";
                var fileName = "";

                fileName = "QRCode" + identity.PlantId + langName + ".pptx";
                strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);  // IDCardEng.xlsx
                File = fileName;

                if (!string.IsNullOrEmpty(fileName))
                {
                    strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);  // IDCardEng.xlsx
                    File = fileName;
                    if (!System.IO.File.Exists(strPath))
                    {
                        throw new CustomException("File Not Found");
                    }
                }

                string concatdata = Convert.ToString(string.Concat(data["ProductCode"].ToString(), "#"
                    , data["PO"].ToString(), "#"
                    , data["LOT"].ToString(), "#"
                    , data["NumberOfCones"].ToString(), "#"
                    , data["NetWeight"].ToString(), "#"
                    , ShadeText
                    ));

                IPresentation presentation = Presentation.Open(strPath);
                for (int i = 0; i < presentation.Slides.Count; i++)
                {
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "PO", Convert.ToString(data["PO"]), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "Shade", ShadeText, "Kalpurush", 8);

                    CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;
                    System.Drawing.Image barcodeImg = qrCode.Draw(concatdata, 200, 2);
                    ConvertPresentationToPdf.SetQRCode(presentation.Slides[i], "EmpQR", barcodeImg);
                }
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                //workbook.Save(fullPath);
                return presentation;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        public ActionResult GenerateQRCode(Dictionary<string, object> data, string ShadeText)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "QRCode" + identity.UserId + ".pptx";

           var datas = CreateQRCode(data, ShadeText);

            string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
            datas.Save(fullPath);
            return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
        }

        #region GeFun
        public ActionResult GetPO()
        {
            string poSql = @"select PO.Id Text from TRN.ProductionOrder  PO
                            left join HKP.ProductionStatus PS on PS.Id = PO.ProductionStatusId
                            where PS.UserName in ('Running', 'To Close')";
            return Json(_sqlRepository.GetDataCollection(poSql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetArticle(string poid)
        {
            string sql = @"select MMA.Id Value, MMA.StandardName Text from MST.MaterialMasterArticle MMA
                            left join TRN.MasterOrderItem MSI on MSI.ArticleId = MMA.Id
                            left join TRN.SalesOrder SO on SO.MasterOrderItemId = MSI.Id
                            left join TRN.ProductionOrderDetail POD on POD.SalesOrderId = SO.Id
                            left join TRN.ProductionOrder PO on PO.Id = POD.ProductionOrderId
                            where PO.Id = '" + poid + @"' 
                            order by StandardName";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetProductCode(string articleid)
        {
            string sql = @"
                    Select PL.Id Value,  PL.Code, ArticleId from ProductLibrary PL
                    where PL.ArticleId = '"+ articleid + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetShade(string prodId)
        {
            string sql = @"select Id Value, AttributeValue Text from ProductLibraryAttribute where ProductLibraryId = '"+ prodId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
       
        #endregion GeFun
    }
}