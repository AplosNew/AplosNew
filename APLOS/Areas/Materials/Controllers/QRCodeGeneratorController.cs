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
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Zen.Barcode;
using System.IO.Ports;
using System.Drawing;
using Syncfusion.Pdf;
using Syncfusion.PresentationToPdfConverter;
using Syncfusion.DocIO.DLS;
using System.Text.RegularExpressions;
using Syncfusion.DocToPDFConverter;
using System.Diagnostics;
using System.Text;
using System.Drawing.Imaging;

namespace Aplos.Areas.Materials.Controllers
{
    public class QRCodeGeneratorController : Controller
    {

        private readonly SqlRepository _sqlRepository;
        public string DataReceived = "";

        SerialPort serialPort = new SerialPort("COM9", 19200, Parity.None, 8, StopBits.One);
        Dictionary<string, object> data;
        string ProductCode, PO, LOT, NumberOfCones, NetWeight, GrossWeight, Shade, Article = null;
        CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        public QRCodeGeneratorController(Dictionary<string, object> dta)
        {
            _sqlRepository = new SqlRepository();

        }
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult LoadGrid(string customerId, string poid)
        {
            string sql = "";
            if (poid != null)
            {
                sql = @"select distinct ''Id, MMA.StandardName Article, MMA.Id ArticleId, PL.Code ProductCode, PLA.AttributeValue Shade, PO.Id PO, PS.UserName ProductionStatus
                            from TRN.ProductionOrder PO
                            left join TRN.ProductionOrderDetail POD on POD.ProductionOrderId = PO.Id
                            left join TRN.SalesOrder SO on SO.Id = POD.SalesOrderId
                            left join TRN.MasterOrderItem MOI on MOI.Id = SO.MasterOrderItemId
                            left join TRN.MasterOrder MO on MO.Id = MOI.MasterOrderId
                            left join MST.MaterialMasterArticle MMA on MMA.Id = MOI.ArticleId
                            left join ProductLibraryAttribute PLA on PLA.ProductLibraryId = MOI.ProductLibraryId and PLA.UserName like 'sh%'
                            left join ProductLibrary PL on PL.Id = PLA.ProductLibraryId
                            left join HKP.ProductionStatus PS on PS.Id = PO.ProductionStatusId
                            where PS.UserName in ('Running', 'ToClose') and PO.Id = '" + poid + "'";
            }
            else if (customerId != null)
            {
                sql = @"select distinct ''Id, MMA.StandardName Article, MMA.Id ArticleId, PL.Code ProductCode, PLA.AttributeValue Shade, PO.Id PO, PS.UserName ProductionStatus
                            from TRN.ProductionOrder PO
                            left join TRN.ProductionOrderDetail POD on POD.ProductionOrderId = PO.Id
                            left join TRN.SalesOrder SO on SO.Id = POD.SalesOrderId
                            left join TRN.MasterOrderItem MOI on MOI.Id = SO.MasterOrderItemId
                            left join TRN.MasterOrder MO on MO.Id = MOI.MasterOrderId
                            left join MST.MaterialMasterArticle MMA on MMA.Id = MOI.ArticleId
                            left join ProductLibraryAttribute PLA on PLA.ProductLibraryId = MOI.ProductLibraryId and PLA.UserName like 'sh%'
                            left join ProductLibrary PL on PL.Id = PLA.ProductLibraryId
                            left join HKP.ProductionStatus PS on PS.Id = PO.ProductionStatusId
                            where PS.UserName in ('Running', 'ToClose') and MO.PartyId = '" + customerId + "'";
            }

            else
            {
                sql = @"select distinct ''Id, MMA.StandardName Article, MMA.Id ArticleId, PL.Code ProductCode, PLA.AttributeValue Shade, PO.Id PO, PS.UserName ProductionStatus
                            from TRN.ProductionOrder PO
                            left join TRN.ProductionOrderDetail POD on POD.ProductionOrderId = PO.Id
                            left join TRN.SalesOrder SO on SO.Id = POD.SalesOrderId
                            left join TRN.MasterOrderItem MOI on MOI.Id = SO.MasterOrderItemId
                            left join TRN.MasterOrder MO on MO.Id = MOI.MasterOrderId
                            left join MST.MaterialMasterArticle MMA on MMA.Id = MOI.ArticleId
                            left join ProductLibraryAttribute PLA on PLA.ProductLibraryId = MOI.ProductLibraryId and PLA.UserName like 'sh%'
                            left join ProductLibrary PL on PL.Id = PLA.ProductLibraryId
                            left join HKP.ProductionStatus PS on PS.Id = PO.ProductionStatusId
                            where PS.UserName in ('Running', 'ToClose')";

            }

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEntity()
        {
            string sql = @"select Id Value, UserName Text from org.Entity
                            where Active = 1
                            order by Text
                            OFFSET 1 ROWS";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        #region Unused PDFConverter
        public IPresentation CreateQRCode(Dictionary<string, object> data)
        {

            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";

                //string langName = "";
                string strPath = "";
                var fileName = "";

                fileName = "QRCode" + identity.PlantId + ".pptx";
                //fileName = "QRCode.pptx";
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
                     //productcodeText, "#"
                     data["ProductCode"].ToString(), "#"
                    , data["PO"].ToString(), "#"
                    , data["LOT"].ToString(), "#"
                    , data["NumberOfCones"].ToString(), "#"
                    , data["NetWeight"].ToString(), "#"
                    , data["Shade"].ToString(), "#"
                    , identity.UserId

                    ));

                IPresentation presentation = Presentation.Open(strPath);
                for (int i = 0; i < presentation.Slides.Count; i++)
                {
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "ProductCode", Convert.ToString(data["ProductCode"]), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "PO", Convert.ToString(data["PO"]), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "LOT", Convert.ToString(data["LOT"]), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "NumberOfCones", Convert.ToString(data["NumberOfCones"]), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "NETWEIGHT", Convert.ToString(data["NetWeight"]), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "GrossWeight", Convert.ToString(data["GrossWeight"]), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "Shade", Convert.ToString(data["Shade"]), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "Article", Convert.ToString(data["Article"]), "Kalpurush", 8);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "PackedBy", identity.UserId, "Kalpurush", 8);
                    CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;
                    System.Drawing.Image barcodeImg = qrCode.Draw(concatdata, 200, 2);
                    ConvertPresentationToPdf.SetQRCode(presentation.Slides[i], "EmpQR", barcodeImg);
                }


                return presentation;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion Unused PDFConverter

        #region Save & Generate
        public ActionResult GenerateQRCode(Dictionary<string, object> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {

                //if (Convert.ToDecimal(NetWeightText) < Convert.ToDecimal(data["MinWeight"]) || Convert.ToDecimal(NetWeightText) > Convert.ToDecimal(data["MaxWeight"]))
                //{
                //    //NetWeightText = NetWeightText.Remove(2, 4);
                //    throw new Exception(NetWeightText + " must be match with define min and max weight");
                //}

                //if (!String.IsNullOrEmpty(data["ProductCode"].ToString()))
                //{
                //    ProductCode = data["ProductCode"].ToString();
                //}

                if (!String.IsNullOrEmpty(data["PO"].ToString()))
                {
                    PO = data["PO"].ToString();
                }
                if (!String.IsNullOrEmpty(data["LOT"].ToString()))
                {
                    LOT = data["LOT"].ToString();
                }

                if (!String.IsNullOrEmpty(data["NumberOfCones"].ToString()))
                {
                    NumberOfCones = data["NumberOfCones"].ToString();
                }
                if (!String.IsNullOrEmpty(data["NetWeight"].ToString()))
                {
                    NetWeight = data["NetWeight"].ToString();
                }
                if (!String.IsNullOrEmpty(data["GrossWeight"].ToString()))
                {
                    GrossWeight = data["GrossWeight"].ToString();
                }
                //if (!String.IsNullOrEmpty(data["Shade"].ToString()))
                //{
                //    Shade = data["Shade"].ToString();
                //}
                if (!String.IsNullOrEmpty(data["Article"].ToString()))
                {
                    Article = data["Article"].ToString();
                }

                string TableName = "[dbo].[WeighingScaleData]";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id, Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = _Id;

                    data["UserId"] = identity.UserId;
                    AddNewRow(dsMaster.Tables[0], data);

                }
                else
                {
                    _Id = data["Id"].ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #endregion data update

                #region unuseddcmnt
                // var fileName = "QRCode.pptx";
                //var fileName = "QRCode" + identity.PlantId + ".pptx";

                //var datas = CreateQRCode(data);

                //string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                #endregion unuseddcmnt

                clsStaticInfo _info = new clsStaticInfo();

                _info.SaveDataSets(dsMaster);

                con.BeginTransaction();

                #region comment
                //if (System.IO.File.Exists(fullPath))
                //    System.IO.File.Delete(fullPath);
                //datas.Save(fullPath);

                //var pdffileName = "QRCode.pdf";
                //string pdffullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + pdffileName;

                //string pdffullPath = Path.Combine(ResourcesPathReader.GetQRPdfDocument(), pdffileName);




                //Opens a PowerPoint Presentation
                // IPresentation presentation = Presentation.Open(fullPath);
                //Converts the PowerPoint Presentation into PDF document
                //PdfDocument pdfDocument = PresentationToPdfConverter.Convert(presentation);
                //Saves the PDF document

                //if (System.IO.File.Exists(pdffullPath))
                //    System.IO.File.Delete(pdffullPath);

                //pdfDocument.Save(pdffullPath);
                //Closes the PDF document
                //pdfDocument.Close(true);
                //Closes the Presentation
                //presentation.Close();
                //This will open the PDF file so, the result will be seen in default PDF viewer
                //System.Diagnostics.Process.Start(pdffullPath);


                //con.executeQuery($"update dbo.WeighingScaleDataCapture set isQR = 1 where Id ='" + data["GrossWeightId"] + "'");
                //con.CommitTransaction();
                //PrintFiles(pdffullPath);
                #endregion comment

                var doc = new PrintDocument();
                var paperSize = new PaperSize("Custom", 520, 820);
                doc.DefaultPageSettings.PaperSize = paperSize;

                // doc.PrintPage += PrintPicture;
                doc.PrintPage += new PrintPageEventHandler(ProvideContent);


                doc.Print();

                return Json(new { Id, Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void PrintPicture(object sender, PrintPageEventArgs e)
        {
            string concatdata = Convert.ToString(
                    string.Concat(
                     //productcodeText, "#"
                     ProductCode, "#"
                    , PO, "#"
                    , LOT, "#"
                    , NumberOfCones, "#"
                    , NetWeight, "#"
                    , GrossWeight, "#"
                    , identity.UserId

                    ));

            CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;
            System.Drawing.Image barcodeImg = qrCode.Draw(concatdata, 200, 2);

            Bitmap bmp = new Bitmap(barcodeImg.Width, barcodeImg.Height);
            e.Graphics.DrawImage(bmp, 0, 0);
            bmp.Dispose();

        }


        public void ProvideContent(object sender, PrintPageEventArgs e)
        {
           
            int itemHeight = 0;
            var curX = e.MarginBounds.X;
            var curY = e.MarginBounds.Y;
           

            string concatdata = Convert.ToString(
                    string.Concat(                     
                     ProductCode, "#"
                    , PO, "#"
                    , LOT, "#"
                    , NumberOfCones, "#"
                    , NetWeight, "#"
                    , GrossWeight, "#"
                    , identity.UserId

                    ));

            CodeQrBarcodeDraw qrCode = BarcodeDrawFactory.CodeQr;
            var barcodeImg = qrCode.Draw(concatdata, 200, 2);

            //ConvertImagePNGToBMP(concatdata);

            var sb = new StringBuilder();

            sb.AppendLine(($"PRD.CD: {ProductCode}"));
            sb.AppendLine(($"PO: {PO} "));
            sb.AppendLine(($"LOT: {LOT} "));
            sb.AppendLine(($"REF.NO: "));
            sb.AppendLine(($"NO.OFCONES: {NumberOfCones} "));
            sb.AppendLine(($"NET WEIGHT: {NetWeight} "));
            sb.AppendLine(($"GRS. WEIGHT: {GrossWeight} "));
            sb.AppendLine(($"PACKED BY: {identity.UserId}"));
            sb.AppendLine(($"SHADE: {Shade}"));
            sb.AppendLine(($"ARTICLE: {Article}"));


            var printText = new PrintText(sb.ToString(), new Font(System.Drawing.FontFamily.GenericSansSerif, 9, System.Drawing.FontStyle.Bold));
            Graphics graphics = e.Graphics;

            using (var fontNormal = new Font("Arial", 9))
            using (var sf = new StringFormat())
            {
                sf.Alignment = sf.LineAlignment = StringAlignment.Far;
                itemHeight = (int)fontNormal.GetHeight(e.Graphics) + 10;


                var imgRect = new Rectangle(150, 50, 90, 90);
                //var labelRect = new Rectangle(150, 50, imgRect.Width, itemHeight);

                using (var qrImage = barcodeImg)
                    e.Graphics.DrawImage(qrImage, imgRect);
               
            }


            int startX = 0;
            int startY = 0;
            int Offset = 20;

            graphics.DrawString(printText.Text, new Font(System.Drawing.FontFamily.GenericMonospace, 9, System.Drawing.FontStyle.Bold),
                                new SolidBrush(System.Drawing.Color.Black), startX, startY + Offset);
            Offset = Offset + 20;
        }
        #endregion Save & Generate

        #region GeFun
        public ActionResult GetPO()
        {
            string poSql = @"select PO.Id Value, PO.Id Text from TRN.ProductionOrder  PO
                            left join HKP.ProductionStatus PS on PS.Id = PO.ProductionStatusId
                            where PS.UserName in ('Running', 'To Close')";
            return Json(_sqlRepository.GetDataCollection(poSql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWeighingScale()
        {
            string poSql = @"Select Id Value, UserName Text from HKP.WeighingScaleMaster order by Text";
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
                    where PL.ArticleId = '" + articleid + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetShade(string prodId)
        {
            string sql = @"select Id Value, AttributeValue Text from ProductLibraryAttribute where ProductLibraryId = '" + prodId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetNetWeight()
        {
            string sql = @"select top(1) Id Value, [NET WEIGHT06] Text from WeighingScaleDataCapture where isQR = 0 order by AddedDate desc";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGrossWeight(int mno)
        {
            string sql = @"select top(1) Id Value, [G. WEIGHT07] Text from WeighingScaleDataCapture where isQR = 0 and MNo = '" + mno + "' order by AddedDate desc";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #endregion GeFun

        #region Add & Edit Row
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
        #endregion Add & Edit Row

        #region WeighingScal Con & Read
        public double GenerateReferenceNumber()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(StartRefNo),0) AS Sequence FROM MST.MaterialMovementMaster");
            DataTable pref_dt = _sqlRepository.GetDataTable("SELECT Prefix FROM MST.MaterialMovementMaster");
            if (dt.Rows.Count > 0)
            {
                if (pref_dt.Rows.Count > 0)
                {
                    return clsStaticInfo.dbl(pref_dt.Rows[0]["Prefix"].ToString()) + clsStaticInfo.dbl(dt.Rows[0]["StartRefNo"].ToString()) + 1;

                }
            }
            return 1;

        }

        public ActionResult GetPort()
        {


            try
            {

                List<Item> items = new List<Item>();

                foreach (var item in SerialPort.GetPortNames())
                {
                    items.Add(new Item() { Text = item, Value = item });
                }



                return Json(items, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool PassConnection()
        {
            try
            {
                Disconnect();
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();

                }

                //var data = Read();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public string Connect()
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();

                }

                var data = Read();
                Disconnect();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public void Disconnect()
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    //MessageBox.Show("Disconnected");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string Read()
        {
            try
            {

                this.DataReceived = serialPort.ReadLine().ToString();


                return (this.DataReceived);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion WeighingScal Con & Read
    }

    #region HyperTerminalAdapter
    public class HyperTerminalAdapter
    {
        SerialPort oSerialPort = new SerialPort();

        // Allow the user to set the appropriate properties. 
        public int BaudRate = 9600;
        public int DataBits = 8;
        public int ReadTimeout = 500;
        public int WriteTimeout = 500;
        public string PortName = "COM4";
        public string Handshake = "";
        public string Name = "user";
        public string DataReceived = "";
        public string sParity = "none";
        public int iStopBits = 1;

        public HyperTerminalAdapter()
        {
            this.Configure();
        }

        public void Configure()
        {
            oSerialPort.PortName = this.PortName;
            oSerialPort.BaudRate = this.BaudRate;
            oSerialPort.DataBits = this.DataBits;
            oSerialPort.ReadTimeout = this.ReadTimeout;
            oSerialPort.WriteTimeout = this.WriteTimeout;

            oSerialPort.Handshake = System.IO.Ports.Handshake.None;

            if (this.sParity == "even")
            {
                oSerialPort.Parity = Parity.Even;
            }
            else if (this.sParity == "odd")
            {
                oSerialPort.Parity = Parity.Odd;
            }
            else if (this.sParity == "mark")
            {
                oSerialPort.Parity = Parity.Mark;
            }
            else if (this.sParity == "space")
            {
                oSerialPort.Parity = Parity.Space;
            }
            else
            {
                oSerialPort.Parity = Parity.None;
            }

            if (this.iStopBits == 0)
            {
                oSerialPort.StopBits = StopBits.None;
            }
            else if (this.iStopBits == 1.5)
            {
                oSerialPort.StopBits = StopBits.OnePointFive;
            }
            else if (this.iStopBits == 2)
            {
                oSerialPort.StopBits = StopBits.Two;
            }
            else
            {
                oSerialPort.StopBits = StopBits.One;
            }

            //MessageBox.Show("Configured");
        }

        public void Connect()
        {
            try
            {
                if (!oSerialPort.IsOpen)
                {
                    oSerialPort.Open();
                    //MessageBox.Show("Connected");
                }
            }
            catch (Exception)
            {
                // MessageBox.Show("Error: Connection is in use or is not available: \n\n" + e1);

            }
        }

        public void Disconnect()
        {
            try
            {
                if (oSerialPort.IsOpen)
                {
                    oSerialPort.Close();
                    //MessageBox.Show("Disconnected");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void Write(string sData /* string data to write to the port */ )
        {

        }

        public string Read()
        {
            try
            {
                this.DataReceived = oSerialPort.ReadLine().ToString();
                // MessageBox.Show(this.DataReceived);
                return (this.DataReceived);
            }
            catch
            {
                return "";
            }
        }


    }
    #endregion HyperTerminalAdapter

    #region Item Properties class
    public class Item
    {
        public Item() { }

        public string Value { set; get; }
        public string Text { set; get; }
    }
    #endregion Item Properties class

    #region PrintText
    public class PrintText
    {
        public PrintText(string text, Font font) : this(text, font, new StringFormat()) { }

        public PrintText(string text, Font font, StringFormat stringFormat)
        {
            Text = text;
            Font = font;
            StringFormat = stringFormat;
        }

        public string Text { get; set; }

        public Font Font { get; set; }

        /// <summary> Default is horizontal string formatting </summary>
        public StringFormat StringFormat { get; set; }
    }
    #endregion PrintText
}