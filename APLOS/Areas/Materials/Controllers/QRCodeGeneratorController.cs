using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Security.Core;
using Library.Service.Employees;
using Library.Service.Helpers;
using Syncfusion.Presentation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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

        public IPresentation CreateQRCode(Dictionary<string, object> data, string ShadeText, string ArticleName, string productcodeText, string NetWeightText)
        {
           
            try
            {
                
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
               
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

                string concatdata = Convert.ToString(
                    string.Concat(
                     productcodeText, "#"
                    , data["PO"].ToString(), "#"
                    , data["LOT"].ToString(), "#"
                    , data["NumberOfCones"].ToString(), "#"
                    , NetWeightText, "#"
                    , ShadeText, "#"
                    , identity.UserId

                    ));

                IPresentation presentation = Presentation.Open(strPath);
                for (int i = 0; i < presentation.Slides.Count; i++)
                {
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "ProductCode", productcodeText, "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "PO", Convert.ToString(data["PO"]), "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "LOT", Convert.ToString(data["LOT"]), "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "NumberOfCones", Convert.ToString(data["NumberOfCones"]), "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "NETWEIGHT", NetWeightText, "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "Shade", ShadeText, "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "Article", ArticleName, "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "PackedBy", identity.UserId, "Kalpurush", 18);
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

        public ActionResult GenerateQRCode(Dictionary<string, object> data, string ShadeText, string ArticleName, string productcodeText, string NetWeightText)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            
            try
            {

                if (Convert.ToDecimal(NetWeightText) < Convert.ToDecimal(data["MinWeight"]) || Convert.ToDecimal(NetWeightText) > Convert.ToDecimal(data["MaxWeight"]))
                {
                    //NetWeightText = NetWeightText.Remove(2, 4);
                    throw new Exception(NetWeightText + " must be match with define min and max weight");
                }
                string TableName = "[dbo].[WeighingScaleData]";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = _Id;
                    data["productcode"] = productcodeText;
                    data["NetWeight"] = NetWeightText;
                    data["Shade"] = ShadeText;
                    data["Article"] = ArticleName;
                    data["UserId"] = identity.UserId;
                    AddNewRow(dsMaster.Tables[0], data);

                }
                else
                {
                    _Id = data["Id"].ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                var fileName = "QRCode" + identity.UserId + ".pptx";

                var datas = CreateQRCode(data, ShadeText, ArticleName, productcodeText, NetWeightText);

                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                datas.Save(fullPath);
                con.BeginTransaction();

                con.executeQuery($"update dbo.WeighingScaleDataCapture set isQR = 1 where Id ='" + data["NetWeightId"] + "'");
                con.CommitTransaction();
                return Json(new { FileName = fileName, Error = false, Message = AplosMessage.Insert });
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
            
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
        
        public ActionResult GetNetWeight()
        {
            string sql = @"select top(1) Id Value, [NET WEIGHT06] Text from WeighingScaleDataCapture where isQR = 0 order by AddedDate desc";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #endregion GeFun

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;


            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
    }
}