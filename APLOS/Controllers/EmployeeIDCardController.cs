using Library.Model.Employees;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class EmployeeIDCardController : BaseController
    {
        public EmployeeIDCardController()
        {

        }
        public ActionResult PrintEmployeeIDCard()
        {
            //List<AplosCustomerData> data = new List<AplosCustomerData>();
            //data.Add(new AplosCustomerData { CustomerID = "1", EmployeeID = "Tarek", OrderID = "001", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "2", EmployeeID = "Ather", OrderID = "002", ShipCountry = "Bangladesh", Freight = 3433 });
            //data.Add(new AplosCustomerData { CustomerID = "3", EmployeeID = "Rasel", OrderID = "003", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "4", EmployeeID = "Monir", OrderID = "001", ShipCountry = "Bangladesh", Freight = 1200 });
            //data.Add(new AplosCustomerData { CustomerID = "5", EmployeeID = "Mizan", OrderID = "004", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "6", EmployeeID = "Raju", OrderID = "005", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "7", EmployeeID = "Sanjay", OrderID = "001", ShipCountry = "Bangladesh", Freight = 1200 });
            //data.Add(new AplosCustomerData { CustomerID = "8", EmployeeID = "Karim", OrderID = "005", ShipCountry = "Bangladesh", Freight = 3480 });
            //data.Add(new AplosCustomerData { CustomerID = "9", EmployeeID = "Prodipta", OrderID = "003", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "10", EmployeeID = "Sazzad", OrderID = "002", ShipCountry = "Bangladesh", Freight = 500 });
            //data.Add(new AplosCustomerData { CustomerID = "11", EmployeeID = "Pavel", OrderID = "004", ShipCountry = "Bangladesh", Freight = 300 });
            //data.Add(new AplosCustomerData { CustomerID = "1", EmployeeID = "Tarek", OrderID = "001", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "2", EmployeeID = "Ather", OrderID = "002", ShipCountry = "Bangladesh", Freight = 3433 });
            //data.Add(new AplosCustomerData { CustomerID = "3", EmployeeID = "Rasel", OrderID = "003", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "4", EmployeeID = "Monir", OrderID = "001", ShipCountry = "Bangladesh", Freight = 1200 });
            //data.Add(new AplosCustomerData { CustomerID = "5", EmployeeID = "Mizan", OrderID = "004", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "6", EmployeeID = "Raju", OrderID = "005", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "7", EmployeeID = "Sanjay", OrderID = "001", ShipCountry = "Bangladesh", Freight = 1200 });
            //data.Add(new AplosCustomerData { CustomerID = "8", EmployeeID = "Karim", OrderID = "005", ShipCountry = "Bangladesh", Freight = 3480 });
            //data.Add(new AplosCustomerData { CustomerID = "9", EmployeeID = "Prodipta", OrderID = "003", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "10", EmployeeID = "Sazzad", OrderID = "002", ShipCountry = "Bangladesh", Freight = 500 });
            //data.Add(new AplosCustomerData { CustomerID = "11", EmployeeID = "Pavel", OrderID = "004", ShipCountry = "Bangladesh", Freight = 300 });
            //data.Add(new AplosCustomerData { CustomerID = "1", EmployeeID = "Tarek", OrderID = "001", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "2", EmployeeID = "Ather", OrderID = "002", ShipCountry = "Bangladesh", Freight = 3433 });
            //data.Add(new AplosCustomerData { CustomerID = "3", EmployeeID = "Rasel", OrderID = "003", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "4", EmployeeID = "Monir", OrderID = "001", ShipCountry = "Bangladesh", Freight = 1200 });
            //data.Add(new AplosCustomerData { CustomerID = "5", EmployeeID = "Mizan", OrderID = "004", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "6", EmployeeID = "Raju", OrderID = "005", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "7", EmployeeID = "Sanjay", OrderID = "001", ShipCountry = "Bangladesh", Freight = 1200 });
            //data.Add(new AplosCustomerData { CustomerID = "8", EmployeeID = "Karim", OrderID = "005", ShipCountry = "Bangladesh", Freight = 3480 });
            //data.Add(new AplosCustomerData { CustomerID = "9", EmployeeID = "Prodipta", OrderID = "003", ShipCountry = "Bangladesh", Freight = 3400 });
            //data.Add(new AplosCustomerData { CustomerID = "10", EmployeeID = "Sazzad", OrderID = "002", ShipCountry = "Bangladesh", Freight = 500 });
            //data.Add(new AplosCustomerData { CustomerID = "11", EmployeeID = "Pavel", OrderID = "004", ShipCountry = "Bangladesh", Freight = 300 });
            //ViewBag.DataSource = data;

            // generateIDCardReportEnglish();
            return View();
        }

        private void generateIDCardReportEnglish()
        {

            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet = null;

                string fileLocation = Server.MapPath("~/POPResources/Templates/IDCardBengali.xlsx");


                //string fileLocation = ResourcesPathReader.GetConfirmationLetterPath()+ "IDCardEng.xlsx";
                //fileLocation.Replace("/", "\\");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2013;

                workbook = excelEngine.Excel.Workbooks.Open(fileLocation, ExcelOpenType.Automatic);
                sheet = workbook.Worksheets[0];



                int COL = 9;
                int ROW = 1;

                sheet.HideColumn(COL);

               
                FormatTextBox(ref sheet, "BloodGroup", "বি+", 6, ExcelKnownColors.Red);
                FormatTextBox(ref sheet, "PermanentAddress", "২৩/১২, খিলজি রোড, শ্যামলী, ঢাকা-১২০৭", 6, ExcelKnownColors.Black);
                FormatTextBox(ref sheet, "PhoneNumber", "০১৯২৮৭০০০০৫", 6, ExcelKnownColors.Black);
                FormatTextBox(ref sheet, "NID", "১৫০৯৯৮৩৭৩৬৪৫৩৫", 6, ExcelKnownColors.Black);
                FormatTextBox(ref sheet, "Name", "তারেক তালুকদার", 6, ExcelKnownColors.Black);
                FormatTextBox(ref sheet, "DESIG", "প্রযোজ্য নয়", 6, ExcelKnownColors.Black);

                FormatTextBox(ref sheet, "ID", "১৮০০০৫", 6, ExcelKnownColors.Black);
                FormatTextBox(ref sheet, "Department", "আইটি", 6, ExcelKnownColors.Black);
                FormatTextBox(ref sheet, "WorkType", "", 6, ExcelKnownColors.Black);
                FormatTextBox(ref sheet, "DOJ", "১৮-এপ্রিল-২০১৯",6, ExcelKnownColors.Black);
                FormatTextBox(ref sheet, "IssueDate", "২৩-এপ্রিল-২০১৯", 6, ExcelKnownColors.Black);

             
                int x = sheet.Pictures.Count;
                IPictureShape oldImage = sheet.Pictures["EmpPicture"];
                int leftPosition = oldImage.Left;
                int topPosition = oldImage.Top;
                int height = oldImage.Height;
                int width = oldImage.Width;
                oldImage.Remove();

                string ImagefileLocation = Server.MapPath("~/POPResources/EmployeeProfiles/EmpPic/1801630.jpg");
                //string ImagefileLocation = ResourcesPathReader.GetEmployeePicPath() + "801630.jpg";
                //ImagefileLocation.Replace("/", "\\");

                IPictureShape newImage = sheet.Pictures.AddPicture(ImagefileLocation);
                newImage.Left = leftPosition;
                newImage.Top = topPosition;
                newImage.Height = height;
                newImage.Width = width;

                 


                var fileName = "IDCARD " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
                workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            }
            catch (Exception ex)
            {


            }

        }

        private void FormatTextBox(ref IWorksheet sheet, string TextBoxName, string Text, float FontSize, ExcelKnownColors FontColor)
        {
            Text = Text == "" ? " " : Text;

            ITextBoxShape textbox = sheet.TextBoxes[TextBoxName];
            textbox.Text = Text;
            IRichTextString rtf = textbox.RichText;
            IFont font = sheet.Workbook.CreateFont();
            font.Color = FontColor;
            font.Size = FontSize;
            rtf.SetFont(0, textbox.Text.Length - 1, font);

            textbox.RichText = rtf;
            textbox.Fill.ForeColor = Color.White;
            textbox.Fill.BackColor = Color.Gold;

        }
    }


    public class AplosCustomerData
    {
        public string OrderID { get; set; } = "";
        public string EmployeeID { get; set; } = "";
        public string CustomerID { get; set; } = "";
        public string ShipCountry { get; set; } = "";
        public double Freight { get; set; } = 0;



    }
}
