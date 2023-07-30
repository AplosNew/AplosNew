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
using System.Threading;
using System.Drawing;

namespace Aplos.Areas.Materials.Controllers
{
    public class QRCodeGeneratorController : Controller
    {
        private Font verdana10Font;
        private StreamReader reader;
        private readonly SqlRepository _sqlRepository;
        SerialPort serialPort = new SerialPort("COM9", 19200, Parity.None, 8, StopBits.One);
        public QRCodeGeneratorController()
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
            else if (customerId != null) {
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

        public ActionResult GetEntity() { 
            string sql = @"select Id Value, UserName Text from org.Entity
                            where Active = 1
                            order by Text
                            OFFSET 1 ROWS";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        private void PrintTextFileHandler (object sender, PrintPageEventArgs ppeArgs)
        {
            //Get the Graphics object

            Graphics g = ppeArgs.Graphics;
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            //Read margins from PrintPageEventArgs
            float leftMargin = ppeArgs.MarginBounds.Left;
            float topMargin = ppeArgs.MarginBounds.Top;
            string line = null;
            //Calculate the lines per page on the basis of the height of the page and the height of the font
            linesPerPage = ppeArgs.MarginBounds.Height;
            //verdana10Font.GetHeight (g);
            //Now read lines one by one, using StreamReader
            while ( ( line = reader.ReadLine ()) != null)
            {
                //Calculate the starting position
                yPos = topMargin + (count *
                verdana10Font.GetHeight (g));
                //Draw text
                g.DrawString (line, verdana10Font, Brushes.Black,
                leftMargin, yPos, new StringFormat());
                //Move to next line
                count++;
            }
            //If PrintPageEventArgs has more pages to print
            if (line != null)
            {
                ppeArgs.HasMorePages = true;
            }
            else
            {
                ppeArgs.HasMorePages = false;
            }
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

                PrintDocument pd = new PrintDocument();
                //Set PrinterName as the selected printer in the printers list  
                //pd.PrinterSettings.PrinterName = "HPRT HT300 - ZP";

                // Add PrintPage event handler
                // pd.PrintPage += new PrintPageEventHandler();

                //Print the document  
                //pd.Print();

                string concatdata = Convert.ToString(
                    string.Concat(
                     productcodeText, "#"
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
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "ProductCode", Convert.ToString(data["ProductCode"]), "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "PO", Convert.ToString(data["PO"]), "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "LOT", Convert.ToString(data["LOT"]), "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "NumberOfCones", Convert.ToString(data["NumberOfCones"]), "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "NETWEIGHT", Convert.ToString(data["NetWeight"]), "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "GWeight", Convert.ToString(data["GrossWeight"]), "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "Shade", Convert.ToString(data["Shade"]), "Kalpurush", 18);
                    ConvertPresentationToPdf.SetText(presentation.Slides[i], "Article", Convert.ToString(data["Article"]), "Kalpurush", 18);
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

                //if (Convert.ToDecimal(NetWeightText) < Convert.ToDecimal(data["MinWeight"]) || Convert.ToDecimal(NetWeightText) > Convert.ToDecimal(data["MaxWeight"]))
                //{
                //    //NetWeightText = NetWeightText.Remove(2, 4);
                //    throw new Exception(NetWeightText + " must be match with define min and max weight");
                //}
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
                    //data["ProductCode"] = productcodeText;
                    //data["NetWeight"] = NetWeightText;
                    //data["Shade"] = ShadeText;
                    //data["Article"] = ArticleName;
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

                con.executeQuery($"update dbo.WeighingScaleDataCapture set isQR = 1 where Id ='" + data["GrossWeightId"] + "'");
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

        public ActionResult GetGrossWeight(int mno)
        {
            string sql = @"select top(1) Id Value, [G. WEIGHT07] Text from WeighingScaleDataCapture where isQR = 0 and MNo = '"+mno+"' order by AddedDate desc";
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

        public double GenerateReferenceNumber()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(StartRefNo),0) AS Sequence FROM MST.MaterialMovementMaster");
            DataTable pref_dt = _sqlRepository.GetDataTable("SELECT Prefix FROM MST.MaterialMovementMaster");
            if (dt.Rows.Count > 0)
            {
                if(pref_dt.Rows.Count > 0) {
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

        public string Connect()
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();

                    
                }
                var data = string.Format("{0:X2} ", serialPort.ReadExisting());
                return data;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
    }

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

    public class Item
    {
        public Item() { }

        public string Value { set; get; }
        public string Text { set; get; }
    }
}