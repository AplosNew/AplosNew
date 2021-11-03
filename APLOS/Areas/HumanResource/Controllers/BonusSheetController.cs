using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class 
        BonusSheetController : BaseController
	{
        #region Constructor
        private readonly ICompliedShiftService _compliedShiftService;
        Library.HumanResource.Report.Payroll.clsPayRegister _clspayRegisterBDReportService = new Library.HumanResource.Report.Payroll.clsPayRegister();

        public BonusSheetController(
			  ICompliedShiftService compliedShiftService 
            )
		{
			_compliedShiftService = compliedShiftService;
            _clspayRegisterBDReportService = new Library.HumanResource.Report.Payroll.clsPayRegister();


        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
		public ActionResult Aplos()
		{
			return View();
		}

        #endregion -- Pages

        #region -- Operations

	    //private void GETBONUSDATA(string PayRollGroupId, string BonusPointId)
	    //{

     //       try
     //       {
     //           var indexS = BonusPointId.IndexOf("__");
     //           var policyid = BonusPointId.Substring(0, indexS);
     //           var cutoffdate = BonusPointId.Substring(indexS + 2);

     //           FileStream fs;
     //           string fileName;
     //           string PdfLocation = string.Empty;

     //           ReportUtility oReportUtility = new ReportUtility();

     //           DataTable dtBioDvAC = null;
     //           DataTable dtCompanny = null;
     //           var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
     //           dtCompanny = _payRegisterBDReportService.CompanyHeader(identity.CompanyId);

     //           LocalReport localReport = new LocalReport();
     //           localReport.ReportPath = Server.MapPath("~/Areas/HumanResource/Reports/BonusSheetReport.rdlc");

     //           ReportDataSource reportDataSource = new ReportDataSource();
     //           reportDataSource.Name = "HrDataSet";

     //           dtBioDvAC = _payRegisterBDReportService.GetBonusData(PayRollGroupId, BonusPointId);

     //           reportDataSource.Value = dtBioDvAC;

     //           string CompanyName = string.Empty;
     //           string CompanyAddress = string.Empty;
     //           //string TotalAmmountInWord = string.Empty;
     //           //double TotalAmmount = 0;
     //           if (dtCompanny.Rows.Count > 0)
     //           {
     //               CompanyName = dtCompanny.Rows[0]["UserName"].ToString();
     //               CompanyAddress = dtCompanny.Rows[0]["Address1"].ToString();

     //           }

     //           //TotalAmmount = Convert.ToDouble(dtBioDvAC.Compute("SUM(DrAmount)", string.Empty));
     //           //TotalAmmountInWord = oReportUtility.InWord(TotalAmmount, dtBioDvAC.Rows[0]["CurrencyId"].ToString());

     //           ReportParameter[] parameter = new ReportParameter[]
     //           {
     //               new ReportParameter("CompanyName", CompanyName),
     //               new ReportParameter("CompanyAddress", CompanyAddress),
     //               new ReportParameter("CutoffDate", cutoffdate),
     //               new ReportParameter("UserID", identity.Name)

     //           };
     //           localReport.SetParameters(parameter);

     //           localReport.DataSources.Add(reportDataSource);

     //           string ReportType = "pdf";
     //           string reportType = ReportType;
     //           String mimeType = string.Empty;
     //           String encoding = string.Empty;
     //           String extension = ReportType == "Excel" ? "xlsx" : "pdf";

     //           Microsoft.Reporting.WebForms.Warning[] warnings = null;
     //           string[] streamids = null;
     //           Byte[] bytes = null;

     //           bytes = localReport.Render(reportType, "", out mimeType, out encoding, out extension, out streamids, out warnings);
     //           string PDFPath = new DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("~/")) + "PDF\\";
     //           fileName = "BonusSheetReport" + DateTime.Now.ToString("dd-MMM-yyyy") + identity.UserId + ".pdf";

     //           if (System.IO.File.Exists(PDFPath + fileName))
     //           {
     //               try
     //               {
     //                   System.IO.File.Delete(PDFPath + fileName);
     //               }
     //               catch (Exception ex)
     //               {
     //                   throw (ex);
     //               }
     //           }


     //           fs = new FileStream(PDFPath + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
     //           byte[] data = new byte[fs.Length];
     //           fs.Write(bytes, 0, bytes.Length);
     //           var keyname = System.Configuration.ConfigurationManager.AppSettings["APP_NAME"];
     //           if (!string.IsNullOrEmpty(keyname))
     //           {
     //               PdfLocation = "/" + keyname + "/PDF/" + fileName;
     //           }
     //           else
     //           {
     //               PdfLocation = "/PDF/" + fileName;

     //           }
     //           fs.Close();
     //           ViewBag.ReportPath = PdfLocation;
     //           string path = Server.MapPath("/PDF/");

     //           string fileName1 = "BonusSheetReport" + DateTime.Now.AddDays(-1).ToString("dd-MMM-yyyy") + identity.UserId + ".pdf"; ;
     //           if (System.IO.File.Exists(path + fileName1))
     //           {
     //               try
     //               {
     //                   System.IO.File.Delete(path + fileName1);
     //               }
     //               catch (Exception ex)
     //               {
     //                   throw (ex);
     //               }
     //           }
     //       }
     //       catch (Exception ex)
     //       {
     //           //throw ex;

     //       }
     //   }


	    [HttpGet, Authorize]
        public ActionResult GetBonusData(string PayRollGroupId, string BonusPointId)
        {

            //GETBONUSDATA(PayRollGroupId, BonusPointId);
            string PdfLocation = string.Empty;
            try
            {
                var indexS = BonusPointId.IndexOf("__");
                var policyid = BonusPointId.Substring(0, indexS);
                var cutoffdate = BonusPointId.Substring(indexS + 2);


                FileStream fs;
                string fileName;
              

                ReportUtility oReportUtility = new ReportUtility();

                DataTable dtBioDvAC = null;
                DataTable dtCompanny = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                dtCompanny = oReportUtility.CompanyHeader(identity.CompanyId);

                LocalReport localReport = new LocalReport();
                localReport.ReportPath = Server.MapPath("~/Areas/HumanResource/Reports/BonusSheetReport.rdlc");

                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Name = "HrDataSet";

                dtBioDvAC = _clspayRegisterBDReportService.GetBonusData(PayRollGroupId, BonusPointId);

                reportDataSource.Value = dtBioDvAC;

                string CompanyName = string.Empty;
                string CompanyAddress = string.Empty;
                //string TotalAmmountInWord = string.Empty;
                //double TotalAmmount = 0;
                if (dtCompanny.Rows.Count > 0)
                {
                    CompanyName = dtCompanny.Rows[0]["UserName"].ToString();
                    CompanyAddress = dtCompanny.Rows[0]["Address1"].ToString();

                }

                //TotalAmmount = Convert.ToDouble(dtBioDvAC.Compute("SUM(DrAmount)", string.Empty));
                //TotalAmmountInWord = oReportUtility.InWord(TotalAmmount, dtBioDvAC.Rows[0]["CurrencyId"].ToString());

                ReportParameter[] parameter = new ReportParameter[]
                {
                    new ReportParameter("CompanyName", CompanyName),
                    new ReportParameter("CompanyAddress", CompanyAddress),
                    new ReportParameter("CutoffDate", cutoffdate),
                    new ReportParameter("UserID", identity.Name)

                };
                localReport.SetParameters(parameter);

                localReport.DataSources.Add(reportDataSource);

                string ReportType = "pdf";
                string reportType = ReportType;
                String mimeType = string.Empty;
                String encoding = string.Empty;
                String extension = ReportType == "Excel" ? "xlsx" : "pdf";

                Microsoft.Reporting.WebForms.Warning[] warnings = null;
                string[] streamids = null;
                Byte[] bytes = null;

                bytes = localReport.Render(reportType, "", out mimeType, out encoding, out extension, out streamids, out warnings);
                string PDFPath = new DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("~/")) + "PDF\\";
                fileName = "BonusSheetReport" + DateTime.Now.ToString("dd-MMM-yyyy") + identity.UserId + ".pdf";

                if (System.IO.File.Exists(PDFPath + fileName))
                {
                    try
                    {
                        System.IO.File.Delete(PDFPath + fileName);
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }


                fs = new FileStream(PDFPath + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                byte[] data = new byte[fs.Length];
                fs.Write(bytes, 0, bytes.Length);
                var keyname = System.Configuration.ConfigurationManager.AppSettings["APP_NAME"];
                if (!string.IsNullOrEmpty(keyname))
                {
                    PdfLocation = "/" + keyname + "/PDF/" + fileName;
                }
                else
                {
                    PdfLocation = "/PDF/" + fileName;

                }
                fs.Close();
                ViewBag.ReportPath = PdfLocation;
                string path = Server.MapPath("/PDF/");

                string fileName1 = "BonusSheetReport" + DateTime.Now.AddDays(-1).ToString("dd-MMM-yyyy") + identity.UserId + ".pdf"; ;
                if (System.IO.File.Exists(path + fileName1))
                {
                    try
                    {
                        System.IO.File.Delete(path + fileName1);
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
            }
            catch (Exception ex)
            {
                //throw ex;

            }

            return View("~/Areas/Accounts/Views/EmployeePaymentReport.cshtml");
            //return RedirectToAction("GetRdlcReport",new {PdfLocation});

        }       
        #endregion -- Operations



        [HttpGet, Authorize]
        public ActionResult GetRdlcReport(string PdfLocation)
        {
            ViewBag.ReportPath = PdfLocation;
            return View("~/Areas/Accounts/Views/EmployeePaymentReport.cshtml");
        }
    }
  
}